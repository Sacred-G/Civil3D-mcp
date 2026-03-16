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
    return typeof value !== "string" || value.trim().length === 0;
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
      missingFields: [],
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
    clarificationQuestions: missingFields.map((field) => `Please provide ${field}.`),
  };
}
