---
title: "Exercise 1: Creating Plan and Profile Sheets"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E45BE45B-DEE4-4717-97CC-402C4919B07A.htm"
category: "tutorial_generating_sheets_for_plotting"
last_updated: "2026-03-17T18:43:23.897Z"
---

                   Exercise 1: Creating Plan and Profile Sheets  

# Exercise 1: Creating Plan and Profile Sheets

In this exercise, you will create plan and profile sheets from the view frames you created in an earlier exercise.

After you have used the Create View Frames wizard to create view frames, then you can create sheets using the Create Sheets wizard.

The sheets that are created represent the layouts (_sheets_) that are used for construction documents (_plans_).

Specify the sheet creation settings

1.  Open _Plan Production-Plan Profile Sheets-Create.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The current drawing must be saved before you create section sheets. To prevent the tutorial drawing from being overwritten, save it to an alternate location.
    
2.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save As.
3.  In the Save Drawing As dialog box, navigate to the [My Civil Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Click Save.
4.  Click Output tab ![](../images/ac.menuaro.gif)Plan Production panel ![](../images/ac.menuaro.gif)Create Sheets ![](../images/GUID-F265CCF4-3883-4C5B-834C-B33B0CFF5E9F.png) Find.
    
    Note:
    
    On any page of this wizard, you may click Create Sheets to create the sheets using the default choices on the wizard pages. If there are criteria that have not been supplied, then the Create Sheets button is not available.
    
5.  In the Create Sheets wizard, on the View Frame Group And Layouts page, specify the following parameters:
    *   View Frame Group: **VFG - Maple Road**.
        
        Notice that you may select all view frames in the group or a selection of view frames within the currently selected view frame group.
        
    *   Layout Creation: Number Of Layouts Per New Drawing. Accept the default value of 1.
        
        This option creates a drawing for each layout (sheet). If you enter a value greater than one, three for example, three layouts are created in each new drawing. The total number of sheets and drawings would depend on the length of the alignment selected and other criteria, such as the size and scale of the viewports in the referenced template. You can only enter an integer that is between zero and 256.
        
    *   Choose The North Arrow Block To Align In Layouts: **North**.
        
        This option orients a North arrow block that is defined in the template. This list is populated with all blocks that are present in the current drawing. Notice that there is a (none) selection available, if you do not wish to include a North arrow block.
        
6.  Click Next.
7.  On the Sheet Set page, under Sheet Set, select New Sheet Set.
    
    This option specifies that a new sheet set be created to organize the new sheets. A sheet set allows you to manage and publish a series of sheets as a unit. In the following steps, you will specify a location for the individual sheets and the sheet set data (_DST_) file. For best results, store the sheet set data file and its associated sheet files in the same location. For more information about working with sheets and sheet sets, see the AutoCAD Help.
    
8.  Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png) next to Sheet Set Storage Location.
9.  In the Browse For Sheet Set Folder dialog box, navigate to the [My Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Click Open.
10.  Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png) next to Sheet Files Storage Location.
11.  In the Browse For Folder dialog box, navigate to the [My Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Click Open.
12.  Click Next.
     
     Note:
     
     If the view frames do not include any profile views, then the Profile Views page in the Create Sheets wizard is skipped. The Data References page is displayed next.
     
13.  On the Profile Views page, under Other Profile View Options, select Choose Settings.
     
     Note:
     
     You specified the Profile View Settings during [Exercise 2: Creating View Frames](GUID-E8788617-0B22-45D9-9930-46C2A7CBC720.htm "In this exercise, you will use the Create View Frames wizard to quickly create view frames along an alignment.").
     
14.  Click Profile View Wizard.
15.  In the Create Multiple Profile Views wizard, use the links along the left side or the Back and Next buttons to examine the available settings. Many of the settings are not available because they are already defined by the currently selected view frame group.
     
     For more information, see the [Creating Multiple Profile Views](GUID-3377C4CA-9533-4F60-A877-86218DE10A87.htm "In this exercise, you will produce a set of profile views to display short, successive segments of a profile.") tutorial exercise.
     
16.  Click Finish.
17.  In the Create Sheets wizard, under Align Views, select Align Profile And Plan View At Start.
     
     This option aligns the alignment starting chainage in plan view with the profile starting chainage in profile view. The profile view shifts to the right to accommodate the exact alignment starting chainage.
     
18.  Click Next.
19.  On the Data References page, you can select or omit the objects for which you want references included in your sheets. Notice that the **Maple Road** alignment and profile are selected by default.
20.  Select the check box next to Pipe Networks.

Create sheets

1.  Click Create Sheets to close the wizard and create the sheets. When you are prompted to save the current drawing, click OK.
2.  When prompted, pan to a clear area in the drawing, then click a location for the profile view origin.
    
    After your sheets are created, the Sheet Set Manager is displayed, showing the newly created sheets. For more information on the Sheet Set Manager, see the Sheet Set Manager Help topics in the AutoCAD Help.
    
3.  If the Sheet Set Manager does not open, enter **SHEETSET** on the command line.
4.  On the Sheet Set Manager, select Open from the drop-down list.
5.  On the Open Sheet Set dialog box, navigate to the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832) and select _VFG - Maple Road.dst_. Click Open.
    
    On the Sheet Set Manager, notice that the _VFG - Maple Road - (1).dst_ is open, displaying the five sheets that were created.
    
6.  Select one of the sheets in the list. Right-click and select Open.
    
    The sheet opens as a new drawing.
    
    Note:
    
    You can publish your sheet(s) directly from the Sheet Set Manager or share them by using the eTransmit feature.
    
    ![](../images/GUID-0B8FCAAD-3540-4986-AC9E-96FA2C204F5B.png)
    

To continue this tutorial, go to [Exercise 2: Creating Section Sheets](GUID-0083168A-8D1D-4B5C-9DC3-7B3CCA312483.htm "In this exercise, you will create sheets from section views.").

**Parent topic:** [Tutorial: Generating Sheets for Plotting](GUID-F1196756-8E33-4ED5-84B8-056BF6505B91.htm "This tutorial demonstrates how to generate plot-ready sheets that display either plan and profile or cross sections.")