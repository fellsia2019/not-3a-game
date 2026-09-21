# Feature: Stage 2 Data-Driven Map

Status: S2-MAP-01 implemented; ready for Integration/Navigation/Economy handoff,
2026-09-21
Owner/workstream: Foundation / Map
Depends on: D-003, D-009–D-011, accepted Stage 1 `IsoGrid` conversion and
dynamic-pathing contracts
Runtime asset: `Assets/Stage2/Map/Data/Stage2Map100x100.asset`

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
