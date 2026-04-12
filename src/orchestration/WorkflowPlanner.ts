import { RouteParams, RouteResult } from "./IntentRouter.js";
import { ProjectContext } from "./ProjectContextService.js";

export interface WorkflowStep {
  title: string;
  toolName: string;
  action: string;
  requiredFields: string[];
  status: "ready" | "missing_input" | "blocked";
  notes?: string;
}

export interface WorkflowPlan {
  kind: "single_action" | "workflow";
  title: string;
  summary: string;
  steps: WorkflowStep[];
  missingFields: string[];
  clarificationQuestions: string[];
}

function getObjectCounts(projectContext: ProjectContext) {
  const drawingInfo = (projectContext.drawingInfo ?? {}) as {
    objectCounts?: {
      surfaces?: number;
      alignments?: number;
      profiles?: number;
      corridors?: number;
    };
  };

  return drawingInfo.objectCounts ?? {};
}

function findMissingFields(params: RouteParams, requiredFields: string[]) {
  return requiredFields.filter((field) => {
    const value = params[field as keyof RouteParams];
    if (typeof value === "string") {
      return value.trim().length === 0;
    }

    if (typeof value === "number") {
      return !Number.isFinite(value);
    }

    return value == null;
  });
}

function buildFieldClarificationQuestions(fields: string[]): string[] {
  return fields.map((field) => {
    switch (field) {
      case "alignmentName":
        return "Which alignment should be used?";
      case "surfaceName":
        return "Which surface should be used?";
      case "baseSurface":
        return "Which base surface should be used?";
      case "comparisonSurface":
        return "Which comparison surface should be used?";
      case "profileName":
        return "What should the profile be named?";
      case "filePath":
        return "What file path should be used?";
      case "outputPath":
        return "What output file path should be used?";
      case "gridSpacing":
        return "What grid spacing should be used?";
      case "designSpeed":
        return "What design speed should be used?";
      case "inflow":
        return "What peak inflow should be used?";
      case "outflow":
        return "What allowable outflow should be used?";
      case "bottomElevation":
        return "What bottom elevation should be used?";
      case "topElevation":
        return "What top elevation should be used?";
      case "payItems":
        return "Please provide the pay item definitions and unit prices.";
      case "name":
        return "Which named Civil 3D object should be used?";
      default:
        return `Please provide ${field}.`;
    }
  });
}

export function buildWorkflowPlan(routed: RouteResult, params: RouteParams, projectContext: ProjectContext): WorkflowPlan {
  const objectCounts = getObjectCounts(projectContext);
  const hasSurfaces = (objectCounts.surfaces ?? 0) > 0;
  const hasAlignments = (objectCounts.alignments ?? 0) > 0;
  const hasProfiles = (objectCounts.profiles ?? 0) > 0;
  const missingFields = findMissingFields(params, routed.match.requiredFields);

  if (routed.match.intent === "corridor_prerequisites") {
    const clarificationQuestions: string[] = [];
    if (!hasAlignments) {
      clarificationQuestions.push("Which alignment should the corridor be based on?");
    }
    if (!hasSurfaces) {
      clarificationQuestions.push("Which existing ground surface should be used?");
    }
    if (!hasProfiles) {
      clarificationQuestions.push("Do you want to create an EG profile, a layout profile, or both before corridor creation?");
    }

    return {
      kind: "workflow",
      title: "Corridor readiness workflow",
      summary: "Checks the typical prerequisite sequence before corridor creation.",
      steps: [
        {
          title: "Confirm alignment",
          toolName: "civil3d_alignment",
          action: "list",
          requiredFields: [],
          status: hasAlignments ? "ready" : "blocked",
          notes: hasAlignments ? "At least one alignment appears to exist in the drawing." : "No alignments detected in drawing info.",
        },
        {
          title: "Confirm existing ground surface",
          toolName: "civil3d_surface",
          action: "list",
          requiredFields: [],
          status: hasSurfaces ? "ready" : "blocked",
          notes: hasSurfaces ? "At least one surface appears to exist in the drawing." : "No surfaces detected in drawing info.",
        },
        {
          title: "Confirm profile availability",
          toolName: "civil3d_profile",
          action: "list",
          requiredFields: ["alignmentName"],
          status: hasProfiles ? "ready" : "blocked",
          notes: hasProfiles ? "At least one profile appears to exist in the drawing." : "No profiles detected in drawing info.",
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "generate_surface_volume_report") {
    const clarificationQuestions: string[] = [];
    if (!params.baseSurface) {
      clarificationQuestions.push("Which base surface should be used for the volume report?");
    }
    if (!params.comparisonSurface) {
      clarificationQuestions.push("Which comparison surface should be used for the volume report?");
    }

    return {
      kind: "workflow",
      title: "Surface volume report workflow",
      summary: "Plans the surface pair needed before generating a formatted cut/fill report.",
      steps: [
        {
          title: "Review available surfaces",
          toolName: "civil3d_surface",
          action: "list",
          requiredFields: [],
          status: hasSurfaces ? "ready" : "blocked",
          notes: hasSurfaces ? "Surface objects are present in the drawing." : "No surfaces detected in drawing info.",
        },
        {
          title: "Generate surface volume report",
          toolName: "civil3d_surface_volume_report",
          action: "generate",
          requiredFields: ["baseSurface", "comparisonSurface"],
          status: missingFields.length === 0 && hasSurfaces ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for surface volume reporting." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "sample_surface_elevations") {
    const clarificationQuestions: string[] = [];
    if (!params.name) {
      clarificationQuestions.push("Which surface should be sampled?");
    }
    if (params.gridSpacing == null) {
      clarificationQuestions.push("What grid spacing should be used for the surface sampling?");
    }

    return {
      kind: "workflow",
      title: "Surface sampling workflow",
      summary: "Plans the surface and grid spacing required before sampling elevations.",
      steps: [
        {
          title: "Review available surfaces",
          toolName: "civil3d_surface",
          action: "list",
          requiredFields: [],
          status: hasSurfaces ? "ready" : "blocked",
          notes: hasSurfaces ? "Surface objects are present in the drawing." : "No surfaces detected in drawing info.",
        },
        {
          title: "Sample surface elevations",
          toolName: "civil3d_surface_sample_elevations",
          action: "sample",
          requiredFields: ["name", "gridSpacing"],
          status: missingFields.length === 0 && hasSurfaces ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for grid-based surface sampling." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "create_surface_from_dem") {
    const clarificationQuestions: string[] = [];
    if (!params.name) {
      clarificationQuestions.push("What should the new surface be named?");
    }
    if (!params.filePath) {
      clarificationQuestions.push("What DEM or raster file path should be imported?");
    }

    return {
      kind: "workflow",
      title: "Create surface from DEM workflow",
      summary: "Plans the DEM file path and target surface name needed for DEM import.",
      steps: [
        {
          title: "Confirm DEM file path",
          toolName: "civil3d_surface_create_from_dem",
          action: "create",
          requiredFields: ["filePath"],
          status: params.filePath ? "ready" : "missing_input",
          notes: params.filePath ? "A DEM input path has been provided." : "A DEM or raster file path is required.",
        },
        {
          title: "Create surface from DEM",
          toolName: "civil3d_surface_create_from_dem",
          action: "create",
          requiredFields: ["name", "filePath"],
          status: missingFields.length === 0 ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for DEM surface creation." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "list_profiles") {
    const clarificationQuestions: string[] = [];
    if (!params.alignmentName) {
      clarificationQuestions.push("Which alignment should be used to list profiles?");
    }

    return {
      kind: "workflow",
      title: "List profiles workflow",
      summary: "Plans the alignment selection needed before listing profiles.",
      steps: [
        {
          title: "Review available alignments",
          toolName: "civil3d_alignment",
          action: "list",
          requiredFields: [],
          status: hasAlignments ? "ready" : "blocked",
          notes: hasAlignments ? "Alignment objects are present in the drawing." : "No alignments detected in drawing info.",
        },
        {
          title: "List profiles for selected alignment",
          toolName: "civil3d_profile",
          action: "list",
          requiredFields: ["alignmentName"],
          status: missingFields.length === 0 && hasAlignments ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available to list alignment profiles." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "create_profile_from_surface") {
    const clarificationQuestions: string[] = [];
    if (!params.alignmentName) {
      clarificationQuestions.push("Which alignment should be used to create the profile?");
    }
    if (!params.surfaceName) {
      clarificationQuestions.push("Which surface should be sampled for the profile?");
    }
    if (!params.profileName) {
      clarificationQuestions.push("What should the new profile be named?");
    }

    return {
      kind: "workflow",
      title: "Create profile from surface workflow",
      summary: "Plans the inputs and prerequisite checks needed to create a profile from an alignment and surface.",
      steps: [
        {
          title: "Review available alignments",
          toolName: "civil3d_alignment",
          action: "list",
          requiredFields: [],
          status: hasAlignments ? "ready" : "blocked",
          notes: hasAlignments ? "Alignment objects are present in the drawing." : "No alignments detected in drawing info.",
        },
        {
          title: "Review available surfaces",
          toolName: "civil3d_surface",
          action: "list",
          requiredFields: [],
          status: hasSurfaces ? "ready" : "blocked",
          notes: hasSurfaces ? "Surface objects are present in the drawing." : "No surfaces detected in drawing info.",
        },
        {
          title: "Create profile from selected surface",
          toolName: "civil3d_profile",
          action: "create_from_surface",
          requiredFields: ["alignmentName", "profileName", "surfaceName"],
          status: missingFields.length === 0 && hasAlignments && hasSurfaces ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for profile creation." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "create_layout_profile") {
    const clarificationQuestions: string[] = [];
    if (!params.alignmentName) {
      clarificationQuestions.push("Which alignment should the layout profile be created on?");
    }
    if (!params.profileName) {
      clarificationQuestions.push("What should the new layout profile be named?");
    }

    return {
      kind: "workflow",
      title: "Create layout profile workflow",
      summary: "Plans the inputs needed to create a layout profile on an alignment.",
      steps: [
        {
          title: "Review available alignments",
          toolName: "civil3d_alignment",
          action: "list",
          requiredFields: [],
          status: hasAlignments ? "ready" : "blocked",
          notes: hasAlignments ? "Alignment objects are present in the drawing." : "No alignments detected in drawing info.",
        },
        {
          title: "Create layout profile",
          toolName: "civil3d_profile",
          action: "create_layout",
          requiredFields: ["alignmentName", "profileName"],
          status: missingFields.length === 0 && hasAlignments ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for layout profile creation." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "get_surface_statistics" || routed.match.intent === "analyze_surface_slope") {
    const clarificationQuestions: string[] = [];
    if (!params.name) {
      clarificationQuestions.push("Which surface should be analyzed?");
    }

    return {
      kind: "workflow",
      title: routed.match.intent === "get_surface_statistics" ? "Surface statistics workflow" : "Surface slope analysis workflow",
      summary: routed.match.intent === "get_surface_statistics"
        ? "Plans the surface selection needed before retrieving detailed statistics."
        : "Plans the surface selection needed before running slope analysis.",
      steps: [
        {
          title: "Review available surfaces",
          toolName: "civil3d_surface",
          action: "list",
          requiredFields: [],
          status: hasSurfaces ? "ready" : "blocked",
          notes: hasSurfaces ? "Surface objects are present in the drawing." : "No surfaces detected in drawing info.",
        },
        {
          title: routed.match.intent === "get_surface_statistics" ? "Get surface statistics" : "Analyze surface slope",
          toolName: routed.match.toolName,
          action: routed.match.action,
          requiredFields: ["name"],
          status: missingFields.length === 0 && hasSurfaces ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for the selected surface analysis." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "calculate_surface_volume") {
    const clarificationQuestions: string[] = [];
    if (!params.baseSurface) {
      clarificationQuestions.push("Which base surface should be used for the volume comparison?");
    }
    if (!params.comparisonSurface) {
      clarificationQuestions.push("Which comparison surface should be used for the volume comparison?");
    }

    return {
      kind: "workflow",
      title: "Surface volume workflow",
      summary: "Plans the surface pair needed for a cut/fill volume comparison.",
      steps: [
        {
          title: "Review available surfaces",
          toolName: "civil3d_surface",
          action: "list",
          requiredFields: [],
          status: hasSurfaces ? "ready" : "blocked",
          notes: hasSurfaces ? "Surface objects are present in the drawing." : "No surfaces detected in drawing info.",
        },
        {
          title: "Calculate surface volume",
          toolName: "civil3d_surface_volume_calculate",
          action: "calculate",
          requiredFields: ["baseSurface", "comparisonSurface"],
          status: missingFields.length === 0 && hasSurfaces ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for surface volume calculation." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "calculate_sight_distance") {
    const clarificationQuestions: string[] = [];
    if (params.designSpeed == null) {
      clarificationQuestions.push("What design speed should be used for the sight distance calculation?");
    }

    return {
      kind: "single_action",
      title: "Sight distance calculation workflow",
      summary: "Calculates required sight distance from the supplied design speed.",
      steps: [
        {
          title: "Calculate sight distance",
          toolName: "civil3d_sight_distance_calculate",
          action: "calculate",
          requiredFields: ["designSpeed"],
          status: missingFields.length === 0 ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for sight distance calculation." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "check_stopping_distance") {
    const clarificationQuestions: string[] = [];
    if (!params.alignmentName) {
      clarificationQuestions.push("Which alignment should be checked for stopping sight distance?");
    }
    if (!params.profileName) {
      clarificationQuestions.push("Which profile should be checked for stopping sight distance?");
    }
    if (params.designSpeed == null) {
      clarificationQuestions.push("What design speed should be used for the stopping sight distance check?");
    }

    return {
      kind: "workflow",
      title: "Stopping sight distance workflow",
      summary: "Plans the alignment, profile, and design speed inputs required for SSD compliance checking.",
      steps: [
        {
          title: "Review available alignments",
          toolName: "civil3d_alignment",
          action: "list",
          requiredFields: [],
          status: hasAlignments ? "ready" : "blocked",
          notes: hasAlignments ? "Alignment objects are present in the drawing." : "No alignments detected in drawing info.",
        },
        {
          title: "Review profiles for alignment",
          toolName: "civil3d_profile",
          action: "list",
          requiredFields: ["alignmentName"],
          status: hasProfiles ? "ready" : "blocked",
          notes: hasProfiles ? "Profile objects are present in the drawing." : "No profiles detected in drawing info.",
        },
        {
          title: "Check stopping distance",
          toolName: "civil3d_stopping_distance_check",
          action: "check",
          requiredFields: ["alignmentName", "profileName", "designSpeed"],
          status: missingFields.length === 0 && hasAlignments && hasProfiles ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for SSD compliance checking." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "calculate_detention_basin_size") {
    const clarificationQuestions: string[] = [];
    if (params.inflow == null) {
      clarificationQuestions.push("What peak inflow should be used for detention sizing?");
    }
    if (params.outflow == null) {
      clarificationQuestions.push("What allowable outflow should be used for detention sizing?");
    }

    return {
      kind: "single_action",
      title: "Detention basin sizing workflow",
      summary: "Calculates required detention storage from inflow and outflow constraints.",
      steps: [
        {
          title: "Calculate detention basin size",
          toolName: "civil3d_detention_basin_size_calculate",
          action: "calculate",
          requiredFields: ["inflow", "outflow"],
          status: missingFields.length === 0 ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for detention sizing." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "generate_detention_stage_storage") {
    const clarificationQuestions: string[] = [];
    if (!params.surfaceName) {
      clarificationQuestions.push("Which detention basin surface should be used for the stage-storage table?");
    }
    if (params.bottomElevation == null) {
      clarificationQuestions.push("What basin bottom elevation should be used?");
    }
    if (params.topElevation == null) {
      clarificationQuestions.push("What top water surface elevation should be used?");
    }

    return {
      kind: "workflow",
      title: "Detention stage-storage workflow",
      summary: "Plans the surface and elevation inputs needed for a stage-storage-discharge table.",
      steps: [
        {
          title: "Review available surfaces",
          toolName: "civil3d_surface",
          action: "list",
          requiredFields: [],
          status: hasSurfaces ? "ready" : "blocked",
          notes: hasSurfaces ? "Surface objects are present in the drawing." : "No surfaces detected in drawing info.",
        },
        {
          title: "Generate detention stage storage",
          toolName: "civil3d_detention_stage_storage",
          action: "generate",
          requiredFields: ["surfaceName", "bottomElevation", "topElevation"],
          status: missingFields.length === 0 && hasSurfaces ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for detention stage-storage generation." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "calculate_slope_geometry" || routed.match.intent === "check_slope_stability") {
    const clarificationQuestions: string[] = [];
    if (!params.alignmentName) {
      clarificationQuestions.push(routed.match.intent === "calculate_slope_geometry"
        ? "Which alignment should be used for the slope geometry calculation?"
        : "Which alignment should be checked for slope stability?");
    }

    return {
      kind: "workflow",
      title: routed.match.intent === "calculate_slope_geometry" ? "Slope geometry workflow" : "Slope stability workflow",
      summary: routed.match.intent === "calculate_slope_geometry"
        ? "Plans the alignment selection needed before calculating slope daylight geometry."
        : "Plans the alignment selection needed before checking slope stability.",
      steps: [
        {
          title: "Review available alignments",
          toolName: "civil3d_alignment",
          action: "list",
          requiredFields: [],
          status: hasAlignments ? "ready" : "blocked",
          notes: hasAlignments ? "Alignment objects are present in the drawing." : "No alignments detected in drawing info.",
        },
        {
          title: routed.match.intent === "calculate_slope_geometry" ? "Calculate slope geometry" : "Check slope stability",
          toolName: routed.match.toolName,
          action: routed.match.action,
          requiredFields: ["alignmentName"],
          status: missingFields.length === 0 && hasAlignments ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for the selected slope analysis." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "export_pay_items") {
    const clarificationQuestions: string[] = [];
    if (!params.outputPath) {
      clarificationQuestions.push("What output file path should be used for the pay item export?");
    }

    return {
      kind: "workflow",
      title: "Pay item export workflow",
      summary: "Plans the output path and optional scoping inputs for pay item export.",
      steps: [
        {
          title: "Set export output path",
          toolName: "civil3d_pay_items_export",
          action: "export",
          requiredFields: ["outputPath"],
          status: params.outputPath ? "ready" : "missing_input",
          notes: params.outputPath ? "An output file path has been provided." : "An output file path is required for pay item export.",
        },
        {
          title: "Export pay items",
          toolName: "civil3d_pay_items_export",
          action: "export",
          requiredFields: ["outputPath"],
          status: missingFields.length === 0 ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for pay item export." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  if (routed.match.intent === "estimate_material_cost") {
    const clarificationQuestions: string[] = [];
    if (!params.payItems || params.payItems.length === 0) {
      clarificationQuestions.push("Please provide the pay item definitions with unit prices for the cost estimate.");
    }

    return {
      kind: "workflow",
      title: "Material cost estimate workflow",
      summary: "Plans the pricing inputs needed before generating a construction cost estimate.",
      steps: [
        {
          title: "Provide pay item pricing",
          toolName: "civil3d_material_cost_estimate",
          action: "estimate",
          requiredFields: ["payItems"],
          status: params.payItems && params.payItems.length > 0 ? "ready" : "missing_input",
          notes: params.payItems && params.payItems.length > 0 ? "Pay item pricing has been supplied." : "Pay item definitions and unit prices are required for cost estimation.",
        },
        {
          title: "Generate material cost estimate",
          toolName: "civil3d_material_cost_estimate",
          action: "estimate",
          requiredFields: ["payItems"],
          status: missingFields.length === 0 ? "ready" : "missing_input",
          notes: missingFields.length === 0 ? "Required inputs are available for material cost estimation." : `Missing required inputs: ${missingFields.join(", ")}`,
        },
      ],
      missingFields,
      clarificationQuestions,
    };
  }

  return {
    kind: "single_action",
    title: routed.match.title,
    summary: routed.match.description,
    steps: [
      {
        title: routed.match.title,
        toolName: routed.match.toolName,
        action: routed.match.action,
        requiredFields: routed.match.requiredFields,
        status: missingFields.length === 0 ? "ready" : "missing_input",
        notes: missingFields.length === 0 ? "This action is ready to execute." : `Missing required inputs: ${missingFields.join(", ")}`,
      },
    ],
    missingFields,
    clarificationQuestions: buildFieldClarificationQuestions(missingFields),
  };
}
