---
title: "Exercise 2: Grip Editing an Alignment"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-2B7CE63E-F2F6-4C54-AB23-C84491FFB5F4.htm"
category: "tutorial_editing_alignments"
last_updated: "2026-03-17T18:42:25.856Z"
---

                  Exercise 2: Grip Editing an Alignment  

# Exercise 2: Grip Editing an Alignment

In this exercise, you will use grips to move alignment curves.

You will learn how to grip edit offset and nearside kerb alignments in the [Working with Offset and Nearside Kerb Alignments](GUID-AF7F5C3B-9B2F-4D3A-8F01-CF474DD8D352.htm "This tutorial demonstrates how to create and modify offset alignments that are dynamically linked to a centerline alignment.") tutorial.

Note: To change the behavior of an element, you can change the Tangency Constraint and Parameter Constraint value.

This exercise continues from [Exercise 1: Editing the Layout Parameter Values of an Alignment](GUID-70269D00-79DF-4FB1-B945-FB5A107C11DE.htm "In this exercise, you will use the Alignment Elements vista and Alignment Layout Parameters dialog box to edit the layout parameter values of an alignment.").

Note:

Ensure that Dynamic Input (DYN) is turned on, and OSNAP is turned off. For more information, see the [Using Basic Functionality](GUID-11DBA8FF-E960-454F-B91F-88A715BD3118.htm "In this tutorial, you will learn how to navigate around Autodesk Civil 3D and how to use some common features of the interface.") tutorial.

Grip edit a free curve element

Note:

This exercise uses _Align-4.dwg_ with the modifications you made in the previous exercise.

2.  Zoom to the area around circle B.
3.  Click the alignment. Grips appear at the curve ends, midpoint, and at the intersection point (PI).
4.  Click the ![](../images/GUID-BA4AF8A7-E690-4D89-8A4B-B4A5F2B1BA7A.png) midpoint grip at the midpoint of the curve. It turns red.
5.  Click a new location for the curve to pass through.
    
    Notice that the curves and straights remain straight to each other, but both endpoints move along the straights.
    
6.  Click the radius ![](../images/GUID-3BE3120F-533A-4D8C-B4E6-722F942512B4.png) grip directly above the pass-through point grip and experiment with moving it.
    
    Notice that this grip affects only the curve radius and constrained to the direction of the radius change.
    
7.  Select either an endpoint grip or the PI grip, and experiment with reshaping the curve in different ways.

Grip edit a floating curve element

1.  Pan to the area around circles D and E.
2.  Select the ![](../images/GUID-4FD21E5E-7D79-4634-86C6-EE5629747C33.png) grip in circle D. It turns red.
3.  Click a new location for the grip.
    
    Notice that the pass-through point ![](../images/GUID-BA4AF8A7-E690-4D89-8A4B-B4A5F2B1BA7A.png) grip in circle F does not move.
    
4.  Select the ![](../images/GUID-4FD21E5E-7D79-4634-86C6-EE5629747C33.png) pass-through point grip in circle E. It turns red.
5.  Click a new location for the grip.
    
    Notice that the ![](../images/GUID-4FD21E5E-7D79-4634-86C6-EE5629747C33.png) pass-through point grip in circle D does not move. The transition-curve element in circle D moves along the preceding straight to accommodate the new pass-through point.
    
    To see how grip editing affects curve specifications, in the next few steps you will add a segment label to the curve. This label shows curve length and radius. The values update each time you reshape the curve.
    
    Notice that with this type of curve, if you edit the triangular PI grip, the curve radius does not change.
    
6.  Click Alignment tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Labels drop-down ![](../images/ac.menuaro.gif)Single Segment![](../images/GUID-10094573-05A5-439E-A30D-2E506775AA08.png).
7.  Click the curve in circle D. A label is placed on the curve.
8.  Press Enter to end the label command.
9.  Click the curve to activate the grips.
10.  Edit the curve using the ![](../images/GUID-4FD21E5E-7D79-4634-86C6-EE5629747C33.png) pass-through point grip.
     
     Notice that while the grip is active, you can use dynamic input to enter a specific pass-through point. You may also enter a specific value in the Alignment Layout Parameters window.
     
11.  Press Esc to deselect the alignment. The label shows the new length of the curve.
12.  Close this drawing.

To continue this tutorial, go to [Exercise 3: Applying a Mask to an Alignment](GUID-B4208BE1-BF5E-41DB-922A-E08EE62AD5C7.htm "In this exercise, you will hide a portion of an alignment from view.").

**Parent topic:** [Tutorial: Editing Alignments](GUID-CC717118-AA00-4F58-84B9-A6E5C9D23BDD.htm "This tutorial demonstrates some common editing tasks for alignments.")