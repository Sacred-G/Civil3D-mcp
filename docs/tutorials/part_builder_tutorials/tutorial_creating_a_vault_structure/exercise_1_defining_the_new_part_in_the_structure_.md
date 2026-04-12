---
title: "Exercise 1: Defining the New Part in the Structure Catalog"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6CD31559-126E-45CD-A44A-A57C1A61ABAE.htm"
category: "tutorial_creating_a_vault_structure"
last_updated: "2026-03-17T18:43:13.139Z"
---

                Exercise 1: Defining the New Part in the Structure Catalog  

# Exercise 1: Defining the New Part in the Structure Catalog

In this exercise, you will begin creating a vault structure in Part Builder by creating a new part chapter, and a new part family within the Structure catalog.

You will also configure work planes in the Part Builder parametric modeling environment so that you can proceed with modeling the part in the subsequent exercises.

Because you will work in the Part Builder environment, you do not need to have a drawing open to begin this exercise. However, in Exercise 5, Using the New Part, you will be instructed to open a drawing (Part Builder-3a.dwg) and use the part in a pipe network.

The Autodesk Civil 3D [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79) includes a drawing that contains the completed part—in this case, a vault manhole structure. If desired, before or after completing this exercise, you can open the Part Builder-3b.dwg file to see what this finished part looks like.

1.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel![](../images/GUID-A7096E57-563B-43D0-B57A-FA69853A76A6.gif)Part Builder ![](../images/GUID-62BEBEF8-D435-4915-9305-B1BAA4A31516.png) Find. The Getting Started - Catalog Screen dialog box is displayed.
2.  Click the US Imperial Structures Folder, then click New Chapter. Enter Custom for name and then click OK. A new Chapter is created for custom structures.
3.  Verify that the Custom Folder is selected, then click New Parametric Part. The New Part dialog box is displayed
4.  For Name enter "Vault 5106-LA with Top 5106TL3- 332". Click in the box next to Description. The description matches the name by default. Click OK. The Part Builder environment is opened.
5.  Expand Part Configuration and change the following:
    
    *   Undefined Part Type: change this to Junction Structure
    *   Undefined: change this to Rectangular Vault (do this by double-clicking and then entering “Rectangular Vault” into this field)
    *   Undefined Bounded Shape: change this to Box
    
    The Part is configured as a box shape with the properties of a junction structure.
    
6.  Expand Modeling. Right-click Work Planes, and then click Add Work Plane. The Create Work Plane dialog box is displayed.
7.  Click Top, and then click OK. The Top work plane is created.
8.  Expand Work Planes. Right-click Top Plane and then click Rename. Change the name to cover. This work plane will become the cover level of the new structure.
9.  Click Save Part Family. Click Yes. Stay in the Part Builder environment for the next exercise. The part is validated and saved.

To continue this tutorial, go to [Exercise 2: Defining the Vault Top Section Geometry](GUID-5013FCFB-AD45-4399-9926-19040957C197.htm "In this exercise, you will build the top portion of the vault. This is a rectangular frame with a rectangular opening. You will build this portion with dimensions that can be modified from within Autodesk Civil 3D when the part is in use.").

**Parent topic:** [Tutorial: Creating a Vault Structure](GUID-5417170D-9D08-4B57-83A6-FE84173720FA.htm "This tutorial demonstrates how to use Part Builder to create a vault structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the vault.")