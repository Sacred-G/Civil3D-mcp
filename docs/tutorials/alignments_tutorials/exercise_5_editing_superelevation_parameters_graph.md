---
title: "Exercise 5: Editing Superelevation Parameters Graphically"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-F2C697A8-A861-45C5-87BE-B65864460B18.htm"
category: "tutorial_applying_superelevation_to_an_alignment"
last_updated: "2026-03-17T18:42:28.240Z"
---

                Exercise 5: Editing Superelevation Parameters Graphically  

# Exercise 5: Editing Superelevation Parameters Graphically

In this exercise, you will use grips in a superelevation view to modify the superelevation crossfalls and critical chainage values.

This exercise continues from [Exercise 4: Adding and Modifying Superelevation Chainages](GUID-53B257E3-D572-407F-8582-96B731644DEE.htm "In this exercise, you will resolve overlap between two superelevated curves by adding and removing critical chainages, and then editing existing superelevation data.").

Examine the grips

1.  Open _Align-Superelevation-5.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  In the Curve.4 portion of the superelevation view, click any of the red or blue lines.
    
    A series of grips appears at each of the superelevation critical chainages along each line, as well as on the superelevation view. You will learn the purpose of the grips later in this exercise.
    
3.  Press Esc.

Move a critical chainage graphically

1.  Near chainage 0+900.00, Ctrl+click the green Shoulder text.
    
    Only the grips of the selected superelevation region are displayed.
    
2.  Hover the cursor over the ![](../images/GUID-F133DFFD-6852-4D0B-A516-3E9C18CFB19C.png) grip at chainage 0+908.44.
    
    The cursor snaps to the grip, and a menu of options is displayed. You can use this grip to either change the chainage value, or add or remove a critical chainage.
    
3.  Select Move End Normal Shoulder.
4.  Move the grip to chainage 0+900.00. Click to place the grip.
    
    The transition in region is updated.
    
    Note:
    
    The new chainage value is also shown in the Superelevation Tabular Editor and Superelevation Curve Manager.
    
5.  Press Esc.

Remove or apply curve smoothing

1.  Pan to the transition out region of Curve.4.
2.  Near chainage 1+147.69, Ctrl+click the light red curve.
3.  Hover the cursor over the ![](../images/GUID-E07760FC-F2D1-4B7E-BFE0-8F4C037E02CF.png) grip at chainage 1+148.59.
    
    The cursor snaps to the grip, and a menu of options is displayed. You can use this grip to change the crossfall at this critical chainage, remove the gradient break, or remove curve smoothing.
    
4.  Select Remove Curve Smoothing.
    
    The curve is removed from the gradient break.
    
    Note:
    
    You may use the same process to add curve smoothing to a gradient break.
    
5.  Press Esc.

Change the crossfall at a superelevation critical chainage

1.  At chainage 1+148.59, hover the cursor over the ![](../images/GUID-E07760FC-F2D1-4B7E-BFE0-8F4C037E02CF.png) gradient grip.
    
    When a gradient grip is cyan, a gradient break exists at the current location. Like other cyan grips in a superelevation view, a menu of options is displayed.
    
2.  At chainage 1+116.45, hover the cursor over the ![](../images/GUID-6387814E-0C04-4402-B54C-1E9546F0B066.png) grip.
    
    When a gradient grip is gray, the current location has a consistent gradient. Because no gradient break is present, the only option is to change the crossfall.
    
3.  Click the grip to make it active.
4.  Enter 1.5.
    
    Notice that the grip color is now cyan. This happened because you created a gradient break at the current location.
    

Change the crossfall between superelevation curves

1.  Pan to the left until you see the ![](../images/GUID-BE754D01-DF84-40C9-A144-ADF315F2E0EB.png) grip near chainage 1+000.00.
2.  Hover the cursor over the ![](../images/GUID-BE754D01-DF84-40C9-A144-ADF315F2E0EB.png) grip.
    
    The cursor snaps to the grip, and a menu of options is displayed. You can use this grip to change the crossfall of either one or both of the shoulders or lanes between the previous and next superelevation critical chainages. This grip is available in sections where lanes or shoulders are fully superelevated, as well as on straights that are between curves.
    
3.  Select Move Both Left and Right.
4.  Enter 4.
    
    The crossfalls of both the left and right shoulders are updated.
    

**Parent topic:** [Tutorial: Applying Superelevation to an Alignment](GUID-AA0068E0-2858-4067-9104-161112DEDBF6.htm "In this tutorial, you will calculate superelevation for alignment curves, create a superelevation view to display the superelevation data, and edit the superelevation data both graphically and in a tabular format.")