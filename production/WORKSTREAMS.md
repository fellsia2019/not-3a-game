# Workstreams And Chat Handoffs

Этот файл — единая координация отдельных чатов. Один активный владелец на
систему/сцену/контракт; параллельные агенты не редактируют одни и те же файлы.

## Current Resource Order

### Active package — S2-DEF-02 (2026-09-25)

- Owner: Defense, `Assets/Stage2/Defense/**` and
  `gamedesign/features/STAGE2_TOWER_ROLES.md`. Integration retains exclusive
  ownership of the Stage 2 scene and its composition/HUD files.
- Inputs: completed S2-DEF-01/DEF-01-FIX, D-009/D-014 and the delivered
  S2-AI-01 brief. Human selected GPT-6 Sol / high.
- Acceptance: two paid one-cell tower roles use distinct catalog data and
  prefabs; role selection changes the preview and price without spending;
  focused EditMode/PlayMode checks cover reach, damage, purchase, pause and
  reset. Scene wiring, build and player choice observation follow as separate
  Integration/QA checkpoints.

### 2026-09-25 — Defense S2-DEF-02 -> Integration/QA

- Behavior: keys 1/2 select Rapid/Longshot; B opens placement; selected role
  controls ghost silhouette, full-price validation, instantiated prefab and
  attack profile. Both roles retain one-cell occupancy, siege and reset.
- Contracts/files: `Assets/Stage2/Defense/Runtime/Stage2Defense.cs`,
  `Stage2BuildDriver.cs`, `Stage2DefenseCatalog.cs`, two catalog assets,
  two inactive prefabs and focused Defense tests. Existing baseline asset
  retains its GUID and references. See `STAGE2_TOWER_ROLES.md` for the exact
  `Configure` overload and data paths; Integration owns scene/HUD changes.
- Verification: Unity 6000.6.2f1 isolated-copy Defense EditMode 10/10,
  PlayMode 3/3, no failures/skips, compile errors or new exceptions in final
  focused logs. A cold import initially resolved pre-existing data assets as
  null; copying the current Editor import cache made both suites pass.
  `git diff --check` clean. No scene or Windows build claim for this delivery.
- Remaining: Integration binds Longshot and shows role/price, then QA checks
  Editor/Windows and the player tests whether the choice changes a real-map
  layout. P-004/P-008 and S2-AI-01 product acceptance remain separate.

Human verification preference, 2026-09-24: keep agent verification focused on
changed logic and necessary regression. The human can perform one manual game
check at the end of a task or task batch. Do not spend further agent turns on
repeated live placement/timing comparisons unless a concrete defect or an
explicit measurement request requires them. Present a short manual checklist.

Stage 1 is complete. For Stage 2, reserve the scarce Integration/scene slot
for a running loop before polishing content. The next work is:

1. S2-ECO-01 closed P-003 as D-013; S2-BLD-01 closed P-002 as D-014.
   S2-ECO-02, S2-DEF-01, S2-AI-02, S2-WAV-02, S2-DEF-01-FIX and S2-INT-02 are complete.
   The human subsequently authorized S2-MAP-03, S2-WAV-01 and S2-AI-01 as a
   serial decision batch, with checks/corrections after all three drafts.
   Their analyses/brief are delivered. The human delegated the next cadence
   choice and implementation on 2026-09-24 ("делай как считаешь правильно и
   лучше всего"). S2-WAV-01/P-004 now owns one diagnostic cadence follow-up;
   S2-WAV-02 remains complete. Map and enemy-role proposals remain separate.
2. Economy supplies three-resource gathering/inventory; Defense supplies paid
   placement/occupancy; Waves/AI supplies the Stage 2 enemy and run loop.
   Reuse accepted Stage 1 rules where they survive the new map contract.
3. Integration connects each delivery to the scene; Platform/QA checks that
   checkpoint before the next content expansion.
4. Design + Platform/QA use the live loop to decide map size P-008 and pacing
   P-004. UI/UX and Art/content may prepare wireframes, scale and provenance
   concurrently, but final assets follow the measured scale and state hooks.
5. Defense/AI add only variation needed for a meaningful choice; UI/UX,
   Art/content and Audio make the tested loop legible with simple 2D assets.
   QA qualifies a full run, then Design conducts the external comprehension
   gate. Detailed Blender models are deferred by D-012.

The confirmed capacity is one developer: keep one implementation package
active at a time, with brief decision/document review only while builds or
external reviews are waiting. If capacity changes later, independently owned
surfaces may overlap after a new WIP decision. This is not a requirement to
create agents or chats. A human product decision cannot be replaced by a
model's guess.

## Stage 2 Ownership Rules

Canonical task register and readiness order are in
`gamedesign/features/STAGE2_VERTICAL_SLICE.md`.

- **Integration — единственный владелец
  `Assets/Scenes/Stage2VerticalSlice.unity`.** Только этот owner создаёт,
  редактирует, сохраняет и меняет build wiring Stage 2 scene. Map, Economy,
  Defense, Waves/AI, UI/UX, Art/Audio и Platform/QA передают ему prefabs, data,
  tests и wiring notes; они не открывают сцену для сохранения.
- Stage 1 scene/assets остаются regression baseline и не мигрируются скрыто.
- Параллельная работа разрешена только для task IDs с готовыми входными
  контрактами и непересекающимися surfaces. Активный owner до начала добавляет в
  этот файл task ID, owned files/contracts, dependencies и acceptance.
- Task-specific feature spec создаётся при фактической активации задачи, а не
  заранее. Integration возвращает дефект владельцу системы вместо
  opportunistic изменения чужого runtime/prefab surface.
- S2-INT-02 и S2-QA-01 выполняются на каждом подключённом checkpoint, а
  S2-QA-02 — после квалифицированной representative сборки. Только принятый
  внешний playtest переводит Stage 2 в complete.

## Model And Token Routing For Project Tasks

These are planning presets for the currently available Codex GPT-6 family,
not a guarantee of actual consumption or account availability. In the Codex
model picker, medium/high/xhigh/ultra mean the user's средний/высокий/очень
высокий/ультра. Official OpenAI API docs describe effort through max; the
Codex UI's ultra option should be treated as an exceptional extra setting,
not as a documented API-equivalent preset.

- **Inventory, status, formatting and narrow documentation updates:** GPT-6
  Luna medium. Supply only the relevant decision, section and diff. Working
  envelope: 5–12k input-context tokens and 1–2k visible result tokens.
- **Ready isolated Unity C# tasks, focused tests and straightforward UI/data
  wiring:** GPT-6 Sol medium; raise to high for lifecycle/reset/serialization
  or tricky PlayMode failures. Working envelope: 12–30k input and 2–8k visible
  output, depending on code volume.
- **P-002/P-003/P-004/P-008 product tradeoffs and Stage 2 gate review:**
  GPT-6 Astra high. Use xhigh when evidence conflicts across design, code and
  playtests. Working envelope: 20–45k input and 2–6k visible decision memo.
  The human accepts player-facing rules in DECISIONS.md.
- **Navigation, economy/build transaction boundaries, scene integration and
  performance regression:** GPT-6 Sol high for a bounded owner surface;
  Astra xhigh for cross-system failure analysis with competing explanations.
  Working envelope: 20–50k input and 3–8k visible report/code summary.
- **MVP art and UI:** Luna medium for provenance/inventory or narrow reference
  extraction; Sol medium for silhouettes, mockup briefs and simple 2D/Unity
  import tasks. Raise to Sol high only for a concrete readability or technical
  integration problem. Do not allocate Astra/xhigh/ultra to routine MVP asset
  polishing. Use the appropriate visual or Unity tools for actual assets.
- **Post-MVP Blender work:** after a separate art-direction decision and
  scoped asset brief, GPT-6 Astra high can lead difficult modeling/editing,
  rigging, export and review tasks using the available Blender workflow.
  Try xhigh only for a measured unresolved blocker.
  Allocate tokens per approved asset, not a standing full-project art thread.
- **UX comprehension and release-critical review:** Sol high for a scoped
  brief; Astra high/xhigh when several accepted rules or a costly irreversible
  choice interact.
- **Ultra:** reserve for a specific unresolved, high-cost decision after a
  focused high/xhigh attempt has failed or produced conflicting evidence.
  State the question and expected evidence first; do not use ultra for file
  reading, routine coding or a broad repo dump.

DEVELOPMENT_PLAN.md now assigns a model, effort and cumulative token envelope
to every remaining Stage 2 task and every Stage 3–7 work package. Use that
per-ID allocation at activation; the bands above are per focused Codex run,
not an additional task or stage budget. Completed S2-MAP-01/S2-INT-01/
S2-MAP-02 receive no forward allocation. Escalate model/effort only when the
acceptance evidence shows a concrete unresolved problem, then record the
reason and actual usage in the handoff.

Envelopes cover planned input context, including selected tool output, and
visible deliverables; they do not estimate hidden reasoning tokens, caching
effects or a Codex subscription quota.
They are initial soft limits, not task estimates or API prices. Track real
usage where the product exposes it; after three comparable tasks, use measured
completion quality, retries, latency and usage to adjust the preset. When
context exceeds the envelope, narrow files and make a handoff summary before
raising effort. Switching to a stronger model never substitutes for a clear
feature brief or a Unity/playtest check.

The routing follows the [OpenAI model selection guide](https://developers.openai.com/api/docs/guides/model-selection),
which places Luna on scoped high-volume work, Sol on everyday coding/judgment
and Astra on ambiguous deep work. The [reasoning guide](https://developers.openai.com/api/docs/guides/reasoning)
explains the quality/latency/token tradeoff and recommends xhigh only when
evaluation shows a benefit. These API guides inform the heuristic; Codex task
usage and model access may differ.

## Generic Prompt For A New Chat

```text
Прочитай AGENTS.md, gamedesign/GAME_VISION.md,
gamedesign/DEVELOPMENT_PLAN.md, production/DECISIONS.md и этот файл.
Ты владеешь workstream <NAME> и задачей <TASK>. Сначала изучи проект и
соответствующий feature spec; не придумывай player-facing правила. Зафиксируй
goal, scope, contracts, acceptance, модель/effort и soft token envelope из
DEVELOPMENT_PLAN.md, а также unresolved decisions. После готовности
реализуй минимальный coherent slice, проверь его и обнови canonical plan/spec и
Handoff Register. Не изменяй поверхности других активных workstreams без
явного handoff.
```

## Active Stage 2 Claims

### S2-WAV-01 / P-004 — diagnostic cadence follow-up

- Owner: Design/Waves in this dialogue; recommended GPT-6 Astra / high for
  S2-WAV-01. The human delegated choosing and implementing the trial after
  reviewing the three-wave, early-start and persistent-state proposal.
- Surface: existing RunState runtime/catalog/tests; Integration regression
  verifies the existing scene binding. Its existing development readout also
  displays current/total wave and group count; no scene rebuild is required.
- Inputs: completed S2-WAV-02, S2-INT-02, S2-QA-01 and measured one-wave runs.
- Acceptance: finite wave sequence, recovery preserving world/resources/throne
  health, final-only victory, loss priority, pause/early-start/restart, a live
  paid-tower replacement circuit, regression and current Windows build.
- Status: implementation delivered; EditMode 70/70 and PlayMode 34/34 passed.
  Windows Development Build succeeded 2026-09-25 with `S2_INT_BUILD_OK`;
  Player/manual run remains outstanding. Live probes ended after the human
  objected to excessive measurement. Final pacing and external-player gate
  stay open.

### S2-MAP-03 -> S2-WAV-01 -> S2-AI-01 — authorized decision batch

- Status: analyses/brief delivered and consolidated verification complete.
  Decision drafts were delivered for review. The later delegated cadence
  follow-up above supersedes this batch's original implementation pause for
  P-004 only; map acceptance and enemy breadth remain separate.
- Human instruction, 2026-09-24: prepare these three packages serially;
  run consolidated checks/tests/corrections only after all three deliveries
  are drafted. This supersedes the prior stop before S2-MAP-03. It does not
  authorize subsequent implementation packages or declare P-008/P-004 accepted.
- Routing: S2-MAP-03 Design/Platform/Map, GPT-6 Astra / high; S2-WAV-01
  Design/Waves, GPT-6 Astra / high; S2-AI-01 Design/Waves, GPT-6 Astra / high.
  Model/effort remain selected by the human in this dialogue.
- Owned surfaces: existing map/run/pathing/vertical-slice specs, a bounded
  enemy-difference brief, canonical plan/decision proposals/handoffs; after
  all three drafts, reproducible Editor-only measurement tooling and evidence.
  No authored scene, map, economy, enemy, tower or run tuning changes are
  implicit in these decision packages.
- Inputs: S2-INT-02/S2-QA-01 integrated baseline, D-009 through D-014,
  current prototype catalogs and S2-MAP-02 diagnostics.
- Acceptance: distinguish observed live timings from geometric estimates,
  report frame/memory/replan evidence with hardware/mode, propose P-008 and
  P-004 conclusions without invented human observations, and provide a minimal
  enemy brief with testable layout choices and downstream ownership. Final
  decision acceptance stays explicit; drafts are not stage closure.

### S2-QA-01 — Platform/QA / integrated checkpoint

- Status: integration checkpoint complete under the human's sequential
  authorization, 2026-09-24; recurs after future integrated deliveries.
  Plan recommends GPT-6 Sol / high, 38–70k soft allocation; UI model choice
  remains with the human.
- Owned surface: regression/build commands, build output, QA evidence and
  canonical status/handoff only. Integration retains sole ownership of the
  Stage 2 scene/build wiring; system runtime/data remain read-only unless a
  concrete defect is returned to its owner.
- Inputs: complete S2-INT-02 real-map scene, S2-ECO-02/S2-DEF-01(-FIX)/
  S2-AI-02/S2-WAV-02 and existing Stage 1 regression.
- Observable acceptance: complete EditMode/PlayMode suites pass; fresh Editor
  scene entry has no attributable Console errors; Windows x64 Development
  Build succeeds and its executable launches without attributable errors.

### S2-INT-02 — Integration / Stage 2 scene

- Status: complete under the human's sequential authorization, 2026-09-24.
  Plan recommends GPT-6 Sol / high, 50–90k soft allocation; UI model choice
  remains with the human.
- Owned surface: `Assets/Stage2/Integration/**`,
  `Assets/Scenes/Stage2VerticalSlice.unity` and its saved Inspector wiring,
  existing Stage 2 Integration tests plus package status/handoff. Only this
  owner edits/saves the scene. Stage 1, Map, Navigation, Economy, Defense,
  WavesAI and RunState runtime/data remain read-only inputs.
- Inputs: S2-MAP-01/02, S2-INT-01 shell, S2-ECO-02, S2-DEF-01 plus baseline
  attack correction, S2-AI-02 and S2-WAV-02; accepted D-013/D-014. P-004/P-008
  remain measurement decisions, not scene assumptions.
- Observable acceptance: the existing authored map/player/camera scene binds
  exactly one shared Navigation, Economy, Defense, enemy group and Run state
  with the supplied catalogs/prefabs. PlayMode on that scene proves gather,
  paid build, route/siege, throne loss, natural win and restart without missing
  references or attributable Console errors. Inspector wiring and handoff are
  explicit; Windows Development Build belongs to S2-QA-01.

### S2-DEF-01-FIX — Defense baseline attack

- Status: complete, corrective follow-up under the human's sequential
  authorization, 2026-09-24. Parent S2-DEF-01 recommends GPT-6 Sol / high.
- Owned surface: narrow extension of `Assets/Stage2/Defense/**` tower combat
  data/runtime/tests and explicit target callback in `Assets/Stage2/WavesAI/`
  and `Assets/Stage2/RunState/`; existing scene remains Integration-owned.
- Inputs: S2-DEF-01 tower/placement, S2-AI-02 live enemies, S2-WAV-02 win
  contract, Stage 1 one-tower attack behavior. No new tower role or final
  balance choice is authorized.
- Observable acceptance: one placed prototype tower can select and damage a
  nearby living enemy at a stable interval, not hit the hero, respect pause,
  and enable a natural one-wave win without test-injected enemy death. Prior
  placement/siege/reset tests remain passing, with clean Console and handoff.
- Exit: EditMode 66/66 and PlayMode 29/29, no failures/skips; a new moving
  wave test wins from the paid tower's own fire. Prototype combat data was
  migrated in place with cost/health preserved. After script reload and
  clearing test-session logs, Stage 2 Play entry/exit had 0 errors/0 warnings
  and no compile failure. Scene/Windows build still belongs to Integration/QA.

### S2-WAV-02 — Waves/AI / Run state

- Status: complete under the human's sequential authorization, 2026-09-24.
  Plan recommends GPT-6 Sol / high, 24–44k soft allocation; UI model choice
  remains with the human.
- Owned surface: `Assets/Stage2/RunState/**` plain run rules, narrow input
  driver, provisional data and tests, task implementation notes in
  `gamedesign/features/STAGE2_VERTICAL_SLICE.md`, status/handoff. The Stage 2
  scene/build settings remain Integration-owned; Economy, Defense, Navigation,
  Map and Stage 1 are inputs. A narrow WavesAI reset fix was needed to make
  terminal cleanup safe during throne-contact callbacks.
- Inputs: S2-AI-02 live enemy group/throne fact; S2-DEF-01 phase/reset contract;
  S2-ECO-02 inventory/recovery/reset; S2-MAP-02 plan generation; Stage 1
  preparation/wave/win/loss/restart semantics. P-004 pacing remains undecided;
  prototype values stay in one Unity data asset.
- Observable acceptance: preparation timer or accepted Space action starts a
  bounded one-wave prototype; spawning/clear and throne damage yield exactly
  one terminal win or loss; pause freezes time/actors/economy and discards
  preview; restart clears enemies, towers, inventory, node timers, plans and
  input latches, restoring preparation without prior-run callbacks. Focused
  EditMode/PlayMode checks plus clean Console and Integration wiring handoff.
- Exit: EditMode 65/65, PlayMode 28/28, no failures/skips. Two full-suite
  negative Integration validation logs identified. An Editor Input System
  monitor error occurred after stopping a test-created Play session; a script
  domain recompile, cleared Console and subsequent Stage 2 Play entry/exit
  reported 0 errors/0 warnings/no compile failure. No scene/build edits.
  Missing Stage 2 tower damage is a concrete Integration blocker; Defense
  owns the narrow correction before a live win/build claim.

### S2-AI-02 — Waves/AI

- Status: complete under the human's sequential authorization, 2026-09-24.
  Plan recommends GPT-6 Sol / high, 32–58k soft allocation; UI model choice
  remains with the human.
- Owned surface: `Assets/Stage2/WavesAI/**` enemy runtime, provisional data,
  prefab and tests, `gamedesign/features/DYNAMIC_ENEMY_PATHING.md` Stage 2
  implementation notes, package status/handoff. Defense, Navigation, Map,
  Economy, Stage 1 and Integration scene are read-only inputs.
- Inputs: D-009 deterministic route/siege, D-014 active-step placement rule,
  S2-MAP-02 shared navigation API and S2-DEF-01 tower identity/health plus
  `SetEnemyProtection` callback.
- Observable acceptance: a live enemy moves continuously from explicit cell
  to cell using current plans; an active step protects both endpoints without
  Navigation Revision changes; mid-step edits finish the step then replan;
  invalidated blocker identity cannot be damaged; reachable blocker is attacked
  at a stable interval and death opens a route; multiple enemies and reset
  release protections. Focused/full EditMode and PlayMode checks, clean Console
  and exact handoff to S2-WAV-02/Integration.
- Exit: EditMode 61/61, PlayMode 26/26, no failures/skips; full PlayMode's two
  negative Integration logs identified. Cleared Console and subsequent Stage 2
  Play entry/exit: 0 errors/0 warnings, compile successful. AI data/prefab/
  tests and wiring notes delivered; no scene or build settings edits.

### S2-DEF-01 — Defense

- Status: complete, human authorized 2026-09-24. Plan recommends GPT-6 Sol /
  high, 32–58k soft allocation; model selection remains in the user's UI.
- Owned surface: `Assets/Stage2/Defense/**` runtime, prototype data/prefab,
  tests and metas; implementation notes in `gamedesign/features/STAGE2_BUILDING.md`,
  package status here and in DEVELOPMENT_PLAN.md/STAGE2_VERTICAL_SLICE.md.
  Integration alone owns the Stage 2 scene; Map, Navigation, Economy and
  Stage 1 surfaces are read-only.
- Inputs: D-014 paid active-wave placement; D-013 shared three-resource
  inventory; S2-ECO-02 `Stage2Inventory`; S2-MAP-02 identity-checked occupancy;
  Stage 2 map masks and anchors.
- Observable acceptance: one valid confirmation yields one usable tower, one
  full spend, one occupancy revision and one built fact; preview/cancel/failure
  leaves no payment or lasting occupancy. Phase, hero and current enemy step
  protect placement; destruction releases by original identity, replacement
  survives stale callbacks, and reset discards preview/towers. Focused and
  full EditMode/PlayMode checks plus clean Console; explicit Integration/AI
  wiring handoff. Sequential subsequent ready packages are authorized up to
  the S2-MAP-03 boundary.
- Exit: EditMode 54/54, PlayMode 24/24, no failures/skips; the two PlayMode
  Console errors are expected negative Integration tests. Cleared Console and
  Stage 2 Play entry: 0 errors/0 warnings, compilation successful. Scene
  remains clean and Integration-owned. Windows build/live-loop evidence awaits
  scene wiring.

### S2-ECO-02 — Economy

- Status: complete, 2026-09-24; human authorized this package. Plan recommends
  GPT-6 Sol / high, 28–50k soft allocation; model selection remains in the
  user's UI. No later implementation package is authorized.
- Owned surface: `Assets/Stage2/Economy/**` runtime, authored prototype data,
  tests and metas; S2-ECO-02 contract in `gamedesign/features/STAGE2_ECONOMY.md`,
  current status in DEVELOPMENT_PLAN.md and STAGE2_VERTICAL_SLICE.md, this
  claim and handoff. Stage 2 scene/build settings, Stage 1, Map, Navigation,
  Packages and other workstream assets remain read-only.
- Inputs: human-accepted D-013 node lifecycle, completed S2-MAP-01 resource
  regions/anchors, Stage 1 held-interaction semantics, D-014 placement boundary.
- Observable acceptance: each of wood/stone/metal can be gathered only in
  range after a completed interaction; depletion and finite active-time
  recovery work without rewards; inventory adds and atomically spends all three
  resources; pause/terminal freeze and restart reset timers/progress/balances;
  relevant focused and full EditMode/PlayMode regression plus clean Console.
  Data and exact Integration wiring notes are delivered without scene edits.
- Entry: main `87fdb1b`; pre-existing documentation/ignore changes preserved.
  Assets/Packages/ProjectSettings are clean. Unity 6000.6.2f1 Editor connected,
  Console 0 errors/0 warnings, no compilation failure; retained historical
  EditMode 42/42 and PlayMode 20/20.
- Exit: EditMode 47/47, PlayMode 22/22, no skips or failures; clean subsequent
  Stage 2 Play entry and Console 0 errors/0 warnings, no compile failure.
  Economy source/data/metas are delivered; scene/build wiring remains with
  Integration. This was the S2-ECO-02 exit state; S2-DEF-01 has since completed.

### S2-BLD-01 — Defense / Design

- Status: complete, human-accepted 2026-09-24; P-002 resolved by D-014.
  Human selected GPT-6 Astra / high (original plan envelope 14–26k).
  This was the S2-BLD-01 exit state; later authorization is recorded above.
- Owned surface: `gamedesign/features/STAGE2_BUILDING.md` placement-policy
  brief, P-002/D-014 record, current package status in DEVELOPMENT_PLAN.md
  and STAGE2_VERTICAL_SLICE.md, accepted consumer notes in
  DYNAMIC_ENEMY_PATHING.md, GAME_VISION.md, this claim/handoff register. No runtime, scene,
  prefab, map, navigation, package or build-wiring changes are owned here.
- Inputs: D-009 route/siege, D-013 renewable/reserved resources, accepted Stage 1
  placement/enemy code and S2-MAP-02 occupancy/plan API and discrete consumers.
- Acceptance: human accepts allowed build phases, current-enemy/continuous-step
  exclusion, future-path placement, full-block siege and cancel/failure without
  spending; rule-level scenarios cover stale preview, repeated confirm,
  phase transitions, destruction/replacement and restart. Implementation and
  continuous-movement tests remain S2-DEF-01/S2-AI-02/Integration work.
- Entry: `main` at `87fdb1b`; existing documentation/ignore changes and accepted
  S2-ECO-01 spec preserved. No Assets/Packages/ProjectSettings diff. Unity
  6000.6.2f1 is now open on this project; stored baseline still reports
  EditMode 42/42 and PlayMode 20/20. Current Console checked separately below.

### S2-ECO-01 — Economy / Design

- Status: complete, human-accepted 2026-09-24; P-003 resolved as D-013.
  Human selected GPT-6 Astra / high; original plan envelope 14–26k.
  Decision surface released for handoff. S2-BLD-01 was subsequently completed;
  later packages required separate approval at this handoff; the human has
  since authorized sequential ready packages up to S2-MAP-03.
- Owned surface: `gamedesign/features/STAGE2_ECONOMY.md` resource lifecycle
  brief, P-003 in `production/DECISIONS.md`, S2-ECO-01 status in
  `gamedesign/DEVELOPMENT_PLAN.md` and `STAGE2_VERTICAL_SLICE.md`, this register.
- Inputs: accepted Stage 1 gather/inventory/restart behavior, D-002/D-006/D-008,
  completed S2-MAP-01 resource regions/anchors and S2-INT-01 scene shell.
  Runtime, tests, data, scenes, packages and build wiring are read-only here.
- Observable acceptance: human accepts depletion/recovery/restart rules;
  rule-level review covers zero inventory, all nodes depleted, lost defense,
  absent optional rewards, pause/terminal states and restart. Distinguish
  permanent resource starvation from a survivable recovery window, which
  requires later live-loop evidence. Exact runtime values remain data-owned.
- Entry verification: `main` at `87fdb1b` matches the local `origin/main` ref;
  no remote fetch claimed. Seven pre-existing modified documentation/ignore
  files are preserved; Assets/Packages/ProjectSettings have no Git diff.
  Stored reports at that commit: EditMode 42/42, PlayMode 20/20, no skips;
  fresh-Play Console snapshot has 0 errors/0 warnings. Unity is currently
  closed; these are historical results, not a new run. Existing Windows build
  qualifies S2-INT-01's shell only. `git diff --check` passes at entry.

### S2-MAP-02 — Foundation / Waves-AI Navigation

- Status: complete, 2026-09-22; surface released for the handoffs below.
  Claimed against clean `main` at `f0d151b`, equal
  to freshly fetched `origin/main`. Editor preflight: 0 errors/0 warnings;
  retained baseline reports: EditMode 20/20, PlayMode 13/13.
- Owned surface: `Assets/Stage2/Navigation/**` (runtime, diagnostics, navigation
  EditMode/PlayMode tests and fixtures, required assembly boundaries and metas),
  Stage 2 navigation section in `DYNAMIC_ENEMY_PATHING.md`, S2-MAP-02 status in
  `STAGE2_VERTICAL_SLICE.md`, current `DEVELOPMENT_PLAN.md` status, this register.
- Dependencies: completed S2-MAP-01 read-only map definition/candidate asset,
  S2-INT-01 composition contracts, accepted Stage 1 dynamic pathing. Map,
  Integration, both scenes, Stage 1, ProjectSettings and Packages stay read-only.
- Observable acceptance: candidate 100x100 open/longer-zigzag/stable reachable
  blocker routes respect authored masks; identity-checked atomic occupancy and
  exact revision/no-op semantics; destruction/reset/replanning recover; simulated
  consumers share unchanged-state searches; measured search/cache/replan counts
  and timings with hardware/map hash; full EditMode/PlayMode regression and
  fresh-domain Console pass. No scene wiring, combat, P-008 or Stage 2 gate change.

### S2-MAP-01 — Foundation / Map

- Status: complete, 2026-09-21; surface released for the handoffs below.
- Owned surface: `Assets/Stage2/Map/**`, its EditMode tests, S2-MAP-01 status in
  `gamedesign/features/STAGE2_VERTICAL_SLICE.md`, current Stage 2 status in
  `gamedesign/DEVELOPMENT_PLAN.md`, and this claim/handoff register.
- Dependencies: D-003 and D-009–D-011; accepted Stage 1 `IsoGrid`, dynamic
  pathing, scene-builder and test contracts. Stage 2 scene/build wiring remains
  exclusively owned by Integration.
- Acceptance: one authored 100x100 map asset exposes cell bounds, green/gray
  surfaces, walkable/buildable masks, one entrance, one throne, wood/stone/metal
  placement regions and camera bounds; deterministic validation rejects invalid
  bounds, missing/out-of-bounds landmarks, invalid regions/camera bounds and
  incompatible masks; isometric cell/world round-trip and deterministic reads
  pass EditMode tests without changing Stage 1 or either scene.

### S2-INT-01 — Integration

- Status: complete, 2026-09-22; claimed 2026-09-21. Handoff ready below;
  further Stage 2 scene edits remain exclusively owned by Integration.
- Owned surface: `Assets/Scenes/Stage2VerticalSlice.unity` and its `.meta`,
  `Assets/Stage2/Integration/**`, Stage 2 integration PlayMode tests,
  `ProjectSettings/EditorBuildSettings.asset`, S2-INT-01 status in
  `gamedesign/features/STAGE2_VERTICAL_SLICE.md`, current Stage 2 status in
  `gamedesign/DEVELOPMENT_PLAN.md`, and this claim/handoff register.
- Dependencies: completed S2-MAP-01 and its read-only
  `Stage2Map100x100.asset`; accepted Stage 1 movement, camera, isometric grid,
  scene-builder and regression-test contracts. `Assets/Stage2/Map/**` and the
  Stage 1 scene/assets remain read-only inputs.
- Acceptance: a builder-authored Stage 2 scene explicitly binds and validates
  the 100x100 map, renders distinct forest/corridor surfaces and readable
  entrance/throne landmarks, moves a non-combat WASD player inside `CellBounds`,
  follows it with a fixed orthographic camera inside `CameraBounds`, reports
  missing/invalid map references with code/reason, preserves Stage 1 regression,
  and is the first enabled Windows Development Build scene.

## Handoff Entry Format

```md
### YYYY-MM-DD — <source> -> <target/status>
- Outcome: <что теперь работает>.
- Contract/files: <что изменено и на что можно опираться>.
- Verification: <tests/build/manual; degraded reason if any>.
- Remaining: <явно вне завершённой задачи>.
```

## Handoff Register

### 2026-09-25 — Integration S2-INT-02 depleted markers and metal input -> accepted

- Behavior: wood, stone and metal markers disappear at zero stock and return
  after active-time recovery or run restart. The Development Build shows the
  remaining recovery time, prompts hold E in range, and explains that gathering
  requires R restart after win/loss. The reported screenshot showed Lost and
  metal 18 m away; active-run held E at the metal anchor gathers successfully.
- Contracts/files: `Stage2GameComposition.cs`, Integration PlayMode tests,
  `STAGE2_ECONOMY.md`, vertical-slice spec and canonical plan. Economy rules,
  map asset and scene wiring are unchanged.
- Verification: focused Integration PlayMode 15/15, including input-driven metal
  gathering and all three marker lifecycles. Windows Development Build logged
  `S2_INT_BUILD_OK` (188083804 bytes); refreshed Integration.dll timestamp
  2026-09-25 06:38 UTC. The relaunched Player is responsive and logged both
  Stage 2 startup messages with no matching Player error/exception.
  `git diff --check` passed. Unity CLI reported its five-second main-thread
  timeout while Unity completed the build; the existing optional Pipeline
  warning remains. The player checked the relaunched build and confirmed
  "все гуд" on 2026-09-25.
- Remaining: Stage 2 external playtest and gate evidence remain open.

### 2026-09-25 — Integration S2-INT-02 resource visibility correction -> human check

- Behavior: after the user saw no resource points in the current 100x100
  Development Build, Integration added visible, nonblocking wood/stone/metal
  markers at the existing Economy anchors. The temporary HUD now shows stock,
  distance and WASD direction, plus a screen-edge label for hidden sources.
- Contracts/files: `Stage2GameComposition.cs`, its PlayMode integration test,
  `STAGE2_ECONOMY.md`, vertical-slice spec and canonical plan. Runtime markers
  reuse project scene sprites; scene/map/economy data and GUIDs stay unchanged.
- Verification: PlayMode 35/35; one Editor screenshot confirms directions and
  offscreen labels. Windows Development Build logged `S2_INT_BUILD_OK`
  (188082452 bytes); updated Integration.dll timestamp 2026-09-25 06:29 UTC.
  The refreshed Player opened a responsive window and logged both
  `S2_INT_STARTUP_OK` and `S2_INT_GAMEPLAY_READY` with unchanged map hash
  `77A23824A9BF24DA`, no matching Player error/exception. `git diff --check`
  passed. Unity CLI itself reported its known five-second main-thread timeout
  while Unity completed the build; optional Pipeline warning remains.
- Remaining: human check of source readability and gameplay in the opened
  Player; final S2-ART-02/S2-UI-02 visuals and external Stage 2 gate.

### 2026-09-24 — S2-WAV-01 cadence implementation -> human manual check

- Behavior: finite waves, persistent world/inventory/health/recovery between
  waves, final-only win and full-run restart. Legacy one-wave catalog fallback
  is preserved; the existing development readout displays wave number/count.
- Files/contracts: RunState catalog/runtime/builder/tests, prototype data and
  Integration regression/readout. The saved scene requires no rewiring.
- Verification: EditMode 70/70, PlayMode 34/34. Local `Artifacts/S2-Cadence/`
  holds completed near-throne and entrance probe reports plus an excluded
  reused-session input failure. Repeated live comparisons exceeded the useful
  scope of this implementation; future manual game checks belong to the human
  at task/batch completion per their instruction. On 2026-09-25 the clipped
  control-help line was split; a new Windows Development Build logged
  `S2_INT_BUILD_OK` (188079016 bytes), and the Integration/RunState Player
  assemblies carry the new build timestamp. Unity CLI returned a five-second
  main-thread timeout error while the Editor continued and completed the
  build; the other three warnings match the earlier optional Pipeline/hidden
  shader build warnings. `git diff --check` passed.
- Remaining: human launch and one manual run of the new Windows build; final
  pacing and external-player gate. S2-WAV-02 remains complete.

### 2026-09-24 — Human map direction -> Design/Map future revision

- Outcome: current focus remains proving the playable core. Rectangular
  expansion, an offset throne with deeper resource territory, a winding gray
  area and possible gray-only tower placement are tentative future options.
- Contracts/files: clarification in `STAGE2_MAP.md`, linked from P-008 in
  `DECISIONS.md` and Checkpoint C in `DEVELOPMENT_PLAN.md`. Current scene,
  runtime, masks and accepted flat-map/Blender scope are unchanged.
- Verification: documentation consistency and diff whitespace check; no new
  runtime behavior, so Unity tests are not repeated for this clarification.
- Remaining: human acceptance of pending slice proposals; any future map
  revision needs its own concrete scope and checks when activated. This note
  does not accept cadence/enemy roles or close the Stage 2 gate.

### 2026-09-24 — Design/Platform/Map S2-MAP-03 -> human P-008 review

- Outcome: recommends retaining 100x100/current masks for Stage 2, with no
  200x200 or compulsory topology rebuild. Actual WASD source travel is
  1.588/4.654/9.221 s. Three paid entrance-neighbor towers cause full siege;
  the earlier transverse-wall-only rework hypothesis was withdrawn after
  checking compact layouts. Human comprehension and entrance-camping balance
  remain unproven.
- Contracts/files: expanded `STAGE2_MAP.md`; `production/measurements/`
  supplies reproducible, scene-unsaved Editor probes and a standard-library
  summary script. Evidence is local in `Artifacts/S2-DecisionBatch/`.
  Authored map hash stays `77A23824A9BF24DA`; scene/data/runtime unchanged.
- Verification: three warmups + 30 samples for eight layouts; search medians
  1.018–1.044 ms, p95 <=1.450 ms. Recorded i5-12400F/GTX1060/Windows11
  Editor Play, 1101x506; 64 movers + 1 Hz revisions: sampled frame p95
  2.351 ms, Editor allocated-memory median 839.47 MiB. This is not Player
  GPU/RAM qualification; the Mono allocation counter was unavailable.
- Remaining: accept/revise P-008 explicitly. The proposal is a Stage 2 size
  selection, not a permanent MVP lock; external player/layout observation
  and representative-content performance remain later gates.

### 2026-09-24 — Design/Waves S2-WAV-01 -> human P-004 review

- Outcome: measured full live policies: 29.489 s preparation + 35.594 s wave
  wins at 65.083 s; no-action automatic start loses at 219.484 s, including
  180 s idle preparation. Defended preparation gathers/spends 3 of each
  resource, buys 3 towers, leaves zero inventory and full throne health;
  no wave gathering/spend or objective rewards. Recommend rework into a
  three-round diagnostic candidate, 45 s first/30 s later preparation,
  early start and surviving run state preserved between rounds.
- Contracts/files: updated `STAGE2_RUN_STATE.md`, canonical plan/spec and
  pending P-004 entry. Current one-wave catalog and runtime are unchanged.
- Verification: actual WASD/E and normal run updates at timeScale 1; one
  replay per policy. Build transactions use the real API with fixed one-second
  spacing, not human build timing. No fresh-player observation claimed.
- Remaining: human cadence approval plus implementation/trial. Lost-tower
  recovery under pressure and the unchanged 15–25 minute target remain
  unproven; do not close P-004 or Checkpoint C from the one-wave measurements.

### 2026-09-24 — Design/Waves S2-AI-01 -> human scope / Defense / Integration

- Outcome: delivered `STAGE2_ENEMY_ROLES.md`: baseline ground enemy plus
  one siege specialist, identical routing/hero exclusion and initially only
  siege output differing. The observable choice is open detour versus full
  blockage; no third type, boss or new damage abstraction is needed yet.
- Contracts/files: brief specifies immutable spawn profiles, existing
  tower damage/identity release, wave-owned composition, required silhouette
  and next-wave cue, negative cases and rejection criteria. New profiles
  and scene references were not implemented.
- Verification: a paid three-tower entrance cap, with tower fire suppressed,
  breaches in 5.0 vs 2.5 simulated seconds under a temporary double-siege
  catalog copy; both reopen a throne route. Consolidated post-draft regression:
  EditMode 66/66, PlayMode 33/33, no skips. Live probes and a subsequent fresh
  Play session with advancing frames ended with 0 Console errors/warnings.
  The input device and callbacks were removed, background execution restored,
  and scene plus five authored data assets matched the recorded SHA256 values.
  A first probe-script compile attempt was corrected for
  eval_file statement syntax; no project C# compiler failure occurred.
- Remaining: human acceptance of the proposed breadth; live comparative
  wave/layout and external readability tests. Stage 2 remains active.
  S2-DEF-02 and later packages have not been started by this batch.

### 2026-09-24 — Platform/QA S2-QA-01 -> Design/Map S2-MAP-03 boundary

- Outcome: the first integrated Stage 2 checkpoint is qualified in the
  Editor and as a Windows x64 Development Player. Stage 2 remains active;
  this is not a map-size, pacing or external-player gate decision.
- Contracts/files: no gameplay/data contract changed. Development build at
  `Builds/Windows/Not3AStage2.exe` uses Stage 2 first and Stage 1 second;
  local smoke log is `Artifacts/S2-QA-01/PlayerLong.log` (both paths Git-ignored).
  Canonical plan/spec/handoff record this checkpoint. Unity's saved scene
  empty-name whitespace was normalized without changing serialized values.
- Verification: EditMode 66/66 and PlayMode 33/33, no skips. One intermediary
  PlayMode invocation yielded 0 tests due Test Runner state and was discarded;
  domain reload followed by the full 33-test pass. Fresh Editor Play after
  reload/Console clear: 0 errors, 0 warnings, no compilation failure. Build
  logged `S2_INT_BUILD_OK`, 188076750 bytes; Player ran 30 seconds on Windows
  11/GTX 1060, logging `S2_INT_STARTUP_OK` and `S2_INT_GAMEPLAY_READY` with
  hash `77A23824A9BF24DA`, no error/exception lines. Build-only warnings
  from optional Pipeline configuration and a stripped hidden shader were
  observed; neither prevented startup. `git diff --check` passes.
- Remaining: S2-MAP-03 needs measured player travel, frame/memory/replans
  and explicit P-008 size decision. The human authorized autonomous packages
  only up to this boundary, so no S2-MAP-03 work starts until they direct it.
  P-004 pacing, representative UI/art/audio, external fresh-player playtest
  and final continue/rework/stop gate remain open.

### 2026-09-24 — Integration S2-INT-02 -> Platform/QA S2-QA-01

- Outcome: the saved 100x100 Stage 2 scene now runs the connected gather ->
  paid tower -> route/wave/siege -> throne outcome -> restart loop. One
  scene-owned composition shares Navigation, Economy inventory, Defense,
  enemy group and Run state; the Stage 1 scene is unchanged.
- Contracts/files: `Assets/Scenes/Stage2VerticalSlice.unity` keeps its six
  roots. `Stage 2 Scene Composition` holds the existing bootstrap plus
  `Stage2GatheringDriver`, `Stage2BuildDriver`, `Stage2RunDriver` and
  `Stage2GameComposition`. Gathering serializes the authored map, Economy
  prototype catalog and hero transform. Composition serializes bootstrap,
  Economy/Defense/Enemy/Run prototype catalogs and all three drivers; it
  initializes one shared Navigation and enemy group, binds enemy protection
  and attack/run state, and exposes a development-build-only readout.
  `Assets/Stage2/Integration/Editor/Stage2IntegrationSceneBuilder.cs` can
  wire the existing clean scene or rebuild with these links; Integration
  runtime/test asmdefs and PlayMode tests reflect the new contracts.
- Verification: Unity compilation passed; fresh scene Play entry logged
  `S2_INT_GAMEPLAY_READY` with map hash `77A23824A9BF24DA`, 0 current Console
  errors/warnings. Full PlayMode 33/33: authored anchors yield all three
  resources, a paid tower spends each once and lengthens the route, a complete
  x=0 wall causes a real-map enemy to destroy the blocker and reopen the route,
  paid towers naturally win a moving wave, an unopposed wave destroys the
  throne, and restart clears population/towers/inventory. The wall test uses
  temporary occupancy tokens outside the paid blocker to isolate siege; it
  is a system verification, not a claim that one run can purchase 100 towers.
- Remaining: Platform/QA runs full EditMode/PlayMode regression, checks fresh
  Console, builds and launches Windows x64 Development. S2-MAP-03 still needs
  measured travel/performance and a human size decision; P-004, UI clarity,
  external player observation and the Stage 2 gate remain open.

### 2026-09-24 — Defense S2-DEF-01-FIX -> Integration/Run state/QA

- Outcome: one paid prototype tower now targets and damages the nearest
  living enemy at a stable interval; pause freezes its timer. A moving
  one-wave PlayMode run wins naturally, while differentiated roles remain
  S2-DEF-02 work.
- Contracts/files: extended `Assets/Stage2/Defense/` tower runtime/catalog,
  prototype data and EditMode test; explicit nearest-target method in
  `Assets/Stage2/WavesAI/Runtime/Stage2EnemyGroup.cs` and run binding in
  `Assets/Stage2/RunState/Runtime/Stage2Run.cs`; natural-win PlayMode test.
  Existing catalog's attack fields were added in place through the Defense
  Editor migration; costs, health, prefab GUID and scene are unchanged.
- Verification: EditMode 66/66, PlayMode 29/29, no skips/failures. Full-suite
  expected Integration negative logs distinguished from an Input System
  test-session monitor issue; script reload and cleared Console then yielded
  0 errors/0 warnings on Stage 2 Play entry/exit. `git diff --check` clean.
- Remaining: Integration binds delivered Economy/Defense/AI/Run state into
  the authored scene, then QA verifies both natural outcomes and Windows
  build. Full-run pacing and human observation are still open.

### 2026-09-24 — Waves/AI S2-WAV-02 -> Defense/Integration/QA

- Outcome: preparation timer/Space leads to a bounded prototype wave;
  confirmed throne arrivals apply damage, completed surviving wave wins,
  zero throne health loses, and R restarts terminal runs. Pause freezes run
  time/enemies and cancels preview. Restart clears enemy protection, towers,
  navigation generation, resource inventory/nodes and prior input progress.
- Contracts/files: `Assets/Stage2/RunState/` plain rules, narrow input driver,
  prototype data, EditMode/PlayMode tests and metas; a narrow nested-callback
  reset fix in `Assets/Stage2/WavesAI/Runtime/Stage2EnemyGroup.cs`.
  `gamedesign/features/STAGE2_RUN_STATE.md` records exact Integration wiring.
- Verification: EditMode 65/65, PlayMode 28/28, no skips/failures. Expected
  negative Integration logs identified. After a test-session Input System
  monitor error, script-domain recompile and cleared Console yielded 0
  errors/0 warnings on Stage 2 Play entry/exit, with successful compilation.
  `git diff --check` clean. No Windows build or live-run claim.
- Remaining: Defense must add one basic tower attack; otherwise the integrated
  wave has no source of enemy damage and cannot win. Integration then binds
  Economy/Defense/AI/Run state to the map scene; QA qualifies a Windows build
  and observes both outcomes. P-004 remains open for measured cadence.

### 2026-09-24 — Waves/AI S2-AI-02 -> Run state/Integration/Defense/QA

- Outcome: one continuously moving ground enemy retains explicit current/step
  cells, protects both step endpoints, replans at centers, sieges the current
  tower owner at a stable interval, resumes after destruction and emits one
  throne-damage fact on arrival. A run-owned group preserves shared protection
  across multiple enemies and clears it on death/reset.
- Contracts/files: `Assets/Stage2/WavesAI/` runtime, prototype enemy data/
  prefab, EditMode/PlayMode tests and metas; exact group/Defense/run-state
  binding in `gamedesign/features/DYNAMIC_ENEMY_PATHING.md`. It consumes the
  shared Navigation and Defense `Stage2Tower`; no scene or Stage 1 edits.
- Verification: EditMode 61/61, PlayMode 26/26; no skips/failures. Expected
  negative Integration logs identified; cleared Console and Stage 2 Play
  entry/exit yielded 0 errors/0 warnings/no compile failure. `git diff --check`
  clean. Windows build remains after scene wiring.
- Remaining: S2-WAV-02 schedules waves, applies `ReachedThrone` damage,
  owns preparation/win/loss/restart; Integration connects the group and driver
  on the real map; QA qualifies the integrated loop and build. No timing,
  variation or full-run gate claim is made by this package.

### 2026-09-24 — Defense S2-DEF-01 -> Waves/AI/Integration/Run state/QA

- Outcome: B/click paid placement on authored Stage 2 cells yields one active
  blocker tower, one three-resource spend, one navigation registration and one
  built fact. Cancel/stale preview/phase/actor/cost failures do not buy; tower
  death releases by identity and allows safe replacement.
- Contracts/files: `Assets/Stage2/Defense/` runtime, prototype catalog/prefab,
  EditMode/PlayMode tests and metas; exact wiring and reset order in
  `gamedesign/features/STAGE2_BUILDING.md`. Defense reads shared Economy
  inventory and Navigation; Integration owns the scene. Wave placement requires
  AI's live current-cell/active-step protection callback.
- Verification: EditMode 54/54, PlayMode 24/24; no skips/failures. Expected
  negative Integration validation logs identified; clear Console then Stage 2
  Play entry yielded 0 errors/0 warnings/no compile failure. `git diff --check`
  clean. No Stage 2 scene or Windows build change in this package.
- Remaining: AI supplies continuous movement protection and siege; run state
  sends phase/pause/restart and invalidates Navigation generation; Integration
  binds prefab/catalog/driver on the real map. QA then observes the full loop,
  Windows build and gate evidence. Hero footprint must track any later collider.

### 2026-09-24 — Economy S2-ECO-02 -> Defense/Integration/Run state/QA

- Outcome: map-anchor wood, stone and metal have held, in-range one-unit gather
  with one confirmed fact; per-node depletion and active-time recovery follow
  D-013. The three-resource inventory adds and atomically spends complete
  costs. Pause/terminal freeze recovery; interruption, build mode and restart
  clear progress; restart resets balances and node timers/stock.
- Contracts/files: `Assets/Stage2/Economy/` runtime, prototype data asset,
  EditMode/PlayMode tests and metas; the S2-ECO-02 section of
  `gamedesign/features/STAGE2_ECONOMY.md` records exact Integration bindings.
  Defense reads `Stage2Inventory` and pays through `TrySpend`; Run state sends
  activity/pause/reset; confirmed `Gathered` facts feed objectives/UI later.
  Integration alone attaches the driver and simple markers to the Stage 2
  scene. Stage 1, Map, Navigation, Integration scene and packages unchanged.
- Verification: Unity 6000.6.2f1 compile succeeded after adding the missing
  Input System asmdef reference. Full EditMode 47/47, PlayMode 22/22, no skips.
  Initial compiler errors are resolved. Two existing Integration negative tests
  intentionally logged validation errors; after clearing retained entries,
  the Stage 2 scene entered Play with 0 new errors/0 warnings, then exited.
  Final Console ground truth: 0 errors, 0 warnings, compilationFailed=false.
  Stage 2 scene remains clean/not dirty; `git diff --check` passes.
- Remaining: S2-DEF-01 binds full cost and depleted-source placement exclusion;
  S2-WAV-02 binds phase/pause/restart; S2-INT-02 adds visible markers and
  serialized references; S2-QA-01 qualifies integrated Windows build.
  Checkpoint C measures travel/rebuild timing and economy flows without reward.
  No complete scene loop, build qualification, or Stage 2 gate is claimed.
  S2-DEF-01 required separate human approval at this handoff and has since
  been authorized and completed.

### 2026-09-24 — Defense/Design S2-BLD-01 -> accepted decision / package complete

- Outcome: human accepted P-002 as D-014; S2-BLD-01 is closed. This supersedes
  the earlier pending proposal status below. Building remains available in
  Preparation/Wave, protects actors and active-step endpoints, allows other
  route cells/full walls and spends only on successful complete confirmation.
- Contracts/files: DECISIONS.md records D-014 and removes P-002 from pending;
  STAGE2_BUILDING.md is accepted. DYNAMIC_ENEMY_PATHING.md now specifies
  finishing a protected continuous step before replanning, without enemy
  protection entering tower occupancy. GAME_VISION.md, DEVELOPMENT_PLAN.md,
  STAGE2_VERTICAL_SLICE.md and this register reflect acceptance.
- Verification: human acceptance received; documentation/status/link review
  and `git diff --check` pass. No Assets/Packages/ProjectSettings changes.
  Read-only live Console check: 0 errors/0 warnings, no compilation failure.
  No Unity tests/builds rerun for this documentation-only acceptance;
  retained 42/42 EditMode and 20/20 PlayMode do not verify continuous placement.
- Remaining: S2-DEF-01 implements/tests transactions and actor exclusion;
  S2-AI-02 supplies/tests active-step state, movement and siege; Integration/QA
  qualify composed behavior. No Stage 2 gate is closed. Next by plan:
  S2-ECO-02 (GPT-6 Sol / high), not started; separate human approval required.

### 2026-09-24 — Defense/Design S2-BLD-01 -> human decision pending

- Outcome: prepared P-002 recommendation: build in unpaused Preparation/Wave,
  protect live enemy current cells and both endpoints of started steps, allow
  other future-route cells/full walls, revalidate paid confirmation and cancel
  without spending. Rules remain proposed; S2-BLD-01 is not closed.
- Contracts/files: `gamedesign/features/STAGE2_BUILDING.md` records code evidence,
  alternatives, phase/continuous-movement/payment rules and scenario review;
  DECISIONS.md retains P-002 as pending; DEVELOPMENT_PLAN.md,
  STAGE2_VERTICAL_SLICE.md and this register name the sole active package.
  Existing user/planning changes and accepted S2-ECO-01 are preserved.
- Verification: reviewed Stage 1 build/movement and S2-MAP-02 API/tests;
  discrete consumers do not prove continuous overlap safety. Historical tests
  remain EditMode 42/42, PlayMode 20/20. Read-only live Unity CLI confirms
  Stage2VerticalSlice loaded/active/not dirty (6 roots), Console 0 errors and
  0 warnings, no compilation failure. Documentation/status/link review and
  `git diff --check` pass. No runtime/scene/package/build-wiring changes;
  no fresh test or Windows build is claimed for this decision-only package.
- Remaining: human accepts/revises P-002. Defense S2-DEF-01 implements atomic
  placement/payment and actor checks; Waves/AI S2-AI-02 supplies active-step
  state and tests continuous movement/siege; Integration alone wires the scene;
  QA qualifies composed behavior. Accepted policy must also be reflected in
  the existing DYNAMIC_ENEMY_PATHING.md consumer notes before closing S2-BLD-01.
  No next package starts without separate human approval; Stage 2 stays active.

### 2026-09-24 — Economy/Design S2-ECO-01 -> accepted decision / package complete

- Outcome: human accepted the prepared P-003 policy; D-013 closes S2-ECO-01.
  This entry supersedes the pending status in the earlier proposal handoff
  below. Finite node stock recovers on active game time independently of
  wave completion/rewards; restart restores nodes and zeroes inventory.
- Contracts/files: DECISIONS.md records D-013 and removes P-003 from pending;
  STAGE2_ECONOMY.md contains the accepted lifecycle and rule-level review;
  GAME_VISION.md, DEVELOPMENT_PLAN.md, STAGE2_VERTICAL_SLICE.md and this
  register reflect acceptance and the separate approval boundary.
- Verification: human acceptance received; documentation/status/link review
  and `git diff --check` pass. No Assets/Packages/ProjectSettings changes.
  Documentation only: Unity tests, Console and Windows build are not rerun;
  retained 42/42 EditMode and 20/20 PlayMode results do not test this lifecycle.
- Remaining: S2-ECO-02 implements/tests the rules; Defense/Integration checks
  spatial access, placement and reset; Checkpoint C measures timely recovery
  after lost defenses without optional rewards. No Stage 2 gate is closed.
  Next by plan: S2-BLD-01, not started and awaiting explicit human approval.

### 2026-09-24 — Economy/Design S2-ECO-01 -> human decision pending

- Outcome: prepared P-003 timed node-recovery proposal with depletion,
  pause/terminal, restart and resource-access rules; reviewed zero inventory,
  full depletion and lost-tower cases without optional rewards. Proposal is
  not accepted and S2-ECO-01 remains open.
- Contracts/files: new `gamedesign/features/STAGE2_ECONOMY.md` records evidence,
  alternatives, proposed rules and downstream checks; P-003 pending entry,
  DEVELOPMENT_PLAN.md, STAGE2_VERTICAL_SLICE.md and this register identify the
  sole active package and the human's approval boundary for the next package.
  Existing uncommitted planning changes are preserved.
- Verification: inspected code/scene/map data and baseline reports at
  `87fdb1b` (EditMode 42/42, PlayMode 20/20, clean fresh-Play Console).
  Documentation/link/status and `git diff --check` review pass. Unity is closed;
  no new runtime/test/build/Console pass is claimed. Assets, Packages and
  ProjectSettings are unchanged by this task; existing build is the scene shell.
- Remaining: human accepts or revises P-003 before closing this decision
  package. Runtime lifecycle tests belong to S2-ECO-02, spatial access and
  placement/reset to system integration, and survivable recovery timing to
  Checkpoint C. These are unverified and no Stage 2 gate pass is implied.
  S2-BLD-01 and all other next packages require separate human approval.

### 2026-09-20 — Project setup -> All workstreams

- Outcome: создана базовая документация, scope MVP и правила работы чатов.
- Contract/files: `AGENTS.md`, `gamedesign/`, `production/`, `.cursor/rules/`.
- Verification: структура и ссылки проверяются до закрытия Stage 0.
- Remaining: настроить version control и реализовать Stage 1 graybox spec.

### 2026-09-20 — Design approval -> Foundation

- Outcome: GAME_VISION утверждён; герой не сражается, обучение и progression
  инструментов перенесены после MVP.
- Contract/files: `gamedesign/GAME_VISION.md`, `production/DECISIONS.md`,
  `gamedesign/features/GRAYBOX_CORE_LOOP.md`,
  `gamedesign/features/OBJECTIVES.md`.
- Verification: решения отражены в canonical vision/plan; Git baseline
  `0016bd5` опубликован в `origin/main`.
- Remaining: реализация интеграционного Unity graybox одним Foundation owner.

### 2026-09-20 — Foundation/integration -> Stage 1 complete

- Outcome: реализован полный graybox loop: WASD/fixed isometric camera, конечные
  tree/stone nodes с автоматическими топором/киркой, одна размещаемая башня,
  фиксированный маршрут и одна волна, throne damage, win/lose/restart и one-shot
  objective «добыть 5 дерева» с наградой существующим ресурсом.
- Contract/files: `Assets/Scenes/Stage1Graybox.unity`, `Assets/Stage1/Runtime/`,
  `Assets/Stage1/Prefabs/`, `Assets/Stage1/Tests/`,
  `ProjectSettings/EditorBuildSettings.asset`; integration scene ownership
  остаётся у Foundation до начала следующего согласованного workstream. Для
  discoverability добавлены `Tools > Stage 1 > Open Graybox Scene` и безопасное
  автопереключение только с неизменённой `SampleScene` при первом импорте.
- Verification: EditMode 7/7, PlayMode 3/3 (включая cell-space map bounds,
  Input System, gathering,
  invalid/valid placement, tower win, unopposed throne loss и restart), Windows
  x64 Development Build создан в `Builds/Windows/Not3AGraybox.exe`; headless
  player smoke запущен без attributable exceptions/errors.
- Readability follow-up: поле расширено с 15x11 до 21x17 клеток, внешний ряд
  получил контрастный boundary treatment, а hero/resources ограничены общей
  изометрической областью вместо несовпадающего world-space прямоугольника.
- Scale follow-up: hero, enemy, tree, stone, tower и throne переведены на
  простые grounded rectangular blocks; PlayMode фиксирует относительную высоту,
  физическую блокировку героя и наличие blocking footprints у ресурсов, трона и
  построенной башни. Встроенный UI sprite заменён на проектный solid sprite 1x1
  world unit; PlayMode проверяет точное совпадение нижней грани с ground origin.
- Visual cleanup: runtime-заливки `Resource Zone`/`Build Zone` удалены после
  обнаружения неверного масштаба встроенного UI sprite; PlayMode запрещает их
  повторное появление, enemy entrance использует изометрический marker.
- Remaining: финальный art/audio/UI, tutorial, metal, дополнительные
  towers/enemies/waves, tool progression, save/Steam и production balance вне
  Stage 1; P-002–P-006 не становятся durable решениями из-за prototype values.

### 2026-09-21 — Product/design pivot -> Stage 1 reopened

- Outcome: принято D-009 — башни формируют динамический grid route; длинный
  зигзаг является основной стратегией, а при полном перекрытии враги разрушают
  blocking towers и продолжают путь. Fixed-route build остаётся исторической
  проверенной базой, но больше не закрывает Stage 1.
- Contract/files: `gamedesign/GAME_VISION.md`,
  `gamedesign/features/DYNAMIC_ENEMY_PATHING.md`,
  `gamedesign/features/GRAYBOX_CORE_LOOP.md`, `gamedesign/DEVELOPMENT_PLAN.md`,
  `production/DECISIONS.md`. Foundation сохраняет integration/scene ownership;
  navigation grid/path result принадлежит Foundation/Waves, tower occupancy и
  health — Defense.
- Verification: canonical документы согласованы с maze-building direction;
  implementation verification ещё не выполнялась и прежние 7/7 EditMode,
  3/3 PlayMode не покрывают новый gate.
- Remaining: заменить `FixedRoute` и route-clearance rejection на grid search,
  occupancy/replanning, tower health и blocker siege; затем заново пройти
  EditMode, PlayMode, Windows build и ручной acceptance. Stage 2 не активен.

### 2026-09-21 — Foundation/integration -> Stage 1 human acceptance

- Outcome: D-009/D-011 реализованы на плоском 21x17 representative slice:
  широкий серый corridor и зелёная территория coplanar; башни занимают клетки
  обеих зон, меняют кратчайший путь и могут полностью закрыть corridor. При
  блокировке враг подходит к доступной стороне башни, разрушает её, освобождает
  occupancy и продолжает к трону. Старый `FixedRoute` больше не связан со
  сценой.
- Contract/files: `Assets/Stage1/Runtime/DynamicNavigationGrid.cs`,
  `Stage1Rules.GridPathfinder`, обновлённые `BuildPlacement`, `EnemyController`,
  `TowerController`, `WaveSpawner`, builder, prefabs и
  `Assets/Scenes/Stage1Graybox.unity`. Prototype tower cost — 2 wood/1 stone;
  значения остаются tuning, не durable balance.
- Verification: scene rebuild/compile passed; EditMode 8/8; PlayMode 4/4,
  включая route-cell placement, deterministic longer zigzag, full wall,
  blocker destruction, route resume, tower win, throne loss и restart. Windows
  x64 Development Build собран в `Builds/Windows/Not3AGraybox.exe`, запущен и
  отвечает без attributable startup errors.
- Remaining: человеку проверить читаемость серого corridor, ощущение
  maze-building/siege и prototype balance в Windows build. До этого Stage 1
  gate остаётся pending; production map 100x100 и сравнение 200x200 относятся
  к Stage 2/P-008, финальный арт и остальные out-of-scope системы не начаты.

### 2026-09-21 — Human playtest -> Stage 1 complete

- Outcome: человек подтвердил, что динамическое движение и разрушение полной
  стены работают корректно; Stage 1 gate закрыт. Временный siege-test profile
  удалён: стартовые ресурсы снова 0/0, здоровье врага снова 8.
- Contract/files: gameplay contracts не изменены; canonical status обновлён в
  `gamedesign/DEVELOPMENT_PLAN.md`, `GRAYBOX_CORE_LOOP.md` и
  `DYNAMIC_ENEMY_PATHING.md`.
- Verification: human acceptance получен; после возврата значений повторно
  пройдены EditMode 8/8 и PlayMode 4/4, Windows x64 Development Build успешно
  пересобран, запущен и не показывает attributable startup errors.
- Remaining: Stage 2 не активирован. Перед ним требуется отдельное решение о
  старте работ; P-002–P-006 и P-008 остаются открытыми, production-scale map и
  контент Stage 2 ещё не реализуются.

### 2026-09-21 — Stage 2 planning coordination -> Stage 2 workstreams

- Outcome: Stage 2 разложен на atomic task backlog от map/economy decisions до
  qualified build и external comprehension gate; отмечены dependency graph,
  parallel windows и единоличное владение Stage 2 scene Integration owner'ом.
- Contract/files: `gamedesign/features/STAGE2_VERTICAL_SLICE.md`, Stage 2 status
  в `gamedesign/DEVELOPMENT_PLAN.md` и ownership rules в этом файле. План не
  создаёт task-specific feature specs или Unity assets заранее.
- Verification: Stage 1 baseline `37178e2` перед планированием совпадал с
  `origin/main`, working tree был чист; документы сверены с D-001–D-011,
  P-002–P-006/P-008, Stage 1 contracts и art direction; Unity code/scenes не
  изменялись.
- Remaining: Stage 2 implementation не начата. Первый owner должен явно claim
  ready task и surface; P-002–P-006/P-008 закрываются соответствующими decision
  tasks до зависящей durable реализации.

### 2026-09-21 — Foundation/Map S2-MAP-01 -> Integration/Navigation/Economy

- Outcome: одна data asset описывает первый плоский 100x100 candidate: green
  forest и gray invasion corridor, entrance/throne, walkable/buildable masks,
  wood/stone/metal placement regions/anchors, camera bounds и разные debug
  colors; runtime больше не требует Stage 1 bounds 21x17.
- Contract/files: `Assets/Stage2/Map/` предоставляет read-only
  `Stage2MapDefinition`, deterministic validation/content hash, generated debug
  mesh renderer, Editor asset builder, candidate asset and EditMode tests;
  `gamedesign/features/STAGE2_MAP.md` содержит точные wiring notes.
- Verification: clean Unity 6000.6.2f1 compile without C# warnings/errors;
  S2-MAP-01 EditMode 12/12 and full EditMode 20/20 pass; `git diff --check`
  passes; no `.unity`, ProjectSettings or package change. Windows build deferred
  by task contract to S2-INT-01.
- Remaining: S2-INT-01 wires the asset/debug renderer in the Stage 2 scene;
  S2-MAP-02 consumes masks for scalable navigation/occupancy; S2-ECO-02 places
  nodes in resource regions. P-008 remains open; no 200x200 variant exists.

### 2026-09-22 — Integration S2-INT-01 -> Map/Economy/system owners/Platform-QA

- Outcome: `Assets/Scenes/Stage2VerticalSlice.unity` is a runnable 100x100 scene
  shell with green forest, gray corridor, orange entrance, gold throne, cyan
  non-combat WASD player and fixed orthographic follow camera. Startup validates
  the explicit map reference and reports `S2_INT_VALIDATION_ERROR` with code and
  reason on missing/invalid data; valid startup logs `S2_INT_STARTUP_OK` and the
  deterministic map hash. Runtime never writes authored map data.
- Contract/files: `Assets/Stage2/Integration/Runtime/Stage2SceneBootstrap.cs` on
  `Stage 2 Scene Composition` owns serialized `mapDefinition`, `mapDebugRenderer`,
  `playerMover`, `cameraFollow`, `entranceMarker`, `throneMarker`. The same
  `Stage2Map100x100.asset` is assigned to all map consumers; player uses
  `CellBounds`, camera target/position uses `CameraBounds` in cell space.
  Landmarks resolve from data at startup. Narrow movement/camera components
  reuse Stage 1 `IsoGrid` without a `Stage1GameController` dependency.
- Build/wiring: enabled order is `Stage2VerticalSlice` (0), `Stage1Graybox` (1).
  `Tools > Stage 2 > Integration` exposes Open Vertical Slice Scene, Rebuild
  Scene Shell and Build Windows Development; the Editor builder creates/saves
  the scene through Unity APIs and refuses to overwrite unsaved open scenes.
  Output is `Builds/Windows/Not3AStage2.exe`. Stage 1 scene/assets/tests, Map
  assets and packages are unchanged; automatic build-time settings serialization
  was restored to the original Git contents.
- Verification: full EditMode 20/20 (Stage 1 8/8, Map 12/12); full PlayMode 13/13
  (Stage 1 4/4 including restart, Integration 9/9). Integration covers all WASD
  directions, four extreme bounds, actual camera following, validation
  diagnostics and a transient changed definition with independent camera bounds.
  Clean Editor startup has 0 errors/0 warnings. Windows x64 Development Build
  succeeded with 0 errors; 56-second batchmode smoke with Direct3D 11 enabled
  logged `S2_INT_STARTUP_OK`, responded and produced no errors/exceptions.
  `git diff --check` passes; protected Stage 1/Map/package surfaces have no diff.
  Local evidence: `Artifacts/S2-INT-01/` contains EditMode/PlayMode reports,
  startup/entrance screenshots, Windows build report and `Windows-player.log`.
- QA environment notes: build emitted three package warnings: disabled optional
  Pipeline runtime and stripped `Hidden/Core/DebugOccluder` / `DebugOcclusionTest`
  shaders. Existing Stage 1 tests alone reproduce stale Input System mouse
  monitor errors on the next Editor Play entry with Domain Reload disabled;
  reload the script domain or restart Editor after those tests. Integration
  alone passes 9/9 and the subsequent Play entry stays clean. No tests are
  skipped or replaced with degraded manual movement checks.
- Remaining/handoffs: map composition/serialized binding points -> S2-MAP-02;
  map/player/camera shell -> S2-ECO-02. Defense, Waves/AI, UI/UX and Art/Audio
  owners deliver prefabs/data plus wiring notes to Integration rather than
  editing the scene. Platform/QA receives the Windows build and smoke evidence.
  Navigation scaling, resource nodes, economy, combat/waves/objectives, final
  UI/art/audio and overall Stage 2 acceptance remain unimplemented by this task;
  P-002–P-006/P-008 remain open and no 200x200 variant was created.

### 2026-09-22 — Foundation/Waves-AI S2-MAP-02 -> Defense/AI/Integration/Platform-QA

- Outcome: data-driven navigation on the actual 100x100 candidate produces
  deterministic open/zigzag/blocker routes; removing blockers and resetting a
  run recover safely. Occupancy validates authored masks and reference identity;
  each real transaction changes revision once. Consumers share a reverse search
  field and retain immutable plans until revision/run generation changes.
- Contracts/files: `Assets/Stage2/Navigation/Runtime/Stage2Navigation.cs`, its
  test assemblies/fixtures and the Stage 2 section of `DYNAMIC_ENEMY_PATHING.md`.
  Defense S2-DEF-01 receives `TryOccupy`/`TryRelease` ownership-token lifecycle;
  S2-AI-01/S2-AI-02 receive `NavigationPlan` path/blocker/diagnostic output and
  `ReplanIfStale`. Integration receives explicit shared-instance injection and
  map-reference/reset instructions; no prefab or scene edit is needed here.
- Verification: EditMode 42/42 = Stage 1 8 + Map 12 + Navigation 22; PlayMode
  20/20 = Stage 1 4 + Integration 9 + Navigation 7; no skips. Fresh Editor
  restart and subsequent Stage 2 Play entry: 0 errors/0 warnings, no compile
  failure or recurring stale Input System monitor error. Integration's two
  intentional validation errors were expected by its negative tests. Scene
  remains clean/unsaved; protected surfaces unchanged; `git diff --check` passes.
- Measurements -> S2-MAP-03/Platform-QA: map hash `77A23824A9BF24DA`, Unity
  6000.6.2f1 Editor/Editor PlayMode, i5-12400F, 12 logical CPUs, 32581 MB RAM,
  GTX 1060 6GB, Windows 11 build 26100. Three warmups and 30 samples per case
  and mode. Search median ms (open/zigzag/full wall): EditMode
  1.0835/1.1217/1.0958; PlayMode 1.0741/1.0448/1.1208. Full p95/max and replan/
  cache timings are recorded in the existing pathing spec. Routes: 89/385 steps;
  wall approach: 48 steps to blocker `(0,0)`. 64 moving consumers/128 frames:
  1 search, 64 requests, 8128 retained-plan hits; 24 consumers with shared
  next-cell placement: 2 searches total and all reach the throne. Local raw
  samples, reports, Console evidence and exact diff: `Artifacts/S2-MAP-02/`.
- Remaining: S2-BLD-01 owns placement-phase/current-cell overlap policy;
  S2-DEF-01 owns tower lifecycle/economy binding; S2-AI-02 owns continuous enemy
  movement/siege; Integration alone wires the shared instance into the scene.
  S2-MAP-03 still decides scale/travel time/P-008, S2-QA-01 qualifies the Windows
  build. No universal timing budget, 200x200 variant, combat/spawning/economy or
  Stage 2 completion is implied. Only S2-MAP-02 is closed by this handoff.

### 2026-09-24 — Design replanning -> Stage 2 workstreams

- Outcome: Stage 2 now advances through a connected loop, measured
  scale/pacing, representative presentation and external comprehension gate.
  The MVP content ceiling is no longer interpreted as a Stage 2 task quota.
- Contract/files: DEVELOPMENT_PLAN.md owns the revised stage gates;
  STAGE2_VERTICAL_SLICE.md keeps existing task IDs but changes their readiness
  order and makes S2-INT-02/S2-QA-01 iterative checkpoints. This file owns
  capacity, model/effort and soft token routing. ART_DIRECTION.md now depicts
  dynamic maze paths per D-009. D-001–D-011 and completed task evidence are
  unchanged; earlier planning entries remain historical.
- Verification: documentation cross-check against GAME_VISION.md,
  DECISIONS.md, Stage 2 map/navigation/integration specs and current Git/Unity
  files; diff whitespace/link/status review. No Unity runtime files or scene
  changed, so EditMode/PlayMode/build checks are not rerun for this handoff.
- Remaining: P-002–P-006/P-008 remain open; no new implementation task is
  claimed here. Next owner claims S2-ECO-01 or S2-BLD-01 with exact surface
  before changing gameplay. S2-MAP-03 still needs live-loop travel/performance
  evidence; Stage 2 requires an external player gate.

### 2026-09-24 — Human asset-scope decision -> Art/Stage 2/Post-MVP

- Outcome: D-012 removes detailed Blender/3D model production from the
  Stage 2–6 critical path. MVP art remains simple, readable 2D isometric work;
  a later Blender pass requires its own scope and visual-direction decision.
- Contract/files: GAME_VISION.md and ART_DIRECTION.md state the MVP asset
  boundary; DEVELOPMENT_PLAN.md places a Blender evaluation after MVP;
  STAGE2_VERTICAL_SLICE.md limits S2-ART-01–03 to representative 2D assets;
  DECISIONS.md records D-012; this file lowers the default model/effort for
  MVP art tasks and reserves GPT-6 Astra for scoped post-MVP Blender work.
- Verification: checked D-003/D-005, current Stage 2 gate and model routing;
  git diff --check passes. Documentation-only change: no Unity scene, prefab,
  runtime, package or Console change; Unity tests/build are not rerun.
- Remaining: no Blender asset or new 3D Unity pipeline is authorized for MVP.
  The later use of Blender source assets versus runtime 3D objects is unresolved
  and must be decided before such production starts.

### 2026-09-24 — Solo-developer allocation -> Stage 2–7 owners

- Outcome: the active Stage 2 backlog and future Stage 3–7 gates now have 55
  named packages with an assigned GPT-6 model, reasoning level, dependency,
  observable exit and soft token envelope. Stage 0–1 remain complete; one
  developer keeps one implementation package active at a time.
- Contract/files: `gamedesign/DEVELOPMENT_PLAN.md` owns the per-task and
  per-stage allocations; this register points to that table as the sole
  routing source. A derived, self-contained dark HTML view is saved locally
  in the Git-ignored `reports/` directory.
- Verification: all 24 original Stage 2 task IDs are accounted for, including
  three completed IDs at zero forward allocation; the 55 new IDs are unique
  and stage totals equal the sum of task bands. HTML was generated from the
  canonical plan and visually checked at desktop and narrow widths;
  `git diff --check` passes. No Unity runtime, scene, asset or package changed,
  so Unity tests/build were not rerun for this planning handoff.
- Remaining: track actual Codex usage and rework after three comparable tasks,
  then revise estimates. P-002–P-006/P-008 and the Stage 2 external player
  gate remain open. S7-VISUAL-01 must settle Blender sprite-source versus
  runtime-3D direction before detailed post-MVP asset production.

### 2026-09-24 — Stage dialogue prompts -> Stage 2–7 owners

- Outcome: each Stage 2–7 has one copyable opening prompt for a single Codex
  dialogue, placed ahead of its task list in the local HTML report. The list
  keeps dependency order, task-level model/effort and token envelopes.
- Contract/files: `gamedesign/DEVELOPMENT_PLAN.md` owns the six prompt texts;
  the Git-ignored `reports/development-roadmap.html` is generated from them.
  Model and reasoning settings remain user-selected in the same dialogue when
  the next package calls for a different setting.
- Verification: all six prompt blocks parse, Stage 2–7 switching shows the
  matching prompt and task count, and the desktop layout was visually checked.
  Documentation/report-only change: Unity tests/build are not rerun.
- Remaining: stage entry gates and human scope/release decisions still apply;
  the Stage 2 gameplay packages remain unimplemented by this handoff.
