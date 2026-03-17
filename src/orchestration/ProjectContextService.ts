import { withApplicationConnection } from "../utils/ConnectionManager.js";

export interface ProjectContext {
  drawingInfo: unknown | null;
  objectTypes: string[];
  selectedObjects: unknown[];
}

export async function getProjectContext(selectedObjectLimit = 25): Promise<ProjectContext> {
  return await withApplicationConnection(async (appClient) => {
    const [drawingInfo, objectTypesResponse, selectedObjectsResponse] = await Promise.all([
      appClient.sendCommand("getDrawingInfo", {}),
      appClient.sendCommand("listCivilObjectTypes", {}),
      appClient.sendCommand("getSelectedCivilObjectsInfo", { limit: selectedObjectLimit }),
    ]);

    return {
      drawingInfo: drawingInfo ?? null,
      objectTypes: Array.isArray(objectTypesResponse) ? objectTypesResponse : [],
      selectedObjects: Array.isArray(selectedObjectsResponse) ? selectedObjectsResponse : [],
    };
  });
}
