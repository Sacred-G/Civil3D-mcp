---
title: "Exercise 2: Inserting an Expression Into a Label Style"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-2B7FA445-CA59-4044-8B5E-B7478662F7F7.htm"
category: "tutorial_using_expressions_in_labels"
last_updated: "2026-03-17T18:43:21.325Z"
---

                  Exercise 2: Inserting an Expression Into a Label Style  

# Exercise 2: Inserting an Expression Into a Label Style

In this exercise, you will insert an expression into an existing label style.

After you set up expressions, they are available in the Properties list in the Text Component Editor so that you can add them to label styles. In effect, expressions become new properties that you can use to compose a label style.

Expressions are unique to a particular label style type. Only those properties that are relevant to the label style type are available in the Expressions dialog box.

This exercise continues from [Exercise 1: Creating an Expression](GUID-50AF5191-2245-4FF6-9708-6590339F17BA.htm "In this exercise, you will create an expression that calculates the magnetic compass direction of an alignment at each geometry point.").

Insert an expression into a label style

Note:

This exercise uses _Labels-6a.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Settings tab, expand Alignment![](../images/ac.menuaro.gif)Label Styles![](../images/ac.menuaro.gif)Chainage![](../images/ac.menuaro.gif)Geometry Point. Right-click **Additional Details**. Click Edit.
3.  In the Label Style Composer dialog box, on the Layout tab, under Text, in the Contents row, click the Value column. Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
4.  In the Text Component Editor, in the Preview pane, place the cursor on a new line below the last property (**True Instantaneous Direction**).
5.  Enter the following text, including the space after the colon: **MAG:** .
6.  On the Properties tab, in the Properties list, select **Magnetic Direction**, which is the expression you created in [Exercise 1: Creating an Expression](GUID-50AF5191-2245-4FF6-9708-6590339F17BA.htm "In this exercise, you will create an expression that calculates the magnetic compass direction of an alignment at each geometry point."), at the top of the list.
7.  In the table, ensure that the value for Unit is Degree, and the Format is DD°MM’SS.SS” (unspaced).
8.  Ensure that the cursor in the right window is located just after the **Mag:** label text. Click ![](../images/GUID-70B44105-B2EC-4016-A100-FA435F289B52.png) to add the magnetic compass direction to the label formula.
9.  Click OK.
    
    The modified labels appear in the Preview pane of the Label Style Composer dialog box.
    
10.  In the Label Style Composer dialog box, click OK.
11.  Press Esc to deselect the labels.
     
     Examine the geometry point labels to see the effect of the expression you added.
     

**Parent topic:** [Tutorial: Using Expressions in Labels](GUID-ADE0D8A5-CB73-40C2-B7FF-10E25FEE55F1.htm "This tutorial demonstrates how to use expressions, which are mathematical formulas that modify a property within a label style.")