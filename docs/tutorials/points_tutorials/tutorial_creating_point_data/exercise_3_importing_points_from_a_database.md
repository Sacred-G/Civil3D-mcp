---
title: "Exercise 3: Importing Points from a Database"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-8CEE8248-BC26-4737-AC3F-F00E5FB596D2.htm"
category: "tutorial_creating_point_data"
last_updated: "2026-03-17T18:42:07.011Z"
---

                 Exercise 3: Importing Points from a Database  

# Exercise 3: Importing Points from a Database

In this exercise, you will import points from a database to a drawing that uses description keys to sort points into groups.

This exercise continues from [Exercise 2: Creating Point Groups](GUID-B65F5014-9D35-46D2-8C29-83721245C1AC.htm "In this exercise, you will create point groups to sort the points as they are imported into a drawing.").

Import points from a database

Note:

This exercise uses Points-1.dwg with the modifications you made in the previous exercise, or you can open Points-1a.dwg from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  In Toolspace, on the Prospector tab, right-click Points. Click Create.
3.  In the Create Points dialog box, click ![](../images/GUID-1E5D0577-3361-4904-8C59-5B4823488488.png). Expand the Default Layer parameter, then change the value to **V-NODE**.
4.  In the Create Points dialog box, click ![](../images/GUID-4E333A76-152F-4849-BCDC-D3F54BC5705C.png)Import Points.
5.  In the Format list, select External Project Point Database.
6.  Click ![](../images/GUID-7633B109-B0E2-42D8-8A07-E27BBF28B731.png). Browse to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC). Select _points.mdb_. Click Open.
7.  In the Import Points dialog box, clear the Advanced Options check boxes.
8.  Click OK.
    
    The points are imported.
    
9.  In Toolspace, on the Prospector tab, right-click the \_All Points point group. Click Zoom To.
    
    The points are displayed both in the drawing and in tabular form in the Toolspace list view. In the drawing window, if you move the cursor over a point, a tooltip displays basic data about the point. Notice that the two stormwater point groups appear to be empty. This is because they have not been updated with their new content. In the next few steps, you will see how Autodesk Civil 3D provides several ways to check the point data before adding it to your drawing.
    

Update point groups

1.  Right-click the Point Groups collection. Click Properties.
    
    The Point Groups dialog box is displayed. Point groups are listed here according to their display order, with the highest priority group at the top. Arrows at the side of the dialog box allow you to change the display order. The icon ![](../images/GUID-9ECE03CD-D682-4912-943B-9E54DEDA464D.png) indicates that an update is pending for a point group.
    
2.  To show the contents of the update for each point group, click ![](../images/GUID-F865834B-BE0E-4316-9D01-FFFD8BDF90AD.png). Review the list of points that the application is prepared to add to the Storm Manholes and Detention Pond point groups.
3.  In the Point Group Changes dialog box, click Close.
4.  To update the point groups, click ![](../images/GUID-B2BA7BFD-A70C-481A-827E-7B95E34FB814.png). Click OK.
    
    Alternatively, you can right-click the Point Groups collection and click Update.
    
    The point groups update. Now, you can display their points in the list view and zoom to them in the drawing.
    
5.  Right-click a point group. Click Edit Points.
    
    The points are displayed in the Point Editor table. Review and change their attributes.
    
    Note:
    
    For information about changing the contents and display of the Panorama window, see the [Using the Panorama Window tutorial](GUID-C72EBB8E-D382-408B-94FF-896BFF054D6E.htm "In this exercise, you will learn how you can use and customize the Panorama window.").
    

To continue to the next tutorial, go to [Displaying and Editing Points](GUID-E43EE235-038D-49EB-8307-6E8D00A9B3A4.htm "This tutorial demonstrates how to use point groups, layers, external references, and styles to display points. It also explains the various ways to edit points using standard AutoCAD tools.").

**Parent topic:** [Tutorial: Creating Point Data](GUID-1A4FCB0E-F708-4AF5-B137-7942ECC3D819.htm "This tutorial demonstrates several useful setup tasks for organizing a large set of points.")