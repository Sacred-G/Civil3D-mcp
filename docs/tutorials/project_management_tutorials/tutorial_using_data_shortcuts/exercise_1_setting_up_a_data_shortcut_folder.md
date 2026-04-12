---
title: "Exercise 1: Setting Up a Data Shortcut Folder"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6F22D431-CC2B-4517-A11D-0713B8F49FBF.htm"
category: "tutorial_using_data_shortcuts"
last_updated: "2026-03-17T18:42:20.710Z"
---

                 Exercise 1: Setting Up a Data Shortcut Folder  

# Exercise 1: Setting Up a Data Shortcut Folder

In this exercise, you will set up a folder in which to store objects that are referenced through data shortcuts.

The data shortcut folder contains all the source drawings and data shortcut objects in a project.

Access the project management tools in Prospector

1.  Open drawing _Project Management-1.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains an existing ground surface, alignments that represent intersecting road centerlines, and plot objects that represent property boundaries. In the following exercises, you will create data shortcuts to the surface and alignments in this drawing, and then reference them in a new drawing.
    
    ![](../images/GUID-B3B5B8EC-33EA-424A-94BC-967F212D3911.png)
    
2.  In Toolspace, on the Prospector tab, select the Master View.
    
    ![](../images/GUID-F0D084D0-9EE4-48D3-867A-3ABCCC201669.png)
    

Set the working folder

1.  Right-click the ![](../images/GUID-93B3E175-74C4-41CA-9592-39DA738CF826.png)Data Shortcuts collection. Click Set Working Folder.
    
    The working folder is the parent folder where you save project folders. For this exercise, you will specify a folder on your hard drive as your working folder.
    
2.  In the Set Working Folder dialog box, navigate to the [Civil 3D Projects folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79). Click Select Folder.

Create a data shortcuts project

1.  Right-click the ![](../images/GUID-93B3E175-74C4-41CA-9592-39DA738CF826.png)Data Shortcuts collection. Click New Data Shortcuts Project Folder.
    
    Notice that the working folder you specified is displayed in the Working Folder field.
    
2.  In the New Data Shortcut Folder dialog box, select the Use Project Template check box.
3.  Under Project Templates Folder, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
4.  In the Browse to Project Template Folder dialog box, select the [Civil 3D Project Templates folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79). Click Select Folder.
5.  In the New Data Shortcuts Folder dialog box, specify the following parameters:
    *   Name: **Tutorial Data Shortcuts Project**
    *   Use Project Template: **Selected**
    *   Project Template: select **\_Sample Project**
6.  Click OK.
7.  Using Windows Explorer, navigate to the [Civil 3D Projects folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79). Examine the folder structure in the Tutorial Data Shortcuts Project folder.
    
    The folder structure provides separate locations for data shortcuts, source drawings, and other data. You will save project objects in these folders in the next exercise.
    
    This is a typical structure for an Autodesk Civil 3D project. Folders are provided for many of the document types that are typical of a civil engineering project.
    

To continue this tutorial, go to [Exercise 2: Creating Data Shortcuts](GUID-1BE3A220-97C8-46D7-B9BA-436133B7F160.htm "In this exercise, you will create data shortcuts from the objects in a drawing. The data shortcuts will be available to reference into other drawings.").

**Parent topic:** [Tutorial: Using Data Shortcuts](GUID-68AC7599-F635-4391-9F9A-6E9EF96F9ED2.htm "This tutorial demonstrates how to create a data shortcuts project, create data shortcuts from objects in one drawing, and then import the data shortcuts into another drawing.")