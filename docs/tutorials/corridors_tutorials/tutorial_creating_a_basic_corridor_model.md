---
title: "Tutorial: Creating a Basic Corridor Model"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-33CD2D78-34B6-4564-A1BD-AE3FBDEB584B.htm"
category: "corridors_tutorials"
last_updated: "2026-03-17T18:42:48.998Z"
---

                  Tutorial: Creating a Basic Corridor Model  

# Tutorial: Creating a Basic Corridor Model

This tutorial demonstrates how to use Autodesk Civil 3D objects to build a basic corridor model.

Note:

This tutorial uses the corridor assembly that you built in the [Creating an Assembly](GUID-01BEF57E-713B-4A16-8FF7-CDE850CB2CB8.htm "In this exercise, you'll use some of the subassemblies that are shipped with Autodesk Civil 3D to create an assembly for a basic crowned carriageway with travel lanes, kerbs, channels, footpaths and slopes to an existing surface.") exercise.

A corridor can be used to model a variety of features, such as highways, drainage channels, and runways. In this tutorial, you will model a residential road.

A corridor model builds on and uses various Autodesk Civil 3D objects and data, including subassemblies, assemblies, alignments, feature lines, surfaces and profiles.

Corridor objects are created along one or more horizontal baselines by placing a 2D section (assembly) at incremental locations and creating matching slopes that reach a surface model at each incremental location.

Specify the basic corridor information

1.  Open _Corridor-1a.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Corridor ![](../images/GUID-194DAB2F-3FBA-4AFA-9BBF-3377ABB79B24.png) Find.
3.  In the Create Corridor dialog box, specify the following parameters. Alternatively, you can use the ![](../images/GUID-8B243375-9F97-4BBA-9333-91D9267E95C0.png) buttons to pick the objects from the drawing.
    *   Name: **First Street**
    *   Baseline Type: Alignment and Profile
    *   Alignment: First Street
        
        ![](../images/GUID-C29FFA57-58E3-4460-90C9-69B734D33D9E.png)
        
    *   Profile: Finished Gradient Centerline - First Street
        
        ![](../images/GUID-5D0C4238-7CCA-4AC6-B6F7-03FBE99EE4BF.png)
        
        (Profile view grid lines removed for clarity)
        
    *   Assembly: Primary Road Full Section
        
        ![](../images/GUID-3DE0A556-4AAE-4C91-8604-41302F8547C4.png)
        
    *   Target Surface: EG
        
        ![](../images/GUID-4306EBDB-D934-4981-B8C4-84FE9CAFB0A7.png)
        
    *   Set Baseline and Region Parameters: Selected
4.  Click OK.

Specify the baseline and region parameters

1.  In the Baseline and Region Parameters dialog box, in the RG-Primary Road Full Section - (1) row, in the End Station cell, enter **0+440.00**.
2.  In the Frequency cell, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
3.  In the Frequency To Apply Assemblies dialog box, under Apply Assembly, for Along Curves, verify that At An Increment is selected.
4.  For Curve Increment, enter **3.000**. Click OK.
5.  Click OK.
6.  In the Corridor Properties - Rebuild task dialog box, click Rebuild the Corridor.
    
    Note: If the task dialog box is not displayed, the corridor is still built.
    
    The corridor model is built and looks like this:
    
    ![](../images/GUID-41181396-128E-493B-A835-1F2995628A78.png)
    

To continue to the next tutorial, go to [Creating a Corridor with a Transition Lane](GUID-05B7F091-145E-4757-B1FD-10DA48B552D0.htm "This tutorial demonstrates how to create a corridor with a transition lane. The tutorial uses some of the subassemblies that are shipped with Autodesk Civil 3D to create an assembly. Then, you create a carriageway where the travel lane widths and crossfalls are controlled by offset alignments, profiles, polylines, and feature lines.").

**Parent topic:** [Corridors Tutorials](GUID-DE41CF86-9A42-4F1F-B880-767F810CE0EB.htm "These tutorials will get you started working with the corridor modeling tools, which are used to design and generate complex carriageway corridor models.")