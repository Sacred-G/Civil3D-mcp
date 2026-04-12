---
title: "Exercise 2: Creating Data Shortcuts"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-1BE3A220-97C8-46D7-B9BA-436133B7F160.htm"
category: "tutorial_using_data_shortcuts"
last_updated: "2026-03-17T18:42:20.752Z"
---

                 Exercise 2: Creating Data Shortcuts  

# Exercise 2: Creating Data Shortcuts

In this exercise, you will create data shortcuts from the objects in a drawing. The data shortcuts will be available to reference into other drawings.

This exercise continues from [Exercise 1: Setting up a Data Shortcut Folder](GUID-6F22D431-CC2B-4517-A11D-0713B8F49FBF.htm "In this exercise, you will set up a folder in which to store objects that are referenced through data shortcuts.").

Save the source drawing with the project

Note:

This exercise uses _Project Management-1.dwg_ with the modifications you made in the previous exercise.

2.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save As.
3.  In the Save Drawing As dialog box, navigate to the [Civil 3D Projects folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24221) **\\Tutorial Data Shortcuts Project\\Source Drawings** folder. Click Save.
    
    Source drawings that contain objects that are referenced in other drawings should be saved with the data shortcuts project.
    

Create data shortcuts

1.  Click Manage tab![](../images/ac.menuaro.gif)Data Shortcuts panel![](../images/ac.menuaro.gif)Create Data Shortcuts ![](../images/GUID-477377B3-3163-4B5F-BA2E-A6029D7A0266.png) Find.
    
    Note:
    
    As a best practice, each object should reside in a separate drawing. To save time in this exercise, all the reference objects are in the current drawing.
    
2.  In the Create Data Shortcuts dialog box, select the following check boxes:
    
    *   Surfaces
    *   Alignments
    
    This action selects the EG surface and both alignments in the drawing.
    
3.  Click OK.
    
    Now that the data shortcuts have been created, the current drawing is associated with the data shortcuts project. Notice that, in the Autodesk Civil 3D title bar, \[Tutorial Data Shortcuts Project\] is displayed after the drawing name.
    

Examine the data shortcuts in the project

1.  In Toolspace, on the Prospector tab, expand the ![](../images/GUID-93B3E175-74C4-41CA-9592-39DA738CF826.png)Data Shortcuts collection. Expand the ![](../images/GUID-0122809D-CF9C-46AA-BE12-CCF3CC92BD44.png)Surfaces and ![](../images/GUID-BC70BFC5-1271-4D5C-A684-893CC195DF39.png)Alignments collections.
    
    Notice that data shortcuts have been created for the objects you selected. In the next exercise, you will reference these objects in another drawing.
    
2.  Using Windows Explorer, navigate to the [Civil 3D Projects folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24221) **\\Tutorial Data Shortcuts Project**. Examine the contents of the subfolders:
    
    *   \_Shortcuts\\Alignments: This folder contains an XML file for each alignment in the source drawing. The XML files identify the path to the drawing that contains the alignment, the name of the source drawing, and the name of the alignment.
    *   \_Shortcuts\\Profiles: This folder contains an XML file for each profile in the source drawing. The XML files identify the path to the drawing that contains the profile, the name of the source drawing, and the name of the profile.
    *   \_Shortcuts\\Surfaces: This folder contains an XML file for the EG surface.
    *   Source Drawings: This folder contains the source drawing. The source drawings should always be saved with the data shortcut project. In a real project, you would save the drawings that contain each object in the subfolders.
    
    While it is useful to know that the data shortcut XML files exist, you do not work directly with them in normal data referencing operations. Management of data references is done in Toolspace on the Prospector tab.
    

Note:

Leave _Project Management-1.dwg_ open for the next exercise.

To continue this tutorial, go to [Exercise 3: Referencing Data Shortcuts](GUID-04D4992A-9BFE-41F9-8BB7-5B0D7BB48973.htm "In this exercise, you will reference several shortcuts in a new drawing.").

**Parent topic:** [Tutorial: Using Data Shortcuts](GUID-68AC7599-F635-4391-9F9A-6E9EF96F9ED2.htm "This tutorial demonstrates how to create a data shortcuts project, create data shortcuts from objects in one drawing, and then import the data shortcuts into another drawing.")