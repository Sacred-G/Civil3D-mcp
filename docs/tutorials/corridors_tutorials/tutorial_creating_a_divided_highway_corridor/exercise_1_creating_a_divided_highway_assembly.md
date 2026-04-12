---
title: "Exercise 1: Creating a Divided Highway Assembly"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-769A44A9-CA84-4986-AD66-6FE1A837BCBD.htm"
category: "tutorial_creating_a_divided_highway_corridor"
last_updated: "2026-03-17T18:42:50.814Z"
---

                 Exercise 1: Creating a Divided Highway Assembly  

# Exercise 1: Creating a Divided Highway Assembly

In this exercise, you will create a fairly complex assembly with a depressed central reserve and separated lanes.

Create an assembly baseline

1.  Open _Corridor-3a.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Assembly drop-down ![](../images/ac.menuaro.gif)Create Assembly ![](../images/GUID-9EF14ADB-F68F-41A2-8C37-C2305BCF1604.png) Find.
3.  In the Create Assembly dialog box, for Name, enter **Divided Highway**. Click OK.
4.  When the ‘Specify assembly baseline location’ prompt is displayed on the command line, click a point in the drawing to build the assembly.
    
    The viewport zooms to the assembly baseline, which looks like this:
    
    ![](../images/GUID-7B40EFFE-DC61-4DF6-8A94-1A7ABE26375E.png)
    

Add a central reserve subassembly

1.  If the Tool Palette containing the subassemblies is not visible, click Home tab ![](../images/ac.menuaro.gif)Palettes panel ![](../images/ac.menuaro.gif)Tool Palettes ![](../images/GUID-7F0579FF-D5B6-41C7-9915-AD38F4803FEB.png) Find.
2.  In the tool palette, right-click the Tool Palettes control bar. Click Civil Imperial Subassemblies.
3.  Click the Central Reserves tab.
4.  Right-click ![](../images/GUID-4C252349-FF3B-4B96-BE01-1DA8697720AD.png)MedianDepressedShoulderExt. Click Help. Review the diagram to better understand the subassembly.
5.  Click ![](../images/GUID-4C252349-FF3B-4B96-BE01-1DA8697720AD.png)MedianDepressedShoulderExt.
6.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Centerline Pivot: **Pivot about centerline**
    *   Left Central Reserve Width: **22.0000**
    *   Right Central Reserve Width: **22.0000**
7.  In the drawing, click the marker point on the assembly baseline. A depressed central reserve and inside shoulders are drawn.

Add a lane subassembly

1.  In the drawing, pan to the left edge of the Central ReserveDepressedShoulderExt subassembly. Zoom in so that each marker point can be seen distinctly.
2.  In the tool palette, click the Lanes tab.
3.  Click ![](../images/GUID-D793AB03-9459-4E7C-A4E7-68DB90642B77.png)LaneSuperelevationAOR.
    
    This subassembly inserts a travel lane that follows the slope for the superelevation properties of the alignment.
    
    Note:
    
    For more information about superelevation, see the [Applying Superelevation to an Alignment](GUID-AA0068E0-2858-4067-9104-161112DEDBF6.htm "In this tutorial, you will calculate superelevation for alignment curves, create a superelevation view to display the superelevation data, and edit the superelevation data both graphically and in a tabular format.") tutorial.
    
4.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Side: **Left**
    *   Width: **24.0000**
5.  In the drawing, click the marker that is at the top left edge of the central reserve to insert the lane:
    
    ![](../images/GUID-68D490A0-9056-443E-B4E8-109FDDBE385C.png)
    

Add a shoulder subassembly

1.  In the drawing, pan to the left side of the LaneSuperelevationAOR subassembly.
2.  In the tool palette, click the Shoulders tab.
3.  Click ![](../images/GUID-AF4FF871-9891-4D01-B79E-71313198895C.png)ShoulderExtendSubbase.
4.  In the Properties palette, under ADVANCED, specify the following parameters:
    
    *   Side: **Left**
    *   Use Superelevation Gradient: **Outside Shoulder Gradient**
    *   Subbase - Use Superelevation: **Outside Shoulder Gradient**
    
    This sets these gradients to the outside shoulder superelevation gradient. For more information, see the subassembly help.
    
5.  In the drawing, click the marker point at the top outside edge-of-lane on finish gradient to add the paved shoulder.

Add a daylight subassembly

1.  In the drawing, pan to the left side of the ShoulderExtendSubbase.
2.  In the tool palette, click the Daylight tab.
3.  Right-click ![](../images/GUID-2E03072C-7CF5-4249-9ECA-BECE15498CC1.png)DaylightStandard. Click Help. Review the diagram and Behavior section to better understand the cut and fill daylighting behaviors.
4.  In the tool palette, click ![](../images/GUID-2E03072C-7CF5-4249-9ECA-BECE15498CC1.png)DaylightStandard.
5.  In the drawing, click the marker point at the outside edge of the ShoulderExtendSubbase subassembly to add the daylight gradients for cut and fill.

Mirror the subassemblies to the right side of the baseline

1.  Press Esc to exit subassembly placement mode.
2.  In the drawing, zoom out until you see the entire assembly. On the left-hand side of the assembly, select the daylight, shoulder, and lane subassemblies. Right click. Click Mirror.
3.  Click the marker point at the top-right edge of the central reserve subassembly to draw a mirror of the daylight, shoulder, and lane subassemblies.
    
    The Mirror command creates a mirror image of the selected subassemblies. All the subassembly parameters, except for the Side parameter, are retained.
    
    Note:
    
    The parameters of the mirrored subassemblies are not dynamically linked. If you change a parameter value for a subassembly on one side of the assembly baseline, the change will not be applied to the opposite side.
    
    The finished assembly looks like this:
    
    ![](../images/GUID-77A0096B-2D99-4A7B-B3BA-8F24561CA7E0.png)
    

To continue this tutorial, go to [Exercise 2: Creating a Divided Highway Corridor](GUID-8704B58C-4F4C-4920-AA88-89251D143B60.htm "In this exercise, you will create a divided highway corridor.").

**Parent topic:** [Tutorial: Creating a Divided Highway Corridor](GUID-1164369B-CC02-47C4-B9E4-12A34DA41287.htm "This tutorial demonstrates how to create a divided highway corridor. The tutorial uses some of the subassemblies that are shipped with Autodesk Civil 3D to create a more complex and realistic highway model.")