---
title: "Exercise 1: Editing the Grading Level"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6C1968B7-168D-4608-BBE1-1C03413E0CA0.htm"
category: "tutorial_editing_gradings"
last_updated: "2026-03-17T18:42:40.641Z"
---

                  Exercise 1: Editing the Grading Level  

# Exercise 1: Editing the Grading Level

In this exercise, you will edit the level of a grading baseline. The grading adjusts to reflect the level change.

Edit feature line levels

1.  Open _Grading-4.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a rectangular building pad gradiented to the surrounding surface at a 3:1 gradient. Where the gradient marking is green, it is a fill gradient in which the grading slopes down from the pad to the surface. Where the gradient marking is red, it is a cut gradient in which the grading slopes up from the pad.
    
2.  Click Modify tab ![](../images/ac.menuaro.gif)Edit Levels panel ![](../images/ac.menuaro.gif)Level Editor ![](../images/GUID-AC155D52-45FC-42D0-8E2D-9595F606FB0D.png) Find.
3.  In the drawing, select the baseline for the grading (rectangular line).
    
    Note:
    
    The surface style for the building pad grading has borders turned off, making it easier to select the baseline. Otherwise, the surface border would be on top of the baseline.
    
    The Level Editor shows the following grading settings for each corner of the feature line in a clockwise direction. The starting point is the upper left corner.
    
    *   Chainage — Distance from the start of the feature line.
    *   Level (Actual) — Level of the current point.
    *   Length — Distance to the next point.
    *   Gradient Ahead — Gradient toward the next point. Adjusting this setting holds the level of the current point and adjust the level of the next point for the new gradient.
    *   Gradient Back — Gradient from the previous point. Adjusting this setting affects the level of the current point, holding the level of the next point for the new gradient.
    
    **Further exploration:** Another way to review and edit the levels of a feature line is to click Modify tab ![](../images/ac.menuaro.gif)Edit Levels panel ![](../images/ac.menuaro.gif)Edit Levels ![](../images/GUID-11DD5EBC-8730-4477-8883-FBB3CA5445AC.png) Find. You can use this command to edit data on the command line.
    
4.  In the Grading Level Editor, Shift+click the third and fourth rows to select them.
5.  Double-click the level value in one of the rows and change it to **730** feet.
    
    Both of the selected rows’ level values change to 730 feet. Notice that the shape of the grading and the Gradient Ahead and Gradient Back values have changed to reflect the level change.
    

Edit feature line gradients

1.  Select the first three rows in the table. Click ![](../images/GUID-F8C0B433-DC00-48D7-BEF2-F0F99095A0CE.png)Flatten Gradient or Levels.
2.  In the Flatten dialog box, select Constant Gradient. Click OK.
    
    The first two points are set to the same gradient, and the shape of the grading changes in response to the level change. Flattening the gradient holds the level values of the first and last selected points and modifies levels of the points in between.
    
3.  Click ![](../images/GUID-51CF7EB5-5062-4231-AEC4-D527367E868A.png)Show Gradient Breaks Only.
    
    The second chainage’s row is hidden, because there was no difference in gradient between it and the previous chainage.
    
4.  In the first row, change the Gradient Ahead value to **\-3.000%**.
5.  Click ![](../images/GUID-51CF7EB5-5062-4231-AEC4-D527367E868A.png)Show Gradient Breaks Only.
    
    Notice that the second chainage’s gradient has changed to reflect the change you made in step 4.
    
6.  Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to close the Grading Level Editor.

To continue this tutorial, go to [Exercise 2: Balancing Cut and Fill Volumes](GUID-74F58D94-A79F-4C4C-B84D-D280CC3AF978.htm "In this exercise, you will adjust the level of a building pad to balance the cut and fill volumes.").

**Parent topic:** [Tutorial: Editing Gradings](GUID-F4CD7511-980F-4935-ABDD-9FB1C1967829.htm "This tutorial demonstrates common grading editing tasks, including level adjustment and grading criteria editing.")