---
title: "Exercise 2: Balancing Mass Haul Volumes"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-A23CD8A8-7C49-489F-91FD-439CAD91B4FB.htm"
category: "tutorial_working_with_mass_haul_diagrams"
last_updated: "2026-03-17T18:43:02.199Z"
---

                      Exercise 2: Balancing Mass Haul Volumes  

# Exercise 2: Balancing Mass Haul Volumes

In this exercise, you will balance the mass haul volumes above and below the balance line, which will eliminate overhaul volume.

This exercise continues from [Exercise 1: Creating a Mass Haul Diagram](GUID-91F4B5CD-4CF4-4B43-8E2D-B28F941BCE7C.htm "In this exercise, you will create a mass haul diagram that displays free haul and overhaul volumes for a project site.").

Balance cut material volumes

Note:

This exercise uses _Mass Haul-1.dwg_ with the modifications you made in the previous exercise, or you can open _Mass Haul-2.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Zoom in on the mass haul region that is on the left-hand side of the diagram.
    
    Notice that the highest point of the green free haul area is at chainage 3+25. This is the gradient point, which is the point at which free haul transitions from cut to fill. Depending on the site conditions, gradient points can be logical locations for dump sites or borrow pits, which can reduce or eliminate overhaul.
    
3.  At chainage 3+25, hover the cursor over the mass haul line.
    
    Notice that the tooltip displays the current chainage number (3+25) and volume (approximately 1500.00 Cu. Yd.).
    
    ![](../images/GUID-081BD565-01C0-42DE-B385-7A8A0778381F.png)
    
4.  Select the mass haul line. Right-click. Click Mass Haul Line Properties.
5.  In the Mass Haul Line Properties dialog box, click the Balancing Options tab.
6.  In the Add/Remove Borrow Pits And Dump Sites area, click Add Dump Site.
7.  In the Chainage cell, enter **325**.
    
    This is the number of the chainage at the gradient point for the volume above the balance line.
    
8.  In the Capacity cell, enter **1500**.
    
    This is the approximate volume (1500.00 Cu.Yd.) at the gradient point.
    
9.  Click OK.
    
    The cut volume above the balance line is entirely free haul. Notice that now there is a red, overhaul volume below the balance line. You will balance the fill volume in the following procedure.
    
    ![](../images/GUID-83CF6521-FDB0-467A-BB33-CBD3C90CEC8B.png)
    

Balance fill material volumes

1.  Below the balance line, zoom in to chainage 6+25 on the mass haul line.
    
    Notice that this is near the point at which the overhaul volume (in red) and the free haul volume (in green) meet the mass haul line. If you examine this chainage on the profile, you see that it is also a relatively flat section of the existing ground surface. Flat areas can also be good locations for dump sites and borrow pits.
    
2.  At chainage 6+25, hover the cursor over the mass haul line.
    
    Notice that the tooltip displays the current chainage number and volume, which is approximately 2000.00 Cu. Yd.
    
    ![](../images/GUID-72AD1A78-4385-45FC-A88F-2209E385BC12.png)
    
3.  Select the mass haul line. Right-click. Click Mass Haul Line Properties.
4.  In the Mass Haul Line Properties dialog box, click the Balancing Options tab.
5.  In the Add/Remove Borrow Pits And Dump Sites area, click Add Borrow Pit.
6.  In the Chainage cell for the borrow pit, click ![](../images/GUID-D68E18C5-282D-4357-A394-C6A38ABACC0E.png).
7.  In the drawing, pan to left so that you can see the corridor and surface. Click near chainage 6+25 on the corridor.
    
    Notice the lack of surface contours in the area around chainage 6+25. This indicates that the region is relatively flat.
    
8.  In the Capacity cell for the borrow pit, enter **2000**.
    
    This is the approximate volume value that you noted in Step 2.
    
9.  Click OK.
    
    The fill volume below the balance line is entirely free haul.
    
    ![](../images/GUID-18157D08-9E81-4D6C-A8BA-8937A2A59136.png)
    
    **Further exploration:** Balance the mass haul volumes in the third region by adding a dump site at chainage 11+50 with a capacity of 10000 Cu. Yd.
    
10.  Close the drawing, but do not save your changes.

To continue this tutorial, go to [Exercise 3: Editing the Mass Haul Line Style](GUID-40701848-9513-4AEB-81B7-1BADC785A20D.htm "In this exercise, you will create a mass haul line style that is based on an existing style.").

**Parent topic:** [Tutorial: Working with Mass Haul Diagrams](GUID-FE54D3EB-0701-4F90-997A-1D86EEEFC947.htm "This tutorial demonstrates how to create and edit mass haul diagrams to display earthworks in profile.")