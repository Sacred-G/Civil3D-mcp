---
title: "Exercise 2: Changing the Surface, Alignment and Rules Configuration"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-6FB5AA3B-B869-4CC5-A21A-A48F48149506.htm"
category: "tutorial_changing_pipe_network_properties"
last_updated: "2026-03-17T18:43:06.953Z"
---

                 Exercise 2: Changing the Surface, Alignment and Rules Configuration  

# Exercise 2: Changing the Surface, Alignment and Rules Configuration

In this exercise, you will change the surface and alignment that are referenced by the pipe network parts. You will also examine the design rules for a part.

Autodesk Civil 3D uses the referenced surface, alignment, and rules to determine the size and placement of pipe network parts. For example, if you create a manhole structure, the top cover of the structure is typically automatically placed at the level of the referenced surface. If the design rules for the manhole specify an adjustment value for cover of the structure, the cover is placed at the surface level plus or minus the adjustment value.

This exercise continues from [Exercise 1: Adding Parts to the Parts List](GUID-089176F6-E1AC-41F4-96CD-B2AEABB5176C.htm "In this exercise, you will add a new part to the parts list by selecting a part family and size from the pipe network part catalog.").

Change the referenced surface

Note:

This exercise uses _Pipe Networks-2.dwg_ with the modifications you made in the previous exercise, or you can open _Pipe Networks-2B.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  If the Network Layout Tools toolbar is not already open, select a pipe network part. Right-click. Click Edit Network.
3.  On the Network Layout Tools toolbar, click ![](../images/GUID-6BA9AC63-A03A-493C-8716-35AD405BF1FC.png).
4.  In the Select Surface dialog box, select EG. Click OK.

Change the referenced alignment

1.  Click ![](../images/GUID-A1AF1DE4-7370-4208-8B4C-ABB3CB562C79.png).
2.  In the Select Alignment dialog box, select **XC\_STORM**.
3.  Click OK.

Change the rule set

1.  Click ![](../images/GUID-13DB04D5-4FA6-46FD-9D37-EC7FBF23A3A9.png).
2.  In the Select Parts List dialog box, click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection.
3.  In the Network Parts List dialog box, on the Structures tab, expand Eccentric Cylindrical Structure. Click Eccentric Structure 48 Dia 18 Frame 24 Cone 5 Wall 6 Floor.
4.  In the Rules cell, click ![](../images/GUID-CDB8152D-2D37-45E6-9F60-E45848AE09C7.png).
5.  In the Structure Rule Set dialog box, click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection.
6.  In the Structure Rule Set dialog box, click the Rules tab.
    
    The selected design rules specify that the structure has a maximum drop value of 3.000’ and maximum pipe diameter or width of 4.000’. You can modify these values, or click Add Rule to add another rule.
    
7.  Click Cancel twice.
8.  On the Pipes tab, expand Standard then Concrete Pipe. Click 24 Inch Concrete Pipe.
9.  In the Rules cell, click ![](../images/GUID-1EA72B11-E339-4969-96D4-13558FAA7D6B.png). In the Pipe Rules Set dialog box, click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection. In the Pipe Rule Set dialog box, click the Rules tab.
    
    The selected design rules allow pipes of a maximum length of 200 feet, which is typical of a pipe layout that travels through open terrain.
    
10.  Click OK four times.

To continue this tutorial, go to [Exercise 3: Adding a Branch to a Pipe Network](GUID-16A23062-0F4F-41BE-9ABE-8DABE9396FF0.htm "In this exercise, you will add a branch to the existing pipe network layout and use the part status to review and edit the layout.").

**Parent topic:** [Tutorial: Changing Pipe Network Properties](GUID-8995BDC3-EE99-4BC4-BC69-5987DFD9C2BF.htm "This tutorial demonstrates how to add parts to your pipe network parts list. You will also learn how to change the surface, alignment, and design rules that are referenced when you are laying out a pipe network.")