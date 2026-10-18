# Feature: Continuing Days, Night Hordes And Dwelling

Status: D-065 implemented and verified as a prototype, 2026-10-18.
D-067 clock/daylight clarification is included in the D-066 interface integration.
Owner: current chat owns Run cadence, dwelling transaction/input and wiring;
GPT-6 Astra owns all visuals, HUD, hero/equipment and actual-frame QA.
Dependencies: D-064 bow/hordes, shared Run/Defense/EnemyGroup, gathering,
production, green-ground collision, workshop/travel sites and fixed camera.

## Accepted player behavior

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
D-062 rewind remains separate and unimplemented; its future snapshot must include
day/night, D-067 WorldMinutes, dwelling and weapon state. Current R still resets the entire run.

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
