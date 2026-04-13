import { TOOL_CATALOG as TOOL_METADATA_CATALOG } from "../tools/tool_catalog.js";
import type {
  ToolCatalogEntry as ToolMetadataCatalogEntry,
  ToolDomain,
} from "../tools/toolMetadata.js";

export type OrchestratorIntent = string;

export interface ToolCatalogEntry {
  intent: OrchestratorIntent;
  title: string;
  domain: ToolDomain;
  toolName: string;
  action: string;
  keywords: string[];
  requiredFields: string[];
  description: string;
  buildToolArgs: (params: Record<string, unknown>) => Record<string, unknown>;
  source?: "explicit" | "derived";
}

function pickDefined(params: Record<string, unknown>, fields: string[]): Record<string, unknown> {
  return Object.fromEntries(
    fields
      .filter((field) => params[field] !== undefined)
      .map((field) => [field, params[field]]),
  );
}

function buildActionArgs(
  action: string,
  fields: string[] = [],
  defaults: Record<string, unknown> = {},
) {
  return (params: Record<string, unknown>) => ({
    action,
    ...defaults,
    ...pickDefined(params, fields),
  });
}

function buildPassthroughArgs(
  fields: string[] = [],
  defaults: Record<string, unknown> = {},
) {
  return (params: Record<string, unknown>) => ({
    ...defaults,
    ...pickDefined(params, fields),
  });
}

const EXPLICIT_TOOL_CATALOG: ToolCatalogEntry[] = [
  {
    intent: "drawing_info",
    title: "Drawing information",
    domain: "drawing",
    toolName: "civil3d_drawing",
    action: "info",
    keywords: ["drawing", "drawing info", "drawing information", "project info", "project information", "what is in this drawing"],
    requiredFields: [],
    description: "Gets document-level information and object counts for the active drawing.",
    buildToolArgs: buildActionArgs("info"),
  },
  {
    intent: "workflow_project_startup",
    title: "Project startup workflow",
    domain: "workflow",
    toolName: "civil3d_workflow_project_startup",
    action: "execute",
    keywords: ["project startup", "start project", "startup drawing", "new project workflow", "drawing startup workflow"],
    requiredFields: [],
    description: "Checks plugin health, inspects drawing readiness, and optionally creates or saves a startup drawing.",
    buildToolArgs: buildPassthroughArgs(["templatePath", "saveAs"]),
  },
  {
    intent: "workflow_drawing_readiness_audit",
    title: "Drawing readiness audit",
    domain: "workflow",
    toolName: "civil3d_workflow_drawing_readiness_audit",
    action: "execute",
    keywords: ["drawing readiness audit", "drawing audit", "readiness check", "audit drawing readiness", "check drawing readiness"],
    requiredFields: [],
    description: "Runs a readiness audit across plugin health, drawing state, selection, and drawing standards.",
    buildToolArgs: buildPassthroughArgs(["layerPrefix", "limit"]),
  },
  {
    intent: "list_tool_capabilities",
    title: "List tool capabilities",
    domain: "docs",
    toolName: "list_tool_capabilities",
    action: "list",
    keywords: ["list tools", "tool capabilities", "available tools", "supported tools", "what tools can you use"],
    requiredFields: [],
    description: "Lists the supported Civil 3D MCP tools and their domains/capabilities.",
    buildToolArgs: buildPassthroughArgs(),
  },
  {
    intent: "list_surfaces",
    title: "List surfaces",
    domain: "surface",
    toolName: "civil3d_surface",
    action: "list",
    keywords: ["list surfaces", "show surfaces", "what surfaces", "surface list", "surfaces in drawing"],
    requiredFields: [],
    description: "Lists Civil 3D surfaces in the active drawing.",
    buildToolArgs: buildActionArgs("list"),
  },
  {
    intent: "get_surface",
    title: "Get surface details",
    domain: "surface",
    toolName: "civil3d_surface",
    action: "get",
    keywords: ["get surface", "show surface", "surface details", "surface info", "inspect surface"],
    requiredFields: ["name"],
    description: "Gets details for a named Civil 3D surface.",
    buildToolArgs: buildActionArgs("get", ["name"]),
  },
  {
    intent: "create_surface",
    title: "Create surface",
    domain: "surface",
    toolName: "civil3d_surface",
    action: "create",
    keywords: ["create surface", "make surface", "new surface", "add surface"],
    requiredFields: ["name"],
    description: "Creates a Civil 3D surface when a surface name is provided.",
    buildToolArgs: buildActionArgs("create", ["name", "style", "layer"]),
  },
  {
    intent: "get_surface_statistics",
    title: "Get surface statistics",
    domain: "surface",
    toolName: "civil3d_surface_statistics_get",
    action: "get",
    keywords: ["surface statistics", "surface stats", "surface analysis summary", "surface triangle count", "surface area statistics"],
    requiredFields: ["name"],
    description: "Gets detailed statistics for a named Civil 3D surface.",
    buildToolArgs: buildPassthroughArgs(["name"]),
  },
  {
    intent: "analyze_surface_slope",
    title: "Analyze surface slope",
    domain: "surface",
    toolName: "civil3d_surface_analyze_slope",
    action: "analyze",
    keywords: ["analyze surface slope", "surface slope analysis", "surface slope distribution", "slope analysis for surface"],
    requiredFields: ["name"],
    description: "Analyzes slope distribution for a named Civil 3D surface.",
    buildToolArgs: buildPassthroughArgs(["name"]),
  },
  {
    intent: "calculate_surface_volume",
    title: "Calculate surface volume",
    domain: "surface",
    toolName: "civil3d_surface_volume_calculate",
    action: "calculate",
    keywords: ["surface volume", "cut fill", "cut/fill", "compare surfaces", "surface volume between"],
    requiredFields: ["baseSurface", "comparisonSurface"],
    description: "Calculates cut/fill volume between two named Civil 3D surfaces.",
    buildToolArgs: buildPassthroughArgs(
      ["baseSurface", "comparisonSurface"],
      { method: "tin_volume" },
    ),
  },
  {
    intent: "generate_surface_volume_report",
    title: "Generate surface volume report",
    domain: "surface",
    toolName: "civil3d_surface_volume_report",
    action: "generate",
    keywords: ["surface volume report", "volume report", "cut fill report", "surface comparison report"],
    requiredFields: ["baseSurface", "comparisonSurface"],
    description: "Generates a formatted cut/fill volume report comparing two named Civil 3D surfaces.",
    buildToolArgs: buildPassthroughArgs(
      ["baseSurface", "comparisonSurface"],
      { format: "summary" },
    ),
  },
  {
    intent: "workflow_surface_comparison_report",
    title: "Workflow surface comparison report",
    domain: "workflow",
    toolName: "civil3d_workflow_surface_comparison_report",
    action: "execute",
    keywords: ["surface comparison workflow", "surface comparison report workflow", "compare surfaces with report", "workflow compare surfaces"],
    requiredFields: ["baseSurface", "comparisonSurface"],
    description: "Runs the structured surface comparison workflow and then generates a formatted report.",
    buildToolArgs: buildPassthroughArgs(
      ["baseSurface", "comparisonSurface"],
      { format: "summary" },
    ),
  },
  {
    intent: "sample_surface_elevations",
    title: "Sample surface elevations",
    domain: "surface",
    toolName: "civil3d_surface_sample_elevations",
    action: "sample",
    keywords: ["sample surface elevations", "sample surface", "surface grid sample", "grid sample surface"],
    requiredFields: ["name", "gridSpacing"],
    description: "Samples a named Civil 3D surface on a regular grid using the requested spacing.",
    buildToolArgs: buildPassthroughArgs(
      ["name", "gridSpacing"],
      { method: "grid" },
    ),
  },
  {
    intent: "create_surface_from_dem",
    title: "Create surface from DEM",
    domain: "surface",
    toolName: "civil3d_surface_create_from_dem",
    action: "create",
    keywords: ["create surface from dem", "import dem surface", "surface from dem", "create surface from tif"],
    requiredFields: ["name", "filePath"],
    description: "Creates a Civil 3D TIN surface by importing a DEM or raster terrain file.",
    buildToolArgs: buildPassthroughArgs(["name", "filePath", "style", "layer"]),
  },
  {
    intent: "list_alignments",
    title: "List alignments",
    domain: "alignment",
    toolName: "civil3d_alignment",
    action: "list",
    keywords: ["list alignments", "show alignments", "what alignments", "alignment list", "alignments in drawing"],
    requiredFields: [],
    description: "Lists Civil 3D alignments in the active drawing.",
    buildToolArgs: buildActionArgs("list"),
  },
  {
    intent: "get_alignment",
    title: "Get alignment details",
    domain: "alignment",
    toolName: "civil3d_alignment",
    action: "get",
    keywords: ["get alignment", "show alignment", "alignment details", "alignment info", "inspect alignment"],
    requiredFields: ["name"],
    description: "Gets details for a named Civil 3D alignment.",
    buildToolArgs: buildActionArgs("get", ["name"]),
  },
  {
    intent: "list_profiles",
    title: "List profiles for alignment",
    domain: "profile",
    toolName: "civil3d_profile",
    action: "list",
    keywords: ["list profiles", "show profiles", "what profiles", "profile list", "profiles for alignment"],
    requiredFields: ["alignmentName"],
    description: "Lists profiles associated with a named alignment.",
    buildToolArgs: buildActionArgs("list", ["alignmentName"]),
  },
  {
    intent: "create_profile_from_surface",
    title: "Create profile from surface",
    domain: "profile",
    toolName: "civil3d_profile",
    action: "create_from_surface",
    keywords: ["create profile from surface", "profile from surface", "surface profile", "eg profile", "existing ground profile"],
    requiredFields: ["alignmentName", "profileName", "surfaceName"],
    description: "Creates a profile from an existing alignment and surface.",
    buildToolArgs: buildActionArgs(
      "create_from_surface",
      ["alignmentName", "profileName", "surfaceName", "style", "layer", "labelSet"],
    ),
  },
  {
    intent: "create_layout_profile",
    title: "Create layout profile",
    domain: "profile",
    toolName: "civil3d_profile",
    action: "create_layout",
    keywords: ["create layout profile", "layout profile", "create design profile", "design profile", "proposed profile"],
    requiredFields: ["alignmentName", "profileName"],
    description: "Creates a layout profile for a named alignment.",
    buildToolArgs: buildActionArgs(
      "create_layout",
      ["alignmentName", "profileName", "style", "layer", "labelSet"],
    ),
  },
  {
    intent: "calculate_sight_distance",
    title: "Calculate sight distance",
    domain: "sight_distance",
    toolName: "civil3d_sight_distance_calculate",
    action: "calculate",
    keywords: ["calculate sight distance", "sight distance", "stopping sight distance", "passing sight distance", "decision sight distance"],
    requiredFields: ["designSpeed"],
    description: "Calculates design sight distance requirements and can optionally evaluate a specific alignment/profile location.",
    buildToolArgs: buildPassthroughArgs(
      ["designSpeed", "alignmentName", "profileName"],
      {
        speedUnits: "kmh",
        sightDistanceType: "stopping",
        grade: 0,
        perceptionReactionTime: 2.5,
        standard: "AASHTO",
      },
    ),
  },
  {
    intent: "check_stopping_distance",
    title: "Check stopping distance",
    domain: "sight_distance",
    toolName: "civil3d_stopping_distance_check",
    action: "check",
    keywords: ["check stopping distance", "stopping distance compliance", "ssd check", "available sight distance"],
    requiredFields: ["alignmentName", "profileName", "designSpeed"],
    description: "Checks stopping sight distance compliance along a named alignment and profile.",
    buildToolArgs: buildPassthroughArgs(
      ["alignmentName", "profileName", "designSpeed"],
      {
        stationInterval: 25,
        standard: "AASHTO",
      },
    ),
  },
  {
    intent: "calculate_detention_basin_size",
    title: "Calculate detention basin size",
    domain: "detention",
    toolName: "civil3d_detention_basin_size_calculate",
    action: "calculate",
    keywords: ["detention basin size", "size detention basin", "detention sizing", "stormwater detention"],
    requiredFields: ["inflow", "outflow"],
    description: "Calculates detention basin storage sizing from inflow and allowable outflow.",
    buildToolArgs: buildPassthroughArgs(
      ["inflow", "outflow", "surfaceName"],
      {
        stormDuration: 60,
        method: "modified_rational",
        sideSlope: 3.0,
        bottomWidth: 10.0,
        freeboardDepth: 1.0,
      },
    ),
  },
  {
    intent: "generate_detention_stage_storage",
    title: "Generate detention stage storage",
    domain: "detention",
    toolName: "civil3d_detention_stage_storage",
    action: "generate",
    keywords: ["stage storage", "detention stage storage", "stage storage table", "storage discharge table"],
    requiredFields: ["surfaceName", "bottomElevation", "topElevation"],
    description: "Generates a detention basin stage-storage-discharge table from a named surface and elevation range.",
    buildToolArgs: buildPassthroughArgs(
      ["surfaceName", "bottomElevation", "topElevation"],
      {
        elevationIncrement: 0.5,
        outletType: "orifice",
      },
    ),
  },
  {
    intent: "calculate_slope_geometry",
    title: "Calculate slope geometry",
    domain: "slope_analysis",
    toolName: "civil3d_slope_geometry_calculate",
    action: "calculate",
    keywords: ["slope geometry", "daylight line", "daylight slopes", "cut fill slope geometry"],
    requiredFields: ["alignmentName"],
    description: "Calculates daylight line and cut/fill slope geometry along a named alignment.",
    buildToolArgs: buildPassthroughArgs(
      ["alignmentName", "profileName", "surfaceName"],
      {
        cutSlopeRatio: 2.0,
        fillSlopeRatio: 3.0,
        benchWidth: 0,
        benchHeightInterval: 20,
        stationInterval: 10,
      },
    ),
  },
  {
    intent: "check_slope_stability",
    title: "Check slope stability",
    domain: "slope_analysis",
    toolName: "civil3d_slope_stability_check",
    action: "check",
    keywords: ["slope stability", "check slopes", "slope stability check", "cut fill stability"],
    requiredFields: ["alignmentName"],
    description: "Checks cut/fill slope stability along a named alignment.",
    buildToolArgs: buildPassthroughArgs(
      ["alignmentName", "surfaceName"],
      {
        maxCutSlopeRatio: 1.5,
        maxFillSlopeRatio: 2.0,
        maxCutHeight: 30,
        maxFillHeight: 40,
        stationInterval: 25,
        soilType: "granular",
      },
    ),
  },
  {
    intent: "export_pay_items",
    title: "Export pay items",
    domain: "cost_estimation",
    toolName: "civil3d_pay_items_export",
    action: "export",
    keywords: ["export pay items", "pay item export", "bill of quantities", "quantity export", "export estimate"],
    requiredFields: ["outputPath"],
    description: "Exports Civil 3D quantities to a pay item schedule file.",
    buildToolArgs: (params) => ({
      outputPath: params.outputPath,
      corridorName: params.corridorName,
      baseSurface: params.baseSurface,
      designSurface: params.comparisonSurface,
      alignmentName: params.alignmentName,
      payItems: params.payItems,
      includeEarthwork: true,
      includeCorridorMaterials: true,
      includePipeLengths: true,
      includeStructureCounts: true,
    }),
  },
  {
    intent: "estimate_material_cost",
    title: "Estimate material cost",
    domain: "cost_estimation",
    toolName: "civil3d_material_cost_estimate",
    action: "estimate",
    keywords: ["material cost estimate", "cost estimate", "estimate construction cost", "pricing estimate"],
    requiredFields: ["payItems"],
    description: "Generates a construction cost estimate from Civil 3D quantities and supplied pay item pricing.",
    buildToolArgs: (params) => ({
      corridorName: params.corridorName,
      baseSurface: params.baseSurface,
      designSurface: params.comparisonSurface,
      alignmentName: params.alignmentName,
      contingencyPercent: 0,
      mobilizationPercent: 5,
      payItems: params.payItems,
      outputPath: params.outputPath,
    }),
  },
  {
    intent: "list_corridors",
    title: "List corridors",
    domain: "corridor",
    toolName: "civil3d_corridor",
    action: "list",
    keywords: ["list corridors", "show corridors", "what corridors", "corridor list", "corridors in drawing"],
    requiredFields: [],
    description: "Lists Civil 3D corridors in the active drawing.",
    buildToolArgs: buildActionArgs("list"),
  },
  {
    intent: "get_corridor",
    title: "Get corridor details",
    domain: "corridor",
    toolName: "civil3d_corridor",
    action: "get",
    keywords: ["get corridor", "show corridor", "corridor details", "corridor info", "inspect corridor"],
    requiredFields: ["name"],
    description: "Gets details for a named Civil 3D corridor.",
    buildToolArgs: buildActionArgs("get", ["name"]),
  },
  {
    intent: "rebuild_corridor",
    title: "Rebuild corridor",
    domain: "corridor",
    toolName: "civil3d_corridor",
    action: "rebuild",
    keywords: ["rebuild corridor", "update corridor", "refresh corridor"],
    requiredFields: ["name"],
    description: "Rebuilds a named Civil 3D corridor.",
    buildToolArgs: buildActionArgs("rebuild", ["name"]),
  },
  {
    intent: "corridor_prerequisites",
    title: "Corridor prerequisites",
    domain: "corridor",
    toolName: "civil3d_corridor",
    action: "advice",
    keywords: ["corridor", "before corridor", "corridor prerequisites", "what do i need for a corridor"],
    requiredFields: [],
    description: "Explains what is typically required before creating a corridor.",
    buildToolArgs: buildPassthroughArgs(),
  },
  {
    intent: "workflow_data_shortcut_reference_sync",
    title: "Data shortcut reference workflow",
    domain: "workflow",
    toolName: "civil3d_workflow_data_shortcut_reference_sync",
    action: "execute",
    keywords: ["reference data shortcut", "data shortcut reference", "reference shortcut and sync", "sync referenced shortcut"],
    requiredFields: ["projectFolder", "shortcutName", "shortcutType"],
    description: "References a project data shortcut into the current drawing and synchronizes it.",
    buildToolArgs: buildPassthroughArgs(["projectFolder", "shortcutName", "shortcutType", "layer"]),
  },
];

const ROUTE_PARAM_FIELDS = [
  "name",
  "alignmentName",
  "corridorName",
  "profileName",
  "surfaceName",
  "projectFolder",
  "shortcutName",
  "shortcutType",
  "templatePath",
  "saveAs",
  "limit",
  "layerPrefix",
  "networkName",
  "pipeName",
  "structureName",
  "fittingName",
  "partName",
  "partsList",
  "targetType",
  "targetName",
  "targetNetwork",
  "sourceNetwork",
  "newPartName",
  "startPoint",
  "endPoint",
  "position",
  "baseSurface",
  "comparisonSurface",
  "style",
  "layer",
  "labelSet",
  "filePath",
  "outputPath",
  "gridSpacing",
  "designSpeed",
  "inflow",
  "outflow",
  "bottomElevation",
  "topElevation",
  "minCoverDepth",
  "maxCoverDepth",
  "payItems",
];

function humanizeToken(token: string): string {
  return token
    .replace(/^civil3d_/i, "")
    .replace(/_/g, " ")
    .replace(/\s+/g, " ")
    .trim();
}

function titleCase(text: string): string {
  return text
    .split(/\s+/)
    .filter(Boolean)
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");
}

function buildActionSynonyms(action: string): string[] {
  switch (action) {
    case "list":
      return ["list", "show", "what"];
    case "get":
      return ["get", "show", "inspect"];
    case "create":
      return ["create", "make", "add", "new"];
    case "delete":
      return ["delete", "remove"];
    case "rebuild":
      return ["rebuild", "update", "refresh"];
    case "report":
      return ["report", "summary"];
    case "export":
      return ["export"];
    case "import":
      return ["import"];
    case "analyze":
      return ["analyze", "analysis", "check"];
    default:
      return [humanizeToken(action)];
  }
}

function inferDerivedRequiredFields(
  tool: ToolMetadataCatalogEntry,
  operation?: string,
): string[] {
  const toolNameFieldMap: Record<string, string[]> = {
    civil3d_pressure_network_get_info: ["name"],
    civil3d_pressure_network_create: ["name", "partsList"],
    civil3d_pressure_network_delete: ["name"],
    civil3d_pressure_network_assign_parts_list: ["networkName", "partsList"],
    civil3d_pressure_network_set_cover: ["networkName", "minCoverDepth"],
    civil3d_pressure_network_validate: ["networkName"],
    civil3d_pressure_network_export: ["networkName"],
    civil3d_pressure_network_connect: ["targetNetwork", "sourceNetwork"],
    civil3d_pressure_pipe_add: ["networkName", "partName", "startPoint", "endPoint"],
    civil3d_pressure_pipe_get_properties: ["networkName", "pipeName"],
    civil3d_pressure_pipe_resize: ["networkName", "pipeName", "newPartName"],
    civil3d_pressure_fitting_add: ["networkName", "partName", "position"],
    civil3d_pressure_fitting_get_properties: ["networkName", "fittingName"],
    civil3d_pressure_appurtenance_add: ["networkName", "partName", "position"],
    civil3d_surface_create_from_dem: ["name", "filePath"],
    civil3d_surface_statistics_get: ["name"],
    civil3d_surface_analyze_slope: ["name"],
    civil3d_surface_analyze_elevation: ["name"],
    civil3d_surface_analyze_directions: ["name"],
    civil3d_surface_watershed_add: ["name"],
    civil3d_surface_contour_interval_set: ["name", "minorInterval", "majorInterval"],
    civil3d_surface_sample_elevations: ["name"],
    civil3d_surface_volume_calculate: ["baseSurface", "comparisonSurface"],
    civil3d_surface_volume_report: ["baseSurface", "comparisonSurface"],
    civil3d_surface_volume_by_region: ["baseSurface", "comparisonSurface", "boundary"],
  };

  const operationFieldMap: Record<string, string[]> = {
    get: ["name"],
    delete: ["name"],
    rebuild: ["name"],
    report: ["name"],
    get_pipe: ["networkName", "pipeName"],
    get_structure: ["networkName", "structureName"],
    check_interference: ["networkName", "targetType", "targetName"],
    create_from_surface: ["alignmentName", "profileName", "surfaceName"],
    create_layout: ["alignmentName", "profileName"],
    get_elevation: ["name"],
    get_statistics: ["name"],
    statistics_get: ["name"],
    analyze_slope: ["name"],
    analyze_elevation: ["name"],
    analyze_directions: ["name"],
    watershed_add: ["name"],
    contour_interval_set: ["name", "minorInterval", "majorInterval"],
    sample_elevations: ["name"],
    create_from_dem: ["name", "filePath"],
    get_pressure_network: ["name"],
    create_pressure_network: ["name", "partsList"],
    delete_pressure_network: ["name"],
    assign_pressure_parts_list: ["networkName", "partsList"],
    set_pressure_cover: ["networkName", "minCoverDepth"],
    validate_pressure_network: ["networkName"],
    export_pressure_network: ["networkName"],
    connect_pressure_networks: ["targetNetwork", "sourceNetwork"],
    add_pressure_pipe: ["networkName", "partName", "startPoint", "endPoint"],
    get_pressure_pipe_properties: ["networkName", "pipeName"],
    resize_pressure_pipe: ["networkName", "pipeName", "newPartName"],
    add_pressure_fitting: ["networkName", "partName", "position"],
    get_pressure_fitting_properties: ["networkName", "fittingName"],
    add_pressure_appurtenance: ["networkName", "partName", "position"],
  };

  if (tool.toolName === "civil3d_health" || tool.toolName === "get_drawing_info") {
    return [];
  }

  const toolNameFields = toolNameFieldMap[tool.toolName];
  if (toolNameFields) {
    return toolNameFields;
  }

  if (!operation) {
    return [];
  }

  if (
    operation === "list" ||
    operation === "info" ||
    operation === "settings" ||
    operation === "status" ||
    operation === "health" ||
    operation.startsWith("list_")
  ) {
    return [];
  }

  const operationFields = operationFieldMap[operation];
  if (operationFields) {
    return operationFields;
  }

  return [];
}

function buildDerivedTitle(tool: ToolMetadataCatalogEntry, operation?: string): string {
  if (!operation) {
    return tool.displayName;
  }

  const readableAction = titleCase(humanizeToken(operation));
  const normalizedDisplay = tool.displayName.toLowerCase();
  return normalizedDisplay.includes(humanizeToken(operation))
    ? tool.displayName
    : `${tool.displayName} ${readableAction}`;
}

function buildDerivedKeywords(tool: ToolMetadataCatalogEntry, operation?: string): string[] {
  const keywordSet = new Set<string>();
  const humanToolName = humanizeToken(tool.toolName);
  const displayName = tool.displayName.replace(/^Civil 3D\s+/i, "").trim();

  keywordSet.add(tool.toolName);
  keywordSet.add(humanToolName);
  keywordSet.add(tool.displayName);
  keywordSet.add(displayName);
  keywordSet.add(tool.domain);

  if (operation) {
    const humanOperation = humanizeToken(operation);
    keywordSet.add(operation);
    keywordSet.add(humanOperation);
    keywordSet.add(`${tool.domain} ${humanOperation}`);
    keywordSet.add(`${displayName} ${humanOperation}`);

    for (const synonym of buildActionSynonyms(operation)) {
      keywordSet.add(`${synonym} ${tool.domain}`);
      keywordSet.add(`${synonym} ${displayName}`);
    }
  }

  for (const capability of tool.capabilities) {
    keywordSet.add(`${capability} ${tool.domain}`);
    keywordSet.add(`${capability} ${displayName}`);
  }

  if (tool.operations) {
    for (const listedOperation of tool.operations) {
      keywordSet.add(`${displayName} ${humanizeToken(listedOperation)}`);
    }
  }

  return [...keywordSet].filter((keyword) => keyword.trim().length > 0);
}

function buildDerivedToolArgs(operation?: string) {
  return (params: Record<string, unknown>) => {
    const picked = pickDefined(params, ROUTE_PARAM_FIELDS);
    return operation ? { action: operation, ...picked } : picked;
  };
}

function buildDerivedRouteEntries(): ToolCatalogEntry[] {
  const explicitKeys = new Set(
    EXPLICIT_TOOL_CATALOG.map((entry) => `${entry.toolName}::${entry.action}`),
  );

  const derivedEntries: ToolCatalogEntry[] = [];

  for (const tool of TOOL_METADATA_CATALOG) {
    if (tool.status !== "implemented" || tool.toolName === "civil3d_orchestrate") {
      continue;
    }

    const operations = tool.operations && tool.operations.length > 0
      ? tool.operations
      : [undefined];

    for (const operation of operations) {
      const action = operation ?? "execute";
      const key = `${tool.toolName}::${action}`;
      if (explicitKeys.has(key)) {
        continue;
      }

      derivedEntries.push({
        intent: `tool:${tool.toolName}:${action}`,
        title: buildDerivedTitle(tool, operation),
        domain: tool.domain,
        toolName: tool.toolName,
        action,
        keywords: buildDerivedKeywords(tool, operation),
        requiredFields: inferDerivedRequiredFields(tool, operation),
        description: tool.description,
        buildToolArgs: buildDerivedToolArgs(operation),
        source: "derived",
      });
    }
  }

  return derivedEntries;
}

export const TOOL_CATALOG: ToolCatalogEntry[] = [
  ...EXPLICIT_TOOL_CATALOG.map((entry) => ({ ...entry, source: "explicit" as const })),
  ...buildDerivedRouteEntries(),
];

export function findToolCatalogEntry(toolName: string, action?: string): ToolCatalogEntry | undefined {
  if (action) {
    return TOOL_CATALOG.find(
      (entry) => entry.toolName === toolName && entry.action === action,
    );
  }

  const exactToolMatch = TOOL_CATALOG.find((entry) => entry.toolName === toolName);
  if (exactToolMatch) {
    return exactToolMatch;
  }

  return undefined;
}
