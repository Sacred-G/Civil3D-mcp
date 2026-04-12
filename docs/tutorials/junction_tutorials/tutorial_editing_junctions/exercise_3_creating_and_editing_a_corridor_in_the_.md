---
title: "Exercise 3: Creating and Editing a Corridor in the Junction Area"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-1A20CA5A-8087-4CAB-B71C-08121042BED8.htm"
category: "tutorial_editing_junctions"
last_updated: "2026-03-17T18:42:56.555Z"
---

                  Exercise 3: Creating and Editing a Corridor in the Junction Area  

# Exercise 3: Creating and Editing a Corridor in the Junction Area

In this exercise, you will create a corridor using existing vertical and horizontal geometry. You will modify the corridor in the junction area, and then experiment with the corridor region recreation tools.

Create a corridor in the junction area

1.  Open _Intersection-Edit-Corridor.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a junction of a primary road (Road A) and a side road (Road B). There currently are no corridors or corridor assemblies in the drawing.
    
2.  Select the junction marker.
    
    ![](../images/GUID-00CC2E79-F426-4EDB-9E2D-6859182204A7.png)
    
3.  Click Junction tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Recreate Corridor Regions ![](../images/GUID-093E5B7C-E003-4668-B2A6-9D8DBCA0E49F.png) Find.
    
    The Junction Corridor Regions dialog box is displayed.
    
4.  Under Select Surface To Daylight, select Existing Ground.
5.  Under Apply An Assembly Set, click Browse.
6.  In the Select Assembly Set File dialog box, navigate to the [Assemblies folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F2421B).
7.  Select _\_Autodesk (Metric) Assembly Sets.xml_. Click Open.
8.  Click Recreate.
    
    A corridor is displayed in the junction area.
    
    ![](../images/GUID-0313012A-CD2F-44A5-9B06-A95573806797.png)
    

Note: If the corridor is not displayed in the junction area, you may need to rebuild the corridor. In Toolspace, on the Prospector tab, expand the Corridors collection. Right-click Corridor - (1) and click Rebuild.

Modify the corridor properties

1.  In Toolspace, on the Prospector tab, expand the Corridors and ![](../images/GUID-C430C1B8-C759-4D55-A837-8A516BF884DA.png)Junctions collections.
    
    If either of the objects in these collections is ![](../images/GUID-9ECE03CD-D682-4912-943B-9E54DEDA464D.png)out of date, right-click the object and select Rebuild.
    
2.  Select the corridor that is in the junction area.
3.  Select the ![](../images/GUID-C2F8CE18-016A-40A3-852F-9A4DE774AD89.png) grip that is at the bottom of the junction. Drag the grip down. Click to place the grip further to the south.
    
    ![](../images/GUID-9340622D-1B2B-401A-A382-76D566A9C54A.png)
    
4.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor panel ![](../images/ac.menuaro.gif)Corridor Properties drop-down ![](../images/ac.menuaro.gif)Corridor Properties ![](../images/GUID-6EDEE16B-2293-4977-BC74-7CB47522DEAA.png) Find.
5.  In the Corridor Properties dialog box, on the Parameters tab, click ![](../images/GUID-8B243375-9F97-4BBA-9333-91D9267E95C0.png)Select Region From Drawing.
6.  In the drawing, click the bottom of the corridor.
    
    ![](../images/GUID-54ED6F81-BA1B-4529-9F8B-CE7D0175CC5A.png)
    
    The specified region is highlighted in the Corridor Properties dialog box.
    
7.  In the highlighted row, in the Frequency column, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
8.  In the Frequency To Apply Assemblies dialog box, under Apply Assembly, specify the following parameters:
    *   Along Straights: **10**
    *   Along Curves: At An Increment
    *   Curve Increment: **5**
    *   Along Transitions: **5**
    *   Along Profile Curves: **5**
9.  Click OK twice.
10.  In the Corridor Properties - Rebuild task dialog box, click Rebuild the Corridor.
     
     The corridor is rebuilt. The corridor extends further to the south. In the extended region, the assemblies are further apart than the junction regions.
     
     ![](../images/GUID-291CD500-FE92-443E-9FCB-A490380C0901.png)
     

Recreate the corridor regions

1.  Select the junction marker.
2.  Click Junction tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Recreate Corridor Regions ![](../images/GUID-093E5B7C-E003-4668-B2A6-9D8DBCA0E49F.png) Find.
3.  Under Select Surface To Daylight, select Existing Ground.
4.  In the Junction Corridor Regions dialog box, under Apply An Assembly Set, click Browse.
5.  In the Select Assembly Set File dialog box, navigate to the [Assemblies folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F2421B).
6.  Select _\_Autodesk (Metric) Assembly Sets.xml_. Click Open.
    
    This is the assembly set that you used to create the corridor. However, Junction Corridor Regions dialog box enables you to specify another assembly set, or individual assemblies, with which to create the corridor.
    
7.  Click Recreate.
    
    The corridor is recreated. Notice that the modifications that you made to the Road A baseline, including the assembly frequencies and region start chainage, returned to their original settings. This happened because the corridor was recreated using the parameters that were originally specified during the junction creation process. Modifications that are made to the corridor in the junction area are not retained when you recreate the corridor from the junction object.
    
    Note:
    
    Corridor regions that are outside the junction extents are not affected by the Recreate Corridor Regions command.
    

**Parent topic:** [Tutorial: Editing Junctions](GUID-74F6ED8E-61DD-496E-8F08-C2FE11655355.htm "This tutorial demonstrates how to modify an existing junction object.")