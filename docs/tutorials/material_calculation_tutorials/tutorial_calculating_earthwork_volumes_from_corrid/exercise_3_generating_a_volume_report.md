---
title: "Exercise 3: Generating a Volume Report"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-80C7C059-EE1C-48AD-B5C0-4A11C7B8E24A.htm"
category: "tutorial_calculating_earthwork_volumes_from_corrid"
last_updated: "2026-03-17T18:43:01.231Z"
---

                 Exercise 3: Generating a Volume Report  

# Exercise 3: Generating a Volume Report

In this exercise, you will use the Earthworks criteria to generate a quantity takeoff report.

This exercise continues from [Exercise 2: Creating a Material List](GUID-DAC90690-3DC4-400E-8A5C-104205DE7C30.htm "In this exercise, you will create a material list, which defines the quantity takeoff criteria and surfaces to compare during an earthworks analysis.").

Generate a volume report

1.  Open _Earthworks-2.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing opens, displaying three viewports.
    
2.  Click Analyze tab ![](../images/ac.menuaro.gif)Volumes And Materials panel ![](../images/ac.menuaro.gif)Volume Report ![](../images/GUID-2BBE143B-81E9-47F3-8E96-3C5B89EF85C8.png) Find.
3.  In the Report Quantities dialog box, specify the following parameters:
    *   Select Alignment: **Centerline (1)**
    *   Select Sample Line Group: **SLG-1**
    *   Select A Material List: **Material List - (1)**
        
        This is the material list you created in [Exercise 2: Creating a Material List](GUID-DAC90690-3DC4-400E-8A5C-104205DE7C30.htm "In this exercise, you will create a material list, which defines the quantity takeoff criteria and surfaces to compare during an earthworks analysis.") by calculating volume quantities for the sample line group using the Earthworks criteria.
        
    *   Select A Style Sheet: _Earthwork.xsl_
    *   Display XML Report: **Selected**
4.  Click OK.
5.  The report is displayed.
    
    The Cut Volume is the area of material in cut, multiplied by the Cut Factor defined in the quantity takeoff criteria. The Fill Volume is the area of fill material multiplied by the Fill Factor.
    
    The areas for each material are averaged between stations and multiplied by the chainage difference to produce the incremental volumes. These volumes are added from chainage to chainage to produce the cumulative volumes.
    
    Finally, the Cum. Net Volume value at each chainage is calculated as the cumulative Reusable volume minus the cumulative Fill volume.
    

To continue to the next tutorial, go to [Working with Mass Haul Diagrams](GUID-FE54D3EB-0701-4F90-997A-1D86EEEFC947.htm "This tutorial demonstrates how to create and edit mass haul diagrams to display earthworks in profile.").

**Parent topic:** [Tutorial: Calculating Earthwork Volumes from Corridor Models](GUID-A9E93299-8F15-42ED-B363-BCDE78046287.htm "This tutorial demonstrates how to calculate cut and fill earthwork quantities between two surfaces.")