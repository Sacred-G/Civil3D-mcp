---
title: "Exercise 1: Importing Field-Coded Survey Data"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-344B584B-0E65-4FCF-B85F-A62E2ABCD5DE.htm"
category: "tutorial_importing_survey_data"
last_updated: "2026-03-17T18:42:15.982Z"
---

                 Exercise 1: Importing Field-Coded Survey Data  

# Exercise 1: Importing Field-Coded Survey Data

In this exercise, you will import survey data from an existing field book file that contains linework codes that can be interpreted by a linework code set.

The field book file that you will import contains the linework codes that you examined in the [Setting Up a Linework Code Set](GUID-14F4D676-EDA8-466F-9C49-2397044026F3.htm "In this exercise, you will learn how to set up a linework code set to interpret the field codes that the survey field crew enters into a data collector.") exercise.

Set up the project

1.  Open _Survey-2A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing is empty, but has point and figure styles that are appropriate for this exercise.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Create Ground Data panel ![](../images/ac.menuaro.gif)Import Survey Data ![](../images/GUID-32DD04FF-06EC-4813-A32B-D091CFF71E4E.png) Find.

Create a survey database

1.  In the Import Survey Data wizard, on the Specify Database page, under Survey Databases, select **Survey 1**.
    
    If you need to create a survey database, you can click Create New Survey Database.
    
2.  Click Edit Survey Database Settings.
    
    You use the Survey Database Settings dialog box to define the parameters of the survey database. Notice that the settings match those you specified in the [Adjusting and Verifying Settings](GUID-3B8ADD98-A14E-4B4A-9FCC-9C946FE84192.htm "In this exercise, you will view and adjust several types of survey settings.") exercise.
    
3.  Click OK.
4.  Click Next.

Specify the file to import

1.  On the Specify Data Source page, under Data Source Type, select Field Book File.
2.  Under Source File, click ![](../images/GUID-8ABD5C87-AE6A-4567-8E38-50532CFB268B.png).
3.  In the Field Book Filename dialog box, navigate to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC).
4.  Select _Survey-1.fbk_. Click Open.
5.  Click Next.

Create a survey network

1.  On the Specify Network page, click Create New Network.
2.  In the New Network dialog box, for Name, enter **Survey Network 1**.
3.  Click OK.
4.  Click Next.

Specify import options

1.  On the Import Options page, specify the following parameters:
    
    *   Current Equipment Database: **Sample**
    *   Current Equipment: **Sample**
    *   Show Interactive Graphics: **Yes (Selected)**
    *   Current Figure Prefix Database: **Sample**
    *   Process Linework During Import: **Yes (Selected)**
    *   Current Linework Code Set: **Sample**
    *   Process Linework Sequence: **By Import Order**
    *   Assign Offset To Point Identifiers: **Yes (Selected)**
    *   Point Identifier Offset: 10000
    *   Insert Network Object: **Yes (Selected)**
    *   Insert Figure Objects: **Yes (Selected)**
    *   Insert Survey Points: **Yes (Selected)**
    
    Accept the remaining default values.
    
2.  Click Finish.
    
    The survey data is imported, and the drawing looks like this:
    
    ![](../images/GUID-6DCEEA43-86A4-41F6-94D8-D4455FD24D34.png)
    
3.  Click ![](../images/GUID-1CE54225-70F9-4E62-9D61-E957C9D52C4C.png)![](../images/ac.menuaro.gif)Save As.
4.  Navigate to the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). For File Name, enter **Survey-2B.dwg**. Click Save.

To continue this tutorial, go to [Exercise 2: Updating Imported Survey Data](GUID-23DF4893-016D-4ECF-9086-B8B4A6946DF3.htm "In this exercise, you will modify some of the imported survey data, and then reprocess the linework to apply the changes.").

**Parent topic:** [Tutorial: Importing Survey Data](GUID-0A5C1B52-4F94-4388-91E6-E9F580EFF18B.htm "This tutorial demonstrates how to import survey data into a drawing, modify the data, and then reprocess the data.")