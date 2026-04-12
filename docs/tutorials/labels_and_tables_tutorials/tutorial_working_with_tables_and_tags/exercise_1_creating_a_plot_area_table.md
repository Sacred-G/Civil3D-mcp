---
title: "Exercise 1: Creating a Plot Area Table"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-16D213DA-AF50-4DA8-8FB3-F2007DB11BF9.htm"
category: "tutorial_working_with_tables_and_tags"
last_updated: "2026-03-17T18:43:18.423Z"
---

                  Exercise 1: Creating a Plot Area Table  

# Exercise 1: Creating a Plot Area Table

In this exercise, you will create a table to display information about plot objects.

You use a similar workflow to create tables for most Autodesk Civil 3D objects. In this exercise, you will learn about the table tools in Autodesk Civil 3D, and the dynamic nature of externally referenced drawings. You will add a plot area table to an externally referenced drawing, and then examine the results in the host drawing.

To create a table for an object, the object must be labeled. Most tables require that you specify the table data by selecting a label style. In this exercise, you will select plot area labels to create a plot area table. However, the plot and area labels exist in an externally referenced drawing. Plot area tables cannot be created through xref, so you cannot create the table in the current drawing.

Apply a simpler plot area label style

Note:

Before you begin this exercise, you must have the provided _Labels-_ drawings saved in the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). See the [Attaching Drawings as Xrefs for Annotation exercise](GUID-073E6BDC-DFAB-4D34-B3D1-3FB5CA40E54D.htm "In this exercise, you will attach several drawings to one drawing. By attaching drawings, you can annotate multiple large objects in a single compact drawing.") for more information.

2.  Open _Labels-4a.dwg_, which is located in the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832).
3.  Open _Labels-Parcels.dwg_, which is located in the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832).
4.  In _Labels-Plots.dwg_, zoom in so that you can see all of the plots adjoining East Street, which is the cul-de-sac alignment on the right-hand side of the drawing.
    
    You will apply a simpler area label style to plots 37 through 41, and then create a table that will display detailed plot data.
    
5.  In Toolspace, on the Prospector tab, expand Sites![](../images/ac.menuaro.gif)Site. Click the Plots collection.
6.  In the item view at the bottom of Toolspace, use Shift+click to select plots number 37 through 41. Right-click the Area Label Style column heading. Click Edit.
7.  In the Select Label Style dialog box, select **Plot Number** . Click OK.
    
    The plot area label consists of the plot number only. In the following steps, you will create a table that displays detailed information about each of these plots.
    

Create a plot area table

1.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Tables menu ![](../images/ac.menuaro.gif)Plot![](../images/ac.menuaro.gif)Add Area.
2.  In the Table Creation dialog box, in the Select By Label Or Style area, in the **Plot Number** row, select the Apply check box.
    
    You can select multiple styles from which to create the plot table. All the plots that use the selected styles will be shown in the table.
    
3.  Click OK.
    
    When you move the cursor into the drawing area, the upper left corner of the table is attached to the cursor.
    
4.  Move the cursor to the right of the plots and click.
    
    The table is inserted into the drawing.
    
5.  On the Quick Access toolbar, click ![](../images/GUID-753C7052-9AE5-46FF-A230-5E2920D3224C.png)Save.
6.  Close the Labels-Plots drawing.

Examine the results in the current drawing

1.  In drawing _Labels-4a_, on the command line, enter **XREF**.
2.  In the External References palette, right-click **Labels-Plots**. Click Reload.
    
    The current drawing is rebuilt using the updated data from the _Labels-Plots_ drawing. Notice that the plot area table you created in the externally referenced drawing is shown, and the plots at the end of the East Street alignment use the Plot Number area label style.
    
    ![](../images/GUID-59FCA6E9-F157-4D33-AE6B-BB4432727AB0.png)
    
    Plot area table added to an externally referenced drawing
    

To continue this tutorial, go to [Exercise 2: Converting Labels to Tags](GUID-8B19DF3C-9CC3-4765-86A4-E422F2FD5A5F.htm "In this exercise, you will create some plot segment labels, and then convert the labels to tags and move the data into a table.").

**Parent topic:** [Tutorial: Working with Tables and Tags](GUID-7052010C-3307-4A41-AFDB-39763F830C6B.htm "This tutorial demonstrates how to place object data into tables.")