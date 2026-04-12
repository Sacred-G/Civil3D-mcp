---
title: "Exercise 4: Verifying the New Part"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-9E457FD2-49EE-4BB3-BB36-4D2DB034E61A.htm"
category: "tutorial_creating_a_cylindrical_manhole_structure"
last_updated: "2026-03-17T18:43:10.749Z"
---

                 Exercise 4: Verifying the New Part  

# Exercise 4: Verifying the New Part

In this exercise, you will verify that the new part reacts as expected in a drawing by opening a drawing, regenerating the structure catalog, and accessing the new part from a Part List.

This exercise continues from [Exercise 3: Matching Offsets and Dimensions to Parameters](GUID-70C8BFFB-EBC8-4272-862A-CDAB162E4B27.htm "In this exercise, you will match the work plane offsets and diameter dimensions to the parameters. The next step, though, is to create a few more structure parameters.").

1.  At this point, it is important to determine how the new part reacts in a drawing situation. Exit the Part Builder utility by clicking the small X in the upper right corner of the Part Browser. (The Part Browser is the left pane portion of the Part Builder application window.) If you are prompted to save the part, click Yes. Part Builder closes.
2.  Open the tutorial drawing called Part Builder-1a.dwg, and enter PartCatalogRegen in the command line. Enter S to indicate you want to regenerate the structure catalog. Press Enter. Click OK, and then press ESC to exit the PARTCATALOGREGEN command. A drawing opens with a sample surface, and the part catalog regenerates.
3.  Create a new Parts List called “Test Parts List”. The Parts List dialog box is displayed.
4.  On the Structures tab, click Add Part Family. The Add Part Family dialog box is displayed.
5.  Click your part, and then click OK. An entry is displayed on the Structures tab for the manhole.
6.  Right-click your part and then click Add Part Size. The Part Size Creator dialog box is displayed. This dialog box lists all of the parameters as constants right now.
    
    Note:
    
    Do a quick visual check on the Part Size Creator dialog box to verify that all values are nonzero. If any of the properties are displayed in the Part Size Creator dialog box as zero, the part will **not** insert into the drawing properly.
    
7.  Now add a few pipes to your Test Parts List. Exit the Parts List dialog box. A new Parts List is created.
8.  Create a pipe network by layout that contains approximately three structures and two pipes.
9.  Observe how your structures are displayed in plan (2D) views and in 3D view (Object Viewer).
    
    ![](../images/GUID-0FE227E1-EE29-4FC7-AB07-B676482BDD2C.png)
    
10.  Make an alignment from network parts, and a corresponding profile view. Note how your structures respond to edits, adjustments, and changes of pipe inverts.
     
     ![](../images/GUID-E62598A2-C7E7-4863-8772-6587FC0C3800.png)
     
11.  Click Home tab ![](../images/ac.menuaro.gif)Create Design panel![](../images/GUID-A7096E57-563B-43D0-B57A-FA69853A76A6.gif)Part Builder ![](../images/GUID-62BEBEF8-D435-4915-9305-B1BAA4A31516.png) Find. Select Manhole 206 Type A, and then click Modify Part Sizes. Part Builder opens to your custom part.
12.  Add some variables to the Cone Height (SCH), Riser 1 Height (SRZ1), and Barrel Height (SBH) to make it easier to edit your structure after it has been inserted into the drawing. Right-click Size Parameters and then click Edit Configuration. The Edit Part Sizes dialog box is displayed.
13.  Under SCH, SRZ1 and SBH, change the Data Storage from Constant to List. Each parameter now accepts additional values.
14.  Click Values from the drop-down list at the top of the Edit Part Sizes dialog box. The Edit Part Sizes dialog box switches to Values mode.
15.  Move your cursor to become active in the SCH column, and then click the Edit button from the Edit Part Sizes dialog box toolbar. The Edit Values dialog box is displayed.
16.  Click Add to add the following values: 0, 6, 12, 18. Click OK to exit the dialog box. A list of values is now be available for SCH.
17.  Repeat Steps 16 and 17 for SRZ1 and SBH, adding the following values to each list:
     
     *   SRZ1 = 12, 24, 36, 48, 60
     *   SBH = 100, 120, 140, 180
     
     Click OK to exit the Edit Part Sizes dialog box. Lists of values are now available for SRZ1 and SBH.
     
18.  Save your part.
     
     Additional customizations can be made to the geometry of a manhole such as this one using the principles learned in the vault structure tutorial exercise, as well as other part building exercises.
     
19.  Exit the Part Builder utility, click the small X in the upper right corner of the Part Browser. (The Part Browser is the left pane portion of the Part Builder application window.) If you are prompted to save the part, click Yes. Part Builder closes.

You can open _Part Builder-1.dwg_ in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79) to see what this finished part looks like in a drawing.

**Parent topic:** [Tutorial: Creating a Cylindrical Manhole Structure](GUID-3C390412-421F-4B68-A826-EA47FE49E7F9.htm "This tutorial demonstrates how to use Part Builder to create a cylindrical-shaped storm drainage manhole structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the manhole.")