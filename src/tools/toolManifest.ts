import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import {
  buildDomainToolCatalogEntries,
  captureDomainToolHandlers,
  registerSelectedDomainTools,
  type DomainActionDefinition,
  type DomainToolDefinition,
  type DomainToolExposure,
} from "./domainRuntime.js";
import { ALIGNMENT_DOMAIN_DEFINITION } from "./domains/alignmentDomain.js";
import { SURFACE_DOMAIN_DEFINITION } from "./domains/surfaceDomain.js";
import { PROFILE_DOMAIN_DEFINITION } from "./domains/profileDomain.js";
import { CORRIDOR_DOMAIN_DEFINITION } from "./domains/corridorDomain.js";
import { SECTION_DOMAIN_DEFINITION } from "./domains/sectionDomain.js";
import { PIPE_DOMAIN_DEFINITION } from "./domains/pipeDomain.js";
import { ASSEMBLY_DOMAIN_DEFINITION } from "./domains/assemblyDomain.js";
import { POINT_DOMAIN_DEFINITION } from "./domains/pointDomain.js";
import { GRADING_DOMAIN_DEFINITION } from "./domains/gradingDomain.js";
import { PARCEL_DOMAIN_DEFINITION } from "./domains/parcelDomain.js";
import { SURVEY_DOMAIN_DEFINITION } from "./domains/surveyDomain.js";
import { PLAN_PRODUCTION_DOMAIN_DEFINITION } from "./domains/planProductionDomain.js";
import { PLOT_DOMAIN_DEFINITION } from "./domains/plotDomain.js";
import { PROJECT_DOMAIN_DEFINITION } from "./domains/projectDomain.js";
import { STANDARDS_DOMAIN_DEFINITION } from "./domains/standardsDomain.js";
import { QC_DOMAIN_DEFINITION } from "./domains/qcDomain.js";
import { HYDROLOGY_DOMAIN_DEFINITION } from "./domains/hydrologyDomain.js";
import { QUANTITY_TAKEOFF_DOMAIN_DEFINITION } from "./domains/quantityTakeoffDomain.js";
import { SUPERELEVATION_DOMAIN_DEFINITION } from "./domains/superelevationDomain.js";
import { INTERSECTION_DOMAIN_DEFINITION } from "./domains/intersectionDomain.js";
import { SIGHT_DISTANCE_DOMAIN_DEFINITION } from "./domains/sightDistanceDomain.js";
import { DETENTION_DOMAIN_DEFINITION } from "./domains/detentionDomain.js";
import { SLOPE_ANALYSIS_DOMAIN_DEFINITION } from "./domains/slopeAnalysisDomain.js";
import { COST_ESTIMATION_DOMAIN_DEFINITION } from "./domains/costEstimationDomain.js";
import { GEOMETRY_DOMAIN_DEFINITION } from "./domains/geometryDomain.js";
import { DRAWING_RUNTIME_DOMAIN_DEFINITION } from "./domains/drawingRuntimeDomain.js";
import { COORDINATE_SYSTEM_DOMAIN_DEFINITION } from "./domains/coordinateSystemDomain.js";
import { JOB_DOMAIN_DEFINITION } from "./domains/jobDomain.js";
import { PLUGIN_DOMAIN_DEFINITION } from "./domains/pluginDomain.js";
import { DOCS_DOMAIN_DEFINITION } from "./domains/docsDomain.js";
import { WORKFLOW_DOMAIN_DEFINITION } from "./domains/workflowDomain.js";
import type { ToolCatalogEntry } from "./toolMetadata.js";

export const MIGRATED_DOMAIN_DEFINITIONS = [
  ALIGNMENT_DOMAIN_DEFINITION,
  SURFACE_DOMAIN_DEFINITION,
  PROFILE_DOMAIN_DEFINITION,
  CORRIDOR_DOMAIN_DEFINITION,
  SECTION_DOMAIN_DEFINITION,
  PIPE_DOMAIN_DEFINITION,
  ASSEMBLY_DOMAIN_DEFINITION,
  POINT_DOMAIN_DEFINITION,
  GRADING_DOMAIN_DEFINITION,
  PARCEL_DOMAIN_DEFINITION,
  SURVEY_DOMAIN_DEFINITION,
  PLAN_PRODUCTION_DOMAIN_DEFINITION,
  PLOT_DOMAIN_DEFINITION,
  PROJECT_DOMAIN_DEFINITION,
  STANDARDS_DOMAIN_DEFINITION,
  QC_DOMAIN_DEFINITION,
  HYDROLOGY_DOMAIN_DEFINITION,
  QUANTITY_TAKEOFF_DOMAIN_DEFINITION,
  SUPERELEVATION_DOMAIN_DEFINITION,
  INTERSECTION_DOMAIN_DEFINITION,
  SIGHT_DISTANCE_DOMAIN_DEFINITION,
  DETENTION_DOMAIN_DEFINITION,
  SLOPE_ANALYSIS_DOMAIN_DEFINITION,
  COST_ESTIMATION_DOMAIN_DEFINITION,
  GEOMETRY_DOMAIN_DEFINITION,
  DRAWING_RUNTIME_DOMAIN_DEFINITION,
  COORDINATE_SYSTEM_DOMAIN_DEFINITION,
  JOB_DOMAIN_DEFINITION,
  WORKFLOW_DOMAIN_DEFINITION,
  PLUGIN_DOMAIN_DEFINITION,
  DOCS_DOMAIN_DEFINITION,
];

export const GENERATED_TOOL_CATALOG_ENTRIES: ToolCatalogEntry[] = MIGRATED_DOMAIN_DEFINITIONS.flatMap(
  (definition) => buildDomainToolCatalogEntries(definition),
);

export interface ManifestActionMatch {
  definition: DomainToolDefinition;
  exposure: DomainToolExposure;
  actionDefinition: DomainActionDefinition;
}

export function findManifestAction(toolName: string, action: string): ManifestActionMatch | undefined {
  for (const definition of MIGRATED_DOMAIN_DEFINITIONS as DomainToolDefinition[]) {
    const exposure = definition.exposures.find(
      (candidate) => candidate.toolName === toolName && candidate.supportedActions.includes(action),
    );
    const actionDefinition = definition.actions[action];
    if (exposure && actionDefinition) {
      return { definition, exposure, actionDefinition };
    }
  }
  return undefined;
}

export function registerManifestTools(server: McpServer) {
  const includeAliases = process.env.CIVIL3D_ENABLE_TOOL_ALIASES?.trim().toLowerCase() === "true";
  for (const definition of MIGRATED_DOMAIN_DEFINITIONS) {
    captureDomainToolHandlers(definition);
    registerSelectedDomainTools(server, definition, selectManifestExposures(definition, includeAliases));
  }
}

export function selectManifestExposures(
  definition: DomainToolDefinition,
  includeAliases = false,
): DomainToolExposure[] {
  if (includeAliases) {
    return definition.exposures;
  }

  const canonical = definition.exposures[0];
  if (!canonical) {
    return [];
  }

  const selected = [canonical];
  if (definition.domain === "docs") {
    const orchestrator = definition.exposures.find(
      (exposure) => exposure.toolName === "civil3d_orchestrate",
    );
    if (orchestrator && orchestrator !== canonical) {
      selected.push(orchestrator);
    }
  }

  return selected;
}
