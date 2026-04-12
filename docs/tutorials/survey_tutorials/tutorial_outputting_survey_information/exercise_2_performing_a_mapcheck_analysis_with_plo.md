---
title: "Exercise 2: Performing a Mapcheck Analysis with Plot Labels"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-712D93AB-2491-4C35-9FAE-206F56FC2F3F.htm"
category: "tutorial_outputting_survey_information"
last_updated: "2026-03-17T18:42:19.240Z"
---

                  Exercise 2: Performing a Mapcheck Analysis with Plot Labels  

# Exercise 2: Performing a Mapcheck Analysis with Plot Labels

In this exercise, you will use the data in plot segment labels to perform a mapcheck analysis.

The Mapcheck command checks the figure for length, course, perimeter, area, error of closure, and precision. It starts at the beginning of the figure and computes the figure vertex XY coordinates for each segment. These computations are based on the actual labeled values, and not the inverse direction and distance/curve data and the Linear and Angle precision (set in the Survey Database Settings).

This method of performing a mapcheck analysis is useful as a final check of closure. The data for the mapcheck analysis is taken from plot segment labels. The precision of the mapcheck analysis is based on the precision of the labels.

This exercise continues from [Exercise 1: Viewing Inverse and Mapcheck Information on a Survey Figure](GUID-5D6ECB87-49AD-4B66-97F0-39D14C25B128.htm "In this exercise, you will display the figure mapcheck and inverse information.").

Set up the mapcheck analysis dialog box

1.  Open _Survey-5B.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a small subdivision with several plots. All the plot segments on the left side of the site are labeled. Notice that along the cul-de-sac, the line and curve labels have been converted to tags, and their data is displayed in a table on the left side of the site. You will perform a mapcheck analysis on plot LOT 5.
    
2.  Click Analyze tab ![](../images/ac.menuaro.gif)Ground Data panel ![](../images/ac.menuaro.gif)Survey drop-down ![](../images/ac.menuaro.gif)Mapcheck ![](../images/GUID-A4B62A9D-ADFA-4C88-AFC3-356BF56CA1AC.png) Find.
3.  If a message that states that the command line mode cannot be used while a command is active is displayed, click OK.
4.  In the Mapcheck Analysis dialog box, make sure that the ![](../images/GUID-A59E7A0A-E5BF-4E58-A0AA-B21AEA7A042F.png)Use Command Line Interface option is toggled on.
5.  Click ![](../images/GUID-E8173005-CC1E-4DF3-A9BA-EE1F37FFAFEB.png)Input View.
    
    The input view provides an interface in which you can add data to the mapcheck analysis.
    

Perform a mapcheck on plot labels

1.  Click ![](../images/GUID-D061B979-9A14-4866-9FFA-DDA18367DDF0.png)New Mapcheck.
2.  On the command line, for the name of the mapcheck, enter **Plot Labels**.
3.  When prompted to specify a point of beginning, click the intersection of the plot lines under label tag **L1**.
    
    The ![](../images/GUID-74420307-09AB-470E-BBEF-63C29C8B8ED4.png) icon indicates the point of beginning.
    
4.  Select the **L1** label tag above the point of beginning.
    
    A temporary arrow graphic is displayed at the point of beginning. Notice that the arrow is pointing away from LOT 5.
    
5.  On the command line, enter **R** to reverse the direction of the arrow.
6.  Select the **C1** label tag.
    
    Notice that the temporary line and arrow are pointing in the wrong direction.
    
7.  Enter **F** to flip the arrow.
8.  Select the **C2** label tag.
9.  Select the quadrant bearing over distance label to the left of the arrow. Enter **R** to reverse the direction.
10.  Select the 100.00’ portion of the quadrant bearing over distance label below the arrow.
     
     A ![](../images/GUID-F7AF06B1-CB4B-47A6-B60B-D6657D77CC55.png) icon appears at the end of the current line, and the command line states that there is not enough data to define the segment. This happened because the current segment is a plot line that is shared by all the plots on the left side of the site.
     
11.  Select the quadrant bearing portion of the label. Enter **R** to reverse the direction.
12.  Select the quadrant bearing over distance label to the right of the arrow.
     
     The arrow returns to the point of beginning.
     
     ![](../images/GUID-BB24FDE6-8F01-43F5-9737-9FED83181FD5.png)
     
13.  Press Enter to end the command.
     
     In the Mapcheck Analysis dialog box, notice that you can edit any of the sides you created during the mapcheck analysis.
     
14.  Click ![](../images/GUID-AFAFAD5D-5635-4419-8C20-48C82384CC74.png)Output View.
     
     The Output View displays the results of the mapcheck analysis. You will learn how to work with the output data in [Exercise 4: Working with Mapcheck Data](GUID-8AB0AD8F-8AF4-47A5-919D-7E17860DB861.htm "In this exercise, you will learn about the tools that can leverage the data obtained from a mapcheck analysis.").
     

To continue this tutorial, go to [Exercise 3: Performing a Mapcheck Analysis by Manually Entering Data](GUID-234E732B-9B17-411C-A9AD-708E43A9826D.htm "In this exercise, you will manually enter survey data to perform a mapcheck analysis.").

**Parent topic:** [Tutorial: Outputting Survey Information](GUID-E4B4FC43-4C35-4922-8B56-3447B40FA32C.htm "This tutorial demonstrates how to view information reports for figures and how to use the figures as a source for surface data.")