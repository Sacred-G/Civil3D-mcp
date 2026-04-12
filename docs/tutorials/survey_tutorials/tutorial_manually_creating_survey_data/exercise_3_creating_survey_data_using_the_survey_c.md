---
title: "Exercise 3: Creating Survey Data Using the Survey Command Window"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-FCEF073D-3C15-49F6-844A-89BB37F22FDB.htm"
category: "tutorial_manually_creating_survey_data"
last_updated: "2026-03-17T18:42:18.399Z"
---

                  Exercise 3: Creating Survey Data Using the Survey Command Window  

# Exercise 3: Creating Survey Data Using the Survey Command Window

In this exercise, you will create survey data using the Survey Command Window.

The Survey Command Window is used to enter survey commands directly using Command line input or interactively using the menus.

This exercise continues from [Exercise 2: Creating Survey Data Using the Traverse Editor](GUID-C87A5A42-4E75-4FC8-B55C-84063755A07C.htm "In this exercise, you will use the Traverse Editor to create survey data.").

Create a new survey database

1.  Open _Survey-4C.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Survey tab, right-click Survey Databases. Click New Local Survey Database.
3.  In the New Local Survey Database dialog box, for the name, enter **Survey 4**. Click OK.
4.  In Toolspace, on the Survey tab, right-click the database **Survey 4**. Click Edit Survey Database Settings.
5.  In the Survey Database Settings dialog box, under Survey Command Window, specify the following parameters:
    *   Ditto Feature: **Yes**
    *   Auto Point Numbering: **No**
    *   Start Point Numbering From: **1**
    *   Point Course Echo: **Yes**
    *   Figure Course Echo: **Yes**
    *   Point Coordinate Echo: **Yes**
    *   Figure Coordinate Echo: **Yes**
    *   Command Echo: **Yes**
    *   Use Batch File: **Yes**
    *   Use Output File: **Yes**
6.  Click OK.

Create a survey network

1.  In Toolspace, on the Survey tab, expand the **Survey 4** database. Right-click the Networks collection. Click New.
2.  In the Network dialog box, for Name, enter **Survey Network 4**.
3.  Click OK.

Enter survey data into the survey command window

1.  In Toolspace, on the Survey tab, right-click **Survey Network 4**. Click Survey Command Window.
2.  In the Survey Command Window, enter the following commands into the Command line.
    
    These are the survey language commands that will create four setups.
    
    NE 1 1000.00 1000.00 “STA 1”
    AZ 1 4 45.0000
    STN 1
    BS 4
    AD 2 90.0000 100.00 “STA 2”
    STN 2
    BS 1
    AD 3 90.0000 100.00 “STA 3”
    STN 3
    BS 2
    AD 4 90.0000 100.00 “STA 4”
    STN 4
    BS 3
    AD 1 90.0000 100.00
    
3.  As you enter the commands, the top section of the Survey Command Window displays the resulting output and the bottom section echoes the input.
    
4.  Close the Survey Command Window.
5.  On the Survey tab, right-click **Survey Network 4** and click Insert Into Drawing.
    
    The new survey data is displayed in the drawing.
    
    ## Click to see what the network should look like.
    
    ![](../images/GUID-E3587911-DEA2-48E6-AB65-0B165308E0C7.png)
    

To continue this tutorial, go to [Exercise 4: Calculating a Whole Circle Bearing in The Astronomic Direction Calculator](GUID-688AC541-7703-4A8F-B42C-C2ECA46B0B8F.htm "In this exercise, you will use the Astronomic Direction Calculator to calculate a whole circle bearing from solar observations by the hour angle method.").

**Parent topic:** [Tutorial: Manually Creating Survey Data](GUID-4ECE08C3-B0C0-4D90-B24C-C4BC0A8FAA41.htm "This tutorial demonstrates how to manually create and add survey data.")