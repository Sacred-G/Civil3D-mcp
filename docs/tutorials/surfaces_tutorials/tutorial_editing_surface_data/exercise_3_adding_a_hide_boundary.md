---
title: "Exercise 3: Adding a Hide Boundary"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-094DD813-8CD1-4178-967E-C0A9F4A26141.htm"
category: "tutorial_editing_surface_data"
last_updated: "2026-03-17T18:42:12.298Z"
---

                 Exercise 3: Adding a Hide Boundary  

# Exercise 3: Adding a Hide Boundary

In this exercise, you will create a hide boundary on the surface, which will mask unwanted triangulation.

A boundary can be created from any polygon or polyline, but in this exercise you will use an existing breakline.

This exercise continues from [Exercise 2: Deleting TIN Lines](GUID-66EFE5F5-74E5-4B95-B7D3-DB610F227368.htm "In this exercise, you will delete TIN lines from a surface.").

Add a hide boundary

1.  Open _Surface-4C.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    Note:
    
    This drawing is similar to _Surface-3.dwg_ with the addition of the C-TOPO-BRKL layer, which displays breaklines.
    
2.  In Toolspace, on the Prospector tab, under the Surfaces collection, expand the **XGND** surface.
3.  Under the **XGND** surface, expand the Definition collection. Right-click ![](../images/GUID-D08C4427-85F9-403D-8CA7-F2F29952B84C.png)Boundaries. Click Add.
4.  In the Add Boundaries dialog box, specify the following parameters:
    *   Name: **XGND-Pond Hide**
    *   Type: **Hide**
    *   Non-Destructive Breakline: **Selected**
    *   Mid-Ordinate Distance: **1.0000**
5.  Click OK.
6.  In the drawing, select the polyline object that matches the perimeter of the pond.
    
    ## The polyline that matches the pond perimeter
    
    ![](../images/GUID-C9C462D6-BAEA-4190-A532-FB844E8D39AD.png)
    
7.  Press Enter.
    
    The hide boundary is added to the surface definition. The surface displayed in the drawing is modified to display the pond as a ‘hole’ in the surface.
    
    ## How the surface should appear with the hide boundary
    
    ![](../images/GUID-48ACDA77-86F9-4576-8F08-523C2406FCE4.png)
    

To continue this tutorial, go to [Exercise 4: Smoothing a Surface](GUID-4A8FC2BB-6DBD-4FC7-BD17-A33C1C81A62D.htm "In this exercise, you will smooth a surface using the Natural Neighbor Interpolation (NNI) method.").

**Parent topic:** [Tutorial: Editing Surface Data](GUID-F8631D3F-ED47-4958-9637-70E7E9FD799F.htm "This tutorial demonstrates some common surface editing tasks, including edge swapping, TIN line deletion, and surface smoothing. You will also hide part of the surface using a hide boundary.")