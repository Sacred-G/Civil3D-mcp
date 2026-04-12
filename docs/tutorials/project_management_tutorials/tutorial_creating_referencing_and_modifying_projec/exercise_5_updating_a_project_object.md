---
title: "Exercise 5: Updating a Project Object"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-9C013C98-2F52-41C8-9AA2-9067C81D5601.htm"
category: "tutorial_creating_referencing_and_modifying_projec"
last_updated: "2026-03-17T18:42:22.337Z"
---

                 Exercise 5: Updating a Project Object  

# Exercise 5: Updating a Project Object

When the drawing that contains a project object is checked in to the database, the changes are immediately available to other drawings that reference the object.

In this exercise you will open the drawing _Project-1.dwg_ that references the surface XGND, which was modified in the previous exercise.

This exercise continues from [Exercise 4: Checking In a Project Object](GUID-5171131F-0121-469C-9A94-5B5C8A47F002.htm "You check in a project object by checking in the checked-out drawing that contains it.").

To update a project object

1.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Open. Navigate to [Civil 3D Projects folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24221) **\\Tutorial Vault Project\\Production Drawings**, and click the drawing _Project-1.dwg_.
2.  In Toolspace, on the Prospector tab, expand the Surfaces collection. Right-click the surface **XGND**, and click Surface Properties.
3.  In the Surface Properties dialog box, change the Surface Style to one that will make the breaklines visible, such as any of the Contours styles.
    
    Notice that the breaklines are displayed on the surface, but you cannot move them. Also, because this surface is a reference object, the surface collection in Prospector does not show the breaklines or other elements of the surface definition that you cannot modify.
    
    **Further exploration:** Keep the drawing _Project-1.dwg_ open and check out _Project-XGND.dwg_. Add another breakline to the master copy of the surface, then right-click Project-1 and click Switch To to make this drawing active. See how the change to the surface is reflected in a reference object within a drawing that is already open.
    

To continue to the next tutorial, go to [Creating and Modifying Project Point Data](GUID-218EB811-9D4C-41F2-BB02-8FFA5B823BF1.htm "This tutorial demonstrates how to create, access, and modify project point data.").

**Parent topic:** [Tutorial: Creating, Referencing, and Modifying Project Object Data](GUID-6289A7A6-7CD1-4BC9-8DB0-7E3A96406F99.htm "In this tutorial, you will add a drawing to the project, create a project surface and then access the surface from another drawing. You will use the Surface-3.dwg tutorial drawing as the starting point.")