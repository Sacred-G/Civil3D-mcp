---
title: "Exercise 2: Defining the Manhole Geometry"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-55B8971D-EDB7-42A1-A5A1-5E23B9F25D18.htm"
category: "tutorial_creating_a_drop_inlet_manhole_structure"
last_updated: "2026-03-17T18:43:12.154Z"
---

                Exercise 2: Defining the Manhole Geometry  

# Exercise 2: Defining the Manhole Geometry

In this exercise, you will define the geometry of the drop inlet manhole by creating a simple schematic of the structure profile. You will build this portion with dimensions that can be modified from within Autodesk Civil 3D when the part is in use.

This exercise continues from [Exercise 1: Defining the New Part in the Structure Catalog](GUID-C76EC023-30AB-4A5D-B5E9-BFE169997E6C.htm "In this exercise, you will begin creating a drop inlet manhole structure in Part Builder by creating a new part chapter, and a new part family within the Structure catalog. You will also configure work planes in the Part Builder parametric modeling environment so that you can proceed with modeling the part in the later exercises.").

1.  Expand Modeling![](../images/ac.menuaro.gif)Work Planes, right-click Vertical Axis![](../images/ac.menuaro.gif)Set View. The current view and UCS is set to match the work plane.
2.  Right-click Vertical Axis, and then click Add Geometry![](../images/ac.menuaro.gif)Line.
    
    Create a line with 5 segments on the work plane. Begin by snapping to the node of the reference point on the cover work plane and Use Ortho to make it easier to draw a straight line. Make the segments about 24 units long. A line geometry object is shown in the drawing. This line represents the vertical axis of the manhole. Each segment represents a component of the structure. Starting from the top, the segments represent the frame, the cone, and the last 3 segments represent the barrel. You will use the extra vertices to place the incoming Dip Tee and the Drop 90° Elbow in the next steps.
    
    ![](../images/GUID-9602DA81-A2E3-4941-98D9-F3ABD2E7E285.png)
    
3.  Use the Add Geometry![](../images/ac.menuaro.gif)Line and Add Geometry![](../images/ac.menuaro.gif)Arc to draw the schematic of the drop assembly. Don’t worry about making the parts perfectly meet. You will use constraints to make the geometry match up properly. Make the two horizontal lines that connect to the vertical line about 36 units long. Next you will establish some constraints to keep the components of the profile in the correct location relative to one another.
    
    ![](../images/GUID-CFC07E64-89C0-418B-B572-4D8E2458DF7F.png)
    
4.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Parallel. Select the bottom line segment of the manhole centerline, and then click the segment directly above it. The bottom two segments are now constrained such that they are parallel to each other.
5.  Repeat the process, working your way up the centerline, constraining adjacent line segments to Parallel. All segments representing the centerline of the structure are constrained to be parallel to one another.
6.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Perpendicular. Select the bottom segment of the structure centerline and the lower horizontal line.
7.  Repeat for the upper horizontal line. The lower and upper horizontal components of the drop pipe are constrained to perpendicular to the structure centerline.
8.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Parallel. Select the bottom line segment of the manhole centerline and then click the vertical segment of the drop pipe. The vertical drop pipe is constrained to parallel to the structure centerline.
9.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Coincident. Click the point at the top of the vertical drop pipe, and then the left end of the upper horizontal line. This positions the rectangle so that its center is located at the fixed point.
    
    ![](../images/GUID-98D48F4E-FC77-4873-88AF-BBFC3CAD7A28.png)
    
10.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Parallel. Select the right upper horizontal segment, and then the left upper horizontal segment. The two upper segments are constrained to parallel.
     
     ![](../images/GUID-6832AE32-FBCD-43B6-A875-3C694CF1BBEF.png)
     
11.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Straight. Select the lower horizontal line, and then the arc. Repeat for the arc and the vertical segment of the drop pipe. The drop pipe bend arc is constrained to be straight with the horizontal and vertical segments of the pipe.
     
     ![](../images/GUID-FA5718CC-7EB6-4E77-A600-88513B70F161.png)
     
12.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Dimension![](../images/ac.menuaro.gif)Distance. Click the bottom and then top points at the ends of the top segment of the centerline. Click a point to set the location of the dimension. A dimension named LenA1 is created for the line segment representing the frame height.
     
     ![](../images/GUID-00DB1A45-C7AB-4574-9790-DCCAE5BC30DF.png)
     
13.  Repeat these steps for each segment of the centerline, starting at the top and ending at the bottom segment. Dimensions named LenA2 through LenA5 are created for the centerline of the structure.
     
     Note:
     
     For this exercise, make sure you dimension the segments in the order shown in the following illustration.
     
     ![](../images/GUID-C12508AF-D4CD-4F21-9E1B-4C74C63157C7.png)
     
14.  Add a Distance dimension to the lower horizontal line by picking the points at each end. This represents the distance from the structure centerline to the start of the elbow. This is used to ensure that the drop stays on the outside of the structure. LenA6 is created.
     
     ![](../images/GUID-29CB64F6-17CB-46BA-AD96-B3E77220063A.png)
     
15.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Dimension![](../images/ac.menuaro.gif)Diameter. Select the arc that represents the elbow. Click a point to set the location of the dimension. BdyD1 is added to the arc.
     
     ![](../images/GUID-D25DAB21-CA60-481E-9050-FBEA5F23F04A.png)
     
16.  Add one final length dimension to the left upper horizontal line segment. LenA7 is added.
     
     ![](../images/GUID-BB38ECD8-1C6C-4208-A7F1-BF2E442B5062.png)
     
17.  Click Save Part Family. The part is saved.
     
     Next you will add Profiles that represent the diameters of the frame, top of cone, barrel, and drop pipe.
     
     First, you’ll create the profile for the frame diameter.
     
18.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Profile![](../images/ac.menuaro.gif)Circular. Click an open area near the top right of the vertical axis to define the center and then click again about 12 units away to define the diameter. A circle profile is drawn.
     
     ![](../images/GUID-7F26689D-F53A-4500-A2B7-F08F615CA22E.png)
     
19.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Dimension![](../images/ac.menuaro.gif)Diameter. Click the circle drawn in the previous step. Click a point to set the location of the dimension. BdyD2 is created for the frame diameter.
     
     ![](../images/GUID-84F77F55-761E-4CAC-B4EC-51CF3C0BE484.png)
     
20.  Expand Vertical Axis. Right-click Circular Profile![](../images/ac.menuaro.gif)Rename. Enter “Frame Cylinder Diameter”. This will make it easier to work with this shape later.
21.  Next, repeat the previous two steps to create and dimension the top of cone profile with a radius of about 18 units, and the barrel profile with a radius of about 24 units. The top of cone profile is drawn and dimensioned with BdyD3. The barrel diameter profile is drawn and dimensioned with BdyD4.
     
     ![](../images/GUID-A8D9D06E-693F-4F33-9AE2-DA411BC7DD9A.png)
     
22.  Next, create and dimension the drop pipe profile. Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Profile![](../images/ac.menuaro.gif)Circular. Click an open area to the left of the upper end of the vertical axis to define the center and then click again about 6 units away to define the diameter.
23.  Add a diameter dimension to the pipe profile. The pipe profile is created and dimensioned with BdyD5.
     
     ![](../images/GUID-C689CB5E-C7E8-4602-9A64-FFA34C0A6DC2.png)
     
24.  Rename the three Circular Profiles to Cone Top Diameter, Barrel Cylinder Diameter, and Drop Pipe Diameter. Renaming the profiles will make them easier to work with later.
25.  Click Save Part Family.
     
     The next exercise continues working on this part.
     

To continue this tutorial, go to [Exercise 3: Creating Profiles and Establishing Parameters](GUID-396FF4D9-E867-41EF-B264-4035F23D00A0.htm "In this exercise, you will extrude the part profiles to create the 3D model. You will then establish the model parameters to control the sizing and dimensions of the manhole.").

**Parent topic:** [Tutorial: Creating a Drop Inlet Manhole Structure](GUID-5EE5F312-EAA0-45EF-AC5C-A8586C9D2F3E.htm "This tutorial demonstrates how to use Part Builder to create a drop inlet manhole structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the manhole.")