---
title: "Exercise 3: Labeling a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-96A2EEE7-BB4B-45D0-95F1-73CD5CC15921.htm"
category: "tutorial_changing_the_surface_style_and_display"
last_updated: "2026-03-17T18:42:11.521Z"
---

                 Exercise 3: Labeling a Surface  

# Exercise 3: Labeling a Surface

In this exercise, you will add labels across surface contours. You will place individual labels manually and a series of labels automatically, using AutoCAD polylines as a guide.

This exercise continues from [Exercise 2: Using a Different Style for a Surface](GUID-CF4D46E2-4F87-4022-8759-40A47A2DFA2C.htm "In this exercise, you will change the surface style, which the surface is referencing, to display different views of the surface.").

Draw a polyline to use as a guide

Note:

This exercise uses _Surface-3.dwg_ with the modifications you made in the previous exercise.

2.  Click View tab ![](../images/ac.menuaro.gif)Named Views panel ![](../images/ac.menuaro.gif)views list ![](../images/ac.menuaro.gif)**Surface Labels**.
3.  On the command line, enter **PLine**.
4.  When prompted for a start point, click in the green circle in the upper left corner of the view.
5.  Click in the middle circle, then the lower right circle. Press Enter to end the PLine command.

Label surface contours along the polyline

1.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Labels menu ![](../images/ac.menuaro.gif)Surface![](../images/ac.menuaro.gif)Add Surface Labels![](../images/GUID-10094573-05A5-439E-A30D-2E506775AA08.png).
2.  In the Add Labels dialog box, set the Label Type to **Contour - Multiple**. Leave the other settings at their defaults. Click Add.
3.  On the command line, enter **O** to specify that you will select an object to use as a guide.
4.  On the command line, enter **Y** to delete the polyline after the labels have been created.
5.  In the drawing window, select the polyline. Press Enter to end the selection command.
    
    The labels are created along the path you specified with the polyline. This method of surface labeling is useful when you want to lay out the path of surface contour labels before you create the labels. If you wanted to create the path and labels simultaneously without first drawing a polyline, you would click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Labels menu ![](../images/ac.menuaro.gif)Surface![](../images/ac.menuaro.gif)Contour - Multiple![](../images/GUID-C0E4D439-A2C5-42FC-9C7B-57E2A4E3914C.png), then draw the path.
    
6.  In the drawing window, click the line on which the surface labels were drawn. Grips appear on the line.
7.  Select the grip in the circle at the upper left. It turns red, indicating that it is active.
8.  Drag the grip to a new location and click. Notice that the labels update automatically to reflect their new position.

Add spot level labels

1.  In the Add Labels dialog box, specify the following parameters:
    *   Label Type: **Spot Level**
    *   Spot Level Label Style: **Standard**
2.  Click Add. When prompted, click a point along the ridge to place a label.
3.  In the Add Labels dialog box, set the Spot Level Label Style to **Foot Meter**.
4.  Click a point along the ridge to place a label.
    
    Using the Add Labels dialog box, you can change label properties as needed while you create surface labels.
    
5.  Click Close.

To continue to the next tutorial, go to [Editing Surface Data](GUID-F8631D3F-ED47-4958-9637-70E7E9FD799F.htm "This tutorial demonstrates some common surface editing tasks, including edge swapping, TIN line deletion, and surface smoothing. You will also hide part of the surface using a hide boundary.").

**Parent topic:** [Tutorial: Changing the Surface Style and Display](GUID-08661740-9006-422C-AD84-8060BBE49593.htm "This tutorial demonstrates how to change and constrain the surface styles and display.")