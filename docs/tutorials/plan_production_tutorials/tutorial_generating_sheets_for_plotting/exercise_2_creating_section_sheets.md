---
title: "Exercise 2: Creating Section Sheets"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-0083168A-8D1D-4B5C-9DC3-7B3CCA312483.htm"
category: "tutorial_generating_sheets_for_plotting"
last_updated: "2026-03-17T18:43:24.020Z"
---

                Exercise 2: Creating Section Sheets  

# Exercise 2: Creating Section Sheets

In this exercise, you will create sheets from section views.

Note:

To create section views, see the [Creating Section Views tutorial](GUID-A9C4C30C-46F2-44CB-B12A-4B0417F105DA.htm "This tutorial demonstrates how to display cross sections of the corridor model surfaces along the centerline alignment. You will create sample lines and then generate the sections.").

This exercise continues from [Exercise 1: Creating Plan and Profile Sheets](GUID-E45BE45B-DEE4-4717-97CC-402C4919B07A.htm "In this exercise, you will create plan and profile sheets from the view frames you created in an earlier exercise.").

Open and save the drawing

1.  Open _Plan Production-Section Sheets-Create.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The current drawing must be saved before you create section sheets. To prevent the tutorial drawing from being overwritten, save it to an alternate location.
    
2.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save As.
3.  In the Save Drawing As dialog box, navigate to the [My Civil Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Click Save.

Create section sheets

1.  Click Output tab ![](../images/ac.menuaro.gif)Plan Production panel ![](../images/ac.menuaro.gif)Create Section Sheets ![](../images/GUID-E5D8DF3A-5DEB-4130-8245-24BD6C5A37B8.png) Find.
2.  In the Create Section Sheets dialog box, specify the following basic parameters:
    *   Select Alignment: Centerline (1)
    *   Sample Line Group Name: SLG-1
    *   Select Section View Group: Section View Group - 1
3.  Under Layout Settings, click ![](../images/GUID-00C5A391-2648-4DC0-A90B-A4CD552D9F37.png).
    
    You can use the Name Template dialog box to specify a default name for each of the layouts. In this exercise, you will accept the default layout name template.
    
4.  Click Cancel.
5.  In the Create Section Sheets dialog box, under Sheet Set, make sure that New Sheet Set is selected.
6.  Under Sheet Set Storage Location, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
7.  In the Browse for Sheet Set Folder dialog box, navigate to the [My Civil Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Click Open.
8.  Click Create Sheets.
9.  When you are notified that the drawing will be saved, click OK.
    
    When the drawing is saved, the following tasks are completed:
    
    *   Layouts are created.
    *   A sheet set database is created.
    *   The Sheet Set Manager window opens.

Note: If the Select Layout as Sheet Template dialog box is displayed, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png) and browse to the [local Template\\Plan Production folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24226). Then select Civil 3D (Imperial) Section.dwt. Select one of the layouts that are listed and click OK.

Examine the section sheets

When the sheets are generated, they are displayed in the Sheet Set Manager window.

2.  In the Sheet Set Manager window, under Section View Group - 1, double-click the first entry.
3.  Select the rectangular section view border. Right-click. Click Properties.
    
    Note:
    
    In the following image, the section view border is red.
    
    ![](../images/GUID-7DC76209-461B-4CA9-96DC-023BF311729F.png)
    
    In the Properties window, under Misc, notice the Standard Scale property. This value should match the drawing scale of the source drawing.
    
4.  Close the Properties window.
5.  At the bottom of the drawing, click the Model tab.

Modify a section view

1.  Zoom in to the first section view, which is in the lower left-hand corner of the leftmost section sheet.
2.  On the left-hand side of the section view, select the offset and level label.
3.  Click the ![](../images/GUID-BE0E5657-F39E-460D-98C9-F1853734A46F.png) grip. Drag the grip up and to the right. Click the place the label.
4.  Repeat Steps 2 and 3 to move the offset and level label on the right-hand side of the section view.
5.  At the bottom of the drawing, click the Section Sheet - (1) tab.
6.  Zoom in to the first section view, which is in the lower left-hand corner of the layout.
    
    The label changes have been applied to the section view. The model is dynamically linked to the section view sheets.
    

**Parent topic:** [Tutorial: Generating Sheets for Plotting](GUID-F1196756-8E33-4ED5-84B8-056BF6505B91.htm "This tutorial demonstrates how to generate plot-ready sheets that display either plan and profile or cross sections.")