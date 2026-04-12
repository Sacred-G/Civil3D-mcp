---
title: "Exercise 3: Assigning Rate Item Codes to Pipe Network Parts"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-B110DB96-B165-45EA-8551-A594A80DC97E.htm"
category: "tutorial_calculating_and_reporting_quantities"
last_updated: "2026-03-17T18:43:03.479Z"
---

                 Exercise 3: Assigning Rate Item Codes to Pipe Network Parts  

# Exercise 3: Assigning Rate Item Codes to Pipe Network Parts

In this exercise, you will modify a parts list to assign rate item codes to pipe network parts as they are created. You will also learn how to assign rate item codes to existing pipe network parts.

This exercise continues from [Exercise 2: Assigning Rate Item Codes to AutoCAD Objects](GUID-826E3EC7-9393-44D2-B3FA-B6A7790E263A.htm "In this exercise, you will assign rate item codes to a variety of AutoCAD objects, including lines, blocks, and closed polyline areas.").

Specify the QTO command settings

1.  Open _Quantities-3.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a commercial site, which consists of a building footprint, a parking lot, and access roads. The site also contains a storm sewer network that has pipes and several types of structures.
    
2.  Click Analyze tab ![](../images/ac.menuaro.gif)QTO panel ![](../images/ac.menuaro.gif)QTO Manager ![](../images/GUID-13CDDC6E-1903-4681-8F6D-130DCE8D61F5.png) Find.
3.  In the QTO Manager vista, click ![](../images/GUID-91450CE9-9539-488E-A422-C442F42D418B.png).
4.  In the Quantity Takeoff Command Settings dialog box, under Compute Takeoff Options, specify the following parameters:
    
    *   Computation Type: 3D
    *   Pipe Length Type: To Inside Edges
    
    These parameters specify that the pipe lengths will be reported, using the end-to-end distance, from the inside edge of each structure.
    
5.  Click OK.

Add rate item codes to a pipe network

1.  In the drawing, select a pipe network part. Click Pipe Networks tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Network Properties ![](../images/GUID-694629EB-1126-4D10-AD2A-64F4CE18F91D.png) Find.
2.  In the Pipe Network Properties dialog box, on the Layout Settings tab, under Network Parts List, click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection.
3.  In the Network Parts List dialog box, on the Pipes tab, expand the Storm Sewer![](../images/ac.menuaro.gif)Concrete Pipe category.
4.  In the **18 inch RCP** row, click ![](../images/GUID-4A68E528-AB71-47DA-AE6F-1978DB608EB5.png).
5.  In the Rate Item List dialog box, expand the ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png)Favorites category.
6.  In the ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png)Favorites category, select Rate Item ID**60201-0600**.
7.  Click OK.
8.  Click the Structures tab.
9.  Repeat Steps 4 through 7 to assign rate items to the following structures:
    
    Structure
    
    Rate Item ID
    
    51 x 6 x 51 inch Concrete Rectangular Headwall Mat\_CONC
    
    **60103-0100**
    
    Eccentric Structure 48 dia 24 frame 24 cone 5 wall 6 floor Mat\_CONC
    
    **60403-1100** **60409-0500**
    
10.  Click OK three times.

Add parts with rate items to the pipe network

1.  In the drawing, select a pipe. Click Pipe Networks tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Edit Pipe Network ![](../images/GUID-0EECE359-28D4-4653-A093-3BB525DF413E.png) Find.
2.  On the Network Layout Tools toolbar, specify the following parameters:
    *   Structure: **Eccentric Structure 48 dia 24 frame 24 cone 5 wall 6 floor Mat\_CONC**
    *   Pipe: **18 inch RCP**
    *   ![](../images/GUID-98319731-5225-4441-8634-18457D8DAD6B.png)Pipes and Structures: **Selected**
3.  In the drawing, click two points.
    
    This action creates two structures that are connected by a pipe.
    
4.  Press Enter.
5.  Hover the cursor over one of the new structures.
    
    The tooltip displays the description and ID of each rate item that has been assigned to that structure.
    
    ![](../images/GUID-E5C35BA4-573D-41EF-80BC-A190CFAD292B.png)
    

Assign rate items to existing pipe network parts

1.  In the drawing, select one of the gullies along the road.
    
    ![](../images/GUID-6E1F5881-E628-4FA7-A8B6-7DD2B0EB139E.png)
    
2.  Right-click. Click Select Similar.
3.  In the QTO Manager vista, click ![](../images/GUID-EB1E86EA-5459-4695-9014-662C7799E3C1.png).
4.  In the ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png)Favorites category, select Rate Item IDs **60403-1100** and **60409-0500**.
5.  Press Enter.
6.  Hover the cursor over one of the gullies.
    
    The tooltip displays the description and ID of the rate items that have been assigned to the gully.
    
    ![](../images/GUID-A1A6C2C7-9D29-461B-847D-508AA57D00B6.png)
    
    **Further exploration**: Repeat this procedure on the other structures in the network, assigning rate item codes that are appropriate for the square gullies, manholes, and headwalls.
    

To continue this tutorial, go to [Exercise 4: Assigning Rate Item Codes to Corridors](GUID-347C3502-FFB4-459F-ADFC-D0C547C5391A.htm "In this exercise, you will create a code set style to assign rate item codes to corridor areas and linear features.").

**Parent topic:** [Tutorial: Calculating and Reporting Quantities](GUID-709DB1D4-FB24-46F0-A54B-E2D9CC6D14F7.htm "In this tutorial, you will learn how to create and manage rate item data, associate rate item codes with several types of drawing objects, and generate rate item quantity reports.")