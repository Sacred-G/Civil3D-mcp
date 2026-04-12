---
title: "Exercise 3: Adding a Widening to an Offset Alignment"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-8208038F-7522-44C1-8DBC-5AAC9D789107.htm"
category: "tutorial_working_with_offset_alignments"
last_updated: "2026-03-17T18:42:26.615Z"
---

                 Exercise 3: Adding a Widening to an Offset Alignment  

# Exercise 3: Adding a Widening to an Offset Alignment

In this exercise, you will add dynamic widening regions between specified stations of an offset alignment.

Widening regions are useful for creating bus bays, central reserves, turning lanes, and parking lanes.

This exercise continues from [Exercise 2: Editing an Offset Alignment](GUID-1CC17214-81E0-45B5-AC1F-373E00DDCA8A.htm "In this exercise, you will examine the offset alignment editing tools.").

Create a widening on an offset alignment

1.  Open _Align-6C.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains a surface and a centerline alignment with two offset alignments.
    
2.  Select the offset alignment on the right-hand side of the centerline alignment.
    
    ![](../images/GUID-FD06E439-DB70-40F9-A881-25E9C0A04AB4.png)
    
3.  Click Offset Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Add Widening ![](../images/GUID-9CA90CF4-F6DD-4B55-93B6-8373328AD328.png) Find.
4.  Follow the command line prompts to specify the following parameters:
    
    *   Create Widening Portion As A New Alignment?: **No**
    *   Start Chainage: **150**
    *   End Chainage: **1000**
    *   Widening Offset: **24**
    
    The widening region is created, and the parameters you entered are displayed in the Offset Alignment Parameters dialog box. You will learn how to use this dialog box and the grips to modify the offset alignment in [Exercise 4: Editing an Offset Widening](GUID-92213F16-7C49-4729-8088-A166CDD17B2C.htm "In this exercise, you will change the transition between an offset alignment and its widening region, and then use grips to modify the widening geometry.").
    
    ![](../images/GUID-E2167BCF-A2AD-4481-A1B4-095D30FB62DB.png)
    

Add a widening region to a widening region

1.  Select the offset alignment that is on the right-hand side of the centerline alignment. Right-click. Click Edit Offset Parameters.
    
    Note:
    
    You can also use the ![](../images/GUID-97BC4CB7-B524-43BF-8F10-73447BAE260A.png) grip to add a widening region.
    
    The offset alignment parameters, including the parameters of the existing widening, are displayed in the Offset Alignment Parameters dialog box.
    
2.  In the Offset Parameters dialog box, click Add A Widening.
3.  Follow the command line prompts to specify the following parameters:
    *   Start Chainage: **550**
    *   End Chainage: **750**
    *   Widening Offset: **42**
4.  Press Esc to deselect the offset alignment.
    
    The second widening region is created within the original widening region. The parameters for the new widening region are displayed in the Offset Alignment Parameters dialog box.
    
    ![](../images/GUID-AD04591E-E389-4C1C-968B-0C99881F198A.png)
    

To continue this tutorial, go to [Exercise 4: Editing an Offset Widening](GUID-92213F16-7C49-4729-8088-A166CDD17B2C.htm "In this exercise, you will change the transition between an offset alignment and its widening region, and then use grips to modify the widening geometry.").

**Parent topic:** [Tutorial: Working with Offset Alignments](GUID-AF7F5C3B-9B2F-4D3A-8F01-CF474DD8D352.htm "This tutorial demonstrates how to create and modify offset alignments that are dynamically linked to a centerline alignment.")