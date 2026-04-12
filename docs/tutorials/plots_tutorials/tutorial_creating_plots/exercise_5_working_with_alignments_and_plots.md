---
title: "Exercise 5: Working with Alignments and Plots"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-AB94147B-E621-4D47-912D-91971CAC2F9E.htm"
category: "tutorial_creating_plots"
last_updated: "2026-03-17T18:42:34.649Z"
---

                   Exercise 5: Working with Alignments and Plots  

# Exercise 5: Working with Alignments and Plots

In this exercise, you will create an alignment outside of a site and move existing alignments out of sites. These practices eliminate unwanted plots being created by alignments interacting with a site.

When an alignment is in a site, it creates new plots if it forms closed areas by crossing over itself or other alignments or plots on the same site.

Examine alignments in a site

1.  Open _Parcel-1E.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains five roads off of a main West-East road. The two Northern side roads have centerline alignments, each of which created a plot in the carriageway and cul-de-sac center island. In the next few steps, you will convert the centerline of one of the Southern side roads to an alignment and prevent it from forming plots.
    
2.  In Toolspace, on the Prospector tab, expand Sites![](../images/ac.menuaro.gif)Site 1![](../images/ac.menuaro.gif)Alignments![](../images/ac.menuaro.gif)Centerline Alignments.
    
    Notice that the four existing centerline alignments all reside in Site 1, while the top-level Alignments collection (above the Sites collection) is empty.
    

Create an alignment outside of a site

1.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel ![](../images/ac.menuaro.gif)Alignment drop-down ![](../images/ac.menuaro.gif)Create Alignment From Objects ![](../images/GUID-6A159C4E-3903-4519-A90D-58731DA1008F.png) Find.
2.  When prompted, select the red centerline for the road in the lower middle of the drawing.
3.  Press Enter twice.
    
    The Create Alignment from Objects dialog box is displayed. Notice that, by default, the Site is set to <None>.
    
4.  Click OK.
5.  Expand the top-level Alignments![](../images/ac.menuaro.gif)Centerline Alignments collection in Prospector.
    
    Notice that the new alignment was placed in the Alignments collection, and did not form a plot in the cul-de-sac center island. This happened because you accepted the default <None>Site selection when you were prompted to select a site in step 3. In the next few steps, you will move one of the two existing cul-de-sac road alignments out of its existing site and into the top-level Alignments collection.
    

Move alignments out of a site

1.  In Toolspace, on the Prospector tab, right-click **Alignment - (4)**, which is the cul-de-sac alignment to the Northeast of the alignment you created. Click Move To Site.
2.  In the Move To Site dialog box, make sure that the Destination Site is set to **<None>** .
3.  Click OK.
    
    Notice that in Toolspace on the Prospector tab, Alignment - (4) has moved to the top-level Alignments collection. In the drawing window, the plot label and hatching has been removed from the cul-de-sac center island.
    
    ![](../images/GUID-CB37C024-E95D-403E-8471-23279F94AC88.png)
    
    **Further Exploration**: Repeat the preceding steps to move Alignment - (3) to the top-level Alignments collection.
    
4.  Close _Plot-1E.dwg_.

To continue to the next tutorial, go to [Editing Plot Data](GUID-F66D1115-3033-45BD-B47E-A07E4187EDE8.htm "This tutorial demonstrates two ways of resizing a plot by moving a plot line.").

**Parent topic:** [Tutorial: Creating Plots](GUID-29D40831-99A7-4CC4-BB29-926433D210C5.htm "This tutorial demonstrates the main methods for creating plots.")