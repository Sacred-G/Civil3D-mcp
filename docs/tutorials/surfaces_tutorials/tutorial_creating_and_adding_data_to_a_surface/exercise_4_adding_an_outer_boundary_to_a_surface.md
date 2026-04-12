---
title: "Exercise 4: Adding an Outer Boundary to a Surface"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-5A13D6DC-8EEB-4CE3-8C6C-BFEB61EA0979.htm"
category: "tutorial_creating_and_adding_data_to_a_surface"
last_updated: "2026-03-17T18:42:10.053Z"
---

                   Exercise 4: Adding an Outer Boundary to a Surface  

# Exercise 4: Adding an Outer Boundary to a Surface

In this exercise, you will create an outer surface boundary from a polyline.

![](../images/GUID-3F1C959E-80C3-48DC-BEF6-6AF7F8AE15B6.png)

This exercise continues from [Exercise 3: Adding Breaklines to a Surface](GUID-9CC1D50D-1542-4CD3-8136-D6E512D5C64E.htm "In this exercise, you will cause the surface to triangulate along a linear feature.").

Create an outer boundary from a polyline

Note:

This exercise uses _Surface-1B.dwg_ with the modifications you made in the previous exercise.

2.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer drop-down. Next to the **\_EG-BNDY** layer, click ![](../images/GUID-F908C832-FEB1-4F3B-AB32-BC8C776F3D5F.png). Click in the drawing to exit the Layer Control list.
    
    A blue polyline, which represents the extents of the site, is displayed. This polyline was imported with the original surface contours.
    
3.  In Toolspace, on the Prospector tab, expand the ![](../images/GUID-6BA9AC63-A03A-493C-8716-35AD405BF1FC.png)Surfaces![](../images/ac.menuaro.gif)![](../images/GUID-6BA9AC63-A03A-493C-8716-35AD405BF1FC.png)**EG**![](../images/ac.menuaro.gif)![](../images/GUID-54475D2E-5092-48C0-B845-4B6AACAF05C7.png)Definition collections. Right-click ![](../images/GUID-D08C4427-85F9-403D-8CA7-F2F29952B84C.png)Boundaries. Click Add.
4.  In the Add Boundaries dialog box, specify the following parameters:
    *   Name: **EG - Outer**
    *   Type: **Outer**
    *   Non-Destructive Breakline: **Cleared**
    *   Mid-Ordinate Distance: **1.000**
5.  Click OK.
6.  Select the blue polyline.
    
    ![](../images/GUID-650DDA12-24A1-4D9E-830C-908593A5468B.png)
    
    The boundary is added to the surface definition, and the surface display in the drawing is clipped to the area that is defined by the new outer boundary.
    

Hide the polyline and change the surface style

1.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer drop-down. Next to the **\_EG-BNDY** layer, click ![](../images/GUID-C47B73AA-EB38-4294-A125-79165A76F079.png).
2.  Select the surface. Right-click. Click Surface Properties.
3.  In the Surface Properties dialog box, on the Information tab, for Surface Style, select **Contours 5' and 25' (Background)**. Click OK.
    
    In the selected surface style, contours are displayed in muted colors at broad intervals. This display allows the major surface features to remain visible while you focus on other aspects of the site design.
    
    ![](../images/GUID-D669C459-C8A9-4D4C-90EE-8841C6D5A65C.png)
    

To continue to the next tutorial, go to [Working with Large Surfaces](GUID-750F4161-2F8F-47AA-AA39-8D5640884AE6.htm "This tutorial demonstrates several features that can help you manage large surfaces efficiently in Autodesk Civil 3D.").

**Parent topic:** [Tutorial: Creating and Adding Data to a Surface](GUID-899731B5-0B6A-451E-9CF2-0DCF00FA9B64.htm "This tutorial demonstrates how to create a TIN surface, and then add contour, breakline, and boundary data to the surface.")