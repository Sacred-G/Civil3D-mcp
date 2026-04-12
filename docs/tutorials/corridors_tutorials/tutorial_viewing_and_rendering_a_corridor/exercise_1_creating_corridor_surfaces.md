---
title: "Exercise 1: Creating Corridor Surfaces"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-FA79D184-B3AC-4B97-B9E2-349C2909D03C.htm"
category: "tutorial_viewing_and_rendering_a_corridor"
last_updated: "2026-03-17T18:42:52.930Z"
---

                 Exercise 1: Creating Corridor Surfaces  

# Exercise 1: Creating Corridor Surfaces

In this exercise, you will create Top, Datum, Pave, and Central Reserve surfaces from the corridor.

The Top surface tracks the finish gradient of the carriageway from the left daylight point to the right daylight point on both paved and unpaved portions. This surface is used for finish gradient modeling.

The Datum surface tracks the finish gradient on unpaved portions, and also the subbase on paved portions, going from the left daylight point to the right daylight point. This surface represents the grading levels before pavement materials are applied. This surface is used for calculating cut and fill quantities.

The Pave surface defines the finished pavement on both travel lanes in the divided highway.

The Central Reserve surface defines the area between the travel lanes.

Create a top corridor surface

1.  Open _Corridor-5a.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the drawing, select the corridor.
3.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor panel ![](../images/ac.menuaro.gif)Corridor Surfaces ![](../images/GUID-A679DF21-6B0C-4B6D-9244-6FF92F861387.png) Find.
4.  In the Corridor Surfaces dialog box, click ![](../images/GUID-2AA358C9-97A0-45D0-B258-5AFB05ADDF05.png)Create A Corridor Surface to create an entry in the surfaces table.
5.  Change the surface name to **Corridor - (1) Top**.
6.  Click the Surface Style cell for the **Corridor - (1) Top** surface.
7.  In the Pick Corridor Surface Style dialog box, select **Border & Contours**. Click OK.
8.  Click the Render Material cell for the **Corridor - (1) Top** surface.
9.  In the Pick Render Material dialog box, select **Sitework.Paving - Surfacing. Asphalt**. Click OK.
10.  Select the **Corridor - (1) Top** surface by clicking the ![](../images/GUID-2AA358C9-97A0-45D0-B258-5AFB05ADDF05.png) icon next to its name.
11.  Change the Overhang Correction setting to Top Links.
     
     This setting specifies that the surface will be built using the links along the top of the assembly. This setting is especially critical when an assembly has overlapping subassembly links that, if connected, would result in errors in surface triangulation.
     
12.  For Specify Code, select Top. Click ![](../images/GUID-7633B109-B0E2-42D8-8A07-E27BBF28B731.png)Add Surface Item.
     
     This action adds the corridor links with the Top code to this surface.
     

Create a datum corridor surface

*   Repeat the previous procedure to create a Datum surface, using these parameters:
    
    *   Name: **Corridor - (1) Datum**
    *   Surface Style: **Hide Surface**
    *   Render Material: **Sitework.Planting.Soil**
    *   Overhang Correction: **Bottom Links**
    *   Link Code: **Datum**

Create a pave corridor surface

*   Create a Pave surface, using these parameters:
    
    *   Name: **Corridor - (1) Pave**
    *   Surface Style: **Border & Contours**
    *   Render Material: **Sitework.Paving - Surfacing Asphalt**
    *   Overhang Correction: **Top Links**
    *   Link Code: **Pave**

Create a central reserve corridor surface

*   Create a Central Reserve surface, using these parameters:
    
    *   Name: **Corridor - (1) Central Reserve**
    *   Surface Style: **Border & Contours**
    *   Render Material: **Sitework.Planting.Gravel.Mixed**
    *   Overhang Correction: **Top Links**
    *   Link Code: **Gravel**

Generate the surfaces and examine the results

1.  Click OK to create the surfaces and close the Corridor Surfaces dialog box.
2.  In Toolspace, on the Prospector tab, expand the Surfaces collection.
    
    Notice that the corridor surfaces you created have been added to the Surfaces collection. You can work with a corridor surface the same way you do with any surface in the Surfaces collection, including changing its style, adding labels to it, and using it for surface analysis. The following features and behaviors are unique to corridor surfaces:
    
    *   When you select a corridor surface, only the surface is selected. The corridor it is based on is not selected.
    *   When you change the surface style of a corridor surface using its surface properties, the style is also changed in the Corridor Properties dialog box on the Surfaces tab.
    *   When a corridor is rebuilt, corridor surfaces are updated to reflect any changes in the corridor, and then any edits are applied to the corridor model.
    *   The corridor from which the surface was taken is listed in the surface properties definition.

To continue this tutorial, go to [Exercise 2: Creating Corridor Surface Boundaries](GUID-5F0C18E8-F32E-42C2-B381-551998705FF0.htm "In this exercise, you will use two different methods to define surface boundaries for your corridor design.").

**Parent topic:** [Tutorial: Viewing and Rendering a Corridor](GUID-007C34DB-D0D8-4E35-B831-1B9E01857FEA.htm "This tutorial demonstrates how to add surfaces to a corridor, create boundaries on the surfaces, and then visualize the corridor using the AutoCAD rendering tools.")