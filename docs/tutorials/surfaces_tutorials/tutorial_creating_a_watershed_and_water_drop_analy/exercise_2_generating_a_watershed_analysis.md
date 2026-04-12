---
title: "Exercise 2: Generating a Watershed Analysis"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-00728CAD-1FAB-46CA-BBCC-0728FADEC2DB.htm"
category: "tutorial_creating_a_watershed_and_water_drop_analy"
last_updated: "2026-03-17T18:42:12.995Z"
---

                 Exercise 2: Generating a Watershed Analysis  

# Exercise 2: Generating a Watershed Analysis

In this exercise, you will generate and display the watershed analysis.

This exercise continues from [Exercise 1: Configuring a Style for Watershed Display](GUID-677BCCE0-D9B9-40D4-B061-66768E370F83.htm "In this exercise, you will configure a style for watershed display.").

Generate a watershed analysis

Note:

This exercise uses _Surface-5A.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Prospector tab, expand the Surfaces collection. Right-click the **XGND** surface. Click Surface Properties.
3.  In the Surface Properties dialog box, on the Information tab, for Surface Style, select **Watersheds**.
4.  On the Analysis tab, for Analysis Type, select Watersheds.
5.  Ensure that **Standard** is selected in the Legend list.
6.  Click ![](../images/GUID-2BC20C50-B178-4028-B4BC-A441C59557E4.png) to generate the watershed analysis.
    
    The details of the surface watersheds are displayed in the Details table.
    
7.  Click ![](../images/GUID-BB4CA61F-6773-4E5F-BC10-76F6F765B4FD.png).
8.  On the Watershed Display dialog box, click ![](../images/GUID-6FD80C03-88AB-44CD-B810-0480DFACC30F.png) next to Boundary Point and Boundary Segment to turn off the display of these watershed types.
9.  Click OK twice.
    
    The watersheds are displayed on the surface in the drawing.
    
10.  In Toolspace, on the Prospector tab, expand the Surfaces![](../images/ac.menuaro.gif)**XGND** collection. Click the Watersheds collection.
     
     The Prospector list view displays a tabular list of the surface watersheds with their IDs, description, type, and the ID of the watershed that they drain into.
     
11.  Optionally, pan or zoom to an individual watershed. Right-click the watershed item in the list view and click Pan To or Zoom To.

To continue this tutorial, go to [Exercise 3: Creating a Watershed Legend](GUID-47353C61-73BB-4EA6-B6BC-5AC7EB563868.htm "In this exercise, you will add a watershed legend table to the drawing.").

**Parent topic:** [Tutorial: Creating a Watershed and Water Drop Analysis](GUID-23EC67A6-0084-4DAC-B58F-141155DB7E29.htm "This tutorial demonstrates how to create two kinds of surface analysis: watershed and water drop.")