---
title: "Exercise 6: Creating Associated Longitudinal Section Views"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-378B5E19-40EB-4904-BD7A-1F02F87A15CB.htm"
category: "tutorial_displaying_and_modifying_profile_views"
last_updated: "2026-03-17T18:42:32.041Z"
---

                    Exercise 6: Creating Associated Longitudinal Section Views  

# Exercise 6: Creating Associated Longitudinal Section Views

In this exercise, you will create a series of three profile views that contain a centerline and left and right offset profiles.

Associated longitudinal section views are a collection of related profiles that are drawn in separate, vertically arranged profile views. When each profile is displayed on its own profile view grid, more space is available to annotate each profile.

![](../images/GUID-5D45DD13-D3BC-4A27-9FB3-1DD28F55A98E.png)

This exercise continues from [Exercise 5: Creating Multiple Profile Views](GUID-3377C4CA-9533-4F60-A877-86218DE10A87.htm "In this exercise, you will produce a set of profile views to display short, successive segments of a profile.").

Create associated longitudinal section views

1.  Open Profile-5F.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains an existing ground surface, several alignments, and a profile view. The profile view contains centerline and left and right edge of pavement (EOP) profiles of the alignment that travels from North to South along the West side of the site. Because the profiles are crowded in the profile view, you will create a series of associated longitudinal section views so that you can see the profiles clearly and have ample room for annotation.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Profile View drop-down ![](../images/ac.menuaro.gif)Create Profile View ![](../images/GUID-CA262469-3751-48A0-8B81-D95D8D2148AD.png) Find.
3.  In the Create Profile View wizard, on the General page, specify the following parameters:
    
    *   Select Alignment: **North-South Road**
    *   Show Offset Profiles By Vertically Stacking Profile Views: **Selected**
        
        The graphic at the bottom of the page changes color to indicate that the option is active.
        
    
    Note:
    
    The Profile View Style setting on this page is not used. You will assign separate styles for each profile view in the stack.
    
4.  On the left side of the Create Profile View wizard, click Associated Longitudinal Section.
5.  On the Associated Longitudinal Section page, specify the following parameters:
    *   Number Of Stacked Views: **3**
    *   Gap Between Views: **0**
        
        Note:
        
        The Gap Between Views value is measured in drawing units. A positive value adds space between the profile view grids. A negative value causes the profile view grids to overlap.
        
    *   Top View Style: **Stacked - Top**
    *   Middle View Style: **Stacked - Middle**
    *   Bottom View Style: **Stacked - Bottom**
6.  Click Next.
7.  On the Profile Display Options page, in the Select Stacked View To Specify Options For list, select Middle View.
    
    Note:
    
    The number of views that is visible in this list depends on the value you specified for **Number Of Stacked Views** in Step 5.
    
8.  In the Specify Profile Display Options table, select the Draw check box for the following profiles:
    *   **EG Centerline**
    *   **Centerline**
9.  In the Select Stacked View To Specify Options For list, select Top View.
10.  In the Specify Profile Display Options table, select the Draw check box for the following profiles:
     *   **EG Left Offset**
     *   **EOP Left**
11.  In the Select Stacked View To Specify Options For list, select Bottom View.
12.  In the Specify Profile Display Options table, select the Draw check box for the following profiles:
     *   **EG Right Offset**
     *   **EOP Right**
13.  Click Next.
14.  On the Pipe Network Display page, in the Select Stacked View To Specify Options For list, select Bottom View.
     
     Most of the pipe network is on the right-hand side of the alignment. You will specify that the pipe network components will be displayed in the profile view that displays the right EOP profiles.
     
15.  In the Select Pipe Networks To Draw In Profile View area, select the Select check box.
16.  In the Name column, expand the **Network - (1)** pipe network.
17.  Clear the check boxes for the following parts:
     
     *   **W-E Pipe - (1)**
     *   **W-E Pipe - (2)**
     *   **W-E Structure - (1)**
     *   **W-E Structure - (2)**
     
     These parts are a branch of the pipe network that follow the West-East alignment.
     
18.  In the Select Stacked View To Specify Options For list, select Middle View.
19.  In the Select Pipe Networks To Draw In Profile View list, select the Select check box.
20.  Clear all check boxes except for the following parts:
     *   **W-E Pipe - (1)**
     *   **W-E Pipe - (2)**
     *   **W-E Structure - (1)**
     *   **W-E Structure - (2)**
21.  In the Select Stacked View To Specify Options For list, select Top View.
22.  In the Select Pipe Networks To Draw In Profile View list, select the Select check box.
23.  Clear all check boxes except for the following parts:
     *   **W-E Pipe - (1)**
     *   **W-E Pipe - (2)**
     *   **W-E Structure - (1)**
     *   **W-E Structure - (2)**
24.  Click Create Profile View.
25.  In the drawing, pan to the clear area at the right of the surface. Click to place the profile views.
26.  In Toolspace, on the Prospector tab, expand the Alignments![](../images/ac.menuaro.gif)Road: North-South![](../images/ac.menuaro.gif)Profile Views collection.
     
     Notice that three separate profile views were created.
     

To continue to the next tutorial, go to [Tutorial: Working with Data Boxes](GUID-9CC02D3F-84C0-4574-89F0-61FBB809D683.htm "This tutorial demonstrates how to add and change the appearance of data boxes in a profile view.").

**Parent topic:** [Tutorial: Displaying and Modifying Profile Views](GUID-0BF95BEA-BDFF-4B0A-A73C-6749A5FFD1C5.htm "This tutorial demonstrates how to change the appearance of profile views.")