---
title: "Exercise 3: Creating a Superelevation View"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-832958B2-A3FC-4F05-B1EF-14ABA74CC950.htm"
category: "tutorial_applying_superelevation_to_an_alignment"
last_updated: "2026-03-17T18:42:28.142Z"
---

                Exercise 3: Creating a Superelevation View  

# Exercise 3: Creating a Superelevation View

In this exercise, you will display superelevation data in a graph, which you can use to graphically edit superelevation data.

This exercise continues from [Exercise 2: Calculating Superelevation for an Individual Curve](GUID-910D4711-E059-4C57-93AE-87B2AEAEFE39.htm "In this exercise, you will calculate superelevation for a single curve in an alignment that already has superelevation data calculated for other curves.").

Create a superelevation view

1.  Open _Align-Superelevation-3.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Select the alignment.
3.  Click Alignment tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Superelevation drop-down ![](../images/ac.menuaro.gif)Create Superelevation View ![](../images/GUID-B2FCF102-0F53-43D8-B4A0-31D97F52E7B3.png) Find.
    
    The Create Superelevation View dialog box enables you to specify the superelevation view properties, including the name and style of the view.
    
4.  In the Create Superelevation View dialog box, under Superelevation View Style, click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection.
5.  In the Superelevation View Style dialog box, on the Display tab, change the Graph Reference Line Component Type Color to White.
    
    In a superelevation view, the reference line is a horizontal line that indicates the zero lane crossfall. In the superelevation view properties, you can specify the colors of the lines that represent the slopes of the lanes and shoulders.
    
    The other superelevation view style properties, such as those that affect the grid and labels, are similar to the properties found in profile view and section view styles.
    
6.  Click OK.
7.  In the Create Superelevation View dialog box, under Specify Superelevation Display Options, in the Right Outside Lane row, double-click the Color cell.
8.  In the Select Color dialog box, under Color, enter red. Click OK.
9.  Repeat Steps 7 and 8 to change the other lane colors:
    *   Left Outside Shoulder: 150
    *   Right Outside Shoulder: 11
10.  Click OK.
11.  In the drawing, pan to an empty area above the surface. Click to place the superelevation view.

Examine the superelevation view

1.  Zoom in to examine the superelevation view.
    
    The white line represents a baseline of 0% slope. The red and blue lines represent the slopes of the outside lanes and outside shoulders.
    
2.  Pan and zoom to the area of the superelevation view that has Curve.2 labels on the horizontal axes.
    
    As the alignment tapers in to the curve, the red and blue lines illustrate the following tapers:
    
    *   At chainage 0+286.28, the left shoulder (light red) starts to transition from the normal -5% slope.
    *   At chainage 0+304.54, the left lane (dark red) starts to transition from the normal -2% slope.
        
        Note:
        
        For the remainder of the curve, the dark red line is not visible because it is obstructed by the light red line.
        
    *   At chainage 0+316.71, the left lane and shoulder are at 0% slope.
    *   At chainage 0+328.88, the right lane (dark blue) starts to transition from the normal -2% slope.
    *   At chainage 0+344.71, all elements are fully superelevated: the left lane and shoulder are at 5%, the right lane is at -4.60%, and the right shoulder is at -5%.
    
    Starting at chainage 0+512.66, the lanes begin to transition out of the fully superelevated state.
    
3.  Pan to the area of the superelevation view that has Curve.4 labels on the horizontal axes.
    
    Notice that there is a curve where each line transitions in or out of a superelevated state. Curves are present on this superelevation curve, but not the others, because you selected the curve smoothing option during [Exercise 2: Calculating Superelevation for an Individual Curve](GUID-910D4711-E059-4C57-93AE-87B2AEAEFE39.htm "In this exercise, you will calculate superelevation for a single curve in an alignment that already has superelevation data calculated for other curves."). Later in this exercise, you will learn how to apply curve smoothing to an existing superelevation curve.
    

To continue this tutorial, go to [Exercise 4: Adding and Modifying Superelevation Chainages](GUID-53B257E3-D572-407F-8582-96B731644DEE.htm "In this exercise, you will resolve overlap between two superelevated curves by adding and removing critical chainages, and then editing existing superelevation data.").

**Parent topic:** [Tutorial: Applying Superelevation to an Alignment](GUID-AA0068E0-2858-4067-9104-161112DEDBF6.htm "In this tutorial, you will calculate superelevation for alignment curves, create a superelevation view to display the superelevation data, and edit the superelevation data both graphically and in a tabular format.")