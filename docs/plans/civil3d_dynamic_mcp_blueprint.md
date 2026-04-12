# Dynamic MCP Server for Civil 3D

## Purpose
This project is an experimental **MCP server + orchestrator architecture** for Autodesk Civil 3D.

The goal is to build a system that can:

1. Connect to Civil 3D through an existing plugin layer.
2. Expose Civil 3D capabilities as MCP tools.
3. Use a vector database containing Civil 3D / Autodesk API documentation, examples, schemas, and internal plugin references.
4. Route requests through an orchestrating agent instead of loading every tool and every API detail into context up front.
5. Detect when an existing tool fails, is missing, or cannot complete a task.
6. Use retrieved documentation and the error context to generate new code or extend plugin functionality.
7. Compile, register, and use the newly created capability within the same session when safe and allowed.

This is **not** just a normal MCP server. The real objective is to create a **self-extending Civil 3D tool system**.

---

## High-Level Vision
The system should behave like this:

- The agent receives a user request.
- It does **not** load 200+ tool implementations into prompt context.
- Instead, it knows:
  - the available domains,
  - the tool registry,
  - which tool families exist,
  - and how to discover or create missing capabilities.
- It first tries an existing Civil 3D tool.
- If the tool works, return the result.
- If the tool fails because the capability does not exist, the plugin does not expose it, or the workflow is incomplete:
  - capture the error,
  - retrieve relevant API and plugin guidance from the vector database,
  - generate or adapt code,
  - compile and validate it,
  - register it as a new tool/plugin capability,
  - then retry the workflow.

This creates an architecture where the MCP server is not only a tool host, but also a **tool evolution layer**.

---

## Core Problem Being Solved
Civil 3D is powerful, but tool access is usually limited by whatever plugin functionality was written ahead of time.

That creates a bottleneck:

- if a user asks for something supported by existing tools, the system works,
- but if they ask for something adjacent, new, or partially supported, the agent fails.

The goal of this project is to reduce that bottleneck by allowing the system to:

- discover what is possible from docs,
- reason about why a current tool failed,
- generate a targeted extension,
- and add that extension back into the runtime.

---

## Main Components

### 1. Civil 3D Plugin Layer
A compiled C# plugin that runs inside or alongside Civil 3D and exposes safe operations.

This plugin layer should:

- wrap Autodesk / Civil 3D API calls,
- expose known commands and operations,
- return structured results,
- return structured errors,
- support registration of new commands or modules,
- and isolate dangerous operations from the orchestrator.

This layer is the bridge between the agent and Civil 3D.

---

### 2. MCP Server
The MCP server exposes tool access in a way the agent can consume.

It should provide:

- a tool registry,
- domain-aware routing,
- capability metadata,
- schemas for tool inputs/outputs,
- execution logging,
- and a mechanism for registering new tools at runtime.

Example domains:

- drawing and geometry
- alignments and profiles
- corridors
- surfaces
- parcels
- pipe networks
- grading
- hydrology / drainage
- labeling / annotation
- reports / exports
- document lookup / API retrieval
- code generation / plugin extension

The orchestrator should be aware of domains, not every low-level implementation detail.

---

### 3. Orchestrator Agent
This is the reasoning layer.

It should:

- interpret the user’s request,
- choose a domain,
- discover whether an existing tool already covers the task,
- call the tool,
- inspect failures,
- decide whether the failure is recoverable,
- retrieve relevant documentation,
- decide whether a new tool/plugin extension is needed,
- and trigger generation only when necessary.

The orchestrator should not try to hold the full Civil 3D API in prompt memory.
It should rely on retrieval and a registry.

---

### 4. Vector Database / Retrieval Layer
This stores the knowledge needed for grounded tool creation and repair.

Recommended content:

- Civil 3D API documentation
- Autodesk .NET API documentation
- sample plugin code
- internal wrapper code examples
- previously generated working tools
- plugin registration patterns
- command schemas
- error-to-fix mappings
- constraints and safe coding rules

The retrieval layer should be used for:

- API usage lookup,
- finding correct classes/methods,
- pulling code examples,
- resolving errors,
- and grounding generated code in real documentation.

This is one of the most important pieces. Without retrieval, generated tools will be unreliable.

---

### 5. Code Generation Layer
This component produces new code when existing tools are not enough.

It should generate:

- C# wrapper methods,
- new plugin modules,
- MCP tool manifests,
- parameter schemas,
- validation logic,
- and retry handlers.

This generation should be constrained by:

- templates,
- allowed namespaces,
- safe API boundaries,
- known plugin contracts,
- and compilation rules.

Free-form code generation without constraints will break often.

---

### 6. Compilation / Validation Layer
If you want runtime tool creation for Civil 3D, generated code cannot stop at text.
It needs to be validated.

This layer should:

- compile generated C# code,
- validate references,
- run static checks,
- reject unsafe or unsupported patterns,
- optionally run unit or smoke tests,
- and only then register the new capability.

Roslyn is the obvious candidate for this in the .NET ecosystem.

---

### 7. Runtime Tool Registration Layer
Once code passes validation, the system needs a way to make the new capability usable.

That means:

- loading the assembly or module,
- registering metadata in the MCP registry,
- exposing the tool name, schema, and domain,
- and allowing the orchestrator to call it in the same session.

This layer is what makes the system feel self-extending instead of just code-generating.

---

## Desired Workflow

### Normal path
1. User asks for a Civil 3D task.
2. Orchestrator identifies the correct domain.
3. MCP registry returns candidate tools.
4. Existing tool is executed.
5. Result is returned.

### Failure-recovery path
1. User asks for a Civil 3D task.
2. Existing tool is attempted.
3. Tool fails or capability is missing.
4. Error is normalized into a structured failure object.
5. Retrieval layer searches docs and examples using:
   - the task,
   - the domain,
   - the current plugin contract,
   - and the failure details.
6. Code generation layer proposes a new wrapper/tool/plugin extension.
7. Compilation and validation run.
8. If valid, register the new tool.
9. Retry the original task using the new capability.
10. Persist the new capability for future use if approved.

---

## Example Trigger Logic
A failure should not automatically create code every time.
It should only trigger generation when the failure is the right kind.

Examples of good generation triggers:

- "Tool not found for requested operation"
- "Plugin does not expose this Civil 3D API workflow"
- "Known domain, but missing wrapper method"
- "Operation is supported in docs, but not implemented in current plugin"

Examples of bad generation triggers:

- invalid user input
- bad drawing state
- locked document
- permission issue
- transaction misuse that should be fixed in core code
- API ambiguity with no grounded example

The orchestrator needs a classifier that separates:

- user error,
- environment/runtime error,
- unsupported capability,
- and plugin gap.

Only the last category should usually lead to tool generation.

---

## Brutal Truth About Feasibility
This is possible, but not in the magical way it sounds when people first describe it.

What is realistic:

- domain-based MCP routing,
- retrieval over Civil 3D docs,
- structured plugin wrappers,
- error-aware fallback logic,
- code generation of wrapper stubs,
- Roslyn-based compile pipelines,
- runtime registration of new modules,
- and human-approved promotion of generated tools.

What is unrealistic, at least early on:

- fully autonomous generation of arbitrary Civil 3D tools with near-perfect correctness,
- completely unsupervised plugin mutation inside production drawings,
- trusting a model to invent safe API workflows from scratch,
- or letting every runtime error spawn new code automatically.

If built correctly, this can become a very strong system.
If built carelessly, it becomes a code factory that produces brittle plugins and unpredictable drawing behavior.

---

## Recommended Architecture Strategy
Do **not** start with fully autonomous tool creation.

Start with a staged design:

### Phase 1: Stable domain-based MCP server
Build well-defined tools across major Civil 3D domains.
The orchestrator only routes requests.
No dynamic generation yet.

### Phase 2: Retrieval-augmented tool selection
Add a vector database with Autodesk/Civil 3D documentation and your own plugin docs.
Use retrieval to help the agent choose the right existing tool and explain failures better.

### Phase 3: Code generation for wrapper suggestions
When a tool gap is found, generate proposed C# wrapper code or MCP manifests.
Do not auto-load yet.
Review outputs first.

### Phase 4: Controlled compile-and-register pipeline
Allow selected generated tools to compile and register in a sandbox or development environment.
Use strict validation and allowlists.

### Phase 5: Semi-autonomous extension mode
Only after the system proves stable should it generate and register new capabilities with limited autonomy.
Even then, scope it to known patterns.

---

## Design Principles

### Keep the plugin layer opinionated
Do not let the agent directly improvise against the full Civil 3D API surface if you can avoid it.
Create wrappers and contracts.

### Use structured errors
Every failure should return machine-readable fields such as:

- error_type
- domain
- attempted_operation
- plugin_capability_missing
- api_class
- api_method
- recoverable
- generation_candidate

Without structured errors, the fallback path will be unreliable.

### Prefer extension of known templates
Generating from a known wrapper template is much safer than generating raw free-form plugin code.

### Separate temporary tools from promoted tools
A generated capability may be:

- session-only,
- dev-approved,
- or permanent.

Do not make everything permanent immediately.

### Log everything
You need strong observability for:

- what request triggered generation,
- what docs were retrieved,
- what code was produced,
- what compile errors happened,
- and whether the final tool actually worked.

---

## Suggested Tool Categories
You mentioned large tool counts. That is fine, but organize them by capability families.

Example structure:

```text
civil3d.geometry.*
civil3d.alignment.*
civil3d.profile.*
civil3d.corridor.*
civil3d.surface.*
civil3d.parcel.*
civil3d.pipe.*
civil3d.hydrology.*
civil3d.labeling.*
civil3d.export.*
civil3d.analysis.*
docs.lookup.*
plugin.codegen.*
plugin.compile.*
plugin.register.*
```

The agent should see domains first, and then discover narrower tools only when needed.

---

## Minimum Viable Version
If building this today, the first realistic version would be:

- a Civil 3D C# plugin with a small but strong wrapper set,
- an MCP server exposing those wrappers,
- a vector database with API docs and sample code,
- an orchestrator that can route requests and inspect structured errors,
- a retrieval step that looks up relevant API references,
- and a code generation pipeline that produces **proposed** new wrappers for review.

That version is realistic.
A fully self-authoring Civil 3D MCP ecosystem is a later-stage goal.

---

## What Success Looks Like
Success is not:

- “the agent can invent anything.”

Success is:

- “the system can reliably execute known Civil 3D workflows,”
- “identify when the failure is caused by a missing capability,”
- “retrieve the right API context,”
- “generate a plausible extension,”
- “validate it safely,”
- and “reuse the new capability after controlled registration.”

That is a serious and differentiated architecture.

---

## Final Summary
The target system is a **retrieval-augmented, self-extending MCP server for Civil 3D**.

It combines:

- domain-based orchestration,
- Civil 3D plugin tooling,
- vector search over Autodesk/Civil 3D API documentation,
- structured failure handling,
- code generation,
- compilation and validation,
- and runtime tool registration.

The practical path is to build it in stages:

1. prebuilt domain tools,
2. retrieval-aware orchestration,
3. suggested code generation,
4. controlled compile-and-register,
5. limited autonomous extension.

That is the honest version of what you are trying to build.
It is hard, but it is not nonsense.
It becomes realistic when treated as a constrained engineering system instead of a fully autonomous magic agent.
