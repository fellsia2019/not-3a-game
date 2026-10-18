# Feature: Wooden Road Barricades

Status: D-064 implemented and verified, approved A/front orientation, 2026-10-18.
Owner: current Stage 3 integration chat; GPT-6 Astra owns all visual implementation.
Dependencies: existing Defense catalog/transaction, shared navigation occupancy,
enemy blocker siege, camera-facing tower sprites and protected build menu.

## Behavior

The wooden barricade is a paid, passive defense on an ordinary gray build cell.
It blocks movement and redirects the shortest enemy route exactly like existing
occupied cells. If no route remains, enemies approach and destroy the blocking
defense, release its occupancy and resume. It has health and zero damage;
even a bound attack callback must never make it shoot or acquire shot motion.
It is not a damaging spike trap. It cannot be placed on green land, actors,
reserved cells or another defense. Purchase/rejection/cancel/reset retain the
existing atomic cost/occupancy contracts. No new placement hotkey is introduced.

Stage2DefenseCatalog.CanAttack defaults true for existing assets; false requires
zero damage. Stage2Tower retains the existing serialized/runtime identity and
health lifecycle. Optional barricadeCatalog in the Stage 3 composition adds
role index 2 after Rapid/Longshot; old scenes keep two roles.
Prices/health are provisional owner-delegated data in
Assets/Stage3/Barricades/WoodenBarricadeCatalog.asset.

## Approved visual and wiring

Approved source: art/concepts/wooden_barricade_2026-10-18/
wooden_barricade_options_v1.png, A only. The owner additionally required front
facing the camera like current towers. Front sprite: wooden_barricade_A_front.png
in the same concept folder; generated from original project references.
All image generation stays with the main agent. Astra imports unchanged pixels,
fits the front/feet inside one cell, shares preview/built registration and updates
the existing menu. Camera, map and existing tower artwork are preserved.
Source, exact prompts, rights and hashes: art/source/
wooden_barricade_2026-10-18_generation.json, including its runtime-generation record.

Stage3BarricadeBuilder.BuildAssets creates the prefab/sprite set/materials/catalog
under Assets/Stage3/Barricades/. Stage3HeroCombatSliceBuilder.BuildAndWire binds
the optional catalog in the existing scene; no manual Inspector work is needed.

## Observable acceptance

- Actual purchase costs once; a passive object occupies its cell, never attacks
  and releases occupancy on destruction/reset. Maze/siege checks remain valid.
- The third card is affordable-gated and stays within protected HUD bounds.
- Preview and built artwork match; adjacent modules/towers fit, stand upright,
  face the camera and show front ground contact at normal/close zoom.
- New assets retain meta files. Relevant EditMode/PlayMode tests and actual
  Windows frames/build/run are recorded in WORKSTREAMS.md.

## Verified result

Passive purchase/cost/occupancy/no-attack checks and actual enemy breach pass.
The third menu card and protected HUD bounds pass the relevant PlayMode checks.
Astra inspected actual adjacent built/preview renders at normal and close zoom:
front-facing modules fit within their cells, with matching feet and clear gaps.
Source/runtime PNG bytes match; all Unity assets keep their meta files.
The Windows build starts without attributable runtime errors. Review renders
come from the actual Editor camera on Windows/Direct3D11; they are not full-HUD
Windows-player screenshots. Source manifest and the slice verification JSON
under art/source/ retain hashes, setup and explicit evidence limits.
