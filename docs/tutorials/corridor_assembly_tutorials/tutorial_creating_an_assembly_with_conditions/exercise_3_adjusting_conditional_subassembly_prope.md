---
title: "Exercise 3: Adjusting Conditional Subassembly Properties"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-BC24C59A-5E1E-4727-B32D-137A6E95141C.htm"
category: "tutorial_creating_an_assembly_with_conditions"
last_updated: "2026-03-17T18:42:46.447Z"
---

                Exercise 3: Adjusting Conditional Subassembly Properties  

# Exercise 3: Adjusting Conditional Subassembly Properties

In this exercise, you will adjust the properties of one of the subassemblies, and then assign descriptive names to each of the subassemblies in the Through Road assembly.

Give each subassembly a specific, meaningful name to make it easy to identify when you are assigning targets. Meaningful names also help you identify subassemblies in the Subassemblies collection in Prospector.

This exercise continues from [Exercise 2: Adding Conditional Subassemblies to a Corridor Assembly](GUID-E3846BC4-E89B-4191-B313-5FF32607173F.htm "In this exercise, you will add ConditionalCutOrFill subassemblies to an existing corridor assembly.").

Omit the daylight link from the cut conditional subassemblies

Note:

This exercise uses _Assembly-2a.dwg_ with the modifications you made in the previous exercise, or you can open _Assembly-2b.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  In the top viewport, select the DaylightBasin subassembly that is attached to the Cut 0.00 : 10000.00 conditional subassembly. Right-click. Click Subassembly Properties.
3.  In the Subassembly Properties dialog box, on the Parameters tab, for Daylight Link, set the Default Input Value to **Omit Daylight Link**. Click OK.
    
    If you attach a ConditionalCutOrFill subassembly to a standard daylight subassembly, omit the daylight link in the host subassembly. If the daylight link is included in the host subassembly, then daylight links will be created for both the host subassembly and the attached subassembly.
    

Rename the subassemblies

1.  In the top viewport, select the red assembly marker. Right-click. Click Assembly Properties.
2.  In the Assembly Properties dialog box, click the Construction tab.
    
    Notice that the subassemblies you added to the left side of the assembly display the default names followed by the side to which they were added. When you build a complex assembly, you should assign meaningful names to the subassemblies so that you can easily identify them when setting corridor targets. This is also a good practice when a drawing contains multiple assemblies.
    
    For more information, see the [Managing Assemblies and Subassemblies](GUID-D5E6B6C1-B04B-4565-BB37-BA5F3A0E50DA.htm "In this exercise, you will apply some assembly and subassembly management best practices to a drawing that contains multiple corridor assemblies.") exercise.
    
    In the following steps, you will give the subassemblies more meaningful names.
    
3.  In the Item list, select the ConditionalCutOrFill - Left subassembly. Click it again to highlight the text. Change the name to **COND Fill 0-5 for TR-L**.
    
    A descriptive naming convention helps to distinguish between the many ConditionalCutOrFill subassemblies that are present:
    
    *   **COND**: Conditional
    *   **Fill**: The specified condition
    *   **0-5**: The minimum and maximum distance values
    *   **TR**: The parent subassembly of the ConditionalCutOrFill subassembly (TR = Through Road Left)
    *   **\-L**: The side of the baseline that the subassembly is on (L = Left)
4.  Rename the other two ConditionalCutOrFill subassemblies to the following names:
    *   ConditionalCutOrFill - Left (1): **COND Fill 5-10000 for TR-L**
    *   ConditionalCutOrFill - Left (2): **COND Cut 0-10000 for TR-L**
5.  Rename the daylight subassemblies to reflect the cut or fill condition to which they apply:
    *   DaylightBench - Left: **Daylight Bench (Fill) for TR-L**
    *   DaylightBasin - Left: **Daylight Basin (Fill) for TR-L**
    *   DaylightBasin - Left: **Daylight Basin (Cut) for TR-L**
6.  Name the second level of the subassemblies the following:
    
    Note:
    
    To save time, you may choose to skip this step. The sample drawing listed in the next exercise has all the subassemblies named appropriately.
    
    *   ConditionalCutOrFill - Left: **COND Cut 0-10000 -- Cut 5-10000 for TR-L**
    *   ConditionalCutOrFill - Left (3): **COND Cut 0-10000 -- Cut 0-5 for TR-L**
    *   ConditionalCutOrFill - Left (4): **COND Fill 0-10000 -- Cut 0-10000 for TR-L**
    *   LinkWidthAndGradient - Left: **Daylight Width Gradient (Cut 0-10000 -- Cut 5-10000) for TR-L**
    *   RetainWallVertical - Left: **Retaining Wall (Cut 0-10000 -- Cut 5-10000) for TR-L**
    *   LinkOffsetOnSurface: **Daylight Offset On Surface (Cut 0-10000 -- Cut 0-5) for TR-L**
    *   LinkGradientToSurface - Left: **Daylight Gradient To Surface (Cut 0-10000 -- Fill 0-10000) for TR-L**
7.  Click OK.

To continue this tutorial, go to [Exercise 4: Rebuilding the Corridor and Examining the Results](GUID-3689ED14-AAE2-46C5-8C15-1DE36A99A7AB.htm "In this exercise, you will reset the corridor targets, rebuild the corridor, and then examine how the conditional subassembly affects the corridor model.").

**Parent topic:** [Tutorial: Creating an Assembly with Conditions](GUID-4CC6F8B3-01A6-4A95-988A-AF9E0A05A875.htm "This tutorial demonstrates how to use the ConditionalCutOrFill subassembly to build a corridor assembly that applies different subassemblies depending on the cut or fill condition at a given chainage.")