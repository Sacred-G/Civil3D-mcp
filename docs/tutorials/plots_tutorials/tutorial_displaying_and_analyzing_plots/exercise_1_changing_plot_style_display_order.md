---
title: "Exercise 1: Changing Plot Style Display Order"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-77764590-BE98-47AE-B080-723776215C3B.htm"
category: "tutorial_displaying_and_analyzing_plots"
last_updated: "2026-03-17T18:42:36.557Z"
---

                  Exercise 1: Changing Plot Style Display Order  

# Exercise 1: Changing Plot Style Display Order

In this exercise, you will control the display of overlapping plot lines.

**Plot style display order** in Autodesk Civil 3D controls which plot lines are visible where different types overlap.

Change plot style display order

1.  Open _Parcel-3A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Prospector tab, expand the Sites![](../images/ac.menuaro.gif)Site 1 collection. Right-click the Plots collection. Click Properties.
    
    In the Plot Style Display Order area, note that Single-Family plots have the highest display order, which explains why their magenta lines cover the blue Standard lines.
    
    ![](../images/GUID-4BC74908-8E9B-4D37-B21A-E4EF9F54B6B9.png)
    
3.  In the Plot Style Display Order list, select **Standard** and click ![](../images/GUID-9939333B-1BF7-46F7-A036-61DD94A740CF.png) to move it to the top of the stack.
4.  Click Apply.
    
    After the model regenerates, notice that the blue Standard plot lines have overwritten the pink ones for Single-Family lots.
    
    ![](../images/GUID-4806B2B0-AE7B-4D22-89AB-FB6A6E1E15D2.png)
    
5.  Repeat steps 3 and 4, but give the **Property** style the highest display order.
    
    This setting displays a light blue line around the extents of the site.
    
    ![](../images/GUID-DF454CFC-13FA-4029-B4CC-42709BA22842.png)
    
    **Further exploration:** Change the display order again, moving the Road (Local) plots to the top of the display order, then moving them to a position between Standard and Single-Family. These settings change the display of the curved road edges.
    
6.  Click OK.

To continue this tutorial, go to [Exercise 2: Exporting Plot Data](GUID-71BE0BAB-A6B8-4660-9A2D-B99450253D33.htm "In this exercise, you will generate a mapcheck report for the plots in the residential road at the top right of the drawing.").

**Parent topic:** [Tutorial: Displaying and Analyzing Plots](GUID-5655E551-4752-494C-8856-B8069B389658.htm "This tutorial demonstrates using plot styles and display order to control the appearance of plots, and exporting reports to analyze plot data.")