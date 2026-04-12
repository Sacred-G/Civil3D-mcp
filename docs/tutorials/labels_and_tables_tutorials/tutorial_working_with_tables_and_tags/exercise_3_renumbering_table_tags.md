---
title: "Exercise 3: Renumbering Table Tags"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-92032A66-357F-4241-8706-C1DB23075D81.htm"
category: "tutorial_working_with_tables_and_tags"
last_updated: "2026-03-17T18:43:18.673Z"
---

                  Exercise 3: Renumbering Table Tags  

# Exercise 3: Renumbering Table Tags

In this exercise, you will renumber the table tags you created in the previous exercise.

You will renumber the curve table tags around the cul-de-sac on the East Street alignment so that they follow a clockwise pattern. You will use the Table Tag Numbering dialog box that you examined in the previous exercise to specify the starting number and increment with which to renumber the table tags.

This exercise continues from [Exercise 2: Converting Labels to Tags](GUID-8B19DF3C-9CC3-4765-86A4-E422F2FD5A5F.htm "In this exercise, you will create some plot segment labels, and then convert the labels to tags and move the data into a table.").

Examine the table tag numbering settings

Note:

This exercise uses _Labels-4a.dwg_ with the modifications you made in the previous exercise, or you can open _Labels-4b.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  In Toolspace, on the Settings tab, right-click the drawing name. Click Table Tag Numbering.
3.  In the Table Tag Numbering dialog box, notice that the Table Tag Creation Starting Number values are not 1 as they were [Exercise 2: Converting Labels to Tags](GUID-8B19DF3C-9CC3-4765-86A4-E422F2FD5A5F.htm "In this exercise, you will create some plot segment labels, and then convert the labels to tags and move the data into a table.").
    
    The Starting Number values are the next available numerals based on the line, curve, and transition tags that exist in the drawing and the specified increment value. If you add more table tags in the drawing, these values prevent the duplication of tag numbers.
    
    Note:
    
    The Table Tag Numbering dialog box specifies the tag numbering settings for all objects. The settings that you specify in the following steps will apply to table tags for all objects.
    
4.  Under Table Tag Renumbering, accept the default values.
    
    For this exercise, you will restart the curve numbering at 1 and use an increment of 1.
    
5.  Click OK.

Renumber the table tags

1.  In the drawing, zoom in to the area around the cul-de-sac at the end of the East Street alignment.
2.  Click a label tag to select it. Click Labels tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Renumber Tags.
3.  Click tag **C6**.
    
    You are notified that tag number 1 already exists. If you press Enter, the next available curve tag will be applied to this tag. For this exercise, a duplicate tag is acceptable, because you will continue the renumbering to resolve duplicates.
    
4.  On the command line, enter **C** to create a duplicate curve table tag.
    
    The curve table tag now appears as C1.
    
5.  Press Esc.
6.  Repeat steps 3 through 5 on the remaining curve table tags, so that the curve table tags are labeled C1 through C7 clockwise around the cul-de-sac.
    
    ![](../images/GUID-CF5FBF36-20F1-4923-BC9F-C5A42061A583.png)
    
    Table tags: original (left) and renumbered (right)
    
7.  Pan to the plot line and curve table you created in [Exercise 2: Converting Labels to Tags](GUID-8B19DF3C-9CC3-4765-86A4-E422F2FD5A5F.htm "In this exercise, you will create some plot segment labels, and then convert the labels to tags and move the data into a table.").
    
    Notice that the table has automatically updated to reflect the new curve numbering.
    
    **Further exploration**: Add segment labels to the rest of the plots along the East Street alignment, and then convert the labels to table tags. Renumber the table tags along the highway boundary so that they follow a clockwise pattern.
    
    ![](../images/GUID-FDDB977D-D796-4B39-B0CB-F8D61A119E12.png)
    
    Table tags renumbered along highway boundary
    

To continue to the next tutorial, go to [Working with Label Styles](GUID-F663DB98-120E-4A8D-9762-CB799972916A.htm "This tutorial demonstrates how to define the behavior, appearance, and content of labels using label styles.").

**Parent topic:** [Tutorial: Working with Tables and Tags](GUID-7052010C-3307-4A41-AFDB-39763F830C6B.htm "This tutorial demonstrates how to place object data into tables.")