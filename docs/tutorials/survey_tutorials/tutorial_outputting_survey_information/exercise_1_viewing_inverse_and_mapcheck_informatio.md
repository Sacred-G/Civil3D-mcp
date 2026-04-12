---
title: "Exercise 1: Viewing Inverse and Mapcheck Information on a Survey Figure"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-5D6ECB87-49AD-4B66-97F0-39D14C25B128.htm"
category: "tutorial_outputting_survey_information"
last_updated: "2026-03-17T18:42:19.199Z"
---

                Exercise 1: Viewing Inverse and Mapcheck Information on a Survey Figure  

# Exercise 1: Viewing Inverse and Mapcheck Information on a Survey Figure

In this exercise, you will display the figure mapcheck and inverse information.

The Mapcheck command checks the figure for length, course, perimeter, area, error of closure, and precision. It starts at the beginning of the figure and computes the figure vertex XY coordinates for each segment. These computations are based on the inverse direction and distance/curve data and the Linear and Angle precision (set in the Survey Database Settings).

For closed figures, error is introduced into the sequential computation of vertices of the mapcheck report, so a closure error, closure direction, and precision can be calculated. The area is also based on the computed vertex XY coordinates.

The Inverse command starts at the beginning of the figure and lists the direction and distance, or curve data computed from the XY coordinates of the endpoints of the figure segments. The area is calculated from the XY coordinates of each segment.

This exercise continues from the [Manually Creating Survey Data](GUID-4ECE08C3-B0C0-4D90-B24C-C4BC0A8FAA41.htm "This tutorial demonstrates how to manually create and add survey data.") tutorial.

View inverse information for a figure

1.  Open _Survey-5A.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains the survey network that was created in the [Importing Field-Coded Survey Data](GUID-344B584B-0E65-4FCF-B85F-A62E2ABCD5DE.htm "In this exercise, you will import survey data from an existing field book file that contains linework codes that can be interpreted by a linework code set.") exercise.
    
2.  In Toolspace, on the Survey tab, right-click the database **Survey 1**. Click Open For Edit.
3.  Under **Survey 1**, select the Figures collections.
    
    Note:
    
    If a ![](../images/GUID-4FD64066-9BD2-467F-B810-769A35D59A90.png) is displayed beside the Figures collection, click the collection to refresh it and then click ![](../images/GUID-EC34E186-CD47-4C57-9776-3E6D0C24CAE3.png) to view all the figures.
    
4.  Right-click the figure **BLDG2**. Click Display Inverse.
    
    The figure inverse information is displayed in the Figure Display vista. The ![](../images/GUID-913B04F7-E31F-4E4F-A98E-DDCDFC28E2C4.png) icon indicates that a vertex is associated with a survey point.
    
5.  When you have finished reviewing the inverse data, click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to close the vista.

View mapcheck information for a figure

1.  In the list view, right-click a figure. Click Display Mapcheck.
    
    The figure mapcheck information is displayed in the Figure Display vista.
    
2.  When you have finished reviewing the mapcheck data, click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to close the vista.

To continue this tutorial, go to [Exercise 2: Performing a Mapcheck Analysis with Plot Labels](GUID-712D93AB-2491-4C35-9FAE-206F56FC2F3F.htm "In this exercise, you will use the data in plot segment labels to perform a mapcheck analysis.").

**Parent topic:** [Tutorial: Outputting Survey Information](GUID-E4B4FC43-4C35-4922-8B56-3447B40FA32C.htm "This tutorial demonstrates how to view information reports for figures and how to use the figures as a source for surface data.")