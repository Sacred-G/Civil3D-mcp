---
title: "Exercise 4: Extracting Objects from a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6A799F9A-6095-4A18-91EB-DF61A92EE6C0.htm"
category: "tutorial_creating_a_watershed_and_water_drop_analy"
last_updated: "2026-03-17T18:42:13.078Z"
---

                  Exercise 4: Extracting Objects from a Surface  

# Exercise 4: Extracting Objects from a Surface

In this exercise, you will use the watershed data to create non-destructive AutoCAD objects from the surface.

This exercise continues from [Exercise 3: Creating a Watershed Legend](GUID-47353C61-73BB-4EA6-B6BC-5AC7EB563868.htm "In this exercise, you will add a watershed legend table to the drawing.").

Extract objects from a surface

Note:

This exercise uses _Surface-5B.dwg_ with the modifications you made in the previous exercise.

2.  In the drawing, select the surface.
3.  Click TIN Surface tab ![](../images/ac.menuaro.gif)Surface Tools panel ![](../images/ac.menuaro.gif) Extract from Surface drop-down ![](../images/ac.menuaro.gif) Extract Objects ![](../images/GUID-09C3149D-DE54-47E0-AE5A-63F4C60162CE.png) Find.
4.  The Extract Objects From Surface dialog box lists all of the surface properties that are visible in the currently selected surface style. Clear all boxes in the Properties column except for Watersheds.
5.  Click OK.
    
    AutoCAD objects are created from each of the watersheds in the drawing.
    
6.  In the drawing, click inside a watershed area.
7.  On the command line, enter **List**.
    
    The AutoCAD text window displays parameters for the object you selected.
    
    You can use any of the standard AutoCAD commands to modify or query the new object.
    

To continue this tutorial, go to [Exercise 5: Analyzing Surface Water Runoff](GUID-0A52A24C-D6A9-4005-AF35-842A8309D167.htm "In this exercise, you will create lines that illustrate the path that flowing water would take across a surface. Then, you will create a polygon that defines the catchment region and its area on the surface.").

**Parent topic:** [Tutorial: Creating a Watershed and Water Drop Analysis](GUID-23EC67A6-0084-4DAC-B58F-141155DB7E29.htm "This tutorial demonstrates how to create two kinds of surface analysis: watershed and water drop.")