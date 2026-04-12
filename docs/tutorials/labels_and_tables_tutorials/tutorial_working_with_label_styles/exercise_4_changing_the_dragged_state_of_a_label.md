---
title: "Exercise 4: Changing the Dragged State of a Label"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-057893BF-B66F-4F60-8ABC-A0C935E986F9.htm"
category: "tutorial_working_with_label_styles"
last_updated: "2026-03-17T18:43:20.061Z"
---

                 Exercise 4: Changing the Dragged State of a Label  

# Exercise 4: Changing the Dragged State of a Label

In this exercise, you will modify a label style so that a label will display differently when it is dragged from its original location.

Every label style has two default formats: one for when the label is initially created in its normal layout location, and another that is applied when you drag the label away from its normal layout location. The controls for editing the dragged state of a label style are similar to those used for the layout state.

This exercise continues from [Exercise 3: Controlling Label Appearance Using Layers](GUID-DD1F2293-A6D5-4449-B21D-D13FF731EDB3.htm "In this exercise, you will use layers to change the color and visibility of labels.").

Drag a label from its original location

Note:

This exercise uses _Labels-5b.dwg_ with the modifications you made in the previous exercise.

2.  Zoom in so that you can see the **0+040** chainage label on the West Street alignment. Click the label to select it and all the other major chainage labels.
3.  Click the grip and drag it away from the alignment.
    
    The label is now in its dragged state. The text and leader line are red because the dragged state of the label style is set to ByLayer. Keep this label in view so you can see the effects of format changes as you make them.
    
    ![](../images/GUID-6A20975F-7FEC-4C41-AEB2-CAFBCC429C66.png)
    
    Major chainage label 0+040 in dragged state
    

Change the dragged state of the label

1.  In Toolspace, on the Settings tab, expand Alignment![](../images/ac.menuaro.gif)Label Styles![](../images/ac.menuaro.gif)Chainage![](../images/ac.menuaro.gif)Major Chainage. Right-click **Perpendicular With Tick** . Click Edit.
2.  In the Label Style Composer dialog box, click the Dragged State tab.
    
    In both the Leader and Dragged State Components categories, notice that the Color, Linetype, and Lineweight properties are all either ByLayer or ByBlock. These settings indicate that when a label is in dragged state, it inherits these properties from either the layer on which the label resides or the block that contains it.
    
    Tip:
    
    Because the Preview pane does not show the dragged state of the label, position the Label Style Composer so that you can see a dragged-state label in the drawing. Each time you change a label property, you can click Apply to see the effects.
    
3.  In the Dragged State Components area, change the Display value to As Composed. Click Apply.
    
    The label returns to the original layout property settings, and all other style controls in this area are disabled. This type of dragged-state format is easy to apply, but is not suitable for all label types. In particular, note that the leader line does not adapt well to all possible dragged locations.
    
4.  Change the Display value back to Stacked Text. Click Apply.
    
    If your label has multiple lines of text, the Stacked Text setting keeps them all stacked horizontally in a compact block. You can see this in action by dragging one of the curve labels from its layout location.
    
5.  Change each of the following property values. Click Apply after each change to see their effects.
    *   Leader Type: **Spline Leader**
    *   Border Visibility: **True**
    *   Border Type: **Rounded Rectangular**
    *   Border and Leader Gap: **2.0mm**
    *   Leader Attachment: **Top Of Top Line**
6.  After you have applied all the changes that you want to see in the dragged state, click OK.
    
    ![](../images/GUID-4C1ECE8F-7ED4-4BBB-A5D7-181AF94988EB.png)
    
    Modified dragged state: major chainage label 0+040
    
    Note:
    
    To return a dragged label to its original layout format, click the ![](../images/GUID-BA4AF8A7-E690-4D89-8A4B-B4A5F2B1BA7A.png) grip.
    

To continue this tutorial, go to [Exercise 5: Changing a Label Style](GUID-FA8618F8-FD86-45B5-8405-169BAC49B2C2.htm "In this exercise, you will learn several ways to change label styles.").

**Parent topic:** [Tutorial: Working with Label Styles](GUID-F663DB98-120E-4A8D-9762-CB799972916A.htm "This tutorial demonstrates how to define the behavior, appearance, and content of labels using label styles.")