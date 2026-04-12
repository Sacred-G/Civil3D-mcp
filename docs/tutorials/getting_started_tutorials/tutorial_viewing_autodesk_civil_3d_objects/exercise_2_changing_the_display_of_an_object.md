---
title: "Exercise 2: Changing the Display of an Object"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6D99FE10-52B7-4D69-95C8-2EB36B3E43F1.htm"
category: "tutorial_viewing_autodesk_civil_3d_objects"
last_updated: "2026-03-17T18:42:05.616Z"
---

                 Exercise 2: Changing the Display of an Object  

# Exercise 2: Changing the Display of an Object

In this tutorial, you will change the appearance of a surface by changing its style. You will examine the style settings that affect how an object is represented in plan, profile, and model views.

This exercise continues from [Exercise 1: Setting Up the Drawing Window](GUID-56257D31-B605-4A5E-A09C-CB0A0A8B6943.htm "In this exercise, you will configure the drawing window, using named views and viewports.").

Modify the display of a surface

Note:

This tutorial uses _Intro-2.dwg_ from the previous tutorial.

2.  In Toolspace, on the Prospector tab, expand the tree under the drawing name. Expand the Surfaces collection to see the surface name **XGND**.
3.  Right-click the surface, **XGND**, and click Surface Properties.
4.  In the Surface Properties dialog box, on the Information tab, under Surface Style, select a different style, such as **Border & Levels**.
5.  Click Apply.
    
    The appearance of the surface now reflects the settings of the style you selected.
    
6.  To show a different view of the surface, repeat steps 2 through 4, selecting a different style.
7.  After you have explored other styles, set the original style, **Visualization**. Click Apply. Leave the Surface Properties dialog box open.

Examine the object style settings

1.  In the Surface Properties dialog box, on the Information tab, in the Default Styles area, notice the Render Material list.
    
    This list indicates the material that is applied to the surface object. When the surface is rendered in model view, the surface will be displayed using this material.
    
2.  Click Cancel.
3.  In the right viewport, zoom in to one of the profile grids. Select the blue, layout profile line. Right-click. Click Profile Properties.
4.  In the Profile Properties dialog box, on the Information tab, in the Object Style area, click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png).
5.  In the Profile Style dialog box, click the Display tab. In the View Direction list, make sure that Profile is selected.
    
    The table identifies how the profile components are displayed in a profile view. The basic appearance of the individual object components is controlled on this tab. Components that have ![](../images/GUID-6FD80C03-88AB-44CD-B810-0480DFACC30F.png) in the Visibility column are visible when the profile is displayed in a profile view. The Layer, Color, Linetype, LT Scale, and Lineweight of the components are controlled on this table. In the drawing window, notice that the layout profile line is blue, as identified in the Color column.
    
    Other Autodesk Civil 3D object styles use the same basic structure to control display components. Most other objects, such as alignments, have a Plan view direction in place of the Profile view direction. The Plan view direction identifies how the object components are displayed in plan view.
    
6.  In the View Direction list, select Model.
    
    In the table, notice that the Layer and Color settings are different from the Profile view direction. When the layout profile line is viewed in model, it uses the display settings listed in this table.
    
    Note:
    
    In the View Direction list, notice that a Section selection is available. This View Direction specifies how the surface will be displayed when it is viewed as part of a corridor section. You will learn about viewing and editing corridor sections in the [Viewing and Editing Corridor Sections tutorial](GUID-B5B31B1A-9F20-4FBD-8A92-DC665B053AFD.htm "This tutorial demonstrates how to edit a corridor in section.").
    
7.  Click Cancel to close the Profile Style and Profile Properties dialog boxes.
8.  Press Esc to deselect the layout profile.

To continue this tutorial, go to [Exercise 3: Viewing a Drawing in Model](GUID-FD2D299D-BF84-4590-961B-B2F74EA5CC75.htm "In this exercise, you will learn some ways to view drawing objects in three-dimensional views.").

**Parent topic:** [Tutorial: Viewing Autodesk Civil 3D Objects](GUID-4C8F58AF-E232-434A-A23C-C0C622F095D4.htm "This tutorial demonstrates several ways to display objects in plan and model views.")