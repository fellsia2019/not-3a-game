# Feature: Stage 2 First Playable / Vertical Slice

Status: active; replanned 2026-09-24. S2-MAP-01, S2-INT-01, S2-MAP-02 and
the S2-ECO-01/S2-BLD-01 decisions and S2-ECO-02/S2-DEF-01/S2-AI-02/
S2-WAV-02 and S2-INT-02 implementations are complete.
S2-MAP-03/S2-WAV-01 measured analyses and S2-AI-01 brief are delivered for
human review after the authorized three-package batch. The human subsequently
delegated the P-004 trial choice and implementation; S2-WAV-01 cadence follow-up
is implemented and awaits human manual checking. P-008, final pacing and new
enemy breadth remain separate decisions.
This file is the Stage 2 task and
dependency contract; stage gates
live in gamedesign/DEVELOPMENT_PLAN.md, ownership and evidence in
production/WORKSTREAMS.md. Accepted D-001–D-014 remain in force.

Owner/workstream: Design coordination for planning; Integration alone owns
the Stage 2 scene and build wiring.
Target: Unity 6000.6.2f1, URP, Input System, Windows x64 Development Build.

## Outcome

One fresh player can gather wood, stone and metal, buy and place defenses,
reshape a wave route, see enemies attack a fully blocked route, and finish or
lose a representative run without coaching. The team learns whether travel,
resource pressure, maze choices, pacing and feedback justify continuing.

Stage 2 is a test of the complete loop at representative quality. The full
MVP ceiling of tower/enemy roles, final elite, map fill and menu polish is
not an automatic Stage 2 checklist. Any added breadth must answer a named
playtest question. Do not add automation, hero combat, tool tiers, tutorial or
new biomes.

## Known Baseline And Reuse

- Stage 1 is human-accepted and remains a regression scene. Its run state,
  gathering, placement, tower/enemy combat and dynamic-siege rules are evidence
  and reuse candidates. The Stage 1 fixed 21x17 assumptions and two-resource
  economy cannot be copied into Stage 2 without adaptation.
- S2-MAP-01 supplies the validated 100x100 authored map candidate. P-008 is
  open; the candidate is not the committed production size.
- S2-INT-01 supplies the clean, runnable scene shell and binding points.
  Only Integration may edit Assets/Scenes/Stage2VerticalSlice.unity or its build
  wiring. Stage 1 scene/assets remain protected.
- S2-MAP-02 supplies navigation/occupancy APIs and deterministic route,
  blocker and reset results. S2-INT-02 now binds its shared instance to the
  Stage 2 scene; Windows integration qualification is pending S2-QA-01.

Before replacing a Stage 1 rule, the task owner records why reuse fails:
changed map bounds, third resource, serialized binding or measured behavior.
Port the smallest rule first. Avoid a new global manager, event bus, DI or
interface layer without a measured need.

## Shared Contracts That Are Ready

- Map definition is read-only at runtime and owns cell bounds, surfaces,
  masks, entrance, throne, resource regions and camera bounds.
- Navigation owns dynamic occupancy, revision and path/blocker queries.
  Defense registers/releases a tower with identity-checked occupancy; enemies
  consume immutable plans and replan after real changes.
- A confirmed gather grants one wood/stone/metal fact. A confirmed placement
  must validate the cell and full cost, spend once, register occupancy once
  and emit one built fact; failed/cancelled placement spends nothing.
- Hero has no attack, health or aggro role. Tower, enemy and throne damage
  stay within the existing loss and siege model.
- UI, VFX and audio show confirmed state/facts; they do not own game rules.

The exact implementation types and hooks remain task-owned. Do not create
interfaces merely because a logical boundary is listed here.

## Execution Order And Task Register

Task IDs are preserved for existing handoffs. A task is ready only when its
input decision and owner are named in WORKSTREAMS.md. Each task closes with
behavior, files/contracts, focused verification and remaining work in one
handoff entry.

### A. Completed Foundation

- S2-MAP-01 — complete: authored map candidate and validation.
- S2-INT-01 — complete: scene shell, player/camera bounds and Windows startup.
- S2-MAP-02 — complete: navigation and occupancy contract with diagnostics.

These results may be read concurrently; no owner should rework them to satisfy
an untested downstream preference.

### B. Reconnect The Loop Before Expanding It

S2-ECO-01 is complete: the human accepted P-003 as D-013 on 2026-09-24.
`STAGE2_ECONOMY.md` contains the accepted lifecycle and conditional soft-lock
review; runtime and live-loop checks remain downstream. S2-BLD-01 is also
complete: the human accepted P-002 as D-014 on 2026-09-24, using GPT-6 Astra / high.
`STAGE2_BUILDING.md` contains the accepted placement rules. S2-ECO-02 is complete:
three-resource gathering/inventory and D-013 recovery have EditMode 47/47 and
PlayMode 22/22. S2-DEF-01 is complete: paid tower placement, three-resource
spend, occupancy and destruction/replacement lifecycle pass EditMode 54/54 and
PlayMode 24/24; Integration scene wiring remains. The human authorized
sequential starts of later ready packages up to the S2-MAP-03 boundary.
S2-AI-02 is complete: continuous enemy movement, live step-cell protection,
siege, blocker identity recheck and post-destruction resume pass EditMode 61/61
and PlayMode 26/26. S2-WAV-02 may consume its run-owned population and throne
damage fact; the scene is still not connected.
S2-WAV-02 is complete: preparation, bounded wave spawning, throne loss,
terminal win/loss and reset pass EditMode 65/65 and PlayMode 28/28. Its
provisional one-wave data is not a P-004 cadence decision. The actual loop
has one baseline Defense tower attack after S2-DEF-01-FIX; a moving wave wins
naturally in PlayMode 29/29, with EditMode 66/66. S2-INT-02 has since bound the
real scene and passed 33/33 PlayMode including natural win/loss, full-wall siege
and restart. S2-QA-01 then passed EditMode 66/66, PlayMode 33/33 and a
Windows x64 Development launch with both Stage 2 startup messages and no
Player errors. P-008/S2-MAP-03 remains the next decision boundary.

1. S2-ECO-01 resolved P-003: node depletion/recovery, restart and resource
   soft-lock policy. S2-BLD-01 resolved P-002: active-wave building and the
   enemy/current-cell placement rule. Both are product decisions informed by
   Stage 1 behavior; neither requires a new UI or final tuning.
2. S2-ECO-02 adapts gathering and inventory to three resources and authored
   map regions. S2-DEF-01 adapts paid placement/build catalog and atomic tower
   occupancy. Their narrow unit and PlayMode checks must include reset and
   cancel/no-spend. Do not require all final tower roles to test placement.
3. S2-AI-02 adapts one usable enemy path/siege loop to S2-MAP-02, including
   mid-wave changes and blocker destruction. S2-WAV-02 provides the minimum
   preparation/wave/throne/win/loss/restart orchestration. Provisional pacing
   values remain data, pending S2-WAV-01.
4. S2-INT-02 integrates these deliveries immediately into the Stage 2 scene,
   one contract at a time. S2-QA-01 runs a checkpoint regression and Windows
   launch after the live loop is connected; it is not postponed until final
   art/audio.

Checkpoint B acceptance: gather each resource; place or cancel without
double-spend; make an open, longer and fully blocked path; see siege reopen it;
finish/lose and restart. Console has no new attributable errors. If the
composition fails, the owning system fixes its contract before more roles,
enemies or art fill are started.

Player build feedback on 2026-09-25 exposed a missing resource-visibility
contract in the integrated Stage 2 scene. Integration's corrective change
draws source markers at the three authored anchors and directional readout
when they are off camera. This correction precedes further role/content work;
the player must verify it in the refreshed Windows build. It does not choose
the final UI stack or change map topology.
The next player check found empty wood/stone markers still visible and reported
metal E gathering inactive. Integration now hides a depleted marker until
recovery/restart and clarifies the terminal-state R restart and in-range hold-E
conditions. The screenshot showed Lost with metal 18 m away, so a fresh active
run check was needed to confirm the player's observed metal behavior. The
input-driven PlayMode check passed and the player accepted the refreshed build
on 2026-09-25; this Integration correction is closed.

### C. Decide Scale, Rhythm And Necessary Variety

Batch review, 2026-09-24: all three decision drafts were prepared serially,
then measured, regression-tested and corrected together per human instruction.
S2-MAP-03 recommends continuing with 100x100/current masks after actual travel,
Editor load and affordable three-tower full-blockage checks. S2-WAV-01 recommends
reworking the one-wave baseline into a three-round diagnostic candidate with
45/30 s preparation; current live runs lasted 65.083 s (defended win) and
219.484 s (unopposed loss). S2-AI-01's two-profile proposal is in
`STAGE2_ENEMY_ROLES.md`. The human then delegated choosing and implementing the
cadence trial. The S2-WAV-01 follow-up adds a finite sequence to the existing
run system; S2-WAV-02 remains complete. Current regression is EditMode 70/70
and PlayMode 34/34. Agent live comparisons have stopped per the human's
cost/scope correction; the 2026-09-25 Windows rebuild succeeded and one human
manual run is pending.
P-008, final representative pacing, enemy breadth and player comprehension
remain open; this does not automatically activate S2-DEF-02.

1. S2-MAP-03 resolves P-008 after measured travel routes, frame time, memory
   and replan cost on the live loop. Record map hash, hardware, build mode and
   repeatable routes. Compare 200x200 only if 100x100 fails the design test.
2. S2-WAV-01 resolves P-004 from an observed full-run cadence: preparation,
   travel, gathering, building and defense. No Markdown balance table becomes
   a runtime source of truth.
3. S2-AI-01 briefs only enemy differences needed to test escalation and maze
   response. S2-DEF-02 adds tower-role differences needed to test an actual
   choice. AI and Defense agree on target/effect facts before implementing
   breadth; the MVP ceiling does not itself set a Stage 2 content quota.
4. S2-OBJ-01 adds the D-008 fixed gather/build objective from confirmed facts,
   with one active optional objective and a one-shot resource reward.

Checkpoint C acceptance: a complete run has no resource soft-lock, has
understandable distance risk and pacing, and gives the player at least one
observable maze or tower decision. P-004/P-008 records state what was measured,
what was selected and what remains adjustable.

Economy evidence for that run records, for each of wood/stone/metal and each
phase, available nodes, gathered amount, build spend, objective reward and
remaining inventory. Check whether the player can fund a plausible recovery
after a lost tower without requiring an optional reward. Change authored
node/cost data only after the P-003 lifecycle and this flow are understood.

### D. Make The Tested Loop Readable

- S2-UI-01 resolves P-006 before final runtime HUD work. S2-UI-02 then shows
  resource counts, wave/throne state, build price/validity, path/block feedback,
  the optional objective and the run result. Keep controls and layout limited
  to the tested run. The conditional UI Toolkit pipeline applies only if
  UI Toolkit is chosen.
- S2-ART-01 can prepare a target frame, silhouette/scale tests and license
  inventory from the accepted vision and map before C is complete. S2-ART-02
  and S2-ART-03 deliver the simplest readable 2D environment,
  actor/tower/enemy and VFX assets after scale and gameplay hooks are stable.
  Detailed 3D/Blender models, rigs and export tooling are outside this gate.
  Art owners do not edit the scene.
- S2-AUD-01 adds distinct critical feedback after the state facts exist.
  Missing optional clips must not break gameplay. Runtime media require
  provenance and rights recorded before integration.
- S2-SAV-01 resolves P-005 before any run persistence is implemented. If Stage 2
  does not require mid-run persistence, assign the run-save
  decision/implementation to Stage 3 explicitly in the gate record.
- S2-INT-02 integrates each accepted delivery; Integration alone saves the
  Stage 2 scene. S2-QA-01 qualifies the resulting build. S2-QA-02 conducts
  the external comprehension playtest and records continue/rework/stop.

Checkpoint D acceptance is the Stage 2 gate in DEVELOPMENT_PLAN.md. A
representative presentation must make the core decision legible; it need not
exhaust the final MVP content list.

## Ownership And Parallel Capacity

- One Integration owner has exclusive write access to the Stage 2 scene and
  ProjectSettings/EditorBuildSettings.asset. System owners deliver code, data,
  prefabs, tests and exact serialized wiring/reset notes.
- Economy and the P-002 building decision may proceed in parallel. Map
  benchmarking can prepare methods early but P-008 waits for a live loop.
  Art brief and UI wireframes can proceed from accepted vision and the map;
  runtime implementation waits for relevant state/contracts.
- Defense and AI may work in parallel only after their shared placement,
  damage and navigation contract is written down. Do not mutate another
  owner's prefab or data asset without a handoff.
- S2-INT-02 and S2-QA-01 are repeated checkpoints with the same owners.
  S2-QA-02 is serial after a qualified, representative build.

## Required Handoff Evidence

For each delivered task: accepted decision or input contract, exact files/data
and serialized references, observed behavior, focused EditMode/PlayMode result
or reason a check is degraded, new attributable Console state, Windows build
status when integration is affected, and explicit remaining work. The handoff
register in WORKSTREAMS.md is the only cross-chat register.

The final Stage 2 gate requires a qualified Windows build, relevant regression,
P-002/P-003/P-004/P-006/P-008 accepted where used, P-005 either decided or
explicitly assigned to Stage 3, recorded asset provenance and an observed
fresh-player run. A failed gate returns to a named owner and does not authorize
extra content or post-MVP systems.
