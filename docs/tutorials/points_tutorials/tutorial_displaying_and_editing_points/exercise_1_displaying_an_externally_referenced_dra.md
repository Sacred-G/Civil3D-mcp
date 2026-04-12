---
title: "Exercise 1: Displaying an Externally Referenced Drawing"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-A0C28000-DFC7-4246-BFF0-76EA43240823.htm"
category: "tutorial_displaying_and_editing_points"
last_updated: "2026-03-17T18:42:07.638Z"
---

                  Exercise 1: Displaying an Externally Referenced Drawing  

# Exercise 1: Displaying an Externally Referenced Drawing

In this exercise, you will use a standard AutoCAD operation to display another drawing of the region around your set of points.

This exercise continues from the [Creating Point Data](GUID-1A4FCB0E-F708-4AF5-B137-7942ECC3D819.htm "This tutorial demonstrates several useful setup tasks for organizing a large set of points.") tutorial.

Display an externally referenced drawing

Note:

This exercise uses Points-1a.dwg with the modifications you made in the previous tutorial, or you can open Points-2.dwg from the [tutorial drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Click Insert tab ![](../images/ac.menuaro.gif)Reference panel ![](../images/ac.menuaro.gif)![](../images/GUID-F914F3BF-E938-4DAC-8FF3-C41C72A3C2B1.png)Attach.
3.  In the Select Reference File dialog box, make sure that Files Of Type is set to Drawing (\*.dwg). Navigate to the [tutorial drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79) and open _Existing Basemap.dwg_. Select it and click Open.
4.  In the External Reference dialog box, specify the following parameters:
    *   Reference Type: **Overlay**
    *   Insertion Point: **Cleared**
    *   Scale: **Cleared**
    *   Rotation: **Cleared**
5.  Click OK.
    
    The basemap appears on the screen, allowing you to see the points of interest in relation to the road design and other contextual features. This external reference remains separate from your drawing. There is no risk of unexpected changes to your drawing. In a later exercise, you will learn how to detach the external reference.
    

To continue this tutorial, go to [Exercise 2: Changing the Style of a Point Group](GUID-59523BA0-4A5F-4F4F-BC5F-E4900A0CF589.htm "In this exercise, you will change the style of a point group. Point styles can help you distinguish the points more easily from other points in the drawing.").

**Parent topic:** [Tutorial: Displaying and Editing Points](GUID-E43EE235-038D-49EB-8307-6E8D00A9B3A4.htm "This tutorial demonstrates how to use point groups, layers, external references, and styles to display points. It also explains the various ways to edit points using standard AutoCAD tools.")