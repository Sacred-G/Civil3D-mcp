---
title: "Exercise 5: Creating Figures from Plots"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-13625594-0083-4006-8FF0-DFE4A409CD5A.htm"
category: "tutorial_manually_creating_survey_data"
last_updated: "2026-03-17T18:42:18.507Z"
---

                Exercise 5: Creating Figures from Plots  

# Exercise 5: Creating Figures from Plots

In this exercise, you will use Autodesk Civil 3D plot objects to add figures to a survey database.

In addition to plot objects, you can also use feature lines, plot lines, and AutoCAD lines and polylines as a source to create figures.

This exercise continues from [Exercise 4: Calculating an Azimuth in The Astronomic Direction Calculator](GUID-688AC541-7703-4A8F-B42C-C2ECA46B0B8F.htm "In this exercise, you will use the Astronomic Direction Calculator to calculate a whole circle bearing from solar observations by the hour angle method.").

Create a new survey database

1.  Open _Survey-4D.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In Toolspace, on the Survey tab, right-click Survey Databases. Click New Local Survey Database.
3.  In the New Local Survey Database dialog box, for the name, enter **Survey 5**. Click OK.

Create survey figures from existing plots

1.  In Toolspace, on the Survey tab, expand the database **Survey 5**. Right-click the Figures collection. Click Create Figure From Object.
2.  In the drawing, click the label for the plot **SINGLE-FAMILY: 101**.
3.  In the Create Figure From Objects dialog box, specify the following parameters:
    *   Name: **LOT CORNER**
    *   Current Figure Prefix Database: **Sample**
    *   Associate Survey Points To Vertices: **Yes**
4.  Click OK.
5.  Press Esc to end the command.
    
    The figure is created and added to the survey database. Information about the figure is displayed in list view.
    

To continue to the next tutorial, go to [Outputting Survey Information](GUID-E4B4FC43-4C35-4922-8B56-3447B40FA32C.htm "This tutorial demonstrates how to view information reports for figures and how to use the figures as a source for surface data.").

**Parent topic:** [Tutorial: Manually Creating Survey Data](GUID-4ECE08C3-B0C0-4D90-B24C-C4BC0A8FAA41.htm "This tutorial demonstrates how to manually create and add survey data.")