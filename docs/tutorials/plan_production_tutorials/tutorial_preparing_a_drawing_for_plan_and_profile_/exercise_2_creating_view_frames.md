---
title: "Exercise 2: Creating View Frames"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E8788617-0B22-45D9-9930-46C2A7CBC720.htm"
category: "tutorial_preparing_a_drawing_for_plan_and_profile_"
last_updated: "2026-03-17T18:43:22.978Z"
---

                   Exercise 2: Creating View Frames  

# Exercise 2: Creating View Frames

In this exercise, you will use the Create View Frames wizard to quickly create view frames along an alignment.

View frames are rectangular areas along an alignment that represent what is displayed in the associated viewports on the layouts (sheets) to be created.

Before you create view frames, you must have the desired alignment already in your drawing. Depending on the type of sheets you want to produce (plan(s) only, profile(s) only, or plan and profile), you may also need to have a profile already created. If you are creating plan only sheets, then you do not need to have a profile in the drawing.

This exercise continues from [Exercise 1: Configuring Viewports](GUID-C4FD5858-DF1B-4738-99AF-76858DD7FB81.htm "In this exercise, you will learn how to prepare an existing drawing template for use with the plan production tools.").

Create view frames

1.  Open _Plan Production-View Frames-Create.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    Note:
    
    In the Select File dialog box, ensure that Files of Type is set to _Drawing (\*.dwg)_.
    
2.  Click Output tab ![](../images/ac.menuaro.gif)Plan Production panel ![](../images/ac.menuaro.gif)Create View Frames ![](../images/GUID-9A26651C-95A2-423E-8C17-6E18E2856632.png) Find.
3.  In the Create View Frames wizard, on the Alignment page, specify the following parameters:
    *   Alignment: **Maple Road**
    *   Chainage Range: **Automatic**
        
        This setting selects the entire Maple Road alignment.
        
4.  Click Next.
5.  On the Sheets page, under Sheet Settings, select Plan And Profile.
    
    This setting creates sheets that display both plan and profile views for each view frame.
    
6.  Under Template For Plan And Profile Sheet, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
7.  In the Select Layout As Sheet Template dialog box, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
8.  In the Select Layout As Sheet Template dialog box, browse to the [local Template\\Plan Production folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24226). Select _Civil 3D (Imperial) Plan and Profile.dwt_. Click Open.
9.  In the Select Layout As Sheet Template dialog box, under Select A Layout To Create New Sheets, select **ANSI D Plan And Profile 40 Scale**.
10.  Click OK.
11.  In the View Frame Placement section, select Along Alignment.
     
     This setting aligns the view frames along the alignment, as shown in the graphic in the wizard.
     
12.  Select the Set The First View Frame Before The Start Of The Alignment By option, and enter **50.000’** in the value field.
     
     This option sets the distance that the first view frame is placed before the start of the alignment. Entering a distance here provides the specified amount of space before the alignment starting chainage. This distance ensures that the alignment start location does not coincide with the start of the view frame. If this check box is not selected, then the first view frame is placed at the start of the alignment.
     
13.  Click Next to open the View Frame Group page.
     
     View frame groups are created automatically. This page lets you specify the object creation criteria for the view frame group object. Examine the settings that are available, but accept the default settings for this exercise.
     
14.  Click Next to open the Match Lines page.
     
     Match lines are straight lines that are drawn across an alignment in plan view to indicate where the corresponding sheet for that alignment begins and ends. Match lines typically include labels that can identify the previous and next sheet (view frame) along the alignment. Examine the settings that are available, but accept the default settings for this exercise.
     
15.  Click Next.
16.  On the Profile Views page, specify the following parameters:
     *   Profile View Style: **Major Grids**
     *   Band Set: **Stations Only**
17.  Click Create View Frames.
     
     The view frames are displayed along the alignment in the drawing window. The match lines are displayed as annotated lines between the view frames.
     
     ![](../images/GUID-F3CAE90A-0948-40E9-80BD-444F6C1CD7CF.png)
     
18.  In Toolspace, on the Prospector tab, expand View Frame Groups, then expand the **VFG - Maple Road** collection.
     
     Expand the View Frames and Match Lines collections. Notice that they contain the same components you created during the view frame group creation process.
     
19.  In Toolspace, on the Prospector tab, right-click one of the view frames. Notice that you may zoom or pan to the view frame in the drawing. Notice that you may also create a sheet for the individual view frame. In [Creating Plan and Profile Sheets](GUID-E45BE45B-DEE4-4717-97CC-402C4919B07A.htm "In this exercise, you will create plan and profile sheets from the view frames you created in an earlier exercise."), you will create sheets for all the view frames.

To continue to the next tutorial, go to [Generating Sheets for Plotting](GUID-F1196756-8E33-4ED5-84B8-056BF6505B91.htm "This tutorial demonstrates how to generate plot-ready sheets that display either plan and profile or cross sections.").

**Parent topic:** [Tutorial: Preparing a Drawing for Plan and Profile Sheet Layout](GUID-63C13E78-B7D5-4FB2-BEAA-BE363624225E.htm "This tutorial demonstrates how to set up a drawing before you publish plan and profile sheets.")