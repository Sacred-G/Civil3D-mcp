---
title: "Exercise 1: Creating Feature Lines"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-44EDAD16-E9FE-41BD-96A8-A3E32844568A.htm"
category: "tutorial_creating_gradings"
last_updated: "2026-03-17T18:42:39.450Z"
---

                 Exercise 1: Creating Feature Lines  

# Exercise 1: Creating Feature Lines

In this exercise, you will use two different methods to create feature lines.

Feature lines can be either drawn with straight and curved feature line segments, or created from existing alignments or AutoCAD lines, arcs, polylines, or 3D polylines.

A feature line can be used as a grading baseline, but not as a target.

This exercise continues from the [Setting Up Grading Standards](GUID-2EF31254-D063-49AF-9DA2-FC4BC7B4647B.htm "This tutorial demonstrates how to adjust grading settings, criteria, and styles.") tutorial.

Create feature lines from AutoCAD objects

Note:

This exercise uses _Grading-1.dwg_ with the modifications you made in the previous tutorial, or you can open _Grading-2.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Click Home tab ![](../images/ac.menuaro.gif)Draw panel ![](../images/ac.menuaro.gif)Create Line ![](../images/GUID-8F046CD9-47F4-41BE-BBB5-FBDDC22BFAB9.png) Find. Draw a line from circle A to B to C.
3.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Feature Line drop-down ![](../images/ac.menuaro.gif)Create Feature Lines From Objects ![](../images/GUID-E3383358-E579-45FB-901B-1E1FE45910DA.png) Find.
4.  Click both lines, then right-click and click Enter to end the selection.
    
    In the Create Feature Lines dialog box, notice that you can assign a name to the feature lines as they are created. Naming significant feature lines can make it easier to assign targets when creating a corridor. For this exercise, you will assign names after the feature lines have been created. Following this workflow enables you to create many feature lines at the same time, but name only the most significant feature lines.
    
5.  In the Create Feature Lines dialog box, click OK.
    
    The lines are converted to feature lines and added to Site 1. You will assign level values to these feature lines later in this exercise. Next, you will draw a feature line and specify levels along the line.
    

Name the feature lines

1.  In the drawing, select feature lines AB and BC. Right-click. Click Apply Feature Line Names.
2.  In the Apply Feature Line Names dialog box, click ![](../images/GUID-12D15354-6E08-488F-B0E7-34B74F5FD373.png).
3.  In the Name Template dialog box, for Property Fields, select Next Counter. Click Insert. Click OK.
4.  In the Apply Feature Line Names dialog box, place the cursor at the beginning of the Name field. Enter **ABC**.
    
    The Name field should contain ABC <\[Next Counter\]>.
    
    Click OK.
5.  In Toolspace, on the Prospector tab, expand Sites![](../images/ac.menuaro.gif)Site 1. Select the Feature Lines collection.
    
    Notice that the two feature lines and their names are displayed in the Prospector list view.
    
    Next, you will draw a feature line and specify levels along the line.
    

Draw a feature line

1.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Feature Line drop-down ![](../images/ac.menuaro.gif)Create Feature Line ![](../images/GUID-F72D81BA-5DDF-4603-9214-575CBE022332.png) Find.
2.  In the Create Feature Lines dialog box, specify the following parameters:
    *   Name Check Box: **Selected**
    *   Name: **CDEF <\[Next Counter\]>**
    *   Style Check Box: **Selected**
    *   Style: **Ditch**
3.  In the Create Feature Lines dialog box, click OK.
4.  In the drawing window, snap to the end of the feature line inside circle C. When prompted, enter **688.00** as the level.
    
    You have entered a known value for the starting level. In the following steps, you will use the transition command to defer entering level values at intermediate points along the feature line.
    
5.  Stretch the feature line and click inside circle D. When prompted for an level, enter **T**.
6.  Stretch the feature line and click inside circle E. When prompted for an level, press Enter to accept Transition.
7.  Stretch the feature line and click inside circle F. When prompted for a transition, enter **SU** to use the level of the surface at that point.
8.  Note the surface level shown at the command line. Press Enter twice to accept the level and end the command.
9.  In Toolspace, on the Prospector tab, expand Sites![](../images/ac.menuaro.gif)Site 1. Select the Feature Lines collection.
    
    Notice that the three feature lines you created are displayed in the list view. You can use this box to edit the feature lines’ name, style, and layer, and view other properties of all the feature lines.
    

To continue this tutorial, go to [Exercise 2: Assigning Feature Line Levels](GUID-C8D3F575-8367-4B04-9537-D8868C948E59.htm "In this exercise, you will assign levels to the feature lines you created from AutoCAD lines in the previous exercise.").

**Parent topic:** [Tutorial: Creating Gradings](GUID-59CA5821-CC8F-499A-8F89-655B6D41CA0F.htm "This tutorial demonstrates how to create a feature line and how to gradient from the feature line.")