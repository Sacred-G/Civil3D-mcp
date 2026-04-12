---
title: "Exercise 1: Creating a TIN Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-34D24CED-7589-4E07-9AC1-E625834FD1D6.htm"
category: "tutorial_creating_and_adding_data_to_a_surface"
last_updated: "2026-03-17T18:42:09.925Z"
---

                  Exercise 1: Creating a TIN Surface  

# Exercise 1: Creating a TIN Surface

In this exercise, you will create an empty TIN surface in a new drawing.

Create a TIN surface in a new drawing

1.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)New.
2.  In the Select Template dialog box, browse to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC). Select _Surface.dwt_. Click Open.
3.  Click Home tab ![](../images/ac.menuaro.gif)Create Ground Data panel ![](../images/ac.menuaro.gif)Surfaces drop-down ![](../images/ac.menuaro.gif)Create Surface ![](../images/GUID-60F68A3D-F1AD-4B0B-8052-9492D88FD36E.png) Find.
4.  In the Create Surface dialog box, for Type, select **TIN surface** .
    
    Note:
    
    By default, a new Surface Layer will be created named C-TOPO- followed by the name you enter in the Name cell. You can also click ![](../images/GUID-A7A402D0-F7F6-40A2-9CC6-106690F887CC.png) to specify an existing layer for the surface.
    
5.  In the Properties table, specify the following parameters:
    *   Name: **EG**
    *   Description: **Existing Ground surface from imported point data**
    *   Style: **Points and Border**
        
        Tip:
        
        To select the style, click the Value cell, and then click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png) to display the Select Surface Style dialog box.
        
    *   Render Material: **ByLayer**
6.  Click OK.
7.  In Toolspace, on the Prospector tab, expand the Surfaces collection.
    
    The new surface name is displayed in the ![](../images/GUID-6BA9AC63-A03A-493C-8716-35AD405BF1FC.png)Surfaces collection in the Master View of the Toolspace on the Prospector tab, but this surface doesn't contain any data.
    

To continue this tutorial, go to [Exercise 2: Adding Point Data to a Surface](GUID-E6432405-A737-489B-97DC-2555D21FF183.htm "In this exercise, you will import point data from a text file into the current drawing.").

**Parent topic:** [Tutorial: Creating and Adding Data to a Surface](GUID-899731B5-0B6A-451E-9CF2-0DCF00FA9B64.htm "This tutorial demonstrates how to create a TIN surface, and then add contour, breakline, and boundary data to the surface.")