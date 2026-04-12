---
title: "Exercise 3: Visualizing a Corridor"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6D746CF2-E33F-4964-BEEF-6589AB2F9287.htm"
category: "tutorial_viewing_and_rendering_a_corridor"
last_updated: "2026-03-17T18:42:53.355Z"
---

                   Exercise 3: Visualizing a Corridor  

# Exercise 3: Visualizing a Corridor

In this exercise, you will visualize the corridor using the rendering and hatching features in Autodesk Civil 3D.

_Rendering_ a corridor requires that you assign an AutoCAD _render material_ to each of the appropriate subassembly links. Rendering produces a realistic image of the corridor that is useful for on-screen presentations.

Applying _hatching_ to a corridor requires that you apply a _material area fill style_ to each of the appropriate subassembly links. Hatching produces a less realistic image of the surface than rendering, but hatching prints easily through AutoCAD.

This exercise continues from [Exercise 2: Creating Corridor Surface Boundaries](GUID-5F0C18E8-F32E-42C2-B381-551998705FF0.htm "In this exercise, you will use two different methods to define surface boundaries for your corridor design.").

Apply 3D render materials to a corridor

1.  Open _Corridor-5c.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the drawing, select the corridor.
    
    Tip:
    
    If you have difficulty selecting the corridor in the drawing, go to the ToolspaceProspector tab. Expand the Corridors collection. Right-click the corridor name and click Select.
    
    First, you will apply render materials to the corridor link codes.
    
3.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor panel drop-down ![](../images/ac.menuaro.gif) Edit Code Set Styles ![](../images/GUID-4A7E27B4-0363-4E06-B5AB-C01FCD6B64C5.png) Find.
4.  On the Edit Code Sets dialog box, under Code Set Style, make sure that All Codes is selected.
    
    In the Render Material column, examine the materials that are set for the links that are included in the subassemblies for the current corridor. These materials will be displayed on each link when you render the corridor model:
    
    *   Daylight\_Cut: **Sitework.Planting.Grass.Short**
    *   Daylight\_Fill: **Sitework.Planting.Grass.Short**
    *   Ditch: **Sitework.Planting.Grass.Thick**
    *   Gravel: **Sitework.Planting.Gravel.Mixed**
    *   Central Reserve: **Sitework.Planting.Grass.Short**
    *   Pave: **Sitework.Paving - Surfacing.Asphalt**
    *   Gradient\_Link: **Sitework.Planting.Grass.Short**
5.  Click OK.

Hide and render corridor surfaces

1.  Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)Named Views list ![](../images/ac.menuaro.gif)Corridor\_3D View.
    
    The drawing is redrawn to a three-dimensional view of the corridor.
    
2.  In Toolspace, on the Prospector tab, expand the Surfaces collection.
3.  Right-click the **Corridor - (1) Central Reserve** surface. Click Surface Properties.
4.  In the Surface Properties dialog box, on the Information tab, change the Surface Style to **Hide Surface**. Click OK.
    
    The Hide Surface style has all of its components turned off, which allows the surface’s render material to be effectively ignored. The rendering method used in this exercise applies render materials that are assigned to the subassembly link codes, and not the surface itself.
    
5.  Follow steps 2 and 3 to apply the **Hide Surface** style to the **Corridor - (1) Pave** and **Corridor - (1) Top** surfaces.
    
    Note:
    
    The Corridor - (1) Datum surface already uses the Hide Surface style.
    
6.  On the command line, enter **RENDER** to render the corridor in 3D using the render materials that are applied to the subassembly links.
    
    ![](../images/GUID-DECD4FE0-A97F-47DF-9B6C-73AC14E84842.png)
    
    Next, you will view 2D hatch patterns on the corridor by applying shape styles to the appropriate subassembly links.
    

Apply 2D hatching to the corridor model

1.  Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)Named Views list ![](../images/ac.menuaro.gif)Corridor\_All.
    
    The drawing is redrawn to plan view.
    
2.  In the drawing, select the corridor.
3.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor panel drop-down ![](../images/ac.menuaro.gif) Edit Code Set Styles ![](../images/GUID-4A7E27B4-0363-4E06-B5AB-C01FCD6B64C5.png) Find.
4.  On the Edit Code Sets dialog box, under Code Set Style, select **All Codes With Hatching**.
    
    In the Material Area Fill Style column, notice that a fill has been applied to each of the subassembly links that you examined in the previous procedure. However, notice that Gradient\_Link does not have a Material Area Fill Style associated with it. In the next few steps, you will apply a style by modifying the code set style.
    
5.  Click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection.
    
    Note:
    
    You can also open the Code Set Style dialog box from Toolspace on the Settings tab. Expand General![](../images/ac.menuaro.gif)Multipurpose Styles![](../images/ac.menuaro.gif)Code Set Styles. Right-click the appropriate code set style and click Edit.
    
6.  In the Code Set Style dialog box, on the Codes tab, under Link, in the Slope\_Link row, set the Material Area Fill Style to **Strip Hatch** .
7.  Click OK twice.
    
    The material area fill styles are applied to the 2D corridor model. Zoom in on the beginning of the corridor to examine the hatch patterns.
    
    ![](../images/GUID-47CEA4E9-18FB-41BC-8F7B-1728FFCBAF78.png)
    

**Parent topic:** [Tutorial: Viewing and Rendering a Corridor](GUID-007C34DB-D0D8-4E35-B831-1B9E01857FEA.htm "This tutorial demonstrates how to add surfaces to a corridor, create boundaries on the surfaces, and then visualize the corridor using the AutoCAD rendering tools.")