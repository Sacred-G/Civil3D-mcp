---
title: "Exercise 2: Creating a Corridor with a Transition Lane"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-AFC8E584-CD60-45E6-B9D5-18E69380CF41.htm"
category: "tutorial_creating_a_corridor_with_a_transition_lan"
last_updated: "2026-03-17T18:42:50.000Z"
---

                 Exercise 2: Creating a Corridor with a Transition Lane  

# Exercise 2: Creating a Corridor with a Transition Lane

In this exercise, you will create a corridor using the assembly created in the last exercise. You will target the width and level of the right lane edge to a right alignment and profile, and the left lane edge to a polyline and a feature line.

This exercise continues from [Exercise 1: Creating an Assembly with a Transition Lane](GUID-3980DDA1-2D0F-4ED8-898E-9522E4FD6D55.htm "In this exercise, you will create a corridor assembly with transitions.").

Specify the basic corridor information

Note:

This exercise uses _Corridor-2a.dwg_ from the previous exercise, or you can open _Corridor-2b.dwg_ from the [Tutorial Folder Locations](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm).

2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Corridor ![](../images/GUID-194DAB2F-3FBA-4AFA-9BBF-3377ABB79B24.png) Find.
3.  In the Create Corridor dialog box, specify the following parameters:
    *   Name: **Corridor - Transition Lanes**
    *   Baseline Type: Alignment and Profile
    *   Alignment: Centerline (1)
    *   Profile: Layout (1)
    *   Assembly: Transition
        
        The Transition assembly includes the BasicLaneTransition subassembly, which uses the Transition parameter to specify that both the offset and level can change on the right side of the corridor. The offset can change on the left side of the corridor but the gradient is held at -2%.
        
    *   Target Surface: EG
    *   Set Baseline and Region Parameters: Selected
4.  Click OK.

Specify the fixed lane horizontal target

1.  In the Baseline and Region Parameters dialog box, click Set All Targets.
2.  In the Target Mapping dialog box, in the Transition Alignment row for BasicLaneTransition - (Right), click the Object Name field.
3.  In the Set Width Or Offset Target dialog box, specify the following parameters:
    *   Select Object Type To Target: Alignments
    *   Select Alignments: **Right (1)**
4.  Click Add. Click OK.

Specify the transition lane horizontal target

1.  In the Transition Alignment row for BasicLaneTransition - (Left), click the Object Name field.
2.  In the Set Width Or Offset Target dialog box, in the Select Object Type To Target list, select Feature Lines, Survey Figures And Polylines.
3.  Click Select From Drawing.
4.  In the drawing, on the left side of the alignment, select the blue polyline and magenta feature line. Press Enter.
    
    ![](../images/GUID-191FC01B-A18D-425C-9343-98F425ACBDF8.png)
    
    The elements are added to the table at the bottom of the Set Width Or Offset Target dialog box.
    
5.  Click OK.
    
    Notice that because the subassembly names contain the assembly side, it is easy to determine which assembly must target which offset object. This naming convention is even more useful in road designs that contain many alignments and subassemblies. For information on updating the subassembly naming template, see the [Modifying the Subassembly Name Template](GUID-6597BC8F-1F9B-4D0A-B030-D84851808127.htm "In this exercise, you will specify a meaningful naming convention to apply to subassemblies as they are created.") exercise.
    

Specify the fixed lane level targets

1.  In the Transition Profile row for BasicLaneTransition - (Right), click the Object Name field.
2.  In the Set Gradient Or Level Target dialog box, specify the following parameters:
    *   Select Object Type To Target: Profiles
    *   Select An Alignment: **Right (1)**
    *   Select Profiles: **Layout (1)**
3.  Click Add. Click OK.
    
    The right-side edge-of-pavement level is set to the **Layout (1)** profile. The left-side edge-of-pavement level does not need to be set since its level is determined by the gradient setting.
    
4.  Click OK twice.
    
    The corridor model is built, and looks like this:
    
    ![](../images/GUID-4150F347-8223-407D-867B-BDC2D87BA945.png)
    
    Note:
    
    Notice that at chainage 7+50, the lane uses the polyline as a target, and not the feature line. When more than one target object is found at a chainage, the object that is closest to the corridor baseline is used as the target.
    
    ## A detail of the overlapping objects
    
    ![](../images/GUID-1C154010-6094-40A1-B27D-A741C687ED9A.png)
    

To continue to the next tutorial, go to [Creating a Divided Highway Corridor](GUID-1164369B-CC02-47C4-B9E4-12A34DA41287.htm "This tutorial demonstrates how to create a divided highway corridor. The tutorial uses some of the subassemblies that are shipped with Autodesk Civil 3D to create a more complex and realistic highway model.").

**Parent topic:** [Tutorial: Creating a Corridor with a Transition Lane](GUID-05B7F091-145E-4757-B1FD-10DA48B552D0.htm "This tutorial demonstrates how to create a corridor with a transition lane. The tutorial uses some of the subassemblies that are shipped with Autodesk Civil 3D to create an assembly. Then, you create a carriageway where the travel lane widths and crossfalls are controlled by offset alignments, profiles, polylines, and feature lines.")