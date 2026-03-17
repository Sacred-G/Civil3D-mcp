import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

// ---------------------------------------------------------------------------
// Shared schemas
// ---------------------------------------------------------------------------

const SheetSummarySchema = z.object({
  name: z.string(),
  number: z.string(),
  handle: z.string(),
  layoutName: z.string().nullable(),
});

const SheetSetSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  description: z.string().nullable(),
  sheetCount: z.number(),
});

const SheetDetailSchema = z.object({
  name: z.string(),
  number: z.string(),
  handle: z.string(),
  layoutName: z.string().nullable(),
  viewportScale: z.number().nullable(),
  alignmentName: z.string().nullable(),
  profileName: z.string().nullable(),
  titleBlock: z.string().nullable(),
});

const GenericResponseSchema = z.object({}).passthrough();

function errorResult(toolName: string, error: unknown) {
  const message = error instanceof Error ? error.message : String(error);
  return {
    content: [{ type: "text" as const, text: `Error in ${toolName}: ${message}` }],
    isError: true,
  };
}

// ---------------------------------------------------------------------------
// 1. civil3d_sheet_set_list
// ---------------------------------------------------------------------------

export function registerCivil3DSheetSetListTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_set_list",
    "Lists all Plan Production sheet sets in the active Civil 3D drawing.",
    {},
    async (_args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listSheetSets", {});
        });
        const validated = z.object({ sheetSets: z.array(SheetSetSummarySchema) }).parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_set_list", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 2. civil3d_sheet_set_get_info
// ---------------------------------------------------------------------------

export function registerCivil3DSheetSetGetInfoTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_set_get_info",
    "Gets detailed information about a Plan Production sheet set including all sheets.",
    {
      name: z.string().describe("Sheet set name"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getSheetSetInfo", { name: args.name });
        });
        const validated = z
          .object({
            name: z.string(),
            handle: z.string(),
            description: z.string().nullable(),
            sheets: z.array(SheetSummarySchema),
          })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_set_get_info", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 3. civil3d_sheet_set_create
// ---------------------------------------------------------------------------

export function registerCivil3DSheetSetCreateTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_set_create",
    "Creates a new Plan Production sheet set in the active Civil 3D drawing.",
    {
      name: z.string().describe("Sheet set name"),
      description: z.string().optional().describe("Optional description"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createSheetSet", {
            name: args.name,
            description: args.description,
          });
        });
        const validated = z
          .object({ name: z.string(), handle: z.string(), created: z.boolean() })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_set_create", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 4. civil3d_sheet_add
// ---------------------------------------------------------------------------

export function registerCivil3DSheetAddTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_add",
    "Adds a new sheet to an existing Plan Production sheet set.",
    {
      sheetSetName: z.string().describe("Name of the sheet set to add the sheet to"),
      sheetName: z.string().describe("Name for the new sheet"),
      sheetNumber: z.string().optional().describe("Sheet number (e.g. '1', 'C-01')"),
      layoutName: z.string().optional().describe("Existing layout name to associate with this sheet"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("addSheet", {
            sheetSetName: args.sheetSetName,
            sheetName: args.sheetName,
            sheetNumber: args.sheetNumber,
            layoutName: args.layoutName,
          });
        });
        const validated = z
          .object({ name: z.string(), number: z.string(), handle: z.string(), added: z.boolean() })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_add", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 5. civil3d_sheet_get_properties
// ---------------------------------------------------------------------------

export function registerCivil3DSheetGetPropertiesTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_get_properties",
    "Gets full properties of a specific sheet within a Plan Production sheet set.",
    {
      sheetSetName: z.string().describe("Name of the sheet set"),
      sheetName: z.string().describe("Name of the sheet"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getSheetProperties", {
            sheetSetName: args.sheetSetName,
            sheetName: args.sheetName,
          });
        });
        const validated = SheetDetailSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_get_properties", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 6. civil3d_sheet_set_title_block
// ---------------------------------------------------------------------------

export function registerCivil3DSheetSetTitleBlockTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_set_title_block",
    "Sets or updates the title block template on a sheet within a Plan Production sheet set.",
    {
      sheetSetName: z.string().describe("Name of the sheet set"),
      sheetName: z.string().describe("Name of the sheet"),
      titleBlockPath: z.string().describe("Full path to the title block DWG template file"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("setSheetTitleBlock", {
            sheetSetName: args.sheetSetName,
            sheetName: args.sheetName,
            titleBlockPath: args.titleBlockPath,
          });
        });
        const validated = z
          .object({ sheetName: z.string(), titleBlock: z.string(), updated: z.boolean() })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_set_title_block", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 7. civil3d_plan_profile_sheet_create
// ---------------------------------------------------------------------------

export function registerCivil3DPlanProfileSheetCreateTool(server: McpServer) {
  server.tool(
    "civil3d_plan_profile_sheet_create",
    "Creates a Plan/Profile sheet for a given alignment and profile using Civil 3D's AeccPlanProductionHelper.",
    {
      sheetSetName: z.string().describe("Sheet set to add the sheet to"),
      alignmentName: z.string().describe("Alignment to use for the plan view"),
      profileName: z.string().optional().describe("Profile to use for the profile view (omit for plan-only)"),
      sheetTemplatePath: z.string().optional().describe("Path to DWT template file for the sheet"),
      startStation: z.number().optional().describe("Start station along the alignment"),
      endStation: z.number().optional().describe("End station along the alignment"),
      viewScale: z.number().optional().describe("Viewport scale (e.g. 50 for 1:50)"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createPlanProfileSheet", {
            sheetSetName: args.sheetSetName,
            alignmentName: args.alignmentName,
            profileName: args.profileName,
            sheetTemplatePath: args.sheetTemplatePath,
            startStation: args.startStation,
            endStation: args.endStation,
            viewScale: args.viewScale,
          });
        });
        const validated = z
          .object({
            sheetName: z.string(),
            handle: z.string(),
            alignmentName: z.string(),
            profileName: z.string().nullable(),
            created: z.boolean(),
          })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_plan_profile_sheet_create", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 8. civil3d_plan_profile_sheet_update_alignment
// ---------------------------------------------------------------------------

export function registerCivil3DPlanProfileSheetUpdateAlignmentTool(server: McpServer) {
  server.tool(
    "civil3d_plan_profile_sheet_update_alignment",
    "Updates the alignment and optionally the profile on an existing Plan/Profile sheet.",
    {
      sheetSetName: z.string().describe("Sheet set name"),
      sheetName: z.string().describe("Sheet name to update"),
      alignmentName: z.string().describe("New alignment name"),
      profileName: z.string().optional().describe("New profile name (optional)"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("updatePlanProfileSheetAlignment", {
            sheetSetName: args.sheetSetName,
            sheetName: args.sheetName,
            alignmentName: args.alignmentName,
            profileName: args.profileName,
          });
        });
        const validated = z
          .object({ sheetName: z.string(), alignmentName: z.string(), updated: z.boolean() })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_plan_profile_sheet_update_alignment", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 9. civil3d_sheet_view_create
// ---------------------------------------------------------------------------

export function registerCivil3DSheetViewCreateTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_view_create",
    "Creates a viewport/view on a sheet layout in the active Civil 3D drawing.",
    {
      layoutName: z.string().describe("Name of the layout (paper space) to add the view to"),
      viewName: z.string().optional().describe("Named view from model space to display"),
      centerX: z.number().optional().describe("Viewport center X in paper space units"),
      centerY: z.number().optional().describe("Viewport center Y in paper space units"),
      width: z.number().optional().describe("Viewport width in paper space units"),
      height: z.number().optional().describe("Viewport height in paper space units"),
      scale: z.number().optional().describe("Viewport scale (e.g. 50 for 1:50)"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createSheetView", {
            layoutName: args.layoutName,
            viewName: args.viewName,
            centerX: args.centerX,
            centerY: args.centerY,
            width: args.width,
            height: args.height,
            scale: args.scale,
          });
        });
        const validated = z
          .object({ handle: z.string(), layoutName: z.string(), scale: z.number().nullable(), created: z.boolean() })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_view_create", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 10. civil3d_sheet_view_set_scale
// ---------------------------------------------------------------------------

export function registerCivil3DSheetViewSetScaleTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_view_set_scale",
    "Updates the scale of a viewport on a sheet layout.",
    {
      layoutName: z.string().describe("Name of the layout containing the viewport"),
      viewportHandle: z.string().optional().describe("Handle of the specific viewport to update (updates first viewport if omitted)"),
      scale: z.number().positive().describe("New custom scale value (e.g. 50 for 1:50)"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("setSheetViewScale", {
            layoutName: args.layoutName,
            viewportHandle: args.viewportHandle,
            scale: args.scale,
          });
        });
        const validated = z
          .object({ handle: z.string(), scale: z.number(), updated: z.boolean() })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_view_set_scale", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 11. civil3d_sheet_publish_pdf
// ---------------------------------------------------------------------------

export function registerCivil3DSheetPublishPdfTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_publish_pdf",
    "Publishes one or more sheet layouts to a PDF file using AutoCAD's PlotEngine.",
    {
      layoutNames: z.array(z.string()).min(1).describe("List of layout names to publish"),
      outputPath: z.string().describe("Full output path for the PDF file (e.g. C:/output/sheets.pdf)"),
      plotStyleTable: z.string().optional().describe("Plot style table name (e.g. 'monochrome.ctb')"),
      paperSize: z.string().optional().describe("Paper size name (e.g. 'ARCH D (24.00 x 36.00 Inches)')"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("publishSheetPdf", {
            layoutNames: args.layoutNames,
            outputPath: args.outputPath,
            plotStyleTable: args.plotStyleTable,
            paperSize: args.paperSize,
          });
        });
        const validated = z
          .object({ outputPath: z.string(), sheetsPublished: z.number(), published: z.boolean() })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_publish_pdf", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 12. civil3d_sheet_set_export
// ---------------------------------------------------------------------------

export function registerCivil3DSheetSetExportTool(server: McpServer) {
  server.tool(
    "civil3d_sheet_set_export",
    "Exports all sheets in a Plan Production sheet set to a single multi-page PDF.",
    {
      sheetSetName: z.string().describe("Name of the sheet set to export"),
      outputPath: z.string().describe("Full output path for the exported PDF file"),
      plotStyleTable: z.string().optional().describe("Plot style table name (e.g. 'monochrome.ctb')"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("exportSheetSet", {
            sheetSetName: args.sheetSetName,
            outputPath: args.outputPath,
            plotStyleTable: args.plotStyleTable,
          });
        });
        const validated = z
          .object({
            sheetSetName: z.string(),
            outputPath: z.string(),
            sheetsExported: z.number(),
            exported: z.boolean(),
          })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sheet_set_export", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// Aggregate registration
// ---------------------------------------------------------------------------

export function registerCivil3DPlanProductionTools(server: McpServer) {
  registerCivil3DSheetSetListTool(server);
  registerCivil3DSheetSetGetInfoTool(server);
  registerCivil3DSheetSetCreateTool(server);
  registerCivil3DSheetAddTool(server);
  registerCivil3DSheetGetPropertiesTool(server);
  registerCivil3DSheetSetTitleBlockTool(server);
  registerCivil3DPlanProfileSheetCreateTool(server);
  registerCivil3DPlanProfileSheetUpdateAlignmentTool(server);
  registerCivil3DSheetViewCreateTool(server);
  registerCivil3DSheetViewSetScaleTool(server);
  registerCivil3DSheetPublishPdfTool(server);
  registerCivil3DSheetSetExportTool(server);
}
