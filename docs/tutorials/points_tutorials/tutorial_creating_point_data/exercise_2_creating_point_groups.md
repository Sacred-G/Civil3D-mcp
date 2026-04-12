---
title: "Exercise 2: Creating Point Groups"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-B65F5014-9D35-46D2-8C29-83721245C1AC.htm"
category: "tutorial_creating_point_data"
last_updated: "2026-03-17T18:42:06.970Z"
---

                 Exercise 2: Creating Point Groups  

# Exercise 2: Creating Point Groups

In this exercise, you will create point groups to sort the points as they are imported into a drawing.

This exercise continues from [Exercise 1: Creating Description Keys](GUID-0C6FE34A-3273-4150-9260-AC906D357FD4.htm "In this exercise, you will create description keys to sort the points as they are imported into a drawing.").

Create point groups

Note:

This exercise uses Points-1.dwg with the modifications you made in the previous exercise.

2.  In Toolspace, on the Prospector tab, right-click the Point Groups collection. Click New.
3.  In the Point Group Properties dialog box, on the Information tab, in the Name field, enter **Detention Pond**. Optionally, enter a short description in the Description field.
4.  On the Raw Desc Matching tab, select **POND\***. Click Apply.
    
    This option specifies that all points with the POND\* raw description are added to the Detention Pond point group.
    
    Notice how the description key setting is recorded on both the Include and Query Builder tabs. If you know SQL, you can see how you could add more criteria to the Query Builder tab to select a more specific set of points for the point group.
    
5.  Click OK.
6.  Create another point group by repeating Steps 1 through 4, but use the following parameters:
    
    Name: **Storm Manholes**
    
    Raw Desc Matching: **MHST\***
    
    Your drawing should now contain the same description keys and point groups shown in sample drawing _Points-1a.dwg_.
    
    Note:
    
    The \_All Points point group is created automatically. A point can belong to other point groups in the drawing, but it is always a member of the \_All Points point group.
    

Change the point group label style

1.  In Toolspace, on the Prospector tab, expand the Point Groups collection.
2.  Right-click the \_All Points collection. Click Properties.
3.  In the Point Group Properties dialog box, on the Information tab, change the Point Label Style to **Standard**.
4.  Click OK to close the Point Group Properties dialog box.

To continue this tutorial, go to [Exercise 3: Importing Points from a Database](GUID-8CEE8248-BC26-4737-AC3F-F00E5FB596D2.htm "In this exercise, you will import points from a database to a drawing that uses description keys to sort points into groups.").

**Parent topic:** [Tutorial: Creating Point Data](GUID-1A4FCB0E-F708-4AF5-B137-7942ECC3D819.htm "This tutorial demonstrates several useful setup tasks for organizing a large set of points.")