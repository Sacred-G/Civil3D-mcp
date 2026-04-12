---
title: "Exercise 1: Drawing an Alignment that Refers to Design Criteria"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-A02CA33C-F6C2-44F1-88DC-FF87C83F032B.htm"
category: "tutorial_designing_an_alignment_that_refers_to_loc"
last_updated: "2026-03-17T18:42:27.285Z"
---

                Exercise 1: Drawing an Alignment that Refers to Design Criteria  

# Exercise 1: Drawing an Alignment that Refers to Design Criteria

In this exercise, you will use the criteria-based design tools to create an alignment that complies with specified standards.

This exercise is divided into two parts:

*   First, you will specify design criteria for an alignment as you create it, and then draw a series of alignment elements that violate the design criteria. You will correct the violations in [Exercise 2: Viewing and Correcting Alignment Design Criteria Violations](GUID-DF1B50F0-18B5-46D0-AA50-18867D236EAE.htm "In this exercise, you will examine alignment design criteria violations, and then learn how to correct a criteria violation.").
*   Second, you will create an alignment element that meets the design criteria specified in the design criteria file. You will use the minimum default values that are displayed on the command line to ensure that the element meets the specified design criteria.

Note: Autodesk Civil 3D can also validate that alignment elements are tangent to one another.

Specify design criteria for an alignment

1.  Open _Align-7A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains a surface marked with several circles, labeled A through E.
    
    Note:
    
    Ensure that Object Snap (OSNAP) is turned on. For more information, see [Object Snapping](GUID-11DBA8FF-E960-454F-B91F-88A715BD3118.htm#GUID-11DBA8FF-E960-454F-B91F-88A715BD3118__WSC7DD9FBD5FB88E861764BE1FD297C528F-8000).
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Alignment drop-down ![](../images/ac.menuaro.gif)Alignment Creation Tools ![](../images/GUID-C6A978DD-F77D-4284-BCBE-7E7D572F9097.png) Find.
3.  In the Create Alignment – Layout dialog box, accept the default values for Name and Starting Chainage. Leave the Description field blank for this exercise.
4.  On the General tab, specify the following parameters:
    *   Site: **<None>**
    *   Alignment Style: **Design Style**
    *   Alignment Layer: **C-ROAD**
    *   Alignment Label Set: **Major Minor and Geometry Points**
5.  On the Design Criteria tab, for Starting Design Speed, enter **50 km/h**.
    
    This speed will be applied to the starting chainage of the alignment. You can add design speeds as needed to other stations. A design speed is applied to all subsequent stations until either the next chainage that has an assigned design speed or the alignment ending chainage.
    
6.  Select the Use Criteria-Based Design check box.
    
    When this option is selected, the criteria-based design tools are available. There are two check boxes that are selected by default:
    
    *   **Use Design Criteria File** —The design criteria file is an XML file that contains minimum design standards for alignment and profile objects. The design criteria file can be customized to support local design standards for design speed, superelevation, and minimum speed, radius, and length of individual elements. The Default Criteria table lists the properties that are included in the default design criteria file, the location of which is displayed in the field above the Default Criteria table.
        
        You will learn more about the design criteria file in [Exercise 4: Modifying a Design Criteria File](GUID-E781CDDC-6D6C-4DEB-AF87-74A758671FAF.htm "In this exercise, you will add a radius and speed table to the design criteria file.").
        
    *   **Use Design Check Set** —Design checks are user-defined formulas that verify alignment and profile parameters that are not contained in the design criteria file. Design checks must be included in a design check set, which is applied to an alignment or profile.
7.  In the Use Design Criteria File area, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
8.  In the Select Design Speed Table dialog box, select **_\_Autodesk Civil 3D Metric Roadway Design Standards.xml_**. Click Open.
    
    Note: There are multiple design criteria files available to choose from. This exercise uses the AASHTO 2001 standards which are provided in _**\_Autodesk Civil 3D Metric Roadway Design Standards.xml**_. If you select the 2004 or 2011 file, please note that you may see somewhat different values than what are shown and described in this tutorial.
    
    You will learn about creating a custom design criteria file in [Exercise 4: Modifying a Design Criteria File](GUID-E781CDDC-6D6C-4DEB-AF87-74A758671FAF.htm "In this exercise, you will add a radius and speed table to the design criteria file.").
    
9.  In the Default Criteria table, in the Minimum Radius Table row, change the Value to **DMRB 2001 eMax 6%**.
10.  In the Use Design Check Set list, select **50kmh Roadway Length Checks**. Click OK.
     
     This design check set contains a simple design check. You will create another design check and add it to this design check set in [Exercise 3: Working with Design Checks](GUID-18AF0EFE-37DA-4B04-B663-7BC6B4A5CCF3.htm "In this exercise, you will create an alignment design check, add the design check to a design check set, and then apply the design check set to an alignment.").
     

Draw alignment elements that meet the specified design criteria

1.  On the Alignment Layout Tools toolbar, click ![](../images/GUID-F4D4D70E-4AC7-42D2-B7A6-53B1B875AC80.png)Straight-Straight (No Curves).
2.  Snap to the center of Circle A to specify a start point for the alignment. Stretch a line out, and specify additional PIs by snapping to the center of Circles B, C, and D (in order). Then, right-click to end the horizontal alignment layout command.
3.  On the Alignment Layout Tools toolbar, click ![](../images/GUID-60D70570-457B-49A5-884E-7940344C78BD.png)Free Transition-Curve-Transition (Between Two Elements).
4.  As prompted on the command line, click the straight element that enters Circle B from the left (the ‘first element’).
5.  Click the straight that exits Circle B on the right (the ‘next element’).
6.  Press Enter to accept the default value of a curve solution angle that is less than 180 degrees.
7.  For radius, enter **75**.
    
    Notice that the Specify Radius prompt includes a default value. This value is the minimum acceptable curve radius at the current design speed. The minimum value is contained in the Minimum Radius Table in the design criteria file. You can enter a different value, as long as it is greater than the default minimum value that is displayed. For this exercise, you will use values that do not meet the design criteria, and then examine the results.
    
8.  For transition in length, enter **25**.
9.  For transition out length, enter **25**.
10.  In the Alignment Layout Tools toolbar, click the drop-down list ![](../images/GUID-B1B6FEF0-4F07-4BBB-A036-85F833FD4493.png). Click ![](../images/GUID-9E138DA8-101A-4A7F-9961-2D982BBB485A.png)Free Curve Fillet (Between Two Elements, Radius).
11.  As prompted on the command line, click the straight that enters Circle C from the left (the ‘first element’).
12.  Click the straight that exits from Circle C on the right (the ‘next element’).
13.  Press Enter to select the default value of a curve less than 180 degrees.
14.  Press Enter to select the minimum radius of 90.000m.
15.  Right-click to end the command.
     
     Notice that in the drawing window, ![](../images/GUID-D5ADDB33-0636-4E9A-BACD-113935F74A84.png) symbols appear on the curve elements you created. The symbols indicate that the elements violate the specified design criteria. You will learn how to correct these violations in [Exercise 2: Viewing and Correcting Alignment Design Criteria Violations](GUID-DF1B50F0-18B5-46D0-AA50-18867D236EAE.htm "In this exercise, you will examine alignment design criteria violations, and then learn how to correct a criteria violation.").
     
     In step 14, you accepted the minimum radius value specified in the design criteria file, yet a warning symbol is displayed on the curve. This happened because while the curve meets the design criteria specified in the design criteria file, it violates the design check that is in the design check set. You will learn how to correct design criteria and design check violations in the next exercise.
     
     ![](../images/GUID-7BAEADA4-C043-4E6F-957E-05C4A7B0AFB0.png)
     
     Next, you will add another curve element and examine the results.
     

Add a sub-element that meets design criteria

1.  On the Alignment Layout Tools toolbar, click the drop-down list ![](../images/GUID-B1B6FEF0-4F07-4BBB-A036-85F833FD4493.png). Select More Floating Curves![](../images/ac.menuaro.gif)![](../images/GUID-1880276C-9934-4FD9-ADD0-06239704F7DC.png)Floating Curve (From Entity End, Radius, Length).
2.  As prompted on the command line, click the straight element that ends in Circle D (the ‘element to attach to’).
    
    Tip:
    
    The warning symbols do not automatically scale when you zoom in. Enter **REGEN** on the command line to resize the warning symbols.
    
3.  On the command line, enter **O** to specify the counterclockwise direction.
4.  Enter a radius value of **200.000m**.
5.  When prompted to specify a curve length, click in the center of Circle D, and then click in the center of Circle E.
    
    The curve is displayed in the drawing. The length value is the distance between the two points that you clicked.
    
6.  Right-click to end the command.
    
    Notice that a warning symbol is not displayed on this curve. The radius value you entered in step 4 exceeds the minimum value defined in the minimum radius table that you specified.
    

![](../images/GUID-9B0819F5-A2B6-4942-B87A-0CD0C013FAC1.png)

To continue this tutorial, go to [Exercise 2: Viewing and Correcting Alignment Design Criteria Violations](GUID-DF1B50F0-18B5-46D0-AA50-18867D236EAE.htm "In this exercise, you will examine alignment design criteria violations, and then learn how to correct a criteria violation.").

**Parent topic:** [Tutorial: Designing an Alignment that Refers to Local Standards](GUID-CAEC3077-D78A-4F42-8E47-41FABAB6915D.htm "This tutorial demonstrates how to validate that your alignment design meets criteria specified by a local agency.")