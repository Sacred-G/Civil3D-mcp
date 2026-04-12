---
title: "Exercise 2: Swinging One End of a Plot Plot Line"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-D7B04F0C-45E6-410D-BB18-DDE98D80C18E.htm"
category: "tutorial_editing_plot_data"
last_updated: "2026-03-17T18:42:35.581Z"
---

                  Exercise 2: Swinging One End of a Plot Plot Line  

# Exercise 2: Swinging One End of a Plot Plot Line

In this exercise, you will resize a plot by swinging an attached plot line from a specified reference point.

This exercise continues from [Exercise 1: Sliding a Plot Lot Line](GUID-4B39807D-7EA5-4B1D-A19A-9A02DAFE822A.htm "In this exercise, you will resize a plot by sliding an attached plot line along the plot frontage.").

Specify plot creation settings

1.  Open _Parcel-2B.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Plot drop-down ![](../images/ac.menuaro.gif)Plot Creation Tools ![](../images/GUID-171C575A-5154-4F3B-9F6D-4716539B73AE.png) Find.
3.  On the Plot Layout Tools toolbar, click ![](../images/GUID-1E5D0577-3361-4904-8C59-5B4823488488.png).
4.  Specify the following parameters:
    
    As you specify each parameter, notice that a preview graphic is displayed at the bottom of the Plot Layout Tools window.
    
    Plot Sizing
    
    Automatic Layout
    
    *   Minimum Area: **8000.00**
    *   Minimum Frontage: **40.0000**
    *   Use Minimum Frontage At Offset: **No**
    *   Frontage Offset: **20.0000**
    *   Minimum Width: **40.0000**
    *   Minimum Depth: **50.0000**
    *   Use Maximum Depth: **Yes**
    *   Maximum Depth: **200.0000**
    *   Multiple Solution Preference: **Use Shortest Frontage**
    *   Automatic Mode: **Off**
    *   Remainder Distribution: **Place Remainder In Last Plot**

Swing one end of a plot plot line

1.  On the Plot Layout Tools toolbar, click ![](../images/GUID-57E227F9-FB23-45F2-B8BE-BAB2A2D742E2.png)Swing Line – Edit.
2.  In the Create Plots – Layout dialog box, click OK.
    
    You are prompted to select the attached plot line to adjust.
    
3.  In the drawing, click the plot line that is between plot 104 and plot 105.
4.  Select the plot to adjust by moving the cursor over property 104 and clicking. Notice that the plot borders are highlighted.
    
    ![](../images/GUID-0937F9C8-87BB-44C5-8D2C-0ACC05B50E2F.png)
    
5.  Specify the plot frontage as shown in the following image.
    
    ![](../images/GUID-1A5A1FAC-FB83-40A5-8F7C-E229877A1365.png)
    
    Note:
    
    You must turn off OSNAP to perform the following step.
    
6.  Move the cursor to the approximate location in the following image.
    
    ![](../images/GUID-23C9B5CD-9A00-4D7D-A8F4-C68D7827D91B.png)
    
7.  Click to place the reference point.
    
    Preview graphics of the plot creation parameters and proposed solution are displayed. The displayed solution swings the plot line along the reference point. The proposed solution encloses an area that meets the plot creation parameters you specified at the beginning of this exercise.
    
    ![](../images/GUID-D3419588-C47B-4461-94F6-21EFAF5466F4.png)
    
8.  Press Enter.
    
    ![](../images/GUID-E298F2D9-BA6E-4DEE-B4C6-9AC299435531.png)
    
    **Further exploration**: Use Steps 3 through 8 to move the plot line that is between plots 106 and 107.
    
    ![](../images/GUID-92FE0326-6A3A-48D8-B8CE-E0F6A00A4FD9.png)
    
9.  Press Esc to end the command.

To continue this tutorial, go to [Exercise 3: Editing Plot Lot Line Geometry](GUID-D8DA7E02-1CDD-4C23-879B-ED8344F732C0.htm "In this exercise, you will use the feature line editing tools to modify plot plot line geometry.").

**Parent topic:** [Tutorial: Editing Plot Data](GUID-F66D1115-3033-45BD-B47E-A07E4187EDE8.htm "This tutorial demonstrates two ways of resizing a plot by moving a plot line.")