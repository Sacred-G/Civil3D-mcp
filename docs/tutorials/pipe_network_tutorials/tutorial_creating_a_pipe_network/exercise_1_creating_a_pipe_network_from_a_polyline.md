---
title: "Exercise 1: Creating a Pipe Network from a Polyline"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-71E14F2E-4F72-45B2-A18D-106D3E18FC0D.htm"
category: "tutorial_creating_a_pipe_network"
last_updated: "2026-03-17T18:43:05.658Z"
---

                 Exercise 1: Creating a Pipe Network from a Polyline  

# Exercise 1: Creating a Pipe Network from a Polyline

In this exercise, you will create a pipe network from an existing polyline. In this method of creating a pipe network, you use standard AutoCAD drawing commands to create a polyline, and then automatically place a pipe endpoint and structure at each polyline vertex.

You can create a pipe network from a variety of elements, including 2D and 3D polylines, AutoCAD lines and arcs, and feature lines. In this exercise, you will use an existing 2D polyline.

Create a pipe network from a 2D polyline

1.  Open drawing _Pipe Networks-1A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains existing ground and corridor surfaces, alignments that represent intersecting road centerlines, plots that represent property boundaries, and a polyline that represents the proposed pipe network layout. In the following steps, you'll create a Autodesk Civil 3D pipe network from the polyline.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Pipe Network drop-down ![](../images/ac.menuaro.gif)Create Pipe Network From Object ![](../images/GUID-74AE4FF7-8EC2-4043-B68E-476BE9E4655A.png) Find.
3.  Click the left end of the blue polyline that is near the center of the road.
    
    The end that you click specifies the beginning of the pipe network. The network will flow away from this end.
    
    ![](../images/GUID-9F76443E-538E-4C23-88A5-48563A16BEF7.png)
    
4.  Press Enter to accept the flow direction.
5.  In the Create Pipe Network From Object dialog box, specify the following parameters:
    *   Network Name: **Storm Sewer Network**
    *   Network Parts List: **Storm Sewer**
    *   Pipe To Create: **450 mm RCP**
    *   Structure To Create: **Eccentric Structure 1,500 dia 530 Frame 900 Cone**
    *   Surface Name: **First Street**
    *   Alignment Name: **First Street**
    *   Erase Existing Element: **Selected**
6.  Click OK.
    
    The pipe network is displayed in plan. A structure was created at each polyline vertex, and a pipe was created between the structures.
    
    ![](../images/GUID-A332A760-2F8B-4283-B44B-829AA11EE262.png)
    

View the pipe network in profile

1.  Select a pipe and a structure.
2.  Right-click. Click Select Similar.
3.  Right-click. Click Draw Parts In Profile View.
4.  Click the First Street Profile view.
    
    The pipes and structures are displayed in the profile view. Notice that as you specified, the direction of flow begins at the end chainage of the profile, and proceeds toward the beginning chainage.
    
    ![](../images/GUID-7677E80D-4668-4E37-863F-7CC92E15D638.png)
    
5.  Press Esc.

Grip edit a network part

1.  Select the pipe on the far right side of the profile view.
2.  Click the ![](../images/GUID-CEED3835-C1DC-4492-9491-AFF56E9A9BB9.png) grip. Drag the grip up to increase the invert level. Click to place the grip.
    
    You can use grips to graphically change the position of pipes and structures in both plan and profile.
    
    ![](../images/GUID-8E41BC58-635B-440F-8BF6-EA8A351ED9CA.png)
    

Edit network parameters

1.  Right-click. Click Edit Network.
2.  In the Network Layout Tools toolbar, click the arrow next to ![](../images/GUID-98319731-5225-4441-8634-18457D8DAD6B.png).
    
    These tools enable you to add pipes or structures to the network using the parameters you set on this toolbar.
    
3.  Click ![](../images/GUID-7BDFFCF6-1694-4995-BDDE-54A6CE78E557.png)Pipe Network Vistas.
    
    On the Panorama window, you use the Pipes and Structures tabs to edit pipes parametrically.
    
4.  On the Pipes tab, in the **Pipe - (1)** row, change the Start Invert Level value to **40**.
5.  On the Network Layout Tools toolbar, click ![](../images/GUID-7BDFFCF6-1694-4995-BDDE-54A6CE78E557.png)Pipe Network Vistas.
    
    The pipe invert level changes to the specified value.
    

To continue this tutorial, go to [Exercise 2: Creating a Pipe Network by Layout](GUID-E0BA1273-33B1-4939-9603-C750440B74DA.htm "In this exercise, you'll create a pipe network using the Autodesk Civil 3D pipe network layout tools. The pipe network is associated with a surface and alignment, and uses parts taken from a standard parts list.").

**Parent topic:** [Tutorial: Creating a Pipe Network](GUID-33CA24DC-1915-4095-9493-9F065B2CD50F.htm "This tutorial demonstrates how to create a pipe network using the specialized layout tools.")