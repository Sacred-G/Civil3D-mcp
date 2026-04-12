---
title: "Exercise 1: Creating User-Defined Properties"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-0D327FB1-5B88-4904-9CBA-5F1FB0BF0B1E.htm"
category: "tutorial_adding_user_defined_properties_to_points"
last_updated: "2026-03-17T18:42:08.446Z"
---

                 Exercise 1: Creating User-Defined Properties  

# Exercise 1: Creating User-Defined Properties

In this exercise, you will learn how to create a user-defined property classification and add items to it.

Create a user-defined property classification

1.  Open Points-4a.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    The drawing is similar to the ones you used earlier in the Points tutorials, except only the points for storm manholes and the detention pond are visible.
    
2.  In Toolspace, on the Settings tab, expand the Point collection. Right-click User-Defined Property Classifications. Click New.
3.  In the User-Defined Property Classification dialog box, enter **Manhole UDP**.
4.  Click OK.
    
    The new classification is created and added to the list of user-defined property classifications.
    
5.  Repeat Steps 2 to 4 to create an additional user-defined property classification named **Trees**.

Define classification properties

1.  On the Settings tab, expand User-Defined Property Classifications. Right-click **Manhole UDP**. Click New.
2.  In the New User-Defined Property dialog box, for Name, enter **MH\_Pipe In Invert**.
3.  In the Property Field Type list, select Level.
4.  Use the default values for all other properties. Click OK.
    
    The property is added to the list of Manhole UDP properties.
    
5.  Repeat Steps 1 to 4 to add additional properties to the **Manhole UDP** classification, using the following parameters:
    
    Note:
    
    The next exercise uses Points-4b.dwg, which contains all of the properties and classifications. To save time, you can skip Steps 5 and 6 and proceed to [Exercise 2: Creating a Label Style That Displays a User-Defined Property](GUID-18D4A323-FF35-4CC0-B3F6-DB093F999F2D.htm "In this exercise, you will create a label style that displays user-defined property information for a point.").
    
    Name
    
    Property Field Type
    
    **MH\_Material**
    
    String
    
    **MH\_Diameter**
    
    Dimension
    
    **MH\_Pipe In Diameter**
    
    Dimension
    
    **MH\_Pipe In Material**
    
    String
    
    **MH\_Pipe Out Invert**
    
    Level
    
    **MH\_Pipe Out Diameter**
    
    Dimension
    
    **MH\_Pipe Out Material**
    
    String
    
6.  Repeat Steps 1 to 4 to add properties to the **Trees** classification using the following parameters:
    
    Name
    
    Property Field Type
    
    **Tree\_Common Name**
    
    String
    
    **Tree\_Genus**
    
    String
    
    **Tree\_Species**
    
    String
    
    **Tree\_Diameter**
    
    Dimension
    
    **Tree\_Height**
    
    Distance
    

To continue this tutorial, go to [Exercise 2: Creating a Label Style That Displays a User-Defined Property](GUID-18D4A323-FF35-4CC0-B3F6-DB093F999F2D.htm "In this exercise, you will create a label style that displays user-defined property information for a point.").

**Parent topic:** [Tutorial: Adding User-Defined Properties to Points](GUID-6F9EFF4C-4D8F-478B-A246-3FCC3B14230A.htm "This tutorial demonstrates how to add custom properties to points.")