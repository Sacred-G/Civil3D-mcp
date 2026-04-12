---
title: "Exercise 3: Editing the Grading Criteria"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-ADB0A2D9-1A90-4518-BD22-1E0195758C90.htm"
category: "tutorial_editing_gradings"
last_updated: "2026-03-17T18:42:40.906Z"
---

                 Exercise 3: Editing the Grading Criteria  

# Exercise 3: Editing the Grading Criteria

In this exercise, you will edit a grading criteria and an associated grading adjusts to reflect the criteria change.

You will edit grading criteria attribute values in two ways:

*   In the Level Editor dialog box. Using this method updates the grading criteria for only the currently selected grading.
*   Directly in the Grading Criteria settings. Using this method will apply the attribute changes to future grading created with the criteria. If the attribute value is locked, attribute changes will also be applied to grading that currently use the criteria.

This exercise continues from [Exercise 2: Balancing Cut and Fill Volumes](GUID-74F58D94-A79F-4C4C-B84D-D280CC3AF978.htm "In this exercise, you will adjust the level of a building pad to balance the cut and fill volumes.").

This exercise uses the drawing _Grading-3A.dwg_, which contains two grading groups that use the same grading criteria.

Edit the grading criteria

1.  Open _Grading-3A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Modify tab ![](../images/ac.menuaro.gif)Design panel ![](../images/ac.menuaro.gif)Grading ![](../images/GUID-784292CA-9AF5-443E-A613-BDEA56CD8262.png).
3.  Click Grading tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Grading Editor ![](../images/GUID-7A44959C-35F9-4B39-BB2E-8D13E1AEB0C4.png) Find.
4.  Click inside the grading that projects from feature line AB.
5.  In the Grading Editor Panorama, change the distance value to **5.0000**.
    
    Notice that grading AB updates to reflect the change. Now, you will make a similar change that will affect all grading groups using the **Distance @ -6%** grading criteria.
    
6.  In Toolspace, on the Settings tab, expand Grading![](../images/ac.menuaro.gif)Grading Criteria Sets![](../images/ac.menuaro.gif)Ditch Criteria Set.
7.  Right-click **Distance @ -6%**. Click Edit.
8.  In the Grading Criteria dialog box, click the Criteria tab. Change the Distance parameter to **20** and click ![](../images/GUID-8E80C8A9-C81E-4982-8D81-84D255AE57FC.png). This locks the attribute value, which will apply it to all grading that currently use the Distance @ -6% grading criteria. Leaving the value unlocked applies the value to only grading created in the future.
9.  Click OK.
    
    The AB and BC grading are both updated in the drawing to account for the new criteria value.
    

To continue to the next tutorial, go to [Grading from a Complex Building Footprint](GUID-5CC7A47C-5448-40FF-83D6-4057E3AB143C.htm "This tutorial demonstrates how to gradient around a building footprint that has relatively complicated geometry and variations in level.").

**Parent topic:** [Tutorial: Editing Gradings](GUID-F4CD7511-980F-4935-ABDD-9FB1C1967829.htm "This tutorial demonstrates common grading editing tasks, including level adjustment and grading criteria editing.")