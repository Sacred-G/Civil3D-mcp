---
title: "Exercise 3: Viewing and Correcting Profile Design Criteria Violations"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-91160B7D-AC46-4957-A6B2-71D3D758C9CC.htm"
category: "tutorial_designing_a_profile_that_refers_to_local_"
last_updated: "2026-03-17T18:42:31.103Z"
---

                  Exercise 3: Viewing and Correcting Profile Design Criteria Violations  

# Exercise 3: Viewing and Correcting Profile Design Criteria Violations

In this exercise, you will check the profile design for criteria violations, and then learn how to correct violations.

When a profile sub-element violates a criteria or design check, a warning symbol is displayed on the sub-element in the drawing window, Profile Elements vista, and Profile Layout Parameters dialog box. When the cursor is hovered over a warning symbol, a tooltip displays information about the violation. If a design criteria has been violated, the tooltip displays the criteria that has been violated, as well as the minimum value that is required to meet the criteria. If a design check has been violated, the tooltip displays the name of the design check that has been violated.

This exercise continues from [Exercise 2: Drawing a Profile that Refers to Design Criteria](GUID-CE04E656-714D-4164-A499-EA9F482A237F.htm "In this exercise, you will draw a profile that refers to specified minimum standards.").

Check the profile design for criteria violations

1.  Open Profile-4B.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Pan and zoom so that you can see Circles B and C on the profile view.
    
    Note:
    
    The warning symbols do not automatically scale when you zoom in. Enter **REGEN** on the command line to resize the warning symbols.
    
3.  Hover the cursor over the ![](../images/GUID-D5ADDB33-0636-4E9A-BACD-113935F74A84.png) symbol in Circle C.
    
    The tooltips are a convenient way to review design criteria violations in the drawing window. Two violations are displayed in the tooltip:
    
    *   First, the curve does not meet the recommended minimum K value for headlight sight distance. The curve K value and minimum acceptable K value are both displayed.
    *   Second, the curve does not meet the formula specified in one of the design checks. Notice that the name of the design check is displayed, but not the current or recommended values. Values are not displayed because design checks are custom formulas that are created by the user.
    
    ![](../images/GUID-2E29F629-A1BC-4920-9DBC-6452F2CF2E50.png)
    
    Note:
    
    If a sub-element violates multiple criteria or design checks, only a single symbol is displayed on the sub-element. To clear a symbol from a sub-element, all the violations must be cleared.
    
4.  If the Profile Layout Tools toolbar is not open, select the red layout profile. Right-click. Click Edit Profile Geometry.
5.  On the Profile Layout Tools toolbar, click ![](../images/GUID-E556264E-99E0-4CCC-971A-6B43B3135DBF.png)Entity Based.
6.  Click ![](../images/GUID-7BDFFCF6-1694-4995-BDDE-54A6CE78E557.png) Profile Grid View.
    
    In the Profile Elements vista, in rows 2 and 4, notice that a ![](../images/GUID-F9D58477-90CB-4F8C-9B2F-F5473DE87F22.png) warning symbol appears in the No. column, as well as several other columns. Warning symbols appear next to each value that violates the design criteria that are specified in the design criteria file.
    
7.  In row 2, hover the cursor over the ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) warning symbol in the No. column.
    
    Notice that the tooltip displays the design criteria and design checks that have been violated. Notice that while the two curves violate the minimum length specified by the design checks, a warning symbol does not appear in either Length cell. Design checks are custom, mathematical formulas that return either a true or false value. They do not provide feedback other than whether the applicable elements meet or violate the conditions in the design check.
    
8.  On the Profile Layout Tools toolbar, click ![](../images/GUID-0C471B43-C80E-4F2E-B3A2-6C49B374A5ED.png)Profile Layout Parameters.
    
    The Profile Layout Parameters window is displayed, containing no data.
    
9.  In the Profile Elements vista, click row No. 4, which is the curve element in Circle C.
    
    The design data for the curve element is displayed in a three-column table in the Profile Layout Parameters window, where data is easy to review and edit.
    
    Notice that in the Profile Layout Parameters window, in the Design Criteria panel, a ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) symbol is displayed next to the design criteria property that has been violated. In the Layout Parameters panel, the Value column displays the actual parameters of each sub-element. The Constraints column displays the design criteria values that the sub-elements must meet. A ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) symbol is displayed next to the K Value row because the value violates the design criteria. As is true in the drawing window and Profile Elements Vista, the design check that has been violated is displayed, but individual parameters that violate the check are not marked.
    

Correct the design criteria violations

1.  In the Profile Layout Parameters window, on the Layout Parameters panel, change the Length Value to **65.000m**. Press Enter.
    
    Notice that the warning symbol is cleared from the Design Checks panel, as well as from row 4 in the Profile Elements vista. The new curve length value meets the value specified by the design check. Increasing the curve length also affected the K value, which increased to meet the minimum value for Headlight Sight Distance.
    
2.  In the Profile Elements vista, click row No. 2, which is the curve element in Circle B.
3.  In the Profile Layout Parameters window, on the Layout Parameters panel, change the Length Value to **35.000m**. Press Enter.
    
    The ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) warning symbol is cleared from the Design Checks panel, but not from the K Value row on the Profile Entities vista.
    
    The curve still does not meet the minimum K value for overtaking sight distance, so the warning symbols are displayed. To clear a warning symbol, the element must meet all values specified in both the design criteria file and the applicable design checks.
    
    Tip:
    
    There are two recommended methods for working around a Minimum K For Overtaking Sight Distance violation:
    
    *   Add a new design speed at the chainage at which the curve begins. You can do this in the Alignment Properties dialog box, on the Design Criteria tab.
    *   Designate the chainage range along curve as a No Passing Zone. This solution does not clear the warning symbol from the drawing, so you should annotate the symbol and No Passing Zone in the final plot.
    

To continue to the next tutorial, go to [Displaying and Modifying Profile Views](GUID-0BF95BEA-BDFF-4B0A-A73C-6749A5FFD1C5.htm "This tutorial demonstrates how to change the appearance of profile views.").

**Parent topic:** [Tutorial: Designing a Profile that Refers to Local Standards](GUID-75B9BF20-42D1-4D86-9465-C906A03FDC16.htm "This tutorial demonstrates how to validate that your profile design meets criteria specified by a local agency.")