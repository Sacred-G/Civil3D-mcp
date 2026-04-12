---
title: "Tutorial: Designing Simple Profiles"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-0937FE14-98C7-4B24-BA8C-0822F31E280A.htm"
category: "profiles_tutorials"
last_updated: "2026-03-17T18:42:28.955Z"
---

                 Tutorial: Designing Simple Profiles  

# Tutorial: Designing Simple Profiles

In this tutorial, you will create simple existing ground and layout profiles for an alignment.

An _existing ground_ profile is extracted from a surface and shows the changes in level along a horizontal alignment. A _layout_ profile is a designed object that shows the proposed gradient and levels to be constructed. Profiles are displayed on an annotated grid called a _profile view_.

![](../images/GUID-8F2B1FBD-D8EC-4C6F-90FD-857BDF44D758.png)

Display an existing ground profile in a profile view

1.  Open drawing Profile-1.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains an existing ground surface, an alignment that represents a road centerline, and a polyline that represents the centerline of an intersecting road. You will use the rectangle in the northeast corner of the site as a guide to create a profile view.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Profile drop-down ![](../images/ac.menuaro.gif)Create Surface Profile ![](../images/GUID-35E8FF53-E099-47DA-9596-6CE0AC7C3547.png) Find.
3.  In the Create Profile From Surface dialog box, click Add.
    
    Note:
    
    For this exercise, the First Street alignment and the EG surface are the only available selections, and are selected by default.
    
4.  Click Draw in Profile View.
5.  In the Create Profile View wizard, click Create Profile View.
6.  In the drawing, click the lower left corner of the rectangular placeholder.
    
    ![](../images/GUID-8BE866BB-1CAC-4010-B80D-6015F3B6B8BB.png)
    
    The First Street Profile view is displayed, containing the dashed profile that represents the existing ground (EG) surface. The left and right sides annotate levels. The bottom annotates the chainages.
    
    ![](../images/GUID-09B95E6D-987C-4C3A-9793-4311A853A6E2.png)
    

Create a layout profile

1.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer drop-down. Next to the **\_PROF-ROAD-FGCL-PL** layer, click ![](../images/GUID-F908C832-FEB1-4F3B-AB32-BC8C776F3D5F.png).
    
    You will use the circles in the profile view as a guide to draw a layout profile.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Profile drop-down ![](../images/ac.menuaro.gif)Profile Creation Tools ![](../images/GUID-78E66A2B-A0B5-40F5-99DB-82B70D6AAB1A.png) Find.
3.  Select the profile view that you created.
4.  In the Create Profile - Draw New dialog box, specify the following parameters:
    
    *   Name: **Finished Gradient Centerline - First Street**
    *   Profile Style: **Design Profile**
    *   Profile Label Set: **Complete Label Set**
    
    Click OK.
    
    The Profile Layout Tools toolbar is displayed. This toolbar enables you to lay out a finished gradient profile, using either _vertical intersection points_ (VIPs) or constraint-based straight and curve elements. For this exercise, you will create VIPs at specified points. Straights will be created between the VIPs, and curves will be created at each VIP.
    
5.  On the Profile Layout Tools toolbar, in the Draw Straights list ![](../images/GUID-BFE2CF91-808A-488D-9512-8B3F59C8F93E.png), select Draw Straights With Curves.
    
    The command line prompts you to specify a start point.
    
    Before selecting a start point, verify that Object Snap (OSNAP) is on and Endpoint and Center modes are selected.
    
6.  On the status bar, right-click Object Snap ![](../images/GUID-003425EA-19A0-4F17-8886-C5A7ADEAD868.png). Click Settings.
7.  In the Drafting Settings dialog box, on the Object Snap tab, click Clear All, then select **Endpoint** and **Center** . Click OK.
8.  Moving from left to right, click the circle center points to place VIPs.
9.  Press Enter to complete the layout profile.
    
    The blue Finished Gradient Centerline profile and its labels are displayed in the profile view.
    
10.  Close the Profile Layout Tools toolbar.
11.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer drop-down. Next to the **PROF-ROAD-FGCL-PL** layer, click ![](../images/GUID-C47B73AA-EB38-4294-A125-79165A76F079.png). Click in the drawing to exit the Layer Control list.
     
     ![](../images/GUID-8F2B1FBD-D8EC-4C6F-90FD-857BDF44D758.png)
     

To continue to the next tutorial, go to [Using Surface Profiles](GUID-8B3EB320-E48D-4CB7-BD4D-AB887F1CF9B3.htm "This tutorial demonstrates how to create surface profiles and display them in a profile view.").

**Parent topic:** [Profiles Tutorials](GUID-768C10C5-7C9F-44F0-BDF2-4156801A41B2.htm)