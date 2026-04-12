---
title: "Exercise 3: Working with Crossing Feature Lines"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-714167AD-A72A-4308-976B-5079ACB6D3F4.htm"
category: "tutorial_using_feature_lines_to_modify_a_grading"
last_updated: "2026-03-17T18:42:43.310Z"
---

                   Exercise 3: Working with Crossing Feature Lines  

# Exercise 3: Working with Crossing Feature Lines

In this exercise, you will learn how feature lines interact when they cross each other at and between vertices.

This exercise continues from [Exercise 2: Adjusting Grading Triangulation with a Feature Line](GUID-5653D269-9D07-48E6-B360-7307EFD5FD8F.htm "In this exercise, you will use a feature line to break a poorly triangulated grading surface.").

To work with crossing feature lines

Note:

This exercise uses _Grading-7.dwg_ with the modifications you made in the previous exercise, or you can open _Grading-8.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  In the right viewport, select feature line ABC. Right-click. Click Level Editor.
    
    The Grading Level Editor displays a list of the points of intersection (![](../images/GUID-4E462953-8FC8-4CFA-A8FE-A255184D6ED3.png) PIs, or horizontal geometry points) and level change points (![](../images/GUID-5D695FFA-144B-447B-913D-A1C2762923FF.png) vertical geometry points) along the feature line.
    
    Notice that the ![](../images/GUID-9FDD1463-4AF9-46F7-89AC-482E325B9B20.png) icon is displayed in the first row. This icon indicates that the building pad feature line also has a PI at this point. When two feature lines cross at a common vertex, both vertices must share the same level value. The level for both feature lines at this point is determined by whichever feature line was most recently edited. In the following illustration, the common vertex is in Circle A.
    
    ![](../images/GUID-363B6F54-ECCE-4720-9324-705A143F5C4D.png)
    
3.  In the Grading Level Editor click ![](../images/GUID-7AB52249-9E7A-4721-8B1A-9728F53EF2B5.png)Unselect All Rows. Click ![](../images/GUID-F8C0B433-DC00-48D7-BEF2-F0F99095A0CE.png)Flatten Gradient or Levels.
4.  In the Flatten dialog box, select Constant Gradient. Click OK.
    
    Notice that in the Grading Level Editor, the Gradient Ahead and Gradient Back values are updated to a consistent value. When you flatten a feature line, the points between the start level and the end level are set to the same gradient, effectively eliminating the gradient breaks. You may flatten either the entire feature line, or a selection of points.
    
5.  In the ![](../images/GUID-9FDD1463-4AF9-46F7-89AC-482E325B9B20.png) row (station 0+00), in the Level column, change the level value to **402.00’**.
6.  In the Grading Level Editor, click ![](../images/GUID-8B243375-9F97-4BBA-9333-91D9267E95C0.png)Select A Feature Line, Parcel Line Or Survey Figure.
7.  In the drawing window, click the building pad feature line.
    
    The level points of the building pad are now displayed in the Grading Level Editor.
    
8.  Locate the row containing the ![](../images/GUID-9FDD1463-4AF9-46F7-89AC-482E325B9B20.png) icon (station 3+09.03).
    
    Notice that the value in the Level column is 402.000’, which is the same value you entered for the other feature line in step 4. Change the Level value to **405.000’**.
    
    In the left viewport, notice that the value you entered for the shared vertex updated the level of both feature lines. As you see in the following image, the new common feature line level affected the infill gradings of both the ramp and gray infill area. When two feature lines share a vertex, the level of both feature lines at that vertex is determined by whichever of the feature lines was most recently edited.
    
    ![](../images/GUID-E7D23AFE-579E-4C31-8BDF-7447CD96CD6E.png)
    
9.  In the Grading Level Editor, in the ![](../images/GUID-9FDD1463-4AF9-46F7-89AC-482E325B9B20.png) row, change the Level value to **400.00’**.
10.  In the right viewport, select feature line ABC. Using the grip inside Circle A, move the beginning point of the feature line toward the lower left of Circle A.
     
     Note:
     
     You may need to hover over the feature line, and then use Shift+spacebar to select the feature line.
     
11.  Right-click the feature line. Select Level Editor.
     
     In the Grading Level Editor, the ![](../images/GUID-B79B8993-3660-46EC-A110-AA179335FDF7.png) icon indicates the point at which the feature line crosses the building pad. The white triangle indicates a _split point_, which is created when two feature lines cross at a location where neither one has a PI. Much like a ![](../images/GUID-9FDD1463-4AF9-46F7-89AC-482E325B9B20.png) shared vertex point, a split point acquires the level of the feature line that was most recently edited. If the other feature line has a different level, it gets a gradient break at the crossing point.
     
     Unlike a shared vertex, there is not an actual point at a split point, so you cannot directly edit the level. When you edit one of the feature lines, its gradient runs straight through the intersection, forcing the other feature line to break at the split point. You can use the Insert PI command to create a permanent point at that location on one of the feature lines. After you convert a split point to a permanent point, you can edit the level of a split point directly, and have better control over that point.
     
12.  Select the grip at the beginning point of the feature line. On the command line, enter **END** to apply an endpoint OSNAP. Snap the feature line to the building pad feature line.
13.  In the Grading Level Editor, click ![](../images/GUID-7AB52249-9E7A-4721-8B1A-9728F53EF2B5.png). Click ![](../images/GUID-F8C0B433-DC00-48D7-BEF2-F0F99095A0CE.png)Flatten Gradient Or Levels.
14.  In the Flatten dialog box, select Constant Gradient. Click OK.
     
     The gradient flattens, and the levels update to accommodate the new gradient.
     

**Parent topic:** [Tutorial: Using Feature Lines to Modify a Grading](GUID-CB2B6E14-14BA-4EB9-A88B-90E315257D45.htm "This tutorial demonstrates how to use feature lines to control grading around inside corners.")