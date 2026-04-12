---
title: "Exercise 5: Setting Up a Linework Code Set"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-14F4D676-EDA8-466F-9C49-2397044026F3.htm"
category: "tutorial_survey_setup"
last_updated: "2026-03-17T18:42:15.328Z"
---

                  Exercise 5: Setting Up a Linework Code Set  

# Exercise 5: Setting Up a Linework Code Set

In this exercise, you will learn how to set up a linework code set to interpret the field codes that the survey field crew enters into a data collector.

When field-coded data is imported into Autodesk Civil 3D, the linework code set interprets the syntax of simple field codes that are contained within survey point descriptions. The linework is connected between similar points. You will use a linework code set to define linework from imported survey data in the [Importing Field-Coded Survey Data](GUID-344B584B-0E65-4FCF-B85F-A62E2ABCD5DE.htm "In this exercise, you will import survey data from an existing field book file that contains linework codes that can be interpreted by a linework code set.") exercise.

This exercise continues from [Exercise 4: Setting Survey Styles](GUID-32C72970-243F-441F-BA12-BBA974AFC29D.htm "In this exercise, you will review the survey network styles and create a figure style.").

Examine the default linework code set

1.  In Windows Explorer, navigate to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC). Open _Survey-1.fbk_ using a text editor.
2.  In Windows Explorer, navigate to the [tutorial folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WSFACF1429558A55DEF27E5F106B5723EEC-5EEC). Open _Survey-X.fbk_ using a text editor.
3.  In the text editor, for _Survey-X.fbk_, highlight the 34th and 35th lines, which contain the following code:
    
    BEGIN TC1
    FC1 VA 105 19.192302 57.714     88.440647 "TC1"
    
4.  In the text editor, for _Survey-1.fbk_, highlight the 24th line, which contains the following code:
    
    FC1 VA 105 19.192302 57.714  88.440647 "TC1 B H0.5 H-0.1 V-0.5"
    
5.  In Toolspace, on the Survey tab, expand the Linework Code Sets collection. Right-click Sample. Click Properties.
    
    The field codes for the Sample linework code set are displayed in the Linework Code Set dialog box. Each property has a user-definable code assigned to it. Compare the codes with the line you selected in the text editor.
    
    The selected lines define the beginning of a top of kerb figure:
    
    *   In _Survey-X.fbk_, the selected lines consist of the feature code (BEGIN TC1), FC1 VA 105 horizontal angle, gradient distance, zenith angle, and description. The point description contains the raw point description (TC1) and XYZ coordinates.
    *   In _Survey-1.fbk_, the selected line contains information that is similar to _Survey-X.fbk_. Notice that horizontal and vertical offset values are also present. If you examine the remainder of the files, you will see that the format used by _Survey-1.fbk_ is simpler and more flexible than _Survey-X.fbk_.
    
    Compare the characters in _Survey-1.fbk_ to the values in the Linework Code Set dialog box. Under Special Codes, notice the codes that are defined for Begin, Horizontal Offset, and Vertical Offset. Each of these codes is displayed in the currently selected line. The current linework code set will interpret this survey point as being the beginning of the survey TC1 figure, with two horizontal offsets and one vertical offset.
    
6.  Close the text editors and Linework Code Set dialog box.

To continue to the next tutorial, go to [Importing Survey Data](GUID-0A5C1B52-4F94-4388-91E6-E9F580EFF18B.htm "This tutorial demonstrates how to import survey data into a drawing, modify the data, and then reprocess the data.").

**Parent topic:** [Tutorial: Survey Setup](GUID-0395EBF8-26C3-4E7B-B98B-81D501BAF73C.htm "This tutorial demonstrates how to access the survey functionality and define and manage the survey settings in Autodesk Civil 3D.")