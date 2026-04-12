---
title: "Exercise 3: Matching Offsets and Dimensions to Parameters"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-70C8BFFB-EBC8-4272-862A-CDAB162E4B27.htm"
category: "tutorial_creating_a_cylindrical_manhole_structure"
last_updated: "2026-03-17T18:43:10.628Z"
---

                Exercise 3: Matching Offsets and Dimensions to Parameters  

# Exercise 3: Matching Offsets and Dimensions to Parameters

In this exercise, you will match the work plane offsets and diameter dimensions to the parameters. The next step, though, is to create a few more structure parameters.

This exercise continues from [Exercise 2: Defining the Manhole Geometry](GUID-91EEF3F1-D6AC-4001-92FC-65326059829F.htm "In this exercise, you will define the geometry of the manhole by creating a simple schematic of the structure profile. You will build this portion with dimensions that can be modified from within Autodesk Civil 3D when the part is in use.").

1.  Expand Model Parameters and Size Parameters.
    
    Note that there are entries listed for the work plane offsets, structure parameters and body diameter dimensions.
    
2.  Right-click Size Parameters and then click Add. The Edit Part Sizes dialog box is displayed, along with the New Parameter dialog box.
3.  Select Structure Riser Height 1 and press OK. A new parameter of SRZ1 is displayed in the Edit Part Sizes dialog box.
4.  In the Edit Part Sizes dialog box, click New to display the New Parameter dialog box. Add the following additional parameters using the New Parameter dialog box:
    
    *   Structure Riser Height 2
    *   Inner Structure Diameter
    *   Frame Diameter
    *   Frame Height
    *   Cone Height
    *   Slab Thickness
    *   Barrel Height
    *   Barrel Pipe Clearance
    
    Each new parameter is displayed in the Edit Part Sizes dialog box. Click OK.
    
5.  The next step is to assign some preliminary values to the structure parameters. These will be refined later by adding lists of possible structure sizes. Right-click Model Parameters![](../images/ac.menuaro.gif)Edit. The Model Parameters dialog box is displayed.
6.  Note that the structure parameters have all been assigned an initial value of zero. Change the numbers in the Equation column to match the following constants (all in inches):
    
    *   FTh = 15
    *   SBH = 84
    *   SBPC = 72
    *   SBSD = 168
    *   SBSH = 315
    *   SCH = 24
    *   SFD = 24
    *   SFH = 18
    *   SID = 144
    *   SRS = 300
    *   SRZ1 = 8
    *   SRZ2 = 70
    *   SSTh = 12
    *   SVPC = 192
    
    These constants can be changed later to variables, lists, ranges, or tables for further part customization. Keep the Model Parameters dialog box open for the next step.
    
7.  Next, you’ll change each of the work plane offset parameters (WPOf1 through WPOf5) to correspond with a structure parameter. For example, instead of the first offset being approximately 12”, it now corresponds with the SFH parameter. So if you later adjust the SFH parameter, the work plane offset (and its plane geometry, profiles and dimensions) will adjust accordingly.
    
    Select WPOf1, and then select Calculator. The Equation Assistant dialog box is displayed.
    
8.  Select the Variable button to use the pull down to select which structure parameter should match this work plane offset. Click SFH. Press OK to exit the Equation Assistant dialog box. The SFH parameter is displayed under the Equation column in the Model Parameters dialog box.
9.  Repeat the previous two steps for each work plane offset parameter using the following values:
    
    *   WPOf2 = SRZ1
    *   WPOf3 = SCH
    *   WPOf4 = SRZ2
    *   WPOf5 = SBSH
    
    Remain in the Model Parameters dialog box for the next step. The following parameters are displayed under the equation column in the Model Parameters dialog box:
    
    WPOf1
    
    8.0000
    
    SFH
    
    Workplane Offset 1
    
    WPOf2
    
    8.0000
    
    SRZ1
    
    Workplane Offset 2
    
    WPOf3
    
    24.0000
    
    SCH
    
    Workplane Offset 3
    
    WPOf4
    
    120.0000
    
    SRZ2
    
    Workplane Offset 4
    
    WPOf5
    
    172.0000
    
    SBSH
    
    Workplane Offset 5
    
10.  Next you map each body diameter dimension to a corresponding structure diameter parameter. Using the calculator tool in the Model Parameters dialog box, set the equation column for each body diameter parameter making the following matches:
     
     *   BdyD1 = SFD
     *   BdyD2 = SFD
     *   BdyD3 = SFD
     *   BdyD4 = 2\*SFD
     *   BdyD5 = 2\*SFD
     *   BdyD6 = SID
     *   BdyD7 = SID
     
     The following parameters are displayed under the equation column in the Model Parameters dialog box:
     
     Bdy01
     
     24.0000
     
     SFD
     
     Body Diameter 1
     
     Bdy02
     
     24.0000
     
     SFD
     
     Body Diameter 2
     
     Bdy03
     
     24.0000
     
     SFD
     
     Body Diameter 3
     
     Bdy04
     
     48.0000
     
     2\*SFD
     
     Body Diameter 4
     
     Bdy05
     
     48.0000
     
     2\*SFD
     
     Body Diameter 5
     
     Bdy06
     
     144.0000
     
     SID
     
     Body Diameter 6
     
     Bdy07
     
     144.0000
     
     SID
     
     Body Diameter 7
     
11.  Click Close to exit the Model Parameters dialog box. Save the part. The part updates to reflect any dimension changes.
     
     If desired, change your view to isometric, and your visual style to conceptual, to see how the part is progressing.
     
12.  Change the Autolayout location to the center of the cover circular profile. This ensures that the structure is inserted properly into your drawing. Select Layout Data, right-click and then click Select Placement Point. The command line prompts you to Select a Placement Point.
13.  Use your node OSNAP to select the point at the center of the cover work plane circular profile. A small, cyan marker is displayed at that location. This marks the structure insertion point.
14.  Save the part.

To continue this tutorial, go to [Exercise 4: Verifying the New Part](GUID-9E457FD2-49EE-4BB3-BB36-4D2DB034E61A.htm "In this exercise, you will verify that the new part reacts as expected in a drawing by opening a drawing, regenerating the structure catalog, and accessing the new part from a Part List.").

**Parent topic:** [Tutorial: Creating a Cylindrical Manhole Structure](GUID-3C390412-421F-4B68-A826-EA47FE49E7F9.htm "This tutorial demonstrates how to use Part Builder to create a cylindrical-shaped storm drainage manhole structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the manhole.")