---
title: "Exercise 7: Creating a Rate Item List"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-C6A00E96-808C-4156-A059-ED22818998FE.htm"
category: "tutorial_calculating_and_reporting_quantities"
last_updated: "2026-03-17T18:43:04.132Z"
---

                 Exercise 7: Creating a Rate Item List  

# Exercise 7: Creating a Rate Item List

In this exercise, you will add content to a sample rate item list, update the categorization file, and then examine the results.

You can use this workflow to create custom rate item lists and categorization files from existing data.

This exercise continues from [Exercise 6: Working with Rate Item Formulas](GUID-AA55D4D1-EB5E-4475-81CF-95EB5D88E68F.htm "In this exercise, you will build a mathematical formula that applies a rate item to a corridor at a specified interval.").

Create a new rate item list

1.  Open _Quantities-7.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a commercial site, which consists of a building footprint, a parking lot, and access roads.
    
2.  Click Analyze tab ![](../images/ac.menuaro.gif)QTO panel ![](../images/ac.menuaro.gif)QTO Manager ![](../images/GUID-13CDDC6E-1903-4681-8F6D-130DCE8D61F5.png) Find.
3.  In Windows Explorer, navigate to the [Data\\Rate Item Data\\Getting Started folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24223).
4.  Select _Getting Started.csv_. Right-click. Click Open With![](../images/ac.menuaro.gif)Microsoft Office Excel.
    
    The rate item list opens in Microsoft Excel. Notice that the Rate Item, Item Description, and Unit\_E columns correspond to the Rate Item ID, Description, and Unit Type columns in the QTO Manager vista. You can use this structure to create a custom rate item list, and then save it as a CSV file.
    
5.  In Microsoft Excel, select row 2. Right-click. Click Insert.
6.  Repeat Step 5 four times to create five empty rows.
7.  Enter the following information in the new rows:
    
    Rate Item
    
    Item Description
    
    Unit
    
    14101-0025
    
    SIGN, SPEED LIMIT, 25
    
    EACH
    
    14101-0030
    
    SIGN, SPEED LIMIT, 30
    
    EACH
    
    14102-0011
    
    SIGN, RIGHT TURN ONLY
    
    EACH
    
    14102-0012
    
    SIGN, RIGHT TURN OR STRAIGHT
    
    EACH
    
    14102-0020
    
    SIGN, LEFT TURN ONLY
    
    EACH
    
8.  Save the rate item list in the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Name the rate item file _Tutorial\_QTO\_Pay-Items.csv_.

Update the rate item categorization file

1.  In the QTO Manager vista, expand the **Division 150**![](../images/ac.menuaro.gif)**Group 151** category.
2.  In Windows Explorer, navigate to the [Data\\Rate Item Data\\Getting Started folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24223).
3.  Select _Getting Started Categories.xml_. Right-click. Click Open With![](../images/ac.menuaro.gif)Notepad.
    
    Tip:
    
    You can use any XML editor to modify the rate item categorization file.
    
    The categorization file opens in Notepad.
    
4.  In Notepad, select the following lines:
    
    <category type="value" start="" end="" title="Division 150" description="Project Requirements">
     <category type="value" start="" end="" title="Group 151" description="Mobilization">
     <category type="value" start="15101" end="" title="Section 15101" description="Mobilization"/>
     </category>
    
    Compare this content to the categories displayed in the QTO Manager vista. The Start values specify the rate items that are in each category. For example, rate items that start with 15101 are included in the Section 15101 category.
    
    You will use this structure as a basis to create a new set of categories.
    
5.  Right-click the highlighted lines. Click Copy.
6.  Place the cursor at the beginning of the block you copied in the previous step. Press Enter.
7.  Place the cursor in the empty row. Right-click. Click Paste.
8.  In the block that you copied, copy the following line:
    
     <category type="value" start="15101" end="" title="Section 15101" description="Mobilization"/>
    
9.  Place the cursor at the end of the line that you copied in the previous step. Press Enter.
10.  Right-click. Click Paste.
11.  In the lines you pasted, replace the existing values with the following values:
     
     Start
     
     End
     
     Title
     
     Description
     
      
     
      
     
     Division 140
     
     Traffic Control
     
      
     
      
     
     Group 141
     
     Signs
     
     14101
     
      
     
     Section 14101
     
     Speed Limit
     
     14102
     
      
     
     Section 14102
     
     Traffic Direction
     
12.  Select the </category> line at the end of the new block. Right-click. Click Copy.
13.  Place the cursor at the end of the line you copied in the previous step. Press Enter.
14.  Right-click. Click Paste.
     
     When you are finished, the new code should look like this:
     
     <category type="value" start="" end="" title="Division 140" description="Traffic Control">
      <category type="value" start="" end="" title="Group 141" description="Signs">
      <category type="value" start="14101" end="" title="Section 14101" description="Speed Limit"/>
      <category type="value" start="14102" end="" title="Section 14102" description="Traffic Direction"/>
      </category>
      </category>
     
15.  Save the rate item list in the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Name the rate item file _Tutorial\_QTO\_Pay-Item\_Categorization.xml_.

Load the updated files into a drawing

1.  In the QTO Manager vista, click ![](../images/GUID-2E4799E9-518E-4844-A055-98090D9A377E.png)Open Pay Item File.
2.  In the Open Rate Item File dialog box, specify the following parameters:
    
    Note:
    
    The Rate Item File and Rate Item Categorization File are located in the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832).
    
    *   Rate Item File Format: **CSV (Comma Delimited)**
    *   Rate Item File: _Tutorial\_QTO\_Pay-Items.csv_
    *   Rate Item Categorization File: _Tutorial\_QTO\_Pay-Item\_Categorization.csv_
3.  Click OK.
4.  In the QTO Manager vista, expand the **Division 140**![](../images/ac.menuaro.gif)**Group 141**![](../images/ac.menuaro.gif)**Section 14101** and **Section 14102** categories.
    
    Notice that the new rate items and categories are present.
    

**Parent topic:** [Tutorial: Calculating and Reporting Quantities](GUID-709DB1D4-FB24-46F0-A54B-E2D9CC6D14F7.htm "In this tutorial, you will learn how to create and manage rate item data, associate rate item codes with several types of drawing objects, and generate rate item quantity reports.")