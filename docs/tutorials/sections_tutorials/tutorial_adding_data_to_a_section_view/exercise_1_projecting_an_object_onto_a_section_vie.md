---
title: "Exercise 1: Projecting an Object onto a Section View"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-0F40AF94-E87A-424A-A3F1-A619AA940590.htm"
category: "tutorial_adding_data_to_a_section_view"
last_updated: "2026-03-17T18:42:59.097Z"
---

                  Exercise 1: Projecting an Object onto a Section View  

# Exercise 1: Projecting an Object onto a Section View

In this exercise, you will project multi-view blocks and 3D polylines from plan view onto section views.

A variety of AutoCAD and Autodesk Civil 3D objects can be projected into a section view. However, linear objects, such as 3D polylines and feature lines, are represented as a marker that indicates the point location where the object crosses the sample line in plan.

Project objects onto multiple section views

1.  Open Section-Project-Objects.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    A proposed corridor and a series of section views are displayed in two viewports in the drawing. Along the corridor, several other objects are visible:
    
    *   Red polylines on either side of the corridor represent the corridor highway boundary.
    *   A blue polyline along the right side of the corridor represents a fence.
    *   AutoCAD blocks that represent utility poles are displayed along the right side of the corridor.
    *   Multi-view blocks that represent a line of trees are displayed along the left side of the corridor.
2.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Section Views drop-down ![](../images/ac.menuaro.gif)Project Objects To Multiple Section Views ![](../images/GUID-99833CA5-9B08-4A77-AB0C-0CC7C1CE1250.png) Find.
3.  In the top viewport, select one of the section views.
    
    The Project Objects To Multiple Section Views dialog box is displayed. Under Projection Rules, you may specify the proximity area for non-linear objects to be projected.
    
4.  Under Projection Rules, select By Distance.
    
    This option specifies that objects that are a specified distance before or after a sample line are projected.
    
5.  For Distance Before and Distance After, enter **50**.
6.  In the table, under Name, clear all check boxes except Blocks and 3D Polylines.
7.  In the Blocks row, specify the following parameters:
    *   Style: **Projection Without Exaggeration**
    *   Level Options: Surface![](../images/ac.menuaro.gif)**Corridor - (1) Surface - (1)**
    *   Label Style: **Offset and Level**
8.  In the Blocks and 3D Polylines rows, specify the following parameters:
    *   Style: **Projection Without Exaggeration**
    *   Level Options: Surface![](../images/ac.menuaro.gif)**EG**
    *   Label Style: **Offset and Level**
9.  Click OK.
    
    The 3D polylines are displayed on the section views. The labels annotate the offset and level at which each object is projected.
    
    ![](../images/GUID-391F2A53-A77E-4520-93A8-39F5E84E413C.png)
    
10.  In the bottom viewport, zoom in to the area between chainages 7+00.00 and 13+00.00.
     
     At sample lines SL-07, SL-08, SL-12, and SL-13, examine the proximity of the utility pole blocks. You specified a distance of 50 feet as the projection distance, which means that specified objects that are within 50 feet of a sample line will be projected.
     
     In the top viewport, notice that a utility pole block does not appear in section view 13+00.00. This section view does not display a utility pole block because one does not appear within 50 feet before or after the sample line.
     

Project objects onto a single section view

1.  In the top viewport, pan and zoom until you see section 13+00.00.
2.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Section Views drop-down ![](../images/ac.menuaro.gif)Project Objects To Section View ![](../images/GUID-F2E5FD97-6992-4CE0-8946-AFCE6FD5733F.png) Find.
3.  In the bottom viewport, select the multi-view block that represents a tree at chainage 13+00.
    
    ![](../images/GUID-3408EB79-7E5B-45D0-A2BA-2F4722E7D831.png)
    
4.  Press Enter.
5.  In the top viewport, click section view 13+00.
6.  In the Project Objects To Section View dialog box, click ![](../images/GUID-F96C62F3-280E-4986-9F77-040900D6C1AE.png)<Set All> in each column to specify the following parameters:
    *   Style: **Projection Without Exaggeration**
    *   Level Options: Surface![](../images/ac.menuaro.gif)**EG**
    *   Label Style: **Offset and Level**
7.  Click OK.
    
    The block is displayed on the section view.
    
    ![](../images/GUID-3D87DDE4-35FF-4398-ADA0-3E38479E2947.png)
    

Edit the level of a projected object

In the following steps, you will change the level of the tree and fence marker so that they reflect the level of the corridor surface.

2.  In the top viewport, click the blue marker that indicates the level of the fence.
    
    When you select the marker in the section view, notice that the blue 3D polyline in plan is highlighted.
    
3.  Click the ![](../images/GUID-E9EABD3F-968A-477D-B830-5E2B970CBF95.png) grip. Drag the grip down to change the level of the 3D polyline.
    
    ![](../images/GUID-03CA756D-993C-4F7E-BE07-63F7CE09C54C.png)
    
    When you click to place the grip, you are notified that the level option for the polyline will be changed to manual. This option enables you to specify an level value for an object at the current chainage. The level value is applied in the current section view, but the value does not affect the object in plan view.
    
4.  In the task dialog box, click No.
5.  Press Esc.
6.  Select section view 13+00. Right-click. Click Section View Properties.
    
    The Projections tab is displayed on the Section View Properties dialog box. You can use the controls on this tab to change the parameters you used when you projected objects onto the section view.
    
    Note:
    
    Like other Autodesk Civil 3D labels, label parameters are changed by selecting the desired label, and then using the Labels contextual tab on the ribbon.
    
7.  In the Section View Properties dialog box, on the Projections tab, under ![](../images/GUID-FC65CCCC-40CC-4211-A290-56E6EA37F2D6.png)3D Polylines, select the **3D Polyline- 23** row.
    
    When you select the row, notice that the corresponding object is highlighted in both plan and section views.
    
8.  In the Level Options column, change the value to ![](../images/GUID-6BA9AC63-A03A-493C-8716-35AD405BF1FC.png)Surface![](../images/ac.menuaro.gif)![](../images/GUID-6BA9AC63-A03A-493C-8716-35AD405BF1FC.png)**Corridor - (1) Surface - (1)**.
9.  Repeat Steps 6 and 7 to change the level of ![](../images/GUID-3C705B89-C8A4-4D7A-83EA-F8D2ED0A678E.png)Multi View Blocks![](../images/ac.menuaro.gif)![](../images/GUID-3C705B89-C8A4-4D7A-83EA-F8D2ED0A678E.png)**Eastern White Pine- 21** to reference **Corridor - (1) Surface - (1)**.
10.  Click OK.
     
     In the section view, notice that the fence marker and tree are now at the corridor surface level.
     
     ![](../images/GUID-6C3318D9-A692-468B-9B58-D417717C37B3.png)
     

Project an object that is at a different chainage

1.  In the bottom viewport, pan until you can see sample lines **SL-3** and **SL-8**.
2.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Section Views drop-down ![](../images/ac.menuaro.gif)Project Objects To Section View ![](../images/GUID-F2E5FD97-6992-4CE0-8946-AFCE6FD5733F.png) Find.
3.  In the bottom viewport, select the blocks that represents trees at SL-3 and SL-8.
4.  Press Enter.
5.  In the top viewport, click section view 13+00.
6.  In the Project Objects To Section View dialog box, click ![](../images/GUID-F96C62F3-280E-4986-9F77-040900D6C1AE.png)<Set All> in each column to specify the following parameters:
    *   Style: **Projection Without Exaggeration**
    *   Level Options: Surface![](../images/ac.menuaro.gif)**Corridor - (1) Surface - (1)**
    *   Label Style: **Offset Level**
7.  Click OK.
    
    The tree is displayed on the section view. Two things are evident:
    
    *   Only one of the blocks was projected onto the section view. Objects in a site can be projected to a section view only if the object falls within the perpendicular swath width at the specified sample line. The block at SL-3 was not projected onto the section view because the block offset value is greater than the perpendicular swath that is encompassed by SL-13. The black, dashed lines in the following image illustrate the sample line extents at SL-13.
        
        ![](../images/GUID-B7864171-8D20-4AB3-8906-C797CE628C63.png)
        
    *   The block from SL-8 is shown at an level that appears to be above the surface. The block is projected at the surface level where it is actually located, and not at the surface level at the current sample line. However, the offset value that is displayed in the label reflects the object’s offset value at the current sample line.
    
    ![](../images/GUID-899CB6C3-41AC-4226-A138-265E63280B30.png)
    

**Further exploration**: Examine the style settings that are available for projected objects. Projected object styles are located in Toolspace, on the Settings tab, in the General![](../images/ac.menuaro.gif)Multipurpose Styles![](../images/ac.menuaro.gif)Projection Styles collection. Label styles for projected objects are located in Toolspace, on the Settings tab, in the Section View![](../images/ac.menuaro.gif)Label Styles![](../images/ac.menuaro.gif)Projection collection.

To continue this tutorial, go to [Exercise 2: Adding a Section View Gradient Label](GUID-21890D53-6A5B-427F-88A0-EBD61C712EC2.htm "In this exercise, you will create a section view gradient label.").

**Parent topic:** [Tutorial: Adding Data to a Section View](GUID-F9CF9DD7-EB09-41E2-BCEE-586730811558.htm "This tutorial demonstrates how to add annotative data to a section view.")