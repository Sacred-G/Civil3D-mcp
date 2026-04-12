---
title: "Exercise 4: Editing an Offset Widening"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-92213F16-7C49-4729-8088-A166CDD17B2C.htm"
category: "tutorial_working_with_offset_alignments"
last_updated: "2026-03-17T18:42:26.657Z"
---

                 Exercise 4: Editing an Offset Widening  

# Exercise 4: Editing an Offset Widening

In this exercise, you will change the transition between an offset alignment and its widening region, and then use grips to modify the widening geometry.

This exercise continues from [Exercise 3: Adding a Widening to an Offset Alignment](GUID-8208038F-7522-44C1-8DBC-5AAC9D789107.htm "In this exercise, you will add dynamic widening regions between specified stations of an offset alignment.").

Change the widening transition

1.  Open _Align-6D.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains a surface and a centerline alignment with two offset alignments. The offset alignment on the right-hand side of the centerline alignment has two widening regions.
    
2.  Select the offset alignment that is on the right-hand side of the centerline alignment. Right-click. Click Edit Offset Parameters.
    
    In the Offset Alignment Parameters dialog box, notice that the offset alignment is divided into a series of regions and transitions. Each region represents the offset values of the sequential chainage ranges along the centerline alignment. The transitions represent the geometry that joins one region to the next.
    
    Note:
    
    The parameters for the first region are displayed in the window. Show the parameters for subsequent regions by using the Select Widening Region list or the arrow buttons.
    
3.  In the Offset Alignment Parameters dialog box, click ![](../images/GUID-727B0387-A0EE-493D-91A6-D6EFA45BF1B1.png).
4.  In the Property column, select each of the Region and Transition entries in turn.
    
    Notice that each region and transition is highlighted in the drawing as you select it.
    
5.  Expand the Taper Out category.
6.  Change the Transition Type from Linear to Curve-Line-Curve.
    
    The transition changes to a line with a curve on either end.
    
    ![](../images/GUID-074D6E95-F109-4689-9B6C-9AE13821B1A1.png)
    
7.  Under Transition Parameters, for Transition Length, enter **50**.
    
    The transition updates to reflect your changes.
    

Grip edit the widening region

1.  On the centerline alignment, select the ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) widening start grip near Chainage 4+50. The grip turns red.
2.  Click to place the grip at Chainage 5+00.
    
    Notice that when you changed the starting location of the widening, the transition moved with it, while its parameters were maintained.
    
    ![](../images/GUID-43B1390E-E505-4D47-AB3A-BE977BE7431B.png)
    
3.  On the offset alignment, click the ![](../images/GUID-B4753DB6-29DB-44EF-9EF6-3B2C3BCCA48E.png) offset grip. The grip turns red.
4.  While the grip is active, enter **50** as the new offset value.
    
    The widening region expands to accommodate the new offset value.
    
    ![](../images/GUID-1AC923DB-F4E4-45E6-8BD2-E8899B154B31.png)
    
    Note:
    
    The ![](../images/GUID-97BC4CB7-B524-43BF-8F10-73447BAE260A.png) grip enables you to add another widening region.
    
5.  Click the ![](../images/GUID-C214CB0E-FD72-4C88-A6CC-62189AFBDE14.png) grip.
    
    The ![](../images/GUID-3BA2ED16-3794-4D17-B17E-7B61035D21A8.png) grips are now gray, and ![](../images/GUID-D38E9178-09C5-4C91-BBBB-0E17C4B5F88E.png) grips are displayed at the beginning and end of each transition sub-element. These grips are used to modify the transition geometry.
    
    ![](../images/GUID-2F7471BC-B3B0-4534-981F-5F2302FEEB23.png)
    
6.  Experiment with using the ![](../images/GUID-D38E9178-09C5-4C91-BBBB-0E17C4B5F88E.png) grips to change the transition geometry.
    
    As you grip-edit the transition geometry, notice that the applicable parameter values are automatically updated in the Offset Parameters dialog box.
    

To continue to the next tutorial, go to [Designing an Alignment that Refers to Local Standards](GUID-CAEC3077-D78A-4F42-8E47-41FABAB6915D.htm "This tutorial demonstrates how to validate that your alignment design meets criteria specified by a local agency.").

**Parent topic:** [Tutorial: Working with Offset Alignments](GUID-AF7F5C3B-9B2F-4D3A-8F01-CF474DD8D352.htm "This tutorial demonstrates how to create and modify offset alignments that are dynamically linked to a centerline alignment.")