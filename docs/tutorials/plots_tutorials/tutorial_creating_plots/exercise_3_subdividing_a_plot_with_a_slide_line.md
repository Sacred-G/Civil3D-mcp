---
title: "Exercise 3: Subdividing a Plot with a Slide Line"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-35AC8372-274B-4EC0-927E-29B2B8C9D614.htm"
category: "tutorial_creating_plots"
last_updated: "2026-03-17T18:42:34.347Z"
---

                 Exercise 3: Subdividing a Plot with a Slide Line  

# Exercise 3: Subdividing a Plot with a Slide Line

In this exercise, you will successively subdivide a plot with segments that are defined by their angle at the frontage.

This exercise continues from [Exercise 2: Subdividing a Plot with a Free-Form Segment](GUID-658E7243-C03C-4A33-8434-244FB2A44D76.htm "In this exercise, you will successively subdivide a plot with segments that can be placed along any plot line.").

Specify plot creation settings

1.  Open _Parcel-1C.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Plot drop-down ![](../images/ac.menuaro.gif)Plot Creation Tools ![](../images/GUID-171C575A-5154-4F3B-9F6D-4716539B73AE.png) Find.
3.  On the Plot Layout Tools toolbar, click ![](../images/GUID-1E5D0577-3361-4904-8C59-5B4823488488.png).
4.  Specify the following parameters:
    
    As you specify each parameter, notice that a preview graphic is displayed at the bottom of the Plot Layout Tools window.
    
    Plot Sizing
    
    Automatic Layout
    
    *   Minimum Area: **7000.00**
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
        
        Note:
        
        When Automatic Mode is set to Off, the Remainder Distribution setting does not affect plot layout. This setting will be used in later exercises.
        

Create individual plots using a slide line

1.  In the Plot Layout Tools toolbar, click ![](../images/GUID-C560B001-29E1-4F85-B5C1-7880FFB570C6.png)Slide Line – Create.
2.  In the Create Plots – Layout dialog box, for Plot Style, select **Single-Family**. Leave other settings at their default values. Click OK.
3.  Click the area label for plot **Single-Family: 101**.
4.  Specify the start and end points of the plot frontage as shown in the following image.
    
    Notice that when you move the cursor to specify the end point, a yellow line displays the proposed frontage.
    
    Note:
    
    When you specify the frontage starting point, do not snap to the beginning of the curve. If the frontage starts at the beginning point of the curve, no solution will be found.
    
    ![](../images/GUID-3219968D-066B-4E02-87C6-79595B6A347C.png)
    
5.  Enter a frontage angle of **90** degrees.
    
    Preview graphics of the plot creation parameters and proposed solution are displayed. The graphics are similar to those that were displayed while you were specifying plot creation parameters in the Plot Layout Tools window.
    
    ![](../images/GUID-9BE16A42-28AD-4673-BE62-611BD1C1BFF6.png)
    
6.  Press Enter.
    
    The new plot is created and labeled, and a preview of the next plot is displayed in the drawing.
    
7.  In the Plot Layout Tools dialog box, for Minimum Area, enter **8000**.
    
    Notice that the preview graphic updates to reflect the changed area value.
    
8.  Press Enter.
    
    Two new plots are created and labeled.
    
    ![](../images/GUID-CD3D0F29-DED9-4BEE-AC9D-1152675B1ADB.png)
    

Create multiple plots simultaneously

1.  In the Plot Layout Tools toolbar, click ![](../images/GUID-183753AC-EC56-4727-9972-4E9F25B1D42E.png)Slide Line – Create.
2.  Under Automatic Layout, specify the following parameters:
    *   Automatic Mode: **On**
    *   Remainder Distribution: **Place Remainder In Last Plot**
3.  Click the area label for the large plot on the south side of the site.
4.  Specify the start and end points of the plot frontage as shown in the following image.
    
    ![](../images/GUID-479762A9-00E4-4A78-B3F8-03465B6172F3.png)
    
5.  Enter a frontage angle of **90** degrees.
    
    A preview of the proposed plots is displayed.
    
    ![](../images/GUID-C214C210-6B10-4EEF-A5E6-FF55B50FFC63.png)
    
6.  Press Enter.
    
    The new plots are created and labeled.
    
    Note:
    
    You will correct the placement of some of the plot lines in the [Editing Plot Data](GUID-F66D1115-3033-45BD-B47E-A07E4187EDE8.htm "This tutorial demonstrates two ways of resizing a plot by moving a plot line.") tutorial.
    
    ![](../images/GUID-D16E2117-8FC5-47DA-A01D-05F3D69283FB.png)
    
7.  Press Esc to end the command.

To continue this tutorial, go to [Exercise 4: Subdividing a Plot with a Swing Line](GUID-F5939D84-6C91-4565-AD22-E52FF9EFF884.htm "In this exercise, you will subdivide a plot with a segment that swings from a reference point on the back line.").

**Parent topic:** [Tutorial: Creating Plots](GUID-29D40831-99A7-4CC4-BB29-926433D210C5.htm "This tutorial demonstrates the main methods for creating plots.")