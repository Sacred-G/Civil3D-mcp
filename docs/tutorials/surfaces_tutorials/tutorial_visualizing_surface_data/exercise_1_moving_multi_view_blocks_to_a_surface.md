---
title: "Exercise 1: Moving Multi-View Blocks to a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-D1BD85CB-AC39-4BE5-B3F9-1285B5115950.htm"
category: "tutorial_visualizing_surface_data"
last_updated: "2026-03-17T18:42:13.846Z"
---

                   Exercise 1: Moving Multi-View Blocks to a Surface  

# Exercise 1: Moving Multi-View Blocks to a Surface

In this exercise, you will insert multi-view blocks into a drawing, and then place them at the appropriate level on a surface.

A multi-view block is a Autodesk Civil 3D object that can have different representations in different view directions.

Predefined multi-view blocks supplied with Autodesk Civil 3D are available in DesignCentre. These blocks represent various items, such as signs, building footprints, trees, and shrubs. In DesignCenter, predefined multi-view blocks are located in the [Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24223) **\\Symbols\\Mvblocks**.

Insert multi-view blocks into the drawing

1.  Open _Surface-7.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)views list ![](../images/ac.menuaro.gif)**Plan Detail**.
    
    The drawing view shows a two-way road with a central reserve separating the lanes.
    
3.  Click View tab ![](../images/ac.menuaro.gif)Palettes panel ![](../images/ac.menuaro.gif)Design Center ![](../images/GUID-189DFBB5-7F84-47C0-99C9-E1087C1779FF.png) Find.
4.  In DesignCenter, navigate to the [Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24223) **\\Symbols\\Mvblocks**. Select the Mvblocks folder in the left pane.
    
    In the right pane, examine the blocks that are available.
    
    Note:
    
    Before continuing with this exercise, either dock the DesignCenter palette or click ![](../images/GUID-F99EDEF5-876D-4631-8256-B92FA79036C2.png) to autohide it.
    
5.  In the right pane, select _R4-7a Keep Right.dwg_. Right-click. Click Insert As Block.
6.  In the Insert dialog box, specify the following parameters:
    *   Insertion Point - Specify On Screen: **Selected**
    *   Scale - Specify On Screen: **Selected**
    *   Scale - Uniform Scale: **Selected**
    *   Rotation - Specify On Screen: **Selected**
    *   Explode: **Cleared**
7.  Click OK.
8.  When prompted to specify an insertion point in the drawing window, click the ![](../images/GUID-8FF21CF9-D3FB-4804-A0AC-322A6FC59F77.png) symbol at the bottom of the central reserve.
9.  Press Enter to accept the default scale factor of 1.
10.  When prompted to specify the rotation angle, zoom in to the insertion point and rotate the block until the sign is parallel with the symbol. Click to set the angle of rotation.
     
     ![](../images/GUID-225BB5F1-5267-451F-8856-A8BA6A6B021A.png)
     
11.  Repeat steps 6 and 7 to insert the _Light Pole 01.dwg_ block.
     
     When prompted to specify an insertion point in the drawing window, click the ![](../images/GUID-750D977F-7304-4240-971D-55C9533EF0DA.png) symbol near the middle of the central reserve. Press Enter to accept the default scale factor and rotation angle.
     

Move multi-view blocks to the surface

1.  Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)views list ![](../images/ac.menuaro.gif)**3D Detail**.
    
    In the 3D view, notice that the street light block appears at the lower portion of the drawing and is not at the appropriate surface level. In the following steps, you will move both blocks onto the surface.
    
2.  Click Modify tab ![](../images/ac.menuaro.gif)Ground Data panel ![](../images/ac.menuaro.gif)Surface ![](../images/GUID-1159D4DC-9E2E-412A-891C-C68B17476BBE.png) Find.
3.  Click Surface tab ![](../images/ac.menuaro.gif)Surface Tools panel ![](../images/ac.menuaro.gif) Move to Surface drop-down Move Blocks To Surface ![](../images/GUID-ABC173AC-4BC0-4CFD-B15E-FAD9C4FDA4CF.png) Find.
4.  In the Move Blocks To Surface dialog box, in the Select Block Reference Names field, select **Light Pole 01** and **R4-7a Keep Right**.
5.  Click OK. Each selected block moves from its current level to the surface level at the block’s insertion point.

To continue this tutorial, go to [Exercise 2: Rendering a Surface](GUID-9087C6F6-9B87-4703-A987-64C92506A017.htm "In this exercise, you will use some of the visualization features in Autodesk Civil 3D to render a surface.").

**Parent topic:** [Tutorial: Visualizing Surface Data](GUID-58270E7F-E5E0-4196-B194-4965255695F6.htm "This tutorial demonstrates how to add multi-view blocks to a surface and render it using a sample of the visualization techniques included with Autodesk Civil 3D.")