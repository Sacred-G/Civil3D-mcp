import { describe, expect, it } from "vitest";
import { routeIntent } from "../src/orchestration/IntentRouter.js";
import { buildWorkflowPlan } from "../src/orchestration/WorkflowPlanner.js";
import { resolveParamsFromSelection } from "../src/orchestration/SelectionResolver.js";
import type { ProjectContext } from "../src/orchestration/ProjectContextService.js";

function createProjectContext(overrides?: Partial<ProjectContext>): ProjectContext {
  return {
    drawingInfo: {
      objectCounts: {
        surfaces: 1,
        alignments: 1,
        profiles: 1,
        corridors: 1,
      },
    },
    objectTypes: ["Alignment", "Surface", "Corridor"],
    selectedObjects: [],
    ...overrides,
  };
}

describe("orchestration routing", () => {
  it("routes tool discovery requests to list_tool_capabilities", () => {
    const routed = routeIntent("what tools can you use");

    expect(routed.match.intent).toBe("list_tool_capabilities");
  });

  it("routes named surface inspection requests to get_surface", () => {
    const routed = routeIntent("show surface EG");

    expect(routed.match.intent).toBe("get_surface");
    expect(routed.extractedParams.name).toBe("EG");
  });

  it("routes surface volume requests and extracts both surface names", () => {
    const routed = routeIntent("calculate surface volume between surfaces EG and FG");

    expect(routed.match.intent).toBe("calculate_surface_volume");
    expect(routed.extractedParams.baseSurface).toBe("EG");
    expect(routed.extractedParams.comparisonSurface).toBe("FG");
  });

  it("routes surface volume report requests", () => {
    const routed = routeIntent("generate surface volume report between surfaces EG and FG");

    expect(routed.match.intent).toBe("generate_surface_volume_report");
    expect(routed.extractedParams.baseSurface).toBe("EG");
    expect(routed.extractedParams.comparisonSurface).toBe("FG");
  });

  it("routes workflow-style surface comparison requests", () => {
    const routed = routeIntent("run surface comparison workflow between surfaces EG and FG");

    expect(routed.match.intent).toBe("workflow_surface_comparison_report");
    expect(routed.extractedParams.baseSurface).toBe("EG");
    expect(routed.extractedParams.comparisonSurface).toBe("FG");
  });

  it("routes grid-based surface sampling requests", () => {
    const routed = routeIntent("sample surface EG with grid spacing 25");

    expect(routed.match.intent).toBe("sample_surface_elevations");
    expect(routed.extractedParams.name).toBe("EG");
    expect(routed.extractedParams.gridSpacing).toBe(25);
  });

  it("routes create surface from DEM requests and extracts the file path", () => {
    const routed = routeIntent('create surface from dem called Existing from file "C:/temp/existing.tif"');

    expect(routed.match.intent).toBe("create_surface_from_dem");
    expect(routed.extractedParams.name).toBe("Existing");
    expect(routed.extractedParams.filePath).toBe("C:/temp/existing.tif");
  });

  it("routes named alignment inspection requests to get_alignment", () => {
    const routed = routeIntent("inspect alignment Mainline");

    expect(routed.match.intent).toBe("get_alignment");
    expect(routed.extractedParams.name).toBe("Mainline");
  });

  it("routes profile listing requests and extracts alignment names", () => {
    const routed = routeIntent("list profiles for alignment CL-1");

    expect(routed.match.intent).toBe("list_profiles");
    expect(routed.extractedParams.alignmentName).toBe("CL-1");
  });

  it("routes layout profile creation requests and extracts profile and alignment names", () => {
    const routed = routeIntent("create layout profile called FG_Main on alignment Mainline");

    expect(routed.match.intent).toBe("create_layout_profile");
    expect(routed.extractedParams.profileName).toBe("FG_Main");
    expect(routed.extractedParams.alignmentName).toBe("Mainline");
  });

  it("routes corridor maintenance requests to rebuild_corridor", () => {
    const routed = routeIntent("rebuild corridor Phase1");

    expect(routed.match.intent).toBe("rebuild_corridor");
    expect(routed.extractedParams.name).toBe("Phase1");
  });

  it("routes stopping sight distance requests and extracts named inputs", () => {
    const routed = routeIntent("check stopping distance on alignment Mainline profile FG_Main design speed 80");

    expect(routed.match.intent).toBe("check_stopping_distance");
    expect(routed.extractedParams.alignmentName).toBe("Mainline");
    expect(routed.extractedParams.profileName).toBe("FG_Main");
    expect(routed.extractedParams.designSpeed).toBe(80);
  });

  it("routes detention sizing requests and extracts flow values", () => {
    const routed = routeIntent("size detention basin inflow 25 outflow 8");

    expect(routed.match.intent).toBe("calculate_detention_basin_size");
    expect(routed.extractedParams.inflow).toBe(25);
    expect(routed.extractedParams.outflow).toBe(8);
  });

  it("routes slope geometry requests to the slope analysis intent", () => {
    const routed = routeIntent("calculate slope geometry for alignment Mainline");

    expect(routed.match.intent).toBe("calculate_slope_geometry");
    expect(routed.extractedParams.alignmentName).toBe("Mainline");
  });

  it("routes pay item export requests and extracts the output path", () => {
    const routed = routeIntent('export pay items output "C:/temp/pay-items.csv"');

    expect(routed.match.intent).toBe("export_pay_items");
    expect(routed.extractedParams.outputPath).toBe("C:/temp/pay-items.csv");
  });

  it("routes project startup workflow requests and extracts template/save paths", () => {
    const routed = routeIntent('project startup template="C:/templates/civil3d.dwt" save as="C:/projects/startup.dwg"');

    expect(routed.match.intent).toBe("workflow_project_startup");
    expect(routed.extractedParams.templatePath).toBe("C:/templates/civil3d.dwt");
    expect(routed.extractedParams.saveAs).toBe("C:/projects/startup.dwg");
  });

  it("routes data shortcut reference workflow requests and extracts project fields", () => {
    const routed = routeIntent('reference data shortcut project folder="C:/Projects/Roadway" shortcut name=FG shortcut type=surface');

    expect(routed.match.intent).toBe("workflow_data_shortcut_reference_sync");
    expect(routed.extractedParams.projectFolder).toBe("C:/Projects/Roadway");
    expect(routed.extractedParams.shortcutName).toBe("FG");
    expect(routed.extractedParams.shortcutType).toBe("surface");
  });

  it("routes material cost estimate requests to the cost-estimation intent", () => {
    const routed = routeIntent("estimate material cost");

    expect(routed.match.intent).toBe("estimate_material_cost");
  });

  it("derives assembly discovery routes from the live tool catalog", () => {
    const routed = routeIntent("list assemblies");

    expect(routed.match.toolName).toBe("civil3d_assembly");
    expect(routed.match.action).toBe("list");
    expect(routed.match.source).toBe("derived");
  });

  it("matches exact tool-name requests for uncataloged tools", () => {
    const routed = routeIntent("run civil3d_pipe_network");

    expect(routed.match.toolName).toBe("civil3d_pipe_network");
    expect(routed.match.source).toBe("derived");
  });

  it("assigns required fields to derived pressure-network routes", () => {
    const routed = routeIntent("run civil3d_pressure_network_get_info");

    expect(routed.match.toolName).toBe("civil3d_pressure_network_get_info");
    expect(routed.match.requiredFields).toEqual(["name"]);
  });

  it("assigns required fields to derived DEM import routes", () => {
    const routed = routeIntent("run civil3d_surface_create_from_dem");

    expect(routed.match.toolName).toBe("civil3d_surface_create_from_dem");
    expect(routed.match.requiredFields).toEqual(["name", "filePath"]);
  });

  it("extracts pressure-network fields from natural language", () => {
    const routed = routeIntent("get pressure network name=WM-1 parts list=Standard Pressure");

    expect(routed.extractedParams.networkName).toBe("WM-1");
    expect(routed.extractedParams.partsList).toBe("Standard Pressure");
  });

  it("extracts 3D point inputs from natural language", () => {
    const routed = routeIntent("add pressure pipe network name=WM-1 part name=8in PVC start point=0,0,100 end point=100,0,98");

    expect(routed.extractedParams.startPoint).toEqual({ x: 0, y: 0, z: 100 });
    expect(routed.extractedParams.endPoint).toEqual({ x: 100, y: 0, z: 98 });
  });
});

describe("orchestration workflow planning", () => {
  it("asks a targeted clarification question when listing profiles without an alignment", () => {
    const routed = routeIntent("list profiles");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("List profiles workflow");
    expect(plan.missingFields).toEqual(["alignmentName"]);
    expect(plan.clarificationQuestions).toContain("Which alignment should be used to list profiles?");
  });

  it("asks targeted clarification questions for layout profile creation", () => {
    const routed = routeIntent("create layout profile");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Create layout profile workflow");
    expect(plan.clarificationQuestions).toContain("Which alignment should the layout profile be created on?");
    expect(plan.clarificationQuestions).toContain("What should the new layout profile be named?");
  });

  it("asks for both surfaces when planning a surface volume comparison", () => {
    const routed = routeIntent("calculate surface volume");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Surface volume workflow");
    expect(plan.missingFields).toEqual(["baseSurface", "comparisonSurface"]);
    expect(plan.clarificationQuestions).toContain("Which base surface should be used for the volume comparison?");
    expect(plan.clarificationQuestions).toContain("Which comparison surface should be used for the volume comparison?");
  });

  it("asks for design speed when planning sight distance calculations", () => {
    const routed = routeIntent("calculate sight distance");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Sight distance calculation workflow");
    expect(plan.missingFields).toEqual(["designSpeed"]);
    expect(plan.clarificationQuestions).toContain("What design speed should be used for the sight distance calculation?");
  });

  it("asks for elevations when planning detention stage storage generation", () => {
    const routed = routeIntent("generate detention stage storage for surface Basin");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Detention stage-storage workflow");
    expect(plan.missingFields).toEqual(["bottomElevation", "topElevation"]);
    expect(plan.clarificationQuestions).toContain("What basin bottom elevation should be used?");
    expect(plan.clarificationQuestions).toContain("What top water surface elevation should be used?");
  });

  it("asks for grid spacing when planning surface sampling", () => {
    const routed = routeIntent("sample surface EG");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Surface sampling workflow");
    expect(plan.missingFields).toEqual(["gridSpacing"]);
    expect(plan.clarificationQuestions).toContain("What grid spacing should be used for the surface sampling?");
  });

  it("asks for a file path when planning DEM-based surface creation", () => {
    const routed = routeIntent("create surface from dem called Existing");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Create surface from DEM workflow");
    expect(plan.missingFields).toEqual(["filePath"]);
    expect(plan.clarificationQuestions).toContain("What DEM or raster file path should be imported?");
  });

  it("asks for an output path when planning pay item export", () => {
    const routed = routeIntent("export pay items");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Pay item export workflow");
    expect(plan.missingFields).toEqual(["outputPath"]);
    expect(plan.clarificationQuestions).toContain("What output file path should be used for the pay item export?");
  });

  it("asks for pay item pricing when planning material cost estimation", () => {
    const routed = routeIntent("estimate material cost");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Material cost estimate workflow");
    expect(plan.missingFields).toEqual(["payItems"]);
    expect(plan.clarificationQuestions).toContain("Please provide the pay item definitions with unit prices for the cost estimate.");
  });

  it("builds a project startup workflow plan from a natural-language startup request", () => {
    const routed = routeIntent("project startup");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Project startup workflow");
    expect(plan.steps[0].toolName).toBe("civil3d_health");
  });

  it("asks for missing project-folder shortcut inputs in the data shortcut reference workflow", () => {
    const routed = routeIntent("reference data shortcut");
    const plan = buildWorkflowPlan(routed, routed.extractedParams, createProjectContext());

    expect(plan.title).toBe("Data shortcut reference workflow");
    expect(plan.missingFields).toEqual(["projectFolder", "shortcutName", "shortcutType"]);
    expect(plan.clarificationQuestions).toContain("Which project folder should be used for the data shortcut reference?");
  });
});

describe("selection-based parameter inference", () => {
  it("infers a generic name from a selected corridor when no name is supplied", () => {
    const resolution = resolveParamsFromSelection(
      {},
      createProjectContext({
        selectedObjects: [{ objectType: "Corridor", name: "Phase1" }],
      })
    );

    expect(resolution.resolvedParams.name).toBe("Phase1");
    expect(resolution.inferredFromSelection).toContain("name");
  });
});
