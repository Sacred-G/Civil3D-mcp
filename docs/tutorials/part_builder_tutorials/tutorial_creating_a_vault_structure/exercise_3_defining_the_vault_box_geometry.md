---
title: "Exercise 3: Defining the Vault Box Geometry"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-65B91E08-D951-4341-A184-DD75E30508AF.htm"
category: "tutorial_creating_a_vault_structure"
last_updated: "2026-03-17T18:43:13.551Z"
---

                Exercise 3: Defining the Vault Box Geometry  

# Exercise 3: Defining the Vault Box Geometry

In this exercise, you will build the box portion of the vault. You will use projection geometry and constraints to link the box to the frame so that a single set of dimensions can control both.

This exercise continues from [Exercise 2: Defining the Vault Top Section Geometry](GUID-5013FCFB-AD45-4399-9926-19040957C197.htm "In this exercise, you will build the top portion of the vault. This is a rectangular frame with a rectangular opening. You will build this portion with dimensions that can be modified from within Autodesk Civil 3D when the part is in use.").

1.  Continue from the previous exercise in the Part Builder environment. Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)SW Isometric. Next you will create a reference work plane at the bottom of the frame. This view makes it easier to perform the next few steps.
    
    ![](../images/GUID-2D3041F8-3FFE-44CF-841D-F5198E432A2B.png)
    
2.  In the Content Builder window, right-click Work Planes, and then click Add Work Plane. The Create Work Plane dialog box is displayed.
3.  Click Reference then enter Top of Box for Name. Click OK. You are prompted for a modifier.
4.  When prompted for modifier, select the 3D frame object.
    
    ![](../images/GUID-74B7C093-07AE-4B72-8D9D-0B33FE851B40.png)
    
    A new yellow rectangle is displayed representing the reference plane at the bottom of the frame (also top of box). This reference plane is attached to the modifier and moves if the frame thickness is adjusted.
    
    ![](../images/GUID-01FFD283-0095-4CA2-851C-68345E803784.png)
    
    When prompted for the work plane, select the yellow rectangle at the top of the frame object (this represents the top plane).
    
    ![](../images/GUID-B0531030-9E8A-43A9-BC96-B8BC12609D17.png)
    
    When prompted for the reference work plane, click along the lower edge of the frame object.
    
    ![](../images/GUID-3EEE3C52-B782-49C3-BA0B-9D0AE6C44EC5.png)
    
5.  Right-click Top of Box and then click Set View. Next, click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)SW Isometric.
    
    This sets the working plane to Top of Box and then returns the drawing to its original view.
    
6.  Right-click Top of Box![](../images/ac.menuaro.gif)Add Geometry![](../images/ac.menuaro.gif)Projected Geometry. You are prompted for a modifier.
7.  Click the 3D frame object. When prompted for the geometry to project, click one of the lower edges. The line highlights in red when the cursor is in the correct position. Repeat this process for the three remaining lower edges.
    
    ![](../images/GUID-117C944A-C800-4619-81EF-3420E2697159.png)
    
    This creates geometry that is linked to the lower edge of the frame extrusion. This is a key relationship in building the box section below the frame so that it is aligned with the frame. The projected geometry is displayed in green.
    
    ![](../images/GUID-EABE1479-16AA-4F96-B8A8-0F9C18E7D2CF.png)
    
8.  Set the visibility of all geometry and profiles in the cover work plane to off. Right-click Top of Box and then click Set View. Objects in the cover work plane are turned off so that you do not snap to them or use them inadvertently.
9.  Right-click Top of Box![](../images/ac.menuaro.gif)Add Profile![](../images/ac.menuaro.gif)Rectangular. You are prompted for rectangle points.
10.  Click two points to create a rectangle that is outside of the current geometry. In Content Builder, change the name Rectangular Profile to Outer Wall. A rectangular profile is drawn.
     
     ![](../images/GUID-DDD71493-702C-4908-85CD-2F9E5F0B7BC8.png)
     
11.  Draw a second rectangular profile within the first. Change its name to Inner Wall. There are now two rectangular profiles.
     
     ![](../images/GUID-0EF96F51-42E9-4D89-9327-270D2AF92E08.png)
     
12.  Right-click Top of Box![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Equal Distance. Click the left side of the outer rectangle, then the left side of the inner rectangle. Then click the top side of the outer rectangle and the top side of the inner rectangle. The distance between the outer wall and inner wall on the left side is set to always match the distance between the outer wall and inner wall on the top side.
     
     ![](../images/GUID-C445114A-F022-426C-A10F-80016B447CC5.png)
     
13.  Repeat this procedure, first clicking the top pair of lines then clicking the right pair. Continue around the rectangle in a clockwise direction, finishing up by setting the right side equal to the bottom side.
     
     ![](../images/GUID-A7255EAD-FA2D-4B9B-84B7-7B88F77658F5.png)
     
     All sides are now equal. With these constraints in place, you can change the thickness of one side and the changes affect all sides.
     
     ![](../images/GUID-827BBA35-146A-40D8-B297-B5795D439B55.png)
     
14.  Right-click Top of Box![](../images/ac.menuaro.gif)Add Dimension![](../images/ac.menuaro.gif)Perpendicular Distance. Click a line on the inner rectangle then the corresponding parallel line on the outer rectangle. Click either of the lines once again to set a perpendicular reference object. Pick a point between the two lines for the dimension position and enter 4 for the dimension value. A new dimension named LenA5 is created. This single dimension sets the wall thickness of the box. Because of the equal distance constraints established in the step above, this dimension controls all four sides.
     
     ![](../images/GUID-C4D1E8A6-C32B-42F5-804C-204FAF309341.png)
     
15.  Right-click Top of Box![](../images/ac.menuaro.gif)Add Constraint![](../images/ac.menuaro.gif)Coincident. Click the black point at the top left corner of the outer box, then click the green projected point at the top left corner of the frame.
     
     ![](../images/GUID-61452935-3B2E-4EB5-B192-10105B2BD411.png)
     
     The black point is moved to be coincident with the green point. Because of constraints, the top and left sides of both rectangles are moved and the 4" distance between the inner and outer walls is maintained.
     
     ![](../images/GUID-673B1ACE-9506-44C6-B2C1-C3A3E7C9C6C3.png)
     
16.  Repeat this procedure for all four corners. You may need to use Shift+Space to select the green point. The Top of Box geometry is linked to the green projected geometry, which is linked to the extrusion modifier projected from the cover work plane. With these relationships, the geometry of the entire vault can be controlled with a few parameters.
17.  Right-click Model Parameter and then click Edit. Change the Equation for LenA5 to Wth. Change the value for Wth to 4. Wall thickness (Wth) is one of the size parameters that is built in to this part type.
18.  Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)SW Isometric. Right-click Modifiers and then click Add Extrusion. You are prompted to select a profile.
19.  Click the outer rectangle. Enter 48 for Distance and check the box next to Flip. Click OK.
     
     ![](../images/GUID-4631AEF6-E9AC-48AD-820F-0C2D0F75444B.png)
     
     An extrusion is created that extends downward 48 inches.
     
     ![](../images/GUID-3266E45A-4F4C-4C3E-BF00-D0753DB93526.png)
     
20.  Change the name of the new extrusion modifier to Box Outside. Repeat these steps for the inner rectangle using a distance of 44 and a name of Box Inside.
     
     ![](../images/GUID-21018A13-0BD0-4727-A6CC-706BEB6F4132.png)
     
     The inner extrusion stops 4 inches shy of the outer extrusion, creating a 4-inch thick floor.
     
     ![](../images/GUID-8BE904F4-CBD0-4E5C-BF58-4B29A92C01C6.png)
     
21.  Right-click Modifiers and then click Add Boolean Subtract. Click the outer extrusion, then the inner extrusion. Press Enter. Name the new modifier Box.
22.  Right-click Size Parameters and then click Edit Values. Change to the following values:
     
     *   SBSL = 84
     *   SBSW = 48
     *   SFL = 72
     *   SFW = 36
     
     Click the Update Model button in the Edit Part Sizes dialog box toolbar, and then click OK. Note the change to the model. The model updates according to the size parameter changes.
     
     ![](../images/GUID-6E4A95BC-207E-40B2-A4B3-B041645A6B51.png)
     
23.  Click Save Part Family. Stay in the Part Builder environment for the next exercise.

To continue this tutorial, go to [Exercise 4: Finalizing the Part](GUID-F1C850F2-E145-4C0E-BEFE-8F713F56EECC.htm "In this exercise, you will add the final model and size parameters that will allow the part geometry to be modified in Autodesk Civil 3D.").

**Parent topic:** [Tutorial: Creating a Vault Structure](GUID-5417170D-9D08-4B57-83A6-FE84173720FA.htm "This tutorial demonstrates how to use Part Builder to create a vault structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the vault.")