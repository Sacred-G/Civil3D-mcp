---
title: "Exercise 3: Adding a Data Box to a Section View"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-126EF350-2364-4621-8F7E-3387174B95DC.htm"
category: "tutorial_adding_data_to_a_section_view"
last_updated: "2026-03-17T18:42:59.366Z"
---

                  Exercise 3: Adding a Data Box to a Section View  

# Exercise 3: Adding a Data Box to a Section View

In this exercise, you will add a data box, which is an optional graphic frame that is associated with the section view.

This exercise continues from [Exercise 2: Adding a Section View Gradient Label](GUID-21890D53-6A5B-427F-88A0-EBD61C712EC2.htm "In this exercise, you will create a section view gradient label.").

Add a data box to a section view

1.  Open Section-Data-Band.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Select the section view. Right-click. Click Section View Properties.
3.  On the Section View Properties dialog box, on the Bands tab, specify the following parameters:
    *   Band Type: **Section Segment**
    *   Select Band Style: **FG Segments**
4.  Click Add.
5.  In the List of Bands table, scroll to the right and click the Section 1 cell for the new band that you created. From the drop-down list, select the **Corridor (1) Top** surface.
    
    Note:
    
    The name of the Corridor (1) Top surface varies depending on the section view that you are editing.
    
6.  Click Apply.
    
    A data box is added to the section view in the drawing. Notice that there is no data displayed in the band. This happened because the default Weeding factor specifies that any section segments that are less than 100 drawing units long are not displayed in the band. In the next step, you will reduce the Weeding factor and examine the results.
    
7.  In the Section Segment row, change the Weeding value to **5.0000**.
8.  Click OK.
    
    Segment lengths that are greater than 5 drawing units long are annotated in the new data box.
    

**Parent topic:** [Tutorial: Adding Data to a Section View](GUID-F9CF9DD7-EB09-41E2-BCEE-586730811558.htm "This tutorial demonstrates how to add annotative data to a section view.")