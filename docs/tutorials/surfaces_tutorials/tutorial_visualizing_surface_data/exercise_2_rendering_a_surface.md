---
title: "Exercise 2: Rendering a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-9087C6F6-9B87-4703-A987-64C92506A017.htm"
category: "tutorial_visualizing_surface_data"
last_updated: "2026-03-17T18:42:13.906Z"
---

                  Exercise 2: Rendering a Surface  

# Exercise 2: Rendering a Surface

In this exercise, you will use some of the visualization features in Autodesk Civil 3D to render a surface.

Like other Autodesk Civil 3D objects, you must apply a render material to the surface using the Surface Properties dialog box.

This exercise continues from [Exercise 1: Moving Multi-View Blocks to a Surface](GUID-D1BD85CB-AC39-4BE5-B3F9-1285B5115950.htm "In this exercise, you will insert multi-view blocks into a drawing, and then place them at the appropriate level on a surface.").

Apply a render material to the surface

Note:

This exercise uses _Surface-7.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Prospector tab, expand the Surfaces collection.
3.  Right-click the **XGND** surface. Click Surface Properties.
4.  In the Surface Properties dialog box, on the Information tab, specify the following parameters:
    *   Render Material: **Sitework.Planting.Sand**
        
        This render material displays contrast in the surface levels.
        
    *   Surface Style: **Standard**
5.  Click OK.

Apply a visual style to the surface

_Visual Styles_ give a fast, basic visualization of the surface that is useful for on-screen presentation in Autodesk Civil 3D.

2.  Click View tab ![](../images/ac.menuaro.gif)Visual Styles panel ![](../images/ac.menuaro.gif) Visual Styles drop-down ![](../images/ac.menuaro.gif)Realistic.
    
    This visual style shades the surface and smooths the edges between polygon faces. The render material that you applied to the surface is displayed.
    
3.  Click View tab ![](../images/ac.menuaro.gif)Visual Styles panel ![](../images/ac.menuaro.gif) Visual Styles drop-down ![](../images/ac.menuaro.gif)Conceptual.
    
    This visual style shades the surface and smooths the edges between polygon faces. The shading in this style uses the Gooch face style, a transition between cool and warm colors rather than dark to light. The effect is less realistic, but it can make the details of the model easier to see.
    

Render the surface

1.  On the command line, enter **RPREF**.
    
    Examine the many render settings that are available, including variations in image quality and output size. If you wanted to save the rendered image to a file, you would click ![](../images/GUID-6D5349F8-574A-4DA0-8F52-A8E33EB0ED38.png) and use the Output File Name control to specify a file name and destination.
    
2.  Click ![](../images/GUID-CE00D131-5A8F-4EEF-9699-8F92BAEA2361.png).
    
    The surface and blocks are rendered in the Render window. The effects of rendering are more apparent in a drawing that has different render materials applied to several surfaces and objects.
    
    ![](../images/GUID-E6222D28-B3D3-47AE-AB40-CEE1D5CC8146.png)
    

**Parent topic:** [Tutorial: Visualizing Surface Data](GUID-58270E7F-E5E0-4196-B194-4965255695F6.htm "This tutorial demonstrates how to add multi-view blocks to a surface and render it using a sample of the visualization techniques included with Autodesk Civil 3D.")