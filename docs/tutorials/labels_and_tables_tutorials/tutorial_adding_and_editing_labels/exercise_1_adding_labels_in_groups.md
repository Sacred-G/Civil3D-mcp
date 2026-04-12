---
title: "Exercise 1: Adding Labels in Groups"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-9A8CBBE8-FAE7-461F-B4B5-C35181213F4A.htm"
category: "tutorial_adding_and_editing_labels"
last_updated: "2026-03-17T18:43:16.246Z"
---

                  Exercise 1: Adding Labels in Groups  

# Exercise 1: Adding Labels in Groups

In this exercise, you will use label sets to apply several types of labels to an alignment.

You can automatically add labels as you create objects, such as points, alignments, or plots. Labeling an object automatically is an efficient way to annotate common elements, such as alignment stations or plot areas, as they are created.

In this exercise, you will specify a label set to apply as you create an alignment from a polyline. Both the newly created alignment and its labels will reside in the current drawing. Next, you will learn how to modify the properties of the label set after the alignment has been created. Finally, you will learn how to apply a label set to an alignment that exists in an externally referenced drawing.

Create a label set for a new alignment

Note:

This exercise uses _Labels-1a.dwg_ with the modifications you made in the previous exercise, or you can open _Labels-2a.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Alignment drop-down ![](../images/ac.menuaro.gif)Create Alignment From Objects ![](../images/GUID-6A159C4E-3903-4519-A90D-58731DA1008F.png) Find. On the command line, enter **X** to select the Xref option.
3.  Click the blue polyline on the west side of the site.
    
    ![](../images/GUID-DA0EB79D-8C3E-4626-BB83-663BA31F9D42.png)
    
    Polyline in the externally referenced drawing
    
4.  Press Enter twice.
5.  In the Create Alignment - From Polyline dialog box, for Name, enter **West Street**. For Alignment Style, ensure that **Proposed** is selected. Examine the contents of the Alignment Label Set list.
    
    When you create an object, its Create dialog box typically has style selector lists for both the object and the labels. The style selector lists identify the object styles and label styles that are available in the current drawing for that object type. When you create an alignment, profile, or section, you select a _label set_, which applies a preset style to each of the various labels types that are in the set. You will examine an example of a label set in the following steps. Notice that there is a **\_No Labels** selection. This selection is an empty label set that does not display any labels along the alignment.
    
    Tip:
    
    If you do not want to annotate objects that do not use label sets, you can create a label style that has the visibility of all of its components turned off.
    
6.  From the Alignment Label Set list, select **Major Minor And Geometry Points** . Click the arrow next to ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png). Select ![](../images/GUID-CA1AD018-7C68-4826-8BAB-59D50446DD6F.png)Copy Current Selection.
    
    The Alignment Label Set dialog box displays information about how the Major Minor and Geometry Points label set is configured. You will use this label set as a basis to create a new label set.
    
    The Information tab displays the label set name, description, and the date when it was created or modified.
    
    The Labels tab specifies the label types that are defined in the label set, as well as the label styles that are used by each type. In this example, you use the label set to apply label styles to the geometry points and major and minor stations of an alignment.
    
    Note:
    
    Label sets for profiles and sections are constructed in the same manner, using a similar dialog box.
    
7.  In the Alignment Label Set dialog box, on the Information tab, for Name, enter **Major-Minor Stations And Start-End Points**.
8.  On the Labels tab, in the Geometry Points row, in the Style column, click ![](../images/GUID-FFC0BD3D-ED67-45AA-A13D-80611626C064.png)
9.  In the Pick Label Style dialog box, select **Alignment Start**. Click OK.
10.  In the Alignment Label Set dialog box, in the Geometry Points row, in the Geometry Points To Label column, click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
11.  In the Geometry Points dialog box, click ![](../images/GUID-2AA48B99-0C63-4F18-BC31-B5C0983656A5.png) to clear all check boxes. Select the Alignment Beginning check box. Click OK.
     
     You can use geometry point label types to label a selection of geometry points using a combination of styles that you specify. In steps 8 and 9, you applied a geometry point label style to the alignment starting chainage. In the following steps, you will create another instance of the Geometry Points label type that applies a different style to the alignment ending chainage.
     
     Tip:
     
     To remove a label type from the label set, select the type and click ![](../images/GUID-627FB583-4737-43B5-B407-A768EF513E84.png).
     
12.  In the Alignment Label Set dialog box, on the Labels tab, specify the following parameters:
     *   Type: **Geometry Points**
     *   Geometry Point Label Style: **Alignment End**
13.  Click Add.
14.  In the Geometry Points dialog box, click ![](../images/GUID-2AA48B99-0C63-4F18-BC31-B5C0983656A5.png) to clear all check boxes. Select the Alignment End check box. Click OK.
15.  In the Alignment Label Set dialog box, click OK.
16.  In the Create Alignment - From Polyline dialog box, under Conversion Options, clear the Add Curves Between Straights check box. Click OK.
17.  Zoom in to the area surrounding the new alignment and examine the labels that were automatically placed along the alignment.
     
     Note:
     
     If the EP: 0+243.63 label is adjacent to the intersecting alignment, click Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/GUID-A7096E57-563B-43D0-B57A-FA69853A76A6.gif)Reverse Direction ![](../images/GUID-A1294340-1EEE-4D8D-9BB8-BFCC8F33F5FE.png) Find. Click the alignment. Click OK to acknowledge the warning about alignment properties that are affected by the command.
     
     When the alignment is created, it is green. The green color is controlled by the alignment style that you specified in step 3. Notice that the new red labels are a brighter than the labels that were brought in with the externally referenced objects. The color tones are different so that you can easily identify where the labels reside: bright labels are in the current drawing, and light labels are in the externally referenced drawings.
     
     Note: Warning symbols (not shown in the following illustrations) are created in the drawing to indicate where tangency has been violated. For more information, see To Check for Tangency Between Alignment Entities.
     
     ![](../images/GUID-BB526022-9ACE-4CD0-89E4-C6D889D8845F.png)
     
     Label set applied to a newly created alignment
     

Modify the label set of an existing alignment

Note:

Changes that you make to the alignment label set after the alignment has been created will not be applied to the original label set. To edit the original label set, in Toolspace, on the Settings tab, expand Alignment![](../images/ac.menuaro.gif)Label Styles![](../images/ac.menuaro.gif)Label Sets. Right-click the appropriate label set. Click Edit.

2.  In the drawing, select the West Street alignment. Right-click. Click Edit Alignment Labels.
3.  In the Alignment Labels dialog box, specify the following parameters:
    *   Type: **Geometry Points**
    *   Geometry Points Label Style: **Perpendicular With Tick And Line**
4.  Click Add.
5.  In the Geometry Points dialog box, click ![](../images/GUID-2AA48B99-0C63-4F18-BC31-B5C0983656A5.png) to clear all check boxes. Select the following check boxes:
    *   Straight-Straight Intersect
    *   Straight-Curve Intersect
    *   Curve-Straight Intersect
6.  Click OK to close the Geometry Points and Alignment Label Set dialog boxes.
7.  Press Esc to deselect the alignment.
    
    ![](../images/GUID-223FC26A-9DE8-45CE-9501-41AFC4291222.png)
    
    Alignment with modified label set
    
    Note:
    
    In the previous image, the EP: 0+243.63 and PC: 0+158.39 labels are shown on opposite sides of the alignment for clarity. You will learn to flip labels to the opposite side of an alignment in a later exercise.
    

Add labels to an alignment in a referenced drawing

1.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Labels menu ![](../images/ac.menuaro.gif)Alignment![](../images/ac.menuaro.gif)Add/Edit Station Labels![](../images/GUID-10094573-05A5-439E-A30D-2E506775AA08.png). Click the Main Street alignment, which is the long alignment in the middle of the site.
    
    Because this alignment exists in an externally referenced drawing, the table in the Alignment Labels dialog box is empty. Only labels that are created in the current drawing can be modified in the current drawing. Labels that were created in an externally referenced drawing must be modified in the source drawing.
    
2.  In the Alignment Labels dialog box, specify the following parameters:
    *   Type: **Geometry Points**
    *   Geometry Point Label Style: **Perpendicular With Tick And Line**
3.  Click Add.
    
    Note:
    
    The Import Label Set button applies a label set that exists in the current drawing.
    
4.  In the Geometry Points dialog box, click OK.
5.  In the Alignment Labels dialog box, specify the following parameters:
    *   Type: **Design Speeds**
    *   Design Speed Label Style: **Chainage Over Speed**
6.  Click Add.
7.  Click OK.
    
    On the Main Street alignment, labels are displayed at each chainage at which a new design speed is applied, and at each geometry point. These label objects reside in the current drawing and annotate the alignment in the externally referenced drawing.
    
    ![](../images/GUID-4FF0D271-A874-4CBC-A56C-120D52A56D31.png)
    
    Labels added to an alignment in an externally referenced drawing
    

To continue this tutorial, go to [Exercise 2: Manually Labeling an Object](GUID-1297F239-F83C-4BF4-A0BA-6E268D221A4F.htm "In this exercise, you will add labels to specific areas on an alignment after it has been created and automatically labeled.").

**Parent topic:** [Tutorial: Adding and Editing Labels](GUID-38C5B56B-B2A1-49EB-8BD6-1BB1715EEB54.htm "This tutorial demonstrates how to add labels to Autodesk Civil 3D objects, and then edit the labels to suit your requirements.")