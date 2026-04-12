---
title: "Exercise 3: Copying a Profile and Offsetting it Vertically"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-3C8441EB-DF58-426A-ADB9-428859A42F2D.htm"
category: "tutorial_using_layout_profiles"
last_updated: "2026-03-17T18:42:30.390Z"
---

                   Exercise 3: Copying a Profile and Offsetting it Vertically  

# Exercise 3: Copying a Profile and Offsetting it Vertically

In this exercise, you will copy part of a centerline layout profile. You will use the copy to create a starting line for a ditch profile that is a specified distance below the centerline.

This exercise continues from [Exercise 2: Editing a Layout Profile](GUID-47F4D115-F40E-4D75-AE6A-CB375052500C.htm "In this exercise, you will modify the layout profile by using grips and entering specific attribute values.").

Copy the layout profile

1.  Open Profile-3B.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In profile view PV-1, select the red layout profile. Right-click. Click Edit Profile Geometry.
3.  In the Profile Layout Tools toolbar, click ![](../images/GUID-E6FC3D2C-535B-4A06-8E07-B3638C994937.png).Copy Profile
4.  In the Copy Profile Data dialog box, specify the following parameters:
    
    To use more advanced features of the product, you will make the profile copy shorter than the original.
    
    Note:
    
    The chainage values recommended in this step have been chosen because in the drawing _Profile-3B.dwg_, they include the two center straights of the Layout (1) profile. If you are using another drawing with a much different profile, you may have to enter different chainage values. To be included in the profile copy, a complete straight must be within the copied range. If part of a straight extends beyond the range, the whole straight is excluded from the selection set.
    
    *   VIP Range: **chainage Range**
    *   Start: **300**
    *   End: **1700**
    *   Destination Profile Options: **Create New Profile**
5.  Click OK.
    
    The new profile is drawn on top of the old one.
    
6.  In Toolspace, on the Prospector tab, expand the Alignments![](../images/ac.menuaro.gif)Centerline Alignments![](../images/ac.menuaro.gif)**Ridge Road**![](../images/ac.menuaro.gif)Profiles collection under the alignment.
    
    Your profile copy is displayed with the layout profile icon ![](../images/GUID-6A9013E0-00A3-425C-91A3-26CD110251ED.png) and name.
    
7.  Press Esc.

Offset the profile

1.  In the drawing, click the profile view. Right-click. Click Profile View Properties.
2.  On the Profiles tab, clear the Draw check box for the original layout profile, **Layout (1)**.
    
    Clearing the check box removes the original profile from the profile view. Later, you can restore this profile to the profile view if you wish.
    
    Tip:
    
    Instead of removing a profile from the profile view, you can try selecting a profile to move it. However, the process described here is more reliable with overlapping profiles.
    
3.  Click OK.
    
    The Profile View Properties dialog box closes and the profile view is redrawn, showing the copy of part of the layout profile.
    
4.  Click the layout profile.
    
    The name of the selected profile is displayed in the Profile Layout Tools toolbar. In the next few steps, you will lower the profile copy by 5 feet to represent the level of the ditch.
    
5.  On the Profile Layout Tools toolbar, click ![](../images/GUID-033314F7-4E87-4C80-8FEB-CD6A261385C1.png)Raise/Lower VIPs.
6.  In the Raise/Lower VIP Level dialog box, specify the following parameters:
    *   Level Change: **\-5**
    *   VIP Range: **All**
7.  Click OK.
    
    In both profile views, the line moves to its new position.
    
    This profile copy is a full-featured object that can be edited in the same way as the original layout profile.
    
8.  Press Esc to deselect the profile.
9.  In the drawing, click the profile view. Right-click. Click Profile View Properties.
10.  On the Profiles tab, set the Draw check boxes to the following states:
     *   **Layout (1)**: **Selected**
     *   **Layout (1) \[Copy\]**: **Cleared**
11.  Click OK.
     
     Clearing the check box removes the copy of the profile from the profile view. Notice that the copy is still displayed in the profile view PV-(2).
     
     ![](../images/GUID-B5314FEB-EA3C-4C58-8FB9-40DE039F5D37.png)
     

To continue to the next tutorial, go to [Designing a Profile that Refers to Local Standards](GUID-75B9BF20-42D1-4D86-9465-C906A03FDC16.htm "This tutorial demonstrates how to validate that your profile design meets criteria specified by a local agency.").

**Parent topic:** [Tutorial: Using Layout Profiles](GUID-DF73E4FC-35CB-4D11-96C9-8A73DB24DC45.htm "This tutorial demonstrates how to create and edit layout profiles, which are often called design profiles or finished design profiles.")