---
title: "Exercise 4: Overriding the Style of a Pipe Network Part in a Profile View"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-3D789523-BE04-4DA1-8B16-D855F2B920FD.htm"
category: "tutorial_viewing_and_editing_pipe_networks"
last_updated: "2026-03-17T18:43:08.440Z"
---

                 Exercise 4: Overriding the Style of a Pipe Network Part in a Profile View  

# Exercise 4: Overriding the Style of a Pipe Network Part in a Profile View

In this exercise, you will change the style used by pipe network parts in a profile view using override settings found in the profile view properties.

This exercise continues from [Exercise 3: Editing Pipe Network Parts in a Profile View](GUID-9651FF33-B254-47B5-96F5-6EAF285B1047.htm "In this exercise, you will edit the pipe network parts drawn in a profile view using editing grips and by directly editing the part properties.").

Override the style of an object in a profile view

Note:

This exercise uses _Pipe Networks-3B.dwg_ with the modifications you made in the previous exercise.

2.  In the drawing, zoom to the profile view of ROAD1 (PV- (1)).
3.  Click the profile view to select it. Right-click. Click Profile View Properties.
4.  On the Pipe Networks tab, click the **Pipe - (6)** row.
5.  Scroll to the right until you can see the Style Override value.
6.  Click the Style Override cell.
7.  In the Pick Pipe Style dialog box, select **Dotted**. Click OK.
8.  Repeat steps 5 and 6 to override style for **Pipe - (7)**, **Pipe - (8)**, and **Pipe - (9)**.
9.  Click OK.
    
    The pipes that cross the road are now displayed using dotted lines, making it easier to view and edit the main pipe segment. You may have to enter **REGEN** on the command line to see the style change.
    

Change the structure style display in profile

1.  Pan and zoom until you can clearly see the structure at chainage 8+00.
    
    Notice that there is no indication of the pipe segment that travels along the XC-STORM alignment. In the next few steps, you will change the structure style to display where a perpendicular pipe connects to a structure.
    
2.  Click the structure to select it. Right-click. Select Edit Structure Style.
3.  In the Structure Style dialog box, on the Display tab, in the View Direction list, select Profile. Make the Structure Pipe Outlines component visible and change its color to red.
4.  Click OK. Notice that a red circle now appears in the structure. The red circle indicates the location of a perpendicular connection of a pipe that is not displayed in the profile view.

To continue this tutorial, go to [Exercise 5: Viewing Pipe Network Parts in a Section View](GUID-194541AC-E060-4B46-BDA8-05CA66286D77.htm "In this exercise, you will view the pipe network parts in a section view.").

**Parent topic:** [Tutorial: Viewing and Editing Pipe Networks](GUID-DCDFFC24-DBD6-4A45-9F9D-7EFEAB45123F.htm "This tutorial demonstrates how you can view and edit the parts of your pipe network in profile and section views.")