---
title: "Exercise 2: Adding Conditional Subassemblies to a Corridor Assembly"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E3846BC4-E89B-4191-B313-5FF32607173F.htm"
category: "tutorial_creating_an_assembly_with_conditions"
last_updated: "2026-03-17T18:42:46.305Z"
---

                Exercise 2: Adding Conditional Subassemblies to a Corridor Assembly  

# Exercise 2: Adding Conditional Subassemblies to a Corridor Assembly

In this exercise, you will add ConditionalCutOrFill subassemblies to an existing corridor assembly.

You will specify two levels of ConditionalCutOrFill subassemblies. In the first level, you will attach three ConditionalCutOrFill subassemblies to an existing guardrail subassembly, and then add standard daylight subassemblies to them:

*   **Cut**: Add a ditch, and then daylight to surface at a 2:1 gradient.
*   **Fill < 5.0000’**: Add a ditch, and then daylight to surface at a 8:1 gradient.
*   **Fill > 5.0000’**: Add benching, and then daylight to surface at a 4.0:1 gradient.
    
    ![](../images/GUID-98B781B6-4F1A-4FEB-8B02-7D8CA248D2A0.png)
    

You will add a second level of ConditionalCutOrFill subassemblies to the ditch subassembly that is applied in cut conditions:

*   **Cut < 5.0000’**: Daylight to the surface at a point that is 60’ from the baseline.
*   **Cut > 5.0000’**: Add a 12.0’ daylight link at a -2.0% gradient, and then add a retaining wall.
*   **Fill**: gradient to surface at 4.0%.
    
    ![](../images/GUID-F4011269-D72A-4C4D-AD57-13550E45C40E.png)
    

This exercise continues from [Exercise 1: Examining the Existing Corridor in Section](GUID-F3D8D55E-A87B-4093-8ADA-2A3E0C5EC6E9.htm "In this exercise, you will examine how the daylight subassemblies are applied to the corridor model in section. You will notice chainages at which the current daylighting parameters are inappropriate for the site conditions.").

Add three conditional subassemblies

1.  If the Tool Palette containing the subassemblies is not visible, click Home tab ![](../images/ac.menuaro.gif)Palettes panel ![](../images/ac.menuaro.gif)Tool Palettes ![](../images/GUID-7F0579FF-D5B6-41C7-9915-AD38F4803FEB.png) Find.
2.  In the tool palette, right-click the Tool Palettes control bar. Click Civil Imperial Subassemblies.
3.  Click the Conditional tab.
4.  Click ![](../images/GUID-2D5A8421-6697-460B-AE49-4C999A734085.png)ConditionalCutOrFill.
5.  In the Properties palette, specify the following parameters:
    
    *   Side: **Left**
    *   Layout Width: **20.0000’**
    *   Layout Gradient: **4.000:1**
    *   Type: **Fill**
    *   Minimum Distance: **0.0000’**
    *   Maximum Distance: **5.0000’**
    
    Note:
    
    The Layout Width and Layout Gradient parameters only affect the appearance of the subassembly in layout view. These parameters enable you to position the conditional subassembly and subassemblies that are attached to it, but do not affect the corridor model.
    
6.  In the drawing, in the top viewport, click the left guardrail to add the ConditionalCutOrFill subassembly.
    
    ![](../images/GUID-882F87B3-78A9-4E75-B9D4-666FE9AE525F.png)
    
7.  Add a second ConditionalCutOrFill subassembly to the left guardrail using the following parameters:
    *   Side: **Left**
    *   Layout Width: **20.0000’**
    *   Layout Gradient: **1.000:1**
    *   Type: **Fill**
    *   Minimum Distance: **5.0001’**
    *   Maximum Distance: **10000.0000’**
8.  Add a third ConditionalCutOrFill subassembly to the left guardrail using the following parameters:
    
    *   Side: **Left**
    *   Layout Width: **20.0000’**
    *   Layout Gradient: **1.000:1**
    *   Type: **Cut**
    *   Minimum Distance: **0.0000’**
    *   Maximum Distance: **10000.0000’**
    
    ![](../images/GUID-B90AAAAA-50C2-4C1D-A3DF-1AE20451F764.png)
    

Add a daylight bench subassembly

1.  Using the Daylight tool palette, add a ![](../images/GUID-9FB18906-6E71-4A7D-88C8-7B2B2DA9243B.png)DaylightBench subassembly to the Fill 5.00 : 10000.00 conditional subassembly using the following parameters:
    *   Side: **Left**
    *   Cut Gradient: **4.000:1**
    *   Max Cut Height: **5.0000’**
    *   Fill Gradient: **4.000:1**
    *   Max Fill Height: **5.0000’**
    *   Bench Width: **6.0000’**
    *   Bench Gradient: -**10.000%**
2.  Press Esc to exit subassembly placement mode.
    
    ![](../images/GUID-D7794860-A427-4776-8878-6C55C1587CE0.png)
    

Move and copy the original daylight subassembly

1.  Select the original left ditch subassembly. Right-click. Click Move To. Click the Fill 0.00 : 5.00 conditional subassembly.
2.  Select the ditch subassembly that you just moved. Right-click. Click Copy To. Click the Cut 0.00 : 10000.00 conditional subassembly.
    
    When you are finished, the assembly should look like this:
    
    ![](../images/GUID-98B781B6-4F1A-4FEB-8B02-7D8CA248D2A0.png)
    

Add a second level of conditional subassemblies

1.  Using the Conditional tool palette, add a ![](../images/GUID-2D5A8421-6697-460B-AE49-4C999A734085.png)ConditionalCutOrFill subassembly to the hinge point on the daylight basin subassembly for the cut condition using the following parameters:
    
    *   Side: **Left**
    *   Layout Width: **12.0000’**
    *   Layout Gradient: **0.500:1**
    *   Type: **Cut**
    *   Minimum Distance: **5.0001’**
    *   Maximum Distance: **10000.0000’**
    
    ![](../images/GUID-A7D2C496-B8DF-4480-9255-696DE51F348A.png)
    
2.  Add a second ConditionalCutOrFill subassembly to the hinge point on the daylight basin subassembly using the following parameters:
    *   Side: **Left**
    *   Layout Width: **12.0000’**
    *   Layout Gradient: **1.000:1**
    *   Type: **Cut**
    *   Minimum Distance: **0.0000’**
    *   Maximum Distance: **5.0000’**
3.  Add a third ConditionalCutOrFill subassembly to the hinge point on the daylight basin subassembly using the following parameters:
    
    *   Side: **Left**
    *   Layout Width: **12.0000’**
    *   Layout Gradient: **1.000:1**
    *   Type: **Fill**
    *   Minimum Distance: **0.0000’**
    *   Maximum Distance: **10000.0000’**
    
    ![](../images/GUID-8F1D26DC-4E06-4DB7-A6D1-991AD792B1AB.png)
    

Add subassemblies to the second level

1.  Using the Generic tool palette, add a ![](../images/GUID-C400A815-E890-45FF-AF50-700812F385E4.png)LinkWidthAndSlope subassembly to the Cut 5.00 : 10000.00 conditional subassembly using the following parameters:
    *   Side: **Left**
    *   Width: **12.0000’**
    *   Gradient: **\-2.000%**
2.  Using the Retaining Walls tool palette, add a ![](../images/GUID-88E37674-44E5-4BDA-8D40-845FE7DCD48E.png)RetainWallVertical subassembly to the LinkWidthAndSlope subassembly using the default parameters.
3.  Using the Generic tool palette, add a ![](../images/GUID-FF772031-6E3F-442A-B816-CAE8B94AE2A9.png)LinkOffsetOnSurface subassembly to the Cut 0.00 : 5.00 conditional subassembly using the following parameters:
    *   Offset From Baseline: **\-60.000’**
    *   Omit Link: **No**
4.  Using the Generic tool palette, add a ![](../images/GUID-44542613-3BAA-489F-8D58-37EF3574B1CD.png)LinkSlopeToSurface subassembly to the Fill 0.00 : 10000.00 conditional subassembly using the following parameters:
    
    *   Side: **Left**
    *   Gradient: **4.000%**
    *   Add Link In: **Fill Only**
    
    Note:
    
    The Fill 0.00 : 10000.00 conditional subassembly that is attached to the cut branch of the assembly will be applied if the daylight basin subassembly were to end in a fill condition.
    
5.  Press Esc to exit subassembly placement mode.
    
    When you are finished, the assembly should look like this:
    
    ![](../images/GUID-F4011269-D72A-4C4D-AD57-13550E45C40E.png)
    

To continue this tutorial, go to [Exercise 3: Adjusting Conditional Subassembly Properties](GUID-BC24C59A-5E1E-4727-B32D-137A6E95141C.htm "In this exercise, you will adjust the properties of one of the subassemblies, and then assign descriptive names to each of the subassemblies in the Through Road assembly.").

**Parent topic:** [Tutorial: Creating an Assembly with Conditions](GUID-4CC6F8B3-01A6-4A95-988A-AF9E0A05A875.htm "This tutorial demonstrates how to use the ConditionalCutOrFill subassembly to build a corridor assembly that applies different subassemblies depending on the cut or fill condition at a given chainage.")