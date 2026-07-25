# Game Development, Narrative Systems & AI Automation Portfolio

This repository contains selected technical cases focused on gameplay prototyping, narrative systems, code refactoring, and small production automation tools.

My primary area of interest is the intersection of **game design and implementation**: turning gameplay or narrative intent into working, inspectable systems with clear state management, maintainable structure, and documented trade-offs.

## Portfolio Cases

### 01 — [Godot Gameplay Architecture Refactor](./01-godot-gameplay-refactor/)

A before-and-after refactoring case based on a playable Godot prototype.

The refactor focuses on:

* extracting shared enemy behavior into a reusable base class;
* simplifying the enemy scene hierarchy;
* separating world and ground collision responsibilities;
* reducing duplicated gameplay logic;
* preserving the original playable behavior during architectural cleanup.

**Demonstrates:** GDScript refactoring, inheritance, scene composition, gameplay architecture, collision logic, and working with an existing codebase.

---

### 02 — [Necessary Evil — Godot Narrative MVP](./02-godot-ne-mvp/)

A compact narrative gameplay prototype built in Godot.

The project implements:

* a complete screen-to-screen gameplay flow;
* centralized state management through an autoload `GameState`;
* choice and result routing;
* a surgery-style interactive layer;
* final outcome resolution;
* persistent meta-progression between runs;
* narrative content integrated directly into the gameplay structure.

**Demonstrates:** narrative systems implementation, state-driven UI flow, persistent progression, scene coordination, GDScript architecture, and technical documentation of an MVP.

---

### 03 — AI Job Scout

Currently in development.

A Python utility for collecting, normalizing, filtering, and evaluating job postings relevant to game development, narrative design, and AI-assisted creative production.

The case will include:

* the working script;
* sanitized input and output examples;
* transparent filtering and scoring rules;
* documented limitations and possible extensions.

**Demonstrates:** Python automation, text processing, structured data handling, explainable filtering logic, and practical AI-assisted workflow design.

## Technical Focus

* Godot 4 and GDScript
* Gameplay and narrative prototyping
* State management and UI flow
* Refactoring existing gameplay code
* Scene and responsibility separation
* Persistent and meta-progression systems
* Python automation
* Structured content processing
* Technical documentation
* Git and GitHub workflows

## Repository Approach

Each completed case is intended to be understandable independently.

A case generally contains:

* a concise description of the original task;
* the implementation or relevant source files;
* an explanation of architectural decisions;
* instructions for inspecting or running the project;
* known limitations and deliberately excluded scope;
* additional examples or documentation when they provide useful evidence.

These are focused portfolio cases rather than claims of production-complete commercial software. The emphasis is on clear problem framing, working implementation, maintainable decisions, and honest documentation of trade-offs.

## Relevant Roles

This portfolio is primarily relevant to roles such as:

* Technical Narrative Designer
* Narrative Systems Designer
* Gameplay or Narrative Prototyper
* Godot Developer
* Junior or Transitioning Gameplay Developer
* AI-Assisted Workflow Prototyper
* Technical Game Designer

## Repository Structure

```text
portfolio-gamedev-ai/
├── 01-godot-gameplay-refactor/
├── 02-godot-ne-mvp/
└── 03-ai-job-scout/
```

Additional cases will be added as complete, inspectable artifacts rather than empty placeholders.

