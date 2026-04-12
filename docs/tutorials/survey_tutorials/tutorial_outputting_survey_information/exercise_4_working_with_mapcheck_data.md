---
title: "Exercise 4: Working with Mapcheck Data"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-8AB0AD8F-8AF4-47A5-919D-7E17860DB861.htm"
category: "tutorial_outputting_survey_information"
last_updated: "2026-03-17T18:42:19.332Z"
---

                 Exercise 4: Working with Mapcheck Data  

# Exercise 4: Working with Mapcheck Data

In this exercise, you will learn about the tools that can leverage the data obtained from a mapcheck analysis.

This tutorial continues from [Exercise 3: Performing a Mapcheck Analysis by Manually Entering Data](GUID-234E732B-9B17-411C-A9AD-708E43A9826D.htm "In this exercise, you will manually enter survey data to perform a mapcheck analysis.").

Change the default appearance of the mapcheck objects

Note:

This exercise uses _Survey-5B.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Settings tab, expand the General![](../images/ac.menuaro.gif)Commands collection. Right-click MapCheck. Click Edit Command Settings.
3.  On the Edit Command Settings dialog box, expand the Mapcheck collection. Examine the default settings that are available.
    
    The colors in this collection specify the appearance of the mapcheck objects. Notice that Mapcheck Color currently is set to green.
    
4.  Click in the Mapcheck ColorValue cell. Click ![](../images/GUID-4453B981-AEBA-41F4-A9AA-AE989CFE1B5F.png).
5.  In the Select Color dialog box, in the Color field, enter **10**. Click OK.
6.  In the Edit Command Settings dialog box, click OK.
    
    In the drawing window, notice that the mapcheck object around LOT 5 is red.
    

Move mapcheck data into the drawing

1.  In the Mapcheck Analysis dialog box, click ![](../images/GUID-AFAFAD5D-5635-4419-8C20-48C82384CC74.png)Output View.
2.  Select the **Plot Labels** mapcheck.
3.  Click ![](../images/GUID-C4E590B4-3053-4520-B972-96072CAA7BFB.png)Insert Mtext.
4.  Pan to a clear area of the drawing. Click to place the Mtext.

Save mapcheck data to a text file

1.  Click ![](../images/GUID-F4A4A6F2-5C79-4593-B9E3-87A6EBE523AA.png)Copy To Clipboard.
2.  On the command line, enter **NOTEPAD**. When you are prompted for a file to edit, press Enter.
3.  In the Microsoft Notepad window, press Ctrl+V.
    
    The mapcheck data is displayed in Microsoft Notepad. You can save this file for later analysis in another application.
    

Create a polyline from the mapcheck data

1.  Select the **Plot Manual Input** mapcheck.
2.  Click ![](../images/GUID-6A3CC7FA-3927-4E23-A61C-0DD24D670856.png)Create Polyline.
    
    A polyline is created on the perimeter of LOT 5. You can perform any standard AutoCAD functions on the polyline.
    

To continue this tutorial, go to [Exercise 5: Creating Surface Breaklines from Figures](GUID-86A6B027-6D7D-4411-8930-DB437C0973A3.htm "In this exercise, you will use figures to add breaklines to a surface.").

**Parent topic:** [Tutorial: Outputting Survey Information](GUID-E4B4FC43-4C35-4922-8B56-3447B40FA32C.htm "This tutorial demonstrates how to view information reports for figures and how to use the figures as a source for surface data.")