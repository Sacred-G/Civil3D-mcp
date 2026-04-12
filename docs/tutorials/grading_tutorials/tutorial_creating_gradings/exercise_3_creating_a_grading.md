---
title: "Exercise 3: Creating a Grading"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-CC753396-5BFA-4FEE-9122-968B3ED5F9B5.htm"
category: "tutorial_creating_gradings"
last_updated: "2026-03-17T18:42:39.762Z"
---

                 Exercise 3: Creating a Grading  

# Exercise 3: Creating a Grading

In this exercise, you will create a set of gradings, called a grading group, that form a runoff on the side of an embankment.

This exercise continues from [Exercise 2: Assigning Feature Line Levels](GUID-C8D3F575-8367-4B04-9537-D8868C948E59.htm "In this exercise, you will assign levels to the feature lines you created from AutoCAD lines in the previous exercise.").

Create a grading group and specify grading creation settings

Note:

This exercise uses _Grading-2.dwg_ with the modifications you made in the previous exercise, or you can open _Grading-3.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Grading drop-down ![](../images/ac.menuaro.gif)Grading Creation Tools ![](../images/GUID-FCA44E9F-DE55-4BFD-9FFC-6D475EA0389E.png) Find.
3.  In the Grading Creation Tools toolbar, click ![](../images/GUID-A76D3F3C-DE06-4F23-B3CE-86A10979C19C.png)Set The Grading Group.
4.  In the Create Grading Group dialog box, specify the following parameters:
    *   Name: **Ditch Drainage**
    *   Automatic Surface Creation: **Selected**
    *   Volume Base Surface: **Selected**
5.  Click OK.
6.  In the Create Surface dialog box, click OK.
7.  In the Grading Creation Tools toolbar, click ![](../images/GUID-1FB50A49-43C1-43CD-9F5A-17FD818E4157.png)Select a Criteria Set.
8.  Select **Ditch Criteria Set** from the list. Click OK.
9.  In the Select A Grading Criteria list, ensure that **Distance @ -6%** is selected.
10.  Click ![](../images/GUID-1E5D0577-3361-4904-8C59-5B4823488488.png) to expand the Grading Creation Tools toolbar.
11.  In the Style list, select **Ditch** .

Create gradings

1.  Click ![](../images/GUID-FCA44E9F-DE55-4BFD-9FFC-6D475EA0389E.png)Create Grading. Click feature line AB.
2.  In response to the command-line prompt, click above the feature line to indicate where to apply the grading.
3.  Press Enter (Yes) to apply the grading to the entire length of the feature line.
4.  Press Enter to accept the default distance (10 feet).
    
    The grading is created. Ditch Drainage is added to the grading groups collection in Site 1 on the ToolspaceProspector tab. This grading creates one side of a ditch, extending down from the baseline at a 6% gradient for a distance of 10 feet.
    
5.  Press Esc to end the command.
    
    In the next few steps, you will create another grading from the target line of the first grading to the existing surface.
    
6.  Set your display so that feature line AB fills most of the drawing window.
7.  Click the Select a Grading Criteria list and click **Surface @ 4-1 Gradient.**
8.  Click ![](../images/GUID-FCA44E9F-DE55-4BFD-9FFC-6D475EA0389E.png)Create Grading and click the red target line from the first grading that you created.
    
    ![](../images/GUID-4F85A14D-B460-41D1-B45C-BB8A3E74D4BF.png)
    
9.  Enter Yes to apply the grading to the entire length of the line.
10.  Press Enter to accept the cut gradient (4:1).
11.  Press Enter to accept the fill gradient (4:1).
12.  Press Esc to end the command.
     
     ![](../images/GUID-EAA5D55B-8D49-423A-8797-67B4FE69303C.png)
     
     The grading is created. This grading creates a 4:1 gradient up from the bottom of the ditch to the surface. Your results may differ from the illustration.
     
     Note:
     
     The Event Viewer might notify you that duplicate points have been ignored. A surface was created from the two gradings, which share a common feature, and therefore share point data. The daylight line of the first grading is the baseline of the second grading. When the surface is created, the data from the points was extracted for each grading. Because the point data is duplicate, the data of one of the two instances of each point is ignored.
     
     To continue to the next tutorial, go to [Editing Gradings](GUID-F4CD7511-980F-4935-ABDD-9FB1C1967829.htm "This tutorial demonstrates common grading editing tasks, including level adjustment and grading criteria editing.").
     

**Parent topic:** [Tutorial: Creating Gradings](GUID-59CA5821-CC8F-499A-8F89-655B6D41CA0F.htm "This tutorial demonstrates how to create a feature line and how to gradient from the feature line.")