---
title: "Exercise 2: Balancing Cut and Fill Volumes"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-74F58D94-A79F-4C4C-B84D-D280CC3AF978.htm"
category: "tutorial_editing_gradings"
last_updated: "2026-03-17T18:42:40.780Z"
---

                  Exercise 2: Balancing Cut and Fill Volumes  

# Exercise 2: Balancing Cut and Fill Volumes

In this exercise, you will adjust the level of a building pad to balance the cut and fill volumes.

This exercise continues from [Exercise 1: Editing the Grading Level](GUID-6C1968B7-168D-4608-BBE1-1C03413E0CA0.htm "In this exercise, you will edit the level of a grading baseline. The grading adjusts to reflect the level change.").

Balance cut and fill volumes

Note:

This exercise uses _Grading-4.dwg_ with the modifications you made in the previous exercise, or you can open _Grading-5.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Click Analyze tab ![](../images/ac.menuaro.gif)Volumes And Materials panel ![](../images/ac.menuaro.gif)Grading Volume Tools ![](../images/GUID-EC321B26-FC39-4946-8809-3339134B8FE9.png) Find.
    
    On the Grading Volume Tools toolbar, notice that the Group is set to Building Pad, which is the only grading group in the drawing. The fields for Cut, Fill, and Net show that the grading as designed requires the net cutting and removal of a large volume of surface material.
    
3.  Click ![](../images/GUID-55575653-9C10-49FD-9E94-16F801C405B0.png)Raise the Grading Group to raise the building pad level by one foot. Note the changes to cut and fill requirements.
    
    **Further exploration:** You can also click ![](../images/GUID-74B49193-9953-467F-9CA2-69E0BF28D1A1.png)Lower the Grading Group and you can change the level increment to a value other than 1.0.
    
4.  Click ![](../images/GUID-67C07B41-8196-45EA-83A4-709D96CFB3E1.png)Automatically Raise/Lower to Balance the Volumes.
5.  In the Auto-Balance Volumes dialog box, leave the Required Volume set to 0, or change the value if you wish. Click OK.
    
    The level of the building pad is adjusted to bring the Net amount as close as possible to the set value.
    
6.  Click ![](../images/GUID-1E5D0577-3361-4904-8C59-5B4823488488.png) (Expand the Grading Volume Tools). The history of your level changes and their effects is displayed.

To continue this tutorial, go to [Exercise 3: Editing the Grading Criteria](GUID-ADB0A2D9-1A90-4518-BD22-1E0195758C90.htm "In this exercise, you will edit a grading criteria and an associated grading adjusts to reflect the criteria change.").

**Parent topic:** [Tutorial: Editing Gradings](GUID-F4CD7511-980F-4935-ABDD-9FB1C1967829.htm "This tutorial demonstrates common grading editing tasks, including level adjustment and grading criteria editing.")