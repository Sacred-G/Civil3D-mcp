---
title: "Exercise 2: Creating Grading Criteria"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-FDA1CD38-3AE4-48E8-A976-DF2AD16F6611.htm"
category: "tutorial_setting_up_grading_standards"
last_updated: "2026-03-17T18:42:38.423Z"
---

                 Exercise 2: Creating Grading Criteria  

# Exercise 2: Creating Grading Criteria

In this exercise, you will create a grading criteria set for a ditch grading, then create three criteria in the set.

Each grading criteria provides a standard formula for creating a gradiented slope. Individual criteria values can be locked so that they cannot be changed for individual gradings.

This exercise continues from [Exercise 1: Reviewing Grading Settings](GUID-3F24CCD5-F41C-4E78-9D08-E964EEC466D5.htm "In this exercise, you will learn how to use grading settings.").

Create a criteria set

Note:

This exercise uses _Grading-1.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Settings tab, expand the Grading collection.
3.  Expand the Grading Criteria Sets collection.
    
    The Grading Criteria Sets collection displays the existing grading sets for the drawing. You will create a new grading criteria set then create two new criteria within the new set.
    
4.  Right-click Grading Criteria Sets. Click New.
5.  In the Grading Criteria Set Properties dialog box, enter **Ditch Criteria Set** in the Name field, and optionally enter a description.
6.  Click OK.
    
    The new criteria set is displayed in the Grading Criteria Sets collection.
    

Create a grading criteria

1.  Right-click **Ditch Criteria Set**. Click New.
2.  In the Grading Criteria dialog box, click the Information tab.
3.  In the Name field, enter **Distance @ -6%**.
    
    This criteria creates a gradient to a distance of 10 feet at -6% gradient.
    
4.  Click the Criteria tab and specify the following parameters:
    *   Target: **Distance**
    *   Distance: **10.000’**
    *   Projection: **Gradient**
    *   Format: **Gradient**
    *   Gradient: **\-6.000%**
5.  For the Gradient parameter, click ![](../images/GUID-8E80C8A9-C81E-4982-8D81-84D255AE57FC.png) to change it to ![](../images/GUID-1117B673-7D7A-4734-B746-CE3DD5292560.png). When you lock a gradient value in a grading criteria, you are not prompted to specify its value each time you use the criteria.
6.  Click OK.

Create a second grading criteria

1.  Create a second criteria by repeating the previous procedure. However, use the name **Surface @ 4-1 Gradient** and set the following values for the criteria:
    
    *   Target: **Surface**
    *   Projection: **Cut/Fill Gradient**
    *   Search Order: **Cut First**
    
    Set the following values for both the Cut Gradient Projection and Fill Gradient Projection property groups:
    
    *   Format: **Gradient**
    *   Gradient: **4:1**
    
    This criteria creates a gradient to an existing surface at a 4-to-1 gradient.
    
2.  Click OK to close the Grading Criteria dialog box.

Create a third marking criteria

1.  Create a third criteria by repeating the preceding procedure. However, use the name **Relative Level @ 3-1 Slope** and set the following values for the criteria:
    
    *   Target: **Relative Level**
    *   Relative Level: -**3.0’**
    *   Projection: **Gradient**
    *   Format: **Gradient**
    *   Gradient: **3:1**
    
    This criteria creates a gradient to a relative level of –3 feet at a 3-to-1 gradient.
    

To continue this tutorial, continue to [Exercise 3: Creating Grading Styles](GUID-23A65C6B-9E99-4B3A-BFAF-AB8A6F7657F2.htm "In this exercise, you will create a new grading style and gradient marking.").

**Parent topic:** [Tutorial: Setting up Grading Standards](GUID-2EF31254-D063-49AF-9DA2-FC4BC7B4647B.htm "This tutorial demonstrates how to adjust grading settings, criteria, and styles.")