---
title: "Exercise 3: Checking Out and Modifying a Project Object"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-9FBF5FEB-3C52-4BF4-A192-46B9FEC48506.htm"
category: "tutorial_creating_referencing_and_modifying_projec"
last_updated: "2026-03-17T18:42:22.245Z"
---

                  Exercise 3: Checking Out and Modifying a Project Object  

# Exercise 3: Checking Out and Modifying a Project Object

You check out a project object by checking out the drawing that contains the object.

In this exercise, you will check out and modify the drawing that contains the surface object XGND.

This exercise continues from [Exercise 2: Creating a Reference to a Project Object](GUID-085C2CC5-8EEB-4E2A-A9D0-C87457A88E80.htm "In this exercise, you will create a drawing and create a read-only copy of a project surface in the drawing.").

Check out a reference object

1.  On the Prospector tab, expand the Projects![](../images/ac.menuaro.gif) Tutorial Vault Project ![](../images/ac.menuaro.gif)Drawings collection.
2.  Select the Source Drawings![](../images/ac.menuaro.gif)Surfaces folder to display the list view for the collection.
    
    In the list view, note that the Version for _Project-XGND.dwg_ is 1.
    
    The ![](../images/GUID-C54FEAA9-53D9-4394-A04F-F6115E1E35B1.png) icon next to _Project-XGND.dwg_ indicates that the drawing is checked in.
    
3.  Right-click _Project-XGND.dwg_ and click Check Out.
4.  In the Check Out Drawing dialog box, verify that _Project-XGND.dwg_ is selected.
5.  Click OK.
    
    The drawing is checked out and opened. The ![](../images/GUID-6EFDF643-8563-440F-BF16-F17A6A625F42.png) icon next to _Project-XGND.dwg_ in the Open Drawings collection indicates that the drawing is checked out to you. In the next few steps, you will modify the surface by adding a breakline.
    

Modify the source object

1.  With the surface clearly visible, click ![](../images/GUID-22822D5C-CB72-4572-B9BC-99874C518FC7.png) (the Polyline tool) and draw a polyline anywhere on the surface.
2.  Expand Open Drawings![](../images/ac.menuaro.gif) Project-XGND ![](../images/ac.menuaro.gif)Surfaces![](../images/ac.menuaro.gif) XGND ![](../images/ac.menuaro.gif)Definition.
3.  In the surface Definition, right-click the Breaklines collection. Click Add.
4.  In the Add Breaklines dialog box, optionally give the breakline a name. Click OK. In the drawing window, click the polyline to convert it to a breakline.
5.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save to save the changes to the surface.

**Further exploration:** If you want to see how the drawing icon appears to another database user, complete the following steps:

*   On the Prospector tab, Master View, right-click the Projects collection and click Log Out.
*   Right-click the Projects collection, click Log In, then log in as user **kgreen**.
*   In the Tutorial Vault Project, note the ‘checked out’ icon next to _Project-XGND.dwg_.
*   Hover the cursor over the ‘checked out’ icon to see a tooltip that indicates which user has checked out the drawing.
*   Log out as **kgreen**, then log in as **pred**.

To continue this tutorial, go to [Exercise 4: Checking In a Project Object](GUID-5171131F-0121-469C-9A94-5B5C8A47F002.htm "You check in a project object by checking in the checked-out drawing that contains it.").

**Parent topic:** [Tutorial: Creating, Referencing, and Modifying Project Object Data](GUID-6289A7A6-7CD1-4BC9-8DB0-7E3A96406F99.htm "In this tutorial, you will add a drawing to the project, create a project surface and then access the surface from another drawing. You will use the Surface-3.dwg tutorial drawing as the starting point.")