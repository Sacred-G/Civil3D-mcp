---
title: "Exercise 1: Defining the New Part in the Structure Catalog"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-C76EC023-30AB-4A5D-B5E9-BFE169997E6C.htm"
category: "tutorial_creating_a_drop_inlet_manhole_structure"
last_updated: "2026-03-17T18:43:11.768Z"
---

                Exercise 1: Defining the New Part in the Structure Catalog  

# Exercise 1: Defining the New Part in the Structure Catalog

In this exercise, you will begin creating a drop inlet manhole structure in Part Builder by creating a new part chapter, and a new part family within the Structure catalog. You will also configure work planes in the Part Builder parametric modeling environment so that you can proceed with modeling the part in the later exercises.

This exercise uses a different process than the process that was used in the previous tutorial.

Because you will work in the Part Builder environment, you do not need to have a drawing open to begin this exercise. However, the Autodesk Civil 3D[tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79) includes a drawing that contains the completed part—in this case, a manhole structure with a drop inlet. If desired, before or after completing this exercise, you can open the Part Builder-2.dwg file to see what this finished part looks like.

1.  Click Home tab ![](../images/ac.menuaro.gif)Create Design![](../images/ac.menuaro.gif) panel ![](../images/GUID-A7096E57-563B-43D0-B57A-FA69853A76A6.gif)![](../images/ac.menuaro.gif)Part Builder ![](../images/GUID-62BEBEF8-D435-4915-9305-B1BAA4A31516.png) Find.
2.  In the Getting Started – Catalog Screen dialog box, in the Part Catalog list, select Structure.
3.  Click the US Imperial Structure Catalog folder, and then click New Chapter. Enter Custom for name, and then click OK.
    
    A new Chapter is created for custom structures.
    
4.  Select the Custom folder, and then click New Parametric Part.
    
    The New Part dialog box is displayed.
    
5.  For Name, enter “NO 233a”. Click in the Description field, and add “Outside Drop Connection” to the default Description, and then click OK.
    
    The Part Builder parametric modeling environment is opened.
    
6.  Expand Part Configuration and change the following:
    
    *   Undefined Part Type: change this to Junction Structure
    *   Undefined: change this to Manhole (do this by double-clicking and then entering “Manhole” into this field)
    *   Undefined Bounded Shape: change this to Cylinder
    
    The Part is configured as a cylinder shape with the properties of a junction structure.
    
7.  Expand Modeling, right-click Work Planes, and then click Add Work Plane.
    
    The Create Work Plane dialog box is displayed.
    
8.  Click Top, enter “cover” for Name, and then click OK.
    
    A top work plane is created which represents the cover level of the structure.
    
9.  Expand Work Planes, right-click Cover![](../images/ac.menuaro.gif)Add Geometry![](../images/ac.menuaro.gif)Point.
    
    You are prompted to pick a point.
    
10.  Click a point near the center of the yellow rectangle, and then press ESC.
     
     A point is created on the cover work plane near the center. This is a reference point to begin the construction of the part.
     
11.  Right-click Work Planes and then click Add Work Plane.
     
     The Create Work Plane dialog box is displayed.
     
12.  Click Right, enter “Vertical Axis” for Name, and then click OK.
     
     The Right side Vertical Axis work plane is created.
     
13.  Click Save Part Family. Click Yes.
     
     Stay in the Part Builder environment for the next exercise. The part is validated and saved. Depending on the part type, and on the Bounding Shape selected, certain Model Parameters and Size Parameters are automatically added to the part definition.
     

To continue this tutorial, go to [Exercise 2: Defining the Manhole Geometry](GUID-55B8971D-EDB7-42A1-A5A1-5E23B9F25D18.htm "In this exercise, you will define the geometry of the drop inlet manhole by creating a simple schematic of the structure profile. You will build this portion with dimensions that can be modified from within Autodesk Civil 3D when the part is in use.").

**Parent topic:** [Tutorial: Creating a Drop Inlet Manhole Structure](GUID-5EE5F312-EAA0-45EF-AC5C-A8586C9D2F3E.htm "This tutorial demonstrates how to use Part Builder to create a drop inlet manhole structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the manhole.")