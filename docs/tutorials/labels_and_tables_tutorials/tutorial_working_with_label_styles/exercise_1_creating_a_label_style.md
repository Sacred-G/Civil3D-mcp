---
title: "Exercise 1: Creating a Label Style"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-352C96B3-03CE-4341-9281-5BEB764D7FCF.htm"
category: "tutorial_working_with_label_styles"
last_updated: "2026-03-17T18:43:19.618Z"
---

                  Exercise 1: Creating a Label Style  

# Exercise 1: Creating a Label Style

In this exercise, you will create a label style.

In most cases, the easiest way to create a style is to find an existing style that is similar to the format that you want, create a copy, and then modify the copy.

In the following steps, you will create a design speed label style. You will learn various ways to create and edit label styles using the Autodesk Civil 3DToolspace.

To create a label style

Note:

This exercise uses _Labels-4b.dwg_ with the modifications you made in the previous exercise, or you can open _Labels-5a.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Zoom in to chainage 0+000 of the Main Street alignment.
    
    Examine the design speed label. You will use this label style as a basis to create a new label style that will display the design speed without the chainage label. The design speed labels will be placed at locations where the stations are already labeled.
    
    Note:
    
    Design speed information, including the chainage at which the design speed has been applied, is available in the Alignment Properties dialog box on the Design Speeds tab.
    
3.  Select the design speed label. Right-click. Select Edit Alignment Labels.
4.  In the Alignment Labels dialog box, in the Design Speeds row, in the Style column, click ![](../images/GUID-FFC0BD3D-ED67-45AA-A13D-80611626C064.png).
    
    Note:
    
    You use the Alignment Labels dialog box to create and edit label sets or to import an existing label set.
    
5.  In the Pick Label Style dialog box, click the arrow next to ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png). Click ![](../images/GUID-BD2CA031-6C63-4F6F-925A-6C455E33D4E1.png)Create New.
    
    Note:
    
    ![](../images/GUID-CA1AD018-7C68-4826-8BAB-59D50446DD6F.png)Copy Current Selection uses the format of the current style as a basis for the new style. You will learn about the ![](../images/GUID-D144B2B4-52AD-4F71-9D35-3EB8BA8C6F0B.png)Create Child Of Current Selection option in [Exercise 2: Using a Child Label Style](GUID-C49FC84B-69EC-455A-A563-11C3B8DF3B29.htm "In this exercise, you will create a child label style that derives its default settings from an existing label style, or parent.").
    
6.  In the Label Style Composer dialog box, on the Information tab, specify the following parameters:
    *   Name: **Design Speeds - Inline**
    *   Description: **Small design speed label perpendicular to the alignment**
7.  Click the General tab.
    
    On the General tab, you can specify the settings for the overall label style, including the visibility, layer, and plan readability. For this exercise, accept the default settings on this tab. You can turn on or off the visibility of the individual label components on either the Summary or Layout tab.
    
8.  Click the Layout tab.
    
    On the Layout tab, you specify the content of the label style. A label can be made up of one or more components, each of which can have separate properties.
    
9.  Examine the contents of the Component Name list.
    
    Each component is shown in the Preview pane on the right side of the dialog box. You can use the buttons to the right of the Component Name list to create, copy, or delete label style components. These components were copied when you created the style from the existing Chainage Over Speed style.
    
10.  In the Component Name list, select **Chainage** . In the General collection, change the Visibility to False.
     
     Notice that your change is shown immediately in the Preview pane. This pane is useful when you are designing a label style. If you do not like what you see in the preview, you can change it before saving the style.
     
     Note:
     
     To delete the Chainage component, select it from the Component Name list, and then click ![](../images/GUID-627FB583-4737-43B5-B407-A768EF513E84.png).
     
11.  In the Component Name list, select **Design Speed**. Specify the following parameters:
     
     **General**
     
     *   Anchor Component: **Line**
     *   Anchor Point: **End**
     
     **Text**
     
     *   Attachment: **Bottom Right**
     *   X Offset: **0.0 mm**
     *   Y Offset: **1.0 mm**
12.  In the Text collection, for the Contents property, click the Value column. Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
     
     You can use the Text Component Editor dialog box to define the content and format of a label style text component. The Properties list displays the available properties that can be displayed in a label component. When you select a component from the list, the applicable values are displayed in the table below.
     
13.  In the Text Component Editor dialog box, in the preview pane, select the **DESIGN** text. Press Delete.
14.  Click the **<\[Design Speed(P3|RN|Sn|OF|AP)\]>** property block.
     
     After you have selected the property block in the Preview pane, you can modify the specific values that will be applied to the property.
     
15.  In the Properties list, select **Design Speed**. Change the Precision value to **0.1**. Click ![](../images/GUID-70B44105-B2EC-4016-A100-FA435F289B52.png) to apply the new Precision value to the block in the Preview pane.
     
     Notice that in the block of code, P0 has changed to P1.
     
16.  In the Preview pane, select the SPEED text.
17.  Click the Format tab.
     
     You can change the style, justification, font, and color of each text component.
     
18.  With the text in the Preview pane selected, change the Font to **Times New Roman** . Click OK.
     
     In the Label Style Composer, notice that the SPEED font is different from the design speed value. The design speed value font did not change because it was not selected when you changed the font.
     
19.  Click OK to close the Label Style Composer, Pick Label Style, and Alignment Labels dialog boxes. Pan along the Main Street alignment to view the format of the new label style.
     
     ![](../images/GUID-E9776396-76C5-49EE-9CD2-BF3B58A4D9B2.png)
     
     Label style created from an existing style
     

To continue this tutorial, go to [Exercise 2: Using a Child Label Style](GUID-C49FC84B-69EC-455A-A563-11C3B8DF3B29.htm "In this exercise, you will create a child label style that derives its default settings from an existing label style, or parent.").

**Parent topic:** [Tutorial: Working with Label Styles](GUID-F663DB98-120E-4A8D-9762-CB799972916A.htm "This tutorial demonstrates how to define the behavior, appearance, and content of labels using label styles.")