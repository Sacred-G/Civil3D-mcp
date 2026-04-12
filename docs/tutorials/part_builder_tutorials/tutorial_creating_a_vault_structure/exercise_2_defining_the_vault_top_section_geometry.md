---
title: "Exercise 2: Defining the Vault Top Section Geometry"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-5013FCFB-AD45-4399-9926-19040957C197.htm"
category: "tutorial_creating_a_vault_structure"
last_updated: "2026-03-17T18:43:13.270Z"
---

                Exercise 2: Defining the Vault Top Section Geometry  

# Exercise 2: Defining the Vault Top Section Geometry

In this exercise, you will build the top portion of the vault. This is a rectangular frame with a rectangular opening. You will build this portion with dimensions that can be modified from within Autodesk Civil 3D when the part is in use.

This exercise continues from [Exercise 1: Defining the New Part in the Structure Catalog](GUID-6CD31559-126E-45CD-A44A-A57C1A61ABAE.htm "In this exercise, you will begin creating a vault structure in Part Builder by creating a new part chapter, and a new part family within the Structure catalog.").

1.  Expand Modeling![](../images/ac.menuaro.gif)Work Planes, right-click Cover![](../images/ac.menuaro.gif)Set View. The current view and UCS is set to match the work plane.
2.  Right-click Cover![](../images/ac.menuaro.gif)Add Profile![](../images/ac.menuaro.gif)Rectangular. Click two points to draw a rectangle in the drawing area. A rectangular profile object is shown in the drawing.
3.  Expand cover, right-click Rectangular Profile in the Content Builder window and then click Rename. Enter Frame Outer Edge as the name. This name makes it easier to identify the component.
4.  Repeat these steps to create another rectangular profile within the first one. Name it Frame Opening.
    
    Next you will establish some construction geometry and constraints to keep the opening positioned at the center of the box. Rectangular profiles representing the outer edge of the frame and the opening have been created.
    
    ![](../images/GUID-37FC2D85-16C0-442B-AB54-4C5180909AEC.png)
    
5.  Right-click Cover![](../images/ac.menuaro.gif)Add Geometry![](../images/ac.menuaro.gif)Point. Click a location near the center of the rectangles. Press ESC. This becomes the center point of the structure.
    
    ![](../images/GUID-C97E2272-BBDC-4AE1-80C0-04A541154730.png)
    
6.  Expand the Geometry folder. Right-click Point 2D![](../images/ac.menuaro.gif)Rename. Enter Fixed Center for the name. This name makes it easier to identify the component.
7.  Right-click Fixed Center![](../images/ac.menuaro.gif)Fixed. The point is now fixed. Constraints that include this point will not move the point.
8.  Right-click Cover![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Equal Distance. Click the point at the top left corner of the outer rectangle, then the Fixed Center Point. For the second pair, click the point at the bottom right corner of the outer rectangle, and the Fixed Center Point.
    
    ![](../images/GUID-B36EACED-00B0-4DED-B9AE-9849465A4004.png)
    
9.  Repeat this step for the bottom left and upper right corners of the outer rectangle. This constrains the outer rectangle so that it is centered about the Fixed Center.
10.  Repeat these steps for the four corners of the inner rectangle. Both rectangles are now centered about the fixed point.
     
     ![](../images/GUID-2511ED3A-22C8-495F-917F-BDB1BA94CB83.png)
     
11.  Right-click Cover![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Parallel. Click any line on the outer rectangle, then the line that is parallel to it on the inner rectangle. This constraint prevents the inner rectangle from rotating within the outer rectangle.
     
     ![](../images/GUID-9D8549DC-91B3-4C45-A314-E1A3E04F4417.png)
     
12.  Right-click Cover![](../images/ac.menuaro.gif)Add Dimension![](../images/ac.menuaro.gif)Distance. Click two points at either end of the top side of the outer rectangle.
     
     Click a point to set the location of the dimension. A dimension named LenA1 is created for the long side of the outer edge of the frame.
     
     ![](../images/GUID-AFC7A484-982C-4E0E-8393-D80CBAE3EC5F.png)
     
13.  Repeat these steps for the right side of the outer rectangle.
     
     A dimension named LenA2 is created for the short side of the outer edge of the frame.
     
     ![](../images/GUID-BEB1BFC3-4F1E-456D-BB07-871572AAADC2.png)
     
14.  Repeat these steps for top and right sides of the opening, in that order. Reposition the dimensions as needed so that they are easy to read. LenA3 and LenA4 are created for the long side and short side of the opening, respectively.
15.  Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)SW Isometric. This 3D view is better suited for the next steps.
     
     ![](../images/GUID-340E34D1-D29E-46EF-A9D5-763BAF9C11A9.png)
     
16.  Right-click Modifiers![](../images/ac.menuaro.gif)Add Extrusion. Click the outer rectangle. The Extrusion Modifier dialog box is displayed.
17.  Verify that Type is set to Blind. Enter 8 for Distance and check the box next to Flip. Click OK. An extrusion modifier is created for the outer rectangle.
     
     ![](../images/GUID-D6CD8C99-2593-43FD-8AB9-A3BB3C91592B.png)
     
18.  Repeat these steps to create an Extrusion modifier for the inner rectangle using a distance of 9. An extrusion modifier is created for the inner rectangle.
     
     ![](../images/GUID-8A5341A5-75DD-4F31-84C4-4AE23944736A.png)
     
19.  Right-click Model Dimensions![](../images/ac.menuaro.gif)Add Distance. Click one of the vertical edges of the outer box extrusion. Click a point to set the location of the dimension. LenB1 is created for the height of the frame section.
     
     ![](../images/GUID-44339AE2-29BF-4C31-B0A6-5CAF4864DA7D.png)
     
20.  Repeat this step for the inner box extrusion. LenB2 is created for the height of the opening extrusion.
     
     ![](../images/GUID-7543CB69-C924-4BDD-9ADE-7BFACC9C16B6.png)
     
21.  Expand Model Parameters, right-click Model Parameters, and then click Edit. The Model Parameters dialog box is displayed.
22.  Click LenB2, then click Calculator. Click Variable, then select LenB1. Enter +1 after LenB1 and then click OK, then Close.
     
     This ensures that the extrusion for the opening is always deeper than the thickness of the frame. You could also have double clicked the cell in the Equation column and entered in your own formula.
     
23.  Right-click Modifiers![](../images/ac.menuaro.gif)Add Boolean Subtract. Click the outer box, then the inner box, and then press Enter. The inner box is subtracted from the outer box creating a third modifier which is the result of this action.
     
     ![](../images/GUID-00E9925D-6089-4CBF-9C0D-6D10139DCB68.png)
     
24.  Expand Modifiers. Notice the three modifiers that have been created. The Extrusion modifiers have been set to invisible by default as a result of the Subtract command. Rename the modifier named Subtract to Frame.
25.  Right-click Size Parameters and then click Edit Configuration. The Edit Part Sizes dialog box is displayed.
26.  Scroll to the right until you see the SBSL (Structure Length) column. Click Constant in the SBSL column, and change it to list. Do the same for the SBSW (Structure Width) Column. These parameters can now be specified by a list of values, rather than by a single constant.
27.  Click the drop-down arrow button next to Parameter Configuration, and select Values. Click the cell under SBSL, and then click the Edit button from the Edit Part Sizes dialog box toolbar. The Edit Values dialog box is displayed.
28.  Change the current value to 48. Then use the Add button to create values of 60, 72, 84, 96, 108, and 120. This makes it so that the Structure Length parameter can be adjusted in 12-inch increments.
29.  Repeat this step for the SBSW column, creating values of 36, 48, 60, and 72.
30.  Set the value of SBSL to 120 and the value of SBSW to 60. Click the Update Model button in the Edit Part Sizes dialog box toolbar. This updates the values under Model Parameters to match the Part Parameters.
31.  Click OK. Right-click Model Parameters and then click Edit. Click LenA1, and then click Calculator.
32.  Click Variable, and then click SBSL. Click OK, and then Close.
33.  Repeat these steps for LenA2, equating it to SBSW. The model is updated to reflect the changes to the dimensions.
     
     ![](../images/GUID-5560B402-F31B-4A72-932B-14AC9EE1EEC3.png)
     
34.  Right-click Size Parameters and then click Edit Configuration. Click New. The New Parameter dialog box is displayed.
35.  Click Frame Length, and then click OK. Click New, then click Frame Width, OK. Two new parameters have been added: frame length (SFL) and frame width (SFW).
36.  Scroll to the right until you see the SFL and SFW columns. Change Constant to List for each of these columns. Then change the view to Values.
37.  Using the same procedure that was used for SBSL and SBSW, add values of 36, 48, 60, 72, 84, 96, and 108 for SFL. Add values of 24, 36, 48, and 60 for SFW.
     
     These values are now controlled by lists that can ultimately be manipulated from within Autodesk Civil 3D when the part is in use.
     
38.  Click OK to close the Edit Part Sizes dialog box. Click Save Part Family. This saves the part as well as updates the Model Parameters to include the newly added Size Parameters.
39.  Right-click Size Parameters and then click Edit Values. Set the following values:
     
     *   SFL: 108
     *   SFW: 48
     
     Click the Update Model button in the Edit Part Sizes dialog box toolbar, and then click OK. This updates the Model Parameter values so that they match the Part Parameter values where applicable.
     
40.  Expand Model Parameters, edit LenA3 and LenA4 to equate to SFL and SFW respectively. The model updates to reflect the changes to the dimensions.
     
     ![](../images/GUID-FADB46AC-06D7-4859-97AB-B800EDEEC59A.png)
     
41.  Click Save Part Family. Remain in the Part Builder environment for the next exercise.

To continue this tutorial, go to [Exercise 3: Defining the Vault Box Geometry](GUID-65B91E08-D951-4341-A184-DD75E30508AF.htm "In this exercise, you will build the box portion of the vault. You will use projection geometry and constraints to link the box to the frame so that a single set of dimensions can control both.").

**Parent topic:** [Tutorial: Creating a Vault Structure](GUID-5417170D-9D08-4B57-83A6-FE84173720FA.htm "This tutorial demonstrates how to use Part Builder to create a vault structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the vault.")