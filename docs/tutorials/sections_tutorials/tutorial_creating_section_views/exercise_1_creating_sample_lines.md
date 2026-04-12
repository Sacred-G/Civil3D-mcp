---
title: "Exercise 1: Creating Sample Lines"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-408653EF-997A-46BD-96A2-40E832FDBBCA.htm"
category: "tutorial_creating_section_views"
last_updated: "2026-03-17T18:42:58.142Z"
---

                 Exercise 1: Creating Sample Lines  

# Exercise 1: Creating Sample Lines

In this exercise, you will create a set of sample lines along the alignment.

The sample lines define the chainages at which the cross sections are cut, and also the width of the sections to the left and right of the alignment. A set of sample lines is stored in a Sample Line Group for the alignment. Each sample line group has a unique name. Each line within the group also has a unique name.

Create sample lines

1.  Open _Sections-Sample-Lines-Create.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Sample Lines ![](../images/GUID-69FD1393-7647-473B-87EB-9D583771DCCB.png) Find.
3.  At the Select An Alignment prompt, press Enter.
4.  In the Select Alignment dialog box, select **Centerline (1)**. Click OK.
    
    The Create Sample Line Group dialog box is displayed. This dialog box defines the characteristics of the sample line group. The templates shipped with Autodesk Civil 3D contain pre-defined line styles and line label styles for the sample lines.
    
5.  In the top portion of the Create Sample Line Group dialog box, specify the following parameters:
    *   Sample Line Style: **Road Sample Lines**
    *   Sample Line Label Style: **Name & Section Marks**
6.  Under Select Data Sources To Sample, verify that the Sample check boxes are selected for all entries in the table.
    
    Data sources may include surfaces, corridor models, and corridor surfaces. Each surface and corridor surface results in a single cross-sectional string. Using the corridor model as a source includes all of the points, links, and shapes in the model.
    
7.  Set the Section Styles to the following:
    
    Note:
    
    You can double-click a Style cell in the table to select the Section Style.
    
    *   **EG**: **Existing Ground**
    *   **Corridor - (1)**: **All Codes**
    *   **Corridor - (1) Top**: **Finished Gradient**
    *   **Corridor - (1) Datum**: **Finished Gradient**
8.  Click OK.
    
    The Sample Line Tools toolbar is displayed. A Specify Chainage prompt is displayed on the command line.
    
9.  On the toolbar, click the arrow next to the ![](../images/GUID-3C2DA29A-AF3D-4BCD-A691-AF246BA467ED.png)Sample Line Creation Methods button. Click ![](../images/GUID-8B4C458E-9942-4BF7-98B5-C4148CCEEB4B.png)From Corridor Stations.
    
    This option creates a sample line at each chainage found in the corridor model.
    
10.  In the Create Sample Lines - From Corridor Chainages dialog box, specify the following parameters:
     *   Left Swath Width: **150**
     *   Right Swath Width: **150**
11.  Click OK.
     
     The sample lines are created, and the Sample Line Tools toolbar is available for defining additional lines, if desired.
     
     ![](../images/GUID-A31A205C-21C6-4B36-8BC2-08C2FCD6A77E.png)
     
12.  Close the Sample Lines Tools toolbar.

To continue this tutorial, go to [Exercise 2: Creating Section Views](GUID-F417BAD7-CDCA-4615-BCCE-2860E6D49603.htm "In this exercise, you will create a section view for a range of sample lines.").

**Parent topic:** [Tutorial: Creating Section Views](GUID-A9C4C30C-46F2-44CB-B12A-4B0417F105DA.htm "This tutorial demonstrates how to display cross sections of the corridor model surfaces along the centerline alignment. You will create sample lines and then generate the sections.")