import { RouteParams } from "./IntentRouter.js";
import { ProjectContext } from "./ProjectContextService.js";

interface SelectedObjectInfo {
  objectType?: string;
  name?: string;
}

export interface SelectionResolution {
  resolvedParams: RouteParams;
  inferredFromSelection: string[];
}

function asSelectedObjects(projectContext: ProjectContext): SelectedObjectInfo[] {
  return Array.isArray(projectContext.selectedObjects)
    ? (projectContext.selectedObjects as SelectedObjectInfo[])
    : [];
}

function findSelectedNameByType(projectContext: ProjectContext, objectTypeMatch: string): string | undefined {
  const matches = asSelectedObjects(projectContext).filter((item) => {
    const objectType = item.objectType?.toLowerCase() ?? "";
    return objectType.includes(objectTypeMatch);
  });

  if (matches.length !== 1) {
    return undefined;
  }

  const selectedName = matches[0]?.name?.trim();
  return selectedName && selectedName.length > 0 ? selectedName : undefined;
}

function findFirstSelectedName(projectContext: ProjectContext, objectTypeMatches: string[]): string | undefined {
  for (const objectTypeMatch of objectTypeMatches) {
    const selectedName = findSelectedNameByType(projectContext, objectTypeMatch);
    if (selectedName) {
      return selectedName;
    }
  }

  return undefined;
}

export function resolveParamsFromSelection(params: RouteParams, projectContext: ProjectContext): SelectionResolution {
  const resolvedParams: RouteParams = { ...params };
  const inferredFromSelection: string[] = [];

  if (!resolvedParams.alignmentName) {
    const selectedAlignmentName = findSelectedNameByType(projectContext, "alignment");
    if (selectedAlignmentName) {
      resolvedParams.alignmentName = selectedAlignmentName;
      inferredFromSelection.push("alignmentName");
    }
  }

  if (!resolvedParams.surfaceName) {
    const selectedSurfaceName = findSelectedNameByType(projectContext, "surface");
    if (selectedSurfaceName) {
      resolvedParams.surfaceName = selectedSurfaceName;
      inferredFromSelection.push("surfaceName");
    }
  }

  if (!resolvedParams.name) {
    const selectedObjectName = findFirstSelectedName(projectContext, ["surface", "alignment", "corridor"]);
    if (selectedObjectName) {
      resolvedParams.name = selectedObjectName;
      inferredFromSelection.push("name");
    }
  }

  return {
    resolvedParams,
    inferredFromSelection,
  };
}
