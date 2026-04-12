---
title: "Exercise 1: Editing the Layout Parameter Values of an Alignment"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-70269D00-79DF-4FB1-B945-FB5A107C11DE.htm"
category: "tutorial_editing_alignments"
last_updated: "2026-03-17T18:42:25.815Z"
---

                  Exercise 1: Editing the Layout Parameter Values of an Alignment  

# Exercise 1: Editing the Layout Parameter Values of an Alignment

In this exercise, you will use the Alignment Elements vista and Alignment Layout Parameters dialog box to edit the layout parameter values of an alignment.

This exercise continues from the [Creating Alignments](GUID-B489AAE6-C5DF-43F7-B6CB-E9E76D7D885C.htm "This tutorial demonstrates how to create and modify alignments.") tutorial.

Note:

Ensure that Dynamic Input (DYN) is turned on. For more information, see the [Dynamic Input](GUID-11DBA8FF-E960-454F-B91F-88A715BD3118.htm#GUID-11DBA8FF-E960-454F-B91F-88A715BD3118__WSFACF1429558A55DE7BEDC1019295BAF0-6877) tutorial.

Open the parametric editing windows

1.  Open _Align-4.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains a surface marked with several circles, labeled A through F.
    
    Note:
    
    Ensure that Object Snap (OSNAP) is turned on.
    
2.  If the Alignment Layout Tools toolbar is not open, select the alignment. Click Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Geometry Editor ![](../images/GUID-483896DA-CE6E-489C-88E7-3A655CB24C3B.png) Find.
3.  On the Alignment Layout Tools toolbar, click Alignment Grid View![](../images/GUID-7BDFFCF6-1694-4995-BDDE-54A6CE78E557.png).
    
    Examine the Alignment Elements table in Panorama. In the first column, alignment segments are numbered in the order in which they are solved. Each row of the table shows useful design data about a specific element. Each curve group has a two-part element number and a separate table row for each transition and curve. Values shown in black can be edited.
    
    Note: To make other parameters available for edit, you can change the Tangency Constraint and Parameter Constraint value of an element.
    
4.  On the Alignment Layout Tools toolbar, click Sub-Element Editor![](../images/GUID-0C471B43-C80E-4F2E-B3A2-6C49B374A5ED.png). The Alignment Layout Parameters window is displayed, containing no data.
5.  In the Alignment Elements table, click any row for segment no. 4, which is the free transition-curve-transition element in circle C in your drawing window.
    
    The design data for all three sub-elements is displayed in a two-column table in the Alignment Layout Parameters window, where data is easy to review and edit.
    
    If your design requires precise values for minimum curve radius, length, or transition A values, you can use the Alignment Layout Parameters window to enter the values.
    
6.  In the Alignment Layout Parameters window, change the Length value for a transition to a higher number, such as 100, and press Enter.
    
    Note how this immediately increases the transition length in three locations: in both the Alignment Layout Parameters window and the Alignment Elements table, numeric values change; in the drawing window, geometry point labels move and their chainage values change, and the length of the transition itself changes.
    
    **Further exploration:** Experiment with changing the curve radius. In the Alignment Elements table, click a line or curve element and note the data that you can edit in the Alignment Layout Parameters window.
    
7.  Press Esc to clear the Alignment Layout Parameters dialog box.

Display a range of sub-elements in the Alignment Elements vista

1.  In the drawing, Ctrl+click the curve element in circle B.
    
    The Alignment Elements vista displays only the attributes for the curve element.
    
2.  In the drawing, Ctrl+click the transition-curve-transition element in circle C.
    
    Notice that the Alignment Elements vista now displays the attributes for each of the elements you selected, plus the straight between them. To display the parameters of another element in the Alignment Layout Parameters dialog box, click the appropriate row in the Alignment Elements vista.
    
3.  Press Esc to display all alignment elements on the Alignment Elements vista.

To continue this tutorial, go to [Exercise 2: Grip Editing an Alignment](GUID-2B7CE63E-F2F6-4C54-AB23-C84491FFB5F4.htm "In this exercise, you will use grips to move alignment curves.").

**Parent topic:** [Tutorial: Editing Alignments](GUID-CC717118-AA00-4F58-84B9-A6E5C9D23BDD.htm "This tutorial demonstrates some common editing tasks for alignments.")