---
title: "Exercise 1: Swapping TIN Edges"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-4A170110-A9DD-4734-BBA2-7CD968F0111A.htm"
category: "tutorial_editing_surface_data"
last_updated: "2026-03-17T18:42:12.221Z"
---

                 Exercise 1: Swapping TIN Edges  

# Exercise 1: Swapping TIN Edges

In this exercise, you will swap several TIN edges in a surface.

Swap TIN edges

1.  Open _Surface-4A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    In this drawing, the surface is displayed as TIN lines overlaid on an externally referenced landbase image.
    
2.  Zoom in to the lower edge of the surface.
    
    ## The area to zoom to
    
    ![](../images/GUID-98E2B4CC-B9ED-486C-A072-EEA5FD40B17F.png)
    
3.  In Toolspace, on the Prospector tab, expand the surface ![](../images/GUID-7D57421A-5F53-439C-BB54-70E51EBF2F85.png)Definition collection. Right-click ![](../images/GUID-A96BA47D-36B9-46CB-AD73-B8F5DC6C4A06.png)Edits.
4.  Click Swap Edge.
    
    On the command line, you are prompted to select an edge (line) to swap.
    
5.  Click a TIN edge to swap it.
    
    ## The recommended edges to swap
    
    ![](../images/GUID-0CD97CD1-5908-4D0F-B611-28F767529F3D.png)
    
    The edge is swapped if the following criteria are met:
    
    *   Two visible triangles are separated by the edge.
    *   The quadrilateral formed by the two triangles (which are separated by the edge) is convex.
6.  Optionally, continue to click other TIN edges to swap them.
7.  Press Enter to end the command.
    
    The edits are added as Swap Edge operations to the Edits list view on the Prospector tab.
    
    Note:
    
    The Description column in the list view provides the coordinates of the pick point along the edge that was swapped.
    

To continue this tutorial, go to [Exercise 2: Deleting TIN Lines](GUID-66EFE5F5-74E5-4B95-B7D3-DB610F227368.htm "In this exercise, you will delete TIN lines from a surface.").

**Parent topic:** [Tutorial: Editing Surface Data](GUID-F8631D3F-ED47-4958-9637-70E7E9FD799F.htm "This tutorial demonstrates some common surface editing tasks, including edge swapping, TIN line deletion, and surface smoothing. You will also hide part of the surface using a hide boundary.")