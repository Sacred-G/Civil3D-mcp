---
title: "Exercise 1: Creating Survey Data Using the Toolspace Survey Tab"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-4011241C-25FD-43BB-8D6F-45F5CCD65543.htm"
category: "tutorial_manually_creating_survey_data"
last_updated: "2026-03-17T18:42:18.310Z"
---

                 Exercise 1: Creating Survey Data Using the Toolspace Survey Tab  

# Exercise 1: Creating Survey Data Using the Toolspace Survey Tab

In this exercise, you will use the ToolspaceSurvey tab to create survey data.

The Survey tab provides centralized access to survey data, settings, and various panorama vista editors, which can be used to create, edit, and manage survey data.

Create a new survey database

1.  Open _Survey-4A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Survey tab, right-click Survey Databases. Click New Local Survey Database.
3.  In the New Local Survey Database dialog box, for the name, enter **Survey 2**. Click OK.
4.  In Toolspace, on the Survey tab, right-click the database **Survey 2**. Click Edit Survey Database Settings.
5.  In the Survey Database Settings dialog box, under Measurement Type Defaults, specify the following parameters:
    *   Angle Type: **Angle**
    *   Distance Type: **Horizontal**
    *   Vertical Type: **None**
    *   Target Type: **None**
6.  Click OK.

Create a survey network

1.  In Toolspace, on the Survey tab, expand the **Survey 2** database. Right-click the Networks collection. Click New.
2.  In the Network dialog box, for Name, enter **Survey Network 2**.
3.  Click OK.

Create a control point

1.  On the Survey tab, expand **Survey Network 2**. Right-click the Control Points collection. Click New.
2.  In the New Control Point dialog box, specify the following parameters:
    *   Number: **1**
    *   Easting: **1000.0000**
    *   Northing: **1000.0000**
    *   Description: **STA 1**
3.  Click OK.

Create a direction

1.  On the Survey tab, under **Survey Network 2**, right-click the Directions collection. Click New.
2.  In the New Direction dialog box, specify the following parameters:
    *   From Point: **1**
    *   To Point: **4**
    *   Direction: **45.0000**
    *   Direction Type: **Whole Circle Bearing**
3.  Click OK.

Create setups and observations

1.  On the Survey tab, under **Survey Network 2**, right-click the Setups collection. Click New.
2.  In the New Setup dialog box, specify the following parameters:
    *   Chainage Point: **1**
    *   Backsight Point: **4**
3.  Press Tab to move to the next field.
    
    A dialog box is displayed indicating that point 4 is not defined.
    
4.  Click No, and then click OK to create the setup.
5.  On the Survey tab, under **Survey Network 2**, select the Setups collection. In the list view, right-click the setup **Chainage: 1, Backsight: 4**. Click Edit Observations.
6.  In the Observations Editor, right-click in the grid. Click New.
7.  For the new observation, specify the following parameters:
    *   Point Number: **2**
    *   Angle: **90.0000**
    *   Distance: **100.00**
    *   Description: **STA 2**
8.  Click ![](../images/GUID-753C7052-9AE5-46FF-A230-5E2920D3224C.png) to save the new observation.
9.  On the Survey tab, under **Survey Network 2**, right-click the Setups collection. Click New.
10.  In the New Setup dialog box, specify the following parameters:
     *   Chainage Point: **2**
     *   Backsight Point: **1**
11.  Click OK.
12.  On the Survey tab, under **Survey Network 2**, expand the Setups collection. Right-click the setup **Chainage: 2, Backsight: 1**. Click Edit Observations.
13.  In the Observations Editor, right click in the grid. Click New.
14.  For the new observation, specify the following parameters:
     *   Point Number: **3**
     *   Angle: **90.0000**
     *   Distance: **100.00**
     *   Description: **STA 3**
15.  Click ![](../images/GUID-753C7052-9AE5-46FF-A230-5E2920D3224C.png) to save the new observation.
16.  Follow steps 9 through 15 to create another setup with an observation.
     
     Information for the new setup:
     
     *   Chainage Point: **3**
     *   Backsight Point: **2**
     
     Information for the new observation:
     
     *   Point Number: **4**
     *   Angle: **90.0000**
     *   Distance: **100.00**
     *   Description: **STA 4**
17.  Follow steps 9 through 15 to create another setup with an observation.
     
     Information for the new setup:
     
     *   Chainage Point: **4**
     *   Backsight Point: **3**
     
     Information for the new observation:
     
     *   Point Number: **1**
     *   Angle: **90.0000**
     *   Distance: **100.00**
18.  On the Survey tab, right-click **Survey Network 2**. Click Insert Into Drawing.
     
     The new survey data is displayed in the drawing.
     
     ## Click to see what the network should look like.
     
     ![](../images/GUID-E3587911-DEA2-48E6-AB65-0B165308E0C7.png)
     

To continue this tutorial, go to [Exercise 2: Creating Survey Data Using the Traverse Editor](GUID-C87A5A42-4E75-4FC8-B55C-84063755A07C.htm "In this exercise, you will use the Traverse Editor to create survey data.").

**Parent topic:** [Tutorial: Manually Creating Survey Data](GUID-4ECE08C3-B0C0-4D90-B24C-C4BC0A8FAA41.htm "This tutorial demonstrates how to manually create and add survey data.")