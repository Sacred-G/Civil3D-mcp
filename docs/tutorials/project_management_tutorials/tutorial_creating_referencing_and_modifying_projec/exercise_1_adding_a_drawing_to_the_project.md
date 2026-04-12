---
title: "Exercise 1: Adding a Drawing to the Project"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-A49170B0-674E-4011-B65A-AB003B092A45.htm"
category: "tutorial_creating_referencing_and_modifying_projec"
last_updated: "2026-03-17T18:42:22.158Z"
---

                 Exercise 1: Adding a Drawing to the Project  

# Exercise 1: Adding a Drawing to the Project

In this exercise, you will add a drawing to a project. In the process, you will create a shared project surface.

You will create a drawing named _Project-XGND.dwg_ that contains the surface to be shared.

Save the drawing with the project

1.  Open drawing _Surface-3.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains COGO points and an existing ground surface. Because you will share this data with other users, you must save the drawing with the project.
    
2.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save As.
3.  In the Save Drawing As dialog box, browse to the following location:
    
    [Civil 3D Projects folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24221) **\\Tutorial Vault Project\\Source Drawings\\Surfaces**
    
4.  For File Name, enter **Project-XGND.dwg**.
5.  Click Save.
    
    Note:
    
    You must be logged in to Autodesk Vault to perform the following steps. see [Exercise 1: Logging In to Autodesk Vault](GUID-F3D4C11B-6BF8-4A3F-92B3-EA7AD90EE5BC.htm "In this exercise, you will log in to Autodesk Vault to prepare for other project tasks.") for instructions.
    

Create a reference object

1.  On the Prospector tab, ensure that Master View is selected, and expand the Open Drawings collection. Right-click _Project-XGND.dwg_ and click Check In.
2.  In the first Add To Project dialog box, select **Tutorial Vault Project**.
3.  Click Next.
4.  On the Select A Drawing Location page, select the Source Drawings![](../images/ac.menuaro.gif)Surfaces folder.
5.  Click Next.
6.  On the Drawing File Dependencies page, select _Project-XGND.dwg_.
7.  Click Next.
8.  On the Share Data page, select **XGND**.
9.  Click Finish.
10.  On the Prospector tab, under the Projects collection, expand the **Tutorial Vault Project** collection. Expand the Drawings collection, and the Source Drawings![](../images/ac.menuaro.gif)Surfaces folder.
     
     The ![](../images/GUID-C54FEAA9-53D9-4394-A04F-F6115E1E35B1.png) icon displayed next to _Project-XGND.dwg_ indicates that the drawing is available to be checked out.
     
11.  Under the **Tutorial Vault Project** collection, expand the Surfaces object collection.
     
     The ![](../images/GUID-C54FEAA9-53D9-4394-A04F-F6115E1E35B1.png) icon next to XGND indicates that the project drawing that contains the surface is available to be checked out.
     

To continue this tutorial, go to [Exercise 2: Creating a Reference to a Project Object](GUID-085C2CC5-8EEB-4E2A-A9D0-C87457A88E80.htm "In this exercise, you will create a drawing and create a read-only copy of a project surface in the drawing.").

**Parent topic:** [Tutorial: Creating, Referencing, and Modifying Project Object Data](GUID-6289A7A6-7CD1-4BC9-8DB0-7E3A96406F99.htm "In this tutorial, you will add a drawing to the project, create a project surface and then access the surface from another drawing. You will use the Surface-3.dwg tutorial drawing as the starting point.")