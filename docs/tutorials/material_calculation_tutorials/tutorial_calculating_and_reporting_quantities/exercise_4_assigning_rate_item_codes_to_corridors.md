---
title: "Exercise 4: Assigning Rate Item Codes to Corridors"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-347C3502-FFB4-459F-ADFC-D0C547C5391A.htm"
category: "tutorial_calculating_and_reporting_quantities"
last_updated: "2026-03-17T18:43:03.616Z"
---

                 Exercise 4: Assigning Rate Item Codes to Corridors  

# Exercise 4: Assigning Rate Item Codes to Corridors

In this exercise, you will create a code set style to assign rate item codes to corridor areas and linear features.

A code set style applies rate items to the link or point codes that define corridor assemblies. The code set style enables you to use the corridor model to compute quantities for a variety of units of measure, such as:

*   **Cumulative Area or Volumes**: To extract cumulative volumes of closed corridor areas, apply rate item codes to corridor links.
    
    Links with rate item codes are used to extract rate item area or volumetric quantities of materials such as asphalt, gravel, or soil.
    
*   **Linear Quantities**: To extract linear quantities along a corridor feature line, apply rate item codes to corridor points.
    
    Points with rate item codes are used to extract linear quantities for materials such as guardrails and kerb.
    
*   **Itemized Count**: To extract itemized quantities of a particular item, apply rate item codes to corridor points, and use a formula to compute the quantity from the feature line length.
    
    Note:
    
    You will learn how to create and apply rate item formulas in [Exercise 6: Working with Rate Item Formulas](GUID-AA55D4D1-EB5E-4475-81CF-95EB5D88E68F.htm "In this exercise, you will build a mathematical formula that applies a rate item to a corridor at a specified interval.").
    

This exercise continues from [Exercise 3: Assigning Rate Item Codes to Pipe Network Parts](GUID-B110DB96-B165-45EA-8551-A594A80DC97E.htm "In this exercise, you will modify a parts list to assign rate item codes to pipe network parts as they are created. You will also learn how to assign rate item codes to existing pipe network parts.").

Create a code set style

1.  Open _Quantities-4.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a commercial site, which consists of a building footprint, a parking lot, and access roads.
    
2.  In Toolspace, on the Settings tab, expand the General![](../images/ac.menuaro.gif)Multipurpose Styles![](../images/ac.menuaro.gif)Code Set Styles collection. Right-click All Codes. Click Copy.
3.  In the Code Set Style dialog box, on the Information tab, for Name, enter **Corridor Quantities**.
4.  On the Codes tab, under Link, in the Base row, click ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png).
5.  In the ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png)Favorites category, select Pay Item ID**30202-0600**.
6.  Click OK.
7.  Repeat Steps 4 through 6 to apply rate item codes to the following links:
    *   Pave: **40920-1000**
    *   Pave1: **40930-0200**
    *   Pave2: **40310-3300**
    *   SubBase: **30202-0800**
8.  Repeat Steps 4 through 6 to apply rate item codes to the following Point:
    
    Top\_Kerb: **60902-0800**
    
9.  Click OK.

Apply the new code set to the corridor and assembly

1.  In the drawing, select the side road corridor. Right-click. Click Properties.
    
    ![](../images/GUID-B5346F20-A758-41F2-8FD7-B30A68009BBA.png)
    
2.  On the Properties palette, under Data, for Code Set Style Name, select **Corridor Quantities**.
3.  Press Esc.
4.  In the drawing, select the baseline of the corridor assembly. Right-click. Click Properties.
5.  On the Properties palette, under Data, for Code Set Style Name, select **Corridor Quantities**.
6.  Press Esc.
7.  Rebuild the corridor.

To continue this tutorial, go to [Exercise 5: Working with Quantity Reports](GUID-E870AEEA-4E45-480B-9DA3-BE6846A41A8F.htm "In this exercise, you will generate quantity reports, and then examine several ways to use the resulting data.").

**Parent topic:** [Tutorial: Calculating and Reporting Quantities](GUID-709DB1D4-FB24-46F0-A54B-E2D9CC6D14F7.htm "In this tutorial, you will learn how to create and manage rate item data, associate rate item codes with several types of drawing objects, and generate rate item quantity reports.")