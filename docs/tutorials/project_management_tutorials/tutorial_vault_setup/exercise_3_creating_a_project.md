---
title: "Exercise 3: Creating a Project"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-256C9E05-8485-4340-AB16-3550414EFC63.htm"
category: "tutorial_vault_setup"
last_updated: "2026-03-17T18:42:21.525Z"
---

                 Exercise 3: Creating a Project  

# Exercise 3: Creating a Project

This exercise demonstrates how to log in to the project management system and create a project.

A project is a collection in the Prospector tree that represents objects in a database (vault). Each project is essentially a folder that contains drawings, databases of points, and reference objects, such as surfaces, alignments, and pipe networks. A project folder can also contain other documents relevant to an engineering project.

A database user with the Document Editor (Level 2) role can create projects.

The following procedure assumes that you are already logged in to the database as described in [Exercise 1: Logging In to Autodesk Vault](GUID-F3D4C11B-6BF8-4A3F-92B3-EA7AD90EE5BC.htm "In this exercise, you will log in to Autodesk Vault to prepare for other project tasks.").

Create a project

1.  In Toolspace, on the Prospector tab, right-click the Projects collection, then click New.
2.  In the New Project dialog box, for Name, enter **Tutorial Vault Project**. Optionally, add a short Description.
3.  Select the Use Project Template check box.
    
    When you select this option, you can specify a template to use in structuring your project. This option enables you to structure similar projects in the same way.
    
4.  In the Project Templates Folder area, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
5.  In the Set Working Folder dialog box, navigate to the [Civil 3D Project Templates folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS73099CC142F48755-1C09418211F25B18DC4-4773). Click Select Folder.
    
    This is a typical structure for an Autodesk Civil 3D project. Folders are provided for many of the document types that are typical of a civil engineering project.
    
6.  In the New Project dialog box, in the Project Template field, select \_Sample Project.
7.  Click OK.
8.  Under the Projects collection, expand the **Tutorial Vault Project** collection.
    
    Examine the folders and nodes that were created from the template you specified in Step 7. For example, the Drawings collection contains folders in which you can save different types of project drawings. The object collections will contain data references to Autodesk Civil 3D objects that reside in the project drawings.
    

To continue to the next tutorial, go to [Creating, Referencing, and Modifying Project Object Data](GUID-6289A7A6-7CD1-4BC9-8DB0-7E3A96406F99.htm "In this tutorial, you will add a drawing to the project, create a project surface and then access the surface from another drawing. You will use the Surface-3.dwg tutorial drawing as the starting point.").

**Parent topic:** [Tutorial: Vault Setup](GUID-05E12EDB-B70D-4E41-9C06-A304036C9C74.htm "In this tutorial, you will act as a project administrator, creating a project in Autodesk Vault and some sample users.")