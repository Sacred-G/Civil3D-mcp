import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

// ---------------------------------------------------------------------------
// Shared schemas
// ---------------------------------------------------------------------------

const Point3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number().optional().default(0),
});

const PressureNetworkSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  pipeCount: z.number(),
  fittingCount: z.number(),
  appurtenanceCount: z.number(),
  partsList: z.string().nullable(),
});

const PressurePipeSchema = z.object({
  name: z.string(),
  handle: z.string(),
  diameter: z.number(),
  length: z.number(),
  material: z.string(),
  startPoint: z.object({ x: z.number(), y: z.number(), z: z.number() }).nullable(),
  endPoint: z.object({ x: z.number(), y: z.number(), z: z.number() }).nullable(),
  coverDepth: z.number().nullable(),
});

const PressureFittingSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.string(),
  position: z.object({ x: z.number(), y: z.number(), z: z.number() }).nullable(),
  partSize: z.string().nullable(),
});

const PressureAppurtenanceSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.string(),
  position: z.object({ x: z.number(), y: z.number(), z: z.number() }).nullable(),
  partSize: z.string().nullable(),
});

const GenericResponseSchema = z.object({}).passthrough();

// ---------------------------------------------------------------------------
// 1. civil3d_pressure_network_list
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkListTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_network_list",
    "Lists all pressure networks in the active Civil 3D drawing with summary counts for pipes, fittings, and appurtenances.",
    {},
    async (_args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listPressureNetworks", {});
        });
        const validated = z.object({ networks: z.array(PressureNetworkSummarySchema) }).parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_network_list", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 2. civil3d_pressure_network_get_info
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkGetInfoTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_network_get_info",
    "Gets detailed information about a pressure network including its pipes, fittings, and appurtenances.",
    { name: z.string().describe("Pressure network name") },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getPressureNetworkInfo", { name: args.name });
        });
        const validated = z
          .object({
            name: z.string(),
            handle: z.string(),
            partsList: z.string().nullable(),
            pipes: z.array(PressurePipeSchema),
            fittings: z.array(PressureFittingSchema),
            appurtenances: z.array(PressureAppurtenanceSchema),
          })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_network_get_info", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 3. civil3d_pressure_network_create
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkCreateTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_network_create",
    "Creates a new pressure network in the active Civil 3D drawing.",
    {
      name: z.string().describe("New network name"),
      partsList: z.string().describe("Parts list (catalog) name to assign"),
      layer: z.string().optional().describe("Layer for the network objects"),
      referenceAlignment: z.string().optional().describe("Reference alignment name"),
      referenceSurface: z.string().optional().describe("Reference surface name"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createPressureNetwork", {
            name: args.name,
            partsList: args.partsList,
            layer: args.layer,
            referenceAlignment: args.referenceAlignment,
            referenceSurface: args.referenceSurface,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_network_create", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 4. civil3d_pressure_network_delete
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkDeleteTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_network_delete",
    "Deletes a pressure network and all its components from the drawing.",
    { name: z.string().describe("Pressure network name to delete") },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("deletePressureNetwork", { name: args.name });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_network_delete", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 5. civil3d_pressure_network_assign_parts_list
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkAssignPartsListTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_network_assign_parts_list",
    "Assigns a parts list (pressure pipe catalog) to an existing pressure network.",
    {
      networkName: z.string().describe("Pressure network name"),
      partsList: z.string().describe("Parts list name to assign"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("assignPressurePartsList", {
            networkName: args.networkName,
            partsList: args.partsList,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_network_assign_parts_list", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 6. civil3d_pressure_network_set_cover
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkSetCoverTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_network_set_cover",
    "Sets the minimum cover depth for pipes in a pressure network.",
    {
      networkName: z.string().describe("Pressure network name"),
      minCoverDepth: z.number().describe("Minimum cover depth (drawing units)"),
      maxCoverDepth: z.number().optional().describe("Maximum cover depth (drawing units)"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("setPressureNetworkCover", {
            networkName: args.networkName,
            minCoverDepth: args.minCoverDepth,
            maxCoverDepth: args.maxCoverDepth,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_network_set_cover", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 7. civil3d_pressure_network_validate
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkValidateTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_network_validate",
    "Validates a pressure network for cover violations, disconnected components, and parts catalog mismatches.",
    { networkName: z.string().describe("Pressure network name to validate") },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("validatePressureNetwork", { networkName: args.networkName });
        });
        const validated = z
          .object({
            networkName: z.string(),
            valid: z.boolean(),
            issues: z.array(
              z.object({
                type: z.string(),
                severity: z.enum(["error", "warning", "info"]),
                message: z.string(),
                objectHandle: z.string().nullable(),
              })
            ),
          })
          .parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_network_validate", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 8. civil3d_pressure_network_export
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkExportTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_network_export",
    "Exports pressure network data as structured JSON including pipes, fittings, and appurtenances with full property details.",
    {
      networkName: z.string().describe("Pressure network name to export"),
      includeCoordinates: z
        .boolean()
        .optional()
        .default(true)
        .describe("Include XYZ coordinates for all components"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("exportPressureNetwork", {
            networkName: args.networkName,
            includeCoordinates: args.includeCoordinates,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_network_export", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 9. civil3d_pressure_network_connect
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkConnectTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_network_connect",
    "Connects two pressure networks by merging the source network into the target network.",
    {
      targetNetwork: z.string().describe("Network to merge into"),
      sourceNetwork: z.string().describe("Network to merge from (will be deleted after merge)"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("connectPressureNetworks", {
            targetNetwork: args.targetNetwork,
            sourceNetwork: args.sourceNetwork,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_network_connect", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 10. civil3d_pressure_pipe_add
// ---------------------------------------------------------------------------

export function registerCivil3DPressurePipeAddTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_pipe_add",
    "Adds a pressure pipe segment to an existing pressure network.",
    {
      networkName: z.string().describe("Target pressure network name"),
      partName: z.string().describe("Part name from the network parts list"),
      startPoint: Point3DSchema.describe("Start point of the pipe"),
      endPoint: Point3DSchema.describe("End point of the pipe"),
      diameter: z.number().optional().describe("Override inner diameter (drawing units)"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("addPressurePipe", {
            networkName: args.networkName,
            partName: args.partName,
            startPoint: args.startPoint,
            endPoint: args.endPoint,
            diameter: args.diameter,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_pipe_add", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 11. civil3d_pressure_pipe_get_properties
// ---------------------------------------------------------------------------

export function registerCivil3DPressurePipeGetPropertiesTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_pipe_get_properties",
    "Gets detailed properties of a specific pressure pipe including diameter, length, material, and cover depth.",
    {
      networkName: z.string().describe("Pressure network name"),
      pipeName: z.string().describe("Pipe name within the network"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getPressurePipeProperties", {
            networkName: args.networkName,
            pipeName: args.pipeName,
          });
        });
        const validated = PressurePipeSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_pipe_get_properties", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 12. civil3d_pressure_pipe_resize
// ---------------------------------------------------------------------------

export function registerCivil3DPressurePipeResizeTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_pipe_resize",
    "Changes the size (part) of an existing pressure pipe to a different entry in the parts list.",
    {
      networkName: z.string().describe("Pressure network name"),
      pipeName: z.string().describe("Pipe name to resize"),
      newPartName: z.string().describe("New part name from the parts list"),
      newDiameter: z.number().optional().describe("New inner diameter override"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("resizePressurePipe", {
            networkName: args.networkName,
            pipeName: args.pipeName,
            newPartName: args.newPartName,
            newDiameter: args.newDiameter,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_pipe_resize", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 13. civil3d_pressure_fitting_add
// ---------------------------------------------------------------------------

export function registerCivil3DPressureFittingAddTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_fitting_add",
    "Adds a pressure fitting (elbow, tee, reducer, cap, etc.) to a pressure network at a specified location.",
    {
      networkName: z.string().describe("Target pressure network name"),
      partName: z.string().describe("Fitting part name from the parts list"),
      position: Point3DSchema.describe("Insertion point for the fitting"),
      rotation: z.number().optional().describe("Rotation angle in radians"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("addPressureFitting", {
            networkName: args.networkName,
            partName: args.partName,
            position: args.position,
            rotation: args.rotation,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_fitting_add", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 14. civil3d_pressure_fitting_get_properties
// ---------------------------------------------------------------------------

export function registerCivil3DPressureFittingGetPropertiesTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_fitting_get_properties",
    "Gets detailed properties of a specific pressure fitting including type, position, and part size.",
    {
      networkName: z.string().describe("Pressure network name"),
      fittingName: z.string().describe("Fitting name within the network"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getPressureFittingProperties", {
            networkName: args.networkName,
            fittingName: args.fittingName,
          });
        });
        const validated = PressureFittingSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_fitting_get_properties", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// 15. civil3d_pressure_appurtenance_add
// ---------------------------------------------------------------------------

export function registerCivil3DPressureAppurtenanceAddTool(server: McpServer) {
  server.tool(
    "civil3d_pressure_appurtenance_add",
    "Adds a pressure appurtenance (valve, hydrant, meter, air release valve, etc.) to a pressure network.",
    {
      networkName: z.string().describe("Target pressure network name"),
      partName: z.string().describe("Appurtenance part name from the parts list"),
      position: Point3DSchema.describe("Insertion point for the appurtenance"),
      rotation: z.number().optional().describe("Rotation angle in radians"),
      onPipeName: z.string().optional().describe("Snap the appurtenance onto an existing pipe by name"),
    },
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("addPressureAppurtenance", {
            networkName: args.networkName,
            partName: args.partName,
            position: args.position,
            rotation: args.rotation,
            onPipeName: args.onPipeName,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pressure_appurtenance_add", error);
      }
    }
  );
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function errorResult(toolName: string, error: unknown) {
  let message = `Failed to execute ${toolName}`;
  if (error instanceof Error) {
    message += `: ${error.message}`;
  } else if (typeof error === "string") {
    message += `: ${error}`;
  }
  console.error(`Error in ${toolName}:`, error);
  return {
    content: [{ type: "text" as const, text: message }],
    isError: true,
  };
}

// ---------------------------------------------------------------------------
// Bulk register
// ---------------------------------------------------------------------------

export function registerCivil3DPressureNetworkTools(server: McpServer) {
  registerCivil3DPressureNetworkListTool(server);
  registerCivil3DPressureNetworkGetInfoTool(server);
  registerCivil3DPressureNetworkCreateTool(server);
  registerCivil3DPressureNetworkDeleteTool(server);
  registerCivil3DPressureNetworkAssignPartsListTool(server);
  registerCivil3DPressureNetworkSetCoverTool(server);
  registerCivil3DPressureNetworkValidateTool(server);
  registerCivil3DPressureNetworkExportTool(server);
  registerCivil3DPressureNetworkConnectTool(server);
  registerCivil3DPressurePipeAddTool(server);
  registerCivil3DPressurePipeGetPropertiesTool(server);
  registerCivil3DPressurePipeResizeTool(server);
  registerCivil3DPressureFittingAddTool(server);
  registerCivil3DPressureFittingGetPropertiesTool(server);
  registerCivil3DPressureAppurtenanceAddTool(server);
}
