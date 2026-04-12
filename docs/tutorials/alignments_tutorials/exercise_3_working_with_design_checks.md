---
title: "Exercise 3: Working with Design Checks"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-18AF0EFE-37DA-4B04-B663-7BC6B4A5CCF3.htm"
category: "tutorial_designing_an_alignment_that_refers_to_loc"
last_updated: "2026-03-17T18:42:27.372Z"
---

                   Exercise 3: Working with Design Checks  

# Exercise 3: Working with Design Checks

In this exercise, you will create an alignment design check, add the design check to a design check set, and then apply the design check set to an alignment.

To create a design check, you set up a mathematical formula, using existing alignment sub-element properties. The complexity of design check formulas can vary greatly. In this exercise, you will create a relatively simple design check that validates whether the straight length meets a minimum value at a given design speed.

Note:

The processes for creating design checks for alignments and profiles are very similar. The basic workflow that is demonstrated in this exercise can be used for both alignment and profile design checks.

This exercise continues from [Exercise 2: Viewing and Correcting Alignment Design Criteria Violations](GUID-DF1B50F0-18B5-46D0-AA50-18867D236EAE.htm "In this exercise, you will examine alignment design criteria violations, and then learn how to correct a criteria violation.").

Create an alignment design check

1.  Open _Align-7C.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Settings tab, expand the Alignment![](../images/ac.menuaro.gif)Design Checks collection.
    
    Notice that five collections are available. The Design Check Sets collection contains combinations of design checks. A design check must be added to a design check set before it can be applied to an alignment.
    
    The other four collections contain the design checks for each type of alignment element. Each element type has specific properties that can be checked. When you create a design check set, you specify the type of element you want to check, and then the specific design check you want to apply to that element. You can apply multiple design checks to each element type.
    
    Note:
    
    The Straight Junction collection contains design checks for transition and curve groups.
    
3.  Right-click the Line collection. Click New.
4.  In the New Design Check dialog box, for Name, enter **310m @ 50km/h**.
    
    BestPractice:
    
    Because design check tooltips do not display specific values, a design check name should be specific and unique. Use the mathematical formula or other specific information to simplify the process of correcting a design check violation.
    
5.  For Description, enter **Straight length must be >= 310m if design speed is >= 50km/h**.
6.  Click ![](../images/GUID-2FDBB45F-EA3B-409B-8A37-86532FA11588.png)Insert Function. Click IF.
    
    The IF function is displayed in the Expression field.
    
7.  Click ![](../images/GUID-A80E6107-AE9C-429D-9396-162CA88F58CA.png)Insert Property. Select Design Speed.
8.  In the Expression field, use the keypad to enter **\>=50,** (including the comma) after the {Design Speed} property.
9.  Click ![](../images/GUID-A80E6107-AE9C-429D-9396-162CA88F58CA.png)Insert Property. Select Length.
10.  In the Expression field, enter **\>=310,1)** (including the closing parenthesis).
     
     The following formula should be displayed in the Expression field:
     
     **IF({Design Speed}>=50,Length>=310,1)**
     
     Note:
     
     In this formula, the ending numeral 1 specifies that the preceding formula is acceptable. If the element parameters do not meet the values specified in the formula, a violation is issued.
     
11.  Click OK.
12.  In Toolspace, on the Settings tab, expand the Alignment![](../images/ac.menuaro.gif)Design Checks![](../images/ac.menuaro.gif)Line collection.
     
     The design check you created is displayed in the Line collection.
     

Add a design check to a design check set

1.  In Toolspace, on the Settings tab, expand the Alignment![](../images/ac.menuaro.gif)Design Checks![](../images/ac.menuaro.gif)Design Check Sets collection.
2.  Right-click the **50kmh Carriageway Length Checks** design check set. Click Edit.
    
    This is the design check set that is applied to the alignment in the current drawing. Notice that the design checks that are in the selected design check set are displayed in the Toolspace list view.
    
    Tip:
    
    To create a new, empty design check set, right-click the Design Check Sets collection and click New.
    
3.  In the Alignment Design Check Set dialog box, click the Design Checks tab.
    
    The table on this tab lists the design checks that are currently in the design check set. The drop-down lists above the table allow you to add design checks to the set.
    
4.  In the Type list, select Line.
5.  In the Line Checks list, select **310m @ 50km/h**, which is the design check that you created in the previous procedure. Click Add. Click OK.
6.  If the Alignment Layout Tools toolbar is not open, select the alignment. Right-click. Click Edit Alignment Geometry.
7.  On the Alignment Layout Tools toolbar, click Alignment Grid View![](../images/GUID-7BDFFCF6-1694-4995-BDDE-54A6CE78E557.png). Click Sub-Entity Editor![](../images/GUID-0C471B43-C80E-4F2E-B3A2-6C49B374A5ED.png).
8.  In the Alignment Elements vista, select row 5.
9.  In the Alignment Layout Parameters dialog box, examine the Length value.
    
    In the Design Checks panel, notice that a warning symbol is displayed next to the310m @ 50km/h design check you created. In the Layout Parameters panel, notice that the Length value is less than the 310 meters specified by the design check.
    

**Further exploration:** Increase the length of the line until it meets or exceeds 310 meters. This is a fixed line that was created using the Straight-Straight (No Curves) command, so you must move the endpoint grip inside Circle D to increase the length.

To continue this tutorial, go to [Exercise 4: Modifying a Design Criteria File](GUID-E781CDDC-6D6C-4DEB-AF87-74A758671FAF.htm "In this exercise, you will add a radius and speed table to the design criteria file.").

**Parent topic:** [Tutorial: Designing an Alignment that Refers to Local Standards](GUID-CAEC3077-D78A-4F42-8E47-41FABAB6915D.htm "This tutorial demonstrates how to validate that your alignment design meets criteria specified by a local agency.")