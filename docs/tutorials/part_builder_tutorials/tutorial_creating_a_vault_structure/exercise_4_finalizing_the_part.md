---
title: "Exercise 4: Finalizing the Part"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-F1C850F2-E145-4C0E-BEFE-8F713F56EECC.htm"
category: "tutorial_creating_a_vault_structure"
last_updated: "2026-03-17T18:43:13.745Z"
---

                Exercise 4: Finalizing the Part  

# Exercise 4: Finalizing the Part

In this exercise, you will add the final model and size parameters that will allow the part geometry to be modified in Autodesk Civil 3D.

This exercise continues from [Exercise 3: Defining the Vault Box Geometry](GUID-65B91E08-D951-4341-A184-DD75E30508AF.htm "In this exercise, you will build the box portion of the vault. You will use projection geometry and constraints to link the box to the frame so that a single set of dimensions can control both.").

1.  Continue from the previous exercise in the Part Builder environment. Click the View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)SW Isometric. Next you will link the Size Parameters that will be available in Autodesk Civil 3D with the actual dimensions of the Top Section. This view makes it easier to perform the next few steps.
2.  Right-click Size Parameters![](../images/ac.menuaro.gif)Edit Configuration. Click the New button at the top of the Edit Part Sizes dialog box. The New Parameter dialog box is displayed.
3.  Click Frame Height, and then click OK. The Frame Height (SFH) parameter is added.
4.  Scroll to the right until you see the SFH column. Click Constant in the SFH column and change it to List. Click the drop-down arrow button next to Parameter Configuration, and select Values. The values for each parameter are shown.
5.  Click the value cell in the SFH column, and then click the Edit button. Add the values 8,12, and 16 to the list and then click OK. Set the value of SFH to 8. This parameter will now be available as three sizes in Autodesk Civil 3D.
6.  Click OK. Click Save Part Family. This updates the Model Parameters to include the Size Parameters.
7.  Right-click Size Parameters and then click Edit Values. Set the following values:
    
    *   SBSL: 120
    *   SBSW: 60
    *   SFL: 108
    *   SFW: 48
    *   SBSH: 48
    
    Click the Update Model button in the Edit Part Sizes dialog box toolbar. Click OK. This updates the model parameter values to match the size parameters.
    
8.  Right-click Model Parameters and then click Edit. Set the value of LenB1 to SFH. Click Close. LenB1 is the dimension that represents the depth of the outer frame extrusion.
    
    ![](../images/GUID-C10C2532-F8BE-4F34-9D25-60419C0A6462.png)
    
9.  Expand Modifiers. Right-click Box and turn off the visibility. Turn on the visibility of Box Outside and Box Inside. This displays the appropriate modifiers for dimensioning.
10.  Right-click Model Dimensions and then click Add Distance. Click the outer box extrusion in the drawing, then click a location for the dimension.
     
     ![](../images/GUID-2F07C459-0B01-4E6A-890F-C74FE855F091.png)
     
     Dimension LenB3 is created.
     
     ![](../images/GUID-13FF125A-612E-4507-9CE0-1A93B06E80F3.png)
     
11.  Repeat this step for the inner box extrusion.
     
     ![](../images/GUID-D7C1F89D-6FCE-49BA-B924-7292E15C8D8D.png)
     
     Dimension Len B4 is created.
     
     ![](../images/GUID-98FA161B-594F-48ED-88E7-ECA24C51CD2E.png)
     
12.  Right-click Model Parameters, click Edit, and set the equation for LenB3 to LenB4 + Fth (floor thickness). While the Model Parameters dialog box is displayed, set the value for Fth to 4. Len B3 is the length of the Box Outer extrusion which is set to the Box Inner extrusion length plus the thickness of the floor.
13.  Equate dimension LenB4 to SRS-SFH (cover to sump height - frame height). Change the value of SRS to 48.
14.  Equate SBSH to LenB1+LenB3 Click Close. SBSH is the Structure Height parameter.
     
     Note:
     
     This parameter will be visible in Autodesk Civil 3D, but will not be able to be edited by the user. It is important that this parameter does not evaluate to zero. If it does, the part will not display in Autodesk Civil 3D.
     
15.  Make the following changes to Size Parameters.
     
     *   Fth
         *   Change to List
         *   Set List to 4,8,12
         *   Set the value to 4
     *   Wth
         *   Change to List
         *   Set list to 4,8,12
         *   Set the value to 4
     *   Add SIL (Inner Structure Length)
         *   Change to List
         *   Set List to 12 inch increments from 48 to 120
         *   Set value to 120
     *   Add SIW (Inner Structure Width)
         *   Change to List
         *   Set List to 12 inch increments from 36 to 60
         *   Set value to 60
     *   SRS
         *   Change to Range
         *   Set Minimum Value to 36
         *   Set Maximum Value to 120
         *   Set Default Value to 72
     
     Fth = Floor thickness and Wth = Wall thickness. These parameters will be available in three sizes, and will be editable structure properties in Autodesk Civil 3D.
     
     SIL=Inner Structure Length and SIW=Inner Structure Width. These parameters will be available in a number of sizes at 12-inch increments. These are the key structure properties that control the length and width of the part in Autodesk Civil 3D.
     
     RS=cover to Sump height. This is a key structure property that will allow the depth of the structure to be controlled in Autodesk Civil 3D.
     
     Click Save Part Family when you are finished so that the part is saved, and the Model Parameters are updated to include the new Size Parameters.
     
16.  Right-click Size Parameters and then click Edit Values. Change the following:
     
     *   SBSL=120
     *   SBSW=60
     *   SFL=108
     *   SFW=48
     
     Click the Update Model button in the Edit Part Sizes dialog box toolbar, and then click OK.
     
17.  Right-click Model Parameters and then click Edit. Make the following changes in the Equation column for each of the following. Make the changes in the order shown.
     *   LenA1: SIL + (2\*Wth) - LenA1 is the model dimension for the outer length of the box. It has been set to the Inside Structure Length (SIL) plus the thickness of each wall (2 x Wth).
     *   LenA2: SIW + (2\*Wth) - LenA2 is the model dimension for the outer width of the box. It has been set to the Inside Structure Width (SIW) plus the thickness of each wall (2 x Wth).
     *   SBSL: LenA1 - SBSL and SBSW are important structure properties that have been set to the actual model dimensions.
     *   SBSW: LenA2
     *   SVPC: SFH + 6 - The vertical pipe clearance (SVPC) has been set to the frame height (SFH) plus six inches.
18.  Expand Work Planes![](../images/ac.menuaro.gif)Cover![](../images/ac.menuaro.gif)Geometry. Right-click Fixed Center![](../images/ac.menuaro.gif)Visible. The cover center point is displayed in the drawing.
     
     ![](../images/GUID-3DCB18E1-9349-47B4-84EF-1E778645B180.png)
     
19.  Expand Autolayout Data. Right-click Layout Data and then click Set Placement Point. Use the NODE object snap to select the Fixed Center point. The placement point is much like an insertion point for an AutoCAD block.
     
     ![](../images/GUID-FBA95D7E-9437-45B6-9A65-672363010FD4.png)
     
20.  Expand Modifiers. Turn on the visibility of Frame and Box. Turn off all other modifiers.
21.  Click Generate Bitmap.
22.  Click SW Isometric View. Click OK. A bitmap image has been generated for the part catalog.
     
     ![](../images/GUID-6DB80E58-3943-4528-97AE-E8EA9B76313D.png)
     
23.  Click Save Part Family. Exit the Part Builder Environment.

To continue this tutorial, go to [Exercise 5: Using the New Part](GUID-08AA6D0B-9326-4E2A-AE69-2C9D895F1BEC.htm "In this exercise, you will insert the new vault part into a Autodesk Civil 3D pipe network, and investigate how it behaves as a pipe network structure.").

**Parent topic:** [Tutorial: Creating a Vault Structure](GUID-5417170D-9D08-4B57-83A6-FE84173720FA.htm "This tutorial demonstrates how to use Part Builder to create a vault structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the vault.")