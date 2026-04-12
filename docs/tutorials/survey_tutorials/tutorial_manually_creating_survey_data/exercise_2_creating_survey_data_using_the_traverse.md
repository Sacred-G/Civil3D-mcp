---
title: "Exercise 2: Creating Survey Data Using the Traverse Editor"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-C87A5A42-4E75-4FC8-B55C-84063755A07C.htm"
category: "tutorial_manually_creating_survey_data"
last_updated: "2026-03-17T18:42:18.357Z"
---

                  Exercise 2: Creating Survey Data Using the Traverse Editor  

# Exercise 2: Creating Survey Data Using the Traverse Editor

In this exercise, you will use the Traverse Editor to create survey data.

The Traverse Editor is used to edit the observations of an existing named traverse or to enter traverse observations for a new traverse.

This exercise continues from [Exercise 1: Creating Survey Data Using the Toolspace Survey Tab](GUID-4011241C-25FD-43BB-8D6F-45F5CCD65543.htm "In this exercise, you will use the ToolspaceSurvey tab to create survey data.").

Create a new survey database

1.  Open _Survey-4B.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Survey tab, right-click Survey Databases. Click New Local Survey Database.
3.  In the New Local Survey Database dialog box, for the name, enter **Survey 3**. Click OK.
4.  In Toolspace, on the Survey tab, right-click the database **Survey 3**. Click Edit Survey Database Settings.
5.  In the Survey Database Settings dialog box, under Measurement Type Defaults, specify the following parameters:
    *   Angle Type: **Angle**
    *   Distance Type: **Horizontal**
    *   Vertical Type: **None**
    *   Target Type: **None**
6.  Click OK.

Create a survey network

1.  In Toolspace, on the Survey tab, expand the **Survey 3** database. Right-click the Networks collection. Click New.
2.  In the Network dialog box, for Name, enter **Survey Network 3**.
3.  Click OK.

Set up a traverse and define control points

1.  In Toolspace, on the Survey tab, expand the Survey Databases![](../images/ac.menuaro.gif)**Survey 3**![](../images/ac.menuaro.gif)Networks![](../images/ac.menuaro.gif)**Survey Network 3**. Right-click the Traverses collection. Click New.
2.  In the New Traverse dialog box, for Name, enter **Traverse 3**. Click OK.
3.  On the Survey tab, select the Traverses collection. In the list view, right-click **Traverse 3**. Click Edit Traverse.
4.  In the Specify Initial Setup dialog box, specify the following parameters:
    *   Initial Chainage: **1**
    *   Initial Backsight: **4**
5.  Click OK.
6.  A message is displayed indicating that initial chainage point 1 is not defined. Click Yes to define it.
7.  In the New Control Point dialog box, specify the following parameters:
    *   Point Number: **1**
    *   Easting: **1000**
    *   Northing: **1000**
    *   Description: **STA 1**
8.  Click OK.
9.  A message is displayed indicating that backsight point 4 is not defined. Click Yes to define it.
10.  A message is displayed indicating that a backsight direction must be created. Click Yes to create it.
11.  In the New Direction dialog box, specify the following parameters:
     *   Direction: **45**
     *   Direction Type: **Whole Circle Bearing**
12.  Click OK.
     
     The Traverse Editor is displayed.
     
13.  In the right side of the editor, for **Chainage 1, Backsight 4**, specify the following parameters:
     *   ![](../images/GUID-6723542D-694B-49FC-881A-22782EAD2D93.png) p
     *   (point number): **2**
     *   Angle: **90**
     *   Distance: **100**
     *   Description: **STA 2**
14.  For **Chainage 2, Backsight 1**, specify the following parameters:
     *   ![](../images/GUID-6723542D-694B-49FC-881A-22782EAD2D93.png) (point number): **3**
     *   Angle: **90**
     *   Distance: **100**
     *   Description: **STA 3**
15.  For **Chainage 3, Backsight 2**, specify the following parameters:
     *   ![](../images/GUID-6723542D-694B-49FC-881A-22782EAD2D93.png) (point number): **4**
     *   Angle: **90**
     *   Distance: **100**
     *   Description: **STA 4**
16.  For **Chainage 4, Backsight 3**, specify the following parameters:
     
     *   ![](../images/GUID-6723542D-694B-49FC-881A-22782EAD2D93.png) (point number): **1**
     *   Angle: **90**
     *   Distance: **100**
     
     Note: If you do not see an entry option for **Chainage 4, Backsight 3** in the Traverse Editor, save the traverse information by clicking ![](../images/GUID-753C7052-9AE5-46FF-A230-5E2920D3224C.png) and then close and reopen the Traverse Editor.
     
17.  Click ![](../images/GUID-753C7052-9AE5-46FF-A230-5E2920D3224C.png) to save the traverse information.
18.  On the Survey tab, right-click **Survey Network 3** and click Insert Into Drawing.
     
     The new survey data is displayed in the drawing.
     
     ## The following illustration shows what the network should look like.
     
     ![](../images/GUID-E3587911-DEA2-48E6-AB65-0B165308E0C7.png)
     

To continue this tutorial, go to [Exercise 3: Creating Survey Data Using the Survey Command Window](GUID-FCEF073D-3C15-49F6-844A-89BB37F22FDB.htm "In this exercise, you will create survey data using the Survey Command Window.").

**Parent topic:** [Tutorial: Manually Creating Survey Data](GUID-4ECE08C3-B0C0-4D90-B24C-C4BC0A8FAA41.htm "This tutorial demonstrates how to manually create and add survey data.")