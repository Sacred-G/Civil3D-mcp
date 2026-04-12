---
title: "Exercise 2: Creating a Reference to a Project Object"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-085C2CC5-8EEB-4E2A-A9D0-C87457A88E80.htm"
category: "tutorial_creating_referencing_and_modifying_projec"
last_updated: "2026-03-17T18:42:22.200Z"
---

                 Exercise 2: Creating a Reference to a Project Object  

# Exercise 2: Creating a Reference to a Project Object

In this exercise, you will create a drawing and create a read-only copy of a project surface in the drawing.

This exercise continues from [Exercise 1: Adding a Drawing to the Project](GUID-A49170B0-674E-4011-B65A-AB003B092A45.htm "In this exercise, you will add a drawing to a project. In the process, you will create a shared project surface.").

Create a reference to a project object

1.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)New.
2.  In the Select Template dialog box, click _\_Autodesk Civil 3D (Imperial) NCS.dwt_. Click Open.
3.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save As.
4.  In the Save Drawing As dialog box, browse to the following location:
    
    [Civil 3D Projects folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24221) **\\Tutorial Vault Project\\Production Drawings**
    
5.  Save the file as _Project-1.dwg_.
6.  On the Prospector tab, expand the Projects collection ![](../images/ac.menuaro.gif) Tutorial Vault Project ![](../images/ac.menuaro.gif)Surfaces. Right-click XGND. Click Create Reference.
7.  In the Create Surface Reference dialog box, change the surface style from Contours 2’ And 10’ (Background) to **Contours And Triangles**.
    
    This style specifies how the surface XGND will be displayed in the drawing Project-1. This style setting is independent of the surface style used for the master copy of the surface in _Project-XGND.dwg_.
    
8.  Click OK.
    
    Note:
    
    If the Event Viewer is displayed, indicating that the surface is created, close it.
    
    The drawing Project-1 now contains a read-only copy of the project surface XGND. The master copy of the object remains untouched in the project database. The drawing Project-1 is not in the project database, but it is considered to be attached to the Tutorial Vault Project because it contains a reference to an object in that project. While this link remains, the drawing Project-1 can include references to other objects in the Tutorial project, but cannot contain references to objects in other projects.
    
9.  Save and close the drawing.

To continue this tutorial, go to [Exercise 3: Checking Out and Modifying a Project Object](GUID-9FBF5FEB-3C52-4BF4-A192-46B9FEC48506.htm "You check out a project object by checking out the drawing that contains the object.").

**Parent topic:** [Tutorial: Creating, Referencing, and Modifying Project Object Data](GUID-6289A7A6-7CD1-4BC9-8DB0-7E3A96406F99.htm "In this tutorial, you will add a drawing to the project, create a project surface and then access the surface from another drawing. You will use the Surface-3.dwg tutorial drawing as the starting point.")