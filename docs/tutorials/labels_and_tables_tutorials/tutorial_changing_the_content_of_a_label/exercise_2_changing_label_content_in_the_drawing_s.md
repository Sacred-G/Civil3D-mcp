---
title: "Exercise 2: Changing Label Content in the Drawing Settings"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-DD158750-0EA0-4AC4-8425-5BF67A44823C.htm"
category: "tutorial_changing_the_content_of_a_label"
last_updated: "2026-03-17T18:43:17.575Z"
---

                 Exercise 2: Changing Label Content in the Drawing Settings  

# Exercise 2: Changing Label Content in the Drawing Settings

In this exercise, you will change the default abbreviations that appear in geometry point labels.

This exercise continues from [Exercise 1: Overriding Label Text](GUID-86A6EA39-F7BF-47E3-B04D-F4D190741454.htm "In this exercise, you will override the text in a single label. Label text overrides are useful for adding text to an individual label to mark a point of interest without modifying all labels that share a style.").

Change label content in the drawing settings

Note:

This exercise uses _Labels-3a.dwg_ with the modifications you made in the previous exercise.

2.  Zoom and pan to the area between stations 0+000 and 0+080 of the West Street alignment.
3.  In Toolspace, on the Settings tab, right-click the drawing name. Click Edit Drawing Settings.
4.  In the Drawing Settings dialog box, click the Abbreviations tab.
    
    Note:
    
    In the Drawing Settings dialog box, you can use the Object Layers tab to change the default layer on which Autodesk Civil 3D objects and their labels are created.
    
    The Alignment Geometry Point Text category lists the abbreviations currently in use for each type of geometry point.
    
5.  In the Value column, change the geometry point abbreviation values to the following:
    *   Alignment End: **End**
    *   Straight-Curve Intersect: **Tan-Cur**
    *   Curve-Straight Intersect: **Cur-Tan**
6.  Click OK.
    
    The geometry point labels update to reflect the change in the drawing settings.
    
    Note:
    
    If the abbreviations in the geometry point labels have not updated to reflect the changes you made, enter **REGEN** on the command line.
    
    ![](../images/GUID-4E94A342-9F98-44E5-8E72-F20EF95EA36F.png)
    
    Geometry point labels with abbreviations modified in drawing settings
    
7.  Close the drawing.

To continue to the next tutorial, go to [Working with Tables and Tags](GUID-7052010C-3307-4A41-AFDB-39763F830C6B.htm "This tutorial demonstrates how to place object data into tables.").

**Parent topic:** [Tutorial: Changing the Content of a Label](GUID-EB73BA6C-81CD-4FD1-B6BD-40C2FA765EF8.htm "This tutorial demonstrates how to change label text content for an individual label and for a group of labels.")