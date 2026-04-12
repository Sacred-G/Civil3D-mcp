---
title: "Exercise 2: Creating a Material List"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-DAC90690-3DC4-400E-8A5C-104205DE7C30.htm"
category: "tutorial_calculating_earthwork_volumes_from_corrid"
last_updated: "2026-03-17T18:43:01.093Z"
---

                Exercise 2: Creating a Material List  

# Exercise 2: Creating a Material List

In this exercise, you will create a material list, which defines the quantity takeoff criteria and surfaces to compare during an earthworks analysis.

A material list is required to generate either an earthworks volume report or a mass haul diagram. A material list specifies the existing ground and datum surface to compare, and is saved with the sample line properties.

This exercise continues from [Exercise 1: Reviewing Quantity Take-off Criteria and Report Settings](GUID-764CEBBB-F573-40B1-9611-9C55B0D9B9CB.htm "In this exercise, you will review the options that are available for quantity takeoff criteria and reports.").

Create a material list

1.  Open _Earthworks-1.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing opens, displaying three viewports.
    
2.  Click Analyze tab![](../images/ac.menuaro.gif)Volumes And Materials panel ![](../images/ac.menuaro.gif)Compute Materials ![](../images/GUID-9F759260-51E5-4E5D-8CD4-6745DC2098FE.png) Find.
3.  In the Select A Sample Line Group dialog box, specify the following parameters:
    *   Select Alignment: **Centerline (1)**
    *   Select Sample Line Group: **SLG-1**
4.  Click OK.
    
    The Compute Materials dialog box is displayed, showing a list of all items defined in the selected criteria.
    
5.  Verify that the Quantity Takeoff Criteria field is set to **Earthworks**.
6.  In the table, expand the Surfaces item.
    
    This shows surfaces EG and Datum. Next, you will set the actual object names that define those surfaces.
    
7.  In the Object Name column, in the **EG** row, click <Click Here...>. Select **EG** from the list.
8.  In the Object Name column, in the **Datum** row, click <Click Here...>. Select **Corridor - (1) Datum** from the list.
    
    In the Earthworks criteria settings, EG is set as the base surface and Datum is set as the Compare surface. The Object Name fields specify which object calls for both an EG surface as the base and a Datum surface as the comparison. These criteria can be used with multiple projects and corridors.
    
    The Object Name fields in the Compute Materials dialog box define a specific surface and corridor surface to map to the names in the Earthworks criteria.
    
9.  Click OK.
    
    The calculation is performed and a list of materials is stored with the sample line group properties. In the drawing, notice that the cut and fill areas in each section are shaded. Hover the cursor over the shaded areas to examine the information that is displayed.
    

To continue this tutorial, go to [Exercise 3: Generating a Volume Report](GUID-80C7C059-EE1C-48AD-B5C0-4A11C7B8E24A.htm "In this exercise, you will use the Earthworks criteria to generate a quantity takeoff report.").

**Parent topic:** [Tutorial: Calculating Earthwork Volumes from Corridor Models](GUID-A9E93299-8F15-42ED-B363-BCDE78046287.htm "This tutorial demonstrates how to calculate cut and fill earthwork quantities between two surfaces.")