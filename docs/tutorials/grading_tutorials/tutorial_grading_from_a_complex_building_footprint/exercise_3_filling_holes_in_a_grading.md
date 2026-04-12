---
title: "Exercise 3: Filling Holes in a Grading"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6BAC6196-EF89-42F5-AC1C-C72C79F7B145.htm"
category: "tutorial_grading_from_a_complex_building_footprint"
last_updated: "2026-03-17T18:42:42.000Z"
---

                 Exercise 3: Filling Holes in a Grading  

# Exercise 3: Filling Holes in a Grading

In this exercise, you will create infill gradings to fill in the open areas inside the grading group.

An _infill grading_ has no criteria applied to it. Any area bounded by feature lines or plot lines that is not already a grading can be converted to an infill grading. Using infill gradings to fill in holes in a grading group ensures correct contour display and volume calculations.

This exercise continues from [Exercise 2: Grading from a Building Footprint to a Surface](GUID-66A515E5-E405-48B5-B873-4C948CA30A82.htm "In this exercise, you will gradient from the simplified, offset footprint to the existing ground surface.").

Create infill gradings

Note:

This exercise uses _Grading-6.dwg_ with the modifications you made in the previous exercise.

2.  On the Grading Creation Tools toolbar, from the Select A Grading Criteria list, select **Gradient To Distance**.
    
    Note:
    
    Although an infill has no criteria, you select its grading style from an existing criteria.
    
3.  Click ![](../images/GUID-1E5D0577-3361-4904-8C59-5B4823488488.png)Expand The Toolbar.
4.  From the Style list, select **Shoulder**.
    
    This setting specifies the grading style to apply to the infill.
    
5.  Click ![](../images/GUID-E432BF06-1353-4276-B49F-201C93D54934.png)Create Infill.
6.  Click in the area between the building footprint and the offset feature line (near the right side of the ramp).
    
    A diamond is displayed in that area, indicating that an infill has been created. In the left viewport, notice that the infill grading fills the entire area between the two blue feature lines.
    
7.  On the Grading Creation Tools toolbar, from the Style list, select **Pad**.
8.  Click inside the building footprint to create an infill using the Pad grading style.
9.  Press Enter to end the command.
    
    In the left viewport, the gray area is the infill grading that represents the shoulder. The gold area is the building pad infill grading. The green and red areas are the gradient-to-surface slope grading.
    
    ![](../images/GUID-93F65EA3-44AD-473D-B422-7BBCB8867892.png)
    

To continue to the next tutorial, go to [Using Feature Lines to Modify a Grading](GUID-CB2B6E14-14BA-4EB9-A88B-90E315257D45.htm "This tutorial demonstrates how to use feature lines to control grading around inside corners.").

**Parent topic:** [Tutorial: Grading from a Complex Building Footprint](GUID-5CC7A47C-5448-40FF-83D6-4057E3AB143C.htm "This tutorial demonstrates how to gradient around a building footprint that has relatively complicated geometry and variations in level.")