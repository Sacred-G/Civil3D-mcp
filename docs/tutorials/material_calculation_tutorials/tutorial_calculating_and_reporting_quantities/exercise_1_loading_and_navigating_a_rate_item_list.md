---
title: "Exercise 1: Loading and Navigating a Rate Item List"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-07D62C88-4439-49F9-A03A-E5A3C89F4C67.htm"
category: "tutorial_calculating_and_reporting_quantities"
last_updated: "2026-03-17T18:43:03.112Z"
---

                  Exercise 1: Loading and Navigating a Rate Item List  

# Exercise 1: Loading and Navigating a Rate Item List

In this exercise, you will open a sample file that contains a rate item list and examine the contents.

A _rate item file_ contains a listing of the rate item codes, descriptions, and units of measure.

An optional _rate item categorization file_ categorizes the rate items into manageable groups. A rate item categorization file groups similar rate items by common rate item code prefixes.

You will learn how to create a custom pay item file and categorization file in [Exercise 7: Creating a Rate Item List](GUID-C6A00E96-808C-4156-A059-ED22818998FE.htm "In this exercise, you will add content to a sample rate item list, update the categorization file, and then examine the results.").

Load a rate item file

1.  Open _Quantities-1.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a commercial site, which consists of a building footprint, a parking lot, and access roads.
    
2.  Click Analyze tab ![](../images/ac.menuaro.gif)QTO panel ![](../images/ac.menuaro.gif)QTO Manager ![](../images/GUID-13CDDC6E-1903-4681-8F6D-130DCE8D61F5.png) Find.
3.  In the QTO Manager vista, click ![](../images/GUID-2E4799E9-518E-4844-A055-98090D9A377E.png)Open Pay Item File.
4.  In the Open Rate Item File dialog box, specify the following parameters:
    
    Note:
    
    The Rate Item File and Rate Item Categorization File are located in the [Data\\Rate Item Data\\Getting Started folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24223).
    
    *   Rate Item File Format: **CSV (Comma Delimited)**
    *   Rate Item File: **Getting Started.csv**
    *   Rate Item Categorization File: **Getting Started Categories.xml**
5.  Click OK.
    
    The rate item categories are displayed in the QTO Manager vista. The Rate Item ID column lists the categories that are specified in the rate item categorization file. The Description column identifies the contents of each category.
    

Search for rate items

1.  In the Rate Item ID column, expand several Rate Item ID categories.
    
    Notice that any material or unit of work can be classified as a rate item. The Unit Type column identifies the unit of measure assigned to each rate item.
    
2.  Click ![](../images/GUID-727B0387-A0EE-493D-91A6-D6EFA45BF1B1.png)Turn Off Categorization.
    
    The categories specified by the rate item categorization file are removed. The QTO Manager displays all rate items in the rate item file.
    
3.  In the Enter Text To Filter Rate Items field, enter **Asphalt**. Click ![](../images/GUID-68D61EEF-EA97-4BD2-954C-FFAE95AF37C2.png).
    
    The filtered rate item list displays only rate items that have a description that contains the word “asphalt”.
    
4.  In the Enter Text To Filter Rate Items field, enter **60902-0800**. Click ![](../images/GUID-68D61EEF-EA97-4BD2-954C-FFAE95AF37C2.png).
    
    A single rate item is displayed. This method of filtering searches in both the rate item ID and description, and is helpful if you know the rate item number.
    
5.  In the Rate Item ID column, right-click the rate item number. Click Add To Favorites List.
6.  Expand the Favorites collection.
    
    The specified rate item is displayed in the Favorites list, which is a convenient location to save frequently used rate items.
    
    Note:
    
    The contents of the Favorites category are saved with the drawing. To save time during this tutorial, the rate items you will use are saved as Favorites in subsequent drawings.
    

To continue this tutorial, go to [Exercise 2: Assigning Rate Item Codes to AutoCAD Objects](GUID-826E3EC7-9393-44D2-B3FA-B6A7790E263A.htm "In this exercise, you will assign rate item codes to a variety of AutoCAD objects, including lines, blocks, and closed polyline areas.").

**Parent topic:** [Tutorial: Calculating and Reporting Quantities](GUID-709DB1D4-FB24-46F0-A54B-E2D9CC6D14F7.htm "In this tutorial, you will learn how to create and manage rate item data, associate rate item codes with several types of drawing objects, and generate rate item quantity reports.")