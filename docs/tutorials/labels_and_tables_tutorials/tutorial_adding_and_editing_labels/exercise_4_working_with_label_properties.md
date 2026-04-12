---
title: "Exercise 4: Working with Label Properties"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-F008B12C-2785-4B13-AE7E-7DA67C6B112E.htm"
category: "tutorial_adding_and_editing_labels"
last_updated: "2026-03-17T18:43:16.621Z"
---

                 Exercise 4: Working with Label Properties  

# Exercise 4: Working with Label Properties

In this exercise, you will use standard AutoCAD tools to control properties for both individual labels and group labels.

You can change the properties of:

*   an individual label object
*   a label object group

This exercise continues from [Exercise 3: Selecting and Moving Labels](GUID-EF8C30C7-55A7-4EF5-B3A3-CC25F4E061BC.htm "In this exercise, you will select labels and change their location in the drawing.").

Examine label properties

Note:

This exercise uses _Labels-2b.dwg_ with the modifications you made in the previous exercise.

2.  On the command line, enter **LIST**.
3.  On the West Street alignment, click a chainage label and a quadrant bearing over distance label. Press Enter.
    
    The AutoCAD Text Window appears, displaying the object type, label type, and layer of the selected label objects.
    
    For example, in AECC\_ALIGNMENT\_CHAINAGE\_LABEL\_GROUP
    
    *   ALIGNMENT is the object type
    *   chainage\_LABEL indicates that it is a chainage label object
    *   GROUP indicates that the label object is part of a label group
    
    AECC\_ALIGNMENT\_TANGENT\_LABEL is not followed by GROUP because it is an individual object label.
    
4.  Close the AutoCAD Text Window.
    
    Next, you will change the label style of an individual label object.
    

Change the style of a single label

1.  Click the curve label near chainage 0+040. Right-click. Click Properties.
2.  In the Properties palette, change the Curve Label Style to Curve Label Style![](../images/ac.menuaro.gif) **Design Data** .
3.  Press Esc to deselect the label.

Change the style of a group of labels

1.  Click chainage label **0+040**.
    
    All chainage labels are highlighted, indicating that they are part of a label group.
    
2.  Right-click. Click Properties.
    
    Note:
    
    Selecting Edit Alignment Labels from the context menu opens the Alignment Labels dialog box, where you can change the alignment label set.
    
3.  In the Properties palette, under Labeling, change the Major Chainage Label Style to **Perpendicular With Tick** .
4.  Press Esc.

Flip a label to the opposite side of the alignment

1.  Ctrl+click geometry point label **PC: 0+018.54**.
2.  In the Properties palette, examine the properties that are available. Under General, change the Flipped property to True. Close the Properties palette.
3.  Press Esc to deselect the labels.
    
    ![](../images/GUID-4A15D390-F9E5-45B3-8456-238C7A1CA355.png)
    
    Alignment labels with modified properties
    

To continue to the next tutorial, go to [Changing the Content of a Label](GUID-EB73BA6C-81CD-4FD1-B6BD-40C2FA765EF8.htm "This tutorial demonstrates how to change label text content for an individual label and for a group of labels.").

**Parent topic:** [Tutorial: Adding and Editing Labels](GUID-38C5B56B-B2A1-49EB-8BD6-1BB1715EEB54.htm "This tutorial demonstrates how to add labels to Autodesk Civil 3D objects, and then edit the labels to suit your requirements.")