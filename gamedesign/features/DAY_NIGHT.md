# Current cycle update — D-074/D-087/D-088

## D127 — paid dwelling construction,2026-10-23

Owner changes current dwelling price to20wood20stone in DwellingCatalog. Both
materials are required atdraft andcommit; cancellation/invalidplacement spends
nothing. Sleep still stops at19:50 with the existing threat/localchecks and no
skippedincome. Actualmanualharvest→paidhome and lifecycle checks cover the new
price; no save/scene/Inspector migration. D127 researchcentre contract is inRESEARCH.


## D123 current portal tuning, 2026-10-22

Current portal enemies use 5 HP; first night spawns every 4 seconds. Later existing
intervals shorten with the same HP, per the owner's follow-up. No new difficulty
curve beyond that trial change. Ten-minute day/three-minute streaming night,
last-won daytime profile, cutoff, sleep, cleanup and retry remain as below.
XP/gold reduction is D121; values stay in the current data assets.

## D-106 — timed day/night stream,2026-10-20

Current Stage3 day is10active real minutes (08:00–20:00); night is3
(20:00–08:00) at normal speed. Pause/speed apply to both. Mandatory night
spawns continue at the pending tier's interval/HP/type for the whole night,
independent of the old authored enemy count. Dawn stops new spawns; living
creeps/shots remain. The tier clears only after dawn AND all threats resolve.
An unresolved horde blocks day farming/sleep and never queues overlapping nights.
D105 day gates still repeat the last WON tier until close or19:30; D088 sleep
ends19:50 without skipped-time income. D087 retry keeps current development
and gives a full morning before the same pending tier.

HordeRun owns600seconds of day and180seconds of night. New optional
nightDurationSeconds=0 preserves finite legacy Stage2 behavior; no scene or
package migration. WaveEnemyCount is a full-night forecast for old observers,
never a timed-night quota. Existing cadence/HP profiles remain trial tuning;
tier2's0.12s interval now means about1500spawns per full night, so old finite
combat budgets do not automatically qualify this balance.
Qualified2026-10-21:36EditMode+26PlayMode PASS, including first-night bow and
boundedpaiddefense tiers2/3. Native D107 phase fixture confirms dawn/live-creep
behavior. Trial balance and Windows/sustained natural playtest remain separate.
Evidence:art/source/timed_nights_2026-10-20_verification.json.


D105 clarifies the optional day stream: last WON night difficulty, continuous
spawning until manual close or19:30, no finite night quota/old live cap. Losing
the next night does not unlock it for farming. Initial night trial buff lives
in HordeRun. Current contract/qualification: PORTAL_CONTROL.md.

Implemented and locally qualified2026-10-19: mandatory nights use continuous game time;
early victory does not jump to dawn. Day gates repeatedly farm the last cleared
tier, close before night and retain spawned enemies. Explicit dwelling Sleep
ends at19:50 and is blocked by an open stream, live creeps/shots or active horde.
D-087 defeat recovery keeps current world/hero progression and returns morning
for the same pending tier. The older rest/full-world-snapshot behavior below is
superseded for Stage3; see PORTAL_CONTROL.md and DEVELOPMENT_PLAN.md.

# Feature: Continuing Days, Night Hordes And Dwelling

## Historical D-074 target — sleep revised by D-088

Владелец подтвердил отмену обоих пропусков: дом больше не заканчивает день,
ранняя победа не переводит часы к рассвету. Ночью приходит обязательная
усиливающаяся орда; днём можно многократно открывать/закрывать непрерывный
поток последнего пройденного уровня. Предночная отсечка 30 игровых минут,
обработка затянувшейся орды и дневной checkpoint предложены для проверки.
Закрытие не удаляет уже вышедших врагов. Дом сохраняется, следующая его
функция не выбрана. Текущая длинная ночь требует проверки темпа без пропуска.
Полный новый контракт: `PORTAL_CONTROL.md`; последовательность внедрения —
`../DEVELOPMENT_PLAN.md`. Ниже сохранено историческое поведение D-065, заменённое текущим контрактом.

## Implemented prototype baseline

Status: D-065 implemented and verified as a prototype, 2026-10-18.
D-067 clock/daylight clarification is included in the D-066 interface integration.
Owner: current chat owns Run cadence, dwelling transaction/input and wiring;
GPT-6 Astra owns all visuals, HUD, hero/equipment and actual-frame QA.
Dependencies: D-064 bow/hordes, shared Run/Defense/EnemyGroup, gathering,
production, green-ground collision, workshop/travel sites and fixed camera.

## Implemented player behavior — D-065/D-067

Each preparation gives about twenty active game minutes for exploration and
gathering. Expiry starts the next portal horde. Winning early advances the clock
to dawn. By D-067, a horde that lasts past 08:00 continues in daylight while the
clock keeps advancing; it is never frozen at 07:59. Preparation starts only
after the entire authored horde and pending attacks are resolved, with its full
duration, preserving the world, buildings, inventory and objective health.
After the initial sequence, days continue with stronger hordes; no third-night
victory screen. Later-night tuning is provisional, delegated data.

The player constructs a dwelling on free green ground for ten wood. It is not
prebuilt and is not an attack tower. At its actual front doorway E ends the
remaining DAY immediately. It does not simulate the skipped time: production,
resource recovery and gathering receive no skipped-time payout. Ordinary active
time still advances these systems normally. Rest is unavailable while any
horde remains active, including after clock dawn,
during pause/debug/construction and after defeat; held E must be released before
another rest. The ordinary remote Space/start-wave control is removed for this
cycle; development tools retain an explicit force-start operation.

Placement rejects road/borders, hero, resource districts/pads/solids, existing
buildings, manual resource nodes and reserved workshop/teleport/landmark sites.
The entrance must remain accessible. Payment and occupancy happen once; preview
cancel/failure does not spend. One home is supported by this prototype. Full
restart/regeneration clears it and its collision. No house attack, inventory
storage, health, healing or hero respawn mechanic is added.

The owner selected hero C as a versatile fantasy hero, not an archer class.
Bow is initial equipment; its artwork stays separate from the body. Future
magic, other weapons and hero progression need their own feature scope.
Approved dwelling B and the cool readable night follow ImageGen concepts;
Astra implements them without changing the fixed camera or existing asset identity.

## Runtime contracts

Stage2Run keeps its existing Preparation/Wave state machine: ContinuousDays maps
them to preparation/active horde and continues beyond the authored waves. D-067
WorldMinutes / CalendarDay / ClockHour / ClockMinute / IsNightTime own the
display clock and daylight independently. An overnight clearance advances to
08:00; a daytime clearance preserves clock time. Every clearance still grants
the full preparation duration, with no overlapping waves or in-horde rest.
Legacy finite scenes
retain their old sequence and controls. Stage2RunCatalog owns duration, initial
hordes and linear extra enemies for later nights; values live in HordeRun.asset.
Pause uses the existing scaled game clock, including development speed controls.

Stage3Dwelling owns placement, collision, payment and local E/rest conditions.
Stage3DwellingPresentation owns prepared inactive art, matched preview/built
registration and HUD binding. Stage3DwellingCatalog owns price/footprint/door
and approved art references. The ordinary fresh start grants no showcase towers.

Stage3DayNightSliceBuilder.BuildAndWire imports approved art, migrates the Stage 3
run data once and binds the dwelling/hero/night material in the saved scene.
It preserves later data tuning. BuildWindowsPlayer builds this saved scene into
Builds/WindowsDayNight/ without regenerating camera or world artwork.
D-062 retry is implemented in the shared pre-launch boundary. It restores the
failed night’s world, dwelling, weapon cooldown and checkpoint day, then starts
that day at dawn with full preparation. Rewinding WorldMinutes grants no income
or recovery ticks. After the white transition, R retries; development full reset
remains separate. Contract and qualification: `STAGE2_RUN_STATE.md`.

## Observable acceptance

- Timer expiry, pause and manual local rest produce one correct phase transition;
  no ordinary remote Space launch and no night skipping or held-E dawn replay.
- Natural fresh start has no granted towers/home/materials; ordinary gathering
  can pay for the home. Valid placement costs once and blocks hero ground;
  invalid/cancel/UI clicks preserve funds, safe doorway and mutual construction.
- Rest preserves production progress/resource-recovery clocks without a payout.
  Days beyond the initial sequence continue with increasing enemy counts.
- Reset, defeat and world regeneration clean up home collision/previews/state.
- Hero body/equipment, house, HUD and night remain readable in actual renders;
  relevant EditMode/PlayMode checks, Windows build/run and canonical handoff pass.

Source prompts, approvals, rights and hashes:
art/source/day_night_and_home_2026-10-18_generation.json.

## Verified result

RunState/Defense EditMode 30/30 and focused PlayMode 39/39 pass, including the
real E-gathered home purchase, mouse/UI/cancel checks, local day-only rest,
production/recovery preservation and a continuing scenario through five nights.
After Astra's final night-tint/window-mask refinement, visual PlayMode 1/1 passes.
Actual D3D11 frames show complete hero C/equipment, matched home preview/built,
readable night and restored daylight. Windows x64 Development builds and starts
with all readiness markers, no showcase grants and no game errors beyond 60s.
The opt-in full-player screenshot includes the ordinary fresh-start IMGUI HUD.

Timer boundaries/long-cycle checks use manual ticks and QA kills; the material
purchase has a separate actual keyboard-gathering check (development x10 time).
Art framing uses declared QA resources and manual positions. These are not a
twenty-minute manual playthrough or universal survival-balance/performance proof.
Enemy crowd overlap and D-062 rewind remain unchanged. Exact setup/results:
art/source/day_night_2026-10-18_verification.json; local XML/logs under
Artifacts/S3-DAY-NIGHT-20261018/.
