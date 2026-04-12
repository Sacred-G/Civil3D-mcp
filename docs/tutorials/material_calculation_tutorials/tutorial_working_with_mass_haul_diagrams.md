---
title: "Tutorial: Working with Mass Haul Diagrams"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-FE54D3EB-0701-4F90-997A-1D86EEEFC947.htm"
category: "material_calculation_tutorials"
last_updated: "2026-03-17T18:43:01.908Z"
---

                 Tutorial: Working with Mass Haul Diagrams  

# Tutorial: Working with Mass Haul Diagrams

This tutorial demonstrates how to create and edit mass haul diagrams to display earthworks in profile.

Mass haul is defined as the volume of material multiplied by the distance it is moved during construction. A mass haul diagram consists of two objects: a mass haul line, and a mass haul view. The mass haul line represents the free haul and overhaul volumes in cut and fill conditions along an alignment. The mass haul view is the grid on which the mass haul line is drawn.

The middle axis of the mass haul view is known as the balance line. The location of the mass haul line relative to the balance line indicates material movement in the current design. When the mass haul line rises above the balance line, it indicates a region in which material is cut. When the mass haul line falls below the balance line, it indicates a region in which material is fill.

There are two methods to compare free haul volume and overhaul volume:

## Gradient Points

Gradient points are stations at which the proposed project design transitions from cut to fill. In a mass haul diagram, a gradient point is the highest or lowest point in a mass haul region. A gradient point is the highest point in a mass haul region where the profile transitions from a cut condition to a fill condition. A gradient point is the lowest point in a mass haul region where the profile transitions from a fill condition to a cut condition.

In the gradient points method of measuring free haul, a horizontal line that is the length of the specified free haul distance is drawn. The line is positioned so that it is both parallel to the balance line and touches the mass haul line. The volume that is enclosed in the area formed by this line and the mass haul line is free haul.

In the following image, the green areas are free haul volume, and the red areas are overhaul volume. The magenta circles and arrows indicate the gradient points on the mass haul line and profile. The vertical magenta lines illustrate the relationship between the mass haul line and profile in the gradient point balancing method.

![](../images/GUID-8A9FB047-F9E0-45FF-9C1A-48D0234CADFA.png)

## Balance Points

Balance points are the stations at which the net cut and fill volumes are equal. In a mass haul diagram, the balance points are located on the balance line, where the net volume is zero. In the balance points method of measuring free haul, the mass haul line is duplicated and shifted horizontally to the right (where the project transitions from cut to fill) or to the left (where the project transitions from fill to cut) by the free haul distance.

In the following image, the green areas are free haul volume, and the red areas are overhaul. The arrows illustrate the free haul distance in cut and fill conditions.

![](../images/GUID-9221C35D-041B-4BFE-9C33-56E6A32E17FD.png)

**Topics in this section**

*   [Exercise 1: Creating a Mass Haul Diagram](GUID-91F4B5CD-4CF4-4B43-8E2D-B28F941BCE7C.htm)  
    In this exercise, you will create a mass haul diagram that displays free haul and overhaul volumes for a project site.
*   [Exercise 2: Balancing Mass Haul Volumes](GUID-A23CD8A8-7C49-489F-91FD-439CAD91B4FB.htm)  
    In this exercise, you will balance the mass haul volumes above and below the balance line, which will eliminate overhaul volume.
*   [Exercise 3: Editing the Mass Haul Line Style](GUID-40701848-9513-4AEB-81B7-1BADC785A20D.htm)  
    In this exercise, you will create a mass haul line style that is based on an existing style.

**Parent topic:** [Material Calculation Tutorials](GUID-91B964AE-D183-4E50-930C-95C84FA35857.htm "These tutorials will get you started working with the Autodesk Civil 3D tools for calculating and reporting material quantities and volumes.")