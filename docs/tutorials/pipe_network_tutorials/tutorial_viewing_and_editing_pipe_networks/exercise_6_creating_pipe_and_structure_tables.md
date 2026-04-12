---
title: "Exercise 6: Creating Pipe and Structure Tables"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-D11CD8CC-F499-49CC-A417-A639AC141B6A.htm"
category: "tutorial_viewing_and_editing_pipe_networks"
last_updated: "2026-03-17T18:43:08.835Z"
---

                    Exercise 6: Creating Pipe and Structure Tables  

# Exercise 6: Creating Pipe and Structure Tables

In this exercise, you will create a table that displays information about the structures in a pipe network. Then, you will create a table style to display other information.

The procedures for creating pipe and structure tables are very similar. While this exercise focuses on structure tables, you can use the same procedure to create pipe tables.

Note:

For more detailed tutorials on tables, go to the [Labels and Tables Tutorials](GUID-BDF3F02E-838E-443D-BFE3-3033F03E214F.htm "These tutorials will get you started creating and editing labels, label styles, and tables.").

This exercise continues from [Exercise 5: Viewing Pipe Network Parts in a Section View](GUID-194541AC-E060-4B46-BDA8-05CA66286D77.htm "In this exercise, you will view the pipe network parts in a section view.").

To create a structure table

Note:

This exercise uses _Pipe Networks-3C.dwg_ with the modifications you made in the previous exercise.

2.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Tables menu ![](../images/ac.menuaro.gif)Pipe Network![](../images/ac.menuaro.gif)Add Structure![](../images/GUID-E87B214D-C289-410F-A123-C47B0D03EDF1.png).
3.  In the Structure Table Creation dialog box, select By Network and click OK.
4.  Pan to a clear area in the drawing window and click to place the table.
5.  Zoom and pan to the structure table. Examine the contents of the table.
    
    Next, you will change the contents of the table by creating a table style.
    

Change the table contents by creating a new style

1.  Select the table by clicking one of the rulings. Right-click the structure table and select Table Properties.
2.  In the Table Properties dialog box, under Table Style, click the down arrow next to ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png). Click ![](../images/GUID-CA1AD018-7C68-4826-8BAB-59D50446DD6F.png)Copy Current Selection.
3.  In the Table Style dialog box, on the Information tab, change the Name to **Structure Stations and Details**.
4.  On the Data Properties tab, specify the following parameters:
    *   Sort Data: **Selected**
    *   Sorting Column: **1**
    *   Order: **Ascending**
        
        These settings ensure that the table rows are sorted in ascending order by the first column.
        
5.  In the Structure area, click ![](../images/GUID-7633B109-B0E2-42D8-8A07-E27BBF28B731.png) to add a new column to the table.
6.  Double-click the new column’s heading cell.
7.  In the Text Component Editor dialog box, in the preview pane, enter **Chainage**. Click OK.
8.  In the Table Style dialog box, in the Structure area, double-click in the Column Value cell in the Chainage column.
9.  In the Table Cell Components dialog box, in the Text Contents row, click in the Value cell. Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
10.  In the Text Component Editor dialog box, select the data that appears in the preview area and press Delete.
11.  In the preview area, enter **CHAINAGE =** .
12.  In the Properties list, select Structure Chainage. Change the Precision value to **1** then click ![](../images/GUID-70B44105-B2EC-4016-A100-FA435F289B52.png).
13.  Click OK twice.
14.  In the Table Style dialog box, click the heading cell of the Chainage column, and then drag the Chainage column over the Structure Details column. The Chainage column is placed between the Structure Name and Structure Details columns.
15.  Click OK twice.
     
     The table now displays the chainage at which each structure is located.
     

**Parent topic:** [Tutorial: Viewing and Editing Pipe Networks](GUID-DCDFFC24-DBD6-4A45-9F9D-7EFEAB45123F.htm "This tutorial demonstrates how you can view and edit the parts of your pipe network in profile and section views.")