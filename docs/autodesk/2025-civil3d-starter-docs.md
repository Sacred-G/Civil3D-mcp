# Autodesk Civil 3D 2025 Starter Documentation

Source: Official Autodesk Help pages extracted via Playwright from `help.autodesk.com`.

## Document 1

- **Title**: Autodesk Civil 3D Help | Autodesk Civil 3D APIs | Autodesk
- **URL**: https://help.autodesk.com/view/CIV3D/2025/ENU/?guid=GUID-1FC5774A-14EB-48CC-8A0A-FA983E8B9703

```text
Help Home
Sign In
English (US)
What's New in Civil 3D
Release Notes
Help
Tutorials
Best Practices Guide
Have You Tried
Subassembly Reference
Subassembly Composer Help
Dynamo for Civil 3D
Geotechnical Modeler (In English Only)
Project Explorer Help
IFC 4.3 Extension for Autodesk Civil 3D
Survey and Transform Tools for Coordinate Geometry (in English only)
Autodesk SHP Import/Export Utility Help (in English only)
Infrastructure Content Authoring
Content Browser Help
Object Enabler Help
CAiCE Translator Help
API Reference Guide
Developer's Guide
API Developer's Guide
About the Developer's Guide
Intended Audience
Autodesk Civil 3D APIs
Organization
New Features in the Autodesk Civil 3D API
Getting Started
Root Objects and Common Concepts
Surfaces
Alignments
Profiles
Sections Overview
Pipe Networks
Corridors
Points
Data Shortcuts
Creating Custom Subassemblies Using .NET
Converting VBA Subassemblies to .NET
Legacy COM API
.NET Core Developer's Guide
Learn AutoCAD Map 3D
Use AutoCAD Map 3D
Customize AutoCAD Map 3D
AutoCAD User's Guide
Customization and Administration Guides
AutoLISP: Developer's Guide
AutoLISP: Reference
Autodesk Installation
Autodesk Civil 3D Installation
SHARE
Autodesk Civil 3D APIs

There are three APIs available for customizing Autodesk Civil 3D:

.NET API — allows you to write extensions to Autodesk Civil 3D in any .NET language. In general, the Autodesk Civil 3D.NET API performs significantly faster than the COM API. Development requires Microsoft Visual Studio 2008 SP1 or better.
COM API — you can create clients that access the COM API from managed (.NET) or unmanaged (C++) code. See Creating Client Applications. In addition, this API can be used in the Visual Basic for Applications (VBA) IDE, which is available as a separate download. VBA support is deprecated.
Custom Draw API (in C++) — an extension of the AutoCAD ObjectARX API that allows you to customize the way Autodesk Civil 3D renders objects. Development requires Microsoft Visual Studio.

The COM and .NET APIs are described in this guide. For more information about the Custom Draw API, see the Custom Draw API Reference (civildraw-reference.chm).

In addition, an API is provided for creating custom subassemblies in .NET. See Creating Custom Subassemblies Using .NET.

Which API you choose to use depends on what you want to do:

If you want to:	Use:
Customize the way objects are rendered in Autodesk Civil 3D	the Custom Draw API. The Custom Draw API is an extension of the AutoCAD ObjectARX API. For example, if you wanted to number the triangles on a TIN surface, you could create a DLL using the Custom Draw API. See the sample applications shipped with Autodesk Civil 3D for an example.
Create macros to automate repetitive actions	.NET or COM API.
Create applications to manipulate Autodesk Civil 3D objects	.NET or COM API.
Note:

Where possible, you should use the Civil .NET API instead of the COM API, especially for longer operations, as the .NET API is a thin layer to Civil objects and has better performance. However, you may find you need to use the COM object to access some functionality or object members that are not yet exposed by the .NET API. In this case it's possible to use both. See Limitations and Using Interop.

Parent topic: About the Developer's Guide
Was this information helpful?YesNo
```

## Document 2

- **Title**: Autodesk Civil 3D Help | Getting Started | Autodesk
- **URL**: https://help.autodesk.com/view/CIV3D/2025/ENU/?guid=GUID-A28F5328-AE1F-4B77-84D8-9CCE9D683675

```text
Help Home
Sign In
English (US)
What's New in Civil 3D
Release Notes
Help
Tutorials
Best Practices Guide
Have You Tried
Subassembly Reference
Subassembly Composer Help
Dynamo for Civil 3D
Geotechnical Modeler (In English Only)
Project Explorer Help
IFC 4.3 Extension for Autodesk Civil 3D
Survey and Transform Tools for Coordinate Geometry (in English only)
Autodesk SHP Import/Export Utility Help (in English only)
Infrastructure Content Authoring
Content Browser Help
Object Enabler Help
CAiCE Translator Help
API Reference Guide
Developer's Guide
API Developer's Guide
About the Developer's Guide
Getting Started
Setting up a .NET Project for Autodesk Civil 3D
Running Commands from the Toolbox
Migrating COM code to .NET
Root Objects and Common Concepts
Surfaces
Alignments
Profiles
Sections Overview
Pipe Networks
Corridors
Points
Data Shortcuts
Creating Custom Subassemblies Using .NET
Converting VBA Subassemblies to .NET
Legacy COM API
.NET Core Developer's Guide
Learn AutoCAD Map 3D
Use AutoCAD Map 3D
Customize AutoCAD Map 3D
AutoCAD User's Guide
Customization and Administration Guides
AutoLISP: Developer's Guide
AutoLISP: Reference
Autodesk Installation
Autodesk Civil 3D Installation
SHARE
Getting Started

This chapter describes how to use VBA, and how to set up a new project for using the COM or .NET APIs for Autodesk Civil 3D.

Topics in this section
Setting up a .NET Project for Autodesk Civil 3D

Running Commands from the Toolbox

Migrating COM code to .NET

Parent topic: API Developer's Guide
Was this information helpful?YesNo
```

## Document 3

- **Title**: Autodesk Civil 3D Help | Root Objects and Common Concepts | Autodesk
- **URL**: https://help.autodesk.com/view/CIV3D/2025/ENU/?guid=GUID-3EDF3403-F225-4934-A7A1-654F3D364097

```text
Help Home
Sign In
English (US)
What's New in Civil 3D
Release Notes
Help
Tutorials
Best Practices Guide
Have You Tried
Subassembly Reference
Subassembly Composer Help
Dynamo for Civil 3D
Geotechnical Modeler (In English Only)
Project Explorer Help
IFC 4.3 Extension for Autodesk Civil 3D
Survey and Transform Tools for Coordinate Geometry (in English only)
Autodesk SHP Import/Export Utility Help (in English only)
Infrastructure Content Authoring
Content Browser Help
Object Enabler Help
CAiCE Translator Help
API Reference Guide
Developer's Guide
API Developer's Guide
About the Developer's Guide
Getting Started
Root Objects and Common Concepts
Root Objects
Settings
Label Styles
Sample Programs
Name API 'ExportTo' using C++
Surfaces
Alignments
Profiles
Sections Overview
Pipe Networks
Corridors
Points
Data Shortcuts
Creating Custom Subassemblies Using .NET
Converting VBA Subassemblies to .NET
Legacy COM API
.NET Core Developer's Guide
Learn AutoCAD Map 3D
Use AutoCAD Map 3D
Customize AutoCAD Map 3D
AutoCAD User's Guide
Customization and Administration Guides
AutoLISP: Developer's Guide
AutoLISP: Reference
Autodesk Installation
Autodesk Civil 3D Installation
SHARE
Root Objects and Common Concepts

This chapter explains how to work with the root objects required to access all other objects exposed by the Autodesk Civil 3D .NET API: CivilApplication and CivilDocument, as well as how to work with collections. It also describes how to work with settings and label styles.

Topics in this section
Root Objects

Settings

Label Styles

Sample Programs

Name API 'ExportTo' using C++

Parent topic: API Developer's Guide
Was this information helpful?YesNo
```

## Document 4

- **Title**: Autodesk Civil 3D Help | Alignments | Autodesk
- **URL**: https://help.autodesk.com/view/CIV3D/2025/ENU/?guid=GUID-5A50B836-D994-4519-925D-CC502D7BB8CA

```text
Help Home
Sign In
English (US)
What's New in Civil 3D
Release Notes
Help
Tutorials
Best Practices Guide
Have You Tried
Subassembly Reference
Subassembly Composer Help
Dynamo for Civil 3D
Geotechnical Modeler (In English Only)
Project Explorer Help
IFC 4.3 Extension for Autodesk Civil 3D
Survey and Transform Tools for Coordinate Geometry (in English only)
Autodesk SHP Import/Export Utility Help (in English only)
Infrastructure Content Authoring
Content Browser Help
Object Enabler Help
CAiCE Translator Help
API Reference Guide
Developer's Guide
API Developer's Guide
About the Developer's Guide
Getting Started
Root Objects and Common Concepts
Surfaces
Alignments
Basic Alignment Operations
Stations
Alignment Style
Sample Programs
Profiles
Sections Overview
Pipe Networks
Corridors
Points
Data Shortcuts
Creating Custom Subassemblies Using .NET
Converting VBA Subassemblies to .NET
Legacy COM API
.NET Core Developer's Guide
Learn AutoCAD Map 3D
Use AutoCAD Map 3D
Customize AutoCAD Map 3D
AutoCAD User's Guide
Customization and Administration Guides
AutoLISP: Developer's Guide
AutoLISP: Reference
Autodesk Installation
Autodesk Civil 3D Installation
SHARE
Alignments

This chapter covers creating and using Alignments, Stations, and Alignment styles using the Autodesk Civil 3D .NET API.

Topics in this section
Basic Alignment Operations

Stations

Alignment Style

Sample Programs

Parent topic: API Developer's Guide
Was this information helpful?YesNo
```
