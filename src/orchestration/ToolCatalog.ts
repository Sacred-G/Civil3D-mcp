export type OrchestratorIntent =
  | "drawing_info"
  | "list_tool_capabilities"
  | "list_surfaces"
  | "get_surface"
  | "create_surface"
  | "get_surface_statistics"
  | "analyze_surface_slope"
  | "calculate_surface_volume"
  | "generate_surface_volume_report"
  | "sample_surface_elevations"
  | "create_surface_from_dem"
  | "list_alignments"
  | "get_alignment"
  | "list_profiles"
  | "create_profile_from_surface"
  | "create_layout_profile"
  | "calculate_sight_distance"
  | "check_stopping_distance"
  | "calculate_detention_basin_size"
  | "generate_detention_stage_storage"
  | "calculate_slope_geometry"
  | "check_slope_stability"
  | "export_pay_items"
  | "estimate_material_cost"
  | "list_corridors"
  | "get_corridor"
  | "rebuild_corridor"
  | "corridor_prerequisites";

export interface ToolCatalogEntry {
  intent: OrchestratorIntent;
  title: string;
  domain: "drawing" | "docs" | "surface" | "alignment" | "profile" | "sight_distance" | "detention" | "slope_analysis" | "cost_estimation" | "corridor";
  toolName: string;
  action: string;
  keywords: string[];
  requiredFields: string[];
  description: string;
}

export const TOOL_CATALOG: ToolCatalogEntry[] = [
  {
    intent: "drawing_info",
    title: "Drawing information",
    domain: "drawing",
    toolName: "civil3d_drawing",
    action: "info",
    keywords: ["drawing", "drawing info", "drawing information", "project info", "project information", "what is in this drawing"],
    requiredFields: [],
    description: "Gets document-level information and object counts for the active drawing.",
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
  },
];
