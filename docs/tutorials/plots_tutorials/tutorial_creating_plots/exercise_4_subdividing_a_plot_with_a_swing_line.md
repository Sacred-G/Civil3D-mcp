---
title: "Exercise 4: Subdividing a Plot with a Swing Line"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-F5939D84-6C91-4565-AD22-E52FF9EFF884.htm"
category: "tutorial_creating_plots"
last_updated: "2026-03-17T18:42:34.511Z"
---

                 Exercise 4: Subdividing a Plot with a Swing Line  

# Exercise 4: Subdividing a Plot with a Swing Line

In this exercise, you will subdivide a plot with a segment that swings from a reference point on the back line.

This exercise continues from [Exercise 3: Subdividing a Plot with a Slide Line](GUID-35AC8372-274B-4EC0-927E-29B2B8C9D614.htm "In this exercise, you will successively subdivide a plot with segments that are defined by their angle at the frontage.").

Specify plot creation settings

1.  Open _Parcel-1D.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Plot drop-down ![](../images/ac.menuaro.gif)Plot Creation Tools ![](../images/GUID-171C575A-5154-4F3B-9F6D-4716539B73AE.png) Find.
3.  On the Plot Layout Tools toolbar, click ![](../images/GUID-1E5D0577-3361-4904-8C59-5B4823488488.png).
4.  Specify the following parameters:
    
    As you specify each parameter, notice that a preview graphic is displayed at the bottom of the Plot Layout Tools window.
    
    Plot Sizing
    
    Automatic Layout
    
    *   Minimum Area: **8000.00**
    *   Minimum Frontage: **40.0000**
    *   Use Minimum Frontage At Offset: **Yes**
    *   Frontage Offset: **20.0000**
    *   Minimum Width: **40.0000**
    *   Minimum Depth: **50.0000**
    *   Use Maximum Depth: **Yes**
    *   Maximum Depth: **200.0000**
    *   Multiple Solution Preference: **Use Smallest Area**
    *   Automatic Mode: **Off**
    *   Remainder Distribution: **Place Remainder In Last Plot**

Create plots using a swing line

1.  On the Plot Layout Tools toolbar, click ![](../images/GUID-895A3A3B-A406-452C-807E-DFD7AB972CC3.png)Swing Line – Create.
2.  In the Create Plots – Layout dialog box, for Plot Style, select **Single-Family**. Leave other settings at their default values. Click OK.
3.  In the drawing, select the area label of the large plot in the northeast corner of the site.
4.  Specify the start and end points of the plot frontage as shown in the following image.
    
    Notice that when you move the cursor to specify the end point, a yellow line displays the proposed frontage.
    
    ![](../images/GUID-C8C823EA-4FA3-4968-B0B8-3E2B2C54CB0F.png)
    
5.  Click the northeast corner of the plot to specify the swing point.
    
    A preview graphic is displayed.
    
    ![](../images/GUID-FE7965E5-64A9-43FE-A721-9B3FC1C16D6E.png)
    
6.  Press Enter to create the plot.
    
    The new plot is created and labeled.
    
    ![](../images/GUID-693D1650-3906-409F-904C-1EB05FB5BC2E.png)
    
7.  Press Esc to end the command.

To continue this tutorial, go to [Exercise 5: Working with Alignments and Plots](GUID-AB94147B-E621-4D47-912D-91971CAC2F9E.htm "In this exercise, you will create an alignment outside of a site and move existing alignments out of sites. These practices eliminate unwanted plots being created by alignments interacting with a site.").

**Parent topic:** [Tutorial: Creating Plots](GUID-29D40831-99A7-4CC4-BB29-926433D210C5.htm "This tutorial demonstrates the main methods for creating plots.")