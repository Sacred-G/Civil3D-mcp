---
title: "Exercise 1: Specifying Profile Design Criteria"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-69526F4A-5C27-4F95-A63D-EADE91D0412A.htm"
category: "tutorial_designing_a_profile_that_refers_to_local_"
last_updated: "2026-03-17T18:42:31.022Z"
---

                 Exercise 1: Specifying Profile Design Criteria  

# Exercise 1: Specifying Profile Design Criteria

In this exercise, you will specify minimum standards for a layout profile.

Specify minimum profile design standards

1.  Open Profile-4A.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Profile drop-down ![](../images/ac.menuaro.gif)Profile Creation Tools ![](../images/GUID-78E66A2B-A0B5-40F5-99DB-82B70D6AAB1A.png) Find.
3.  Click one of the grid lines to select the profile view.
4.  In the Create Profile – Draw New dialog box, on the General tab, specify the following parameters:
    *   Name: **Main Road**
    *   Profile Style: **Design Profile**
    *   Profile Label Set: **<none>**
5.  On the Design Criteria tab, select the Use Criteria-Based Design check box.
    
    The Use Design Criteria File, Default Criteria, and Use Design Check Set options are now available. The design criteria file that was selected by default is the same file that was applied to the parent alignment. You may choose to use a different design criteria file for the profile. For this exercise, you will accept the default.
    
6.  Under Use Design Check Set, click the arrow next to ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png). Click ![](../images/GUID-BD2CA031-6C63-4F6F-925A-6C455E33D4E1.png)Create New.
    
    In the next few steps, you will create a new design check set to validate that the sag and crest curves meet a minimum length value.
    
7.  In the Profile Design Check Set dialog box, on the Information tab, for Name, enter **Profile Curve Length**.
8.  On the Design Checks tab, in the Type list, select Curve. In the Curve Checks list, select **L>=30**. Click Add.
9.  In the Design Check table, in the Apply To column, select Crest Curves Only.
10.  Repeat Steps 8 and 9 to add the **L>=60** curve design check to the design check set. In the Apply To column, select Sag Curves Only.
11.  Click OK.
12.  In the Create Profile - Draw New dialog box, click OK.
     
     The Profile Layout Tools toolbar is displayed in the drawing window. You can start drawing the layout profile that refers to the criteria you specified.
     

To continue this tutorial, go to [Exercise 2: Drawing a Profile that Refers to Design Criteria](GUID-CE04E656-714D-4164-A499-EA9F482A237F.htm "In this exercise, you will draw a profile that refers to specified minimum standards.").

**Parent topic:** [Tutorial: Designing a Profile that Refers to Local Standards](GUID-75B9BF20-42D1-4D86-9465-C906A03FDC16.htm "This tutorial demonstrates how to validate that your profile design meets criteria specified by a local agency.")