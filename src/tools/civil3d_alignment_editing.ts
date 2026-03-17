import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const GenericResponseSchema = z.object({}).passthrough();

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return {
    content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }],
    isError: true,
  };
}

// ─── 1. civil3d_alignment_add_tangent ────────────────────────────────────────

const AlignmentAddTangentInputShape = {
  alignmentName: z.string().describe("Name of the alignment to modify"),
  startX: z.number().describe("Start point X coordinate"),
  startY: z.number().describe("Start point Y coordinate"),
  endX: z.number().describe("End point X coordinate"),
  endY: z.number().describe("End point Y coordinate"),
};

// ─── 2. civil3d_alignment_add_curve ──────────────────────────────────────────

const AlignmentAddCurveInputShape = {
  alignmentName: z.string().describe("Name of the alignment to modify"),
  passThroughX: z.number().describe("Pass-through point X coordinate"),
  passThroughY: z.number().describe("Pass-through point Y coordinate"),
  radius: z.number().positive().describe("Curve radius"),
};

// ─── 3. civil3d_alignment_add_spiral ─────────────────────────────────────────

const AlignmentAddSpiralInputShape = {
  alignmentName: z.string().describe("Name of the alignment to modify"),
  spiralType: z
    .enum(["clothoid", "cubic", "biquadratic"])
    .optional()
    .default("clothoid")
    .describe("Spiral type (default: clothoid)"),
  startX: z.number().describe("Spiral start point X"),
  startY: z.number().describe("Spiral start point Y"),
  startRadius: z
    .number()
    .describe("Radius at spiral start (0 = infinity / tangent end)"),
  endRadius: z
    .number()
    .describe("Radius at spiral end (0 = infinity / tangent end)"),
  length: z.number().positive().describe("Spiral length"),
};

// ─── 4. civil3d_alignment_delete_entity ──────────────────────────────────────

const AlignmentDeleteEntityInputShape = {
  alignmentName: z.string().describe("Name of the alignment"),
  entityIndex: z
    .number()
    .int()
    .min(0)
    .describe("Zero-based index of the entity to delete"),
};

// ─── 5. civil3d_alignment_set_station_equation ───────────────────────────────

const AlignmentSetStationEquationInputShape = {
  alignmentName: z.string().describe("Name of the alignment"),
  rawStation: z.number().describe("Raw (measured) station value"),
  nominalStation: z
    .number()
    .describe("Nominal (assigned/displayed) station value"),
};

// ─── 6. civil3d_alignment_get_station_offset ─────────────────────────────────

const AlignmentGetStationOffsetInputShape = {
  alignmentName: z.string().describe("Name of the alignment"),
  x: z.number().describe("Point X coordinate"),
  y: z.number().describe("Point Y coordinate"),
};

// ─── 7. civil3d_alignment_offset_create ──────────────────────────────────────

const AlignmentOffsetCreateInputShape = {
  alignmentName: z
    .string()
    .describe("Name of the base (parent) alignment"),
  offsetName: z.string().describe("Name for the new offset alignment"),
  offset: z
    .number()
    .describe(
      "Offset distance — positive = right side, negative = left side (in drawing units)",
    ),
  style: z.string().optional().describe("Alignment style name (optional)"),
  layer: z.string().optional().describe("Layer name (optional)"),
  labelSet: z
    .string()
    .optional()
    .describe("Label set name (optional)"),
};

// ─── 8. civil3d_alignment_widen_transition ────────────────────────────────────

const AlignmentWidenTransitionInputShape = {
  alignmentName: z.string().describe("Name of the base alignment"),
  side: z.enum(["left", "right"]).describe("Side to apply the widening"),
  startStation: z
    .number()
    .describe("Station where widening begins"),
  endStation: z.number().describe("Station where widening ends"),
  startOffset: z
    .number()
    .describe("Offset at startStation (positive = outward)"),
  endOffset: z
    .number()
    .describe("Offset at endStation (positive = outward)"),
  offsetName: z
    .string()
    .optional()
    .describe("Name for the resulting offset alignment (optional)"),
};

// ─── Registration ─────────────────────────────────────────────────────────────

export function registerCivil3DAlignmentEditingTools(server: McpServer) {
  // 1. add_tangent
  server.tool(
    "civil3d_alignment_add_tangent",
    "Appends a fixed tangent (straight line) entity to a Civil 3D alignment using two end points.",
    AlignmentAddTangentInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(AlignmentAddTangentInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("alignmentAddTangent", {
            alignmentName: parsed.alignmentName,
            startX: parsed.startX,
            startY: parsed.startY,
            endX: parsed.endX,
            endY: parsed.endY,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_alignment_add_tangent", error);
      }
    },
  );

  // 2. add_curve
  server.tool(
    "civil3d_alignment_add_curve",
    "Appends a fixed horizontal curve entity to a Civil 3D alignment using a pass-through point and radius.",
    AlignmentAddCurveInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(AlignmentAddCurveInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("alignmentAddCurve", {
            alignmentName: parsed.alignmentName,
            passThroughX: parsed.passThroughX,
            passThroughY: parsed.passThroughY,
            radius: parsed.radius,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_alignment_add_curve", error);
      }
    },
  );

  // 3. add_spiral
  server.tool(
    "civil3d_alignment_add_spiral",
    "Appends a spiral (transition curve) entity to a Civil 3D alignment. Clothoid, cubic, and biquadratic spiral types are supported.",
    AlignmentAddSpiralInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(AlignmentAddSpiralInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("alignmentAddSpiral", {
            alignmentName: parsed.alignmentName,
            spiralType: parsed.spiralType,
            startX: parsed.startX,
            startY: parsed.startY,
            startRadius: parsed.startRadius,
            endRadius: parsed.endRadius,
            length: parsed.length,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_alignment_add_spiral", error);
      }
    },
  );

  // 4. delete_entity
  server.tool(
    "civil3d_alignment_delete_entity",
    "Deletes a single entity (tangent, curve, or spiral) from a Civil 3D alignment by its zero-based index.",
    AlignmentDeleteEntityInputShape,
    async (args, _extra) => {
      try {
        const parsed =
          z.object(AlignmentDeleteEntityInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("alignmentDeleteEntity", {
            alignmentName: parsed.alignmentName,
            entityIndex: parsed.entityIndex,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_alignment_delete_entity", error);
      }
    },
  );

  // 5. set_station_equation
  server.tool(
    "civil3d_alignment_set_station_equation",
    "Adds a station equation to a Civil 3D alignment, allowing the nominal (displayed) station to differ from the raw measured station.",
    AlignmentSetStationEquationInputShape,
    async (args, _extra) => {
      try {
        const parsed = z
          .object(AlignmentSetStationEquationInputShape)
          .parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("alignmentSetStationEquation", {
            alignmentName: parsed.alignmentName,
            rawStation: parsed.rawStation,
            nominalStation: parsed.nominalStation,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult(
          "civil3d_alignment_set_station_equation",
          error,
        );
      }
    },
  );

  // 6. get_station_offset
  server.tool(
    "civil3d_alignment_get_station_offset",
    "Returns the station, offset, and perpendicular distance of an arbitrary XY point relative to a Civil 3D alignment. High-value sampling tool for AI geometry reasoning.",
    AlignmentGetStationOffsetInputShape,
    async (args, _extra) => {
      try {
        const parsed = z
          .object(AlignmentGetStationOffsetInputShape)
          .parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("alignmentGetStationOffset", {
            alignmentName: parsed.alignmentName,
            x: parsed.x,
            y: parsed.y,
          }),
        );
        const result = z
          .object({
            station: z.number(),
            offset: z.number(),
            distanceFromAlignment: z.number(),
            units: z.string(),
          })
          .parse(response);
        return {
          content: [{ type: "text", text: JSON.stringify(result, null, 2) }],
        };
      } catch (error) {
        return errorResult("civil3d_alignment_get_station_offset", error);
      }
    },
  );

  // 7. offset_create
  server.tool(
    "civil3d_alignment_offset_create",
    "Creates a new offset alignment at a constant distance from an existing base alignment. Positive offset = right side; negative = left.",
    AlignmentOffsetCreateInputShape,
    async (args, _extra) => {
      try {
        const parsed =
          z.object(AlignmentOffsetCreateInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("alignmentOffsetCreate", {
            alignmentName: parsed.alignmentName,
            offsetName: parsed.offsetName,
            offset: parsed.offset,
            style: parsed.style,
            layer: parsed.layer,
            labelSet: parsed.labelSet,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_alignment_offset_create", error);
      }
    },
  );

  // 8. widen_transition
  server.tool(
    "civil3d_alignment_widen_transition",
    "Creates a variable-offset (widening/narrowing) transition region on a Civil 3D alignment between two stations. Useful for modelling lane tapers and turn lanes.",
    AlignmentWidenTransitionInputShape,
    async (args, _extra) => {
      try {
        const parsed = z
          .object(AlignmentWidenTransitionInputShape)
          .parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("alignmentWidenTransition", {
            alignmentName: parsed.alignmentName,
            side: parsed.side,
            startStation: parsed.startStation,
            endStation: parsed.endStation,
            startOffset: parsed.startOffset,
            endOffset: parsed.endOffset,
            offsetName: parsed.offsetName,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_alignment_widen_transition", error);
      }
    },
  );
}
