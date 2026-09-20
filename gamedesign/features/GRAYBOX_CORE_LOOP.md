# Feature: Stage 1 Graybox Core Loop

Status: Ready  
Owner/workstream: Foundation / integration owner  
Depends on: GAME_VISION, D-001–D-007  
Target: Windows Editor and development build

## Player Outcome

Игрок может пройти короткий цикл без боёвки героя: добыть дерево/камень,
построить одну башню и защитить трон от одной волны.

## Prototype Assumptions

Эти значения проверяют механику и не являются финальным балансом:

- одна компактная graybox-карта: трон в центре, вход врагов с одного края,
  деревья/камни на противоположной стороне;
- фиксированный enemy route, который нельзя перекрыть постройкой;
- строительство разрешено до и во время волны;
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
- mouse placement shows valid/invalid ghost and never blocks the enemy route;
- the tower automatically targets and damages one enemy type;
- enemies follow the fixed route and damage the throne at its end.

### Run State And Temporary UI

- resource counters, preparation/wave state, throne health and build action;
- one compact secondary objective: gather 5 wood, then receive a small fixed
  reward from existing resources; no quest log or extra currency;
- win when the wave is defeated; lose when throne health reaches zero;
- restart action returns the prototype to its initial state.

## Scope Out

Final art/audio/UI, tutorial, hero combat, metal, multiple towers/enemies/waves,
tool progression, upgrades, save/load, procedural maps, free pathfinding,
Steam integration and production balance.

## Required Contracts

- Gathering outputs a resource type and amount to a narrow inventory contract.
- Building queries cost/availability and spends resources only on valid confirm.
- Tower and enemy damage use one shared minimal health/damage contract.
- Run state owns preparation, active wave, win and loss transitions.
- Scene references are explicit and validated; no global service framework.

## Acceptance

- [ ] A fresh run is playable from spawn to win without Console errors.
- [ ] The hero cannot damage or be targeted by enemies.
- [ ] Tree and stone gathering use the correct contextual tool and counters.
- [ ] The gather objective tracks only valid collection, completes once, grants
  its configured resource reward once, and never blocks the wave.
- [ ] Invalid placement explains why and never spends resources.
- [ ] A placed tower kills enemies; survivors reaching the throne damage it.
- [ ] Both win and loss can be reached and restarted without scene corruption.
- [ ] Deterministic resource/cost/damage/state rules have EditMode coverage.
- [ ] Critical scene/input/placement/wave integration has a PlayMode smoke test
  or is explicitly marked degraded with an exact manual check.

## Exit And Split Rule

Implement this as one integrated graybox first. Split it into separate feature
specs only when a system becomes independently active or its contract needs
another workstream; do not write every future feature document before evidence
from this prototype exists.
