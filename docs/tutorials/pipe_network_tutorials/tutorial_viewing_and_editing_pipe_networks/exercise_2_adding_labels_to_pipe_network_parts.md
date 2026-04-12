---
title: "Exercise 2: Adding Labels to Pipe Network Parts"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-A30B903F-AC71-4ECB-965B-B06578E788C8.htm"
category: "tutorial_viewing_and_editing_pipe_networks"
last_updated: "2026-03-17T18:43:08.186Z"
---

                  Exercise 2: Adding Labels to Pipe Network Parts  

# Exercise 2: Adding Labels to Pipe Network Parts

In this exercise, you will add labels to the pipe network parts drawn in both plan and profile views.

Note:

For more detailed tutorials on labels, go to the [Labels and Tables Tutorials](GUID-BDF3F02E-838E-443D-BFE3-3033F03E214F.htm "These tutorials will get you started creating and editing labels, label styles, and tables.").

This exercise continues from [Exercise 1: Drawing Pipe Network Parts in a Profile View](GUID-5178E9B2-0E48-4BB9-A971-C90A6629728B.htm "In this exercise, you will draw the pipe network parts in a profile view.").

Add labels to pipe network parts in a profile

Note:

This exercise uses _Pipe Networks-3.dwg_ with the modifications you made in the previous exercise.

2.  Zoom to the pipe network parts drawn in the profile view.
3.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Labels menu ![](../images/ac.menuaro.gif)Pipe Network![](../images/ac.menuaro.gif)Add Pipe Network Labels![](../images/GUID-10094573-05A5-439E-A30D-2E506775AA08.png).
4.  In the Add Labels dialog box, specify the following parameters:
    *   Label Type: **Single Part Profile**
    *   Pipe Label Style: **Standard**
5.  Click Add.
6.  In the drawing window, click Pipe - (1), which is between stations 7+00 and 8+00 of the layout profile displayed in the profile view.
    
    The pipe is labeled with its description. In the following steps, you will create a label style that displays the level of the start invert of the pipe.
    

Create a pipe label style

1.  In the Add Labels dialog box, next to the Pipe Label Style list, click the arrow next to ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png). Click ![](../images/GUID-CA1AD018-7C68-4826-8BAB-59D50446DD6F.png)Copy Current Selection.
2.  In the Label Style Composer dialog box, on the Information tab, for Name, enter **Start Invert Level**.
3.  On the Layout tab, click the Contents value under Text. Click ![](../images/GUID-65F1F27B-38A6-4387-A336-CD752ADA52AF.png).
4.  In the Text Component Editor dialog box, on the Properties tab, in the Properties list, select **Start Invert Level**.
5.  Click ![](../images/GUID-70B44105-B2EC-4016-A100-FA435F289B52.png).
6.  In the text editor window, click the <\[Description(CP)\]> property field. Press Delete.
7.  Your label content should look like this:
    
    ![](../images/GUID-8FB27920-EE5E-42F6-800F-B59368EDBC35.png)
    
8.  Click OK twice.
9.  In the Add Labels dialog box, specify the following parameters:
    *   Label Type: Single Part Profile
    *   Pipe Label Style: **Start Invert Level**
10.  Click Add.
11.  In the profile view, click Pipe - (2), which is between stations 8+00 and 9+50 of the layout profile.
12.  Press Esc to end the add label command.
13.  Click the label text to select the label.
14.  Click the diamond-shaped label edit grip to make it active. Click a new location for the label at the start end of the pipe, which is the end located next to chainage 8+00.
     
     The start and end of a pipe is determined using the direction in which the pipe was drawn.
     
15.  Click the square label edit grip to make it active. Click a new location for the label text that moves it off the pipe.
16.  Press Esc to deselect the label.
     
     Next, you will add a spanning label to a series of two pipes in plan view.
     

Add labels to a single pipe network part in plan view

1.  Pan and zoom until you can clearly see the North-South pipe run along the XC\_STORM alignment.
2.  In the Add Labels dialog box, specify the following parameters:
    *   Label Type: **Spanning Pipes Plan**
    *   Pipe Label Style: **2D Length - Total Span**
3.  Click Add.
4.  Click both pipes along the XC\_STORM alignment, then press Enter.
5.  When prompted, click a location along the pipe span to place the label.
    
    The span label is placed on the pipe run in the location you specified. To see which pipes are included in the span, hover the cursor over the label to highlight the pipes.
    

Add labels to a multiple pipe network parts in plan view

1.  Pan and zoom until you can clearly see the plan view of the pipe network.
2.  In the Add Labels dialog box, specify the following parameters:
    *   Label Type: **Entire Network Plan**
    *   Pipe Label Style: **Standard**
    *   Structure Label Style: **Structure Name**
3.  Click Add.
4.  Click a part in the pipe network. All pipes and structures are labeled using the styles you selected.
5.  In the Add Labels dialog box, click Close.

To continue this tutorial, go to [Exercise 3: Editing Pipe Network Parts in a Profile View](GUID-9651FF33-B254-47B5-96F5-6EAF285B1047.htm "In this exercise, you will edit the pipe network parts drawn in a profile view using editing grips and by directly editing the part properties.").

**Parent topic:** [Tutorial: Viewing and Editing Pipe Networks](GUID-DCDFFC24-DBD6-4A45-9F9D-7EFEAB45123F.htm "This tutorial demonstrates how you can view and edit the parts of your pipe network in profile and section views.")