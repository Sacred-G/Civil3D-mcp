---
title: "Exercise 1: Attaching Drawings as Xrefs for Annotation"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-073E6BDC-DFAB-4D34-B3D1-3FB5CA40E54D.htm"
category: "tutorial_preparing_to_annotate_a_drawing"
last_updated: "2026-03-17T18:43:15.349Z"
---

                   Exercise 1: Attaching Drawings as Xrefs for Annotation  

# Exercise 1: Attaching Drawings as Xrefs for Annotation

In this exercise, you will attach several drawings to one drawing. By attaching drawings, you can annotate multiple large objects in a single compact drawing.

Detailed labeling is usually one of the last steps in the design process. In Autodesk Civil 3D 2025, you do not have to annotate objects in the drawings in which they reside. You can create a single drawing with external references (Xrefs) to the object drawings. This process enables you to keep your annotation drawing size to a minimum, while maintaining the benefit of dynamic label updates.

Attach drawings as Xrefs for annotation

1.  Using Windows Explorer, navigate to the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79). Select all of the drawings that have names beginning with _Labels-_. Click Edit menu ![](../images/ac.menuaro.gif)Copy.
2.  Navigate to the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Click Home tab ![](../images/ac.menuaro.gif)Clipboard panel ![](../images/ac.menuaro.gif)Paste drop-down ![](../images/ac.menuaro.gif)Paste ![](../images/GUID-B8235700-1DA3-4CB2-A1B6-C3F9B0D18D9A.png) Find.
    
    In a later exercise, you will modify some of the externally referenced drawings. To make the modifications, the drawings must be in a location to which you have write access.
    
3.  Open _Labels-1a.dwg_, which you saved in the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832) in step 2.
4.  In Toolspace, on the Prospector tab, expand the _Labels-1a_ drawing.
    
    Although a surface with an outside boundary is displayed in the drawing window, notice that the Surfaces collection is empty. The Surfaces collection is empty because the composite surface is an external reference, or Xref. The surface data, which is quite large, exists in a separate drawing.
    
    Tip:
    
    In large projects that have multiple surfaces, each surface object should reside in its own drawing. For example, the existing ground surface should exist in one drawing, with other drawings containing data references to it. For example, the proposed ground surface should exist in its own drawing, with a data reference to the existing ground surface.
    
5.  On the command line, enter **XREF**.
    
    In the External References dialog box, notice the difference in size between the current drawing (_Labels-1a_) and the externally referenced drawing that contains the surface (_Labels-Surface_). While the surface object appears in the current drawing, the size of the current drawing is a fraction of the surface drawing. Using Xrefs, you can use data from another drawing without actually inserting the data.
    
6.  In the External References palette, click the arrow next to ![](../images/GUID-EB705075-6FE3-4070-9E53-2707BFA5957B.png). Click Attach DWG.
    
    Note:
    
    Notice that you can also attach a drawing from Vault.
    
7.  In the Select Reference File dialog box, navigate to the [My Civil 3D Tutorial Data folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS1A9193826455F5FF1848157110156D8271-7832). Select the drawings _Labels\_Alignments.dwg_ and _Labels\_Plots.dwg_. Click Open.
8.  In the External Reference dialog box, make sure that the following settings are selected:
    *   Reference Type: Attachment
        
        This setting specifies that the Xrefs remain with the current host drawing (_Labels-1a.dwg_) if the current drawing is attached as an Xref to another drawing. If Overlay is selected, Xrefs that exist in the drawing are ignored if the drawing is attached to another drawing as an Xref.
        
    *   Path Type: Relative Path
        
        This setting specifies that if you move the current and referenced drawings to another location, the references are less likely to be lost.
        
        Tip:
        
        Before creating an Xref, make sure that the referenced drawings are saved in the same directory as the current drawing.
        
        Important:
        
        Attached Xrefs must have the following settings. If the insertion point, scale, or rotation is different from the current drawing, the Autodesk Civil 3D labels will not work.
        
    *   Insertion Point, Specify On-Screen: Cleared
    *   Scale, Specify On-Screen: Cleared
    *   Scale, Uniform Scale: Selected
    *   Rotation: Specify On-Screen: Cleared
9.  Click OK.
    
    The alignments and plots appear in the drawing window, and the drawings in which they reside are listed in the External References palette. Close the External References palette.
    
    Note: When you attach the Xrefs, a message may be displayed at the status bar indicating that there are unreconciled layers. For more information about reconciling layers, see [About Receiving Notification of New Layers](https://beehive.autodesk.com/community/service/rest/cloudhelp/resource/cloudhelpchannel/guidcrossbook/?v=2025&p=CIV3D&l=ENG&accessmode=live&guid=GUID-A6A7D6F4-5AAB-431D-AA3C-77B663AAD9D9).
    
    ![](../images/GUID-5C231502-B297-423B-8E71-161F88AF35C1.png)
    
    Drawing with multiple externally referenced drawings
    

To continue this tutorial, go to [Exercise 2: Exploring the Annotation Tools on the Ribbon](GUID-EB5E85B1-41F6-4336-B0BE-3EB668B2644C.htm "In this exercise, you will learn how to locate annotation tools on the ribbon tabs.").

**Parent topic:** [Tutorial: Preparing to Annotate a Drawing](GUID-7ACEFC62-E6E9-4A5F-B34A-597118F0646B.htm "This tutorial demonstrates how to perform some optional tasks that can make annotating your drawing easier.")