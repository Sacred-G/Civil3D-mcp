---
title: "Exercise 1: Reviewing Quantity Takeoff Criteria and Report Settings"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-764CEBBB-F573-40B1-9611-9C55B0D9B9CB.htm"
category: "tutorial_calculating_earthwork_volumes_from_corrid"
last_updated: "2026-03-17T18:43:00.958Z"
---

                 Exercise 1: Reviewing Quantity Takeoff Criteria and Report Settings  

# Exercise 1: Reviewing Quantity Takeoff Criteria and Report Settings

In this exercise, you will review the options that are available for quantity takeoff criteria and reports.

The quantity takeoff report settings include the default quantity takeoff criteria used to create material lists and default styles for tables. The quantity takeoff criteria includes a list of materials that specifies the surfaces and shapes from which you want to generate volume information.

Review quantity takeoff settings

1.  Open _Earthworks-1.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing opens, displaying three viewports.
    
2.  In Toolspace, on the Settings tab, expand Quantity Takeoff![](../images/ac.menuaro.gif)Commands.
3.  Under the Commands collection, double-click GenerateQuantitiesReport to display the Edit Command Settings dialog box.
4.  Browse through the various settings available, but do not change any settings. When finished, click Cancel.

Examine the quantity takeoff criteria

1.  In Toolspace, on the Settings tab, expand Quantity Takeoff![](../images/ac.menuaro.gif)Quantity Takeoff Criteria.
    
    Three styles are defined in the collection.
    
2.  Double-click the **Earthworks** style to open the Quantity Takeoff Criteria dialog box.
3.  Click the Material List tab.
    
    This tab contains a pre-defined table for calculating earthworks (cut and fill) by comparing a Datum surface layer to an existing ground surface layer.
    
4.  Expand the **Earthworks** item in the table.
    
    You will use the Earthworks criteria in the next exercise to calculate the quantity takeoff.
    
    Notice that the Condition for the EG surface is set to Base, while the condition of the Datum surface is set to Compare. This indicates that the material is going to be fill when Datum is above EG, and cut when Datum is below EG.
    
    Also note the three Factor values in the table:
    
    *   The Cut factor is typically used as an expansion factor for excavated material. It is usually 1.0 or higher.
    *   The Fill factor is typically used as a compaction factor for fill material. It is usually 1.0 or higher.
    *   The Refill factor indicates what percentage of cut material can be reused as fill. It should be 1.0 or lower.
5.  Click Cancel.

To continue this tutorial, go to [Exercise 2: Creating a Material List](GUID-DAC90690-3DC4-400E-8A5C-104205DE7C30.htm "In this exercise, you will create a material list, which defines the quantity takeoff criteria and surfaces to compare during an earthworks analysis.").

**Parent topic:** [Tutorial: Calculating Earthwork Volumes from Corridor Models](GUID-A9E93299-8F15-42ED-B363-BCDE78046287.htm "This tutorial demonstrates how to calculate cut and fill earthwork quantities between two surfaces.")