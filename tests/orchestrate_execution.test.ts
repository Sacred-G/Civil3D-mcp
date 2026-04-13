import { beforeEach, describe, expect, it, vi } from "vitest";

const { executeRegisteredToolMock, getProjectContextMock } = vi.hoisted(() => ({
  executeRegisteredToolMock: vi.fn(),
  getProjectContextMock: vi.fn(async () => ({
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
  })),
}));

vi.mock("../src/tools/toolHandlerRegistry.js", () => ({
  executeRegisteredTool: executeRegisteredToolMock,
}));

vi.mock("../src/orchestration/ProjectContextService.js", () => ({
  getProjectContext: getProjectContextMock,
}));

import {
  executeCivil3DOrchestrate,
  executeToolCallViaOrchestrator,
} from "../src/tools/civil3d_orchestrate.js";

describe("civil3d_orchestrate execution", () => {
  beforeEach(() => {
    executeRegisteredToolMock.mockReset();
    executeRegisteredToolMock.mockImplementation(async (toolName, parameters) => ({
      toolName,
      parameters,
      ok: true,
    }));
    getProjectContextMock.mockClear();
  });

  it("executes routed domain tools through the registered tool surface", async () => {
    const response = await executeCivil3DOrchestrate({
      request: "show surface EG",
      execute: true,
    });

    expect(executeRegisteredToolMock).toHaveBeenCalledWith("civil3d_surface", {
      action: "get",
      name: "EG",
    });
    expect(response.status).toBe("executed");
    expect(response.selectedTool).toBe("civil3d_surface");
    expect(response.result).toEqual({
      toolName: "civil3d_surface",
      parameters: {
        action: "get",
        name: "EG",
      },
      ok: true,
    });
  });

  it("executes routed alias tools with orchestrator defaults applied", async () => {
    const response = await executeCivil3DOrchestrate({
      request: "generate surface volume report between surfaces EG and FG",
      execute: true,
    });

    expect(executeRegisteredToolMock).toHaveBeenCalledWith("civil3d_surface_volume_report", {
      baseSurface: "EG",
      comparisonSurface: "FG",
      format: "summary",
    });
    expect(response.selectedTool).toBe("civil3d_surface_volume_report");
    expect(response.result).toEqual({
      toolName: "civil3d_surface_volume_report",
      parameters: {
        baseSurface: "EG",
        comparisonSurface: "FG",
        format: "summary",
      },
      ok: true,
    });
  });

  it("executes routed workflow tools through the registered tool surface", async () => {
    const response = await executeCivil3DOrchestrate({
      request: 'project startup template="C:/templates/civil3d.dwt" save as="C:/projects/startup.dwg"',
      execute: true,
    });

    expect(executeRegisteredToolMock).toHaveBeenCalledWith("civil3d_workflow_project_startup", {
      templatePath: "C:/templates/civil3d.dwt",
      saveAs: "C:/projects/startup.dwg",
    });
    expect(response.status).toBe("executed");
    expect(response.selectedTool).toBe("civil3d_workflow_project_startup");
    expect(response.result).toEqual({
      toolName: "civil3d_workflow_project_startup",
      parameters: {
        templatePath: "C:/templates/civil3d.dwt",
        saveAs: "C:/projects/startup.dwg",
      },
      ok: true,
    });
  });

  it("exposes a shared dispatch helper for non-natural-language tool execution", async () => {
    const result = await executeToolCallViaOrchestrator("civil3d_alignment", {
      action: "list",
    });

    expect(executeRegisteredToolMock).toHaveBeenCalledWith("civil3d_alignment", {
      action: "list",
    });
    expect(result).toEqual({
      toolName: "civil3d_alignment",
      parameters: {
        action: "list",
      },
      ok: true,
    });
    expect(getProjectContextMock).not.toHaveBeenCalled();
  });

  it("uses toolParameters to satisfy required fields in exact-tool orchestration", async () => {
    const response = await executeCivil3DOrchestrate({
      toolName: "civil3d_surface",
      toolParameters: {
        action: "get",
        name: "EG",
      },
      execute: true,
    });

    expect(executeRegisteredToolMock).toHaveBeenCalledWith("civil3d_surface", {
      action: "get",
      name: "EG",
    });
    expect(response.status).toBe("executed");
    expect(response.selectedTool).toBe("civil3d_surface");
    expect(response.result).toEqual({
      toolName: "civil3d_surface",
      parameters: {
        action: "get",
        name: "EG",
      },
      ok: true,
    });
  });

  it("applies orchestrator validation to direct helper execution", async () => {
    await expect(
      executeToolCallViaOrchestrator("civil3d_surface", {
        action: "get",
      }),
    ).rejects.toThrow("Missing required fields: name");

    expect(executeRegisteredToolMock).not.toHaveBeenCalled();
  });

  it("supports exact-tool orchestration for tools outside the curated intent list", async () => {
    const response = await executeCivil3DOrchestrate({
      toolName: "civil3d_assembly",
      toolAction: "list",
      execute: true,
    });

    expect(executeRegisteredToolMock).toHaveBeenCalledWith("civil3d_assembly", {
      action: "list",
    });
    expect(response.selectedTool).toBe("civil3d_assembly");
    expect(response.status).toBe("executed");
    expect(response.result).toEqual({
      toolName: "civil3d_assembly",
      parameters: {
        action: "list",
      },
      ok: true,
    });
  });

  it("applies required-field validation to derived pressure-network routes", async () => {
    await expect(
      executeToolCallViaOrchestrator("civil3d_pressure_network_get_info", {}),
    ).rejects.toThrow("Missing required fields: name");

    expect(executeRegisteredToolMock).not.toHaveBeenCalled();
    expect(getProjectContextMock).not.toHaveBeenCalled();
  });

  it("uses request extraction for exact-tool orchestration when toolName is provided", async () => {
    const response = await executeCivil3DOrchestrate({
      request: "get pressure network name=WM-1",
      toolName: "civil3d_pressure_network_get_info",
      execute: true,
    });

    expect(executeRegisteredToolMock).toHaveBeenCalledWith(
      "civil3d_pressure_network_get_info",
      expect.objectContaining({
        name: "WM-1",
        networkName: "WM-1",
      }),
    );
    expect(response.status).toBe("executed");
    expect(response.selectedTool).toBe("civil3d_pressure_network_get_info");
  });

  it("uses extracted point inputs for exact pressure-pipe tool execution", async () => {
    const response = await executeCivil3DOrchestrate({
      request: "add pressure pipe network name=WM-1 part name=8in PVC start point=0,0,100 end point=100,0,98",
      toolName: "civil3d_pressure_pipe_add",
      execute: true,
    });

    expect(executeRegisteredToolMock).toHaveBeenCalledWith(
      "civil3d_pressure_pipe_add",
      expect.objectContaining({
        networkName: "WM-1",
        partName: "8in PVC",
        startPoint: { x: 0, y: 0, z: 100 },
        endPoint: { x: 100, y: 0, z: 98 },
      }),
    );
    expect(response.status).toBe("executed");
    expect(response.selectedTool).toBe("civil3d_pressure_pipe_add");
  });
});
