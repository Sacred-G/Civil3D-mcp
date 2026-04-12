---
title: "Exercise 1: Creating a Layout Profile"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-C0C6F25C-B0A8-486E-9C32-13948DEF1995.htm"
category: "tutorial_using_layout_profiles"
last_updated: "2026-03-17T18:42:30.304Z"
---

                   Exercise 1: Creating a Layout Profile  

# Exercise 1: Creating a Layout Profile

In this exercise, you will create the layout profile. Typically, this profile is used to show the levels along a proposed road surface or a finished gradient.

The layout profile is similar to a horizontal alignment, in that it is constructed of straight straights with optional curves placed where the straights intersect. These straights and curves on a layout profile are located in the vertical plane and the intersection points are called vertical intersection points (VIP).

This exercise continues from the [Using Surface Profiles](GUID-8B3EB320-E48D-4CB7-BD4D-AB887F1CF9B3.htm "This tutorial demonstrates how to create surface profiles and display them in a profile view.") tutorial.

Hide the offset profiles

1.  Open Profile-2C.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click the bottom grid to select profile view PV-1. Right-click. Click Profile View Properties.
3.  In the Profile View Properties dialog box, on the Profiles tab, clear the Draw check boxes for the right offset and left offset profiles.
    
    The Profiles tab displays all existing profiles for a given horizontal alignment, both surface profiles and layout profiles. You can use the Draw check boxes to specify which profiles to display in the profile view.
    
    **Further exploration:** You can permanently delete a profile by selecting it in the drawing (or in Toolspace) and pressing the Delete key. If you delete a profile, it is removed from all profile views, the list of profiles in the Profile View Properties dialog box, and Toolspace. To restore a deleted surface profile, create a new one. The new profile is displayed in any applicable profile views, and can be edited in the Profile View Properties dialog box.
    
    To continue with this exercise, ensure that the centerline profile is visible in profile view PV- (1).
    
4.  Click OK.
    
    ![](../images/GUID-163B5487-F2D8-4BE3-B77B-CFA39EBE1B80.png)
    

Specify the profile creation settings

1.  Note:
    
    Turn off Object Snap (OSNAP).
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Profile drop-down ![](../images/ac.menuaro.gif)Profile Creation Tools ![](../images/GUID-78E66A2B-A0B5-40F5-99DB-82B70D6AAB1A.png) Find.
3.  Click the bottom grid to select profile view PV-1.
4.  In the Create Profile – Draw New dialog box, change the Profile Style to **Finished Ground**.
5.  In the Profile Label Set list, select **Standard**. Click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png).
6.  In the Profile Label Set dialog box, on the Labels tab, specify the following parameters:
    *   Type: **Horizontal Geometry Points**
    *   Profile Horizontal Geometry Point: **Chainage & Type**
7.  Click **Add**.
8.  In the Geometry Points dialog box, examine the geometry points that can be labeled. You can specify any combination of points that you want to label. Click OK.
    
    Note:
    
    For more details about geometry point labels, see the [Adding Labels in Groups tutorial exercise](GUID-9A8CBBE8-FAE7-461F-B4B5-C35181213F4A.htm "In this exercise, you will use label sets to apply several types of labels to an alignment.").
    
9.  In the Profile Label Set dialog box, click OK.
10.  Click the Design Criteria tab.
     
     The options on this tab are used only if you want to ensure that the profile design meets specified design criteria. You will not apply design criteria to the profile in this exercise. You will learn how to use the design criteria feature in the [Designing a Profile that Refers to Local Standards tutorial](GUID-75B9BF20-42D1-4D86-9465-C906A03FDC16.htm "This tutorial demonstrates how to validate that your profile design meets criteria specified by a local agency.").
     
11.  Click OK to accept the settings.

Draw the layout profile

1.  In the Profile Layout Tools toolbar, click the arrow on the right side of ![](../images/GUID-0B99A868-6C54-486F-A4E0-39A28F7D66EB.png) and click Curve Settings.
2.  On the Vertical Curve Settings dialog box, specify the following parameters:
    
    *   Curve Type: **Parabolic**
    *   Crest Curve Length: **100**
    *   Sag Curve Length: **100**
    
    Notice that you can select one of three curve types and specify parameters for each type.
    
3.  Click OK.
4.  In the Profile Layout Tools toolbar, ensure that Draw Straights With Curves![](../images/GUID-0B99A868-6C54-486F-A4E0-39A28F7D66EB.png) is selected.
    
    You are now ready to draw the layout profile by clicking in the drawing at the proposed locations of VIPs. At each VIP, the application inserts a curve. To be realistic, your line should follow the general profile of the surface centerline. However, it can cut across steep hills and valleys to outline a smoother road surface.
    
5.  In the profile view, click the left side, near the centerline surface profile, to start the layout profile.
    
    ![](../images/GUID-7FF6085B-9F86-4B88-A671-16D4BC1781D6.png)
    
6.  Extend the line to the right and click at another location near the centerline surface profile. Continue in this manner.
    
    ![](../images/GUID-C540FBD3-A0E2-4526-914C-A30987621886.png)
    
7.  At the last point, right-click to end the profile. The layout profile is now drawn and labeled.
    
    ![](../images/GUID-CD4878C4-F9BD-4FFC-8341-AA57F0A31CE9.png)
    
8.  Zoom and pan along the layout profile to examine the labels.

To continue this tutorial, go to [Exercise 2: Editing a Layout Profile](GUID-47F4D115-F40E-4D75-AE6A-CB375052500C.htm "In this exercise, you will modify the layout profile by using grips and entering specific attribute values.").

**Parent topic:** [Tutorial: Using Layout Profiles](GUID-DF73E4FC-35CB-4D11-96C9-8A73DB24DC45.htm "This tutorial demonstrates how to create and edit layout profiles, which are often called design profiles or finished design profiles.")