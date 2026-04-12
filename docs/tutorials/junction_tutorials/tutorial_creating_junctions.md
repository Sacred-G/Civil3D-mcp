---
title: "Tutorial: Creating Junctions"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-16F7BEE8-A134-4D9F-9AD1-EC8399E4CDB4.htm"
category: "junction_tutorials"
last_updated: "2026-03-17T18:42:54.875Z"
---

                 Tutorial: Creating Junctions  

# Tutorial: Creating Junctions

This tutorial demonstrates how to create several types of junctions.

You will create two basic types of junctions, which differ in how the intersecting road crowns are blended:

*   In a _peer road_ junction, the crowns of both roads are maintained. The primary road centerline profile is maintained, and a locked VIP is created on the side road centerline profile where it intersects with the primary road centerline. The pavement of both roads is blended into the radius kerbs.
*   In a _primary road_ junction, the primary road crown is maintained. The primary road centerline profile is maintained, and a locked VIP is created on the side road centerline profile where it intersects with the primary road centerline. Two additional locked VIPs are created on the side road centerline profile at the primary road edges of pavement. The primary road cross-gradient is maintained, and the side road pavement is blended from the primary road edges of pavement along the side road pavement edges.

You will also experiment with radius kerb widening parameters, which are used to create turning lanes.

For information on adding widening regions to offset alignments that are outside the junction area, see the [Adding a Widening to an Offset Alignment](GUID-8208038F-7522-44C1-8DBC-5AAC9D789107.htm "In this exercise, you will add dynamic widening regions between specified stations of an offset alignment.") exercise.

**Topics in this section**

*   [Exercise 1: Creating a Peer Road Junction](GUID-1A83964C-D705-44FC-9A69-0F63E969D848.htm)  
    In this exercise, you will create a three-way junction and generate a corridor that maintains the crowns of both roads.
*   [Exercise 2: Creating a Primary Road Junction with Turning Lanes](GUID-91BAC9A2-6820-4646-988E-3756B36FCDBA.htm)  
    In this exercise, you will create a junction with entry and exit turning lanes at the primary road. The side road crown will blend into the primary road edge of pavement.
*   [Exercise 3: Creating a Junction with Existing Geometry](GUID-091643F5-9680-4440-B7A9-333F0FD5CB44.htm)  
    In this exercise, you will use the existing offset alignments and profiles of the primary road to create a junction, and then add the new junction to the existing primary road corridor.

**Parent topic:** [Junction Tutorials](GUID-AAAFC8F1-14E5-4AE4-AE69-BC8DE743AFD4.htm "These tutorials will get you started working with junctions.")