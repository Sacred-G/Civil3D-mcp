---
title: "Exercise 2: Manually Labeling an Object"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-1297F239-F83C-4BF4-A0BA-6E268D221A4F.htm"
category: "tutorial_adding_and_editing_labels"
last_updated: "2026-03-17T18:43:16.366Z"
---

                 Exercise 2: Manually Labeling an Object  

# Exercise 2: Manually Labeling an Object

In this exercise, you will add labels to specific areas on an alignment after it has been created and automatically labeled.

This exercise continues from [Exercise 1: Adding Labels in Groups](GUID-9A8CBBE8-FAE7-461F-B4B5-C35181213F4A.htm "In this exercise, you will use label sets to apply several types of labels to an alignment.").

Label multiple alignment segments

Note:

This exercise uses _Labels-2a.dwg_ with the modifications you made in the previous exercise.

2.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Labels menu ![](../images/ac.menuaro.gif)Alignment![](../images/ac.menuaro.gif)Add Alignment Labels![](../images/GUID-10094573-05A5-439E-A30D-2E506775AA08.png).
3.  In the Add Labels dialog box, specify the following parameters:
    *   Feature: **Alignment**
    *   Label Type: **Multiple Segment**
4.  In the three label style fields, accept the default styles.
    
    You can choose specific styles for each of the Line, Curve, and Transition label styles. This alignment does not contain any transitions, so the Transition Label Style setting is ignored.
    
5.  Click Add.
6.  On the left-hand side of the site, click the West Street alignment, which you created in [Exercise 1: Adding Labels in Groups](GUID-9A8CBBE8-FAE7-461F-B4B5-C35181213F4A.htm "In this exercise, you will use label sets to apply several types of labels to an alignment.").
    
    The Multiple Segment label command places a label at the middle of each line and curve. This method can be convenient, but sometimes a few of the labels overlap other features of the drawing. In [Exercise 3: Selecting and Moving Labels](GUID-EF8C30C7-55A7-4EF5-B3A3-CC25F4E061BC.htm "In this exercise, you will select labels and change their location in the drawing."), you will move some of these labels.
    

Label single alignment segments

1.  In the Add Labels dialog box, specify the following parameters:
    
    *   Feature: **Alignment**
    *   Label Type: **Single Segment**
    *   Line Label Style: Line Label Style![](../images/ac.menuaro.gif) **Alignment Name**
    
    Note:
    
    Notice that you can use either a General Line Label Style or Alignment Line Label Style. General Line and Curve label styles can be applied to lines and curves that are part of an Alignment or Plot object.
    
2.  Click Add.
    
    You are now ready to choose a specific location for a label on the alignment. Unlike multiple segment labels, single segment labels are placed exactly where you click.
    
3.  On the West Street alignment, click near chainage **0+120** to place a line segment label. Be sure to click the line segment, and not the chainage label.
    
    Because you clicked a line segment, the label style specified in the Line Label Style list in the Add Labels dialog box was used. If you had clicked a curve, the label style specified in the Curve Label Style list would have been used.
    
4.  On the Main Street alignment, which is the long alignment in the middle of the site, click a location between stations **0+140** and **0+160**.
5.  On the East Street alignment, which is the alignment with the cul-de-sac on the right-hand side of the site, click a location between stations **0+200** and **0+220**.
6.  In the Add Labels dialog box, in the Line Label Style list, select Line Label Style![](../images/ac.menuaro.gif) **Bearing Over Distance** . Click Add.
7.  On the Main Street alignment, click a location between stations **0+100** and **0+120**. A new label displaying quadrant bearing and distance information is created.

Examine label settings for other objects

1.  In the Add Labels dialog box, in the Feature list, select Plot.
    
    This selection changes the label type and style selections that are available. The Add Labels dialog box works in the same manner for all of the feature types shown in the Feature list. When you annotate objects in Autodesk Civil 3D, you can switch the type of object you are labeling, as well as the label type and style of the various elements, as needed.
    
    Pay attention to the command line as you annotate objects. Some label types, such as span, slope, gradient, and depth, annotate a range of data between two points. For example, if you want to label a gradient or depth between two points, you must specify those points in the drawing.
    
2.  In the Add Labels dialog box, click Close.
    
    ![](../images/GUID-87150260-6B79-4BED-A443-61F88431ADA4.png)
    
    Alignment with manually inserted segment labels
    

To continue this tutorial, go to [Exercise 3: Selecting and Moving Labels](GUID-EF8C30C7-55A7-4EF5-B3A3-CC25F4E061BC.htm "In this exercise, you will select labels and change their location in the drawing.").

**Parent topic:** [Tutorial: Adding and Editing Labels](GUID-38C5B56B-B2A1-49EB-8BD6-1BB1715EEB54.htm "This tutorial demonstrates how to add labels to Autodesk Civil 3D objects, and then edit the labels to suit your requirements.")