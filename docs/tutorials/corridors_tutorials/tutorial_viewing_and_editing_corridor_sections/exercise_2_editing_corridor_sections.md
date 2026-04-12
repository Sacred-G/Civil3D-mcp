---
title: "Exercise 2: Editing Corridor Sections"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-B92D793D-947C-4E04-A6C2-3C0F0565493A.htm"
category: "tutorial_viewing_and_editing_corridor_sections"
last_updated: "2026-03-17T18:42:52.123Z"
---

                 Exercise 2: Editing Corridor Sections  

# Exercise 2: Editing Corridor Sections

In this exercise, you will edit the parameters at several corridor sections.

You will edit a section in two ways. First, you will modify a subassembly parameter at a single chainage, which will override the subassembly settings for that chainage only. Second, you will modify a subassembly parameter, and then apply the modification to a range of stations.

This exercise continues from [Exercise 1: Viewing Corridor Sections](GUID-8CD5788B-54A7-4DCA-98E6-CA6494E1ACA0.htm "In this exercise, you will view how a corridor assembly is applied at various chainages along a baseline alignment.").

Modify subassembly properties for a single chainage

Note:

This exercise uses _Corridor-4a.dwg_ from the previous exercise.

2.  On the Chainage Selection panel, in the Select A Chainage list, select **7+75.00**.
3.  On the View Tools panel, click Zoom To Subassembly ![](../images/GUID-6C9A2784-901C-4909-AB4D-67A92467C509.png) Find.
4.  In the Pick Subassembly dialog box, select **Lane (Right)**. Click OK.
5.  On the Corridor Edit Tools panel, toolbar, click Parameter Editor ![](../images/GUID-7A44959C-35F9-4B39-BB2E-8D13E1AEB0C4.png).
6.  In the Corridor Parameters dialog box, in the Assembly - (1) tree, under Group - (1), expand **Lane (Right)**.
    
    Notice that identical values are displayed in the Design Value and Value columns. The Design Value column displays the value that was specified when the subassembly was added to the assembly. The Value column displays the actual value of the subassembly at the current chainage. In the following steps, you will override the Design Value at the current chainage, and then examine the results.
    
7.  Change the Width Value to **36.0000’**.
    
    Notice that the Override check box is automatically selected, which indicates that the Design Value has been overridden at this chainage.
    
8.  Click Go To Next Chainage ![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png) Find several times.
    
    Notice that for the other chainages, the WidthValue is 12.000’. The lane subassembly that is displayed in the section view updates in width to reflect the width at the current chainage.
    
9.  On the Chainage Selection panel, in the Select A Chainage list, select **7+75.00**.
10.  In the Corridor Parameters dialog box, in the Assembly - (1) tree, under Group - (1), under **Lane (Right)**, in the Width row, clear the Override check box.
     
     The Value column displays the same value as the Design Value column.
     

Modify subassembly properties for a range of stations

1.  On the View Tools panel, click Zoom To Extents ![](../images/GUID-F5EFC90C-16E9-4EB1-9C39-7D9713025534.png) Find.
2.  On the Chainage Selection panel, in the Select A Chainage list, select **4+50.00**.
    
    Notice that the road is in a shallow cut on one side and deep cut on the other. The criteria set for the daylight subassembly caused it to use a 6:1 gradient on the left side, and a 4:1 gradient on the right side. Also notice the superelevation transition of the road. At chainage 4+50.00, the lanes are relatively flat.
    
    Note:
    
    For more information about superelevation, see the [Applying Superelevation to an Alignment](GUID-AA0068E0-2858-4067-9104-161112DEDBF6.htm "In this tutorial, you will calculate superelevation for alignment curves, create a superelevation view to display the superelevation data, and edit the superelevation data both graphically and in a tabular format.") tutorial.
    
    ![](../images/GUID-DE8E5EDF-CF0C-4B1C-AB66-DEA335B555D3.png)
    
3.  On the Chainage Selection panel, in the Select A Chainage list, select **7+75.00**.
    
    Notice the superelevation transition at this chainage. Using the Centerline Pivot option on the depressed central reserve subassembly causes the lanes and shoulders to superelevate about a point above the centerline ditch. A straight edge laid against the lane surfaces would pass through the profile gradient point.
    
    ![](../images/GUID-FAC8725D-B01C-4697-B44B-9519DBED80E4.png)
    
4.  In the Corridor Parameters dialog box, in the Assembly - (1) tree, under Group - (1), expand **Central Reserve**.
    
    Notice that the Centerline PivotDesign Value is set to Pivot About Centerline.
    
5.  In the Centerline Pivot? row, click the Value cell. Select **Pivot About Inside Edge of Carriageway** .
6.  On the Corridor Edit Tools panel, click Apply To A Chainage Range ![](../images/GUID-37D14FB2-DDBF-42E2-B268-42EC43D42AC5.png) Find.
7.  In the Apply To A Range Of Stations dialog box, notice that Start Chainage is **7+75.00**, which is the current chainage. For End Chainage, enter **11+00.00**. Click OK.
8.  On the Corridor Edit Tools panel, click Update Corridor ![](../images/GUID-AEA5492F-CEE2-4050-ACCB-F4F7F493C434.png) Find to update the corridor model.
9.  View the corridor section at chainage 7+75.00.
    
    Notice that the profile gradient is held at the inside edges of carriageways and the lanes and shoulders pivot about this point.
    
    ![](../images/GUID-4B13DE64-738E-4EBF-AF8E-C02FF03B3537.png)
    
10.  To view the gradient at subsequent chainages, click Go To Next Chainage ![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png) Find.
     
     Notice that the change you made is visible through chainage 11+00.00. At chainage 11+25.00, the Centerline Pivot?Value returns to Pivot About Centerline.
     

To continue to the next tutorial, go to [Viewing and Rendering a Corridor](GUID-007C34DB-D0D8-4E35-B831-1B9E01857FEA.htm "This tutorial demonstrates how to add surfaces to a corridor, create boundaries on the surfaces, and then visualize the corridor using the AutoCAD rendering tools.").

**Parent topic:** [Tutorial: Viewing and Editing Corridor Sections](GUID-B5B31B1A-9F20-4FBD-8A92-DC665B053AFD.htm "This tutorial demonstrates how to edit a corridor in section.")