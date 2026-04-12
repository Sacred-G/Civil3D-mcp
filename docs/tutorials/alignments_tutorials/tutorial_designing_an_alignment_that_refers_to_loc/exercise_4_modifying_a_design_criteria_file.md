---
title: "Exercise 4: Modifying a Design Criteria File"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E781CDDC-6D6C-4DEB-AF87-74A758671FAF.htm"
category: "tutorial_designing_an_alignment_that_refers_to_loc"
last_updated: "2026-03-17T18:42:27.425Z"
---

                   Exercise 4: Modifying a Design Criteria File  

# Exercise 4: Modifying a Design Criteria File

In this exercise, you will add a radius and speed table to the design criteria file.

If your local agency standards differ from the standards in the supplied design criteria file, you can use the Design Criteria Editor dialog box to customize the file to support your local standards.

In this exercise, you will add a minimum radius table to an existing design criteria file, and then save the file under a new name.

This exercise continues from [Exercise 3: Working with Design Checks](GUID-18AF0EFE-37DA-4B04-B663-7BC6B4A5CCF3.htm "In this exercise, you will create an alignment design check, add the design check to a design check set, and then apply the design check set to an alignment.").

Add a minimum radius table

1.  Open _Align-7C.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the drawing, select the alignment.
3.  Click Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Design Criteria Editor ![](../images/GUID-BA51450F-C824-49DE-A9B1-DDF708320471.png) Find.
    
    When the Design Criteria Editor dialog box opens, it displays the design criteria for the default design criteria file. The folders on the left side of the dialog box contain tables that specify the units of measure used in the design criteria file, and design criteria tables for alignments and profiles. You can use this dialog box to modify the criteria in the current file, open another file, or create a new file. In the following steps, you will add a criteria table to an existing file, and then save the changes as a new file.
    
4.  On the left side of the dialog box, expand the Alignments![](../images/ac.menuaro.gif)Minimum Radius Tables collection.
    
    The collection contains several minimum radius tables.
    
5.  Right-click Minimum Radius Tables. Click New Minimum Radius Table.
    
    An empty ![](../images/GUID-1B68397A-998A-4198-A583-9E01430FCE9A.png) table appears at the end of the Minimum Radius Tables collection.
    
6.  Double-click the new entry and replace the New Minimum Radius Table text with **Local Standards eMax 7%**. Press Enter.

Save the design criteria file

1.  Click ![](../images/GUID-105CFC68-2C5B-42D7-84FF-D7B1256FBE18.png)Save As.
2.  In the Enter A File Name To Save dialog box, navigate to the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). In the File Name field, enter **Sample\_Local\_Criteria.xml**. Click Save.
    
    If a design criteria file must be shared by multiple users, it must be saved in a location to which all applicable users have access. If you send a drawing that uses a custom design criteria file to a user that does not have access to the shared location, then you also must send the design criteria file.
    
    Tip:
    
    When the Use Design Criteria File option is selected during alignment creation, the first design criteria found in the [Data\\Corridor Design Standards\\\[units\] folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24223) is applied to an alignment by default. To ensure that a custom design criteria file is selected by default, make sure that its name places it first in the directory.
    

Add criteria to a table

1.  In the Design Criteria File Editor dialog box, on the left-hand side, ensure that the **Local Standards eMax 7%** table is selected.
2.  On the right-hand side of the dialog box, select the first row in the table. Click the Speed cell. Select **50** from the list.
3.  Click the Radius cell. Enter **54**. Press Enter.
4.  Repeat steps 2 and 3 to add the following values to the table:
    
    Speed
    
    Radius
    
    70
    
    125
    
    90
    
    235
    
    110
    
    387
    
    130
    
    586
    
    Tip:
    
    To add a row between two existing rows, click ![](../images/GUID-7633B109-B0E2-42D8-8A07-E27BBF28B731.png). To remove a row, click ![](../images/GUID-627FB583-4737-43B5-B407-A768EF513E84.png).
    
5.  In the Comments field, enter **Minimum radii for 7% superelevation at various design speeds**.
    
    **Further exploration:** Expand the other collections on the left-hand side of the dialog box. Right-click the various folders and tables and examine the options that are available.
    
6.  Click Save And Close. When you are notified that the file has unsaved changes, click Save Changes And Exit.
    
    **Further exploration:** Use the Alignment Properties dialog box to apply the new **Sample\_Local\_Criteria.xml** design criteria file and **Local Standards eMax 7%** table to the alignment in _Align-4b.dwg_.
    

To continue to the next tutorial, go to [Applying Superelevation to an Alignment](GUID-AA0068E0-2858-4067-9104-161112DEDBF6.htm "In this tutorial, you will calculate superelevation for alignment curves, create a superelevation view to display the superelevation data, and edit the superelevation data both graphically and in a tabular format.").

**Parent topic:** [Tutorial: Designing an Alignment that Refers to Local Standards](GUID-CAEC3077-D78A-4F42-8E47-41FABAB6915D.htm "This tutorial demonstrates how to validate that your alignment design meets criteria specified by a local agency.")