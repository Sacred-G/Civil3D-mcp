---
title: "Exercise 2: Exporting Plot Data"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-71BE0BAB-A6B8-4660-9A2D-B99450253D33.htm"
category: "tutorial_displaying_and_analyzing_plots"
last_updated: "2026-03-17T18:42:36.696Z"
---

                   Exercise 2: Exporting Plot Data  

# Exercise 2: Exporting Plot Data

In this exercise, you will generate a mapcheck report for the plots in the residential road at the top right of the drawing.

In Autodesk Civil 3D you can export inverse or mapcheck reports for either a single plot, a series of plots, or all plots in a site.

This exercise continues from [Exercise 1: Changing Plot Style Display Order](GUID-77764590-BE98-47AE-B080-723776215C3B.htm "In this exercise, you will control the display of overlapping plot lines.").

To export plot data

Note:

This exercise uses _Plot-3A.dwg_ with the modifications you made in the previous exercise.

2.  On the ToolspaceProspector tab, expand Sites![](../images/ac.menuaro.gif)Site 1![](../images/ac.menuaro.gif)Plots.
3.  In the list view below the Prospector tree, click the heading of the Number column to sort the plots by number.
4.  Select plot numbers **120** through **136**. Plot number 136, STANDARD: 131, represents the road on which the plots are located. Ctrl+click plot number **136** in the list view to deselect it and exclude it from the analysis.
5.  In the list view, right-click and select Export Analysis.
6.  In the Export Plot Analysis dialog box, specify the following parameters:
    *   Destination File: Click ![](../images/GUID-8ABD5C87-AE6A-4567-8E38-50532CFB268B.png) and navigate to the [My Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Name the file _ExportAnalysis.txt_.
    *   Analysis Type: Mapcheck Analysis
7.  Click OK.
    
    The _ExportAnalysis.txt_ file automatically opens in the text editor associated with Autodesk Civil 3D and is saved in the location you specified in Step 5.
    
    Note:
    
    If the file does not automatically open, open it from the [My Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832).
    

**Parent topic:** [Tutorial: Displaying and Analyzing Plots](GUID-5655E551-4752-494C-8856-B8069B389658.htm "This tutorial demonstrates using plot styles and display order to control the appearance of plots, and exporting reports to analyze plot data.")