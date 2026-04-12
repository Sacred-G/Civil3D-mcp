---
title: "Exercise 3: Adding Floating Curves to an Alignment"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-882EC195-0160-4195-88F6-2BDBD47B6481.htm"
category: "tutorial_creating_alignments"
last_updated: "2026-03-17T18:42:25.193Z"
---

                   Exercise 3: Adding Floating Curves to an Alignment  

# Exercise 3: Adding Floating Curves to an Alignment

In this exercise, you will add two floating curve elements to a simple alignment. First, you will add a best fit floating curve that follows the most likely path through a series of points. Then, you will add a floating reverse curve with transitions.

The initial drawing shows a simple alignment consisting of three straights with curves. In the next few steps, you will add two floating curves to the end of the alignment.

This exercise continues from [Exercise 2: Adding Free Curves and Transitions to an Alignment](GUID-2133A9D7-9CE9-4BFF-A248-C71BA0756457.htm "In this exercise, you will add a free curve and a free transition-curve-transition to a simple alignment.").

Add a floating curve by best fit to the alignment

Note:

This exercise uses _Align-2.dwg_ with the modifications you made in the previous exercise, or you can open _Align-3.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Pan and zoom until you can see circles D and E on the surface.
3.  On the Home tab ![](../images/ac.menuaro.gif)Layers panel, in the Layer Control list, in the **C-POINTS** row, click ![](../images/GUID-8E5A9E6C-CDB6-46BA-985A-6F9CCDCE98E8.png) to turn on the C-POINTS layer.
    
    A group of points is displayed. You will use these points as a basis for creating a floating curve that follows the most likely path through the points.
    
    Note:
    
    If the points do not appear, enter **REGEN** on the command line.
    
4.  If the Alignment Layout Tools toolbar is not open, select the alignment. Right-click and click Edit Alignment Geometry.
5.  In the Alignment Layout Tools toolbar, click the drop-down list ![](../images/GUID-B1B6FEF0-4F07-4BBB-A036-85F833FD4493.png). Select ![](../images/GUID-1CBA69B8-0EEF-4178-95D1-E975411653DF.png)Floating Curve - Best Fit.
6.  As prompted on the command line, click the straight that ends in circle D (the ‘element to attach to’).
7.  In the Curve By Best Fit dialog box, make sure that From COGO Points is selected. Click OK.
8.  The command line prompts you to Select Point Objects or \[Numbers/Groups\]. Enter **G**.
9.  In the Point Groups dialog box, select \_All Points. Click OK. The points in the drawing are selected.
    
    The Regression Data vista displays information about each of the points that are included in the regression analysis. The drawing displays a dashed red line indicating the path of the best fit curve. An X marks the location of each regression data point.
    
    Note:
    
    The point numbers in the Pt No. column are sequentially generated as regression data points are added or removed. They do not correspond to the actual Autodesk Civil 3D point numbers.
    
    Now you will modify some of the regression data to better suit your design.
    
    ![](../images/GUID-F9327288-B897-4538-8318-73F041098835.png)
    
10.  In the Regression Data vista, select the Pass Through check box for Pt No. 16.
     
     Selecting this check box specifies that if the curve does not deviate from the regression data, it will always pass through point 16.
     
11.  Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to create the best fit floating curve.
     
     The Regression Data vista closes, and the regression point markers disappear.
     

Modify the best fit curve

1.  In the drawing, select the alignment. Click the ![](../images/GUID-4FD21E5E-7D79-4634-86C6-EE5629747C33.png) grip at the end of the floating curve, and drag it toward one of the other points. Click to place the grip in its new location.
2.  In the Alignment Layout Tools toolbar, click ![](../images/GUID-7BDFFCF6-1694-4995-BDDE-54A6CE78E557.png)Edit Best Fit Data For All Entities.
    
    Notice that the original regression data is displayed in the Regression Data vista, and the original best fit curve is displayed in the drawing. The ![](../images/GUID-9ECE03CD-D682-4912-943B-9E54DEDA464D.png) icon in the Regression Data vista indicates that the alignment layout does not comply with the regression data. When you moved the grip in step 1, you moved the pass-through point from the location you specified earlier.
    
    ![](../images/GUID-1CB37056-052C-4A24-A95F-6110D9B63CF5.png)
    
3.  Click ![](../images/GUID-3F3CBB6F-BB56-4A20-A842-19D06B22372A.png) to synchronize the element to the original regression data. Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to close the Regression Data vista.
    
    The element returns to its original location.
    
4.  On the Home tab ![](../images/ac.menuaro.gif)Layers panel, in the Layer Control list, in the **C-POINTS** row, click ![](../images/GUID-6FD80C03-88AB-44CD-B810-0480DFACC30F.png) to turn off the C-POINTS layer.

To add a floating reverse curve with transitions to the alignment

1.  On the Alignment Layout Tools toolbar, click the arrow next to ![](../images/GUID-60D70570-457B-49A5-884E-7940344C78BD.png). Select ![](../images/GUID-635A87DC-BFA0-42B2-A05F-B02DBD70E924.png)Floating Reverse Curve with Transitions (From Curve, Radius, Through Point).
2.  As prompted on the command line, click the curve element that ends in circle E (the ‘curve to attach to’).
3.  Enter a transition in length of **75**.
4.  Enter a radius of **200**.
5.  Enter a transition out length of **75**.
6.  Specify a pass-through point in circle F.
    
    The reverse curve with transitions appears.
    
7.  Exit the layout command by right-clicking in the drawing.
    
    ![](../images/GUID-5C6FF43B-4F2D-4A02-BBB7-E175B86F4415.png)
    

To continue to the next tutorial, go to [Tutorial: Editing Alignments](GUID-CC717118-AA00-4F58-84B9-A6E5C9D23BDD.htm "This tutorial demonstrates some common editing tasks for alignments.").

**Parent topic:** [Tutorial: Creating Alignments](GUID-B489AAE6-C5DF-43F7-B6CB-E9E76D7D885C.htm "This tutorial demonstrates how to create and modify alignments.")