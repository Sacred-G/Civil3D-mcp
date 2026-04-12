---
title: "Exercise 3: Reviewing Surface Profile Characteristics"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-A506BE94-5C7C-42F7-A228-4FA7E44BA9D5.htm"
category: "tutorial_using_surface_profiles"
last_updated: "2026-03-17T18:42:29.657Z"
---

                  Exercise 3: Reviewing Surface Profile Characteristics  

# Exercise 3: Reviewing Surface Profile Characteristics

In this exercise, you will examine some of the information displayed in the profile and the profile view.

This exercise continues from [Exercise 2: Changing the Profile Style](GUID-E211661C-5334-456F-B5A4-8125D0D64F54.htm "In this exercise, you will change a profile style in two different ways.").

Examine the profile view characteristics

1.  Open Profile-2C.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Zoom in to the lower profile view PV - (1) so that you can clearly see the three profile lines.
3.  Select the highest profile line (the right offset profile), being careful not to select the profile view grid. Right-click. Click Edit Profile Geometry.
    
    The Profile Layout Tools toolbar is displayed. Notice that no editing grips are displayed along the profile, and most controls in the Profile Layout Tools toolbar are shaded and unavailable. This profile is dynamic. It is linked to the surface levels, and no part of the line can be edited.
    
    ![](../images/GUID-61E34AF1-BAE8-44FD-91A4-9E6356991514.png)
    
4.  In the Profile Layout Tools toolbar, click ![](../images/GUID-7BDFFCF6-1694-4995-BDDE-54A6CE78E557.png).
    
    The Profile Elements vista is displayed in the Panorama window. This table displays useful gradient data for the entire profile. Again, values are shaded and unavailable so you cannot edit them. Editing these values would break the integrity of the link between the profile and the surface.
    
5.  Press Esc to deselect the right offset profile.
6.  Click the red (left offset) profile line.
    
    Notice that editing grips appear along this profile. If you zoom out, you can see that the same profile is also selected in the other profile view. If you grip edit the profile in one profile view, the same changes apply to the other copy of the profile.
    
    ![](../images/GUID-EFDA2380-8292-4970-B78B-65B8AE7D41F8.png)
    
    Because this line is a static profile, and detached from the surface, you can edit it in various ways, including copying and moving it. You would not edit this line if you wanted to preserve it as a snapshot of the surface at a particular time.
    
    Notice that the Profile Elements table now displays the design data for the left offset profile, with values you can edit. When you selected the left offset profile, it became the active profile for the editing tools.
    
7.  Close the Profile Layout Tools toolbar.
    
    The Profile Layout Tools toolbar and Profile Elements vista both close, and the left offset profile line is deselected in the drawing.
    

To continue to the next tutorial, go to [Using Layout Profiles](GUID-DF73E4FC-35CB-4D11-96C9-8A73DB24DC45.htm "This tutorial demonstrates how to create and edit layout profiles, which are often called design profiles or finished design profiles.").

**Parent topic:** [Tutorial: Using Surface Profiles](GUID-8B3EB320-E48D-4CB7-BD4D-AB887F1CF9B3.htm "This tutorial demonstrates how to create surface profiles and display them in a profile view.")