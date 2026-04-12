---
title: "Exercise 2: Viewing and Correcting Alignment Design Criteria Violations"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-DF1B50F0-18B5-46D0-AA50-18867D236EAE.htm"
category: "tutorial_designing_an_alignment_that_refers_to_loc"
last_updated: "2026-03-17T18:42:27.327Z"
---

                  Exercise 2: Viewing and Correcting Alignment Design Criteria Violations  

# Exercise 2: Viewing and Correcting Alignment Design Criteria Violations

In this exercise, you will examine alignment design criteria violations, and then learn how to correct a criteria violation.

When a sub-element violates either a criteria or design check, a warning symbol is displayed on the sub-element in the drawing window, Alignment Elements vista, and Alignment Layout Parameters dialog box. When the cursor is hovered over a warning symbol in the drawing window, a tooltip displays information about the violation. If a design criteria has been violated, the tooltip displays the criteria that has been violated, as well as the minimum value required to meet the criteria. If a design check has been violated, the tooltip displays the name of the design check that has been violated.

This exercise continues from [Exercise 2: Drawing a Profile that Refers to Design Criteria](GUID-A02CA33C-F6C2-44F1-88DC-FF87C83F032B.htm "In this exercise, you will use the criteria-based design tools to create an alignment that complies with specified standards.").

Check the alignment design for criteria violations

1.  Open _Align-7B.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Pan and zoom until you can see Circles B and C on the surface.
    
    Tip:
    
    The warning symbols do not automatically scale when you zoom in. Enter **REGEN** on the command line to resize the warning symbols.
    
3.  Hover the cursor over the middle ![](../images/GUID-D5ADDB33-0636-4E9A-BACD-113935F74A84.png) symbol in Circle B.
    
    The tooltips are a convenient way to review design criteria violations in the drawing window. Two violations are displayed in the tooltip:
    
    *   First, the curve does not meet the recommended minimum radius. The curve radius and minimum acceptable parameter values are both displayed.
    *   Second, the curve violates a design check that has been applied to the alignment. Notice that the name of the design check is displayed, but not the current or recommended values. Values are not displayed because design checks are custom formulas that are created by the user.
    
    ![](../images/GUID-B9101D86-6315-4C62-B21B-B6C8B54EE6F7.png)
    
    Note:
    
    If a sub-element violates multiple criteria or design checks, only a single symbol is displayed on the sub-element. To clear a symbol from a sub-element, all the violations must be cleared.
    
4.  If the Alignment Layout Tools toolbar is not open, select the alignment. Right-click. Click Edit Alignment Geometry.
5.  On the Alignment Layout Tools toolbar, click Alignment Grid View![](../images/GUID-7BDFFCF6-1694-4995-BDDE-54A6CE78E557.png).
    
    In the Alignment Elements vista, in rows 2.1 through 2.3, a ![](../images/GUID-F9D58477-90CB-4F8C-9B2F-F5473DE87F22.png) warning symbol appears in the No. column, as well as in several other columns. Warning symbols appear next to each value that violates the design criteria that are specified in the design criteria file.
    
6.  In row 2.2, hover the cursor over the ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) warning symbol in the No. column.
    
    Notice that the tooltip displays the design criteria and design checks that have been violated.
    
7.  Hover the cursor over the ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) warning symbol in row 4.
    
    Notice that the name of the design check that has been violated is displayed in the tooltip. Design checks are custom mathematical formulas that return either a true or false value. They indicate whether the applicable elements violate the conditions in the design check, but do not specify how to correct the violation. You will learn more about design checks in [Exercise 3: Working with Design Checks](GUID-18AF0EFE-37DA-4B04-B663-7BC6B4A5CCF3.htm "In this exercise, you will create an alignment design check, add the design check to a design check set, and then apply the design check set to an alignment.").
    
8.  On the Alignment Layout Tools toolbar, click Sub-Element Editor![](../images/GUID-0C471B43-C80E-4F2E-B3A2-6C49B374A5ED.png).
    
    The Alignment Layout Parameters window is displayed, containing no data.
    
9.  In the Alignment Elements vista, click any row for segment No. 2, which is the transition-curve-transition element in Circle B.
    
    The design data for all three sub-elements is displayed in a three-column table in the Alignment Layout Parameters window, where data is easy to review and edit.
    
    Notice that in the Alignment Layout Parameters window, in the Design Criteria panel, a ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) symbol is displayed next to each design criteria property that has been violated. In the Layout Parameters panel, the Value column displays the actual parameters of each sub-element. The Constraints column displays the design criteria values that the sub-elements must meet. A ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) symbol is displayed next to each parameter that violates the design criteria. As is true in the drawing window and Alignment Elements Vista, the design check that has been violated is displayed, but individual parameters that violate the check are not marked.
    

Correct design criteria violations

1.  In the Alignment Layout Parameters window, on the Layout Parameters panel, change the Transition In Length Value to **33.000m**. Press Enter.
    
    Notice that the warning symbol is cleared from the Transition In Length row.
    
2.  Change the Transition Out Length Value to **33.000m**. Press Enter.
3.  Change the Curve RadiusValue to **100.000m**. Press Enter.
    
    The ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) warning symbol is cleared from the Curve Radius row, as well as from the Alignment Elements vista.
    
    Notice that the warning symbol is still displayed on all the curve sub-element. The curve still violates the design check. To clear the warning symbols, all sub-elements in the group must meet the values specified in both the design criteria file and the applicable design checks.
    
4.  In the Alignment Elements vista, in row 2.2, examine the Length column.
    
    Notice that the Length value is less than the value of 40 that is specified by the design check. Notice that you cannot edit the Length value for this type of curve. However, you can increase the curve radius to increase the curve length.
    
5.  In row 2.2, change the Radius value to **200.000m**. Press Enter.
6.  In the Alignment Entities vista, select row 4. In the Radius column, change the value to **100.000m**. Press Enter.

To continue this tutorial, go to [Exercise 3: Working with Design Checks](GUID-18AF0EFE-37DA-4B04-B663-7BC6B4A5CCF3.htm "In this exercise, you will create an alignment design check, add the design check to a design check set, and then apply the design check set to an alignment.").

**Parent topic:** [Tutorial: Designing an Alignment that Refers to Local Standards](GUID-CAEC3077-D78A-4F42-8E47-41FABAB6915D.htm "This tutorial demonstrates how to validate that your alignment design meets criteria specified by a local agency.")