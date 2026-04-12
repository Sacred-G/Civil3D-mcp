---
title: "Exercise 4: Splitting a Profile View"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-569237B4-951A-4692-9522-88AAF25E027C.htm"
category: "tutorial_displaying_and_modifying_profile_views"
last_updated: "2026-03-17T18:42:31.920Z"
---

                  Exercise 4: Splitting a Profile View  

# Exercise 4: Splitting a Profile View

In this exercise, you will split a profile view so that the full level range of a layout profile fits in a shorter profile view.

This exercise continues from [Exercise 3: Projecting Objects onto a Profile View](GUID-188BE199-D837-4B1C-849C-12C0E992370D.htm "In this exercise, you will project multi-view blocks, COGO points, and 3D polylines from plan view onto a profile view.").

Split a profile view

1.  Open Profile-5D.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Profile View drop-down ![](../images/ac.menuaro.gif)Create Profile View ![](../images/GUID-CA262469-3751-48A0-8B81-D95D8D2148AD.png) Find.
3.  In the Create Profile View wizard, on the General page, specify the following parameters:
    *   Select Alignment: **Ridge Road**
    *   Profile View Style: **Standard**
4.  On the left side of the wizard, click Profile View Height.
5.  On the Profile View Height page, specify the following parameters:
    *   Profile View Height: **User Specified**
    *   Maximum: **670.00’**
    *   Split Long Section View: **Selected**
        
        The split long section view controls are now available. These controls allow you to select separate profile view styles for the first, intermediate, and last segments of the split long section view. For this exercise, accept the default split long section view settings.
        
6.  Click Next.
7.  On the Profile Display Options page, clear the Draw check boxes for all profiles except EG - Surface (1) and Layout (1). In the Layout (1) row, select the Split At option. This option specifies that the split occurs at the appropriate level of the layout profile and ensures that the entire layout profile will appear in the profile view.
8.  Scroll to the right until the Labels column is visible. In the EG - Surface (1) row, click the Labels cell.
9.  In the Pick Profile Label Set dialog box, select **<None>**. Click OK.
10.  Click Create Profile View.
11.  When prompted, pan and zoom to a clear area in the drawing window, then click to create the profile view.
     
     A new profile view is created. Notice that because you specified a shorter maximum height in step 7, the profile view grid is shorter than the other profile views in the drawing. In order to fit the profile in the shorter grid, the profile has been split in two segments. The full length and levels of the red, layout profile are visible because you set the Split At setting to Layout (1) in step 6. Notice that there is a vertical axis in the middle of the profile view that displays the levels for both split segments.
     
     ![](../images/GUID-5764E0A7-5219-4C24-97FF-C5CD7D731008.png)
     

Modify the properties of the split long section view

1.  In Toolspace, on the Prospector tab, expand the Alignments![](../images/ac.menuaro.gif)Centerline Alignments![](../images/ac.menuaro.gif)Ridge Road ![](../images/ac.menuaro.gif)Profile Views collections.
    
    Notice that a single new profile view (PV - (4)) was created.
    
2.  On the Prospector tab, right-click PV - (4). Click Properties.
3.  In the Profile View Properties dialog box, on the Elevations tab, under Level Range, change the Height to **15.000’**.
4.  Click Apply.
    
    Notice that in the drawing window, the profile view has been split into five segments to accommodate the new height.
    
    ![](../images/GUID-C5E03EFB-B99B-499F-85D8-7202A0259A62.png)
    
    Now you will change the style of the first and last profile view segments.
    
5.  In the Profile View Properties dialog box, in the Split Long Section View Data table, in the No. 1 row, in the Profile View Style column, click ![](../images/GUID-0B54F71A-E223-46A6-BC7E-7B1673D8BF80.png).
6.  In the Pick Profile View Style dialog box, select **Left & Bottom Axis**. Click OK.
7.  Repeat steps 5 and 6 to change the Profile View Style in row No. 4 to **Full Grid**.
8.  Click OK.
    
    Three different profile view styles are displayed in the single profile view grid. While a split long section is displayed in a single profile view grid, it may have separate styles applied to each of its split segments.
    
    ![](../images/GUID-CA5E0BC1-0DA8-4261-880B-8FEA3DBACD08.png)
    

To continue this tutorial, go to [Exercise 5: Creating Multiple Profile Views](GUID-3377C4CA-9533-4F60-A877-86218DE10A87.htm "In this exercise, you will produce a set of profile views to display short, successive segments of a profile.").

**Parent topic:** [Tutorial: Displaying and Modifying Profile Views](GUID-0BF95BEA-BDFF-4B0A-A73C-6749A5FFD1C5.htm "This tutorial demonstrates how to change the appearance of profile views.")