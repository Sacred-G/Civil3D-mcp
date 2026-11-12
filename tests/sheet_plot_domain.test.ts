import { readFileSync } from "node:fs";
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

import { PLOT_DOMAIN_DEFINITION, SheetPlotArgsSchema } from "../src/tools/domains/plotDomain.js";
import { isApprovalRequired } from "../src/tools/approvalPolicy.js";

const sheetSource = readFileSync(new URL("../Civil3D-MCP-Plugin/SheetCommands.cs", import.meta.url), "utf8");
const dispatcherSource = readFileSync(new URL("../Civil3D-MCP-Plugin/CommandDispatcher.cs", import.meta.url), "utf8");

describe("civil3d_plot sheet_plot", () => {
  beforeEach(() => {
    sendCommandMock.mockReset();
    sendCommandMock.mockResolvedValue({ layoutName: "SHEET-1", outputPath: "C:/out/sheet.pdf", bytes: 1234, created: true });
  });

  it("routes to createSheetAndPlot with overwrite defaulting to false", async () => {
    await PLOT_DOMAIN_DEFINITION.actions.sheet_plot.execute({
      action: "sheet_plot",
      outputPath: "C:/out/sheet.pdf",
      layoutName: "SHEET-1",
      modelCenterX: 500,
      modelCenterY: 250,
      modelHeight: 400,
    });

    expect(sendCommandMock).toHaveBeenCalledWith("createSheetAndPlot", expect.objectContaining({
      layoutName: "SHEET-1",
      outputPath: "C:/out/sheet.pdf",
      modelCenterX: 500,
      modelCenterY: 250,
      modelHeight: 400,
      overwrite: false,
    }));
  });

  it("requires a .pdf output path and positive model sizes", () => {
    expect(SheetPlotArgsSchema.safeParse({ action: "sheet_plot", outputPath: "C:/out/sheet.pdf" }).success).toBe(true);
    expect(SheetPlotArgsSchema.safeParse({ action: "sheet_plot", outputPath: "C:/out/sheet.png" }).success).toBe(false);
    expect(SheetPlotArgsSchema.safeParse({ action: "sheet_plot" }).success).toBe(false);
    expect(SheetPlotArgsSchema.safeParse({ action: "sheet_plot", outputPath: "C:/out/a.pdf", modelHeight: 0 }).success).toBe(false);
  });

  it("is approval-gated, not retry-safe, and listed on civil3d_plot", () => {
    const action = PLOT_DOMAIN_DEFINITION.actions.sheet_plot;
    expect(action.safeForRetry).toBe(false);
    expect(isApprovalRequired({
      toolName: "civil3d_plot",
      action: "sheet_plot",
      capabilities: action.capabilities,
      safeForRetry: action.safeForRetry,
    })).toBe(true);
    const tool = PLOT_DOMAIN_DEFINITION.exposures.find((entry) => entry.toolName === "civil3d_plot");
    expect(tool?.supportedActions).toContain("sheet_plot");
  });

  it("is registered in the plugin and restores what it changes", () => {
    expect(dispatcherSource).toContain('"createSheetAndPlot" => SheetCommands.CreateSheetAndPlotAsync(parameters)');
    expect(sheetSource).toContain("CivilExecution.ExecuteCommandSequenceAsync");
    expect(sheetSource).toContain("using var sysvars = new PlotCommands.SystemVariableScope();");
    expect(sheetSource).toContain('sysvars.Set("BACKGROUNDPLOT", 0);');
    expect(sheetSource).toContain('sysvars.Remember("CTAB");');
    expect(sheetSource).toContain("doc.Database.Textstyle = originalTextStyle");
    expect(sheetSource).toContain('FileBoundary.ResolveExportPath(outputPath, overwrite, ".pdf")');
    // Commands go through the shared runner, never SendStringToExecute.
    expect(sheetSource).not.toContain("SendStringToExecute");
  });
});
