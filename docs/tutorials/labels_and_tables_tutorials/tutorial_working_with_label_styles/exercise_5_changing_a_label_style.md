---
title: "Exercise 5: Changing a Label Style"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-FA8618F8-FD86-45B5-8405-169BAC49B2C2.htm"
category: "tutorial_working_with_label_styles"
last_updated: "2026-03-17T18:43:20.226Z"
---

                 Exercise 5: Changing a Label Style  

# Exercise 5: Changing a Label Style

In this exercise, you will learn several ways to change label styles.

Depending on the type of label type you must change, and how many labels of that label type you must change, you can change label styles at the following levels:

*   Single label objects, including alignment curve and straight labels.
*   Groups of label objects, including alignment chainage and geometry point labels.
*   Individual label objects that are part of a group, such as a single alignment chainage label.
*   Individually labeled objects in the Prospector list view, including plots, points, and pipe network objects.
    
    See [Exercise 1: Creating a Plot Area Table](GUID-16D213DA-AF50-4DA8-8FB3-F2007DB11BF9.htm "In this exercise, you will create a table to display information about plot objects.") for information about changing plot area label styles using the Prospector list view.
    

This exercise continues from [Exercise 4: Changing the Dragged State of a Label](GUID-057893BF-B66F-4F60-8ABC-A0C935E986F9.htm "In this exercise, you will modify a label style so that a label will display differently when it is dragged from its original location.").

Change the label style of a single label object

Note:

This exercise uses _Labels-5b.dwg_ with the modifications you made in the previous exercise, or you can open _Labels-5c.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Pan and zoom to the area at the end of the West Street cul-de-sac.
3.  Click the red curve label between stations 0+200 and 0+220 on West Street.
    
    Grips appear on the curve label, but no other labels are selected. This label is a single label object that is not part of a group of labels.
    
4.  Right-click the label. Click Properties.
5.  On the Properties palette, in the Curve Label Style list, notice that you can select a General Curve Label Style. General styles can be applied to line and curve labels that annotate alignment, profile, or plot segments. Select General Curve Label Style![](../images/ac.menuaro.gif)**Radius Only**.
    
    The individual curve label style changes, but the remaining curve labels on the cul-de-sac are not affected.
    
6.  Press Esc to deselect the label.
    
    ![](../images/GUID-56771A0E-96CD-4B71-8CDB-5C0058E03A25.png)
    
    Curve label style changed
    

Change the label style of a group of label objects

1.  Click the **0+220** chainage label.
    
    Grips appear on all the chainage labels, indicating that chainage label 0+220 is part of the chainage label group.
    
2.  Right-click the label. Click Properties.
3.  On the Properties palette, in the Major Chainage Label Style list, select **Parallel With Tick**.
    
    The label style changes for all major chainage labels on West Street.
    
4.  Press Esc to deselect the major chainage labels.
    
    ![](../images/GUID-A0248E5C-1C79-4457-98CD-AA9687D22F2A.png)
    
    Major chainage label type group style changed
    

Change the label style of an individual label object that is part of a group

1.  Ctrl+click the **0+060** chainage label.
    
    A grip appears only on label 0+060. The other major chainage labels are not selected.
    
2.  Right-click the label. Click Properties.
3.  On the Properties palette, in the Major Chainage Label Style list, select **Perpendicular With Line**.
    
    The label style changes for only chainage label 0+060. The rest of the major chainage labels on West Street retain their current style.
    
4.  Press Esc to deselect the label.
    
    ![](../images/GUID-4BD47E62-9A4F-4BDD-B3DD-1D53CAE5A4C8.png)
    
    Major chainage label type style changed: STA: 0+060 only
    

To continue this tutorial, go to [Exercise 6: Creating a Label Style that Refers to Another Object](GUID-B15073FC-DABF-42A5-9CE6-7C18F77C7E83.htm "In this exercise, you will use the reference text component to create a single label style that annotates two objects of different types.").

**Parent topic:** [Tutorial: Working with Label Styles](GUID-F663DB98-120E-4A8D-9762-CB799972916A.htm "This tutorial demonstrates how to define the behavior, appearance, and content of labels using label styles.")