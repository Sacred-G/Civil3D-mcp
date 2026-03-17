import { describe, it, expect, vi } from "vitest";
import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";

// ---------------------------------------------------------------------------
// Schemas under test (mirrored from civil3d_plan_production.ts)
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

// Tool input schemas
const SheetSetCreateInputSchema = z.object({
  name: z.string(),
  description: z.string().optional(),
});

const SheetSetGetInfoInputSchema = z.object({
  name: z.string(),
});

const SheetAddInputSchema = z.object({
  sheetSetName: z.string(),
  sheetName: z.string(),
  sheetNumber: z.string().optional(),
  layoutName: z.string().optional(),
});

const SheetGetPropertiesInputSchema = z.object({
  sheetSetName: z.string(),
  sheetName: z.string(),
});

const SheetSetTitleBlockInputSchema = z.object({
  sheetSetName: z.string(),
  sheetName: z.string(),
  titleBlockPath: z.string(),
});

const PlanProfileSheetCreateInputSchema = z.object({
  sheetSetName: z.string(),
  alignmentName: z.string(),
  profileName: z.string().optional(),
  sheetTemplatePath: z.string().optional(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  viewScale: z.number().optional(),
});

const PlanProfileSheetUpdateAlignmentInputSchema = z.object({
  sheetSetName: z.string(),
  sheetName: z.string(),
  alignmentName: z.string(),
  profileName: z.string().optional(),
});

const SheetViewCreateInputSchema = z.object({
  layoutName: z.string(),
  viewName: z.string().optional(),
  centerX: z.number().optional(),
  centerY: z.number().optional(),
  width: z.number().optional(),
  height: z.number().optional(),
  scale: z.number().optional(),
});

const SheetViewSetScaleInputSchema = z.object({
  layoutName: z.string(),
  viewportHandle: z.string().optional(),
  scale: z.number().positive(),
});

const PublishPdfInputSchema = z.object({
  layoutNames: z.array(z.string()).min(1),
  outputPath: z.string(),
  plotStyleTable: z.string().optional(),
  paperSize: z.string().optional(),
});

const ExportSheetSetInputSchema = z.object({
  sheetSetName: z.string(),
  outputPath: z.string(),
  plotStyleTable: z.string().optional(),
});

// Response schemas
const SheetSetListResponseSchema = z.object({
  sheetSets: z.array(SheetSetSummarySchema),
});

const SheetSetInfoResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  description: z.string().nullable(),
  sheets: z.array(SheetSummarySchema),
});

const SheetSetCreateResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  created: z.boolean(),
});

const SheetAddResponseSchema = z.object({
  name: z.string(),
  number: z.string(),
  handle: z.string(),
  added: z.boolean(),
});

const SheetSetTitleBlockResponseSchema = z.object({
  sheetName: z.string(),
  titleBlock: z.string(),
  updated: z.boolean(),
});

const PlanProfileSheetCreateResponseSchema = z.object({
  sheetName: z.string(),
  handle: z.string(),
  alignmentName: z.string(),
  profileName: z.string().nullable(),
  created: z.boolean(),
});

const SheetViewCreateResponseSchema = z.object({
  handle: z.string(),
  layoutName: z.string(),
  scale: z.number().nullable(),
  created: z.boolean(),
});

const SheetViewSetScaleResponseSchema = z.object({
  handle: z.string(),
  scale: z.number(),
  updated: z.boolean(),
});

const PublishPdfResponseSchema = z.object({
  outputPath: z.string(),
  sheetsPublished: z.number(),
  published: z.boolean(),
});

const ExportSheetSetResponseSchema = z.object({
  sheetSetName: z.string(),
  outputPath: z.string(),
  sheetsExported: z.number(),
  exported: z.boolean(),
});

// ---------------------------------------------------------------------------
// Schema validation tests
// ---------------------------------------------------------------------------

describe("civil3d_plan_production — schema validation", () => {
  // ------ Sheet set list ------
  describe("civil3d_sheet_set_list", () => {
    it("validates a list response with one sheet set", () => {
      const data = {
        sheetSets: [
          { name: "Road Plans", handle: "A1B2", description: "Main road plan set", sheetCount: 5 },
        ],
      };
      expect(() => SheetSetListResponseSchema.parse(data)).not.toThrow();
    });

    it("validates an empty sheet set list", () => {
      const data = { sheetSets: [] };
      expect(() => SheetSetListResponseSchema.parse(data)).not.toThrow();
    });

    it("validates nullable description", () => {
      const data = {
        sheetSets: [{ name: "Plans", handle: "1234", description: null, sheetCount: 0 }],
      };
      expect(() => SheetSetListResponseSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Sheet set get info ------
  describe("civil3d_sheet_set_get_info", () => {
    it("validates input with name", () => {
      expect(() => SheetSetGetInfoInputSchema.parse({ name: "Road Plans" })).not.toThrow();
    });

    it("validates response with sheets", () => {
      const data = {
        name: "Road Plans",
        handle: "A1",
        description: null,
        sheets: [
          { name: "Sheet 1", number: "C-01", handle: "B2", layoutName: "Layout1" },
          { name: "Sheet 2", number: "C-02", handle: "B3", layoutName: null },
        ],
      };
      expect(() => SheetSetInfoResponseSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Sheet set create ------
  describe("civil3d_sheet_set_create", () => {
    it("validates minimal create input", () => {
      expect(() => SheetSetCreateInputSchema.parse({ name: "New Set" })).not.toThrow();
    });

    it("validates create input with description", () => {
      expect(() =>
        SheetSetCreateInputSchema.parse({ name: "New Set", description: "A plan set" })
      ).not.toThrow();
    });

    it("validates create response", () => {
      const data = { name: "New Set", handle: "C3", created: true };
      expect(() => SheetSetCreateResponseSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Add sheet ------
  describe("civil3d_sheet_add", () => {
    it("validates minimal add sheet input", () => {
      expect(() =>
        SheetAddInputSchema.parse({ sheetSetName: "Road Plans", sheetName: "Sheet 1" })
      ).not.toThrow();
    });

    it("validates add sheet input with all optional fields", () => {
      expect(() =>
        SheetAddInputSchema.parse({
          sheetSetName: "Road Plans",
          sheetName: "Sheet 1",
          sheetNumber: "C-01",
          layoutName: "Layout1",
        })
      ).not.toThrow();
    });

    it("validates add sheet response", () => {
      const data = { name: "Sheet 1", number: "C-01", handle: "D4", added: true };
      expect(() => SheetAddResponseSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Get sheet properties ------
  describe("civil3d_sheet_get_properties", () => {
    it("validates input", () => {
      expect(() =>
        SheetGetPropertiesInputSchema.parse({ sheetSetName: "Road Plans", sheetName: "Sheet 1" })
      ).not.toThrow();
    });

    it("validates full detail response", () => {
      const data: z.infer<typeof SheetDetailSchema> = {
        name: "Sheet 1",
        number: "C-01",
        handle: "E5",
        layoutName: "Layout1",
        viewportScale: 50,
        alignmentName: "Main Road",
        profileName: "Ground Profile",
        titleBlock: "C:/templates/title.dwg",
      };
      expect(() => SheetDetailSchema.parse(data)).not.toThrow();
    });

    it("validates response with all nullable fields null", () => {
      const data = {
        name: "Sheet 1",
        number: "",
        handle: "F6",
        layoutName: null,
        viewportScale: null,
        alignmentName: null,
        profileName: null,
        titleBlock: null,
      };
      expect(() => SheetDetailSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Set title block ------
  describe("civil3d_sheet_set_title_block", () => {
    it("validates input", () => {
      expect(() =>
        SheetSetTitleBlockInputSchema.parse({
          sheetSetName: "Road Plans",
          sheetName: "Sheet 1",
          titleBlockPath: "C:/templates/title.dwg",
        })
      ).not.toThrow();
    });

    it("validates response", () => {
      const data = { sheetName: "Sheet 1", titleBlock: "C:/templates/title.dwg", updated: true };
      expect(() => SheetSetTitleBlockResponseSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Plan/profile sheet create ------
  describe("civil3d_plan_profile_sheet_create", () => {
    it("validates minimal input (alignment only)", () => {
      expect(() =>
        PlanProfileSheetCreateInputSchema.parse({
          sheetSetName: "Road Plans",
          alignmentName: "Main Road",
        })
      ).not.toThrow();
    });

    it("validates full input", () => {
      expect(() =>
        PlanProfileSheetCreateInputSchema.parse({
          sheetSetName: "Road Plans",
          alignmentName: "Main Road",
          profileName: "EG Profile",
          sheetTemplatePath: "C:/templates/plan-profile.dwt",
          startStation: 0.0,
          endStation: 500.0,
          viewScale: 50,
        })
      ).not.toThrow();
    });

    it("validates response", () => {
      const data = {
        sheetName: "Main Road Plan/Profile",
        handle: "G7",
        alignmentName: "Main Road",
        profileName: "EG Profile",
        created: true,
      };
      expect(() => PlanProfileSheetCreateResponseSchema.parse(data)).not.toThrow();
    });

    it("validates response with null profileName", () => {
      const data = {
        sheetName: "Main Road Plan",
        handle: "H8",
        alignmentName: "Main Road",
        profileName: null,
        created: true,
      };
      expect(() => PlanProfileSheetCreateResponseSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Update alignment ------
  describe("civil3d_plan_profile_sheet_update_alignment", () => {
    it("validates input", () => {
      expect(() =>
        PlanProfileSheetUpdateAlignmentInputSchema.parse({
          sheetSetName: "Road Plans",
          sheetName: "Sheet 1",
          alignmentName: "New Road",
        })
      ).not.toThrow();
    });

    it("validates input with profileName", () => {
      expect(() =>
        PlanProfileSheetUpdateAlignmentInputSchema.parse({
          sheetSetName: "Road Plans",
          sheetName: "Sheet 1",
          alignmentName: "New Road",
          profileName: "Design Profile",
        })
      ).not.toThrow();
    });
  });

  // ------ Sheet view create ------
  describe("civil3d_sheet_view_create", () => {
    it("validates minimal input (layout name only)", () => {
      expect(() =>
        SheetViewCreateInputSchema.parse({ layoutName: "Layout1" })
      ).not.toThrow();
    });

    it("validates full input", () => {
      expect(() =>
        SheetViewCreateInputSchema.parse({
          layoutName: "Layout1",
          viewName: "Main View",
          centerX: 12.0,
          centerY: 8.5,
          width: 24.0,
          height: 18.0,
          scale: 100,
        })
      ).not.toThrow();
    });

    it("validates response", () => {
      const data = { handle: "I9", layoutName: "Layout1", scale: 100, created: true };
      expect(() => SheetViewCreateResponseSchema.parse(data)).not.toThrow();
    });

    it("validates response with null scale", () => {
      const data = { handle: "J10", layoutName: "Layout1", scale: null, created: true };
      expect(() => SheetViewCreateResponseSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Sheet view set scale ------
  describe("civil3d_sheet_view_set_scale", () => {
    it("validates input", () => {
      expect(() =>
        SheetViewSetScaleInputSchema.parse({ layoutName: "Layout1", scale: 50 })
      ).not.toThrow();
    });

    it("validates input with viewport handle", () => {
      expect(() =>
        SheetViewSetScaleInputSchema.parse({ layoutName: "Layout1", viewportHandle: "K11", scale: 100 })
      ).not.toThrow();
    });

    it("rejects non-positive scale", () => {
      expect(() =>
        SheetViewSetScaleInputSchema.parse({ layoutName: "Layout1", scale: -10 })
      ).toThrow();
    });

    it("validates response", () => {
      const data = { handle: "L12", scale: 50, updated: true };
      expect(() => SheetViewSetScaleResponseSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Publish PDF ------
  describe("civil3d_sheet_publish_pdf", () => {
    it("validates minimal input", () => {
      expect(() =>
        PublishPdfInputSchema.parse({
          layoutNames: ["Layout1"],
          outputPath: "C:/output/sheets.pdf",
        })
      ).not.toThrow();
    });

    it("validates input with multiple layouts and options", () => {
      expect(() =>
        PublishPdfInputSchema.parse({
          layoutNames: ["Layout1", "Layout2", "Layout3"],
          outputPath: "C:/output/sheets.pdf",
          plotStyleTable: "monochrome.ctb",
          paperSize: "ARCH D (24.00 x 36.00 Inches)",
        })
      ).not.toThrow();
    });

    it("rejects empty layoutNames array", () => {
      expect(() =>
        PublishPdfInputSchema.parse({ layoutNames: [], outputPath: "C:/out.pdf" })
      ).toThrow();
    });

    it("validates response", () => {
      const data = { outputPath: "C:/output/sheets.pdf", sheetsPublished: 3, published: true };
      expect(() => PublishPdfResponseSchema.parse(data)).not.toThrow();
    });
  });

  // ------ Export sheet set ------
  describe("civil3d_sheet_set_export", () => {
    it("validates minimal input", () => {
      expect(() =>
        ExportSheetSetInputSchema.parse({
          sheetSetName: "Road Plans",
          outputPath: "C:/output/road-plans.pdf",
        })
      ).not.toThrow();
    });

    it("validates input with plot style table", () => {
      expect(() =>
        ExportSheetSetInputSchema.parse({
          sheetSetName: "Road Plans",
          outputPath: "C:/output/road-plans.pdf",
          plotStyleTable: "monochrome.ctb",
        })
      ).not.toThrow();
    });

    it("validates response", () => {
      const data = {
        sheetSetName: "Road Plans",
        outputPath: "C:/output/road-plans.pdf",
        sheetsExported: 12,
        exported: true,
      };
      expect(() => ExportSheetSetResponseSchema.parse(data)).not.toThrow();
    });
  });
});

// ---------------------------------------------------------------------------
// Tool registration smoke tests
// ---------------------------------------------------------------------------

describe("civil3d_plan_production — tool registration", () => {
  it("registers all 12 plan production tools without error", async () => {
    const registeredTools: string[] = [];

    // Minimal McpServer mock
    const mockServer = {
      tool: (name: string, _description: string, _schema: unknown, _handler: unknown) => {
        registeredTools.push(name);
      },
    } as unknown as McpServer;

    const { registerCivil3DPlanProductionTools } = await import(
      "../src/tools/civil3d_plan_production.js"
    );
    registerCivil3DPlanProductionTools(mockServer);

    const expected = [
      "civil3d_sheet_set_list",
      "civil3d_sheet_set_get_info",
      "civil3d_sheet_set_create",
      "civil3d_sheet_add",
      "civil3d_sheet_get_properties",
      "civil3d_sheet_set_title_block",
      "civil3d_plan_profile_sheet_create",
      "civil3d_plan_profile_sheet_update_alignment",
      "civil3d_sheet_view_create",
      "civil3d_sheet_view_set_scale",
      "civil3d_sheet_publish_pdf",
      "civil3d_sheet_set_export",
    ];

    expect(registeredTools).toEqual(expected);
    expect(registeredTools).toHaveLength(12);
  });
});
