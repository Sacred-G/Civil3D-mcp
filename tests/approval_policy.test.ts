import { describe, expect, it } from "vitest";
import {
  ApprovalPolicyService,
  ApprovalRequiredError,
  ApprovalValidationError,
  isApprovalRequired,
} from "../src/tools/approvalPolicy.js";

const saveTarget = {
  toolName: "civil3d_drawing",
  action: "save",
  capabilities: ["edit", "manage"] as const,
  safeForRetry: false,
};

describe("approval policy", () => {
  it("requires approval for non-retryable drawing mutations but not safe queries", () => {
    expect(isApprovalRequired(saveTarget)).toBe(true);
    expect(isApprovalRequired({
      toolName: "get_drawing_info",
      action: "info",
      capabilities: ["query", "inspect"],
      safeForRetry: true,
    })).toBe(false);
  });

  it("binds a single-use token to exact parameters and drawing identity", async () => {
    let drawing = "drawing-a";
    let now = 10_000;
    const policy = new ApprovalPolicyService(async () => drawing, () => now);
    const parameters = { action: "save", saveAs: "C:/work/design.dwg" };
    const receipt = await policy.requestApproval(saveTarget, parameters, 30_000);

    await expect(policy.enforce(saveTarget, { ...parameters, approvalToken: receipt.approvalToken })).resolves.toBeUndefined();
    await expect(policy.enforce(saveTarget, { ...parameters, approvalToken: receipt.approvalToken }))
      .rejects.toBeInstanceOf(ApprovalValidationError);

    const changedParameters = await policy.requestApproval(saveTarget, parameters, 30_000);
    await expect(policy.enforce(saveTarget, {
      action: "save",
      saveAs: "C:/work/other.dwg",
      approvalToken: changedParameters.approvalToken,
    })).rejects.toBeInstanceOf(ApprovalValidationError);

    const changedDrawing = await policy.requestApproval(saveTarget, parameters, 30_000);
    drawing = "drawing-b";
    await expect(policy.enforce(saveTarget, { ...parameters, approvalToken: changedDrawing.approvalToken }))
      .rejects.toBeInstanceOf(ApprovalValidationError);

    now += 31_000;
    const expired = await policy.requestApproval(saveTarget, parameters, 30_000);
    now += 31_000;
    await expect(policy.enforce(saveTarget, { ...parameters, approvalToken: expired.approvalToken }))
      .rejects.toBeInstanceOf(ApprovalValidationError);
  });

  describe("plan approval", () => {
    const eraseTarget = {
      toolName: "acad_erase_entities",
      action: "erase_entities",
      capabilities: ["delete"] as const,
      safeForRetry: false,
    };
    const readTarget = {
      toolName: "acad_list_text_entities",
      action: "list_text_entities",
      capabilities: ["query"] as const,
      safeForRetry: true,
    };

    it("issues ordered tokens that survive drawing changes between steps but not out-of-order use", async () => {
      let content = "state-0";
      const policy = new ApprovalPolicyService(async () => content, () => 1_000, async () => "doc-a");
      const steps = [
        { target: eraseTarget, parameters: { handles: ["A1"] } },
        { target: saveTarget, parameters: { action: "save" } },
        { target: eraseTarget, parameters: { handles: ["B2"] } },
      ];
      const plan = await policy.requestPlan(steps);
      expect(plan.steps.map((s) => s.index)).toEqual([0, 1, 2]);

      await policy.enforce(eraseTarget, { handles: ["A1"], approvalToken: plan.steps[0]!.approvalToken });
      content = "state-1"; // step 0 changed the drawing: a single-step token would now be rejected
      await policy.enforce(saveTarget, { action: "save", approvalToken: plan.steps[1]!.approvalToken });
      content = "state-2";
      await expect(policy.enforce(eraseTarget, { handles: ["B2"], approvalToken: plan.steps[2]!.approvalToken }))
        .resolves.toBeUndefined();
    });

    it("rejects a skipped step, other parameters, another document and expiry", async () => {
      let doc = "doc-a";
      let now = 1_000;
      const policy = new ApprovalPolicyService(async () => "fp", () => now, async () => doc);
      const steps = [
        { target: eraseTarget, parameters: { handles: ["A1"] } },
        { target: eraseTarget, parameters: { handles: ["B2"] } },
      ];

      const skipped = await policy.requestPlan(steps);
      await expect(policy.enforce(eraseTarget, { handles: ["B2"], approvalToken: skipped.steps[1]!.approvalToken }))
        .rejects.toThrow(/in order/);

      const wrongParams = await policy.requestPlan(steps);
      await expect(policy.enforce(eraseTarget, { handles: ["ZZ"], approvalToken: wrongParams.steps[0]!.approvalToken }))
        .rejects.toBeInstanceOf(ApprovalValidationError);

      const otherDoc = await policy.requestPlan(steps);
      doc = "doc-b";
      await expect(policy.enforce(eraseTarget, { handles: ["A1"], approvalToken: otherDoc.steps[0]!.approvalToken }))
        .rejects.toThrow(/document changed/);
      doc = "doc-a";

      const expiring = await policy.requestPlan(steps, 60_000);
      now += 61_000;
      await expect(policy.enforce(eraseTarget, { handles: ["A1"], approvalToken: expiring.steps[0]!.approvalToken }))
        .rejects.toBeInstanceOf(ApprovalValidationError);
    });

    it("refuses empty, oversized and no-approval-needed plans and caps the ttl", async () => {
      const policy = new ApprovalPolicyService(async () => "fp", () => 0, async () => "doc-a");
      await expect(policy.requestPlan([])).rejects.toBeInstanceOf(ApprovalValidationError);
      await expect(policy.requestPlan(Array.from({ length: 41 }, () => ({ target: eraseTarget, parameters: {} }))))
        .rejects.toThrow(/at most 40/);
      await expect(policy.requestPlan([{ target: readTarget, parameters: {} }])).rejects.toThrow(/does not require approval/);
      const capped = await policy.requestPlan([{ target: eraseTarget, parameters: {} }], 24 * 60 * 60 * 1000);
      expect(Date.parse(capped.expiresAt)).toBe(30 * 60 * 1000);
    });
  });

  it("rejects protected execution that has no approval token", async () => {
    const policy = new ApprovalPolicyService(async () => "drawing-a");
    await expect(policy.enforce(saveTarget, { action: "save" })).rejects.toBeInstanceOf(ApprovalRequiredError);
  });

  it("permits approval for drawing-independent creation when no drawing is open", async () => {
    const noDrawing = Object.assign(new Error("No active drawing is open in Civil 3D."), {
      code: "CIVIL3D.NO_DRAWING",
    });
    const policy = new ApprovalPolicyService(async () => { throw noDrawing; });
    const target = {
      toolName: "civil3d_drawing",
      action: "new",
      capabilities: ["create", "manage"] as const,
      safeForRetry: false,
      requiresActiveDrawing: false,
    };
    const parameters = { action: "new", templatePath: "C:/templates/civil.dwt" };
    const receipt = await policy.requestApproval(target, parameters);

    expect(receipt.drawingFingerprint).toBe("no-active-drawing");
    await expect(policy.enforce(target, { ...parameters, approvalToken: receipt.approvalToken }))
      .resolves.toBeUndefined();
  });
});
