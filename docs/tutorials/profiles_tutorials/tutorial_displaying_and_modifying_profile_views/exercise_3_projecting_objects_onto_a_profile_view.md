---
title: "Exercise 3: Projecting Objects onto a Profile View"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-188BE199-D837-4B1C-849C-12C0E992370D.htm"
category: "tutorial_displaying_and_modifying_profile_views"
last_updated: "2026-03-17T18:42:31.846Z"
---

                  Exercise 3: Projecting Objects onto a Profile View  

# Exercise 3: Projecting Objects onto a Profile View

In this exercise, you will project multi-view blocks, COGO points, and 3D polylines from plan view onto a profile view.

You can project a variety of objects, such as AutoCAD points, solids, blocks, multi-view blocks, 3D polylines, COGO points, feature lines, and survey figures onto a profile view. The process you will use in this exercise can be applied to any of these objects.

Note:

Before you project an object into a profile view, make sure that the object has a defined level. Otherwise, the level may be zero, and the profile view will be expanded vertically to accommodate the zero level value.

![](../images/GUID-2D89FAB5-80ED-4C06-8FA2-0B47E69F5390.png)

Project multi-view blocks onto a profile view

1.  Open Profile-5C.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    Two viewports are displayed in this drawing. A plan view of COGO points that represent an existing road, and a proposed corridor is displayed in the left viewport. A profile view that contains existing ground and proposed ground profiles of the proposed road is displayed in the right viewport.
    
2.  In the left viewport, at chainage 0+025, select the multi-view block that represents a light post. Right-click. Click Select Similar.
    
    ![](../images/GUID-CC28921E-900F-457B-9316-0309B9066E34.png)
    
    All the light posts along the proposed road corridor are selected.
    
    When you insert a multi-view block into a drawing, it is created as a standard AutoCAD block. Before it can be projected onto a profile view, a multi-view block must be exploded from its original AutoCAD block form.
    
3.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Profile View drop-down ![](../images/ac.menuaro.gif)Project Objects To Profile View ![](../images/GUID-3F4FF2E9-B330-4BFB-89EB-2DA054FF9AC6.png) Find.
4.  In the right viewport, click the profile view grid.
5.  In the Project Objects to Profile View dialog box, click ![](../images/GUID-F96C62F3-280E-4986-9F77-040900D6C1AE.png)<Set All> in each column to specify the following parameters:
    *   Style: **Projection Without Exaggeration**
    *   Level Options: Surface![](../images/ac.menuaro.gif)**First Street Surface**
    *   Label Style: **Object Name Chainage And Level**
6.  Click OK.
    
    The light poles are displayed on the profile view.
    
    ![](../images/GUID-89A95A43-7091-4707-8D0C-FF4592CC3399.png)
    

Project COGO points and 3D polylines onto a profile view

1.  In the left viewport, select the three COGO points that are along the proposed road centerline.
    
    ![](../images/GUID-0BCBD411-49F6-4D96-912C-6C87BD1E03E6.png)
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Profile & Section Views panel ![](../images/ac.menuaro.gif)Profile View drop-down ![](../images/ac.menuaro.gif)Project Objects To Profile View ![](../images/GUID-3F4FF2E9-B330-4BFB-89EB-2DA054FF9AC6.png) Find.
3.  In the right viewport, click the profile view grid.
4.  In the Project Objects To Profile View dialog box, click ![](../images/GUID-8B243375-9F97-4BBA-9333-91D9267E95C0.png)Pick Objects.
5.  In the left viewport, zoom out and select each of the blue 3D polylines that represent the front of building footprints.
    
    ![](../images/GUID-16D7E109-60DA-44EA-AF50-A5C992979792.png)
    
6.  Press Enter.
7.  In the Project Objects To Profile View dialog box, in the ![](../images/GUID-FC65CCCC-40CC-4211-A290-56E6EA37F2D6.png)3D Polylines row, under Style, click ![](../images/GUID-F96C62F3-280E-4986-9F77-040900D6C1AE.png)<Set All>.
8.  In the Select Projection Style dialog box, select **Projection Without Exaggeration**.
    
    Leave the Level Options setting at ![](../images/GUID-56AA5F63-6085-4889-A966-A6CA48CDA3BD.png)Use Object. In this case, the appropriate level value is a property of the selected objects.
    
9.  Click OK twice.
    
    The COGO points and building 3D polylines are displayed in the profile view.
    
    ![](../images/GUID-2D89FAB5-80ED-4C06-8FA2-0B47E69F5390.png)
    

Edit the projected object level

1.  In the left viewport, select the 3D polyline in plot 101.
    
    ![](../images/GUID-1C9239AE-E486-4C00-8BE2-B83B3667416C.png)
    
2.  On the command line, enter **LIST**.
    
    In the AutoCAD Text Window, notice that the level values for the polyline vertices are approximately 38.
    
3.  Press Enter. Close the AutoCAD Text Window.
4.  In the right viewport, select the 3D polyline that crosses between chainages 0+060 and 0+080.
    
    When you select the 3D polyline in the profile view, notice that the 3D polyline in plan view is highlighted.
    
5.  Drag the left ![](../images/GUID-E9EABD3F-968A-477D-B830-5E2B970CBF95.png) grip down toward the bottom of the profile view.
    
    ![](../images/GUID-DC663B27-DC6A-41C3-BCFD-B3038BCF5618.png)
    
6.  Repeat Steps 1 and 2 to examine the new level value.
    
    When you grip edit a projected feature line or 3D polyline, the corresponding level of the source object is adjusted.
    
7.  Press Enter. Close the AutoCAD Text Window.

Modify the display of projected objects in profile view

1.  In the right viewport, select the profile view grid. Right-click. Click Profile View Properties.
    
    The Projections tab is displayed on the Profile View Properties dialog box. You use the controls on this tab to change the parameters that you used when you projected objects onto the profile view.
    
    Note:
    
    Like other Autodesk Civil 3D labels, label parameters are changed by selecting the desired label, and then using the Labels contextual tab on the ribbon.
    
2.  In the Profile View Properties dialog box, on the Projections tab, clear the ![](../images/GUID-FC65CCCC-40CC-4211-A290-56E6EA37F2D6.png)3D Polylines check box.
3.  Click Apply.
    
    The 3D polylines are removed from the profile view, and are cleared from the Profile View Properties dialog box.
    
4.  Click OK.

**Further exploration**: Examine the style settings that are available for projected objects. Projected object styles are located in Toolspace, on the Settings tab, in the General![](../images/ac.menuaro.gif)Multipurpose Styles![](../images/ac.menuaro.gif)Projection Styles collection. Label styles for projected objects are located in Toolspace, on the Settings tab, in the Profile View![](../images/ac.menuaro.gif)Label Styles![](../images/ac.menuaro.gif)Projection collection.

To continue this tutorial, go to [Exercise 4: Splitting a Profile View](GUID-569237B4-951A-4692-9522-88AAF25E027C.htm "In this exercise, you will split a profile view so that the full level range of a layout profile fits in a shorter profile view.").

**Parent topic:** [Tutorial: Displaying and Modifying Profile Views](GUID-0BF95BEA-BDFF-4B0A-A73C-6749A5FFD1C5.htm "This tutorial demonstrates how to change the appearance of profile views.")