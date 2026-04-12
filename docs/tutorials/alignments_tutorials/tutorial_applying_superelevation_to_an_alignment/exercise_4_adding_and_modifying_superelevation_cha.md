---
title: "Exercise 4: Adding and Modifying Superelevation Chainages"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-53B257E3-D572-407F-8582-96B731644DEE.htm"
category: "tutorial_applying_superelevation_to_an_alignment"
last_updated: "2026-03-17T18:42:28.181Z"
---

                Exercise 4: Adding and Modifying Superelevation Chainages  

# Exercise 4: Adding and Modifying Superelevation Chainages

In this exercise, you will resolve overlap between two superelevated curves by adding and removing critical chainages, and then editing existing superelevation data.

This exercise continues from [Exercise 3: Creating a Superelevation View](GUID-832958B2-A3FC-4F05-B1EF-14ABA74CC950.htm "In this exercise, you will display superelevation data in a graph, which you can use to graphically edit superelevation data.").

Examine the superelevation parameters

1.  Open _Align-Superelevation-4.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the top viewport, zoom in to the middle of the superelevation view.
    
    Near chainage 0+558.66, the ![](../images/GUID-D5ADDB33-0636-4E9A-BACD-113935F74A84.png) indicates that the two curves overlap. At the bottom of the superelevation view, the light blue and red lines, which represent the left and right shoulders, cross over each other.
    
3.  In the superelevation view, select one of the lines.
4.  Right-click. Click Open Tabular Editor.
5.  In the Superelevation Tabular Editor, scroll down to Curve.3.
    
    In the Overlap column, two rows display ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png). This icon indicates that the superelevation stations of two or more critical chainages overlap. In this case, chainage value for the last critical chainage of Curve.2 is greater than the chainage value for the first critical chainage of Curve.3.
    
6.  Under Curve.3, under Transition In Region, select the End Normal Shoulder row.
    
    The curve is highlighted in the bottom viewport, and the critical chainage is marked with a blue tick.
    
7.  In the Overlap column, click ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png).
    
    The Superelevation - Overlap Detected dialog box presents two options:
    
    *   Automatically Resolve Overlap —This option removes the overlapping critical chainages of the affected curves.
    *   Ignore Overlap—This option enables you to return to the Superelevation Tabular Editor to manually modify the data. The ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) icons are not cleared.
8.  In the Superelevation - Overlap Detected dialog box, click Ignore Overlap.

Edit superelevation chainages

1.  In the Superelevation Tabular Editor, under Curve.2![](../images/ac.menuaro.gif)Transition Out Region, select the Begin Normal Shoulder row.
2.  Change the Start Chainage value to 0+560.00.

Remove a superelevation critical chainage

1.  In the Superelevation Tabular Editor, under Curve.3![](../images/ac.menuaro.gif)Transition In Region, select the End Normal Shoulder row.
2.  Click ![](../images/GUID-627FB583-4737-43B5-B407-A768EF513E84.png).
    
    The End Normal Shoulder superelevation critical chainage is removed.
    

Add a superelevation critical chainage

1.  In the Superelevation Tabular Editor, select the Curve.3 row.
2.  Click ![](../images/GUID-7633B109-B0E2-42D8-8A07-E27BBF28B731.png).
    
    The Superelevation Tabular Editor is hidden, and you are prompted to specify a chainage along the alignment.
    
3.  On the command line, enter 568. Press Enter.
    
    A new manual chainage, which starts at chainage 0+568.00, is displayed in the Superelevation Tabular Editor.
    
4.  Right-click the Manual Chainage. Click Assign Critical Chainage![](../images/ac.menuaro.gif)End Normal Shoulder.
5.  In the End Normal Shoulder row, enter the following values:
    
    Note:
    
    The remaining columns are interpreted from these values.
    
    *   Left Outside Shoulder: -5.00%
    *   Left Outside Lane: -2.00%
    *   Right Outside Lane: -2.00%
    *   Right Outside Shoulder: -5.00%
    
    Note:
    
    To import existing superelevation data from a CSV file, click ![](../images/GUID-D068AEF9-3789-43BB-8481-CBD14F9C090C.png).
    
    Notice that the ![](../images/GUID-31EA29CA-8050-4036-BCEC-FE3C9CE233C5.png) icons are no longer displayed in the Overlap column.
    
6.  Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to close the Superelevation Tabular Editor.

To continue this tutorial, go to [Exercise 5: Editing Superelevation Parameters Graphically](GUID-F2C697A8-A861-45C5-87BE-B65864460B18.htm "In this exercise, you will use grips in a superelevation view to modify the superelevation crossfalls and critical chainage values.").

**Parent topic:** [Tutorial: Applying Superelevation to an Alignment](GUID-AA0068E0-2858-4067-9104-161112DEDBF6.htm "In this tutorial, you will calculate superelevation for alignment curves, create a superelevation view to display the superelevation data, and edit the superelevation data both graphically and in a tabular format.")