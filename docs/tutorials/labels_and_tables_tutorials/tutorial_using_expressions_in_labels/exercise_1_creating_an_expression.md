---
title: "Exercise 1: Creating an Expression"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-50AF5191-2245-4FF6-9708-6590339F17BA.htm"
category: "tutorial_using_expressions_in_labels"
last_updated: "2026-03-17T18:43:21.198Z"
---

                 Exercise 1: Creating an Expression  

# Exercise 1: Creating an Expression

In this exercise, you will create an expression that calculates the magnetic compass direction of an alignment at each geometry point.

Expressions make use of the same properties that you can add to label styles, such as Point Level, Northing, and Easting. By using expressions, you can set up separate mathematical formulas, using the existing properties. For example, you could subtract a value from a point level, and display that number along side the actual level in a point label.

Create an expression

Note:

This exercise uses _Labels-5c.dwg_ with the modifications you made in the previous exercise, or you can open _Labels-6a.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Zoom and pan to the area between stations 0+000 and 0+100 of the Main Street alignment.
3.  Click the **PC: 0+035.99** label to select all geometry point labels. Right-click. Click Properties.
4.  On the Properties palette, in the Geometry Point Label Style list, select **Additional Details**.
    
    This label style displays geometry points, design speed, and true compass direction at the geometry point. In the next few steps, you will create an expression to calculate the magnetic direction. You will add this expression to the geometry point label in [Exercise 2: Inserting an Expression Into a Label Style](GUID-2B7FA445-CA59-4044-8B5E-B7478662F7F7.htm "In this exercise, you will insert an expression into an existing label style.").
    
5.  In Toolspace, on the Settings tab, expand Alignment![](../images/ac.menuaro.gif)Label Styles![](../images/ac.menuaro.gif)Chainage![](../images/ac.menuaro.gif)Geometry Point. Right-click the Expressions node. Click New.
6.  In the New Expression dialog box, specify the following parameters:
    *   Name: **Magnetic Direction**
    *   Description: **Converts true to magnetic for declination -15.5 degrees.**
7.  Click ![](../images/GUID-A80E6107-AE9C-429D-9396-162CA88F58CA.png)Insert Property. Click Instantaneous Direction.
    
    This property will be used as the basis for computing the magnetic direction.
    
8.  In the New Expression dialog box, in the Expression field, use the keypad to enter **\-(15.5\*(2\*pi/360))** .
    
    Note:
    
    Use the ![](../images/GUID-93C8B4EF-046F-427C-9382-764BCCE18B5C.png) button to enter pi.
    
    The completed equation looks like this:
    
    **{Instantaneous Direction}-(15.5\*(2\*pi/360))**
    
    The expression includes a conversion from degrees to radians because Autodesk Civil 3D uses radians for all internal angle calculations. The value used for declination of magnetic North (-15.5 degrees) is just an example. To be accurate, this must match the current value, subject to geographical location and gradual changes over time.
    
9.  In the Format Result As list, select Direction.
10.  Click OK.
     
     In Toolspace, on the Settings tab, ![](../images/GUID-379A8AD5-3792-4A22-8815-E61573B7DCFD.png) appears next to the Expressions node, and the new expression appears in the list view.
     

To continue this tutorial, go to [Exercise 2: Inserting an Expression Into a Label Style](GUID-2B7FA445-CA59-4044-8B5E-B7478662F7F7.htm "In this exercise, you will insert an expression into an existing label style.").

**Parent topic:** [Tutorial: Using Expressions in Labels](GUID-ADE0D8A5-CB73-40C2-B7FF-10E25FEE55F1.htm "This tutorial demonstrates how to use expressions, which are mathematical formulas that modify a property within a label style.")