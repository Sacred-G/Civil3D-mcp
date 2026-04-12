---
title: "Exercise 1: Creating a Mass Haul Diagram"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-91F4B5CD-4CF4-4B43-8E2D-B28F941BCE7C.htm"
category: "tutorial_working_with_mass_haul_diagrams"
last_updated: "2026-03-17T18:43:02.062Z"
---

                 Exercise 1: Creating a Mass Haul Diagram  

# Exercise 1: Creating a Mass Haul Diagram

In this exercise, you will create a mass haul diagram that displays free haul and overhaul volumes for a project site.

To create a mass haul diagram, the following items must be available:

*   an alignment
*   two surfaces
*   a sample line group
*   a material list

The sample drawing that you will use for this exercise contains all of these items.

To learn how to create a material list, see the [Creating a Material List](GUID-DAC90690-3DC4-400E-8A5C-104205DE7C30.htm "In this exercise, you will create a material list, which defines the quantity takeoff criteria and surfaces to compare during an earthworks analysis.") exercise.

Create a mass haul diagram

1.  Open _Mass Haul-1.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Analyze tab ![](../images/ac.menuaro.gif)Volumes And Materials panel ![](../images/ac.menuaro.gif)Mass Haul ![](../images/GUID-D34B0FCB-5C15-4CE2-9D7E-1A005E503D3F.png) Find.
3.  In the Create Mass Haul Diagram wizard, on the General page, specify the following parameters:
    *   Mass Haul View Name: **Mass Haul Balancing**
    *   Mass Haul View Style: **Free Haul and Overhaul**
4.  Click Next.
5.  On the Mass Haul Display Options page, examine the settings in the Material area. The selection in the Material List is saved with the selected sample line group. To learn how to create a material list, see the [Creating a Material List](GUID-DAC90690-3DC4-400E-8A5C-104205DE7C30.htm "In this exercise, you will create a material list, which defines the quantity takeoff criteria and surfaces to compare during an earthworks analysis.") exercise. Examine the options that are available in the Choose A Material To Display As Mass Haul Line list, but accept the default Total Volume option. In the Mass Haul Line area, specify the following settings:
    *   Mass Haul Line Name: **Mass Haul Line Total Volume**
    *   Mass Haul Line Style: **Free Haul and Overhaul- Gradient Point**
6.  Click Next.
7.  On the Balancing Options page, under Free Haul Options, select the Free Haul Distance check box. Enter **300.0000’** as the free haul distance.
    
    This value specifies that the distance that the earthmover hauls material at the standard rate. Material moved beyond this distance is considered overhaul, and typically is charged at a higher rate.
    
8.  Click Create Diagram.
9.  In the drawing, snap to the center of the red circle that is above the profile view to place the mass haul diagram.
    
    The mass haul diagram is displayed, and contains three mass haul regions. As shown in the following image, mass haul regions identify chainage ranges where material is either cut or fill. When the mass haul line is above the balance line, material is cut. When the mass haul line is below the balance line, material is fill.
    
    ![](../images/GUID-8A8538A1-B246-45F2-BA08-34567CA40C14.png)
    

To continue this tutorial, go to [Exercise 2: Balancing Mass Haul Volumes](GUID-A23CD8A8-7C49-489F-91FD-439CAD91B4FB.htm "In this exercise, you will balance the mass haul volumes above and below the balance line, which will eliminate overhaul volume.").

**Parent topic:** [Tutorial: Working with Mass Haul Diagrams](GUID-FE54D3EB-0701-4F90-997A-1D86EEEFC947.htm "This tutorial demonstrates how to create and edit mass haul diagrams to display earthworks in profile.")