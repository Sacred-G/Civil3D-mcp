---
title: "Exercise 5: Viewing Pipe Network Parts in a Section View"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-194541AC-E060-4B46-BDA8-05CA66286D77.htm"
category: "tutorial_viewing_and_editing_pipe_networks"
last_updated: "2026-03-17T18:43:08.674Z"
---

                  Exercise 5: Viewing Pipe Network Parts in a Section View  

# Exercise 5: Viewing Pipe Network Parts in a Section View

In this exercise, you will view the pipe network parts in a section view.

This exercise continues from [Exercise 4: Overriding the Style of a Pipe Network Part in a Profile View](GUID-3D789523-BE04-4DA1-8B16-D855F2B920FD.htm "In this exercise, you will change the style used by pipe network parts in a profile view using override settings found in the profile view properties.").

Create a sample line

Note:

This exercise uses _Pipe Networks-3B.dwg_ with the modifications you made in the previous exercise, or you can open _Pipe Networks-3C_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  In the drawing, zoom to the area around chainage 12+60 on the ROAD1 alignment.
3.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Sample Lines ![](../images/GUID-69FD1393-7647-473B-87EB-9D583771DCCB.png) Find.
4.  Press Enter when prompted to select an alignment.
5.  In the Select Alignment dialog box, select **ROAD1**. Click OK.
6.  In the Create Sample Line Group dialog box, click OK.
7.  On the command line, enter **1260**.
8.  For the left swath width, enter **20**.
9.  For the right swath width, enter **20**.
    
    The sample line is created at the specified chainage.
    
10.  Press Enter to end the sample line creation command.

Create a section view

1.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Section Views drop-down ![](../images/ac.menuaro.gif)Create Section View ![](../images/GUID-23E75D99-7B04-447B-921A-D72C5F4D0327.png) Find.
2.  In the Create Section View wizard, click Section Display Options.
3.  On the Section Display Options page, in the Select Sections To Draw list, in the ROAD1\_SURF row, click the Change Labels cell.
4.  In the Select Style Set dialog box, select **No Labels**. Click OK.
5.  Repeat Steps 3 and 4 to apply the No Labels label style set to the entries in the Change Labels column.
6.  Click Create Section View.
7.  Zoom and pan to a location for the section view.
8.  Click to create the section view at your selected location.
9.  If a warning event is displayed, close the Event Viewer window.
10.  Zoom and pan to the section view to see the pipe network parts in the section view.

To continue this tutorial, go to [Exercise 6: Creating Pipe and Structure Tables](GUID-D11CD8CC-F499-49CC-A417-A639AC141B6A.htm "In this exercise, you will create a table that displays information about the structures in a pipe network. Then, you will create a table style to display other information.").

**Parent topic:** [Tutorial: Viewing and Editing Pipe Networks](GUID-DCDFFC24-DBD6-4A45-9F9D-7EFEAB45123F.htm "This tutorial demonstrates how you can view and edit the parts of your pipe network in profile and section views.")