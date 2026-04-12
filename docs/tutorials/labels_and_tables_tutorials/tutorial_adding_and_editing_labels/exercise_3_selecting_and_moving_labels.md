---
title: "Exercise 3: Selecting and Moving Labels"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-EF8C30C7-55A7-4EF5-B3A3-CC25F4E061BC.htm"
category: "tutorial_adding_and_editing_labels"
last_updated: "2026-03-17T18:43:16.495Z"
---

                  Exercise 3: Selecting and Moving Labels  

# Exercise 3: Selecting and Moving Labels

In this exercise, you will select labels and change their location in the drawing.

Your drawing currently contains many labels, some of which overlap one another. You will move them in the following steps.

Some label types, such as alignment chainage and geometry point labels, are part of a label type group that shares properties. When you click one label in the group, the entire group is selected. Properties for the group are controlled using the AutoCAD Properties palette, which is available when you select the group, right-click, and click Label Properties. You can select the group and then change the properties of all labels in the group simultaneously. Alternatively, you can use Ctrl+click to select and change label properties individually.

Other label types, such as segment labels, are not part of a group. Each of these labels is treated as an individual object.

Labels are distinct objects that are independent of the parent object that they annotate. Labels are dynamically linked to their parent object and automatically update when the parent object changes. However, labels reside on a separate layer and are not selected when you select the parent object.

If a label resides in an externally referenced drawing, the label cannot be edited in the current drawing.

Note:

Point, plot area, corridor, and surface watershed labels are not label objects. They are sub-elements of a parent object and their properties are managed in the Label Properties dialog box.

This exercise continues from [Exercise 2: Manually Labeling an Object](GUID-1297F239-F83C-4BF4-A0BA-6E268D221A4F.htm "In this exercise, you will add labels to specific areas on an alignment after it has been created and automatically labeled.").

Select labels in a drawing

Note:

This exercise uses _Labels-2a.dwg_ with the modifications you made in the previous exercise, or you can open _Labels-2b.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Make sure that you are zoomed in to the area around the **West Street** label on the alignment on the left-hand side of the site.
3.  Select the alignment.
    
    Notice that the alignment is highlighted, and grips appear along the alignment. The alignment labels, however, are not highlighted.
    
    In previous versions of Autodesk Civil 3D, labels were sub-elements of their parent object. Now, most Autodesk Civil 3D labels are independent objects that reside on their own layer. Although they are independent objects, labels are dynamically linked to their parent object and automatically update to reflect changes in the parent object.
    
4.  Press Esc to deselect the alignment.
5.  On the road alignment, click one of the chainage labels.
    
    Notice that all other chainage labels along the alignment are now highlighted.
    
6.  Press Esc to deselect the chainage labels.
7.  Click one of the curve labels.
    
    Notice that while that curve label is selected, the other curve labels are not.
    
    There are two distinct label object types:
    
    *   _Label type groups_—When you select a label that is part of a label type group, such as an alignment chainage label, the entire group is selected. If you select a label type group and use the right-click menu to update the label properties, the properties of every label in the group update. To select a single label within a label type group, use Ctrl+click.
    *   _Single label objects_—When you select a single label object, such as an alignment curve label, only that label object is selected. You can change the properties, including the label style, of single label objects without affecting the other labels of the parent object.
8.  Press Esc to deselect the curve label.

Move labels in the drawing

1.  Click the label showing quadrant bearing over distance near chainage 0+100. Grips appear on the label.
2.  Click the ![](../images/GUID-BE0E5657-F39E-460D-98C9-F1853734A46F.png) label location grip. The grip turns red. Drag the label down and to the left to move it to a clear location. Click to place the label. Press Esc to deselect the label.
    
    ![](../images/GUID-B2FE8696-A7FB-4BD2-A7B3-DFB3D602BB34.png)
    
    Label dragged with the label location grip
    
    A leader line is created from the label to the alignment.
    
3.  Click the **West Street** label. Grips appear on the label.
4.  Click the ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) label anchor grip. The grip turns red. Drag the label to the right. Click near chainage 0+100 to place the label closer to the center of the line segment. Press Esc to deselect the label.
    
    ![](../images/GUID-FF3AED96-6446-4476-9671-AE79C173EE5A.png)
    
    Label moved with label anchor grip
    
5.  Pan and zoom to the cul-de-sac of the West Street alignment.
6.  Use the ![](../images/GUID-BE0E5657-F39E-460D-98C9-F1853734A46F.png) grips to drag each curve label away from the alignment.
7.  Click the **EP: 0+243.63** label. A grip appears on the label.
8.  Click the grip. The grip turns red. Drag the label up and to the right to move it to a clear location.
    
    A leader line is created from the label to the alignment.
    
    Notice that the label displays without a border. The border is hidden because the _dragged state_ of the label style specified a different format for when the label is dragged to a new location. You will learn about label style settings in the [Working with Label Styles tutorials](GUID-F663DB98-120E-4A8D-9762-CB799972916A.htm "This tutorial demonstrates how to define the behavior, appearance, and content of labels using label styles.").
    
    ![](../images/GUID-4E2ACE62-7F0D-45D8-8CF9-9DEDAB5CC834.png)
    
    Alignment end point label displayed in dragged state
    
    Note:
    
    You can reset a selected label by clicking the ![](../images/GUID-BA4AF8A7-E690-4D89-8A4B-B4A5F2B1BA7A.png) circle grip.
    
9.  Press Esc to deselect the EP: 0+243.63 label.
10.  Ctrl+click chainage label **0+000**. Press Delete.
11.  Repeat these operations, moving and dragging labels to other locations where required. If you want to undo any flipping or dragging operations, select the labels in question. Right-click and click Reset Label.
     
     ![](../images/GUID-DE1397BA-A775-4B56-93F8-D973ACF20811.png)
     
     Alignment with labels moved to improve readability
     

Select labels in an Xref

1.  Click one of the chainage labels on the Main Street alignment.
    
    Notice that both the Main Street and East Street alignments and their chainage labels are selected. The labels were selected because the labels were created in the externally referenced drawing in which the alignments reside. When an object has been created through an Xref, you can edit its labels only in the source drawing.
    
2.  Press Esc to deselect the Xref.

To continue this tutorial, go to [Exercise 4: Working with Label Properties](GUID-F008B12C-2785-4B13-AE7E-7DA67C6B112E.htm "In this exercise, you will use standard AutoCAD tools to control properties for both individual labels and group labels.").

**Parent topic:** [Tutorial: Adding and Editing Labels](GUID-38C5B56B-B2A1-49EB-8BD6-1BB1715EEB54.htm "This tutorial demonstrates how to add labels to Autodesk Civil 3D objects, and then edit the labels to suit your requirements.")