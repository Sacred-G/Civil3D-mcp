---
title: "Exercise 1: Adding Data Boxes to a Profile View"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6DE2A16B-8CCD-4B0E-8AD0-E26DDA33619B.htm"
category: "tutorial_working_with_data_boxes"
last_updated: "2026-03-17T18:42:32.681Z"
---

                   Exercise 1: Adding Data Boxes to a Profile View  

# Exercise 1: Adding Data Boxes to a Profile View

In this exercise, you will add data boxes along the bottom of a profile view.

Add profile view data boxes

1.  Open Profile-6A.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The data box at the top of profile view PV - (3) shows the locations of horizontal curves in the parent alignment. Blue vertical lines cross the profile view grid to mark the start and end of each horizontal curve. The data box at the bottom annotates the level of both profiles at the major chainages.
    
    ![](../images/GUID-0BF88AC6-51AF-4519-A3F4-A4E50715C2EE.png)
    
2.  Select the profile grid. Right-click. Click Profile View Properties.
3.  In the Profile View Properties dialog box, on the Bands tab, specify the following parameters:
    *   Band Type: **Vertical Geometry**
    *   Select Band Style: **Geometry**
    *   Location: Bottom Of Profile View
4.  Click Add.
    
    The band is added to the bottom of the list.
    
5.  Click Apply.
    
    The new data box is displayed at the bottom of the stack of data boxes. This data box labels the vertical geometry points of the first profile in the list. In this case, the existing ground profile was selected by default. You will change the referenced profile later in this exercise.
    
    ![](../images/GUID-231AE515-BE68-43A5-BC23-BE2F6C0CB988.png)
    
6.  In the Profile View Properties dialog box, on the Bands tab, specify the following parameters:
    *   Band Type: **Horizontal Geometry**
    *   Select Band Style: **Curvature**
    *   Location: **Bottom Of Profile View**
7.  Click Add.
8.  Click Apply.
    
    The horizontal geometry band is added to the bottom of the profile view in the drawing. This data box is drawn in a different style from the one along the top of the grid. However, both styles show the location of horizontal curves and are labeled with basic engineering data about the curves. These bands are useful for evaluating the design profile from a drainage and safety point-of-view.
    
    ![](../images/GUID-B0D0EA68-4B4E-401C-88A6-B3388DABD622.png)
    
9.  In the Profile View Properties dialog box, on the Bands tab, specify the following parameters:
    *   Band Type: **Profile Data**
    *   Select Band Style: **Horizontal and Vertical Geometry Point Distance**
    *   Location: Bottom Of Profile View
10.  Click Add.
     
     In the Geometry Points To Label In Band dialog box, you can specify the individual horizontal and vertical geometry points to label using the current style. For this exercise, you will accept the default selections.
     
11.  In the Geometry Points To Label In Band dialog box, click OK.
     
     Note:
     
     For more information on geometry point labeling, see the [Adding Labels in Groups tutorial exercise](GUID-9A8CBBE8-FAE7-461F-B4B5-C35181213F4A.htm "In this exercise, you will use label sets to apply several types of labels to an alignment.").
     
12.  Click Apply.
     
     The new data box is displayed at the bottom of the stack of data boxes. This data box labels the incremental distance between the horizontal geometry points of the parent alignment.
     
     ![](../images/GUID-ABA0FC71-719E-402C-A9FC-154324816D86.png)
     

Change the profiles referenced in data boxes

1.  In the List Of Bands table, in the Profile1 column, change the value to **Layout (1)** for the Profile Data box at the bottom of the list.
2.  Click Apply.
    
    Now, the Profile Data box shows levels of both the existing ground and finished design profile at each major chainage. The Horizontal Geometry Point Distance band displays the finished ground level at each horizontal geometry point.
    
    ![](../images/GUID-2DC997B6-52BD-4F2D-8DFA-E440288E47E5.png)
    
3.  For the Vertical Geometry band, change the Profile1 setting to **Layout - (1)**.
4.  Click Apply.
    
    Now, this band shows the length of each gradient segment along the layout profile.
    
    ![](../images/GUID-5F214732-B34A-4987-995F-9106909E7AA3.png)
    

Rearrange the data boxes

1.  In the table of bands list, select the Vertical Geometry band. Click ![](../images/GUID-1CDAB802-11BB-489A-9D01-8AB3FF65C57B.png) twice, then click Apply.
    
    The Vertical Geometry band moves to the bottom of the stack of data boxes.
    
2.  Click OK.
    
    This arrangement of bands is convenient for analysis. It displays horizontal and vertical geometry, as well as comparative level data for the surface profile and the layout profile.
    
    Notice that in the bottom, Vertical Geometry band, the labels in the uphill straights are obscured by the straights. You will correct this in [Exercise 3: Modifying a Data Box Style](GUID-D3416B2C-884F-4C49-B408-7A1CE8C40E0D.htm "In this exercise, you will learn how to change the data that is displayed in a data box.").
    
    ![](../images/GUID-48CD0FE2-A54F-4145-A145-E60C10C1AC6A.png)
    

To continue this tutorial, go to [Exercise 2: Moving Labels in a Data Box](GUID-A7EEB5AB-57E0-49B6-B850-CB8EE2E086C7.htm "In this exercise, you will learn how to rearrange labels in data boxes.").

**Parent topic:** [Tutorial: Working with Data Boxes](GUID-9CC02D3F-84C0-4574-89F0-61FBB809D683.htm "This tutorial demonstrates how to add and change the appearance of data boxes in a profile view.")