---
title: "Exercise 3: Creating Profiles and Establishing Parameters"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-396FF4D9-E867-41EF-B264-4035F23D00A0.htm"
category: "tutorial_creating_a_drop_inlet_manhole_structure"
last_updated: "2026-03-17T18:43:12.297Z"
---

                Exercise 3: Creating Profiles and Establishing Parameters  

# Exercise 3: Creating Profiles and Establishing Parameters

In this exercise, you will extrude the part profiles to create the 3D model. You will then establish the model parameters to control the sizing and dimensions of the manhole.

This exercise continues from [Exercise 2: Defining the Manhole Geometry](GUID-55B8971D-EDB7-42A1-A5A1-5E23B9F25D18.htm "In this exercise, you will define the geometry of the drop inlet manhole by creating a simple schematic of the structure profile. You will build this portion with dimensions that can be modified from within Autodesk Civil 3D when the part is in use.").

1.  Right-click Modifiers![](../images/ac.menuaro.gif)Add Path. When prompted for path, select the top line segment (the segment dimensioned LenA1), then select the Frame Cylinder Diameter profile (the circle dimensioned BdyD2) for the start profile, and select it again for the end profile. The Frame Cylinder Diameter profile is applied along the path (length) of the top line segment.
    
    ![](../images/GUID-580443F4-3E11-472E-8747-F5F6EB73E04F.png)
    
2.  Change the view to SE Isometric to get a better view of the Path. Change the view back to Right.
    
    ![](../images/GUID-7BA9B990-79BA-4F5E-885D-9B29A357C0CD.png)
    
3.  Repeat the Add Path command for the cone segment. For the start profile, select the Cone Top Diameter profile (dimensioned with BdyD3). For the end profile, select the Barrel Cylinder Diameter profile (dimensioned with BdyD4).
    
    ![](../images/GUID-34A6DC29-9DC5-4C71-A357-EE79BA955727.png)
    
4.  Repeat the Add Path command for the remaining three line segments. Use the Barrel Cylinder Diameter profile for both the start and end profiles for each of the three segments. Change your view to SE Isometric to see the part in 3D. Change view back to Right.
    
    ![](../images/GUID-33D9B63F-2EED-4EDD-B978-2BEC55084A9D.png)
    
5.  Next, you’ll add paths for the drop pipe assembly. Right-click Modifiers![](../images/ac.menuaro.gif)Add Path. Select the lower horizontal segment for the path and the Drop Pipe Diameter profile for the start and end profiles.
    
    ![](../images/GUID-1615A566-24CB-489B-B05B-2DECF1BE38F9.png)
    
6.  Add Path for the curved elbow. When the Enter Number of Path Segments dialog box is displayed, verify that “Do not segment path” is checked, and then click OK. Then add the path modifier for the vertical segment of the drop pipe. Use the Drop Pipe Diameter profile.
    
    ![](../images/GUID-D65669DB-BD5C-4CF8-B795-0C931251791E.png)
    
7.  Finish the drop pipe assembly by adding paths for the upper segments. Start with the right upper segment. Switch to SE isometric view.
    
    ![](../images/GUID-127D0D03-DCDB-47E4-9310-6E687E4694CD.png)
    
8.  Switch back to Right view.
    
    Next, you will merge the structure components with the drop pipe assembly components. Right-click Modifiers![](../images/ac.menuaro.gif)Boolean Add. When prompted to select objects, select the bottom two barrel segments, and the lower horizontal pipe segment, and then press Enter. The parts are merged.
    
    ![](../images/GUID-C1AFE06D-36E6-48E8-BEB2-763E1F00FA13.png)
    
    ![](../images/GUID-3F4952D9-E4F4-40BB-A49B-94EAD17E438B.png)
    
9.  Right-click Modifiers![](../images/ac.menuaro.gif)Boolean Add. When prompted to select objects, select the top two barrel segments, and the upper two horizontal pipe segments, and then press Enter.
    
    ![](../images/GUID-CF4B32B7-DAF2-4EDF-89EB-B2EDA1DC877B.png)
    
10.  Right-click Modifiers![](../images/ac.menuaro.gif)Boolean Add. When prompted to select objects, select the upper horizontal pipe segment, and the vertical pipe segment, and then press Enter.
     
     ![](../images/GUID-5FAEA191-0C07-459F-8D24-E66E7EDCF5FA.png)
     
11.  Switch the view to SE Isometric.
     
     ![](../images/GUID-C3347BEB-F92B-4CAC-9786-5BF87101C249.png)
     
12.  Next you will set the placement point for the part.
     
     Add new Top workplane; Add Point reference; Select placement point on Top workplane Expand AutoLayout Data. Right-click Layout Data, and then click Set Placement Point. Use the node object snap to click the top point on the vertical axis. This is the point at which the part is inserted into the drawing (insertion point).
     
     ![](../images/GUID-40C95EBF-DD8A-4946-9741-A7D7A18B3A4F.png)
     
13.  Save the part. Click Save Part Family.
14.  Right-click Size Parameters![](../images/ac.menuaro.gif)Add![](../images/ac.menuaro.gif)Edit Configuration.
15.  Click New. Add the following Parameters clicking New for each:
     
     *   Barrel Height
     *   Frame Height (SFH)
     *   Frame Diameter (SFD)
     *   Cone Height (SCH)
     *   Inner Structure Diameter (SID)
     *   Barrel Pipe Clearance (SBPC)
     
     Click OK when all of the parameters have been added.
     
16.  Click Save Part Family. Right-click Model Parameters, and then click Edit. Double-click in the Equation column next to each of the following, and set the Equation for each parameters as shown below:
     
     *   FTh to 6
     *   WTh to 4
     *   SBH to 74
     *   FTh to 6
     *   SBPC to 3
     *   SBSD to 48
     *   SBSH to 108
     *   SCH to 24
     *   SFD to 24
     *   SFH to 4
     *   SID to 48
     *   SRS to 102
     *   SVPC to 36
     *   WTh to 4
     
     Saving the part causes the Model Parameters to update, including the new Size Parameters.
     
17.  Right-click Size Parameters, and then click Edit Configuration. The Edit Part Size dialog box is displayed.
18.  Locate the SRS column, click Constant, and change it to Range. The SRS parameter is now formatted as a range of values. Click the drop-down arrow button next to Parameter Configuration, and select Values. The view is changed to show the value of each parameter. Click the cell in the SRS column, and then click the Edit button on the Edit Part Sizes dialog box toolbar. The Edit Values dialog box is displayed. Set the Minimum to 36, the Maximum to 12000, the Default to 120, and then click OK.
19.  Right-click Model Parameters, and then click Edit. Edit the Equations and Descriptions for the BdyD# parameters as shown in the following table:
     
     BdyD1
     
     12
     
     Elbow Bend Diameter
     
     BdyD2
     
     SFD
     
     Frame Cylinder Diameter
     
     BdyD3
     
     SFD+(2\*Wth)
     
     Cone Top Diameter
     
     BdyD4
     
     SID+(2\*Wth)
     
     Barrel Cylinder Diameter
     
     BdyD5
     
     12
     
     Incoming Pipe Diameter
     
20.  Make the following additional edits:
     
     SBH
     
     SRS-SFH-SCH
     
     SBPC
     
     3.0
     
     SBSD
     
     SID+(2\*Wth)
     
     SBSH
     
     SRS+FTh
     
21.  Edit the Equations and Descriptions for the LenA# parameters as shown in the following table:
     
     LenA1
     
     SFH
     
     Frame Cylinder Height
     
     LenA2
     
     SCH
     
     Cone Cylinder Height
     
     LenA3
     
     SVPC-SFH-SCH+(BdyD5/2)
     
     Top Pipe CL
     
     LenA4
     
     SRS+FTh-SFH-SCH-LenA3-LenA5
     
     Top Pipe CL to Bottom Pipe CL
     
     LenA5
     
     24
     
     Bottom Pipe CL to Struct Bottom
     
     LenA6
     
     (SID/2)+WTh+(BdyD5/2+2)
     
     CL Vert Pipe to Incoming Pipe
     
     Click Close. Change to SE Isometric view and note the changes to the geometry of the structure.
     
22.  Change the 4 circular profiles so they are not visible by right-clicking on each and then clicking Visible.
     
     The circular profiles are no longer visible.
     
23.  Repeat the previous step for all of the Dimensions.
24.  Change view to SE Isometric and examine the part. It should look like the following image.
     
     ![](../images/GUID-AF5C49C2-22A4-469C-BF2D-E62F651BF1F4.png)
     
     Click Save Part Family. Close out of the Part Builder Environment, and then re-open the Part.
     
     To exit the Part Builder utility, click the small X in the upper right corner of the Part Browser. (The Part Browser is the left pane portion of the Part Builder application window.) If you are prompted to save the part, click Yes. Part Builder closes.
     
     After all the changes that have been made, now is a good time to close out and re-open the part so that all of the data is freshly loaded into the part builder environment.
     
25.  Right-click Size Parameters, and then click Edit Configuration.
26.  Change the Data Storage type to List for the following parameters:
     *   WTh
     *   FTh
     *   SFH
     *   SFD
     *   SCH
     *   SID
27.  Click the drop-down arrow button next to Parameter Configuration, and select Values.
28.  Select the 4.000 in the WTh column, click the Edit button from the Edit Part Sizes dialog box toolbar, and add the following values: 4.0, 6.0. Click OK.
29.  Repeat the previous step for each of the following parameters:
     *   FTh: 6.0, 8.0, 12.0
     *   SFH: 4.0, 6.0, 8.0
     *   SFD: 24.0, 36.0
     *   SCH: 24.0, 36.0
     *   SID: 48.0, 60.0, 72.0
30.  Click the drop-down arrow button next to Values, and select Calculations.
31.  Double click in the cell in the PrtSN to open the Calculation Assistant.
32.  Change the Precision to 0.
33.  Click in the text box, right after PrtD, and press the space bar.
34.  From the Insert Variable list, select FTh and then click Insert. The FTh variable is inserted into the Part size name.
35.  Enter “in Floor” after the FTh variable. (Exclude the quotation marks.)
36.  Repeat the previous steps, adding variables and text for SCH, SFD, SFH, SID, WTh. The complete string should look something like this:
     
     PrtD FTh in Floor SCH in Cone Hgt SFD in Frame Dia SFH in Frame Hgt SID in Barrel Dia WTh in Wall.
     
37.  Click Evaluate to see the resultant part name.
     
     Note that the name is long and partially cut off on the right. Click the name and use your keyboard arrow keys to see the rest of the name. This is a required step to ensure that each part has a unique part name when added to the part list.
     
38.  Click OK twice to close all dialog boxes.
39.  Save the Part. Switch Visual Style to Conceptual. The part should look like the following:
     
     ![](../images/GUID-D5B33013-627D-43F7-9396-ACCF8BAFBEE1.png)
     
40.  Right-click Model Parameters, and then click Edit.
41.  Double click the Equation for SVPC and enter: SFH+SCH+SBPC. Change the Visual Style to 2D Wireframe. Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Geometry![](../images/ac.menuaro.gif)Point Reference.
42.  Click the point in the center of the cover work plane.
     
     ![](../images/GUID-877C96CA-3446-4202-BDB8-913075C5D2AD.png)
     
     A reference point (green) is created where the two planes meet in line with the vertical axis of the structure.
     
     ![](../images/GUID-0455C777-4B05-4805-A694-6C1C69B71774.png)
     
43.  Right-click Vertical Axis![](../images/ac.menuaro.gif)Add Constraints![](../images/ac.menuaro.gif)Coincident. Click the top point of the vertical axis, then click the reference point created in the previous step. The entire structure moves upward so that the cover level matches the top work plane.
     
     ![](../images/GUID-C7F03893-0FBA-444C-8C3E-0F27472ED321.png)
     
44.  Click Generate Bitmap.
45.  Click SE Isometric View.
46.  Click OK. A bitmap image is generated. This is the image of the part that is displayed when viewing the part in the part catalog. It is not used not when viewing the part in a drawing.
     
     ![](../images/GUID-C7F03893-0FBA-444C-8C3E-0F27472ED321.png)
     
47.  Click Save Part Family.
48.  Exit the Part Builder utility by clicking the small X in the upper right corner of the Part Browser. (The Part Browser is the left pane portion of the Part Builder application window.) If you are prompted to save the part, click Yes. Part Builder closes.

You can open _Part Builder-2.dwg_ in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79) to see what this finished part looks like in a drawing.

**Parent topic:** [Tutorial: Creating a Drop Inlet Manhole Structure](GUID-5EE5F312-EAA0-45EF-AC5C-A8586C9D2F3E.htm "This tutorial demonstrates how to use Part Builder to create a drop inlet manhole structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the manhole.")