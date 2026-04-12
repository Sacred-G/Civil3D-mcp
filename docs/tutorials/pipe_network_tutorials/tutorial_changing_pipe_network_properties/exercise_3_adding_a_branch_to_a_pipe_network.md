---
title: "Exercise 3: Adding a Branch to a Pipe Network"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-16A23062-0F4F-41BE-9ABE-8DABE9396FF0.htm"
category: "tutorial_changing_pipe_network_properties"
last_updated: "2026-03-17T18:43:07.136Z"
---

                 Exercise 3: Adding a Branch to a Pipe Network  

# Exercise 3: Adding a Branch to a Pipe Network

In this exercise, you will add a branch to the existing pipe network layout and use the part status to review and edit the layout.

In addition to indicator icons that indicate where parts can be connected, you will see icons when the pipe or structure you are adding to the layout will break an existing pipe. The pipes created by the break are automatically connected to the new structure or pipe.

This exercise continues from [Exercise 2: Changing the Surface, Alignment and Rules Configuration](GUID-6FB5AA3B-B869-4CC5-A21A-A48F48149506.htm "In this exercise, you will change the surface and alignment that are referenced by the pipe network parts. You will also examine the design rules for a part.").

Add a headwall structure to the pipe network

Note:

This exercise uses _Pipe Networks-2B.dwg_ with the modifications you made in the previous exercise.

2.  If the Network Layout Tools toolbar is not already open, select a pipe network part. Right-click. Click Edit Network.
3.  On the Network Layout Tools toolbar, ensure that the surface **EG** and the alignment **XC\_STORM** are selected.
    
    For more information, see [Exercise 2: Changing the Surface, Alignment and Rules Configuration](GUID-6FB5AA3B-B869-4CC5-A21A-A48F48149506.htm "In this exercise, you will change the surface and alignment that are referenced by the pipe network parts. You will also examine the design rules for a part.").
    
4.  In the Structures list, expand the Concrete Rectangular Headwall collection. Select 44 x 6 x 37 Inch Concrete Rectangular Headwall.
5.  Click ![](../images/GUID-FE7E01D7-EB48-48D1-9081-9BD81A3951F2.png)Structures Only.
6.  In the drawing window, zoom to chainage 4+00 on XC\_STORM.
7.  Click near the alignment to place the headwall structure.

Connect the headwall structure to the pipe network

1.  On the Network Layout Tools toolbar, click Pipes Only.
2.  In the Pipes list, expand the Concrete Pipe collection. Select 24 Inch Concrete Pipe.
3.  Toggle the Upslope/Downslope button to ![](../images/GUID-2AC098C0-F7D4-4B70-9434-D5E9A9396394.png) Upslope.
4.  Hover the cursor over the rectangular headwall structure. With the connection marker displayed, click to connect the pipe.
5.  Pan to the structure that is offset -15 feet from chainage 8+00 on ROAD1.
6.  Hover the cursor over the structure. With the connection marker displayed, click the structure to connect the pipe.
7.  Press Enter to end the command.

Rotate the headwall structure

1.  Pan to the headwall at the end of the branch. Click the headwall to select it.
2.  Use the circular editing grip to rotate the headwall until it is perpendicular to the attached pipe.
3.  Press Esc to deselect the headwall.

Validate that design rules have been met

1.  In Toolspace, on the Prospector tab, expand Pipe Networks![](../images/ac.menuaro.gif)Networks![](../images/ac.menuaro.gif)**Network - (1)**. Click Pipes.
2.  In the item view, in the row for **Pipe - (10)**, place your cursor over the circular icon in the Status cell to view the design rules that have not been met.
    
    The pipe exceeds the maximum length and maximum cover found in the rules. In the next few steps, you will correct the length by adding a structure to the middle of the pipe span.
    
3.  Right-click the row for **Pipe - (10)**. Click Zoom To.
    
    The full extents of the pipe are displayed in the drawing window.
    

Insert a structure in the middle of a pipe

1.  On the Network Layout Tools toolbar, in the Structures list, select Eccentric Cylindrical Structure![](../images/ac.menuaro.gif)Eccentric Structure 48 Dia 18 Frame 24 Cone 5 Wall 6 Floor.
2.  Click ![](../images/GUID-B68CC0FB-4913-4596-AB1E-8A301BBC4497.png)Structures Only.
3.  In the drawing window, pan to chainage 2+00 on alignment XC\_STORM.
    
    Note:
    
    You must turn off OSNAP to complete the following steps.
    
4.  Place your cursor over a location on the pipe that is close to chainage 2+00.
    
    A break pipe marker is displayed ![](../images/GUID-2BD909AF-4940-484A-AD42-A58B4E4BC6C8.png) to indicate that the pipe will be severed by placing the structure there.
    
    ![](../images/GUID-3B0E062D-8DE2-4172-9B0D-8169826F1A01.png)
    
5.  With the break pipe marker displayed, click to place the manhole at the location, and create two pipes from the one.
6.  Press Enter to end the command.

To continue to the next tutorial, go to [Viewing and Editing Pipe Networks](GUID-DCDFFC24-DBD6-4A45-9F9D-7EFEAB45123F.htm "This tutorial demonstrates how you can view and edit the parts of your pipe network in profile and section views.").

**Parent topic:** [Tutorial: Changing Pipe Network Properties](GUID-8995BDC3-EE99-4BC4-BC69-5987DFD9C2BF.htm "This tutorial demonstrates how to add parts to your pipe network parts list. You will also learn how to change the surface, alignment, and design rules that are referenced when you are laying out a pipe network.")