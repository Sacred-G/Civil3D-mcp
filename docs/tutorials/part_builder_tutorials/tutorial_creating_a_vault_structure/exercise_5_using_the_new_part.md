---
title: "Exercise 5: Using the New Part"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-08AA6D0B-9326-4E2A-AE69-2C9D895F1BEC.htm"
category: "tutorial_creating_a_vault_structure"
last_updated: "2026-03-17T18:43:13.894Z"
---

                Exercise 5: Using the New Part  

# Exercise 5: Using the New Part

In this exercise, you will insert the new vault part into a Autodesk Civil 3D pipe network, and investigate how it behaves as a pipe network structure.

This exercise continues from [Exercise 4: Finalizing the Part](GUID-F1C850F2-E145-4C0E-BEFE-8F713F56EECC.htm "In this exercise, you will add the final model and size parameters that will allow the part geometry to be modified in Autodesk Civil 3D.").

1.  Make sure that you have closed the Part Builder environment from the previous exercise.
2.  In the Autodesk Civil 3D window, click Quick Access toolbar ![](../images/ac.menuaro.gif)![](../images/GUID-2E4799E9-518E-4844-A055-98090D9A377E.png)Open. Navigate to the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79). Open the drawing _Part Builder-3a.dwg_. This drawing contains a simple sanitary sewer pipe network.
3.  In the Toolspace Settings tab, expand Pipe Networks![](../images/ac.menuaro.gif)Parts Lists, and then right-click Sanitary Sewer and then click Edit.
4.  Click the Structures tab. Right-click Sanitary Sewer and then click Add Part Family.
5.  Check the box next to Vault 5106-LA with Top 5106-TL3-332. Click OK. The part family is added to the parts list.
6.  Right-click Vault 5106-LA with Top 5106-TL3332, and then click Add Part Size. Note the new part size that has been included.
7.  Click OK to add a single part size with the default values. Expand Vault 5106-LA with Top 5106-TL3-332. Note the new part size that has been included.
8.  Click OK. Click the sanitary sewer manhole in any of the drawing views. Right-click and then click Swap Part.
    
    ![](../images/GUID-A6E2089E-55A4-4B38-8B76-148CFFB9D37E.png)
    
9.  Expand Vault 5106-LA with Top 5106-TL3-332, and select the part beneath it. The part now is displayed as the vault in 3D view, but still is displayed as a manhole in plan view.
    
    ![](../images/GUID-498D6AC0-F51F-4C8C-94BA-2CBCDE038F76.png)
    
10.  Click the structure in any of the views. Right-click and then click Structure Properties.
11.  Click the Information tab. Click the black triangle next to the edit button, and select Create New.
12.  Click the Information tab. Enter Model for Name.
13.  Click the Profile tab. Click Display as solid.
14.  Click the Display tab. Set the visibility for Model 3D Solid to On.
15.  Click OK twice. Note the change to the appearance of the structure in plan view. The vault structure now is displayed with its actual dimensions.
     
     ![](../images/GUID-9D3D9D5A-9340-4176-91B2-726691997DB2.png)
     
16.  Click the vault structure. Right-click and select Structure Properties. Click the Part Properties tab.
17.  Change the following:
     *   Frame Length = 108
     *   Frame Width = 48
     *   Inner Structure Length = 120
     *   Inner Structure Width = 60
18.  Click OK. Note the change to the structure in the drawing. The structure has been updated with the new dimension properties.
     
     ![](../images/GUID-E2452DB4-2F17-4528-AA8E-D00D28CE1D4C.png)
     
19.  Save and close the drawing.

**Parent topic:** [Tutorial: Creating a Vault Structure](GUID-5417170D-9D08-4B57-83A6-FE84173720FA.htm "This tutorial demonstrates how to use Part Builder to create a vault structure. It will go through the steps to define the new part in the structure catalog, define the manhole geometry, create profiles, and then establish parameters to control the sizing and dimensions of the vault.")