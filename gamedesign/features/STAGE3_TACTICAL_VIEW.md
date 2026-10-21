# Stage 3 Tactical View

2026-10-20, D-103: current authored zoom starts5, nearest2.5, farthest15
(was10). Wheel/pinch share the same bounds. Maximum-distance multiplier lives
in TacticalZoomSettings and the existing camera Inspector; legacy default is2.
Measured top-edge hero-body clipping at15 is corrected by existing planar edge
clearance1→1.5; Unity installer changes that one saved-scene field and builders
retain it. Strict full-renderer viewport and ground-frustum checks pass at
2.5/5/10/15. This does not promise HUD-safe visibility at world corners: existing
timecard/bottom dock can overlay the hero. No art/angle/scale/pivot change.
Verification: art/source/hero_vitals_2026-10-20_verification.json.

2026-10-18, D-071: independent manual sources now populate the green world.
Ordinary trees form seeded irregular groups 1–8; stone/ore retain the small
scale and add larger/depleting forms. Hero axe/pick contacts, eight-phase
hero/skeleton walks, native left mirroring and foreground-tree reveal are wired.
The shared sprite shader honors URP renderer flip/tint/alpha.
Contract: STAGE2_ECONOMY.md. Actual-player evidence and platform limitations:
art/source/manual_resources_motion_2026-10-18_verification.json.

2026-10-18, D-070: PC URP Forward+ uses4xMSAA at renderScale1, without a new
post-AA blur. The three authored landmarks and three production images use
mipmaps/trilinear sampling; original PNGs, camera angle, zoom limits and scene
references remain. Hero animation atlases retain no-mips due to narrow gutters.
The native AppKit bridge now distinguishes precise two-finger translation from
magnification. Existing scene binding routes gestures to M when its viewport
owns the pointer, otherwise to the world. Physical wheel and touchscreen rules
remain. Current verification/limitations are separate from older history below:
art/source/quality_gestures_2026-10-18_verification.json.

Active brief: D-019, 2026-09-25. Owner: Stage 3 visual integration.

2026-10-16, D-051: replace the rectilinear zigzag with the owner's confirmed
sketch: portal at upper left, descent into the first valley, a central rise,
second valley and ascent to the throne at upper right. Retain the lower-left
district, 250×250 bounds, about seven-cell width, current camera/zoom and art.
Editor-authored cubic segments bake a 3.5-cell-radius stroke into the existing
disjoint row spans; terrain/building/enemies consume the same cell mask.
No runtime spline, waypoint system or package. Rebuild entry:
`Tools > Stage 3 > Invasion Road > Apply Accepted Curved Road`.
The existing builder class/method and map asset/GUID remain in use.
Landmark migration: portal (-42,12), throne (55,12); the throne's rightward
placement clears the unchanged metal node/region. Resource anchors, hero start,
catalog values, reservations and enemy gray-only/siege policy retain their roles.
Bootstrap reads new landmark coordinates from the map; no scene/Inspector rewiring.
Original reference/provenance: `art/source/curved_road_2026-10-16_reference.json`.
The D-045/D-050 road-cell hash, old endpoint coordinates and 195-step metric
below describe the preceding map, superseded by D-051's new route.

Windows D-051 verification: Map/Navigation EditMode 47/47; Stage3/Integration/
RunState PlayMode 47/47; final focused route/render check 1/1, no skips. Both
valleys/central rise are traversed in order; width, six-tower detour, full-block
siege, resource access, paid full run/restart, minimap and camera checks pass.
Baked mask: 1,626 gray cells; unopposed route: 225 steps. Builder reproduces
the saved asset byte-for-byte. Prices/waves/speeds retain previous values;
pacing acceptance is still open. Windows Development build and visible player
startup passed with hash `ACB42D4354F114AE`, no attributable runtime error.
Actual player screenshot includes the changed minimap; separate overview uses
only a disposable QA camera's wide zoom/distance/far clip. Player camera intact.
Evidence: `Artifacts/S3-CURVED-ROAD-20261016/`; shared frames/result:
`art/review/curved_road_*2026-10-16*`. Game remains open for owner review.

2026-10-16, D-050: the entire combat road belongs in the world's lower-left
corner. Keep the old 100×100 minimum (-50,-50), extending green land only to
the right and upward. Playable/camera bounds are now (-50,-50)..(199,199),
still 250×250. This supersedes D-049's symmetric bounds below. Road geometry,
gray/build masks, landmarks, resource anchors and hero start keep their exact
world coordinates; no scene or Inspector rewiring. The whole road fits in the
world's lower-left quadrant. The original road-cell hash and 195-step route
remain acceptance checks. Ground, movement, camera clamp and minimap use these
authored bounds. The QA overview follows the authored map center.

Windows D-050 verification: Map/Navigation EditMode 47/47; Stage 3 / Integration /
RunState PlayMode 47/47, no skips. The entire unchanged gray footprint is in the
lower-left quadrant; its relative extents are x=5..92, y=17..65 within 250×250.
Road hash, route, actor/resource anchors, all-cell minimap, edge camera/zoom,
full run/restart and resource checks pass. The map builder changes only the two
bounds relative to D-049. Windows build and visible player startup passed with
map hash `7445A7082A36130A`, no attributable runtime errors. Game stays open for
owner review. Evidence: `Artifacts/S3-WORLD-CORNER-20261016/`, shared overview
and summary: `art/review/world_corner_*2026-10-16*`. Stage 3 gate stays open.

2026-10-16, D-049: expand the green world to 250×250, adding 75 cells on each
side of the former 100×100 bounds. Playable and camera bounds become
(-125,-125)..(124,124); cell scale, fixed view angle and zoom stay unchanged.
The accepted D-045 road, all 1,582 gray cells, build masks/reservations,
portal/throne, hero start and three resource anchors retain their coordinates.
The enemy route remains 195 steps. New forest is walkable by the hero and
forbids combat towers/enemy travel. No extra resource nodes or gameplay tuning.
`Stage3DiagonalMapBuilder` reproduces the expanded bounds; the saved scene
continues referencing the same map asset. Ground, movement, camera clamp and
minimap read those bounds. No scene/Inspector migration or new package.
The D-034 100×100 description below records the earlier migration.

Windows verification, 2026-10-16: Map/Navigation EditMode 47/47 and Stage 3 /
Integration / RunState PlayMode 47/47, no skips. The road-cell SHA256 matches
the pre-expansion asset; every new cell is walkable forest and non-buildable.
WASD crosses the old boundary; rendered ground, full-map minimap, camera edge
clamp, full run/restart and resource lifecycle pass. The map-only builder round
trip preserves every serialized field outside world/camera bounds. Windows
Development build and standalone startup passed without an attributable error.
Evidence: `Artifacts/S3-WORLD-250-20261016/`; shared frames/result:
`art/review/world_250_*2026-10-16*`. The full-world overview uses QA-only zoom
and far clip; normal camera limits remain 2.5–10. No stable FPS claim or Stage 3
visual acceptance is implied by this expansion check.

2026-10-15, D-048: remove floating resource direction boxes and WASD suffixes
from the temporary source panel. Counts/distances, nearby E-gather prompt and
minimap markers remain. The owner approved the cartoon sprite previews for all
three D-037/D-038 resource identities and requested integration. The resource
binding and checks below supersede the initial preview-only scope.

2026-10-15, D-047: bottom-left passive minimap is the next authorized UI element;
its live markers, camera footprint, input exclusion and checks are owned by
`STAGE3_MINIMAP.md`. The wider HUD/UI remains deferred.

2026-10-15, D-046 — Mac trackpad pinch: the owner requested spreading/pinching
two fingers, rather than two-finger scrolling. `Stage2CameraFollow` accepts a
continuous pinch scale ratio, sharing wheel zoom's smoothing, viewport clamp
and original 0.5×..2× size bounds. Spread zooms in; pinch zooms out. Disable /
enable preserves the original zoom limits and does not replay stale input.
`Stage2MacTrackpad` reads an app-local AppKit magnify monitor from the original
universal ARM64/x86_64 `Assets/Plugins/macOS/libNot3ATrackpad.dylib`.
In the Editor, capture is enabled only in unpaused Play Mode with Game both
focused and under the pointer; consumed gestures do not scale the Editor's
Game-view toolbar setting. Focus loss, pause, Play exit and domain reload clear
the monitor/pending deltas. No system-wide input listener or permission is used.
The importer includes macOS Editor/player only, excluding Windows/Linux;
preprocessor guards avoid native calls there. Mouse wheel behavior/settings stay
available. This is Mac development input, not a new mobile/Mac release target.

Native source/rebuild: `native/macos/Not3ATrackpad.mm`,
`bash native/macos/build-trackpad.sh` (Apple Command Line Tools). After rebuilding,
restart Unity because native libraries stay loaded; the committed `.meta`
preserves platform restrictions. Initial setup/reconfiguration entry is
`Not3A.Stage2.Integration.Editor.Stage2MacTrackpadPluginSetup.Configure`.
Native checks: compile `native/macos/tests/TrackpadTests.mm` with clang++ C++17,
ARC/blocks and `-framework AppKit`. They cover monitor lifetime, additive gesture
scaling across polling rates, cancellation/focus reset and invalid input. Unity
PlayMode also checks the real native library load, platform flags, pinch
direction/bounds, neutral/invalid input, disable/resume, and wheel regression.
Physical two-finger gesture acceptance is checked separately in the live Game view.
Mac verification, 2026-10-15: native checks and Unity PlayMode 43/43 passed;
live Console 0 errors/0 warnings. The owner tested pinch/spread and confirmed
"Да, работает"; `S3_PINCH_INPUT` in the live log records actual native delivery.
Shared result: `art/review/trackpad_zoom_2026-10-15_verification.json`;
full logs: `Artifacts/S3-TRACKPAD-20261015/`.

API references: [AppKit magnification](https://developer.apple.com/documentation/appkit/nsevent/magnification),
[app-local event monitor](https://developer.apple.com/documentation/appkit/nsevent/addlocalmonitorforevents(matching:handler:)),
[Unity native plug-in import settings](https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/plug-ins/plug-in-inspector).

2026-10-15, D-045: the owner selected the I double rectangular zigzag and
authorized a lower-left portal instead of redrawing the concept around the
old upper-right entrance. The existing map asset now authors nine connected
seven-wide road reaches, eight square bends, both endpoint reservations and
an opt-in enemy-only gray-surface restriction. Rebuild entry:
`Tools > Stage 3 > Invasion Road > Apply Accepted Double Zigzag`.
The historical `Stage3DiagonalMapBuilder` file/entry is retained so ordinary
Stage 3 rebuilds reproduce the selected route rather than restore the diagonal.
Cartoon World live review frames the portal; the normal hero/resource starting
position, camera tilt, zoom settings, billboard art and controls are unchanged.
An overview image uses the same Game-camera orientation at a wider QA zoom;
this does not change the player's authored zoom range.
Mac/Metal verification: Map/Navigation EditMode 46/46, Stage 3 + Integration +
RunState PlayMode 42/42. Actual Game-camera overview/portal images and JSON
summary are under `art/review/invasion_route_*2026-10-15*`; full logs are in
`Artifacts/S3-INVASION-ROAD-20261015/`. Owner tested and accepted the changed
road; grass appearance is a separate follow-up and longer-route pacing remains
open. The early roughly 17.2-cell distance study is not this blockout's scale.

## Acceptance

- Shared fixed elevated view like the user's TD examples: readable ground,
  upright towers, foundations, spacing and cast shadows.
- Mouse wheel adjusts orthographic size smoothly. The hero is centered away
  from edges; near an edge the camera stops following while the hero moves
  toward the screen edge. The full hero silhouette stays visible there.
  WASD never changes zoom. Both maximum close-up and zoom-out are 2× relative
  to the opening view (orthographic size 0.5×..2×). A finite
  non-walkable visual border remains as backing, outside the ordinary Game
  view. A persistent `TacticalZoomSettings` asset exposes the
  per-step multiplier (1.05..3, initially 1.4) in the Unity Inspector during
  Play Mode. Small and standard device deltas both have a visible effect. Angle stays fixed.
- Screen click selects the ground cell under the cursor at any camera follow
  position. Mesh preview shares the built tower's orientation and footprint;
  allowed/forbidden colors follow existing placement rules. Grass rejects
  construction without spending resources.
- Hero, objectives, resources and enemies remain recognizable as temporary
  solids, including depletion, hit/shot feedback, victory/loss and restart.
- A paid two-role defense completes the run in the new scene; legacy Stage 2
  checks remain valid. No content, prices or combat rule expansion.

## Binding And Migration

`Stage3Rapid3DGamePilot` uses a cloned `Stage3PlanarCorner` map. Map's opt-in
`planarGrid` converts cell coordinates to unit XY positions; the old IsoGrid
mode remains default and retains its content hashes. Navigation still stores
integer cells; combat/economy still use Vector2 ground distances. Moving from
the old projected grid changes distances per cell; passing a run does not
establish final balance.

D-034 updates only the Stage 3 map to 100×100 cells. `Stage3DiagonalMapBuilder`
authors disjoint one-row gray surface regions for an approximately 30-cell
diagonal from the upper right corner, 12 cells across. The gameplay surface
still owns defense placement; the ground renderer samples those same cells.
Entrance, objective and resource anchors move through map data. The 3×3
building exclusions remain centered on entrance and objective, with no model
colliders. The hero starts on green beyond the end of the invasion strip.
The initial D-034 scene opened at orthographic size 17 so the gray corner,
portal and objective fitted in one frame. D-036 restores the player-selected
start size 5 and closest/farthest bounds 2.5–10. The hero stays by the resource
area; the portal and objective begin off-screen. Existing Stage 2 map assets
and scenes keep their previous cells. The 128-cell rendered non-walkable frame
remains, while D-043 keeps all four visible ground corners inside the playable
map at the current zoom. The larger map uses one relief quad per green cell instead of the
old 4×4 subdivisions; the same world-aligned pigment, sampled height and
normals remain. The portal's square foundation rotates in quarter turns and
stays within the authored 3×3 exclusion. Its upright ring, steps and red
aperture are authored at 45 degrees within that foundation, facing along the
D-034 diagonal toward the objective (D-035).

`Stage2CameraFollow` has an opt-in fixed tactical orientation and an offset
from the clamped ground focus. Its tactical-only mouse zoom changes the camera
size, not player position or rules. Its focus follows the hero until a viewport
edge reaches the playable boundary; zoom size eases toward the wheel's requested scale. At wide
sizes it increases camera distance along the same viewing axis so every screen
ray reaches the ground plane. The bounded terrain frame has no
walkable/buildable cells; the viewport clamp keeps it out of sight in the
100×100 Stage 3 map. Camera scale remains unchanged during movement.
The camera reads the shared zoom settings asset on each wheel event, so
adjustments in the right-hand Inspector take effect immediately and persist
after Play Mode. The pilot builder selects this asset when opening the game.
`Stage2PlayerMover` clamps through the map's coordinate conversion. In Stage 3
its planar movement has one cell of visual clearance inside the authored map
centers, so the hero's upper silhouette cannot leave the stopped camera view;
Stage 2 movement keeps its original bounds. `Stage2BuildDriver.TryScreenToCell` intersects the
camera ray with Z=0 instead of assuming an identity camera.

Height is -Z; the Blender FBX axis-conversion root is preserved below a
ground-aligned parent. `Stage3TacticalPresentation` owns terrain materials,
temporary actor geometry and mesh preview. Presentation primitives have no
colliders; they cannot affect navigation. `Stage3MeshFeedback` forwards the
prototype sprite tint into per-renderer material properties, without changing
combat timing or importing model dependencies into rule classes.

Landscape concept B was selected. The first procedural grass texture was
rejected by the player as blurry and too far from the concept. The revised
`LandscapeGrass.png` is an original imagegen ground pigment, regenerated with
approved concept B as the style reference; gray
`LandscapeDefenseEarth.png` is also an original imagegen output. Their exact
prompts and source/rights record are in `art/source/` and `ART_DIRECTION.md`.
The old procedural gray source is retained only as a fallback comparison.
The ground mesh uses continuous
world-aligned UVs (eight cells per grass repeat); the gray cell guides stay visible. The grass
texture contains no bushes, rocks, flowers or raised grass. Separate runtime
3D mesh parts provide individual grass blades and small flowers. The player
rejected the bush shapes, so no bush is generated or scene-bound.
`Stage3LandscapeProps` owns deterministic visual scattering and a denser grass
fringe at the green/gray edge. The dark straight sidewall was rejected and
replaced by an irregular grass-textured verge that slopes from a 0.08-cell
minimum visual lift onto the gray ground on every forest/gray boundary. The
green mesh now has shallow deterministic 3D relief with smooth normals;
separate grass and flowers follow its sampled height. Hero/resource
scale markers follow the visible
surface. None of these meshes has a collider or affects rules. Walkability,
tower placement, click/target coordinates and the single flat gameplay plane
are unchanged. The player still needs to review the revised scene.

Reproduce assets/scene/player using Tools > Stage 3 > Build Rapid Playable
Windows Pilot. The main editor command opens the playable scene via Tools >
Stage 3 > Open Rapid in Playable Game. Source Stage 2 scene/catalogs and the
sprite comparison scene remain available. Final art, many-tower performance,
camera framing at different resolutions and human acceptance remain Stage 3
work; Stage 4 starts with a new agent only after the Stage 3 gate.

## Cartoon skeleton sprite walk trial (D-039, 2026-10-15)

The existing baseline enemy in `Stage3Rapid3DGamePilot` now has a narrow
sprite visual. `Stage3TacticalPresentation.skeletonSpritePrefab` binds
`Assets/Stage3/SkeletonSpritePilot/SkeletonSprite.prefab` to each normal spawn;
the hidden prototype marker and all Stage 2 movement/combat/run rules remain.
The old capsule is retained only as a fallback when the sprite reference is
absent. The source Stage 2 enemy prefab/catalog are not replaced.

`SkeletonWalk.png` is one transparent 1536x1024 imagegen sheet: six frames per
row, rows right/away/left/toward. `Stage3SkeletonSpriteBuilder` uses alpha
extents and torso registration to create shared Sprite subassets in
`SkeletonWalks.asset`; it never rewrites the source image. Body height is
approximately 0.78 view units. `Stage3SkeletonSprite` selects a direction from
actual camera-space movement and advances its cycle per 0.85 traveled cells.
It stops cycling at a blocker and freezes on pause/outcome. The existing hit
flash is forwarded as a short sprite tint. No attack/death animation is claimed.

The body is an unlit alpha-blended, depth-tested camera-facing SpriteRenderer;
its small ground shadow has no collider. Opaque tower/terrain meshes occlude
it through normal scene depth; D-040 sprite landmarks share ground-anchor
sorting with the skeleton. No package or asmdef added.
Tools > Stage 3 > Skeleton Sprites > Build and Wire reproduces import/prefab
and the narrow scene binding. Existing Stage 3 rebuilds retain this binding.
Open Looping Live Review starts an Editor-only ordinary-wave inspection near
the portal/objective, starting prepared waves early and restarting after outcome
until Play Mode stops. It saves no tuning
or hero-spawn change. Normal game entry keeps the existing start/countdown.

Verification: Stage 3/Integration/RunState PlayMode 39/39 passed, including
actual spawned-frame progression, four directions, alpha, hit feedback,
pause, reset and the existing full paid three-wave win/restart. Actual camera
captures at size 5/3.2 remain in `art/review/`; the animated QA recording was
removed at the owner's request. Logs remain in `Artifacts/S3-SKELETON-SPRITE/`.
Windows Development Build: `Builds/Windows/Stage3SkeletonSprite/Not3A.exe`,
0 errors / 7 warnings; see the retained build report/log. Owner acceptance of
style, gait and scale is still required; no final pipeline or Stage 3 completion
is implied.

## Longshot bowl-catapult model (D-028)

The player selected concept A and separately authorized Blender production.
`Stage3LongshotBuilder` creates `LongshotCatapultGame.prefab` and a visual catalog
from the existing Stage 2 Longshot data, replacing only the prefab reference.
Rebuilding copies source catalog values including splash radius; game rules
never reference the mesh. The Stage 3 scene binds this catalog as its second
tower. The Stage 2 baseline prefab/catalog binding is retained.

The shared `Pilot ground-aligned mesh` child is consumed by both paid tower and
placement preview. The model adds no colliders or navigation footprint. Its
square stone base stays inside the occupied cell; the upper machine is angled
for the fixed camera. Tower shot tint has been removed at the player's request;
`Stage3MeshFeedback` remains only on enemies for damage feedback.
The startup showcase places Rapid and Longshot through normal paid construction
so both are visible immediately. Restart clears them through the existing run
lifecycle; it does not grant additional starting resources.

The Blender source retains editable construction parts. The animation revision
exports four shared mesh parts (base, aiming frame, arm/bowl and loaded stone), using the
same original 2048-square atlas and one URP Lit material. Its idle shape and
triangle count are unchanged; the throwing arm pivots at the axle.

## Portal And Soul Objective Sprite Replacements (D-040)

The saved B4 portal and soul-sphere ice-dome concept now provide original
cartoon RGBA sprite artwork, in `Assets/Stage3/LandmarkSpritePilot/`.
`PortalSprite.prefab` and `SoulObjectiveSprite.prefab` replace only the two
existing visual references in `Stage3TacticalPresentation`; old prefab/source
files are retained. Scene regeneration keeps these bindings when the PNGs
exist. Use Tools > Stage 3 > Landmark Sprites > Build and Wire to reproduce
the narrow import/prefab/scene change without rebuilding unrelated models.

The portal imports at 3.25 view units wide, the objective at 3.8 including its
halo; their existing map-centred 3x3 building exclusions remain authoritative.
`Stage3LandmarkSprite` faces the fixed camera and moves the quad along the
view ray just enough to keep the baked foreground base above opaque ground,
without shifting its screen registration. This corrects the initially observed
clipping of front steps/anchors. Both landmarks and skeletons sort by actual
ground Y, so view-ray clearance cannot hide a walking unit behind the portal.
No colliders, new mesh model, package or gameplay rule are introduced.

`LandmarkSpriteFlow.shader` circulates the red interior while keeping the
masonry still; the existing `Stage3EntrancePortal` owns its pause-aware ambient
clock. Cyan light travels along the objective's baked horizontal arc band and
the central flame shimmers, using a separate pause-aware presentation clock.
It never rotates the whole dome/platform. Both effects remain visible after
an outcome and freeze on explicit pause. The glass preserves partial alpha.

Updated regressions verify the actual foreground step/anchor pixels against
the rendered ground, red/blue pixel changes and paused stability, ordinary
portal spawning, reservations/reset and the existing full paid win/restart.
Final Stage 3/Integration/RunState PlayMode: 39/39 in 22.38 s. Windows Development
Build `Builds/Windows/Stage3LandmarkSprites/Not3A.exe`: 0 errors / 7 warnings.
Screenshots are `art/review/{portal_sprite_game_close,
soul_objective_sprite_game_close,landmark_sprites_game_overview}.png`;
the animated QA recording was removed. Logs/reports are in
`Artifacts/S3-LANDMARK-SPRITES/`. The owner approved the appearance before
D-041; these replacements do not close Stage 3 or choose every remaining asset.

## Cartoon Towers And Terrain (D-041)

The owner approved the D-040 landmark appearance, then requested matching
Rapid/Longshot sprites and terrain based on the original cartoon B board.
The subsequent clarification explicitly keeps the existing map shape,
100x100 cell data, diagonal invasion area, anchors, resources and camera.

`Assets/Stage3/CartoonWorldPilot/` owns the four original imagegen PNGs,
shared Sprite subassets, two inactive `Stage2Tower` prefabs and copied catalogs.
Only the prefab references change; all existing catalog gameplay values are
preserved. `Stage3CartoonWorldBuilder.BuildAndWire` imports and wires this
surface without rebuilding maps/resources. The main Stage 3 builder retains
these selections. Old 3D towers and the historical comparison scene remain.
Runtime scene remains `Assets/Scenes/Stage3Rapid3DGamePilot.unity` despite its
historical filename.

`Stage3TowerSprite` billboards the artwork and sorts by the occupied ground
anchor. Foot pivots register foundations, and view-ray clearance keeps the
drawn front footings above terrain. Shared soft contact shadows are flat
presentation meshes, without colliders. The placement preview uses the same
sprite artwork with allowed/rejected tint and existing cell selection.

Rapid remains still and releases its existing arrow view at the drawn slit.
Longshot selects one of four drawn mechanism bearings; it no longer has
continuous 3D yaw. Shot-clock poses show loaded rest, draw-back, empty release
and return. The sheet's detached-stone frame is never shown, preventing a
duplicate projectile. The actual stone view starts at a per-bearing cup socket
after the existing release delay; rule-owned damage and area effects are
unchanged. Explicit pause freezes targeting, poses and projectile movement.
This is a finite drawn-frame animation, not a smooth 3D interpolation.

`TacticalGrass.mat` and `TacticalPaving.mat` now reference original cartoon
textures with unlit colors. Paving UVs rotate their courses toward the reference
and correct the source slab aspect; this changes neither picking nor navigation.
The existing grass apron uses `CartoonGrassVerge.shader` with vertex alpha,
fading toward paving while retaining the existing slight visual relief and
separate grass/flowers. No walls, bushes, collision or new terrain type added.

Acceptance checks include transparent source PNGs, unchanged map size/anchors,
shared preview/build artwork, paid construction of 24 towers, four aiming
bearings, shot windup/release/impact, pause, restart and the paid full-run test.
Review evidence: `art/review/cartoon_world_game_overview.png` and
`cartoon_{rapid,longshot}_game_close.png`. The animated QA recording was removed.
Exact prompts and source hashes: `art/source/cartoon_map_tower_prompts.json`
and `cartoon_map_tower_provenance.json`. Runtime tower/terrain appearance still
awaits the owner's review; remaining Stage 3 and stable performance are open.
Final Stage 3/Integration/RunState PlayMode: 40/40 in 29.69 s. Windows
Development Build: 0 errors / 7 warnings. `Tools > Stage 3 > Cartoon World >
Open Live Review` starts ordinary preparation framed on the towers and
objectives; Space starts the wave. This framing is Editor-only and not saved.

## Tower placement readability — D-061, 2026-10-17

During tower selection, show a contrasting ground grid over actual gray road
build cells. It follows the existing unit-cell snapping, remains visible when
the pointer is over HUD, and hides with cancel/pause/end/reset or switching to a
generator. Forest, portal/objective reservations and non-buildable cells are
excluded. Existing red/green silhouettes continue to show transient legality.

`Stage3TacticalPresentation` owns one cached mesh/material per landscape build,
drawn above paving and below tower sprites, without colliders or navigation
changes. It is released/rebuilt with the landscape; no scene wiring is required.

`RapidGridFacing.png` and `LongshotGridFacing.png` are original-image edits with
45-degree yaw-corrected front views, rather than tilted billboard transforms.
`Stage3CartoonWorldBuilder.RefreshTowerFacing` imports only the two tower sets
and prefabs. After owner feedback on the first integration's vertical gap and
overlapping neighboring bodies, the set stores robust visible body bounds and
cell-fit settings. Import ignores isolated alpha specks in transparent padding.
`Stage3TowerSprite` uniformly fits the whole body width (including roof) within
one cell, then registers its visible bottom at the front edge with a small inset.
Depth clearance changes only camera depth, keeping that screen contact fixed.
The logical cell anchor stays unchanged; preview and built towers share this fit.
Contact shadows fit the same cell. Longshot keeps its original separate spoon
and stone; both scale with the body, and `Stage3CombatPresentation` carries the
loaded stone's world scale into flight. Camera, map, original PNGs and gameplay
values remain. Existing sets/prefabs are updated in place; no manual wiring.
Full prompts, source rights and hashes: `art/source/tower_grid_facing_2026-10-17.json`.
Mac/Metal final verification: 14/14 PlayMode, zero failures/skips. Checks cover legal
gray-cell geometry, visible rendered line contrast, selection/HUD hover/cancel,
generator switching, pause/reset, both shared preview/built sprites and bounds,
repeated paid placement, four aiming directions and both projectile/impact flows.
The follow-up renders a dense Longshot/Rapid/Rapid/Longshot row plus previews of
both types, measuring actual silhouette pixels against cell sides and front at
normal/near zoom; it also checks loaded-to-flight stone scale. Final row captures
were inspected at both zooms. No new attributable Console errors. Evidence:
`Artifacts/S3-TOWER-CELL-FIT-20261017/{final-playmode.xml,final/}` and
`art/review/tower_cell_fit_*2026-10-17.png`; initial grid evidence remains in
`Artifacts/S3-TOWER-GRID-20261017/`. Owner visual acceptance and Windows target
validation remain; no scene or Inspector migration is required.

## D-042 — Revised terrain and planted articulated sprite towers

D-041's first rendered result was rejected. D-042 authorizes original V2 turf,
overgrown chipped paving, low broken masonry, bushes and flowers inspired by
the reference's gameplay strip. `Stage3RoadsideArt` stores six shared sprites;
`Stage3RoadsideDecoration` composes a bounded set along the existing corridor,
including its nonwalkable visible border. Decoration has no colliders and keeps
objective/resource clearance; it adds no occupied cell or gameplay obstruction.
The former bright triangle verge is removed. The grass apron has an irregular
cutout edge; paving UVs account for the drawn masonry's screen perspective.

`Stage3TowerSpriteSet` now also holds a spoon and stone sprite. Longshot uses
one stationary foundation/support sprite for every bearing and throw, preventing
whole-building frame deformation. `Stage3TowerSprite.SetThrow` drives the
separate spoon smoothly; bowl dimensions remain fixed. Its actual transformed
cup center is sampled at the existing release time and used by the existing
projectile view. Loaded stone is visible until release, then after recovery.
Reset restores loaded rest, explicit pause freezes articulation. Rapid remains
static with red roof and improved footing contact. Both placement previews and
paid towers consume the same artwork. This layered 2D mechanism does not claim
physically exact 3D foreshortening or a rotating 3D support frame.

Narrow rebuild remains `Stage3CartoonWorldBuilder.BuildAndWire`; the main scene
builder preserves V2 art and roadside references. No migration of gameplay data
is needed, no package is added, original PNGs and their metadata are retained.
Current evidence: `art/review/astra_world_game_overview.png`, both tower closeups,
and `Artifacts/S3-ASTRA-VISUAL/frames/` (ordinary first-wave actual Game camera).
Source rights/prompts/hashes: `art/source/astra_cartoon_rework_*.json`.
Owner review remains required before visual acceptance.

## Historical 3D Soul Sphere Objective (D-031, visuals replaced by D-040)

`SoulSphereIceDome.prefab` replaces the temporary throne marker in this scene.
Its original Blender geometry has separate slate masonry, raised central bowl,
four anchors, an inner soul sphere and flame, a smooth transparent blue dome,
and an external orbit. Dedicated smooth-ice and additive flame/wave shaders
separate clear surfaces from emitted light; the Stage 3 scene enables a modest
bloom profile for the energy. No cracks or shards are added to the ice.

The footprint is approximately three cells wide. `Stage3PlanarCorner.asset`
reserves a 3x3 building exclusion centered on the existing throne cell. This
is authored map data, not a mesh-bounds query. Enemy target, health, movement
and loss rules retain their existing contract. The visual has no colliders.
`Stage3SoulSphereMotion` rotates only the wave around world -Z, the ground
normal. Model-local FBX axes must never be used as the gameplay vertical.

The initial small model and its vertical orbit were rejected. The owner
accepted runtime revision 3 on 2026-09-26; retain its approved appearance.

## Historical 3D Entrance Portal (D-033, visuals replaced by D-040)

`EntrancePortal.prefab` replaces the entrance marker in the actual 3D scene.
It has three original Blender meshes: intact dark masonry, red inlays and a
red aperture. `Stage3EntrancePortal` animates only a shader property, pauses
with the run and has no spawn, damage, placement or navigation authority.
`Stage3TacticalPresentation` binds it to the existing `EntranceMarker` and
run driver; the old marker remains the missing-prefab fallback.

The red interior continuously circulates through rotating texture coordinates
and a bounded inward-moving distortion. Its speed is authored in
`EntrancePortalVortex.mat` (`_CirculationSpeed`). Explicit run pause freezes
the presentation clock; victory/defeat no longer stop this ambient effect.
An unbound portal can also animate in a Play-mode prefab preview. Render
regressions compare actual red pixels over time, including pause and the
victory screen, rather than treating an increasing clock as visible motion.
`Stage3EntrancePortalReview` accepts an explicit `motion` request for a
six-second Game camera recording at a fixed 15 Hz simulation step, restored
after capture. It records both tactical and close views without changing the
authored camera or scene.

After the first placement review, the owner requested a quarter turn and
the same 3x3 size as the objective. The source now has a 2.945x2.945 base;
the ground-aligned prefab pivot turns 90 degrees from its previous bearing,
facing +X along the route. Height is approximately 3.16 world units along -Z.
The builder migrates only the entrance's building exclusion in
`Stage3PlanarCorner.asset`, from the central cell to a centered 3x3 area.
This operation is repeatable. The spawn cell, walkability, original Stage 2
map and game rules are unchanged. There are no model colliders.

Rebuild through `Stage3RapidPilotBuilder.Build3DGameAssets`; the portal builder
preserves prefab/material GUIDs. The paid full-run regression now places its
first tower just outside the expanded exclusion. The owner saw the aperture
edge-on in the actual tactical screenshot and explicitly retained its bearing
toward the objective. On the D-034 map the owner then requested a 45-degree
gate angle (D-035). The Blender source turns the upright assembly within the
unchanged square base, giving a visible red opening in the actual Game scene;
the map's nine-cell exclusion and all spawn/navigation rules stay the same.
The overall Stage 3 gate remains open.

## Approved resource sprites — D-048, following D-037/D-038

2026-10-15, D-048 follow-up: owner requested new sprite previews for all three
sources in the current cartoon B style. Preserve the chosen conifer/granite/
rust-seamed iron identities and the camera angle. Original concepts are shape
references, B is the style reference; built-in ImageGen produces transparent
original cutouts, saved unchanged under
`art/concepts/resource_sprite_review_2026-10-15/`. Prompts and source/output
hashes are in `art/source/resource_sprite_preview_2026-10-15_*`. The owner
approved all three previews and explicitly requested using them in the game.
All three original RGBA previews are saved and inspected; input/output hashes
and alpha checks passed. UI deletion compiled and was checked in actual Game,
with Console 0 errors/0 warnings. Shared result/capture:
`art/review/resource_preview_2026-10-15_verification.json`,
`art/review/resource_direction_labels_removed_2026-10-15.png`.

Runtime integration: exact PNG copies under
`Assets/Stage3/ResourceSpritePilot/`, with single-sprite import settings and
one shared existing unlit transparent sprite shader. `Stage3ResourceSprite`
faces the tactical camera, sorts by ground anchor and clears the terrain along
the view ray, keeping the painted footing registered to the original resource
node. `Stage3TacticalPresentation` still places the visual at the same economy
anchor/grass height; the existing parent marker hides and returns on depletion/
recovery. There are no gameplay colliders or changes to gathering, stock,
timers, map cells or minimap markers.

Rebuild/wiring: `Tools > Stage 3 > Resource Sprites > Build and Wire`, or
`Not3A.Stage3.VisualPilot.Editor.Stage3ResourceSpriteBuilder.BuildAndWire`.
The existing `woodSourceModel`/`stoneSourceModel`/`metalSourceModel` serialized
fields reference the new sprite prefabs; their names and old 3D assets are
preserved. Ordinary Stage 3 and Cartoon World rebuilds retain this selection.
Import computes scale from solid-alpha width, rather than transparent canvas
margins, and authors the base pivot separately for foliage and rock footprints.
PNG pixels remain unchanged. Texture metadata/prefabs hold runtime scale/pivot.
Live review: `Tools > Stage 3 > Resource Sprites > Open Live Review`; the three
`Frame ...` commands are Editor-only QA framing, not player camera controls.
Verification: Mac/Metal Stage 3 + Integration + RunState PlayMode 46/46 passed
(42.62 s, no skips). Affected resource checks now exercise sprite size/footing,
camera alignment, no duplicated mesh/collider, depletion/recovery/reset and
actual rendered visibility at sizes 5 and 2.5. The initial footing assertion
was corrected to use the existing grass relief rather than the flat rules
plane; final run passed. Existing camera/trackpad, minimap, paid defense,
combat and full-run/restart checks passed. All six real Game-camera frames
were inspected. Interactive Game review visited all three nodes with an
E-gather prompt; Console 0 errors/0 warnings. All 90 preexisting review/settings
files were restored and hash-verified after keeping fresh captures separately.
The accepted road/map is byte-identical; Windows was not rebuilt/run on Mac.
Evidence: `Artifacts/S3-RESOURCE-SPRITES-20261015/`; shared captures/result:
`art/review/resource_sprite_*2026-10-15*`; original/runtime PNG provenance:
`art/source/resource_sprites_2026-10-15_provenance.json`.

### Earlier 3D resource trial (preserved history)

The existing wood, stone and metal nodes now use original Blender models:
wood B is a compact conifer, stone A is a pale granite boulder and metal is
iron-ore outcrop A with rusty seams. `art/source/build_resource_sources.py`
rebuilds their editable `.blend` files, three FBXs and three 2048×2048 baked
albedo atlases. Both the coarse first set and the smoothed/textured second
set were rejected by the owner. The concepts remain correct and approved.
The current stone revision corrects its exported front, lowers
and widens the compact composition, uses independent fracture planes and
surface-projected sparse cracks. Its original imagegen granite surface is
baked to the same runtime atlas; source/prompt/provenance are in `art/source/`
and `ART_DIRECTION.md`. Per-triangle albedo tint was replaced with continuous
POINT tint from smooth vertex normals following the owner's smoothing request.
The owner authorized continuing the other approved concepts. The new conifer
uses individually shaped closed boughs, a closed terminal shoot and a grooved
trunk (`resource_wood_model.py`). Upper/lower bough normals split only at the
perimeter, preventing downward-facing upper edge normals and dark stitches.
Radial contour samples are ordered around each bough center to prevent
folded strips; source checks reject reversed upper/lower shell faces and
reversed crown faces. The outline's point positions are preserved.
The ore module (`resource_ore_model.py`) places rusty mineral inside separate
dark fractured masses and uses an original generated slate surface. Iron
seams, foliage tint and bark remain authored procedurally.
All three authoring modules bake a 180-degree Z turn before FBX export so their
foreground fragments face the existing tactical camera through the unchanged
FBX and prefab X pivots. The stone base is recessed by 0.03 cell into the sampled
visual turf; no collider, cell coordinate or gameplay plane is changed.
These meshes and their URP materials are project-created; the review concepts
are not copied into runtime textures. `Stage3ResourceSourceBuilder` imports
the albedo as sRGB with mipmaps and the stone/ore relief as tangent normal maps,
then creates one visual prefab for
each FBX. `Stage3TacticalPresentation` parents it under the corresponding
existing Economy marker and lifts it to the sampled grass surface. Each has
one renderer, one atlas material and no collider. The conifer atlas uses a
foliage-colored background and extended UV padding to prevent black mip bleed
on its narrow bough islands. It receives and casts shadows normally.
`Stage2GameComposition` still hides each marker
when stock reaches zero and restores it on recovery. Resource cells, stock,
timers, navigation, interaction range and payments are independent of meshes.

The playable-scene checks assert cell footprint, visible height, no collider,
supported materials, depletion/recovery and restore. Actual Game-camera
captures at orthographic sizes 5 and 2.5 are in
`art/review/resource_{stone,wood,metal}_game_{default,close}.png`. The
corresponding PlayMode suite includes the paid full three-wave win and restart.
`Artifacts/S3-RESOURCE-SOURCES/render-comparison.txt` records a local
Editor camera render/readback comparison; it is not a standalone FPS target.
The rejected second revision had large p95 outliers even with sources hidden.
The current stone/wood/ore have 5832/33236/9464 triangles. Final local Editor
on/off medians are 2.449/2.433, 2.457/2.455 and 2.595/2.424 ms respectively;
on p95 is 3.138/2.935/3.225 ms on GTX 1060 6GB. This bounded comparison does
not close the standalone performance gate.
The latest full Stage 3/Integration/RunState PlayMode suites passed 36/36 in
35.50 s after the stone smoothing and wood/ore replacement. The Windows
Development Build with these assets succeeded with 0 errors and 3 warnings;
the playable scene was reopened after build. Evidence is
`Artifacts/S3-RESOURCE-SOURCES/playmode.xml` and `build.txt`.

## Tower combat animation — player-requested follow-up

- Rapid releases a visible arrow with a short trail; its model and color stay fixed.
  its impact produces a small spark burst at the enemy.
- Longshot smoothly aims from its current bearing, winds back with the stone
  still in its cup, then throws. The projectile appears exactly at the cup's
  release position; only then is the loaded stone hidden. The arm completes
  its swing and smoothly returns before reloading. The stone follows a raised arc. Impact produces stone
  fragments and a fading ground ring at the actual area-damage boundary.
- Per the player's marked screenshot, the entire inner frame, axle, arm and
  cup turn together about a vertical line through the occupied cell center.
  `Catapult centered yaw` is an explicit world-aligned visual pivot; FBX import
  conversions remain below it. The masonry/parapet mesh stays outside that
  hierarchy. Yaw cannot tilt, translate or orbit the frame around another point.
  The arm's throw remains a separate rotation about its axle. Forward is derived
  from the imported cup/axle positions. Between throws, the mechanism tracks
  the nearest live enemy through the same read-only query used by combat;
  aiming itself cannot fire, reserve a target or apply damage.
- `Stage2EnemyGroup` owns pending shot time and resolves damage on arrival.
  `projectileFlightSeconds` is authored in the Defense catalogs. Legacy data
  defaults to instantaneous hits. No Animation Event, mesh, collider or visual
  component applies damage or chooses a target.
- The selected target's current position is followed until impact. If it dies
  earlier, the arrow lands at its last known position without a second hit or
  reward; stone area damage resolves around that last position. No retargeting.
  Range, cooldown, prices, damage and Longshot radius remain unchanged.
- Windup/release presentation uses the run-owned shot clock and fits inside
  the existing launch-to-impact duration, preserving damage timing.
- Pause freezes rule flight, tower motion and effects. Restart clears all
  pending shots and visual effects. A wave waits for outstanding shots to land
  before recovery/victory; existing loss cleanup cancels them.
- `Stage3CombatPresentation` observes launch/impact/clear facts and shares
  original procedural arrow/debris meshes and effect material. Preview has no
  combat motion. The approved tower roots and their occupied cells never move.

D-042 verification: Stage3/Integration/RunState PlayMode 40/40 (33.00 s);
after final verge removal, capture/landscape 2/2 and clearance 1/1. Windows
Development build 0 errors / 7 existing warnings; startup smoke passed.
Evidence: `Artifacts/S3-ASTRA-VISUAL/`, `art/review/astra_*`. Visual acceptance
remains open with the owner.

D-042 final close review: Longshot flight uses the same painted stone as the
loaded cup, including foreground ordering at release. The paved map edge gets
an irregular grass apron against the nonwalkable border; border turf uses the
same tint. The throw was checked at 60 Hz (90 rendered frames / 1.5 s,
no synthetic interpolation); the optional recording was removed.
