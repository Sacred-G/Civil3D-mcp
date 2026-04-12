---
title: "Exercise 1: Creating a Peer Road Junction"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-1A83964C-D705-44FC-9A69-0F63E969D848.htm"
category: "tutorial_creating_junctions"
last_updated: "2026-03-17T18:42:55.054Z"
---

                  Exercise 1: Creating a Peer Road Junction  

# Exercise 1: Creating a Peer Road Junction

In this exercise, you will create a three-way junction and generate a corridor that maintains the crowns of both roads.

To create a complete junction model, you must have a centerline alignment and profile for each of the intersecting roads. The horizontal and vertical geometry for the remaining elements, including the offsets and corner radius, is generated based on the parameters you specify.

In a peer road junction, the crowns of all intersecting roads are held at a common gradient. The pavement for both roads is blended into the corner radius regions, which form the transitions between the intersecting roads.

The drawing for this exercise contains a corridor along each of the intersecting roads. Each corridor is made up of a corridor assembly and a centerline alignment and profile.

At the end of the exercise, the drawing also will contain the following elements:

*   A junction object
*   Two corner radius alignments and profiles
*   Four offset alignments and profiles (two for each centerline alignment)
*   Several new corridor regions
*   Corridor assemblies for each region of the junction

Specify the junction location

1.  Open _Junction-Create-1.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Junctions drop-down ![](../images/ac.menuaro.gif)Create Junction ![](../images/GUID-5A1E3538-4F79-49C0-9A83-168926AF50AC.png) Find.
3.  In the drawing, click the junction point of the two alignments.
    
    ![](../images/GUID-43C79624-4E37-4D15-A1B7-2408EDA6C2D4.png)
    

Specify the corridor gradient parameters

1.  In the Create Junction wizard, on the General page, under Junction Corridor Type, select All Crowns Maintained.
2.  Click Next.

Specify the geometry of the offsets and corner radius

1.  On the Geometry Details page, click Offset Parameters.
    
    Default parameters are stored in the drawing settings. You can modify the default parameters during the junction creation process.
    
2.  In the Offset Parameters dialog box, specify the following parameters:
    *   Secondary Road![](../images/ac.menuaro.gif)Left Offset Alignment Definition![](../images/ac.menuaro.gif)Offset Value: **3.5000**
    *   Secondary Road![](../images/ac.menuaro.gif)Right Offset Alignment Definition![](../images/ac.menuaro.gif)Offset Value: **3.5000**
    *   Create New Offsets From Start To End Of Centerlines: **Cleared**
        
        When this option is selected, offset alignments are created along the entire length of the centerline alignment. This option is useful when you need to use offset alignments and profiles as targets for other objects, including other junctions along the same road.
        
3.  Click OK.
4.  On the Geometry Details page, click Radius Kerb Parameters.
5.  In the Junction Corner Radius dialog box, under Corner Radius Parameters, specify the following parameters:
    
    *   Corner Radius Type: Circular Fillet
    *   Radius: **7.5**
    
    Note:
    
    In the drawing, temporary graphics highlight the currently selected corner radius.
    
6.  Right-click Corner Radius Parameters. Click Copy These To All Quadrants.
    
    This command copies the corner radius parameters to all junction corner radius regions. The number of corner radius regions is automatically generated based on the existing horizontal geometry. For example, if this was a four-way junction, four corner radius regions would be available.
    
7.  Click OK.
8.  In the Create Junction wizard, under Offset And Corner Radius Profiles, make sure that Create Offset And Corner Radius Profiles is selected.
    
    To produce a complete corridor model of the junction, it is necessary to create profiles for the offset alignments and corner radius alignments. For this exercise, you will accept the default offset and corner radius profile settings.
    
9.  Click Next.

Specify the corridor parameters

1.  On the Corridor Regions page, specify the following options:
    *   Create Corridors In The Junction Area: **Selected**
    *   Add To An Existing Corridor: **Selected**, **Second Street**
    *   Select Surface To Daylight: **EG**
2.  Under Select Assembly Set To Import, click Browse.
3.  In the Select Assembly Set File dialog box, navigate to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC).
4.  Select _Junction-Assembly-Set\_All Crowns.xml_. Click Open.
    
    An assembly set enables you to quickly import a group of existing corridor assemblies, and then apply them to specific section types.
    
5.  Click Create Junction.
    
    The junction is created, and new corridor regions are created in the junction area.
    
    ![](../images/GUID-44BB5106-6A71-48EF-9E6F-B966A65341AC.png)
    

Examine the new objects

1.  In Toolspace, on the Prospector tab, expand the ![](../images/GUID-A1AF1DE4-7370-4208-8B4C-ABB3CB562C79.png)Alignments collection.
    
    Four alignments collections are available.
    
2.  Under ![](../images/GUID-A1AF1DE4-7370-4208-8B4C-ABB3CB562C79.png)Alignments, expand the ![](../images/GUID-A1AF1DE4-7370-4208-8B4C-ABB3CB562C79.png)Centerline Alignments, ![](../images/GUID-C84B1C0E-2655-4C96-9DAB-455672773ADC.png)Offset Alignments, and ![](../images/GUID-6959C7DA-7ECC-4C77-BF71-24561E2D7933.png)Corner Radius Alignments collections.
    
    At the beginning of this exercise, only Centerline Alignments existed. The Offset Alignments and Nearside Kerb Alignments were created using the parameters that you specified in the Create Junction wizard.
    
    Note:
    
    In the drawing, the offset alignments and chainage labels are blue, and the nearside kerb alignments are red.
    
3.  Under ![](../images/GUID-C84B1C0E-2655-4C96-9DAB-455672773ADC.png)Offset Alignments, expand the ![](../images/GUID-C84B1C0E-2655-4C96-9DAB-455672773ADC.png)**First Street-Left-3.500**![](../images/ac.menuaro.gif)![](../images/GUID-6A9013E0-00A3-425C-91A3-26CD110251ED.png)Profiles collection.
    
    Layout profiles for the Offset Alignments and Nearside Kerb Alignments were created using the parameters that you specified in the Create Junction wizard.
    

Closing gaps in the corridor

1.  In the drawing, select the corridor in the junction area.
    
    Slider ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) grips are displayed at the start and end chainages of the corridor regions.
    
2.  Click the ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) grip at chainage 0+040.
    
    The grip turns red.
    
3.  On the command line, enter 21.
4.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Region panel ![](../images/ac.menuaro.gif)Copy Region ![](../images/GUID-79AC363E-E667-452A-8139-3CA2B851A2FC.png) Find.
5.  Select the portion of the Second Street corridor that loops around the site.
6.  On the command line, enter F to fill a gap in the corridor.
7.  Move the cursor toward the gap in the corridor.
    
    A red graphic indicates that the gap may be filled.
    
8.  Click when the red graphic is visible.
    
    The gap is filled.
    
9.  Press Enter to end the command.
10.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor panel ![](../images/ac.menuaro.gif)Rebuild Corridor ![](../images/GUID-83E36DF7-3798-4501-B2EB-9D16A1665757.png) Find.
     
     The Second Street corridor rebuilds, eliminating the gaps.
     
     ![](../images/GUID-44BB5106-6A71-48EF-9E6F-B966A65341AC.png)
     
11.  Select the First Street corridor. Select the ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) grip at Chainage 0+440.
     
     The grip turns red.
     
12.  Drag the grip toward the junction. Click to place the grip at the beginning of the junction.
13.  Right-click the First Street corridor. Click Rebuild Corridor.
     
     The corridor rebuilds, eliminating the gaps between it and the junction.
     
     ![](../images/GUID-826409EC-CA6A-4384-9B6A-61FD7FEF93DB.png)
     

To continue this tutorial, go to [Exercise 2: Creating a Primary Road Junction with Turning Lanes](GUID-91BAC9A2-6820-4646-988E-3756B36FCDBA.htm "In this exercise, you will create a junction with entry and exit turning lanes at the primary road. The side road crown will blend into the primary road edge of pavement.").

**Parent topic:** [Tutorial: Creating Junctions](GUID-16F7BEE8-A134-4D9F-9AD1-EC8399E4CDB4.htm "This tutorial demonstrates how to create several types of junctions.")