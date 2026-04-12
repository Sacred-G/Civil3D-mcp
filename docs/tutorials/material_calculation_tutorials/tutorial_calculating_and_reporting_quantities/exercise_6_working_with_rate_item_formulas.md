---
title: "Exercise 6: Working with Rate Item Formulas"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-AA55D4D1-EB5E-4475-81CF-95EB5D88E68F.htm"
category: "tutorial_calculating_and_reporting_quantities"
last_updated: "2026-03-17T18:43:03.966Z"
---

                 Exercise 6: Working with Rate Item Formulas  

# Exercise 6: Working with Rate Item Formulas

In this exercise, you will build a mathematical formula that applies a rate item to a corridor at a specified interval.

In the example used in this exercise, the rate item formula adds recessed pavement markers at 10-foot intervals along the corridor. Similar applications of this formula include lane striping and quantity of posts along a guardrail or fence.

Formulas can also be used to convert rate item quantities from one unit of measure to another. For example, you could create a formula that converts square yards of a given rate item to tonnage.

Rate item formulas, label expressions, and design checks are created in a similar manner. However, unlike label expressions and design checks, rate item formulas are not saved in the current drawing. You will learn how to save and manage rate item formula files in this exercise.

This exercise continues from [Exercise 5: Working with Quantity Reports](GUID-E870AEEA-4E45-480B-9DA3-BE6846A41A8F.htm "In this exercise, you will generate quantity reports, and then examine several ways to use the resulting data.").

Create a rate item formula

1.  Open _Quantities-6.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a commercial site, which consists of a building footprint, a parking lot, and access roads.
    
2.  In the drawing, select the side road corridor. Click Corridor tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Corridor Properties drop-down ![](../images/ac.menuaro.gif)Corridor Properties ![](../images/GUID-6EDEE16B-2293-4977-BC74-7CB47522DEAA.png) Find.
3.  In the Corridor Properties dialog box, on the Feature Lines tab, in the Crown row, click ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png).
4.  In the Rate Item List dialog box, expand the ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png)Favorites category.
5.  In the ![](../images/GUID-76877489-DFF2-450F-BD08-2CDD3675B10E.png)Favorites category, select Rate Item ID**63407-0000**.
6.  In the Rate Item ID number 63407-0000 row, click the Formula cell.
    
    You are notified that rate item formulas must be written to an external file. After the formula file has been saved, it remains associated with the current drawing. Other formulas that you write in the current drawing will be saved to the same formula file.
    
7.  Click OK.
8.  In the Specify A Quantity Takeoff Formula File dialog box, navigate to the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832).
    
    Best Practice:
    
    Save the rate item formula file in the same location as either the drawing with which it is used, or the master rate item list. If you send the drawing to another user, you must also send the formula file.
    
9.  For File Name, enter _Tutorial\_QTO\_Pay-Item-Formulas.for_. Click Save.
10.  In the Rate Item Formula dialog box, click ![](../images/GUID-2FDBB45F-EA3B-409B-8A37-86532FA11588.png). Click TRUNC.
11.  Click ![](../images/GUID-A80E6107-AE9C-429D-9396-162CA88F58CA.png). Click Item Length.
12.  Using either your keyboard or the buttons on the QTO Rate Item Formula dialog box, enter **/10)+1** in the Expression field.
     
     When you are finished, the formula in the Expression field should look like this:
     
     **TRUNC({Item Length}/10)+1**
     
     This formula truncates the feature line length to an integer value, and then divides it by ten. The resulting value is used as the rate item count for the recessed pavement marker. If there is a remainder from dividing the feature line length by ten, then one recessed pavement marker is added to the sum.
     
13.  Click OK.
     
     In the Rate Item List dialog box, in the Rate Item ID number 63407-0000 row, a ![](../images/GUID-EDF5A20C-2163-4772-B573-5E2E2A9049BD.png) is displayed in the Formula cell. This indicates that a formula has been added to this rate item.
     
14.  Click OK twice.

Generate a detailed quantity takeoff report

1.  Click Analyze tab ![](../images/ac.menuaro.gif)QTO panel ![](../images/ac.menuaro.gif)QTO Manager ![](../images/GUID-13CDDC6E-1903-4681-8F6D-130DCE8D61F5.png) Find.
2.  In the QTO Manager vista, in the Favorites category, select the Rate Item ID**63407-0000** row.
3.  Click Analyze tab ![](../images/ac.menuaro.gif)QTO panel ![](../images/ac.menuaro.gif)![](../images/GUID-9988FD73-56C6-404C-BD15-E7F958F37F87.png)Takeoff.
4.  In the Compute Quantity Takeoff dialog box, specify the following parameters:
    *   Report Type: **Detailed**
    *   Report Extents: **Drawing**
    *   Limit Extents to Alignment Chainage Range: **Selected**
    *   Alignment: **Side Road**
    *   Report Selected Rate Items Only: **Selected**
    *   Report Chainage And Offset Relative To: **Side Road**
5.  Click Compute.
6.  In the Quantity Takeoff Report dialog box, in the drop-down menu, select **Detailed Count (HTML).xsl**
    
    Scroll through the report and examine the Recessed Pavement Marker rate items. The alignment is 1090 feet long. The formula you created divided the alignment length by ten, which resulted in the quantity of 110.
    
7.  Click Close twice.

Load a different formula file

1.  Click Analyze tab ![](../images/ac.menuaro.gif)QTO panel ![](../images/ac.menuaro.gif)QTO Manager ![](../images/GUID-13CDDC6E-1903-4681-8F6D-130DCE8D61F5.png) Find.
2.  In the QTO Manager vista, click Open![](../images/ac.menuaro.gif)Formula File.
    
    Use the Open dialog box to navigate to an existing rate item formula file. You can have several formula files available, and switch between them as needed.
    
3.  Click Cancel.

To continue this tutorial, go to [Exercise 7: Creating a Rate Item List](GUID-C6A00E96-808C-4156-A059-ED22818998FE.htm "In this exercise, you will add content to a sample rate item list, update the categorization file, and then examine the results.").

**Parent topic:** [Tutorial: Calculating and Reporting Quantities](GUID-709DB1D4-FB24-46F0-A54B-E2D9CC6D14F7.htm "In this tutorial, you will learn how to create and manage rate item data, associate rate item codes with several types of drawing objects, and generate rate item quantity reports.")