---
title: "Exercise 2: Editing an Offset Alignment"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-1CC17214-81E0-45B5-AC1F-373E00DDCA8A.htm"
category: "tutorial_working_with_offset_alignments"
last_updated: "2026-03-17T18:42:26.578Z"
---

                 Exercise 2: Editing an Offset Alignment  

# Exercise 2: Editing an Offset Alignment

In this exercise, you will examine the offset alignment editing tools.

The geometry editing tools that are available for an offset alignment depend on whether the alignment is static or dynamic. If the offset alignment is dynamically linked to the parent centerline alignment, then you cannot edit the offset alignment geometry. If the offset alignment is static, then you can use the tools on the Alignment Layout Tools toolbar.

An Offset Parameters tab is available in the Alignment Properties dialog box. From this location, you can change parameters such as nominal offset value and start and end stations.

This exercise continues from [Exercise 1: Creating Offset Alignments](GUID-F42CA69D-BFB5-440E-8F56-C9BD7C56D159.htm "In this exercise, you will create dynamic offset alignments for an existing centerline alignment.").

Examine the offset alignment geometry

1.  Open _Align-6B.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains a surface and a centerline alignment with two offset alignments.
    
2.  Select the left offset alignment.
    
    ![](../images/GUID-25E76043-FBD8-46A3-A7F8-C79BCECCC240.png)
    
3.  Click Offset Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Geometry Editor ![](../images/GUID-ACCD8C0B-0AC7-4E4E-B016-46ED56E83423.png) Find.
    
    On the Alignment Layout Tools toolbar, most tools are not available, because the offset alignment geometry is dynamically linked to the parent alignment. You can use the Alignment Elements vista and Alignment Layout Parameters window to view the parameters of a dynamic offset alignment, but you cannot change the values.
    

Edit the offset alignment parameters

1.  Click Offset Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Alignment Properties drop-down ![](../images/ac.menuaro.gif)Alignment Properties ![](../images/GUID-11ECBC1E-E4F4-480B-A333-5DA19E67201D.png) Find.
2.  In the Alignment Properties dialog box, on the Information tab, expand the Type list.
    
    You can change an offset alignment to any of the types in this list. However, if you change the alignment type, the alignment will not be dynamically linked to the centerline alignment.
    
3.  Press Esc.
4.  Click the Offset Parameters tab.
    
    You use this tab to refine the offset alignment design. If you do not want the offset alignment to react to changes in the parent alignment geometry, use the Update Mode list to make the alignment static.
    
    Note:
    
    The Offset Parameters tab is displayed in the Alignment Properties dialog box only for offset alignments.
    
5.  Specify the following parameters:
    *   Nominal Offset Value: **\-24.0000**
    *   End Chainage: **10+00**
6.  Click OK.
    
    The offset alignment now ends at chainage 10+00, and is offset twice as much as the offset alignment on the opposite side of the centerline.
    
    ![](../images/GUID-B2720307-5B82-4F55-9F32-D9C1CF8A1112.png)
    

**Further exploration:** Experiment with the centerline alignment grips. Notice that when you change the centerline alignment geometry, the geometry of the offset alignment automatically updates.

To continue this tutorial, go to [Exercise 3: Adding a Widening to an Offset Alignment](GUID-8208038F-7522-44C1-8DBC-5AAC9D789107.htm "In this exercise, you will add dynamic widening regions between specified stations of an offset alignment.").

**Parent topic:** [Tutorial: Working with Offset Alignments](GUID-AF7F5C3B-9B2F-4D3A-8F01-CF474DD8D352.htm "This tutorial demonstrates how to create and modify offset alignments that are dynamically linked to a centerline alignment.")