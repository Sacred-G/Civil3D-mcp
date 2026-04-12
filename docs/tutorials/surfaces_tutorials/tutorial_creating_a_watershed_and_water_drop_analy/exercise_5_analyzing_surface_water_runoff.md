---
title: "Exercise 5: Analyzing Surface Water Runoff"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-0A52A24C-D6A9-4005-AF35-842A8309D167.htm"
category: "tutorial_creating_a_watershed_and_water_drop_analy"
last_updated: "2026-03-17T18:42:13.158Z"
---

                   Exercise 5: Analyzing Surface Water Runoff  

# Exercise 5: Analyzing Surface Water Runoff

In this exercise, you will create lines that illustrate the path that flowing water would take across a surface. Then, you will create a polygon that defines the catchment region and its area on the surface.

The water drop utility creates either a 2D or 3D polyline object that runs downhill on a surface from any point you select. You can specify whether the point you select is indicated with a marker. After the lines are created, you can edit or modify them as needed.

You can use the information you get from the water drop analysis to calculate catchment areas, based on specified low points of the surface. The catchment regions can be created on the surface as either 2D or 3D polygons, which can be exported to a hydrology application for detailed analysis.

This exercise uses a drawing file similar to the one used in [Exercise 3: Creating a Watershed Legend](GUID-47353C61-73BB-4EA6-B6BC-5AC7EB563868.htm "In this exercise, you will add a watershed legend table to the drawing."). The surface style is changed to make it easier to see the basic surface features, and it contains a simple point style for you to use as a start point marker.

This exercise continues from [Exercise 4: Extracting Objects from a Surface](GUID-6A799F9A-6095-4A18-91EB-DF61A92EE6C0.htm "In this exercise, you will use the watershed data to create non-destructive AutoCAD objects from the surface.").

Perform a water drop analysis

1.  Open _Surface-5C.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Analyze tab ![](../images/ac.menuaro.gif)Ground Data panel ![](../images/ac.menuaro.gif)Flow Paths drop-down![](../images/ac.menuaro.gif)Water Drop ![](../images/GUID-008A8E39-6AF3-42D2-809A-8B338242C408.png) Find.
3.  In the Water Drop dialog box, specify the following parameters.
    *   Path Layer: **C-TOPO-WDRP**
    *   Path Object Type: **2D Polyline**
    *   Place Marker At Start Point: **Yes**
    *   Start Point Marker Style: **WD Start**
4.  Click OK.
5.  In the drawing, click several locations near those displayed in the following illustration.
    
    A 2D polyline is drawn, representing the flow of water from the spot you selected. The start point of the path is indicated by a ![](../images/GUID-205604B4-CA9D-4039-AFFE-043721177544.png) marker.
    
    If a water path splits, additional lines are drawn to follow each path.
    
    ![](../images/GUID-37B4F1D2-C2FC-4330-A86E-31935F966BBE.png)
    
    Waterdrop paths (arrows indicate culvert inlet locations)
    
6.  Press Enter to end the waterdrop command.
    
    Notice that most waterdrop paths in this area drain to the culverts that are indicated by the red arrows. Next, you will use the water drop paths you just created to define a catchment area that influences the culverts.
    

Create catchment areas

1.  Click Analyze tab ![](../images/ac.menuaro.gif)Ground Data panel ![](../images/ac.menuaro.gif)Catchments drop-down![](../images/ac.menuaro.gif)Catchment Area ![](../images/GUID-6CC27D59-5898-491A-9B0F-FAB72E9B1F37.png) Find.
2.  In the Catchment dialog box, specify the following parameters:
    *   Display Discharge Point: **Yes**
    *   Discharge Point: **Catchment**
    *   Catchment Layer: **C-TOPO-CATCH**
    *   Catchment Object Type: **2D Polyline**
3.  Click OK.
4.  In the drawing, click the ![](../images/GUID-205604B4-CA9D-4039-AFFE-043721177544.png) marker for each waterdrop path.
    
    Blue polygons that define each catchment region are created. The area value of each catchment region is displayed on the command line. In the drawing, each catchment point is indicated by a ![](../images/GUID-1649E6D8-C5CD-48FF-8FFA-67454D488044.png) marker.
    
    Note:
    
    If you receive a message indicating that the specified location results in a catchment area with no area, it means that there is no flat area or high spot on the specified point.
    
5.  Press Enter to end the catchment area command.
    
    ![](../images/GUID-2367CE9A-384B-430A-8872-CD1414BB7739.png)
    
    Catchment areas defined
    

Combine multiple catchment areas

1.  In the drawing, select the polygons that define catchment areas. Right-click. Select Isolate Objects![](../images/ac.menuaro.gif)Isolate Selected Objects.
    
    The polygons are displayed in the drawing window, but all other objects are hidden.
    
2.  Select the polygons again.
3.  On the command line, enter **LineWorkShrinkWrap**.
    
    A polygon that contains the combined area of the catchment areas is displayed. The LineWorkShrinkWrap command creates a single outside boundary of a selection of touching polygons. The original polygons are not deleted.
    
    ![](../images/GUID-5F40466E-7E76-492B-99A5-371721080B0A.png)
    
    Combined catchment areas
    
4.  In the drawing, right-click. Click Isolate Objects![](../images/ac.menuaro.gif)End Object Isolation.
    
    The remaining drawing objects are displayed in the drawing window.
    
    Tip:
    
    You can transfer individual or shrinkwrapped catchment area polygons to a hydrology application for further analysis.
    

To continue to the next tutorial, go to [Generating Surface Volume Information](GUID-20DD4422-EC0D-4537-870F-96C62AD66790.htm "This tutorial demonstrates how to create a composite volume surface from a base surface and a comparison surface, and then perform composite volume calculations.").

**Parent topic:** [Tutorial: Creating a Watershed and Water Drop Analysis](GUID-23EC67A6-0084-4DAC-B58F-141155DB7E29.htm "This tutorial demonstrates how to create two kinds of surface analysis: watershed and water drop.")