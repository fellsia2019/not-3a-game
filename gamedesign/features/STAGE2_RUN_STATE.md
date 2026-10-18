# Feature: Stage 2 Run State And Cadence

Current Stage 3 runtime update — D-065, 2026-10-18: the optional ContinuousDays
catalog maps Preparation/Wave to day/night and continues with growing hordes.
The normal remote Space control is disabled for that scene; local dwelling rest
uses the same StartWave boundary as timer expiry. Legacy finite catalogs retain
the baseline below. Current contract/verification: `DAY_NIGHT.md`.
The D-062 rewind target below remains unimplemented, including for night defeat.

## Current Product Target — D-062, 2026-10-17

Owner/workstream: Game Design / Run state integration. The owner clarified the
large-world survival/TD concept and approved the retry rule below. This section
is the current target; the Stage 2 implementation and verification below remain
the historical finite-run baseline. The target rewind is **not implemented**.

### Accepted player behavior

- Red portal is the enemy entrance; blue throne is the defended objective.
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
- Every subsequent wave launch establishes its own current pre-launch snapshot,
  including another attempt at the same wave after preparing differently.
  Repeated defeats can be retried without an attempt limit.

The extra preparation is intentional. Resetting the failed future prevents
keeping its rewards or duplicating stock; the player can use each new
preparation to change the defense. How this affects difficulty requires live
play rather than an unapproved retry penalty or resource cap.

### Existing surfaces and integration work

`Stage2Run.StartWave()` currently owns both start paths but stores no snapshot.
`Stage2Run.ResetRun()` clears Defense/Navigation/Economy and selects wave one;
`Stage2RunDriver.ResetRun()` also resets gathering and hero position.
`Stage2ProductionDriver` currently clears production on the normal Reset event.
`Stage2GameHud.DrawOutcome()` offers «Новый забег [R]» through the same full reset.
These existing entry points do not satisfy «Повторить» and must not be relabeled
without implementing coherent restoration of their consumers.

Dependencies: Run state, Defense/Navigation occupancy, Economy/resource recovery,
hero/gathering lifecycle, production buildings and collision, combat cleanup,
objective presentation and HUD. Preserve scene/asset references and current
authored gameplay data. The snapshot's storage shape is an implementation choice
to resolve when that work begins; no disk save system or huge-world architecture
is selected by this concept pass. Full reset can remain an explicitly separate
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
  attributable Console errors. No such runtime verification is claimed yet.

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
