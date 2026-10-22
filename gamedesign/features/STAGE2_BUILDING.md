# Feature: Stage 2 Building Policy

## D118 — green-ground base placement correction

Workshop/dwelling/forge do not reject a free plot due to unbuilt placeholder
workshop/circle sites or already-depleted manual sources. Existing city/auto-
production district, solid footprint, active resources, terrain, hero overlap
and front approach checks remain. Growing a source back inside a newly built
solid is blocked until the solid leaves; its recovery clock is held there.
Cause-specific existing-shell hints distinguish hero overlap, insufficient
materials, wrong terrain and occupied plot/entrance. Preview/confirm use the
same rules. No cost/footprint/catalogue/Inspector migration.


## D-079 active Stage3 tower actions

Stage2Defense now owns paid upgrade, same-type/same-level merge, invested-cost
half-refund sale and preparation-only empty-cell move transactions. Role catalogs
store upgrade rows (empty means legacy base-only); Stage2Tower owns live level,
investments and battle state. See TOWER_UPGRADES.md for current rules. Stage3
Rapid/Longshot/barricade rows are explicitly populated; only active Stage3 Rapid's
legacy gold surcharge is removed. Historical Stage2 catalog prices stay intact.
No scene/Inspector migration is required. Future non-tower upgrades stay outside.

## D-075 target — not implemented

Новые постройки стоят только дерево/камень/металл. Улучшения/исследования
могут дополнительно стоить золото. Новые типы башен требуют приобретённого
и изученного в мастерской исследования. Старые цены ниже — прототип.
Выбор готового здания открывает карточку над HUD по PLAYER_INTERFACE.md;
платный ап, слияние двух башен одного уровня, move и продажа 50% описаны
в TOWER_UPGRADES.md. Башни/частокол реализованы в D-079 выше; будущие
апы прочих зданий остаются отдельными срезами. Пустая клетка переноса
проходит проверки занятости и героев/врагов.

Status: S2-BLD-01 complete, human-accepted 2026-09-24; P-002 resolved by D-014.
S2-DEF-01 implementation complete 2026-09-24; real-scene Integration pending.
Owner/workstream: Defense / Design, current Stage 2 dialogue.
Preset: GPT-6 Astra / high, explicitly selected by the human; 14–26k envelope.
Inputs: D-009, D-013, accepted Stage 1 placement and S2-MAP-02 navigation.
Scope: allowed phases, enemy overlap, paid confirmation/cancellation and route
changes. Runtime placement, combat and scene wiring remain downstream packages.

## Accepted Outcome

Stage 3 D-019 keeps this payment/occupancy policy. The shared build driver now
projects mouse rays onto the XY ground plane, which supports both the old
identity camera and the new fixed inclined camera. The tactical scene replaces
the generic ghost with its actual current tower model/sprite; all approval and spending still
come from the same `Stage2Defense` preview/confirm path.

D-059 (2026-10-16) adds the current input/readability contract: dim and disable
an unaffordable tower card, and reject unfunded role selection/build initiation
through HUD/public actions or existing B/1/2 shortcuts. Keys 1/2 retain their
select-only behavior when affordable. The transparent actual-model ghost and
cell outline are green only for Ready and red otherwise, with no orange/yellow
state. UI hover hides the world ghost. Funds remain unreserved and must still be
rechecked at confirmation, including when the balance changes during placement.
This changes no tower prices, gray-road policy, occupancy or combat rules.

Allow building during preparation and active waves. Reject construction under
a live enemy and across a step it has already started. Allow construction on
the rest of its future route, including a complete wall: enemies then reroute
or siege per D-009. Preview/cancel/failure spends nothing; a successful confirm
buys exactly one tower and changes occupancy once.

## Evidence And Reuse Boundary

- `Stage1GameController.CanBuild` already allows Preparation and Wave.
  `BuildPlacement` previews snapped cells, cancels without spending and checks
  the cell again on confirm. Preserve those behaviors.
- Stage 1 `DynamicNavigationGrid.ValidateBuildCell` checks bounds, authored
  forbidden cells and towers; it does not check moving enemies. Stage 1 enemy
  replanning uses a rounded world cell. Copying this into continuous Stage 2
  movement would leave current-cell and between-cell overlap unspecified.
- Stage 1 `TryPlaceAt` spends before instantiation/registration; its failure
  branch after `RegisterTower` has no refund. This is a reuse limitation from
  code review, not a reproduced Stage 1 gameplay regression. Stage 2 must meet
  the existing all-or-nothing placement contract instead of copying that order.
- `Stage2Navigation.TryOccupy` validates authored masks/landmarks/tower identity,
  not enemy bodies, run phase or inventory. `PlanFrom` explicitly returns
  `StartOccupied` for an occupied start; it supplies no escape or teleport.
- S2-MAP-02 tests cover shared-next-cell rerouting, blocker removal and reset.
  `NavigationTestConsumer` jumps between discrete cells; those passes do not
  establish correctness for an enemy partway along a continuous step.
- The Stage 2 scene is still a map/player/camera shell. No Stage 2 paid
  placement, moving combat enemies or shared economy is wired yet.

## Accepted P-002 Rules (D-014)

### Phases And Input

1. A purchase can commit only during unpaused Preparation or Wave. Starting
   the wave does not cancel an otherwise valid preview or change its price.
   Opening build mode does not pause enemies or the wave timer.
2. Preview never reserves resources or navigation occupancy. Recheck phase,
   selected recipe/full cost, authored cell, resource reservations, tower
   occupancy and current actor exclusion at confirm time. A previously valid
   ghost does not authorize a now-invalid purchase.
3. Cancel clears the preview without spending. Pause, win/loss and restart
   also discard it; resuming requires a fresh placement action. No queued
   confirmation is replayed across pause, terminal state or restart. If cancel
   and confirm are handled together, cancel wins. Do not add new controls.

### Enemy And Hero Overlap

4. A live enemy standing at a cell center protects that cell from construction.
   Before it begins a cardinal move A -> B, movement revalidates navigation
   and protects both A and B for the duration of that step. On arrival at B,
   A is released; B remains protected. A target merely listed in a cached path
   is not protected until movement actually commits to that step.
5. Multiple enemies may share these cells; construction remains rejected
   while any live enemy protects the cell. Death/removal releases that enemy's
   protection once, without releasing another enemy's protection. Restart
   clears all protection before spawning the new run.
6. Protection is a placement constraint supplied by Waves/AI, not a tower
   registration in Stage2Navigation. Enemy movement must not increment tower
   occupancy Revision or turn enemies into siege blockers. Defense reads
   current movement state through a narrow explicit contract; no new global
   manager, path reservation service or enemy collision system is required.
7. Commit of a step and commit of a build are serialized on the main thread.
   If a build commits first at B, an enemy at A replans before entering B.
   If A -> B starts first, a build at either endpoint fails without spending.
   Do not infer the committed cell by rounding a moving transform each frame.
8. When another cell changes mid-step, finish the already-protected step
   continuously, then obtain a current plan at B before the next step or attack.
   At a cell center, replan before any further movement/attack. No snapping,
   teleporting, pushing or damaging the enemy to make room for a tower.
9. The hero is also protected against placement overlapping its ground
   footprint, so construction cannot spawn a blocking tower around the player.
   Use the same ground footprint as hero movement/collision; large decorative
   sprites are not placement blockers. The hero remains non-combat and never
   becomes an enemy navigation occupant or siege target. No new build radius
   or hero traversal rule is decided by this package.

### Cell, Payment And Route Result

10. Respect authored buildability/walkability, map bounds, entrance/throne,
    existing towers and resource locations, including depleted nodes per D-013.
    D-016 additionally permits combat towers only on `InvasionCorridor` cells;
    green `Forest` cells reject preview and confirm without spending. The
    general buildable mask remains available for future civilian buildings.
    Keep the candidate asset read-only. Full blockage of enemy routes is not
    a rejection reason; it triggers the existing deterministic siege contract.
11. Successful confirm makes one usable tower, spends its complete recipe
    once, registers one fresh ownership token and emits one confirmed built
    fact. Failure/cancel creates no tower or built fact, spends nothing and
    makes no lasting occupancy change. Ordinary rejected confirmations leave
    Revision unchanged; success adds exactly one revision. A second handling
    of the same confirmation must not buy a second tower.
12. S2-DEF-01 owns the transaction: prepare fallible creation/bindings before
    publishing success; revalidate at commit and expose no half-built state
    to AI, objectives or UI. Do not publish spend/build events until the whole
    purchase succeeds. Test failed registration/creation; do not silently
    retain the price after a failed build. The concrete rollback mechanism is
    implementation-owned, not a reason to reopen S2-MAP-02's completed API.
13. Tower destruction releases its cell using its original ownership token;
    delayed callbacks cannot release a replacement. A new purchase on that
    cell is possible only after release and the same fresh validation. Preview
    cancellation does not remove or refund an already committed tower; this
    policy adds no selling, moving, repair or post-purchase undo feature.
14. Current plans choose an open route if one exists; otherwise enemies
    approach/attack the returned reachable tower blocker. Recheck plan and
    blocker identity before applying damage. Revision changes alone must not
    grant an attack or indefinitely restart its cooldown. Attack timing belongs
    to S2-AI-02. After destruction, movement resumes via a current plan.

## Alternatives And Tradeoff

Preparation-only building reduces overlap cases but removes the live maze
adjustment already present in the core loop. It is not recommended for this
slice. Allowing placement under enemies would require an additional rule for
escape, displacement or damage and would conflict with StartOccupied handling.

Protecting only the rounded current cell is insufficient between centers:
the source or destination can become occupied while the enemy still traverses
that segment. Protecting the entire future path is unnecessarily restrictive.
Protecting just both endpoints of the active step is the recommended boundary;
the cost is that a visibly empty destination may briefly reject construction
as an enemy approaches. Preview must explain that movement occupies the cell.
Footprints larger than this one-cell/cardinal-movement slice need a separate
review if a later enemy brief actually requires them.

## Rule-Level Scenario Review

- Preview is valid, then an enemy enters or starts entering the cell: confirm
  rejects with no spend/revision/built fact; the ghost reflects the reason.
- Enemy stands at A with a future route through B: building at A rejects;
  building at B succeeds if no step has started and other checks pass. Enemy
  replans from A; the existing discrete-next-cell tests support this case only.
- Enemy is partway A -> B: either endpoint rejects; a free later cell C may
  be built upon. Enemy reaches B without crossing a tower and replans there.
- Several enemies traverse opposite directions or share a destination:
  both endpoints remain unavailable until all relevant protections are gone.
- Full wall, including one erected during Wave: payment may succeed; enemies
  choose a reachable blocker. No open-path requirement or hero target appears.
- Two confirms contend for one cell or the remaining resources: validate each
  against committed state; only a valid complete purchase can spend. Repeated
  handling of one input, cancellation or stale preview cannot duplicate it.
- Pause/terminal/restart intervenes before purchase: reject/discard with no
  spend. Already committed purchases are part of the old run; reset discards
  that run and invalidates its callbacks, per D-013 and navigation generation.
- A blocker dies and is replaced: release by old identity cannot remove the
  new tower; an old attack cannot damage a replacement through a stale target.

These are contract walkthroughs, not runtime passes. S2-DEF-01 must test phase,
preview/confirm, full-cost/no-spend, failure injection, duplicate handling and
actor exclusion. S2-AI-02 must test continuous mid-step placement, shared
protection, death cleanup, reroute/siege/destruction and repeated replans without
cooldown starvation. Integration/QA must demonstrate them on the real map,
including cancellation, terminal/restart and depleted-resource reservations.

## Acceptance And Verification

- [x] Inspected existing placement, movement, navigation, tests and scene state.
- [x] Prepared phase/overlap/payment rules and adversarial scenario review.
- [x] Human accepted P-002 on 2026-09-24; D-014 recorded in DECISIONS.md.
- [x] S2-BLD-01 closed with implementation and live-check handoffs; existing
  DYNAMIC_ENEMY_PATHING.md consumer notes updated to match the accepted rule.

Documentation only; no C#, scene, prefab, map or package change. Retained
EditMode 42/42 and PlayMode 20/20 at `87fdb1b` are historical baseline evidence,
not tests of this unimplemented policy. No new runtime tests/builds are claimed
for this documentation-only decision. Unity 6000.6.2f1 is open; read-only live
Console check on 2026-09-24 reports 0 errors, 0 warnings, no compile failure.
At the S2-BLD-01 close, no runtime package had yet been authorized; the
S2-DEF-01 implementation and later authorization are recorded below.

## S2-DEF-01 Implementation And Handoff (2026-09-24)

`Assets/Stage2/Defense/Runtime/` owns `Stage2Defense` as the main-thread
transaction, `Stage2Tower` as the identity/health blocker, and
`Stage2BuildDriver` for the accepted B, left-click and Escape/right-click
controls. `Assets/Stage2/Defense/Data/Stage2DefensePrototype.asset` holds the
temporary one-tower full three-resource cost and health. The inactive
`Prefabs/Stage2BlockerTower.prefab` reuses the original Stage 1 graybox sprite
read-only. These values and art are test prototypes, not an approved final role
or balance. The scene and shared map/navigation/economy assets were not edited.

- Integration must initialize `Stage2GatheringDriver` first, construct one
  shared `Stage2Navigation` from the same `Stage2MapDefinition`, then call
  `Stage2BuildDriver.Configure(map, defenseCatalog, gatheringDriver, hero,
  camera, navigation)`. The driver gets the exact shared inventory from
  `gatheringDriver.Economy.Inventory`; it turns Economy build mode on/off while
  preview is active. It creates its own tinted preview sprite from the prefab.
- Run state must call `SetRunState(Preparation|Wave|Inactive, paused)` on the
  build driver for every phase/pause transition. Pause/terminal discards preview;
  Preparation -> Wave preserves it. Call `ResetRun()` before clearing Economy,
  then `Stage2Navigation.Reset()` once for prior-run plan invalidation, and
  rebind the new run's enemy-protection callback before Wave building. The
  run-state owner also resets Economy and enemy state. No deferred input is
  replayed after reset.
- Waves/AI must provide `SetEnemyProtection(cell => ...)` using **live** enemy
  state: each living enemy protects its current cell and both endpoints of an
  active step. This callback must be installed after Defense Configure and
  before Wave; absent callback makes Wave placement fail closed. Enemy movement
  never registers in Navigation or changes its tower Revision. The callback
  should read the already-decided active step, not round a moving transform.
- The hero exclusion currently uses `map.WorldToCell(hero.position)` to match
  the Stage 2 shell's point-ground movement. If Integration later gives the
  hero a wider collision footprint, extend the Defense exclusion to those
  intersected cells before claiming the final actor-overlap criterion.
- The confirmed `Built(Stage2Tower)` fact emits once after full payment and
  `TryOccupy`. AI can use `NavigationPlan.BlockerOwner as Stage2Tower` and call
  `TakeDamage`; death releases occupancy immediately by the original object
  identity and a stale release cannot remove a replacement. `ResetRun` clears
  Defense-owned towers and preview; final run coordination is still external.

EditMode 54/54 and PlayMode 24/24 pass, including actual B/click/cancel input,
full-cost failure, late occupancy contention, actor/phase rejection, duplicate
confirm, death/replacement and reset. The two logged errors in full PlayMode
are expected negative Integration validation tests; a cleared Console and
subsequent Stage 2 Play entry show 0 errors/0 warnings/no compile failure.
No Windows build or live-loop observation is claimed by Defense alone. AI's
continuous mid-step protection and siege, Integration wiring, and real-map
play observation remain required before Checkpoint B or Stage 2 gate closes.

## S2-DEF-01-FIX — Baseline Tower Attack (2026-09-24)

After S2-WAV-02, the first integrated wave could only lose: the placed Stage 2
tower was a blocker with health but no enemy-damage behavior. This corrective
package adds one simple nearest-living-enemy attack to that same prototype
tower, preserving its cost, health, occupancy identity and paid transaction.
Range, interval and damage are provisional fields in the existing
`Stage2DefensePrototype.asset`; the Editor builder's **Add Baseline Tower
Attack** migration wrote them while retaining the authored cost and health.
No new tower role or final balance decision is implied.

Defense owns the attack timer and `Stage2Tower.Tick`; Waves/AI's explicit
`Stage2EnemyGroup.TryHitNearest` picks the nearest live enemy within range,
with spawn order breaking exact ties, then applies enemy damage. The run state
connects the callback to Defense, and disables attack/timer progression during
pause and after terminal outcome. There is no scene search, circular assembly
reference, hero target or new global manager. Before the first wave,
Integration constructs the group and Run state using the same Navigation;
the Run state binds the attack callback as part of its constructor.

Verification: EditMode 66/66, PlayMode 29/29. New tests cover nearest target,
fixed fire interval, frozen pause timer and a moving one-wave **natural win**
from a paid tower, without test-injected enemy death. Full PlayMode's two
negative Integration logs and an Editor Input System test-session monitor
error were isolated; after a script reload and cleared Console, Stage 2 Play
entry/exit showed 0 errors/0 warnings/no compile failure. Scene integration,
Windows build and player observation remain outstanding.
