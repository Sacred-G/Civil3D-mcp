---
title: "Exercise 1: Simplifying a Building Footprint"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-295DDF7E-A22E-4055-B79C-C60E1758FCE4.htm"
category: "tutorial_grading_from_a_complex_building_footprint"
last_updated: "2026-03-17T18:42:41.723Z"
---

                   Exercise 1: Simplifying a Building Footprint  

# Exercise 1: Simplifying a Building Footprint

In this exercise, you will use the feature line stepped offset command to generate a simplified footprint from which to gradient.

The drawing you will use in this tutorial displays a building pad in two vertically arranged viewports. In the right viewport, the building pad is shown in plan view. You will design the building pad in the right viewport. In the left viewport, the building pad is displayed in model view. You will use this viewport to see the status of the design as you work.

Create a stepped offset feature line

1.  Open _Grading-6.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  On the command line, enter **OffsetGapType**. Enter **1** as the value.
    
    The OffsetGapType variable controls how potential gaps between segments are treated when closed polylines, such as the building pad feature line, are offset. Setting this variable to 1 fills the gaps with filleted arc segments; the radius of each arc segment is equal to the offset distance.
    
3.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Feature Line drop-down ![](../images/ac.menuaro.gif)Create Feature Line From Stepped Offset ![](../images/GUID-96531BAB-499A-41AA-B7C7-46E3DB2AEAAD.png) Find.
4.  On the command line, enter **1.5** as the offset distance.
5.  In the right viewport, click the blue feature line representing the building pad when prompted to select a feature line.
6.  Click outside the building pad when prompted to specify the side to offset.
7.  At the command line, enter **G** to specify a gradient value. Enter **\-1** as the gradient.
8.  Press Enter to end the command.
    
    The stepped offset feature line is displayed. Notice that in the left viewport, on the left side of the ramp, the curve of the stepped offset feature line is not well formed. Because it is an independent feature line, you can use the feature line editing tools to refine the solution.
    
    ![](../images/GUID-B6637EA3-B3F1-4580-AC95-259B528E39D7.png)
    

Add fillets to the feature line

1.  Click Modify tab ![](../images/ac.menuaro.gif)Edit Geometry panel ![](../images/ac.menuaro.gif)Fillet ![](../images/GUID-AED0A5CE-DC3F-4061-A439-6E6A2CD1C269.png) Find.
2.  Click the offset feature line when prompted to select an object.
3.  On the command line, enter **R** to specify a radius for the fillet. Enter **15.000** as the radius value.
4.  On each side of the ramp, click a sharp inside corner of the offset feature line.
    
    When you move the cursor over the feature line, a green triangle highlights the corners that can be filleted and a preview fillet is displayed. See the following image for an example of a corner to click.
    
    ![](../images/GUID-924177B6-1324-472D-B071-AE15F5BA1326.png)
    
    The fillet is applied to the feature line. In the left viewport, notice that the fillet uses the levels from the existing feature line and smoothly interpolates the levels along the length of the fillet.
    
    ![](../images/GUID-481878A0-5510-4599-861D-DA51441F04B8.png)
    
5.  Press Enter twice to end the command.

To continue this tutorial, go to [Exercise 2: Grading from a Building Footprint to a Surface](GUID-66A515E5-E405-48B5-B873-4C948CA30A82.htm "In this exercise, you will gradient from the simplified, offset footprint to the existing ground surface.").

**Parent topic:** [Tutorial: Grading from a Complex Building Footprint](GUID-5CC7A47C-5448-40FF-83D6-4057E3AB143C.htm "This tutorial demonstrates how to gradient around a building footprint that has relatively complicated geometry and variations in level.")