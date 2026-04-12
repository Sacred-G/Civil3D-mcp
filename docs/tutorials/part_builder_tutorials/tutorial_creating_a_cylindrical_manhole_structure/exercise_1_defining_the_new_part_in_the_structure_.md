---
title: "Exercise 1: Defining the New Part in the Structure Catalog"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-B8A5FAA1-8946-457F-A02A-4DF5B1FC23F3.htm"
category: "tutorial_creating_a_cylindrical_manhole_structure"
last_updated: "2026-03-17T18:43:10.355Z"
---

                Exercise 1: Defining the New Part in the Structure Catalog  

# Exercise 1: Defining the New Part in the Structure Catalog

In this exercise, you will begin creating a cylindrical-shaped manhole structure in Part Builder by creating a new part chapter, and a new part family within the Structure catalog. You will also configure work planes in the Part Builder parametric modeling environment so that you can proceed with modeling the part in the subsequent exercises.

Because you will be working within the Part Builder environment, you do not need to have a drawing open to begin this exercise. However, the Autodesk Civil 3D [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79) includes a drawing that contains the completed part—in this case, a cylindrical manhole structure. If desired, before or after completing this exercise, you can open the Part Builder-1b.dwg file to see what this finished part looks like.

1.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/GUID-A7096E57-563B-43D0-B57A-FA69853A76A6.gif)Part Builder ![](../images/GUID-62BEBEF8-D435-4915-9305-B1BAA4A31516.png) Find.
2.  In the Getting Started – Catalog Screen dialog box, in the Part Catalog list, select Structure. Select the Junction Structure With Frames folder, and then click New Parametric Part. The New Part box dialog is displayed.
3.  For Name, enter “Cylindrical Manhole1.” Click in the box next to Description. The description matches the name by default. Click OK. The Part Builder parametric modeling environment is opened.
4.  Expand Part Configuration and change the following:
    
    *   Undefined Part Type: change this to Junction Structure
    *   Undefined: leave this as Undefined
    *   Undefined Bounded Shape: change this to Cylinder
    
    The Part is configured as a cylinder shape with the properties of a junction structure.
    
5.  Expand Modeling, and then right-click Work Planes and then click Add Work Plane. The Create Work Plane dialog box is displayed.
6.  Click Top, and then click OK. The Top work plane is created.
7.  Expand Work Planes. Right-click Top Plane![](../images/ac.menuaro.gif)Rename, and change the name to Cover.
    
    This work plane will be the cover level of the new structure.
    
8.  Click Save Part Family. Click Yes. The part is validated and saved.
    
    Note that additional parameters are displayed under the Model Parameters and Size Parameters to reflect the Part Configuration settings established in step 1.
    

To continue this tutorial, go to [Exercise 2: Defining the Manhole Geometry](GUID-91EEF3F1-D6AC-4001-92FC-65326059829F.htm "In this exercise, you will define the geometry of the manhole by creating a simple schematic of the structure profile. You will build this portion with dimensions that can be modified from within Autodesk Civil 3D when the part is in use.").

**Parent topic:** [Tutorial: Creating a Cylindrical Manhole Structure](GUID-3C390412-421F-4B68-A826-EA47FE49E7F9.htm "This tutorial demonstrates how to use Part Builder to create a cylindrical-shaped storm drainage manhole structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the manhole.")