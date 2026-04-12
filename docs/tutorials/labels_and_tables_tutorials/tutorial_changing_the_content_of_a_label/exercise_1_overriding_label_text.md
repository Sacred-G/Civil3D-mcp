---
title: "Exercise 1: Overriding Label Text"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-86A6EA39-F7BF-47E3-B04D-F4D190741454.htm"
category: "tutorial_changing_the_content_of_a_label"
last_updated: "2026-03-17T18:43:17.440Z"
---

                 Exercise 1: Overriding Label Text  

# Exercise 1: Overriding Label Text

In this exercise, you will override the text in a single label. Label text overrides are useful for adding text to an individual label to mark a point of interest without modifying all labels that share a style.

Override the text of a label

Note:

This exercise uses _Labels-2b.dwg_ with the modifications you made in the previous exercise, or you can open _Labels-3a.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Zoom and pan to the area between stations 0+000 and 0+100 of the Main Street alignment.
3.  Ctrl+click the geometry point label **PC: 0+035.99**. Right-click. Click Edit Label Text.
    
    Note:
    
    Using Ctrl+click selects only one label in a group. For this exercise, you will override the text of only the PC: 0+035.99 label, and not the other geometry point labels.
    
4.  In the Text Component Editor - Label Text dialog box, place your cursor at the end of the equation in the preview pane. Press Enter.
5.  Enter **N:** in the preview pane.
6.  In the Properties list, select Northing. Click ![](../images/GUID-70B44105-B2EC-4016-A100-FA435F289B52.png) to move the Northing formula, then place your cursor at the end of the equation and press Enter.
7.  Enter **E:** in the preview pane.
8.  In the Properties list, select Easting. Click ![](../images/GUID-70B44105-B2EC-4016-A100-FA435F289B52.png) to move the Easting formula.
    
    The formula in the preview pane should look like this:
    
    <\[Geometry Point Text(CP)\]>: <\[Chainage Value(Um|FS|P2|RN|AP|Sn|TP|B3|EN|WO|OF)\]>
    
    N: <\[Northing(Um|P4|RN|AP|Sn|OF)\]>
    
    E: <Easting(Um|P4|RN|AP|Sn|OF)\]>
    
9.  Click OK.
    
    Notice that the label updates to show the Northing and Easting values at the point of curvature. The other labels at the points of curvature and tangency have maintained their original style settings. To apply this change to the entire group of geometry point labels, you would modify the style that is used by the entire group.
    
    ![](../images/GUID-52EA9513-9FF5-4F2A-BD6B-7400AA3EED94.png)
    
    Geometry point label PC: 0+035.99 with overridden text
    
10.  To return the label to its original style settings, Ctrl+click the label. Right-click. Click Clear Label Text Override.

To continue this tutorial, go to [Exercise 2: Changing Label Content in the Drawing Settings](GUID-DD158750-0EA0-4AC4-8425-5BF67A44823C.htm "In this exercise, you will change the default abbreviations that appear in geometry point labels.").

**Parent topic:** [Tutorial: Changing the Content of a Label](GUID-EB73BA6C-81CD-4FD1-B6BD-40C2FA765EF8.htm "This tutorial demonstrates how to change label text content for an individual label and for a group of labels.")