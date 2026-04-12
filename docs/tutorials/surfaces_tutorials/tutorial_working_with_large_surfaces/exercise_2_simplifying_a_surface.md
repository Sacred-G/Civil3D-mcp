---
title: "Exercise 2: Simplifying a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-808DDFE2-424D-4B04-8A73-0AE3FF0F4B26.htm"
category: "tutorial_working_with_large_surfaces"
last_updated: "2026-03-17T18:42:10.823Z"
---

                 Exercise 2: Simplifying a Surface  

# Exercise 2: Simplifying a Surface

In this exercise, you will reduce the number of points that are used to define a surface.

A surface can be simplified by removing either TIN edges or points. When the simplify surface command is complete, new points and TIN edges are calculated based on specified parameters. The original surface points are still contained in the referenced point file, but are not used in the surface triangulation.

In this exercise, you will use the Point Removal method of simplifying a surface. This method randomly selects points from the surface, and removes them based on the point density at different areas of the surface. More points are removed from areas in which the concentration of points is very dense than from areas that contain fewer points.

Note:

You cannot specify which points to remove. Points that are used to define surface borders and breaklines are not removed with the Simplify Surface command.

This exercise continues from [Exercise 1: Limiting Imported Surface Data](GUID-E82C376A-5E40-4921-8299-92D1E8E3D8A0.htm "In this exercise, you will use a data clip boundary to restrict the quantity of points that is referenced by a surface.").

Simplify a surface

Note:

This exercise uses _Surface-2.dwg_ with the modifications you made in the previous exercise.

2.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer drop-down. Next to the **C-TOPO-CONT-MAJR-ORIG** and **C-TOPO-CONT-MINR-ORIG** layers, click ![](../images/GUID-F908C832-FEB1-4F3B-AB32-BC8C776F3D5F.png).
    
    These layers contains polylines that represent the original major and minor contours. These polylines will enable you to observe the results of the Simplify Surface command.
    
3.  Click Modify tab ![](../images/ac.menuaro.gif)Ground Data panel ![](../images/ac.menuaro.gif)Surface ![](../images/GUID-1159D4DC-9E2E-412A-891C-C68B17476BBE.png) Find.
4.  Click Surface tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Edit Surface drop-down ![](../images/ac.menuaro.gif)Simplify Surface ![](../images/GUID-737C2C92-F699-40FD-9690-C9E501913AA3.png) Find.
5.  In the Simplify Surface wizard, on the Simplify Methods page, select the Point Removal option.
6.  Click Next.
7.  On the Region Options page, specify the following parameters:
    *   Select Objects: **Selected**
    *   Mid-Ordinate Distance: **1.000’**
8.  Click Pick In Drawing.
9.  In the drawing, click the orange corridor boundary.
    
    In the Simplify Surface wizard, notice the value for Total Points Selected In Region. This is the current number of points in the selected region.
    
10.  Click Next.
11.  On the Reduction Options page, specify the following parameters:
     *   Percentage Of Points To Remove: **Selected**, **50%**
     *   Maximum Change In Level: **Cleared**
12.  Click Apply.
     
     At the bottom of the wizard, notice the Total Points Removed value. This value is the number of points that the simplify surface command removed within the selected boundary.
     
     Note:
     
     You can click Apply again to repeat the Simplify Surface command and keep the wizard open. If you click Finish, the Simplify Surface command is repeated and the wizard is closed.
     
13.  Click Cancel.
14.  Zoom in to the surface.
     
     Notice that the points are not as dense as they were at the beginning of the exercise, and the new, gray surface contours are very similar to the original contours. The Simplify Surface command reduced the amount of data that the surface uses without sacrificing much surface accuracy.
     
     ![](../images/GUID-FC98170C-7E03-4AF4-996B-997C59BF246E.png)
     
     Simplified surface
     

To continue to the next tutorial, go to [Changing the Surface Style and Display](GUID-08661740-9006-422C-AD84-8060BBE49593.htm "This tutorial demonstrates how to change and constrain the surface styles and display.").

**Parent topic:** [Tutorial: Working with Large Surfaces](GUID-750F4161-2F8F-47AA-AA39-8D5640884AE6.htm "This tutorial demonstrates several features that can help you manage large surfaces efficiently in Autodesk Civil 3D.")