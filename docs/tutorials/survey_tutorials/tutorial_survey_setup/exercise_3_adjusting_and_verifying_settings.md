---
title: "Exercise 3: Adjusting and Verifying Settings"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-3B8ADD98-A14E-4B4A-9FCC-9C946FE84192.htm"
category: "tutorial_survey_setup"
last_updated: "2026-03-17T18:42:15.221Z"
---

                  Exercise 3: Adjusting and Verifying Settings  

# Exercise 3: Adjusting and Verifying Settings

In this exercise, you will view and adjust several types of survey settings.

Survey user settings are specific to a Windows user login account and affect only the survey features, and not the database or drawing data.

Survey database settings are specific to the survey features of an Autodesk Civil 3D survey database.

This exercise continues from [Exercise 2: Setting the Equipment and Figure Prefix Databases](GUID-5DC983D4-24E2-4F65-A531-948687401B72.htm "In this exercise, you will create new survey equipment and figure prefix databases and definitions.").

Specify user settings

Note:

This exercise uses _Survey-1.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Survey tab, click ![](../images/GUID-D0538841-27D6-4160-A1B7-CA014E005DA2.png).
3.  In the Survey User Settings dialog box, specify the following parameters:
    
    *   Miscellaneous![](../images/ac.menuaro.gif)Use External Editor: **Yes**
    *   Network Preview: **All Selected**
    *   Setup Preview: **All Selected**
    *   Figure Preview: **All Selected**
    
    Selecting the check boxes enables the previewing of all survey components in the ToolspaceSurvey tab.
    
4.  Click OK.

Specify survey database settings

1.  In Toolspace, on the Survey tab, in the Survey Databases collection, right-click the **Survey 1** database. Click Edit Survey Database Settings.
2.  In the Survey Database Settings dialog box, under Precision, specify the following parameters:
    
    *   Angle: **4**
    *   Distance: **3**
    *   Level: **3**
    *   Coordinate: **4**
    *   Latitude And Longitude: **8**
    
    These precision settings are independent of the Drawing Settings precision settings and affect all aspects of the user interface that displays the survey data.
    
3.  Under Least Squares Analysis Defaults, specify the following parameters:
    
    *   Network Adjustment Type: **3-Dimensional**
    *   Confidence Level: **99% confidence**
    *   Perform Blunder Detection: **Yes**
    
    Note:
    
    The Error Tolerance values specify the acceptable error values for the survey measurement. The values in this collection correspond to the units of measure that are specified in the Units collection.
    
4.  Click OK.

To continue this tutorial, go to [Exercise 4: Setting Survey Styles](GUID-32C72970-243F-441F-BA12-BBA974AFC29D.htm "In this exercise, you will review the survey network styles and create a figure style.").

**Parent topic:** [Tutorial: Survey Setup](GUID-0395EBF8-26C3-4E7B-B98B-81D501BAF73C.htm "This tutorial demonstrates how to access the survey functionality and define and manage the survey settings in Autodesk Civil 3D.")