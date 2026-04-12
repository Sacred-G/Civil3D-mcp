---
title: "Exercise 2: Editing a Layout Profile"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-47F4D115-F40E-4D75-AE6A-CB375052500C.htm"
category: "tutorial_using_layout_profiles"
last_updated: "2026-03-17T18:42:30.347Z"
---

                  Exercise 2: Editing a Layout Profile  

# Exercise 2: Editing a Layout Profile

In this exercise, you will modify the layout profile by using grips and entering specific attribute values.

This exercise continues from [Exercise 1: Creating a Layout Profile](GUID-C0C6F25C-B0A8-486E-9C32-13948DEF1995.htm "In this exercise, you will create the layout profile. Typically, this profile is used to show the levels along a proposed road surface or a finished gradient.").

Edit the profile parameters

1.  Open Profile-3A.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the bottom profile view, select the red layout profile. Right-click. Click Edit Profile Geometry.
    
    The Profile Layout Tools toolbar is displayed.
    
3.  On the Profile Layout Tools toolbar, click ![](../images/GUID-2B269ED9-1ABA-451F-874F-6E1050AF03C7.png).
    
    This option specifies that you will edit the data for each profile VIP. If you had clicked ![](../images/GUID-E556264E-99E0-4CCC-971A-6B43B3135DBF.png), you would edit the data for each profile line and curve sub-element.
    
4.  On the Profile Layout Tools toolbar, click Profile Grid View ![](../images/GUID-7BDFFCF6-1694-4995-BDDE-54A6CE78E557.png).
    
    The Profile Elements vista is displayed in the Panorama window. The first row in the table provides data about the starting point of the layout profile. Subsequent rows provide data about the VIPs. The last row provides data about the end point.
    
5.  Examine the Gradient In and Gradient Out columns with the aim of reducing one or more of the steeper gradients in the profile.
    
    Notice that the Gradient Out value for one VIP is the same as the Gradient In value for the next VIP. The values are the same because they refer to the same straight.
    
6.  In row No. 3, in the Gradient Out column, double-click the 8.000% value and then enter **5.000**. Press Enter.
    
    The value changes in the Gradient In column of row No. 4. The line in the drawing adjusts to the new value.
    
    **Further exploration:** Experiment with changing K values and Profile Curve Lengths. In each case, the Profile Curve Radius also changes.
    
    This exercise demonstrates that if your design process provides you with guidelines for K values or vertical curve length, you can easily edit profile specifications in the Profile Elements table.
    
7.  On the Profile Layout Tools toolbar, click Profile Layout Parameters![](../images/GUID-0C471B43-C80E-4F2E-B3A2-6C49B374A5ED.png).
8.  In the Profile Elements vista, select row 2.
    
    The Profile Layout Parameters dialog box displays the parameters for the first VIP on the profile.
    

Grip edit the profile

1.  With a profile curve clearly visible, click the circular grip at the curve midpoint. The grip turns red, which indicates that it can be moved.
    
    ![](../images/GUID-06E4C248-34F5-41FC-886B-B105FD55E4F7.png)
    
2.  Move the cursor to a new location closer to or farther from the VIP, then click.
    
    The curve moves to pass through the point you clicked. The length of the curve changes.
    
    Notice that the affected attributes update in the Profile Elements vista and Profile Layout Parameters window.
    
    **Further exploration:** Click another grip and move it to a new location. Note how other grips react.
    
3.  Click the triangular grip at the curve midpoint.
    
    ![](../images/GUID-020AED7C-A3C0-443F-8FA5-814BC32F931A.png)
    
4.  Move the cursor to a new location closer to or farther from the VIP, then click.
    
    In the Profile Elements vista, notice which element’s VIP Level updates.
    
5.  Select the row of the element that you changed.
    
    The element’s attribute values are displayed in the Profile Layout Parameters window.
    
6.  Close the Profile Layout Tools toolbar.
    
    The Panorama window (Profile Elements vista) and Profile Layout Parameters dialog box close.
    

To continue this tutorial, go to [Exercise 3: Copying a Profile and Offsetting it Vertically](GUID-3C8441EB-DF58-426A-ADB9-428859A42F2D.htm "In this exercise, you will copy part of a centerline layout profile. You will use the copy to create a starting line for a ditch profile that is a specified distance below the centerline.").

**Parent topic:** [Tutorial: Using Layout Profiles](GUID-DF73E4FC-35CB-4D11-96C9-8A73DB24DC45.htm "This tutorial demonstrates how to create and edit layout profiles, which are often called design profiles or finished design profiles.")