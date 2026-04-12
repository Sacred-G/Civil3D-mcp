---
title: "Exercise 2: Adjusting Grading Triangulation with a Feature Line"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-5653D269-9D07-48E6-B360-7307EFD5FD8F.htm"
category: "tutorial_using_feature_lines_to_modify_a_grading"
last_updated: "2026-03-17T18:42:43.190Z"
---

                  Exercise 2: Adjusting Grading Triangulation with a Feature Line  

# Exercise 2: Adjusting Grading Triangulation with a Feature Line

In this exercise, you will use a feature line to break a poorly triangulated grading surface.

The triangles in the area on the right side of the ramp are steeper than desired. These triangles can be corrected by creating a feature line to break the surface in this area.

This exercise continues from [Exercise 1: Smoothing Feature Line Arcs](GUID-E6D1CD7A-DD3F-44DC-A8A8-81A0D3BF4B57.htm "In this exercise, you will adjust the tessellation of the arcs around the ramp, which will result in a more accurate representation of the ramp.").

Use a feature line to adjust surface triangulation

Note:

This exercise uses _Grading-7.dwg_ with the modifications you made in the previous exercise.

2.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer drop-down. Next to the **C-TOPO-FEAT-CROSS** layer, click ![](../images/GUID-8E5A9E6C-CDB6-46BA-985A-6F9CCDCE98E8.png).
3.  Zoom in to the area to the right of the ramp.
    
    Notice the polyline between the three circles labeled A, B, and C. You can use the following steps on any polyline. However, this polyline is in a specific location so that you will get the results described in this tutorial.
    
4.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Feature Line drop-down ![](../images/ac.menuaro.gif)Create Feature Lines From Objects ![](../images/GUID-E3383358-E579-45FB-901B-1E1FE45910DA.png) Find.
5.  Click the polyline between Circles A, B, and C. Press Enter.
6.  In the Create Feature Lines dialog box, under Site Name, make sure that **Grading - Building Pad** is selected.
7.  Under Conversion Options, make sure that the Erase Existing Elements and Assign Levels check boxes are selected. Click OK.
8.  In the Assign Levels dialog box, select From Surface. Make sure that the Insert Intermediate Gradient Break Points check box is selected and that Relative Level To Surface is cleared. Click OK.
    
    Note: For more information about using the Relative Level To Surface option to make a feature line relative to a surface, see [About Relative Feature Lines](https://beehive.autodesk.com/community/service/rest/cloudhelp/resource/cloudhelpchannel/guidcrossbook/?v=2025&p=CIV3D&l=ENG&accessmode=live&guid=GUID-62AEE70C-34BA-4B1D-8BD0-E94E4A8C4FF0).
    
    Tip: To reduce processing time when working with larger grading groups, clear the Insert Intermediate Gradient Break Points check box. This option adds an level point at each point at which the feature line crosses a triangle.
    
    The polyline is converted to a feature line and breaks the triangulation along the right side of the ramp. The new triangulation creates a more gradual gradient in the infill area.
    
    ![](../images/GUID-18ADC6CC-7B41-4837-BCD0-65F2DEF5324E.png)
    

To continue this tutorial, go to [Exercise 3: Working with Crossing Feature Lines](GUID-714167AD-A72A-4308-976B-5079ACB6D3F4.htm "In this exercise, you will learn how feature lines interact when they cross each other at and between vertices.").

**Parent topic:** [Tutorial: Using Feature Lines to Modify a Grading](GUID-CB2B6E14-14BA-4EB9-A88B-90E315257D45.htm "This tutorial demonstrates how to use feature lines to control grading around inside corners.")