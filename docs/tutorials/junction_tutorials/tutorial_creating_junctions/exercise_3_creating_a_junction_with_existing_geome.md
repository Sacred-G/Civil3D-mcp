---
title: "Exercise 3: Creating a Junction with Existing Geometry"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-091643F5-9680-4440-B7A9-333F0FD5CB44.htm"
category: "tutorial_creating_junctions"
last_updated: "2026-03-17T18:42:55.365Z"
---

                 Exercise 3: Creating a Junction with Existing Geometry  

# Exercise 3: Creating a Junction with Existing Geometry

In this exercise, you will use the existing offset alignments and profiles of the primary road to create a junction, and then add the new junction to the existing primary road corridor.

The workflow that is demonstrated in this exercise is useful when you need to create several junctions along a single corridor. You define the offset geometry for the primary road, and then reuse it for subsequent junctions.

Specify the junction location and primary road

1.  Open _Intersection-Create-3.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a junction of two alignments, Road A and Road C. Offset alignments exist on either side of Road A, and there is an existing junction north of Road C.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Intersections drop-down ![](../images/ac.menuaro.gif)Create Intersection ![](../images/GUID-5A1E3538-4F79-49C0-9A83-168926AF50AC.png) Find.
3.  In the drawing, click the junction point of the Road A and Road C alignments.
    
    ![](../images/GUID-8D2CDCD6-5398-4E11-ACD6-CAC6FB3C694B.png)
    
4.  Click the Road A alignment to specify it as the primary road.
    
    ![](../images/GUID-61C5CC18-88B5-4FF5-A821-97B4B5895688.png)
    

Specify the corridor gradient parameters

1.  In the Create Junction wizard, on the General page, under Junction Corridor Type, select Primary Road Crown Maintained.
2.  Click Next.

Specify the horizontal geometry parameters

1.  On the Geometry Details page, click Offset Parameters.
2.  In the Offset Parameters dialog box, under Primary Road![](../images/ac.menuaro.gif)Left Offset Alignment Definition, for Use An Existing Alignment, select **Yes.**
3.  For Alignment Name, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
4.  In the Junction Offset Alignment Name dialog box, click ![](../images/GUID-8B243375-9F97-4BBA-9333-91D9267E95C0.png).
5.  In the drawing, select the offset alignment on the left-hand side of the Road A alignment.
    
    ![](../images/GUID-48AEA9D0-674A-4646-B7B3-449E319C13E1.png)
    
6.  Click OK.
7.  In the Junction Offset Parameters dialog box, for Right Offset Alignment Definition, repeat Steps 2 through 6 to assign the offset alignment that is on the right-hand side of the Road A alignment.
    
    ![](../images/GUID-DDF4D9DC-C628-407F-861E-5F1306154AE2.png)
    
8.  Click OK.
9.  On the Geometry Details page, click Radius Kerb Parameters.
    
    The default parameters for the first junction quadrant are displayed in the Junction Radius Kerb Parameters dialog box. In the drawing, the first quadrant is highlighted, and arrows indicate the direction of incoming and outgoing traffic.
    
    ![](../images/GUID-4D87292E-F78B-40A1-8EA6-1644D8DD94B8.png)
    
10.  In the Junction Radius Kerb dialog box, under Junction Quadrant, select **SE - Quadrant**.
11.  SE - Quadrant, select the Widen Turning Lane For Incoming Road check box.
12.  Under Junction Quadrant, select **NW - Quadrant**.
13.  NW - Quadrant, select the Widen Turning Lane For Incoming Road check box.
14.  Click OK.
15.  In the Create Junction wizard, make sure that the Create Offset And Radius Kerb Profiles check box is selected.
16.  Click Next.

Specify the vertical geometry parameters

1.  On the Geometry Details page, under Offset and Corner Radius Profiles, click Lane Crossfall Parameters.
2.  In the Intersection Lane Crossfall Parameters dialog box, under Primary Road![](../images/ac.menuaro.gif)Left Offset Alignment Definition, for Use An Existing Alignment, select **Yes.**
3.  For Profile Name, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
4.  In the Junction Offset Alignment Name dialog box, select Road A - -2.000%.
5.  Click OK.
6.  In the Junction Offset Parameters dialog box, for Right Edge Profile Definition, repeat Steps 2 through 5 to assign the offset profile that is on the right-hand side of the Road A alignment. Use profile Road A - -2.000% (1) as the Right Edge Profile Definition![](../images/ac.menuaro.gif)Profile Name.
7.  Click OK.

Specify the corridor parameters

1.  On the Corridor Regions page, specify the following options:
    *   Create Corridors In The Junction Area: **Selected**
    *   Add To An Existing Corridor: **Selected**, **Corridor - (1)**
    *   Select Surface To Daylight: **Existing Ground**
2.  Under Select Assembly Set To Import, click Browse.
3.  In the Select Assembly Set File dialog box, navigate to the [Assemblies folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F2421B).
4.  Select _\_Autodesk (Metric) Assembly Sets.xml_. Click Open.
5.  Click Create Junction.
    
    The junction is created, and new corridor regions are created in the junction area.
    
    ![](../images/GUID-A8BCFBAC-9702-443B-82AA-0CC9A8CAE171.png)
    

**Further exploration**: To extend the corridor between the two junctions, add a corridor region between the two junctions.

![](../images/GUID-522A43E2-BD15-4E9A-AE4A-78536AAB0666.png)

To continue to the next tutorial, go to [Editing Junctions](GUID-74F6ED8E-61DD-496E-8F08-C2FE11655355.htm "This tutorial demonstrates how to modify an existing junction object.").

**Parent topic:** [Tutorial: Creating Junctions](GUID-16F7BEE8-A134-4D9F-9AD1-EC8399E4CDB4.htm "This tutorial demonstrates how to create several types of junctions.")