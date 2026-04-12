---
title: "Exercise 3: Adding Breaklines to a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-9CC1D50D-1542-4CD3-8136-D6E512D5C64E.htm"
category: "tutorial_creating_and_adding_data_to_a_surface"
last_updated: "2026-03-17T18:42:10.014Z"
---

                 Exercise 3: Adding Breaklines to a Surface  

# Exercise 3: Adding Breaklines to a Surface

In this exercise, you will cause the surface to triangulate along a linear feature.

Breaklines are used to define surface features and to force triangulation along the breakline. Surfaces do not triangulate across breaklines, creating more accurate TIN surface models.

In this exercise, you will create breaklines along the edge of pavement for an existing road. Breaking the surface along features produces a more accurate surface rendering.

![](../images/GUID-90B31A27-AD88-42A8-83ED-67A0786AAC96.png)

This exercise continues from [Exercise 2: Adding Point Data to a Surface](GUID-E6432405-A737-489B-97DC-2555D21FF183.htm "In this exercise, you will import point data from a text file into the current drawing.").

Display the source polylines and change the surface style

Note:

This exercise uses the drawing you created in the previous exercises, or you can open _Surface-1B.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer drop-down. Next to the **\_EG\_BREAKLINES** layer, click ![](../images/GUID-F908C832-FEB1-4F3B-AB32-BC8C776F3D5F.png).
    
    The 3D polylines that represent the edge of pavement (EP) of an existing road are displayed on the east side of the site.
    
    Note:
    
    The EP polylines were included in the drawing template you used in [Exercise 1: Creating a TIN Surface](GUID-34D24CED-7589-4E07-9AC1-E625834FD1D6.htm "In this exercise, you will create an empty TIN surface in a new drawing.").
    
3.  Select the surface. Right-click. Click Surface Properties.
4.  In the Surface Properties dialog box, on the Information tab, for Surface Style, select **Contours and Triangles**. Click OK.
    
    The surface now shows contours and triangles that illustrate the EG surface triangulation.
    
    ![](../images/GUID-800312A4-6477-4666-838C-D0AB6826AE85.png)
    

Create breaklines from the polylines

1.  In Toolspace, on the Prospector tab, expand the ![](../images/GUID-6BA9AC63-A03A-493C-8716-35AD405BF1FC.png)Surfaces![](../images/ac.menuaro.gif)**EG**![](../images/ac.menuaro.gif)![](../images/GUID-54475D2E-5092-48C0-B845-4B6AACAF05C7.png)Definition collections. Right-click ![](../images/GUID-052BDD5B-5D89-4852-8609-C43FF348D66B.png)Breaklines. Click Add.
2.  In the Add Breaklines dialog box, for Description, enter **Edge of pavement - existing road**. Use the default values for the other fields. Click OK.
3.  The Select Objects prompt becomes active. While in this command, use the Zoom and Pan commands to locate the two blue 3D polylines on the east side of the site.
    
    Zoom in close so you can see that the triangles cross over the polylines.
    
    ![](../images/GUID-D9724EE8-3FBE-4C3B-9099-49EE2C4CFA65.png)
    
4.  Select the polylines. Press Enter.
    
    The surface triangulation is modified. The edge of pavement breaklines are applied, and the TIN surface is adjusted along the breakline edges, modifying the surface triangulation.
    
    ![](../images/GUID-90B31A27-AD88-42A8-83ED-67A0786AAC96.png)
    
5.  Click View tab ![](../images/ac.menuaro.gif)Navigate 2D panel ![](../images/ac.menuaro.gif)Extents.
    
    The drawing window zooms to the extents of the surface. With the breakline data added, the layer that contained the source data for the breaklines can be frozen.
    
6.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer drop-down. Next to the **EG\_BREAKLINES** layer, click ![](../images/GUID-C47B73AA-EB38-4294-A125-79165A76F079.png).

**Further exploration:** Notice that, along some portions of the polylines, the surface triangulation incorrectly crosses the breakline. This happened because the surface contours also act as breaklines. The new breaklines are not added because the contours are already acting as breaklines, and the current surface setting does not allow more than one breakline to affect the surface at a given point. To override this behavior, you can perform any of the following tasks:

*   **Build the surface with contours and breaklines:** In the Surface Properties dialog box, on the Definition tab, expand the Build collection. Set Allow Crossing Breaklines to Yes, and then set Level to Use to Use Last Breakline Level at Intersection.
*   **Modify the surface:** Use the DeleteSurfacePoint command to delete surface points that are located exactly on the polylines.
*   **Modify the polylines:** Add a vertex to the polylines at each location where it crosses a surface contour.

To continue this tutorial, go to [Exercise 4: Adding an Outer Boundary to a Surface](GUID-5A13D6DC-8EEB-4CE3-8C6C-BFEB61EA0979.htm "In this exercise, you will create an outer surface boundary from a polyline.").

**Parent topic:** [Tutorial: Creating and Adding Data to a Surface](GUID-899731B5-0B6A-451E-9CF2-0DCF00FA9B64.htm "This tutorial demonstrates how to create a TIN surface, and then add contour, breakline, and boundary data to the surface.")