---
title: "Exercise 1: Creating and Displaying Surface Profiles with Offsets"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-769F7E5F-5CBE-47BD-A753-EF2E64163A39.htm"
category: "tutorial_using_surface_profiles"
last_updated: "2026-03-17T18:42:29.573Z"
---

                    Exercise 1: Creating and Displaying Surface Profiles with Offsets  

# Exercise 1: Creating and Displaying Surface Profiles with Offsets

In this exercise, you will create a surface profile from an existing surface.

After creating the profile and several offsets, you will create a profile view to display the profiles.

Create centerline and offset surface profiles

1.  Open Profile-2A.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains an existing ground surface and two horizontal alignments. Examine the alignments. The red one with curves is named Ridge Road, and represents a proposed road centerline. The other is named Power Line, and represents a proposed power line offset about 25 feet from the road.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Profile drop-down ![](../images/ac.menuaro.gif)Create Surface Profile ![](../images/GUID-35E8FF53-E099-47DA-9596-6CE0AC7C3547.png) Find.
3.  In the Create Profile From Surface dialog box, under Alignment, select **Ridge Road**.
4.  Click Add.
5.  Select the Sample Offsets check box. In the field next to the check box, enter **25,-25** (including comma).
    
    This field enables you to specify that profiles be created on either side of the centerline. The left offset (–25) will mark the approximate location of the power line. If you wanted more offsets, you could enter a series of them here. Use positive numbers for right offsets and negative numbers for left offsets, with values separated by commas.
    
6.  Click Add.
7.  In the Profile List, in the Description column, enter the following descriptions:
    *   EG - Surface: **Centerline**
    *   EG - Surface - 25.000: **Right Offset**
    *   EG - Surface - -25.000: **Left Offset**
8.  In the Profile List, in the Update Mode column, for the left offset, change the value to Static.
    
    This option specifies that the left offset reflects the surface levels at the time of its creation. It will not update to reflect future changes in the surface.
    
9.  Click OK.
    
    A message that indicates profiles have been created is displayed in the Event Viewer.
    

Display the surface profiles in a profile view

1.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Profile View drop-down ![](../images/ac.menuaro.gif)Create Profile View ![](../images/GUID-CA262469-3751-48A0-8B81-D95D8D2148AD.png) Find.
    
    The Create Profile View wizard is displayed, where you can configure the display of the profile. The wizard contains the many controls for displaying profiles in a profile view. You can use either the Back and Next buttons at the bottom or the links along the left side to navigate through the pages. You can click Create Profile View at any time to accept the settings and create the profile view in the drawing.
    
2.  In the Create Profile View wizard, on the General page, under Select Alignment, select **Ridge Road**.
3.  Click Profile Display Options.
    
    The table on the Profile Display Options page shows the existing profiles for Ridge Road. By default, they are all checked in the Draw column, indicating that they will appear in the profile view.
    
4.  In the Specify Profile Display Options table, in the Style column, double-click the cell for the Left Offset.
5.  In the Pick Profile Style dialog box, change the style to **Existing Ground**. Click OK.
6.  In the Specify Profile Display Options table, in the Labels column, double-click the cell for the Left Offset.
7.  In the Pick Profile Label Set dialog box, change the style to **<None>**. Click OK.
    
    You will not create labels for the existing ground profiles. You will specify a label set when you create a layout profile in the [Using Layout Profiles tutorial](GUID-DF73E4FC-35CB-4D11-96C9-8A73DB24DC45.htm "This tutorial demonstrates how to create and edit layout profiles, which are often called design profiles or finished design profiles.").
    
8.  Repeat Steps 6 and 7 for the other two profiles.
9.  Click Create Profile View.
10.  In the drawing, pan and zoom to a blank area at the lower right of the surface. Click at a suitable location for the lower left corner of the profile view grid.
     
     The profile view is drawn, with a grid, axes, title, and two data boxes along the X axis, one above the grid and another below it.
     
     Because of its style, the left offset line is red.
     
     ![](../images/GUID-3FFFBFFF-62A3-48FB-B01C-A182F0B689FA.png)
     
     Note:
     
     If you want to move a profile view within a drawing, click anywhere on the grid to select it. A blue grip appears near the lower left corner. Click the grip and drag the profile view to a new location.
     
     To continue this tutorial, go to [Exercise 2: Changing the Profile Style](GUID-E211661C-5334-456F-B5A4-8125D0D64F54.htm "In this exercise, you will change a profile style in two different ways.").
     

**Parent topic:** [Tutorial: Using Surface Profiles](GUID-8B3EB320-E48D-4CB7-BD4D-AB887F1CF9B3.htm "This tutorial demonstrates how to create surface profiles and display them in a profile view.")