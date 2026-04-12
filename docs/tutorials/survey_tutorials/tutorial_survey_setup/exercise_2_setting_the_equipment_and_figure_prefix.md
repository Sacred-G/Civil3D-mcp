---
title: "Exercise 2: Setting the Equipment and Figure Prefix Databases"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-5DC983D4-24E2-4F65-A531-948687401B72.htm"
category: "tutorial_survey_setup"
last_updated: "2026-03-17T18:42:15.165Z"
---

                  Exercise 2: Setting the Equipment and Figure Prefix Databases  

# Exercise 2: Setting the Equipment and Figure Prefix Databases

In this exercise, you will create new survey equipment and figure prefix databases and definitions.

_Equipment definitions_ specify the values associated with a specific surveying instrument, such as the standard deviations associated with the measuring capabilities of the instrument.

_Figure prefixes_ specify the layer that a figure is drawn on, how a figure is stylized, and whether figures are created as breaklines and plot lines. When figures are created, they are matched based on their names and the prefix names. All figures that match a specific prefix are assigned the properties of the prefix.

This exercise continues from [Exercise 1: Creating a Survey Database](GUID-07C021D3-5FBE-496B-9C0F-906AC0779664.htm "In this exercise, you will open the Survey tab in Toolspace, create a local survey database, and then open a drawing to display the survey data.").

Create an equipment database

Note:

This exercise uses _Survey-1.dwg_ with the modifications you made in the previous exercise.

2.  In Toolspace, on the Survey tab, right-click the Equipment Databases collection. Click New.
3.  In the New Equipment Database dialog box, enter **Survey 1** for the new equipment database and click OK.

Create an equipment definition

1.  Right-click the **Survey 1** equipment database. Click Manage Equipment Database.
2.  In the Equipment Database Manager - Survey 1 dialog box, under the Miscellaneous property, for Name, enter **Survey 1**.
    
    Note:
    
    The Standard Deviations settings determine the accuracy of the survey observations based on the equipment that measured them.
    
3.  Click OK.
4.  In Toolspace, on the Survey tab, right-click the **Survey 1** equipment definition. Click Make Current.
    
    In Toolspace, the current equipment database name is displayed in bold text.
    

Create a figure prefix database

1.  In Toolspace, on the Survey tab, right-click the Figure Prefix Databases collection. Click New.
2.  In the New Figure Prefix Database dialog box, enter **Survey 1** for the new figure prefix database and click OK.

Create a figure prefix definition

1.  In Toolspace, on the Survey tab, expand the Figure Prefix Databases collection. Right-click the figure prefix database **Survey 1**. Click Manage Figure Prefix Database.
2.  In the Figure Prefix Database Manager dialog box, click ![](../images/GUID-7633B109-B0E2-42D8-8A07-E27BBF28B731.png).
3.  Specify the following parameters:
    *   Name: **LOT**
    *   Plot Line: **Selected**
    *   Site: **Survey Site**
        
        All figures that match the LOT prefix name will have the Plot Line setting set to Yes. When the figure is inserted into the drawing, Autodesk Civil 3D will create plot lines in the drawing in the Survey Site.
        
4.  Click OK.
5.  In Toolspace, on the Survey tab, right-click the **Survey 1** figure prefix database. Click Make Current.

To continue this tutorial, go to [Exercise 3: Adjusting and Verifying Settings](GUID-3B8ADD98-A14E-4B4A-9FCC-9C946FE84192.htm "In this exercise, you will view and adjust several types of survey settings.").

**Parent topic:** [Tutorial: Survey Setup](GUID-0395EBF8-26C3-4E7B-B98B-81D501BAF73C.htm "This tutorial demonstrates how to access the survey functionality and define and manage the survey settings in Autodesk Civil 3D.")