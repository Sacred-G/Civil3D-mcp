# Civil 3D MCP Server — Tool Catalog Specification

> **IMPORTANT:** This document is an original specification defining 19 foundational tools. The actual implementation has grown significantly and now includes **180+ tools** across all Civil 3D domains. See [README.md](../README.md) for the complete current tool catalog.
>
> This document defines every MCP tool, its actions, input parameters, and response schemas for the Civil 3D MCP Server. Tools are consolidated by domain to keep the total count within the LLM's optimal selection range. Implement tools in the phased order specified at the end of this document.

---

## Tool Architecture

All tools follow the  **consolidated action pattern** . Each tool covers a domain and accepts an `action` parameter that determines the specific operation. This keeps the total tool count between 20–25 while exposing the full Civil 3D API surface.

### Universal Response Schema

Every tool response must conform to this structure:

```
{
  status: "success" | "error" | "busy" | "pending",
  data: object | null,
  error: { code: string, message: string, details: string | null } | null,
  executionTimeMs: number,
  warnings: string[],
  unsavedChanges: boolean
}
```

### Universal Error Codes

All tools share these error code prefixes:

* `CIVIL3D.NOT_CONNECTED` — Plugin is not responding
* `CIVIL3D.NO_DRAWING` — No drawing is open
* `CIVIL3D.READ_ONLY` — Drawing is in read-only state
* `CIVIL3D.OBJECT_NOT_FOUND` — Referenced object does not exist
* `CIVIL3D.BUSY` — Another operation is in progress
* `CIVIL3D.TIMEOUT` — Operation exceeded time limit
* `CIVIL3D.TRANSACTION_FAILED` — Database transaction could not commit
* `CIVIL3D.DEPENDENCY_CONFLICT` — Operation blocked by object dependencies
* `CIVIL3D.INVALID_INPUT` — Input validation failed at the plugin level

---

## System Tools

### Tool 1: `civil3d_drawing`

Manages the active drawing state, provides document-level information, and controls save/undo operations.

**Actions:**

#### `info`

Returns metadata about the currently active drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"info"` | yes      | —          |

**Response data:**

```
{
  fileName: string,
  filePath: string,
  coordinateSystem: string | null,
  linearUnits: "feet" | "meters" | "other",
  angularUnits: "degrees" | "radians" | "grads",
  unsavedChanges: boolean,
  objectCounts: {
    surfaces: number,
    alignments: number,
    profiles: number,
    corridors: number,
    pipeNetworks: number,
    points: number,
    parcels: number
  }
}
```

#### `save`

Saves the current drawing. Requires explicit confirmation from the LLM.

| Parameter | Type       | Required | Description                              |
| --------- | ---------- | -------- | ---------------------------------------- |
| action    | `"save"` | yes      | —                                       |
| saveAs    | string     | no       | If provided, saves to this new file path |

#### `undo`

Undoes the last operation. Uses Civil 3D's built-in undo stack.

| Parameter | Type       | Required | Description                               |
| --------- | ---------- | -------- | ----------------------------------------- |
| action    | `"undo"` | yes      | —                                        |
| steps     | number     | no       | Number of undo steps. Default: 1. Max: 10 |

#### `redo`

Redoes the last undone operation.

| Parameter | Type       | Required | Description                               |
| --------- | ---------- | -------- | ----------------------------------------- |
| action    | `"redo"` | yes      | —                                        |
| steps     | number     | no       | Number of redo steps. Default: 1. Max: 10 |

#### `settings`

Returns drawing-level ambient settings.

| Parameter | Type           | Required | Description |
| --------- | -------------- | -------- | ----------- |
| action    | `"settings"` | yes      | —          |

**Response data:**

```
{
  coordinateSystem: string | null,
  coordinateZone: string | null,
  datum: string | null,
  scaleFactor: number,
  elevationReference: string | null,
  defaultLayer: string,
  defaultStyles: {
    surface: string,
    alignment: string,
    profile: string,
    corridor: string,
    pipeNetwork: string
  }
}
```

---

### Tool 2: `civil3d_health`

Reports the status of the Civil 3D connection and plugin.

| Parameter | Type | Required | Description            |
| --------- | ---- | -------- | ---------------------- |
| —        | —   | —       | No parameters required |

**Response data:**

```
{
  connected: boolean,
  civil3dVersion: string,
  pluginVersion: string,
  drawingLoaded: boolean,
  operationInProgress: boolean,
  currentOperation: string | null,
  queueDepth: number,
  memoryUsageMb: number
}
```

---

### Tool 3: `civil3d_job`

Checks the status of long-running asynchronous operations (corridor rebuilds, large surface operations).

| Parameter | Type                        | Required | Description                                |
| --------- | --------------------------- | -------- | ------------------------------------------ |
| action    | `"status"`or `"cancel"` | yes      | —                                         |
| jobId     | string                      | yes      | The job ID returned by the initiating tool |

**Response data (status):**

```
{
  jobId: string,
  state: "running" | "completed" | "failed" | "cancelled",
  progressPercent: number | null,
  currentPhase: string | null,
  estimatedRemainingSeconds: number | null,
  result: object | null
}
```

---

### Tool 4: `civil3d_coordinate_system`

Provides coordinate system information and performs coordinate transformations.

**Actions:**

#### `info`

Returns the drawing's coordinate system configuration.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"info"` | yes      | —          |

**Response data:**

```
{
  name: string | null,
  zone: string | null,
  datum: string | null,
  projection: string | null,
  linearUnits: string,
  centralMeridian: number | null,
  falseEasting: number | null,
  falseNorthing: number | null,
  scaleFactor: number | null
}
```

#### `transform`

Converts coordinates between the drawing coordinate system and geographic coordinates.

| Parameter  | Type                             | Required | Description               |
| ---------- | -------------------------------- | -------- | ------------------------- |
| action     | `"transform"`                  | yes      | —                        |
| fromSystem | `"drawing"`or `"geographic"` | yes      | Source coordinate system  |
| toSystem   | `"drawing"`or `"geographic"` | yes      | Target coordinate system  |
| x          | number                           | yes      | X coordinate or longitude |
| y          | number                           | yes      | Y coordinate or latitude  |
| z          | number                           | no       | Z coordinate or elevation |

---

## Domain Tools — Surfaces

### Tool 5: `civil3d_surface`

Reads surface data, creates surfaces, and retrieves surface properties.

**Actions:**

#### `list`

Returns all surfaces in the current drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

**Response data:**

```
{
  surfaces: [
    {
      name: string,
      handle: string,
      type: "TIN" | "Grid" | "TINVolume",
      isReference: boolean,
      sourcePath: string | null
    }
  ]
}
```

#### `get`

Returns detailed properties of a specific surface.

| Parameter | Type      | Required | Description  |
| --------- | --------- | -------- | ------------ |
| action    | `"get"` | yes      | —           |
| name      | string    | yes      | Surface name |

**Response data:**

```
{
  name: string,
  handle: string,
  type: "TIN" | "Grid" | "TINVolume",
  style: string,
  layer: string,
  statistics: {
    minimumElevation: number,
    maximumElevation: number,
    meanElevation: number,
    area2d: number,
    area3d: number,
    numberOfPoints: number,
    numberOfTriangles: number
  },
  boundingBox: {
    minX: number, minY: number,
    maxX: number, maxY: number
  },
  units: string,
  isReference: boolean,
  dependentAlignments: string[],
  dependentCorridors: string[]
}
```

#### `get_elevation`

Samples the elevation at a specific point on a surface.

| Parameter | Type                | Required | Description                  |
| --------- | ------------------- | -------- | ---------------------------- |
| action    | `"get_elevation"` | yes      | —                           |
| name      | string              | yes      | Surface name                 |
| x         | number              | yes      | X coordinate (drawing units) |
| y         | number              | yes      | Y coordinate (drawing units) |

**Response data:**

```
{
  elevation: number,
  units: string,
  surfaceName: string
}
```

#### `get_elevation_along`

Samples elevations along a series of points (for cross-sections or profiles).

| Parameter | Type                           | Required | Description               |
| --------- | ------------------------------ | -------- | ------------------------- |
| action    | `"get_elevation_along"`      | yes      | —                        |
| name      | string                         | yes      | Surface name              |
| points    | `{ x: number, y: number }[]` | yes      | Array of coordinate pairs |

#### `get_statistics`

Returns computed statistics and analysis for a surface.

| Parameter    | Type                                                                | Required | Description                                             |
| ------------ | ------------------------------------------------------------------- | -------- | ------------------------------------------------------- |
| action       | `"get_statistics"`                                                | yes      | —                                                      |
| name         | string                                                              | yes      | Surface name                                            |
| analysisType | `"elevations"`or `"slopes"`or `"contours"`or `"watersheds"` | no       | Specific analysis. Default returns all basic statistics |

#### `create`

Creates a new empty TIN surface.

| Parameter   | Type         | Required | Description                                 |
| ----------- | ------------ | -------- | ------------------------------------------- |
| action      | `"create"` | yes      | —                                          |
| name        | string       | yes      | Surface name (must be unique)               |
| style       | string       | no       | Style name. Uses drawing default if omitted |
| layer       | string       | no       | Layer name. Uses drawing default if omitted |
| description | string       | no       | Optional description                        |

#### `delete`

Deletes a surface from the drawing.

| Parameter | Type         | Required | Description  |
| --------- | ------------ | -------- | ------------ |
| action    | `"delete"` | yes      | —           |
| name      | string       | yes      | Surface name |

---

### Tool 6: `civil3d_surface_edit`

Modifies surface data by adding points, breaklines, and boundaries.

**Actions:**

#### `add_points`

Adds point data to a surface.

| Parameter   | Type                                      | Required | Description                                     |
| ----------- | ----------------------------------------- | -------- | ----------------------------------------------- |
| action      | `"add_points"`                          | yes      | —                                              |
| name        | string                                    | yes      | Target surface name                             |
| points      | `{ x: number, y: number, z: number }[]` | yes      | Array of 3D points                              |
| description | string                                    | no       | Operation description for the surface build log |

#### `add_breakline`

Adds a breakline to a surface.

| Parameter     | Type                                          | Required | Description           |
| ------------- | --------------------------------------------- | -------- | --------------------- |
| action        | `"add_breakline"`                           | yes      | —                    |
| name          | string                                        | yes      | Target surface name   |
| breaklineType | `"standard"`or `"wall"`or `"proximity"` | yes      | Breakline type        |
| points        | `{ x: number, y: number, z: number }[]`     | yes      | Array of 3D vertices  |
| description   | string                                        | no       | Breakline description |

#### `add_boundary`

Adds a boundary to a surface.

| Parameter    | Type                                                    | Required | Description                                   |
| ------------ | ------------------------------------------------------- | -------- | --------------------------------------------- |
| action       | `"add_boundary"`                                      | yes      | —                                            |
| name         | string                                                  | yes      | Target surface name                           |
| boundaryType | `"show"`or `"hide"`or `"outer"`or `"data_clip"` | yes      | Boundary type                                 |
| points       | `{ x: number, y: number }[]`                          | yes      | Array of 2D vertices forming a closed polygon |

#### `extract_contours`

Extracts contour lines from a surface at a specified interval.

| Parameter     | Type                   | Required | Description                             |
| ------------- | ---------------------- | -------- | --------------------------------------- |
| action        | `"extract_contours"` | yes      | —                                      |
| name          | string                 | yes      | Surface name                            |
| minorInterval | number                 | yes      | Minor contour interval in drawing units |
| majorInterval | number                 | yes      | Major contour interval in drawing units |

#### `compute_volume`

Computes cut/fill volumes between two surfaces.

| Parameter         | Type                 | Required | Description             |
| ----------------- | -------------------- | -------- | ----------------------- |
| action            | `"compute_volume"` | yes      | —                      |
| baseSurface       | string               | yes      | Base surface name       |
| comparisonSurface | string               | yes      | Comparison surface name |

**Response data:**

```
{
  cutVolume: number,
  fillVolume: number,
  netVolume: number,
  cutArea: number,
  fillArea: number,
  units: { volume: string, area: string }
}
```

---

## Domain Tools — Alignments

### Tool 7: `civil3d_alignment`

Reads and creates horizontal alignments.

**Actions:**

#### `list`

Returns all alignments in the current drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

**Response data:**

```
{
  alignments: [
    {
      name: string,
      handle: string,
      type: "centerline" | "offset" | "curb_return" | "rail",
      length: number,
      startStation: number,
      endStation: number,
      site: string | null,
      profileCount: number,
      isReference: boolean
    }
  ]
}
```

#### `get`

Returns detailed properties and entity breakdown of an alignment.

| Parameter | Type      | Required | Description    |
| --------- | --------- | -------- | -------------- |
| action    | `"get"` | yes      | —             |
| name      | string    | yes      | Alignment name |

**Response data:**

```
{
  name: string,
  handle: string,
  type: string,
  style: string,
  layer: string,
  length: number,
  startStation: number,
  endStation: number,
  entityCount: number,
  entities: [
    {
      index: number,
      type: "line" | "arc" | "spiral",
      startStation: number,
      endStation: number,
      length: number
    }
  ],
  dependentProfiles: string[],
  dependentCorridors: string[],
  isReference: boolean
}
```

#### `station_to_point`

Converts a station and offset value to drawing X/Y coordinates.

| Parameter | Type                   | Required | Description                                  |
| --------- | ---------------------- | -------- | -------------------------------------------- |
| action    | `"station_to_point"` | yes      | —                                           |
| name      | string                 | yes      | Alignment name                               |
| station   | number                 | yes      | Station value                                |
| offset    | number                 | no       | Offset from alignment centerline. Default: 0 |

**Response data:**

```
{
  x: number,
  y: number,
  station: number,
  offset: number,
  units: string
}
```

#### `point_to_station`

Converts drawing X/Y coordinates to the nearest station and offset on an alignment.

| Parameter | Type                   | Required | Description    |
| --------- | ---------------------- | -------- | -------------- |
| action    | `"point_to_station"` | yes      | —             |
| name      | string                 | yes      | Alignment name |
| x         | number                 | yes      | X coordinate   |
| y         | number                 | yes      | Y coordinate   |

**Response data:**

```
{
  station: number,
  offset: number,
  distanceFromAlignment: number,
  units: string
}
```

#### `create`

Creates a new alignment from a set of points.

| Parameter | Type                            | Required | Description                                               |
| --------- | ------------------------------- | -------- | --------------------------------------------------------- |
| action    | `"create"`                    | yes      | —                                                        |
| name      | string                          | yes      | Alignment name (must be unique)                           |
| points    | `{ x: number, y: number }[]`  | yes      | Array of pass-through points defining the horizontal path |
| type      | `"centerline"`or `"offset"` | no       | Default:`"centerline"`                                  |
| site      | string                          | no       | Site name. Uses default if omitted                        |
| style     | string                          | no       | Style name. Uses drawing default if omitted               |
| layer     | string                          | no       | Layer name. Uses drawing default if omitted               |
| labelSet  | string                          | no       | Label set name. Uses default if omitted                   |

#### `delete`

Deletes an alignment and optionally its dependent objects.

| Parameter | Type         | Required | Description    |
| --------- | ------------ | -------- | -------------- |
| action    | `"delete"` | yes      | —             |
| name      | string       | yes      | Alignment name |

---

## Domain Tools — Profiles

### Tool 8: `civil3d_profile`

Reads and creates vertical profiles along alignments.

**Actions:**

#### `list`

Returns all profiles for a specific alignment.

| Parameter     | Type       | Required | Description           |
| ------------- | ---------- | -------- | --------------------- |
| action        | `"list"` | yes      | —                    |
| alignmentName | string     | yes      | Parent alignment name |

**Response data:**

```
{
  alignmentName: string,
  profiles: [
    {
      name: string,
      handle: string,
      type: "surface" | "layout" | "superimposed",
      style: string,
      startStation: number,
      endStation: number,
      minElevation: number,
      maxElevation: number
    }
  ]
}
```

#### `get`

Returns detailed properties of a specific profile.

| Parameter     | Type      | Required | Description           |
| ------------- | --------- | -------- | --------------------- |
| action        | `"get"` | yes      | —                    |
| alignmentName | string    | yes      | Parent alignment name |
| profileName   | string    | yes      | Profile name          |

**Response data:**

```
{
  name: string,
  handle: string,
  type: string,
  style: string,
  layer: string,
  startStation: number,
  endStation: number,
  minElevation: number,
  maxElevation: number,
  entityCount: number,
  entities: [
    {
      index: number,
      type: "tangent" | "circular_curve" | "parabola" | "asymmetric_parabola",
      startStation: number,
      endStation: number,
      startElevation: number,
      endElevation: number,
      grade: number | null,
      length: number
    }
  ],
  pviCount: number,
  units: { horizontal: string, vertical: string }
}
```

#### `get_elevation`

Returns the elevation at a specific station along a profile.

| Parameter     | Type                | Required | Description           |
| ------------- | ------------------- | -------- | --------------------- |
| action        | `"get_elevation"` | yes      | —                    |
| alignmentName | string              | yes      | Parent alignment name |
| profileName   | string              | yes      | Profile name          |
| station       | number              | yes      | Station value         |

**Response data:**

```
{
  station: number,
  elevation: number,
  grade: number,
  units: string
}
```

#### `sample_elevations`

Returns elevations at regular intervals along the profile.

| Parameter     | Type                    | Required | Description                        |
| ------------- | ----------------------- | -------- | ---------------------------------- |
| action        | `"sample_elevations"` | yes      | —                                 |
| alignmentName | string                  | yes      | Parent alignment name              |
| profileName   | string                  | yes      | Profile name                       |
| startStation  | number                  | no       | Default: profile start             |
| endStation    | number                  | no       | Default: profile end               |
| interval      | number                  | yes      | Sampling interval in drawing units |

#### `create_from_surface`

Creates a new profile by sampling surface elevations along an alignment.

| Parameter     | Type                      | Required | Description                             |
| ------------- | ------------------------- | -------- | --------------------------------------- |
| action        | `"create_from_surface"` | yes      | —                                      |
| alignmentName | string                    | yes      | Parent alignment name                   |
| profileName   | string                    | yes      | New profile name                        |
| surfaceName   | string                    | yes      | Source surface to sample                |
| style         | string                    | no       | Profile style. Uses default if omitted  |
| layer         | string                    | no       | Layer name. Uses default if omitted     |
| labelSet      | string                    | no       | Label set name. Uses default if omitted |

#### `create_layout`

Creates a new empty layout profile for design.

| Parameter     | Type                | Required | Description                             |
| ------------- | ------------------- | -------- | --------------------------------------- |
| action        | `"create_layout"` | yes      | —                                      |
| alignmentName | string              | yes      | Parent alignment name                   |
| profileName   | string              | yes      | New profile name                        |
| style         | string              | no       | Profile style. Uses default if omitted  |
| layer         | string              | no       | Layer name. Uses default if omitted     |
| labelSet      | string              | no       | Label set name. Uses default if omitted |

#### `delete`

Deletes a profile.

| Parameter     | Type         | Required | Description           |
| ------------- | ------------ | -------- | --------------------- |
| action        | `"delete"` | yes      | —                    |
| alignmentName | string       | yes      | Parent alignment name |
| profileName   | string       | yes      | Profile name          |

---

## Domain Tools — Corridors

### Tool 9: `civil3d_corridor`

Reads corridor data and controls corridor rebuilds.

**Actions:**

#### `list`

Returns all corridors in the current drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

**Response data:**

```
{
  corridors: [
    {
      name: string,
      handle: string,
      baselineCount: number,
      regionCount: number,
      surfaceCount: number,
      state: "built" | "out_of_date" | "error",
      lastBuildTime: string | null
    }
  ]
}
```

#### `get`

Returns detailed properties of a corridor including baselines, regions, and surfaces.

| Parameter | Type      | Required | Description   |
| --------- | --------- | -------- | ------------- |
| action    | `"get"` | yes      | —            |
| name      | string    | yes      | Corridor name |

**Response data:**

```
{
  name: string,
  handle: string,
  style: string,
  layer: string,
  baselines: [
    {
      name: string,
      alignmentName: string,
      profileName: string,
      regions: [
        {
          name: string,
          assemblyName: string,
          startStation: number,
          endStation: number,
          frequency: number
        }
      ]
    }
  ],
  surfaces: [
    {
      name: string,
      boundaries: string[]
    }
  ],
  featureLineCount: number,
  state: "built" | "out_of_date" | "error"
}
```

#### `rebuild`

Triggers a corridor rebuild. This is a long-running operation — returns a job ID.

| Parameter | Type          | Required | Description   |
| --------- | ------------- | -------- | ------------- |
| action    | `"rebuild"` | yes      | —            |
| name      | string        | yes      | Corridor name |

**Response:**
Returns `status: "pending"` with a `jobId` to poll via `civil3d_job`.

#### `get_surfaces`

Lists all surfaces generated by a corridor.

| Parameter | Type               | Required | Description   |
| --------- | ------------------ | -------- | ------------- |
| action    | `"get_surfaces"` | yes      | —            |
| name      | string             | yes      | Corridor name |

#### `get_feature_lines`

Lists feature lines extracted from a corridor.

| Parameter | Type                    | Required | Description   |
| --------- | ----------------------- | -------- | ------------- |
| action    | `"get_feature_lines"` | yes      | —            |
| name      | string                  | yes      | Corridor name |

#### `compute_volumes`

Computes cut/fill volumes for a corridor surface against a reference surface.

| Parameter        | Type                  | Required | Description                         |
| ---------------- | --------------------- | -------- | ----------------------------------- |
| action           | `"compute_volumes"` | yes      | —                                  |
| name             | string                | yes      | Corridor name                       |
| corridorSurface  | string                | yes      | Name of the corridor surface        |
| referenceSurface | string                | yes      | Name of the existing ground surface |

---

## Domain Tools — Pipe Networks

### Tool 10: `civil3d_pipe_network`

Reads pipe network data including networks, pipes, and structures.

**Actions:**

#### `list`

Returns all pipe networks in the drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

**Response data:**

```
{
  networks: [
    {
      name: string,
      handle: string,
      pipeCount: number,
      structureCount: number,
      surface: string | null
    }
  ]
}
```

#### `get`

Returns detailed properties of a pipe network.

| Parameter | Type      | Required | Description  |
| --------- | --------- | -------- | ------------ |
| action    | `"get"` | yes      | —           |
| name      | string    | yes      | Network name |

**Response data:**

```
{
  name: string,
  handle: string,
  style: string,
  referenceSurface: string | null,
  referenceAlignment: string | null,
  pipes: [
    {
      name: string,
      handle: string,
      startStructure: string | null,
      endStructure: string | null,
      length: number,
      diameter: number,
      slope: number,
      material: string,
      invertIn: number,
      invertOut: number
    }
  ],
  structures: [
    {
      name: string,
      handle: string,
      type: string,
      rimElevation: number,
      sumpElevation: number,
      x: number,
      y: number,
      connectedPipes: string[]
    }
  ]
}
```

#### `get_pipe`

Returns detailed properties of a single pipe.

| Parameter   | Type           | Required | Description  |
| ----------- | -------------- | -------- | ------------ |
| action      | `"get_pipe"` | yes      | —           |
| networkName | string         | yes      | Network name |
| pipeName    | string         | yes      | Pipe name    |

#### `get_structure`

Returns detailed properties of a single structure.

| Parameter     | Type                | Required | Description    |
| ------------- | ------------------- | -------- | -------------- |
| action        | `"get_structure"` | yes      | —             |
| networkName   | string              | yes      | Network name   |
| structureName | string              | yes      | Structure name |

#### `check_interference`

Runs an interference check between a pipe network and other objects.

| Parameter   | Type                               | Required | Description                     |
| ----------- | ---------------------------------- | -------- | ------------------------------- |
| action      | `"check_interference"`           | yes      | —                              |
| networkName | string                             | yes      | Network name                    |
| targetType  | `"surface"`or `"pipe_network"` | yes      | Type of object to check against |
| targetName  | string                             | yes      | Name of the target object       |

**Response data:**

```
{
  interferences: [
    {
      objectA: string,
      objectB: string,
      location: { x: number, y: number, z: number },
      clearance: number,
      conflictType: "crossing" | "proximity" | "cover_violation"
    }
  ],
  totalConflicts: number
}
```

---

### Tool 11: `civil3d_pipe_network_edit`

Creates and modifies pipe networks, pipes, and structures.

**Actions:**

#### `create`

Creates a new empty pipe network.

| Parameter          | Type         | Required | Description                   |
| ------------------ | ------------ | -------- | ----------------------------- |
| action             | `"create"` | yes      | —                            |
| name               | string       | yes      | Network name (must be unique) |
| partsList          | string       | yes      | Parts list name to use        |
| referenceSurface   | string       | no       | Reference surface name        |
| referenceAlignment | string       | no       | Reference alignment name      |
| style              | string       | no       | Network style                 |
| layer              | string       | no       | Layer name                    |

#### `add_pipe`

Adds a pipe between two points or structures.

| Parameter      | Type                                    | Required                   | Description                                      |
| -------------- | --------------------------------------- | -------------------------- | ------------------------------------------------ |
| action         | `"add_pipe"`                          | yes                        | —                                               |
| networkName    | string                                  | yes                        | Target network name                              |
| startPoint     | `{ x: number, y: number, z: number }` | yes (if no startStructure) | Start coordinates                                |
| endPoint       | `{ x: number, y: number, z: number }` | yes (if no endStructure)   | End coordinates                                  |
| startStructure | string                                  | no                         | Start structure name (alternative to startPoint) |
| endStructure   | string                                  | no                         | End structure name (alternative to endPoint)     |
| partName       | string                                  | yes                        | Pipe part name from the parts list               |
| diameter       | number                                  | no                         | Pipe diameter override                           |

#### `add_structure`

Adds a structure (manhole, inlet, etc.) to a pipe network.

| Parameter    | Type                | Required | Description                             |
| ------------ | ------------------- | -------- | --------------------------------------- |
| action       | `"add_structure"` | yes      | —                                      |
| networkName  | string              | yes      | Target network name                     |
| x            | number              | yes      | X coordinate                            |
| y            | number              | yes      | Y coordinate                            |
| partName     | string              | yes      | Structure part name from the parts list |
| rimElevation | number              | no       | Rim elevation override                  |
| sumpDepth    | number              | no       | Sump depth below lowest pipe            |

---

## Domain Tools — Points

### Tool 12: `civil3d_point`

Reads, creates, and manages COGO points and point groups.

**Actions:**

#### `list`

Returns all COGO points in the drawing or in a specific point group.

| Parameter | Type       | Required | Description                                      |
| --------- | ---------- | -------- | ------------------------------------------------ |
| action    | `"list"` | yes      | —                                               |
| groupName | string     | no       | If provided, returns only points in this group   |
| limit     | number     | no       | Maximum number of points to return. Default: 500 |
| offset    | number     | no       | Pagination offset. Default: 0                    |

**Response data:**

```
{
  totalCount: number,
  returnedCount: number,
  points: [
    {
      number: number,
      name: string | null,
      x: number,
      y: number,
      z: number,
      rawDescription: string,
      fullDescription: string
    }
  ],
  units: string
}
```

#### `get`

Returns detailed properties of a single point by point number.

| Parameter   | Type      | Required | Description  |
| ----------- | --------- | -------- | ------------ |
| action      | `"get"` | yes      | —           |
| pointNumber | number    | yes      | Point number |

#### `create`

Creates one or more COGO points.

| Parameter   | Type                                                    | Required  | Description                                    |
| ----------- | ------------------------------------------------------- | --------- | ---------------------------------------------- |
| action      | `"create"`                                            | yes       | —                                             |
| points      | `{ x: number, y: number, z: number, description: string | null }[]` | yes                                            |
| startNumber | number                                                  | no        | Starting point number. Auto-assigns if omitted |

#### `list_groups`

Returns all point groups in the drawing.

| Parameter | Type              | Required | Description |
| --------- | ----------------- | -------- | ----------- |
| action    | `"list_groups"` | yes      | —          |

**Response data:**

```
{
  groups: [
    {
      name: string,
      description: string | null,
      pointCount: number,
      includePattern: string | null,
      excludePattern: string | null
    }
  ]
}
```

#### `import`

Imports points from a structured dataset.

| Parameter     | Type                                              | Required | Description                                                         |
| ------------- | ------------------------------------------------- | -------- | ------------------------------------------------------------------- |
| action        | `"import"`                                      | yes      | —                                                                  |
| format        | `"pnezd"`or `"penz"`or `"xyzd"`or `"xyz"` | yes      | Point data format                                                   |
| data          | string                                            | yes      | Delimited point data (comma or space separated, one point per line) |
| targetSurface | string                                            | no       | If provided, adds imported points to this surface                   |

#### `delete`

Deletes points by number or range.

| Parameter    | Type         | Required | Description                      |
| ------------ | ------------ | -------- | -------------------------------- |
| action       | `"delete"` | yes      | —                               |
| pointNumbers | number[]     | yes      | Array of point numbers to delete |

---

## Domain Tools — Sections

### Tool 13: `civil3d_section`

Reads section data including sample lines and cross-sections.

**Actions:**

#### `list_sample_lines`

Returns all sample line groups for an alignment.

| Parameter     | Type                    | Required | Description           |
| ------------- | ----------------------- | -------- | --------------------- |
| action        | `"list_sample_lines"` | yes      | —                    |
| alignmentName | string                  | yes      | Parent alignment name |

**Response data:**

```
{
  sampleLineGroups: [
    {
      name: string,
      handle: string,
      sampleLineCount: number,
      stations: number[]
    }
  ]
}
```

#### `get_section_data`

Returns cross-section data at a specific station.

| Parameter           | Type                   | Required | Description            |
| ------------------- | ---------------------- | -------- | ---------------------- |
| action              | `"get_section_data"` | yes      | —                     |
| alignmentName       | string                 | yes      | Alignment name         |
| sampleLineGroupName | string                 | yes      | Sample line group name |
| station             | number                 | yes      | Station value          |

**Response data:**

```
{
  station: number,
  surfaces: [
    {
      surfaceName: string,
      offsets: number[],
      elevations: number[]
    }
  ],
  units: { horizontal: string, vertical: string }
}
```

#### `create_sample_lines`

Creates sample lines at specified stations along an alignment.

| Parameter     | Type                      | Required | Description                                  |
| ------------- | ------------------------- | -------- | -------------------------------------------- |
| action        | `"create_sample_lines"` | yes      | —                                           |
| alignmentName | string                    | yes      | Alignment name                               |
| groupName     | string                    | yes      | New sample line group name                   |
| stations      | number[]                  | no       | Explicit stations. If omitted, uses interval |
| interval      | number                    | no       | Station interval for automatic generation    |
| leftWidth     | number                    | yes      | Sample width to the left of alignment        |
| rightWidth    | number                    | yes      | Sample width to the right of alignment       |
| surfaces      | string[]                  | yes      | Surface names to sample                      |

---

## Domain Tools — Data Shortcuts

### Tool 14: `civil3d_data_shortcut`

Manages data shortcuts (cross-drawing references).

**Actions:**

#### `list`

Returns all data shortcuts (incoming references and exportable objects).

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

**Response data:**

```
{
  incoming: [
    {
      objectName: string,
      objectType: "surface" | "alignment" | "profile" | "pipe_network",
      sourceFilePath: string,
      isSynced: boolean,
      isValid: boolean
    }
  ],
  exportable: [
    {
      objectName: string,
      objectType: string,
      isExported: boolean
    }
  ]
}
```

#### `sync`

Synchronizes (updates) all incoming data shortcuts.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"sync"` | yes      | —          |

#### `create_reference`

Creates a data shortcut reference to an object in another drawing.

| Parameter      | Type                                                                 | Required | Description                     |
| -------------- | -------------------------------------------------------------------- | -------- | ------------------------------- |
| action         | `"create_reference"`                                               | yes      | —                              |
| sourceFilePath | string                                                               | yes      | Path to the source drawing      |
| objectName     | string                                                               | yes      | Name of the object to reference |
| objectType     | `"surface"`or `"alignment"`or `"profile"`or `"pipe_network"` | yes      | Type of object                  |

---

## Domain Tools — Styles & Labels

### Tool 15: `civil3d_style`

Lists and inspects styles for all Civil 3D object types.

**Actions:**

#### `list`

Returns all styles for a specific object type.

| Parameter  | Type                                                                                                                                                         | Required | Description                    |
| ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------- | ------------------------------ |
| action     | `"list"`                                                                                                                                                   | yes      | —                             |
| objectType | `"surface"`or `"alignment"`or `"profile"`or `"corridor"`or `"pipe"`or `"structure"`or `"point"`or `"section"`or `"label"`or `"assembly"` | yes      | Object type to list styles for |

**Response data:**

```
{
  objectType: string,
  styles: [
    {
      name: string,
      handle: string,
      isDefault: boolean
    }
  ]
}
```

#### `get`

Returns detailed properties of a specific style.

| Parameter  | Type      | Required | Description |
| ---------- | --------- | -------- | ----------- |
| action     | `"get"` | yes      | —          |
| objectType | string    | yes      | Object type |
| styleName  | string    | yes      | Style name  |

---

### Tool 16: `civil3d_label`

Manages labels on Civil 3D objects.

**Actions:**

#### `list`

Returns all labels on a specific object.

| Parameter  | Type                                                                 | Required | Description |
| ---------- | -------------------------------------------------------------------- | -------- | ----------- |
| action     | `"list"`                                                           | yes      | —          |
| objectType | `"alignment"`or `"profile"`or `"surface"`or `"pipe_network"` | yes      | Object type |
| objectName | string                                                               | yes      | Object name |

#### `add`

Adds a label to a Civil 3D object at a specified location.

| Parameter  | Type                         | Required | Description                                                                           |
| ---------- | ---------------------------- | -------- | ------------------------------------------------------------------------------------- |
| action     | `"add"`                    | yes      | —                                                                                    |
| objectType | string                       | yes      | Object type                                                                           |
| objectName | string                       | yes      | Object name                                                                           |
| labelType  | string                       | yes      | Label type (varies by object:`"station"`,`"elevation"`,`"slope"`,`"contour"`) |
| labelStyle | string                       | no       | Label style name. Uses default if omitted                                             |
| station    | number                       | no       | Station for alignment/profile labels                                                  |
| point      | `{ x: number, y: number }` | no       | Location for surface labels                                                           |

#### `list_styles`

Returns available label styles for a specific context.

| Parameter  | Type              | Required | Description |
| ---------- | ----------------- | -------- | ----------- |
| action     | `"list_styles"` | yes      | —          |
| objectType | string            | yes      | Object type |

---

## Domain Tools — Feature Lines

### Tool 17: `civil3d_feature_line`

Reads and manages feature lines.

**Actions:**

#### `list`

Returns all feature lines in the drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

#### `get`

Returns detailed properties of a feature line including vertices and elevations.

| Parameter | Type      | Required | Description                 |
| --------- | --------- | -------- | --------------------------- |
| action    | `"get"` | yes      | —                          |
| name      | string    | yes      | Feature line name or handle |

**Response data:**

```
{
  name: string,
  handle: string,
  layer: string,
  style: string,
  length: number,
  vertexCount: number,
  vertices: [
    { x: number, y: number, z: number }
  ],
  minElevation: number,
  maxElevation: number,
  units: string
}
```

#### `export_as_polyline`

Exports a feature line as a 3D polyline.

| Parameter   | Type                     | Required | Description                     |
| ----------- | ------------------------ | -------- | ------------------------------- |
| action      | `"export_as_polyline"` | yes      | —                              |
| name        | string                   | yes      | Feature line name or handle     |
| targetLayer | string                   | no       | Layer for the exported polyline |

---

## Domain Tools — Parcels

### Tool 18: `civil3d_parcel`

Reads parcel and site data.

**Actions:**

#### `list_sites`

Returns all sites in the drawing.

| Parameter | Type             | Required | Description |
| --------- | ---------------- | -------- | ----------- |
| action    | `"list_sites"` | yes      | —          |

#### `list`

Returns all parcels within a site.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |
| siteName  | string     | yes      | Site name   |

**Response data:**

```
{
  siteName: string,
  parcels: [
    {
      name: string,
      handle: string,
      number: number,
      area: number,
      perimeter: number,
      style: string
    }
  ],
  units: { area: string, length: string }
}
```

#### `get`

Returns detailed properties of a single parcel.

| Parameter  | Type      | Required | Description |
| ---------- | --------- | -------- | ----------- |
| action     | `"get"` | yes      | —          |
| siteName   | string    | yes      | Site name   |
| parcelName | string    | yes      | Parcel name |

---

## Domain Tools — Assemblies

### Tool 19: `civil3d_assembly`

Lists and inspects assemblies and subassemblies used by corridors.

**Actions:**

#### `list`

Returns all assemblies in the drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

**Response data:**

```
{
  assemblies: [
    {
      name: string,
      handle: string,
      subassemblyCount: number,
      usedByCorridors: string[]
    }
  ]
}
```

#### `get`

Returns detailed properties of an assembly including its subassemblies.

| Parameter | Type      | Required | Description   |
| --------- | --------- | -------- | ------------- |
| action    | `"get"` | yes      | —            |
| name      | string    | yes      | Assembly name |

**Response data:**

```
{
  name: string,
  handle: string,
  style: string,
  subassemblies: [
    {
      name: string,
      side: "left" | "right" | "none",
      className: string,
      parameters: { [key: string]: number | string | boolean }
    }
  ],
  usedByCorridors: string[]
}
```

---

## Domain Tools — Pressure Networks

### Tool 20: `civil3d_pressure_network`

Reads and manages pressure pipe networks (water, gas, force mains).

**Actions:**

#### `list`

Returns all pressure networks in the drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

**Response data:**

```
{
  networks: [
    {
      name: string,
      handle: string,
      pipeCount: number,
      fittingCount: number,
      appurtenanceCount: number
    }
  ]
}
```

#### `get`

Returns detailed properties of a pressure network.

| Parameter | Type      | Required | Description   |
| --------- | --------- | -------- | ------------- |
| action    | `"get"` | yes      | —             |
| name      | string    | yes      | Network name  |

#### `create`

Creates a new pressure network.

| Parameter   | Type         | Required | Description                   |
| ----------- | ------------ | -------- | ----------------------------- |
| action      | `"create"` | yes      | —                            |
| name        | string       | yes      | Network name (must be unique) |
| partsList   | string       | yes      | Parts list name               |
| description | string       | no       | Optional description          |

#### `delete`

Deletes a pressure network.

| Parameter | Type         | Required | Description  |
| --------- | ------------ | -------- | ------------ |
| action    | `"delete"` | yes      | —           |
| name      | string       | yes      | Network name |

---

## Domain Tools — Grading

### Tool 21: `civil3d_grading`

Reads and manages grading groups and grading objects.

**Actions:**

#### `list_groups`

Returns all grading groups in the drawing.

| Parameter | Type             | Required | Description |
| --------- | ---------------- | -------- | ----------- |
| action    | `"list_groups"` | yes      | —          |

#### `get_group`

Returns detailed properties of a grading group.

| Parameter | Type          | Required | Description    |
| --------- | ------------- | -------- | -------------- |
| action    | `"get_group"` | yes      | —             |
| name      | string        | yes      | Group name     |

#### `create_group`

Creates a new grading group.

| Parameter | Type              | Required | Description                   |
| --------- | ----------------- | -------- | ----------------------------- |
| action    | `"create_group"` | yes      | —                            |
| name      | string            | yes      | Group name (must be unique)   |
| site      | string            | no       | Site name                     |

#### `delete_group`

Deletes a grading group.

| Parameter | Type              | Required | Description  |
| --------- | ----------------- | -------- | ------------ |
| action    | `"delete_group"` | yes      | —           |
| name      | string            | yes      | Group name   |

---

## Domain Tools — Plan Production

### Tool 22: `civil3d_sheet_set`

Manages sheet sets for plan production.

**Actions:**

#### `list`

Returns all sheet sets in the drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

#### `get`

Returns detailed properties of a sheet set.

| Parameter | Type      | Required | Description  |
| --------- | --------- | -------- | ------------ |
| action    | `"get"` | yes      | —           |
| name      | string    | yes      | Sheet set name |

#### `create`

Creates a new sheet set.

| Parameter | Type              | Required | Description                   |
| --------- | ----------------- | -------- | ----------------------------- |
| action    | `"create"`       | yes      | —                            |
| name      | string            | yes      | Sheet set name (must be unique) |
| template  | string            | no       | Sheet set template path       |

---

## Domain Tools — QC Checks

### Tool 23: `civil3d_qc`

Performs quality control checks on Civil 3D objects.

**Actions:**

#### `check_alignment`

Validates alignment geometry against design standards.

| Parameter | Type                   | Required | Description    |
| --------- | ---------------------- | -------- | -------------- |
| action    | `"check_alignment"` | yes      | —             |
| name      | string                 | yes      | Alignment name |

#### `check_profile`

Validates profile grades and K-values against design standards.

| Parameter     | Type                 | Required | Description           |
| ------------- | -------------------- | -------- | --------------------- |
| action        | `"check_profile"`  | yes      | —                    |
| alignmentName | string               | yes      | Parent alignment name |
| profileName   | string               | yes      | Profile name          |

#### `check_corridor`

Validates corridor build status and target mappings.

| Parameter | Type                 | Required | Description   |
| --------- | -------------------- | -------- | ------------- |
| action    | `"check_corridor"` | yes      | —            |
| name      | string               | yes      | Corridor name |

#### `check_surface`

Validates surface TIN triangles for errors.

| Parameter | Type                 | Required | Description  |
| --------- | -------------------- | -------- | ------------ |
| action    | `"check_surface"`  | yes      | —           |
| name      | string               | yes      | Surface name |

---

## Domain Tools — Quantity Takeoff

### Tool 24: `civil3d_qty`

Performs quantity takeoff calculations for earthwork, materials, and construction.

**Actions:**

#### `surface_volume`

Calculates cut/fill volumes between two surfaces.

| Parameter         | Type                    | Required | Description             |
| ----------------- | ----------------------- | -------- | ----------------------- |
| action            | `"surface_volume"`    | yes      | —                      |
| baseSurface       | string                  | yes      | Base surface name       |
| comparisonSurface | string                  | yes      | Comparison surface name |

**Response data:**

```
{
  cutVolume: number,
  fillVolume: number,
  netVolume: number,
  units: { volume: string, area: string }
}
```

#### `corridor_volumes`

Calculates material volumes for a corridor.

| Parameter | Type                     | Required | Description   |
| --------- | ------------------------ | -------- | ------------- |
| action    | `"corridor_volumes"`   | yes      | —            |
| name      | string                   | yes      | Corridor name |

#### `export_csv`

Exports quantity takeoff data to CSV format.

| Parameter   | Type                  | Required | Description                   |
| ----------- | --------------------- | -------- | ----------------------------- |
| action      | `"export_csv"`       | yes      | —                            |
| filePath    | string                | yes      | Output file path              |
| data        | object                | yes      | Quantity data to export       |

---

## Domain Tools — Hydrology

### Tool 25: `civil3d_hydrology`

Performs hydrology analysis including flow path tracing and watershed delineation.

**Actions:**

#### `list_capabilities`

Returns available hydrology analysis capabilities.

| Parameter | Type                    | Required | Description |
| --------- | ----------------------- | -------- | ----------- |
| action    | `"list_capabilities"` | yes      | —          |

#### `trace_flow_path`

Traces water flow path from a specified point on a surface.

| Parameter   | Type                  | Required | Description                |
| ----------- | --------------------- | -------- | -------------------------- |
| action      | `"trace_flow_path"`  | yes      | —                           |
| surfaceName | string                | yes      | Surface name                |
| x           | number                | yes      | Start X coordinate          |
| y           | number                | yes      | Start Y coordinate          |

#### `delineate_watershed`

Delineates watershed area for a specified point.

| Parameter   | Type                     | Required | Description                |
| ----------- | ------------------------ | -------- | -------------------------- |
| action      | `"delineate_watershed"` | yes      | —                           |
| surfaceName | string                   | yes      | Surface name                |
| x           | number                   | yes      | Watershed outlet X coordinate |
| y           | number                   | yes      | Watershed outlet Y coordinate |

---

## Domain Tools — Pipe Hydraulics

### Tool 26: `civil3d_pipe_hydraulics`

Performs hydraulic analysis on pipe networks.

**Actions:**

#### `calculate_hgl`

Calculates Hydraulic Grade Line (HGL) for a pipe network.

| Parameter   | Type                  | Required | Description   |
| ----------- | --------------------- | -------- | ------------- |
| action      | `"calculate_hgl"`    | yes      | —            |
| networkName | string                | yes      | Network name  |

#### `analyze_hydraulics`

Performs full hydraulic capacity analysis.

| Parameter   | Type                    | Required | Description   |
| ----------- | ----------------------- | -------- | ------------- |
| action      | `"analyze_hydraulics"` | yes      | —            |
| networkName | string                  | yes      | Network name  |

---

## Domain Tools — Assembly Creation

### Tool 27: `civil3d_assembly_create`

Creates and manages assemblies and subassemblies for corridors.

**Actions:**

#### `create`

Creates a new assembly at a specified location.

| Parameter | Type                  | Required | Description                   |
| --------- | --------------------- | -------- | ----------------------------- |
| action    | `"create"`           | yes      | —                            |
| name      | string                | yes      | Assembly name (must be unique) |
| x         | number                | yes      | Insertion X coordinate        |
| y         | number                | yes      | Insertion Y coordinate        |

#### `add_subassembly`

Adds a subassembly from the catalog to an existing assembly.

| Parameter      | Type                  | Required | Description                   |
| -------------- | --------------------- | -------- | ----------------------------- |
| action         | `"add_subassembly"` | yes      | —                            |
| assemblyName   | string                | yes      | Target assembly name          |
| subassemblyName| string                | yes      | Subassembly name from catalog |
| side           | `"left"`or `"right"`| yes      | Side of assembly              |

---

## Domain Tools — Sight Distance

### Tool 28: `civil3d_sight_distance`

Calculates sight distance for design compliance.

**Actions:**

#### `calculate`

Calculates AASHTO sight distance for design speed.

| Parameter  | Type     | Required | Description      |
| ---------- | -------- | -------- | ---------------- |
| action     | `"calculate"` | yes      | —               |
| designSpeed| number   | yes      | Design speed (mph)|
| type       | `"stopping"`or `"passing"`or `"decision"` | yes | Sight distance type |

#### `check_compliance`

Checks stopping sight distance compliance along an alignment.

| Parameter     | Type                  | Required | Description           |
| ------------- | --------------------- | -------- | --------------------- |
| action        | `"check_compliance"` | yes      | —                    |
| alignmentName | string                | yes      | Alignment name        |
| designSpeed   | number                | yes      | Design speed (mph)    |

---

## Domain Tools — Cost Estimation

### Tool 29: `civil3d_cost`

Performs cost estimation for construction projects.

**Actions:**

#### `export_pay_items`

Extracts quantities and exports as pay item schedule.

| Parameter | Type                 | Required | Description                   |
| --------- | -------------------- | -------- | ----------------------------- |
| action    | `"export_pay_items"` | yes      | —                            |
| filePath  | string               | yes      | Output file path (CSV/Excel) |

#### `calculate_estimate`

Generates construction cost estimate.

| Parameter | Type                  | Required | Description                   |
| --------- | --------------------- | -------- | ----------------------------- |
| action    | `"calculate_estimate"`| yes      | —                            |
| quantities| object                | yes      | Quantity data                 |
| unitPrices| object                | yes      | Unit price data               |

---

## Domain Tools — Survey Processing

### Tool 30: `civil3d_survey`

Manages survey databases and figures.

**Actions:**

#### `list_databases`

Returns all survey databases.

| Parameter | Type                 | Required | Description |
| --------- | -------------------- | -------- | ----------- |
| action    | `"list_databases"`  | yes      | —          |

#### `create_database`

Creates a new survey database.

| Parameter | Type                  | Required | Description                   |
| --------- | --------------------- | -------- | ----------------------------- |
| action    | `"create_database"`  | yes      | —                            |
| name      | string                | yes      | Database name (must be unique) |

#### `list_figures`

Returns all survey figures in a database.

| Parameter   | Type               | Required | Description     |
| ----------- | ------------------ | -------- | --------------- |
| action      | `"list_figures"`  | yes      | —              |
| databaseName | string             | yes      | Database name   |

#### `get_figure`

Returns detailed figure data including vertices.

| Parameter   | Type             | Required | Description   |
| ----------- | ---------------- | -------- | ------------- |
| action      | `"get_figure"`   | yes      | —            |
| databaseName| string           | yes      | Database name |
| figureName  | string           | yes      | Figure name  |

---

## Domain Tools — Standards Lookup

### Tool 31: `civil3d_standards_lookup`

Looks up Civil 3D standards, templates, and governance information.

**Actions:**

#### `lookup`

Retrieves standards information for a specific domain.

| Parameter | Type       | Required | Description                                    |
| --------- | ---------- | -------- | ---------------------------------------------- |
| action    | `"lookup"` | yes      | —                                             |
| domain    | string     | yes      | Domain (e.g., "layers", "styles", "labels")   |
| key       | string     | no       | Specific standard key to retrieve              |

**Response data:**

```
{
  domain: string,
  standards: {
    [key: string]: string | number | object
  }
}
```

---

## Domain Tools — Detention & Stormwater

### Tool 32: `civil3d_detention`

Performs detention basin sizing and stage-storage analysis.

**Actions:**

#### `calculate_basin_size`

Calculates required detention basin size to reduce peak runoff.

| Parameter   | Type                     | Required | Description                |
| ----------- | ------------------------ | -------- | -------------------------- |
| action      | `"calculate_basin_size"`| yes      | —                           |
| inflowRate  | number                   | yes      | Peak inflow rate (cfs)      |
| outflowRate | number                   | yes      | Target outflow rate (cfs)   |
| duration    | number                   | yes      | Storm duration (minutes)    |

#### `stage_storage`

Generates stage-storage-discharge table for a detention basin.

| Parameter   | Type              | Required | Description                |
| ----------- | ----------------- | -------- | -------------------------- |
| action      | `"stage_storage"` | yes      | —                           |
| surfaceName | string            | yes      | Basin surface name          |

---

## Domain Tools — Slope Analysis

### Tool 33: `civil3d_slope`

Performs slope geometry and stability analysis.

**Actions:**

#### `calculate_geometry`

Calculates daylight line coordinates and slope geometry.

| Parameter     | Type                  | Required | Description           |
| ------------- | --------------------- | -------- | --------------------- |
| action        | `"calculate_geometry"`| yes      | —                    |
| alignmentName | string                | yes      | Alignment name        |
| station       | number                | yes      | Station value         |
| cutSlope      | number                | yes      | Cut slope ratio      |
| fillSlope     | number                | yes      | Fill slope ratio     |

#### `check_stability`

Evaluates cut and fill slope stability.

| Parameter     | Type                | Required | Description           |
| ------------- | ------------------- | -------- | --------------------- |
| action        | `"check_stability"` | yes      | —                    |
| alignmentName | string              | yes      | Alignment name        |
| designSpeed   | number              | yes      | Design speed (mph)    |

---

## Domain Tools — Parcel Editing

### Tool 34: `civil3d_parcel_edit`

Creates and modifies parcels.

**Actions:**

#### `create`

Creates a parcel from polylines or feature lines.

| Parameter | Type                  | Required | Description                   |
| --------- | --------------------- | -------- | ----------------------------- |
| action    | `"create"`           | yes      | —                            |
| name      | string                | yes      | Parcel name (must be unique)   |
| siteName  | string                | yes      | Site name                     |
| objects   | string[]              | yes      | Object handles to use as boundary |

#### `edit`

Modifies parcel properties.

| Parameter | Type       | Required | Description    |
| --------- | ---------- | -------- | -------------- |
| action    | `"edit"` | yes      | —             |
| siteName  | string     | yes      | Site name     |
| parcelName| string     | yes      | Parcel name   |
| style     | string     | no       | Parcel style  |

#### `lot_line_adjust`

Adjusts lot lines to achieve target area.

| Parameter   | Type                   | Required | Description                |
| ----------- | ---------------------- | -------- | -------------------------- |
| action      | `"lot_line_adjust"`   | yes      | —                           |
| parcelName  | string                 | yes      | Parcel name                 |
| targetArea  | number                 | yes      | Target area (sq ft or sq m) |

---

## Domain Tools — COGO

### Tool 35: `civil3d_cogo`

Performs coordinate geometry calculations.

**Actions:**

#### `inverse`

Calculates bearing and distance between two points.

| Parameter | Type                  | Required | Description                |
| --------- | --------------------- | -------- | -------------------------- |
| action    | `"inverse"`           | yes      | —                           |
| x1        | number                | yes      | First point X coordinate    |
| y1        | number                | yes      | First point Y coordinate    |
| x2        | number                | yes      | Second point X coordinate   |
| y2        | number                | yes      | Second point Y coordinate   |

#### `traverse`

Solves a traverse from a starting point.

| Parameter | Type                            | Required | Description                  |
| --------- | ------------------------------- | -------- | ---------------------------- |
| action    | `"traverse"`                    | yes      | —                             |
| courses   | `{ bearing: string, distance: number }[]` | yes | Array of bearing/distance courses |

#### `curve_solve`

Solves a horizontal curve given any two elements.

| Parameter  | Type     | Required | Description                |
| ---------- | -------- | -------- | -------------------------- |
| action     | `"curve_solve"` | yes      | —                           |
| radius     | number   | no       | Curve radius                |
| delta      | number   | no       | Delta angle (degrees)       |
| arcLength  | number   | no       | Arc length                  |
| chord      | number   | no       | Chord length                |

---

## Domain Tools — Intersection Design

### Tool 36: `civil3d_intersection`

Creates and manages road intersections.

**Actions:**

#### `list`

Returns all intersections in the drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

#### `create`

Creates an intersection between two alignments.

| Parameter      | Type     | Required | Description                   |
| -------------- | -------- | -------- | ----------------------------- |
| action         | `"create"`| yes      | —                            |
| primaryAlignment| string  | yes      | Primary alignment name        |
| secondaryAlignment| string| yes      | Secondary alignment name      |
| intersectionName| string | yes      | Intersection name (must be unique) |

#### `get`

Returns detailed properties of an intersection.

| Parameter   | Type      | Required | Description     |
| ----------- | --------- | -------- | --------------- |
| action      | `"get"`   | yes      | —              |
| name        | string    | yes      | Intersection name |

---

## Domain Tools — Superelevation

### Tool 37: `civil3d_superelevation`

Manages superelevation design for alignments.

**Actions:**

#### `get`

Retrieves superelevation design data.

| Parameter     | Type                 | Required | Description           |
| ------------- | -------------------- | -------- | --------------------- |
| action        | `"get"`              | yes      | —                    |
| alignmentName | string               | yes      | Alignment name        |

#### `apply`

Applies superelevation using AASHTO attainment method.

| Parameter     | Type                 | Required | Description           |
| ------------- | -------------------- | -------- | --------------------- |
| action        | `"apply"`            | yes      | —                    |
| alignmentName | string               | yes      | Alignment name        |
| maxSuperRate  | number               | yes      | Maximum superelevation rate (%) |
| attainmentMethod | string           | yes      | AASHTO attainment method |

#### `check_design`

Validates superelevation rates and attainment lengths.

| Parameter     | Type                  | Required | Description           |
| ------------- | --------------------- | -------- | --------------------- |
| action        | `"check_design"`      | yes      | —                    |
| alignmentName | string                | yes      | Alignment name        |
| designSpeed   | number                | yes      | Design speed (mph)    |

---

## Domain Tools — Profile Editing

### Tool 38: `civil3d_profile_edit`

Modifies layout profiles with PVI and curve operations.

**Actions:**

#### `add_pvi`

Adds a PVI (Point of Vertical Intersection) to a layout profile.

| Parameter     | Type                 | Required | Description           |
| ------------- | -------------------- | -------- | --------------------- |
| action        | `"add_pvi"`          | yes      | —                    |
| alignmentName | string               | yes      | Alignment name        |
| profileName   | string               | yes      | Profile name          |
| station       | number               | yes      | Station value         |
| elevation     | number               | yes      | Elevation value       |

#### `add_curve`

Adds a parabolic vertical curve at a PVI.

| Parameter     | Type                 | Required | Description           |
| ------------- | -------------------- | -------- | --------------------- |
| action        | `"add_curve"`         | yes      | —                    |
| alignmentName | string               | yes      | Alignment name        |
| profileName   | string               | yes      | Profile name          |
| station       | number               | yes      | PVI station           |
| length        | number               | yes      | Curve length          |

#### `set_grade`

Sets the grade of a tangent entity.

| Parameter     | Type                 | Required | Description           |
| ------------- | -------------------- | -------- | --------------------- |
| action        | `"set_grade"`        | yes      | —                    |
| alignmentName | string               | yes      | Alignment name        |
| profileName   | string               | yes      | Profile name          |
| station       | number               | yes      | Station on tangent    |
| grade         | number               | yes      | New grade (%)         |

---

## Domain Tools — Time of Concentration

### Tool 39: `civil3d_time_of_concentration`

Calculates time of concentration and generates hydrographs.

**Actions:**

#### `calculate`

Calculates Tc using standard methods.

| Parameter   | Type                     | Required | Description                |
| ----------- | ------------------------ | -------- | -------------------------- |
| action      | `"calculate"`            | yes      | —                           |
| method      | string                   | yes      | Tc calculation method       |
| flowLength  | number                   | yes      | Flow path length (ft)       |
| slope       | number                   | yes      | Average slope (%)           |

#### `generate_hydrograph`

Generates SCS hydrograph for a watershed.

| Parameter   | Type                  | Required | Description                |
| ----------- | --------------------- | -------- | -------------------------- |
| action      | `"generate_hydrograph"`| yes      | —                           |
| tc          | number                | yes      | Time of concentration (min) |
| area        | number                | yes      | Watershed area (acres)     |
| rainfall    | number                | yes      | Rainfall depth (inches)    |

---

## Domain Tools — Storm & Sanitary Analysis

### Tool 40: `civil3d_stm`

Manages Storm and Sanitary Analysis (SSA) workflows.

**Actions:**

#### `export`

Exports Civil 3D data to SSA STM file format.

| Parameter   | Type        | Required | Description                |
| ----------- | ----------- | -------- | -------------------------- |
| action      | `"export"`  | yes      | —                           |
| filePath    | string      | yes      | Output STM file path        |

#### `import`

Imports SSA STM file data into Civil 3D.

| Parameter   | Type        | Required | Description                |
| ----------- | ----------- | -------- | -------------------------- |
| action      | `"import"`  | yes      | —                           |
| filePath    | string      | yes      | Input STM file path         |

---

## Domain Tools — Catchment

### Tool 41: `civil3d_catchment`

Manages catchments and catchment groups.

**Actions:**

#### `list_groups`

Returns all catchment groups in the drawing.

| Parameter | Type                | Required | Description |
| --------- | ------------------- | -------- | ----------- |
| action    | `"list_groups"`    | yes      | —          |

#### `list`

Returns all catchments in a group.

| Parameter   | Type      | Required | Description     |
| ----------- | --------- | -------- | --------------- |
| action      | `"list"`  | yes      | —              |
| groupName   | string    | no       | Group name     |

#### `get_properties`

Returns detailed catchment properties.

| Parameter   | Type               | Required | Description   |
| ----------- | ------------------ | -------- | ------------- |
| action      | `"get_properties"` | yes      | —            |
| catchmentName| string             | yes      | Catchment name|

---

## Domain Tools — Section Views

### Tool 42: `civil3d_section_view`

Creates and manages section views.

**Actions:**

#### `create`

Creates section views for a sample line group.

| Parameter            | Type                  | Required | Description                   |
| -------------------- | --------------------- | -------- | ----------------------------- |
| action               | `"create"`            | yes      | —                            |
| sampleLineGroupName  | string                | yes      | Sample line group name        |
| insertionPoint       | `{ x: number, y: number }` | yes | Insertion point coordinates |
| styleName            | string                | no       | Section view style            |

#### `list`

Returns all section views in the drawing.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"list"` | yes      | —          |

---

## Domain Tools — Alignment Editing

### Tool 43: `civil3d_alignment_edit`

Modifies alignment geometry with tangent, curve, and spiral operations.

**Actions:**

#### `add_tangent`

Appends a fixed tangent entity to an alignment.

| Parameter | Type                  | Required | Description                |
| --------- | --------------------- | -------- | -------------------------- |
| action    | `"add_tangent"`       | yes      | —                           |
| name      | string                | yes      | Alignment name              |
| x1        | number                | yes      | Start X coordinate          |
| y1        | number                | yes      | Start Y coordinate          |
| x2        | number                | yes      | End X coordinate            |
| y2        | number                | yes      | End Y coordinate            |

#### `add_curve`

Appends a fixed horizontal curve to an alignment.

| Parameter | Type                  | Required | Description                |
| --------- | --------------------- | -------- | -------------------------- |
| action    | `"add_curve"`         | yes      | —                           |
| name      | string                | yes      | Alignment name              |
| radius    | number                | yes      | Curve radius                 |
| delta     | number                | yes      | Delta angle (degrees)       |

#### `add_spiral`

Appends a spiral (transition curve) to an alignment.

| Parameter | Type                  | Required | Description                |
| --------- | --------------------- | -------- | -------------------------- |
| action    | `"add_spiral"`        | yes      | —                           |
| name      | string                | yes      | Alignment name              |
| length    | number                | yes      | Spiral length               |

---

## Domain Tools — Corridor Editing

### Tool 44: `civil3d_corridor_edit`

Modifies corridor regions and target mappings.

**Actions:**

#### `add_region`

Adds a new region to a corridor baseline.

| Parameter    | Type                 | Required | Description                |
| ------------ | -------------------- | -------- | -------------------------- |
| action       | `"add_region"`       | yes      | —                           |
| corridorName | string               | yes      | Corridor name               |
| baselineName | string               | yes      | Baseline name               |
| startStation | number               | yes      | Region start station        |
| endStation   | number               | yes      | Region end station          |
| assemblyName | string               | yes      | Assembly name for region    |

#### `set_target`

Sets a target mapping for a subassembly.

| Parameter      | Type                 | Required | Description                |
| -------------- | -------------------- | -------- | -------------------------- |
| action         | `"set_target"`       | yes      | —                           |
| corridorName   | string               | yes      | Corridor name               |
| subassemblyName| string              | yes      | Subassembly name            |
| targetType     | string               | yes      | Target type (surface, alignment, profile) |
| targetName     | string               | yes      | Target object name          |

---

## Domain Tools — Drawing Primitives

### Tool 45: `civil3d_primitive`

Creates basic AutoCAD primitives.

**Actions:**

#### `create_polyline`

Creates a 2D polyline in model space.

| Parameter | Type                            | Required | Description                |
| --------- | ------------------------------- | -------- | -------------------------- |
| action    | `"create_polyline"`             | yes      | —                           |
| points    | `{ x: number, y: number }[]`    | yes      | Array of 2D points         |
| layer     | string                          | no       | Layer name                  |

#### `create_3dpolyline`

Creates a 3D polyline in model space.

| Parameter | Type                                | Required | Description                |
| --------- | ----------------------------------- | -------- | -------------------------- |
| action    | `"create_3dpolyline"`               | yes      | —                           |
| points    | `{ x: number, y: number, z: number }[]` | yes | Array of 3D points         |
| layer     | string                              | no       | Layer name                  |

#### `create_line`

Creates a line segment between two points.

| Parameter | Type                  | Required | Description                |
| --------- | --------------------- | -------- | -------------------------- |
| action    | `"create_line"`      | yes      | —                           |
| x1        | number                | yes      | Start X coordinate          |
| y1        | number                | yes      | Start Y coordinate          |
| x2        | number                | yes      | End X coordinate            |
| y2        | number                | yes      | End Y coordinate            |

#### `create_text`

Creates DBText (single-line text) in model space.

| Parameter | Type                  | Required | Description                |
| --------- | --------------------- | -------- | -------------------------- |
| action    | `"create_text"`       | yes      | —                           |
| text      | string                | yes      | Text string                 |
| x         | number                | yes      | Insertion X coordinate      |
| y         | number                | yes      | Insertion Y coordinate      |
| height    | number                | no       | Text height                 |

#### `create_mtext`

Creates MText (multi-line text) in model space.

| Parameter | Type                  | Required | Description                |
| --------- | --------------------- | -------- | -------------------------- |
| action    | `"create_mtext"`      | yes      | —                           |
| text      | string                | yes      | Text string                 |
| x         | number                | yes      | Insertion X coordinate      |
| y         | number                | yes      | Insertion Y coordinate      |
| width     | number                | no       | Text width                  |

---

## Domain Tools — Pressure Network Components

### Tool 46: `civil3d_pressure_network_components`

Manages pressure network pipes, fittings, and appurtenances.

**Actions:**

#### `add_pipe`

Adds a pressure pipe segment to a network.

| Parameter   | Type                  | Required | Description                |
| ----------- | --------------------- | -------- | -------------------------- |
| action      | `"add_pipe"`          | yes      | —                           |
| networkName | string                | yes      | Network name                |
| startPoint  | `{ x: number, y: number, z: number }` | yes | Start coordinates |
| endPoint    | `{ x: number, y: number, z: number }` | yes | End coordinates |
| partName    | string                | yes      | Pipe part name from catalog |

#### `add_fitting`

Adds a pressure fitting (elbow, tee, reducer, cap).

| Parameter   | Type                  | Required | Description                |
| ----------- | --------------------- | -------- | -------------------------- |
| action      | `"add_fitting"`       | yes      | —                           |
| networkName | string                | yes      | Network name                |
| x           | number                | yes      | X coordinate                |
| y           | number                | yes      | Y coordinate                |
| z           | number                | yes      | Z coordinate                |
| partName    | string                | yes      | Fitting part name           |

#### `add_appurtenance`

Adds a pressure appurtenance (valve, hydrant, meter).

| Parameter   | Type                  | Required | Description                |
| ----------- | --------------------- | -------- | -------------------------- |
| action      | `"add_appurtenance"`  | yes      | —                           |
| networkName | string                | yes      | Network name                |
| x           | number                | yes      | X coordinate                |
| y           | number                | yes      | Y coordinate                |
| partName    | string                | yes      | Appurtenance part name      |

#### `resize_pipe`

Changes pressure pipe size to a different catalog entry.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"resize_pipe"`      | yes      | —                           |
| networkName | string               | yes      | Network name                |
| pipeName    | string               | yes      | Pipe name                   |
| newSize     | string               | yes      | New size from catalog       |

---

## Domain Tools — Plan Production Details

### Tool 47: `civil3d_sheet`

Manages individual sheets within sheet sets.

**Actions:**

#### `add`

Adds a new sheet to an existing sheet set.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"add"`              | yes      | —                           |
| sheetSetName| string               | yes      | Sheet set name              |
| layoutName  | string               | yes      | Layout name                 |
| sheetNumber | number               | yes      | Sheet number                |

#### `get_properties`

Returns full properties of a specific sheet.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"get_properties"`   | yes      | —                           |
| sheetSetName| string               | yes      | Sheet set name              |
| sheetName   | string               | yes      | Sheet name                  |

#### `set_title_block`

Sets or updates title block template on a sheet.

| Parameter    | Type                 | Required | Description                |
| ------------ | -------------------- | -------- | -------------------------- |
| action       | `"set_title_block"`  | yes      | —                           |
| sheetSetName | string               | yes      | Sheet set name              |
| sheetName    | string               | yes      | Sheet name                  |
| titleBlock   | string               | yes      | Title block name            |

#### `publish_pdf`

Publishes sheet layouts to PDF.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"publish_pdf"`      | yes      | —                           |
| sheetSetName| string               | yes      | Sheet set name              |
| sheetNames  | string[]             | no       | Specific sheets to publish  |
| filePath    | string               | yes      | Output PDF file path        |

---

## Domain Tools — Additional QC

### Tool 48: `civil3d_qc_extended`

Extended quality control checks.

**Actions:**

#### `check_labels`

Checks for missing labels and style standard violations.

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"check_labels"`     | yes      | —                           |
| objectType| string               | no       | Object type to check        |

#### `check_drawing_standards`

Audits drawing for layer naming, lineweights, colors.

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"check_drawing_standards"` | yes | —                     |

#### `fix_drawing_standards`

Automatically fixes common drawing standard violations.

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"fix_drawing_standards"` | yes | —                      |

#### `generate_report`

Generates comprehensive QC report.

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"generate_report"`   | yes      | —                           |
| filePath  | string               | yes      | Output report file path      |

---

## Domain Tools — Extended Quantity Takeoff

### Tool 49: `civil3d_qty_extended`

Extended quantity takeoff calculations.

**Actions:**

#### `corridor_volumes`

Calculates subassembly material volumes by region.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"corridor_volumes"` | yes      | —                           |
| corridorName| string              | yes      | Corridor name               |
| regionName  | string               | no       | Specific region name        |

#### `alignment_lengths`

Calculates total length for one or more alignments.

| Parameter      | Type      | Required | Description                |
| -------------- | --------- | -------- | -------------------------- |
| action         | `"alignment_lengths"` | yes | —                           |
| alignmentNames | string[]  | yes      | Array of alignment names     |

#### `point_count_by_group`

Counts COGO points per point group.

| Parameter | Type       | Required | Description |
| --------- | ---------- | -------- | ----------- |
| action    | `"point_count_by_group"` | yes | —          |

#### `material_list_get`

Retrieves material list defined on a corridor.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"material_list_get"` | yes      | —                           |
| corridorName| string               | yes      | Corridor name               |

#### `earthwork_summary`

Generates running earthwork cut/fill summary.

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"earthwork_summary"` | yes      | —                           |

---

## Domain Tools — Pipe Design Automation

### Tool 50: `civil3d_pipe_design`

Automates gravity pipe network design.

**Actions:**

#### `size_network`

Sizes gravity-network pipes from Manning full-flow capacity.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"size_network"`     | yes      | —                           |
| networkName | string               | yes      | Network name                |
| designFlow  | number               | yes      | Design flow rate (cfs)      |
| slope       | number               | yes      | Pipe slope (%)              |

#### `automate_profile_view`

Automates gravity-pipe profile-view setup.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"automate_profile_view"` | yes | —                       |
| networkName | string              | yes      | Network name                |
| alignmentName | string             | yes      | Alignment name              |

---

## Domain Tools — Extended Grading

### Tool 51: `civil3d_grading_extended`

Extended grading operations.

**Actions:**

#### `list_gradings`

Lists all grading objects within a grading group.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"list_gradings"`    | yes      | —                           |
| groupName   | string               | yes      | Grading group name          |

#### `get_grading`

Returns detailed properties of a grading object.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"get_grading"`      | yes      | —                           |
| groupName   | string               | yes      | Grading group name          |
| gradingName | string              | yes      | Grading object name         |

#### `create_grading`

Creates a new grading from a feature line.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"create_grading"`   | yes      | —                           |
| groupName   | string               | yes      | Grading group name          |
| featureLineName| string           | yes      | Feature line name           |
| criteriaName| string              | yes      | Grading criteria name       |

#### `delete_grading`

Deletes a grading object.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"delete_grading"`   | yes      | —                           |
| groupName   | string               | yes      | Grading group name          |
| gradingName | string               | yes      | Grading object name         |

#### `list_criteria`

Lists all available grading criteria sets.

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"list_criteria"`    | yes      | —                           |

#### `create_feature_line`

Creates a new feature line from 3D points.

| Parameter | Type                                | Required | Description                |
| --------- | ----------------------------------- | -------- | -------------------------- |
| action    | `"create_feature_line"`            | yes      | —                           |
| points    | `{ x: number, y: number, z: number }[]` | yes | Array of 3D points |

---

## Domain Tools — Point Groups

### Tool 52: `civil3d_point_group`

Manages point groups and point group operations.

**Actions:**

#### `create`

Creates a new point group with filter criteria.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"create"`           | yes      | —                           |
| name        | string               | yes      | Point group name (must be unique) |
| includePattern | string          | no       | Include pattern (e.g., "100-*") |
| excludePattern | string          | no       | Exclude pattern              |

#### `update`

Updates filter criteria and description of a point group.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"update"`           | yes      | —                           |
| name        | string               | yes      | Point group name            |
| includePattern | string          | no       | New include pattern          |
| excludePattern | string          | no       | New exclude pattern          |

#### `delete`

Deletes a point group (points are NOT deleted).

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"delete"`            | yes      | —                           |
| name      | string               | yes      | Point group name            |

#### `export_points`

Exports COGO points from a point group.

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"export_points"`     | yes      | —                           |
| groupName | string               | yes      | Point group name            |
| filePath  | string               | yes      | Output file path            |
| format    | string               | yes      | Format (pnezd, penz, xyzd, xyz, csv) |

#### `transform_points`

Transforms points by translation, rotation, and/or scale.

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"transform_points"`  | yes      | —                           |
| pointNumbers | number[]          | yes      | Array of point numbers       |
| translationX | number           | no       | X translation                |
| translationY | number           | no       | Y translation                |
| rotation  | number               | no       | Rotation angle (degrees)     |
| scaleX    | number               | no       | X scale factor               |
| scaleY    | number               | no       | Y scale factor               |

---

## Domain Tools — Extended Data Shortcuts

### Tool 53: `civil3d_data_shortcut_extended`

Extended data shortcut operations.

**Actions:**

#### `create`

Creates a data shortcut for a Civil 3D object.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"create"`           | yes      | —                           |
| objectName  | string               | yes      | Object name                 |
| objectType | string               | yes      | Object type (surface, alignment, profile, pipe_network) |

#### `promote`

Promotes a data shortcut reference to a full editable object.

| Parameter   | Type                 | Required | Description                |
| ----------- | -------------------- | -------- | -------------------------- |
| action      | `"promote"`          | yes      | —                           |
| objectName  | string               | yes      | Shortcut object name        |

---

## Domain Tools — Extended Parcel

### Tool 54: `civil3d_parcel_extended`

Extended parcel operations.

**Actions:**

#### `report`

Generates parcel report with coordinate and unit settings.

| Parameter | Type                 | Required | Description                |
| --------- | -------------------- | -------- | -------------------------- |
| action    | `"report"`           | yes      | —                           |
| siteName  | string               | yes      | Site name                   |
| parcelName| string               | no       | Specific parcel name (optional) |
| filePath  | string               | yes      | Output report file path     |

---

## Summary — Tool Count

> **Note:** This specification document covers the foundational tool architecture. The actual implementation includes 180+ tools across all Civil 3D domains. See README.md for the complete tool catalog.

| #  | Tool Name                     | Actions                                                                                 | Category              |
| -- | ----------------------------- | --------------------------------------------------------------------------------------- | --------------------- |
| 1  | `civil3d_drawing`           | info, save, undo, redo, settings                                                        | System               |
| 2  | `civil3d_health`            | (single action)                                                                         | System               |
| 3  | `civil3d_job`               | status, cancel                                                                          | System               |
| 4  | `civil3d_coordinate_system` | info, transform                                                                         | System               |
| 5  | `civil3d_surface`           | list, get, get_elevation, get_elevation_along, get_statistics, create, delete           | Surface              |
| 6  | `civil3d_surface_edit`      | add_points, add_breakline, add_boundary, extract_contours, compute_volume               | Surface              |
| 7  | `civil3d_alignment`         | list, get, station_to_point, point_to_station, create, delete                           | Alignment            |
| 8  | `civil3d_profile`           | list, get, get_elevation, sample_elevations, create_from_surface, create_layout, delete | Profile              |
| 9  | `civil3d_corridor`          | list, get, rebuild, get_surfaces, get_feature_lines, compute_volumes                    | Corridor             |
| 10 | `civil3d_pipe_network`      | list, get, get_pipe, get_structure, check_interference                                  | Pipe Network         |
| 11 | `civil3d_pipe_network_edit` | create, add_pipe, add_structure                                                         | Pipe Network         |
| 12 | `civil3d_point`             | list, get, create, list_groups, import, delete                                          | Points               |
| 13 | `civil3d_section`           | list_sample_lines, get_section_data, create_sample_lines                                | Sections             |
| 14 | `civil3d_data_shortcut`     | list, sync, create_reference                                                            | Data Shortcuts       |
| 15 | `civil3d_style`             | list, get                                                                               | Styles               |
| 16 | `civil3d_label`             | list, add, list_styles                                                                  | Labels               |
| 17 | `civil3d_feature_line`      | list, get, export_as_polyline                                                           | Feature Lines        |
| 18 | `civil3d_parcel`            | list_sites, list, get                                                                   | Parcels              |
| 19 | `civil3d_assembly`          | list, get                                                                               | Assemblies           |
| 20 | `civil3d_pressure_network`  | list, get, create, delete                                                               | Pressure Networks    |
| 21 | `civil3d_grading`           | list_groups, get_group, create_group, delete_group                                      | Grading              |
| 22 | `civil3d_sheet_set`         | list, get, create                                                                       | Plan Production      |
| 23 | `civil3d_qc`                | check_alignment, check_profile, check_corridor, check_surface                             | QC Checks            |
| 24 | `civil3d_qty`               | surface_volume, corridor_volumes, export_csv                                             | Quantity Takeoff     |
| 25 | `civil3d_hydrology`         | list_capabilities, trace_flow_path, delineate_watershed                                  | Hydrology            |
| 26 | `civil3d_pipe_hydraulics`   | calculate_hgl, analyze_hydraulics                                                       | Pipe Hydraulics      |
| 27 | `civil3d_assembly_create`   | create, add_subassembly                                                                 | Assembly Creation    |
| 28 | `civil3d_sight_distance`    | calculate, check_compliance                                                             | Sight Distance       |
| 29 | `civil3d_cost`              | export_pay_items, calculate_estimate                                                    | Cost Estimation      |
| 30 | `civil3d_survey`            | list_databases, create_database, list_figures, get_figure                                | Survey Processing    |
| 31 | `civil3d_standards_lookup`  | lookup                                                                                  | Standards Lookup     |
| 32 | `civil3d_detention`         | calculate_basin_size, stage_storage                                                     | Detention & Stormwater|
| 33 | `civil3d_slope`             | calculate_geometry, check_stability                                                     | Slope Analysis       |
| 34 | `civil3d_parcel_edit`       | create, edit, lot_line_adjust                                                           | Parcel Editing       |
| 35 | `civil3d_cogo`              | inverse, traverse, curve_solve                                                           | COGO                 |
| 36 | `civil3d_intersection`      | list, create, get                                                                       | Intersection Design  |
| 37 | `civil3d_superelevation`    | get, apply, check_design                                                                 | Superelevation       |
| 38 | `civil3d_profile_edit`      | add_pvi, add_curve, set_grade                                                            | Profile Editing      |
| 39 | `civil3d_time_of_concentration`| calculate, generate_hydrograph                                                      | Time of Concentration|
| 40 | `civil3d_stm`              | export, import                                                                          | SSA                  |
| 41 | `civil3d_catchment`        | list_groups, list, get_properties                                                      | Catchment            |
| 42 | `civil3d_section_view`     | create, list                                                                            | Section Views        |
| 43 | `civil3d_alignment_edit`   | add_tangent, add_curve, add_spiral                                                       | Alignment Editing    |
| 44 | `civil3d_corridor_edit`    | add_region, set_target                                                                  | Corridor Editing     |
| 45 | `civil3d_primitive`        | create_polyline, create_3dpolyline, create_line, create_text, create_mtext                 | Drawing Primitives   |
| 46 | `civil3d_pressure_network_components`| add_pipe, add_fitting, add_appurtenance, resize_pipe                              | Pressure Network Components|
| 47 | `civil3d_sheet`            | add, get_properties, set_title_block, publish_pdf                                       | Plan Production Details|
| 48 | `civil3d_qc_extended`      | check_labels, check_drawing_standards, fix_drawing_standards, generate_report              | Additional QC        |
| 49 | `civil3d_qty_extended`     | corridor_volumes, alignment_lengths, point_count_by_group, material_list_get, earthwork_summary| Extended Quantity Takeoff|
| 50 | `civil3d_pipe_design`      | size_network, automate_profile_view                                                      | Pipe Design Automation|
| 51 | `civil3d_grading_extended`  | list_gradings, get_grading, create_grading, delete_grading, list_criteria, create_feature_line| Extended Grading|
| 52 | `civil3d_point_group`      | create, update, delete, export_points, transform_points                                     | Point Groups         |
| 53 | `civil3d_data_shortcut_extended`| create, promote                                                                     | Extended Data Shortcuts|
| 54 | `civil3d_parcel_extended`  | report                                                                                  | Extended Parcel      |

**Specification Total: 54 tools exposing 175+ actions**
**Implementation Total: 180+ tools across all Civil 3D domains**

---

## Implementation Phases

### Phase 1 — Foundation & Read-Only (Tools 1–5, parts of 7–8)

Build the IPC bridge, plugin loading, and all listing/reading tools. No write operations. This phase validates the entire architecture end-to-end.

**Tools:** `civil3d_health`, `civil3d_drawing` (info, settings), `civil3d_coordinate_system` (info), `civil3d_surface` (list, get, get_elevation, get_elevation_along, get_statistics), `civil3d_alignment` (list, get, station_to_point, point_to_station), `civil3d_profile` (list, get, get_elevation, sample_elevations)

**Milestone:** The LLM can query any existing Civil 3D drawing and report on all surfaces, alignments, and profiles including elevations and station data.

### Phase 2 — Extended Read-Only (Tools 9–10, 12–15, 17–19)

Add read tools for all remaining object types.

**Tools:** `civil3d_corridor` (list, get, get_surfaces, get_feature_lines), `civil3d_pipe_network` (list, get, get_pipe, get_structure), `civil3d_point` (list, get, list_groups), `civil3d_section` (list_sample_lines, get_section_data), `civil3d_data_shortcut` (list), `civil3d_style` (list, get), `civil3d_feature_line` (list, get), `civil3d_parcel` (list_sites, list, get), `civil3d_assembly` (list, get)

**Milestone:** The LLM has complete read access to every Civil 3D object type in the drawing.

### Phase 3 — Write Operations (Tools 5–8 write actions, Tool 12 create/import)

Add creation and modification capabilities for core objects.

**Tools:** `civil3d_drawing` (save, undo, redo), `civil3d_surface` (create, delete), `civil3d_surface_edit` (all actions), `civil3d_alignment` (create, delete), `civil3d_profile` (create_from_surface, create_layout, delete), `civil3d_point` (create, import, delete), `civil3d_section` (create_sample_lines)

**Milestone:** The LLM can create surfaces, alignments, profiles, and points, and perform surface editing operations.

### Phase 4 — Advanced Operations (Tools 3, 9 rebuild, 10–11, 14, 16)

Add long-running operations, pipe network editing, and cross-drawing references.

**Tools:** `civil3d_job` (status, cancel), `civil3d_corridor` (rebuild, compute_volumes), `civil3d_pipe_network` (check_interference), `civil3d_pipe_network_edit` (all actions), `civil3d_data_shortcut` (sync, create_reference), `civil3d_label` (all actions), `civil3d_coordinate_system` (transform), `civil3d_feature_line` (export_as_polyline)

**Milestone:** Full read/write capability across all Civil 3D domains including long-running async operations.
