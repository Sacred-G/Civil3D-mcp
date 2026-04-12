---
title: "Exercise 2: Creating Section Views"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-F417BAD7-CDCA-4615-BCCE-2860E6D49603.htm"
category: "tutorial_creating_section_views"
last_updated: "2026-03-17T18:42:58.276Z"
---

                 Exercise 2: Creating Section Views  

# Exercise 2: Creating Section Views

In this exercise, you will create a section view for a range of sample lines.

First, you will modify some of the settings that apply to section views.

This exercise continues from [Exercise 1: Creating Sample Lines](GUID-408653EF-997A-46BD-96A2-40E832FDBBCA.htm "In this exercise, you will create a set of sample lines along the alignment.").

Modify the group plot style

1.  Open _Sections-Views-Create.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Settings tab, expand the Section View![](../images/ac.menuaro.gif)Group Plot Styles collection. Select the Basic style. Right-click. Click Edit.
3.  In the Group Plot Style dialog box, click the Display tab.
4.  Under Component Display, in the Print Area row, change the Color to **Red**.
    
    These colors make it easy to identify the extents of the sheet, as well as the portion of the sheet that contains the section views. You will observe these components when you create the section views later in this exercise.
    
5.  Click OK.

Specify basic section view parameters

1.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Section Views drop-down ![](../images/ac.menuaro.gif)Create Multiple Views ![](../images/GUID-1F5ECEAB-E53F-4C3C-8E99-C1ECEF513927.png) Find.
2.  In the Create Multiple Section Views wizard, on the General page, specify the following parameters:
    *   Select Alignment: **Centerline (1)**
    *   Sample Line Group Name: **SLG-1**
    *   Chainage Range: **User Specified**
    *   Start: **0+00.00**
    *   End: **10+00.00**
    *   Section View Style: **Road Section**
3.  Click Next.

Specify a plot style and a layout template

On the Section Placement page, you specify how the section views are displayed and arranged in the sheets. Select a template, viewport scale, and the group plot style.

2.  Under Placement Options, select Production.
    
    Note:
    
    The Draft option creates section views in the current drawing only. You cannot generate sheets from Draft section views.
    
3.  Under Template for Cross Section Sheet, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
4.  In the Select Layout As Sheet Template dialog box, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
5.  In the [Local Template folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24226), make sure that _Civil 3D (Imperial) Section.dwt_ is selected. Click Open.
6.  In the Select Layout As Sheet Template dialog box, under Select A Layout To Create New Sheets, select ARCH D Section 40 Scale.
7.  Click OK.
8.  On the Create Multiple Section Views wizard, under Group Plot Style, select Basic.
9.  Click Next.

Specify the section view offsets

1.  On the Offset Range page, under Offset Range, select Automatic.
    
    Notice that the Left and Right values are 150. This is the sample line swath width value that you specified in [Exercise 1: Creating Sample Lines](GUID-408653EF-997A-46BD-96A2-40E832FDBBCA.htm "In this exercise, you will create a set of sample lines along the alignment.").
    
2.  Click Next.

Specify the height of the section views

1.  On the Level Range page, specify the following parameters:
    
    *   User Specified: Selected
    *   Height: **100.000’**
    *   Section Views Height Option: **Follow A Section**
    *   Select Section: **EG**
    
    These settings specify that all section views will be 100-feet tall and the level will follow the EG surface level.
    
2.  Click Next.

Specify the sampled sections and labels

The Section Display Options page specifies the object and label styles for the sampled objects. In this exercise, you will suppress the labels.

2.  On the Section Display Options page, in the Clip Grid column, click the **Corridor - (1) Top** row.
3.  In the **EG** row, click the value in the Change Labels column.
4.  In the Select Style Set dialog box, select **No Labels**. Click OK.
5.  Repeat Steps 2 and 3 to apply the **No Labels** style to the **Corridor - (1) Top** and **Corridor - (1) Datum** surfaces.
6.  Click Next.

Specify the data box settings

1.  On the Data Boxes page, under Select Band Set, select **Major Chainage**.
2.  In the Set Band Properties area, specify the following parameters:
    *   Surface1: **Corridor (1) - Top**
    *   Surface 2: **Corridor (1) - Datum**

Create and examine the section views

1.  Click Create Section Views.
2.  At the Identify Section View Origin prompt, select a point in the top viewport.
3.  Zoom in to one of the sheets.
    
    Groups of section views are arranged inside two rectangles. You specified the color of the rectangles at the beginning of this exercise:
    
    *   The blue rectangle represents the extents of the sheet.
    *   The red rectangle represents the extents of the printable area in which the section views are placed.
    
    When you create section sheets, the area between the red and blue rectangles contains the title block, border, and other information that is contained in the plan production template you selected.
    
    You will create section sheets in the [Creating Section Sheets](GUID-0083168A-8D1D-4B5C-9DC3-7B3CCA312483.htm "In this exercise, you will create sheets from section views.") exercise.
    
    ![](../images/GUID-7C4D37E1-3D42-4A1A-B343-3432F2CFE988.png)
    
4.  Zoom in to one of the section views.
    
    ![](../images/GUID-19B8C7B0-1878-4356-A0A7-8BDFE601CB72.png)
    

To continue to the next tutorial, go to [Adding Data to a Section View](GUID-F9CF9DD7-EB09-41E2-BCEE-586730811558.htm "This tutorial demonstrates how to add annotative data to a section view.").

**Parent topic:** [Tutorial: Creating Section Views](GUID-A9C4C30C-46F2-44CB-B12A-4B0417F105DA.htm "This tutorial demonstrates how to display cross sections of the corridor model surfaces along the centerline alignment. You will create sample lines and then generate the sections.")