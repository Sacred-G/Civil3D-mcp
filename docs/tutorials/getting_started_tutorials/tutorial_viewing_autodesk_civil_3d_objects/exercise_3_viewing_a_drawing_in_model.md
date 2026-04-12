---
title: "Exercise 3: Viewing a Drawing in Model"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-FD2D299D-BF84-4590-961B-B2F74EA5CC75.htm"
category: "tutorial_viewing_autodesk_civil_3d_objects"
last_updated: "2026-03-17T18:42:05.656Z"
---

                  Exercise 3: Viewing a Drawing in Model  

# Exercise 3: Viewing a Drawing in Model

In this exercise, you will learn some ways to view drawing objects in three-dimensional views.

This exercise continues from [Exercise 2: Changing the Display of an Object](GUID-6D99FE10-52B7-4D69-95C8-2EB36B3E43F1.htm "In this tutorial, you will change the appearance of a surface by changing its style. You will examine the style settings that affect how an object is represented in plan, profile, and model views.").

Examine object display in model views

Note:

This tutorial uses _Intro-2.dwg_ from the previous tutorial.

2.  Click the left viewport to make it active.
3.  Click View tab ![](../images/ac.menuaro.gif)Named Views panel ![](../images/ac.menuaro.gif) views list ![](../images/ac.menuaro.gif)SE Isometric.
    
    A Southeast isometric view of the surface is displayed in the left viewport, and the right viewport stays in plan view.
    
4.  Zoom in to the isometric view of the surface.
    
    Notice the green and blue lines. The green lines are the layout profiles that you examined in profile view. They are green because the Model view direction in its style indicated that they will display as green. The blue lines under the surface are the horizontal alignments from which the profiles were created.
    
    ## The SE Isometric 3D View
    
    ![](../images/GUID-E39E3F46-0F63-47CC-A09B-3E1B9D768713.png)
    

Change the visual style of the surface

1.  Click View tab ![](../images/ac.menuaro.gif)Visual Styles panel![](../images/GUID-A7096E57-563B-43D0-B57A-FA69853A76A6.gif)Visual Styles drop-down ![](../images/ac.menuaro.gif)3D Wireframe.
    
    AutoCAD _visual styles_ give a fast, basic visualization of an object that is useful for on-screen presentation in Autodesk Civil 3D. The 3D Wireframe visual style displays the surface in model view without applying a fill material to the object.
    
    ## The 3D Wireframe visual style
    
    ![](../images/GUID-B25A61EB-978B-429C-800A-3A3104ED8ACA.png)
    
    Notice that a cube is displayed in the upper right-hand corner. This is the AutoCAD ViewCube, which provides visual feedback of the current orientation of a model. You can use the ViewCube to adjust the viewpoint of the model when a visual style has been applied.
    
    ![](../images/GUID-3D711FCE-1000-4BD4-8514-85349A60A9F7.png)
    
2.  Click a corner of the ViewCube, and drag it to a new position. Experiment with dragging the ViewCube to various positions. When you are finished, click ![](../images/GUID-CC29D7EC-55D2-4918-8E32-97E1F0FF9D7F.png) to return the model and ViewCube to their original positions.
3.  Click View panel ![](../images/ac.menuaro.gif)Visual Styles panel![](../images/GUID-A7096E57-563B-43D0-B57A-FA69853A76A6.gif)Visual Styles drop-down ![](../images/ac.menuaro.gif)Conceptual.
    
    The Conceptual visual style shades the object and smooths the edges between polygon faces. The shading in this style uses the Gooch face style, a transition between cool and warm colors rather than dark to light. The effect is not realistic, but it can make the details of the model easy to see.
    
    ## The conceptual visual style
    
    ![](../images/GUID-634B1D6A-7824-499F-A54D-9EDF5290D714.png)
    
4.  Click View tab ![](../images/ac.menuaro.gif)Visual Styles panel ![](../images/GUID-A7096E57-563B-43D0-B57A-FA69853A76A6.gif)Visual Styles drop-down ![](../images/ac.menuaro.gif)Realistic.
    
    The Realistic visual style shades the surface and smooths the edges between polygon faces. The render material that is specified in the surface style is displayed.
    
    ## The realistic visual style
    
    ![](../images/GUID-583FC6BF-FBBC-4E2E-A70A-615A1B8E4F53.png)
    

**Parent topic:** [Tutorial: Viewing Autodesk Civil 3D Objects](GUID-4C8F58AF-E232-434A-A23C-C0C622F095D4.htm "This tutorial demonstrates several ways to display objects in plan and model views.")