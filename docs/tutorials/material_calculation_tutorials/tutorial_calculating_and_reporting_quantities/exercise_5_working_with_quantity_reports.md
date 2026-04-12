---
title: "Exercise 5: Working with Quantity Reports"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E870AEEA-4E45-480B-9DA3-BE6846A41A8F.htm"
category: "tutorial_calculating_and_reporting_quantities"
last_updated: "2026-03-17T18:43:03.835Z"
---

                 Exercise 5: Working with Quantity Reports  

# Exercise 5: Working with Quantity Reports

In this exercise, you will generate quantity reports, and then examine several ways to use the resulting data.

You can display and save reports in multiple formats, including XML, CSV, HTML, and TXT. Two types of quantity takeoff reports are available:

*   **Summary Report:** Lists the total sum of each rate item. You can restrict a summary report to compute rate item quantities relative to the chainage range of a specified alignment.
*   **Detailed Report:** Lists the quantity of each rate item type (area, count, and linear). Each instance of a rate item is reported as a separate line item, and its position relative to a specified alignment may be reported. You can restrict an itemized report to compute rate items relative to the chainage range of a specified alignment.

This exercise continues from [Exercise 4: Assigning Rate Item Codes to Corridors](GUID-347C3502-FFB4-459F-ADFC-D0C547C5391A.htm "In this exercise, you will create a code set style to assign rate item codes to corridor areas and linear features.").

Generate a summary rate item quantity report

1.  Open _Quantities-5.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a commercial site, which consists of a building footprint, a parking lot, and access roads. Several of the objects in this drawing have rate items associated with them.
    
2.  Click Analyze tab ![](../images/ac.menuaro.gif)QTO panel ![](../images/ac.menuaro.gif)Takeoff ![](../images/GUID-9988FD73-56C6-404C-BD15-E7F958F37F87.png) Find.
3.  In the Compute Quantity Takeoff dialog box, specify the following parameters:
    *   Report Type: **Summary**
    *   Report Extents: **Drawing**
        
        This option specifies that the report will include rate item data for all objects in the current drawing. Other options enable you to restrict the report to objects that are within a sheet or selection set.
        
    *   Limit Extents To Alignment Chainage Range: **Cleared**
        
        Use this option to specify an alignment chainage range to which to restrict the report. The report will include rate item data for drawing objects that can be projected onto the alignment within the specified chainage range.
        
        In this exercise, you will not restrict the quantity report.
        
    *   Report Selected Rate Items Only: **Cleared**
4.  Click Compute.
5.  In the Quantity Takeoff Report dialog box, in the drop-down menu, select **Summary (TXT).xsl**.
    
    Examine the report. Notice that the report lists the ID, description, total quantity, and unit of measure for each rate item.
    
    Note:
    
    The quantities for the rate items that are associated with the corridor codes are not calculated in a summary report. In the following steps, these items will be calculated in a itemized report.
    
6.  In the Quantity Takeoff Report dialog box, click Close.

Generate a detailed rate item quantity report

1.  In the Compute Quantity Takeoff dialog box, specify the following parameters:
    *   Report Type: **Detailed**
    *   Report Extents: **Selection Set**
    *   Limit Extents To Alignment Chainage Range: **Cleared**
    *   Report Selected Rate Items Only: **Cleared**
    *   Report Chainage And Offset Relative To: **Side Road**
        
        In a detailed report, the chainage and offset value for each item relative to a specified alignment is displayed.
        
2.  Next to Report Extents, click ![](../images/GUID-8B243375-9F97-4BBA-9333-91D9267E95C0.png).
3.  In the drawing, select the pipes and structures along the side road. Press Enter.
    
    ![](../images/GUID-63700258-3687-4F3B-8892-04118A0AA024.png)
    
4.  Click Compute.
5.  In the Quantity Takeoff Report dialog box, in the drop-down menu, select **Detailed Linear (HTML).xsl** .
    
    Examine the report. The length, chainage, and offset of the start and end of each pipe is displayed in this report.
    
6.  In the drop-down menu, select **Detailed Count (HTML).xsl** .
    
    The baseline, chainage value, and offset of the object to which each rate item is assigned is displayed for each rate item instance.
    
    Notice that several instances of Rate Item 60409-0500 are 17 feet on either side of the alignment. This indicates that these inlets are placed along the edges of pavement of the Side Road corridor.
    

Insert a quantity takeoff report into the drawing

1.  Click Draw.
2.  Pan to an empty space in the drawing.
3.  Click the place the table.
    
    The drawing zooms to the quantity takeoff report, which is in an AutoCAD table.
    

Export a quantity takeoff report

1.  In the Quantity Takeoff Report dialog box, in the drop-down menu, select **Detailed Area (CSV).xsl** .
    
    You can export a quantity takeoff report to any of the formats in this list.
    
2.  Click Save As.
3.  In the Save Quantity Takeoff Report As dialog box, navigate to the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832).
4.  For File Name, enter _QTO\_Detailed Area (CSV).txt_. Click Save.
    
    You can import the text file into a spreadsheet application, such as Microsoft Excel.
    
5.  Click Close twice.

To continue this tutorial, go to [Exercise 6: Working with Rate Item Formulas](GUID-AA55D4D1-EB5E-4475-81CF-95EB5D88E68F.htm "In this exercise, you will build a mathematical formula that applies a rate item to a corridor at a specified interval.").

**Parent topic:** [Tutorial: Calculating and Reporting Quantities](GUID-709DB1D4-FB24-46F0-A54B-E2D9CC6D14F7.htm "In this tutorial, you will learn how to create and manage rate item data, associate rate item codes with several types of drawing objects, and generate rate item quantity reports.")