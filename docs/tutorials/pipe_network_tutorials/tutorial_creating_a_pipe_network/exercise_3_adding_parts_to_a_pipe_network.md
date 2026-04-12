---
title: "Exercise 3: Adding Parts to a Pipe Network"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-592BB1C6-23FE-44B5-B230-3ED957ADC654.htm"
category: "tutorial_creating_a_pipe_network"
last_updated: "2026-03-17T18:43:05.937Z"
---

                 Exercise 3: Adding Parts to a Pipe Network  

# Exercise 3: Adding Parts to a Pipe Network

In this exercise, you will add to your pipe network by creating pipes and structures that connect to existing structures.

Whenever you are laying out a pipe network, you have the option of connecting to existing pipe network parts. Autodesk Civil 3D gives you visual cues when the pipe or structure you are creating will either connect to an existing object or break a pipe to create a junction.

In the previous exercise, you used the Draw Pipes And Structures tool to place structures and pipes simultaneously. In this exercise, you will add gullies using the Draw Structures Only tool, and then connect the gullies to network using the Draw Pipes Only tool.

This exercise continues from [Exercise 2: Creating a Pipe Network by Layout](GUID-E0BA1273-33B1-4939-9603-C750440B74DA.htm "In this exercise, you'll create a pipe network using the Autodesk Civil 3D pipe network layout tools. The pipe network is associated with a surface and alignment, and uses parts taken from a standard parts list.").

Note:

This exercise uses _Pipe Networks-1B.dwg_ with the modifications you made in the previous exercise, or you can open _Pipe Networks-1C.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

Add structures to the pipe network

1.  In the drawing, select a pipe network part. Right-click. Click Edit Network.
2.  In the Structures list, ensure that Eccentric Cylindrical Structure![](../images/ac.menuaro.gif)Eccentric Structure 48 Dia 18 Frame 24 Cone 5 Wall 6 Floor is selected.
3.  Toggle the Upslope/Downslope option to ![](../images/GUID-A92FF0D1-DD78-4019-9BD5-9803A1D01BB7.png) (downslope).
4.  Click ![](../images/GUID-FE7E01D7-EB48-48D1-9081-9BD81A3951F2.png)Structures Only.
5.  On the command line, enter **‘SO**.
6.  Click a label on the ROAD1 alignment to select it.
7.  Create a structure by entering **960** for the chainage and **15** as the offset.
8.  Repeat Step 7 to add structures that are offset 15 feet from stations 11+10, 12+60, and 13+10.
9.  Press Enter twice to exit the Chainage Offset and Add Structures commands.

Add pipes to the pipe network

1.  In the Pipes list, ensure that 18 Inch Concrete Pipe is selected.
2.  On the Network Layout Tools toolbar, click Pipes Only.
3.  Place the cursor over the structure that is offset 15 from chainage 9+60.
    
    A connection marker ![](../images/GUID-55CAB8E6-0ED9-43C2-B235-25D33A239844.png) that indicates that the pipe can be attached to the structure is displayed.
    
4.  With the connection marker displayed, click the structure to connect the new pipe to it.
5.  Place the cursor over the structure that is offset -15 feet from chainage 9+50. With the connection marker displayed, click the structure to connect the new pipe to it.
6.  On the command line, enter **S** to select a new start point.
7.  Repeat Steps 3 through 6 to add pipes between the structures that are offset 15 feet from stations 11+10, 12+60, and 13+10 and the main network.
8.  Press Enter twice to exit the Chainage Offset and Add Pipes commands.

To continue to the next tutorial, go to [Changing Pipe Network Properties](GUID-8995BDC3-EE99-4BC4-BC69-5987DFD9C2BF.htm "This tutorial demonstrates how to add parts to your pipe network parts list. You will also learn how to change the surface, alignment, and design rules that are referenced when you are laying out a pipe network.").

**Parent topic:** [Tutorial: Creating a Pipe Network](GUID-33CA24DC-1915-4095-9493-9F065B2CD50F.htm "This tutorial demonstrates how to create a pipe network using the specialized layout tools.")