---
title: "Exercise 3: Referencing Data Shortcuts"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-04D4992A-9BFE-41F9-8BB7-5B0D7BB48973.htm"
category: "tutorial_using_data_shortcuts"
last_updated: "2026-03-17T18:42:20.794Z"
---

                 Exercise 3: Referencing Data Shortcuts  

# Exercise 3: Referencing Data Shortcuts

In this exercise, you will reference several shortcuts in a new drawing.

This exercise continues from [Exercise 2: Creating Data Shortcuts](GUID-1BE3A220-97C8-46D7-B9BA-436133B7F160.htm "In this exercise, you will create data shortcuts from the objects in a drawing. The data shortcuts will be available to reference into other drawings.").

Reference data shortcuts in a new drawing

Note:

Before you perform this exercise, you must have created data shortcuts as described in the previous exercise.

2.  Open drawing _Project Management-2.dwg_, which is available in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing is empty. In the following steps, you will reference the objects for which you created data shortcuts in the previous exercise.
    
3.  In Toolspace, on the Prospector tab, in the ![](../images/GUID-93B3E175-74C4-41CA-9592-39DA738CF826.png)Data Shortcuts![](../images/ac.menuaro.gif)![](../images/GUID-0122809D-CF9C-46AA-BE12-CCF3CC92BD44.png)Surfaces collection, right-click **EG**. Click Create Reference.
    
    In the Create Surface Reference dialog box, notice that you can specify a Name, Description, Style, and Render Material for the surface. The parameters that define the object cannot be modified in the current drawing, but you can adjust the object properties. For this exercise, you will accept the existing property settings, except for the surface style.
    
4.  In the Create Surface Reference dialog box, in the Style row, click the Value column. Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
5.  In the Select Surface Style dialog box, select **Contours 5m and 25m (Background)**. Click OK.
6.  In the Create Surface Reference dialog box, click OK.
    
    The EG surface is displayed in the drawing using the style that you specified.
    
    Note:
    
    If you cannot see the surface, enter **ZE** on the command line.
    
    ![](../images/GUID-37C77735-BA88-4D7B-9D72-771A346FFB66.png)
    
    Now that the data shortcuts have been referenced, the current drawing is associated with the data shortcuts project. Notice that, in the Autodesk Civil 3D title bar, \[Tutorial Data Shortcuts Project\] is displayed after the drawing name.
    
7.  In Toolspace, on the Prospector tab, expand the Project Management-2 ![](../images/ac.menuaro.gif)![](../images/GUID-6BA9AC63-A03A-493C-8716-35AD405BF1FC.png)Surfaces collection.
    
    Notice that the EG surface is displayed in the Surfaces collection. The ![](../images/GUID-0122809D-CF9C-46AA-BE12-CCF3CC92BD44.png) icon indicates that the surface was created from a data shortcut.
    
8.  In the ![](../images/GUID-93B3E175-74C4-41CA-9592-39DA738CF826.png)Data Shortcuts![](../images/ac.menuaro.gif)![](../images/GUID-BC70BFC5-1271-4D5C-A684-893CC195DF39.png)Alignments collection, right-click **First Street**. Click Create Reference.
9.  In the Create Alignment Reference dialog box, click OK.
    
    The alignment is displayed in the drawing.
    
    ![](../images/GUID-731764FC-9550-4E8B-98BA-A0D0308DB084.png)
    

Create an object from the referenced objects

1.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Profile drop-down ![](../images/ac.menuaro.gif)Create Surface Profile ![](../images/GUID-35E8FF53-E099-47DA-9596-6CE0AC7C3547.png) Find.
2.  In the Create Profile From Surface dialog box, specify the following parameters:
    *   Alignment: **First Street**
    *   Select Surfaces: **EG**
3.  Click Add.
4.  Click Draw In Profile View.
5.  In the Create Profile View wizard, on the General page, for Profile View Style, select **Major Grids**.
6.  Click Create Profile View.
7.  In the drawing, click to place the profile view grid.
    
    The referenced surface and alignment are read-only objects in the drawing and require little storage space. You can use the referenced object data to create other objects in the current drawing, but you cannot change the source objects.
    
    ![](../images/GUID-2DD3D2E9-7FE3-4A3B-8397-C2A81B5B3DD4.png)
    

Save the current drawing

1.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save As.
2.  In the Save Drawing As dialog box, navigate to the [Civil 3D Projects folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24221) **\\Tutorial Data Shortcuts Project\\Production Drawings**. Click Save.
    
    Like source drawings, the final production drawings that contain references to other objects should be saved with the data shortcuts project.
    
    Next, you will modify the alignment in the source drawing, and then update the current drawing to reflect the changes.
    

Change a referenced object in the source drawing

1.  In Toolspace, on the Prospector tab, click _Project Management-1.dwg_. Right-click. Click Switch To.
2.  Select the alignment.
3.  Move the grips to change the alignment layout.
    
    ![](../images/GUID-2800544F-F253-48B7-9680-8E5066B21798.png)
    
4.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save.

Synchronize the current drawing to the source drawing

1.  In Toolspace, on the Prospector tab, click _Project Management-2.dwg_. Right-click. Click Switch To.
2.  Expand the _Project Management-2_![](../images/ac.menuaro.gif)Alignments![](../images/ac.menuaro.gif)Centerline Alignments collection.
    
    The ![](../images/GUID-C18CD8F2-5645-4FF4-827C-CC284AFF9A6B.png) icon indicates that the reference to the source drawing is out of date.
    
3.  Right-click the First Street alignment. Click Synchronize.
    
    Notice that the alignment is updated to reflect the changes you made in the source drawing. Also notice that the surface profile that you created from the alignment has been updated.
    
    ![](../images/GUID-7CEA4C60-2BDF-449E-89A3-D0C7145D668A.png)
    

Note:

This is the end of the data shortcuts tutorial. The next tutorials in this section demonstrate how to structure a project using Autodesk Vault.

To continue to the next tutorial, go to [Vault Setup](GUID-05E12EDB-B70D-4E41-9C06-A304036C9C74.htm "In this tutorial, you will act as a project administrator, creating a project in Autodesk Vault and some sample users.").

**Parent topic:** [Tutorial: Using Data Shortcuts](GUID-68AC7599-F635-4391-9F9A-6E9EF96F9ED2.htm "This tutorial demonstrates how to create a data shortcuts project, create data shortcuts from objects in one drawing, and then import the data shortcuts into another drawing.")