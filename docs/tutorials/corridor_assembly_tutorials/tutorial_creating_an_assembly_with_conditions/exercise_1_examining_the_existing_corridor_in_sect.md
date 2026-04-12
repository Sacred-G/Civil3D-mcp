---
title: "Exercise 1: Examining the Existing Corridor in Section"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-F3D8D55E-A87B-4093-8ADA-2A3E0C5EC6E9.htm"
category: "tutorial_creating_an_assembly_with_conditions"
last_updated: "2026-03-17T18:42:46.058Z"
---

                Exercise 1: Examining the Existing Corridor in Section  

# Exercise 1: Examining the Existing Corridor in Section

In this exercise, you will examine how the daylight subassemblies are applied to the corridor model in section. You will notice chainages at which the current daylighting parameters are inappropriate for the site conditions.

Examine the existing corridor

1.  Open _Assembly-2a.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains two viewports. A completed corridor assembly is displayed in the top viewport. A surface, corridor, and profile view are displayed in the bottom viewport.
    
2.  In the bottom viewport, select the corridor. Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor Sections panel ![](../images/ac.menuaro.gif) Section Editor![](../images/GUID-524AFC3A-7C80-4E4E-9BDB-FE0F2DB3F952.png).
3.  On the Section Editor tab, use the ![](../images/GUID-5C996F79-926D-4B4E-8652-0FE408CB2C48.png)![](../images/GUID-89BDDE53-C4AA-4EAF-ABB0-06BF21BFD9C9.png)![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png)![](../images/GUID-DA741EF3-B6B8-4935-AEF7-3DC0BDA49AD0.png) buttons to examine how the Through Road assembly is applied to at the corridor chainages.
    
    The assembly creates a ditch on either side of the road. At the beginning and end of the corridor, the cut and fill is relatively consistent on both sides.
    
    ![](../images/GUID-AE6B2CE1-EF7F-4E4D-9ABE-51161A782BE4.png)
    
    In the following exercises, you will address two conditions:
    
    *   First, the fill condition from chainages 0+00 through 1+00 produces a relatively deep fill on the left side. While the corridor assembly is constructed appropriately for other regions of the corridor, you will modify the design to use a different approach in this region.
        
        ![](../images/GUID-7B950419-4B7A-4460-B581-6380C9D422A2.png)
        
    *   Second, from chainages 5+00 through 8+00, a much greater amount of material must be cut from the left side of the corridor. While the Through Road assembly is appropriate for most the corridor, it is not ideal for these chainages.
        
        ![](../images/GUID-A8CE275B-67F0-4A83-BA10-B47BF323CCD7.png)
        
4.  In the View/Edit Corridor Section Tools toolbar, click ![](../images/GUID-5C996F79-926D-4B4E-8652-0FE408CB2C48.png) to return to chainage 0+00.

To continue this tutorial, go to [Exercise 2: Adding Conditional Subassemblies to a Corridor Assembly](GUID-E3846BC4-E89B-4191-B313-5FF32607173F.htm "In this exercise, you will add ConditionalCutOrFill subassemblies to an existing corridor assembly.").

**Parent topic:** [Tutorial: Creating an Assembly with Conditions](GUID-4CC6F8B3-01A6-4A95-988A-AF9E0A05A875.htm "This tutorial demonstrates how to use the ConditionalCutOrFill subassembly to build a corridor assembly that applies different subassemblies depending on the cut or fill condition at a given chainage.")