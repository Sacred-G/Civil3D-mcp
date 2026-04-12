---
title: "Exercise 2: Understanding the Toolspace"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-E20E87D2-1F6B-4BF7-A708-3156ED5F034B.htm"
category: "tutorial_understanding_the_autodesk_civil_3d_user_"
last_updated: "2026-03-17T18:42:04.848Z"
---

                 Exercise 2: Understanding the Toolspace  

# Exercise 2: Understanding the Toolspace

In this exercise, you will learn how to use the Autodesk Civil 3DToolspace, which provides an object-oriented view of your engineering data.

This exercise continues from [Exercise 1: Finding Tools](GUID-0EEE3051-D1BC-4039-943D-5A0AAB45B0DE.htm "In this exercise, you will learn how to locate the tools that are available for a given task.").

Explore the Prospector tab

Note:

This tutorial uses _Intro-1.dwg_ from the previous tutorial.

2.  In Toolspace, click the Prospector tab.
    
    Toolspace can be docked, but it can also float. The Prospector tab provides you with a categorized view of all objects in the drawing.
    
    Note:
    
    If the Toolspace is not visible, enter **ShowTS** on the command line. The command line is not case sensitive, but in this document, commands are written in mixed case.
    
3.  Click ![](../images/GUID-39A20497-2128-485B-A2C4-819813C980FC.png) next to the ![](../images/GUID-DCFF69A4-4AE1-4466-8C38-10E480B929E2.png) Sites collection.
    
    The drawing contains only one site, Site 1.
    
4.  Expand the **Site 1** collection.
    
    Notice that the Site 1 collection includes sub-collections for the following objects:
    
    *   ![](../images/GUID-A1AF1DE4-7370-4208-8B4C-ABB3CB562C79.png) Alignments
    *   ![](../images/GUID-F25BD16F-14A9-4933-A34C-97FD36278587.png) Feature Lines
    *   ![](../images/GUID-A76D3F3C-DE06-4F23-B3CE-86A10979C19C.png) Grading Groups
    *   ![](../images/GUID-4E05987C-7642-4F2D-95E1-F97C99C7C2C7.png) Plots
    
    A site provides a logical grouping of objects that form part of the same design project, or are otherwise related. An object can belong to only one site.
    
5.  Expand the ![](../images/GUID-4E05987C-7642-4F2D-95E1-F97C99C7C2C7.png) Plots collection to see the names of individual plots in Site 1.
    
    Notice that the drawing includes different types of plots, such as **Single-Family** and **Easement**.
    
6.  Click a plot name.
    
    The plot is displayed in a preview region of the Prospector tab.
    
    Note:
    
    If the preview does not work, you can activate it. First, ensure that the item preview button ![](../images/GUID-660E2463-9184-4EF9-8BA2-351CEC756621.png) at the top of the Prospector tab is pressed in. Then, right-click the Plots collection and click Show Preview.
    
7.  Right-click one of the **Single-Family** plots. Click Properties.
    
    The properties of the plot are displayed in a dialog box. Note the detailed survey data shown on the Analysis tab. Review these properties as you wish, but do not change anything.
    
8.  Click the Information tab. Change the Object Style from **Single-Family** to **Open Space**. Click OK.
    
    Notice that the appearance of the plot changes in the drawing, and in the item view preview. The name of the plot changes in the Plots collection on the Prospector tab. This happened because the style name is part of the naming template that is associated with the plot.
    
    A distinct set of custom styles for each Autodesk Civil 3D object type can be saved in a drawing template. Object styles can be changed as needed to change the display of an object.
    

Explore the Settings tab

1.  Click the Settings tab.
    
    The Settings tab contains a tree structure of object styles and settings for the drawing. Like the Prospector tab, it has object collections at several levels.
    
2.  Expand the Settings tree by clicking ![](../images/GUID-39A20497-2128-485B-A2C4-819813C980FC.png) next to the ![](../images/GUID-5B6EA6FC-F65B-4557-BC7C-30DAAEBE2A51.png) _Intro-1_. Expand the ![](../images/GUID-C5C2C7CF-3596-4DF4-A07B-9850369235F7.png) Plot![](../images/ac.menuaro.gif)![](../images/GUID-2E4799E9-518E-4844-A055-98090D9A377E.png) Plot Styles collection.
    
    This collection displays the styles that are available in the current drawing.
    
3.  Right-click the **Standard** plot style. Click Edit.
    
    The object style dialog box displays the current style attributes. Explore the contents of the tabs to see the various attributes that can be changed when you create a style.
    
4.  Click Cancel.
    
    **Further exploration:** Expand the Settings tree and look at several style objects and commands. Right-click various objects to see the available menu selections, but do not change anything.
    

To continue this tutorial, go to [Exercise 3: Using the Panorama Window](GUID-C72EBB8E-D382-408B-94FF-896BFF054D6E.htm "In this exercise, you will learn how you can use and customize the Panorama window.").

**Parent topic:** [Tutorial: Understanding the Autodesk Civil 3D User Interface](GUID-8E2345B7-AAF2-4AC6-A4DE-70C6EBD26FC7.htm "In this tutorial, you'll examine some of the major components of the Autodesk Civil 3D user interface.")