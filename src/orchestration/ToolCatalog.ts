export type OrchestratorIntent =
  | "drawing_info"
  | "list_surfaces"
  | "create_surface"
  | "list_alignments"
  | "create_profile_from_surface"
  | "corridor_prerequisites";

export interface ToolCatalogEntry {
  intent: OrchestratorIntent;
  title: string;
  domain: "drawing" | "surface" | "alignment" | "profile" | "corridor";
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
