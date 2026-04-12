---
title: "Exercise 2: Performing Traverse Analysis"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-C184A3AF-0761-4879-9239-AB8E85A3AC55.htm"
category: "tutorial_analyzing_and_reducing_survey_data"
last_updated: "2026-03-17T18:42:17.519Z"
---

                  Exercise 2: Performing Traverse Analysis  

# Exercise 2: Performing Traverse Analysis

In this exercise, you will reduce some of the survey data using the traverse analysis Compass Rule adjustment method.

The Compass Rule is a method of corrections. It assumes that the closing errors are as much due to errors in observed angles as by the errors in measured distances. The closing errors in latitude and departure are distributed according to the ratio of the length of the line to the total length of the traverse.

This exercise continues from [Exercise 1: Querying Survey Data](GUID-A049134B-B90C-463B-86C6-B2C9276F2C55.htm "In this exercise, you will use the Survey Command Window to perform a query on the survey data.").

Set up a traverse

Note:

This exercise uses _Survey-3.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Survey tab, expand the Survey Databases![](../images/ac.menuaro.gif)**Survey 1**![](../images/ac.menuaro.gif)Networks![](../images/ac.menuaro.gif)**Survey Network 1**. Right-click the Traverses collection. Click New.
3.  In the New Traverse dialog box, for Name, enter **Traverse 1**. Click OK.
4.  On the Survey tab, in the list view, right-click **Traverse 1**, and click Properties.
5.  In the Traverse Properties dialog box, specify the following parameters:
    *   Initial Chainage: **1**
    *   Initial Backsight: **1000**
    *   Chainages: **2,3,4,5,6**
    *   Final Foresight: **6000**
6.  Click OK.

Run a traverse analysis

1.  On the Survey tab, in the list view, right-click **Traverse 1**. Click Traverse Analysis.
2.  In the Traverse Analysis dialog box, specify the following parameters:
    *   Horizontal Adjustment Method: **Compass Rule**
    *   Vertical Adjustment Method: **Length Weighted Distribution**
        
        Use the default values for the other properties.
        
3.  Click OK.
    
    The analysis runs, and the following files are displayed in the ASCII text editor:
    
    *   _Traverse 1 Raw Closure.trv_: Displays the horizontal closure and angular error.
    *   _Traverse 1 Vertical Adjustment.trv_: Displays a report of raw and adjusted levels from the vertical adjustment methods.
    *   _Traverse 1 Balanced Angles.trv_: Displays the adjusted chainage coordinates derived from balancing the angular error and horizontal closure with no angular error.
    *   _Traverse 1.lso_: Displays the adjusted chainage coordinates based on the Horizontal Adjustment Type setting (Compass Rule).
4.  Close all the text files.
    
    A dialog box notifies you that you should use the Process Linework command to update the figures with the new traverse information. Reprocessing the survey linework is a manual process. You will see how to update the linework in the following steps.
    
5.  In the Survey Network Updated dialog box, click Close.
6.  In Toolspace, on the Survey tab, under Import Events, right-click Survey-1.fbk. Click Process Linework.
    
    The Process Linework dialog box enables you to reprocess the survey network linework connectivity after you make corrections to the survey data.
    
    When the survey database is updated following a traverse analysis, the following adjustments to the data that references the traverse take place:
    
    *   Observed chainage points are updated and added to the Control Points collection.
    *   All sides shots from adjusted setups within the traverse are updated.
    *   All figures that reference points within the traverse are updated.
    *   Any of the above data that is displayed in the current drawing is also updated.
    
    Because you will use this survey database to perform other analyses in later exercises, you will not reprocess the linework.
    
7.  Click Cancel.
8.  In Toolspace, on the Survey tab, select the Control Points collection. Right-click. Click Reset Adjusted Coordinates.
    
    This action resets the adjusted coordinates, enabling you to continue with the next analysis.
    

To continue this tutorial, go to [Exercise 3: Performing Least Squares Analysis](GUID-8E0A1B90-378B-45D2-AAA4-BA7036F1217F.htm "In this exercise, you will reduce the survey information using the least squares adjustment method.").

**Parent topic:** [Tutorial: Analyzing and Reducing Survey Data](GUID-3E8237DF-5A64-4206-A038-FC5836269A8B.htm "This tutorial demonstrates how to analyze and reduce survey data.")