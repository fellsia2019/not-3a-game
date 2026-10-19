# Tower Upgrades

Status: D-079 implemented and locally qualified, 2026-10-19. Owner requests card upgrade/sale
and equal-level drag merging, and clarifies upgrade scope as towers and barricades.
D-078 v4 presentation is already approved. The following runtime contract supersedes
older proposed details below; local qualification is recorded below.

## D-080 selection repair

Construction remains open after placement. D-079 input had incorrectly required
Window.None, blocking world selection/drag in that normal state. D-080 allows
selection and the same strip above B; owned-building presses cancel any draft
before placement can charge. Build cards already choose their blueprint, so the
redundant Select button is removed. Input System event capture retains the
press origin when a fast down/move/up arrives before the next Update. Native
open-dock catalogue→select→move→merge→upgrade→sale passed, as did24 unique
PlayMode cases and final4 input regressions. FastMouse merging is disabled only
while the binding owns input and its previous runtime setting is restored.
Evidence: art/source/construction_selection_fix_2026-10-19_verification.json.

## Current implementation contract

- Matching type + level merges into the recipient cell. Donor is removed once;
  no additional payment. Paid upgrade uses the next row in that type's catalog.
- Price and next HP/damage are visible before paying; current range/interval stay
  visible. Maximum/unaffordable actions are disabled. Refund is shown in advance.
- Sale refunds floor(50% actual investments) per currency. Both merged towers and
  paid upgrades contribute; no conversion or duplicate payout. Overflow rejects
  the whole sale. Zero rounded refund still permits removal.
- Upgrade preserves HP fraction and cooldown fraction; merge uses total current /
  total maximum HP and the larger cooldown fraction. No free full heal or instant
  extra shot. Existing projectiles keep their launch stats.
- Drag keeps source and occupancy in place until commit; silhouette picking finds
  recipient above its cell. D-081 uses light blue for a valid merge, green for
  valid empty-cell movement/construction and red for invalid drops. Small silver
  D-082 compact LVL tags above built towers show their actual current level at a readable
  screen size through zoom; no tier classification is added. Escape,
  right click, UI drop, pause/modal/loss/reset cancel with no payment or deletion.
  Empty-cell movement is free during preparation only. Upgrade/merge/sale also
  work during waves; movement cannot be used to kite sieging enemies.
- D-062 snapshots include level, actual investments, HP, cooldown, role and cell.
  Rewind restores both buildings and original inventory; clears selection/drag.
- Existing Stage3 Rapid/Longshot/Barricade catalogs contain editable prototype
  tier rows; five current levels are provisional, not a permanent content limit.
  Prices and stats are not duplicated as Markdown balance tables. Stage3 Rapid's
  five-gold construction surcharge is removed explicitly. Historical Stage2 data
  stays unchanged; missing upgrade rows mean base level only for older catalogs.
- No scene, prefab, package or Inspector rewire; no new tower raster artwork.
  Production, workshop, dwelling and throne upgrades are outside this slice.

## Future tower unlocks and other buildings

New tower types follow the approved future chain: buy a technology from a city
merchant, study it at the research workshop, then construct that type. Purchasing
is not studying and does not create a tower. Cities/merchants/new tower types are
not implemented by this slice; existing Rapid/Longshot/barricade remain available.

All owned buildings use the common selected card, showing only real statistics.
Workshop links to full-screen research; no Move button is added. Production,
workshop, dwelling and throne effects/prices need their own implementation slice.
The throne's three +5 max-HP upgrades remain an accepted target, not runtime here.

The five current tower levels are editable prototype tuning. Future caps,
research gates and per-level artwork require their own brief; no permanent limit
is inferred from the earlier design proposal. Hero/profession/runes progression
is governed by its own specs and is not part of tower upgrade accounting.

## Verification and wiring

D-081 visual correction: 4/4 focused PlayMode verify distinct merge/move state,
drop cleanup, card upgrade/sale and HUD input protection. Final Mac/Metal player
passes actual transaction/render checks: 45 frames at 720p/900p/1080p, including
zoom 2.5/5/10 and live Level 1→2/max. No attributable runtime exceptions, no
scene/Inspector/asset/package changes. Evidence:
art/source/tower_merge_levels_2026-10-19_verification.json.

No new scene/Inspector references are required: existing binding observes the
Defense instance and inventory; catalog upgrade arrays default to empty for old
assets. Runtime delivery requires transaction EditMode checks, PlayMode card/drag
and D-062 rewind checks, actual Mac player frames/native action clicks, and Astra
visual review. Windows is unverified locally without its Unity build module.
Results: 76/76 EditMode; 28/28 unique PlayMode cases after two test-fixture
corrections; real pre-wave retry covers upgrade/merge/sale; native Upgrade/Sell
clicks pass. Final Mac build and 30 frames at720p/900p/1080p pass; Astra final
visual PASS. Evidence: art/source/building_actions_2026-10-19_verification.json.
Existing PNG/prefab/scene/package/project-settings surfaces remain unchanged.
Windows qualification and future building systems remain open.
