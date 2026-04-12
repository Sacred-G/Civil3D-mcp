---
title: "Exercise 2: Defining the Manhole Geometry"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-91EEF3F1-D6AC-4001-92FC-65326059829F.htm"
category: "tutorial_creating_a_cylindrical_manhole_structure"
last_updated: "2026-03-17T18:43:10.484Z"
---

                Exercise 2: Defining the Manhole Geometry  

# Exercise 2: Defining the Manhole Geometry

In this exercise, you will define the geometry of the manhole by creating a simple schematic of the structure profile. You will build this portion with dimensions that can be modified from within Autodesk Civil 3D when the part is in use.

This exercise continues from [Exercise 1: Defining the New Part in the Structure Catalog](GUID-B8A5FAA1-8946-457F-A02A-4DF5B1FC23F3.htm "In this exercise, you will begin creating a cylindrical-shaped manhole structure in Part Builder by creating a new part chapter, and a new part family within the Structure catalog. You will also configure work planes in the Part Builder parametric modeling environment so that you can proceed with modeling the part in the subsequent exercises.").

1.  Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)Front.
    
    The view of your Part Builder drawing area changes so that you see the cover work plane from a front view.
    
2.  Expand Modeling. Right-click Work Planes and then click Add Work Plane. The Create Work Plane dialog box is displayed.
3.  Click Offset. Name the work plane Top of Riser 1 then click OK.
4.  The command line asks you to Select reference work plane. Click the yellow bounding square of the cover work plane.
    
    The command line asks you to Select Offset from work plane. Turn on ORTHO. Select a location approximately 12” lower than the cover work plane in the negative Z direction, and press enter. In later steps, you will establish a more precise offset by equating these distances with structure parameters. A second work plane is displayed approximately 12” below the first work plane.
    
    ![](../images/GUID-F212C4B9-8288-415E-A863-6CCBD1AA7763.png)
    
5.  Repeat Steps 2 through 4 to create the following additional work planes, using the following approximate offsets from the referenced work plane:
    
    *   **Name**: Top of Cone, **Offset**: 12" below 'Top of Riser 1'
    *   **Name**: Top of Riser 2, **Offset**: 24" below 'Top of Cone'
    *   **Name**: Top of Barrel, **Offset**: 85" below 'Top of Riser 2'
    *   **Name**: Bottom of Structure, **Offset**: 200" below 'Top of Barrel'
    
    The new work planes are displayed. The new parameters are displayed under Model Parameters showing you the work plane offsets.
    
6.  Change your view to be oriented above the cover work plane. Expand Work Planes, right-click cover and then click Set View. The view is oriented to be above the cover work plane.
7.  Add a circular profile to represent the diameter of the frame. Right-click the Rim work plane ![](../images/ac.menuaro.gif)Add Profile![](../images/ac.menuaro.gif)Circular. At the command prompt, select a center point for the profile at the approximate center of the work plane and a radius of 12”, similar to how you would draw an AutoCAD circle. A circular profile is displayed, and an entry for a circular profile is displayed under the cover work plane.
8.  Expand Work Planes, right-click Cover![](../images/ac.menuaro.gif)Add Geometry![](../images/ac.menuaro.gif)Point. Place a point at the approximate center of your circular profile. A point is displayed at the approximate center of the circular profile.
    
    ![](../images/GUID-CA6F0A4A-E62D-4961-AD67-FAE2D66ED8A4.png)
    
9.  Constrain the profile so that the point will be forced to always be located at the center of the profile. Right-click the Cover work plane ![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Concentric. The command line prompts you to select a first geometry and a second geometry. Select the point and the circular profile. The point moves to be at the center of the circular profile, and a concentric constraint is displayed under the cover work plane.
10.  Add a diameter dimension that you can later use as a structure parameter. Right-click the Cover work plane ![](../images/ac.menuaro.gif)Add Dimension![](../images/ac.menuaro.gif)Diameter. The command line prompts you to select circle or arc geometry. Select the circular profile on the screen. The command line prompts you to select a dimension position. Select a location on your screen that is close to the profile, but out of your way. A diameter dimension is displayed on screen as well as under your Model Parameters. The actual drawn diameter and dimension value are unimportant at this time.
11.  Draw a circular profile on the Top of Riser 1 work plane. Right-click the Top of Riser 1 work plane, and then click Add Profile![](../images/ac.menuaro.gif)Circular. Following the same procedure as Step 7, draw a circular profile. You can use your center OSNAP to ensure that the center of this profile matches the center of the profile from the cover work plane. Set the radius to be just slightly larger than the cover work plane circular profile to ease selection of this profile. The actual radius is not important at this time. A circular profile is displayed.
12.  Add a diameter dimension to the circular profile on the Top of Riser 1 work plane. Right-click the Top of Riser 1 work plane, and then click Add Dimension![](../images/ac.menuaro.gif)Diameter. Follow the same procedure as step 10 to place the dimension. A diameter dimension is displayed for your second circular profile.
13.  Repeat the previous two steps to create a circular profile and corresponding dimension on the Top of Cone work plane. You have three circular profiles with appropriate dimensions. Your dimensions may look different than the following illustration.
14.  Repeat steps 11 and 12 to create a circular profile and corresponding dimension for the Top of Riser 2 work plane; however, this time, make the profile radius approximately twice as large (approximately 24” radius/48” diameter). You should have four circular profiles displayed, and four corresponding dimensions displayed in the modeling area, and under Model Parameters node.
     
     ![](../images/GUID-8A20DBB2-ECDF-41AE-BAB8-1BFC423AA441.png)
     
15.  Click View tab ![](../images/ac.menuaro.gif)Views panel ![](../images/ac.menuaro.gif)Front. Four profiles from front view are displayed.
     
     ![](../images/GUID-CA62AB2E-62A4-4D49-8790-95F04B517565.png)
     
16.  Add a transition between the cover circular profile and the Top of Riser 1 circular profile. This represents the manhole cover and frame. Right-click Modifiers and then click Add Transition. The command line prompts you to Select Start Profile. On the screen, select the circular profile on the cover work plane. The command line prompts you to Select End Profile. On the screen, select the circular profile on the Top of Riser 1 work plane. A transition is displayed similar to the following illustration:
     
     ![](../images/GUID-88EE8AB0-3B97-42F4-9461-4CFD7B5ACC30.png)
     
17.  Right-click the transition on the screen, and then click Display Order![](../images/ac.menuaro.gif)Send to Back. Add a transition between the Top of Riser 1 circular profile and the Top of Cone circular profile. This represents the manhole cover and frame. Right-click Modifiers and then click Add Transition. The command line prompts you to Select Start Profile. On the screen, select the circular profile on the Top of Riser 1 work plane. The command line prompts you to Select End Profile. On the screen, select the circular profile on the Top of Cone work plane.
     
     Repeat the process to create a transition between the circular profiles on the Top of Cone work plane and the Top of Riser 2 work plane. Remember that you may have to use Display Order![](../images/ac.menuaro.gif)Send to Back to send the transitions to the back, so that you can choose the profiles. To do this, right-click the transition on the screen, and then click Display Order![](../images/ac.menuaro.gif)Send to Back. Transitions are displayed similar to the following illustration:
     
     ![](../images/GUID-E3FC8B2A-87D8-4DF4-AF03-127A794D3A3B.png)
     
     These transitions stay dynamic to profiles used to create them, including adjustments to work plane offsets, and diameter dimensions.
     
18.  Switch your view back to an overhead view by selecting the Top of Barrel work plane, right-clicking and choosing Set View. Your view switches to an overhead view in relation to this work plane.
19.  Make two circular profiles with corresponding dimensions on the Top of Barrel work plane. Use steps 11 and 12 for reference. The first should be approximately the same size as the profile on the Top of Riser 2 work plane (24”R/48”D), the second should be approximately twice as large (48”R/ 96”D). Two circular profiles are displayed on your screen. Two corresponding dimensions are displayed both on screen and under Model Parameters.
20.  Make one circular profile with a corresponding dimension on the Bottom of Structure work plane. Use steps 11 and 12 for reference. The profile should be approximately the same size as the larger profile on the Top of Barrel work plane (48”R/ 96”D). You should now have seven (7) total circular profiles and corresponding dimensions both on screen and under Model Parameters. Remember that these diameters are approximate at this point. The ones you created may not look exactly like the ones displayed in this tutorial.
     
     ![](../images/GUID-C8AA8040-95EF-4345-8D3D-CD4358EFAE64.png)
     
21.  Change your view to a Front view. The stack of work planes and the transitions created in step 17 are displayed.
22.  Following the same methodology for the first batch of transitions created in steps 16 and 17, add transitions between the Top of Riser 2 Profile, and the smaller Top of Barrel profile, then the larger Top of Barrel profile and the Bottom of Structure profile. (You may have to use Draw Order to send certain profiles to the back, in order to choose the correct one for each transition.) The model you created should now look similar to the following illustration:
     
     ![](../images/GUID-5E33BE86-FC06-4D38-86D5-4ED20F9DADD1.png)
     
23.  Change your view to SW isometric.
     
     ![](../images/GUID-0C4D88B3-3793-4392-82F9-EE0ECD2D17FA.png)
     
24.  Change your visual style to Conceptual.
     
     ![](../images/GUID-BB0067DB-D422-495E-9920-05785DC4A5AA.png)
     
25.  Change your visual style back to 2D wireframe, then change your view back to Top view.
     
     ![](../images/GUID-0B9A3610-4027-41BD-B5A3-88D82B8AE13D.png)
     

To continue this tutorial, go to [Exercise 3: Matching Offsets and Dimensions to Parameters](GUID-70C8BFFB-EBC8-4272-862A-CDAB162E4B27.htm "In this exercise, you will match the work plane offsets and diameter dimensions to the parameters. The next step, though, is to create a few more structure parameters.").

**Parent topic:** [Tutorial: Creating a Cylindrical Manhole Structure](GUID-3C390412-421F-4B68-A826-EA47FE49E7F9.htm "This tutorial demonstrates how to use Part Builder to create a cylindrical-shaped storm drainage manhole structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the manhole.")