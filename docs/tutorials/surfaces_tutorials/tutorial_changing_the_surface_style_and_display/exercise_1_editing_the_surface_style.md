---
title: "Exercise 1: Editing the Surface Style"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-0488B00F-71AA-484A-8A9E-E3AAFC53471D.htm"
category: "tutorial_changing_the_surface_style_and_display"
last_updated: "2026-03-17T18:42:11.441Z"
---

                 Exercise 1: Editing the Surface Style  

# Exercise 1: Editing the Surface Style

In this exercise, you will hide the display of the points on the surface and turn on the display of depression contours.

Depression contours form closed loops around areas of descending level. These are areas where lakes or ponds can form if the rainfall and soil conditions are right.

Edit the surface style

1.  Open _Surface-3.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the Active Drawing Settings View of the Toolspace, on the Settings tab, expand the Surface![](../images/ac.menuaro.gif)Surface Styles collection.
    
    This collection contains the existing surface styles in the drawing.
    
3.  The style (**Standard**) that is being referenced by a surface in the drawing is designated with an orange marker: ![](../images/GUID-D0A84661-1256-4EC4-8CF3-53BD2614B708.png)![](../images/GUID-527ADEAA-1B3E-4541-8FDB-89759B9A948F.png)
4.  Right-click the **Standard** surface style. Click Edit.
5.  In the Surface Style dialog box, click the Display tab.
6.  In the Component Display table, turn off the visibility of Points in the surface. To do this, click ![](../images/GUID-6FD80C03-88AB-44CD-B810-0480DFACC30F.png) in the Visible column. Click Apply.
7.  Click the Contours tab.
8.  Expand the Contour Depressions property group. Specify the following parameters:
    *   Display Depression Contours: True
    *   Tick Mark Length: **5**
9.  Click OK.
10.  Depression contours are now visible in the drawing, with tick marks along their length.

To continue this tutorial, go to [Exercise 2: Using a Different Style for a Surface](GUID-CF4D46E2-4F87-4022-8759-40A47A2DFA2C.htm "In this exercise, you will change the surface style, which the surface is referencing, to display different views of the surface.").

**Parent topic:** [Tutorial: Changing the Surface Style and Display](GUID-08661740-9006-422C-AD84-8060BBE49593.htm "This tutorial demonstrates how to change and constrain the surface styles and display.")