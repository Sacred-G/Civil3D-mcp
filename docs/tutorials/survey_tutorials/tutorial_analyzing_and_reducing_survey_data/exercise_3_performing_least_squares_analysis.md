---
title: "Exercise 3: Performing Least Squares Analysis"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-8E0A1B90-378B-45D2-AAA4-BA7036F1217F.htm"
category: "tutorial_analyzing_and_reducing_survey_data"
last_updated: "2026-03-17T18:42:17.566Z"
---

                  Exercise 3: Performing Least Squares Analysis  

# Exercise 3: Performing Least Squares Analysis

In this exercise, you will reduce the survey information using the least squares adjustment method.

The least squares method calculates the most probable value for each observation. The values are calculated by adjusting each of the observations simultaneously so that the sum of the squares of the residuals (the difference between measured and adjusted observations) is at a minimum.

This exercise continues from [Exercise 2: Performing Traverse Analysis](GUID-C184A3AF-0761-4879-9239-AB8E85A3AC55.htm "In this exercise, you will reduce some of the survey data using the traverse analysis Compass Rule adjustment method.").

Modify the survey network style

Note:

This exercise uses _Survey-3.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Settings tab, expand the Survey![](../images/ac.menuaro.gif)Network Styles collection.
    
    This collection contains the existing network styles in the drawing.
    
3.  Right-click the **Standard** network style. Click Edit.
4.  In the Network Style dialog box, click the Components tab.
5.  Under Error Ellipse, set the Error Ellipse Scale Factor to **10000.00**.
6.  Click OK.

Run the least squares analysis

1.  In Toolspace, on the Survey tab, expand the Survey Databases![](../images/ac.menuaro.gif)**Survey 1**![](../images/ac.menuaro.gif)Networks collection. Right-click the network **Survey Network 1**. Click Least Squares Analysis![](../images/ac.menuaro.gif)Perform Analysis.
2.  In the Least Squares Analysis dialog box, under Input, specify the following parameters:
    
    *   Create Input File: **Selected**
    *   Input File Name: **Survey Network 1**
    *   Network Adjustment Type: **3-Dimensional**
    
    Use the default values for the other settings.
    
3.  Click OK. If prompted, click Yes to overwrite the existing Network File.
    
    The analysis runs, the network and drawing updates, and the following files are displayed in the ASCII text editor:
    
    *   **<survey network>.lsi**: Displays the initial chainage coordinates and levels. The angle and distance information for each chainage, as well as the standard errors for angles and distances are also displayed.
    *   **<survey network>.lso**: Displays the results of the calculations as well as the adjusted coordinate information.
4.  When you have finished reviewing the least squares calculation data, close the text files.
5.  To view an error ellipse, on the Survey tab, select the **Survey Network 1**![](../images/ac.menuaro.gif)Control Points collection. In the list view, right-click **2**. Click Zoom To.
    
    The drawing zooms to the point and ellipse.
    

To continue this tutorial, go to [Exercise 4: Translating a Survey Database](GUID-09CF6798-DFA8-4F40-BC6F-564F5DA7DE40.htm "In this exercise, you will translate the contents of a survey database from assumed coordinates to that of known coordinates.").

**Parent topic:** [Tutorial: Analyzing and Reducing Survey Data](GUID-3E8237DF-5A64-4206-A038-FC5836269A8B.htm "This tutorial demonstrates how to analyze and reduce survey data.")