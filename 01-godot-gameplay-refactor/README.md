# Godot Gameplay Architecture Refactor

## What this project demonstrates

This case demonstrates my ability to take a playable but rough Godot prototype and improve its gameplay architecture without breaking the core loop.

The refactor focuses on:
- extracting shared enemy behavior into a reusable base class;
- cleaning up enemy hierarchy and scene structure;
- reducing duplicated logic between enemy types;
- moving ground/tile logic into a dedicated script;
- simplifying the world scene;
- improving pickup and player interaction consistency;
- keeping the prototype playable while making the code easier to extend.

## Context

The project started as a small playable Godot prototype with player movement, enemies, pickups, collision logic and a basic world scene.

The initial version was functional, but several gameplay responsibilities were still mixed between concrete enemy scripts, scene nodes and the main world scene.

The goal of the refactor was not to rewrite the prototype from scratch, but to preserve the working gameplay loop while making the project easier to maintain and expand.

## Refactor scope

Compared commits:

- Before: `b9ea256` — Complete playable prototype before cleanup
- After: `455cff5` — Cleanup gameplay architecture, enemy hierarchy and collision model

Changed gameplay-related files:

- `scripts/BaseEnemy.gd`
- `scripts/Enemy.gd`
- `scripts/GuardEnemy.gd`
- `scripts/GroundTiles.gd`
- `scripts/BonusHeart.gd`
- `scripts/ShapeShard.gd`
- `scripts/Player.gd`
- `scripts/World.gd`
- enemy, pickup, player and world scenes

Code / scene diff:

- 15 gameplay-related files changed
- 200 insertions
- 157 deletions

## Main changes

### 1. Enemy hierarchy cleanup

A new `BaseEnemy.gd` script was introduced to hold shared enemy behavior.

Before the refactor, different enemy types had more duplicated or scene-specific logic. After the refactor, common behavior can live in the base class, while concrete enemy scripts can focus on their specific behavior.

This makes it easier to add new enemy types without copying the same logic again.

### 2. Enemy scene structure

Enemy scenes were adjusted to support the new hierarchy:

- `scenes/enemy/Enemy.tscn`
- `scenes/enemy/GuardEnemy.tscn`

The goal was to make the scene setup match the code structure more clearly.

### 3. Ground / tile logic extraction

A new `GroundTiles.gd` script was added.

This moves ground-related behavior out of the main world scene and into a dedicated component. The world scene becomes less overloaded and easier to scan.

### 4. World scene simplification

`scenes/world/world.tscn` was significantly reduced and cleaned up.

This suggests that part of the scene-specific setup was moved into scripts or more focused scene components, reducing the amount of fragile logic directly embedded in the world scene.

### 5. Interaction consistency

Small changes were made to:

- `BonusHeart.gd`
- `ShapeShard.gd`
- `Player.gd`
- `World.gd`

These changes support the refactor and help keep player / pickup / world interactions consistent with the new structure.

## Result

The prototype remains playable, but the codebase is cleaner and easier to extend.

The main benefit of the refactor is structural:

- shared behavior is easier to maintain;
- enemy types are easier to expand;
- the world scene has fewer responsibilities;
- gameplay logic is less duplicated;
- future features can be added with less risk of breaking existing behavior.

## Limitations

This is still a small prototype, not a production-ready game architecture.

Possible next steps:

- add more enemy types using the new base class;
- add clearer debug tools;
- add comments to the most important gameplay methods;
- move tunable values into exported variables or resources;
- add a short manual test checklist;
- move exported builds out of the source repository and into GitHub Releases.
