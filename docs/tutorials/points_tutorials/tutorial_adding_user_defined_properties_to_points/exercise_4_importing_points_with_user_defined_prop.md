---
title: "Exercise 4: Importing Points with User-Defined Properties"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-25875A26-A363-4E9E-BEA6-83E0A1346FD5.htm"
category: "tutorial_adding_user_defined_properties_to_points"
last_updated: "2026-03-17T18:42:08.571Z"
---

                  Exercise 4: Importing Points with User-Defined Properties  

# Exercise 4: Importing Points with User-Defined Properties

In this exercise, you will create a custom point file format, and then import point information that includes user-defined properties from an external file.

This exercise continues from [Exercise 3: Assigning User-Defined Properties to Points](GUID-9EEC2A7E-9995-4642-979C-589646C5594B.htm "In this exercise, you will use point groups to associate user-defined properties with points in your drawing.").

Create a point file format for importing user-defined properties

1.  Open Points-4d.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Settings tab, expand the Point collection. Right-click Point File Formats. Click New.
3.  In the Point File Formats – Select Format Type dialog box, select User Point File. Click OK.
4.  In the Point File Format dialog box, specify the following properties:
    *   Format Name: **Manhole Data**
    *   Comment Tag: **#**
    *   Format Options: **Delimited By**
    *   Delimited By: _,_ (a comma)
5.  In the table of column names, click the first column heading (labeled <unused>).
6.  In the Point File Formats – Select Column Name dialog box, in the Column Name list, select Point Number. Click OK.
7.  Repeat steps 5 and 6 to name additional columns using the following values:
    *   Column 2: **MH\_Material**
    *   Column 3: **MH\_Diameter**
    *   Column 4: **MH\_Pipe In Invert**
    *   Column 5: **MH\_Pipe In Diameter**
    *   Column 6: **MH\_Pipe In Material**
    *   Column 7: **MH\_Pipe Out Invert**
    *   Column 8: **MH\_Pipe Out Diameter**
    *   Column 9: **MH\_Pipe Out Material**
8.  Click OK.

Import user-defined property data from a text file

1.  In Toolspace, on the Prospector tab, ensure that the Point Groups collection is expanded, and select the **Storm Manholes** group.
    
    In the item view, note that this group contains only nine points, and some of the data columns are blank.
    
2.  In Toolspace, on the Prospector tab, right-click Points. Click Create.
3.  In the Create Points dialog box, click ![](../images/GUID-4E333A76-152F-4849-BCDC-D3F54BC5705C.png)Import Points.
4.  In the Import Points dialog box, in the Format list, select **Manhole Data**.
5.  Click ![](../images/GUID-7633B109-B0E2-42D8-8A07-E27BBF28B731.png). Browse to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC). Select manhole\_data.txt. Click Open.
6.  Click OK.
7.  In the Duplicate Point Number dialog box, in the Resolution list, select Merge. Click OK.
    
    The point data is imported.
    
8.  On the Prospector tab, click the **Storm Manholes** point group.
    
    The point data from the file import is displayed in the item view, including specific values for manhole data.
    
9.  Close the Create Points dialog box.

To continue this tutorial, go to [Exercise 5: Querying User-Defined Property Information](GUID-9D050261-65C3-490D-A7E1-2718FB23F379.htm "In this exercise, you will create a point group. The list of points included in the group is determined by a query that contains user-defined properties.").

**Parent topic:** [Tutorial: Adding User-Defined Properties to Points](GUID-6F9EFF4C-4D8F-478B-A246-3FCC3B14230A.htm "This tutorial demonstrates how to add custom properties to points.")