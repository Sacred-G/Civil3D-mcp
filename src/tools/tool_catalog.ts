export type ToolDomain =
  | "drawing"
  | "geometry"
  | "point"
  | "surface"
  | "alignment"
  | "profile"
  | "corridor"
  | "section"
  | "pipe"
  | "parcel"
  | "labeling"
  | "coordinate_system"
  | "data_shortcut"
  | "assembly"
  | "feature_line"
  | "style"
  | "job"
  | "hydrology"
  | "docs"
  | "plugin";

export type ToolCapability =
  | "query"
  | "create"
  | "edit"
  | "delete"
  | "analyze"
  | "manage"
  | "generate"
  | "register"
  | "inspect"
  | "export";

export interface ToolCatalogEntry {
  toolName: string;
  displayName: string;
  description: string;
  domain: ToolDomain;
  capabilities: ToolCapability[];
  operations?: string[];
  pluginMethods?: string[];
  requiresActiveDrawing: boolean;
  safeForRetry: boolean;
  status: "implemented" | "planned";
}

export const TOOL_CATALOG: ToolCatalogEntry[] = [
  {
    toolName: "civil3d_health",
    displayName: "Civil 3D Health",
    description: "Reports connection and plugin runtime health.",
    domain: "plugin",
    capabilities: ["inspect", "query"],
    pluginMethods: ["getCivil3DHealth"],
    requiresActiveDrawing: false,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_drawing",
    displayName: "Civil 3D Drawing",
    description: "Reads and manages drawing state, settings, save, undo, redo, and new drawing creation.",
    domain: "drawing",
    capabilities: ["query", "create", "manage"],
    operations: ["info", "settings", "new", "save", "undo", "redo"],
    pluginMethods: ["getDrawingInfo", "getDrawingSettings", "newDrawing", "saveDrawing", "undoDrawing", "redoDrawing"],
    requiresActiveDrawing: false,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "get_drawing_info",
    displayName: "Get Drawing Info",
    description: "Returns basic drawing information.",
    domain: "drawing",
    capabilities: ["query", "inspect"],
    pluginMethods: ["getDrawingInfo"],
    requiresActiveDrawing: false,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "list_civil_object_types",
    displayName: "List Civil Object Types",
    description: "Lists major Civil 3D object types available in the current context.",
    domain: "docs",
    capabilities: ["query", "inspect"],
    pluginMethods: ["listCivilObjectTypes"],
    requiresActiveDrawing: false,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "get_selected_civil_objects_info",
    displayName: "Get Selected Civil Objects Info",
    description: "Returns information about currently selected Civil 3D objects.",
    domain: "drawing",
    capabilities: ["query", "inspect"],
    pluginMethods: ["getSelectedCivilObjectsInfo"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_alignment",
    displayName: "Civil 3D Alignment",
    description: "Lists, inspects, creates, deletes, and maps stations for alignments.",
    domain: "alignment",
    capabilities: ["query", "create", "delete", "analyze"],
    operations: ["list", "get", "station_to_point", "point_to_station", "create", "delete"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_alignment_report",
    displayName: "Civil 3D Alignment Report",
    description: "Builds a structured alignment report by fetching alignment geometry and sampling station locations.",
    domain: "alignment",
    capabilities: ["query", "analyze", "generate"],
    pluginMethods: ["getAlignment", "alignmentStationToPoint"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_profile",
    displayName: "Civil 3D Profile",
    description: "Lists, inspects, samples, creates, and deletes profiles.",
    domain: "profile",
    capabilities: ["query", "create", "delete", "analyze"],
    operations: ["list", "get", "get_elevation", "sample_elevations", "create_from_surface", "create_layout", "delete"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_profile_report",
    displayName: "Civil 3D Profile Report",
    description: "Builds a structured profile report from profile detail and sampled elevations.",
    domain: "profile",
    capabilities: ["query", "analyze", "generate"],
    pluginMethods: ["getProfile", "sampleProfileElevations"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_surface",
    displayName: "Civil 3D Surface",
    description: "Lists, inspects, analyzes, creates, and deletes surfaces.",
    domain: "surface",
    capabilities: ["query", "create", "delete", "analyze"],
    operations: ["list", "get", "get_elevation", "get_elevation_along", "get_statistics", "create", "delete"],
    pluginMethods: ["listSurfaces", "getSurface", "getSurfaceElevation", "getSurfaceElevationsAlong", "getSurfaceStatistics", "createSurface", "deleteSurface"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_surface_edit",
    displayName: "Civil 3D Surface Edit",
    description: "Edits and analyzes surface definitions including points, breaklines, boundaries, contours, and volume.",
    domain: "surface",
    capabilities: ["edit", "analyze"],
    operations: ["add_points", "add_breakline", "add_boundary", "extract_contours", "compute_volume"],
    pluginMethods: ["addSurfacePoints", "addSurfaceBreakline", "addSurfaceBoundary", "extractSurfaceContours", "computeSurfaceVolume"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "civil3d_surface_drainage_workflow",
    displayName: "Civil 3D Surface Drainage Workflow",
    description: "Builds a structured drainage analysis by fetching a surface, tracing flow, sampling elevations, and estimating runoff.",
    domain: "hydrology",
    capabilities: ["query", "analyze", "generate"],
    pluginMethods: ["getSurface", "traceHydrologyFlowPath", "getSurfaceElevationsAlong", "estimateHydrologyRunoff"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_surface_comparison_workflow",
    displayName: "Civil 3D Surface Comparison Workflow",
    description: "Builds a structured comparison between two surfaces and computes cut/fill volume differences.",
    domain: "surface",
    capabilities: ["query", "analyze", "generate"],
    pluginMethods: ["getSurface", "computeSurfaceVolume"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_corridor",
    displayName: "Civil 3D Corridor",
    description: "Lists, inspects, rebuilds, and reads corridor surfaces and feature lines.",
    domain: "corridor",
    capabilities: ["query", "manage", "analyze"],
    operations: ["list", "get", "rebuild", "get_surfaces", "get_feature_lines", "compute_volumes"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "civil3d_corridor_summary",
    displayName: "Civil 3D Corridor Summary",
    description: "Builds a structured corridor summary from corridor detail, corridor surfaces, and optional volume analysis.",
    domain: "corridor",
    capabilities: ["query", "analyze", "generate"],
    pluginMethods: ["getCorridor", "getCorridorSurfaces", "computeCorridorVolumes"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_section",
    displayName: "Civil 3D Section",
    description: "Lists sample line groups, gets section data, and creates sample lines.",
    domain: "section",
    capabilities: ["query", "create"],
    operations: ["list_sample_line_groups", "get_section_data", "create_sample_lines"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "civil3d_point",
    displayName: "Civil 3D Point",
    description: "Lists, gets, creates, imports, and deletes COGO points and point groups.",
    domain: "point",
    capabilities: ["query", "create", "delete", "manage"],
    operations: ["list", "get", "create", "list_groups", "import", "delete"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "create_cogo_point",
    displayName: "Create COGO Point",
    description: "Creates COGO points in the current drawing.",
    domain: "point",
    capabilities: ["create"],
    pluginMethods: ["createCogoPoints"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "create_line_segment",
    displayName: "Create Line Segment",
    description: "Creates a line segment in the current drawing.",
    domain: "geometry",
    capabilities: ["create"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "acad_create_polyline",
    displayName: "Create AutoCAD Polyline",
    description: "Creates a 2D polyline in model space.",
    domain: "geometry",
    capabilities: ["create"],
    pluginMethods: ["createPolyline"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "acad_create_3dpolyline",
    displayName: "Create AutoCAD 3D Polyline",
    description: "Creates a 3D polyline in model space.",
    domain: "geometry",
    capabilities: ["create"],
    pluginMethods: ["create3dPolyline"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "acad_create_text",
    displayName: "Create AutoCAD Text",
    description: "Creates DBText in model space.",
    domain: "labeling",
    capabilities: ["create"],
    pluginMethods: ["createText"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "acad_create_mtext",
    displayName: "Create AutoCAD MText",
    description: "Creates MText in model space.",
    domain: "labeling",
    capabilities: ["create"],
    pluginMethods: ["createMText"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "civil3d_parcel",
    displayName: "Civil 3D Parcel",
    description: "Queries and manages parcel data.",
    domain: "parcel",
    capabilities: ["query", "manage"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_pipe_network",
    displayName: "Civil 3D Pipe Network",
    description: "Queries pipe network data.",
    domain: "pipe",
    capabilities: ["query", "inspect"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_pipe_network_edit",
    displayName: "Civil 3D Pipe Network Edit",
    description: "Edits pipe network data.",
    domain: "pipe",
    capabilities: ["edit", "manage"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "civil3d_label",
    displayName: "Civil 3D Label",
    description: "Works with Civil 3D labels and annotation.",
    domain: "labeling",
    capabilities: ["query", "create", "edit"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "civil3d_coordinate_system",
    displayName: "Civil 3D Coordinate System",
    description: "Queries coordinate system information.",
    domain: "coordinate_system",
    capabilities: ["query", "inspect"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_data_shortcut",
    displayName: "Civil 3D Data Shortcut",
    description: "Works with Civil 3D data shortcuts.",
    domain: "data_shortcut",
    capabilities: ["query", "manage"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_assembly",
    displayName: "Civil 3D Assembly",
    description: "Lists and inspects assemblies and subassemblies.",
    domain: "assembly",
    capabilities: ["query", "inspect"],
    operations: ["list", "get"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_feature_line",
    displayName: "Civil 3D Feature Line",
    description: "Lists feature lines and exports them as 3D polylines.",
    domain: "feature_line",
    capabilities: ["query", "create"],
    requiresActiveDrawing: true,
    safeForRetry: false,
    status: "implemented",
  },
  {
    toolName: "civil3d_style",
    displayName: "Civil 3D Style",
    description: "Queries available Civil 3D styles.",
    domain: "style",
    capabilities: ["query", "inspect"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_job",
    displayName: "Civil 3D Job",
    description: "Inspects and manages background job state.",
    domain: "job",
    capabilities: ["query", "manage"],
    requiresActiveDrawing: false,
    safeForRetry: true,
    status: "implemented",
  },
  {
    toolName: "civil3d_hydrology",
    displayName: "Civil 3D Hydrology",
    description: "Provides hydrology-oriented capability discovery, surface-based flow path tracing, low-point search, and runoff estimation.",
    domain: "hydrology",
    capabilities: ["query", "analyze"],
    operations: ["list_capabilities", "trace_flow_path", "find_low_point", "estimate_runoff", "watershed"],
    pluginMethods: ["listHydrologyCapabilities", "traceHydrologyFlowPath", "findHydrologyLowPoint", "estimateHydrologyRunoff"],
    requiresActiveDrawing: true,
    safeForRetry: true,
    status: "implemented",
  },
];

export function listToolCatalog() {
  return TOOL_CATALOG;
}

export function listDomains() {
  return [...new Set(TOOL_CATALOG.map((entry) => entry.domain))].sort();
}
