---
title: "Exercise 2: Modifying the Subassembly Name Template"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6597BC8F-1F9B-4D0A-B030-D84851808127.htm"
category: "tutorial_working_with_assemblies"
last_updated: "2026-03-17T18:42:45.103Z"
---

                 Exercise 2: Modifying the Subassembly Name Template  

# Exercise 2: Modifying the Subassembly Name Template

In this exercise, you will specify a meaningful naming convention to apply to subassemblies as they are created.

This default subassembly naming template specifies that subassemblies use the subassembly name on the tool palette followed by a sequential number. For example, if BasicLane subassemblies are placed on either side of the assembly, they are named BasicLane- (1) and BasicLane - (2).

In this exercise, you will change the naming template so that assemblies will include the side on which the subassembly is placed. For example, if BasicLane subassemblies are placed on either side of the assembly, they are named BasicLane- (Left) and BasicLane - (Right).

Performing this task makes it easy to manage assemblies and subassemblies in complex drawings.

For more information, see [About Subassemblies](https://beehive.autodesk.com/community/service/rest/cloudhelp/resource/cloudhelpchannel/guidcrossbook/?v=2025&p=CIV3D&l=ENG&accessmode=live&guid=GUID-E9277B4B-CC74-4FD7-ABAD-B1F8B369F5A3).

Examine the default subassembly naming convention

1.  Open _Assembly-1b.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains an assembly baseline that does not have any subassemblies attached to it.
    
2.  If the Tool Palette containing the subassemblies is not visible, click Home tab ![](../images/ac.menuaro.gif)Palettes panel ![](../images/ac.menuaro.gif)Tool Palettes ![](../images/GUID-7F0579FF-D5B6-41C7-9915-AD38F4803FEB.png) Find.
3.  In the tool palette, right-click the Tool Palettes control bar. Click Civil Imperial Subassemblies
4.  Click the Basic tab.
5.  Click ![](../images/GUID-87516101-DEB4-4301-81B4-EFA5A1DCE0F4.png)BasicLane.
6.  In the Properties palette, under ADVANCED, set the Side to **Right**.
7.  In the drawing, click the marker point on the assembly baseline.
    
    The right lane subassembly is now attached to the assembly baseline.
    
8.  In the Properties palette, under ADVANCED, set the Side to **Left**.
9.  In the drawing, click the marker point on the assembly baseline.
    
    The left lane subassembly is now attached to the assembly baseline.
    
10.  Press Esc.
11.  In Toolspace, on the Prospector tab, select the Assemblies collection. Expand the Assembly – (1) ![](../images/ac.menuaro.gif) Baseline ![](../images/ac.menuaro.gif) Right and Left collections to see the BasicLane subassembly listed in both. You will change the naming convention in the following steps so that additional subassemblies that are inserted do not use the same name.

Modify the subassembly name template

1.  In Toolspace, on the Settings tab, right-click the Subassembly collection. Click Edit Feature Settings.
2.  In the Edit Feature Settings dialog box, expand Subassembly Name Templates. In the Create From Macro row, click the Value cell. Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
3.  In the Name Template dialog box, in the Name field, highlight the <\[Next Counter\]> property.
4.  In the Property Fields list, select Subassembly Side. Click Insert.
    
    The Name field should contain the following formula:
    
    **<\[Macro Short Name(CP)\]> - (<\[Subassembly Side\]>)**
    
    Tip:
    
    To display the subassembly name in the local language, use the <\[Subassembly Local Name\]> property in place of the <\[Macro Short Name(CP)\]> property.
    
5.  Click OK twice.

7.  In Toolspace, on the Settings tab, expand the Subassembly ![](../images/ac.menuaro.gif) Commands collection.
8.  Right-click CreateSubAssemblyTool ![](../images/ac.menuaro.gif) Edit Command Settings.
9.  In the Edit Command Settings – CreateSubAssemblyTool dialog box, expand Subassembly Options.
10.  In the Subassembly Name row, click the Value cell. Click Use Name Template. Click OK.

Examine the updated subassembly naming convention

1.  In the Tool Palettes window, click ![](../images/GUID-E42DDF76-61A1-44AD-BABC-70E590399AF2.png)BasicKerbAndChannel.
2.  In the Properties palette, under ADVANCED, set the Side to **Right**.
3.  In the drawing, click the marker point at the top-right edge of the travel lane.
    
    ![](../images/GUID-431B8F73-F6EB-47FD-93B2-D4FA03557628.png)
    
    Note:
    
    If you attach the subassembly to the wrong marker, you can move it to the correct location. Press Esc to exit subassembly placement mode. Select the subassembly you wish to move. A blue grip is displayed when the subassembly is selected. Select the grip, and then click the correct marker point.
    
4.  In the Properties palette, under ADVANCED, set the Side to **Left**.
5.  In the drawing, click the marker point at the top-left edge of the travel lane.
6.  Press Esc.
7.  In Toolspace, on the Prospector tab, select the Assemblies collection. Expand the Assembly – (1) ![](../images/ac.menuaro.gif) Baseline ![](../images/ac.menuaro.gif) Right and Left collections.
    
    Notice that there are two new subassemblies, BasicKerbAndChannel – (Left) and BasicKerbAndChannel – (Right). These names are more specific than those of the BasicLane subassemblies.
    
    Note:
    
    The next exercise demonstrates more best practices for assembly and subassembly naming in drawings that contain many corridor assemblies.
    

To continue this tutorial, go to [Exercise 3: Managing Assemblies and Subassemblies](GUID-D5E6B6C1-B04B-4565-BB37-BA5F3A0E50DA.htm "In this exercise, you will apply some assembly and subassembly management best practices to a drawing that contains multiple corridor assemblies.").

**Parent topic:** [Tutorial: Working with Assemblies](GUID-797C93BF-BA97-42A3-BE77-855001A395AD.htm "This tutorial demonstrates the basic tasks you will use to use Autodesk Civil 3D subassemblies to build corridor assemblies.")