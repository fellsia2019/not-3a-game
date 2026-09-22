# Feature: Player-Shaped Enemy Pathing

Status: Stage 1 complete and human-accepted, 2026-09-21;
Stage 2 S2-MAP-02 navigation complete, 2026-09-22
Owner/workstream: Foundation / integration, with Defense and Waves/AI contracts
Depends on: D-002, D-006, D-009, D-010, D-011; grid, tower placement,
health/damage
Decision links: `production/DECISIONS.md` D-009–D-011

## Player Outcome

Игрок использует башни не только как оружие, но и как стены: короткий прямой
проход менее эффективен, чем длинный зигзагообразный лабиринт. Если игрок
полностью закрывает проход, волна не застревает и не проходит сквозь постройки —
враги разрушают мешающие башни и продолжают движение к трону.

## Scope

### In For Stage 1 Rework

- клеточная walkability map на компактном репрезентативном map slice;
- один entrance, один throne goal и один тип наземного врага;
- башня занимает клетку и меняет доступный маршрут;
- поиск детерминированного пути entrance -> throne и пересчёт после изменения
  occupied cells;
- разрешённая полная блокировка: враги выбирают мешающую башню, атакуют её,
  уничтожают и возобновляют движение;
- минимальное здоровье башни и enemy damage/attack interval;
- placement preview показывает buildable/occupied/outside-map состояние;
- герой остаётся non-combat и не является целью;
- одна волна и существующие win/lose/restart правила.

### Out

- NavMesh и свободное движение вне клеточной сетки;
- летающие/копающие/телепортирующиеся типы врагов;
- несколько entrances, развилки между несколькими тронами или cooperative lanes;
- repair, tower upgrades, walls как отдельный тип постройки;
- финальный размер 100x100/200x200, chunk streaming и production performance;
- vertical traversal, уступы и режим полёта героя;
- финальный баланс здоровья, урона и стоимости.

## Behavior

### Placement And Route Shaping

- Башню можно ставить на buildable свободную клетку серой или зелёной зоны,
  кроме throne/resource/outside-map cells.
- Существование текущего пути не является условием успешной постройки: игроку
  разрешено полностью перекрыть corridor.
- Если открытый путь существует, враги используют кратчайший доступный путь с
  детерминированным tie-break, чтобы одинаковое состояние сетки давало одинаковый
  маршрут.
- Более длинный зигзаг должен наблюдаемо увеличивать пройденную дистанцию и время
  подхода к трону.

### Blocked State

- Если открытого пути до трона нет, враг переходит из движения в siege behavior.
- Целью становится построенная башня, разрушение которой восстанавливает проход
  или продвигает врага к восстановлению прохода. Враг не выбирает героя.
- Враг подходит к доступной соседней клетке цели, атакует с фиксированным
  interval и наносит урон через общий health/damage contract.
- После разрушения башни occupied cell освобождается, путь пересчитывается, и
  враги продолжают движение. Волна не должна зависать в blocked state.
- Если несколько blockers эквивалентны, выбор стабилен и воспроизводим; точный
  tie-break является технической деталью, а не отдельной player-facing системой.

### Runtime Changes

- Изменение occupied cells инвалидирует route revision один раз; каждый враг не
  запускает полный поиск пути каждый кадр.
- Размещение во время волны следует P-002. Pathing contract обязан поддерживать
  безопасный пересчёт mid-wave, даже если UI временно разрешает строительство
  только в preparation.
- Restart очищает башни, их здоровье, navigation occupancy, route revisions и
  blocked targets.

## Contracts

- **Grid/navigation owner:** Foundation/Waves хранит buildable, walkable и
  occupied cell state и выдаёт path/blocker result.
- **Placement input:** Defense запрашивает cell validation, списывает ресурсы
  только после успешного confirm и затем публикует occupancy change.
- **Tower lifecycle:** Defense владеет tower health; spawn/destruction атомарно
  добавляет/удаляет occupied cell.
- **Enemy output:** Waves/AI следует path result либо атакует returned blocker;
  throne damage остаётся текущим loss contract.
- **Data:** costs, hit points, damage, intervals и grid dimensions являются
  prototype data, не дублируются как durable balance в Markdown.
- **Save/load:** Stage 1 не сохраняет незавершённый забег; будущий save обязан
  восстановить occupancy до запуска pathfinding.

## Edge Cases

- башня поставлена mid-wave на следующую клетку нескольких врагов;
- одновременно разрушены blocker и последний enemy;
- blocker исчез до завершения attack wind-up;
- несколько стен полностью перекрывают corridor;
- враг находится внутри клетки, ставшей occupied;
- placement отменён или не оплачен;
- throne/entrance случайно помечены occupied;
- restart во время movement и siege states.

## Acceptance

- [x] На открытой сетке враг находит путь entrance -> throne.
- [x] Одна и та же сетка всегда возвращает один и тот же путь.
- [x] Размещение башен зигзагом увеличивает длину пути относительно прямого.
- [x] Башни нельзя ставить на throne/resource/outside-map/occupied cells; отказ
  не списывает ресурсы.
- [x] Полная стена разрешается и переводит врагов в атаку на башню, а не в
  остановку или проход сквозь неё.
- [x] После разрушения blocker occupancy освобождается, путь восстанавливается и
  волна доходит до win/lose terminal state.
- [x] Герой не получает health/aggro/attack role.
- [x] EditMode покрывает pathfinding, deterministic tie-break, replanning и
  blocker selection.
- [x] PlayMode покрывает full block, tower destruction, resume, win/loss и
  restart; zigzag/path length покрыт детерминированным EditMode тестом.
- [x] Windows Development Build собран и запущен без новых startup errors;
  gameplay behavior дополнительно закрыт PlayMode.
- [x] Canonical plan, graybox spec и handoff обновлены.

## Open Decisions

- P-002: разрешено ли размещение новых башен во время активной волны.
- P-008: целевой production-размер карты после компактного Stage 1 slice.

## Stage 2 Navigation — S2-MAP-02

Owned implementation: `Assets/Stage2/Navigation/`. The Stage 1 runtime and both
scenes are unchanged. This extends the navigation contract only; it does not
approve P-002/P-008 or add combat, enemies, spawning, economy or scene wiring.

### Map And Occupancy API

`new Stage2Navigation(Stage2MapDefinition)` validates and snapshots `CellBounds`,
`EnemyEntrance`, `Throne` and the authored walkable/buildable masks via `ReadCell`.
It records `ComputeDeterministicContentHash()` as `MapHash`. The actual candidate
is `Assets/Stage2/Map/Data/Stage2Map100x100.asset`; both surfaces are walkable as
authored. Gray corridor color is not a navigation boundary. The definition must
remain read-only for the instance lifetime; construct a new instance after an
authored-map change. Missing/invalid definitions return `IsValid == false`, a
validation code/reason and `InvalidMap` plans, without fallback map dimensions.

- `TryOccupy(cell, owner)` atomically validates and registers one buildable,
  walkable, in-bounds cell; entrance/throne and existing occupants are rejected.
  `OccupancyResult` distinguishes success, invalid owner/map, bounds, masks,
  landmark, duplicate/occupied, empty release and ownership mismatch.
- Pass a non-null, stable reference token unique to each placement lifetime
  (for example a fresh plain `object` held by that tower). `TryRelease(cell,
  owner)` compares reference identity, so an old destruction callback cannot
  release a replacement's cell. Navigation does not infer destruction from
  Unity's overloaded null check; Defense explicitly releases its registration.
- A successful addition/removal increments monotonic `Revision` exactly once;
  rejected/duplicate/empty operations do not change it. Occupancy is instance
  state, never ScriptableObject data. These synchronous operations are atomic
  on Unity's main thread; the API is not a worker-thread synchronization layer.
- `Reset()` clears all occupancy as one transaction: one revision if nonempty,
  none if already empty. It always clears the computed field and advances a run
  generation, invalidating old plans even when occupancy was empty. Lifetime
  diagnostics remain cumulative; a new instance starts them at zero.

### Search, Blocker And Consumer Contract

One reverse Dijkstra field serves all start cells on a revision. Cost compares
occupied cells to enter first, then cardinal step count. Thus any open path wins
over siege, open paths are shortest, and blocked paths minimize required
destructions before distance. The first occupant on that path is returned with
an entirely free, reachable approach. Removing it either opens a route or
advances toward the next blocker. Permanently disconnected authored terrain
returns `Unreachable`, without selecting an irrelevant tower.

An indexed heap and flat arrays are bounded by authored cell count, with
O(V log V) search and O(V) retained field/occupancy storage. There is no per-enemy
search structure or unbounded cache of all start-to-goal path arrays. A valid
first request after any number of occupancy changes builds the field once;
later starts reuse it. `PlanFrom(start)` materializes an immutable route snapshot
in O(path length). `ReplanIfStale(currentCell, previous)` returns the existing
plan by identity in O(1) while it remains current, without route allocation.

Equal-cost routes choose the next cell with lower y, then lower x. The heap uses
the same cell ordering after cost. Occupancy insertion order and object identity
do not affect route/blocker-cell selection. Repeatability applies to identical
authored data and occupancy state; a numeric revision is meaningful only within
its owning navigation instance. Plans retain source identity, revision and run
generation; use `IsCurrent(plan)` instead of comparing revision numbers alone.

`NavigationPlan` exposes `Status`, read-only `Path`, `Revision`, `BlockerCell`,
`BlockerOwner` and `Diagnostic`. Successful paths include the current start;
`ReachesThrone` ends at the authored throne. `ReachableBlocker` ends adjacent to
the blocker and excludes every occupied cell. Explicit failure statuses are
`InvalidMap`, `StartOutsideBounds`, `StartNotWalkable`, `StartOccupied` and
`Unreachable`; failure paths are empty. Occupying an enemy's current cell is a
placement-policy issue for S2-BLD-01: navigation reports `StartOccupied` rather
than inventing escape/teleport movement. Next-cell placement is supported and
covered for multiple moving consumers.

### Integration Handoff And Wiring

No navigation prefab or MonoBehaviour is required: Integration constructs one
plain navigation instance from the composition root's validated serialized map
reference and explicitly passes that same instance to Defense and Waves/AI.
Do not create an instance per enemy. Only Integration edits the Stage 2 scene.

1. Defense stores its fresh ownership token after `TryOccupy == Changed` and
   releases with that token on destruction/removal. Resource spending and allowed
   build phases remain Defense/Economy responsibilities.
2. Each consumer stores its plan and path index. Before movement or blocker
   interaction, call `ReplanIfStale(currentCell, plan)`. When plan identity
   changes, reset path index to 1 (index 0 is the current cell). Advance through
   `Path` without repeatedly calling `PlanFrom`. A retained plan is only for that
   consumer's continuous route; clear it after external relocation.
3. Follow cells via the map's `CellToWorld`/`WorldToCell` contract. On revision
   change, replan from the consumer's committed cell before entering its next
   cell. Continuous movement/placement overlap arbitration belongs to S2-AI-02
   and S2-BLD-01; the included harness advances discrete cells only.
4. At the end of a blocker approach, Waves/AI receives its owner token and cell
   for the future siege/health binding. Recheck plan currency before attack;
   navigation implements neither attacks nor target health.
5. Restart calls `Reset()` and resets consumer positions/path indices/targets;
   old ownership tokens must not be reused for new placements. No authored
   asset, scene reference or Unity project setting is changed by navigation.

### Diagnostics And Reproduction

Profiler markers: `Stage2.Navigation.FullSearch`, `.PlanFrom`, `.Replan`, `.Cache`.
`NavigationDiagnostics` exposes actual `FullSearchCount`, `RequestCount`,
`FieldReuseCount`, `ReplanCount`, `PlanCacheHitCount`, expanded cells, last/total
search milliseconds and last request/replan/cache milliseconds. Measurements use
`Stopwatch`; sub-tick cache observations may be zero. Request timing includes
field construction when needed and immutable route materialization; replan
timing includes that request; cache timing observes the retained-plan fast path.

EditMode and Editor PlayMode measurement tests use the actual asset, three
warmups then 30 samples per open/zigzag/full-wall case. Each sample creates a
fresh instance, computes an initial plan, changes occupancy at remote cell
`(-49,49)`, replans and requests the retained plan again. Recorded search/request/
replan/cache timings belong to this revision change; initialization, layout
construction and test assertions are outside the timed regions. Every sample
asserts exactly two total searches, two replans and one retained-plan cache hit.
This is a synthetic navigation workload, not approved tower counts or balance.

Evidence is emitted to ignored `Artifacts/S2-MAP-02/EditMode-timings.json` and
`PlayMode-timings.json` with raw samples, Unity/OS/CPU/GPU/RAM, UTC, map hash,
path/occupancy/expanded-cell counts. Run the complete EditMode and PlayMode suites
through Unity Test Runner, or the installed optional CLI:
`unity command run_tests --mode editor --async_tests true --json` and
`unity command run_tests --mode playmode --async_tests true --json`; poll
`unity command test_status --json` until completion. No millisecond threshold is
asserted. Windows build qualification remains S2-QA-01; scale/travel-time and
target-hardware budget decisions remain S2-MAP-03/Platform-QA.

### Recorded Measurements — 2026-09-22

Unity 6000.6.2f1 Editor, Windows 11 build 26100, Intel Core i5-12400F (12 logical
processors), 32581 MB RAM, NVIDIA GeForce GTX 1060 6GB. Candidate content hash:
`77A23824A9BF24DA`. Each mode/case has 3 discarded warmups and 30 recorded
samples; p95 below is nearest-rank sample 29 of 30. These are observations on
this workstation, not a target-hardware frame budget or build qualification.

Full-search milliseconds, expressed as median / p95 / maximum:

- EditMode open: 1.0835 / 1.3711 / 1.4496.
- EditMode zigzag: 1.1217 / 1.8070 / 1.9663.
- EditMode full wall: 1.0958 / 1.4078 / 2.4686.
- Editor PlayMode open: 1.0741 / 1.2675 / 1.3042.
- Editor PlayMode zigzag: 1.0448 / 1.4962 / 2.1065.
- Editor PlayMode full wall: 1.1208 / 1.3266 / 1.4642.

Replan milliseconds (including search and route allocation), median / p95 / max:

- EditMode open: 1.0900 / 1.3776 / 1.4627.
- EditMode zigzag: 1.1374 / 1.9845 / 5.0760.
- EditMode full wall: 1.1012 / 1.4160 / 2.4744.
- Editor PlayMode open: 1.0779 / 1.2765 / 1.3148.
- Editor PlayMode zigzag: 1.0547 / 1.5108 / 2.1267.
- Editor PlayMode full wall: 1.1277 / 1.3343 / 1.4720.

Retained-plan cache p95 is 0.0001 ms in all cases; observed maxima are 0.0001 ms
in EditMode and 0.0002 ms in PlayMode. These tiny values approach timer
resolution and exclude external call overhead. All searches expand 10000 cells.
Open/zigzag routes contain 90/386 cells (89/385 steps); full wall has a 49-cell
approach ending adjacent to blocker `(0,0)`. Synthetic layouts contain 0/297/100
occupants before the remote invalidation edit and 1/298/101 during measurement.
Zigzag walls are at x=25/0/-25 with alternating gaps at y=49/-50/49.

Invocation evidence: 64 consumers from different starts move for 128 PlayMode
frames with 1 full search, 64 plan requests, 63 field reuses and 8128 retained
plan hits. In the 24-consumer shared-next-cell placement case, total searches
are 2 (initial plus changed revision), with 48 replans, and every consumer reaches
the throne. Static full-wall waiting causes no repeated search. The seven
navigation PlayMode tests use only a discrete test harness, not enemy prefabs.

Verification: full EditMode 42/42 (Stage 1 8, Map 12, Navigation 22), full
PlayMode 20/20 (Stage 1 4, Integration 9, Navigation 7), no skips. Fresh Editor
restart and subsequent Stage 2 Play entry each report 0 Console errors and
0 warnings, compilation not failed. The expected invalid-map/missing-reference
error logs from Integration negative tests are accounted for by `LogAssert`;
the known stale Input System monitor issue does not recur on the fresh run.
`git diff --check` passes. Scenes, Map, Integration, Stage 1, ProjectSettings and
Packages have no diff. Raw test reports and fresh Console evidence are in
`Artifacts/S2-MAP-02/`; navigation build-level qualification remains S2-QA-01.
