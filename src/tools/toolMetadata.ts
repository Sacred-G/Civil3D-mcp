export type ToolDomain =
  | "drawing"
  | "geometry"
  | "workflow"
  | "grading"
  | "point"
  | "surface"
  | "alignment"
  | "profile"
  | "corridor"
  | "section"
  | "pipe"
  | "parcel"
  | "quantity_takeoff"
  | "standards"
  | "qc"
  | "labeling"
  | "coordinate_system"
  | "data_shortcut"
  | "assembly"
  | "project"
  | "feature_line"
  | "style"
  | "job"
  | "hydrology"
  | "sight_distance"
  | "detention"
  | "slope_analysis"
  | "cost_estimation"
  | "survey"
  | "plan_production"
  | "docs"
  | "catchment"
  | "stm"
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
  | "export"
  | "import";

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
