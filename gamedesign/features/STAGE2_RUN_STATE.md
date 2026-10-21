# Current Stage3 cycle and recovery override — D-074/D-087/D-088

Active implementation2026-10-19: Stage3 opts into daytime gates in HordeRun.
D-087 replaces its full-world loss snapshot: keep all current development,
restore throne, clear failed threats and return morning for the pending tier.
Living-creep snapshots are superseded by that explicit owner correction.
D-088 Sleep until19:50 is blocked during combat; no automatic dawn on victory.
The older full-snapshot description below applies to legacy Stage2 fixtures.
Current clock/gate contract: PORTAL_CONTROL.md. Qualification is in the plan.

D105 retains LastClearedTier after retry. Daytime copies its night-profile
interval and strength, not its count. It has no total quota/live cap and ends
only on close/cutoff. D106 supersedes finite Stage3 nights:600s day/180s night,
spawn until dawn, then wait for all living threats/shots. Stage2 finite catalogs
retain their quota. Old serialized day interval/cap remain compatibility fields only.

D-094 adds exactly three paid throne reinforcements of+5current/max HP. Run owns
count/health/payment; prices live in HordeRun. D-087 keeps the CURRENT count and
spending, then restores the effective maximum on retry. Full Reset clears count.
Legacy finite checkpoints capture/restore count alongside health and inventory;
old catalogs with no upgrade array retain their original fixed HP. See OBJECTIVES.md.

# Feature: Stage 2 Run State And Cadence

## D-075 campaign/base-growth target — not implemented

Campaign victory requires three distinct defeated bosses; ordinary night
clearance is not final victory. See BOSSES_AND_VICTORY.md. The boss venue,
hero death/retry and simultaneous final-boss/throne outcome need an explicit
rule before integration. Existing finite scenes retain their historical end.

D-079 runtime snapshots include tower type/level/health/cooldown/cell and actual
investments; paid upgrades, merges, moves and sales rewind together with inventory.
Selection and drag are transient and clear on retry. This extension is verified
by a real-scene failed-wave retry test.

Future snapshots must also include item/tool instances, forge/workshop/generator
stages, purchased/studied new research and boss flags/rewards once implemented.
Throne upgrades are now covered by D-094 above. The other future systems in this
historical snapshot proposal are not supported by the finite D-062 runtime yet.

## New cadence/checkpoint target — D-074, not implemented

PORTAL_CONTROL.md defines the proposed daytime farm stream beside mandatory
night hordes and the accepted removal of both clock skips. HERO_PROGRESSION.md
and EQUIPMENT_AND_RUNES.md define future state that joins the snapshot.
Night defeat retains D-062 full preparation; daytime-stream defeat needs its
own agreed checkpoint/time rule. The proposed rule restores the pre-open world
and actual opening time, not a free full day.

Living farm enemies and their attacks may precede the night launch or next
opening. The implementation below assumes no prior living enemy population;
it cannot erase those actors as failed-future cleanup. Before enabling farming,
extend capture/restore for those actors and reward/random state, XP, professions,
equipment and learned runes. Preserve pre-checkpoint rewards, discard failed
future rewards, and prevent duplicate grants or reward rerolls through defeat.
This is a documented dependency, not delivered support for daytime checkpoints.

Current Stage 3 runtime update — D-065, 2026-10-18: the optional ContinuousDays
catalog maps Preparation/Wave to day/night and continues with growing hordes.
The normal remote Space control is disabled for that scene; local dwelling rest
uses the same StartWave boundary as timer expiry. Legacy finite catalogs retain
the baseline below. Current contract/verification: `DAY_NIGHT.md`.
The D-062 rewind implementation below is active, including night defeat.

## Implemented Night Retry Contract — D-062, 2026-10-17

Owner/workstream: Game Design / Run state integration. The owner clarified the
large-world survival/TD concept and approved the retry rule below. This section
is the current target; the Stage 2 implementation and verification below remain
the historical finite-run baseline. Runtime retry is implemented and locally
qualified on macOS/Metal, 2026-10-19; Windows qualification remains open.

### Accepted player behavior

- Red portal is the enemy entrance; blue throne is the defended objective.
- Owner visual clarification, 2026-10-19: every actual HP loss emits a brief
  white radial fracture pulse from the dome's soul when it is in view, plus a
  small short screen shake. Nonfatal fractures fade completely; accumulated
  damage scales the next pulse, never permanent screen obstruction. Offscreen
  damage shakes the view without fractures at a false onscreen origin. Logical
  camera/follow/picking remain unchanged. Fatal whiteout works from anywhere.
  Owner approved the ImageGen storyboard with these changes. Copy: «Ещё одна
  попытка», «Вернитесь к подготовке и измените исход», «Повторить волну».
- When the throne is destroyed, light rays emerge from it and intensify until
  the screen is fully white. A readable «Повторить» action then permits retry.
  Destruction must not leave the normal world simulation running under the
  presentation. The transition must finish while gameplay is stopped.
- Capture the state immediately before each actual wave launch, before its first
  enemy or shot. Manual early start and automatic timer expiry use the same
  checkpoint boundary. This is not the beginning of the preceding preparation.
- Retry restores that snapshot: the same generated world, hero position,
  inventory, resource stock/recovery, tower placement/health and objective
  health, and production buildings/progress. It removes the failed attempt's
  enemies, projectiles, effects and temporary actions. It must not combine
  resources, kills or construction from the failed future with restored state.
- Return to Preparation for the **failed wave**, with its full configured
  preparation duration. The old remaining timer and manual early-start time do
  not reduce it. Objective and tower health come from the snapshot, not a new
  full-run initialization. The restored world supports ordinary gathering,
  building and the next manual/timed launch.
- Continuing-day retry begins at the checkpoint day's normal morning with a
  full preparation; world/resource/building state comes from immediately before
  launch. Clock rewind grants no production or recovery ticks.
- Include dwelling, workshop position, teleport research and discovery, mineral
  stock stages, and hero/tower cooldowns. Cancel incomplete gathering, placement
  and failed-future effects; require fresh E release before further interaction.
- Every subsequent wave launch establishes its own current pre-launch snapshot,
  including another attempt at the same wave after preparing differently.
  Repeated defeats can be retried without an attempt limit.

The extra preparation is intentional. Resetting the failed future prevents
keeping its rewards or duplicating stock; the player can use each new
preparation to change the defense. How this affects difficulty requires live
play rather than an unapproved retry penalty or resource cap.

### Existing surfaces and integration work

`Stage2Run.StartWave()` owns both start paths. The active implementation captures
Economy/Defense and notifies narrow hero/production/dwelling/travel owner handlers
before the first spawn. `RetryFailedWave()` is distinct from full reset; the
driver gates R and the button until the white transition/UI is ready.
`Stage2Run.ResetRun()` clears Defense/Navigation/Economy and selects wave one;
`Stage2RunDriver.ResetRun()` also resets gathering and hero position.
`Stage2ProductionDriver` currently clears production on the normal Reset event.
The old «Новый забег [R]» loss path is replaced by coherent retry; the won-run
and development reset actions retain full reset explicitly.
Full reset stays a separate development operation. World regeneration
invalidates an older checkpoint instead of applying it to a different world.

Dependencies: Run state, Defense/Navigation occupancy, Economy/resource recovery,
hero/gathering lifecycle, production buildings and collision, combat cleanup,
objective presentation and HUD. Preserve scene/asset references and current
authored gameplay data. The snapshot stays in memory in the existing plain C# owners; no disk save
system or huge-world architecture is introduced. Full reset can remain an explicitly separate
development operation; it must not be silently substituted for loss retry.

### Observable acceptance for implementation

- A manual early launch and a launch after timer expiry both retry the failed
  wave, including a wave after the first, with its full authored preparation.
- The restored world matches the pre-launch state. Damage, destroyed/bought
  towers, resource spending/gains and production changes during the failed
  attempt are undone together; restored buildings and occupied cells agree.
- Production/gathering/recovery freeze during defeat and resume from restored
  progress, without duplicate income, old attacks or held-input replay.
- A second failure/retry works after changing the defense in the new preparation;
  progression to the following wave creates the next checkpoint correctly.
- Actual rays-to-white presentation completes with gameplay stopped and leaves
  a working, visible «Повторить» action. The retry clears the defeat transition.
- Relevant EditMode/PlayMode checks and scene wiring are verified with no new
  attributable Console errors.

Qualification, 2026-10-19: 60/60 scoped EditMode; 46/46 existing regression
PlayMode plus final 4/4 cross-system retry/presentation cases (50 unique cases),
including real arrivals after an intentionally stalled 800 ms frame. New hit and
fatal clocks do not inherit pre-event frame time. Actual Mac Development/Metal
1600×900 player confirms temporary visible-dome pulses, stronger accumulated
damage, offscreen shake only, fatal whiteout from both views and repeated R
retry; a native pointer click also restores the checkpoint. This is disposable
QA setup, not a natural full-day playthrough or balance acceptance. Existing
scene/prefab/package references are unchanged; TacticalPresentation binds the
presenter and Resources/Stage3DefeatSettings provides authored shader/timings,
with no Inspector migration. Full record: art/source/defeat_rewind_2026-10-19_verification.json.
Windows module is unavailable on the current Mac; Stage 3 gate stays open.

## Historical Implemented Baseline

Status: S2-WAV-02 complete and wired by S2-INT-02; S2-QA-01 qualified the
Windows baseline on 2026-09-24. The human subsequently delegated the cadence
trial choice and implementation under S2-WAV-01/P-004; the multi-wave follow-up
is implemented with EditMode/PlayMode passing. Further agent live comparisons
were stopped after the human's cost/scope correction; manual game checking is
handed to the human. S2-WAV-02 remains a completed baseline package.
Under D-015 the three-wave trial is enough for the first playable if a full
run is understandable. The former 15–25-minute target is historical and does
not block this gate; detailed pacing/balance waits for live play.
Owner/workstream: Waves/AI / Run state.
Historical preset: GPT-6 Sol / high. Inputs: S2-AI-02, S2-DEF-01, S2-ECO-02,
S2-MAP-02, D-009/D-013/D-014 and accepted Stage 1 run semantics.

## Player Outcome And Boundary

A preparation timer leads into each wave in a finite sequence, which the player
may start early with Space. The throne loses health only from confirmed enemy
arrivals. Clearing an intermediate wave enters the next Preparation; surviving
the final wave wins. Zero throne health loses. R
restarts after a terminal result. Building remains available during unpaused
Preparation and Wave, and a phase transition to Wave preserves its preview.
Pause remains an explicit run-state call. D-052 now supplies a development
menu that combines this call with zero scaled time, so the hero and presentation
also freeze; F3 opens that menu without pausing by itself.

This is a diagnostic run to test repeated recovery, not final P-004 pacing.
Prototype values live in `Assets/Stage2/RunState/Data/Stage2RunPrototype.asset`.
The initial trial has three increasing groups of the existing baseline enemy;
the separate enemy-role proposal is not required for cadence verification.
This package does not own tower firing, side objective, HUD, final content
breadth or scene wiring. The subsequently completed S2-DEF-01-FIX supplies
one baseline attack and a natural PlayMode win. S2-DEF-02 still owns
differentiated roles later.

## Runtime Contract

`Assets/Stage2/RunState/Runtime/Stage2Run.cs` is the plain state owner. It
tracks `Preparation`, `Wave`, `Won`, `Lost`, pause, remaining preparation,
zero-based `WaveIndex`, one-based `WaveNumber`, `WaveCount`, `IsFinalWave`,
`WaveEnemyCount`, spawn completion and throne health. `StartWave()` is idempotent and refused
outside unpaused Preparation. The first enemy spawns at wave start; remaining
enemies use that wave's data interval. Empty active population completes a wave
only after all its spawns finish and the throne survives. Intermediate completion
advances the index and loads the next preparation without resetting health,
towers, inventory, navigation, gather progress or source recovery. Final
completion wins. A last enemy's throne contact applies
damage before removal can resolve the wave, so zero health loses rather than
winning in a tie. Terminal transitions occur once and cancel pending gather
and preview. Loss removes the remaining group without waiting a frame.

`ResetRun()` clears enemy protection, Defense-owned towers/preview, Navigation
occupancy and plan generation, Economy inventory/node stock/recovery/progress,
then restores the first wave, its preparation and full throne health. It cannot replay an old input or
death callback. `Stage2EnemyGroup.ResetRun()` removes group subscriptions
before despawning, including if called inside a throne-contact callback.

`Stage2RunDriver` is the narrow Input System/lifecycle binding. Integration
must initialize Economy and Defense first; build one shared Navigation and
EnemyGroup from the same map; bind
`buildDriver.SetEnemyProtection(group.IsCellProtected)`; then call
`runDriver.Configure(runCatalog, gatheringDriver, buildDriver, hero,
sharedNavigation, group)`. The driver forwards state to Economy and Defense
immediately, uses Space for early start and R for terminal restart, and restores
the hero to its configured initial world position on restart. A future pause UI
calls `runDriver.SetPaused(bool)`; neither pause nor terminal allows resource
recovery. The driver exposes `Run.Changed` and read-only phase/health/counts for
HUD without duplicating rules. Only Integration edits the scene.

### Catalog Compatibility And Scene Binding

`Stage2RunCatalog.waves` is an optional serialized array of
`Stage2WaveDefinition` values (preparation, enemy count, spawn interval).
Every populated entry is validated. An absent/empty array uses the retained
`preparationSeconds`, `enemyCount`, `spawnInterval` fields as a single legacy
wave. `Configure(...)` retains that behavior; `ConfigureWaves(...)` copies a
finite sequence. Existing first-wave accessors remain compatible.

The existing prototype asset now authors the sequence; its GUID and the scene's
catalog reference stay valid. No Inspector rewiring or scene rebuild is needed.
The builder creates the same trial for a missing asset and never overwrites an
existing one. Authored timing and counts must be changed in the asset rather
than duplicated as balance tables in documentation.
The existing development readout now shows the current/total wave and group
count through these facts. Final HUD design remains S2-UI-02.

## Verification And Remaining Work

EditMode 65/65 and PlayMode 28/28 pass with no failures or skips. Tests cover
timer/Space start, bounded spawning, last-enemy win, throne-loss priority,
pause freeze, immediate preview cancellation, terminal once, R restart and
reset of towers, actors, three resources and prior-run plans. The full PlayMode
suite emits two expected negative Integration validation errors. In this Editor
session an Input System monitor error appeared after stopping a test-created
Play session; a fresh script-domain recompile, cleared Console and subsequent
Stage 2 Play entry/exit reported 0 errors, 0 warnings and no compile failure.
These package-local counts are historical. S2-INT-02 now wires the delivered
components; S2-QA-01 passed EditMode 66/66 and PlayMode 33/33 plus Windows
startup. No human-observed full run is implied. P-004 remains for measured
full-run cadence.

## S2-WAV-01 — Preparation, Recovery And Run Cadence

Status: analysis and consolidated measurement complete, 2026-09-24; the human
then delegated choosing and implementing the trial. Runtime changes below
implement the recovery contract; final P-004 pacing still needs live evidence
and external observation. The following rationale describes the measured baseline.

The original one-wave run could not show the approved recover -> repeat rhythm.
Its catalog and state machine had no next-wave index or inter-wave transition.
Simply multiplying its preparation timer would extend waiting rather than
demonstrate repeated layout decisions. The 15–25 minute vision target remains
an acceptance target, not an observed duration of this prototype.

Working recommendation for P-004: reject the single-wave baseline as final
cadence; retain early start with Space; use a finite sequence of Preparation ->
Wave -> Recovery/next Preparation, with victory only after the last wave and
loss taking precedence at zero throne health. Surviving towers, stock,
inventory and node recovery persist between waves; only whole-run restart
resets them. Do not add an automatic resource reward or repair to conceal an
unaffordable recovery. D-013 timers continue in all active phases and freeze
on pause/terminal. Recovery may use the existing Preparation state with a new
wave index rather than introducing an unnecessary extra runtime state.

Before choosing exact preparation lengths and wave count, compare a live
defended early-start run with an unopposed timed-start run. Record travel,
active gathering, building, idle preparation, wave time, outcome and each
resource's gathered/spent/remaining amounts. Include a lost-tower funding
case without objective rewards. Timer duration must leave a useful resource
circuit and build decision while making early start meaningful. Report how
much measured duration is actual activity versus avoidable waiting.

The next cadence candidate should add only enough rounds to observe first
construction, recovery/reconfiguration and a changed threat; the MVP ceiling
does not mandate five waves. Exact counts and timing become a data-backed
implementation under the human's delegated trial authorization, while the affordable maze
choices are carried through multiple rounds. If that candidate is shorter than 15–25 minutes, record
the mismatch for the final pacing decision; do not silently change the vision
or pad it with idle countdowns.

Trial implementation: Waves/AI extends the run catalog with an explicit
finite wave sequence and compatible migration of the current one-wave asset;
Run state exposes current/final-wave facts; Integration binds the resulting
data; UI shows the next threat and remaining preparation. Required checks:
no premature victory between waves, no inventory/tower reset at transition,
pause/resume and early-start idempotence, final-enemy loss priority, exhausted
node recovery across transitions, and clean whole-run restart.

### Measured Full Runs And Concrete Next Trial

Reproduction and mode/hardware limits are recorded in STAGE2_MAP.md and
`production/measurements/README.md`; raw data is local in
`Artifacts/S2-DecisionBatch/`. Each policy was replayed once at normal game
time through live movement, gathering and run updates, with known source
coordinates. It is not a fresh-player playtest.

The defended early-start policy spent 29.489 seconds on preparation and
35.594 seconds in the wave, winning at 65.083 seconds with full throne health
and all three paid towers surviving. In Preparation it gathered 3 wood,
3 stone and 3 metal and spent all 3 of each on the towers; inventory ended
at zero for each resource. No reward was granted. During Wave it gathered
and spent zero. The outcome does not require a large player-built maze.

The unopposed automatic-start policy lost at 219.484 seconds: 180.000 seconds
of preparation and 39.484 seconds of wave. It gathered/spent/received zero
of all resources in both phases; all inventories remained zero. Idle
preparation accounted for about 82% of that run. There is no recovery round
in either historical policy because that baseline state machine had only one wave.

Disposition: **rework cadence; do not accept the current single-wave data as
P-004.** A concrete next diagnostic candidate is three rounds, covering initial
construction, recovery/reconfiguration, then a changed siege threat. Propose
45 seconds for the first preparation and 30 seconds for subsequent preparation,
with Space still allowing early start. The first allowance leaves roughly
15.5 seconds beyond the observed 29.5-second scripted first circuit/build;
the subsequent allowance is a trial hypothesis, not a demonstrated human
recovery budget. The human subsequently authorized this trial by delegation;
the authoritative runtime values are now in the catalog asset.

2026-09-25 D-018 supersedes the preparation candidates above: all three
authored waves now wait 180 seconds. The initial D-018 request changed only
the first wave; the player then extended the same wait to the second and third.
Space still starts a wave early. The model reads each entry of the authored
wave list, so the legacy fallback field alone cannot set these durations.

This proposed three-round trial is for finding the rhythm, not a claim to
meet the unchanged 15–25 minute vision. Exact pressure/group composition and
final round count cannot be accepted from one easy 65-second win. With the
measured affordable layouts, measure the whole sequence and decide whether more decisions
justify longer duration; do not infer a 15-minute run by multiplying idle
timers. Final P-004 stays open pending representative cadence evidence.

No lost-tower funding event was observed in those original live policies. Renewable
nodes remove permanent depletion but do not guarantee enough time to recover
before throne loss. The next trial must destroy a paid tower, fund replacement
from reachable renewable stock without optional rewards, and record remaining
time/health. The delegated follow-up below tests this specific recovery case;
it does not by itself establish recovery under every possible layout or loss.

### Delegated Cadence Follow-Up — 2026-09-24

Authorization: after reviewing the trial proposal, the human instructed
"делай как считаешь правильно и лучше всего". Design/Waves chose a bounded
implementation under S2-WAV-01/P-004. S2-WAV-02 remains complete; its run state
is extended rather than repeated as a new package.

The trial isolates repeated preparation/recovery with increasing groups of the
existing enemy. A new siege role remains a separate content decision. The
three-wave sequence is a diagnostic baseline; neither its duration nor a
successful automated win accepts the final pacing or proves meaningful choices.

Verification so far: EditMode 70/70, PlayMode 34/34, no failures/skips. Added
coverage checks legacy catalog behavior, invalid sequence entries, intermediate
spawn completion, final-only victory, final-contact loss priority, preserved
paid tower/health/inventory/navigation/node recovery, pause, Space and R.
Integration runs all authored waves with real moving enemies and paid towers.
The development-readout wave counter was checked by fresh scene entry and a
screenshot; the clipped control hint was then split into two lines. The Windows
Development Build succeeded on 2026-09-25 (`S2_INT_BUILD_OK`); the updated
Player has not yet been manually launched. Completed raw probes
are in `Artifacts/S2-Cadence/`; they are not human observations or final pacing
acceptance. The human requested focused automatic checks and a manual game
check at task/batch completion; repeated agent live comparisons have stopped.
