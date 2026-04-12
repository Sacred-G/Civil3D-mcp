---
title: "Exercise 1: Creating an Alignment with the Alignment Layout Tools"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-2EA85F3E-E50C-446A-8DF7-EFCD3EEB09FD.htm"
category: "tutorial_creating_alignments"
last_updated: "2026-03-17T18:42:25.092Z"
---

                 Exercise 1: Creating an Alignment with the Alignment Layout Tools  

# Exercise 1: Creating an Alignment with the Alignment Layout Tools

In this exercise, you will use the alignment layout tools to draw an alignment that has curves.

Specify alignment properties

1.  Open _Align-1.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing contains a surface marked with several circles, labeled A through D.
    
    Note:
    
    Ensure that Object Snap (OSNAP) is turned on.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Alignment drop-down ![](../images/ac.menuaro.gif)Alignment Creation Tools ![](../images/GUID-C6A978DD-F77D-4284-BCBE-7E7D572F9097.png) Find.
3.  In the Create Alignment – Layout dialog box, for Name, enter **First Street**.
4.  On the General tab, select the following settings:
    
    *   Site: **<None>**
    *   Alignment Style: **Layout**
    *   Alignment Layer: **C-ROAD**
    *   Alignment Label Set: **Major Minor and Geometry Points**
    
    Setting the site to <None> prevents the alignment from interacting with other objects in the drawing. The <None> selection is helpful when you do not want plots to be created when either intersecting alignments form closed regions or an alignment crosses an existing plot.
    
5.  Click the Design Criteria tab.
    
    The Starting Design Speed value specifies the default design speed at the alignment starting chainage. Design speeds can be specified at other chainages along the alignment. If no other design speeds are specified, the Starting Design Speed is applied to the entire alignment. Accept the default Starting Design Speed value for this exercise.
    
    The other options on this tab are used only if you want to ensure that the alignment design meets specified design criteria. You do not apply design criteria to the alignment in this exercise. You will learn how to use the design criteria feature in the [Designing an Alignment that Refers to Local Standards](GUID-CAEC3077-D78A-4F42-8E47-41FABAB6915D.htm "This tutorial demonstrates how to validate that your alignment design meets criteria specified by a local agency.") tutorial.
    
6.  Click OK.
    
    The Alignment Layout Tools toolbar is displayed. It includes the controls required to create and edit alignments.
    

Draw the alignment

1.  In the Alignment Layout Tools toolbar, click the drop-down list ![](../images/GUID-BFE2CF91-808A-488D-9512-8B3F59C8F93E.png) and select Curve and Transition Settings![](../images/GUID-C61CEB34-12D7-4C0E-AF3C-D6BC4B521545.png). In the Curve and Transition Settings dialog box, you can specify the type of curve to be automatically placed at every intersection point (PI) between straights.
2.  In the Curve and Transition Settings dialog box, specify the following parameters:
    *   Type: **Clothoid**
    *   Transition In: **Cleared**
    *   Curve: **Selected**
    *   Transition Out: **Cleared**
    *   Default Radius: **350.0000’**
3.  Click OK.
4.  On the Alignment Layout Tools toolbar, click the drop-down list ![](../images/GUID-BFE2CF91-808A-488D-9512-8B3F59C8F93E.png). Select Straight-Straight (With Curves)![](../images/GUID-BFE2CF91-808A-488D-9512-8B3F59C8F93E.png).
5.  Snap to the center of circle A to specify a start point for the alignment.
6.  Stretch a line out, and specify additional PIs by snapping to the center of circles B, C, and D (in order). Then, right-click to end the horizontal alignment layout command.
7.  Pan and zoom in the drawing to examine the style and content of the labels. Note especially the geometry point labels marking the start and end points of each line, transition, and curve.
    
    ![](../images/GUID-9A23A713-561A-44C7-9C2D-A6BB29FBD1E2.png)
    

To continue this tutorial, go to [Exercise 2: Adding Free Curves and Transitions to an Alignment](GUID-2133A9D7-9CE9-4BFF-A248-C71BA0756457.htm "In this exercise, you will add a free curve and a free transition-curve-transition to a simple alignment.").

**Parent topic:** [Tutorial: Creating Alignments](GUID-B489AAE6-C5DF-43F7-B6CB-E9E76D7D885C.htm "This tutorial demonstrates how to create and modify alignments.")