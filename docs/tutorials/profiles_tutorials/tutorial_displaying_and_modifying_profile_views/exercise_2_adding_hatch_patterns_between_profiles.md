---
title: "Exercise 2: Adding Hatch Patterns Between Profiles"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-0FEFEE68-DD81-4DC7-BFC6-1F33F6BDB594.htm"
category: "tutorial_displaying_and_modifying_profile_views"
last_updated: "2026-03-17T18:42:31.779Z"
---

                  Exercise 2: Adding Hatch Patterns Between Profiles  

# Exercise 2: Adding Hatch Patterns Between Profiles

In this exercise, you will illustrate the cut and fill regions along an alignment by applying hatch patterns between the surface and layout profiles.

Hatch patterns can be applied to areas that are formed by two profile lines. Hatch patterns are applied in the Profile View Properties dialog box. You can either specify the area type, or use an existing quantity takeoff criteria. You use shape styles to apply the desired hatch patterns and colors to the areas you have defined.

![](../images/GUID-9007515F-CC2D-4C56-AAA0-1FFD8A3962CC.png)

This exercise continues from [Exercise 1: Editing the Profile View Style](GUID-6EA3E98E-4E1F-4C82-ACFC-CA56B3492784.htm "In this exercise, you will learn how to change the data displayed in a profile view.").

Access the profile view properties

1.  Open Profile-5B.dwg, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing is similar to the drawings you used in previous profiles tutorial exercises. This drawing contains an additional profile view, Profile View PV - (3), which contains an existing ground and layout profile. You will add hatch patterns that highlight the cut and fill areas between the two profiles.
    
    ![](../images/GUID-5D75ACA3-8A7D-4359-BDA9-6B37F4CF2DA2.png)
    
2.  Click the Profile View PV - (3) grid to select the profile view. Right-click. Click Profile View Properties.

Define a cut area hatch

1.  In the Profile View Properties dialog box, on the Hatch tab, click ![](../images/GUID-0AF6198D-A104-47CB-A12B-833FB793083C.png)Cut Area.
    
    A Cut - (1) area is displayed in the Hatch Area table. For Upper Boundary, the first surface profile in the list is automatically assigned. For Lower Boundary, the first layout profile in the list is assigned automatically.
    
2.  In the Shape Style column, click **Standard**.
3.  In the Pick Shape Style dialog box, select **Cut**. Click OK.
4.  In the Profile View Properties dialog box, click Apply.
    
    The specified shape style is displayed in the cut areas between the profiles.
    
    ![](../images/GUID-2CEFCD25-981E-47C2-A577-8D93D7DF0D20.png)
    

Define a fill area hatch

1.  In the Profile View Properties dialog box, on the Hatch tab, click ![](../images/GUID-0FF2BDDB-C18B-444B-A088-C834654F71D4.png)Fill Area.
    
    A Fill - (1) area is displayed in the Hatch Area table. For Upper Boundary, the first layout profile in the list is automatically assigned. For Lower Boundary, the first surface profile in the list is assigned automatically.
    
2.  In the Fill - (1) entry, for Shape Style, click the **Standard** entry.
3.  In the Pick Shape Style dialog box, expand the list of shape styles.
    
    An appropriate Fill style does not exist in this drawing. You will create one in the following steps.
    

Create a fill shape style

1.  In the Pick Shape Style dialog box, select **Cut**.
2.  In the Pick Shape Style dialog box, click the down arrow next to ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png). Click ![](../images/GUID-CA1AD018-7C68-4826-8BAB-59D50446DD6F.png)Copy Current Selection.
3.  In the Shape Style dialog box, on the Information tab, for Name, enter **Fill**.
4.  On the Display tab, under View Direction, select Profile.
5.  Select both entries in the Component Display table.
6.  Click one of the Color cells.
7.  In the Select Color dialog box, for Color, enter **92**. Click OK.
8.  In the Component Hatch Display table, for Pattern, click the **Dash** entry.
9.  In the Hatch Pattern dialog box, for Pattern Name, select **Cross**.
10.  Click OK four times.
     
     The Profile View Properties dialog box closes, and the new hatch pattern is displayed in the fill areas between the profiles.
     
     ![](../images/GUID-9007515F-CC2D-4C56-AAA0-1FFD8A3962CC.png)
     

To continue this tutorial, go to [Exercise 3: Projecting Objects onto a Profile View](GUID-188BE199-D837-4B1C-849C-12C0E992370D.htm "In this exercise, you will project multi-view blocks, COGO points, and 3D polylines from plan view onto a profile view.").

**Parent topic:** [Tutorial: Displaying and Modifying Profile Views](GUID-0BF95BEA-BDFF-4B0A-A73C-6749A5FFD1C5.htm "This tutorial demonstrates how to change the appearance of profile views.")