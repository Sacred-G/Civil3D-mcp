---
title: "Exercise 1: Adding Parts to the Parts List"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-089176F6-E1AC-41F4-96CD-B2AEABB5176C.htm"
category: "tutorial_changing_pipe_network_properties"
last_updated: "2026-03-17T18:43:06.803Z"
---

                  Exercise 1: Adding Parts to the Parts List  

# Exercise 1: Adding Parts to the Parts List

In this exercise, you will add a new part to the parts list by selecting a part family and size from the pipe network part catalog.

This exercise demonstrates how to access your parts lists from the Network Layout Tools toolbar. You can also create, view, and edit parts lists using the Toolspace Settings tab.

Add a part family to the pipe network parts list

1.  Open _Pipe Networks-2.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the drawing window, click a pipe network part.
3.  Click Pipe Networks tab![](../images/ac.menuaro.gif)Modify panel![](../images/ac.menuaro.gif)Edit Pipe Network ![](../images/GUID-0EECE359-28D4-4653-A093-3BB525DF413E.png) Find.
4.  On the Network Layout Tools toolbar, click ![](../images/GUID-13DB04D5-4FA6-46FD-9D37-EC7FBF23A3A9.png).
5.  In the Select Parts List dialog box, click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection.
6.  In the Network Parts List dialog box, on the Structures tab, right-click the parts list name in the tree view. Click Add Part Family.
7.  In the Part Catalog dialog box, under Inlet-Outlets, select Concrete Rectangular Headwall. Click OK.
    
    The new part family is added to the tree view.
    
8.  On the Structures tab, in the tree view, notice that no part sizes are available in the Concrete Rectangular Headwall family. You will add them in the following steps.
9.  Right-click Concrete Rectangular Headwall, and click Add Part Size.
10.  In the Part Size Creator dialog box, click the Headwall Base Width row. In the Add All Sizes cell, select the check box. Click OK.
11.  Expand Concrete Rectangular Headwall.
     
     Notice that all available part sizes were added to the tree view. Notice that for each part in the list you can select an object style, design rules, and a render material.
     
12.  On the Pipes tab, in the tree view, right-click Concrete Pipe. Click Add Part Size.
13.  In the Part Size Creator dialog box, click the Inner Pipe Diameter row. Click the Value cell. From the value list, select **24.000000**. Click OK.
     
     The new part size is added to the tree view.
     
14.  Click OK to close the Network Parts List dialog box.
15.  Click OK to close the Select Parts List dialog box.

To continue this tutorial, go to [Exercise 2: Changing the Surface, Alignment and Rules Configuration](GUID-6FB5AA3B-B869-4CC5-A21A-A48F48149506.htm "In this exercise, you will change the surface and alignment that are referenced by the pipe network parts. You will also examine the design rules for a part.").

**Parent topic:** [Tutorial: Changing Pipe Network Properties](GUID-8995BDC3-EE99-4BC4-BC69-5987DFD9C2BF.htm "This tutorial demonstrates how to add parts to your pipe network parts list. You will also learn how to change the surface, alignment, and design rules that are referenced when you are laying out a pipe network.")