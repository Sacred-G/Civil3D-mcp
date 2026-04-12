---
title: "Exercise 2: Calculating Superelevation for an Individual Curve"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-910D4711-E059-4C57-93AE-87B2AEAEFE39.htm"
category: "tutorial_applying_superelevation_to_an_alignment"
last_updated: "2026-03-17T18:42:28.103Z"
---

                Exercise 2: Calculating Superelevation for an Individual Curve  

# Exercise 2: Calculating Superelevation for an Individual Curve

In this exercise, you will calculate superelevation for a single curve in an alignment that already has superelevation data calculated for other curves.

In the drawing that is used with this exercise, the alignment has a fourth curve, for which superelevation has been calculated. You will make a change to the alignment that will cause the superelevation data of the fourth curve to be out of date, and then you will recalculate the superelevation data for that curve.

This exercise continues from [Exercise 1: Calculating Superelevation for an Alignment](GUID-30F003B9-95E1-4F9A-ACB9-544A1FC99E54.htm "In this exercise, you will calculate superelevation for all the curves in an alignment.").

Change the design speed

1.  Open _Align-Superelevation-2.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Select the alignment.
3.  Click Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Superelevation drop-down ![](../images/ac.menuaro.gif)Calculate/Edit Superelevation ![](../images/GUID-58811766-26BB-440D-A960-BED552C24ACC.png) Find.
    
    The Create Superelevation wizard was not displayed because superelevation has already been calculated for the first three curves of this alignment. The Superelevation Curve Manager window is displayed. This window enables you to view and edit superelevation parameters on a curve-by-curve basis. By default, the window displays superelevation parameters for the first curve in the alignment.
    
4.  Under Superelevation Curve, click Next twice.
    
    Notice that the window displays the parameters for the third curve, which is highlighted in the drawing.
    
5.  Under Superelevation Curve Details, in the Design Speed row, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
    
    In the Alignment Properties - Design Speed dialog box, you can change the design speed of the entire alignment, or add a new design speed at a specified chainage. In this exercise, you will increase the design speed for the end portion of the alignment and leave the existing design speed at the beginning of the alignment.
    
6.  In the Alignment Properties - Design Speed dialog box, click ![](../images/GUID-9E69E35C-4186-4535-B5E4-4459E671CDCB.png).
    
    A second row is displayed in the Design Speeds table.
    
7.  In the second row, enter the following parameters:
    *   Start Chainage: 0+820.00
    *   Design Speed: 70 km/h
8.  Click OK.
    
    In the Superelevation Curve Manager dialog box, the Design Speed value did not change because the chainage at which you changed the design speed is located after Curve.3.
    
9.  Click Next.
    
    Under Superelevation Curve Details, for Curve.4, the Design Speed value is 70 km/h, which is what you specified in the preceding steps.
    
    At the bottom of the dialog box, the Superelevation status is displayed as \*Out of Date\*. Applying the new design speed to this curve caused the superelevation data to become out of date. In the following steps, you will recalculate the superelevation data for this curve to accommodate the new design speed.
    

Calculate superelevation for an individual curve

1.  Click Superelevation Wizard.
    
    The Calculate Superelevation dialog box enables you to select which curves to recalculate.
    
2.  Select This Curve Only. Click OK.
    
    The Calculate Superelevation wizard enables you to specify the parameters used for the calculation. The parameters you specify in the wizard apply to only the curves you selected in the Calculate Superelevation dialog box in Step 1. For example, if you specify a different attainment method or criteria file, then those parameters will not match the rest of the curves in the alignment. In this exercise, you will accept most of the default settings.
    
3.  In the Calculate Superelevation wizard, click Attainment.
4.  On the Attainment page, for Superelevation Rate Table, select AASHTO 2001 eMax 6%.
5.  Under Curve Smoothing, specify the following parameters:
    *   Apply Curve Smoothing: Selected
    *   Curve Length: 30
6.  Click Finish.
    
    On the Superelevation Tabular Editor window, examine the superelevation values for Curve.4.
    
7.  On the Superelevation Curve Manager, scroll down to the Superelevation Criteria category. Expand the category.
8.  Right-click the Normal Shoulder Width row.
    
    The Apply To Entire Alignment option enables you to update the design criteria at a curve, and then quickly apply the change to all curves in the alignment.
    
9.  Right-click the Superelevation Criteria row.
    
    When this option is applied at this level, the design criteria of the current curve overwrites all manual design criteria changes that have been made to the alignment.
    
10.  Press Esc.

To continue this tutorial, go to [Exercise 3: Creating a Superelevation View](GUID-832958B2-A3FC-4F05-B1EF-14ABA74CC950.htm "In this exercise, you will display superelevation data in a graph, which you can use to graphically edit superelevation data.").

**Parent topic:** [Tutorial: Applying Superelevation to an Alignment](GUID-AA0068E0-2858-4067-9104-161112DEDBF6.htm "In this tutorial, you will calculate superelevation for alignment curves, create a superelevation view to display the superelevation data, and edit the superelevation data both graphically and in a tabular format.")