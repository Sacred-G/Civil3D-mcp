---
title: "Exercise 2: Assigning Rate Item Codes to AutoCAD Objects"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-826E3EC7-9393-44D2-B3FA-B6A7790E263A.htm"
category: "tutorial_calculating_and_reporting_quantities"
last_updated: "2026-03-17T18:43:03.255Z"
---

                 Exercise 2: Assigning Rate Item Codes to AutoCAD Objects  

# Exercise 2: Assigning Rate Item Codes to AutoCAD Objects

In this exercise, you will assign rate item codes to a variety of AutoCAD objects, including lines, blocks, and closed polyline areas.

You will use the AutoCAD Quick Select and Select Similar commands to select similar objects. You can use these commands to assign a rate item code to many objects at the same time.

This exercise continues from [Exercise 1: Loading and Navigating a Rate Item List](GUID-07D62C88-4439-49F9-A03A-E5A3C89F4C67.htm "In this exercise, you will open a sample file that contains a rate item list and examine the contents.").

Assign a rate item code to linear objects

1.  Open _Quantities-2.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a commercial site, which consists of a building footprint, a parking lot, and access roads.
    
2.  On the command line, enter **QSELECT**.
3.  Click OK to acknowledge that objects were not selected.
4.  In the Quick Select dialog box, specify the following parameters:
    *   Apply To: **Entire Drawing**
    *   Object Type: **Line**
    *   Properties: **Layer**
    *   Operator: **\= Equals**
    *   Value: **PKNG-STRP-AISLES**
    *   How To Apply: **Include In New Selection Set**
    *   Append to Current Selection Set: **Selected**
5.  Click OK.
6.  Press Enter to return to the Quick Select dialog box.
7.  Repeat Steps 2 through 4 to select the lines on the **PKNG-STRP-STALLS** layer.
    
    In the drawing, all parking lot stall lines are selected.
    
    ![](../images/GUID-56CADBB6-9847-4B2F-8884-BFA7385B82EF.png)
    
8.  Click Analyze tab ![](../images/ac.menuaro.gif)QTO panel ![](../images/ac.menuaro.gif)QTO Manager ![](../images/GUID-13CDDC6E-1903-4681-8F6D-130DCE8D61F5.png) Find.
9.  In the QTO Manager vista, click ![](../images/GUID-EB1E86EA-5459-4695-9014-662C7799E3C1.png).
    
    Tip:
    
    You can also right-click the rate item and click Assign Rate Item.
    
10.  In the ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png)Favorites category, select Rate Item ID**63401-0300**.
11.  Press Enter.
12.  Hover the cursor over one of the parking plot lines.
     
     The tooltip displays the rate item description and ID that has been assigned to that object.
     
     ![](../images/GUID-54D162EA-7B13-4B21-8369-EB60DFFE3C44.png)
     

Assign a rate item to AutoCAD blocks

1.  In the drawing, select one of the blocks that represent parking lot lamps.
    
    ![](../images/GUID-16D19E7D-0E56-4C15-A3CD-2932139EBD46.png)
    
2.  Right-click. Click Select Similar.
    
    All the lamp blocks are selected.
    
3.  Click ![](../images/GUID-EB1E86EA-5459-4695-9014-662C7799E3C1.png).
4.  In the ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png)Favorites category, select Rate Item ID**63612-0300**.
5.  Press Enter.
6.  Hover the cursor over one of the blocks.
    
    The tooltip displays the description and ID of each rate item that has been assigned to that block.
    
    ![](../images/GUID-6217817E-5EDF-4916-B4F6-E5CF80962458.png)
    

Assign multiple rate item codes to a closed area

1.  In the QTO Manager vista, click ![](../images/GUID-002D7F9A-0870-4108-8AF8-6AD212775A46.png).
2.  In the ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png)Favorites category, select the following pay items:
    
    *   **62401-0400**
    *   **62511-2000**
    *   **62525-0000**
    
    Tip:
    
    To select multiple items, hold the Ctrl key down and then click the items.
    
3.  Press Enter.
4.  On the command line, enter **O**.
    
    This action activates object selection mode, in which you select the outline of a closed object, as opposed to a point inside the object. Because some of the parking lot islands are subdivided by pipes, you can use object selection mode to assign the rate item codes to the entire object, not only to the closed area that you select.
    
    Tip:
    
    For faster performance, zoom in to a closed polygon before you select it.
    
5.  In the drawing, click the green border in a parking lot island.
    
    A solid hatch pattern is displayed in the parking lot island. This indicates that the rate items have been applied to the area.
    
6.  Select several other islands.
    
    ![](../images/GUID-2AC269C5-D7C9-4476-B4B4-DCA24411675D.png)
    
7.  Press Enter to end the command.
8.  Hover the cursor over one of the parking lot islands.
    
    The tooltip displays the description and ID of each rate item that has been assigned to that area.
    
    ![](../images/GUID-6737BA20-74DD-475B-8CDB-FD7D55B1136A.png)
    

To continue this tutorial, go to [Exercise 3: Assigning Rate Item Codes to Pipe Network Parts](GUID-B110DB96-B165-45EA-8551-A594A80DC97E.htm "In this exercise, you will modify a parts list to assign rate item codes to pipe network parts as they are created. You will also learn how to assign rate item codes to existing pipe network parts.").

**Parent topic:** [Tutorial: Calculating and Reporting Quantities](GUID-709DB1D4-FB24-46F0-A54B-E2D9CC6D14F7.htm "In this tutorial, you will learn how to create and manage rate item data, associate rate item codes with several types of drawing objects, and generate rate item quantity reports.")