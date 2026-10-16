# Feature: Procedural Resource Districts

Status: D-054 generation, D-055 collision and D-056 hybrid refinement implemented
and Windows-verified; owner-selected 2026-10-16. Appearance review remains open.
Owner/workstream: current Stage 3 Map/Integration/Art chat.
Surface: pure footprint/structural layout, scenery/relief/ground/visibility,
scene binding, hero collision diagnostics, minimap and tests.
Dependencies: current planar map, fixed orthographic camera, original sprite
kit and original painted stone/grass pigments. No new package/assembly.

## Accepted outcome and boundary

The owner selected varied seeded resource zones, initially 200–300 cells each.
D-055 adds solid stone/iron and individual tree-trunk collision. D-056 follows
live feedback about abrupt near-erasure, ground seams, suspected forest stutter,
disconnected mine pieces/no visible pit, flat stone height and crowded trees.
The owner explicitly selected hybrid procedural 3D relief plus painted sprites.

The current result must show a real descending excavation with connected
stairs/landings, varied coherent stone terraces and a forest with young/mature
trees, crown-aware gaps and irregular openings. Ground must integrate with the
existing surface. Occluding objects retain partial visibility with a smooth
local reveal around the hero. Suspected motion stutter is measured separately
from confirmed visibility flicker.

Original combat road, portal/throne, hero start, manual resource anchors,
gathering/depletion/recovery, economy, enemies and tower-placement rules remain
authoritative. No production buildings, harvesting large deposits, renewal,
prices, underground traversal or save-file feature is added. Hero rules remain
on XY; visible height/depth uses -Z.

## Generation and structural plan

ResourceDistrictGenerator accepts the authored planar map, settings, seed and
reserved hero start. It returns read-only connected footprints with exact
inclusive cell counts, using a seeded asymmetric connected frontier. It reserves
road/nonwalkable cells with clearance, old anchors/landmarks/start, map edges
and separation between districts. A failed finite placement throws an explicit
diagnostic. It does not modify the map/hash/occupancy, economy or global Random.

ResourceDistrictLayout consumes the footprint and shape settings:

- Forest candidates vary position, scale/age and variant. Minimum spacing grows
  with combined crown size and a variable gap. A winding opening remains clear.
- Stone levels follow distance from the edge and a coherent seeded ridge; they
  are not independent random heights per sprite. Several broad levels remain.
- Iron levels descend into the footprint. A foreground approach is selected
  before stairs are created. Each stair tile's top/bottom matches its successive
  landings and orientation; decoration cannot occupy a stair tile.

The same version/map/settings/seed/start reproduce the footprint and structure.
Shape/spacing/heights and district count/size live in ResourceDistrictArt.asset,
not competing Markdown balance tables. A scene chooses a fresh random seed;
`-worldSeed <integer>` or fixed-seed data reproduces it. Run restart retains the
world. Regenerate(seed) replaces owned scenery, collision, ground/landscape and
minimap overlay; an overlapped hero recovers to nearby free in-bounds ground.

## Hybrid presentation and ground

Stage3ResourceDistrictWorld consumes that single plan. Forest trees remain
painted sprites. Stone/mine use a continuous beveled height surface and connected
stair geometry; surface normals are smoothed across cell samples. Cell data
still owns the levels, while interpolation rounds the transitions. Rock accents
attach to boundary/terrace lips at their authored surface height. Hoist,
shoring and ore attach to their planned rim/landing; old unconnected wall/stair
billboards no longer build the excavation.

Stage3TacticalPresentation omits relief cells from the grass surface. The
outside-world grass is four strips, not a hidden sheet across the entire world.
Thus the pit contains actual lower geometry rather than a dark image over an
intact plane. Rebuilding the world releases and rebuilds the owned terrain
surface and landscape exclusion; the authored map and road are not changed.

Ground aprons conform to the actual rendered grass triangles and use the same
grass pigment, UV scale and phase, with a broad noisy alpha transition. Forest
soil is restrained. The geological boundary also blends back into grass pigment.
The old flat generated ground sprites/textures are superseded. All owned meshes
are explicitly destroyed on regeneration/teardown; the normal world has six
aprons and four geological meshes.

Geology uses an opaque depth-writing base outside the local reveal and a
separate partial-alpha material only within that region. The latter discards
unneeded fragments before expensive shading. The base casts its full shadow;
the partial overlay does not duplicate it. Faces are ordered once for the
fixed camera; no per-frame mesh sorting or arbitrary camera-rotation support.
The painted stone face reuses an interior region of the existing original
StoneResourceSourceAlbedo atlas; source pixels are not edited.

## Stable obstruction reveal and movement

Sprite opacity masks are cached once from the readable atlas on world creation.
Frame updates use only those small masks, not Texture2D pixel reads. Transparent
margins do not trigger occlusion. Per-renderer opacity approaches a retained
partial target with easing and an exit hold; the shader limits that fade to a
soft screen-space area around the hero. The rest of a tree remains visible.
The geological reveal also checks surface depth relative to the hero. Zoom
changes the pixel radius while retaining its world-scale appearance.

HeroObstacleMap is plain C# in the existing Map assembly. Complete stone/iron
footprints remain solid, including the excavation interior; horizontal strips
avoid internal cell seams. Ground circles cover protruding rock/hoist bases.
Each placed tree contributes a scaled trunk circle at its exact ground anchor.
Canopies, transparency and billboard offsets do not define walls.

Stage2PlayerMover sweeps the round hero over the entire displacement and slides
along contacts. Both developer multipliers can run at x10 without tunnelling.
No Rigidbody or new movement manager. Scenes without the district kit retain
existing movement. Teardown detaches only this world's collision binding.
OverlapRecoveryCount distinguishes explicit regeneration/teleport recovery from
ordinary walking; normal walking must not invoke it. Profiler markers:
Hero.GroundMovement and ResourceDistrict.Visibility.

## Data, art and binding

Original D-054 PNGs: Trees, RockModules and MineModules, each a 1536x1024 RGBA
sheet with six parts. Exact prompts, original paths, hashes and rights:
art/source/procedural_resource_districts_2026-10-16_generation.json. D-053 full
concepts remain visual references. All art is original project-generated work;
no third-party pictures become runtime assets. D-056 reuses these PNGs and the
existing original stone/grass pigments without source-image edits.

Tools > Stage 3 > Resource Districts > Build Sprite Kit And Wire imports the
sprite kit and preserves subasset identity. Update Hybrid Data creates/binds
the geology, local reveal and ground-blend materials and saves the data only.
The scene retains its existing resourceDistrictArt reference. The normal game
scene rebuild rebinds that data when present. No other Inspector wiring needed.

Stage2Minimap retains authored terrain/live marker/input semantics and a cached
resource overlay: green forest, pale stone and rusty iron. Regeneration replaces
that overlay; teardown releases it. HUD/minimap pointer exclusion remains owned
by composition. No new UI controls are added for this revision.

## Research informing this implementation

Glowfish's own Trifox write-up uses screen-space masking and player depth to
reveal obstructions smoothly while preserving awareness of their presence:
https://www.trifox-game.com/dissolving-the-world-part-2/.
Epic documents that its temporal dither technique requires TemporalAA; that
particular technique is not enabled in this project's current sprite/AA setup:
https://dev.epicgames.com/documentation/en-us/unreal-engine/stylized-rendering-materials?application_version=4.27.
Unity recommends limiting overlapping barely visible transparent layers:
https://unity.com/blog/games/optimize-your-mobile-game-performance-expert-tips-on-graphics-and-assets.
These inform the choice; project code/shaders are original implementation.

## Observable acceptance and verification

- Safe connected seeded footprints keep area, reservations and authored hash.
- Structural samples verify crown-aware gaps, scale variation and coherent
  stone levels; 80 seeds verify real stair/landing connections.
- Real two-seed Game-camera views show the excavation, stairs and landings,
  varied stone levels, forest gaps and matching ground transitions.
- Framewise reveal keeps partial opacity, eases both directions and holds at
  the edge. Paired frames with/without the hero prove visible contribution.
- Long movement, head-on x10/x10, individual trunks, forest gaps, repeated
  contact, regeneration recovery, reset, clear road and owned cleanup pass.
- Controlled walking must show no recovery jumps or per-update managed
  allocation. Hot-path timing and actual post-warmup walking frames are recorded
  separately; Editor frame time is not a hardware-independent FPS guarantee.
- Relevant map/live-scene checks, no new attributable errors, Windows build/run,
  preserved baseline art and updated canonical handoff are required.

D-054 evidence: Artifacts/S3-PROCEDURAL-DISTRICTS-20261016/, 54 EditMode and
55 PlayMode checks. D-055: Artifacts/S3-RESOURCE-COLLISION-20261016/, 63 and 58.
Those remain historical verified baselines; D-056 changes the presentation.

D-056 verification: Map/Navigation EditMode 69/69; final Stage3/Integration/
RunState PlayMode 61/61 including nine district scenarios. Initial render/paired-capture
and alpha-range failures are retained. The paired capture waits a frame after
changing renderer visibility so the native renderer state is observed. A ground
alpha normalization error and an above-one stair tint alpha were corrected.
The old expanded-world test now accounts precisely for cut-out relief cells
while retaining bounds/movement/minimap checks. Two seeds and close views of
each mineral kind, local canopy reveal and paired hero visibility are captured.
2,000 controlled steps: zero managed bytes, zero walking recoveries, largest
step 0.070002 for 0.07 requested; mean movement+visibility CPU 0.0616 ms.
120 post-warmup walking frames in Windows PlayMode: p50 0.6387 ms, p95 0.8861 ms,
max 2.2926 ms; visibility p95 0.0611 ms, movement p95 0.0248 ms, no recoveries.
These are local Editor samples, not a guarantee of FPS on other hardware.
Windows Development build succeeded (305294365 bytes); visible player PID 21564
started with seed 1103560765, six districts, 138 sprite details, 77 solid strips,
134 ground circles and 18 cached masks. Gameplay/Tactical Ready, authored map
hash ACB42D4354F114AE and no new attributable startup errors. Two validation
errors in the Integration suite are expected negative tests.
Final full-suite/profile/build results: Artifacts/S3-DISTRICT-REFINEMENT-20261016/;
shared frames/JSON: art/review/resource_district_hybrid_*. All 123 preexisting
review/settings files and original sprite PNG bytes hash-verified unchanged.
Owner appearance review and the wider Stage 3 gate remain open.
