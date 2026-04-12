---
title: "Labels and Tables Tutorials"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-BDF3F02E-838E-443D-BFE3-3033F03E214F.htm"
category: "tutorials"
last_updated: "2026-03-17T18:43:14.557Z"
---

                   Labels and Tables Tutorials  

# Labels and Tables Tutorials

These tutorials will get you started creating and editing labels, label styles, and tables.

In Autodesk Civil 3D 2025, you can annotate objects that exist in externally referenced drawings. This keeps annotations separate from the design data. This greatly reduces the file size of the annotation drawing, and allows greater flexibility in managing design data.

_Object labels_ in Autodesk Civil 3D include types that are placed automatically and others that you can place manually at points of interest. Sometimes, automatic labels overlap other objects and must be moved. Also, you may want to make some of them look different from others.

Most objects in Autodesk Civil 3D have two types of labels. The first type is automatically created when the object is created, as defined by the object properties. The second type is a range of object labels that are manually applied as needed. For both types of labels, you can edit the label style and make changes to all labels that use that style. You can also modify individual labels by moving them manually in the drawing. When labels are moved, they assume their dragged state, which can use a different display format. Some labels can be converted to small tags that occupy less space in the drawing, and then the data for each tagged object can be displayed in a table.

Labels are distinct objects that are independent of the parent object that they annotate. Labels are dynamically linked to their parent object and automatically update when the parent object changes. However, labels reside on a separate layer and are not selected when you select the parent object.

Note:

Point, plot area, corridor, and surface watershed labels are not object type labels. They are sub-elements of a parent object and their properties are managed in the Label Properties dialog box.

You can move most labels by simply selecting and dragging them. When you move a label, a leader is automatically created, which points back to the precise point annotated by the label. Also, you can flip labels along a linear object, such as a plot segment, to the other side of the line.

The easiest way to change the format of a label is by changing its style. If a suitable style is not available, you can copy an existing style that is similar to what you want, make the required changes, and save it as a new style. You can also change the attributes of the existing style. This action requires some forethought, because your changes affect all objects in the drawing that use the style.

Label visibility can be controlled in several ways. First, label visibility is dependent on the parent object. When the layer of the parent object is either turned off or frozen, its labels are also turned off or frozen. The label style can also control its visibility. The visibility of the individual label components, or the entire label object, can be turned on or off in the style. An individual label can also be selected and turned on or off using the Properties palette.

**Topics in this section**

*   [Tutorial: Preparing to Annotate a Drawing](GUID-7ACEFC62-E6E9-4A5F-B34A-597118F0646B.htm)  
    This tutorial demonstrates how to perform some optional tasks that can make annotating your drawing easier.
*   [Tutorial: Adding and Editing Labels](GUID-38C5B56B-B2A1-49EB-8BD6-1BB1715EEB54.htm)  
    This tutorial demonstrates how to add labels to Autodesk Civil 3D objects, and then edit the labels to suit your requirements.
*   [Tutorial: Changing the Content of a Label](GUID-EB73BA6C-81CD-4FD1-B6BD-40C2FA765EF8.htm)  
    This tutorial demonstrates how to change label text content for an individual label and for a group of labels.
*   [Tutorial: Working with Tables and Tags](GUID-7052010C-3307-4A41-AFDB-39763F830C6B.htm)  
    This tutorial demonstrates how to place object data into tables.
*   [Tutorial: Working with Label Styles](GUID-F663DB98-120E-4A8D-9762-CB799972916A.htm)  
    This tutorial demonstrates how to define the behavior, appearance, and content of labels using label styles.
*   [Tutorial: Using Expressions in Labels](GUID-ADE0D8A5-CB73-40C2-B7FF-10E25FEE55F1.htm)  
    This tutorial demonstrates how to use expressions, which are mathematical formulas that modify a property within a label style.