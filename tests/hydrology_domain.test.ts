import { beforeEach, describe, expect, it, vi } from "vitest";

const { sendCommandMock } = vi.hoisted(() => ({
  sendCommandMock: vi.fn(),
}));

vi.mock("../src/utils/ConnectionManager.js", () => ({
  withApplicationConnection: async <T>(
    operation: (client: { sendCommand: typeof sendCommandMock }) => Promise<T>,
  ) => await operation({
    sendCommand: sendCommandMock,
  }),
}));

import { HYDROLOGY_DOMAIN_DEFINITION } from "../src/tools/domains/hydrologyDomain.js";

describe("hydrology domain workflows", () => {
  beforeEach(() => {
    sendCommandMock.mockReset();
  });

  it("routes watershed runoff workflow through the native plugin handler", async () => {
    sendCommandMock.mockResolvedValue({
      workflow: "watershed_to_runoff",
      surfaceName: "EG",
      runoffEstimate: { runoffRate: { cfs: 5.6 } },
    });

    const result = await HYDROLOGY_DOMAIN_DEFINITION.actions.watershed_runoff_workflow.execute({
      action: "watershed_runoff_workflow",
      surfaceName: "EG",
      runoffCoefficient: 0.7,
      rainfallIntensity: 2.5,
      intensityUnits: "in_per_hr",
      findLowPointSampleSpacing: 20,
    });

    expect(sendCommandMock).toHaveBeenCalledTimes(1);
    expect(sendCommandMock).toHaveBeenCalledWith("watershedRunoffWorkflow", {
      surfaceName: "EG",
      outletX: undefined,
      outletY: undefined,
      findLowPointSampleSpacing: 20,
      gridSpacing: undefined,
      searchRadius: undefined,
      catchmentSampleSpacing: undefined,
      maxDistance: undefined,
      runoffCoefficient: 0.7,
      rainfallIntensity: 2.5,
      intensityUnits: "in_per_hr",
      runoffAreaUnits: undefined,
    });
    expect(result).toMatchObject({
      workflow: "watershed_to_runoff",
      surfaceName: "EG",
    });
  });

  it("routes runoff detention workflow through the native plugin handler", async () => {
    sendCommandMock.mockResolvedValue({
      workflow: "runoff_to_detention",
      runoffEstimate: { runoffRate: { cfs: 9.2 } },
      detentionSizing: { requiredStorageCubicFeet: 12000 },
    });

    const result = await HYDROLOGY_DOMAIN_DEFINITION.actions.runoff_detention_workflow.execute({
      action: "runoff_detention_workflow",
      drainageArea: 12,
      areaUnits: "acres",
      runoffCoefficient: 0.85,
      rainfallIntensity: 3.1,
      intensityUnits: "in_per_hr",
      allowableOutflow: 4,
      detentionMethod: "modified_rational",
    });

    expect(sendCommandMock).toHaveBeenCalledTimes(1);
    expect(sendCommandMock).toHaveBeenCalledWith("runoffDetentionWorkflow", {
      drainageArea: 12,
      areaUnits: "acres",
      runoffCoefficient: 0.85,
      rainfallIntensity: 3.1,
      intensityUnits: "in_per_hr",
      allowableOutflow: 4,
      stormDuration: undefined,
      detentionMethod: "modified_rational",
      sideSlope: undefined,
      bottomWidth: undefined,
      freeboardDepth: undefined,
      basinSurfaceName: undefined,
      bottomElevation: undefined,
      topElevation: undefined,
      elevationIncrement: undefined,
      outletType: undefined,
      outletDiameter: undefined,
      weirLength: undefined,
      dischargeCoefficient: undefined,
    });
    expect(result).toMatchObject({
      workflow: "runoff_to_detention",
    });
  });

  it("routes runoff pipe workflow through the native plugin handler", async () => {
    sendCommandMock.mockResolvedValue({
      workflow: "runoff_to_pipe_network",
      networkName: "STM-1",
      designFlowCfs: 7.5,
    });

    const result = await HYDROLOGY_DOMAIN_DEFINITION.actions.runoff_pipe_workflow.execute({
      action: "runoff_pipe_workflow",
      networkName: "STM-1",
      drainageArea: 8,
      areaUnits: "acres",
      runoffCoefficient: 0.9,
      rainfallIntensity: 2.2,
      intensityUnits: "in_per_hr",
      designFlowMultiplier: 1.15,
      manningsN: 0.013,
    });

    expect(sendCommandMock).toHaveBeenCalledTimes(1);
    expect(sendCommandMock).toHaveBeenCalledWith("runoffPipeWorkflow", {
      networkName: "STM-1",
      drainageArea: 8,
      areaUnits: "acres",
      runoffCoefficient: 0.9,
      rainfallIntensity: 2.2,
      intensityUnits: "in_per_hr",
      designFlowMultiplier: 1.15,
      tailwaterElevation: undefined,
      manningsN: 0.013,
      minCoverDepth: undefined,
      minVelocity: undefined,
      maxVelocity: undefined,
      minSlope: undefined,
    });
    expect(result).toMatchObject({
      workflow: "runoff_to_pipe_network",
      networkName: "STM-1",
    });
  });
});
