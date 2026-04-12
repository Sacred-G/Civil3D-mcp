---
title: "Exercise 1: Smoothing Feature Line Arcs"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E6D1CD7A-DD3F-44DC-A8A8-81A0D3BF4B57.htm"
category: "tutorial_using_feature_lines_to_modify_a_grading"
last_updated: "2026-03-17T18:42:43.043Z"
---

                 Exercise 1: Smoothing Feature Line Arcs  

# Exercise 1: Smoothing Feature Line Arcs

In this exercise, you will adjust the tessellation of the arcs around the ramp, which will result in a more accurate representation of the ramp.

Modify feature line arc tessellation

Note:

This exercise uses _Grading-6.dwg_ with the modifications you made in the previous exercise, or you can open _Grading-7.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Click the right viewport to make it active.
3.  Click View tab ![](../images/ac.menuaro.gif)Visual Styles panel ![](../images/ac.menuaro.gif)Visual Styles drop-down ![](../images/ac.menuaro.gif)3D Wireframe.
    
    The grading triangulation is displayed in the 3D Wireframe visual style. It is helpful to observe the triangles as you use the feature line to break the surface.
    
4.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Grading drop-down ![](../images/ac.menuaro.gif)Grading Creation Tools ![](../images/GUID-FCA44E9F-DE55-4BFD-9FFC-6D475EA0389E.png) Find.
5.  On the Grading Creation Tools toolbar, click ![](../images/GUID-3D15596D-5F55-49F7-AEDC-68E62741D31A.png)Grading Group Properties.
    
    Note: If the Select Grading Group dialog box is displayed, select the Grading - Building Pad site and the Slope Grading With Feature Lines group and click OK. Then click the Grading Group Properties icon again.
    
6.  In the Grading Group Properties dialog box, on the Information tab, select the Automatic Surface Creation check box.
7.  In the Create Surface dialog box, click OK.
    
    To adjust the arc tessellation, a surface must be created from the grading group. This surface is temporary. You will delete it after you have adjusted the arc tessellation.
    
8.  In the Grading Group Properties dialog box, on the Information tab, change the Tessellation Spacing setting to **1.000’**. Click Apply.
9.  Clear the Automatic Surface Creation check box. When asked if you want to delete the Slope - Projection Grading surface, click Yes. Click OK.
    
    Now that you have updated the arc tessellation, you must update the grading infill areas to apply the new setting. The easiest way to update the infill areas is by using the AutoCAD Move command.
    
10.  On the command line, enter **MOVE**.
11.  Click the original, interior feature line (the blue building pad). Press Enter.
12.  Press Enter again to select the default Displacement selection.
13.  Press Enter again to accept the default displacement of <0.0000, 0.0000, 0.0000>.
     
     In the left viewport, notice that the triangulation of the arcs along the ramp has improved.
     
     ![](../images/GUID-AB29457C-F021-4B2C-91B4-D4BC615035B1.png)
     

To continue this tutorial, go to [Exercise 2: Adjusting Grading Triangulation with a Feature Line](GUID-5653D269-9D07-48E6-B360-7307EFD5FD8F.htm "In this exercise, you will use a feature line to break a poorly triangulated grading surface.").

**Parent topic:** [Tutorial: Using Feature Lines to Modify a Grading](GUID-CB2B6E14-14BA-4EB9-A88B-90E315257D45.htm "This tutorial demonstrates how to use feature lines to control grading around inside corners.")