---
title: "Exercise 1: Viewing Corridor Sections"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-8CD5788B-54A7-4DCA-98E6-CA6494E1ACA0.htm"
category: "tutorial_viewing_and_editing_corridor_sections"
last_updated: "2026-03-17T18:42:51.973Z"
---

                 Exercise 1: Viewing Corridor Sections  

# Exercise 1: Viewing Corridor Sections

In this exercise, you will view how a corridor assembly is applied at various chainages along a baseline alignment.

The view/edit corridor section tools are useful for inspecting how the corridor assemblies interact with other objects in the corridor model.

View a corridor in section

1.  Open _Corridor-4a.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains an assembly, profile view, and corridor model. Each object is displayed in a separate viewport. The viewport that contains the assembly is active.
    
2.  Click Modify tab ![](../images/ac.menuaro.gif)Design panel ![](../images/ac.menuaro.gif)Corridor ![](../images/GUID-A7EED820-EACA-49FA-8D6D-15B7543DC2CA.png) Find.
3.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor Sections panel ![](../images/ac.menuaro.gif)Section Editor![](../images/GUID-524AFC3A-7C80-4E4E-9BDB-FE0F2DB3F952.png).
4.  On the Section Editor tab, on the Chainage Selection panel, in the Select A Chainage list, select **0+00.00**.
    
    A cross-section of the corridor at the starting chainage is displayed. The levels and offsets are displayed on the grid axes. The cross section view contains the assembly, as well as the other adjacent objects.
    
    At the current chainage, the offset alignments are represented by vertical green lines, and the existing ground surface is represented by the horizontal red line. The vertical red line in the center of the grid represents the assembly baseline. Profile intersections with the baseline are indicated by ![](../images/GUID-5A8E9A2D-96A2-44DF-803D-4797679769E4.png) markers.
    
    ![](../images/GUID-A35FB898-8AE5-4150-82F9-4723F4F4BE2F.png)
    
5.  Use the tools on the Section Editor tab to view the corridor sections at each corridor chainage. Click Go To Previous Chainage ![](../images/GUID-89BDDE53-C4AA-4EAF-ABB0-06BF21BFD9C9.png) Find and Go To Next Chainage ![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png) Find, or select chainages from the Select A Chainage list.
    
    Notice that as each chainage is displayed on the grid, its location in the plan and profile viewports is identified by a perpendicular line.
    

Experiment with the zoom modes

1.  Zoom in to the lane on the right-hand side of the assembly. Click Go To Next Chainage ![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png) Find.
    
    Notice that the view zooms back out to the grid extents. There are three zoom modes in the view/edit corridor section tools. These modes control the behavior of the grid when you navigate to another chainage:
    
    *   ![](../images/GUID-F5EFC90C-16E9-4EB1-9C39-7D9713025534.png) **Zoom To Extents** —View zooms out to the extents of the assembly, plus the view scale factor. This is the default zoom mode.
    *   ![](../images/GUID-401BF12E-2EB0-4839-88EC-6FF5A31B3124.png) **Zoom To An Offset And Level** —View remains zoomed in on the current offset and level. As you navigate to other sections, the current offset and level remains at the center of the viewport.
    *   ![](../images/GUID-6C9A2784-901C-4909-AB4D-67A92467C509.png) **Zoom To A Subassembly** —View remains zoomed in on a selected subassembly. As you navigate to other sections, the selected subassembly remains at the center of the viewport.
2.  In the Select A Chainage list, select **3+00.00**.
3.  On the View Tools panel, click Zoom To Subassembly ![](../images/GUID-6C9A2784-901C-4909-AB4D-67A92467C509.png) Find.
4.  In the Pick Subassembly dialog box, select **Daylight (Right)**. Click OK.
    
    The view zooms in to the Daylight (Right) subassembly at chainage 3+00.00. Notice the shape, level, and offset of the subassembly.
    
5.  In the Select A Chainage list, select **9+00.00**.
    
    The view zooms in to the Daylight (Right) subassembly at chainage 9+00.00. Notice that the shape, level, and offset of the subassembly is quite different from chainage 3+00.00. The subassembly remains at the center of the grid and at the same zoom factor as you navigate to other stations.
    
6.  On the View Tools panel, click Zoom To An Offset And Level ![](../images/GUID-401BF12E-2EB0-4839-88EC-6FF5A31B3124.png) Find.
7.  Click Go To Next Chainage ![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png) Find several times.
    
    Notice that the offset and level values that are displayed on the grid axes do not change. The shape of the Daylight (Right) subassembly changes to reflect how it ties in to the existing ground surface.
    
8.  On the View Tools panel, click Zoom To Extents ![](../images/GUID-F5EFC90C-16E9-4EB1-9C39-7D9713025534.png) Find.
9.  Click Go To Next Chainage ![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png) Find.
    
    The view zooms back out to the extents of the assembly.
    

To continue this tutorial, go to [Exercise 2: Editing Corridor Sections](GUID-B92D793D-947C-4E04-A6C2-3C0F0565493A.htm "In this exercise, you will edit the parameters at several corridor sections.").

**Parent topic:** [Tutorial: Viewing and Editing Corridor Sections](GUID-B5B31B1A-9F20-4FBD-8A92-DC665B053AFD.htm "This tutorial demonstrates how to edit a corridor in section.")