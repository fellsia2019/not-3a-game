# Development Plan

Canonical state, replanned 2026-09-24. Stage 1 was accepted by a human on
2026-09-21. Stage 2 is active: S2-MAP-01, S2-INT-01, S2-MAP-02 and the
S2-ECO-01/S2-BLD-01 decision packages and S2-ECO-02/S2-DEF-01/S2-AI-02/
S2-WAV-02/S2-DEF-01-FIX/S2-INT-02/S2-QA-01 checkpoints are complete;
P-003/P-002 are accepted as D-013/D-014 respectively.
S2-MAP-03/S2-WAV-01 analysis and S2-AI-01 brief are delivered with consolidated
verification, 2026-09-24. The human then delegated the P-004 diagnostic trial
choice and implementation. S2-WAV-01 cadence implementation is delivered; S2-WAV-02
remains complete. Final pacing, P-008 and enemy breadth remain separate
decisions. Their live evidence is in the map/run specs and enemy-role brief.
The detailed active-task contracts and evidence are in
gamedesign/features/STAGE2_VERTICAL_SLICE.md and production/WORKSTREAMS.md.

## Planning Rule

Each stage buys down a specific risk. A stage closes after a playable result,
relevant tests/build checks, a human observation where specified, and an explicit
continue/rework decision. The MVP budget in GAME_VISION.md is a ceiling, not a
shopping list. Numbers for content, map size and balance become commitments only
after the test that needs them. Accepted D-001–D-012 are unchanged by this plan.

Keep one owner for each shared Unity surface and limit work in progress to tasks
whose inputs are ready. Prototype values live in Unity data, not a second table
here. When a gate fails, fix the observed cause before expanding content.

## Stage 0 — Foundation And Product Boundary

Status: core product boundary and version control established. Remaining
administrative items do not reopen the completed Stage 1 prototype.

- [x] Approve GAME_VISION, non-combat hero, three resources, art direction and
  MVP/post-MVP boundary.
- [x] Initialize Git and ignore generated Unity directories.
- [ ] Resolve the working title, IP ownership and sources/licenses before any
  public asset is used.
- [ ] Decide whether the experimental com.unity.pipeline package is needed
  before relying on it for build automation.
- [ ] Resolve P-002–P-006/P-008 at their actual implementation or gate point;
  P-007 remains post-MVP.

Gate: repository and concept are usable for a technical prototype. Passed for
Stage 1; the remaining items retain their own deadlines above.

## Stage 1 — Technical Maze Loop

Status: complete and human-accepted, 2026-09-21. The compact graybox proves
gather -> spend -> build a longer route or full wall -> enemy path/siege -> throne
win/loss/restart. D-009 replaced the original fixed-route assumption.

Evidence: EditMode 8/8, PlayMode 4/4, Windows x64 Development Build startup
without attributable errors, and human playtest of the maze behavior. Stage 1
scene remains the regression baseline; it is not the production map.

## Stage 2 — First Playable And Representative Slice

Status: active. Its question is whether the accepted loop works on the
data-driven map and is understandable and worth repeating. The first 100x100
candidate is an experiment; P-008 remains open. Stage 2 does not require every
tower, enemy, boss, VFX or menu allowed by the MVP ceiling.

### Checkpoint A — Map And Navigation Contract

Status: passed. S2-MAP-01 provides the 100x100 map asset; S2-INT-01 provides the
Stage 2 scene shell; S2-MAP-02 provides deterministic navigation and occupancy.
The latest recorded regression is EditMode 42/42 and PlayMode 20/20, with clean
Editor/Play entry at that checkpoint. S2-INT-02/S2-QA-01 subsequently wired and
qualified its integrated use; the current cumulative counts are recorded below.

### Checkpoint B — One Playable Loop On The Real Map

Completed package: **S2-ECO-01**, Economy/Design; GPT-6 Astra / high,
human-accepted on 2026-09-24. P-003 is resolved by D-013; the accepted contract
and rule-level review are in `features/STAGE2_ECONOMY.md`. Runtime implementation
and live-loop recovery checks remain downstream.

Last completed package: **S2-BLD-01**, Defense/Design; GPT-6 Astra / high,
human-accepted on 2026-09-24. D-014 resolves P-002; `features/STAGE2_BUILDING.md`
contains the accepted rules. Runtime verification remains downstream.
Last completed package: **S2-ECO-02**, Economy; human authorized it on
2026-09-24. The plan recommends GPT-6 Sol / high; model/effort selection stays
in this dialogue's UI. Three-resource gathering, timed recovery, atomic
full-cost spending and reset pass EditMode 47/47 and PlayMode 22/22, with a
clean subsequent Stage 2 Play entry. The scene is not yet wired to Economy.
Last completed package: **S2-DEF-01**, Defense; human authorized it on
2026-09-24. The plan recommends GPT-6 Sol / high; model/effort selection stays
in this dialogue's UI. Paid placement and tower lifecycle pass EditMode 54/54,
PlayMode 24/24 and a clean subsequent Stage 2 Play entry. The Defense prefab,
catalog and exact wiring contract are delivered; Integration still owns scene
connection. The human authorized sequential starts of following ready packages
up to the S2-MAP-03 boundary without separate approval. **S2-AI-02** is
complete: continuous movement, active-step exclusion and siege pass EditMode
61/61, PlayMode 26/26 and a clean subsequent Stage 2 Play entry. Its prefab,
data and exact wiring contract are delivered; the scene remains Integration-owned.
**S2-WAV-02** and corrective **S2-DEF-01-FIX** are complete under the human's
sequential authorization. The one-wave run and baseline tower attack pass
EditMode 66/66, PlayMode 29/29; a moving wave can now end in a natural win.
P-004 remains open for live pacing, and Integration/Windows qualification is
still required. S2-DEF-02 role variation remains later.
**S2-INT-02** is complete under the human's sequential authorization. The
saved Stage 2 scene binds one shared Navigation/Economy/Defense/enemy group/run;
real-map PlayMode covers three sources, paid towers, route diversion, full-wall
siege, natural win, throne loss and restart (33/33 PlayMode). **S2-QA-01** is
complete under the same authorization: EditMode 66/66, PlayMode 33/33, fresh
Editor Play 0 errors/0 warnings, Windows x64 Development Build succeeded and
its Player log reached both Stage 2 startup messages without errors. The human
subsequently authorized S2-MAP-03 -> S2-WAV-01 -> S2-AI-01, with consolidated
checks/corrections after all three drafts. Those deliveries are now reviewed;
product acceptance remains explicit.

2026-09-25 player observation found a missing S2-INT-02 deliverable: the
resource anchors had Economy logic but no visible scene markers, so the
100x100 test build did not show how to find wood, stone or metal. Integration
added three nonblocking runtime markers and source directions in the temporary
Development Build readout; focused full PlayMode regression is 35/35. The
corrected Windows build passed on 2026-09-25 and was relaunched with a clean
Stage 2 startup. The human recheck of source readability is pending before
this defect is closed. No map/economy values were changed.
The recheck identified a further S2-INT-02 defect: depleted wood/stone markers
did not disappear. The same screenshot showed the run was Lost and metal was
18 m away, explaining why E could not gather there. The follow-up makes empty
markers disappear until recovery/restart and makes hold-E/restart requirements
explicit; an active-run E input test and renewed human build check qualify it.
Integration PlayMode 15/15 passed, including depletion/recovery/restart marker
lifecycle and actual held-E metal gathering. The Windows Development Build was
rebuilt and relaunched on 2026-09-25 with clean startup. The player confirmed
the corrected build works ("все гуд"), closing this S2-INT-02 correction.
Stage 2 gate is not decided by this correction alone.

Priority: first. Reuse the tested Stage 1 rules where appropriate; adapt only
the contracts that fail on the new map or the third resource. Apply the accepted
P-003 node lifecycle and P-002 active-wave placement rules.
Connect resource gathering/inventory, paid placement, tower occupancy, enemy
movement/siege, throne damage and restart in the Integration-owned scene.
Provisional wave/timing values may live in data while P-004 is tested.

Gate: a developer can repeat gather -> build -> defend -> recover on the
100x100 candidate, including an open route, a longer route, a full wall, blocker
destruction and restart. Invalid placement cannot spend resources. All three
resources are obtainable. Relevant EditMode/PlayMode checks and a Windows
development launch have no new attributable errors. A failed gate returns to
the narrow system owner before content breadth or final art is added.

### Checkpoint C — Distance, Pacing And Choice

Current disposition, 2026-09-24: **continue the 100x100 scale candidate;
rework one-wave cadence; review one additional siege profile.** S2-MAP-03
measured actual source travel and affordable compact blockage as well as
Editor frame/memory/navigation load. It recommends keeping current masks,
not a larger map or compulsory choke rebuild. S2-WAV-01 observed a 65.083 s
defended win and 219.484 s unopposed loss; proposes a three-round diagnostic
candidate with 45/30 s preparation and persistent state. S2-AI-01 delivers
`features/STAGE2_ENEMY_ROLES.md`. The human subsequently delegated the cadence
trial; the existing run catalog now supports the finite sequence. P-008,
final representative P-004 pacing and the new profile remain open. No
fresh-player run has been observed. Checkpoint C and the Stage 2 gate remain open.

The human clarified that future map shape and placement rules remain open:
rectangular expansion, an offset throne, a winding invasion area and possibly
gray-only building are tentative options recorded in `features/STAGE2_MAP.md`.
They do not expand the current slice scope or make 100x100 a permanent size lock.

Priority: after a live loop exists. Measure travel to each resource region,
time spent gathering/building, resource gained/spent/remaining by type and
phase, route changes, frame time, memory and navigation replans on recorded
hardware. S2-MAP-03 resolves P-008 from those observations;
200x200 is only a comparison if the 100x100 result cannot meet the design
intent. S2-WAV-01 resolves P-004 from a complete run rather than an abstract
wave count. Test whether tower/enemy variation adds a meaningful decision;
implement only the breadth needed for this slice.

Gate: a complete run has a clear preparation/defense rhythm, no resource
soft-lock, readable risk of distance and a recorded map-size/pacing decision.
Each resource has an understandable source and useful sink; objective rewards
cannot silently mask an economy that fails without them.
If travel or waiting dominates, change data/map composition before increasing
content. The final MVP content count remains a later decision.

### Checkpoint D — Representative Presentation And External Test

Choose P-006 before final HUD implementation. Build a compact HUD and
build/route feedback, one optional objective per D-008, simple readable
original or licensed 2D art, basic audio and win/loss/pause states needed to
understand the run. Detailed Blender models are not a Stage 2 gate.
Visual research and UI wireframes can start before Checkpoint C; runtime art
fill and scene wiring follow tested scale/contracts. P-005 save policy is
decided before a persistence feature is built; it may be assigned to Stage 3
if the Stage 2 test does not require mid-run save.

Gate: S2-QA-01 qualifies an integrated Windows x64 build and full-run
regression. S2-QA-02 observes at least one fresh player without developer
coaching. The player locates throne, entrance and resources, understands that
towers reshape routes and full blockage triggers siege, completes or loses a
run, and can name a meaningful layout decision to change. The 15–25 minute
run target is measured, not assumed. Record continue/rework/stop and the
specific evidence in the canonical handoff register.

## Stage 3 — MVP Alpha

Enter only after the Stage 2 gate. Turn the accepted slice into a feature-complete
MVP within GAME_VISION.md. Choose the final content breadth, fill the selected
map, finish approved tower/enemy roles and run content, settings, controls,
save policy, basic accessibility and balance. Resolve pathing edge cases under
representative loads. Finish only the 2D assets required for a legible MVP;
do not introduce detailed 3D-model production. No automatic expansion to
post-MVP systems.

Gate: every approved MVP feature is playable end to end; no known blocker
prevents a complete run; data and any saved state survive the intended update
path. Record content lock candidates and remaining polish risks.

## Stage 4 — Beta And Polish

Focus on defects, clarity, performance, hardware/resolution coverage, audio
mix, selected localization and clean-machine QA. Lock content before release
candidate qualification; avoid a late Blender pipeline or mass visual rebuild.

Gate: release candidate passes full regression, starts on a clean Windows PC,
has no new attributable console errors and does not lose supported data.

## Stage 5 — Steam Readiness

Prepare Steamworks legal/banking/tax requirements, app fee, store assets/copy,
content survey, price, depots/branches and launch options early enough for
review lead times. Publish Coming Soon after the visual direction and core
feature set are stable. Submit store page and near-final build separately,
then verify install, launch and rollback through Steam.

Gate: Valve approves page/build; required Coming Soon interval has elapsed;
the clean-PC Steam install and rollback/hotfix path work. Recheck current
Steamworks policy before scheduling: the 2026-09-20 source snapshot recorded
[app fee](https://partner.steamgames.com/doc/gettingstarted/appfee),
[onboarding timing](https://partner.steamgames.com/doc/gettingstarted/onboarding),
[release reviews](https://partner.steamgames.com/doc/store/releasing),
[review lead times](https://partner.steamgames.com/doc/store/review_process) and
[SteamPipe](https://partner.steamgames.com/doc/sdk/uploading).

## Stage 6 — Release And First Patches

Freeze the release candidate, back it up, run final smoke/regression, release,
watch crashes and player reports, and ship verified hotfixes with short patch
notes.

Gate: critical launch issues are resolved and the remaining backlog is split
between patches and post-MVP experiments.

## Stage 7 — Post-MVP Decisions

Use playtest and sales evidence to evaluate tutorial, tool tiers,
automation/logistics, roguelite progression, additional maps/biomes, other
platforms and detailed Blender asset work separately. GPT-6 Astra may lead
scoped Blender modeling, editing and review after that work is activated;
it is not a standing cost for MVP. None is promised by the MVP plan.

## Task Allocation, Models And Token Envelopes

Stage 0 and Stage 1 are complete: no new model or token allocation is planned
for them. Their evidence remains above and in WORKSTREAMS.md. The allocations
below cover only remaining Stage 2 work and future Stage 3–7 work packages.
Future packages are scheduling units, not permission to build unapproved
features or create feature specs before activation.

One developer works through one implementation package at a time. A short
decision or art brief may be prepared while a build or external review is
waiting, but the Integration-owned scene has one writer. Stage 5 account and
store preparation may begin during Stage 3/4 when its inputs are ready; its
stage number names the release gate, not a prohibition on early preparation.

Each range is a **soft cumulative allocation per task in thousands of tokens**
for selected input/tool context plus visible output over its planned Codex
runs. It excludes hidden reasoning, tool charges, retries caused by changed
requirements and account quotas. Stop when acceptance is met; do not spend the
upper bound merely because it exists. Stage totals below are sums of these
task envelopes, with conditional tasks shown separately. After three
comparable tasks, replace these estimates with measured usage and rework rate.
In the Codex UI, medium/high/xhigh/ultra correspond to средний/высокий/очень
высокий/ультра; ultra is an exception, never a default task assignment.

### Stage 2 — remaining tasks

S2-MAP-01, S2-INT-01, S2-MAP-02, S2-ECO-01, S2-BLD-01, S2-ECO-02,
S2-DEF-01, S2-AI-02, S2-WAV-02 and S2-DEF-01-FIX are complete and
receive **0 forward tokens**. The two decision packages retain their original
allocations below for planning history;
the original Stage 2 total is not a new remaining-work estimate.
The following order follows Checkpoints B–D and the active feature
spec; S2-INT-02 and S2-QA-01 recur at integration checkpoints.

- **S2-ECO-01 · Политика ресурсных узлов — complete, 2026-09-24** — Economy/Design; GPT-6 Astra · high · исходный план 14–26k; P-003 принят как D-013, проверены правила против постоянного исчерпания; runtime и своевременное восстановление обороны проверяются в следующих пакетах.
- **S2-BLD-01 · Строительство во время волны — complete, 2026-09-24** — Defense/Design; GPT-6 Astra · high · исходный план 14–26k; P-002 принят как D-014, проверены правила фаз, занятых клеток/переходов и отмены без траты; runtime verification остаётся downstream.
- **S2-ECO-02 · Три ресурса и инвентарь — complete, 2026-09-24** — Economy; GPT-6 Sol · high · исходный план 28–50k; данные карты связывают добычу дерева/камня/металла, запас/восстановление, полную цену и reset; EditMode 47/47, PlayMode 22/22. Scene/Windows integration остаётся отдельным checkpoint.
- **S2-DEF-01 · Оплаченное размещение — complete, 2026-09-24** — Defense; GPT-6 Sol · high · 32–58k; после S2-ECO-02/S2-BLD-01: один confirm, одна occupancy-транзакция, отказ без списания; EditMode 54/54, PlayMode 24/24.
- **S2-DEF-01-FIX · Базовый урон башни — complete, 2026-09-24** — Defense; GPT-6 Sol · high (рекомендация родительского S2-DEF-01); одна базовая атака, пауза замораживает таймер и естественная победа волны подтверждена без новых ролей; EditMode 66/66, PlayMode 29/29.
- **S2-AI-02 · Движение и siege — complete, 2026-09-24** — Waves/AI; GPT-6 Sol · high · 32–58k; после S2-MAP-02/S2-DEF-01: враг перестраивает путь, разрушает blocker и продолжает движение; EditMode 61/61, PlayMode 26/26.
- **S2-WAV-02 · Минимальный run state — complete, 2026-09-24** — Waves/AI; GPT-6 Sol · high · 24–44k; после S2-AI-02: подготовка, волна, урон трону, win/lose/restart без утечек состояния; EditMode 65/65, PlayMode 28/28. Реальную победу должен обеспечить базовый урон башни в Defense и подтвердить Integration.
- **S2-INT-02 · Пошаговая сборка сцены — complete, 2026-09-24** — Integration; GPT-6 Sol · high · 50–90k; реальная сцена связана, 33/33 PlayMode; Windows qualification передана S2-QA-01.
- **S2-QA-01 · Регрессия и сборка — checkpoint complete, 2026-09-24** — Platform/QA; GPT-6 Sol · high · 38–70k; EditMode 66/66, PlayMode 33/33, fresh Console clean, Windows Development Build and Player startup pass. Повторяется после будущих integration checkpoints.
- **S2-MAP-03 · Размер карты P-008 — analysis delivered, acceptance pending** — Design/Platform/Map; GPT-6 Astra · high · 24–44k; живые travel/frame/memory и 8 layout probes: рекомендация оставить 100x100/current masks для Stage 2. P-008 ждёт человека.
- **S2-WAV-01 · Темп забега P-004 — cadence implemented, manual check pending** — Design/Waves; GPT-6 Astra · high · 18–32k; последовательность волн и сохранение состояния реализованы, EditMode 70/70 и PlayMode 34/34. Windows Development Build обновлён 2026-09-25 (`S2_INT_BUILD_OK`); запуск и игровой прогон новой сборки переданы человеку. Дополнительные агентские прогоны остановлены по его замечанию; окончательный темп и цель 15–25 минут остаются открыты.
- **S2-AI-01 · Бриф различий врагов — brief delivered, acceptance pending** — Design/Waves; GPT-6 Astra · high · 12–22k; baseline + один siege specialist, существующие damage/path contracts достаточны; новые runtime profiles не добавлены.
- **S2-DEF-02 · Различимые роли башен** — Defense; GPT-6 Sol · high · 28–50k; после S2-DEF-01 и брифа врагов: проверить разные игровые решения, не заполняя весь MVP ceiling.
- **S2-OBJ-01 · Побочная задача** — Run state; GPT-6 Sol · medium · 16–28k; после подтверждённых gather/build facts: один активный прогресс и награда ровно один раз.
- **S2-UI-01 · Выбор UI stack P-006** — UI/Integration; GPT-6 Sol · high · 10–18k; до финального HUD: сравнить текущие сцены и нужные состояния, принять стек без миграции ради миграции.
- **S2-UI-02 · HUD и build feedback** — UI/UX; GPT-6 Sol · high · 28–50k; после контрактов игры и P-006: читаемы ресурсы, волна, трон, цена, маршрут, цель и terminal state.
- **S2-SAV-01 · Граница сохранения P-005** — Design/Platform; GPT-6 Sol · high · 10–18k; до persistence: принять run-save policy или явно перенести реализацию в Stage 3.
- **S2-ART-01 · Визуальный бриф и права** — Art/Design; GPT-6 Sol · medium · 8–16k; по карте и D-003/D-012: silhouette/scale sheet и подтверждённое происхождение ассетов.
- **S2-ART-02 · Простая среда и ресурсы** — Art; GPT-6 Sol · medium · 12–22k; после P-008: читаемые 2D surfaces, landmark и resource prefabs без Blender-полировки.
- **S2-ART-03 · Акторы, башни и VFX** — Art; GPT-6 Sol · medium · 16–30k; после gameplay hooks: роли и удары различимы в busy-wave кадре без детальных 3D-моделей.
- **S2-AUD-01 · Критический звук** — Audio; GPT-6 Sol · medium · 10–18k; после state hooks: различимые alerts, без клиппинга и с подтверждёнными правами.
- **S2-QA-02 · Внешний playtest и gate** — Design/QA; GPT-6 Astra · high · 16–30k; после qualified build: наблюдение без подсказок и решение continue/rework/stop.

Stage 2 allocation: **440–800k**; no token reallocation to completed tasks or
detailed Blender work. If a task exceeds its upper band, narrow the evidence
or split the task before raising reasoning effort.

### Stage 3 — MVP Alpha packages

These packages become implementation-ready only after the Stage 2 gate and
the resulting scope decision. Counts in GAME_VISION.md remain ceilings.

- **S3-SCOPE-01 · Зафиксировать состав MVP** — Design; GPT-6 Astra · high · 18–32k; после S2-QA-02: человек принимает нужные роли, контент и явные исключения без расширения D-001–D-012.
- **S3-MAP-01 · Наполнить выбранную карту** — Map/Integration; GPT-6 Sol · high · 25–45k; после P-008: выбранная карта, ресурсы и landmarks работают без пустых переходов и потери читаемости.
- **S3-ECO-01 · Экономика полного забега** — Economy; GPT-6 Sol · high · 25–45k; после S3-SCOPE-01/P-003: приток и расход дерева/камня/металла проходят весь run без soft-lock.
- **S3-DEF-01 · Утверждённые башни** — Defense; GPT-6 Sol · high · 30–55k; после S3-SCOPE-01: роли различаются поведением, оплачиваются и меняют путь без сломанной occupancy.
- **S3-AI-01 · Утверждённые враги и волны** — Waves/AI; GPT-6 Sol · high · 30–55k; после S3-SCOPE-01/P-004: escalation, siege и финальная победа работают с нужным контентом.
- **S3-RUN-01 · Цели и состояние забега** — Run state; GPT-6 Sol · high · 22–40k; после S3-AI-01: phase, optional objective, win/lose/restart согласованы с D-008.
- **S3-UI-01 · Настройки и доступность** — UI/UX; GPT-6 Sol · high · 25–45k; после P-006 и gameplay hooks: поддержаны утверждённые controls, настройки и базовая читаемость на целевых разрешениях.
- **S3-ART-01 · MVP 2D-графика и звук** — Art/Audio; GPT-6 Sol · medium · 20–38k; после S3-SCOPE-01: необходимые ассеты оригинальны/лицензированы и читаемы без детального Blender pipeline.
- **S3-SAVE-01 · Run-save при принятом P-005 (условно)** — Platform/Run state; GPT-6 Sol · high · 22–40k; только если P-005 требует сохранения забега: version/round-trip/corrupt-data/relaunch проходят тесты.
- **S3-QA-01 · Alpha end-to-end gate** — Platform/QA; GPT-6 Sol · high · 36–65k; после активных S3-пакетов: полный run, update-path и регрессия без блокеров.

Stage 3 allocation: **231–420k** for the base scope, plus **22–40k** only if
P-005 accepts run-save implementation.

### Stage 4 — Beta And Polish packages

- **S4-LOCK-01 · Content lock и triage** — Design/QA; GPT-6 Astra · medium · 12–22k; после Alpha: утверждённый список дефектов и запрет незаметного расширения scope.
- **S4-UX-01 · Исправить непонятные состояния** — UI/UX; GPT-6 Sol · high · 20–36k; по playtest evidence: build, route, wave, warning и terminal feedback понятны без подсказки.
- **S4-PERF-01 · Производительность и память** — Platform/Navigation; GPT-6 Sol · high · 28–50k; по profiler capture: frame time, replan и memory на representative load в принятом бюджете.
- **S4-COMPAT-01 · Windows/input/resolution** — Platform/QA; GPT-6 Sol · high · 20–36k; на целевых конфигурациях: окно, DPI, ввод, pause и relaunch работают.
- **S4-AUDIO-01 · Аудиомикс и выбранные языки** — Audio/Localization; GPT-6 Sol · medium · 12–24k; после решения о языках: важные сигналы слышны, строки помещаются, права проверены.
- **S4-QA-01 · Release candidate regression** — Platform/QA; GPT-6 Sol · high · 28–50k; после исправлений: clean-machine run и поддержанные данные проходят полный чеклист.

Stage 4 allocation: **120–218k**.

### Stage 5 — Steam Readiness packages

Human/account actions remain with the owner; the model prepares checklists,
copy, scripts and evidence. Recheck current Steamworks rules when each task
starts. Store preparation can overlap earlier stages after visual/core lock.

- **S5-ADMIN-01 · Steamworks onboarding** — Human/Platform; GPT-6 Sol · medium · 8–16k; до публикации: владелец подтверждает account, legal/tax/banking и app-fee status по актуальному Steamworks checklist.
- **S5-STORE-01 · Store copy, assets, survey** — Marketing/Design; GPT-6 Astra · high · 16–30k; после стабильного визуала: страница обещает только функции текущей сборки, материалы имеют права.
- **S5-PAGE-01 · Review и Coming Soon** — Human/Marketing; GPT-6 Sol · medium · 10–18k; после S5-STORE-01: страница отправлена, замечания закрыты, обязательные сроки проверены.
- **S5-DEPOT-01 · SteamPipe/depot/branch** — Platform; GPT-6 Sol · high · 20–36k; после build contract: загрузка в тестовую ветку воспроизводима, launch options верны.
- **S5-BUILD-01 · Near-final build review** — Platform/QA; GPT-6 Sol · high · 14–26k; после S4-QA-01: сборка соответствует store claims и одобрена по актуальному процессу.
- **S5-SMOKE-01 · Steam install и rollback** — Platform/QA; GPT-6 Sol · high · 16–28k; из Steam на чистом Windows ПК: install, launch, update и возврат ветки проверены.
- **S5-GATE-01 · Финальный Steam gate** — Human/Design; GPT-6 Astra · high · 12–22k; после S5-PAGE-01/S5-BUILD-01/S5-SMOKE-01: человек подтверждает готовность выпуска.

Stage 5 allocation: **96–176k**. Account fees and other monetary costs are
outside this token envelope.

### Stage 6 — Release And First Patches packages

- **S6-RC-01 · Freeze, backup, final smoke** — Platform/QA; GPT-6 Sol · medium · 8–14k; после S5 gate: известный build hash, backup, финальный запуск и rollback evidence.
- **S6-LAUNCH-01 · Release action** — Human/Platform; GPT-6 Sol · medium · 6–12k; после S6-RC-01: владелец запускает release в Steam и фиксирует результат.
- **S6-MONITOR-01 · Crash/feedback triage** — Platform/Support; GPT-6 Luna · medium · 12–24k; после выпуска: группировка воспроизводимых проблем и приоритет по влиянию на игроков.
- **S6-PATCH-01 · Проверенный hotfix (условно)** — Owning system/QA; GPT-6 Sol · high · 24–44k; только при подтверждённом дефекте: узкий фикс, регрессия, публикация и rollback.
- **S6-EXIT-01 · Стабилизация и остаток** — Design/QA; GPT-6 Astra · medium · 10–18k; после первой волны патчей: launch blockers закрыты, backlog разделён на patches/post-MVP.

Stage 6 allocation: **36–68k** without a hotfix; add **24–44k** per activated
S6-PATCH-01 cycle.

### Stage 7 — Post-MVP Decision packages

These are evaluations, not promised systems. Open a separate feature brief and
budget only after a human go/no-go. D-012 keeps detailed Blender work here.

- **S7-EVIDENCE-01 · Результаты MVP** — Design/Analytics; GPT-6 Astra · high · 18–34k; по playtest/sales/feedback: явные проблемы и сильные стороны до выбора нового scope.
- **S7-VISUAL-01 · Решить формат Blender-ассетов** — Art/Design; GPT-6 Astra · xhigh · 20–38k; после S7-EVIDENCE-01: человек выбирает 2D sprite source либо runtime 3D и при необходимости supersedes D-003.
- **S7-BLENDER-01 · Пилот одного ассета (условно)** — Art; GPT-6 Astra · high · 25–45k; только после S7-VISUAL-01: один лицензированный/оригинальный asset проходит Blender-to-game проверку до массового производства.
- **S7-SYSTEMS-01 · Оценить tutorial/tools/automation/meta** — Design; GPT-6 Astra · high · 20–38k; после данных MVP: каждое направление получает отдельный benefit/cost/gate, без автоматического принятия.
- **S7-PLATFORM-01 · Другие платформы** — Platform/QA; GPT-6 Sol · high · 12–24k; при запросе на порт: проверить стоимость поддержки и тестовую матрицу до обещания.
- **S7-GATE-01 · Выбрать следующую итерацию** — Human/Design; GPT-6 Astra · high · 14–26k; после оценок: человек принимает только одну готовую следующую цель и её ресурсный предел.

Stage 7 allocation, only if activated: **84–160k** for evaluation; add
**25–45k** for the conditional Blender pilot. This is a decision budget,
not authorization to implement all post-MVP candidates.

Across mandatory remaining Stage 2–6 packages the initial visible/context
allocation is **923–1,682k tokens**. Conditional run-save and one hotfix would
add **46–84k**. Stage 7 is separate. These are sums of planning envelopes,
not a forecast of actual Codex usage or a spending target.

## Stage Opening Prompts

Use one Codex dialogue for each stage. Paste the corresponding prompt once at
the start of that dialogue; continue its tasks in the same dialogue. The task
list above remains the source for each package's model, reasoning level and
token envelope. A prompt cannot change Codex UI settings by itself: select the
listed model and level for the next task in that dialogue. Stage 5 preparation
may start early in its own dialogue when its inputs are ready. These prompts
do not waive human decisions or stage gates.

### Stage 2 — стартовый промпт

```text
Веди Stage 2 проекта Not 3A в этом диалоге до решения по gate. Сначала прочитай AGENTS.md, GAME_VISION.md, DEVELOPMENT_PLAN.md, DECISIONS.md, WORKSTREAMS.md и STAGE2_VERTICAL_SLICE.md; проверь Git, Unity-сцены, тесты и фактический статус. Stage 0–1 завершены, S2-MAP-01, S2-INT-01 и S2-MAP-02 закрыты: не повторяй их без нового дефекта. Иди по готовым пакетам Stage 2 из DEVELOPMENT_PLAN.md в порядке зависимостей, по одному пакету реализации за раз. Перед каждым пакетом назови ID, рекомендуемые модель/уровень из плана, владельца поверхности, входы и критерий готовности; настройки модели и уровня я выбираю в этом же диалоге. Реализуй и проверяй пакет, обновляй канонические документы и handoff. Сначала собери на реальной карте цикл добыча → оплаченное строительство → маршрут/осада → трон → исход/рестарт, затем измеряй масштаб и темп, доводи читаемость и проведи внешний playtest. Детальные Blender-модели не входят в Stage 2. По gate предъяви сборку, тесты, наблюдения игрока и конкретное решение continue/rework/stop; не объявляй этап закрытым без этих данных.
```

### Stage 3 — стартовый промпт

```text
Веди Stage 3 MVP Alpha проекта Not 3A в одном диалоге. Сначала прочитай AGENTS.md, GAME_VISION.md, DEVELOPMENT_PLAN.md, DECISIONS.md, WORKSTREAMS.md и результат Stage 2 gate; проверь Git и Unity. Если Stage 2 не принят, покажи недостающие данные и не начинай Alpha. Начни с S3-SCOPE-01: подготовь для моего решения ровно тот состав MVP, который подтверждён playtest и ограничениями GAME_VISION.md. Затем выполняй готовые пакеты Stage 3 по зависимостям, по одному пакету реализации за раз. Перед каждым назови ID, модель/уровень из плана, владельца, входы и критерий; настройки я выбираю в этом диалоге. Не расширяй content ceiling и не запускай детальный Blender pipeline. S3-SAVE-01 выполняй только при принятом P-005. После каждого пакета проверяй поведение, тесты и Console, обновляй канонический план и handoff. Заверши Alpha полным забегом, проверкой пути обновления и списком блокеров/рисков для следующего gate.
```

### Stage 4 — стартовый промпт

```text
Веди Stage 4 Beta и полировку Not 3A в одном диалоге. Проверь AGENTS.md, GAME_VISION.md, DEVELOPMENT_PLAN.md, DECISIONS.md, WORKSTREAMS.md, принятый Stage 3 gate, Git, Unity и результаты playtest; если Alpha не принята, сначала покажи блокирующие пробелы. Начни с content lock и triage по доказанному влиянию на игрока. Затем иди по готовым пакетам Stage 4, один пакет реализации за раз; перед каждым указывай ID, модель/уровень из плана, владельца, зависимости и критерий, чтобы я выставил настройки в этом же диалоге. Исправляй ясность игровых состояний, производительность, Windows/input/resolution и аудио/локализацию без расширения контента и без позднего Blender-перехода. Для каждого исправления оставляй проверку и обновляй канонические документы/handoff. Заверши этап release candidate regression и clean-machine проверкой; покажи оставшиеся риски до решения по gate.
```

### Stage 5 — стартовый промпт

```text
Веди Stage 5 Steam Readiness Not 3A в одном диалоге. Проверь AGENTS.md, GAME_VISION.md, DEVELOPMENT_PLAN.md, DECISIONS.md, WORKSTREAMS.md, текущую сборку, Git и актуальные официальные правила Steamworks. Подготовку страницы и аккаунта можно начать до окончания Stage 4, когда зафиксированы визуал и функции; финальный gate требует принятый release candidate. Иди по готовым пакетам Stage 5, перед каждым назови ID, модель/уровень из плана, владельца, входы и критерий; настройки я выбираю в этом диалоге. Готовь copy, материалы, чеклисты, depot/branch и доказательства install/update/rollback. Юридические, банковские, ценовые и публикационные действия оставляй владельцу проекта. Сверяй обещания страницы с реальной сборкой и проверяй сроки/review заново. Не объявляй Steam gate пройденным без подтверждения страницы, сборки и проверки установки через Steam.
```

### Stage 6 — стартовый промпт

```text
Веди Stage 6 выпуска и первых патчей Not 3A в одном диалоге. Сначала проверь AGENTS.md, DEVELOPMENT_PLAN.md, DECISIONS.md, WORKSTREAMS.md, принятый Steam gate, Git, точный build hash, backup и rollback; без этого не переходи к выпуску. Иди по пакетам Stage 6 в порядке зависимостей, перед каждым указывай ID, модель/уровень из плана, владельца и критерий, чтобы я выставил настройки в этом же диалоге. Подготовь финальный smoke и выпускной чеклист; само действие release выполняет владелец проекта. После выпуска группируй реальные crash/feedback, приоритизируй по влиянию и делай S6-PATCH-01 только для подтверждённого дефекта с узким тестом и путём отката. Обновляй канонические документы/handoff. Заверши этап перечнем закрытых launch blockers и разделением остатка на патчи и post-MVP.
```

### Stage 7 — стартовый промпт

```text
Веди Stage 7 решений после MVP Not 3A в одном диалоге только после моего явного решения открыть этап. Прочитай AGENTS.md, GAME_VISION.md, DEVELOPMENT_PLAN.md, DECISIONS.md, WORKSTREAMS.md и данные playtest, продаж и отзывов; проверь Git и текущее состояние проекта. Начни с S7-EVIDENCE-01 и оцени направления по отдельности, без обещания реализовать их все. Перед каждым пакетом назови ID, модель/уровень из плана, владельца, входы и критерий; настройки я выбираю в этом диалоге. Для Blender сначала вынеси на моё решение формат: источник 2D-спрайтов или runtime 3D, включая последствия для D-003. Только после этого и отдельного принятия бюджета делай пилот одного ассета через GPT-6 Astra. Не запускай массовое производство моделей, automation, meta-progression или портирование по умолчанию. Заверши сравнением вариантов, затрат и проверяемых gate; попроси выбрать одну следующую цель.
```

## Documentation At Each Gate

- GAME_VISION.md and DECISIONS.md own stable player/product truth.
- This file owns stage status and gates; WORKSTREAMS.md owns task claims and
  handoffs. Do not create a parallel roadmap or QA tracker.
- One active feature spec defines a nontrivial mechanic when implementation
  starts. Existing Stage 1 specs remain evidence, not new Stage 2 mandates.
- Technical architecture is written only after a durable boundary is measured.
  Unity data assets own runtime balance; documentation owns intent and rules.
