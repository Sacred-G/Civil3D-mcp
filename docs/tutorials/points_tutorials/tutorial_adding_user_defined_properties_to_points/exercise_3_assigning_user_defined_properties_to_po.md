---
title: "Exercise 3: Assigning User-Defined Properties to Points"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-9EEC2A7E-9995-4642-979C-589646C5594B.htm"
category: "tutorial_adding_user_defined_properties_to_points"
last_updated: "2026-03-17T18:42:08.529Z"
---

                  Exercise 3: Assigning User-Defined Properties to Points  

# Exercise 3: Assigning User-Defined Properties to Points

In this exercise, you will use point groups to associate user-defined properties with points in your drawing.

This exercise continues from [Exercise 2: Creating a Label Style That Displays a User-Defined Property](GUID-18D4A323-FF35-4CC0-B3F6-DB093F999F2D.htm "In this exercise, you will create a label style that displays user-defined property information for a point.").

Assign user-defined properties to points

1.  Open Points-4c.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Prospector tab, click Point Groups.
3.  In the item view, click the **Storm Manholes** entry. In the Classification column, select **Manhole UDP**.

Apply the user-defined property point label style

1.  In the Prospector tree view, expand Point Groups. Click **Storm Manholes**.
2.  In the item view, right-click in a column heading.
3.  Clear the check mark from all items in the list, except the following:
    
    *   **Point Number**
    *   **Point Label Style**
    *   **MH\_Pipe In Invert**
    *   **MH\_Pipe In Material**
    
    Clearing the check boxes turns off the display of columns you do not need to see for this exercise.
    
4.  Click the row for point **307**.
5.  Click the Point Label Style cell to display the Select Label Style dialog box.
6.  In the Select Label Style dialog box, select **Manhole UDP** as the label style. Click OK.
7.  Repeat Steps 4 to 6 to apply the **Manhole UDP** for point **667**.

Specify user-defined property values

1.  For point **307**, click the **MH\_Pipe In Invert** cell. Enter **93.05**.
2.  Right-click the row for point **307**. Click Zoom To.
    
    The value is displayed with the other point information in the drawing window.
    
3.  For point **667**, click the **MH\_Pipe In Invert** cell. Enter **93.00**.
4.  Right-click the entry for point **667**. Click Zoom To.

To continue this tutorial, go to [Exercise 4: Importing Points with User-Defined Properties](GUID-25875A26-A363-4E9E-BEA6-83E0A1346FD5.htm "In this exercise, you will create a custom point file format, and then import point information that includes user-defined properties from an external file.").

**Parent topic:** [Tutorial: Adding User-Defined Properties to Points](GUID-6F9EFF4C-4D8F-478B-A246-3FCC3B14230A.htm "This tutorial demonstrates how to add custom properties to points.")