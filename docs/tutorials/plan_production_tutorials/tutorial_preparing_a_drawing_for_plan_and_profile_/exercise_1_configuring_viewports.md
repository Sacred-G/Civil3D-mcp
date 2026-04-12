---
title: "Exercise 1: Configuring Viewports"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-C4FD5858-DF1B-4738-99AF-76858DD7FB81.htm"
category: "tutorial_preparing_a_drawing_for_plan_and_profile_"
last_updated: "2026-03-17T18:43:22.833Z"
---

                  Exercise 1: Configuring Viewports  

# Exercise 1: Configuring Viewports

In this exercise, you will learn how to prepare an existing drawing template for use with the plan production tools.

Before using your own custom templates for plan production, you must set the layout viewport type to either _Plan_ or _Profile_.

By default, most of the templates provided with Autodesk Civil 3D have the Viewport Type property set to Undefined. However, the plan production templates that are included have viewports that are already configured to the appropriate viewport type: plan or profile.

Examine viewport properties in an existing template

1.  Click Quick Access toolbar ![](../images/ac.menuaro.gif)![](../images/GUID-2E4799E9-518E-4844-A055-98090D9A377E.png)Open. Navigate to the [local Template\\Plan Production folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24226). Open _Civil_ _3D__(Imperial) Plan and Profile.dwt_.
    
    Note:
    
    In the Select File dialog box, ensure that Files of Type is set to _Drawing Template (\*.dwt)_.
    
    The template contains two viewports.
    
2.  Select the top viewport. Right-click. Click Properties.
    
    Examine the settings of the viewport, including the size and position contained in the Geometry category.
    
3.  On the Properties palette, scroll down to the Viewport category.
4.  Double-click the cell next to Viewport Type.
    
    The drop-down list displays the configuration options for the viewport. The current (top) viewport is set to Plan, while the bottom viewport is set to Profile. These settings specify that when you use this template in the plan production process, the plan view of the alignment will appear in the top viewport, and the corresponding profile will appear in the bottom viewport.
    
    By default, all templates that are not contained in the [local Template\\Plan Production folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF-7F10494411F03A24F24226) have their Viewport Type set to Undefined. To configure your custom templates for use with the plan production process, you must change the viewport setting as appropriate.
    
5.  Close _Civil_ _3D__(Imperial) Plan and Profile.dwt_, but do not save it.

To continue this tutorial, go to [Exercise 2: Creating View Frames](GUID-E8788617-0B22-45D9-9930-46C2A7CBC720.htm "In this exercise, you will use the Create View Frames wizard to quickly create view frames along an alignment.").

**Parent topic:** [Tutorial: Preparing a Drawing for Plan and Profile Sheet Layout](GUID-63C13E78-B7D5-4FB2-BEAA-BE363624225E.htm "This tutorial demonstrates how to set up a drawing before you publish plan and profile sheets.")