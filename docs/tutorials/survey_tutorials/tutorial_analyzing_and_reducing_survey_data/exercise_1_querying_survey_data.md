---
title: "Exercise 1: Querying Survey Data"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-A049134B-B90C-463B-86C6-B2C9276F2C55.htm"
category: "tutorial_analyzing_and_reducing_survey_data"
last_updated: "2026-03-17T18:42:17.457Z"
---

                 Exercise 1: Querying Survey Data  

# Exercise 1: Querying Survey Data

In this exercise, you will use the Survey Command Window to perform a query on the survey data.

You will use the Inverse Points command to determine the direction and distance between two points.

This exercise continues from the [Importing and Viewing Survey Data](GUID-48040D82-47A0-41C8-86B9-247D2520C977.htm "This tutorial demonstrates how to view and modify survey data in your drawing.") tutorial.

Specify the survey database settings

1.  Open _Survey-3.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains the survey network that was created in the [Importing Field-Coded Survey Data](GUID-344B584B-0E65-4FCF-B85F-A62E2ABCD5DE.htm "In this exercise, you will import survey data from an existing field book file that contains linework codes that can be interpreted by a linework code set.") exercise.
    
2.  In Toolspace, on the Survey tab, expand the Survey Databases collection.
3.  If the database **Survey 1** is not open (that is, it is not displayed with a ![](../images/GUID-EC34E186-CD47-4C57-9776-3E6D0C24CAE3.png) next to it), right-click and click Open For Edit.
    
    Note:
    
    By default, to save on resource usage, when you start Autodesk Civil 3D, all survey databases are displayed in a closed state.
    
4.  Right-click **Survey 1**. Click Edit Survey Database Settings.
5.  In the Survey Database Settings dialog box, expand the Survey Command Window property group. Specify the following parameters:
    
    *   Point Course Echo: **Yes**
    *   Figure Course Echo: **Yes**
    *   Point Coordinate Echo: **Yes**
    *   Figure Coordinate Echo: **Yes**
    *   Command Echo: **Yes**
    
    These settings determine what information will be displayed in the Survey Command Window.
    
6.  Click OK.

Query data using the Survey Command Window

1.  On the Survey tab, expand the Networks collection. Right-click **Survey Network 1**. Click Survey Command Window.
2.  In the Survey Command Window, click View menu ![](../images/ac.menuaro.gif)Zoom To Point.
3.  In the Enter Point dialog box, enter **1**. Click OK.
    
    The drawing zooms to point 1.
    
4.  Click Point Information menu ![](../images/ac.menuaro.gif)Inverse Points.
5.  In the Point Information - Inverse Points dialog box, enter:
    *   Start Point: **1**
    *   Ahead Point: **2**
6.  Click OK.
    
    The following information, which describes the location of each point and the direction and distance between the points, is displayed in the command output area:
    
    !
    ! POINT 1       NORTH: 5000.0000       EAST: 5000.0000       EL: 263.6500   
    !
    !    Distance:  300.000         Course:  N 72-56-33 E
    !
    ! POINT 2       NORTH: 5087.9995       EAST: 5286.8036       EL: 259.9600
    
7.  Close the Survey Command Window.

To continue this tutorial, go to [Exercise 2: Performing Traverse Analysis](GUID-C184A3AF-0761-4879-9239-AB8E85A3AC55.htm "In this exercise, you will reduce some of the survey data using the traverse analysis Compass Rule adjustment method.").

**Parent topic:** [Tutorial: Analyzing and Reducing Survey Data](GUID-3E8237DF-5A64-4206-A038-FC5836269A8B.htm "This tutorial demonstrates how to analyze and reduce survey data.")