# Civil3D Hydrology Tools Guide

## Overview

The Civil3D MCP hydrology tools provide comprehensive surface-based hydrologic analysis capabilities for drainage design and watershed analysis.

## Available Operations

### 1. List Capabilities
Get a list of all available hydrology operations.

```json
{
  "action": "list_capabilities"
}
```

### 2. Trace Flow Path
Traces a downhill flow path from a starting point by following the steepest descent.

**Parameters:**
- `surfaceName` (string, required): Name of the Civil3D surface
- `x` (number, required): Starting X coordinate
- `y` (number, required): Starting Y coordinate
- `stepDistance` (number, optional): Distance between flow path steps (default: 5.0)
- `maxSteps` (number, optional): Maximum number of steps to trace (default: 100)

**Example:**
```json
{
  "action": "trace_flow_path",
  "surfaceName": "EG",
  "x": 1000.0,
  "y": 2000.0,
  "stepDistance": 10.0,
  "maxSteps": 200
}
```

**Returns:**
- Flow path points with elevations
- Total distance and elevation drop
- Status (complete, stopped_flat, stopped_local_minimum, max_steps_reached)

### 3. Find Low Point
Finds the lowest elevation point on a surface by grid sampling.

**Parameters:**
- `surfaceName` (string, required): Name of the Civil3D surface
- `sampleSpacing` (number, optional): Grid spacing for sampling (default: 25.0)

**Example:**
```json
{
  "action": "find_low_point",
  "surfaceName": "EG",
  "sampleSpacing": 20.0
}
```

**Returns:**
- Low point coordinates and elevation
- Number of sampled points

### 4. Delineate Watershed
Delineates a watershed boundary by tracing flow paths from grid points to determine which areas contribute to an outlet.

**Parameters:**
- `surfaceName` (string, required): Name of the Civil3D surface
- `outletX` (number, required): Outlet point X coordinate
- `outletY` (number, required): Outlet point Y coordinate
- `gridSpacing` (number, optional): Grid spacing for testing points (default: 10.0)
- `searchRadius` (number, optional): Radius around outlet to search (default: 100.0)

**Example:**
```json
{
  "action": "delineate_watershed",
  "surfaceName": "EG",
  "outletX": 1500.0,
  "outletY": 2500.0,
  "gridSpacing": 15.0,
  "searchRadius": 150.0
}
```

**Returns:**
- Outlet point information
- Contributing point count
- Boundary points (convex hull)
- Approximate watershed area
- Units information

**Use Cases:**
- Determine drainage basin boundaries
- Identify contributing areas for stormwater design
- Visualize watershed extents

### 5. Calculate Catchment Area
Calculates the catchment area for an outlet point by analyzing which surface cells drain to that location.

**Parameters:**
- `surfaceName` (string, required): Name of the Civil3D surface
- `outletX` (number, required): Outlet point X coordinate
- `outletY` (number, required): Outlet point Y coordinate
- `sampleSpacing` (number, optional): Grid cell size for analysis (default: 15.0)
- `maxDistance` (number, optional): Maximum search distance from outlet (default: 200.0)

**Example:**
```json
{
  "action": "calculate_catchment_area",
  "surfaceName": "EG",
  "outletX": 1500.0,
  "outletY": 2500.0,
  "sampleSpacing": 10.0,
  "maxDistance": 250.0
}
```

**Returns:**
- Catchment area (in surface units²)
- Contributing cell count
- Elevation statistics (min, max, average, relief)
- Units information

**Use Cases:**
- Calculate drainage area for inlet design
- Determine contributing area for runoff calculations
- Analyze topographic characteristics of catchments

### 6. Estimate Runoff
Calculates peak runoff using the Rational Method (Q = CiA).

**Parameters:**
- `drainageArea` (number, required): Drainage area
- `runoffCoefficient` (number, required): Runoff coefficient (0.0 to 1.0)
- `rainfallIntensity` (number, required): Rainfall intensity
- `areaUnits` (string, required): "acres" or "hectares"
- `intensityUnits` (string, required): "in_per_hr" or "mm_per_hr"

**Example:**
```json
{
  "action": "estimate_runoff",
  "drainageArea": 5.2,
  "runoffCoefficient": 0.65,
  "rainfallIntensity": 3.5,
  "areaUnits": "acres",
  "intensityUnits": "in_per_hr"
}
```

**Returns:**
- Runoff rate in CFS and m³/s
- Normalized inputs in both unit systems

## Workflow Examples

### Complete Watershed Analysis Workflow

1. **Find the outlet point** (lowest point in area):
```json
{
  "action": "find_low_point",
  "surfaceName": "EG",
  "sampleSpacing": 25.0
}
```

2. **Delineate the watershed** using the outlet:
```json
{
  "action": "delineate_watershed",
  "surfaceName": "EG",
  "outletX": 1234.56,
  "outletY": 7890.12,
  "gridSpacing": 10.0,
  "searchRadius": 200.0
}
```

3. **Calculate precise catchment area**:
```json
{
  "action": "calculate_catchment_area",
  "surfaceName": "EG",
  "outletX": 1234.56,
  "outletY": 7890.12,
  "sampleSpacing": 5.0,
  "maxDistance": 200.0
}
```

4. **Estimate runoff** for the catchment:
```json
{
  "action": "estimate_runoff",
  "drainageArea": 12.5,
  "runoffCoefficient": 0.70,
  "rainfallIntensity": 4.2,
  "areaUnits": "acres",
  "intensityUnits": "in_per_hr"
}
```

### Flow Path Analysis

Trace multiple flow paths from different starting points to understand drainage patterns:

```json
{
  "action": "trace_flow_path",
  "surfaceName": "EG",
  "x": 1000.0,
  "y": 2000.0,
  "stepDistance": 5.0,
  "maxSteps": 300
}
```

## Performance Considerations

- **Grid Spacing**: Smaller values = more accurate but slower
  - Watershed delineation: 10-20 units recommended
  - Catchment area: 5-15 units recommended
  - Low point finding: 20-50 units recommended

- **Search Radius/Max Distance**: Larger values = more comprehensive but slower
  - Start with conservative estimates
  - Increase if watershed appears truncated

- **Step Distance**: For flow tracing
  - Smaller = more accurate path
  - Typical range: 1-10 units

## Algorithm Details

### Flow Path Tracing
- Uses 8-directional steepest descent algorithm
- Samples surface at 0°, 45°, 90°, 135°, 180°, 225°, 270°, 315°
- Stops at local minima, flat areas, or max steps

### Watershed Delineation
- Grid-based approach testing points within search radius
- Each point traced to determine if it reaches outlet
- Convex hull computed for boundary visualization
- Area calculated using shoelace formula

### Catchment Area Calculation
- Raster-based cell analysis
- Each cell tested for drainage to outlet
- Area = cell count × cell size²
- Elevation statistics computed from contributing cells

## Units

All coordinates and distances use the drawing's coordinate system units. Area calculations are in square units of the drawing system (e.g., feet² if drawing is in feet).

## Error Handling

Common errors:
- **Surface not found**: Check surface name spelling
- **Point outside surface**: Verify coordinates are within surface bounds
- **No flow path**: May indicate flat area or local depression
- **Invalid parameters**: Check that all required parameters are provided

## Integration with Civil3D

These tools work directly with Civil3D surfaces and respect:
- Surface boundaries
- Breaklines
- TIN triangulation
- Drawing coordinate systems
