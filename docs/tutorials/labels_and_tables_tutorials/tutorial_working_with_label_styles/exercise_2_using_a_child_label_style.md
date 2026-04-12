---
title: "Exercise 2: Using a Child Label Style"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-C49FC84B-69EC-455A-A563-11C3B8DF3B29.htm"
category: "tutorial_working_with_label_styles"
last_updated: "2026-03-17T18:43:19.784Z"
---

                 Exercise 2: Using a Child Label Style  

# Exercise 2: Using a Child Label Style

In this exercise, you will create a child label style that derives its default settings from an existing label style, or parent.

In the following steps, you will create a child of an existing alignment label style, and modify some of the child label style properties. You will examine the results, and then override some of the child style properties with those of the parent label style.

This exercise continues from [Exercise 1: Creating a Label Style](GUID-352C96B3-03CE-4341-9281-5BEB764D7FCF.htm "In this exercise, you will create a label style.").

Create a child label style

Note:

This exercise uses _Labels-5a.dwg_ with the modifications you made in the previous exercise.

2.  Pan to chainage 0+080 on the West Street alignment.
3.  Ctrl+click the chainage label **0+080**. Right-click. Click Label Properties.
4.  In the Properties palette, on the Design tab, click the value for Major Chainage Label Style. Select Create/Edit from the list.
5.  In the Label Style dialog box, change the style to **Perpendicular With Line**.
    
    Note:
    
    Make sure that you select Perpendicular With Line, and not Perpendicular With Tick.
    
6.  Click the arrow next to ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png). Click ![](../images/GUID-D144B2B4-52AD-4F71-9D35-3EB8BA8C6F0B.png)Create Child Of Current Selection.
7.  In the Label Style Composer, on the Information tab, change the style name from **Perpendicular With Line \[Child\]** to **Chainage Emphasis**.
8.  On the Layout tab, under Component Name, select **Chainage** .
9.  Under Text specify the following parameters:
    *   Text Height: **5.00mm**
    *   Attachment: Middle Left
10.  Click OK twice.
     
     The chainage is now displayed in large text that is attached to the end of the line. The size and position of the text are properties of the new style, Chainage Emphasis. The contents of the text, and the color and length of the line are properties of the parent style, Perpendicular With Line.
     
11.  Press Esc to deselect the label.
12.  In the drawing, Ctrl+click the label **0+100**. Change its style to **Perpendicular With Line** , using the Properties palette.
     
     Notice that in the style collection displayed in the Properties palette, the new style is indented under its parent. You can also see this placement in Toolspace, on the Settings tab, under Alignment![](../images/ac.menuaro.gif)Label Styles![](../images/ac.menuaro.gif)Chainage![](../images/ac.menuaro.gif)Major Chainage![](../images/ac.menuaro.gif)**Perpendicular With Line**.
     
     ![](../images/GUID-61FF24B9-139F-4744-8375-D132FC756E62.png)
     
     Parent (STA: 0+100) and child (STA: 0+080) label styles
     
     Notice that in the drawing, the **STA:0+080** and **STA:0+100** label text are different sizes, and the text and line are aligned differently. A child style shares its basic properties with the parent style from which it was created. If a property value changes in the parent style, then the change is also applied to the child style. If a property value changes in the child style, the parent style is not affected.
     
     By creating a child style, you have created the chainage display you need, without affecting other parts of the drawing. If you had instead changed the properties of the parent style, it would have automatically changed the appearance of any other major stations using that style.
     
     Next, you will set the parent style to override the text setting in the child style.
     

Override the child label style

1.  In Toolspace, on the Settings tab, expand the Alignment![](../images/ac.menuaro.gif)Label Styles![](../images/ac.menuaro.gif)Chainage![](../images/ac.menuaro.gif)Major Chainage collections.
2.  Right-click **Perpendicular With Line**. Click Edit.
3.  In the Label Style Composer dialog box, click the Summary tab.
4.  Expand the Component 1 property.
5.  In the Text : Text Height row, click ![](../images/GUID-1CDAB802-11BB-489A-9D01-8AB3FF65C57B.png) in the Child Override column.
    
    In the Child Override column, ![](../images/GUID-BB5A38FC-86C5-431D-A89C-2F2630C71556.png) is displayed, indicating that the previously independent property of the child has been overridden by the parent.
    
6.  Click OK.
    
    Notice that the labels now use the same text size. The STA: 0+080 label text is now the same size as the STA: 0+000 label because the text height of the parent style has overridden that of the child style. The alignment of the line is still different because the text attachment point of the child style was not overridden.
    
    ![](../images/GUID-484B33B4-62E4-4CF4-9B22-5CA846795576.png)
    
    Parent (STA: 0+100) and child (STA: 0+080) label styles, with child text size overridden by parent
    

To continue this tutorial, go to [Exercise 3: Controlling Label Appearance Using Layers](GUID-DD1F2293-A6D5-4449-B21D-D13FF731EDB3.htm "In this exercise, you will use layers to change the color and visibility of labels.").

**Parent topic:** [Tutorial: Working with Label Styles](GUID-F663DB98-120E-4A8D-9762-CB799972916A.htm "This tutorial demonstrates how to define the behavior, appearance, and content of labels using label styles.")