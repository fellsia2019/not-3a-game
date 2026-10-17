# Feature: Procedural Resource Districts

Status: D-057 implemented and appearance accepted by the owner, 2026-10-16.
The previous D-056/Astra appearance remains rejected. D-058 adds separate
buildable generators/menu in `RESOURCE_PRODUCTION_BUILDINGS.md`; the spatial
and accepted artwork contracts below remain authoritative for the landmarks.
Owner/workstream: GPT-6 Astra, Stage 3 Map/Integration/Art; runtime and canonical
docs. Parent owns new image generation/provenance outside Assets. Dependency:
reference-faithful artwork must be inspected before runtime art wiring.

## Current D-057 brief — supersedes historical representation below

Use the three original D-053 PNGs as the visual targets. The stone reference
has a high rear/left mass, broad descending extraction benches, pale angular
vertical faces, fractures, loose chips and restrained grass islands. The mine
has a heavy rust-veined dark rock ring around a deep negative space, with
readable successive inner ledges/stairs. The forest has overlapping mature
conifer groups, exposed trunks, young trees and connected sunlit clearings.
Future production buildings shown in those pictures remain separate work.

The smallest boundary is an authored compound definition in the existing art
data: fixed local visual layers/elements, ground contour or trunk circles,
and one local locked production point with its resource/building type. A plain
seeded placement plan chooses safe world origins without shuffling that local
composition. The scene-owned world binds visuals, collision, reveal and minimap
from the same transformed definition. No production manager or economy/UI API
is introduced. Existing small-node E gathering does not include these landmarks.
A typed point is a reservation/attachment contract, not a build command.

A shared data-controlled 0.75 linear scale applies to visual dimensions, ground
footprint/collision and point offset/clearance together relative to the previous
large-object size. Small manual nodes are excluded. Retain reset/regenerate
semantics and all map/road/objective reservations. Fixed artwork orientation
matches the camera; random placement is sufficient and shape variation optional.

First prove a coherent quarry artwork in the actual Game camera. No rejected
procedural geology, old aprons or terrain hole may duplicate or show through
an authored painted landmark. A bitmap carrying the depth illusion can sit
above intact ground; transparency outside the ground contact must blend into
the existing surface. The forest needs independently occludable groups/trunks
where the hero can pass; a single flattened image must not turn its clearings
into walls. Authored stone/iron remain solid at their actual ground contours.

Acceptance is visual first: original-vs-Game whole-object, zoom 5 and 2.5 for
seeds 164032/164033 must preserve the reference's silhouette, main volume,
material and detail hierarchy. Then verify scaled collision/anchor alignment,
matching type and deterministic placement, small-node gathering unchanged,
reset/regeneration cleanup, hero contact/reveal and Console with no new errors.
The previous 13/9/3 passes are technical history only and are explicitly not
visual acceptance. Owner appearance judgement remains required.

## D-057 implemented contract

The three `Landmarks/*.png` runtime imports preserve the generated RGBA source
bytes and fixed-camera composition. Source prompts, original-reference hashes
and output hashes are in `art/source/resource_landmarks_2026-10-16_generation.json`.
`ResourceDistrictArt.asset` owns `landmarkScale`, authored layers, ground contours,
solid strips/trunk circles, depth regions and local point/clearance/facing.
`Stage3ResourceLandmarkBuilder` is an explicit import/review utility; it does not
rewrite the saved scene. Runtime values are serialized in that art asset.

`ResourceDistrictGenerator` places the scaled authored footprint with the existing
seed/reservation rules. `CutsTerrain=false` leaves intact ground; the authored
branch creates no procedural core, apron or scattered edge modules. Local grass
props are excluded beneath the reserved landmark footprint. Mineral solids use
authored ground strips; forest passages use individual authored trunk circles.
All spatial data and artwork share the same scale and world origin.

`ResourceProductionPoint` exposes district index, resource/matching building type,
world position, world clearance, facing, locked status and exact type acceptance.
It is read-only spatial data. No building/economy action consumes it yet. Each
full clearance is tested with hero radius, with an unobstructed approach from
outside the landmark. Existing manual nodes alone continue E gathering.

Authored image regions place clearings at ground depth without moving or altering
the source pixels. Feathered depth boundaries let the hero stand on the production
pad; foreground art retains local eased partial reveal. Fixed camera is part of
this representation contract; arbitrary camera rotation is not supported.
The explicit Capture Live Districts menu captures two seeds at whole/5/2.5 zoom
and production pads, then restores seed, hero, camera and follow state.

## Historical D-054–D-056 implementation (superseded where D-057 differs)

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
- Iron levels descend into the footprint. A rear-rim approach visible from the fixed camera is selected
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
The painted stone pigment samples only a clean interior patch of the original
StoneResourceSourceAlbedo model atlas, with continuous mirrored triplanar
projection. Repeating the previous atlas crop also repeated baked silhouette
shading as dark wedges and rectangular seams; flat-pigment diagnostic renders
confirmed this on Mac as well as the saved Windows frames. Source pixels are
not edited. The terrace transition spans enough existing samples to avoid a
comb-shaped bevel without increasing mesh density.

Ground aprons sort behind painted scenery, so their transparent color cannot
wash over trees or rim rocks. Rock anchors sample the same interpolated height
as the relief; mine rim accents stand at the grass lip. Painted relief accents
use a one-time depth clearance against their own relief vertices inside the
opaque sprite mask, preserving screen position and collision anchors while
preventing cliffs from slicing through the flat billboard. Mine depth retains
readable material values at the lowest landing. The descending stair route
starts on the rear lip: the old foreground descent was nearly parallel to the
fixed view and hid its own treads. Tree positions, crown gaps and collision
remain as accepted in D-056; improved forest floor contrast is local to its
apron, with no change to the shared grass texture.

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

Tools > Stage 3 > Resource Districts > Capture Live Districts is an explicit
Play-mode-only review command. It records both fixture seeds at overview,
normal and close camera sizes under Artifacts/S3-DISTRICT-REVIEWS, then restores
the prior seed, hero and camera state. It does not poll files or author assets.

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


2026-10-16 Astra visual correction: all three D-053 references and both-seed
Windows captures were compared with fresh Mac/Metal Game-camera renders.
Controlled no-shadow and flat-pigment captures isolated the baked-atlas wedges;
local apron/reveal ordering and billboard/relief intersections were also fixed.
Mac verification: generator/layout EditMode 13/13 (including 80 stair-layout
seeds), district PlayMode 9/9; final contact/reveal/capture 3/3 after the last
transparent-order correction. Captures cover seeds 164032/164033 at orthographic
sizes 14, 10, normal 5 and close 2.5, contact and paired hero visibility. The
existing two-seed/lifecycle/collision/reveal assertions remain intact. 2,000
controlled steps report zero managed allocation and recovery jumps; 120 local
Editor walking frames p95 0.4933 ms, maximum 3.7328 ms. No GPU/FPS guarantee is
inferred. Evidence: Artifacts/S3-ASTRA-RESOURCE-VISUAL-20261016/ and new shared
art/review/astra_resource_district_* frames. The original PNGs, old manual nodes,
scene/map, packages/settings and historical captures are preserved. No new
Windows build was performed; owner's appearance review remains open.
