---
title: "Exercise 5: Querying User-Defined Property Information"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-9D050261-65C3-490D-A7E1-2718FB23F379.htm"
category: "tutorial_adding_user_defined_properties_to_points"
last_updated: "2026-03-17T18:42:08.640Z"
---

                 Exercise 5: Querying User-Defined Property Information  

# Exercise 5: Querying User-Defined Property Information

In this exercise, you will create a point group. The list of points included in the group is determined by a query that contains user-defined properties.

This exercise continues from [Exercise 4: Importing Points with User-Defined Properties](GUID-25875A26-A363-4E9E-BEA6-83E0A1346FD5.htm "In this exercise, you will create a custom point file format, and then import point information that includes user-defined properties from an external file.").

Create a point query

1.  Open Points-4e.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Prospector tab, right-click Point Groups. Click New.
3.  In the Point Group Properties dialog box, on the Information tab, for Name, enter **Storm Manholes - Invert In**.
4.  On the Query Builder tab, select Modify Query.
5.  Right-click the Query Builder table. Click Insert Row.
6.  Click the row you created. Click Name in the Property column. In the Property list, select **MH\_Pipe In Invert**.
7.  Click the Operator value. In the Operator list, select < (less than).
8.  Click the Value value. Enter **93**.
9.  Click OK.
10.  On the Prospector tab, click **Storm Manholes - Invert In**.
     
     A list of points that match your query is displayed in the item view. Points number 307 and 667 are excluded, because in a previous exercise you set their values for MH\_Pipe In Invert to 93.05 and 93.00.
     

**Parent topic:** [Tutorial: Adding User-Defined Properties to Points](GUID-6F9EFF4C-4D8F-478B-A246-3FCC3B14230A.htm "This tutorial demonstrates how to add custom properties to points.")