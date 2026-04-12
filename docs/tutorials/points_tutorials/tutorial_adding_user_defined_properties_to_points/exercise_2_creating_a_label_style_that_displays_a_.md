---
title: "Exercise 2: Creating a Label Style That Displays a User-Defined Property"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-18D4A323-FF35-4CC0-B3F6-DB093F999F2D.htm"
category: "tutorial_adding_user_defined_properties_to_points"
last_updated: "2026-03-17T18:42:08.487Z"
---

                 Exercise 2: Creating a Label Style That Displays a User-Defined Property  

# Exercise 2: Creating a Label Style That Displays a User-Defined Property

In this exercise, you will create a label style that displays user-defined property information for a point.

This exercise continues from [Exercise 1: Creating User-Defined Properties](GUID-0D327FB1-5B88-4904-9CBA-5F1FB0BF0B1E.htm "In this exercise, you will learn how to create a user-defined property classification and add items to it.").

Create a label style that displays user-defined property information

1.  Open Points-4b.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Settings tab, expand the Point collection. Expand the Label Styles collection.
3.  Under Label Styles, right-click **Standard**. Click Copy.
4.  In the Label Style Composer, on the Information tab, for Name, enter **Manhole UDP**.
5.  On the Layout tab, in the Preview list on the upper right side of the tab, select Point Label Style.
    
    Now, any edits you make to the point label style will be displayed in the preview pane.
    
6.  Click ![](../images/GUID-C4E590B4-3053-4520-B972-96072CAA7BFB.png) to create a text component for the label.
7.  For the new text component, specify the following parameters:
    *   Name: **Invert In**
    *   Anchor Component: **Point Description**
    *   Anchor Point: **Bottom Left**
    *   Attachment (under Text): **Top Left**
8.  Under Text, for Contents, click the default value. Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
9.  In the Text Component Editor – Contents dialog box, on the Properties tab, specify the following parameters:
    *   Properties: **MH\_Pipe In Invert**
    *   Precision: 0.01
10.  Click ![](../images/GUID-70B44105-B2EC-4016-A100-FA435F289B52.png).
11.  In the text editing window, delete the text “Label Text” from the label. Enter **Invert In:** before the property field, which is enclosed in angle brackets(<>). The text in the editor should look like this:
     
     ![](../images/GUID-332A5229-4A0B-4D9B-AEA7-346C08AF12AF.png)
     
12.  Click OK.
13.  In the preview pane, your label should look like this:
     
     ![](../images/GUID-8E09672B-B24F-42C7-8C97-EEFB9DF9533F.png)
     
14.  Click OK.

To continue this tutorial, go to [Exercise 3: Assigning User-Defined Properties to Points](GUID-9EEC2A7E-9995-4642-979C-589646C5594B.htm "In this exercise, you will use point groups to associate user-defined properties with points in your drawing.").

**Parent topic:** [Tutorial: Adding User-Defined Properties to Points](GUID-6F9EFF4C-4D8F-478B-A246-3FCC3B14230A.htm "This tutorial demonstrates how to add custom properties to points.")