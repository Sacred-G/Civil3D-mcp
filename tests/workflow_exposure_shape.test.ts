import { describe, expect, it } from "vitest";
import { z } from "zod";
import { WORKFLOW_DOMAIN_DEFINITION } from "../src/tools/domains/workflowDomain.js";

// A dedicated workflow tool strips every field its inputShape does not list before the
// action schema ever sees it, so a field added only to the action schema is silently dropped
// (civil3d_workflow_fase1_build shipped without expectedDocument/stripPropNotes/sheet).
describe("workflow dedicated tools expose every action field", () => {
  const single = WORKFLOW_DOMAIN_DEFINITION.exposures.filter((e) => e.supportedActions.length === 1);

  it.each(single.map((e) => [e.toolName, e] as const))("%s", (_name, exposure) => {
    const action = WORKFLOW_DOMAIN_DEFINITION.actions[exposure.supportedActions[0]];
    let schema: z.ZodTypeAny = action.inputSchema;
    while (schema instanceof z.ZodEffects) schema = schema.innerType();
    const actionKeys = Object.keys((schema as z.ZodObject<z.ZodRawShape>).shape).filter((k) => k !== "action");
    const exposed = Object.keys(exposure.inputShape);
    expect(actionKeys.filter((k) => !exposed.includes(k))).toEqual([]);
  });
});
