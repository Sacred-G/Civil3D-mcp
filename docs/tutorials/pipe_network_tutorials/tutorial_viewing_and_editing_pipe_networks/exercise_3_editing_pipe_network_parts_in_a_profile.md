---
title: "Exercise 3: Editing Pipe Network Parts in a Profile View"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-9651FF33-B254-47B5-96F5-6EAF285B1047.htm"
category: "tutorial_viewing_and_editing_pipe_networks"
last_updated: "2026-03-17T18:43:08.313Z"
---

                  Exercise 3: Editing Pipe Network Parts in a Profile View  

# Exercise 3: Editing Pipe Network Parts in a Profile View

In this exercise, you will edit the pipe network parts drawn in a profile view using editing grips and by directly editing the part properties.

This exercise continues from [Exercise 2: Adding Labels to Pipe Network Parts](GUID-A30B903F-AC71-4ECB-965B-B06578E788C8.htm "In this exercise, you will add labels to the pipe network parts drawn in both plan and profile views.").

Grip edit pipe network parts in profile view

Note:

This exercise uses _Pipe Networks-3.dwg_ with the modifications you made in the previous exercise, or you can open _Pipe Networks-3B_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  In the profile view for ROAD1 (PV - (1)), click Pipe - (4), which connects the structures that are offset from stations 11+00 and 12+50 on ROAD1.
3.  Click the square editing grip at the center of the pipe to make it active. Click a new vertical location for the pipe so the start invert level is approximately 650 feet.
    
    Note:
    
    The horizontal grid line immediately below the pipe indicates 650 feet.
    
    As you move your cursor, the tooltip displays the current level of the active grip.
    
4.  Click the triangular grip at the start (downslope) end of the pipe to make it active. Click a new position for the pipe end so that it aligns with the end invert level of Pipe - (3), which is between stations 11+00 and 9+50.

Parametrically edit pipe network parts in profile view

1.  With Pipe - (4) still selected, right-click. Click Pipe Properties.
2.  In the Pipe Properties dialog box, on the Part Properties tab, click the Start Invert Level value to select it. Press Ctrl+C to copy the value.
3.  Click OK.
4.  Press Esc to deselect Pipe - (4).
5.  Click Pipe - (3) to select it. Right-click. Click Pipe Properties.
6.  In the Pipe Properties dialog box, on the Part Properties tab, under Geometry, click the End Invert Level value to select it. Press Ctrl+V to replace the value with the one copied from Pipe - (4).
7.  Click OK.
    
    Pipe - (3) and Pipe - (4) now connect to Structure - (4) at the same invert level.
    
8.  Press Esc to deselect the pipe.
    
    Tip:
    
    You also can use OSNAPs to quickly match pipe start and end levels. For more information, see the [Using Basic Functionality tutorial exercise](GUID-11DBA8FF-E960-454F-B91F-88A715BD3118.htm "In this tutorial, you will learn how to navigate around Autodesk Civil 3D and how to use some common features of the interface.").
    
    **Further exploration:** Repeat the editing procedures with Pipe - (5), which is between stations 12+50 and 13+00, to create a continuous invert level for the pipes.
    

To continue this tutorial, go to [Exercise 4: Overriding the Style of a Pipe Network Part in a Profile View](GUID-3D789523-BE04-4DA1-8B16-D855F2B920FD.htm "In this exercise, you will change the style used by pipe network parts in a profile view using override settings found in the profile view properties.").

**Parent topic:** [Tutorial: Viewing and Editing Pipe Networks](GUID-DCDFFC24-DBD6-4A45-9F9D-7EFEAB45123F.htm "This tutorial demonstrates how you can view and edit the parts of your pipe network in profile and section views.")