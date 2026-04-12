---
title: "Tutorial: Creating and Adding Data to a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-899731B5-0B6A-451E-9CF2-0DCF00FA9B64.htm"
category: "surfaces_tutorials"
last_updated: "2026-03-17T18:42:09.879Z"
---

                 Tutorial: Creating and Adding Data to a Surface  

# Tutorial: Creating and Adding Data to a Surface

This tutorial demonstrates how to create a TIN surface, and then add contour, breakline, and boundary data to the surface.

When you create a surface, its name is displayed in the Surfaces collection in Toolspace on the Prospector tab. From this location, you can perform other operations, such as adding data and editing the surface. When first created, the surface is empty, so it is not visible in the drawing.

After data has been added to a surface, it becomes visible in the drawing in accordance with the display settings specified in the referenced surface style.

## TIN Surfaces

A TIN surface is composed of the triangles that form a triangulated irregular network. A TIN line is one of the lines that makes up the surface triangulation.

To create TIN lines, Autodesk Civil 3D connects the surface points that are closest together. The TIN lines form triangles. The level of any point in the surface is defined by interpolating the levels of the vertices of the triangles that the point lies in.

### TIN surface with contour lines displayed

![](../images/GUID-21556A94-0325-4854-B6B6-BFAB8001E439.png)

## Contour Data

Contours are graphical illustrations of surface level changes. You can create a surface from contours drawn as 2D or 3D polylines that have x, y, and z coordinate data.

## Boundaries

Boundaries are closed polylines that affect the visibility of the triangles either inside or outside the polylines. An outer boundary defines the extents of the surface. All triangles inside the boundary are visible, and all triangles that are outside the boundary are invisible.

Areas hidden by boundaries are not included in calculations, such as total area and volume.

Surface boundaries are defined by selecting existing polygons in the drawing. The surface definition displays the numerical ID and a list of vertices for each boundary.

### A surface before adding a non-destructive outer boundary

![](../images/GUID-51ECCEED-43AA-4B82-AA92-5FF4F455ADAC.png)

### The effects of a non-destructive outer boundary

![](../images/GUID-20206F8F-496D-454B-9781-09D8F86618F7.png)

## Breaklines

Breaklines define linear surface features, such as retaining walls, kerbs, tops of ridges, and streams. Breaklines force surface triangulation to run along the breakline; triangles do not cross a breakline.

Breaklines are critical to creating an accurate surface model. Breaklines are important because it is the interpolation of the data, not just the data itself, that determines the shape of the model.

You can use 3D lines or 3D polylines as breaklines. Each vertex on the polyline is converted to a TIN point with the same XYZ coordinates. For 3D lines, each line that you select is defined as a two-point breakline.

**Topics in this section**

*   [Exercise 1: Creating a TIN Surface](GUID-34D24CED-7589-4E07-9AC1-E625834FD1D6.htm)  
    In this exercise, you will create an empty TIN surface in a new drawing.
*   [Exercise 2: Adding Point Data to a Surface](GUID-E6432405-A737-489B-97DC-2555D21FF183.htm)  
    In this exercise, you will import point data from a text file into the current drawing.
*   [Exercise 3: Adding Breaklines to a Surface](GUID-9CC1D50D-1542-4CD3-8136-D6E512D5C64E.htm)  
    In this exercise, you will cause the surface to triangulate along a linear feature.
*   [Exercise 4: Adding an Outer Boundary to a Surface](GUID-5A13D6DC-8EEB-4CE3-8C6C-BFEB61EA0979.htm)  
    In this exercise, you will create an outer surface boundary from a polyline.

**Parent topic:** [Surfaces Tutorials](GUID-F6A629B7-EB6B-443A-91EE-491076F71440.htm)