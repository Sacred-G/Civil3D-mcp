---
title: "Exercise 4: Calculating a Whole Circle Bearing in The Astronomic Direction Calculator"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-688AC541-7703-4A8F-B42C-C2ECA46B0B8F.htm"
category: "tutorial_manually_creating_survey_data"
last_updated: "2026-03-17T18:42:18.451Z"
---

                  Exercise 4: Calculating a Whole Circle Bearing in The Astronomic Direction Calculator  

# Exercise 4: Calculating a Whole Circle Bearing in The Astronomic Direction Calculator

In this exercise, you will use the Astronomic Direction Calculator to calculate a whole circle bearing from solar observations by the hour angle method.

To calculate the whole circle bearing, you can use either a single foresight or a multiple foresight. For this exercise, you will use a multiple foresight because it is the more commonly used solar observation method.

This exercise continues from [Exercise 3: Creating Survey Data Using the Survey Command Window](GUID-FCEF073D-3C15-49F6-844A-89BB37F22FDB.htm "In this exercise, you will create survey data using the Survey Command Window.").

Calculate a whole circle bearing using the Astronomic Direction Calculator

Note:

This exercise uses _Survey-4C.dwg_ with the modifications you made in the previous exercise.

2.  Click Analyze tab ![](../images/ac.menuaro.gif)Ground Data panel ![](../images/ac.menuaro.gif)Survey drop-down ![](../images/ac.menuaro.gif)Astronomic Direction ![](../images/GUID-E03442CE-EB5F-411A-BE1E-725E00B320B2.png) Find.
3.  In the Astronomic Direction Calculator dialog box, specify the following parameters:
    
    Calculation Type
    
    *   Calculation Type: **Sun Shot Calculation**
    
    Observation Chainage Data
    
    *   Chainage Point: **2**
    *   Backsight Point: **1**
    *   Chainage Latitude: **36.04**
    *   Chainage Longitude: **\-94.1008**
    *   UT1 Time: **13.34024**
    
    Ephemeris Data
    
    *   GHA 00 Hours: **180.13402**
    *   GHA 24 Hours: **180.10431**
    *   Declination 00 Hours: **22.54505**
    *   Declination 24 Hours: **22.59437**
    *   Sun Semi-diameter: **0.15468**
4.  Click ![](../images/GUID-3509BBD1-D097-4012-B949-1EB363162A0A.png).
    
    A new observation set named Set:1 is displayed in the table.
    
5.  Specify the following parameters for Set:1:
    
    Direct
    
    *   Backsight Observation: **0.00**
    *   Sun Observation: **351.0835**
    *   Stop Time: **0.121590**
    
    Reverse
    
    *   Backsight Observation: **180.0005**
    *   Sun Observation: **171.3520**
    *   Stop Time: **0.154210**
    
    Note:
    
    The observations will determine the true astronomic direction from the chainage point to the backsight point. Notice that after you enter the stop time for an observation, the observed and average direction are calculated automatically.
    
6.  Click ![](../images/GUID-3509BBD1-D097-4012-B949-1EB363162A0A.png)
    
    A new observation set named Set:2 is displayed in the table.
    
7.  Specify the following parameters for Set:2:
    
    Direct
    
    *   Backsight Observation: **0.00**
    *   Sun Observation: **351.1300**
    *   Stop Time: **0.12491**
    
    Reverse
    
    *   Backsight Observation: **180.0005**
    *   Sun Observation: **171.3800**
    *   Stop Time: **0.16030**
8.  Click ![](../images/GUID-3509BBD1-D097-4012-B949-1EB363162A0A.png).
    
    A new observation set named Set:3 is displayed in the table.
    
9.  Specify the following parameters for Set:3:
    
    Direct
    
    *   Backsight Observation: **0.00**
    *   Sun Observation: **351.1450**
    *   Stop Time: **0.13112**
    
    Reverse
    
    *   Backsight Observation: **180.0005**
    *   Sun Observation: **171.4145**
    *   Stop Time: **0.16313**
    
    After you have entered the above data, notice that the Mean Direction value calculated by the Astronomic Direction Calculator is SOUTH87.967088EAST (if the drawing settings Direction Measurement Type is set to Quadrant Bearings).
    
10.  Close the Astronomic Direction Calculator.

To continue this tutorial, go to [Exercise 5: Creating Figures from Plots](GUID-13625594-0083-4006-8FF0-DFE4A409CD5A.htm "In this exercise, you will use Autodesk Civil 3D plot objects to add figures to a survey database.").

**Parent topic:** [Tutorial: Manually Creating Survey Data](GUID-4ECE08C3-B0C0-4D90-B24C-C4BC0A8FAA41.htm "This tutorial demonstrates how to manually create and add survey data.")