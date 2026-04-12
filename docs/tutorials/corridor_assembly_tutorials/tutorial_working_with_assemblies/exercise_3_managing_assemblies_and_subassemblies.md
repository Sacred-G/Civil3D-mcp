---
title: "Exercise 3: Managing Assemblies and Subassemblies"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-D5E6B6C1-B04B-4565-BB37-BA5F3A0E50DA.htm"
category: "tutorial_working_with_assemblies"
last_updated: "2026-03-17T18:42:45.233Z"
---

                 Exercise 3: Managing Assemblies and Subassemblies  

# Exercise 3: Managing Assemblies and Subassemblies

In this exercise, you will apply some assembly and subassembly management best practices to a drawing that contains multiple corridor assemblies.

The sample drawing contains several corridor assemblies. This exercise demonstrates how to name and label the assemblies so that they will be easy to manage.

In the sample drawing, several subassemblies are used in multiple assemblies. For example, the LaneSuperelevationAOR subassembly is used in several assemblies. When they were created, the LaneSuperelevationAOR subassemblies all used the same naming template and a sequential number was appended to each name.

Performing the tasks demonstrated in this exercise will make it easy to manage assemblies and subassemblies in complex drawings.

Examine the assemblies

1.  Open _Assembly-1c.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains several completed corridor assemblies. The assemblies are designed to create an intersecting main and side road.
    
2.  In Toolspace, on the Prospector tab, select the Assemblies collection.
    
    In the Toolspace list view, notice that a specific name was assigned to the assemblies when they were created. The names describe the type of corridor, as well as the specific portion of the corridor to which they apply.
    
    In the drawing, notice that each assembly has a label that corresponds to the assembly name. The labels are a simple AutoCAD MText components that make it easy to see the construction of the available assemblies.
    
3.  On the command line, enter **ZE**.
    
    The drawing zooms out to the drawing extents.
    
4.  In Toolspace, on the Prospector tab, select the **Main Road** assembly. Right-click. Click Zoom To.
    
    The drawing zooms to the Main Road assembly.
    

Assign specific names to subassemblies

1.  In the drawing, click the Main Road assembly baseline. Right-click. Click Assembly Properties.
2.  In the Assembly Properties dialog box, on the Construction tab, expand the collections in the Item tree.
    
    The subassemblies that comprise the assembly are displayed in the Item tree. Notice that the subassemblies are categorized into groups. Subassembly groups manage the order in which subassemblies are processed during corridor modeling. The first time you add a subassembly to an assembly, the subassembly is added to the first group. When you add a second subassembly by attaching it to the first subassembly, the second subassembly also gets added to the first subassembly group. The next time you select an assembly baseline, a new subassembly group is automatically created and subsequent subassemblies added to the assembly are added that group.
    
3.  In the Item tree, click the **LaneSuperelevationAOR - (Right) (1)** subassembly.
    
    The subassembly parameters are displayed in the Input Values panel. You can modify the parameters as necessary from this panel.
    
4.  Click the **LaneSuperelevationAOR - (Right) (1)** subassembly again to highlight the text.
5.  Replace the **LaneSuperelevationAOR - (Right) (1)** text with **LaneSuperelevationAOR - (Right) Main Road**. Press Enter.
6.  Repeat Steps 4 through 6 to rename the other subassemblies:
    *   UrbanKerbChannelValley1 - (Right) (1):**UrbanKerbChannelValley1 - (Right) Main Road**
    *   LinkWidthAndGradient - (Right) (1): **LinkWidthAndGradient - (Right) Main Road**
    *   BasicSideGradientCutDitch - (Right) (1): **BasicSideGradientCutDitch - (Right) Main Road**
    *   **LaneSuperelevationAOR - (Left) (1)**: **LaneSuperelevationAOR - (Left) Main Road**
    *   UrbanKerbChannelValley1 - (Left) (1): **UrbanKerbChannelValley1 - (Left) Main Road**
    *   LinkWidthAndGradient - (Left) (1): **LinkWidthAndGradient - (Left) Main Road**
    *   BasicSideGradientCutDitch - (Left) (1): **BasicSideGradientCutDitch - (Left) Main Road**
7.  Repeat Steps 4 through 6 to rename the subassembly groups:
    *   Group - (1): **Main Road Right**
    *   Group - (2): **Main Road Left**
8.  Click OK.
9.  In Toolspace, on the Prospector tab, select the Assemblies collection. Expand the Assembly – (1) ![](../images/ac.menuaro.gif) Baseline ![](../images/ac.menuaro.gif) Right and Left collections to see the subassemblies listed in both.
    
    Notice that the subassembly names you specified are displayed in the Prospector list view.
    

**Further exploration:** Practice what you learned by renaming the subassemblies in the remaining assemblies.

To continue to the next tutorial, go to [Creating an Assembly with Conditions](GUID-4CC6F8B3-01A6-4A95-988A-AF9E0A05A875.htm "This tutorial demonstrates how to use the ConditionalCutOrFill subassembly to build a corridor assembly that applies different subassemblies depending on the cut or fill condition at a given chainage.").

**Parent topic:** [Tutorial: Working with Assemblies](GUID-797C93BF-BA97-42A3-BE77-855001A395AD.htm "This tutorial demonstrates the basic tasks you will use to use Autodesk Civil 3D subassemblies to build corridor assemblies.")