---
title: "Exercise 1: Creating an Assembly"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-01BEF57E-713B-4A16-8FF7-CDE850CB2CB8.htm"
category: "tutorial_working_with_assemblies"
last_updated: "2026-03-17T18:42:44.970Z"
---

                 Exercise 1: Creating an Assembly  

# Exercise 1: Creating an Assembly

In this exercise, you'll use some of the subassemblies that are shipped with Autodesk Civil 3D to create an assembly for a basic crowned carriageway with travel lanes, kerbs, channels, footpaths and slopes to an existing surface.

Note:

The corridor assembly you build will be used to create a corridor model in the [Creating a Basic Corridor](GUID-33CD2D78-34B6-4564-A1BD-AE3FBDEB584B.htm "This tutorial demonstrates how to use Autodesk Civil 3D objects to build a basic corridor model.") tutorial.

Create an assembly baseline

1.  Open _Assembly-1a.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Assembly drop-down ![](../images/ac.menuaro.gif)Create Assembly ![](../images/GUID-9EF14ADB-F68F-41A2-8C37-C2305BCF1604.png) Find.
3.  In the Create Assembly dialog box, for name, enter **Primary Road Full Section**. Click OK.
4.  When the ‘Specify assembly baseline location’ prompt is displayed on the command line, click in the rectangle under the profile views.
    
    The viewport zooms to the assembly baseline, which looks like this:
    
    ![](../images/GUID-7B40EFFE-DC61-4DF6-8A94-1A7ABE26375E.png)
    

Add a lane subassembly

1.  If the Tool Palette containing the subassemblies is not visible, click Home tab ![](../images/ac.menuaro.gif)Palettes panel ![](../images/ac.menuaro.gif)Tool Palettes ![](../images/GUID-7F0579FF-D5B6-41C7-9915-AD38F4803FEB.png) Find.
2.  In the tool palette, right-click the Tool Palettes control bar. Click Civil Metric Subassemblies.
3.  Click the Lanes tab.
4.  Click ![](../images/GUID-D793AB03-9459-4E7C-A4E7-68DB90642B77.png)LaneSuperelevationAOR.
5.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Side: **Right**
    *   Width: **3.5**
    *   Potential Pivot: **No**
6.  In the drawing, click the marker point on the assembly baseline.
    
    The right lane subassembly is now attached to the assembly baseline.
    

Add a kerb subassembly

1.  In the Tool Palettes window, on the Kerbs tab, click ![](../images/GUID-A25E0C68-9BAD-4C11-ADF7-EE5D1DC70B75.png)UrbanKerbChannelGeneral.
2.  In the drawing, click the marker point at the top-right edge of the travel lane.
    
    ![](../images/GUID-431B8F73-F6EB-47FD-93B2-D4FA03557628.png)
    
    Note:
    
    If you attach the subassembly to the wrong marker, you can move it to the correct location. Press Esc to exit subassembly placement mode. Select the subassembly you wish to move. A blue grip is displayed when the subassembly is selected. Select the grip, and then click the correct marker point.
    

Add a sidewalk subassembly

1.  In the Tool Palettes window, on the Basic tab, click ![](../images/GUID-FBC46EEF-FFFC-4370-B917-22377A0F7C80.png)Basic Sidewalk.
2.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Side: **Right**
    *   Width: **1.5**
    *   Buffer Width 1: **0.5**
    *   Buffer Width 2: **0.5**
3.  In the drawing, click the marker point at the top, back of the kerb.
    
    ![](../images/GUID-A905EB9E-36A2-445C-9C6A-3746837F4D60.png)
    

Add a daylight subassembly

1.  In the Tool Palettes window, on the Basic tab, click ![](../images/GUID-2A08956E-31EE-4A8C-9CA3-D8D306FF86BD.png)BasicSideSlopeCutDitch.
2.  In the Properties palette, under ADVANCED, specify the following parameters:
    *   Side: **Right**
    *   Cut Slope: **2.000:1**
    *   Fill Gradient: **4.000:1**
3.  In the drawing, click the marker point at the outside edge of the sidewalk subassembly.
    
    ![](../images/GUID-CA9D18CF-63E6-44DE-B293-153E0A7C89AF.png)
    
4.  Press Esc.
    
    This action ends the subassembly placement command.
    

Mirror the subassemblies to the left of the baseline

1.  In the drawing, select the four subassemblies you added.
    
    ![](../images/GUID-296F82CE-5D4E-4482-B0D6-2EC6BC122DAB.png)
    
2.  Right click. Click Mirror.
3.  Click the marker point on the assembly baseline.
    
    The subassemblies are displayed on the left side of the assembly marker. The Mirror command creates a mirror image of the selected subassemblies. All the subassembly parameters, except for the Side parameter, are retained.
    
    ![](../images/GUID-82974E0C-02C3-4A03-991A-ED7036ADCED7.png)
    
    Note:
    
    The parameters of the mirrored subassemblies are not dynamically linked. If you change a parameter value for a subassembly on one side of the assembly baseline, the change will not be applied to the opposite side.
    

To continue this tutorial, go to [Exercise 2: Modifying the Subassembly Name Template](GUID-6597BC8F-1F9B-4D0A-B030-D84851808127.htm "In this exercise, you will specify a meaningful naming convention to apply to subassemblies as they are created.").

**Parent topic:** [Tutorial: Working with Assemblies](GUID-797C93BF-BA97-42A3-BE77-855001A395AD.htm "This tutorial demonstrates the basic tasks you will use to use Autodesk Civil 3D subassemblies to build corridor assemblies.")