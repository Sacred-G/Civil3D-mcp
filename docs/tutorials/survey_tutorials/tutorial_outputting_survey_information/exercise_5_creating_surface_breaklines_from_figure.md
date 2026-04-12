---
title: "Exercise 5: Creating Surface Breaklines from Figures"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-86A6B027-6D7D-4411-8930-DB437C0973A3.htm"
category: "tutorial_outputting_survey_information"
last_updated: "2026-03-17T18:42:19.398Z"
---

                Exercise 5: Creating Surface Breaklines from Figures  

# Exercise 5: Creating Surface Breaklines from Figures

In this exercise, you will use figures to add breaklines to a surface.

The breaklines will define the edge of pavement (EP) features, such as retaining walls, kerbs, tops of ridges, and streams. Breaklines force surface triangulation along the breakline and prevent triangulation across the breakline.

This exercise continues from [Exercise 4: Working with Mapcheck Data](GUID-8AB0AD8F-8AF4-47A5-919D-7E17860DB861.htm "In this exercise, you will learn about the tools that can leverage the data obtained from a mapcheck analysis.").

To create surface breaklines from figures

1.  Open _Survey-5C.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains the empty surface **Figure Surface 1**, to which you will add the figure data.
    
    Note:
    
    For more information about creating surfaces, see the [Creating and Adding Data to a Surface](GUID-899731B5-0B6A-451E-9CF2-0DCF00FA9B64.htm "This tutorial demonstrates how to create a TIN surface, and then add contour, breakline, and boundary data to the surface.") tutorial.
    
2.  In Toolspace, on the Survey tab, open the survey database **Survey 1** and click the Figures collection.
3.  In the list view, click figure **EP1**.
4.  Press the Shift key, click figure **EP7**.
5.  Keeping the Shift key pressed, right-click figure **EP7** and click Edit Figures.
    
    The Figures Editor is displayed with only the EP figures that you selected.
    
6.  To change the Breakline property to Yes for all EP figures, right-click the column heading Breakline and click Edit.
7.  Enter **Y** and press Enter.
    
    The Breakline property for all figures is changed to Yes.
    
    Note:
    
    The figures are displayed with bold text indicating there are unsaved changes.
    
8.  Click ![](../images/GUID-753C7052-9AE5-46FF-A230-5E2920D3224C.png) to save the changes to the survey database.
9.  Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to close the Figures Editor vista.
10.  In Toolspace, on the Survey tab, right-click the Figures collection and click Create Breaklines.
11.  In the Create Breaklines dialog box, click the Select Surface drop-down list and click **Figure Surface 1**. All the figures are listed in the dialog box, and the **EP** figures are listed as breaklines.
12.  Click OK.
13.  In the Add Breaklines dialog box, enter the following:
     *   Description: **EP**
     *   Type: **Standard**
     *   Mid-ordinate Distance: **0.1**
14.  Click OK.

The surface border and contours are displayed in the drawing.

![](../images/GUID-24E494E1-7E4D-4C93-A983-0135189E9122.png)

**Parent topic:** [Tutorial: Outputting Survey Information](GUID-E4B4FC43-4C35-4922-8B56-3447B40FA32C.htm "This tutorial demonstrates how to view information reports for figures and how to use the figures as a source for surface data.")