---
title: "Exercise 1: Viewing Survey Data"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-95633FF1-4B40-4790-B702-6AFE4DE16261.htm"
category: "tutorial_viewing_and_editing_survey_data"
last_updated: "2026-03-17T18:42:16.665Z"
---

                 Exercise 1: Viewing Survey Data  

# Exercise 1: Viewing Survey Data

In this exercise, you will use the Toolspace![](../images/ac.menuaro.gif)Survey tab and panorama vistas to view some of the data that you imported from the field book file.

You will also browse to the newly created network and figure objects in the Autodesk Civil 3DProspector tab and drawing.

View the survey data

Note:

This exercise uses _Survey-3.dwg_, which you saved in the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832) during the previous exercise. If you did not do this, you can use the copy of _Survey-3.dwg_ that is in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79), but you will get duplicate objects when you update the survey data.

2.  In Toolspace, on the Survey tab, expand Survey Databases![](../images/ac.menuaro.gif)Survey 1 ![](../images/ac.menuaro.gif)Networks![](../images/ac.menuaro.gif)**Survey Network 1** to display the collections of survey data, including control points, directions, and setups.
    
    Note:
    
    If you cannot expand the collection, right-click the database name and click Open For Edit.
    
3.  To view the control points, click the Control Points collection.
    
    The control points are displayed in the Toolspace list view.
    
    Note:
    
    By default, if the Toolspace is docked, the list view is the lower part of the Toolspace.
    
4.  To view the setups, select the Setups collection.
    
    The setups are displayed in the Toolspace list view.
    
5.  To view observations for a setup, right-click the setup and click Edit Observations.
    
    The Observations Editor vista is displayed with all the observations for the selected setup.
    
    Note:
    
    The observations for the setup are highlighted in the drawing.
    
6.  Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to close the Observations Editor.
7.  To view the figures, select the Figures collection.
    
    The figures are displayed in the Toolspace list view.
    
8.  In Toolspace, click the Prospector tab and expand the Survey collection to display the collections of survey networks and figures.
    
    These collections are for the survey network and figure drawing objects as opposed to the survey database data that is displayed on the Survey tab.
    

Browse to the survey data

1.  To view a figure in the drawing, on the Prospector tab, expand the Figures collection, right-click the figure name, for example **BLDG1**, and click Zoom To.
    
    The drawing zooms to the selected figure.
    
2.  In the drawing, select the figure that you zoomed to. Right-click. Click Browse To Survey Data.
    
    The Toolspace switches to the Survey tab with the figure selected. The survey data for the figure is displayed in the Figures Editor.
    
    Note:
    
    If you use the Survey-3.dwg that is in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79), you are notified that the associated survey database is not available. Click OK.
    
3.  In the drawing window, hover your cursor over one of the survey network components extending from **BLDG1**. Notice that the properties for the network object are displayed in the tooltip.
    
    ![](../images/GUID-D4528B3A-78BA-4E15-82C4-774FA57ED889.png)
    
4.  Right-click the network object. Click Browse To Survey Network.
    
    In Toolspace, on the Survey tab, the survey network is selected. The survey data for the survey network is displayed in the list view.
    
5.  Ctrl+click one of the survey network components shown in the previous image. Right-click. Click Browse To Survey Data.
    
    The related setup is highlighted the Observations Editor vista.
    
6.  Click ![](../images/GUID-4E47E8C6-B677-488E-B92D-895598698585.png) to dismiss the Observations Editor vista.

To continue this tutorial, go to [Exercise 2: Editing a Figure](GUID-CE815046-F38B-4F08-9931-915E20B2F5FA.htm "In this exercise, you will edit a figure to change its display in the drawing.").

**Parent topic:** [Tutorial: Viewing and Editing Survey Data](GUID-48040D82-47A0-41C8-86B9-247D2520C977.htm "This tutorial demonstrates how to view and modify survey data in your drawing.")