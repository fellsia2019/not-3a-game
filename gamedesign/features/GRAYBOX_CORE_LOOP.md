# Feature: Stage 1 Graybox Core Loop

Status: Complete and human-accepted, 2026-09-21
Owner/workstream: Foundation / integration owner  
Depends on: GAME_VISION, D-001–D-011, DYNAMIC_ENEMY_PATHING
Target: Windows Editor and development build

## Player Outcome

Игрок может пройти короткий цикл без боёвки героя: добыть дерево/камень,
построить башни, сформировать ими маршрут одной волны и защитить трон.

## Prototype Assumptions

Эти значения проверяют механику и не являются финальным балансом:

- один компактный representative slice большой карты: entrance сверху справа,
  throne у противоположного конца corridor, ресурсы с противоположной стороны;
- динамический grid route меняется из-за занятых башнями клеток;
- полное перекрытие разрешено: враги разрушают blocking tower и продолжают путь;
- строительство во время волны остаётся prototype assumption до решения P-002;
- узлы конечны и не восстанавливаются в рамках прототипа;
- одна подготовка до 180 секунд и одна небольшая волна;
- незавершённый забег не сохраняется.

## Scope In

### Player And Camera

- WASD movement through Input System;
- fixed orthographic isometric camera follows the hero, no rotation;
- hero has no attack, health, enemies targeting him, combat skills or weapons.

### Gathering

- interact with a tree and a stone node in range;
- context automatically selects a basic axe or pickaxe presentation;
- timed gathering grants wood or stone and depletes the node;
- no durability, tool inventory, crafting, tiers or upgrades.

### Building And Defense

- one tower recipe costs prototype resources;
- mouse placement shows valid/invalid ghost; tower cells shape or полностью
  перекрывают доступный enemy route;
- the tower automatically targets and damages one enemy type;
- towers have health and can be destroyed by blocked enemies;
- enemies follow the recalculated grid path or attack a blocker, then damage the
  throne at the destination.

### Run State And Temporary UI

- resource counters, preparation/wave state, throne health and build action;
- one compact secondary objective: gather 5 wood, then receive a small fixed
  reward from existing resources; no quest log or extra currency;
- win when the wave is defeated; lose when throne health reaches zero;
- restart action returns the prototype to its initial state.

## Scope Out

Final art/audio/UI, tutorial, hero combat, metal, multiple tower/enemy roles or
waves, tool progression, upgrades, save/load, procedural maps, NavMesh/free-form
movement, production-size map, Steam integration and production balance.

## Required Contracts

- Gathering outputs a resource type and amount to a narrow inventory contract.
- Building queries cost/availability, occupies a grid cell and spends resources
  only on valid confirm.
- Navigation returns a deterministic path or a destructible blocking tower and
  invalidates cached routes on occupancy changes.
- Tower, enemy and throne damage use one shared minimal health/damage contract.
- Run state owns preparation, active wave, win and loss transitions.
- Scene references are explicit and validated; no global service framework.

## Acceptance

- [x] A fresh dynamic-pathing run is playable from spawn to win without Console
  errors.
- [x] The hero cannot damage or be targeted by enemies.
- [x] Tree and stone gathering use the correct contextual tool and counters.
- [x] The gather objective tracks only valid collection, completes once, grants
  its configured resource reward once, and never blocks the wave.
- [x] Invalid placement explains why and never spends resources; valid placement
  may lengthen or fully block the current route.
- [x] A zigzag tower layout produces a longer route than an open field.
- [x] A full wall causes enemies to destroy a blocking tower and resume movement.
- [x] A placed tower kills enemies; survivors reaching the throne damage it.
- [x] Both dynamic-path win and loss can be reached and restarted cleanly.
- [x] Deterministic path/resource/cost/damage/state rules have EditMode coverage.
- [x] Critical placement/replanning/siege/wave integration has PlayMode coverage.

## Implemented Graybox Surface

The items below describe the current verified dynamic-route surface. The
2026-09-20 fixed-route version remains only a historical baseline.

- Integration scene: `Assets/Scenes/Stage1Graybox.unity`; it is the only enabled
  build scene. `SampleScene` remains unchanged.
- The playable ground is a 21x17 isometric cell field with a high-contrast
  perimeter. Hero movement and all resource nodes use the same cell-space
  bounds, so neither can appear beyond the visible map edge.
- Green terrain and the broad gray invasion corridor are coplanar. The enemy
  entrance is on the right; the throne is at the opposite end of the corridor,
  while resources remain predominantly on the opposite green side.
- `DynamicNavigationGrid` owns deterministic four-neighbor pathing and tower
  occupancy. Towers may be placed on corridor or green cells, including a full
  corridor wall. Enemies then attack a reachable blocker, its death releases
  the cell, and route revision triggers replanning.
- Runtime actors and landmarks use simple grounded rectangular blocks with an
  explicit scale hierarchy: stones below hero height, enemies near hero height,
  trees substantially taller, and the throne as the largest landmark. Trees,
  stones, the throne and placed towers have blocking ground footprints; tower
  placement rejects occupied positions without spending resources. Rectangles
  use a project-owned 1x1 world-unit solid sprite, and their lower bounds align
  exactly with the owning object's ground position.
- Keyboard/mouse graybox controls: WASD movement, hold E to gather, B to enter
  tower placement, left click to confirm, Escape/right click to cancel, Space to
  start the wave early, and R/restart button after win or loss.
- `Assets/Stage1/Editor/Stage1SceneBuilder.cs` can open or rebuild the generated
  scene and prefabs through `Tools > Stage 1`; on first import it also replaces
  an untouched open `SampleScene` with the graybox scene. Scene references are
  serialized and validated by PlayMode smoke coverage.
- uGUI is temporary prototype HUD only and does not resolve P-006.

## Exit And Split Rule

Keep the scene integrated under Foundation ownership. Dynamic navigation is now
an independently active durable boundary and is specified in
`DYNAMIC_ENEMY_PATHING.md`; do not begin Stage 2 content expansion until its
Stage 1 acceptance passes.
