# 🤖 AI CAD Agent for Civil 3D (Proof of Concept)

![C#](https://img.shields.io/badge/C%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Civil 3D](https://img.shields.io/badge/Autodesk-Civil_3D-0696D7?style=for-the-badge&logo=autodesk&logoColor=white)
![Gemini API](https://img.shields.io/badge/Google-Gemini_API-8E75B2?style=for-the-badge&logo=google&logoColor=white)

## Overview
**Let's be honest: infrastructure engineers spend way too much time clicking through menus instead of focusing on actual design logic.** This repository contains a **Proof of Concept (PoC)** for a native AI Co-Pilot built directly inside the Autodesk Civil 3D workspace. By leveraging the **Gemini API** and the **Civil 3D .NET API**, this agent translates natural language prompts into executable, chained CAD database transactions.

Instead of manual drafting, you can generate a complete 3D infrastructure model with a single sentence.

*(Note: This is an early-stage prototype, demonstrating the potential of LLM-to-BIM automation. It is not yet a production-ready enterprise tool.)*

---

## 🚀 Features (v2.1)

Currently, the AI Agent uses a custom WPF dockable palette to execute the following workflows via natural language:

* 📍 **COGO Points:** Generation and placement.
* 🛣️ **Horizontal Alignments:** Routing through specified PI coordinates.
* ⛰️ **Surface & Profiles:** Existing Ground sampling and dynamic Profile View grid creation.
* 📐 **Auto-Design Profile:** Algorithmic calculation of a Best-Fit layout profile with parabolic curves.
* 🚧 **3D Corridor:** Automated assembly of the corridor using a pre-defined assembly.
* 📊 **Cross-Sections:** Section generation fully formatted with Code Sets in a clean grid.
* 🧹 **Model Cleansing:** A quick "delete what you drew" command to safely erase transactions and reset the drawing.

---

## 🧠 How It Works Under the Hood

1.  **User Prompt:** The user types a request in the custom WPF panel (e.g., *"Draw an alignment through PIs X,Y and build the complete 3D road model."*).
2.  **LLM Processing:** The prompt is sent to the Gemini API with strict system instructions to return a structured JSON response.
3.  **Command Router:** The C# backend parses the JSON array and routes the instructions to specific CAD transaction classes (`AlignmentDrawer`, `AutoProfileDrawer`, `CorridorBuilder`, etc.).
4.  **Database Execution:** Commands are executed in strict sequential order via the Civil 3D API to prevent transaction collisions (e.g., waiting for the EG profile to commit before calculating the design profile).

---

## 🛠️ Tech Stack

* **Language:** C#
* **Framework:** .NET Framework (compatible with target AutoCAD/Civil 3D versions)
* **UI:** WPF (Windows Presentation Foundation) with a custom Dark Theme
* **CAD API:** Autodesk AutoCAD Database Services & Civil 3D Application Services
* **AI Provider:** Google Gemini API (2.5 Flash / 3.1 Pro)

---

## ⚠️ Disclaimer & Future Roadmap

Building this solo has been a major reality check. Turning an AI-CAD prototype into a stable, production-ready enterprise tool requires rigorous edge-case handling, complex geometry math, and a dedicated team of API developers and engineers. 

We are still in the "baby steps" phase of AI-driven infrastructure design. The goal of this repo is to explore the limits of the Civil 3D API and spark discussions about the transition from manual "drafting" to intelligent "orchestrating."

---

## 🤝 Let's Connect
I'm an infrastructure engineer passionate about #BIMAutomation and pushing the boundaries of CAD software. 

If you are a developer, API enthusiast, or civil engineer interested in the future of AI in AEC, I'd love to hear your thoughts!
