---
title: "Tutorial: Displaying and Editing Points"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E43EE235-038D-49EB-8307-6E8D00A9B3A4.htm"
category: "points_tutorials"
last_updated: "2026-03-17T18:42:07.578Z"
---

                  Tutorial: Displaying and Editing Points  

# Tutorial: Displaying and Editing Points

This tutorial demonstrates how to use point groups, layers, external references, and styles to display points. It also explains the various ways to edit points using standard AutoCAD tools.

You can use _point groups_ to organize points and to control their appearance in a drawing. While points are independent objects that do not have to be categorized into specific point groups, every point in a drawing is always part of the \_All Points point group. The point group _display order_ determines which point group’s properties take precedence. For example, if a point belongs to a point group that is higher in the display order than the \_All Points point group, the higher group’s properties override the properties set in the \_All Points point group.

The _point layer_ controls the display attributes of the point. To see this, open the Point Group Properties dialog box, click the Point List tab, and look at the Point Layer column. This column also appears in the Prospector list view when the point group is selected. The point layer can be assigned by using a description key. If a point layer is not assigned during creation, points are placed on the default point layer specified in the drawing settings.

An external reference drawing (_xref_) is a useful way to see points in relation to other surface features without adding these features to your drawing. You can reference another drawing and make it appear as an underlay in your current drawing. Then, you can detach the external drawing when you no longer need it.

Changing the point or label style of a point group can help you distinguish these points more easily from other points in the drawing.

Each point is an object that can be individually selected and manipulated. Point objects have commands, property attributes, and grip behavior that are similar to other AutoCAD elements.

**Topics in this section**

*   [Exercise 1: Displaying an Externally Referenced Drawing](GUID-A0C28000-DFC7-4246-BFF0-76EA43240823.htm)  
    In this exercise, you will use a standard AutoCAD operation to display another drawing of the region around your set of points.
*   [Exercise 2: Changing the Style of a Point Group](GUID-59523BA0-4A5F-4F4F-BC5F-E4900A0CF589.htm)  
    In this exercise, you will change the style of a point group. Point styles can help you distinguish the points more easily from other points in the drawing.
*   [Exercise 3: Changing Point Group Display Order](GUID-3CC8E6EF-660E-4321-B499-5DFEB1E902C8.htm)  
    In this exercise, you will use the point group display order to change the appearance of points.
*   [Exercise 4: Removing an Externally Referenced Drawing](GUID-0BFB632B-5E66-48CF-80AB-6EC0F4D74A81.htm)  
    In this exercise, you will remove the externally referenced drawing that you added previously.
*   [Exercise 5: Editing Points](GUID-0B84CE9C-D349-414D-BF80-5C2143D92BA8.htm)  
    In this exercise, you will move and rotate point objects to improve their position in the drawing.

**Parent topic:** [Points Tutorials](GUID-7EDF7EC4-829A-45B2-970F-AB559275E077.htm)