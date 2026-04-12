---
title: "Exercise 3: Editing the Mass Haul Line Style"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-40701848-9513-4AEB-81B7-1BADC785A20D.htm"
category: "tutorial_working_with_mass_haul_diagrams"
last_updated: "2026-03-17T18:43:02.321Z"
---

                 Exercise 3: Editing the Mass Haul Line Style  

# Exercise 3: Editing the Mass Haul Line Style

In this exercise, you will create a mass haul line style that is based on an existing style.

The mass haul line style controls the display of the line that displays free haul and overhaul volumes in a mass haul view. The display components, such as color, linetype, and hatch patterns, in the mass haul line style are similar to the components that are in other object styles. The mass haul line style also specifies the method with which free haul is measured.

In this exercise, you will copy an existing mass haul line style to create a new style. You will examine the differences between the gradient point and balance point methods of measuring free haul.

Note:

The mass haul view style uses many of the same options as the profile view style. For information about editing the profile view style, see the [Editing the Profile View Style exercise](GUID-6EA3E98E-4E1F-4C82-ACFC-CA56B3492784.htm "In this exercise, you will learn how to change the data displayed in a profile view.").

This exercise continues from [Exercise 2: Balancing Mass Haul Volumes](GUID-A23CD8A8-7C49-489F-91FD-439CAD91B4FB.htm "In this exercise, you will balance the mass haul volumes above and below the balance line, which will eliminate overhaul volume.").

To edit the mass haul line style

1.  Open _Mass Haul-2.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the drawing, select the mass haul line. Right-click. Click Mass Haul Line Properties.
3.  In the Mass Haul Line Properties dialog box, on the Information tab, in the Object Style area, click the arrow next to ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png). Click ![](../images/GUID-CA1AD018-7C68-4826-8BAB-59D50446DD6F.png)Copy Current Selection.
4.  In the Mass Haul Line Style dialog box, on the Information tab, change the Name to **Free Haul and Overhaul - Balance Point**.
5.  On the Free Haul tab, in the Free Haul Options area, notice that the Measure From Gradient Point option is selected. Also notice that the graphic to the right resembles the mass haul diagram in the drawing. Gradient points are the points at which volumes transition from cut to fill. When free haul is measured from gradient points, the highest point (or lowest, if below the balance line) is the gradient point.
6.  Select Measure From Balance Point.
    
    Notice that the graphic changes. Balance points are the points at which the mass haul line crosses the balance line. These are the chainage at which the cut volume and fill volume are equal.
    
7.  On the Display tab, in the Component Hatch Display area, in the Free Haul Area Hatch row, click the Pattern cell.
8.  In the Hatch Pattern dialog box, for Pattern Name, select **CROSS**. Click OK.
9.  In the Mass Haul Line Style dialog box, click the Display tab. In the Component Hatch Display area, in the Free Haul Area Hatch row, change the Scale to **30.0000**.
10.  Repeat Steps 7 through 9 to change the Overhaul Area HatchPattern to **DASH**.
     
     Note:
     
     A solid component fill provides the best performance. Drawing regeneration may be slower if a hatch pattern is used on a long mass haul diagram.
     
11.  Click OK twice.
     
     The patterns you selected are displayed in the free haul and overhaul areas of the mass haul diagram. Notice that the mass haul diagram uses the balance point method to measure free haul.
     
     ![](../images/GUID-9C77D977-726D-4E8E-9035-19B4C6F61870.png)
     

To continue to the next tutorial, go to [Calculating and Reporting Quantities](GUID-709DB1D4-FB24-46F0-A54B-E2D9CC6D14F7.htm "In this tutorial, you will learn how to create and manage rate item data, associate rate item codes with several types of drawing objects, and generate rate item quantity reports.").

**Parent topic:** [Tutorial: Working with Mass Haul Diagrams](GUID-FE54D3EB-0701-4F90-997A-1D86EEEFC947.htm "This tutorial demonstrates how to create and edit mass haul diagrams to display earthworks in profile.")