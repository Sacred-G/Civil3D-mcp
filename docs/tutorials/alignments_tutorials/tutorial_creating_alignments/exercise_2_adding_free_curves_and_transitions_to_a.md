---
title: "Exercise 2: Adding Free Curves and Transitions to an Alignment"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-2133A9D7-9CE9-4BFF-A248-C71BA0756457.htm"
category: "tutorial_creating_alignments"
last_updated: "2026-03-17T18:42:25.140Z"
---

                   Exercise 2: Adding Free Curves and Transitions to an Alignment  

# Exercise 2: Adding Free Curves and Transitions to an Alignment

In this exercise, you will add a free curve and a free transition-curve-transition to a simple alignment.

The drawing contains a simple alignment consisting of three straights. In the next few steps, you will add free curves at circles B and C.

This exercise continues from [Exercise 1: Creating an Alignment with the Alignment Layout Tools](GUID-2EA85F3E-E50C-446A-8DF7-EFCD3EEB09FD.htm "In this exercise, you will use the alignment layout tools to draw an alignment that has curves.").

Add a free curve between two straights

1.  Open _Align-2.dwg_, which is located in the [tutorials drawings folder](GUID-DECD2305-4906-4329-A973-CFC9B625B4CD.htm#GUID-DECD2305-4906-4329-A973-CFC9B625B4CD__WS27101FEF35E777F9922804FBF5414E08-8000-79).
2.  Set your drawing window so that you can see circles B and C on the surface.
3.  If the Alignment Layout Tools toolbar is not open, select the alignment. Right-click and click Edit Alignment Geometry.
4.  In the Alignment Layout Tools toolbar, click the drop-down list ![](../images/GUID-B1B6FEF0-4F07-4BBB-A036-85F833FD4493.png). Select ![](../images/GUID-9E138DA8-101A-4A7F-9961-2D982BBB485A.png)Free Curve Fillet (Between Two Elements, Radius).
5.  As prompted on the command line, click the straight that enters circle B from the left (the ‘first element’).
6.  Click the straight that exits from circle B on the right (the ‘next element’).
7.  Press Enter to select the default value of a curve less than 180 degrees.
8.  Enter a radius value of **200**. The curve is drawn between the straights as specified.

Add a free transition-curve-transition between two straights

1.  In the Alignment Layout Tools toolbar, click the arrow next to ![](../images/GUID-60D70570-457B-49A5-884E-7940344C78BD.png). Select ![](../images/GUID-60D70570-457B-49A5-884E-7940344C78BD.png)Free Transition-Curve-Transition (Between Two Elements).
2.  As prompted on the command line, click the straight that enters circle C from the left (the ‘first element’).
3.  Click the straight that exits circle C on the right (the ‘next element’).
4.  Press Enter to select the default value of a curve less than 180 degrees.
5.  Enter a radius value of **200**.
6.  Enter a transition in length of **50**.
7.  Enter a transition out length of **50**.
    
    Note:
    
    Notice that default values that are shown on the command line.
    
8.  Exit the layout command by right-clicking in the drawing area.

![](../images/GUID-0F1FA612-D892-4202-AE36-2BF5FCEB6C50.png)

To continue this tutorial, go to [Exercise 3: Adding Floating Curves to an Alignment](GUID-882EC195-0160-4195-88F6-2BDBD47B6481.htm "In this exercise, you will add two floating curve elements to a simple alignment. First, you will add a best fit floating curve that follows the most likely path through a series of points. Then, you will add a floating reverse curve with transitions.").

**Parent topic:** [Tutorial: Creating Alignments](GUID-B489AAE6-C5DF-43F7-B6CB-E9E76D7D885C.htm "This tutorial demonstrates how to create and modify alignments.")