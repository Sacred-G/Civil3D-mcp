---
title: "Exercise 2: Changing the Profile Style"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E211661C-5334-456F-B5A4-8125D0D64F54.htm"
category: "tutorial_using_surface_profiles"
last_updated: "2026-03-17T18:42:29.620Z"
---

                  Exercise 2: Changing the Profile Style  

# Exercise 2: Changing the Profile Style

In this exercise, you will change a profile style in two different ways.

First, you will change a profile style globally, which changes the profile’s appearance in all profile views. Then, you will learn how to override a profile style in a single profile view. Finally, you will hide the offset profiles.

This exercise continues from [Exercise 1: Creating and Displaying Surface Profiles with Offsets](GUID-769F7E5F-5CBE-47BD-A753-EF2E64163A39.htm "In this exercise, you will create a surface profile from an existing surface.").

Create a profile view

1.  Open Profile-2B.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Profile View Drop-down ![](../images/ac.menuaro.gif)Create Profile View ![](../images/GUID-CA262469-3751-48A0-8B81-D95D8D2148AD.png) Find.
3.  In the Create Profile View wizard, on the General page, under Select Alignment, click **Ridge Road**.
4.  Click Profile Display Options.
5.  Under Specify Profile Display Options, in the Style column, double-click the cell for the Left Offset profile.
6.  In the Pick Profile Style dialog box, select **Standard**. Click OK.
7.  In the Name column, select the first row. Hold down the Shift key, and then select the last row.
8.  In the Labels column, double-click one of the cells.
9.  In the Pick Profile Label Set dialog box, select **<None>**. Click OK.
10.  In the Create Profile View wizard, click Create Profile View.
11.  Pan to a location above the top of the first profile view, then click in the drawing.
     
     The new profile view, PV - (2), is drawn. The left offset profile is the same color as the other two profiles.
     
12.  Pan to the lower profile view, PV - (1).
     
     Notice that its left offset line has also changed. The left offset profile changed because you changed the style of the profile, which affects every instance of the profile in every profile view in the drawing.
     
     ![](../images/GUID-DDC6C326-2A44-46EE-A49C-34884D2CD6E2.png)
     

Change a profile style

1.  Select the PV - (1) profile view grid. Right-click. Click Profile View Properties.
2.  Click the Profiles tab.
    
    On this tab, you can change properties of a profile line after it has been drawn in a profile view.
    
3.  On the Profiles tab, scroll until you can see the Style and Override Style columns.
4.  In the Override Style column for the Left Offset, double-click the check box.
5.  In the Pick Profile Style dialog box, select **Existing Ground**. Click OK.
6.  In the Profile View Properties dialog box, click Apply.
    
    The Left Offset profile changes to red (reflecting the Existing Ground style) in profile view PV - (1), but not in PV - (2). The left offset profiles are different because you overrode the profile style for the particular profile view, but did not change the profile style. You can use a style override to preserve the profile style within a profile view, protecting it from later style changes.
    
    ![](../images/GUID-A8ADB46A-8AD1-42EC-AD41-6BA0FE38FE9D.png)
    
    **Further exploration:** In the Profile View Properties dialog box, clear the Override Style check box for the left offset. Click the Style column for this offset, change it to **Standard**, then click Apply. This changes the style for the profile and affects both profile views in the drawing.
    
    Note:
    
    The left offset line is an approximate and static profile of the terrain along the power line. Optionally, if you wanted to see the actual profile, you could create a profile and profile view based on the Power Line alignment.
    

To continue this tutorial, go to [Exercise 3: Reviewing Surface Profile Characteristics](GUID-A506BE94-5C7C-42F7-A228-4FA7E44BA9D5.htm "In this exercise, you will examine some of the information displayed in the profile and the profile view.").

**Parent topic:** [Tutorial: Using Surface Profiles](GUID-8B3EB320-E48D-4CB7-BD4D-AB887F1CF9B3.htm "This tutorial demonstrates how to create surface profiles and display them in a profile view.")