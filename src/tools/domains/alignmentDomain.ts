import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const Point2DSchema = z.object({
  x: z.number(),
  y: z.number(),
});

const GenericAlignmentResponseSchema = z.object({}).passthrough();

const AlignmentSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.enum(["centerline", "offset", "curb_return", "rail"]),
  length: z.number(),
  startStation: z.number(),
  endStation: z.number(),
  site: z.string().nullable(),
  profileCount: z.number(),
  isReference: z.boolean(),
});

const AlignmentListResponseSchema = z.object({
  alignments: z.array(AlignmentSummarySchema),
});

const AlignmentEntitySchema = z.object({
  index: z.number(),
  type: z.enum(["line", "arc", "spiral"]),
  startStation: z.number(),
  endStation: z.number(),
  length: z.number(),
});

const AlignmentDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.string(),
  style: z.string(),
  layer: z.string(),
  length: z.number(),
  startStation: z.number(),
  endStation: z.number(),
  entityCount: z.number(),
  entities: z.array(AlignmentEntitySchema),
  dependentProfiles: z.array(z.string()),
  dependentCorridors: z.array(z.string()),
  isReference: z.boolean(),
});

const AlignmentStationToPointResponseSchema = z.object({
  x: z.number(),
  y: z.number(),
  station: z.number(),
  offset: z.number(),
  units: z.string(),
});

const AlignmentPointToStationResponseSchema = z.object({
  station: z.number(),
  offset: z.number(),
  distanceFromAlignment: z.number(),
  units: z.string(),
});

const AlignmentReportStationSampleSchema = z.object({
  station: z.number(),
  x: z.number(),
  y: z.number(),
  offset: z.number(),
  units: z.string(),
});

const AlignmentReportResponseSchema = z.object({
  alignment: AlignmentDetailResponseSchema,
  summary: z.object({
    interval: z.number(),
    sampleCount: z.number(),
    startStation: z.number(),
    endStation: z.number(),
    length: z.number(),
    entityCount: z.number(),
    entityTypeBreakdown: z.object({
      line: z.number(),
      arc: z.number(),
      spiral: z.number(),
    }),
    dependentProfileCount: z.number(),
    dependentCorridorCount: z.number(),
  }),
  stationSamples: z.array(AlignmentReportStationSampleSchema),
});

const canonicalAlignmentInputShape = {
  action: z.enum([
    "list",
    "get",
    "station_to_point",
    "point_to_station",
    "create",
    "delete",
    "report",
    "add_tangent",
    "add_curve",
    "add_spiral",
    "delete_entity",
    "set_station_equation",
    "get_station_offset",
    "offset_create",
    "widen_transition",
  ]),
  name: z.string().optional(),
  station: z.number().optional(),
  offset: z.number().optional(),
  x: z.number().optional(),
  y: z.number().optional(),
  points: z.array(Point2DSchema).optional(),
  type: z.enum(["centerline", "offset"]).optional(),
  site: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
  interval: z.number().positive().optional(),
  maximumSamples: z.number().int().positive().max(200).optional(),
  startX: z.number().optional(),
  startY: z.number().optional(),
  endX: z.number().optional(),
  endY: z.number().optional(),
  passThroughX: z.number().optional(),
  passThroughY: z.number().optional(),
  radius: z.number().positive().optional(),
  spiralType: z.enum(["clothoid", "cubic", "biquadratic"]).optional(),
  startRadius: z.number().optional(),
  endRadius: z.number().optional(),
  length: z.number().positive().optional(),
  entityIndex: z.number().int().min(0).optional(),
  rawStation: z.number().optional(),
  nominalStation: z.number().optional(),
  offsetName: z.string().optional(),
  side: z.enum(["left", "right"]).optional(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  startOffset: z.number().optional(),
  endOffset: z.number().optional(),
};

const AlignmentListArgsSchema = z.object({
  action: z.literal("list"),
});

const AlignmentGetArgsSchema = z.object({
  action: z.literal("get"),
  name: z.string(),
});

const AlignmentStationToPointArgsSchema = z.object({
  action: z.literal("station_to_point"),
  name: z.string(),
  station: z.number(),
  offset: z.number().optional(),
});

const AlignmentPointToStationArgsSchema = z.object({
  action: z.literal("point_to_station"),
  name: z.string(),
  x: z.number(),
  y: z.number(),
});

const AlignmentCreateArgsSchema = z.object({
  action: z.literal("create"),
  name: z.string(),
  points: z.array(Point2DSchema).min(2),
  type: z.enum(["centerline", "offset"]).optional(),
  site: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
});

const AlignmentDeleteArgsSchema = z.object({
  action: z.literal("delete"),
  name: z.string(),
});

const AlignmentReportArgsSchema = z.object({
  action: z.literal("report"),
  name: z.string(),
  interval: z.number().positive().optional(),
  offset: z.number().optional(),
  maximumSamples: z.number().int().positive().max(200).optional(),
});

const AlignmentAddTangentArgsSchema = z.object({
  action: z.literal("add_tangent"),
  name: z.string(),
  startX: z.number(),
  startY: z.number(),
  endX: z.number(),
  endY: z.number(),
});

const AlignmentAddCurveArgsSchema = z.object({
  action: z.literal("add_curve"),
  name: z.string(),
  passThroughX: z.number(),
  passThroughY: z.number(),
  radius: z.number().positive(),
});

const AlignmentAddSpiralArgsSchema = z.object({
  action: z.literal("add_spiral"),
  name: z.string(),
  spiralType: z.enum(["clothoid", "cubic", "biquadratic"]).optional().default("clothoid"),
  startX: z.number(),
  startY: z.number(),
  startRadius: z.number(),
  endRadius: z.number(),
  length: z.number().positive(),
});

const AlignmentDeleteEntityArgsSchema = z.object({
  action: z.literal("delete_entity"),
  name: z.string(),
  entityIndex: z.number().int().min(0),
});

const AlignmentSetStationEquationArgsSchema = z.object({
  action: z.literal("set_station_equation"),
  name: z.string(),
  rawStation: z.number(),
  nominalStation: z.number(),
});

const AlignmentGetStationOffsetArgsSchema = z.object({
  action: z.literal("get_station_offset"),
  name: z.string(),
  x: z.number(),
  y: z.number(),
});

const AlignmentOffsetCreateArgsSchema = z.object({
  action: z.literal("offset_create"),
  name: z.string(),
  offsetName: z.string(),
  offset: z.number(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
});

const AlignmentWidenTransitionArgsSchema = z.object({
  action: z.literal("widen_transition"),
  name: z.string(),
  side: z.enum(["left", "right"]),
  startStation: z.number(),
  endStation: z.number(),
  startOffset: z.number(),
  endOffset: z.number(),
  offsetName: z.string().optional(),
});

function buildStationSequence(startStation: number, endStation: number, interval: number, maximumSamples: number) {
  const stations: number[] = [];
  const length = Math.max(0, endStation - startStation);
  const estimatedSamples = Math.floor(length / interval) + 2;

  if (estimatedSamples > maximumSamples) {
    throw new Error(
      `Alignment report would require ${estimatedSamples} samples, which exceeds the maximum of ${maximumSamples}. ` +
      "Increase the interval or maximumSamples.",
    );
  }

  let currentStation = startStation;
  while (currentStation < endStation) {
    stations.push(Number(currentStation.toFixed(6)));
    currentStation += interval;
  }

  if (stations.length === 0 || stations[stations.length - 1] !== endStation) {
    stations.push(Number(endStation.toFixed(6)));
  }

  return stations;
}

export const ALIGNMENT_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "alignment",
  actions: {
    list: {
      action: "list",
      inputSchema: AlignmentListArgsSchema,
      responseSchema: AlignmentListResponseSchema,
      capabilities: ["query"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listAlignments"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listAlignments", {}),
      ),
    },
    get: {
      action: "get",
      inputSchema: AlignmentGetArgsSchema,
      responseSchema: AlignmentDetailResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getAlignment"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getAlignment", {
          name: args.name,
        }),
      ),
    },
    station_to_point: {
      action: "station_to_point",
      inputSchema: AlignmentStationToPointArgsSchema,
      responseSchema: AlignmentStationToPointResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["alignmentStationToPoint"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentStationToPoint", {
          name: args.name,
          station: args.station,
          offset: args.offset ?? 0,
        }),
      ),
    },
    point_to_station: {
      action: "point_to_station",
      inputSchema: AlignmentPointToStationArgsSchema,
      responseSchema: AlignmentPointToStationResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["alignmentPointToStation"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentPointToStation", {
          name: args.name,
          x: args.x,
          y: args.y,
        }),
      ),
    },
    create: {
      action: "create",
      inputSchema: AlignmentCreateArgsSchema,
      responseSchema: GenericAlignmentResponseSchema,
      capabilities: ["create"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createAlignment"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createAlignment", {
          name: args.name,
          points: args.points,
          type: args.type,
          site: args.site,
          style: args.style,
          layer: args.layer,
          labelSet: args.labelSet,
        }),
      ),
    },
    delete: {
      action: "delete",
      inputSchema: AlignmentDeleteArgsSchema,
      responseSchema: GenericAlignmentResponseSchema,
      capabilities: ["delete"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["deleteAlignment"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("deleteAlignment", {
          name: args.name,
        }),
      ),
    },
    report: {
      action: "report",
      inputSchema: AlignmentReportArgsSchema,
      responseSchema: AlignmentReportResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getAlignment", "alignmentStationToPoint"],
      execute: async (args) => await withApplicationConnection(async (appClient) => {
        const alignment = AlignmentDetailResponseSchema.parse(
          await appClient.sendCommand("getAlignment", {
            name: args.name,
          }),
        );

        const interval = Number(
          args.interval ?? Math.max((alignment.endStation - alignment.startStation) / 20, 25),
        );
        const maximumSamples = Number(args.maximumSamples ?? 100);
        const stations = buildStationSequence(
          alignment.startStation,
          alignment.endStation,
          interval,
          maximumSamples,
        );

        const stationSamples = [];
        for (const station of stations) {
          stationSamples.push(
            AlignmentStationToPointResponseSchema.parse(
              await appClient.sendCommand("alignmentStationToPoint", {
                name: args.name,
                station,
                offset: args.offset ?? 0,
              }),
            ),
          );
        }

        const entityTypeBreakdown = alignment.entities.reduce(
          (accumulator, entity) => {
            accumulator[entity.type] += 1;
            return accumulator;
          },
          { line: 0, arc: 0, spiral: 0 },
        );

        return {
          alignment,
          summary: {
            interval,
            sampleCount: stationSamples.length,
            startStation: alignment.startStation,
            endStation: alignment.endStation,
            length: alignment.length,
            entityCount: alignment.entityCount,
            entityTypeBreakdown,
            dependentProfileCount: alignment.dependentProfiles.length,
            dependentCorridorCount: alignment.dependentCorridors.length,
          },
          stationSamples,
        };
      }),
    },
    add_tangent: {
      action: "add_tangent",
      inputSchema: AlignmentAddTangentArgsSchema,
      responseSchema: GenericAlignmentResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["alignmentAddTangent"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentAddTangent", {
          alignmentName: args.name,
          startX: args.startX,
          startY: args.startY,
          endX: args.endX,
          endY: args.endY,
        }),
      ),
    },
    add_curve: {
      action: "add_curve",
      inputSchema: AlignmentAddCurveArgsSchema,
      responseSchema: GenericAlignmentResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["alignmentAddCurve"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentAddCurve", {
          alignmentName: args.name,
          passThroughX: args.passThroughX,
          passThroughY: args.passThroughY,
          radius: args.radius,
        }),
      ),
    },
    add_spiral: {
      action: "add_spiral",
      inputSchema: AlignmentAddSpiralArgsSchema,
      responseSchema: GenericAlignmentResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["alignmentAddSpiral"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentAddSpiral", {
          alignmentName: args.name,
          spiralType: args.spiralType,
          startX: args.startX,
          startY: args.startY,
          startRadius: args.startRadius,
          endRadius: args.endRadius,
          length: args.length,
        }),
      ),
    },
    delete_entity: {
      action: "delete_entity",
      inputSchema: AlignmentDeleteEntityArgsSchema,
      responseSchema: GenericAlignmentResponseSchema,
      capabilities: ["edit", "delete"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["alignmentDeleteEntity"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentDeleteEntity", {
          alignmentName: args.name,
          entityIndex: args.entityIndex,
        }),
      ),
    },
    set_station_equation: {
      action: "set_station_equation",
      inputSchema: AlignmentSetStationEquationArgsSchema,
      responseSchema: GenericAlignmentResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["alignmentSetStationEquation"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentSetStationEquation", {
          alignmentName: args.name,
          rawStation: args.rawStation,
          nominalStation: args.nominalStation,
        }),
      ),
    },
    get_station_offset: {
      action: "get_station_offset",
      inputSchema: AlignmentGetStationOffsetArgsSchema,
      responseSchema: AlignmentPointToStationResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["alignmentGetStationOffset"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentGetStationOffset", {
          alignmentName: args.name,
          x: args.x,
          y: args.y,
        }),
      ),
    },
    offset_create: {
      action: "offset_create",
      inputSchema: AlignmentOffsetCreateArgsSchema,
      responseSchema: GenericAlignmentResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["alignmentOffsetCreate"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentOffsetCreate", {
          alignmentName: args.name,
          offsetName: args.offsetName,
          offset: args.offset,
          style: args.style,
          layer: args.layer,
          labelSet: args.labelSet,
        }),
      ),
    },
    widen_transition: {
      action: "widen_transition",
      inputSchema: AlignmentWidenTransitionArgsSchema,
      responseSchema: GenericAlignmentResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["alignmentWidenTransition"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("alignmentWidenTransition", {
          alignmentName: args.name,
          side: args.side,
          startStation: args.startStation,
          endStation: args.endStation,
          startOffset: args.startOffset,
          endOffset: args.endOffset,
          offsetName: args.offsetName,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_alignment",
      displayName: "Civil 3D Alignment",
      description: "Reads Civil 3D alignments, reports geometry, and manages editing actions through a single domain tool.",
      inputShape: canonicalAlignmentInputShape,
      supportedActions: [
        "list",
        "get",
        "station_to_point",
        "point_to_station",
        "create",
        "delete",
        "report",
        "add_tangent",
        "add_curve",
        "add_spiral",
        "delete_entity",
        "set_station_equation",
        "get_station_offset",
        "offset_create",
        "widen_transition",
      ],
      resolveAction: (rawArgs) => ({
        action: String(rawArgs.action ?? ""),
        args: rawArgs,
      }),
    },
    {
      toolName: "civil3d_alignment_report",
      displayName: "Civil 3D Alignment Report",
      description: "Builds a structured alignment report by fetching alignment geometry and sampling station locations.",
      inputShape: {
        alignmentName: z.string(),
        interval: z.number().positive().optional(),
        offset: z.number().optional(),
        maximumSamples: z.number().int().positive().max(200).optional(),
      },
      supportedActions: ["report"],
      resolveAction: (rawArgs) => ({
        action: "report",
        args: {
          action: "report",
          name: rawArgs.alignmentName,
          interval: rawArgs.interval,
          offset: rawArgs.offset,
          maximumSamples: rawArgs.maximumSamples,
        },
      }),
    },
    {
      toolName: "civil3d_alignment_add_tangent",
      displayName: "Civil 3D Alignment Add Tangent",
      description: "Appends a fixed tangent (straight line) entity to a Civil 3D alignment using two end points.",
      inputShape: {
        alignmentName: z.string(),
        startX: z.number(),
        startY: z.number(),
        endX: z.number(),
        endY: z.number(),
      },
      supportedActions: ["add_tangent"],
      resolveAction: (rawArgs) => ({
        action: "add_tangent",
        args: {
          action: "add_tangent",
          name: rawArgs.alignmentName,
          startX: rawArgs.startX,
          startY: rawArgs.startY,
          endX: rawArgs.endX,
          endY: rawArgs.endY,
        },
      }),
    },
    {
      toolName: "civil3d_alignment_add_curve",
      displayName: "Civil 3D Alignment Add Curve",
      description: "Appends a fixed horizontal curve entity to a Civil 3D alignment using a pass-through point and radius.",
      inputShape: {
        alignmentName: z.string(),
        passThroughX: z.number(),
        passThroughY: z.number(),
        radius: z.number().positive(),
      },
      supportedActions: ["add_curve"],
      resolveAction: (rawArgs) => ({
        action: "add_curve",
        args: {
          action: "add_curve",
          name: rawArgs.alignmentName,
          passThroughX: rawArgs.passThroughX,
          passThroughY: rawArgs.passThroughY,
          radius: rawArgs.radius,
        },
      }),
    },
    {
      toolName: "civil3d_alignment_add_spiral",
      displayName: "Civil 3D Alignment Add Spiral",
      description: "Appends a spiral (transition curve) entity to a Civil 3D alignment. Clothoid, cubic, and biquadratic spiral types are supported.",
      inputShape: {
        alignmentName: z.string(),
        spiralType: z.enum(["clothoid", "cubic", "biquadratic"]).optional().default("clothoid"),
        startX: z.number(),
        startY: z.number(),
        startRadius: z.number(),
        endRadius: z.number(),
        length: z.number().positive(),
      },
      supportedActions: ["add_spiral"],
      resolveAction: (rawArgs) => ({
        action: "add_spiral",
        args: {
          action: "add_spiral",
          name: rawArgs.alignmentName,
          spiralType: rawArgs.spiralType,
          startX: rawArgs.startX,
          startY: rawArgs.startY,
          startRadius: rawArgs.startRadius,
          endRadius: rawArgs.endRadius,
          length: rawArgs.length,
        },
      }),
    },
    {
      toolName: "civil3d_alignment_delete_entity",
      displayName: "Civil 3D Alignment Delete Entity",
      description: "Deletes a single entity (tangent, curve, or spiral) from a Civil 3D alignment by its zero-based index.",
      inputShape: {
        alignmentName: z.string(),
        entityIndex: z.number().int().min(0),
      },
      supportedActions: ["delete_entity"],
      resolveAction: (rawArgs) => ({
        action: "delete_entity",
        args: {
          action: "delete_entity",
          name: rawArgs.alignmentName,
          entityIndex: rawArgs.entityIndex,
        },
      }),
    },
    {
      toolName: "civil3d_alignment_set_station_equation",
      displayName: "Civil 3D Alignment Set Station Equation",
      description: "Adds a station equation to a Civil 3D alignment, allowing the nominal station to differ from the raw measured station.",
      inputShape: {
        alignmentName: z.string(),
        rawStation: z.number(),
        nominalStation: z.number(),
      },
      supportedActions: ["set_station_equation"],
      resolveAction: (rawArgs) => ({
        action: "set_station_equation",
        args: {
          action: "set_station_equation",
          name: rawArgs.alignmentName,
          rawStation: rawArgs.rawStation,
          nominalStation: rawArgs.nominalStation,
        },
      }),
    },
    {
      toolName: "civil3d_alignment_get_station_offset",
      displayName: "Civil 3D Alignment Get Station Offset",
      description: "Returns the station, offset, and perpendicular distance of an XY point relative to a Civil 3D alignment.",
      inputShape: {
        alignmentName: z.string(),
        x: z.number(),
        y: z.number(),
      },
      supportedActions: ["get_station_offset"],
      resolveAction: (rawArgs) => ({
        action: "get_station_offset",
        args: {
          action: "get_station_offset",
          name: rawArgs.alignmentName,
          x: rawArgs.x,
          y: rawArgs.y,
        },
      }),
    },
    {
      toolName: "civil3d_alignment_offset_create",
      displayName: "Civil 3D Alignment Offset Create",
      description: "Creates a new offset alignment at a constant distance from an existing base alignment.",
      inputShape: {
        alignmentName: z.string(),
        offsetName: z.string(),
        offset: z.number(),
        style: z.string().optional(),
        layer: z.string().optional(),
        labelSet: z.string().optional(),
      },
      supportedActions: ["offset_create"],
      resolveAction: (rawArgs) => ({
        action: "offset_create",
        args: {
          action: "offset_create",
          name: rawArgs.alignmentName,
          offsetName: rawArgs.offsetName,
          offset: rawArgs.offset,
          style: rawArgs.style,
          layer: rawArgs.layer,
          labelSet: rawArgs.labelSet,
        },
      }),
    },
    {
      toolName: "civil3d_alignment_widen_transition",
      displayName: "Civil 3D Alignment Widen Transition",
      description: "Creates a variable-offset widening or narrowing transition region on a Civil 3D alignment.",
      inputShape: {
        alignmentName: z.string(),
        side: z.enum(["left", "right"]),
        startStation: z.number(),
        endStation: z.number(),
        startOffset: z.number(),
        endOffset: z.number(),
        offsetName: z.string().optional(),
      },
      supportedActions: ["widen_transition"],
      resolveAction: (rawArgs) => ({
        action: "widen_transition",
        args: {
          action: "widen_transition",
          name: rawArgs.alignmentName,
          side: rawArgs.side,
          startStation: rawArgs.startStation,
          endStation: rawArgs.endStation,
          startOffset: rawArgs.startOffset,
          endOffset: rawArgs.endOffset,
          offsetName: rawArgs.offsetName,
        },
      }),
    },
  ],
};
