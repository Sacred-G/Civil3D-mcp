---
title: "Exercise 2: Converting Labels to Tags"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-8B19DF3C-9CC3-4765-86A4-E422F2FD5A5F.htm"
category: "tutorial_working_with_tables_and_tags"
last_updated: "2026-03-17T18:43:18.550Z"
---

                  Exercise 2: Converting Labels to Tags  

# Exercise 2: Converting Labels to Tags

In this exercise, you will create some plot segment labels, and then convert the labels to tags and move the data into a table.

In [Exercise 1: Creating a Plot Area Table](GUID-16D213DA-AF50-4DA8-8FB3-F2007DB11BF9.htm "In this exercise, you will create a table to display information about plot objects."), you created a table in the externally referenced Labels-Plots drawing. In this exercise, you will create plot segment labels directly in the current drawing. You will then convert the segment labels to tags and create a table to display the detailed label information.

This exercise continues from [Exercise 1: Creating a Plot Area Table](GUID-16D213DA-AF50-4DA8-8FB3-F2007DB11BF9.htm "In this exercise, you will create a table to display information about plot objects.").

Label the plot segments

Note:

This exercise uses _Labels-4a.dwg_ with the modifications you made in the previous exercise.

2.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Labels menu ![](../images/ac.menuaro.gif)Plot![](../images/ac.menuaro.gif)Add Plot Labels![](../images/GUID-10094573-05A5-439E-A30D-2E506775AA08.png).
3.  In the Add Labels dialog box, specify the following parameters:
    *   Label Type: **Multiple Segment**
    *   Line Label Style: Plot Line Label Style![](../images/ac.menuaro.gif)**Bearing Over Distance**
    *   Curve Label Style: Plot Curve Label Style![](../images/ac.menuaro.gif)**Delta Over Length And Radius**
4.  Click Table Tag Numbering.
5.  In the Table Tag Numbering dialog box, examine the settings that are available.
    
    The settings specify the starting number and increment for line, curve, and transition tags. Notice that there are separate starting number and increment properties for both Table Tag Creation and Table Tag Renumbering. For this exercise, accept the default value of 1 for all properties. When you convert the labels to table tags, they will all use a starting number and an increment of 1. You will use the Table Tag Renumbering properties later.
    
6.  In the Table Tag Numbering dialog box, click Cancel.
7.  In the Add Labels dialog box, click Add.
8.  Click the plot area labels in the following order: **39**, **40**, **41**, **37**, **38**. Press Enter to accept the default Clockwise label direction.
    
    As you click, labels are placed on each plot segment. If you wish, zoom in to inspect the labels before you convert them to tags.
    
    ![](../images/GUID-96C702AC-E8AF-462D-911C-91D173631C09.png)
    
    Plot segment labels
    
9.  When you finish labeling plots, right-click to end the command.
    
    The Add Labels dialog box remains open, in case you want to label more plots or other objects. You can close it, as you will not use it again in this exercise.
    

Place the segment labels in a table

1.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Tables menu ![](../images/ac.menuaro.gif)Plot![](../images/ac.menuaro.gif)Add Segment.
    
    This option creates a table that shows both the line and curve segments of the labeled plots.
    
2.  In the Table Creation dialog box, in the Select By Label Or Style area, select the Apply check box for the two label styles you placed on the plot segments:
    *   Plot Curve: **Delta Over Length And Radius**
    *   Plot Line: **Quadrant Bearing Over Distance**
3.  Click OK.
    
    When you move the cursor into the drawing, the upper left corner of the table is attached to the cursor.
    
4.  Move the cursor outside the surface extents and click.
    
    The table is inserted into the drawing. Notice that the line and curve labels around the plots have been converted to tags.
    
    ![](../images/GUID-1D1A780D-78EA-468B-9ADA-C1A8D446425F.png)
    
    Plot segment labels converted to tags, with a plot segment table added to drawing
    

To continue this tutorial, go to [Exercise 3: Renumbering Table Tags](GUID-92032A66-357F-4241-8706-C1DB23075D81.htm "In this exercise, you will renumber the table tags you created in the previous exercise.").

**Parent topic:** [Tutorial: Working with Tables and Tags](GUID-7052010C-3307-4A41-AFDB-39763F830C6B.htm "This tutorial demonstrates how to place object data into tables.")