---
title: "Exercise 4: Translating a Survey Database"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-09CF6798-DFA8-4F40-BC6F-564F5DA7DE40.htm"
category: "tutorial_analyzing_and_reducing_survey_data"
last_updated: "2026-03-17T18:42:17.608Z"
---

                 Exercise 4: Translating a Survey Database  

# Exercise 4: Translating a Survey Database

In this exercise, you will translate the contents of a survey database from assumed coordinates to that of known coordinates.

This command is useful when you must move all the data in the survey database from an assumed location to a known location.

This exercise continues from [Exercise 3: Performing Least Squares Analysis](GUID-8E0A1B90-378B-45D2-AAA4-BA7036F1217F.htm "In this exercise, you will reduce the survey information using the least squares adjustment method.").

Examine the contents of a survey database

Note:

This exercise uses _Survey-3.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Survey tab, expand the Survey Databases![](../images/ac.menuaro.gif)Survey 1 ![](../images/ac.menuaro.gif)Networks![](../images/ac.menuaro.gif)Survey Network 1 collections.
3.  Click the Setups collection.
    
    In the list view, examine the level values of the following setups:
    
    *   Chainage Point 1, Backsight Point 1000: Instrument Level = 263.650
    *   Chainage Point 2, Backsight Point 1: Instrument Level = 259.960
    *   Chainage Point 3, Backsight Point 2: Instrument Level = 257.438
    
    For this exercise, you will assume that the level value for Chainage Point 1, Backsight Point 1000, which is at Easting 5000 and Northing 5000, is 2.25 greater than the actual level.
    

Translate the survey database

1.  In Toolspace, on the Survey tab, select the **Survey 1** database. Right-click. Click Translate Survey Database.
2.  In the Translate Survey Database wizard, on the Base Point page, specify the following parameters:
    
    *   Easting: **5000**
    *   Northing: **5000**
    
    On this page, you specify the base point from which the survey network will be moved.
    
3.  Click Next.
    
    On the Rotation Angle page, you specify the base point from which the survey network will be moved. For this exercise, you will not change the rotation of the survey network.
    
4.  On the Rotation Angle page, for Rotation Angle, enter **0**.
5.  Click Next.
    
    On the Destination Point page, you specify the point to which the survey network will be moved. Notice that you can specify a new Easting, Northing, or Level Change value. In this exercise, you will change only the level.
    
6.  On the Destination Point page, for Level Change, enter **\-2.25**.
7.  Click Next.
    
    On the Summary page, you can examine the results of the translation before it is applied to the survey network. If you want to modify the translation, you can use the Back button to return to previous pages on the wizard.
    
8.  Click Finish.
9.  In Toolspace, on the Survey tab, click the Setups collection.
    
    In the list view, examine the level values of the setups that you examined in Step 2. Notice that the values have been decreased by 2.25.
    
    *   Chainage Point 1, Backsight Point 1000: Instrument Level = 261.400
    *   Chainage Point 2, Backsight Point 1: Instrument Level = 257.710
    *   Chainage Point 3, Backsight Point 2: Instrument Level = 255.188

To continue to the next tutorial, go to [Manually Creating Survey Data](GUID-4ECE08C3-B0C0-4D90-B24C-C4BC0A8FAA41.htm "This tutorial demonstrates how to manually create and add survey data.").

**Parent topic:** [Tutorial: Analyzing and Reducing Survey Data](GUID-3E8237DF-5A64-4206-A038-FC5836269A8B.htm "This tutorial demonstrates how to analyze and reduce survey data.")