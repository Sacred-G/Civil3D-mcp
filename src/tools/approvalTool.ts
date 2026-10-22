import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { approvalPolicy, isApprovalRequired } from "./approvalPolicy.js";
import { captureToolHandler } from "./toolHandlerRegistry.js";
import { findManifestAction } from "./toolManifest.js";

type JsonObject = Record<string, unknown>;

const approvalInputShape = {
  toolName: z.string().min(1),
  action: z.string().min(1),
  parameters: z.record(z.unknown()),
  ttlSeconds: z.number().int().min(30).max(600).optional(),
};

const planInputShape = {
  steps: z.array(z.object({
    toolName: z.string().min(1),
    action: z.string().min(1),
    parameters: z.record(z.unknown()),
  })).min(1).max(40),
  ttlSeconds: z.number().int().min(60).max(1800).optional(),
};

const PlanApprovalResponseSchema = z.object({
  status: z.literal("approved"),
  planId: z.string(),
  documentId: z.string(),
  expiresAt: z.string(),
  steps: z.array(z.object({
    index: z.number(),
    toolName: z.string(),
    action: z.string(),
    approvalToken: z.string(),
  })),
  instruction: z.string(),
});

const PreviewResponseSchema = z.object({
  status: z.enum(["approval_required", "ready"]),
  toolName: z.string(),
  action: z.string(),
  capabilities: z.array(z.string()),
  safeForRetry: z.boolean(),
  instruction: z.string(),
});

const ApprovalResponseSchema = z.object({
  status: z.literal("approved"),
  toolName: z.string(),
  action: z.string(),
  approvalToken: z.string(),
  expiresAt: z.string(),
  drawingFingerprint: z.string(),
  instruction: z.string(),
});

function resolveApprovalTarget(args: { toolName: string; action: string; parameters: JsonObject }) {
  const target = findManifestAction(args.toolName, args.action);
  if (!target) {
    throw new Error(`Unknown tool action '${args.toolName}' / '${args.action}'.`);
  }

  const resolved = target.exposure.resolveAction(args.parameters);
  if (resolved.action !== args.action) {
    throw new Error(
      `Parameters do not resolve to action '${args.action}' for tool '${args.toolName}'. ` +
      "Include the tool action in parameters when calling a multi-action tool.",
    );
  }
  target.actionDefinition.inputSchema.parse(resolved.args);
  return target;
}

function successResult(structuredContent: JsonObject) {
  return {
    content: [{
      type: "text" as const,
      text: JSON.stringify(structuredContent, null, 2),
    }],
    structuredContent,
  };
}

function errorResult(toolName: string, error: unknown) {
  const message = error instanceof Error ? error.message : String(error);
  return {
    content: [{ type: "text" as const, text: `${toolName} failed: ${message}` }],
    isError: true,
  };
}

export function registerApprovalTool(server: McpServer) {
  const previewHandler = async (rawArgs: JsonObject) => {
    try {
      const args = rawArgs as { toolName: string; action: string; parameters: JsonObject };
      const target = resolveApprovalTarget(args);
      const requiresApproval = isApprovalRequired({
        toolName: args.toolName,
        action: args.action,
        capabilities: target.actionDefinition.capabilities,
        safeForRetry: target.actionDefinition.safeForRetry,
        requiresActiveDrawing: target.actionDefinition.requiresActiveDrawing,
      });
      return successResult({
        status: requiresApproval ? "approval_required" : "ready",
        toolName: args.toolName,
        action: args.action,
        capabilities: target.actionDefinition.capabilities,
        safeForRetry: target.actionDefinition.safeForRetry,
        instruction: requiresApproval
          ? "Call civil3d_request_approval with the same toolName, action, and parameters before execution."
          : "This action may be executed directly.",
      });
    } catch (error) {
      return errorResult("civil3d_preview_action", error);
    }
  };

  const requestHandler = async (rawArgs: JsonObject) => {
    try {
      const args = rawArgs as {
        toolName: string;
        action: string;
        parameters: JsonObject;
        ttlSeconds?: number;
      };
      const target = resolveApprovalTarget(args);
      const receipt = await approvalPolicy.requestApproval(
        {
          toolName: args.toolName,
          action: args.action,
          capabilities: target.actionDefinition.capabilities,
          safeForRetry: target.actionDefinition.safeForRetry,
          requiresActiveDrawing: target.actionDefinition.requiresActiveDrawing,
        },
        args.parameters,
        (args.ttlSeconds ?? 300) * 1000,
      );

      return successResult({
        status: "approved",
        toolName: args.toolName,
        action: args.action,
        ...receipt,
        instruction: "Retry the approved tool call with approvalToken and the identical parameters.",
      });
    } catch (error) {
      return errorResult("civil3d_request_approval", error);
    }
  };

  const planHandler = async (rawArgs: JsonObject) => {
    try {
      const args = rawArgs as {
        steps: Array<{ toolName: string; action: string; parameters: JsonObject }>;
        ttlSeconds?: number;
      };
      const steps = args.steps.map((step) => {
        const target = resolveApprovalTarget(step);
        return {
          target: {
            toolName: step.toolName,
            action: step.action,
            capabilities: target.actionDefinition.capabilities,
            safeForRetry: target.actionDefinition.safeForRetry,
            requiresActiveDrawing: target.actionDefinition.requiresActiveDrawing,
          },
          parameters: step.parameters,
        };
      });
      const receipt = await approvalPolicy.requestPlan(steps, (args.ttlSeconds ?? 900) * 1000);

      return successResult({
        status: "approved",
        ...receipt,
        instruction:
          "Execute the steps IN ORDER, each with its own approvalToken and exactly the parameters it was approved with. " +
          "The plan is bound to this document (its contents may change between steps). A different document, other parameters, " +
          "a skipped step or expiry rejects the token; then request a new plan.",
      });
    } catch (error) {
      return errorResult("civil3d_request_plan_approval", error);
    }
  };

  captureToolHandler("civil3d_preview_action", previewHandler);
  captureToolHandler("civil3d_request_plan_approval", planHandler);
  captureToolHandler("civil3d_request_approval", requestHandler);

  server.registerTool(
    "civil3d_preview_action",
    {
      title: "Preview Civil 3D Action",
      description: "Validates and previews whether a Civil 3D action requires approval before it can execute. This tool never changes the drawing.",
      inputSchema: {
        toolName: approvalInputShape.toolName,
        action: approvalInputShape.action,
        parameters: approvalInputShape.parameters,
      },
      outputSchema: PreviewResponseSchema,
      annotations: {
        title: "Preview Civil 3D Action",
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false,
      },
    },
    previewHandler,
  );

  server.registerTool(
    "civil3d_request_plan_approval",
    {
      title: "Request Civil 3D Plan Approval",
      description:
        "Issues one single-use approval token PER STEP for an ordered list of up to 40 exact Civil 3D actions in ONE call " +
        "(e.g. the clean-up steps of a Fase 1 delivery), instead of one request per step. Unlike single tokens, the plan is bound " +
        "to the active DOCUMENT rather than to its contents (every step changes them) and its steps must run in order. Every step " +
        "must require approval; exact parameters are still enforced per step.",
      inputSchema: planInputShape,
      outputSchema: PlanApprovalResponseSchema,
      annotations: {
        title: "Request Civil 3D Plan Approval",
        readOnlyHint: false,
        destructiveHint: false,
        idempotentHint: false,
        openWorldHint: false,
      },
    },
    planHandler,
  );

  server.registerTool(
    "civil3d_request_approval",
    {
      title: "Request Civil 3D Approval",
      description: "Issues a short-lived, single-use approval token for a specific destructive or ambiguous Civil 3D action in the active drawing.",
      inputSchema: approvalInputShape,
      outputSchema: ApprovalResponseSchema,
      annotations: {
        title: "Request Civil 3D Approval",
        readOnlyHint: false,
        destructiveHint: false,
        idempotentHint: false,
        openWorldHint: false,
      },
    },
    requestHandler,
  );
}
