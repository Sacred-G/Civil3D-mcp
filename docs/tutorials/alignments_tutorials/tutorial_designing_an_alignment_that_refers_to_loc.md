---
title: "Tutorial: Designing an Alignment that Refers to Local Standards"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-CAEC3077-D78A-4F42-8E47-41FABAB6915D.htm"
category: "alignments_tutorials"
last_updated: "2026-03-17T18:42:27.231Z"
---

                  Tutorial: Designing an Alignment that Refers to Local Standards  

# Tutorial: Designing an Alignment that Refers to Local Standards

This tutorial demonstrates how to validate that your alignment design meets criteria specified by a local agency.

To create an alignment using design criteria, you use the same basic workflow that you use to create an alignment without design criteria. During alignment creation, you can select a design criteria file, from which you can specify the superelevation attainment method and minimum radius and transition (transition) length tables. Design criteria files that contain DMRB design standards are included with Autodesk Civil 3D. If your local design standards differ from the DMRB standards, you can create a custom design criteria file using the Design Criteria Editor dialog box.

Some alignment design criteria is not available in table form in the design criteria file. For these criteria, you can define design checks to validate design standards. To apply a design check to an alignment, you must add it to a design check set.

If the design parameters for a sub-element violate a design check or the minimum values established in the design criteria file, a warning symbol appears on the sub-element in the drawing window, and next to the violated value in the Alignment Elements vista and Alignment Layout Parameters dialog box. When you hover the cursor over a warning symbol, a tooltip displays the standard that has been violated. The display of the warning symbol is controlled by the alignment style.

Note:

You can also use the Design Criteria tab on the Alignment Properties dialog box to apply design criteria to an alignment after it has been created.

**Topics in this section**

*   [Exercise 1: Drawing an Alignment that Refers to Design Criteria](GUID-A02CA33C-F6C2-44F1-88DC-FF87C83F032B.htm)  
    In this exercise, you will use the criteria-based design tools to create an alignment that complies with specified standards.
*   [Exercise 2: Viewing and Correcting Alignment Design Criteria Violations](GUID-DF1B50F0-18B5-46D0-AA50-18867D236EAE.htm)  
    In this exercise, you will examine alignment design criteria violations, and then learn how to correct a criteria violation.
*   [Exercise 3: Working with Design Checks](GUID-18AF0EFE-37DA-4B04-B663-7BC6B4A5CCF3.htm)  
    In this exercise, you will create an alignment design check, add the design check to a design check set, and then apply the design check set to an alignment.
*   [Exercise 4: Modifying a Design Criteria File](GUID-E781CDDC-6D6C-4DEB-AF87-74A758671FAF.htm)  
    In this exercise, you will add a radius and speed table to the design criteria file.

**Parent topic:** [Alignments Tutorials](GUID-D625ABC3-2224-425A-BF5C-971439403C30.htm)