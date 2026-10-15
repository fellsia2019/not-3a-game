# Workstreams And Chat Handoffs

Этот файл — единая координация отдельных чатов. Один активный владелец на
систему/сцену/контракт; параллельные агенты не редактируют одни и те же файлы.
Для новой работы читать текущий блок и handoff затрагиваемой системы. Старые
claims ниже открывать только при разборе соответствующего контракта/дефекта.

D-017: каждый новый Stage 2–7 ведёт новый агент в новой задаче Codex. Он
сверяет предыдущий gate и использует стартовый промпт соответствующего этапа
в `reports/development-roadmap.html`; внутри stage мелкие задачи не дробятся
по агентам. На переходе обновляются этот handoff и канонический план.

### 2026-10-15 — Approved resource sprites in the game

- Owner/surface: current Stage 3 chat; three sprite imports/prefabs, narrow
  camera-facing resource view, existing tactical resource binding/builders,
  actual-scene resource checks and canonical art/spec docs.
- Authorization: owner approved all three D-048 previews and explicitly asked
  to put them in the game. Use those exact PNGs, preserving their source files.
- Dependencies/acceptance: keep the existing wood/stone/metal anchors, gather
  interaction, stocks/recovery, minimap and run reset; visible grounded sprites
  at opening/close zoom, no gameplay colliders, no duplicated 3D model; preserve
  the current map, terrain, camera and earlier changes. Standard builders must
  reproduce sprite wiring. Verify lifecycle/real rendering and live Console.
- Implementation: exact approved PNG copies and three new prefabs in
  `Assets/Stage3/ResourceSpritePilot/`, shared existing unlit sprite treatment,
  alpha-based visible scale/custom footing pivots and ground-anchor sorting.
  Existing scene changed only its three resource visual references. Normal
  Stage 3/Cartoon World rebuilds retain the sprites; 3D sources stay preserved.
- Verification: Mac/Metal Stage 3 + Integration + RunState PlayMode 46/46 passed
  (42.62 s, no skips). Updated legacy mesh checks cover one visible sprite/no
  duplicate mesh or collider, original anchors/grass relief, size/pivot/camera
  alignment, depletion/recovery/reset and actual rendered pixels at size 5/2.5.
  Existing minimap, input/trackpad, building, combat, full run/restart checks pass.
  All six default/close Game-camera captures inspected. Interactive Game review
  visited each resource with the nearby E-gather prompt; Console 0 errors and
  0 warnings. All 90 preexisting review/settings files restored/hash-verified;
  fresh captures kept separately. Accepted road/map remains byte-identical.
  Evidence: `Artifacts/S3-RESOURCE-SPRITES-20261015/`; shared frames/result:
  `art/review/resource_sprite_*2026-10-15*`. PNG copies/source provenance:
  `art/source/resource_sprites_2026-10-15_provenance.json`.
- Remaining: owner live appearance feedback; no implementation blocker. Editor
  is running beside the ore node. No Windows build/run performed on this Mac;
  wider Stage 3 gate remains open.

### 2026-10-15 — Remove resource direction labels; resource sprite previews

- Owner/surface: current Stage 3 chat; resource-direction rendering in
  `Stage2GameComposition`, original ImageGen previews and canonical art/spec docs.
  Depends on the current cartoon B scene and approved D-037/D-038 resource shapes.
- Authorization/acceptance: remove floating `Stone W+A` / `Wood W+D` labels;
  prepare previews now (owner clarified the voice-transcribed timing). Keep the
  conifer, pale granite boulder and rust-seamed iron outcrop identities, rendered
  as cartoon sprites for review. This request is previews, not runtime replacement.
- Implementation: remove floating resource boxes and keyboard-direction suffixes
  from the temporary source readout. Resource counts/distances, nearby gathering
  prompt, minimap markers and economy remain existing behavior.
- Verification: refreshed/compiled in Unity and inspected the real live Game;
  floating resource boxes and direction suffixes are gone, minimap/resource
  counters remain, Console 0 errors/0 warnings. Actual frame:
  `art/review/resource_direction_labels_removed_2026-10-15.png`.
  No new tests or broad rerun for this label-only deletion. Three distinct
  original RGBA previews saved unchanged under
  `art/concepts/resource_sprite_review_2026-10-15/`; transparent corners and
  visible content verified. Stone/ore had a separate background-cleanup pass;
  prompts/source/output hashes: `art/source/resource_sprite_preview_2026-10-15_*`.
  Existing runtime resource prefabs match pre-change HEAD. Summary/evidence:
  `art/review/resource_preview_2026-10-15_verification.json`,
  `Artifacts/S3-RESOURCE-PREVIEW-20261015/`. Earlier minimap capture restored
  byte-identically after the new frame was saved separately.
- Remaining: owner assessment of the three previews. No runtime sprite
  replacement in this request. Wider UI and Stage 3 gate remain open.

### 2026-10-15 — Bottom-left minimap

- Owner/surface: current Stage 3 chat; narrow minimap presentation in Integration,
  Stage 3 binding, pointer exclusion in build/camera input and relevant tests.
  Depends on authored map cells, existing actor/run state and tactical camera.
- Request/acceptance: owner authorized this UI element now while the wider UI
  remains deferred. Bottom-left map must show the whole road/world, live hero,
  portal/throne, resources, towers/enemies and current camera footprint; follow
  movement/zoom/reset, resize within the screen and prevent building through it.
- Approach: cached schematic terrain and live markers in the existing immediate
  HUD path; no extra world-rendering camera, packages or final UI stack migration.
  Passive map only: no new movement command, camera pan, fog or exploration rule.
- Implementation: `Stage2Minimap` caches authored terrain and reads live marker
  state; `Stage3TacticalPresentation` creates/binds it at runtime. Existing
  build/camera drivers take a scene-owned pointer blocker, with teardown cleanup.
  No scene/material/map asset or package changed for this feature.
- Verification: Mac/Metal Stage 3 + Integration + RunState PlayMode 46/46 passed
  (42.26 s, no skips). Three new tests cover every terrain cell, live movement/
  actor removal/resource depletion/recovery/reset, viewport layout/edge zoom and
  an otherwise valid purchase blocked through the panel, with outside purchase
  still succeeding. Existing 43 camera/trackpad/gameplay checks passed.
  Interactive Game captures at 819×556 and 1206×614 were visually checked;
  real WASD movement moved the hero/camera outline and ordinary wave creeps
  appeared along the zigzag. Final observed Console: 0 errors, 0 warnings.
  The first batch screenshot attempt had no displayed Game surface; visual
  capture moved to the interactive Editor and passed. Raw initial evidence kept.
  All 85 preexisting review/settings files restored and hash-verified after
  retaining fresh test captures separately. Evidence: `Artifacts/S3-MINIMAP-20261015/`;
  shared image/summary: `art/review/minimap_{game,2026-10-15_verification}*`.
- Remaining: owner appearance feedback; no implementation blocker. Editor is
  open in a fresh ordinary Play review, original panel layout restored. Windows
  build/run not performed on this Mac. Wider UI and Stage 3 gate remain open.

### 2026-10-15 — Mac trackpad pinch zoom

- Owner/surface: current Stage 3 chat; `Stage2CameraFollow`, narrow AppKit
  adapter/native library + source/build script, importer metadata and camera
  regression test. Depends on existing tactical zoom, camera clamp and Input System.
- Authorization/acceptance: owner clarified two-finger pinch/spread on the Mac
  trackpad. Continuous zoom shares wheel smoothing and 0.5×..2× bounds; wheel
  remains usable. Native monitor acts only over the focused live Game view and
  clears on focus loss, Editor pause, Play exit/domain reload. Windows excludes
  this binary and never calls the native adapter.
- Verification: native source compiled universal ARM64/x86_64 and ad-hoc signed;
  native AppKit monitor/ratio/reset/invalid-input checks passed. Unity importer
  configured macOS only. Stage 3 + Integration + RunState PlayMode passed 43/43
  (39.38 s, no skips), including actual native loading, zoom bounds, disable/
  resume and existing wheel/movement/combat regressions. All 84 preexisting
  art/review/settings files restored and hash-verified; fresh test captures kept
  separately. Live Mac/Metal Game view is open in Play; observed Console has
  0 errors and 0 warnings. Evidence: `Artifacts/S3-TRACKPAD-20261015/`;
  shared summary: `art/review/trackpad_zoom_2026-10-15_verification.json`.
  Owner physically tested pinch/spread in the live Game view and confirmed
  "Да, работает"; the live log also records native `S3_PINCH_INPUT` samples.
- Remaining: no open work for this input request. Intel slice compiled/signed
  only; Windows build/run not performed. The wider Stage 3 appearance/pacing
  review stays open.

### 2026-10-15 — Accepted invasion road and lower-left portal

- Owner/surface: current Stage 3 chat; existing Stage 3 map builder/asset,
  opt-in map/navigation contract, live review framing and affected tests/docs.
  Depends on I's owner-accepted shape, existing hero movement, paid tower
  occupancy, enemy detours/siege and camera-facing landmark sprites.
- Authorization: keep I's horizontal double rectangular zigzag, seven tower
  footprints across. The owner allowed a lower-left portal if relocation is
  straightforward; spawning and landmarks already consume authored map anchors,
  so use that option. No new drawing or upper-right redraw is necessary.
- Implementation: existing `Stage3PlanarCorner` in the 100×100 world, nine
  connected seven-cell strips/eight right-angle bends, updated endpoint
  reservations and gray-only building mask. `EnemiesStayOnInvasionCorridor`
  restricts enemy navigation while keeping hero/resource walkability intact.
  Default false preserves earlier Stage 2 routing/hashes. Existing Stage 3
  rebuilds reproduce the new layout. Cartoon World live review frames the portal.
- Observable acceptance: width seven on all nine reaches; one connected road;
  enemy traverses both detours without grass shortcuts, six towers leave a
  passage and seven cause siege, release/reset restore navigation; portal
  spawns enemies, faces the Game camera and renders above the paving.
- Verification: Mac/Metal Map+Navigation EditMode 46/46 (0.55 s) and Stage 3,
  Integration+RunState PlayMode 42/42 (33.17 s), no skips. Existing short-road
  capture timing/off-screen pointer setup updated; capture pre-roll uses normal
  run/enemy ticks before combat, and full-run guard allows the longer route.
  Ordinary combat, victory/restart and the complete unopposed enemy traversal
  passed. Width tested transversely on all nine reaches; portal front/base
  visibility checked by the actual Game camera. No combat tuning changed.
  Actual route: 196 visited cells/195 steps, direct endpoint distance 91.24 cells;
  initial 17.20-cell concept target is not the scale of this blockout. At the
  existing 2.2-cell/s movement this is roughly 89 s before frame rounding/detours.
  Evidence: `Artifacts/S3-INVASION-ROAD-20261015/`; shareable captures and summary:
  `art/review/invasion_route_{game_overview,portal_lower_left}_2026-10-15.png`,
  `art/review/invasion_route_2026-10-15_verification.json`. All 81 preexisting
  reference/settings files were restored and hash-verified after fresh captures
  were retained separately. Windows was not rebuilt/retested on this Mac.
- Owner follow-up, 2026-10-15: tested the changed road and confirmed it is fine.
  Road review accepted; grass appearance raised separately. Hash comparison to
  pre-road HEAD confirms identical CartoonMeadowV2 PNG/import settings,
  TacticalGrass/GrassVerge materials, terrain shader/UV code and scene wiring.
  Live review now frames the lower-left portal instead of the old midpoint.
- Remaining: longer travel/pacing assessment; grass appearance clarification.
  Unity live review is open in Play Mode at the lower-left portal; observed
  Console: 0 errors, 0 warnings, portal front fully visible. Stage 3 stays open.

### 2026-10-15 — Winding invasion route concept review (historical exploration)

Historical concept handoff, superseded by the accepted-road implementation above.

- Owner/surface: current chat; ImageGen layout references under `art/concepts/`
  and the current design/handoff documents. Depends on the current Stage 3
  cartoon scene, original B board and existing map/navigation contracts.
- Request: explore a winding gray invasion/building zone, with roughly twice
  the existing portal-to-objective straight-line distance. Large explorable
  world is the longer-term direction; final world bounds are not selected.
- Current acceptance: ONE horizontal DOUBLE RECTANGULAR ZIGZAG road of uniform
  about SEVEN tower ground footprints width, no branches, portal and throne
  at its two ends. Use the B gameplay strip's gray paving/grass style. Preserve
  room for player-built mazes, original art and prompts/provenance. The earlier
  organic/variable-width and single vertical studies below are superseded.
  No runtime map, scene, camera or mechanic change in this task.
- Verification: all three original reference PNGs were visually inspected and
  copied unchanged into `art/concepts/invasion_route_review_2026-10-15/`.
  Exact generation/refinement prompts, input/output SHA256 and provenance are
  in `art/source/invasion_route_concepts_2026-10-15_{prompts,provenance}.json`.
  C was refined once to remove an unintended road spur beyond the objective.
  Current endpoints are (46,44)/(39,39): 8.60 cells direct, giving 17.20 cells
  for a 2x comparison. Image geometry is illustrative, not a measured map.
  Current navigation allows green terrain; future routing must prevent that
  shortcut while preserving hero resource travel and local tower detour/siege.
  Runtime/scene/package files are unchanged; gameplay tests were not rerun for
  this concept-only work.
- Owner correction: first A/B/C were too road-like. The original TD uses towers
  as blockers; the player, not narrow terrain, creates the repeated zigzag.
  A's earlier recommendation is withdrawn. D1 shows the wide, gently curved
  construction field; D2 edits that same field with alternating tower barriers
  and an illustrative gold route through their open ends. All output PNGs are
  unchanged ImageGen originals. New prompts/hashes/provenance:
  `art/source/maze_field_width_review_2026-10-15_{prompts,provenance}.json`.
  12–16 usable construction cells across is a starting hypothesis for blockout,
  not accepted tuning or measured image geometry. Example tower counts do not
  select economy. Current full-block/siege behavior stays unchanged.
- Further owner correction: D1/D2 rejected as oval/plaza-like. The gray area
  must remain an organically winding wide road, not a straight construction
  arena. E restores an asymmetrical S/meander and irregular worn shoulders;
  it is a new candidate, not an accepted result. Original output is retained as
  `art/concepts/invasion_route_review_2026-10-15/e_natural_wide_road.png`.
  Prompts/source hashes: `art/source/natural_wide_road_2026-10-15_*`.
- Quantified owner correction: E is still too narrow. Broad sections must fit
  7–8 tower ground footprints transversely; short necks may fit about 3. This
  supersedes the earlier agent-proposed 12–16-cell hypothesis. New F candidate
  `f_variable_width_road.png` has countable 8/3 scale rows, with unchanged PNG
  copy and prompts/source hashes in `art/source/variable_width_road_2026-10-15_*`.
  Initial generated attempt was discarded for an unwanted branch and a row
  along the reach; refinement uses the continuous E road as its edit target.
  Painted perspective/row direction do not certify usable grid width. No Unity
  tests rerun for concept-only work. Owner next requested the original B board:
  `art/concepts/sprite_style_review_2026-10-15/b_cartoon_2d.png`, lower gameplay
  strip road; found and opened. This does not select a new runtime layout.
- Latest owner correction: B road style confirmed, then uniform five-wide
  rectangular road requested; H single vertical zigzag was corrected to width
  seven, DOUBLE zigzag and HORIZONTAL composition. Current I original PNG:
  `art/concepts/invasion_route_review_2026-10-15/i_horizontal_double_zigzag_7wide.png`.
  Exact original SVG guide, prompts and hashes:
  `art/source/horizontal_double_zigzag_7wide_2026-10-15_*`. Guide has one open
  nine-segment polyline, eight square turns, width 7 and seven transverse
  footprint markers. Initial I generation was discarded for projection/count
  drift; final uses only the exact guide as edit input. Final route visually
  inspected, PNG copied byte-identically, JSON parses and diff whitespace
  checked. Generated paving/markers are illustrative, not certified Unity cells.
- Remaining: owner review of I, then measured width/layout/shortest-route
  blockout before map/navigation implementation. Stage 3 remains open;
  Stage 4 is not started. No gameplay tests rerun for concept-only changes.

### 2026-10-15 — Mac Editor startup and shared Windows development

- Owner/surface: chat `Запуск и продолжение Not 3A Game`; local Mac Unity
  installation, import, existing tests, live Stage 3 review and this handoff.
  Depends on clean `main` at `649938c4aa81404739ca10e4495ea01aef12b60d`,
  Unity 6000.6.2f1 ARM64 and the existing Personal license.
- Contract: develop the same project on Mac and Windows, retaining Windows/Steam
  as the release target. Preserve current mechanics, visuals, scene bindings and
  package versions; only fix an observed startup/compatibility blocker.
- Acceptance: import and compile, relevant existing tests, no new attributable
  Console errors, then `Stage3Rapid3DGamePilot` running in Editor Play through
  `Tools > Stage 3 > Cartoon World > Open Live Review`.
- Verification: Unity 6000.6.2f1 ARM64 imported packages/FBX and compiled on
  Apple M3/Metal. Existing Stage3/Integration/RunState PlayMode passed 41/41,
  45.87 s, including paid full run/restart, both tower roles and camera bounds.
  XML/log: `Artifacts/S3-MAC-20261015/playmode.{xml,log}`. Mac-rendered review
  PNGs are in its `mac-review/`; original tracked review PNGs were restored.
  Import-only URP runtime-list serialization was restored to the clean baseline.
  No new compiler/shader/test error; existing deprecated-API warnings remain.
  Initial repository status was clean. No Blender dependency in Unity `Assets/`.
  Owner accepted the first-launch Editor terms. First GUI startup produced one
  UnityEditor.Search indexing exception; an ordinary Editor restart resolved it,
  with no package/cache/source edits. Final GUI Console: 0 errors, 0 warnings.
  `Open Live Review` runs the actual `Stage3Rapid3DGamePilot` in Play, with
  `S2_INT_GAMEPLAY_READY`, map hash `996FF3F8BC05F95C`, both tower readiness
  markers and `S3_CARTOON_LIVE_REVIEW_READY`. Log: `editor-reopen.log` in the
  same evidence folder. Editor remains open in Game view for the owner.
  On the owner's follow-up, the project was re-cloned with ordinary
  `git clone https://github.com/fellsia2019/not-3a-game.git`: complete 14-commit
  remote history, no shallow/partial clone, `git fsck --full` passed. This full
  clone now occupies the original project path. The documentation commit was
  reapplied with `git am` and all three document hashes matched. At the owner's
  subsequent request, `not-3a-game-backup-20261015` was moved from `game-dev` to
  the Mac Trash. Local Unity cache and Mac test evidence remain in the active
  full clone; all 17 stable evidence files (XML/log/summary and 14 PNGs) retained
  their SHA256 hashes. GitHub CLI login/setup is complete
  for `fellsia2019`, with repository push permission verified.
  After the replacement, the same scene was reopened through Open Live Review:
  both towers/gameplay ready, unchanged map hash, GUI Console 0 errors/0 warnings.
  Startup log: `Artifacts/S3-MAC-20261015/editor-full-clone.log`.
  The earlier long Editor session also logged two native JobTempAlloc warning
  messages during first-time indexing; no project error accompanied them.
- Files changed: this handoff, canonical plan and README instructions for
  alternating Windows/Mac Git work. Runtime code, scenes, art and packages are
  unchanged.
- Remaining: owner visual review and Stage 3 gate stay open; Stage 4 is not
  started. No macOS player or new Windows build was requested or produced.

### 2026-10-15 — Remove unneeded animated QA captures

- Owner/surface: current chat; eight agent-generated GIF/MP4 recordings under
  `art/review/` and their documentation links. No runtime Unity asset changed.
- Behavior: remove motion recordings that are unnecessary for playing or
  continuing the project on Mac; retain static review PNGs, original sprite
  artwork, prompts/provenance and gameplay verification results.
- Verification: no GIF/MP4 remains in `art/review/`, no documentation link
  points to a deleted recording, and the Git working tree is synced after the
  removal. The historical commit still contains the recordings; no history
  rewrite was requested.
- Remaining: Stage 3 visual acceptance and actual Unity import/Play on Mac.

### 2026-10-15 — Portable Mac development handoff

- Owner/surface: current chat; README, reference inventory and GitHub handoff.
  No gameplay or Unity scene behavior changed.
  Depends on the current Stage 3 cartoon scene and its existing art provenance.
- Contract: Unity Editor 6000.6.2f1 opens the cloned project root; `Assets/` and
  all `.meta`, `Packages/`, `ProjectSettings/`, `art/`, canonical docs and the
  historical reference folder are saved in public GitHub `main`. Windows remains the approved
  build target; Mac is for continuing development and visual review.
- Verification: project-owned B board and both attached B PNGs are pixel-equal;
  all `Assets/` entries have `.meta` and portable paths have no case-fold name
  collisions. Historic third-party reference files under
  `concepts_visual_scene/` and `art/references/` stay outside `Assets/` and
  have explicit unknown-rights documentation. The owner approved placing all
  references in the existing public repository (D-044). GitHub `main` was
  verified at `8b156ffd8a5e655e73b217269c35e7c22428c735`; a local ZIP is
  an older snapshot, not the Mac transfer path. `art/review` GIF/MP4
  files were agent-generated QA captures, not player-provided references; the
  owner subsequently requested their removal (handoff above).
- Remaining: open/import and Play in Unity on the actual Mac; owner review of
  Stage 3 visuals and the open Stage 3 gate.

### 2026-10-15 — D-043 tactical camera viewport clamp

- Owner/surface: current chat; `Stage2CameraFollow` tactical branch, existing
  Stage 3 camera checks and tactical-view docs. Depends on current Stage 3
  100×100 map, fixed view angle and the D-042 playable scene.
- Behavior: camera follows the hero centrally until a visible ground corner
  reaches a playable map edge. It then stops translating on that axis so the
  hero moves through the viewport. The clamp tracks orthographic zoom and
  Game view aspect; Stage 2 non-tactical following and map rules stay intact.
- Acceptance: all four viewport corners remain over playable ground at map
  corners and zoom extremes; center follow, clicks, movement and zoom work.
- Verification/handoff: Stage3/Integration/RunState PlayMode 41/41; focused
  edge capture 1/1. Ground corners checked at 2.5/5/10 zoom and two aspects;
  edge-follow lock and center follow checked. Actual Game frames:
  `art/review/camera_clamp_{southwest,northeast}.png`. Windows Development
  build succeeded, 277524877 bytes, 0 errors/3 warnings; standalone reached
  Gameplay Ready with unchanged map hash `996FF3F8BC05F95C`. Reports/logs:
  `Artifacts/S3-CAMERA-CLAMP/`. Outer visual border remains unseen backing;
  owner visual review remains. No Git mutations performed.
- Follow-up from owner: the complete hero silhouette must stay on screen at
  the stopped camera edge. Stage 3 planar movement now has one cell of inner
  clearance, serialized into its current scene and preserved by both builders;
  Stage 2 movement is unchanged. Full PlayMode 41/41 passed again, including
  hero renderer bounds at the map extremes. Updated edge frames:
  `art/review/camera_clamp_{southwest,northeast}.png`. Final Development build
  succeeded, 277525565 bytes, 0 errors/7 warnings; standalone reached
  Gameplay Ready with unchanged map hash. Logs: `Artifacts/S3-HERO-EDGE/`.
  Owner visual judgement of the clearance remains open.

### 2026-10-15 — D-042 Astra cartoon rework -> owner visual review

- Owner/surface: human-requested Astra implementation agent in current chat;
  `Assets/Stage3/CartoonWorldPilot/**`, narrow tactical terrain/landscape/shot
  presentation, narrow/main builders, existing Stage 3 tests and canonical docs.
  Depends on original B gameplay reference and existing map/build/combat contracts.
- Behavior: V2 short turf with earth pockets, worn paving with joint weeds,
  67 bounded roadside ruin/shrub/flower sprites including the visible nonwalkable
  border. No collision or buildable-cell occupation; objective/resource clearance.
  Old bright triangle fringe removed, irregular grass apron and perspective-aware
  paving UVs retained. Map 100x100, anchors, resources and camera unchanged.
- Towers: grounded red-roof Rapid stays static. Longshot uses ONE stationary
  masonry/support sprite plus smoothly articulated spoon and separate loaded
  stone. The cup has fixed dimensions; projectile starts at its sampled actual
  release position on the existing clock. Pause and reset restore expected state.
  Four bearings are still observable; the support is a 2D fixed drawing, not a
  continuously rotating 3D mechanism. Preview and paid build share artwork.
- Source/rights: five original built-in imagegen PNGs under CartoonWorldPilot;
  exact prompts/provenance/SHA256 in `art/source/astra_cartoon_rework_*.json`.
  Original PNGs and metadata retained; V2 files match generation outputs exactly.
- Verification: full Stage3/Integration/RunState PlayMode 40/40, 33.00 s,
  including paid full run/restart, 24 builds, foundation-stable aiming/fire,
  actual cup release, pause and loaded-stone state. After final fringe removal,
  capture/landscape 2/2 (13.03 s); extra decor-cell/landmark-clearance check 1/1.
  After border-seam and painted-projectile fixes, final focused capture/combat/
  landscape checks passed 4/4 (17.32 s), including 90 actual 60 Hz throw frames.
  Windows Development build succeeded, 277524044 bytes, 0 errors/7 pre-existing
  warnings. Standalone startup reached gameplay and 67-prop readiness without a
  new attributable error; owned smoke process terminated after startup verification.
- Evidence: `art/review/astra_world_game_overview.png`, tower closeups and
  the retained PlayMode results; motion recordings were later removed at the
  owner's request.
  Final close-frame review found and corrected legacy cube-like flight artwork:
  released Longshot stone now shares the loaded painted sprite. The outside
  world edge also receives the same irregular grass apron; its former dark
  border tint is removed to avoid a straight color seam.
  Logs/XML/build report/frames: `Artifacts/S3-ASTRA-VISUAL/`.
  Build: `Builds/Windows/Stage3CartoonWorld/Not3A.exe`.
- Rebuild: `Stage3CartoonWorldBuilder.BuildAndWire`; main builder preserves V2
  art and roadside reference. No new scene Inspector work required. Live review
  frames ordinary preparation once; Space begins the wave.
- Remaining: owner's judgement of closeness to reference, wider Stage 3 visuals
  and stable standalone performance gate. First D-041 result was rejected;
  automated checks do not imply visual acceptance. Git was read-only throughout.

### 2026-10-15 — Stage 3 cartoon towers/terrain -> live review

- Owner/surface: current root; `Assets/Stage3/CartoonWorldPilot/**`, narrow
  tactical presentation/shot-motion/scene-builder changes, existing Stage 3
  tests and canonical documents. Depends on original cartoon B board and the
  existing building/combat/map contracts. D-041 authorizes two tower visuals
  and terrain; the owner's clarification explicitly retains map/camera.
- Behavior: Rapid and Longshot are transparent sprite towers with shared
  authored artwork in the placement preview. Longshot uses four drawn aiming
  directions and shot-clock windup/release/return; the actual projectile starts
  at its drawn cup socket. Rapid stays still. Pause, prices, damage, occupancy,
  paid building, waves and restart remain unchanged. Soft contact shadows have
  no collision. Four original source PNGs and exact prompts/hashes are saved.
- Terrain: cartoon grass and gray stone paving replace the old pigment/grid
  texture. Paving courses and slab aspect match the reference more closely;
  an alpha-faded visual grass apron softens the existing edge. The 100x100
  map, diagonal invasion zone, anchors, resource positions and camera stay.
  Original 3D assets remain available; no package or assembly added.
- Integration: existing Stage 3 scene uses copied catalogs with unchanged
  gameplay values. `Stage3CartoonWorldBuilder.BuildAndWire` rebuilds only this
  surface; the main builder retains it. `Open Live Review` frames the two
  towers/objectives once during ordinary preparation, with Space starting
  the wave. It saves no player-position/timing change and does not loop/reset.
- Verification: final Stage 3/Integration/RunState PlayMode 40/40 in 29.69 s,
  including 24 paid sprite towers, four bearings, windup/release/impact/pause,
  reset, the paid full-run check and actual first-wave firing of both roles.
  Windows Development Build succeeded, 269135061 bytes, 0 errors/7 warnings.
  Logs/reports are under `Artifacts/S3-CARTOON-WORLD/`.
  Final standalone startup reached gameplay/tactical readiness without a new
  error; the owned hidden smoke process was closed. The Editor opens the
  framed ordinary preparation scene for live inspection.
- Evidence: overview and both tower close-ups under `art/review/`; the
  animated QA capture was later removed at the owner's request.
  Source/rights: `art/source/cartoon_map_tower_provenance.json`; exact prompts:
  `cartoon_map_tower_prompts.json`. All four PNG hashes match imagegen originals.
- Remaining: owner review of new tower/terrain appearance and drawn animation,
  remaining resource/hero visuals and other Stage 3 gates. D-040 landmark
  appearance was approved in the current request. No full-project art approval
  or stable performance claim. Pre-existing work preserved; Git read-only.

### 2026-10-15 — Stage 3 portal/soul sprites -> live review

- Owner/surface: current root; `Assets/Stage3/LandmarkSpritePilot/**`, narrow
  presentation/scene-builder binding, skeleton ground sorting, existing Stage 3
  tests and canonical documents. Depends on saved B4/soul-sphere references,
  cartoon B style and existing map/run/portal contracts. D-040 authorizes these
  two replacements; full-project pipeline/art approval remains open.
- Behavior: original transparent cartoon PNGs replace the portal and soul
  objective visuals in the existing Stage 3 scene. The portal retains its red
  circulation clock; cyan highlights travel around the objective's horizontal
  arc band and its soul flame shimmers. Explicit pause freezes both. Root map
  anchors/bearing, 3x3 build exclusions, spawns, health/loss, combat/economy and
  waves remain unchanged. Old 3D prefabs/Blender sources are preserved.
- Integration: Sprite prefabs reuse the existing serialized visual fields;
  deterministic import/build and the main scene rebuilder retain this choice.
  View-ray clearance corrects observed foreground base clipping by opaque
  terrain without moving screen registration. Ground-anchor sorting keeps
  skeletons in front after leaving the portal, while opaque meshes depth-test
  normally. No collider, package, assembly boundary or gameplay system added.
- Verification: final Stage 3/Integration/RunState PlayMode 39/39 in 22.38 s.
  Checks retain paid three-wave win/restart and ordinary spawn/reservations,
  compare foreground steps/anchors against rendered ground, confirm changing
  red/blue pixels, pause stability and skeleton walking/feedback/reset.
  Windows Development Build succeeded: 268557149 bytes, 0 errors, 7 warnings.
  Final compile/test/build logs show no attributable compiler/runtime error.
  Standalone startup logged gameplay/tactical readiness and responded without
  a new runtime error; the owned hidden smoke process was closed afterward.
  The interactive Editor scene remains open for the owner's live inspection.
- Evidence: `Artifacts/S3-LANDMARK-SPRITES/` reports/logs and 60 actual camera
  frames; overview and individual close stills remain under `art/review/`.
  The animated QA capture was later removed at the owner's request.
  Exact prompts/source rights are in `art/source/landmark_sprites_provenance.json`
  and referenced prompt files; both PNGs match original imagegen outputs.
- Review: the existing explicit Editor preview now frames both landmarks and
  starts prepared waves early, repeating after outcome until Play Mode stops.
  It saves no player-position or timing change. Remaining: owner review of
  artwork, perceived facing, scale and flow; the rest of Stage 3 and stable
  standalone performance remain open. No Stage 4 or whole-project sprite
  conversion is claimed. Pre-existing work is preserved; Git read-only.

### 2026-10-15 — Stage 3 cartoon skeleton walk -> live review

- Owner/surface: current root; `Assets/Stage3/SkeletonSpritePilot/**`, narrow
  enemy-visual binding in `Stage3TacticalPresentation`/its scene builder and
  existing Stage 3 PlayMode tests/spec. Depends on chosen-for-trial cartoon B
  and the existing enemy/run/portal contracts. D-039 authorizes this one actor
  trial, not a full project pipeline switch.
- Behavior: one original transparent 1536x1024 imagegen sheet supplies six
  walk frames in each of four directions. Shared Sprite subassets register
  feet/torso from alpha; a small view changes frames with actual distance,
  faces the tactical camera, freezes on pause/outcome, stops at blockers and
  forwards the existing hit flash. A ground shadow has no collider. Normal
  portal spawning, navigation, combat, economy and wave values are unchanged;
  Stage 2 prefab/catalog remain intact. Scene regeneration retains the binding.
- Verification: Stage 3/Integration/RunState PlayMode 39/39 passed in 37.81 s,
  including normal portal spawns, real walking, four directions, alpha,
  pause/hit/reset and the full paid three-wave win/restart. Windows Development
  Build succeeded, 255543777 bytes, 0 errors, 7 warnings (full build log retained).
  The initial builder hit Unity's missing-component fake-null behavior with
  `??`; explicit Unity null checks corrected it before final import/tests/build.
  Final compile/test/build logs have no new attributable compiler/runtime error.
- Evidence: `Artifacts/S3-SKELETON-SPRITE/` retains import pivots, reports,
  logs and 60 actual Game-camera frames; `art/review/skeleton_sprite_portal_*`
  includes size-5/3.2 stills. The looping GIF was later removed. Prompts/provenance and the
  recording assembler are in `art/source/skeleton_walk_cartoon_*` and
  `assemble_skeleton_walk_review.cjs`. Source art is original built-in imagegen;
  no third-party image or mesh input. The PNG itself is preserved unchanged.
- Review: Tools > Stage 3 > Skeleton Sprites > Open Looping Live Review opens
  the existing scene near the portal and repeats ordinary first waves after
  outcome, only inside Editor Play Mode. Stop Play Mode ends this temporary
  inspection; normal game startup/countdown and saved hero spawn are unchanged.
- Remaining: owner's assessment of gait, style, scale and scene occlusion.
  No attack/death animation, final pipeline approval, resource-art acceptance
  or standalone performance claim. Stage 3 gate stays open; Stage 4 not started.
  Pre-existing uncommitted work is preserved; Git was used read-only.

### 2026-10-15 — Stage 3 sprite style comparison -> awaiting owner choice

- Owner/surface: current root; original visual review concepts under
  `art/concepts/sprite_style_review_2026-10-15/` and current plan/handoff only.
  Depends on the existing Rapid arrow / Longshot bowl-catapult roles and
  baseline undead identity. No new tower or enemy role is introduced.
- Intent/outcome: owner rejected the current 3D tower quality relative to
  production cost and requested pixel-art and smooth cartoon sprite options
  before deciding. Inspected current tower/resource Game frames and project
  tower concepts; generated two original comparison boards with the built-in
  imagegen tool. Each shows both towers, the same proposed skeleton in several
  poses, and an illustrative gameplay-scale strip. These strips are not Unity
  captures; scenery/heraldry do not approve new mechanics or lore.
- Files/provenance: `a_pixel_art.png`, `b_cartoon_2d.png` and
  `generation_manifest.json` retain outputs, exact prompts and source notes.
  Cartoon is a style edit of the original pixel board; no third-party image
  input was supplied. Both outputs were visually inspected before handoff.
- Remaining: owner selects or revises the direction. No style/pipeline switch,
  runtime asset, animation delivery or model approval is implied. Actual sprite
  animation/import/readability still need validation after the choice. No Unity
  gameplay/scene change, so tests/build are not rerun. Stage 3 stays open.

### 2026-09-26 — Stage 3 resources -> smooth stone, then continue wood/ore

- Latest owner instruction after the stone review: "поправь сглаживание и го
  дальше". This supersedes the temporary stone-only hold below. Preserve the
  approved A/B/A concepts and proceed with their implementation, without a
  new concept-selection loop. Stage 3 gate remains open; hero remains last.
- Ownership: root owns the shared Blender driver, stone assets, Unity import,
  actual-scene checks and canonical docs. Conifer subtask owns only
  `art/source/resource_wood_model.py` and wood candidate/source files; ore
  subtask owns only `art/source/resource_ore_model.py` and ore candidate/source
  files. They return geometry/materials for root integration and do not modify
  shared runtime assets or run the Unity test/build request watcher.
- Stone smoothing: confirmed false faceting came from CORNER albedo tint
  computed from each triangle's normal. Changed to POINT tint computed from
  the smooth vertex normal, then rebaked/exported. Imported mesh normals already
  preserve Blender smoothing; no gameplay or camera change is required.
  Its actual-scene verification passed 36/36 in 35.39 s; evidence is
  `Artifacts/S3-RESOURCE-SOURCES/stone-smoothing-playmode.xml` and the
  `stone-smoothed-game-close.png` capture there. The all-resource final
  test/build results are recorded below.
- Wood/ore are integrated at the existing asset paths. Wood has 33236
  triangles, closed boughs and a closed crown; ore has 9464 triangles with
  dark fractured masses surrounding recessed iron mineral. Ore's original
  generated slate surface and its provenance/prompt are in `art/source/`.
  Each resource still has one renderer/material and no collider.
- The actual wood frame exposed a second geometry defect beyond smooth
  normals: some radial contour samples went backwards around the center,
  folding thin top strips inward. A/B renders with a constant texture and
  disabled shadow reception retained the marks; unlit rendering did not.
  Ordered contour samples retain the same outline points and prevent the
  folds. The generator now checks outward upper/lower faces on all 22 boughs
  and the crown. No production lighting, shadow or shader workaround is used.
- Final verification: corrected stone and replacement wood/ore were captured
  in the actual scene at zoom 5/2.5. Stage 3/Integration/RunState PlayMode
  passed 36/36 in 35.50 s, including footprint/placement, material imports,
  resource depletion/recovery and the paid three-wave win/restart. Evidence:
  `Artifacts/S3-RESOURCE-SOURCES/playmode.xml`, `status.txt` and
  `art/review/resource_{stone,wood,metal}_game_{default,close}.png`.
  Editor render/readback on/off medians: stone 2.449/2.433 ms, wood
  2.457/2.455 ms, ore 2.595/2.424 ms; on p95 3.138/2.935/3.225 ms.
  These 1280×720 GTX 1060 observations do not establish standalone FPS.
- Final Windows Development Build succeeded: 249623372 bytes, 0 errors,
  3 warnings (`Artifacts/S3-RESOURCE-SOURCES/build.txt`). The playable scene
  was reopened after build. Asset/meta audit preserves existing GUIDs and
  verifies source/FBX/2048 atlases. The final suites/build log has no new
  compiler or runtime exception/error lines. A temporary SSAO diagnostic
  lacked a URP test assembly reference; it was removed before the final
  passing suites/build. No diagnostic material or rendering change remains.
- Remaining: owner's visual review of the actual resource set and stable
  standalone performance. The ore's front rust seam is still broader than
  its concept; technical checks do not constitute art approval. Then D-032
  basic undead, finished HUD/audio and hero last. Stage 3 gate stays open;
  Stage 4 is not started. All pre-existing uncommitted work is preserved;
  replaced wood/ore artifacts were copied into `rejected-r2/` before export.

### 2026-09-26 — Stage 3 resource visuals -> second implementation rejected

- Owner/surface: current Stage 3 agent; resource appearance and production
  approach. The owner rejected all three replacement models after seeing
  actual Game frames. Neither runtime iteration is accepted art.
- State/contracts: existing assets, uncommitted work and technical evidence
  are preserved. No asset deletion, rollback, gameplay change or new model
  production accompanies this status update. D-037/D-038 remain the selected
  concepts unless the owner revises them.
- Owner clarification: the A/B/A concepts are correct; the in-game execution
  is the failure. Active surface is now stone A only: authored major planes,
  front-facing fragment composition in the actual camera, sparse structural
  fractures and a light granite palette. Independent review produced an unused
  isolated experiment under `Artifacts/S3-RESOURCE-SOURCES/`; that reviewer
  did not edit runtime assets, the main generator or project documents.
- Stone review candidate: corrected the authored front for the unchanged
  FBX/prefab pivot chain, compacted/widened/lowered the composition and flattened
  its pointed crown. The existing stone FBX, `.blend`, albedo and normal paths
  now contain the new candidate; their `.meta` GUIDs and Economy bindings are
  preserved. New original imagegen granite source/prompt/provenance are in
  `art/source/stone_granite_painted_albedo{.png,_prompt.txt}`; only its baked
  atlas ships. No concept pixels or third-party assets are copied.
- Verification: 5832 triangles, one renderer/material, footprint about
  1.44×1.05 and height 1.03 cell; actual camera frames at unchanged zoom 5/2.5
  are `art/review/resource_stone_game_{default,close}.png`. Final PlayMode
  36/36 passed in 35.50 s, including paid full run/restart and resource lifecycle.
  Evidence is `Artifacts/S3-RESOURCE-SOURCES/playmode.xml`. The new candidate
  has not been visually accepted; wood and ore were not regenerated this pass.
- Remaining: resolve the visual mismatch on one representative object before repeating
  production across all three. Existing 36/36 tests establish behavior only.
  Resource art acceptance and standalone performance remain open; D-032
  undead, HUD/audio and hero last follow later. Stage 3 gate open, Stage 4
  not started.

### 2026-09-26 — Stage 3 second resource implementation -> technically checked, art rejected

- Owner/surface: current Stage 3 agent; three resource Blender sources,
  exported geometry/textures, resource prefab materials and actual Game review.
  Dependencies: approved D-037/D-038 concepts and existing Economy bindings.
- Owner feedback: first runtime models look excessively low-poly and do not
  meet the approved concepts. Their technical pass below is not art approval.
  The approved A/B/A choices remain in force. Replacements now have broad
  weathered rock surfaces, softened edges, baked cracks/rust and individual
  rounded conifer boughs with bark. The source script, editable Blender files,
  FBXs and prefabs retain the existing asset paths and gameplay contracts.
- Implementation: stone 28172, wood 51620, ore 37478 triangles; one renderer
  and one material each. Original 2048 albedo atlases for all three and 2048
  tangent normal maps for stone/ore. `Stage3ResourceSourceBuilder` configures
  sRGB/mipmap/normal imports. Foliage atlas padding and background prevent
  black mip bleed; ordinary receiving/casting shadows remain enabled.
- Verification: final PlayMode 36/36 passed in 43.70 s, including material
  import, source dimensions/anchors, depletion/recovery, paid three-wave win
  and restart (`Artifacts/S3-RESOURCE-SOURCES/playmode.xml`). Actual playable
  camera frames at zoom 5/2.5 are
  `art/review/resource_{stone,wood,metal}_game_{default,close}.png`.
  Final Editor render/readback medians were 8.932–10.866 ms with the source,
  9.641–11.167 ms without; p95 outliers reached 87.258 ms. This noisy local
  comparison does not establish release FPS; standalone performance remains
  degraded/pending for the Stage 3 gate. Updated Windows Development Build
  succeeded with 0 errors and 3 warnings
  (`Artifacts/S3-RESOURCE-SOURCES/build.txt`); final asset/meta checks passed
  and no new attributable Editor errors were found. The playable scene was
  reopened for review after build.
- Remaining: owner review of the actual replacement models, stable standalone
  performance review, then the D-032 undead, HUD/audio and hero last. Stage 3
  gate open; Stage 4 not started. Earlier uncommitted work is preserved;
  rejected resource artifacts were also copied under
  `Artifacts/S3-RESOURCE-SOURCES/rejected-r1/` before replacement.

### 2026-09-26 — Stage 3 first resource models -> technically integrated, art rejected

- Owner/surface: current Stage 3 agent; D-032 resource visuals in
  `art/source/build_resource_sources.py`, `Assets/Stage3/VisualPilot/**`,
  playable Stage 3 scene, related PlayMode tests and documentation. Depends
  on D-037/D-038 selections, existing Economy lifecycle and D-034 map.
- Behavior/contracts: the approved stone A granite boulder, wood B conifer
  and iron-ore A source are separate Blender FBX/prefabs under the existing
  wood/stone/metal markers. `Stage3ResourceSourceBuilder` creates materials and
  prefabs; `Stage3TacticalPresentation` binds and grass-aligns them. Each has
  one renderer and no collider. Source cells, gathering, stock recovery,
  navigation, buildability and run rules are unchanged. Source concepts remain
  review images only; original geometry is reproducible from the Blender
  script and editable `.blend` files.
- Verification: Blender export reports 1280/634/640 triangles for stone/wood/
  ore. Actual Game camera captures at zoom 5 and 2.5 are
  `art/review/resource_{stone,wood,metal}_game_{default,close}.png` and show
  distinct silhouettes in the playable scene. PlayMode 36/36 passed, including
  source dimensions/anchors, depletion/recovery, full paid three-wave win and
  restart (`Artifacts/S3-RESOURCE-SOURCES/playmode.xml`). The 1280×720 Editor
  render/readback comparison is in
  `Artifacts/S3-RESOURCE-SOURCES/render-comparison.txt`; local medians varied
  by less than 0.5 ms on/off per source after warmups, and are not release FPS.
  Windows Development Build succeeded with 0 errors and 3 warnings
  (`Artifacts/S3-RESOURCE-SOURCES/build.txt`); no new attributable Editor
  errors were found after build.
- Remaining: owner can review the three actual Game frames; complete the
  D-032 baseline undead, HUD, audio and hero last, then final full-run and
  performance review. Stage 3 gate open; Stage 4 not started.

### 2026-09-26 — Stage 3 D-036 former zoom scale -> implemented, gate open

- Owner/surface: current Stage 3 agent; tactical Stage 3 scene camera setup,
  its zoom regression and current Game capture. Depends on D-026 wheel
  behavior and the D-034 100×100 map.
- Behavior/contracts: `Stage3RapidPilotBuilder` again configures the Stage 3
  camera with opening orthographic size 5. `Stage2CameraFollow` retains its
  0.5×..2× clamps, making the actual bounds 2.5–10. Angle, centered follow,
  smooth wheel, Inspector step setting, map cells, gameplay rules and all
  existing uncommitted work remain preserved. The player chose this scale
  after being told the portal and objective begin outside the resource-area
  hero's frame.
- Verification: full Stage 3/Integration/RunState PlayMode 34/34 passed,
  including exact near/far bounds, WASD zoom stability, paid three-wave win
  and restart. Evidence: `Artifacts/S3-ENTRANCE-PORTAL/zoom-restored-playmode.xml`.
  Windows Development Build succeeded with 0 errors and 3 warnings. Actual
  starting Game-camera view: `art/review/stage3_zoom_restored_default_game.png`.
- Remaining: owner assesses the close framing and whether the defense corner
  can be found without explanation. Then continue only the D-032 accepted
  model/UI/audio set, with image approval before each new model; hero last.
  Stage 3 gate is open, Stage 4 has not started.

### 2026-09-26 — Stage 3 portal circulation -> implemented, gate open

- Owner/surface: current Stage 3 agent; portal shader/material, narrow
  presentation clock, actual-render regression and explicit review recording.
  Dependency: the approved B4 red opening and D-035 placement. The owner
  reported a static-looking interior and requested visible circulation.
- Behavior/contracts: `EntrancePortalVortex.shader` rotates the red strands
  with bounded inward distortion; speed lives in `EntrancePortalVortex.mat`
  and is reproduced by `Stage3EntrancePortalBuilder`. `Stage3EntrancePortal`
  honors explicit pause and keeps the ambient vortex alive after an outcome
  or in an unbound Play-mode preview. Game rules, 45-degree ring and 3x3
  foundation remain independent of the effect.
- Verification: all 34 Stage 3/Integration/RunState PlayMode checks passed,
  including full paid victory/restart. Actual red-pixel comparisons detected
  2,870 changed pixels over 0.4 seconds, zero on pause, and continued motion
  after victory. Evidence: `Artifacts/S3-ENTRANCE-PORTAL/portal-circulation-playmode.xml`.
  Windows Development Build succeeded with 0 errors and 7 warnings; the
  visible Unity Console has no errors. The explicit `motion` review captures
  90 actual Game camera frames over six simulation seconds, preserving time
  and camera settings afterward. The resulting GIF/MP4 recordings were later
  removed as unnecessary for the project handoff.
- Remaining: owner review of presentation and overall map composition, then
  the D-032 baseline undead, three resource models, hero last, HUD and sounds.
  Each new model needs its specific image approval. Stage 3 gate is open;
  Stage 4 has not started. Existing uncommitted work is retained.

### 2026-09-26 — Stage 3 D-035 portal at 45 degrees -> implemented, gate open

- Owner/surface: current Stage 3 agent; approved B4 portal Blender source,
  exported FBX/prefab, tactical placement and portal PlayMode regression.
- Dependency/intent: after the D-034 diagonal map, the player requested the
  enemy gate at 45 degrees. Its exit remains toward the objective, with the
  red vortex and intact dark ring already approved in D-033.
- Behavior/contracts: `art/source/build_entrance_portal.py` and the editable
  `.blend` turn the upright ring, buttresses, steps and aperture 45 degrees
  inside the unchanged 2.945x2.945 square foundation. The FBX, baked stone
  texture and `EntrancePortal.prefab` were rebuilt without changing asset
  GUIDs, spawn/navigation rules or the nine-cell building exclusion.
  `Stage3RapidPilotTests` checks the diagonal aperture extents, foundation,
  spawn/movement, pause/reset and exclusion. The actual Game frames are
  `art/review/stage3_diagonal_portal_45_game_overview.png` and
  `art/review/stage3_diagonal_portal_45_game_close.png`.
- Verification: focused PlayMode 1/1 and full Stage 3/Integration/RunState
  PlayMode 34/34 passed, including the paid three-wave win and restart.
  Evidence: `Artifacts/S3-ENTRANCE-PORTAL/portal-45-playmode.xml`.
  Windows Development Build succeeded with 0 errors and 3 warnings. Model:
  3 meshes, 6,860 triangles, 0 colliders, world ground extents 2.945x2.945.
  Local Editor 1600x900 Camera.Render/readback medians on GTX 1060 were
  14.850ms on and 15.237ms off; these are not standalone FPS. No new
  attributable console errors observed.
- Remaining: owner reviews the combined D-034 map composition. Then continue
  only the D-032 enemy, three resources, hero last, HUD and sound set, with
  specific image approval before each new model. Stage 3 gate stays open;
  Stage 4 has not started.

### 2026-09-26 — Stage 3 D-034 diagonal 100×100 map -> active

- Owner/surface: Stage 3 planar map data, playable 3D scene composition,
  tactical camera start, temporary actor placement, and relevant PlayMode
  checks. Existing Stage 2 map/scene assets and approved model shapes stay
  with their owners.
- Dependency/intent: player supplied a screen composition and confirmed a
  100×100 map with a gray strip about 30 cells long and 12 across, descending
  from the upper right corner. Portal and objective sit along it; three
  resource types remain in green. The short three-wave run remains the check.
- Acceptance: actual game frame reflects the diagonal corner; both 3×3
  landmarks fit and remain legible, hero/resources are reachable, gray-only
  construction agrees with rendered ground, enemies reach the objective,
  paid defense can win all waves and restart, and map/render overhead is
  measured. Preserve unrelated uncommitted work.
- Outcome/contracts: `Stage3PlanarCorner.asset` holds the new map;
  `Stage3DiagonalMapBuilder` reproduces it. `Stage3RapidPilotBuilder` positions
  the hero and opens a wider fixed camera. The portal foundation turns by a
  grid-aligned quarter turn toward the new objective, keeping its 3x3 square
  base within the reserved cells; D-035 angles the aperture within it.
  `Stage3TacticalPresentation` uses one grass
  quad per cell on this large map and extends only the non-walkable visual
  border for maximum zoom. Stage 2 assets are retained.
- Verification: 34/34 Stage 3/Integration/RunState PlayMode tests pass,
  including the map, resources, gray-only placement, enemy movement, paid
  three-wave victory and restart, and zoom limits. Evidence:
  `Artifacts/S3-ENTRANCE-PORTAL/diagonal-map-final-playmode.xml`.
  Windows Development build succeeded with 0 errors and 7 obsolete-API
  warnings. Actual 1600x900 Game camera capture:
  `art/review/stage3_diagonal_map_game_overview.png`. Local Editor render and
  readback median is 17.7 ms on GTX 1060, a bounded observation rather than a
  standalone frame budget. The initial far-zoom border test failure was fixed
  and rerun; no new attributable console errors remain.
- Remaining: owner reviews the actual composition against the supplied
  mockup. If accepted, continue the agreed Stage 3 model/UI/audio set with
  specific image approval before each new model; hero stays last. Stage 3
  gate is open; no Stage 4 work started.

### 2026-09-26 — Stage 3 entrance portal B4 -> 3x3 placement corrected

- Owner/surface: Stage 3 portal Blender source, FBX/materials/prefab, tactical
  presentation binding and focused regression; other uncommitted work retained.
- Dependency/intent: player approved intact dark stone/red vortex B4 and
  explicitly requested modeling (D-033). Existing map entrance and run rules
  remain the source of spawn and navigation behavior.
- Acceptance: recognizable portal at tactical camera sizes, intact masonry,
  red animated center, owner-requested 90-degree turn and 3x3 base/exclusion,
  unchanged spawn/path, valid placement outside the base, pause/reset, full run and no new
  errors in a fresh Play session; model overhead measured locally.
- Contracts/files: `build_entrance_portal.py`/`.blend`, portal FBX, textures,
  shader and prefab; `Stage3EntrancePortalBuilder`, tactical presentation,
  `Stage3PlanarCorner.asset` and portal/full-run PlayMode checks. Base is
  2.945x2.945, with a centered nine-cell building exclusion in authored map
  data. The quarter turn faces the +X objective route; no collider or game-rule
  dependency on the mesh was added.
- Verification: 33/33 PlayMode checks passed after rebuilding the new map,
  including all three waves to victory with paid defense and restart,
  spawn/movement, all nine excluded cells, walkability, bounds, pause/reset
  and existing integration/run-state regressions. Evidence:
  `Artifacts/S3-ENTRANCE-PORTAL/placement-3x3-playmode.xml`.
  An earlier stale-map run failed the exclusion assertion; two zero-test
  runner results were discarded. Explicit script reload fixed test discovery.
  Windows development build succeeded (0 errors, 7 warnings); the fresh
  tactical capture confirms 3 meshes/6,860 triangles, no colliders and a
  2.945x2.945 footprint. The exact 90-degree turn makes the aperture edge-on
  to the fixed camera. After seeing that exact tactical screenshot and the
  alternative directions, the owner explicitly chose to retain the exit
  toward the objective. D-035 subsequently changed the exact bearing to 45
  degrees while keeping that destination.
  Local Editor render/readback medians: portal on 9.845ms, off 10.056ms;
  this noisy bounded comparison is not a minimum-target frame-budget result.
  Standalone startup logged gameplay and tactical readiness with no error or
  exception. Its hidden-window screenshot was black and was discarded as
  visual evidence; the reviewed images are from the actual Editor Play scene.
  Remaining: baseline undead enemy, three resources, hero last, accepted HUD
  and sounds, with specific image approvals before each new model and playable
  validation. The requested portal placement correction is complete; Stage 3
  gate stays open and Stage 4 has not started.

### 2026-09-26 — Stage 3 minimal MVP scope accepted -> visual/UI/audio work

- Active owner/surface: Stage 3 agent; remaining visual models in the playable
  `Stage3Rapid3DGamePilot`, its HUD and sound presentation. Dependencies are
  D-027/D-030/D-031, current map/economy/defense/run contracts and the
  individually approved concept for each new model.
- Outcome: player accepted D-032's minimal set: current short run, existing two
  towers and soul objective, then entrance portal, one baseline enemy, three
  resource sources, and hero last; finished run HUD and short cues for gather,
  build, both attacks/impacts, objective danger, wave and outcome. No new game
  mechanic was accepted. Existing hero, enemy, portal and resources are still
  temporary solids; the only HUD is development-only IMGUI; no gameplay audio
  assets or playback were found. Portal is the next concept candidate.
- Contracts/files: `production/DECISIONS.md` D-032 and Stage 3 in
  `gamedesign/DEVELOPMENT_PLAN.md`; runtime contracts and scene untouched.
- Verification: repository asset/code/scene/package and Git-status audit;
  earlier accepted soul-objective 11/11 PlayMode evidence remains recorded
  above. No runtime edit in this handoff, so tests/build were not rerun.
- Remaining: get portal concept approval before Blender/Unity; repeat that
  approval and playable-scene scale/silhouette/performance check for each
  remaining model, implement accepted HUD and sounds, verify full run and
  relevant regressions, then seek player acceptance of the Stage 3 gate.
  Stage 4 remains closed and belongs to a separate agent.
  Portal review options: `art/concepts/entrance_portal_option_a_violet_arch.png`
  and `entrance_portal_option_b_ember_ring.png`; no model was built from them.
  Player selected B's direction but rejected its broken stonework; intact
  revision `art/concepts/entrance_portal_option_b2_intact_ember_ring.png` is
  awaiting specific approval before production.
  Player then specified a darker entrance for an undead baseline enemy. The
  intermediate review candidate was
  `art/concepts/entrance_portal_option_b3_undead_ring.png`; no new enemy rules
  or runtime art have been added.
  The player restored the red interior; current review candidate is
  `art/concepts/entrance_portal_option_b4_dark_red_ring.png`, pending approval.

### 2026-09-26 — Stage 3 soul objective revision 3 after owner rejection

- Active owner/surface: Stage 3 objective model, shaders, prefab, tactical scene
  binding and the owner-requested 3x3 building exclusion. Depends on the D-031
  approved concept and the existing Stage 2 center-cell throne contract.
- Acceptance: separate stone bowl/anchors, soul sphere with readable blue flame,
  transparent smooth protective dome and horizontal external flowing strips;
  about 3x3 cells, readable from the existing tactical camera, no visual-driven
  combat rules. Initial small uniformly lit result and vertical orbit rejected.
- Changes: original `build_soul_sphere.py`, Blender/FBX, baked 1024² slate atlas,
  `SoulSphereIceDome.prefab`, smooth-ice/flame/wave shaders and materials,
  `Stage3SoulSphereMotion`, builder and scene bloom profile. Raised bowl and
  four anchors have real geometry. Stone face winding corrected. Orbit uses
  world -Z, independent of imported FBX local axes; solids remain stationary.
  Nine cells are reserved in `Stage3PlanarCorner.asset`; target, health and
  movement contracts remain unchanged. All earlier uncommitted work preserved.
- Verification: 11/11 Stage 3 PlayMode tests pass, including full run/win/restart,
  building rejection across all nine cells and more than a complete orbit with
  constant wave altitude. Evidence: `Artifacts/S3-SOUL-SPHERE/playmode.xml`.
  Final material tuning inspected in actual Unity captures
  `art/review/soul_sphere_game_close.png` and `soul_sphere_game_overview.png`.
  Console 0 errors/0 warnings after fresh Play start; shaders supported.
  Runtime objective: 26,796 triangles, six renderers, one shadowless point light.
  One local Editor Stats observation: 317.9 FPS, total 3.1 ms/GPU 1.9 ms,
  442.5k scene triangles, 113 draws at roughly 1100x506. No controlled build
  baseline; standalone/minimum-target performance remains a Stage 3 gate item.
- Review tooling: `Stage3SoulSphereReview` consumes explicit local
  `Artifacts/S3-SOUL-SPHERE/review.request` (`validate` or `open`), rebuilds the
  pilot, optionally runs its PlayMode suite, then opens Play and captures only
  after the model exists and shader compilation finishes. No background task
  remains after the request. Sources and generated review files stay local.
- Owner visually approved revision 3 after the playable scene review.
- Remaining: agree the minimal commercial MVP content; produce only its needed
  UI/audio and remaining models with individual visual approvals, then check
  target-build performance and the complete playable run. Hero remains last.
  Stage 4 stays closed until the Stage 3 gate is accepted by the owner.

### 2026-09-26 — Stage 3 soul sphere model in playable scene (rejected revision 1)

- Owner/surface: loss-critical objective visual only. D-031 selected the fourth
  concept and authorized modeling. No health, occupancy or rule changes.
- Output: original project geometry from `art/source/build_soul_sphere.py` and
  `soul_sphere_ice_dome.blend`; `Assets/Stage3/VisualPilot/SoulSphereIceDome.fbx`,
  six URP materials, `SoulSphereIceDome.prefab`, `Stage3SoulSphereMotion`, and
  the `soulSpherePrefab` binding in `Stage3Rapid3DGamePilot.unity`. Source is
  project-owned original Blender geometry derived from the AI-generated
  approved concept; no third-party mesh, texture or material was shipped.
- Verification: Blender export/render succeeded (4183 vertices, 7452 triangles,
  six mesh renderers); interactive Game Play Mode shows the sphere, dome and
  rotating orbit at the authored throne cell beside both towers. Unity Console
  showed zero errors and warnings in that review. Stats showed about 383k scene
  triangles and 300 FPS on the local machine; no controlled baseline measured.
- Remaining: owner review of the in-game model; full run, relevant PlayMode
  regression and final screenshot after shader warm-up before Stage 3 gate.

### 2026-09-26 — Stage 3 central soul sphere correction

- Owner/surface: visual concepts for the loss-critical objective only. The
  player specified a sphere containing the hero's soul in the center.
- Output: `art/concepts/throne_soul_sphere_energy_dome.png` and
  `art/concepts/throne_soul_sphere_ice_dome.png` show that sphere beneath a
  transparent blue dome with circular energy strips. Prompts are in
  `art/source/throne_soul_sphere_concept_prompts.txt`. Previous dome-throne
  concepts are rejected historical sketches.
- Verification: both generated images visually inspected and saved. No Blender
  model, Unity asset, scene, gameplay rule or test changed.
- Remaining: player visual approval of the corrected concept before modeling
  and playable-scene integration. Stage 3 gate remains open.

### 2026-09-26 — Stage 3 dome-throne concept replaces smooth obelisk

- Owner/surface: visual concepts for the player's loss-critical throne only.
  The player redirected the design before model production.
- Output: two original built-in imagegen concepts in `art/concepts/`:
  `throne_dome_option_energy.png` has a transparent cyan force dome;
  `throne_dome_option_ice.png` has a smooth translucent ice dome. Both place a
  smooth blue throne on a compact circular dais with broken orbiting energy
  arcs and no front-facing soul emblem. Exact prompts are in
  `art/source/throne_dome_concept_prompts.txt`. The supplied gameplay screenshot
  is a motion/composition reference only; its architecture, statues and UI are
  not reused. The A4 obelisk remains a historical review image.
- Verification: images visually inspected. An unfinished unapproved Blender
  script started before the direction change was removed. No runtime Unity
  asset, scene, gameplay rule or test changed.
- Remaining: player chooses the dome and central form before Blender modeling,
  Unity integration and a playable-scene check. Stage 3 gate remains open.

### 2026-09-26 — Stage 3 ice throne smooth-monolith revision

- Owner/surface: visual concept only for the player's loss-critical objective.
- Output: `art/concepts/throne_ice_option_a4_smooth_monolith.png` removes the
  previous monolith's small facets, cracks and busy ice texture. It keeps one
  continuous softly sculpted cone, with the embedded soul prism as the sole
  detailed focal point. Exact built-in imagegen edit prompt is appended to
  `art/source/throne_ice_concept_prompts.txt`.
- Verification: image-only visual review. No Blender, Unity asset, scene,
  gameplay rule or test changed.
- Remaining: player visual approval before model production or integration.

### 2026-09-26 — Stage 3 ice throne homogeneous-monolith revision

- Owner/surface: visual concept only for the player's loss-critical objective.
- Output: `art/concepts/throne_ice_option_a3_monolith.png` revises A1 into one
  continuous tapered ice body. All satellite, front and crown-cluster shards
  are removed; the soul prism remains embedded in the monolith, held by the
  existing stone altar and metal retainers. Exact built-in imagegen edit prompt
  is appended to `art/source/throne_ice_concept_prompts.txt`.
- Verification: image-only visual review. No Blender, Unity asset, scene,
  gameplay rule or test changed.
- Remaining: player visual approval before model production or integration.

### 2026-09-26 — Stage 3 ice-obelisk narrative revisions

- Owner/surface: visual concept only for the player's loss-critical objective.
  The player selected original option A as the closest direction and proposed
  a possible story motivation: an evil sorceress imprisoned the hero's soul in
  the ice, so breaking it kills the hero. This remains a visual premise until
  explicitly accepted as canon.
- Output: `throne_ice_option_a1_soul_prism.png` strengthens hierarchy with a
  clean visible soul prism, metal seals and one energy ring;
  `throne_ice_option_a2_witch_reliquary.png` adds a cursed violet fracture,
  clamps and chains. Both preserve the chosen conical obelisk and one-cell base.
  Exact built-in imagegen edit prompts are appended to
  `art/source/throne_ice_concept_prompts.txt`.
- Verification: image-only visual review. No Blender, Unity asset, scene,
  gameplay rule or test changed.
- Remaining: player selects one revision or requests a blend before any model
  production or playable-scene integration.

### 2026-09-26 — Stage 3 ice-throne concepts awaiting selection

- Owner/surface: visual concept only for the player's loss-critical throne;
  its gameplay role and rules are unchanged.
- Output: three original imagegen options in `art/concepts/`: A is an ice-crown
  obelisk, B is a recognizable empty throne with a high tapered crystal back,
  and C is a glacial core shrine. Approved Rapid and landscape images were
  used only as style and mood references. Exact prompts are recorded in
  `art/source/throne_ice_concept_prompts.txt`.
- Verification: image-only review; each option is a single readable object
  with a broad base and isometric presentation. No Unity assets, Blender file,
  scene, rules or tests changed.
- Remaining: player selects or revises one concept. Blender production and
  playable-scene integration require that visual approval first.

### 2026-09-26 — Stage 3 landscape parked by player

- Owner/surface: Stage 3 content order; no Unity asset or gameplay edit.
- Outcome: the player chose to leave the current landscape in place for now
  and move on. `DEVELOPMENT_PLAN.md` records this as a pause, not final visual
  acceptance. Existing build/capture and prior 49/49 verification remain the
  latest evidence; no tests rerun for this documentation-only update.
- Remaining: agree the minimum commercial MVP models, UI and sound before
  further production; review the landscape again before Stage 3 gate. The hero
  model remains last.

### 2026-09-25 — Stage 3 landscape closer to concept B

- Owner/surface: Stage 3 playable landscape only. The player said the previous
  runtime frame improved but remained far from approved concept B.
- Behavior/contracts: original grass and compacted-earth albedos regenerated
  from concept B as a style reference; exact prompts in `art/source/`. The
  earth keeps fine cell guides and flush mineral flecks. Grass now has shallow
  deterministic visual relief with smooth normals, marker/foliage height
  alignment, and a wider irregular slope into gray earth. `Stage3TacticalPresentation.cs`,
  `Stage3LandscapeProps.cs`, two runtime textures, the material builder, and
  the focused PlayMode check changed. Bushes stay removed; no colliders or
  gameplay rules changed. The old procedural earth script produces only a
  fallback outside runtime Assets.
- Verification: isolated Unity 6000.6.2f1 PlayMode 49/49 including complete
  run, two paid towers, no-collider terrain and map rule checks. Windows x64
  Development build succeeded and is running from
  `Builds/Windows/Stage3LandscapeConceptB`; capture:
  `art/review/landscape-concept-b-game.png`. Player log has no attributable
  errors; runtime reports 0 bushes, 10,099 combined foliage triangles and 0
  foliage colliders. `git diff --check` passed.
- Remaining: player visual acceptance and quantitative frame cost. Commercial
  MVP model/UI/audio scope and Stage 3 gate remain open; final hero model last.

### 2026-09-25 — Stage 3 concept B grass and soft gray-zone verge

- Owner/surface: Stage 3 playable landscape. The player pointed back to the
  approved concept B image and requested that grass texture and its soft,
  irregular transition on every gray-zone side, with no bushes.
- Behavior/contracts: a new original `LandscapeGrass.png` was generated with
  concept B as a style reference (prompt at
  `art/source/landscape_grass_prompt.txt`). `TacticalGrass.mat` repeats it at an
  eight-cell scale. `Stage3TacticalPresentation.cs` lowers the visual grass
  lift to 0.08 cell and replaces the straight dark sidewall with a textured
  sloping verge on each forest/gray boundary. `Stage3LandscapeProps.cs` puts
  real grass fringe at the outer edge; no bush meshes or materials return.
  The shared rules plane and gray-only tower placement stay unchanged.
- Verification: isolated Unity 6000.6.2f1 PlayMode 49/49 including full run,
  grass-verge geometry, no foliage colliders, two paid towers and existing map
  rules; 1/1 focused check passed after the final grass material scale edit.
  Windows x64 Development build succeeds. Running build:
  `Builds/Windows/Stage3LandscapeConceptB`; actual game capture:
  `art/review/landscape-concept-b-scale-game.png`; player log has no new
  attributable errors. Main/built scene, scripts, material and texture hashes
  match; `git diff --check` passes.
- Remaining: player visual acceptance and quantitative frame-cost check if
  accepted. Commercial MVP model/UI/audio scope and overall Stage 3 gate are
  open. The final hero model remains last.

### 2026-09-25 — Stage 3 raised grass edge, rejected bushes removed

- Owner/surface: Stage 3 playable landscape presentation. The player rejected
  the procedural bushes and asked for grass visibly above compacted gray earth
  as in concept B. No new model or gameplay system is introduced.
- Behavior/contracts: `Stage3LandscapeProps.cs` now generates only grass blades
  and flowers, with denser real 3D blades along the green/gray boundary. Bush
  methods, scene bindings and their unreferenced material assets were removed.
  `Stage3TacticalPresentation.cs` renders green cells 0.18 cell above gray and
  builds a shallow soil sidewall, while lifting hero/resource scale markers to
  match; the scene and builder use the reduced material set. Navigation,
  movement, ray clicks, combat and tower placement still use the unchanged
  flat map coordinates. D-011/D-030 and tactical/art specs record the visual
  exception; no collider was added.
- Verification: isolated Unity 6000.6.2f1 PlayMode 49/49 including full run,
  followed by 1/1 focused edge geometry check after the fringe density edit.
  Windows x64 Development build succeeded. The actual game capture is
  `art/review/landscape-raised-grass-final.png`; running player
  `Builds/Windows/Stage3LandscapeRaisedGrass` has no attributable log errors.
  Runtime reports 0 bushes, 3 shared foliage meshes, 7487 triangles and 0
  foliage colliders. `git diff --check` and main/isolated input hashes checked.
- Remaining: player visual review and quantitative frame cost if the terrain
  is accepted. Stage 3 commercial MVP model/UI/audio scope and gate remain
  open; hero model remains last.

### 2026-09-25 — Stage 3 landscape visual correction

- Owner/surface: Stage 3 landscape presentation in the playable 3D scene.
  The player rejected the first texture-only pass as blurry and distant from
  selected concept B. Hero, towers, combat rules and map masks remain owned by
  their existing surfaces.
- Behavior/contracts: `LandscapeGrass.png` now carries original text-prompt
  generated painted turf, imported uncompressed with full NPOT resolution.
  `Stage3LandscapeProps.cs` adds separate rounded bush, leaf, grass-blade and
  flower meshes on green terrain/border; five combined renderers, no colliders
  or rule references. `Stage3TacticalPresentation` and the scene bind the new
  materials. The procedural source script generates only gray earth now;
  `STAGE3_TACTICAL_VIEW.md` and `ART_DIRECTION.md` record provenance.
- Verification: isolated Unity 6000.6.2f1 PlayMode 49/49 including full run,
  map-rule and no-collider checks; Windows x64 Development build succeeded.
  `Builds/Windows/Stage3LandscapePolished` runs and responds, with both paid
  towers visible; `Artifacts/S3-RAPID-PILOT/landscape-polished-player.log` has
  no attributable errors. Actual capture: `art/review/landscape-polished-game.png`.
  Five shared meshes contain 44,245 triangles total, 0 colliders;
  `git diff --check` passes.
- Remaining: the player must judge the revised look in the game. Measure
  frame cost after visual acceptance and adjust only if needed. Commercial MVP
  model/UI/audio scope and the overall Stage 3 gate remain open; the final
  hero model is still last.

### 2026-09-25 — Stage 3 landscape concept selection

- Owner/surface: Stage 3 visual landscape planning. The player deferred the
  final hero model until the end; the current basic hero marker remains.
- Input: player-supplied bright grass, low bushes and small white flower
  reference at `art/references/landscape_player_grass_reference.png`; unknown
  source license, inspiration only. Current game screenshot provided scale and
  camera composition. No external image is runtime-ready.
- Output: two imagegen review concepts in `art/concepts/landscape_option_a_stone_paving.png`
  and `landscape_option_b_packed_earth.png`. A uses weathered stone paving in
  the gray zone; B uses compacted earth/slate chips. Both explore the grassy
  green zone. Player selected B and required real 3D props for any raised
  bushes/rocks; omitting these in the first pass is accepted.
- First pass (subsequently rejected): original seamless turf and compacted earth textures via
  `art/source/build_landscape_textures.py`, continuous four-cell UVs in
  `Stage3TacticalPresentation`, updated materials and builder bindings. No
  fake-relief props are painted into the textures. Scene/map rules and prefabs
  are unchanged; the existing terrain mesh and draw-call structure are reused.
- Verification: PlayMode 48/48 including full run, catapult aiming, camera and
  map bounds. Main and tested landscape inputs matched by hash. Windows x64
  Development build `Builds/Windows/Stage3LandscapeB` launched with both
  purchased towers; `Artifacts/S3-RAPID-PILOT/landscape-player.log` has no
  attributable errors. Actual-player capture: `art/review/landscape-b-game.png`.
  Two 512-square maps replace two 128-square maps; no raised prop geometry was
  added. Release GPU timing was not separately measured.
- Historical remaining work: the texture-only pass awaited review; the player
  rejected it, and the newer entry above records the visual correction.

### 2026-09-25 — Stage 3 centered rotation of the catapult inner mechanism

- Owner/surface: Stage 3 visual aiming, following the player's annotated
  screenshot. Existing model, textures, material, camera and prior uncommitted
  changes are preserved. No additional content is produced.
- Behavior: an explicit `Catapult centered yaw` pivot lies on the vertical
  line through the occupied cell center. Imported frame/axle/arm/cup move
  together below that pivot; masonry and parapet remain outside it. Forward
  comes from the imported cup and axle positions. Between throws the mechanism
  continuously tracks the nearest living enemy; the throwing arm retains its
  separate axle motion. Pause freezes tracking and reset cleans it up.
- Contracts/files: `Stage3TowerShotMotion` and combat presentation; read-only
  `Stage2Tower.AttackRange` and `Stage2EnemyGroup.FindNearest` reuse combat's
  existing target selection. Aiming cannot fire, reserve targets or deal damage.
  Catalog values, damage timing and shots are unchanged; no Inspector migration.
- Verification: EditMode 79/79 and complete PlayMode 48/48, including full run/restart and a new
  four-bearing check of fixed pivot, frame height/radius, fixed foundation and
  parapet, live-target tracking without shots, and pause/reset. Preview versus
  placed bounds allow 0.0001 world units for float rounding during the
  world-preserving reparent. Evidence: `Artifacts/S3-RAPID-PILOT/catapult-yaw-*`.
  Windows build `Builds/Windows/Stage3CatapultYaw` launched; player startup and
  both tower bindings are ready, with no attributable errors. Actual gameplay
  captures `art/review/catapult-yaw-stone-windup.png`, `catapult-yaw-stone-release.png`
  and `catapult-yaw-stone-flight.png` show the mechanism inside the fixed base.
  Main Assets/Packages/ProjectSettings match the tested build inputs by hash.
- Acceptance: player inspected the restarted playable build and accepted the
  revised aiming on 2026-09-25.
- Remaining: agree commercial MVP model/UI/audio scope, produce only approved
  content, then verify the full run. Stage 3 gate remains open; Stage 4 waits.

### 2026-09-25 — Stage 3 player corrections to tower animation

- Owner/surface: Stage 3 combat presentation and removal of obsolete tower
  shot tint from `Stage2Tower`. Existing assets, camera, catalogs and all prior
  uncommitted changes are preserved. No new model or package was introduced.
- Behavior: both towers retain their authored colors; Rapid's model remains
  completely still. Catapult aims from its current bearing, winds back with
  the stone still loaded, releases at the cup socket, follows through and
  smoothly returns/reloads. The existing shot clock drives the pose and release;
  windup fits within the original flight duration, so damage timing is unchanged.
  Enemy damage feedback remains. No Inspector reassignment is required.
- Contracts/files: `Stage3TowerShotMotion`, `Stage3CombatPresentation`, tactical
  binding, `Stage2Tower`, focused tests, tactical spec, D-029 and canonical plan.
- Verification: Unity 6000.6.2f1 EditMode 79/79 and PlayMode 47/47, including
  the full tactical run/restart. The presentation check asserts loaded stone
  during windup, no visible duplicate projectile, release within 0.002 world
  units of the cup, unchanged Rapid position/rotation/color, pause and reset.
  Evidence: `Artifacts/S3-RAPID-PILOT/combat-r2-*`.
  Windows build `Builds/Windows/Stage3CombatR2` launched with a clean player
  log. Actual player frames `art/review/combat-r2-stone-windup.png`,
  `combat-r2-stone-release.png`, `combat-r2-arrow-flight.png` verified distinct
  loaded/released phases and unchanged tower colors. `git diff --check` passes.
- Remaining: player evaluates corrected motion in the open build; Stage 3 gate and
  commercial MVP model/UI/audio scope remain open. Stage 4 awaits that gate.

### 2026-09-25 — Stage 3 tower firing and impact animation (D-029)

- Owner/surface: Stage 3 combat presentation with a narrow Defense/WavesAI/Run
  timing contract. The player requested firing and impact animation for both
  existing towers. Prior uncommitted changes and approved camera are preserved.
- Behavior: Rapid arrow/trail/recoil/impact sparks; Longshot aims the upper
  frame, releases its arm/bowl, throws a stone along an arc, returns and reloads.
  Stone impact emits fragments and a ring at the real area boundary. The
  catapult uses four shared mesh parts with the original atlas/material and
  unchanged triangle count; no new tower/character production was started.
- Contracts/files: `Stage2DefenseCatalog.projectileFlightSeconds` is authored
  in baseline and visual catalogs; Tower forwards it to EnemyGroup. The group
  owns pending shots and launch/impact/clear facts; Run advances them, freezes
  them on pause and clears them on reset. Damage resolves on arrival without
  any renderer/collider/animation event dependency. Dead targets are not
  retargeted; arrows do no extra damage, stone resolves at last known position.
  Wave completion waits for outstanding shots. Range/cadence/damage/costs stay.
  `Stage3CombatPresentation`/`Stage3TowerShotMotion` bind automatically through
  the tactical presentation; no manual Inspector step or package change.
- Verification: EditMode 79/79 and complete PlayMode 47/47, including full
  tactical run and visual flight/pause/impact/restart. A preview test exposed an
  extra 0.338-cell vertical offset in the aiming pivot; fixed in source and
  verified against the original bounds without relaxing the test. Shared mesh
  placement was checked with 24 catapults. This Editor diagnostic does not
  substitute for release GPU profiling. Logs/XML: `Artifacts/S3-RAPID-PILOT/combat-*`.
  Final Windows x64 Development Build is `Builds/Windows/Stage3Combat`; source
  Assets/Packages/ProjectSettings hashes matched the tested isolated copy.
  Final player launched; clean gameplay-ready log and refreshed actual-player
  captures `art/review/combat-arrow-flight.png`, `combat-stone-flight.png`,
  `combat-stone-impact.png` confirm trails, articulated pose and area impact.
  The player remains open; Space starts the next wave. `git diff --check` passes.
- Remaining: player review of animation readability/timing; commercial MVP
  model/UI/audio roster and representative release performance pass remain
  open. Stage 3 gate stays open; Stage 4 is reserved for a new agent after it.

### 2026-09-25 — Stage 3 bowl catapult model -> playable review

- Owner/surface: Stage 3 art and visual integration; dependencies are approved
  concept A, explicit Blender authorization and existing D-028 Longshot rules.
  Added original `art/source/longshot_catapult.blend`, reproducible script,
  atlas/FBX, material, prefab and visual catalog in `Assets/Stage3/VisualPilot`.
  Scene changes only bind the new Longshot catalog; prior camera settings and
  all pre-existing working-tree changes are preserved.
- Behavior: the player-owned second tower now shows masonry, timber frame,
  axle/rope, throwing arm and bowl with a stone. Showcase buys both roles
  through normal placement, Longshot left of Rapid. The visual catalog copies
  rule values from the baseline catalog; no geometry-based combat/navigation.
  Added `BuildCurrentGamePlayer` to build the saved scene without regenerating
  accepted camera/model settings. No manual Inspector wiring remains.
- Verification: isolated Unity 6000.6.2f1 PlayMode 46/46, including complete
  tactical run/restart, matching mesh preview/placement bounds and 24 paid
  copies sharing one mesh/material. Footprint 0.918 x 0.916 cells, height 1.581;
  22,968 triangles, one 2048-square atlas. Batchmode Editor frame diagnostics
  are not release GPU performance evidence. Prior rule EditMode 77/77 remains
  the unchanged gameplay baseline. Final Windows Development build succeeds;
  visible player logs startup, both towers and tactical ready without errors.
  Actual playable camera inspection shows both distinct silhouettes, correct
  one-cell bases and shadows; screenshot `art/review/longshot_in_game.png`.
  Evidence: `Artifacts/S3-RAPID-PILOT/catapult-*`; build/player in
  `Builds/Windows/Stage3Longshot`. `git diff --check` passes.
- Remaining: owner evaluates the finished model in the launched game; this
  does not imply visual acceptance. Arm/projectile animation is not implemented;
  existing attack flash remains. Agree commercial MVP model/UI/audio roster
  before further production; profile representative release load later.
  Existing interactive Editor hot-reload errors were not used as validation;
  the fresh build/test environment is clean. Stage 3 gate remains open;
  Stage 4 starts with a new agent only after that gate closes.

### 2026-09-25 — Stage 3 Longshot concept and D-028 rules checkpoint

- Owner/surface: Stage 3 visual-content proposal and Longshot catalog/attack
  contract; `art/concepts/`, Defense/WavesAI rules and focused tests, canonical
  Stage 3 plan, tower-role spec, art direction and D-028.
- Behavior: player chose the original stone catapult concept A, price 2 wood/5
  stone/0 metal and a two-cell blast dealing the current two damage to each
  enemy. Rapid remains single-target. Metal stays gatherable on the map.
  Blender modeling is explicitly held until separate player authorization.
- Verification: original working-tree changes preserved. Unity 6000.6.2f1
  isolated-copy EditMode 77/77 and PlayMode 45/45 passed, including the
  tactical-scene full two-role run and restart. Test logs/XML are under
  `Artifacts/S3-RAPID-PILOT/longshot-*`. No attributable C# errors; two
  expected invalid-map diagnostics came from negative tests. Windows x64
  Development Build succeeded; hidden headless player smoke logged Stage 3
  gameplay/tactical ready without attributable errors.
- Remaining: agree proposed commercial MVP model/UI/audio scope, then await explicit permission
  before Blender work. Once a model is accepted, check its scale, silhouette,
  camera readability, placement and many-tower cost in the playable scene.
  Stage 3 gate remains open; Stage 4 belongs to a new agent.

### 2026-09-25 — Stage 3 owner accepted runtime 3D (D-027)

- Owner/surface: Stage 3 product decision and visual-production handoff;
  `production/DECISIONS.md`, `gamedesign/GAME_VISION.md`,
  `gamedesign/ART_DIRECTION.md`, `gamedesign/DEVELOPMENT_PLAN.md` and this entry.
- Behavior: player accepted the playable Rapid watchtower and tactical camera,
  and explicitly chose runtime 3D over sprites rendered from 3D. Each future
  model still requires a role/look discussion and image concept approval
  before Blender modeling and Unity integration.
- Verification: player confirmation in chat; documentation-only decision,
  with no scene, runtime, asset or test changes in this entry.
- Remaining: agree commercial MVP asset scope and the next model (Longshot is
  the likely next tower), review its concepts with the player, then implement
  only accepted content. Stage 3 gate stays open; Stage 4 belongs to a new
  agent after that gate closes.

### 2026-09-25 — Stage 3 closer-view limit from player screenshots (D-026)

- Owner/surface: Stage 3 tactical camera near clamp, matching PlayMode check,
  and current visual/plan handoff. The player supplied two real game frames;
  their cell and tower sizes indicate a closest view about 2× less magnified.
- Behavior: closest zoom is 2× the opening view (orthographic size 0.5×)
  instead of 4× (0.25×). D-025 movement behavior, far 2× limit and the saved
  1.205 Inspector step remain.
- Verification: isolated PlayMode 45/45 passed, including the new near bound,
  unchanged far bound, movement/corners, full run and restart. Windows build
  succeeded and was opened from `Builds/Windows/Stage3CloserZoom/`. Player log
  has `S2_INT_GAMEPLAY_READY`, `S3_RAPID_GAME_READY` and
  `S3_TACTICAL_VIEW_READY` without a new attributable error. `git diff --check`
  passed. Stage 3 remains open for visual acceptance by the player.

### 2026-09-25 — Stage 3 fixed movement zoom and non-walkable border (D-025)

- Owner/surface: Stage 3 tactical camera/presentation and their pilot tests.
  The player selected a finite non-walkable visual border after weighing the
  conflicting center, map-edge and 2× zoom requirements.
- Behavior: WASD no longer changes orthographic size. Wheel alone eases toward
  a size from 0.25× to 2× the opening view; the hero remains centered. A
  darkened terrain border surrounds the unchanged 32×32 gameplay map, with no
  walkable/buildable/navigation cells or new gameplay content. The saved
  Inspector multiplier remains 1.205.
- Verification: isolated PlayMode 45/45 passed, including WASD without zoom,
  smooth wheel steps, 2× retreat, centered hero and all viewport corners at
  old gameplay-map edges; the complete run and restart also pass. Windows build
  succeeded and was opened from `Builds/Windows/Stage3CameraSteady/`.
  Player log has `S2_INT_GAMEPLAY_READY`, `S3_RAPID_GAME_READY` and
  `S3_TACTICAL_VIEW_READY` without a new attributable error. The current
  build screenshot is `art/review/stage3_tactical_game.png`; `git diff --check`
  passed. Human camera/tower acceptance and Stage 3 gate remain open.

### 2026-09-25 — Stage 3 map-bound, hero-centered smooth camera (D-024)

- Owner/surface: Stage 3 tactical presentation and opt-in camera behavior in
  `Stage2CameraFollow`; the Stage 2 camera path and gameplay rules are unchanged.
- Behavior: the hero's ground point is the exact screen center. Mouse wheel
  updates a target orthographic size and the image eases toward it. Maximum
  size follows the nearest playable map edge and Game view aspect; the camera
  approaches automatically near borders. The outer grass skirt was removed.
  The player's persistent `TacticalZoomSettings.asset` value of 1.205 is kept.
- Verification: isolated PlayMode 45/45 passed, including live Inspector
  setting, smooth wheel zoom, map corners, ground picking, the full run and
  restart. Windows build succeeded; launched player logged
  `S2_INT_GAMEPLAY_READY`, `S3_RAPID_GAME_READY`, `S3_TACTICAL_VIEW_READY` with
  no new attributable error. `git diff --check` passed. Build:
  `Builds/Windows/Stage3CameraBounds/Not3AStage3RapidPilot.exe`.
- Remaining: the owner assesses the visible camera behavior and tower style in
  the actual game. Stage 3 gate and runtime-3D-versus-sprites production choice
  remain open; no mass content production or Stage 4 handoff yet.

### 2026-09-25 — Stage 3 Inspector zoom tuning (D-023)

- Owner/surface: Stage 3 pilot scene/Editor opening flow and a narrow Stage 2
  camera settings asset. Player highlighted the empty Inspector and asked to
  tune the abrupt wheel strength there.
- Behavior: `TacticalZoomSettings.asset` stores a 1.05..3 multiplier (default
  1.4), which the tactical camera reads live; its custom Inspector presents a
  Russian slider and saves edits during Play Mode. The game opening command
  selects this asset in the right-hand Inspector. Wheel normalization and
  near/far limits remain as D-022; rules have no asset dependency.
- Verification/build: isolated PlayMode 45/45 passed, including live setting
  changes via a scratch ScriptableObject plus small/standard wheel deltas,
  range clamps and ground picking. Windows build succeeded and startup/gameplay/
  tower/tactical-ready markers passed. The Editor had one native crash during
  script reload in `com.unity.pipeline` startup; after restart the scene entered
  Play Mode without a new attributable error. The main scene's serialized
  camera reference points to `TacticalZoomSettings.asset`; Editor status
  confirms `zoomSettingsSelected=True`, scene ready and step 1.4. The Inspector
  was opened and the temporary standalone window closed so the user can tune
  in Unity. `git diff --check` passed. Player chooses final strength; Stage 3
  gate remains open.

### 2026-09-25 — Stage 3 wheel delta correction (D-022)

- Owner/surface: same Stage 3 tactical camera and its input regression test.
  Player still saw almost no zoom after D-021. The camera divided every wheel
  value by 120, so devices reporting small deltas could barely move it.
- Behavior: each nonzero wheel event counts as at least one 2.5× scale step;
  large deltas retain multi-step behavior. D-021 limits and fixed angle remain.
  First four wheel events log raw delta and resulting size for a real-device
  follow-up. No change to building, navigation or playable map cells.
- Verification/build: full isolated PlayMode suite 45/45 passed, including the
  regression with raw wheel deltas 1 and 120, both directions, range clamps,
  ground coverage and picking. Windows build succeeded and was copied to
  `Builds/Windows/Stage3TacticalWheelFix/`. The previous standalone player was
  closed and this build launched; its log has startup, gameplay, tower and
  tactical-ready markers without attributable errors. The main Editor also
  refreshed and resumed the playable scene (`editor-game-status.txt`: ready).
  `git diff --check` passed. Actual wheel units on the player's device will
  be visible as `S3_ZOOM_INPUT` after their next scroll; owner feel review remains.

### 2026-09-25 — Stage 3 stronger tactical zoom (D-021)

- Owner/surface: same Stage 3 tactical camera and noninteractive surrounding
  ground; user found the previous wheel change too weak in both directions.
- Behavior: each wheel notch changes orthographic scale 1.8×; limits are 4×
  closer and 8× farther than opening scale 5. Fixed angle, rules-plane clicks,
  map cell bounds and gameplay rules stay as before. Background ground now
  covers the wider view.
- Verification/build: full isolated PlayMode suite 45/45 passed after correcting
  wide-view ray/ground intersection. Windows build succeeded and is open from
  `Builds/Windows/Stage3TacticalZoomStrong/`. Its Player log has Stage 2
  startup/gameplay and Stage 3 tower/tactical-ready markers, with no attributable
  error. The main interactive Editor also reloaded the change and returned to
  `Stage3Rapid3DGamePilot` in Play Mode (`editor-game-status.txt`: ready=true).
  Owner visual review remains open.

### 2026-09-25 — Stage 3 tactical mouse zoom

- Owner/surface: Stage 3 camera follow, surrounding visual ground and focused
  PlayMode coverage. Depends on D-019 tactical scene and input system.
- Player request: mouse wheel zooms relative to the current opening view;
  `orthographicSize` 5 is clamped to 2.5..20. Fixed angle, navigation, build
  rules and the 32×32 playable boundary stay intact. The visual ground skirt
  fills the wide camera view but adds no cells or collision.
- Verification: focused wheel/boundary/picking check 1/1 and full PlayMode
  suite 45/45 passed in the isolated Unity project. Updated Windows build
  succeeded (`zoom-build.log`) and was copied to
  `Builds/Windows/Stage3TacticalZoom/`. The main interactive Editor reloaded
  the changed scripts, resumed `Stage3Rapid3DGamePilot` in Play Mode and logged
  startup/tactical/tower readiness; status confirms `playing=True, ready=True`.
  Actual Editor game capture `art/review/stage3_editor_game.png` was inspected.
  `git diff --check` passed. Owner visual approval remains open.

### 2026-09-25 — Stage 3 playable scene in the user's existing Editor

- Fixed review delivery: the Windows player had started, but the visible Editor
  still showed the standalone model probe. Opened `Stage3Rapid3DGamePilot` in
  that existing Editor, entered Play Mode and selected the Game tab.
- Surface: `Stage3RapidPilotBuilder` accepts an explicit local request at
  `Artifacts/S3-RAPID-PILOT/open-game.request` on script reload. Requests expire
  after five minutes, are consumed once, skip batch mode, and use Unity's save
  prompt before changing scenes. No automatic launch without a request.
- Verified in the main interactive Editor: successful compilation, startup,
  gameplay, tower and tactical-ready markers; `editor-game-status.txt` records
  the intended scene with `playing=True`, `ready=True`. No new attributable
  exception. Desktop screenshot verification was unavailable because the
  computer-use tool failed to initialize; runtime status is verified via Editor
  APIs. Existing 76 EditMode / 44 PlayMode results apply to unchanged gameplay.
- Remaining: owner's visual review and the previously recorded Stage 3 work.

### 2026-09-25 — Stage 3 D-019 tactical camera/ground -> playable review

- Owner/surface: current Stage 3 agent; shared camera follow, ground coordinate
  conversion and mouse picking plus isolated Stage 3 scene/presentation. Depends
  on accepted Stage 2 loop and the original Rapid R2 Blender output. User
  explicitly authorized camera adaptation now, after supplying more TD views.
- Behavior: one fixed elevated orthographic camera over a unit square ground
  grid; upright Rapid with foundations/shadows, temporary solid hero/objective/
  resource/enemy markers, matching mesh build preview and existing tint feedback.
  Gray-only placement, paid construction, navigation, damage and run rules stay
  authoritative. No final new actor/environment models were approved or made.
- Contracts/files: map adds opt-in planar conversion/hash/clamp; mover uses map
  clamp; camera has opt-in tactical follow and bootstrap validates its fixed
  orientation; build input intersects Z=0. `Stage3PlanarCorner` is a separate
  map asset. `Stage3TacticalPresentation` and `Stage3MeshFeedback` own visuals.
  Source Stage 2 assets keep their legacy view. Updated vision/art/plan, D-019,
  tactical feature brief, related map/build specs, both requested HTML reports.
- Verification: EditMode 76/76; final PlayMode 44/44, including upright FBX,
  real mouse placement/grass rejection, planar movement, full viewport within
  map edges, paid two-role full run/restart and legacy regressions. Windows
  build/startup/pilot/tactical-ready markers passed, final build/player logs
  have no new attributable error. Visually checked actual player capture
  `art/review/stage3_tactical_game.png`. Build is open from
  `Builds/Windows/Stage3TacticalView/`. Evidence: `Artifacts/S3-RAPID-PILOT/tactical-*`.
  Both HTML reports now identify Stage 3 as current and embed that game capture;
  local image links/unique IDs were checked.
- Remaining: owner evaluates the new view; mass art/production budget, sustained
  many-tower performance, final actor art/UI/audio and the Stage 3 gate remain
  open. Stage 4 starts with a new agent. Working implementation is runtime 3D;
  the previous sprite-first recommendation is superseded by D-019.

### 2026-09-25 — Stage 3 Rapid model revision 2 -> owner visual review

- Behavior: the original concept (wood, no visible archer) is approved, but the
  player rejected the first overly simple model. R2 rebuilds the silhouette,
  layered curved shingles, hip caps, heavy beams, rails/slats, riveted collars
  and stone feet. Original procedural wood/stone color and AO are baked into
  one 2048 atlas; FBX has one renderer/material and 31,668 triangles. Actual
  Blender render: `art/review/rapid_watchtower_r2_blender.png`. Model approval
  is still pending; do not confuse concept approval with model acceptance.
- Playable wiring: `Stage3Rapid3DGamePilot` buys one Rapid through normal paid
  placement for immediate review. Its prefab keeps the imported FBX axis
  conversion inside a separate visual orientation parent for the XY camera;
  a dedicated scene light makes the mesh readable. A small depth offset keeps
  feet above the flat map surface. Reimport texture settings before prefab
  mutation (reimporting during mutation can reset imported-root overrides).
  Sprite preview and separate sprite pilot remain available. Gameplay rules
  and the original Stage 2 scene/catalog were not changed by this revision.
- Contracts/files: `art/concepts/rapid_watchtower_approved.png`,
  `art/source/rapid_watchtower.blend` and build scripts,
  `Assets/Stage3/VisualPilot/**`, `Assets/Scenes/Stage3Rapid*`, review captures,
  this register, art direction and the canonical plan. Other uncommitted
  changes were preserved. Build: `Builds/Windows/Stage3RapidPilotR2/`.
- Verification: Blender source exported and rendered; Unity imported the atlas,
  model and sprite. Windows build emitted `S3_RAPID_GAME_BUILD_OK`; actual
  player emitted startup/gameplay/pilot-ready markers and was visually checked
  in `art/review/rapid_r2_in_game.png`. Final build/player logs have no new
  attributable error. Earlier full PlayMode suite passed 40/40; final revision
  checks passed 3/3: both visual pilots plus the existing complete winning
  run/restart (`revision2-verified-tests.xml`). One test launch hit a transient
  Unity IL postprocessor startup failure; the unchanged retry completed.
  Evidence: `Artifacts/S3-RAPID-PILOT/revision2-*`. Immediate editor Camera.Render
  capture has an unreliable ground-material preview; use the actual player
  screenshot and Blender render for visual judgment.
- Remaining: owner reviews R2 and chooses runtime 3D or Blender-rendered sprites.
  Current runtime mesh pilot does not show the existing SpriteRenderer attack
  flash, and uses a sprite placement preview. Actor overlap, picking/surface
  consistency and many-tower performance/LOD still need evaluation before
  adopting runtime 3D. Geometry count is pilot data, not an accepted production
  budget. No broader art, UI or audio production yet; Stage 3 gate remains
  open. Stage 4 belongs to a new agent after Stage 3 closes.

### 2026-09-25 — Stage 2 Rapid hit visibility -> Stage 3

- Behavior: a live corner-map check confirmed one Rapid kills multiple first-wave
  enemies. Each successful shot now flashes the tower briefly, and nonlethal
  enemy damage flashes the enemy; attack rules and values stay the same.
- Contracts/files: `Stage2Tower`, `Stage2Enemy`, focused Defense and Integration
  tests, tower-role spec and canonical plan. No scene or Inspector migration.
- Verification: EditMode 76/76 and PlayMode 39/39 passed. Windows player build
  emitted `S2_INT_BUILD_OK`, launched with `S2_INT_STARTUP_OK` and
  `S2_INT_GAMEPLAY_READY`, and has no new attributable startup error.
  Evidence: `Artifacts/S2-RAPID-FEEDBACK/`.
- Human observation: player confirmed the new Windows build: «теперь
  работает». Repeated attacks are readable; Stage 3 remains a separate new
  task.

### 2026-09-25 — Stage 2 D-018 economy and first-wave pacing -> Stage 3

- Behavior: preparation before every authored wave is 180 s; Rapid costs
  5 gold, 2 wood, 1 stone;
  each run starts with 5 gold and each killed enemy grants 1. Throne contact
  and restart cleanup grant none. Longshot price remains.
- Contracts/files: Run catalog/builder, Economy inventory/catalog/asset,
  Defense payment/catalog/asset/builder, enemy death fact, run reward, temporary
  HUD, focused tests and vision/plan/spec/decision docs. Stage 2 scene uses
  the existing asset references; no Inspector migration is needed.
- Verification: Unity 6000.6.2f1 EditMode 76/76, PlayMode 38/38; the full
  automated run buys a second Rapid after first-wave kills, wins and restarts.
  Windows Development Build emitted `S2_INT_BUILD_OK` and the launched player
  emitted `S2_INT_STARTUP_OK` and `S2_INT_GAMEPLAY_READY`, responsive with no
  new attributable startup error. The later extension to 180 s for waves 2/3
  passed EditMode 76/76 and PlayMode 38/38; its new Windows build started
  cleanly. Evidence: `Artifacts/S2-ECONOMY-REVISION/` and
  `Artifacts/S2-ALL-WAVES-180/`.
- Remaining: no new human pacing observation since D-018; values are
  provisional. Stage 3 still starts in a separate task with a new agent.

### 2026-09-25 — Stage 2 first playable gate -> new Stage 3 agent

- Behavior: D-016 corner defense now runs on a 32×32 map; Rapid and Longshot
  are both wired, selectable, purchasable and active in one automated winning
  run. Gray-only combat placement remains enforced. The old 100×100 asset is
  retained for historical navigation/performance fixtures.
- Contracts/files: new `Stage2Corner32x32.asset` and Map builder, Integration
  scene/builder/tests, map test and current map/vertical-slice/plan/decision
  docs. The existing uncommitted Longshot wiring was preserved. Scene map
  references, entrance/throne/hero/camera positions and resource anchors now
  agree with the corner asset; no new Inspector assignment is required.
- Verification: Unity 6000.6.2f1 EditMode 74/74, PlayMode 38/38, then focused
  Integration 16/16 after adding a viewport assertion. Windows x64 Development
  Build `S2_INT_BUILD_OK` launched with `S2_INT_STARTUP_OK` and
  `S2_INT_GAMEPLAY_READY` for final hash `48A2358B440C3C65`, responsive and
  without a new attributable startup error. A human completed and restarted
  a run, then confirmed the final start shows portal/throne and clear resource
  directions. Screenshot, test reports and logs are under
  `Artifacts/S2-CORNER/`; `git diff --check` passes.
- Remaining: Stage 2 gate is closed. Map size and tempo are provisional; no
  specific blocker was reported. Stage 3 starts only in a separate new task
  with another agent and begins with the small Blender visual pilot.

### 2026-09-25 — Stage agent prompts -> future stage owners

- Behavior: roadmap now contains a standalone starter prompt for each Stage
  2–7 and regression-only prompts for completed Stage 0–1. Each new stage uses
  a new Codex agent/task after the previous gate; one agent carries its stage.
- Contracts/files: `reports/development-roadmap.html`,
  `gamedesign/DEVELOPMENT_PLAN.md`, D-017 in `production/DECISIONS.md` and this
  handoff. Existing Unity scene/runtime/data changes were left untouched.
- Verification: HTML parses; all eight prompts sit in their matching stage,
  and all 18 links/anchors resolve. `git diff --check` passes. Documentation
  only, so Unity tests and build were not rerun for this change.
- Remaining: current Stage 2 agent still needs to complete its playable gate;
  create the Stage 3 agent only after that gate is accepted.

### 2026-09-25 — Roadmap stage navigation

- Behavior/files: `reports/development-roadmap.html` now switches Stage 0–1 and
  Stage 2–7 through accessible button tabs; only the chosen stage panel shows.
  Stage prompts, task-level model guidance and the canonical stage gates remain.
- Verification: HTML structure/links and seven tab-panel bindings pass; a DOM
  interaction check covers click, Arrow, Home/End and the current-stage shortcut.
  `git diff --check` passes. No Unity behavior changed in this edit.
- Remaining: Stage 2 playable work and gate are unchanged.

### 2026-09-25 — Model settings in stage prompts

- Behavior/files: all eight starter prompts in
  `reports/development-roadmap.html` now begin with an explicit Codex model and
  reasoning level; the setting is also visible above the copyable text.
  `gamedesign/DEVELOPMENT_PLAN.md` clarifies that the user selects that setting
  in Codex before creating the stage task, with any later switch described in
  the prompt.
- Verification: parsed every prompt and checked its first line against the
  stage recommendation; HTML links resolve and `git diff --check` passes.
- Remaining: stage gates and Unity work are unchanged.

## Completed Work — First Playable (D-015)

Active workstream: Integration, owner of
`Assets/Scenes/Stage2VerticalSlice.unity`, its scene builder, composition and
temporary readout. Defense owns its existing tower data/runtime. The
uncommitted Longshot scene binding has been preserved and automatically
verified. Dependencies: completed gather/build/path/siege/run loop and the
two delivered Defense roles. Exit evidence: both roles work in the same
automated full run; a player completed and restarted a Windows run, then
confirmed the corrected startup view. Relevant checks pass and no new
attributable startup errors appear.

Next action: a new Stage 3 agent begins the small visual pilot when requested.
`DEVELOPMENT_PLAN.md` is the current short
path. The earlier task schedule, token estimates and proposed content below
are retained only as historical evidence; they are not current assignments.

D-016 update, 2026-09-25: Defense owns the gray-only combat placement check in
`Assets/Stage2/Defense/Runtime/Stage2Defense.cs` and its focused tests. Map owns
the authored map and Editor asset builder; Integration alone owns the scene,
composition and its tests. The corner layout, three resource directions,
gray-only paid towers, route/siege and camera now pass automated checks. The
100×100 asset remains an older opposite-end candidate. Human Windows-run
acceptance and final startup-visibility confirmation are recorded above.

### 2026-09-25 — Defense gray-zone rule -> Map/Integration/QA

- Behavior: Defense preview and confirm reject combat towers on green Forest
  cells, even when the general buildable/walkable masks allow them; no tower,
  payment or occupancy is produced. Gray InvasionCorridor placement remains
  subject to all existing actor, economy and map checks.
- Contracts/files: `Stage2Defense.cs`, focused Defense EditMode/PlayMode tests,
  D-016, current vision/plan/map/building/vertical-slice docs and both HTML
  reports. No Stage 2 scene, map asset or Integration work was overwritten.
- Verification: `git diff --check` passes; both HTML files parse, SVG tags
  balance and local links/anchors resolve. Focused Unity tests are degraded:
  the project is open in an Editor process, and a second batchmode instance
  cannot use the same project. Run Defense EditMode/PlayMode checks in that
  Editor when the current Integration scene work is ready. Map geometry has
  not yet been changed.
- Remaining: Map and Integration implement the selected corner composition,
  update coordinate-dependent tests and qualify the Windows player run.

## Historical Stage 2 Package Notes

### Former active package — S2-DEF-02 (2026-09-25)

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

Historical sequence before D-015: Stage 1 is complete. For Stage 2, reserve the scarce Integration/scene slot
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

Current playability check is in
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

## Historical Stage 2 Claims

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

### 2026-09-25 — Current vision HTML and roadmap cleanup -> All workstreams

- Behavior: the current game design is presented in `reports/game-design.html`;
  `reports/development-roadmap.html` now follows Stage 0–7 with a short active
  path, a model/effort recommendation per task or stage, and the Stage 3
  Blender trigger immediately after the first playable gate.
- Contracts/files: updated `README.md`, reference index, `.gitignore`,
  `DEVELOPMENT_PLAN.md`, `GAME_VISION.md`, D-015 and the two HTML reports.
  The old 55-package roadmap was replaced; historical feature specs,
  measurements and original local reference images were preserved.
- Verification: both HTML documents parse with balanced tags, unique IDs and
  working local links/anchors; `git diff --check` passes. Browser preview of
  local `file://` pages was blocked by the browser URL policy. No Unity
  runtime/scene/asset changed in this documentation task, so Unity tests were
  not rerun.
- Remaining: Stage 2 Integration verifies the in-progress two-tower scene and
  complete Windows run. After acceptance, start one Blender/Unity pilot before
  mass model production.

### 2026-09-25 — Design audit / D-015 -> Integration and later Art

- Behavior: first playable is now one short understandable gather/build/defend
  run on a compact map with two towers and the existing three resources.
  Pixel art is no longer the target; soft cartoon isometric presentation is
  deferred until the core run works. Optional objectives, more enemy/content
  breadth, final UI/audio, fine balance and speculative token packages are
  removed from the first-playable gate.
- Contracts/files: `GAME_VISION.md`, `ART_DIRECTION.md`, `DEVELOPMENT_PLAN.md`,
  `STAGE2_VERTICAL_SLICE.md`, D-015 in `DECISIONS.md`, and status notes in map,
  run, economy, enemy and objective specs. Existing Unity scene/code changes
  by Integration were inspected and left untouched.
- Verification: repository and Unity 6000.6.2f1 scene/data inspected;
  documentation references checked and `git diff --check` passed. No runtime
  behavior changed by this audit, so Unity tests/build were not rerun.
- Remaining: Integration verifies the in-progress Longshot scene wiring and
  one full Windows run; live play decides whether 100×100 feels compact and
  which temporary feedback is still unclear. After that gate, Art tests a
  small target-look asset before choosing runtime 3D or rendered sprites.

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
