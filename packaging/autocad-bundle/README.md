# AutoCAD / Civil 3D bundle (restore notes)

`PackageContents.xml` is the manifest Civil 3D reads to autoload the plugin. Copy it to:

    C:\ProgramData\Autodesk\ApplicationPlugins\Civil3DMcpPlugin.bundle\PackageContents.xml

and put the built `Civil3DMcpPlugin.dll` (+ `.deps.json`, `.runtimeconfig.json`) in `...\Civil3DMcpPlugin.bundle\Contents\`.

The DLLs are NOT in git (`*.dll` is ignored; Autodesk assemblies must never be committed):
- Plugin DLL: rebuild with `dotnet build -c Release Civil3D-MCP-Plugin/Civil3DMcpPlugin.csproj`.
- Autodesk references (`accoremgd`, `AcDbMgd`, `acmgd`, `AecBaseMgd`, `AeccDbMgd`, `AeccPressurePipesMgd`): copy them from the Civil 3D install folder into `C_References/` at the repo root (the csproj default is `..\C_References`) (or pass `/p:Civil3DReferencesPath=<dir>`).
