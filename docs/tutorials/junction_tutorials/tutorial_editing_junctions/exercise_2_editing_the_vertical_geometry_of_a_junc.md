---
title: "Exercise 2: Editing the Vertical Geometry of a Junction"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-D156D6A4-DFFA-4CF3-BEC7-6BD112DF7DFB.htm"
category: "tutorial_editing_junctions"
last_updated: "2026-03-17T18:42:56.427Z"
---

                  Exercise 2: Editing the Vertical Geometry of a Junction  

# Exercise 2: Editing the Vertical Geometry of a Junction

In this exercise, you will edit the profiles that define the vertical geometry of a junction object. You will edit the profiles graphically and parametrically, and examine how the changes affect the junction.

Examine locked VIPs

1.  Open _Intersection-Edit-Vertical.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
    
    This drawing contains a junction of a primary road (Road A) and a side road (Road C).
    
2.  In the lower right viewport, select the layout profile.
    
    Notice that ![](../images/GUID-F77DF8D4-1EA5-4FD4-87E7-346BECB16A87.png) lock icons are displayed on three of the VIPs. The lock icons indicate that the VIPs are locked to another profile. When the junction was created, the middle VIP was created at the point where the side road intersects with the primary road profile. The other two VIPs were created to maintain the primary road crown through the junction, and are locked to the edges of the primary road.
    
    ![](../images/GUID-89219B18-57CB-4592-A645-3CF93FF7E595.png)
    
3.  Click Profile tab ![](../images/ac.menuaro.gif)Modify Profile panel ![](../images/ac.menuaro.gif)Geometry Editor ![](../images/GUID-B5C4BD75-51B3-43CA-942D-5DEDE83E44D8.png) Find.
4.  On the Profile Layout Tools toolbar, click ![](../images/GUID-09A2EF83-2E3D-4539-81EA-972C2C50F717.png).
    
    In the Profile Elements vista, notice that a ![](../images/GUID-1956A4B0-40CF-4B47-B301-4B35711AF2F8.png) is displayed in the Lock column for VIPs 5 through 7.
    
5.  Hover the cursor over the ![](../images/GUID-1956A4B0-40CF-4B47-B301-4B35711AF2F8.png) icon for VIP 6.
    
    Information about the locked VIP, including alignment, profile, and junction, is displayed in a tooltip. VIPs that are created as part of the junction creation process are dynamically linked to the primary road profile.
    
    Note:
    
    You can unlock a VIP by clicking the icon. If a VIP is unlocked, the profile will no longer react to changes in either the junction or primary road profile.
    
6.  Click the ![](../images/GUID-E6A92700-C81C-4DF5-B87C-886B480AF68B.png) icon for VIP 8.
    
    The VIP is locked at the current chainage and level. Notice that another ![](../images/GUID-F77DF8D4-1EA5-4FD4-87E7-346BECB16A87.png) icon is displayed on the profile, and the VIP Chainage and VIP Level values are no longer available. A VIP can be manually locked to a specified chainage and level value. Manually locked VIPs are not affected by modifications to other portions of the profile.
    
    ![](../images/GUID-A178925A-01BF-41DF-B2E9-B98EFF9B96BF.png)
    
7.  Close the Profile Layout Tools toolbar.
8.  In the left viewport, select the junction marker.
    
    On the ribbon, the Junction tab is displayed. Tools for adjusting the side road profile are displayed on the Modify panel. You can edit the primary road profile with the standard profile editing tools.
    

Modify the side road gradient

1.  Click Junction tab ![](../images/ac.menuaro.gif)Modify panel ![](../images/ac.menuaro.gif)Side Road Profile ![](../images/GUID-2E50CF7A-8992-4966-9DDD-4A507CDDF439.png) Find.
    
    The Side Road Profile Rules dialog box is displayed. Use this dialog box to specify the side road gradient entering and exiting the junction.
    
2.  In the Side Road Profile Rules dialog box, specify the following parameters:
    
    Note:
    
    Enter the parameters in the following order.
    
    *   Apply Gradient Rules: **Yes**
    *   Distance Rule To Adjust The Gradient: **Specify Distance**
        
        This option enables you to specify a distance from the junction of the primary and side road alignments. This enables you to extend the side road gradient rules outside the extents of the junction.
        
    *   Distance Value: **100.000m**
    *   Maximum Gradient Change: **2.00%**
    
    In the lower right viewport, a new VIP is created 100 meters to the left of the locked VIPs. The gradient entering the junction is 0.21%, which is exactly 2.00% less than the primary road gradient.
    
    ![](../images/GUID-F1B7EEA4-8664-415E-872D-E4350E8383E7.png)
    
3.  In the lower right viewport, select the layout profile.
    
    You can move the grip at the VIP to make minor changes to the profile. If you drag the grip outside the range of parameters specified in the profile gradient rules, the grip snaps back to the default position that satisfies the gradient rules.
    
4.  Close the Side Road Profile Rules dialog box.
5.  Press Esc.

Add a low point to a radius kerb profile

1.  In the upper right viewport, zoom in to the Junction - 2 - (SE) profile view.
2.  Select the profile.
    
    The ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) grips indicate the extents of the radius kerb profile. The profile portions that are outside the extents represent the offset profiles. Changes to the offset profiles affect the radius kerb profiles, but changes to the radius kerb profile do not affect the offset profiles. Use the ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) grips to extend the radius kerb profile along either offset profile.
    
    ![](../images/GUID-07C46B65-B33D-486A-9B63-5E06240D5316.png)
    
3.  Click Profile tab ![](../images/ac.menuaro.gif)Modify Profile panel ![](../images/ac.menuaro.gif)Geometry Editor ![](../images/GUID-B5C4BD75-51B3-43CA-942D-5DEDE83E44D8.png) Find.
4.  On the Profile Layout Tools toolbar, click ![](../images/GUID-6C8D7FCF-6768-4BBB-BFCC-1E6C9DF18182.png)Insert VIP.
5.  Click between the two ![](../images/GUID-1D6BACBC-9188-4999-9604-EFA85DC87E86.png) grips to place a VIP, creating a low point on the radius kerb.
    
    ![](../images/GUID-643FB7BF-A1D8-4E18-85FC-9BBCDF645B3C.png)
    
    A low point facilitates drainage along a radius kerb. In the following procedures, you will see how the radius kerb reacts to changes in other objects.
    
6.  Close the Profile Layout Tools toolbar.

Move the primary road alignment

1.  In the left viewport, select the Road A alignment.
2.  Select the grip at the southern end of the Road A alignment. Drag the grip to the left. Click to place the grip.
    
    ![](../images/GUID-A080A515-56FD-47F8-9D98-BBB59E3A6208.png)
    
    In the bottom right viewport, notice that the three dynamically locked VIPs moved to a new location. This happened because you moved the alignment to which they are locked.
    
    In the top right viewport, examine how the changes to the junction location affect the radius kerb profile that you modified.
    

Change the primary road profile level

1.  In the top viewport, pan to the Road A profile view.
2.  In the Road A Profile view, select the layout profile.
3.  Select the second ![](../images/GUID-3BE3120F-533A-4D8C-B4E6-722F942512B4.png) PI grip from the left. Drag the grip up. Click to place the grip.
    
    ![](../images/GUID-94822A27-F62B-4AF4-BFE6-CFE060F141B4.png)
    
    In the bottom viewport, notice that the three locked VIPs moved up to accommodate the new primary road level.
    
    In the top right viewport, the VIP you added to the southeast radius kerb has stayed in the location you specified, but the ends of the profile moved up to accommodate the new level of the offset profiles. The ends of the radius kerb profile are locked to the offset profiles. You must manually update VIPs that have been placed within the profile.
    
    ![](../images/GUID-B91ECAA6-260C-4C74-B41D-5A6E7051B8FE.png)
    

To continue this tutorial, go to [Exercise 3: Creating and Editing a Corridor in the Junction Area](GUID-1A20CA5A-8087-4CAB-B71C-08121042BED8.htm "In this exercise, you will create a corridor using existing vertical and horizontal geometry. You will modify the corridor in the junction area, and then experiment with the corridor region recreation tools.").

**Parent topic:** [Tutorial: Editing Junctions](GUID-74F6ED8E-61DD-496E-8F08-C2FE11655355.htm "This tutorial demonstrates how to modify an existing junction object.")