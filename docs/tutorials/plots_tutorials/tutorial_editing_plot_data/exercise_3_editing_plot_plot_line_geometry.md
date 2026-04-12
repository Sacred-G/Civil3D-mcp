---
title: "Exercise 3: Editing Plot Plot Line Geometry"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-D8DA7E02-1CDD-4C23-879B-ED8344F732C0.htm"
category: "tutorial_editing_plot_data"
last_updated: "2026-03-17T18:42:35.741Z"
---

                 Exercise 3: Editing Plot Plot Line Geometry  

# Exercise 3: Editing Plot Plot Line Geometry

In this exercise, you will use the feature line editing tools to modify plot plot line geometry.

You will use two different methods to change the geometry of the two large plots at the end of the cul-de-sac.

First, you will learn about the grips that are available on plot lines. You will use plot line grips with the feature line tools to change the geometry of a plot.

Second, you will join two separate plot lines, and then remove a intersection point from the combined plot line.

This exercise continues from [Exercise 2: Swinging One End of a Plot Lot Line](GUID-D7B04F0C-45E6-410D-BB18-DDE98D80C18E.htm "In this exercise, you will resize a plot by swinging an attached plot line from a specified reference point.").

Add a intersection point to a plot plot line

1.  Open _Parcel-2C.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Select the back plot line that is shared by plots 105 and 106.
3.  Click Plot Segment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)![](../images/GUID-A33F63BF-62AF-4740-A40C-768E9318642F.png)Edit Geometry.
    
    The Edit Geometry panel is displayed in the ribbon.
    
4.  Click Plot Segment tab ![](../images/ac.menuaro.gif)Edit Geometry panel ![](../images/ac.menuaro.gif)![](../images/GUID-A33F63BF-62AF-4740-A40C-768E9318642F.png)Insert PI.
5.  Snap to the intersection of the back plot line and the plot line that separates plots 105 and 106. Click to insert a intersection point.
    
    ![](../images/GUID-1A9FDD73-2A14-4AA7-91A2-B0A371D82BB0.png)
    
6.  Press Enter to accept the default level of 0.
7.  Press Esc twice to end the command.
    
    The back plot line now has an intersection point at the point at which the plots meet. With a PI in this location, you can edit the plot line on one of the plots without affecting the other.
    

Grip edit a plot plot line

1.  In the drawing, select the plot line that separates plot 104 and 105.
    
    Notice the ![](../images/GUID-E9B35677-3419-4BC6-8F36-B8711DD058A0.png) grip on the plot line. This grip is available on attached plot lines, which are created with the slide angle, slide direction, and swing line precise sizing tools available on the Plot Layout Tools toolbar. You can use this grip to slide the plot line along the plot line to which it is attached.
    
2.  Press Esc to deselect the plot line.
3.  Select the back plot line that is shared by plots 105 and 106.
    
    Notice the ![](../images/GUID-88A18BA3-90FA-4D80-BC09-4FBA151261F4.png) grips on the ends of the plot line. These grips are available on plot lines that were created either from other Autodesk Civil 3D objects or the fixed line and curve tools available on the Plot Layout Tools toolbar. You can use these grips to change the endpoint location of a plot line.
    
4.  Click the grip at the top of the plot line. Drag the grip toward the plot line that separates plots 104 and 105. Snap to the intersection of the three plot lines. Click to place the grip.
    
    ![](../images/GUID-55FEF650-AF9A-4797-8B56-8D38BFCA31F9.png)
    
    Notice that the area of plot 105 has changed. However, there is now an unnecessary plot line remaining to the North of the plot. You will delete the unnecessary portion of that plot line in the following steps.
    

Trim an extraneous plot plot line

1.  Click Plot Segment tab ![](../images/ac.menuaro.gif)Edit Geometry panel ![](../images/ac.menuaro.gif)![](../images/GUID-3FCA3FED-76FC-4124-9A14-4051BA817BD8.png)Trim.
2.  Select the back plot line of plot 105 as the cutting edge. Press Enter.
3.  Select the plot line that extends past plot 105 as the object to trim.
    
    ![](../images/GUID-E3254CAE-57C8-4355-BFAF-C7E7122AD9DC.png)
    
4.  Press Enter to end the command.

Break a plot plot line

1.  Click Plot Segment tab ![](../images/ac.menuaro.gif)Edit Geometry panel ![](../images/ac.menuaro.gif)![](../images/GUID-D6E76EC9-FFA4-47C1-8F23-9ED59559C4A6.png)Break.
2.  Select the back plot line that is shared by plots 106 through 110.
3.  On the command line, enter F to specify the first point to break.
4.  In the drawing, snap to the intersection of the back plot line and the plot line that separates plots 107 and 108. Click the intersection.
    
    ![](../images/GUID-53E0FAE7-11B6-4190-BAE5-C2FECAA87CD4.png)
    
5.  Press Enter.
    
    Two plot lines are created, separated at the point you specified.
    

Join two plot plot lines

1.  Click Plot Segment tab ![](../images/ac.menuaro.gif)Edit Geometry panel ![](../images/ac.menuaro.gif)![](../images/GUID-1A8E6020-E25F-4E3E-8755-AF56DAD51CF5.png)Join.
2.  In the drawing, click the two plot lines that form the southeast corner of plot 106.
3.  Press Enter.
    
    The two plot lines are now a single element.
    

Delete an intersection point

1.  Click Plot Segment tab ![](../images/ac.menuaro.gif)Edit Geometry panel ![](../images/ac.menuaro.gif)![](../images/GUID-B21CF675-F1D5-48D6-AC67-E00F7408C245.png)Delete PI.
2.  In the drawing, select the plot line that forms the south and east boundaries of the plot.
3.  Click the green PI at the southeast corner of plot 108.
    
    ![](../images/GUID-3F0DB47A-2C6F-410E-A535-7A909888C253.png)
    
4.  Press Enter twice to end the command.

To continue to the next tutorial, go to [Displaying and Analyzing Plots](GUID-5655E551-4752-494C-8856-B8069B389658.htm "This tutorial demonstrates using plot styles and display order to control the appearance of plots, and exporting reports to analyze plot data.").

**Parent topic:** [Tutorial: Editing Plot Data](GUID-F66D1115-3033-45BD-B47E-A07E4187EDE8.htm "This tutorial demonstrates two ways of resizing a plot by moving a plot line.")