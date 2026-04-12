---
title: "Exercise 2: Updating Imported Survey Data"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-23DF4893-016D-4ECF-9086-B8B4A6946DF3.htm"
category: "tutorial_importing_survey_data"
last_updated: "2026-03-17T18:42:16.039Z"
---

                 Exercise 2: Updating Imported Survey Data  

# Exercise 2: Updating Imported Survey Data

In this exercise, you will modify some of the imported survey data, and then reprocess the linework to apply the changes.

You will learn about import events, which are a reference to the original survey data file that was imported into the survey database. Import events are useful when you need to determine how the survey data was originally imported and the individual points and figures that were imported during that event. Import events provide a convenient way to remove, re-import, and reprocess the survey data referenced within the event.

Reprocess the survey points

Note:

This exercise uses _Survey-2B.dwg_, which you saved in the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832) during the previous exercise. If you did not do this, you can use the copy of _Survey-2B.dwg_ that is in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79), but you will get duplicate objects when you update the survey data.

2.  In Toolspace, on the Survey tab, expand the Survey Databases![](../images/ac.menuaro.gif)Survey 1 ![](../images/ac.menuaro.gif)Import Events collection.
    
    Note:
    
    If you cannot expand the collection, right-click the database name and click Open For Edit.
    
3.  Select Import Events.
    
    The import options that were specified when the survey data was imported are displayed in the Toolspace list view. Notice that Point Identifier Offset is 10000. This indicates that as each survey point was imported, 10000 was added to the original point number. You will remove this offset value in the following steps.
    
4.  Under the Survey-1.fbk import event, select Survey Points.
    
    In the Toolspace list view, compare the values in the Number column with the values in the Original Number column. The Number column reflects the offset of 10000 that was added to the point numbers when they were imported.
    
5.  Right-click Survey-1.fbk. Click Re-Import.
6.  In the Re-Import Field Book dialog box, clear the Assign Offset To Point Identifiers check box.
7.  Click OK.
    
    Note:
    
    If you are prompted to abort the import process, click No.
    
    The points are re-imported, and the linework is reprocessed.
    

Update and reprocess a survey figure

1.  In Toolspace, on the Survey tab, select the Survey Databases![](../images/ac.menuaro.gif)Survey 1 ![](../images/ac.menuaro.gif)Figures collection.
2.  In the list view, select BLDG7. Right-click. Click Zoom To.
    
    The BLDG7 figure is displayed in the drawing. The ending line segments are incorrect, and the figure is not closed. You will edit the survey point to correct the figure.
    
    ![](../images/GUID-120BC418-1531-4468-BFBF-9426E5D20BFF.png)
    
3.  In the drawing, select survey point 804.
    
    ![](../images/GUID-F1779EAD-538F-44B8-8A14-932D5F40A604.png)
    
4.  Click Survey Point tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Survey Point Properties ![](../images/GUID-A211E165-7A43-4CC5-A3AD-5BFCAA3DC259.png) Find.
    
    In the Description, two errors are evident. First, a - (hyphen) was omitted from one of the values, which caused the line to extend in the wrong direction. Second, the close code is CLO, while the close code specified in the linework code set is CLS.
    
5.  In the Survey Point Properties dialog box, change the Description to the following:
    
    **BLDG7 RT -36 12 CLS**
    
6.  Click OK.
    
    You are prompted to select another survey point object. The command persists, so you can continue modifying survey point properties, as necessary.
    
7.  Press Enter to end the command.
    
    You are prompted to update the linework associated with the survey points.
    
    Note:
    
    All survey points must be reprocessed because any point potentially can contribute to the definition of any generated figure.
    
8.  Click Yes.
9.  In the Process Linework dialog box, clear the Insert Survey Points check box.
    
    In this case, it is only necessary to update the figure linework. The point coordinates did not change.
    
10.  Click OK.
     
     The linework is reprocessed, and the survey figure is corrected.
     
     ![](../images/GUID-FCB36BB8-B836-4522-8938-5863391A922C.png)
     
11.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save As.
12.  Navigate to the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). For File Name, enter **Survey-3.dwg**. Click Save.

To continue to the next tutorial, go to [Viewing and Editing Survey Data](GUID-48040D82-47A0-41C8-86B9-247D2520C977.htm "This tutorial demonstrates how to view and modify survey data in your drawing.").

**Parent topic:** [Tutorial: Importing Survey Data](GUID-0A5C1B52-4F94-4388-91E6-E9F580EFF18B.htm "This tutorial demonstrates how to import survey data into a drawing, modify the data, and then reprocess the data.")