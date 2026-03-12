# 🚀 AI-CAD Agent Integration Masterplan (v2.5)

## 🎯 Project Vision
Create a professional, natively integrated AI assistant within **AutoCAD & Civil 3D**. The system acts as a smart "Co-Pilot" for infrastructure engineers, transforming natural language intent into precise, rule-based engineering objects. By bypassing manual menu navigation, the agent streamlines complex workflows, from basic geometry to advanced corridor modeling.

---

## 🏗️ Architecture Overview
1.  **The Interface (WPF Palette):** A native, dark-themed "Dockable" palette with advanced UI features (Sidebar history, Live-drawing visualization, Settings tab).
2.  **The Brain (Multi-LLM Support):** A flexible AI layer supporting dynamic model switching (e.g., Gemini 2.5 Flash vs. 3.1 Pro), returning structured responses (Conversational Message + Executable Commands).
3.  **The Controller (Clean Code Router):** A decoupled C# logic layer utilizing `CommandRouter` for scalable command execution and `AgentPromptManager` for prompt isolation.
4.  **The Hands (CAD/Civil 3D API):** Compiled C# transactions that interact directly with the AutoCAD/Civil 3D Database (`AeccDbMgd`, `AecBaseMgd`) to create and modify infrastructure objects.

---

## 🗺️ Execution Phases

### ✅ Phase 1: Native Integration (Completed)
* **PaletteSet Implementation:** Developed a native, dockable UI to replace the external console.
* **Modern UI/UX:** Created a WPF chat interface with a "New Chat" feature and dark theme.
* **Secure Connection:** Established a direct TLS 1.2 connection to the Gemini API.

### ✅ Phase 2: Core Engine & Advanced UX (Completed)
* **Smart Parsing:** Upgraded JSON schema to return both conversational feedback and command arrays.
* **Live Drafting:** Implemented asynchronous transaction delays to visualize the drawing process in real-time.
* **Session Management:** Built a dynamic Chat History sidebar with save, rename, and delete functionalities.
* **Scalable Architecture:** Refactored code for Separation of Concerns (SoC) to handle future commands efficiently.
* **Base Tools:** Implemented and tested base geometric transactions (`DrawLine`, `DrawCircle`).

### ✅ Phase 3: Civil 3D Engineering Core (Completed)
* **API Key & Model Settings:** Implemented a secure Settings tab with Windows Registry memory for API keys and dynamic AI provider selection.
* **Cogo Points:** Integrated `CogoPointCollection` API for automated placement of engineering points based on coordinates and elevations.
* **Alignment Generation:** Developed tools to create horizontal alignments from natural language vertex descriptions.

### 🚀 Phase 4: Advanced Infrastructure Modeling (Current - Massive Progress)
* **✅ Auto-Design Profiles:** Implemented algorithmic Finished Ground (FG) profile generation. Dynamically calculates segments and automatically applies Crest/Sag parabolas based on tangent grades, bypassing manual Layout tools.
* **✅ Automated Corridor Assembly:** Successfully chained Alignments, Layout Profiles, and Assemblies to generate full 3D Corridors via natural language.
* **✅ Smart Targeting:** Engineered C# overrides to automatically attach Surface Targets (EG) to Daylight subassemblies, grounding the 3D model.
* **✅ Cross-Section Framework:** Automated the creation of Sample Line Groups, geometric Sample Lines (e.g., -20m to +20m swaths), and Section View plotting.
* **🛠️ Advanced Section Sampling (Next):** Implement robust API/Reflection workarounds to programmatically sample Corridors and Surfaces into Section Views without manual "Sample More Sources" intervention.

### 🧠 Phase 5: Drawing Intelligence & "The LinkedIn Showcase" (Upcoming)
* **Drawing Scanner (Read Ability):** Implement a tool that allows the AI to "read" existing layers, alignments, and surface data back into its context.
* **"Killer Feature" Prompt:** A single, flawless prompt execution: *"Generate a 500m alignment, create an auto-profile, build a corridor with surface targets, and plot cross-sections."*
* **Contextual Modification:** Enable prompts like *"Find the 'MainRoad' alignment and change its style to 'Proposed'"*.
* **Self-Correction:** Train the AI to recognize CAD errors and suggest programmatic fixes.

---

## 📝 Project Details & Context
* **Lead Developer:** Beka Tchigladze, Civil Engineer (Infrastructure Design).
* **Organization:** Green Road Group.
* **Tech Stack:** C# (.NET 8.0/Framework), WPF, AutoCAD/Civil 3D .NET API, Gemini API.