---
title: "Exercise 2: Drawing a Profile that Refers to Design Criteria"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-CE04E656-714D-4164-A499-EA9F482A237F.htm"
category: "tutorial_designing_a_profile_that_refers_to_local_"
last_updated: "2026-03-17T18:42:31.062Z"
---

                  Exercise 2: Drawing a Profile that Refers to Design Criteria  

# Exercise 2: Drawing a Profile that Refers to Design Criteria

In this exercise, you will draw a profile that refers to specified minimum standards.

You will use the standard profile layout tools to create a profile using the criteria-based design feature.

This exercise continues from [Exercise 1: Specifying Profile Design Criteria](GUID-69526F4A-5C27-4F95-A63D-EADE91D0412A.htm "In this exercise, you will specify minimum standards for a layout profile.").

Draw profile straights

Note:

This exercise uses Profile-4A.dwg with the modifications you made in the previous exercise.

2.  On the Profile Layout Tools toolbar, ensure that Draw Tangents![](../images/GUID-0B99A868-6C54-486F-A4E0-39A28F7D66EB.png) is selected.
3.  In the profile view, snap to the center of each of the circles that are labeled A through E.
    
    Important: To replicate the results described in this exercise, ensure that you use the Center object snap to snap to the center of the circles when you are drawing the straights. For information about using object snaps, see the [Using Basic Functionality tutorial](GUID-11DBA8FF-E960-454F-B91F-88A715BD3118.htm "In this tutorial, you will learn how to navigate around Autodesk Civil 3D and how to use some common features of the interface.").
    
4.  After you click in Circle E, right-click to end the profile.
    
    The layout profile consists of straights connected at vertical intersection points (VIPs). Next, you will add curves at each VIP.
    

Add a free curve that exceeds the design standards

1.  On the Profile Layout Tools toolbar, click the arrow next to ![](../images/GUID-090A3EA8-307C-419A-99DE-657F731CE350.png). Select ![](../images/GUID-6189289B-4CAF-41D1-8FDC-148B116D261C.png)Free Vertical Curve (Parabola).
2.  On the profile view, click the straight that enters Circle B on the left (the “first element”).
3.  Click the straight that exits Circle B on the right (the “next element”).
    
    On the command line, notice that you can select the parameter that you want to use to define the curve. The displayed value for the selected parameter is the minimum value that is required by the design criteria file. For this exercise, you will enter values that do not meet the design criteria, and then examine the results.
    
4.  On the command line, enter **R** to specify a radius value. Enter a radius value of 500.
    
    The curve is drawn between the straights, and a ![](../images/GUID-D5ADDB33-0636-4E9A-BACD-113935F74A84.png) warning symbol is displayed. You will learn how to diagnose and correct the violations in [Exercise 3: Viewing and Correcting Profile Design Criteria Violations](GUID-91160B7D-AC46-4957-A6B2-71D3D758C9CC.htm "In this exercise, you will check the profile design for criteria violations, and then learn how to correct violations.").
    
5.  Repeat steps 3 and 4 to add an identical curve to the VIP in Circle C.

Add a free curve that meets the design standards

1.  On the profile view, click the straight that enters Circle D on the left (the “first element”).
2.  Click the straight that exits Circle D on the right (the “next element”).
3.  Press Enter to accept the minimum radius value that is displayed on the command line.
    
    The curve is drawn at Circle D, but this time no warning symbol is displayed. You can use the command line to quickly apply minimum values to profile elements as you draw them.
    
4.  Right-click to end the command.

To continue this tutorial, go to [Exercise 3: Viewing and Correcting Profile Design Criteria Violations](GUID-91160B7D-AC46-4957-A6B2-71D3D758C9CC.htm "In this exercise, you will check the profile design for criteria violations, and then learn how to correct violations.").

**Parent topic:** [Tutorial: Designing a Profile that Refers to Local Standards](GUID-75B9BF20-42D1-4D86-9465-C906A03FDC16.htm "This tutorial demonstrates how to validate that your profile design meets criteria specified by a local agency.")