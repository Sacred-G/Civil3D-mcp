---
title: "Exercise 2: Grading from a Building Footprint to a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-66A515E5-E405-48B5-B873-4C948CA30A82.htm"
category: "tutorial_grading_from_a_complex_building_footprint"
last_updated: "2026-03-17T18:42:41.864Z"
---

                 Exercise 2: Grading from a Building Footprint to a Surface  

# Exercise 2: Grading from a Building Footprint to a Surface

In this exercise, you will gradient from the simplified, offset footprint to the existing ground surface.

The stepped offset feature line has much simpler geometry than the original footprint. The simpler feature line geometry will result in a much simpler grading than one created directly from the original footprint.

This exercise continues from [Exercise 1: Simplifying a Building Footprint](GUID-295DDF7E-A22E-4055-B79C-C60E1758FCE4.htm "In this exercise, you will use the feature line stepped offset command to generate a simplified footprint from which to gradient.").

Create a grading group and specify grading creation settings

Note:

This exercise uses _Grading-6.dwg_ with the modifications you made in the previous exercise.

2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Grading drop-down ![](../images/ac.menuaro.gif)Grading Creation Tools ![](../images/GUID-FCA44E9F-DE55-4BFD-9FFC-6D475EA0389E.png) Find.
3.  On the Grading Creation Tools toolbar, click ![](../images/GUID-A76D3F3C-DE06-4F23-B3CE-86A10979C19C.png)Set The Grading Group.
4.  In the Select Grading Group dialog box, under Site Name, ensure that **Grading - Building Pad** site is selected.
5.  Under Group Name, click ![](../images/GUID-1FCECF29-AB8F-4D53-BBAE-60BA29EC0A3A.png)Create A Grading Group.
6.  In the Create Grading Group dialog box, for Name, enter **Gradient Grading With Feature Lines**.
7.  Click OK twice.
8.  On the Grading Creation Tools toolbar, click ![](../images/GUID-6BA9AC63-A03A-493C-8716-35AD405BF1FC.png)Set The Target Surface.
9.  In the Select Surface dialog box, ensure that **EG** surface is selected. Click OK.
10.  On the Grading Creation Tools toolbar, click ![](../images/GUID-1FB50A49-43C1-43CD-9F5A-17FD818E4157.png)Select a Criteria Set.
11.  In the Select A Criteria Set dialog box, ensure that Basic Set is selected. Click OK.
12.  From the Select a Grading Criteria list, select **Gradient To Surface** .

Create a grading

1.  Click ![](../images/GUID-FCA44E9F-DE55-4BFD-9FFC-6D475EA0389E.png)Create Grading.
2.  When prompted to select a feature, click the blue feature line that is offset from the building pad.
3.  When prompted to select the grading side, click outside the offset feature line.
4.  Press Enter to apply the grading to the entire length of the feature line.
5.  Press Enter to accept the default Gradient Cut Format.
6.  Press Enter to accept the default 2.00:1 Cut Gradient.
7.  Press Enter to accept the default Gradient Fill Format.
8.  Press Enter to accept the default 2.00:1 Fill Gradient.
9.  Press Esc to end the command.
    
    The stepped offset feature line is gradiented to the EG surface. Notice that while the cut and fill gradients are shown in red and green, there are still open areas inside the grading group. In the next exercise, you will fill these areas and apply appropriate grading styles.
    
    ![](../images/GUID-A51C656C-7E21-4E56-97D9-1448AF2BD422.png)
    

To continue this tutorial, go to [Exercise 3: Filling Holes in a Grading](GUID-6BAC6196-EF89-42F5-AC1C-C72C79F7B145.htm "In this exercise, you will create infill gradings to fill in the open areas inside the grading group.").

**Parent topic:** [Tutorial: Grading from a Complex Building Footprint](GUID-5CC7A47C-5448-40FF-83D6-4057E3AB143C.htm "This tutorial demonstrates how to gradient around a building footprint that has relatively complicated geometry and variations in level.")