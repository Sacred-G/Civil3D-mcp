---
title: "Exercise 3: Controlling Label Appearance Using Layers"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-DD1F2293-A6D5-4449-B21D-D13FF731EDB3.htm"
category: "tutorial_working_with_label_styles"
last_updated: "2026-03-17T18:43:19.936Z"
---

                   Exercise 3: Controlling Label Appearance Using Layers  

# Exercise 3: Controlling Label Appearance Using Layers

In this exercise, you will use layers to change the color and visibility of labels.

The components that make up a label object get their color, line weight, and line type property settings from either the label style or the layer to which the label style refers. When a label style refers to a specific layer, any label style components that are set to either ByLayer or ByBlock inherit the properties of that specific layer. However, if the label style refers to layer 0, then any label style components that are set to either ByLayer or ByBlock inherit their properties from the layer on which the label resides.

A label is an independent Autodesk Civil 3D object that can be on a separate layer from its parent object. However, the visibility of a label is linked to the layer of the parent object. Turning off or freezing the layer of an object also hides the labels of that object, even if they reside on a different layer.

This exercise continues from [Exercise 2: Using a Child Label Style](GUID-C49FC84B-69EC-455A-A563-11C3B8DF3B29.htm "In this exercise, you will create a child label style that derives its default settings from an existing label style, or parent.").

Examine how parent object layer state affects label visibility

Note:

This exercise uses _Labels-5a.dwg_ with the modifications you made in the previous exercise, or you can open _Labels-5b.dwg_ from the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).

2.  On the West Street alignment, zoom and pan to the area between stations 0+100 and 0+120.
3.  Click chainage label **0+120** to select all major chainage labels. Right-click. Click Properties.
    
    On the Properties palette, notice that the alignment chainage labels are on layer C-ROAD-TEXT. If you look at the properties for the alignment itself, you will see that it is on the C-ROAD layer.
    
4.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer list. Next to the **C-ROAD** layer, click ![](../images/GUID-6FD80C03-88AB-44CD-B810-0480DFACC30F.png) to turn off the C-ROAD layer.
5.  On the command line, enter **REGEN**.
    
    The alignment and its labels are hidden. This happened because, while labels are independent objects on a separate layer, their visibility is linked to the layer of the parent object, C-ROAD. Turning off the layer of an object also hides the labels of that object, even if they reside on a different layer.
    
    Note:
    
    The blue line that remains in place of the alignment is the polyline in the externally referenced drawing, from which you created the alignment in the [Adding Labels In Groups](GUID-9A8CBBE8-FAE7-461F-B4B5-C35181213F4A.htm "In this exercise, you will use label sets to apply several types of labels to an alignment.") exercise.
    
6.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer list. Next to the **C-ROAD** layer, click ![](../images/GUID-8E5A9E6C-CDB6-46BA-985A-6F9CCDCE98E8.png) to turn on the C-ROAD layer and the chainage labels.
7.  On the command line, enter **REGEN**.

Create a label style that is not affected by the parent object layer

1.  Select label **0+120** to select all major chainage labels. On the Properties palette, under Labeling, click the field next to Major Chainage Label Style. Click Create/Edit.
2.  In the Major Chainage Label Style dialog box, click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection.
    
    In the Label Style Composer dialog box, on the General tab, in the Label category, notice that the Layer is 0. If a label style is set to layer 0, any of its color, line type, and line weight properties that are set to ByLayer or ByBlock will inherit the corresponding properties of the layer on which the label resides.
    
3.  Click the Layout tab.
    
    In the Text category, notice that the Color property for the Chainage component is ByLayer. This means that the major chainage label text inherits the color of the layer to which the style refers. Because the label style refers to layer 0, the major chainage label text inherits the color of the label object layer, which is C-ROAD\_TEXT.
    
4.  In the Color row, click the Value cell. Click ![](../images/GUID-4453B981-AEBA-41F4-A9AA-AE989CFE1B5F.png) and change the Chainage component color to blue.
    
    Notice that the text color updates in the Preview pane.
    
    Note:
    
    For greater control of color, lineweight, and linetype, keep all style components set to either ByBlock or ByLayer, and the label style layer set to 0. Then, you can use the layers to modify these properties. This tutorial uses a specific color setting to demonstrate how layer settings affect styles.
    
5.  In the Component Name list, select **Tick**.
    
    In the Tick category, notice that the Color property for the Tickcomponent is ByLayer.
    
6.  Click OK to apply the change in color to the Major Chainage component and exit the Label Style Composer and Major Chainage Label Style dialog boxes.
7.  Press Esc to deselect the labels.
8.  On the command line, enter **REGEN**.
    
    Notice that while the ticks still inherit the red color from the C-ROAD-TEXT layer, the text is blue.
    
    ![](../images/GUID-F1298B21-B603-4E16-B92A-DDCF590159DD.png)
    
    Label style that uses color that differs from referenced layer
    

Add another label

1.  Click Annotate tab ![](../images/ac.menuaro.gif)Labels & Tables panel ![](../images/ac.menuaro.gif)Add Labels menu ![](../images/ac.menuaro.gif)Alignment![](../images/ac.menuaro.gif)Single Segment![](../images/GUID-10094573-05A5-439E-A30D-2E506775AA08.png).
2.  When prompted to select a segment to label, click the alignment between stations 0+060 and 0+080. When a label appears on the alignment, press Enter to end the command.
    
    ![](../images/GUID-44F0C4AE-5C4C-40EE-B72E-C8AF72E34B77.png)
    
    Segment label added to alignment
    
3.  Select the new label. On the Properties palette, notice that the label is on C-ROAD-TEXT.
    
    When labels are created, they are placed on the layer specified for the label object in the Drawing Settings dialog box on the Object Layers tab. The drawing settings for this drawing specify that alignment labels are created on the C-ROAD-TEXT layer.
    
4.  On the Properties palette, click the field next to Line Label Style. Click Create/Edit.
5.  In the Line Label Style dialog box, click ![](../images/GUID-2843DA70-3F27-47F0-B628-D9D50271C288.png)Edit Current Selection.
    
    In the Label Style Composer dialog box, on the General tab, in the Label category, notice that the layer style is C-ROAD-BRNG. This means that if any of the color, line type, and line weight properties are set to ByLayer or ByBlock, they inherit the corresponding properties of the C-ROAD-BRNG layer, no matter what layer the label object is on.
    
6.  Click the Layout tab.
    
    Use the Component Name list to examine the properties of the various label components. Notice that the Color of all the components is ByLayer, except for the Direction Arrow component.
    
    The label components that have their Color property set to ByLayer are red because the color property of the C-ROAD-BRNG layer is red. The direction arrow is blue because its Color property is blue.
    
7.  Click Cancel to exit the Label Style Composer and Line Label Style dialog boxes.

Examine the affects of layer visibility on the labels

1.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer list. Next to the **C-ROAD-TEXT** layer, click ![](../images/GUID-9E403626-F29D-448F-B033-C50EBCBB4D71.png) to turn off the C-ROAD-TEXT layer.
    
    The major chainage labels and ticks are hidden, but the straight label you added is not. The chainage labels were hidden because they are on the C-ROAD-TEXT layer and their style refers to the C-ROAD-TEXT layer, which you turned off. The straight label is still visible because, while you turned off the layer it is on, its style components refer to the C-ROAD-BRNG layer, which is still visible.
    
    ![](../images/GUID-81F8AF5C-4BA5-4B16-81E5-4276B8CD0FFD.png)
    
    Chainage labels hidden
    
    Notice that the STA:0+080 and STA:0+100 labels you changed in [Exercise 2: Using a Child Label Style](GUID-C49FC84B-69EC-455A-A563-11C3B8DF3B29.htm "In this exercise, you will create a child label style that derives its default settings from an existing label style, or parent.") are also still visible. This is because the rest of the major chainage labels use the Perpendicular With Tick style, which refers to layer 0. Labels STA=0+060 and STA=0+080 use the Perpendicular With Lineand Chainage Emphasis styles, which refer to the C-ROAD-LABL layer.
    
2.  Click Home tab ![](../images/ac.menuaro.gif)Layers panel ![](../images/ac.menuaro.gif)Layer list. Next to the **C-ROAD-TEXT** layer, click ![](../images/GUID-9FD70D42-9613-455A-AEF7-0428FB774CDC.png) to turn on the **C-ROAD-TEXT** layer and the chainage labels.

To continue this tutorial, go to [Exercise 4: Changing the Dragged State of a Label](GUID-057893BF-B66F-4F60-8ABC-A0C935E986F9.htm "In this exercise, you will modify a label style so that a label will display differently when it is dragged from its original location.").

**Parent topic:** [Tutorial: Working with Label Styles](GUID-F663DB98-120E-4A8D-9762-CB799972916A.htm "This tutorial demonstrates how to define the behavior, appearance, and content of labels using label styles.")