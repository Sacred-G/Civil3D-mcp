---
title: "Exercise 1: Creating Plots from AutoCAD Objects"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-176618EA-6D47-4BF1-90F9-58F05F42F1A4.htm"
category: "tutorial_creating_plots"
last_updated: "2026-03-17T18:42:33.979Z"
---

                  Exercise 1: Creating Plots from AutoCAD Objects  

# Exercise 1: Creating Plots from AutoCAD Objects

In this exercise, you will convert AutoCAD elements to land plots with automatic labels that show useful topological data, such as land area.

Plot layout tools are available to create and edit plots with precision. You'll learn more about the Plot Layout tools in later Autodesk Civil 3D tutorial exercises.

![](../images/GUID-96B595A2-E9AA-4DAA-92A6-1957EDE78955.png)

Create plots from existing AutoCAD objects

1.  Open drawing _Plot-1A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains an existing ground surface, alignments that represent intersecting road centerlines, and AutoCAD lines and arcs that represent property boundaries. In the following steps, you'll create Autodesk Civil 3D plot objects from the existing lines and arcs.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Plot drop-down ![](../images/ac.menuaro.gif)Create Plot From Object ![](../images/GUID-4146A87E-D5A8-4183-AAA7-9528CAEBFC81.png) Find.
3.  Select the lines and the polyline border at the east side of the site, north of the First Street alignment. Press Enter.
    
    ![](../images/GUID-FE39559A-9787-4639-9615-A212A28FAF36.png)
    
    Note:
    
    On the command line, notice that a variety of AutoCAD objects can be used to create plots.
    
4.  In the Create Plots – From Objects dialog box, specify the following parameters:
    *   Site: **Site 1**
    *   Plot Style: **Single-Family**
    *   Area Label Style: **Plot Number And Area**
        
        Note:
        
        Plots must have area labels, but segment labels are optional.
        
    *   Automatically Add Segment Labels: **Selected**
    *   Erase Existing Elements: **Selected**
5.  Click OK.
    
    The plots are created and labeled. The labels annotate the overall plot area, as well as the quadrant bearing and distance of each line and curve segment. These labels are automatically updated if any line or curve segments are edited or deleted.
    
    The numbers with a circular border are automatically generated plot numbers. You will change these numbers to use a more desirable numbering convention.
    
    ![](../images/GUID-F0C8A6C9-BBBC-4E64-BE39-89789323D6E4.png)
    

Change the plot numbering

1.  Click a plot number to select it. Click Plot tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Renumber/Rename ![](../images/GUID-45AC5A01-21D9-4029-AAB4-9DDC3748F46F.png) Find.
2.  In the Renumber/Rename Plots dialog box, specify the following parameters:
    *   Renumber: **Selected**
    *   Starting Number: **101**
    *   Increment Value: **1**
3.  Click OK.
4.  To specify a start point, click to top-most plot.
5.  To specify an end point, click the bottom-most plot.
    
    ![](../images/GUID-FFC71238-4485-4F46-AADE-8306196C9780.png)
    
6.  Press Enter twice.
    
    The plot area labels are now numbered in ascending order, with three digits.
    
    ![](../images/GUID-48ED2DD8-F061-4F8D-B71C-3B6187ADAD53.png)
    

To continue this tutorial, go to [Exercise 2: Subdividing a Plot with a Free-Form Segment](GUID-658E7243-C03C-4A33-8434-244FB2A44D76.htm "In this exercise, you will successively subdivide a plot with segments that can be placed along any plot line.").

**Parent topic:** [Tutorial: Creating Plots](GUID-29D40831-99A7-4CC4-BB29-926433D210C5.htm "This tutorial demonstrates the main methods for creating plots.")