---
title: "Exercise 2: Assigning Feature Line Levels"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-C8D3F575-8367-4B04-9537-D8868C948E59.htm"
category: "tutorial_creating_gradings"
last_updated: "2026-03-17T18:42:39.582Z"
---

                  Exercise 2: Assigning Feature Line Levels  

# Exercise 2: Assigning Feature Line Levels

In this exercise, you will assign levels to the feature lines you created from AutoCAD lines in the previous exercise.

This exercise continues from [Exercise 1: Creating Feature Lines](GUID-44EDAD16-E9FE-41BD-96A8-A3E32844568A.htm "In this exercise, you will use two different methods to create feature lines.").

Edit feature line levels

Note:

This exercise uses _Grading-2.dwg_ with the modifications you made in the previous exercise.

2.  Click Modify tab ![](../images/ac.menuaro.gif)Edit Levels panel ![](../images/ac.menuaro.gif)Level Editor ![](../images/GUID-AC155D52-45FC-42D0-8E2D-9595F606FB0D.png) Find.
3.  Click the feature line that stretches from circles C, D, E, to F.
    
    In the Grading Level Editor, you see the length of each segment and levels at each vertex. Notice that as you specified in [Exercise 1: Creating Feature Lines](GUID-44EDAD16-E9FE-41BD-96A8-A3E32844568A.htm "In this exercise, you will use two different methods to create feature lines."), the level of the first point is 688.000 and the level of the last point matches the level of the surface. The level values of the two intermediate points are automatically interpolated based on the beginning and end-point values. You can use this table to edit the level and gradient values. Now you will assign level values to line AB.
    
4.  Click ![](../images/GUID-8B243375-9F97-4BBA-9333-91D9267E95C0.png), and then click the feature line between circles A and B. The Grading Level Editor now displays the levels along the feature line between circles A and B.
5.  Double-click the level value for the starting chainage (0+00.00), and change it to **630.00’**. This value puts it a few feet below the surface. Change the level of the end point to **690.00’**.
    
    In the next few steps, you will insert an level point on the feature line.
    

Insert an level point on a feature line

1.  In the Grading Level Editor, click ![](../images/GUID-F006712C-51FA-4076-A133-6A84D1BA6B0C.png) (Insert Level Point).
    
    On feature line AB, you see a small circle and a tooltip that shows the chainage value and level of the point. You can use the cursor to move this point to a new location.
    
2.  Move the point to a location near the middle of the feature line, then click.
3.  In the Insert VIP dialog box, optionally move the level point to a specific chainage. Click OK.
    
    The point is added to the table in the Grading Level Editor, where you can edit the chainage, level, gradient, and length (distance between points).
    
4.  Click feature line AB.
    
    Editing grips are displayed for the endpoints and level point. You can click the level point and slide it along the feature line. You can also click one of the endpoints and move it to a new location. When you do any grip editing, values in the Grading Level Editor update.
    
5.  Move the cursor over the surface near the level point to see the surface level displayed.
    
    ![](../images/GUID-5740F42C-CC26-4C5A-A24C-82DE1488FB87.png)
    
6.  In the Grading Level Editor, set the level point to an level that is at or below the surrounding surface.
    
    In the next few steps, you will drape feature line BC across the existing ground surface. This command assigns an level to each vertex of the feature line.
    

Drape a feature line on a surface

1.  If necessary, press Esc once or twice to deselect feature line AB.
2.  Click Modify tab ![](../images/ac.menuaro.gif)Edit Levels panel ![](../images/ac.menuaro.gif)Levels From Surface ![](../images/GUID-E5A437DB-B390-4F75-AC19-14B1F7140BA7.png) Find.
3.  In the Set Levels From Surface dialog box, ensure that Insert Intermediate Gradient Break Points is selected and that Relative Level To Surface is cleared. click OK.
    
    Note: For more information about using the Relative Level To Surface option to make a feature line relative to and dynamic to a surface, see [About Feature Lines That Are Relative to Surfaces](https://beehive.autodesk.com/community/service/rest/cloudhelp/resource/cloudhelpchannel/guidcrossbook/?v=2025&p=CIV3D&l=ENG&accessmode=live&guid=GUID-62AEE70C-34BA-4B1D-8BD0-E94E4A8C4FF0).
    
4.  Click feature line BC, then right-click and click Enter to end the command.
5.  Click feature line BC again. An level point has been added wherever the line crosses the edge of a triangle in the TIN surface.
6.  Right click and click Level Editor.
    
    The Level Editor displays data for each level point along feature line BC, including its level, and the distance and gradient to the next point. A feature line on the surface like this can be a useful starting point for a grading. You can use controls along the top of the Grading Level Editor to add and delete level points, and to adjust their levels. You can select multiple points within the table for group operations, such as raising or lowering them the same amount, or “flattening” their levels to the same value.
    
7.  Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to close the Grading Level Editor.

To continue this tutorial, go to [Exercise 3: Creating a Grading](GUID-CC753396-5BFA-4FEE-9122-968B3ED5F9B5.htm "In this exercise, you will create a set of gradings, called a grading group, that form a runoff on the side of an embankment.").

**Parent topic:** [Tutorial: Creating Gradings](GUID-59CA5821-CC8F-499A-8F89-655B6D41CA0F.htm "This tutorial demonstrates how to create a feature line and how to gradient from the feature line.")