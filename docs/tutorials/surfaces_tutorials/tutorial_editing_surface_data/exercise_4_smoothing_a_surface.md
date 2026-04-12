---
title: "Exercise 4: Smoothing a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-4A8FC2BB-6DBD-4FC7-BD17-A33C1C81A62D.htm"
category: "tutorial_editing_surface_data"
last_updated: "2026-03-17T18:42:12.336Z"
---

                  Exercise 4: Smoothing a Surface  

# Exercise 4: Smoothing a Surface

In this exercise, you will smooth a surface using the Natural Neighbor Interpolation (NNI) method.

This exercise continues from [Exercise 3: Adding a Hide Boundary](GUID-094DD813-8CD1-4178-967E-C0A9F4A26141.htm "In this exercise, you will create a hide boundary on the surface, which will mask unwanted triangulation.").

Smooth a surface using NNI

1.  Open _Surface-4D.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Prospector tab, expand the **XGND** surface ![](../images/GUID-7D57421A-5F53-439C-BB54-70E51EBF2F85.png)Definition collection and right-click ![](../images/GUID-A96BA47D-36B9-46CB-AD73-B8F5DC6C4A06.png)Edits.
3.  Click Smooth Surface.
4.  In the Smooth Surface dialog box, specify the following parameters:
    *   Select Method: **Natural Neighbor Interpolation**
    *   Output Locations: **Grid Based**
        
        The Grid Based output location interpolates surface points on a grid defined within specified polygon areas selected in the drawing. After the areas are defined, you can specify the grid X and Y spacing and orientation properties.
        
5.  For the Select Output Region parameter, click the Value column. Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
6.  On the command line, enter **Surface** for the output region. This option will smooth the whole surface, rather than just the area within a specified rectangle or polygon.
7.  In the Smooth Surface dialog box, specify the following parameters:
    *   Grid X-Spacing: **10**
    *   Grid Y-Spacing: **10**
8.  In the drawing window, notice where some of the contour lines are especially angular. Click OK to smooth the surface.
    
    The display of the surface is smoothed; contours are less angular. A Smooth Surface item is added to the Edits list view on the Prospector tab.
    
    Note:
    
    The Description column in the list view displays the type of surface smoothing that was used (Natural Neighbor Smoothing). You can delete the Smooth Surface edit from the list, but this does not reverse the smoothing operation until you rebuild the surface. You can also reverse the smoothing operation by using the **U** (undo) command.
    

To continue to the next tutorial, go to [Creating a Watershed and Water Drop Analysis](GUID-23EC67A6-0084-4DAC-B58F-141155DB7E29.htm "This tutorial demonstrates how to create two kinds of surface analysis: watershed and water drop.").

**Parent topic:** [Tutorial: Editing Surface Data](GUID-F8631D3F-ED47-4958-9637-70E7E9FD799F.htm "This tutorial demonstrates some common surface editing tasks, including edge swapping, TIN line deletion, and surface smoothing. You will also hide part of the surface using a hide boundary.")