---
title: "Exercise 2: Creating a Pipe Network by Layout"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E0BA1273-33B1-4939-9603-C750440B74DA.htm"
category: "tutorial_creating_a_pipe_network"
last_updated: "2026-03-17T18:43:05.787Z"
---

                 Exercise 2: Creating a Pipe Network by Layout  

# Exercise 2: Creating a Pipe Network by Layout

In this exercise, you'll create a pipe network using the Autodesk Civil 3D pipe network layout tools. The pipe network is associated with a surface and alignment, and uses parts taken from a standard parts list.

This exercise continues from [Exercise 1: Creating a Pipe Network from a Polyline](GUID-71E14F2E-4F72-45B2-A18D-106D3E18FC0D.htm "In this exercise, you will create a pipe network from an existing polyline. In this method of creating a pipe network, you use standard AutoCAD drawing commands to create a polyline, and then automatically place a pipe endpoint and structure at each polyline vertex.").

Specify pipe network creation parameters

1.  Open _Pipe Networks-1B.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains an existing ground surface, an alignment, and existing ground and layout profiles for the alignment. It also contains a surface exported from a corridor that uses the alignment as its baseline.
    
2.  In the drawing window, zoom to the area on the alignment between chainage 7+00 and 11+00.
3.  In Toolspace, on the Prospector tab, expand the Pipe Networks collection, then right-click Networks. Click Create Pipe Network By Layout.
    
    Alternatively, you can click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Pipe Network drop-down ![](../images/ac.menuaro.gif)Pipe Network Creation Tools ![](../images/GUID-45175DF0-0670-4DC5-92E9-6E7D56CCDC47.png) Find.
    
4.  To select the surface taken from the corridor, in the Create Pipe Network dialog box, in the Surface Name list, select **ROAD1\_SURF**.
5.  In the Alignment Name list, select **ROAD1**.
6.  Click OK.
    
    The new pipe network is added to the Toolspace Prospector tab, Pipe Networks![](../images/ac.menuaro.gif)Networks collection, and the Network Layout Tools toolbar is displayed. The network currently is empty. You will add parts to the network in the following steps.
    

Draw contiguous pipes and structures

1.  On the Network Layout Tools toolbar, in the Structure List, expand Eccentric Cylindrical Structure. Select Eccentric Structure 48 Dia 18 Frame 24 Cone 5 Wall 6 Floor.
2.  In the Pipes List, expand Concrete Pipe. Select 18 Inch Concrete Pipe.
3.  Ensure that ![](../images/GUID-98319731-5225-4441-8634-18457D8DAD6B.png)Pipes and Structures is selected.
4.  Ensure the Upslope/Downslope option is set to ![](../images/GUID-A92FF0D1-DD78-4019-9BD5-9803A1D01BB7.png) (downslope).
5.  On the command line, enter **‘SO** to activate the Chainage Offset Civil Transparent command.
6.  In the drawing window, click one of the alignment chainage labels to select the alignment ROAD1.
7.  On the command line, enter **700** as the chainage.
8.  On the command line, enter **\-15** as the offset.
    
    A gully is placed at the specified point. The offset is designed to position the gully so that its outside edge is flush with the outside edge of the road shoulder.
    
9.  With the Chainage Offset command still active, create another structure by entering **800** for the chainage and **\-15** as the offset.
    
    A second gully is created. The two structures are connected by a pipe of the type specified in the Pipe list. The pipes follow a downhill gradient based on the corridor surface terrain and the design rules for the type and size of pipe. Later, you will view the vertical placement of the pipes you created in a profile view.
    
10.  To change the direction of the vertical pipe network layout, toggle the ![](../images/GUID-A92FF0D1-DD78-4019-9BD5-9803A1D01BB7.png)Upslope/Downslope button to ![](../images/GUID-2AC098C0-F7D4-4B70-9434-D5E9A9396394.png).
11.  With the Chainage Offset command still active, create additional structures with an offset of -15 at stations 9+50, 11+00, and 12+50.
     
     As you place the gullies, connecting pipes are created with gradient values specified by the design rules and the Upslope/Downslope setting.
     
12.  Press Enter to end the Chainage Offset command.

Draw a curved pipe with a structure

1.  Pan until you can see the segment of the alignment between chainage 12+50 and Chainage 13+00.
2.  With the drawing command still active, on the command line, enter **C** to begin creating a curved pipe.
3.  On the command line, enter **‘SO**.
4.  Create a structure at the end of the curved pipe by entering **1300** for the chainage and **\-15** as the offset.
5.  Press Enter to end the Chainage Offset command.
6.  Press Enter to end the drawing command.

To continue this tutorial, go to [Exercise 3: Adding Parts to a Pipe Network](GUID-592BB1C6-23FE-44B5-B230-3ED957ADC654.htm "In this exercise, you will add to your pipe network by creating pipes and structures that connect to existing structures.").

**Parent topic:** [Tutorial: Creating a Pipe Network](GUID-33CA24DC-1915-4095-9493-9F065B2CD50F.htm "This tutorial demonstrates how to create a pipe network using the specialized layout tools.")