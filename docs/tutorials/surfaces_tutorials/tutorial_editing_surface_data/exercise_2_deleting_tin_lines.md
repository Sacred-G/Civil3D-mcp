---
title: "Exercise 2: Deleting TIN Lines"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-66EFE5F5-74E5-4B95-B7D3-DB610F227368.htm"
category: "tutorial_editing_surface_data"
last_updated: "2026-03-17T18:42:12.259Z"
---

                 Exercise 2: Deleting TIN Lines  

# Exercise 2: Deleting TIN Lines

In this exercise, you will delete TIN lines from a surface.

The TIN lines fall within a pond. By removing these lines, you can prevent contours from being drawn through the pond area.

This exercise continues from [Exercise1: Swapping TIN Edges](GUID-4A170110-A9DD-4734-BBA2-7CD968F0111A.htm "In this exercise, you will swap several TIN edges in a surface.").

Delete TIN lines

1.  Open _Surface-4B.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    In this drawing, the surface is displayed as TIN lines overlaid on an externally referenced landbase image.
    
2.  Zoom in to the rounded pond area in the upper left of the surface.
    
    ## The lines that cross the pond area
    
    ![](../images/GUID-83DC9847-100D-4FA7-950D-D34C0AF4C1CA.png)
    
3.  In Toolspace, on the Prospector tab, expand the surface ![](../images/GUID-7D57421A-5F53-439C-BB54-70E51EBF2F85.png)Definition collection, and right-click the ![](../images/GUID-A96BA47D-36B9-46CB-AD73-B8F5DC6C4A06.png)Edits item.
4.  Click Delete Line.
    
    On the command line, you are prompted to select an edge (line) to remove.
    
5.  Click an edge that crosses the surface of the pond. Press Enter.
    
    The edge is removed and an interior border is created, following the adjacent TIN lines.
    
6.  Repeat the Delete Line command and remove all TIN lines that cross the pond surface.
    
    Tip:
    
    Enter **C** on the command line to use crossing selection during the delete line command.
    
    ## The revised triangulation and the interior border
    
    ![](../images/GUID-3F310603-15C7-490B-851E-1461A67605E1.png)
    
    The edits are added as Delete Line operations to the Edits list view in Prospector.
    
    Note:
    
    The Description column in the list view provides the coordinates of the vertices for the edge that was deleted.
    

To continue this tutorial, go to [Exercise 3: Adding a Hide Boundary](GUID-094DD813-8CD1-4178-967E-C0A9F4A26141.htm "In this exercise, you will create a hide boundary on the surface, which will mask unwanted triangulation.").

**Parent topic:** [Tutorial: Editing Surface Data](GUID-F8631D3F-ED47-4958-9637-70E7E9FD799F.htm "This tutorial demonstrates some common surface editing tasks, including edge swapping, TIN line deletion, and surface smoothing. You will also hide part of the surface using a hide boundary.")