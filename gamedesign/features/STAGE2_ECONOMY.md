# Feature: Stage 2 Resource Lifecycle

Status: S2-ECO-01 human-accepted and S2-ECO-02 implemented, 2026-09-24;
P-003 resolved by D-013.
Owner/workstream: Economy / Design, current Stage 2 dialogue.
Preset: GPT-6 Astra / high, selected by the human; 14–26k soft plan envelope.
Scope: resource-node lifecycle, recovery policy, runtime gathering, inventory
and authored prototype values. Scene composition remains Integration-owned.

## Outcome

The player can return to a known wood, stone or metal source after exhausting
it and gather again to replace lost defenses. Obtaining resources still costs
travel and active gathering time. Recovery does not require an optional
objective reward, a purchased tool, a building or winning the next wave.
This policy prevents permanent exhaustion; it cannot guarantee survival after
every spending or layout mistake.

## Evidence And Reuse Boundary

- `Assets/Stage1/Runtime/GatherableNode.cs` gives one unit per successful
  gather, decrements a finite stock and disables the object at zero. It has
  no recovery rule. This is the accepted single-wave prototype assumption,
  not a defect to fix in Stage 1.
- `PlayerGatherer.cs` requires range and held interaction, resets incomplete
  progress on interruption/target change and suppresses gathering during
  placement or after run end. Preserve these rules in the Stage 2 adaptation.
- `Stage1Rules.cs` inventory supports only wood/stone. `Stage1GameController.cs`
  restarts by reloading the scene; its optional reward grants stone. Neither
  that reward nor the two-resource inventory proves Stage 2 recovery.
- `Stage2Map100x100.asset` provides three forest resource regions and one
  walkable, non-buildable anchor per resource. All candidate cells are
  authored walkable. `Stage2VerticalSlice.unity` currently binds only the
  map, hero, camera and landmarks; it has no resource nodes or economy.
- Map/navigation and Stage 1 remain unchanged. Stage 2 needs a three-resource
  inventory and lifecycle independent of the Stage 1 controller. A depleted
  node must remain represented in the lifecycle even if its visual is hidden;
  disabling its only timer owner would prevent recovery.

## Accepted P-003 (D-013)

Accepted policy: finite stock per node with automatic full recovery after
a finite interval of active game time, at the same authored location.

1. Each wood/stone/metal node starts full. A completed valid gather consumes
   one stock unit, credits one inventory unit and reports one confirmed gather
   fact. Interrupted, out-of-range or depleted-node attempts grant nothing.
2. The last unit starts that node's recovery interval exactly once. Attempts
   on the depleted node do not restart the interval. Partial depletion does
   not start recovery and does not refill stock.
3. Recovery advances during preparation and active waves. Pause and terminal
   win/loss freeze it. Finishing a wave does not refill nodes or restart their
   timers. No real-world/offline clock or save system is introduced here.
4. At expiry, stock returns to the authored capacity once, in the same place.
   Recovery itself grants no inventory and emits no gather/objective fact.
   Further depletion starts a new independent recovery cycle. Surplus elapsed
   time cannot generate resources in inventory or stack extra capacity.
5. The resource location remains reserved while depleted, so a tower cannot
   replace the source. Use existing reserved anchors for the first integration;
   any additional authored node requires the same placement protection.
   Recovery must not trap the hero inside a reappearing blocking collider.
   Collision/placement wiring is delivered by Economy to Defense/Integration;
   this task does not alter map masks or enemy occupancy.
6. Gathering and recovery need no resource payment, tool tier, objective or
   wave completion. Every required resource has at least one reachable,
   renewable source. A scene/data validation failure must identify a missing
   or unusable source rather than silently substitute an objective reward.
7. A restart restores all nodes to authored capacity, clears recovery timers
   and incomplete gathering, and resets all three inventory balances to zero.
   No resources or delayed grants carry across runs. Run state owns the wider
   reset of towers, navigation, wave/throne state and objectives.
8. Node capacity is a positive integer; gathering and recovery durations are
   positive finite values. Values and distribution live in Unity data when
   implemented. This decision sets no seconds, node counts or tower prices;
   S2-MAP-03/S2-WAV-01 measure travel and pacing before final tuning.

## Alternatives And Tradeoff

Keeping nodes finite for the whole run preserves depletion-driven exploration
and matches Stage 1 most closely, but needs a measured finite reserve against
all planned spending and acceptable losses. That evidence does not exist yet.

Refilling only between waves is simpler to signal, but recovery then depends
on surviving a wave with the remaining defense. It does not meet the accepted
independent recovery route during an active wave.

Timed recovery allows repeat visits without making gathering free. Its main
risk is farming one convenient location or waiting beside an empty source.
The live-loop test must check those behaviors against distance risk. Adjust
authored location, stock and timing from observations; do not assume that
renewability alone proves good pacing.

## Rule-Level Soft-Lock Review

Here, resource soft-lock means a live run can no longer obtain a resource
needed to construct a usable defense because every source is permanently
exhausted or its recovery requires that same unaffordable defense. Losing the
throne while attempting recovery remains a valid defeat.

- **Zero inventory and all sources depleted:** active time alone restores
  stock; manual gathering costs no inventory. No purchase/reward/wave-clear
  prerequisite forms a circular dependency. Repeated finite cycles can fund
  any finite recipe, conditional on accessible sources and a still-live run.
- **Last tower destroyed and no optional reward:** the same recovery path
  remains available. This does not prove that the player can rebuild before
  the throne falls; that must be measured after combat and prices exist.
- **Repeated attempts at an empty node:** neither stock nor inventory changes,
  and recovery is not postponed indefinitely by attempts.
- **Partially depleted node:** the remaining stock is still obtainable; taking
  the last unit activates recovery. Leaving partial stock is not a lock.
- **Pause, run end and restart:** pause cannot farm stock; terminal state
  cannot grant resources; restart restores sources without old callbacks or
  gathering progress crediting the new inventory.
- **Spatial access:** authored anchors support the initial route, but a future
  collider or tower layout must not permanently seal every source of a needed
  resource. Economy/Defense/Integration verify access in the composed scene.

Conclusion: the accepted policy removes a permanent-exhaustion dependency at the
rule level under explicit access/time assumptions. Runtime conformance and
timely recovery on the real map are unverified; no full-run soft-lock pass
or Stage 2 gate is claimed.

## Required Downstream Verification

S2-ECO-02 tests each resource: successful versus interrupted gather,
last-unit depletion, expiry boundary, repeated empty attempts, repeated
cycles, independent node timers, pause/terminal handling and restart from
partial/depleted states. Inventory tests cover full-cost affordability,
failed spend with no partial deduction and zeroed restart. Lifecycle tests
must show that recovery emits no gather fact and no stale action crosses reset.

S2-DEF-01/S2-INT-02 must demonstrate rejection on occupied resource locations
even while depleted, safe reappearance near the hero, reachable interaction
and gathering after all sources have been exhausted, with optional rewards
disabled. Integration alone owns serialized scene wiring.

Checkpoint C records node availability, gathered/spent/reward/remaining amounts
by resource and phase, with map/data identity and measured travel, gathering,
waiting and rebuild times. Use an explicit lost-tower scenario and a usable
replacement recipe. If recovery is available only after inevitable defeat or
waiting dominates, return the observed cause to Economy/Map/Waves for tuning.

## S2-ECO-02 Implementation And Integration Contract

Status: S2-ECO-02 complete, 2026-09-24. Economy owns
`Assets/Stage2/Economy/**`. The authored
`Data/Stage2EconomyPrototype.asset` is the sole source of provisional stock,
interaction range, gather and recovery times for the three resource kinds.
These experiment values are not accepted balance.

`Stage2Economy` validates the shared map and catalog, creates one runtime node
at each authored resource anchor, and owns `Stage2Inventory`.
`AdvanceGather(kind, playerWorld, held, elapsedSeconds, allowed)` enforces
range, held duration, interruption and one-unit/one-fact semantics. `Tick`
advances only when the caller declares active game time. The node continues
timing even when a depleted marker is visually hidden. `Reset` clears gathering
progress, recovery and all balances. `Inventory.CanAfford` and
`TrySpend(wood, stone, metal)` examine the complete price and never partially
deduct. `Gathered(kind, amount)` is a confirmed fact for later objectives/UI;
recovery never raises it. The model has no Stage 1 controller dependency.

2026-09-25 D-018 extension: the current run starts with five gold. Gold is a
currency in the same inventory, not a fourth map node or gatherable resource.
Each confirmed enemy kill credits one gold; throne contact and despawn credit
none. `CanAfford`/`TrySpend` include gold in the same atomic tower payment.
Restart clears earned gold and restores the authored starting five.

Integration S2-INT-02: attach `Stage2GatheringDriver` to its owned scene root,
bind the same `Stage2Map100x100.asset` used by the scene bootstrap, assign
`Stage2EconomyPrototype.asset` and the non-combat player's `Transform`.
The driver owns the existing E hold interaction. It locates nearby nodes by
the map's cell-to-world coordinates; it does not create art or change the map.
Supply simple visible markers at those exact anchors when composing the scene;
hide the entire marker while its node is depleted and restore it on recovery or
restart. Keep the reserved ground cell non-buildable even while its marker is
hidden, and do not enable a blocking collider on top of the hero when it recovers.

2026-09-25 Integration correction: the three runtime markers are now created
from existing scene sprites at the exact authored anchors, without colliders.
After the 2026-09-25 player check, empty markers disappear and return when the
node recovers or the run restarts. The temporary Development Build readout shows
each source's stock, recovery time, distance and WASD direction; a label remains
at the screen edge when the source is outside the camera or covered by the
readout. In range it prompts the player to hold E; after win/loss it prompts R
to restart before gathering. This is discoverability scaffolding until
Temporary Stage 2 markers/readout provide first-playable feedback under D-015.
Finished art and UI follow the playable check.

The run state calls `SetRunActivity(running, paused)` on phase/pause changes,
with `running` true in Preparation/Wave only, and `ResetRun()` at restart.
Defense calls `SetBuildMode` while a placement preview is active. Hold input
must be released after pause/build/restart before a new gather starts. Defense
reads `Economy.Inventory` for all three balances/cost checks and receives
confirmed gather facts from `Gathered`, without modifying Economy-owned files.
Only Integration edits/saves the Stage 2 scene. The first candidate's anchors
are already authored non-buildable; Defense must also honor reservations for
any later added source throughout depletion/recovery.

Focused EditMode 47/47 and PlayMode 22/22 cover model rules, prototype asset
loading and held Input System interaction. A subsequent Stage 2 Play entry had
0 new Console errors/warnings; compile failure flag is false. Initial
compilation found a missing Input System asmdef reference; that reference was
added and the final compilation/tests are clean. Existing Integration
negative tests intentionally log two validation errors before Console clearing.
These checks do not qualify the scene's visual markers,
player travel, tower costs, live run pacing or Windows build; those remain
Integration/Defense/QA/Checkpoint C work.

## Verification And Acceptance

- [x] Existing code, authored regions, scene bindings and stored evidence read.
- [x] Alternatives and rule-level failure scenarios reviewed above.
- [x] Human accepted P-003 on 2026-09-24; D-013 recorded in DECISIONS.md.
- [x] S2-ECO-01 closed with accepted rules and explicit downstream limitations.

S2-ECO-01 was a documentation-only decision. Its historical baseline at
`87fdb1b` was EditMode 42/42 and PlayMode 20/20; those tests did not cover
resource recovery. S2-ECO-02 has since implemented and tested the lifecycle
as described above. Later package authorization is tracked in
DEVELOPMENT_PLAN.md; the human has since authorized sequential ready packages
up to S2-MAP-03.
