# civil3d-mcp
 
 [English](README.md) | English
 
 ## Overview
 
 `civil3d-mcp` connects an MCP-compatible client such as Claude Desktop to Autodesk Civil 3D.
 
 This repository contains two parts:
 
 - The **Node.js MCP server** that exposes tools to your AI client
 - The **Civil 3D plugin** that runs inside Civil 3D and executes requests
 
 The normal flow is:
 
 1. Claude Desktop starts the MCP server
 2. The MCP server exposes Civil 3D tools to Claude
 3. The Civil 3D plugin is loaded with `NETLOAD`
 4. The plugin listens inside Civil 3D and processes requests
 
 ## Features
 
 - Read data from the active Civil 3D drawing
 - Create and modify Civil 3D objects
 - Query drawing state and selected object information
 - Bridge AI tool calls into a running Civil 3D session
 
 ## Requirements
 
 - Windows
 - Autodesk Civil 3D installed and able to run locally
 - Node.js 18+
 - .NET 8 SDK if you want to build the plugin locally
 - Claude Desktop or another MCP-compatible client
 
 ## Quick Start
 
 1. Install Node.js
 2. Run `npm install`
 3. Run `npm run build`
 4. Build the plugin DLL
 5. Add this server to Claude Desktop config
 6. Start Civil 3D
 7. Run `NETLOAD`
 8. Load `Civil3DMcpPlugin.dll`
 9. Run `C3DMCPSTATUS`
 10. Restart Claude Desktop
 
 ## Installation
 
 ### 1. Clone the repository
 
 ```bash
 git clone https://github.com/eltraviesolui/civil3d-mcp.git
 cd civil3d-mcp
 ```
 
 ### 2. Install Node dependencies
 
 ```bash
 npm install
 ```
 
 ### 3. Build the MCP server
 
 ```bash
 npm run build
 ```
 
 This produces:
 
 ```text
 build\index.js
 ```
 
 ### 4. Build the Civil 3D plugin
 
 ```bash
 dotnet build .\Civil3D-MCP-Plugin\Civil3DMcpPlugin.csproj -c Release
 ```
 
 The plugin output DLL is:
 
 ```text
 Civil3D-MCP-Plugin\bin\Release\net8.0-windows\Civil3DMcpPlugin.dll
 ```
 
 ## Claude Desktop Configuration
 
 Open:
 
 ```text
 %APPDATA%\Claude\claude_desktop_config.json
 ```
 
 Add this under `mcpServers`:
 
 ```json
 {
   "mcpServers": {
     "civil3d-mcp": {
       "command": "node",
       "args": [
         "c:\\Users\\tech01\\Documents\\steven_dev\\civil3d-mcp\\build\\index.js"
       ]
     }
   }
 }
 ```
 
 Then fully restart Claude Desktop.
 
 ![claude](./assets/claude.png)
 
 ## Running the Plugin in Civil 3D
 
 ### 1. Start Civil 3D
 
 Launch Civil 3D and open a drawing.
 
 ### 2. Show the command line if needed
 
 ```text
 Ctrl+9
 ```
 
 ### 3. Run `NETLOAD`
 
 In Civil 3D, type:
 
 ```text
 NETLOAD
 ```
 
 Then open:
 
 ```text
 c:\Users\tech01\Documents\steven_dev\civil3d-mcp\Civil3D-MCP-Plugin\bin\Release\net8.0-windows\Civil3DMcpPlugin.dll
 ```
 
 ### 4. Verify the listener
 
 Run:
 
 ```text
 C3DMCPSTATUS
 ```
 
 If needed, run:
 
 ```text
 C3DMCPSTART
 ```
 
 To stop it:
 
 ```text
 C3DMCPSTOP
 ```
 
 ## Running the MCP Server Manually
 
 ```bash
 node .\build\index.js
 ```
 
 A successful startup prints:
 
 ```text
 Civil 3D MCP Server start success
 ```
 
 ## Architecture
 
 ```mermaid
 flowchart LR
   ClaudeDesktop --> MCPServer[civil3d-mcp Node server]
   MCPServer --> Civil3DPlugin[Civil 3D MCP plugin]
   Civil3DPlugin --> Civil3D[Autodesk Civil 3D]
 ```
 
 ## Supported Tools
 
 - `civil3d_assembly`
 - `civil3d_alignment`
 - `civil3d_coordinate_system`
 - `civil3d_corridor`
 - `civil3d_data_shortcut`
 - `civil3d_drawing`
 - `civil3d_feature_line`
 - `civil3d_health`
 - `civil3d_job`
 - `civil3d_label`
 - `civil3d_parcel`
 - `civil3d_pipe_network`
 - `civil3d_pipe_network_edit`
 - `civil3d_point`
 - `civil3d_profile`
 - `civil3d_section`
 - `civil3d_style`
 - `civil3d_surface`
 - `civil3d_surface_edit`
 - `get_drawing_info`
 - `list_civil_object_types`
 - `get_selected_civil_objects_info`
 - `create_cogo_point`
 - `create_line_segment`
 
 Join [Discord](https://discord.gg/cGzUGurq) | [QQ Group](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=kLnQiFVtYBytHm7R58KFoocd3mzU_9DR&authKey=fyXDOBmXP7FMkXAWjddWZumblxKJH7ZycYyLp40At3t9%2FOfSZyVO7zyYgIROgSHF&noverify=0&group_code=792379482)
