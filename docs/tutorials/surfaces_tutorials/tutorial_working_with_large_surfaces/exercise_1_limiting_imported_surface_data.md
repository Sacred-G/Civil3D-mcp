---
title: "Exercise 1: Limiting Imported Surface Data"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E82C376A-5E40-4921-8299-92D1E8E3D8A0.htm"
category: "tutorial_working_with_large_surfaces"
last_updated: "2026-03-17T18:42:10.783Z"
---

                  Exercise 1: Limiting Imported Surface Data  

# Exercise 1: Limiting Imported Surface Data

In this exercise, you will use a data clip boundary to restrict the quantity of points that is referenced by a surface.

Points that are in the point file, but outside the specified data clip boundary, will be ignored when the surface is built and during any subsequent surface editing operations.

Add a data clip boundary to a surface

1.  Open _Surface-2.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Prospector tab, expand the Surfaces collection. Expand the **EG** surface.
    
    The EG surface is currently empty. In the following steps, you will use the red polyline in the drawing to create a _Data Clip_ boundary, which will restrict imported surface data to the extents of the boundary. Then, you will import a relatively dense LIDAR point file and examine the results.
    
3.  Expand the **EG** surface ![](../images/ac.menuaro.gif)Definition collection. Right-click Boundaries. Click Add.
4.  In the Add Boundaries dialog box, specify the following parameters:
    *   Name: **Site**
    *   Type: **Data Clip**
    *   Mid-Ordinate Distance: **1.000’**
5.  Click OK.
6.  In the drawing window, click the red polyline.
    
    The polyline is added to the EG surface definition as a boundary. The presence of a boundary in the surface definition is indicated by the ![](../images/GUID-379A8AD5-3792-4A22-8815-E61573B7DCFD.png) marker next to the Boundaries item on the Prospector tab. When the Boundaries collection is selected, the boundaries that have been added to the surface appear in the Prospector list view.
    
    In the following steps, you will add a relatively dense LIDAR point file to the surface definition. The point file will be added only within the extents of the data clip boundary that you just added.
    

Import surface data from a point file

1.  Expand the **EG** surface ![](../images/ac.menuaro.gif)Definition collection. Right-click Point Files. Click Add.
2.  In the Add Point File dialog box, under Selected Files, click ![](../images/GUID-7633B109-B0E2-42D8-8A07-E27BBF28B731.png).
3.  In the Select Source File dialog box, ensure that the Files Of Type field is set to (\*.csv).
4.  Navigate to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC). Select _LIDAR\_ENZ (comma delimited).csv_. Click Open.
5.  In the Format list, select ENZ (Comma Delimited).
6.  In the Add Point File dialog box, clear both the Do Level Adjustment If Possible and Do Coordinate Transformation If Possible check boxes. Click OK.
    
    The point data is added to the drawing.
    
    A reference to the point file is added to the EG surface definition. The presence of point data in the surface definition is indicated by the ![](../images/GUID-379A8AD5-3792-4A22-8815-E61573B7DCFD.png) marker next to the Point Files item on the Prospector tab.
    
7.  Zoom in to the lower right corner of the surface.
    
    Notice that only points that are inside the data clip boundary have been imported, and that a green border was created from the imported data.
    
    The border is outside the red data clip boundary in some areas, and inside the data clip boundary in other areas. This happened because points in the point file that are outside the data clip boundary were excluded during the import operation. The green border is formed by the points at the outermost extents of the points that were imported.
    
    ![](../images/GUID-D5EFDAFF-96A5-4F50-A56D-12B2ED1C8592.png)
    
    Surface with points imported within a data clip boundary (left) and detail of the surface (right)
    
8.  On the command line, enter **ZE** to zoom to the extents of the drawing.

Restrict the surface data to a smaller area

1.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer drop-down. Next to the **C-TOPO-BNDY-CORR** layer, click ![](../images/GUID-F908C832-FEB1-4F3B-AB32-BC8C776F3D5F.png).
    
    This layer contains an orange polyline that you will use to create a second data clip boundary.
    
2.  Expand the **EG** surface ![](../images/ac.menuaro.gif)Definition collection. Right-click Boundaries. Click Add.
3.  In the Add Boundaries dialog box, specify the following parameters:
    *   Name: **Corridor**
    *   Type: **Data Clip**
    *   Mid-Ordinate Distance: **1.000’**
4.  Click OK.
5.  In the drawing window, click the orange polyline.
    
    The polyline is added to the EG surface definition as a boundary, but the point data did not change. Data clip boundaries only affect surface editing operations that are performed after the data clip boundary has been added. Because the points were added to the surface before the Corridor boundary, the boundary currently does not affect the point data.
    
    In the following steps, you will rearrange the surface definition operations so that the points will be restricted to the extents of the new Corridor data clip boundary.
    
6.  In Toolspace, on the Prospector tab, right-click the **EG** surface. Click Surface Properties.
7.  In the Surface Properties dialog box, on the Definition tab, in the Operation Type column, examine the order of the operations.
    
    The operations you performed in this exercise are listed in the order in which they were performed. The Site data clip boundary was added first, and it affects the operations that follow it. The Corridor data clip boundary was added last, so it currently does not affect any other operations.
    
8.  Select the last Add Boundary operation in the list. Click ![](../images/GUID-197CFF1E-2B1A-483E-8A38-42185F2C36A1.png) to move the Add Boundary operation to the top of the list.
9.  Select the other Add Boundary operation. Click ![](../images/GUID-FF195884-3E3F-42E9-AE91-F932A22CC892.png) to move the Add Boundary operation to the bottom of the list.
10.  Click OK.
11.  In the Surface Properties - Rebuild Surface dialog box, click Rebuild the Surface.
     
     When the surface rebuilds, the points outside the orange Corridor data clip boundary are excluded from the surface.
     
     ![](../images/GUID-60C7437F-F396-47ED-89AE-0011CDAFF519.png)
     
     Surface with Corridor data clip boundary applied
     
     **Further exploration:** Rearrange the surface definition operations and observe the results. Before you continue to the next exercise, make sure that the Operation Type table is in the following order:
     
     *   Add Boundary: **Corridor**
     *   Import Point File
     *   Add Boundary: **Site**

To continue this tutorial, go to [Exercise 2: Simplifying a Surface](GUID-808DDFE2-424D-4B04-8A73-0AE3FF0F4B26.htm "In this exercise, you will reduce the number of points that are used to define a surface.").

**Parent topic:** [Tutorial: Working with Large Surfaces](GUID-750F4161-2F8F-47AA-AA39-8D5640884AE6.htm "This tutorial demonstrates several features that can help you manage large surfaces efficiently in Autodesk Civil 3D.")