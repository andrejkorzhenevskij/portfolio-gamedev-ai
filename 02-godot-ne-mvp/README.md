# Godot NE MVP — Narrative Vertical Slice Wiring

## What this project demonstrates

This case demonstrates my ability to design and implement a minimal narrative/gameplay vertical slice in Godot.

The focus is not on building a universal dialogue engine, but on wiring a concrete playable flow:

- global game state;
- screen-to-screen progression;
- F1 / F2 / F3 scene structure;
- surgery interaction layer;
- route/result logic;
- final screen binding;
- dossier and snapshot screens;
- text repository for final output.

## Core scripts

- `GameState.gd` — central state owner for the MVP.
- `main_gameplay_screen.gd` — main gameplay screen / flow coordinator.
- `f1_scene.gd`, `f2_scene.gd`, `f3_scene.gd` — narrative scene fragments.
- `surgery_layer.gd` — interactive surgery layer.
- `surgery_route_layer.gd` — route/choice layer for surgery flow.
- `final_screen.gd` — final result screen.
- `final_text_repository.gd` — repository for final text variants.
- `dossier_review_screen.gd` — dossier review UI.
- `snapshot_screen.gd` — snapshot/result-style UI.
- `intake_desk.gd` — intake/front-desk style screen logic.

## Architecture approach

The MVP keeps the system intentionally small.

Instead of introducing a full dialogue framework, the project uses dedicated Godot scripts for the screens and mechanics required by this vertical slice.

The main architectural idea is:

```text
GameState → scene flow → surgery choice/result → final text / dossier / snapshot output

## What this demonstrates technically

- Godot scene flow planning.
- Centralized state ownership.
- Narrative screen progression.
- UI scripts separated by screen responsibility.
- Interactive layer feeding into later result screens.
- Text variants handled through a repository script.
- MVP-first architecture without overengineering.

## Limitations

This is a vertical slice wiring case, not a full narrative engine.

Possible next steps:

- Move more content into external Resources or JSON.
- Add a debug menu for jumping between screens.
- Add save/load.
- Add a manual test checklist.
- Expand result mapping.
- Add clearer automated validation for missing state.
- Separate visual/presentation layers from logic even further.
