---
title: "Exercise 2: Creating a Primary Road Junction with Turning Lanes"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-91BAC9A2-6820-4646-988E-3756B36FCDBA.htm"
category: "tutorial_creating_junctions"
last_updated: "2026-03-17T18:42:55.214Z"
---

                  Exercise 2: Creating a Primary Road Junction with Turning Lanes  

# Exercise 2: Creating a Primary Road Junction with Turning Lanes

In this exercise, you will create a junction with entry and exit turning lanes at the primary road. The side road crown will blend into the primary road edge of pavement.

You can use the workflow that is demonstrated in this exercise to create a junction with any combination of turning lanes at the radius kerbs.

Specify the junction location and primary road

1.  Open _Intersection-Create-2.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Intersections drop-down ![](../images/ac.menuaro.gif)Create Intersection ![](../images/GUID-5A1E3538-4F79-49C0-9A83-168926AF50AC.png) Find.
3.  In the drawing, click the junction point of the Road A and Road B alignments.
    
    ![](../images/GUID-4A8A5870-A860-456F-8CCF-98071E2F4AB9.png)
    
4.  Click the Road A alignment to specify it as the primary road.
    
    ![](../images/GUID-35BD82F1-F310-4635-B583-DA2B68322DCD.png)
    

Specify the corridor gradient parameters

1.  In the Create Junction wizard, on the General page, under Junction Corridor Type, select Primary Road Crown Maintained.
2.  Click Next.

Specify the horizontal and vertical geometry parameters

1.  On the Geometry Details page, click Offset Parameters.
    
    Default horizontal and vertical geometry parameters are stored in the drawing settings. You can modify the default parameters during the junction creation process.
    
2.  In the Offset Parameters dialog box, specify the following parameters:
    *   Primary Road![](../images/ac.menuaro.gif)Left Offset Alignment Definition![](../images/ac.menuaro.gif)Offset Value: **6.0000**
    *   Primary Road![](../images/ac.menuaro.gif)Right Offset Alignment Definition![](../images/ac.menuaro.gif)Offset Value: **6.0000**
    *   Secondary Road![](../images/ac.menuaro.gif)Left Offset Alignment Definition![](../images/ac.menuaro.gif)Offset Value: **3.0000**
    *   Secondary Road![](../images/ac.menuaro.gif)Right Offset Alignment Definition![](../images/ac.menuaro.gif)Offset Value: **3.0000**
    *   Create New Offsets From Start To End Of Centerlines: **Selected**
3.  Click OK.
4.  On the Geometry Details page, click Radius Kerb Parameters.
    
    The default parameters for the first junction quadrant are displayed in the Junction Radius Kerb Parameters dialog box. In the drawing, the first quadrant is highlighted, and arrows indicate the direction of incoming and outgoing traffic.
    
    Note:
    
    If you cannot see the temporary graphics, move the dialog box.
    
    ![](../images/GUID-BF6F3BCE-8A4F-415F-BC52-16E276739893.png)
    
5.  In the Junction Radius Kerb dialog box, select the Widen Turning Lane For Outgoing Road check box.
    
    The Widening Details At Outgoing Lane parameter collection is displayed in the property tree. When you highlight a property, the preview graphic at the bottom of the dialog box updates to illustrate the property in a typical junction. Examine the default values that have been specified for this drawing, but do not change any of them.
    
6.  Click Next.
7.  For SE - Quadrant, select the Widen Turning Lane For Incoming Road check box.
8.  Click Next.
9.  For SW - Quadrant, select the Widen Turning Lane For Outgoing Road check box.
10.  Click Next.
11.  For NW - Quadrant, select the Widen Turning Lane For Incoming Road check box.
12.  Click OK.
13.  In the Create Junction wizard, make sure that the Create Offset And Radius Kerb Profiles check box is selected.
14.  Click Next.

Specify the corridor parameters

1.  On the Corridor Regions page, specify the following options:
    *   Create Corridors In The Junction Area: **Selected**
    *   Create A New Corridor: **Selected**
    *   Select Surface To Daylight: **Existing Ground**
2.  In the Select Assembly Set File dialog box, navigate to the [Assemblies folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F2421B).
3.  Select _\_Autodesk (Metric) Assembly Sets.xml_. Click Open.
4.  Under Maintain Priority Road Crown, in the Radius Kerb Fillets row, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
    
    You can use the Select An Assembly dialog box to substitute an assembly with another assembly that is in the current drawing. To save your changes as a new assembly set, click Save As Set on the Corridor Regions page. For this exercise, you will accept the default assembly set.
    
    For more information about managing corridor assemblies, see the [Corridor Assembly Tutorials](GUID-2F3011CB-15DC-49E3-BB8B-B04A02B2C12E.htm "These tutorials will get you started working with the corridor assemblies, which create the primary structure of Autodesk Civil 3D corridor models.").
    
5.  Click Cancel.
6.  Click Create Junction.
    
    The junction is created, and new corridor regions are created in the junction area. Notice that the radius kerbs have widening regions to allow traffic to exit from and merge onto Road A.
    
    ![](../images/GUID-92CD9A9F-1589-495B-8492-693C8489BBCA.png)
    

To continue this tutorial, go to [Exercise 3: Creating a Junction with Existing Geometry](GUID-091643F5-9680-4440-B7A9-333F0FD5CB44.htm "In this exercise, you will use the existing offset alignments and profiles of the primary road to create a junction, and then add the new junction to the existing primary road corridor.").

**Parent topic:** [Tutorial: Creating Junctions](GUID-16F7BEE8-A134-4D9F-9AD1-EC8399E4CDB4.htm "This tutorial demonstrates how to create several types of junctions.")