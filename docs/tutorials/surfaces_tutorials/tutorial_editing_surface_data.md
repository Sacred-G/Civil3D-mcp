---
title: "Tutorial: Editing Surface Data"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-F8631D3F-ED47-4958-9637-70E7E9FD799F.htm"
category: "surfaces_tutorials"
last_updated: "2026-03-17T18:42:12.177Z"
---

                 Tutorial: Editing Surface Data  

# Tutorial: Editing Surface Data

This tutorial demonstrates some common surface editing tasks, including edge swapping, TIN line deletion, and surface smoothing. You will also hide part of the surface using a hide boundary.

## Edge Swapping

Edge swapping is used to change the direction of two triangle faces in the surface, to create a more accurate surface model. For example, edges can be swapped to match the triangle edges to ridges or swales.

### Click to view the effects of edge swapping.

![](../images/GUID-74BA4E64-6C5D-46A5-AC8E-25F2F779B778.png)

## Deleting TIN Lines

Deleting TIN lines may be required, for example, if the surface has TIN triangles on the perimeter that are long and narrow. In this case, the triangles might not be accurate for the surface, and should be deleted.

Surface TIN or Grid lines can also be deleted within a pond, for example, to create a void area. By removing these lines, you can prevent contours from being drawn through the void areas.

When an edge is removed, either an interior border that follows the adjacent lines is created, or the exterior border is modified to follow the new lines.

## Hide Boundaries

Hide boundaries mask areas of the surface so triangulation, and therefore contours, are not visible in the area. Use hide boundaries to create holes in a surface, for example, to mark a building footprint.

### Click to view the effects of a hide surface boundary.

![](../images/GUID-73E35702-8677-4833-A15C-3D8001053E9B.png)

Note:

When you use a hide boundary, the surface is not deleted. The full surface remains intact. If there are surface TIN lines that you want to permanently remove from the surface, use the Delete Line command.

## Surface Smoothing

Surface smoothing is an operation that adds points at system-determined levels using Natural Neighbor Interpolation(NNI) or Kriging methods. The result is smoothed contours, with no overlap.

You perform smoothing as an edit operation on a surface. You can specify smoothing properties and then turn them on or off. When the smoothing is turned off, the surface reverts back to its original state. However, the smoothing operation remains in the surface operation list, and it can be turned on again.

NNI is a method used to estimate the level (Z) of an arbitrary point (P) from a set of points with known levels.

This method uses information in the triangulation of the known points to compute a weighted average of the levels of the natural neighbors of a point.

### Click to view the nearest neighbors of an arbitrary point (p).

![](../images/GUID-BAC4763A-6C6E-409D-B36D-30E76CDA8132.png)

To use NNI, specify only the output locations of the interpolated points. The levels of the interpolated points are always based on the weighted average of the levels of the existing neighboring points. NNI interpolates only within the surface.

**Topics in this section**

*   [Exercise 1: Swapping TIN Edges](GUID-4A170110-A9DD-4734-BBA2-7CD968F0111A.htm)  
    In this exercise, you will swap several TIN edges in a surface.
*   [Exercise 2: Deleting TIN Lines](GUID-66EFE5F5-74E5-4B95-B7D3-DB610F227368.htm)  
    In this exercise, you will delete TIN lines from a surface.
*   [Exercise 3: Adding a Hide Boundary](GUID-094DD813-8CD1-4178-967E-C0A9F4A26141.htm)  
    In this exercise, you will create a hide boundary on the surface, which will mask unwanted triangulation.
*   [Exercise 4: Smoothing a Surface](GUID-4A8FC2BB-6DBD-4FC7-BD17-A33C1C81A62D.htm)  
    In this exercise, you will smooth a surface using the Natural Neighbor Interpolation (NNI) method.

**Parent topic:** [Surfaces Tutorials](GUID-F6A629B7-EB6B-443A-91EE-491076F71440.htm)