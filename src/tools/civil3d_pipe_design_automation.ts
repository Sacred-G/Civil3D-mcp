import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const PipeFlowSchema = z.object({
  pipeName: z.string(),
  designFlow: z.number().positive(),
});

const PipeNetworkSizingInputSchema = z.object({
  networkName: z.string().describe("Gravity pipe network name to size."),
  partsList: z.string().optional().describe("Parts list to evaluate. Defaults to the network parts list when available."),
  defaultDesignFlow: z.number().positive().optional().describe("Fallback design flow applied to any pipe without an explicit per-pipe design flow."),
  perPipeDesignFlows: z.array(PipeFlowSchema).optional().describe("Per-pipe design flows used for sizing calculations."),
  manningsN: z.number().positive().optional().default(0.013).describe("Manning roughness coefficient."),
  targetVelocityMin: z.number().positive().optional().default(2.0).describe("Minimum preferred full-flow velocity."),
  targetVelocityMax: z.number().positive().optional().default(10.0).describe("Maximum preferred full-flow velocity."),
  applyChanges: z.boolean().optional().default(false).describe("When true, resize the drawing pipes to the selected parts."),
});

const PipeProfileViewAutomationInputSchema = z.object({
  networkName: z.string().describe("Gravity pipe network name."),
  profileViewName: z.string().describe("Name for the profile view to create."),
  insertX: z.number().describe("Profile view insertion X coordinate."),
  insertY: z.number().describe("Profile view insertion Y coordinate."),
  alignmentName: z.string().optional().describe("Override alignment name. Defaults to the network reference alignment."),
  surfaceName: z.string().optional().describe("Override existing-ground surface. Defaults to the network reference surface."),
  existingProfileName: z.string().optional().describe("Use an existing profile instead of creating one."),
  surfaceProfileName: z.string().optional().describe("Name for a surface profile to create if needed."),
  createSurfaceProfileIfMissing: z.boolean().optional().default(true).describe("Create an EG surface profile when one is not supplied and none exists with the target name."),
  style: z.string().optional().describe("Optional profile view style."),
  bandSet: z.string().optional().describe("Optional profile view band set."),
});

type PipeNetworkDetail = {
  name: string;
  partsList?: string;
  referenceAlignment?: string;
  referenceSurface?: string;
  pipes: Array<{
    name: string;
    diameter: number;
    slope: number;
    length: number;
  }>;
};

type PartsCatalogResponse = {
  partsLists: Array<{
    name: string;
    parts: string[];
  }>;
};

function errorResult(toolName: string, error: unknown) {
  const message = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return {
    content: [{ type: "text" as const, text: `${toolName} failed: ${message}` }],
    isError: true,
  };
}

function parsePartDiameter(partName: string): number | null {
  const match = partName.match(/(\d+(?:\.\d+)?)/);
  if (!match) {
    return null;
  }

  const value = Number(match[1]);
  return Number.isFinite(value) ? value : null;
}

function computeFullFlowCapacity(diameter: number, slopePct: number, manningsN: number): number {
  const slope = Math.max(Math.abs(slopePct) / 100, 1e-6);
  const area = Math.PI * diameter * diameter / 4;
  const hydraulicRadius = diameter / 4;
  return (1 / manningsN) * area * Math.pow(hydraulicRadius, 2 / 3) * Math.sqrt(slope);
}

function computeVelocity(flow: number, diameter: number): number {
  const area = Math.PI * diameter * diameter / 4;
  return area > 0 ? flow / area : 0;
}

function solveRequiredDiameter(flow: number, slopePct: number, manningsN: number): number {
  let low = 0.01;
  let high = 100;

  while (computeFullFlowCapacity(high, slopePct, manningsN) < flow && high < 1_000_000) {
    high *= 2;
  }

  for (let i = 0; i < 60; i++) {
    const mid = (low + high) / 2;
    const capacity = computeFullFlowCapacity(mid, slopePct, manningsN);
    if (capacity >= flow) {
      high = mid;
    } else {
      low = mid;
    }
  }

  return high;
}

function chooseBestPart(
  partNames: string[],
  requiredDiameter: number,
  flow: number,
  slopePct: number,
  manningsN: number,
  velocityMin: number,
  velocityMax: number,
) {
  const candidates = partNames
    .map((name) => ({ name, diameter: parsePartDiameter(name) }))
    .filter((candidate): candidate is { name: string; diameter: number } => candidate.diameter != null)
    .sort((a, b) => a.diameter - b.diameter);

  const preferred = candidates.find((candidate) => {
    if (candidate.diameter < requiredDiameter) {
      return false;
    }

    const velocity = computeVelocity(flow, candidate.diameter);
    return velocity >= velocityMin && velocity <= velocityMax;
  });

  if (preferred) {
    return preferred;
  }

  return candidates.find((candidate) => candidate.diameter >= requiredDiameter) ?? candidates[candidates.length - 1] ?? null;
}

export function registerCivil3DPipeDesignAutomationTools(server: McpServer) {
  server.tool(
    "civil3d_pipe_network_size",
    "Sizes gravity-network pipes from Manning full-flow capacity, chooses matching catalog parts, and optionally applies the selected sizes back to the drawing.",
    PipeNetworkSizingInputSchema.shape,
    async (rawArgs) => {
      try {
        const args = PipeNetworkSizingInputSchema.parse(rawArgs);
        const result = await withApplicationConnection(async (appClient) => {
          const network = await appClient.sendCommand("getPipeNetwork", { name: args.networkName }) as PipeNetworkDetail;
          const partsListName = args.partsList ?? network.partsList;
          if (!partsListName) {
            throw new Error("No parts list was provided and the pipe network does not expose one.");
          }

          const catalog = await appClient.sendCommand("listPipePartsCatalog", { partsList: partsListName }) as PartsCatalogResponse;
          const parts = catalog.partsLists.find((item) => item.name === partsListName)?.parts ?? [];
          if (parts.length === 0) {
            throw new Error(`Parts list '${partsListName}' does not contain any pipe parts.`);
          }

          const flowMap = new Map((args.perPipeDesignFlows ?? []).map((item) => [item.pipeName.toLowerCase(), item.designFlow]));
          const recommendations: Array<Record<string, unknown>> = [];
          let appliedCount = 0;

          for (const pipe of network.pipes) {
            const designFlow = flowMap.get(pipe.name.toLowerCase()) ?? args.defaultDesignFlow;
            if (!designFlow) {
              recommendations.push({
                pipeName: pipe.name,
                currentDiameter: pipe.diameter,
                status: "skipped",
                reason: "No design flow was supplied for this pipe.",
              });
              continue;
            }

            const requiredDiameter = solveRequiredDiameter(designFlow, pipe.slope, args.manningsN);
            const selectedPart = chooseBestPart(
              parts,
              requiredDiameter,
              designFlow,
              pipe.slope,
              args.manningsN,
              args.targetVelocityMin,
              args.targetVelocityMax,
            );

            if (!selectedPart) {
              recommendations.push({
                pipeName: pipe.name,
                currentDiameter: pipe.diameter,
                status: "skipped",
                reason: "No catalog part could be parsed into a numeric diameter.",
              });
              continue;
            }

            const selectedVelocity = computeVelocity(designFlow, selectedPart.diameter);
            const shouldApply = args.applyChanges
              && (selectedPart.name !== pipe.name || Math.abs(selectedPart.diameter - pipe.diameter) > 1e-6);

            if (shouldApply) {
              await appClient.sendCommand("resizePipeInNetwork", {
                networkName: args.networkName,
                pipeName: pipe.name,
                newPartName: selectedPart.name,
                newDiameter: selectedPart.diameter,
              });
              appliedCount++;
            }

            recommendations.push({
              pipeName: pipe.name,
              currentDiameter: pipe.diameter,
              designFlow,
              requiredDiameter: Number(requiredDiameter.toFixed(3)),
              selectedPart: selectedPart.name,
              selectedDiameter: selectedPart.diameter,
              selectedVelocity: Number(selectedVelocity.toFixed(3)),
              applied: shouldApply,
              status: "ok",
            });
          }

          return {
            networkName: args.networkName,
            partsList: partsListName,
            applyChanges: args.applyChanges,
            appliedCount,
            recommendations,
          };
        });

        return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pipe_network_size", error);
      }
    },
  );

  server.tool(
    "civil3d_pipe_profile_view_automation",
    "Automates a gravity-pipe profile-view setup by resolving the network alignment/surface, creating an EG profile if needed, and creating the profile view with optional style and band set.",
    PipeProfileViewAutomationInputSchema.shape,
    async (rawArgs) => {
      try {
        const args = PipeProfileViewAutomationInputSchema.parse(rawArgs);
        const result = await withApplicationConnection(async (appClient) => {
          const network = await appClient.sendCommand("getPipeNetwork", { name: args.networkName }) as PipeNetworkDetail;
          const alignmentName = args.alignmentName ?? network.referenceAlignment;
          if (!alignmentName) {
            throw new Error("The pipe network does not expose a reference alignment. Provide 'alignmentName' explicitly.");
          }

          const surfaceName = args.surfaceName ?? network.referenceSurface;
          let profileName = args.existingProfileName ?? args.surfaceProfileName ?? `EG_${alignmentName}`;

          const profileList = await appClient.sendCommand("listProfiles", { alignmentName }) as { profiles?: Array<{ name: string }> };
          const existingNames = new Set((profileList.profiles ?? []).map((profile) => profile.name.toLowerCase()));

          if (!existingNames.has(profileName.toLowerCase())) {
            if (!args.createSurfaceProfileIfMissing) {
              throw new Error(`Profile '${profileName}' does not exist and automatic creation is disabled.`);
            }

            if (!surfaceName) {
              throw new Error("No surface was supplied or found on the pipe network, so the EG profile cannot be created.");
            }

            await appClient.sendCommand("createProfileFromSurface", {
              alignmentName,
              profileName,
              surfaceName,
            });
          }

          const profileViewResult = await appClient.sendCommand("profileViewCreate", {
            alignmentName,
            profileViewName: args.profileViewName,
            insertX: args.insertX,
            insertY: args.insertY,
            style: args.style,
            bandSet: args.bandSet,
          });

          return {
            networkName: args.networkName,
            alignmentName,
            surfaceName: surfaceName ?? null,
            profileName,
            profileView: profileViewResult,
          };
        });

        return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pipe_profile_view_automation", error);
      }
    },
  );
}
