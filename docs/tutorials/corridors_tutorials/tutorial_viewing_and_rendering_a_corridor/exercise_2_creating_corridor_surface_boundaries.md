---
title: "Exercise 2: Creating Corridor Surface Boundaries"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-5F0C18E8-F32E-42C2-B381-551998705FF0.htm"
category: "tutorial_viewing_and_rendering_a_corridor"
last_updated: "2026-03-17T18:42:53.173Z"
---

                   Exercise 2: Creating Corridor Surface Boundaries  

# Exercise 2: Creating Corridor Surface Boundaries

In this exercise, you will use two different methods to define surface boundaries for your corridor design.

Use corridor surface boundaries to prevent triangulation outside of the daylight lines of a corridor surface. You may also use boundaries to either prevent an area of a surface from being displayed or to render an area of the corridor surface using a render material.

Corridor surfaces support the following types of boundaries:

*   **Outside Boundary** — Used to define the outer boundary of the corridor surface.
*   **Hide Boundary** — Used as a mask to create void areas or punch holes in the corridor surface. For example, a corridor might use a link code Paved either side of the corridor with another surface (a central reserve), separating them. When you create a corridor surface using Paved as the data, Autodesk Civil 3D tries to connect the gap in between two link codes. To create voids, you define boundaries to represent the surface appropriately.
*   **Render Only** — Used to represent different parts of corridor surface with different materials (when rendering), for example, asphalt and grass.

Note:

A Corridor Extents As Outer Boundary command is available for corridors that have multiple baselines, such as a corridor at a junction.

This exercise continues from [Exercise 1: Creating Corridor Surfaces](GUID-FA79D184-B3AC-4B97-B9E2-349C2909D03C.htm "In this exercise, you will create Top, Datum, Pave, and Central Reserve surfaces from the corridor.").

Create outside boundaries automatically

1.  Open _Corridor-5b.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the drawing, select the corridor.
    
    Tip:
    
    If you have difficulty selecting the corridor in the drawing, go to Toolspace on the Prospector tab. Expand the Corridors collection. Right-click the corridor name and click Select.
    
3.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor panel ![](../images/ac.menuaro.gif)Corridor Surfaces ![](../images/GUID-A679DF21-6B0C-4B6D-9244-6FF92F861387.png) Find.
4.  In the Corridor Surfaces dialog box, click the Boundaries tab.
    
    Four corridor surfaces are displayed in the boundary table.
    
5.  Select the **Corridor - (1) Top** surface. Right-click. Click Add Automatically![](../images/ac.menuaro.gif)Daylight.
    
    This creates a boundary from the daylight lines that are generated from the daylight point codes in the subassembly.
    
    Note:
    
    A Corridor Extents As Outer Boundary command is available for corridors that have multiple baselines, such as a corridor at a junction.
    
6.  Select the **Corridor - (1) Datum** surface. Right-click. Click Add Automatically![](../images/ac.menuaro.gif)Daylight.
7.  For both boundaries, make sure the Use Type is set to **Outside Boundary** .
    
    The daylight line in the corridor model is created at the points where the design surface matches the existing ground on each side. By selecting Outside Boundary, the surface will be clipped outside the boundary formed by the left and right daylight lines.
    
8.  Click OK.
    
    The new boundaries are added to the Corridor - (1) Top and Corridor- (1) Datum surfaces. The corridor model is regenerated and the surfaces are rebuilt.
    
    These surface boundaries are defined by a pair of feature lines. When there are more than two of a given type of feature lines, then you must use the interactive method to use them to define a boundary.
    
    For example, you were able to automatically create a surface boundary for the daylight region because there is a single pair of Daylight feature lines that define the daylight edges of the corridor assembly.
    
    By contrast, the assembly has two lanes, each of which are defined by its own pair of EPS feature lines. In this case, you must define the boundary interactively.
    

Create a pave outside boundary interactively

This boundary will define the outside edges of both lanes by using the lanes’ outer EPS feature lines. This will be an outside boundary to define the outside edges of the Corridor - (1) Pave surface.

2.  Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)Named Views list ![](../images/ac.menuaro.gif)Corridor\_Begin.
    
    The drawing is redrawn to a zoomed-in view of the starting area of Corridor (1).
    
3.  In the drawing, select the corridor.
4.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor panel ![](../images/ac.menuaro.gif)Corridor Surfaces ![](../images/GUID-A679DF21-6B0C-4B6D-9244-6FF92F861387.png) Find.
5.  In the Corridor Surfaces dialog box, on the Boundaries tab, select the **Corridor - (1) Pave** surface. Right-click. Click Add Interactively.
6.  In the drawing, select the feature line along the left-inside edge of the paved shoulder within circle 3.
7.  Since there are multiple feature lines at this location, the Select A Feature Line dialog box is displayed. Select **EPS**. Click OK.
8.  Pan to the other end of the corridor. Notice that a red line appears along the first feature line you selected.
9.  Select the feature line in circle 10.
10.  In the Select A Feature Line dialog box, select **EPS**. Click OK.
11.  Pan to the beginning of the corridor, and select the feature line along right-inside edge of paved shoulder within circle 4.
12.  On the command line, enter **C** to close the boundary.
13.  In the Corridor Surfaces dialog box, expand the **Corridor (1) – Pave** surface collection item to see the boundary item. Change the corridor boundary name to **Pave Outside** and set its Use Type to Outside Boundary.

Create a hide boundary interactively

This boundary will define the inside edges of both lanes by using the lanes’ inner EPS feature lines. This will be a _hide boundary_ and will act as a mask over the central reserve area of the Corridor - (1) Pave surface.

2.  In the Corridor Surfaces dialog box, on the Boundaries tab, select the **Corridor - (1) Pave** surface. Right-click. Click Add Interactively
3.  Repeat the previous procedure to define the inside boundary of the paved region:
    *   Click in circle 1 and select **EPS** to define the left-outside edge of the paved shoulder.
    *   Click in circle 8 and select **EPS** to define the left-outside edge of the paved shoulder.
    *   Click in circle 2.
    *   On the command line, enter **C** to close the boundary.
    *   Change the name of the boundary to **Pave Inside** .
    *   Change the **Use Type** to **Hide Boundary** .

Create a central reserve outside boundary interactively

This boundary will define the outside edges of the central reserve area using the lanes’ inner EPS feature lines. This will be an _outside boundary_ to define the outside edges of the Corridor - (1) Central Reserve surface.

2.  In the Corridor Surfaces dialog box, on the Boundaries tab, select the **Corridor - (1) Central Reserve** surface. Right-click and click Add Interactively
3.  Define the outside boundary of the central reserve:
    *   Click in circle 1 and select **EPS** to define the left-outside edge of the paved shoulder.
    *   Click in circle 8 and select **EPS** to define the left-outside edge of the paved shoulder.
    *   Click in circle 2.
    *   On the command line, enter **C** to close the boundary.
    *   Change the name of the boundary to **Central Reserve** .
    *   Change the **Use Type** to **Outside Boundary** .
4.  Click OK to create the boundaries and close the Corridor Properties dialog box.

To continue this tutorial, go to [Exercise 3: Visualizing a Corridor](GUID-6D746CF2-E33F-4964-BEEF-6589AB2F9287.htm "In this exercise, you will visualize the corridor using the rendering and hatching features in Autodesk Civil 3D.").

**Parent topic:** [Tutorial: Viewing and Rendering a Corridor](GUID-007C34DB-D0D8-4E35-B831-1B9E01857FEA.htm "This tutorial demonstrates how to add surfaces to a corridor, create boundaries on the surfaces, and then visualize the corridor using the AutoCAD rendering tools.")