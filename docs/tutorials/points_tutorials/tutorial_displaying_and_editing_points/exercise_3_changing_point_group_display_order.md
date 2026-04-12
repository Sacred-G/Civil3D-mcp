---
title: "Exercise 3: Changing Point Group Display Order"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-3CC8E6EF-660E-4321-B499-5DFEB1E902C8.htm"
category: "tutorial_displaying_and_editing_points"
last_updated: "2026-03-17T18:42:07.725Z"
---

                 Exercise 3: Changing Point Group Display Order  

# Exercise 3: Changing Point Group Display Order

In this exercise, you will use the point group display order to change the appearance of points.

Change the point group display order

Note:

This exercise uses Points-2.dwg and Existing Basemap.dwg with the modifications you made in the previous exercise.

2.  In Toolspace, on the Prospector tab, expand the Point Groups collection.
    
    Notice the order of the point groups in the Prospector tree. The point group display order determines how points that belong to more than one point group are displayed in a drawing. When a drawing is opened or regenerated, Autodesk Civil 3D searches down the point group display order to determine how the point will appear.
    
    For example, if a point belongs to all three groups, Autodesk Civil 3D will first look in the Storm Manholes point to determine if a point label style has been assigned to that point group. If it has not, Autodesk Civil 3D will look in the Detention Pond point group, and then the \_All Points group until the point label style setting is found.
    
3.  In Toolspace, on the Prospector tab, right-click the Point Groups collection. Click Properties.
4.  In the Point Groups dialog box, select the **Storm Manholes** point group.
5.  Click ![](../images/GUID-FF195884-3E3F-42E9-AE91-F932A22CC892.png) to move the **Storm Manholes** point group to the bottom of the display order.
6.  Click OK.
    
    Notice that the point style for the STORM MH points has changed to an X, and the label has disappeared. This happened because when the Storm Manholes point group was placed below the \_All Points point group, the \_All Points point group’s point style and point label style settings took precedence over those of the Storm Manholes point group.
    
7.  In Toolspace, on the Prospector tab, right-click the \_All Points point group. Click Properties.
8.  In the Point Group Properties dialog box, on the Overrides tab, select the Point Label Style box. Click OK.
    
    This option ensures that the Point Label Style setting of the point group overrides the Point Label Style setting of the individual points included in the point group.
    
9.  In Toolspace, on the Prospector tab, right-click the Point Groups collection. Click Properties.
10.  In the Point Groups dialog box, select the \_All Points point group. Click ![](../images/GUID-197CFF1E-2B1A-483E-8A38-42185F2C36A1.png) to move the \_All Points point group to the top of the display order.
11.  Click OK.
     
     Notice that all point labels in the drawing are hidden. This happened because the \_All Points point group’s point label style set to **<none>**, and you placed the \_All Points point group at the top of the display order.
     

To continue this tutorial, go to [Exercise 4: Removing an Externally Referenced Drawing](GUID-0BFB632B-5E66-48CF-80AB-6EC0F4D74A81.htm "In this exercise, you will remove the externally referenced drawing that you added previously.").

**Parent topic:** [Tutorial: Displaying and Editing Points](GUID-E43EE235-038D-49EB-8307-6E8D00A9B3A4.htm "This tutorial demonstrates how to use point groups, layers, external references, and styles to display points. It also explains the various ways to edit points using standard AutoCAD tools.")