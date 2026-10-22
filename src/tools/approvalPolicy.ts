import { createHash, randomUUID } from "node:crypto";
import { withApplicationConnection } from "../utils/ConnectionManager.js";
import type { ToolCapability } from "./toolMetadata.js";

type JsonObject = Record<string, unknown>;

export interface ApprovalTarget {
  toolName: string;
  action: string;
  capabilities: ToolCapability[];
  safeForRetry: boolean;
  requiresActiveDrawing?: boolean;
}

interface ApprovalGrant {
  toolName: string;
  action: string;
  parametersHash: string;
  drawingFingerprint: string;
  expiresAt: number;
  /** Set for tokens issued by requestPlan: they belong to an ordered plan on ONE document. */
  planId?: string;
  stepIndex?: number;
}

interface PlanState {
  documentId: string;
  length: number;
  nextIndex: number;
  expiresAt: number;
}

export interface PlanStepInput {
  target: ApprovalTarget;
  parameters: JsonObject;
}

export interface PlanApprovalReceipt {
  planId: string;
  documentId: string;
  expiresAt: string;
  steps: Array<{ index: number; toolName: string; action: string; approvalToken: string }>;
}

export const MAX_PLAN_STEPS = 40;
export const MAX_PLAN_TTL_MS = 30 * 60 * 1000;

export interface ApprovalReceipt {
  approvalToken: string;
  expiresAt: string;
  drawingFingerprint: string;
}

export class ApprovalRequiredError extends Error {}

export class ApprovalValidationError extends Error {}

export type DrawingFingerprintProvider = () => Promise<string>;

const APPROVAL_TOKEN_TTL_MS = 5 * 60 * 1000;
const NO_ACTIVE_DRAWING_FINGERPRINT = "no-active-drawing";
const MUTATING_CAPABILITIES = new Set<ToolCapability>([
  "create",
  "edit",
  "delete",
  "manage",
  "import",
  "export",
]);
const EXPLICIT_APPROVAL_ACTION = /(?:^|_)(?:delete|remove|import|export|save|new|undo|redo|overwrite|replace|publish|promote|sync|fix|remediate)(?:_|$)/i;

function stableJson(value: unknown): string {
  if (Array.isArray(value)) {
    return `[${value.map((item) => stableJson(item)).join(",")}]`;
  }

  if (value && typeof value === "object") {
    const object = value as Record<string, unknown>;
    return `{${Object.keys(object)
      .sort()
      .map((key) => `${JSON.stringify(key)}:${stableJson(object[key])}`)
      .join(",")}}`;
  }

  return JSON.stringify(value);
}

function hashParameters(parameters: JsonObject): string {
  const normalized = { ...parameters };
  delete normalized.approvalToken;
  return createHash("sha256").update(stableJson(normalized)).digest("hex");
}

export function isApprovalRequired(target: ApprovalTarget): boolean {
  if (process.env.CIVIL3D_APPROVAL_MODE === "disabled") {
    return false;
  }

  return hasApprovalRisk(target);
}

export function hasApprovalRisk(target: ApprovalTarget): boolean {
  const mutatesState = target.capabilities.some((capability) => MUTATING_CAPABILITIES.has(capability));
  return (
    target.capabilities.some((capability) => ["delete", "import", "export"].includes(capability)) ||
    EXPLICIT_APPROVAL_ACTION.test(target.action) ||
    (!target.safeForRetry && mutatesState)
  );
}

/** Identity of the active document (path/name only, NOT its contents): stays stable while a plan mutates the drawing. */
export async function getActiveDocumentIdentity(): Promise<string> {
  const info = await withApplicationConnection((client) => client.sendCommand("getDrawingInfo", {})) as JsonObject | null;
  const identity = String(info?.filePath ?? info?.drawingName ?? info?.fileName ?? "");
  return createHash("sha256").update(identity).digest("hex");
}

export async function getActiveDrawingFingerprint(): Promise<string> {
  const drawingInfo = await withApplicationConnection((client) => client.sendCommand("getDrawingInfo", {}));
  return createHash("sha256").update(stableJson(drawingInfo)).digest("hex");
}

function isNoActiveDrawingError(error: unknown): boolean {
  const code = error && typeof error === "object" && "code" in error
    ? String(error.code)
    : "";
  const message = error instanceof Error ? error.message : String(error);
  return code === "CIVIL3D.NO_DRAWING" || code === "CIVIL3D.NO_ACTIVE_DRAWING" ||
    /no active (?:civil 3d )?drawing|no active (?:civil 3d )?document/i.test(message);
}

export class ApprovalPolicyService {
  private readonly grants = new Map<string, ApprovalGrant>();
  private readonly plans = new Map<string, PlanState>();

  public constructor(
    private readonly drawingFingerprint: DrawingFingerprintProvider = getActiveDrawingFingerprint,
    private readonly now: () => number = () => Date.now(),
    private readonly documentIdentity: DrawingFingerprintProvider = getActiveDocumentIdentity,
  ) {}

  /**
   * Approves an ORDERED list of exact actions in one call (e.g. the ~10 steps of a Fase 1 clean-up). Each step gets its own
   * single-use token bound to its exact parameters; the whole plan is bound to the active DOCUMENT (not to its contents, which
   * every step changes) and steps must be executed in order. Any deviation (other document, other parameters, skipped step,
   * expiry) is rejected exactly like a single-step token.
   */
  public async requestPlan(
    steps: PlanStepInput[],
    ttlMs = 15 * 60 * 1000,
  ): Promise<PlanApprovalReceipt> {
    if (steps.length === 0) {
      throw new ApprovalValidationError("A plan needs at least one step.");
    }
    if (steps.length > MAX_PLAN_STEPS) {
      throw new ApprovalValidationError(`A plan may have at most ${MAX_PLAN_STEPS} steps (got ${steps.length}); split it.`);
    }
    steps.forEach((step, index) => {
      if (!isApprovalRequired(step.target)) {
        throw new ApprovalValidationError(
          `Plan step ${index} ('${step.target.toolName}' action '${step.target.action}') does not require approval; remove it from the plan and execute it directly.`,
        );
      }
    });

    const boundedTtl = Math.min(ttlMs, MAX_PLAN_TTL_MS);
    const expiresAt = this.now() + boundedTtl;
    const documentId = await this.documentIdentity();
    const drawingFingerprint = await this.drawingFingerprint();
    const planId = randomUUID();
    this.plans.set(planId, { documentId, length: steps.length, nextIndex: 0, expiresAt });

    const issued = steps.map((step, index) => {
      const approvalToken = randomUUID();
      this.grants.set(approvalToken, {
        toolName: step.target.toolName,
        action: step.target.action,
        parametersHash: hashParameters(step.parameters),
        drawingFingerprint,
        expiresAt,
        planId,
        stepIndex: index,
      });
      return { index, toolName: step.target.toolName, action: step.target.action, approvalToken };
    });

    return { planId, documentId, expiresAt: new Date(expiresAt).toISOString(), steps: issued };
  }

  private async fingerprintFor(target: ApprovalTarget): Promise<string> {
    try {
      return await this.drawingFingerprint();
    } catch (error) {
      if (target.requiresActiveDrawing === false && isNoActiveDrawingError(error)) {
        return NO_ACTIVE_DRAWING_FINGERPRINT;
      }
      throw error;
    }
  }

  public async requestApproval(
    target: ApprovalTarget,
    parameters: JsonObject,
    ttlMs = APPROVAL_TOKEN_TTL_MS,
  ): Promise<ApprovalReceipt> {
    if (!isApprovalRequired(target)) {
      throw new ApprovalValidationError(
        `'${target.toolName}' action '${target.action}' does not require approval. Execute it directly.`,
      );
    }

    const drawingFingerprint = await this.fingerprintFor(target);
    const expiresAt = this.now() + ttlMs;
    const approvalToken = randomUUID();
    this.grants.set(approvalToken, {
      toolName: target.toolName,
      action: target.action,
      parametersHash: hashParameters(parameters),
      drawingFingerprint,
      expiresAt,
    });

    return {
      approvalToken,
      expiresAt: new Date(expiresAt).toISOString(),
      drawingFingerprint,
    };
  }

  public async enforce(target: ApprovalTarget, parameters: JsonObject): Promise<void> {
    if (!isApprovalRequired(target)) {
      return;
    }

    const approvalToken = typeof parameters.approvalToken === "string" ? parameters.approvalToken : undefined;
    if (!approvalToken) {
      throw new ApprovalRequiredError(
        `Approval required for '${target.toolName}' action '${target.action}'. ` +
        "Call civil3d_request_approval with the same toolName, action, and parameters, then retry with approvalToken.",
      );
    }

    const grant = this.grants.get(approvalToken);
    if (!grant) {
      throw new ApprovalValidationError("Approval token is missing, expired, or has already been used.");
    }

    this.grants.delete(approvalToken);

    if (grant.expiresAt <= this.now()) {
      throw new ApprovalValidationError("Approval token has expired. Request a new approval.");
    }

    if (
      grant.toolName !== target.toolName ||
      grant.action !== target.action ||
      grant.parametersHash !== hashParameters(parameters)
    ) {
      throw new ApprovalValidationError("Approval token does not match this tool action and its parameters.");
    }

    if (grant.planId !== undefined) {
      const plan = this.plans.get(grant.planId);
      if (!plan || plan.expiresAt <= this.now()) {
        this.plans.delete(grant.planId);
        throw new ApprovalValidationError("The approved plan has expired. Request a new plan approval.");
      }
      if (grant.stepIndex !== plan.nextIndex) {
        throw new ApprovalValidationError(
          `Plan steps must run in order: expected step ${plan.nextIndex}, got step ${grant.stepIndex}. Request a new plan approval.`,
        );
      }
      if (plan.documentId !== await this.documentIdentity()) {
        throw new ApprovalValidationError("The active document changed after the plan was approved. Request approval for the current document.");
      }
      plan.nextIndex += 1;
      if (plan.nextIndex >= plan.length) {
        this.plans.delete(grant.planId);
      }
      return;
    }

    const activeDrawingFingerprint = await this.fingerprintFor(target);
    if (grant.drawingFingerprint !== activeDrawingFingerprint) {
      throw new ApprovalValidationError("The active drawing changed after approval. Request approval for the current drawing.");
    }
  }
}

export const approvalPolicy = new ApprovalPolicyService();
