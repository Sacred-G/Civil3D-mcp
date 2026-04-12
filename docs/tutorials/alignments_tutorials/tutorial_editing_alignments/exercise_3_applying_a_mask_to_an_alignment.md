---
title: "Exercise 3: Applying a Mask to an Alignment"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-B4208BE1-BF5E-41DB-922A-E08EE62AD5C7.htm"
category: "tutorial_editing_alignments"
last_updated: "2026-03-17T18:42:25.905Z"
---

                 Exercise 3: Applying a Mask to an Alignment  

# Exercise 3: Applying a Mask to an Alignment

In this exercise, you will hide a portion of an alignment from view.

When you apply a mask to a portion of an alignment, the alignment sub-elements, labels, and marker points are not drawn. These items still exist, but are hidden from view.

This feature is useful when working on an intersection. In many cases, you do not need to see the alignment geometry that passes through the intersection. In this exercise, you will mask the portion of an offset alignment that passes through an intersection.

![](../images/GUID-5B8DD83C-B7F6-4497-8DDB-79AEA37D06B0.png)

This exercise continues from [Exercise 2: Grip Editing an Alignment](GUID-2B7CE63E-F2F6-4C54-AB23-C84491FFB5F4.htm "In this exercise, you will use grips to move alignment curves.").

Specify the alignment stations to mask

1.  Open _Align-5.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains a four-way junction. Notice that the offset alignments along Road A are not displayed in the junction area. Each of these alignments has a mask applied to the region that passes through the junction. In the following steps, you will apply a mask to the offset alignments along Road B.
    
    ![](../images/GUID-9B2A5214-CC7D-46F6-B4DD-5A82914EDEF6.png)
    
2.  Select the offset alignment on the north side of Road B.
3.  Click Offset Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Alignment Properties drop-down ![](../images/ac.menuaro.gif)Alignment Properties ![](../images/GUID-11ECBC1E-E4F4-480B-A333-5DA19E67201D.png) Find.
4.  In the Alignment Properties dialog box, click the Masking tab.
5.  On the Masking tab, click ![](../images/GUID-9E69E35C-4186-4535-B5E4-4459E671CDCB.png).
6.  In the drawing, click the end point on the northwest radius kerb to specify the start point of the masked region.
    
    ![](../images/GUID-7CBCB6EA-C030-46D5-9472-E740BB675789.png)
    
7.  Click the end point on the northeast radius kerb to specify the end point of the masked region.
    
    ![](../images/GUID-A94424EB-03BD-4158-9444-101CCF3698A6.png)
    
8.  In the Alignment Properties dialog box, click Apply.
    
    The mask is applied to the specified region.
    
    ![](../images/GUID-5DA32217-B450-4B1E-B8D2-2450E02E65E1.png)
    

**Further exploration:** Apply masks to the west-to-east road offset alignments.

![](../images/GUID-5B8DD83C-B7F6-4497-8DDB-79AEA37D06B0.png)

To continue to the next tutorial, go to [Working with Offset Alignments](GUID-AF7F5C3B-9B2F-4D3A-8F01-CF474DD8D352.htm "This tutorial demonstrates how to create and modify offset alignments that are dynamically linked to a centerline alignment.").

**Parent topic:** [Tutorial: Editing Alignments](GUID-CC717118-AA00-4F58-84B9-A6E5C9D23BDD.htm "This tutorial demonstrates some common editing tasks for alignments.")