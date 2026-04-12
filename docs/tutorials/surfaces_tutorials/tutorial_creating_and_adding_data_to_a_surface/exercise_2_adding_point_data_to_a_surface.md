---
title: "Exercise 2: Adding Point Data to a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E6432405-A737-489B-97DC-2555D21FF183.htm"
category: "tutorial_creating_and_adding_data_to_a_surface"
last_updated: "2026-03-17T18:42:09.970Z"
---

                  Exercise 2: Adding Point Data to a Surface  

# Exercise 2: Adding Point Data to a Surface

In this exercise, you will import point data from a text file into the current drawing.

This exercise continues from [Exercise 1: Creating a New TIN Surface](GUID-34D24CED-7589-4E07-9AC1-E625834FD1D6.htm "In this exercise, you will create an empty TIN surface in a new drawing.").

Import point data into the current drawing

1.  Open drawing _Surface-1A.dwg_ , which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains an empty surface definition, which is named EG.
    
2.  Click Modify tab ![](../images/ac.menuaro.gif)Ground Data panel ![](../images/ac.menuaro.gif)Surface ![](../images/GUID-1159D4DC-9E2E-412A-891C-C68B17476BBE.png) Find.
3.  Click Surface tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)![](../images/GUID-7D57421A-5F53-439C-BB54-70E51EBF2F85.png)Add Data![](../images/ac.menuaro.gif)Point Files ![](../images/GUID-E4AF00EA-4891-4948-8EB7-A848022AD094.png) Find.
4.  Under Selected Files, click ![](../images/GUID-7633B109-B0E2-42D8-8A07-E27BBF28B731.png).
5.  In the Select Source File dialog box, browse to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC). Select _Surface-1A-PENZD (space delimited).txt_. Click Open.
6.  In the Add Point File dialog box, under Specify Point File Format, select PENZD (Space Delimited).
7.  In the Add Point File dialog box, click OK.
8.  At the command line, enter **ZE**.
    
    The surface, which contains the imported point data, is displayed in the drawing.
    
    ![](../images/GUID-9D1C56AA-7C96-40F6-92AA-F592F492859D.png)
    

To continue this tutorial, go to [Exercise 3: Adding Breaklines to a Surface](GUID-9CC1D50D-1542-4CD3-8136-D6E512D5C64E.htm "In this exercise, you will cause the surface to triangulate along a linear feature.").

**Parent topic:** [Tutorial: Creating and Adding Data to a Surface](GUID-899731B5-0B6A-451E-9CF2-0DCF00FA9B64.htm "This tutorial demonstrates how to create a TIN surface, and then add contour, breakline, and boundary data to the surface.")