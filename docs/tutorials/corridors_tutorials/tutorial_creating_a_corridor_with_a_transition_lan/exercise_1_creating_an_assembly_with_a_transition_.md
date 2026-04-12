---
title: "Exercise 1: Creating an Assembly with a Transition Lane"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-3980DDA1-2D0F-4ED8-898E-9522E4FD6D55.htm"
category: "tutorial_creating_a_corridor_with_a_transition_lan"
last_updated: "2026-03-17T18:42:49.871Z"
---

                 Exercise 1: Creating an Assembly with a Transition Lane  

# Exercise 1: Creating an Assembly with a Transition Lane

In this exercise, you will create a corridor assembly with transitions.

Create an assembly baseline

1.  Open _Corridor-2a.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Assembly drop-down ![](../images/ac.menuaro.gif)Create Assembly ![](../images/GUID-9EF14ADB-F68F-41A2-8C37-C2305BCF1604.png) Find.
3.  In the Create Assembly dialog box, for Name, enter **Transition**. Click OK.
4.  When the ‘Specify assembly baseline location’ prompt is displayed on the command line, click a point in the drawing to place the assembly.
    
    The viewport zooms to the assembly baseline, which looks like this:
    
    ![](../images/GUID-7B40EFFE-DC61-4DF6-8A94-1A7ABE26375E.png)
    

Add a lane subassembly

1.  If the Tool Palette containing the subassemblies is not visible, click Home tab ![](../images/ac.menuaro.gif)Palettes panel ![](../images/ac.menuaro.gif)Tool Palettes ![](../images/GUID-7F0579FF-D5B6-41C7-9915-AD38F4803FEB.png) Find.
2.  In the tool palette, right-click the Tool Palettes control bar. Click Civil Imperial Subassemblies.
3.  Click the Basic tab.
4.  Click ![](../images/GUID-5F401557-E252-45B6-BFB4-6C7291268DF0.png)BasicLaneTransition.
5.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Side: **Right**
    *   Default Width: **14.0000**
    *   Depth: **1.0000**
    *   Transition: **Change Offset And Level**
6.  In the drawing, click the marker point on the assembly baseline.
    
    A lane is drawn, extending 14 feet to the right, with a gradient of -2% and a depth of 1 foot.
    

Add a kerb and channel subassembly

1.  In the tool palette, click ![](../images/GUID-E42DDF76-61A1-44AD-BABC-70E590399AF2.png)BasicKerbAndChannel.
2.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Side: **Right**
    *   Channel Width: **1.2500**
3.  In the drawing, click the marker point at the top-right edge of the lane to draw the kerb and channel.

Add a sidewalk subassembly

1.  In the tool palette, click ![](../images/GUID-FBC46EEF-FFFC-4370-B917-22377A0F7C80.png)BasicSidewalk.
2.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Side: **Right**
    *   Buffer Width 1: **2.0000**
    *   Buffer Width 2: **3.0000**
3.  In the drawing, click the marker point at the top back-side of the kerb to add the sidewalk and its buffer zones.

Add a ditch subassembly

1.  In the tool palette, click ![](../images/GUID-2A08956E-31EE-4A8C-9CA3-D8D306FF86BD.png)BasicSideSlopeCutDitch.
2.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Side: **Right**
    *   Cut Gradient: **3.000:1**
3.  In the drawing, click the marker point at the outside edge of the outer sidewalk buffer zone to add the cut-and-fill gradient.

Add a transition lane subassembly

1.  In the tool palette, click ![](../images/GUID-5F401557-E252-45B6-BFB4-6C7291268DF0.png)BasicLaneTransition.
2.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Side: **Left**
    *   Default Width: **12.0000**
    *   Depth: **1.0000**
    *   Transition: **Hold Gradient, Change Offset**
3.  In the drawing, click the marker point on the assembly baseline. A lane is drawn, extending 12 feet to the left, with a gradient of -2% and a depth of 1 foot.

Mirror the subassemblies outside the right lane

1.  Press Esc to exit subassembly placement mode.
2.  In the drawing, on the right-hand side of the assembly, select the kerb, sidewalk, and daylight subassemblies. Right click. Click Mirror.
3.  Click the marker point at the top-left edge of the transition lane to draw a mirror of the kerb, sidewalk, and daylight subassemblies.
    
    The subassemblies are displayed on the left side of the assembly marker.
    
    The Mirror command creates a mirror image of the selected subassemblies. All the subassembly parameters, except for the Side parameter, are retained.
    
    Note:
    
    The parameters of the mirrored subassemblies are not dynamically linked. If you change a parameter value for a subassembly on one side of the assembly baseline, the change will not be applied to the opposite side.
    
    The finished assembly looks like this:
    
    ![](../images/GUID-F337E437-06E6-47CD-A0EE-5ADB3FA96E3E.png)
    

To continue this tutorial, go to [Exercise 2: Creating a Corridor with a Transition Lane](GUID-AFC8E584-CD60-45E6-B9D5-18E69380CF41.htm "In this exercise, you will create a corridor using the assembly created in the last exercise. You will target the width and level of the right lane edge to a right alignment and profile, and the left lane edge to a polyline and a feature line.").

**Parent topic:** [Tutorial: Creating a Corridor with a Transition Lane](GUID-05B7F091-145E-4757-B1FD-10DA48B552D0.htm "This tutorial demonstrates how to create a corridor with a transition lane. The tutorial uses some of the subassemblies that are shipped with Autodesk Civil 3D to create an assembly. Then, you create a carriageway where the travel lane widths and crossfalls are controlled by offset alignments, profiles, polylines, and feature lines.")