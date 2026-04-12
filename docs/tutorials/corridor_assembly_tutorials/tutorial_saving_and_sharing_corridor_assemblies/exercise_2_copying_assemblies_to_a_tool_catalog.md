---
title: "Exercise 2: Copying Assemblies to a Tool Catalog"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-A3124920-12F6-4405-85CC-3EC19B7BFEE4.htm"
category: "tutorial_saving_and_sharing_corridor_assemblies"
last_updated: "2026-03-17T18:42:47.700Z"
---

                  Exercise 2: Copying Assemblies to a Tool Catalog  

# Exercise 2: Copying Assemblies to a Tool Catalog

In this exercise, you will create a tool palette within a new tool catalog, and then add assemblies to it. After assemblies have been added to a tool catalog, you can share the tool catalog with other users.

Tip:

You can also use this procedure to copy assemblies from the drawing directly into a tool catalog.

This exercise continues from [Exercise 1: Saving Assemblies to a Tool Palette](GUID-CA79AC98-241A-49EB-89D3-2BF16DC0F553.htm "In this exercise, you will create a tool palette, and then save the assemblies that are included in the sample drawing.").

Create a tool catalog

1.  Click Home tab ![](../images/ac.menuaro.gif)Palettes panel drop-down ![](../images/ac.menuaro.gif)Content Browser ![](../images/GUID-F8E7639B-67EA-417E-8B00-9B6E667BB1A2.png) Find.
2.  In the Autodesk Content Browser 2025 window, click ![](../images/GUID-19146196-3324-48F9-A385-91BAAA8F2191.png).
3.  In the Add Catalog dialog box, select Create A New Catalog.
4.  Replace the New Catalog text with **Residential Assemblies (Tutorial)**.
5.  Click Browse.
6.  In the Browse For Folder dialog box, examine the file path.
    
    This is the default location where custom tool catalogs are saved. You will accept the default location for this tutorial.
    
7.  Click OK twice.
8.  In the Autodesk Content Browser 2025 window, right-click the **Residential Assemblies (Tutorial)** catalog. Click Properties.
9.  In the Catalog Properties dialog box, double-click the Image rectangle.
    
    ![](../images/GUID-C2F5020C-CE96-4923-90E4-1AF31F62217C.png)
    
10.  In the Select Image File dialog box, navigate to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC). Select _Assembly\_catalog\_image.png_. Click **Open**.
11.  In the Catalog Properties dialog box, click OK.
     
     The image you selected is displayed in the Autodesk Content Browser 2025 window. Assigning an image to a tool catalog can make it easy to identify the tool catalog contents.
     
     ![](../images/GUID-45932E19-1EE9-4942-8CEB-CA02BBB4C564.png)
     

Create a tool palette in a tool catalog

1.  In the Autodesk Content Browser 2025 window, click the **Residential Assemblies (Tutorial)** tool catalog.
    
    The empty Residential Assemblies (Tutorial) tool catalog opens.
    
    Tip:
    
    To ensure that the Content Browser remains visible over the Autodesk Civil 3D window, right-click the Autodesk Content Browser 2025 title bar and click Always On Top.
    
2.  In the Autodesk Content Browser 2025 window, click ![](../images/GUID-158DC81B-7328-4374-B6F2-A3E8D221003B.png).
3.  In the Tool Palette Properties dialog box, specify the following parameters:
    *   Name: **50-ft Highway Boundary Assemblies**
    *   Description: **Corridor assemblies for residential subdivisions that require a 50-ft highway boundary**
4.  Click OK.

Add assemblies to a tool palette in a tool catalog

1.  In the Autodesk Content Browser 2025 window, double-click the **50-ft ROW Assemblies** tool palette.
2.  In the Autodesk Civil 3D window, click the **Tutorial Assemblies** tool palette. Press Ctrl+A to select all the assemblies on the palette.
3.  Drag the assemblies into the **50-ft Highway Boundary Assemblies** tool palette in the **Residential Assemblies (Tutorial)** tool catalog.

To continue this tutorial, go to [Exercise 3: Publishing a Tool Catalog](GUID-C952A48E-0A35-4E5F-A1D4-01C14927CF75.htm "In this exercise, you will publish an assembly tool catalog so that it can be shared with other users.").

**Parent topic:** [Tutorial: Saving and Sharing Corridor Assemblies](GUID-A7837167-9635-4291-8624-D4E263D02BB6.htm "This tutorial demonstrates how to use Autodesk Civil 3D to save commonly used corridor assemblies, and then share them with other users.")