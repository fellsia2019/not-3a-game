# D-104 development hero-death command

Existing F3 development viewport adds «Проверить смерть героя» beneath WTF.
It invokes the same exploration-damage/reincarnation rules, with lethal damage;
no XP, ranks or bag items are granted. An empty bag creates no false drop marker.
Only development builds, live unpaused run, safe base spawn required. No ordinary
hotkey or portal-enemy damage was added. The command is integration-tested;
native button appearance AstraPASS; actual E pickup/I bag return and F3 toggle
are qualified. Coordinate mouse clicks and Windows are not claimed.

# Feature: Stage 3 Player HUD And Development Menu

## D-103: explicit test launch and WTF

The Stage3 F3 next-wave command now explicitly advances the clock to the next
pending night and starts it once, including on day1. It credits no skipped-time
income or regeneration; living daytime creeps/shots remain, and the daytime
gate closes. Pause/active wave/outcome block launch. Ordinary clock-gated wave
start remains unchanged; older normal-preparation command descriptions below
are historical for Stage3. Legacy finite Stage2 retains its normal start rule.

The development-only WTF toggle sets the current active ability Blink to0MP
and ZERO cooldown, immediately clearing an existing cooldown. Disabling restores
normal rank costs/timings. Full Reset or disabling the bound HUD clears WTF.
Pause, modal UI and valid landing checks still apply; it grants no ranks/XP or
mana refund. Existing scrollable debug panel/frame is reused. Verification:
art/source/hero_vitals_2026-10-20_verification.json. Windows needs qualification.

Status: implemented and Windows-verified, owner-requested 2026-10-16 (D-052). Workstream: UI/UX +
Integration. Surface: scene-owned HUD, player movement multiplier, existing
build/run/economy bindings and pointer exclusion. Dependencies: initialized
Stage2GameComposition, Input System, existing IMGUI/minimap and Stage 3 scene.
No map, economy/combat tuning, package or serialized scene migration.

## Accepted behavior

The owner requested a usable debug menu: launch waves, pause, speed up to x10,
grant gold and all materials, and remove or improve the temporary windows.
The owner explicitly chose two independent multipliers: hero movement and game
simulation. Each is x1–x10; effective hero travel combines both. The authored
base movement speed remains unchanged. Simulation acceleration affects all
scaled-time gameplay, including gathering, recovery, waves, towers and enemies.

Development builds expose a button and F3. The menu starts hidden; opening it
alone does not pause. Commands: pause/resume; start the next wave through the
normal preparation rule; separate integer sliders; +100/+1000 gold or each of
wood/stone/metal; restart. Wave start is disabled during a wave, pause or outcome;
it never overlaps or skips authored waves. Grants use existing inventory methods,
saturate safely at integer limits, and do not persist across a run restart.
Multipliers remain across run restart, return to x1 when the HUD is disabled or
the scene exits. Pause stops the hero and scaled simulation, keeps menu/input
usable, restores the selected game speed on resume and clears on restart.
The component restores the prior global timeScale on disable/scene exit.
No save format, new currency or balance preset is introduced.

The normal player HUD remains present in non-development builds. Current
Stage3 uses D-066/D-068 lunar porcelain, sans-serif and safe content bounds:
resource counts, day/ordinary clock, normal hero dock or nonmodal B construction,
I/P/O/M windows and useful local hints. Global throne health/countdown and
ordinary remote wave start are removed. PLAYER_INTERFACE.md owns this surface.
The original dark-panel Stage2GameHud drawing remains a legacy fallback when
the new scene-local presentation is not bound; it is not the current visual
target. Source stock/distance tables and internal placement enums stay hidden.
Nearby gathering hints appear only for a reachable available source; placement
failures receive player-facing text. The passive minimap keeps its current role.
Debug controls are absent when Debug.isDebugBuild is false or the existing
showDevelopmentHud flag is false; that flag no longer hides player information.

## Wiring and implementation

Stage2GameComposition adds/binds one Stage2GameHud after its systems initialize,
and owns the union of HUD/minimap pointer exclusions for the build/camera
drivers. Minimap teardown no longer clears another panel's blocker. HUD controls,
status cards, tower dock and result card block placement and wheel zoom through
their visible areas. Layout scales with viewport; debug content scrolls in short
windows. No Inspector operation or scene/prefab regeneration is required.

Stage2PlayerMover retains serialized speed and accepts a runtime multiplier.
Stage2BuildDriver.SelectAndBeginPlacement uses the existing role/preview rules
and updates ghost/gathering binding. HUD commands call Run.StartWave,
RunDriver.ResetRun, SetPaused and inventory grants rather than duplicate rules.
Existing IMGUI is retained for this scoped improvement; the commercial UI stack
is still open. All colors/graphics here are original code-native UI primitives.

## Observable acceptance and verification

- F3 and menu commands work at timeScale zero; resume/restart/scene exit restore
  a usable game. World movement and enemy updates freeze while paused.
- Separate movement and simulation changes work in a real scene and preserve
  the serialized base speed; both clamp to x1–x10.
- Grants fund an actual paid tower transaction, handle integer limits, and clear
  on restart. Active/paused wave start cannot spawn an extra group.
- Real click/wheel input over debug UI cannot place a tower or zoom; minimap
  exclusion remains active alongside it and after HUD disable.
- Ordinary HUD and the open debug menu are visually inspected in Game captures;
  a rebuilt Windows player starts without new attributable errors.
- Existing Stage3/Integration/RunState regression stays green; canonical status
  and handoff record actual results. Owner appearance feedback remains separate.

Windows verification 2026-10-16: Economy/Defense/RunState EditMode 29/29;
Stage3/Integration/RunState PlayMode 52/52 (including five new HUD scenarios),
final focused HUD run 5/5, no skips. Windows Development build succeeded
(274435971 bytes); actual player HUD and open paused menu captured at 1600×900.
The menu layout was tightened after its restart button was partly clipped;
final capture includes every command without scrolling at that size. Short
windows retain scrolling. Initial test-only WaitForEndOfFrame failure in
batchmode is retained in HudTests.xml; runtime Game captures are used for visual
QA instead. Native window automation could not initialize its Node kernel
(OS error 3); manual native clicking is degraded, with Input System click/wheel
exclusion and actual command/state transitions covered by PlayMode instead.
Evidence: Artifacts/S3-DEBUG-HUD-20261016/. Owner appearance feedback remains.
