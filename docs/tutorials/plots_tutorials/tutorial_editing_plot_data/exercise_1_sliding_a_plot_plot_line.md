---
title: "Exercise 1: Sliding a Plot Plot Line"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-4B39807D-7EA5-4B1D-A19A-9A02DAFE822A.htm"
category: "tutorial_editing_plot_data"
last_updated: "2026-03-17T18:42:35.445Z"
---

                  Exercise 1: Sliding a Plot Plot Line  

# Exercise 1: Sliding a Plot Plot Line

In this exercise, you will resize a plot by sliding an attached plot line along the plot frontage.

Specify plot creation settings

1.  Open _Parcel-2A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
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

Slide a plot plot line

1.  In the Plot Layout Tools toolbar, click ![](../images/GUID-2DD16B12-054E-4DCD-98AE-CC962B2CE7A3.png)Slide Line - Edit.
2.  In the Create Plots – Layout dialog box, click OK.
    
    You are prompted to select the attached plot line to adjust.
    
    Note:
    
    An attached plot line is one that was created with the slide angle, slide direction, and swing line precise sizing tools available on the Plot Layout Tools toolbar. You can use the slide line edit command to edit only attached plot lines. You will learn how to edit lot lines created using other methods in [Exercise 3: Editing Plot Lot Line Geometry](GUID-D8DA7E02-1CDD-4C23-879B-ED8344F732C0.htm "In this exercise, you will use the feature line editing tools to modify plot plot line geometry.").
    
3.  In the drawing, click the plot line that is between plot 108 and plot 109.
4.  Select the plot to adjust by moving the cursor over property 109. The plot borders are highlighted. Click inside the plot.
    
    ![](../images/GUID-32AE14A2-1BC9-4CE2-8994-591FF1ABA62A.png)
    
5.  Specify the plot frontage as shown in the following image.
    
    ![](../images/GUID-10A92B19-9AD4-4B94-B6BB-02FD04B705AC.png)
    
6.  Enter a frontage angle of **90**.
    
    Preview graphics of the plot creation parameters and proposed solution are displayed. The displayed solution slides the plot line along the frontage at the angle specified. The proposed solution encloses an area that meets the plot creation parameters you specified at the beginning of this exercise.
    
    ![](../images/GUID-0D1D0868-D077-47F0-B8DA-687D5B8BF2D5.png)
    
7.  Press Enter.
    
    ![](../images/GUID-76E0EB57-D811-4228-A334-1DD9CF2CBFE4.png)
    
    **Further exploration**: Use Steps 3 through 8 to move the plot lines that are between plots 108, 109, and 110 to match the plot layout of plots 101, 102, and 103.
    
    ![](../images/GUID-90846A2D-E283-40D0-847A-EE8F86DA6235.png)
    
8.  Press Esc to end the command.

To continue this tutorial, go to [Exercise 2: Swinging One End of a Plot Lot Line](GUID-D7B04F0C-45E6-410D-BB18-DDE98D80C18E.htm "In this exercise, you will resize a plot by swinging an attached plot line from a specified reference point.").

**Parent topic:** [Tutorial: Editing Plot Data](GUID-F66D1115-3033-45BD-B47E-A07E4187EDE8.htm "This tutorial demonstrates two ways of resizing a plot by moving a plot line.")