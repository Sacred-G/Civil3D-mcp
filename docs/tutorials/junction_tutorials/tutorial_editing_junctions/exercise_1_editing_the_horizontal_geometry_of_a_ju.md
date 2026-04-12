---
title: "Exercise 1: Editing the Horizontal Geometry of a Junction"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-747B4E40-488D-4A5E-888E-E11E04D9AE6C.htm"
category: "tutorial_editing_junctions"
last_updated: "2026-03-17T18:42:56.193Z"
---

                 Exercise 1: Editing the Horizontal Geometry of a Junction  

# Exercise 1: Editing the Horizontal Geometry of a Junction

In this exercise, you will edit the alignments that define the horizontal geometry of a junction. You will edit the alignments graphically and parametrically, and then examine how the changes affect the junction.

Modify offset alignment parameters

1.  Open _Intersection-Edit-Horizontal.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a junction of a primary road (Road A) and a side road (Road B).
    
    *   The offset alignments for Road A extend along the full length of the centerline alignment.
    *   The offset alignment for Road B does not extend beyond the junction extents.
    *   The radius kerbs have widening regions on all four corners of Road A.
    
    ![](../images/GUID-88C801AD-DCF0-436B-93A6-57C39E217EF8.png)
    
2.  Click the junction marker.
    
    The Junction tab is displayed on the ribbon. The Modify panel has tools that you can use to modify the parameters of the horizontal and vertical geometry of the junction.
    
3.  Click Junction tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Edit Offsets ![](../images/GUID-E455CCF9-D473-4029-BE65-B9329640C901.png) Find.
    
    The offset alignment parameters are displayed in the Junction Offset Parameters dialog box.
    
4.  Under Side Road, change the Offset Value for both offset alignments to **4.000**.
    
    Notice that as the values change, the junction updates in the drawing.
    
    ![](../images/GUID-3B6A021D-3A98-464A-8782-1772D7CFB52C.png)
    

Modify the radius kerb parameters

1.  Click Junction tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Edit Corner Radius ![](../images/GUID-42184859-FCB1-49D0-B528-36FCAB63263E.png) Find.
    
    The Junction Radius Kerb Parameters dialog box displays the nearside kerb alignment parameters for the northeast junction quadrant. This dialog box enables you to change basic parameters, as well as specific details of the radius kerb at each junction quadrant.
    
    In the drawing, notice that the currently selected radius kerb is highlighted.
    
    ![](../images/GUID-665D916B-7EBB-4E96-BCA4-7282F387AF69.png)
    
2.  Clear the Widen Turning Lane For Outgoing Road check box.
    
    In the drawing, the widening region for the northeast quadrant is removed.
    
    ![](../images/GUID-CAC00743-3BB9-4345-B931-4D7084E72499.png)
    
3.  Under Junction Quadrant, click **SW - Quadrant**.
4.  Clear the Widen Turning Lane For Outgoing Road check box.
    
    ![](../images/GUID-4201F528-E628-4AE1-82C8-E434A0A7B3AC.png)
    

Grip edit a nearside kerb alignment

1.  In the drawing, select the southeast nearside kerb alignment.
    
    Grips appear along the nearside kerb alignment.
    
    ![](../images/GUID-4F96F2A3-886A-4E20-89BD-D74471AA8212.png)
    
2.  On the Road A alignment, experiment with the ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) grips.
    
    When you move a grip, the radius kerb widening region updates, and the values update in the Junction Radius Kerb Parameters dialog box.
    
3.  Press Esc.

Grip edit the centerline alignments

1.  Select both offset alignments along Road B.
2.  Click the ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) grip on the left. Drag the grip to the left. Click near chainage 0+660 to place the grip.
    
    ![](../images/GUID-3AC492E7-3E4D-42A0-A9BB-9A0CC7C2A9DB.png)
    
    This action enables the relationship between the radius kerbs and the offset alignments to be maintained as you move the junction along the centerline alignment.
    
3.  Press Esc.
4.  Zoom out to see the ends of both centerline alignments.
5.  Select the Road A centerline alignment.
6.  Select the grip at the southern end of the alignment.
7.  Drag the grip to the left. Click to place the grip.
    
    ![](../images/GUID-CB01F910-13B3-4926-8A86-EB7216DD6B99.png)
    
    The junction slides along the Road B centerline and offset alignment. The nearside kerb alignments and Road A offset alignments move to accommodate the new junction point. The radius kerb and offset alignment geometry parameters are maintained.
    
    ![](../images/GUID-06F6128A-C731-45AF-A78C-2C99D86C7BE0.png)
    

To continue this tutorial, go to [Exercise 2: Editing the Vertical Geometry of an Intersection](GUID-D156D6A4-DFFA-4CF3-BEC7-6BD112DF7DFB.htm "In this exercise, you will edit the profiles that define the vertical geometry of a junction object. You will edit the profiles graphically and parametrically, and examine how the changes affect the junction.").

**Parent topic:** [Tutorial: Editing Junctions](GUID-74F6ED8E-61DD-496E-8F08-C2FE11655355.htm "This tutorial demonstrates how to modify an existing junction object.")