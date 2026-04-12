---
title: "Exercise 6: Creating a Label Style that Refers to Another Object"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-B15073FC-DABF-42A5-9CE6-7C18F77C7E83.htm"
category: "tutorial_working_with_label_styles"
last_updated: "2026-03-17T18:43:20.341Z"
---

                 Exercise 6: Creating a Label Style that Refers to Another Object  

# Exercise 6: Creating a Label Style that Refers to Another Object

In this exercise, you will use the reference text component to create a single label style that annotates two objects of different types.

The Reference Text label component is used to annotate multiple types of objects with a single label. You can insert references to surfaces, profiles, plots, and alignments. Each reference text component can refer to only one Autodesk Civil 3D object. If you need a label style to refer to several objects, create a separate reference text component for each referenced object.

In this exercise, you will create a label style that displays the alignment chainage and surface level at each horizontal geometry point.

This exercise continues from [Exercise 5: Changing a Label Style](GUID-FA8618F8-FD86-45B5-8405-169BAC49B2C2.htm "In this exercise, you will learn several ways to change label styles.").

Create a label style that refers to another object

Note:

This exercise uses _Labels-5c.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Settings tab, expand the Alignment![](../images/ac.menuaro.gif)Label Styles![](../images/ac.menuaro.gif)Chainage![](../images/ac.menuaro.gif)Geometry Point collection. Right-click **Perpendicular With Tick And Line**. Click Copy.
3.  In the Label Style Composer dialog box, on the Information tab, for Name, enter **Surface Level at Alignment Chainage**.
4.  Click the Layout tab.
    
    You can use this tab to create and edit label style components. You will modify the existing line and geometry point components, and then create two new label components for the new label style. The first component will display the surface level, and the second component will display the alignment chainage.
    
5.  Under Component Name, select **Line**. Specify the following parameters:
    *   Start Point Anchor Component: **Tick**
    *   Start Point Anchor Point: **Middle Center**
    *   Length: **15.00mm**
6.  Under Component Name, select **Geometry Point & Chainage**. Specify the following parameters:
    *   Anchor Component: **Line**
    *   Anchor Point: **End**
    *   Attachment: **Middle Left**
    *   X Offset: **2.00mm**
7.  Click the arrow next to ![](../images/GUID-C4E590B4-3053-4520-B972-96072CAA7BFB.png). Click ![](../images/GUID-0502DB43-682C-4627-AC68-35BC892A5D0C.png)Reference Text.
    
    A reference text label component refers to other object types in the drawing, instead of to the object type you are labeling. In this case, the reference text component will refer to a surface object.
    
8.  In the Select Type dialog box, select Surface. Click OK.
9.  In the Label Style Composer dialog box, specify the following parameters:
    *   Name: **Level**
    *   Anchor Component: **Geometry Point & Chainage**
    *   Anchor Point: **Bottom Left**
    *   Attachment: **Top Left**
10.  Under Text, in the Contents row, click the Value cell. Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
11.  In the Text Component Editor dialog box, in the preview pane, replace **Label Text** with **EL:**. Under Properties, select **Surface Level** . Click ![](../images/GUID-70B44105-B2EC-4016-A100-FA435F289B52.png) to move the Surface Level property to the preview pane.
     
     The property block in the preview pane should look like this:
     
     EL: <\[Surface Level(Um|P3|RN|AP|Sn|OF)\]>
     
12.  Click OK.
13.  In the Label Style Composer dialog box, click the General tab. Set the Flip Anchors With Text property to True.
     
     Using this setting ensures that when the labels are flipped to maintain plan readability, they will display as mirror images of the original labels.
     
14.  In the Label Style Composer dialog box, click OK.

To apply the label style that refers to another object

1.  In the drawing, pan to the junction of the Main Street and East Street alignments.
2.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Labels menu ![](../images/ac.menuaro.gif)Alignment![](../images/ac.menuaro.gif)Add/Edit Station Labels![](../images/GUID-10094573-05A5-439E-A30D-2E506775AA08.png). Click one of the chainage labels on the East Street alignment.
3.  In the Alignment Labels dialog box, specify the following parameters:
    *   Type: **Geometry Points**
    *   Geometry Point Label Style: **Surface Level At Alignment Chainage**
4.  Click Add.
5.  In the Geometry Points dialog box, click ![](../images/GUID-49C0F964-08E5-4341-8F67-D6C553038A77.png) to clear all check boxes. Select the Alignment Beginning check box. Click OK.
6.  In the Alignment Labels dialog box, click OK.
7.  Drag the label to a clear location, if necessary.
    
    Notice that the label is added to the alignment, but the level value is displayed as ???. These characters are displayed because you have not associated a surface with the Level component.
    
    ![](../images/GUID-E00731C4-A812-4BD0-B8A4-34376B545F3A.png)
    
    Label style that refers to an alignment and surface, with no surface associated with the label
    
8.  Ctrl+click the Surface Level At Alignment Chainage label. Right-click. Click Label Properties.
9.  In the Properties palette, under Reference Text Objects, click the cell to the right of **Surface Level At Alignment Chainage** . Click the drop-down list and select the surface name that appears in the list.
    
    Examine the label. The surface level at the junction of the alignments is now displayed.
    
    ![](../images/GUID-122C1608-630C-4CFF-AFA7-482122BA1850.png)
    
    Label style that refers to an alignment and surface
    
10.  Press Esc to deselect the label.

To continue to the next tutorial, go to [Using Expressions in Labels](GUID-ADE0D8A5-CB73-40C2-B7FF-10E25FEE55F1.htm "This tutorial demonstrates how to use expressions, which are mathematical formulas that modify a property within a label style.").

**Parent topic:** [Tutorial: Working with Label Styles](GUID-F663DB98-120E-4A8D-9762-CB799972916A.htm "This tutorial demonstrates how to define the behavior, appearance, and content of labels using label styles.")