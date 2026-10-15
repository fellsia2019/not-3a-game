# Stage 3 Minimap

Active brief: D-047, 2026-10-15. Owner: current Stage 3 chat.
The player requested a bottom-left minimap now; the wider UI remains deferred.

## Behavior

- Show the entire authored playable map, with grass and the actual gray road.
  Fixed orientation: +X right, +Y up, consistent with the tactical camera.
- Distinguish the hero, portal, throne, available resources, living towers and
  living creeps. Update live from the existing systems; depleted resources and
  removed actors disappear and restart restores current state.
- Outline the main camera's visible ground footprint; follow movement, edge
  clamp and mouse/trackpad zoom. Do not include the decorative outer border.
- Anchor a compact framed panel at bottom left, preserving map aspect and
  adapting to viewport size. Keep it visible after win/loss and during pause.
- The panel is passive. Pointer input over it must not place a tower, show a
  world placement ghost or zoom the world behind it. Movement keys retain
  their existing behavior. Map click/pan commands and fog are outside this brief.

## Integration and verification

Use the current immediate HUD rendering path with cached map terrain and live
marker data, rather than selecting the final interface stack for the game.
No extra rendering camera or package is needed. Stage 3's tactical presentation
owns runtime binding; the Stage 2 graybox stays unchanged.

Acceptance: real scene shows the panel and full zigzag at multiple viewport
sizes; actor movement/removal, resource depletion/recovery and restart update
the map; camera outline responds to zoom and clamping. Input tests must prove
an otherwise valid purchase cannot be triggered through the panel, while an
ordinary world click still works. Relevant existing gameplay/camera checks pass
and the live Console has no new attributable errors.

Binding: `Stage3TacticalPresentation.Start` adds/reuses `Stage2Minimap` on its
scene root after composition initialization, then calls `Initialize` with the
existing bootstrap/composition. No serialized scene, material or map migration
is required; ordinary scene rebuilds retain this binding through the component.
`Stage2Defense.GetTower` exposes current registered towers for read-only display.
Build/camera drivers accept a scene-owned pointer exclusion delegate; minimap
teardown removes it. Disabled maps stop excluding input.

Markers read existing state every LateUpdate without scene searches. The
100×100 terrain uses one cached 300×300 texture with a maximum edge of 512 for
larger authored maps; only the panel/markers/camera outline redraw. This is a
schematic map, so it does not change the grass or paving assets in the world.

Visual capture requires the interactive Editor's displayed Game view: use
`Tools > Stage 3 > Minimap > Capture Live Game`. It captures the actual frame
after OnGUI; a separate `Camera.Render()` omits the overlay. Batch PlayMode
checks cover rules/input/projection; the displayed UI is reviewed separately.

API: [Unity GUI.DrawTexture](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/gui/drawtexture).
Mac/Metal verification, 2026-10-15: Stage 3 + Integration + RunState PlayMode
46/46 passed (42.26 s, no skips). Three new checks compare terrain to every
authored cell, exercise live marker lifecycle/depletion/recovery/reset, test
four layout sizes and edge/zoom projection, and prove a valid build behind
the panel cannot spend resources while an outside click still builds.
Interactive actual-Game captures at 819×556 and 1206×614 include the UI and
were visually inspected. Real WASD movement and an ordinary wave also showed
the hero/camera frame moving and red creep markers following the road.
Final Console: 0 errors/0 warnings. All 85 preexisting review/settings files
were restored and hash-verified, with fresh captures retained separately.
Windows was not built/run on this Mac; owner appearance feedback remains open.

Evidence folder: `Artifacts/S3-MINIMAP-20261015/`; shared actual captures:
`art/review/minimap_game_2026-10-15.png`,
`art/review/minimap_game_expanded_2026-10-15.png`; shared JSON summary:
`art/review/minimap_2026-10-15_verification.json`.
