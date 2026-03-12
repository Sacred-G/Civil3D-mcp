import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DParcelInputShape = {
  action: z.enum(["list_sites", "list", "get"]),
  siteName: z.string().optional(),
  parcelName: z.string().optional(),
};

const Civil3DParcelInputSchema = z.object(Civil3DParcelInputShape);

const ParcelListSitesArgsSchema = Civil3DParcelInputSchema.extend({
  action: z.literal("list_sites"),
});

const ParcelListArgsSchema = Civil3DParcelInputSchema.extend({
  action: z.literal("list"),
  siteName: z.string(),
});

const ParcelGetArgsSchema = Civil3DParcelInputSchema.extend({
  action: z.literal("get"),
  siteName: z.string(),
  parcelName: z.string(),
});

const ParcelSiteSchema = z
  .object({
    name: z.string().optional(),
    handle: z.string().optional(),
    parcelCount: z.number().optional(),
  })
  .passthrough();

const ParcelSitesResponseSchema = z.object({
  sites: z.array(ParcelSiteSchema),
});

const ParcelSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  number: z.number(),
  area: z.number(),
  perimeter: z.number(),
  style: z.string(),
});

const ParcelListResponseSchema = z.object({
  siteName: z.string(),
  parcels: z.array(ParcelSummarySchema),
  units: z.object({
    area: z.string(),
    length: z.string(),
  }),
});

const ParcelDetailResponseSchema = z
  .object({
    siteName: z.string().optional(),
    name: z.string().optional(),
    handle: z.string().optional(),
    number: z.number().optional(),
    area: z.number().optional(),
    perimeter: z.number().optional(),
    style: z.string().optional(),
  })
  .passthrough();

export function registerCivil3DParcelTool(server: McpServer) {
  server.tool(
    "civil3d_parcel",
    "Reads Civil 3D parcel and site data.",
    Civil3DParcelInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list_sites"
            ? ParcelListSitesArgsSchema.parse(args)
            : args.action === "list"
              ? ParcelListArgsSchema.parse(args)
              : ParcelGetArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list_sites") {
            return await appClient.sendCommand("listParcelSites", {});
          }

          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listParcels", {
              siteName: parsedArgs.siteName,
            });
          }

          return await appClient.sendCommand("getParcel", {
            siteName: parsedArgs.siteName,
            parcelName: parsedArgs.parcelName,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list_sites"
            ? ParcelSitesResponseSchema.parse(response)
            : parsedArgs.action === "list"
              ? ParcelListResponseSchema.parse(response)
              : ParcelDetailResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_parcel action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_parcel tool:", error);
        return {
          content: [
            {
              type: "text",
              text: errorMessage,
            },
          ],
          isError: true,
        };
      }
    }
  );
}
