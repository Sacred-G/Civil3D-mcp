---
title: "Exercise 3: Performing a Mapcheck Analysis by Manually Entering Data"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-234E732B-9B17-411C-A9AD-708E43A9826D.htm"
category: "tutorial_outputting_survey_information"
last_updated: "2026-03-17T18:42:19.287Z"
---

                 Exercise 3: Performing a Mapcheck Analysis by Manually Entering Data  

# Exercise 3: Performing a Mapcheck Analysis by Manually Entering Data

In this exercise, you will manually enter survey data to perform a mapcheck analysis.

This method of performing a mapcheck analysis is useful when you must check survey data that does not exist as labels in the drawing. For example, you can use this method to enter survey data from a paper drawing.

This exercise continues from [Exercise 2: Performing a Mapcheck Analysis with Plot Labels](GUID-712D93AB-2491-4C35-9FAE-206F56FC2F3F.htm "In this exercise, you will use the data in plot segment labels to perform a mapcheck analysis.").

Set up the mapcheck analysis dialog box

Note:

This exercise uses _Survey-5B.dwg_ with the modifications you made in the previous exercise.

The plot segments on the right side of the site are not labeled. Because the data is not available in plot labels, you must enter the mapcheck information manually.

3.  Click Analyze tab ![](../images/ac.menuaro.gif)Ground Data panel ![](../images/ac.menuaro.gif)Survey drop-down ![](../images/ac.menuaro.gif)Mapcheck ![](../images/GUID-A4B62A9D-ADFA-4C88-AFC3-356BF56CA1AC.png) Find.
4.  If a message that states the command line mode cannot be used while a command is active is displayed, click OK.
5.  In the Mapcheck Analysis dialog box, make sure that the ![](../images/GUID-A59E7A0A-E5BF-4E58-A0AA-B21AEA7A042F.png)Use Command Line Interface option is toggled off.
6.  Click ![](../images/GUID-E8173005-CC1E-4DF3-A9BA-EE1F37FFAFEB.png)Input View.
    
    The input view provides an interface in which you can add data to the mapcheck analysis.
    

Enter plot data

1.  Click ![](../images/GUID-D061B979-9A14-4866-9FFA-DDA18367DDF0.png)New Mapcheck.
2.  In the Mapcheck Analysis dialog box, for the name of the mapcheck, enter **Plot Manual Input**.
3.  To specify the point of beginning, enter the following values:
    
    *   Easting: **5576.199**
    *   Northing: **5291.0640**
    
    The ![](../images/GUID-74420307-09AB-470E-BBEF-63C29C8B8ED4.png) icon indicates the point of beginning.
    
4.  Click ![](../images/GUID-BD7C403C-2AEA-497C-B4EE-44C7E0E4ECF3.png)New Side.
5.  Expand the **Side 1** collection. Specify the following parameters:
    
    Note:
    
    Notice that the ![](../images/GUID-F9D58477-90CB-4F8C-9B2F-F5473DE87F22.png) icon is displayed next to the side collection, and also next to the top-level mapcheck collection. This indicates that the mapcheck is incomplete, because it requires more data about the side.
    
    *   Side Type: **Line**
    *   Angle Type: **Direction**
    *   Angle: **N00 00 10E**
    *   Distance: **16.330**
6.  Click ![](../images/GUID-BD7C403C-2AEA-497C-B4EE-44C7E0E4ECF3.png)New Side.
7.  Expand the **Side 2** collection. Specify the following parameters:
    *   Side Type: **Curve**
    *   Curve Direction: **Clockwise**
    *   Radius: **20.00**
    *   Arc Length: **21.550**
8.  Click ![](../images/GUID-BD7C403C-2AEA-497C-B4EE-44C7E0E4ECF3.png)New Side.
9.  Expand the **Side 3** collection. Specify the following parameters:
    *   Side Type: **Curve**
    *   Curve Direction: **Counter-clockwise**
    *   Radius: **75.00**
    *   Arc Length: **80.800**
10.  Click ![](../images/GUID-BD7C403C-2AEA-497C-B4EE-44C7E0E4ECF3.png)New Side.
11.  Expand the **Side 4** collection. Specify the following parameters:
     *   Side Type: **Line**
     *   Angle: **N90 00 00E**
     *   Distance: **99.990**
12.  Click ![](../images/GUID-BD7C403C-2AEA-497C-B4EE-44C7E0E4ECF3.png)New Side.
13.  Expand the **Side 5** collection. Specify the following parameters:
     *   Side Type: **Line**
     *   Angle: **S0 00 00E**
     *   Distance: **100**
14.  Click ![](../images/GUID-BD7C403C-2AEA-497C-B4EE-44C7E0E4ECF3.png)New Side.
15.  Expand the **Side 6** collection. Specify the following parameters:
     
     *   Side Type: **Line**
     *   Angle: **N89 59 54W**
     *   Distance: **149.990**
     
     The direction arrow meets the point of beginning.
     
     ![](../images/GUID-C6E880B0-7F4B-4063-B038-C80F9649269F.png)
     
16.  Click ![](../images/GUID-AFAFAD5D-5635-4419-8C20-48C82384CC74.png)Output View.
     
     The Output View displays the results of the mapcheck analysis. You will learn how to work with the output data in [Exercise 4: Working with Mapcheck Data](GUID-8AB0AD8F-8AF4-47A5-919D-7E17860DB861.htm "In this exercise, you will learn about the tools that can leverage the data obtained from a mapcheck analysis.").
     

To continue this tutorial, go to [Exercise 4: Working with Mapcheck Data](GUID-8AB0AD8F-8AF4-47A5-919D-7E17860DB861.htm "In this exercise, you will learn about the tools that can leverage the data obtained from a mapcheck analysis.").

**Parent topic:** [Tutorial: Outputting Survey Information](GUID-E4B4FC43-4C35-4922-8B56-3447B40FA32C.htm "This tutorial demonstrates how to view information reports for figures and how to use the figures as a source for surface data.")