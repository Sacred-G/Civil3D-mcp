---
title: "Exercise 4: Rebuilding the Corridor and Examining the Results"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-3689ED14-AAE2-46C5-8C15-1DE36A99A7AB.htm"
category: "tutorial_creating_an_assembly_with_conditions"
last_updated: "2026-03-17T18:42:46.592Z"
---

                Exercise 4: Rebuilding the Corridor and Examining the Results  

# Exercise 4: Rebuilding the Corridor and Examining the Results

In this exercise, you will reset the corridor targets, rebuild the corridor, and then examine how the conditional subassembly affects the corridor model.

This exercise continues from [Exercise 3: Adjusting Conditional Subassembly Properties](GUID-BC24C59A-5E1E-4727-B32D-137A6E95141C.htm "In this exercise, you will adjust the properties of one of the subassemblies, and then assign descriptive names to each of the subassemblies in the Through Road assembly.").

Set targets and rebuild the corridor

1.  Open _Assembly-2c.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains the corridor assembly that you designed in the previous three exercises.
    
2.  In the drawing, click the **Corridor - (1)** corridor.
3.  Click Corridor tab ![](../images/ac.menuaro.gif)Modify Corridor panel ![](../images/ac.menuaro.gif) Corridor Properties drop-down ![](../images/ac.menuaro.gif) Corridor Properties![](../images/GUID-6EDEE16B-2293-4977-BC74-7CB47522DEAA.png).
4.  In the Corridor Properties dialog box, on the Parameters tab, click Set All Targets.
5.  In the Target Mapping dialog box, in the Object Name column, click <Click Here To Set All>.
6.  In the Pick A Surface dialog box, click **Existing Ground**.
7.  Click OK three times to close the dialog boxes and rebuild the corridor.

Examine the rebuilt corridor

1.  In the View/Edit Corridor Section Tools toolbar, click ![](../images/GUID-5C996F79-926D-4B4E-8652-0FE408CB2C48.png) to return to chainage 0+00.
    
    At the first few chainages, the corridor is in a relatively deep fill condition. In [Exercise 2: Adding Conditional Subassemblies to a Corridor Assembly](GUID-E3846BC4-E89B-4191-B313-5FF32607173F.htm "In this exercise, you will add ConditionalCutOrFill subassemblies to an existing corridor assembly."), you attached the DaylightBench subassembly to the Fill 5.00: 10000.00 conditional subassembly. The fill condition at this chainage is greater than 5.0001’, so the DaylightBench subassembly is applied.
    
    ![](../images/GUID-EF7692F4-40EA-465B-B5FF-6D2EA9362B6D.png)
    
2.  Click ![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png)eight times to advance to chainage 2+00.
    
    Starting at chainage 2+00, the corridor enters a cut condition. At this chainage, the cut is less than 5.0000’, so the DaylightOffsetToSurface subassembly is applied after the ditch.
    
    ![](../images/GUID-5402D9D8-7072-492F-94E2-F42AFEF607A0.png)
    
3.  Click ![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png) again.
    
    Starting at chainage 2+25, the cut condition is greater than 5.0001’. As you specified, the DaylightWidthGradient and RetainWallVertical subassemblies are applied after the ditch.
    
    ![](../images/GUID-7047A52C-AB1D-4309-B3DD-C5629EBA3077.png)
    
4.  Continue using the ![](../images/GUID-5C996F79-926D-4B4E-8652-0FE408CB2C48.png)![](../images/GUID-89BDDE53-C4AA-4EAF-ABB0-06BF21BFD9C9.png)![](../images/GUID-31604CE7-96E9-478A-A876-BF8BE1EED13D.png)![](../images/GUID-DA741EF3-B6B8-4935-AEF7-3DC0BDA49AD0.png) buttons to examine the cut and fill conditions along the corridor.

**Further exploration:** Apply what you learned to the right-hand side of the corridor assembly. Use different combinations of daylight subassemblies with the ConditionalCutOrFill subassembly and examine the results.

To continue to the next tutorial, go to [Saving and Sharing Corridor Assemblies](GUID-A7837167-9635-4291-8624-D4E263D02BB6.htm "This tutorial demonstrates how to use Autodesk Civil 3D to save commonly used corridor assemblies, and then share them with other users.").

**Parent topic:** [Tutorial: Creating an Assembly with Conditions](GUID-4CC6F8B3-01A6-4A95-988A-AF9E0A05A875.htm "This tutorial demonstrates how to use the ConditionalCutOrFill subassembly to build a corridor assembly that applies different subassemblies depending on the cut or fill condition at a given chainage.")