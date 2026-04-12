---
title: "Exercise 2: Moving Labels in a Data Box"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-A7EEB5AB-57E0-49B6-B850-CB8EE2E086C7.htm"
category: "tutorial_working_with_data_boxes"
last_updated: "2026-03-17T18:42:32.728Z"
---

                  Exercise 2: Moving Labels in a Data Box  

# Exercise 2: Moving Labels in a Data Box

In this exercise, you will learn how to rearrange labels in data boxes.

Data box labels may overlap one another if the points they label are close together. In this exercise, you will learn how to stagger a series of labels in a data box, and then move individual data box labels to specific locations.

This exercise continues from [Exercise 1: Adding Data Boxes to a Profile View](GUID-6DE2A16B-8CCD-4B0E-8AD0-E26DDA33619B.htm "In this exercise, you will add data boxes along the bottom of a profile view.").

Stagger data box labels

1.  Open Profile-6B.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The data boxes at the bottom of profile view PV - (3) annotate the horizontal and vertical geometry of the proposed road alignment, and the level of each profile at the major chainages.
    
2.  Zoom in to the following area.
    
    ![](../images/GUID-2F25F95D-D6AC-4C1F-B502-05D8E4B1D51F.png)
    
    The horizontal and vertical geometry labels overlap in this area.
    
3.  Select one of the geometry labels in this band. Right-click. Click Properties.
4.  In the Properties palette, under Staggering, specify the following parameters:
    
    *   Auto Stagger: **Stagger Both Sides**
    *   Stagger Line Height: **0.0250**
    
    The band labels are evenly spaced along the data box, and leader lines are created to the label anchor points.
    
    ![](../images/GUID-D42F1BB1-29D2-49D4-9DE8-4FD4E69F3FAD.png)
    

Move data box labels

1.  In the bottom data box, Ctrl+click the following label.
    
    ![](../images/GUID-EC83FDF7-51A5-4593-BBD8-794CBA74773F.png)
    
2.  Drag the grip down and to the right. Click to place the label.
    
    ![](../images/GUID-7E08BE1E-2CAE-48EF-A7CA-9CBA7AE3FFCF.png)
    
3.  Click the ![](../images/GUID-97BC4CB7-B524-43BF-8F10-73447BAE260A.png) grip. Drag the grip to the left. Click to place the grip.
    
    A new vertex is added to the label leader line.
    
    ![](../images/GUID-AB85202F-E3A1-494E-B15B-D4DEFA2EA6B5.png)
    
4.  Repeat Steps 1 through 3 to move the labels that are to the left of the one you just moved.
    
    ![](../images/GUID-EF8706A2-BA4B-43F5-9163-8F4FFD3D62DD.png)
    

To continue this tutorial, go to [Exercise 3: Modifying a Data Box Style](GUID-D3416B2C-884F-4C49-B408-7A1CE8C40E0D.htm "In this exercise, you will learn how to change the data that is displayed in a data box.").

**Parent topic:** [Tutorial: Working with Data Boxes](GUID-9CC02D3F-84C0-4574-89F0-61FBB809D683.htm "This tutorial demonstrates how to add and change the appearance of data boxes in a profile view.")