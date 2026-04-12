---
title: "Exercise 1: Calculating Superelevation for an Alignment"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-30F003B9-95E1-4F9A-ACB9-544A1FC99E54.htm"
category: "tutorial_applying_superelevation_to_an_alignment"
last_updated: "2026-03-17T18:42:28.062Z"
---

                Exercise 1: Calculating Superelevation for an Alignment  

# Exercise 1: Calculating Superelevation for an Alignment

In this exercise, you will calculate superelevation for all the curves in an alignment.

To calculate superelevation for a curve

1.  Open _Align-Superelevation-1.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Select the alignment.
3.  Click Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Superelevation drop-down ![](../images/ac.menuaro.gif)Calculate/Edit Superelevation ![](../images/GUID-58811766-26BB-440D-A960-BED552C24ACC.png) Find.
    
    A dialog box indicates that the alignment does not contain superelevation data.
    
4.  Click Calculate Superelevation Now.
    
    In the Create Superelevation dialog box, the Carriageway Type page contains options for how to apply superelevation to various types of carriageways. Conceptual graphics illustrate the point about which each lane will pivot.
    
5.  Select Single Crowned.
6.  In the Pivot Method list, select Center Baseline.
7.  Click Next.
    
    The Lanes page contains specifications for the number, width, and crossfall of each lane.
    
8.  Specify the following parameters:
    *   Symmetric Carriageway: Selected
        
        This specifies that the same parameters are used for both sides.
        
    *   Number of Lanes Right: 1
    *   Normal Lane Width: 6.000m
    *   Normal Lane Crossfall: -2.00%
9.  Click Next.
    
    The Shoulder Control page contains specifications for how the carriageway shoulders behave when the lanes are superelevated.
    
10.  Under Outside Edge Shoulders, specify the following parameters:
     
     *   Calculate: Selected
     *   Normal Shoulder Width: 5.000m
     *   Normal Shoulder Slope: -5.000%
     *   Low Side: Breakover Removal
     *   High Side: Match Lane Crossfall
     
     Note:
     
     The Inside Central Reserve Shoulder options are disabled because you selected an undivided carriageway type on the Carriageway Type page.
     
11.  Click Next.
     
     The Attainment page enables you to specify the superelevation standards to apply. You apply standards by selecting them from a series of lists. The content of the lists reflects the content of the currently selected design criteria file, which you can customize to suit your local standards. For more information, see the [Modifying a Design Criteria File](GUID-E781CDDC-6D6C-4DEB-AF87-74A758671FAF.htm "In this exercise, you will add a radius and speed table to the design criteria file.") tutorial exercise.
     
12.  Specify the following parameters:
     *   Design Criteria File: \_Autodesk Civil 3D Metric Roadway Design Standards.xml, which is located in the [Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24223) in Corridor Design Standards/Metric
         
         This is the same design criteria file that the criteria-based design feature uses to validate the alignment and profile geometry.
         
     *   Superelevation Rate Table: AASHTO 2001 eMax 6%
     *   Transition Length Table: 2 Lane
     *   Attainment Method: AASHTO 2001 Crowned Carriageway
     *   % Transition Into Straight: 70.00%
     *   % Transition Into Transition: 100.00%
     *   Apply Curve Smoothing: Cleared
     *   Automatically Resolve Overlap: Cleared
13.  Click Finish.
     
     The superelevation values are calculated for each curve, and the Superelevation Tabular Editor vista is displayed. In the drawing, the chainage value and crossfall at each critical superelevation chainage is indicated by symbols and green labels. The symbols and labels were specified as part of the alignment label set. They were not displayed before because there was no superelevation data on the alignment.
     
     You will learn how to edit superelevation data in the Superelevation Tabular Editor in [Exercise 4: Adding and Modifying Superelevation Chainages](GUID-53B257E3-D572-407F-8582-96B731644DEE.htm "In this exercise, you will resolve overlap between two superelevated curves by adding and removing critical chainages, and then editing existing superelevation data.").
     
14.  Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to close the Superelevation Tabular Editor.

To continue this tutorial, go to [Exercise 2: Calculating Superelevation for an Individual Curve](GUID-910D4711-E059-4C57-93AE-87B2AEAEFE39.htm "In this exercise, you will calculate superelevation for a single curve in an alignment that already has superelevation data calculated for other curves.").

**Parent topic:** [Tutorial: Applying Superelevation to an Alignment](GUID-AA0068E0-2858-4067-9104-161112DEDBF6.htm "In this tutorial, you will calculate superelevation for alignment curves, create a superelevation view to display the superelevation data, and edit the superelevation data both graphically and in a tabular format.")