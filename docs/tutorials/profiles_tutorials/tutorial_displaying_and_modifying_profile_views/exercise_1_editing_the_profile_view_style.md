---
title: "Exercise 1: Editing the Profile View Style"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6EA3E98E-4E1F-4C82-ACFC-CA56B3492784.htm"
category: "tutorial_displaying_and_modifying_profile_views"
last_updated: "2026-03-17T18:42:31.738Z"
---

                 Exercise 1: Editing the Profile View Style  

# Exercise 1: Editing the Profile View Style

In this exercise, you will learn how to change the data displayed in a profile view.

You use the Profile View Style dialog box to define profile view styles that control the format for titles, axis annotation, and other elements of a profile view.

The Profile View Properties dialog box is the central location in which you can modify all components of the profile view, including profiles, labels, styles, and data boxes.

You can also do some common editing tasks, such as deleting profiles or modifying profile labels, within the drawing window by right-clicking the appropriate object.

Change the profile view style

1.  Open Profile-5A.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click the grid to select profile view profile view PV- (1). Right-click. Click Profile View Properties.
3.  Click the Information tab.
4.  In the Object Style field, change the profile view style to **Major Grids**. Click Apply.
    
    Notice that this style change affects the X-axis annotation as well as the grid.
    
5.  Click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection.
6.  In the Profile View Style dialog box, examine the contents of the various tabs to see the many settings that can be included in a style definition.
    
    For example, on the Title Annotation tab, you can change the format and location of the profile view title. On the Display tab, you can turn various parts of the profile view on and off.
    
7.  Click Cancel, and then click OK to close the Profile View Properties dialog box.
8.  Zoom out so you can see both profile views.
    
    When you applied the Major Grids style, which includes grid padding above and below the profile, PV- (1) enlarged to overlap PV- (2).
    
9.  Click PV- (2) and move it up above the title block for PV- (1).
    
    ![](../images/GUID-B3749417-47B4-43AC-8EBB-5405F46C4B51.png)
    
    Now you will edit the Major Grids profile view style to add consistent grid padding, clip the profile view grid, and modify the appearance of the ticks along the axis.
    

Modify the grid in the profile view style

1.  Click the PV - (1) grid to select the profile view. Right-click. Click Edit Profile View Style.
2.  In the Profile View Style dialog box, on the Grid tab, under Grid Padding, change the padding of all four axes to **1.0000**.
3.  Click Apply.
    
    Notice that in the drawing, there is now one full major grid between the profiles and the profile view extents.
    
    ![](../images/GUID-FD34336B-E11F-4FC4-BB25-489D2EE05D2D.png)
    
4.  Under Grid Options, select Clip Vertical Grid and Clip Horizontal Grid. Under both selections, select Omit Grid In Padding Areas.
    
    Notice that the graphics in the dialog box change to demonstrate the effect the setting has on the profile view.
    
5.  Click Apply.
    
    Notice that in the drawing, the profile view grid has been removed from above the surface profile and the padding area you specified.
    
    ![](../images/GUID-81069454-B1A3-4EE3-A992-2792AA076041.png)
    
    The grid is clipped to the surface profile because the Clip Grid setting in the profile view properties for PV - (1) is set to the centerline surface profile. Setting the profile view style to Clip To Highest Profile(s) would override the property setting and clip the grid to the layout profile.
    
6.  Click OK.
    
    Note:
    
    If the style changes have not been applied to the profile view, enter REGEN at the command line.
    

Modify the axis annotation in the profile view style

1.  Pan and zoom to see the upper left corner of the profile view grid. Zoom in so you can clearly see the tick marks on the horizontal and vertical axes.
    
    Notice that the starting chainage labels overlap. In the next few steps, you will correct the overlap and modify the justification of the ticks at the major chainages.
    
    ![](../images/GUID-32A93455-3260-487D-B54A-001A972C58DF.png)
    
2.  Click the grid to select the profile view. Right-click. Click Edit Profile View Style.
3.  In the Profile View Style dialog box, on the Horizontal Axes tab, click Top.
    
    This control sets the focus of the controls on this tab to the top axis. If, after you have changed the top axis, you would like the changes to carry over to the bottom axis, select Bottom and repeat the changes.
    
    Note:
    
    The bottom axis controls the major and minor grid spacing.
    
4.  Under Major Tick Details, specify the following parameters:
    *   Tick Size: **0.2500**
    *   Y Offset: **0.1000**
5.  Click Apply.
    
    The major ticks are longer, and the chainage labels move up.
    
    ![](../images/GUID-6B52DD69-04A9-4798-8444-3C6E1746342B.png)
    
6.  On the Vertical Axes tab, make sure Left is selected as the axis to control.
    
    Note:
    
    The left axis controls the major and minor grid spacing.
    
7.  Under Major Tick Details, specify the following parameters:
    *   Tick Size: **0.2500**
    *   X Offset: **\-0.1000**
8.  Click OK.
    
    The ticks are longer, and the level labels move to the left.
    
    ![](../images/GUID-F416D6AA-3A9F-4124-A886-43DFE9FE1482.png)
    
    **Further exploration:** Experiment with the other settings in the Major Tick Details area. Make the same changes you made in the previous steps to the right axis.
    

To continue this tutorial, go to [Exercise 2: Adding Hatch Patterns Between Profiles](GUID-0FEFEE68-DD81-4DC7-BFC6-1F33F6BDB594.htm "In this exercise, you will illustrate the cut and fill regions along an alignment by applying hatch patterns between the surface and layout profiles.").

**Parent topic:** [Tutorial: Displaying and Modifying Profile Views](GUID-0BF95BEA-BDFF-4B0A-A73C-6749A5FFD1C5.htm "This tutorial demonstrates how to change the appearance of profile views.")