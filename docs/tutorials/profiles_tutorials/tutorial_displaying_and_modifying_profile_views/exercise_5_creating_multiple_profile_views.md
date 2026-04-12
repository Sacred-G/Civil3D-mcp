---
title: "Exercise 5: Creating Multiple Profile Views"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-3377C4CA-9533-4F60-A877-86218DE10A87.htm"
category: "tutorial_displaying_and_modifying_profile_views"
last_updated: "2026-03-17T18:42:31.991Z"
---

                   Exercise 5: Creating Multiple Profile Views  

# Exercise 5: Creating Multiple Profile Views

In this exercise, you will produce a set of profile views to display short, successive segments of a profile.

Multiple profile views are most useful when you are creating final construction documents from your design. For best results, design your profile in a single profile view, then use the plan production tools to create multiple profile views for plotting or publishing. During the plan production process, you create sheets that display sections of alignments and profiles.

In this exercise, you will bypass the plan production tools to create multiple profile views in a currently open drawing. You will use the Create Multiple Profile Views wizard, which allows you to quickly specify the profile view properties before you create them. If you access this wizard during the plan production process, many of the properties are not available because they are already set in the _view frame group_.

For a tutorial that demonstrates the plan production tools, go to [Plan Production Tutorials](GUID-4E042CFC-F82A-4EBF-85E3-B1AE2A0C1C57.htm).

This exercise continues from [Exercise 4: Splitting a Profile View](GUID-569237B4-951A-4692-9522-88AAF25E027C.htm "In this exercise, you will split a profile view so that the full level range of a layout profile fits in a shorter profile view.").

Create multiple profile views

1.  Open Profile-5E.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains two roads, Maple Road and Oak Road, and a single profile view of Oak Road.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Profile View drop-down ![](../images/ac.menuaro.gif)Create Multiple Profile Views ![](../images/GUID-A8FA3E82-3D03-4602-AAFF-1EE88D335575.png) Find.
3.  In the Create Multiple Profile Views wizard, on the General page, specify the following parameters:
    *   Select Alignment: **Oak Road**
    *   Profile View Style: **Major Grids**
4.  Click Next.
5.  On the Chainage Range page, in the Length of Each View box, enter **500.00’**.
6.  Click Next.
7.  On the Profile View Height page, specify the following parameters:
    *   Profile View Height: **User Specified**
    *   User Specified: **50.00’**
    *   Profile View Datum By: **Mean Level**
        
        This option specifies that the profile lines will be positioned in the profile based on the mean of the highest and lowest level value of the profiles that are drawn in the grid. This option provides an equal amount of space above and below the profile lines. This option is useful when you have to annotate a profile that has fairly consistent level values.
        
    *   Split Long Section View: **Selected**
        
        This option makes the split long section view controls available, which allow you to select separate profile view styles for the first, intermediate, and last segments of any split long section views. For this exercise, accept the default split long section view settings.
        
8.  Click Next.
9.  On the Profile Display Options page, ensure that the Draw check box is selected for both profiles.
10.  In the Oak Road - Proposed row, select the Split At option.
     
     This option specifies that if the profiles contained in the profile view must be split to fit in the specified profile view height, the split will occur at the appropriate level of the layout profile. This option ensures that the entire layout profile appears in the profile view.
     
11.  Click Next to open the Pipe Network Display page.
     
     You can use this page to select the pipe network or parts that you want to display in the profile view. For this exercise, you will not display any pipe network parts.
     
12.  Click Next.
13.  On the Data Boxes page, under Select Band Set, select **EG-FG Levels and Chainages**.
14.  Click Next to open the Profile Hatch Options page.
     
     You can use this page to specify hatch patterns between the profiles in the profile view. For this exercise, you will not specify any hatch patterns.
     
15.  Click Next.
16.  On the Multiple Plot Options page, set Maximum in a Row to **4**.
17.  Click Create Profile Views.
18.  When prompted, pan and zoom to a clear area in the drawing window, then click to create the profile views.
     
     ![](../images/GUID-4A07758A-9866-46A6-8439-0855577A8651.png)
     
19.  In Toolspace, on the Prospector tab, expand the Alignments, Oak Road, and Profile Views collections.
     
     Notice that a separate profile view was created for each 500.00’ segment.
     

To continue this tutorial, go to [Exercise 6: Creating Stacked Profile Views](GUID-378B5E19-40EB-4904-BD7A-1F02F87A15CB.htm "In this exercise, you will create a series of three profile views that contain a centerline and left and right offset profiles.").

**Parent topic:** [Tutorial: Displaying and Modifying Profile Views](GUID-0BF95BEA-BDFF-4B0A-A73C-6749A5FFD1C5.htm "This tutorial demonstrates how to change the appearance of profile views.")