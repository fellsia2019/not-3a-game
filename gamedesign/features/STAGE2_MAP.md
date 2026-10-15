# Feature: Stage 2 Data-Driven Map

Status: S2-MAP-01 complete, 2026-09-21. S2-MAP-03 measured recommendation is
ready for human P-008 review, 2026-09-24; see its section below.
Owner/workstream: Foundation / Map
Depends on: D-009–D-011, accepted Stage 1 `IsoGrid` conversion and
dynamic-pathing contracts
Runtime asset: `Assets/Stage2/Map/Data/Stage2Map100x100.asset`

D-015 supersedes the former D-003 pixel-art target. The 100x100 asset and
measurements remain historical evidence. D-016 is implemented in a separate
32x32 playable asset: portal and throne share the lower corner; wood, stone and
metal extend right, left and upward in the isometric view. Combat tower
placement requires the gray `InvasionCorridor` surface in addition to the
general masks. The old 100x100 asset stays intact for historical navigation
and performance tests.

## D-016 Playable Map, 2026-09-25

Stage 3 D-019 adds an opt-in planar coordinate layout in the separate
`Stage3PlanarCorner` asset; this Stage 2 asset remains on the original IsoGrid
projection. Map consumers clamp and convert through `Stage2MapDefinition`.
See `STAGE3_TACTICAL_VIEW.md` for the camera/ground migration and its checks.

`Stage2Corner32x32.asset` is now the asset wired into `Stage2VerticalSlice`.
Its bounds are (-16,-16) through (15,15), with the gray defense area at the
lower corner. Entrance (-14,-12) and throne (-5,-12) are nine route cells
apart. The three reachable forest anchors are wood (10,-10), stone (-10,10)
and metal (10,10). The full compact field is within camera bounds. The
`Create or Update Corner Playable Map` Editor command reproduces the asset;
the Integration scene builder uses the same asset and spawns the hero on the
green edge between entrance and throne, so both landmarks are visible at
startup. Map validation, route/diversion, siege, camera, and resource
access are covered by EditMode/PlayMode checks. These coordinates are current
Unity data, not a final size or balance commitment. A player completed a run
and confirmed the final startup view and resource directions are clear; no
distance complaint was reported.

## Outcome And Scope

The first Stage 2 map candidate is one authored 100x100 ScriptableObject rather
than bounds and regions embedded in scene or runtime code. It represents the
forest and invasion corridor as different surfaces on the same 2:1 isometric
plane, with one entrance, one throne, buildable/walkable masks, placement
regions for wood/stone/metal and camera bounds.

This task does not select the final P-008 map size, create a 200x200 variant,
scale navigation, place resource nodes, change either scene or establish final
terrain art/balance.

## Authored Contract

- Cell bounds use an inclusive minimum plus a positive size. Consumers obtain
  minimum, maximum and size from the asset; no Stage 2 consumer hard-codes the
  Stage 1 21x17 bounds.
- A default surface plus non-overlapping rectangular overrides resolves every
  in-bounds cell to `Forest` or `InvasionCorridor`.
- Walkable and buildable masks have authored defaults and rectangular enable/
  disable areas. Disable areas win deterministically. A buildable cell must also
  be walkable before dynamic occupancy is applied.
- The single entrance and throne are explicit optional authored landmarks so a
  missing value can be distinguished from cell `(0, 0)`. Both must be distinct,
  in bounds, walkable, non-buildable and on the invasion corridor.
- Exactly one placement region is authored for each of wood, stone and metal.
  Each has an in-bounds anchor inside its region. Anchors are forest, walkable,
  reserved from building and contained by camera bounds. The candidate orders
  their anchors progressively farther from the throne; exact distribution stays
  runtime data for S2-ECO-02 and later scale evaluation.
- Camera bounds are expressed in the same cell space, remain within the map and
  contain both landmarks and all resource anchors.
- Forest and corridor debug colors live with the map definition and must differ.
  `Stage2MapDebugRenderer` converts the read-only definition into one generated
  vertex-colored diamond mesh; it does not persist generated mesh/material data.

## Validation And Determinism

`ValidateDefinition()` returns an ordered result with stable error codes and
plain-language reasons. It rejects invalid/unsafe cell bounds, unsupported or
overlapping surface regions, invalid mask regions, buildable-but-not-walkable
cells, missing/duplicate/out-of-bounds landmarks, invalid or duplicate resource
regions/anchors, invalid camera bounds and indistinguishable debug colors.

Runtime reads are query-only. Editor-only configuration creates or updates the
asset; consumers use cell, landmark, region and conversion queries. A stable
content hash covers authored fields for diagnostics and reproducibility.

## Integration Wiring Notes

1. Assign `Stage2Map100x100.asset` by serialized reference in the Integration-
   owned Stage 2 scene/composition root.
2. Validate the definition during composition startup and report the first issue
   plus its code; do not silently substitute scene defaults.
3. Assign the same asset to `Stage2MapDebugRenderer` for the gray/green blockout.
   The renderer creates its mesh at runtime/editor preview and owns its cleanup.
4. Bind player limits to `CellBounds`, camera target limits to `CameraBounds`,
   navigation to `ReadCell`/landmarks, and future resource placement to the
   corresponding resource regions. Consumers must not mutate the asset.
5. S2-MAP-02 still owns scalable path search/occupancy. S2-ECO-02 still owns
   actual resource nodes. S2-INT-01 alone creates and wires the Stage 2 scene.

## Acceptance And Verification

- [x] One generated data asset validates and reports 100x100 bounds.
- [x] Green forest and gray invasion-corridor cells are separately queryable and
  produce distinct colors in the generated debug mesh.
- [x] Entrance, throne, masks, wood/stone/metal regions and camera bounds are
  authored data and can change without runtime-code edits.
- [x] Invalid bounds, landmarks, masks, resource regions and camera bounds return
  deterministic validation reasons.
- [x] Stage 1 2:1 cell/world conversion round-trips representative Stage 2 cells.
- [x] S2-MAP-01 EditMode 12/12 passes; full EditMode 20/20 regression passes.
- [x] No scene, ProjectSettings or package file is part of this change.

## Remaining Work

S2-INT-01 wires and smoke-tests the asset in the Integration-owned scene;
S2-MAP-02 consumes the walkability/buildability contract for production-scale
navigation; S2-ECO-02 populates the three placement regions. S2-MAP-03 alone may
accept P-008 or create a comparison 200x200 variant after travel-time and
performance evidence exists.

## S2-MAP-03 — Live Scale And Maze Decision

Status: analysis and consolidated measurement complete, 2026-09-24; P-008
recommendation is ready for human acceptance. The authored candidate is
unchanged. No final size or topology acceptance is inferred from task activation.

The decision must distinguish world bounds, travel layout and enemy battlefield
width. The current asset makes every cell walkable; the gray corridor is a
surface classification, not an enemy boundary. Earlier 100-cell wall and
297-tower zigzag measurements establish navigation correctness, not an
affordable player layout. A current source circuit yields only a few towers.

Recommendation after compact-layout checks: retain the 100x100 envelope and
current masks for the Stage 2 candidate. Do not create 200x200. Resource travel
is differentiated and affordable compact layouts already permit both detour
and siege. A new enemy-only mask or compulsory choke-point rebuild is not
justified by these measurements; it would need separate evidence and acceptance.

Consolidated measurements must record the unchanged map hash, hero speed,
actual WASD travel to each anchor and a complete three-resource return circuit,
gather/build/defense duration and per-resource spend. Record live frame intervals,
Unity memory and Navigation diagnostics on named hardware. Paid layouts must
be evaluated separately from synthetic wall stress cases. A fast search alone
does not accept P-008; a longer walk alone does not justify enlarging the map.

Observable follow-up: use the measured affordable layouts in a comparison
with an actual player and the next wave candidate. Resource ordering must
remain visible and practical. The external player still has to identify the
distance/layout tradeoff; automated travel cannot establish that comprehension.

### Consolidated Evidence And Proposed Disposition

Source: `production/measurements/` reproduction scripts and local raw evidence
`Artifacts/S2-DecisionBatch/summary.json`. Unity 6000.6.2f1 Editor Play,
timeScale 1, i5-12400F, GTX 1060 6GB, 32581 MB system RAM, Windows 11 build
26100; Game view 1101x506, vSync off, target frame rate unrestricted. The map
hash remains `77A23824A9BF24DA`. These are Editor observations, not a standalone
Windows performance certification.

- Actual WASD replay from the authored hero spawn to wood/stone/metal took
  1.588/4.654/9.221 seconds; the return legs took 1.544/4.640/9.221 seconds.
  Eight-direction geometric estimates were 1.633/4.729/9.311 seconds; the
  replay stops within 0.18 world units of the target. One replay per policy,
  known coordinates and no discovery/search time; no human observation claimed.
- A source circuit, holding E for three units of each resource, returning
  and confirming three paid towers, took 29.489 seconds. Travel accounted
  for 19.269 seconds and gathering for 7.218; build timing included three
  scripted one-second confirmation allowances, not human UI timing.
- On the unchanged map, a three-cell wall changes the open 89-step route
  to 93; nine cells change it to 99. A transverse full wall requires 100
  occupancy cells in this diagnostic; the 385-step zigzag uses 297 tokens.
  These particular full-map layouts are not affordable from one three-tower
  resource circuit. Compact controls correct that incomplete comparison:
  three sides of the throne produce a 95-step open route; a four-tower throne
  ring produces siege after an 87-step approach; three neighbors of the
  edge entrance produce immediate siege. The three-tower entrance cap was
  additionally purchased through actual Defense transactions for 3 of each
  resource with no token wall, then breached by an enemy and reopened.
- Three warmups and 30 samples per navigation layout: median search times
  range from 1.018 to 1.044 ms across all eight layouts; observed search p95
  is at most 1.450 ms and replan p95 at most 1.412 ms. The managed allocation counter
  returned zero on this Mono runtime and is unavailable, not zero allocation.
- Three ten-second live-load phases, discarding each first second: sampled
  frame p95 was 2.132 ms idle, 2.348 ms with 64 moving enemies and 2.351 ms
  with 64 enemies plus occupancy revisions at 1 Hz. Total Unity Editor
  allocated-memory medians were 838.93/839.37/839.47 MiB; this includes
  Editor/probe overhead, not Player RAM or a leak test. No sampled frames
  were skipped. The workload produced 11 new full searches, 704 requests
  and 2496 retained-plan hits; shared field reuse worked.

Disposition: **continue with 100x100 and the current masks for Stage 2; do not
expand to 200x200 or mandate a topology rebuild.** Travel orders resource
distance, and no measured navigation bottleneck argues for a size change.
The gray nine-cell corridor does not constrain enemies, but the actual map
already supports an affordable open detour and a three-tower complete
blockage. The earlier draft's proposed compulsory choke-point rework was
withdrawn after the compact-layout control; a 100-cell transverse wall alone
was insufficient evidence for that change. No map asset was edited.

P-008 still needs human acceptance of 100x100 for this slice, not a permanent
MVP-size lock. Reopen the choice if representative content causes a measured
performance/travel problem or players cannot make a meaningful layout choice.
Current evidence does not establish resource discoverability, absence of a
dominant entrance-camping strategy or human comprehension.

### Human Clarification — Future Map Composition, 2026-09-24

Historical proposal, superseded in part by D-016 on 2026-09-25: the player
selected the corner composition and accepted gray-only combat towers for the
first playable. The bullets below describe the earlier alternatives, not the
current implementation brief. General walkability still does not confine
enemies to gray cells.

The current priority is proving the playable core with the MVP scope. The
100x100 recommendation concerns this test slice; it does not fix the eventual
map's size, shape or placement rules. The following are human-proposed future
options, not accepted requirements for a Stage 2 rebuild:

- Extend the map as a rectangle along the resource/invasion axis. "Height"
  here means map extent, not terrain elevation; the flat-map decision remains.
- Offset the throne toward the enemy entrance, with a shorter throne-to-gate
  distance and more depth toward the opposite resource-side boundary. No
  dimensions or distance ratio have been selected.
- Make the gray invasion area wide and winding/zigzag rather than necessarily
  rectangular; towers could further shape the enemies' route.
- Possibly forbid towers on green terrain and permit them only on gray terrain.
  Build permission and enemy walkability are separate rules: a future brief
  must specify both. Currently surface color alone does not confine enemies.

Keep these options in this spec until a map revision becomes active; do not
create speculative feature documents or introduce new architecture for them.
No scene, masks or runtime values change with this clarification. Scope and
verification of a later revision must be agreed when it becomes necessary.
Detailed Blender production remains later, separately scoped work under D-015,
not a prerequisite for proving the Stage 2 loop. This clarification does not
accept the pending cadence or enemy-role proposals.
