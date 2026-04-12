---
title: "Exercise 1: Creating Description Keys"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-0C6FE34A-3273-4150-9260-AC906D357FD4.htm"
category: "tutorial_creating_point_data"
last_updated: "2026-03-17T18:42:06.924Z"
---

                  Exercise 1: Creating Description Keys  

# Exercise 1: Creating Description Keys

In this exercise, you will create description keys to sort the points as they are imported into a drawing.

Create a description key set

1.  Open Points-1.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Settings tab, expand the Point collection.
3.  Right-click Description Key Sets. Click New.
4.  In the Description Key Set dialog box, Name field, enter **Stormwater Keys**.
5.  In the Description field, enter **Stormwater manhole and pond points**.
6.  Click OK.
    
    The new description key set is created.
    

Create description keys

1.  In Toolspace, on the Settings tab, expand the Description Key Sets collection. Right-click **Stormwater Keys**. Click Edit Keys. The DescKey Editor vista is displayed in the Panorama window.
    
    In the DescKey Editor, you will enter the raw description codes, and specify how Autodesk Civil 3D handles new points that have these codes. All entries in the Code column of the DescKey Editor are case sensitive.
    
2.  In DescKey Editor, in the Code column, click the default entry. Change it to **POND\***.
    
    The asterisk is a wild-card character. The asterisk causes any imported point with a description code that begins with POND, followed by any other characters, to be handled according to the settings in this table row.
    
3.  In both the Style and Point Label Style columns, clear the check box to deactivate these settings.
    
    Clearing these settings allows you to control these settings by using point group properties.
    
    Note:
    
    The Format column contains the entry $\*, which specifies that a point’s raw description is copied without changes and used for the full description in the point label. This is an acceptable setting for the POND points.
    
4.  In the Layer column, select the check box. Click the cell to open the Layer Selection dialog box.
5.  In the Layer Selection dialog box, select **V-NODE-STRM**. Click OK.
    
    This setting means that the POND points reference the V-NODE-STRM layer for their display attributes. In the next few steps, you create another description key.
    
6.  In the Code column, right-click the **POND\*** entry. Click New.
7.  In the new description key, click the default Code entry and change it to **MHST\***.
8.  Set the same styles and layer as you did for POND\* by repeating Steps 3 through 5.
9.  In the Format column, enter **STORM MH**.
    
    This setting ensures that points with a raw description of MHST\* (stormwater manholes) are labeled in the drawing as STORM MH.
    
10.  Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to save the description keys and close the editor.

To continue this tutorial, go to [Exercise 2: Creating Point Groups](GUID-B65F5014-9D35-46D2-8C29-83721245C1AC.htm "In this exercise, you will create point groups to sort the points as they are imported into a drawing.").

**Parent topic:** [Tutorial: Creating Point Data](GUID-1A4FCB0E-F708-4AF5-B137-7942ECC3D819.htm "This tutorial demonstrates several useful setup tasks for organizing a large set of points.")