# Feature: Resource Production Buildings And Build Menu

## D-093 production advancement —approved and locally qualified2026-10-20

Owner approved5stages with1/2/3/4/5units per cycle and individual material+gold
upgrades. All stages are available without research in the current prototype;
future technology acquisition/research is not added by this slice. Existing
intervals5/7/10seconds, building art/footprint/pads and construction costs stay.
The existing D078 selected card shows actual level/output/interval and next
output/price; no new visual layout or ImageGen art is needed for live data.
Astra reviews native card states; main owns rules/integration/tests.

Each building owns Level; its catalog owns the four paid transitions. On
upgrade, validate active state, current ownership, maximum and whole price,
then charge once and advance that instance. Preserve ProgressSeconds unchanged;
no production tick occurs during purchase. Fixed intervals preserve fractional
progress automatically. Payment callbacks cannot reenter the transaction.
Tick uses instance OutputAmount; overflow is discarded as before. Original
construction starts1. D087 retry keeps CURRENT buildings/level/progress/spending;
full Reset/world replacement removes them. Legacy finite snapshots now carry
level as well as fractional progress, without repurchasing or income.

Authoritative trial prices: existing ProductionCatalog.asset, extended additively
without GUID/scene/package migration. Stage3ProductionUpgradeInstaller.Install
writes the four paid transitions without touching base costs/art/intervals.
Older catalogs with no transitions retain one-stage legacy behavior. No sale,
merge, relocation, auto-profession XP, offline income or skipped-time simulation.
Acceptance: real selected-card upgrade for all3types; exact price once, growing
output, preserved fraction, unfunded/max/pause/stale/reentry rejection, D087
versus Reset, unchanged collision/art and readable regular/construction HUD.

Qualification:31EditMode +35PlayMode checks passed (66unique;7new). Native Mac
build succeeded; actual mouse upgraded an individual sawmill2→3→4→5 with
exact price once and disabled max. Astra visual PASS on17native frames: three
types at1/2/5, current/next price and unfunded at900p, max above B at720/1080p.
No new attributable console errors. Windows unverified. Evidence/data migration:
art/source/production_upgrades_2026-10-20_verification.json.

## D-075 longer target: technology-gated production advancement

Владелец активировал развитие лесопилки, каменоломни и железного рудника.
Рекомендуемый путь использует существующую исследовательскую мастерскую:
получить/купить технологию → изучить → оплатить ап конкретного здания.
Город и торговец D-071 пока будущие; новый дублирующий «центр апгрейдов»
не выбран. Технология разрешает ступень, а не улучшает все здания бесплатно.

По D-093 принят предел **5 ступеней каждого генератора** с базовой первой.
Основной эффект — рост реальной добычи за активную секунду. Можно менять
выход за цикл/интервал, но проверять их совместно; работники, доставка,
склады, офлайн-доход и истощение больших районов не добавляются автоматически.
Мастерство героя, лучшая кирка/топор и исследование генератора разделены.

Начальное строительство — только дерево/камень/металл. Ап здания и
исследование могут дополнительно требовать золото. Точные цены/темп ещё
не выбраны. При новом строительстве ступень базовая, если явно не оплачено
другое; изученная технология сама по себе не выдаёт повышенный уровень.

Рекомендация: при апе сохранить долю завершённого цикла, не обнулять её
и не выдавать немедленную награду от смены интервала. Успешный ап платит
один раз, отклонённый ничего не списывает. Точка, тип, исходный footprint
и правила коллизии сохраняются до отдельно выбранной визуальной ступени.
В current Stage3 D-087 сохраняет текущее здание/ступень, дробный прогресс и
расход денег при повторе; только legacy Stage2 использует D-062 snapshot.
Пауза/исход замораживают производство по текущим правилам.

Приёмка будущего среза: технология и локальный ап различимы; растёт
измеренный выпуск, а не только надпись; нет двойной оплаты/дохода при
смене интервала, отмене и повторе; ручная добыча/коллизия остаются корректны.
Ниже — исторический реализованный D-058–D-060 прототип до D-093.

Status: D-058 and D-059 construction readability implemented and verified on Mac,
2026-10-16.
D-060, 2026-10-17: the owner approved the corrected Astra vector set and requested
runtime integration plus a commit of all accumulated changes. Approved sheet:
`art/concepts/placement_icons_2026-10-17/approval_sheet_astra_v2.{png,svg}`.
Preserve Astra v1 glyph geometry, full real pad clearance, subtle dashed inner
safe border and 12% per-edge icon margins, with one actual camera projection.
The inset is artwork layout, not collision/gameplay. Parent v1 and original
D-059 glyph appearance were rejected and remain historical evidence only.
Runtime integration is complete on Mac; final D-060 checks are recorded below.
Owner/workstream: Stage 3 Integration/Economy/UI/Art. GPT-6 Astra owns production
rules, driver, data/import, scene binding and runtime verification; parent owns
the existing HUD, menu tests, generated source artwork and canonical docs.
Dependencies: accepted D-057 landmarks/locked points, shared inventory,
Preparation/Wave run state and existing tower placement.

## D-070 correction, 2026-10-18

The owner explicitly removed the inner dashed pad safe border. Only the
outer clearance outline and existing glyph remain visible; the 12% internal
layout margins, real reservation/footprint and placement rules are unchanged.
This supersedes D-060's visible dashed border, without new icon artwork.
Production and landmark PNGs retain their original bytes. Dedicated world
images use mipmaps/trilinear sampling for zoom; original atlas gutters and
hero animation assets are not changed by this correction.

## Accepted intent

The owner accepted the D-057 landmark appearance and requested three matching
constructible generators plus a simple bottom menu. Its categories are Towers
(existing Rapid/Longshot) and Generators (sawmill, quarry, iron mine). The owner
later added a Research tab for the player-built workshop under D-063/RESEARCH.md;
the three generator entries and their prices/rates remain unchanged. The owner
explicitly delegated provisional construction prices and production rates.
Those values live in `Assets/Stage3/ResourceDistricts/Production/ProductionCatalog.asset`.
This is a narrow prototype exception to the former production deferral. It
does not authorize logistics, research, upgrades, workers, renewals or selling.

## Player flow

Choose a category and a building card in the bottom HUD. Every card shows its
price; generator cards also show artwork and the production interval. A card
that the shared inventory cannot afford is dimmed and cannot start construction;
hovering explains the missing resources. Public selection and existing tower
hotkeys use the same affordability gate. Commit still rechecks the complete price.

Free landmark pads remain visible outside build mode as neutral ground stamps:
axe/log for a sawmill, pick/fractured stone for a quarry, and pick/metal bar for a mine.
Each stamp sits on its exact locked point, has no collision or E interaction,
disappears when occupied, and returns after restart/regeneration. During matching
hover its neutral mark hides beneath the placement outline, returning on cancel
or UI hover; other free sites stay visible.

Selecting a building shows one semitransparent silhouette under the cursor,
snapped to the nearby pad for generators or the cell for towers. The actual
building silhouette and ground outline are green only when construction is Ready,
and red on rejected locations. There is no yellow/orange placement state. UI
hover hides the world ghost. Point at a valid site and left-click to build.
Escape/right-click cancels. A wrong resource,
occupied pad, insufficient resources, blocked hero or inactive run rejects the
purchase with feedback and no spend. The same menu retains tower placement and
the existing B/1/2 controls. Tower and generator previews are mutually exclusive.
Clicks and wheel events over HUD/minimap must not also affect the world.

## Construction and production contracts

- One generator per locked point, with exact resource/building type matching.
  No generator placement on arbitrary grass or on the combat road.
- Construction is allowed in unpaused Preparation/Wave. Recheck activity,
  point/type, occupancy, footprint, hero exclusion, full price and actual visual
  availability at commit. Prepare an inactive visual before charging. Success
  spends once, activates one building and adds its ground collision once.
  Cancel/rejection never creates production, collision or a charge.
- Footprints fit the reserved pad and protect the hero's ground radius.
  Buildings do not change enemy navigation or the approved landmark images,
  their 0.75 scale, map/road layout or existing tower prices.
- Each generator credits its corresponding resource directly to the shared
  inventory after its configured interval of active game time. Production works
  during Preparation/Wave independently of E and manual-node stocks. Pause and
  terminal states freeze it; no offline production or delivery simulation.
- Preserve fractional progress and account for complete intervals on a long
  frame. Inventory overflow is capped and discarded, not stored as future output.
- Restart clears buildings, progress, ghosts and owned collision, along with
  the existing inventory reset. World regeneration replaces point bindings and
  removes stale buildings without a refund. No persistence is introduced.
- Manual E gathering remains available on the original small wood/stone/ore
  nodes, and is suppressed while either construction mode is active.

## Implementation and wiring

`Stage2Production` is plain rules in the existing Economy assembly. The catalog
owns the three entries, prices/rates, sprites, widths and ground footprints.
`Stage2ProductionDriver` in Integration binds the shared run/inventory, input,
locked points, collision and visuals. `Stage2GameComposition` has an optional
serialized production catalog; old scenes without it retain their existing flow.
`Stage3ResourceDistrictWorld` binds current points/obstacles after generation.
The narrow run Reset event lets direct rule-level resets clear production too.

The existing `Stage2GameHud` owns these categories plus the D-063 Research tab;
no UI-stack/package change.
`Stage2BuildDriver.PlacementStarting` coordinates existing tower input with
generator cancellation. `Stage2ProductionDriver.PlacementStarting` also lets
workshop placement yield before generator input takes ownership. All three
construction modes are exclusive; workshop uses the existing non-tower gather block.

D-059 uses original code-authored vector ground glyphs in
`Stage2PlacementVisuals`, plus an isolated transparent PlacementPreview shader.
These marks are project-created geometry, with no third-party artwork or traced
reference. Preview materials do not alter the built sprites/materials or source
PNGs. No extra scene/Inspector binding is required for the readability pass.

D-060 replaces the earlier glyph geometry with the approved original Astra SVG
shapes as code-native mesh geometry, retaining real transparent negative spaces.
Outer clearance and inner icon safe border are neutral world-plane geometry;
uniform glyph fit preserves tool proportions. Source/rights and approved layout:
`art/source/placement_icons_astra_2026-10-17.json` and
`art/source/placement_icons_astra_v2_2026-10-17.json`.
The stdlib-only offline converter `build_runtime_mesh.py` emits
`Stage2ApprovedPadGeometry.cs`, so the runtime owns only mesh data and simple
plane geometry. Runtime-ready SVG/data hashes and source rights:
`art/source/placement_icons_runtime_2026-10-17.json`.

Use Tools > Stage 3 > Resource Districts > Import Production Buildings for the
explicit import/scene migration. The saved Stage3Rapid3DGamePilot composition
references ProductionCatalog. Preserve scene/map data and existing asset GUIDs.

Original-reference-derived transparent buildings are saved under
`art/concepts/resource_production_buildings_2026-10-16/`; source roles, full
prompts, hashes and rights are in
`art/source/resource_production_buildings_2026-10-16_generation.json`.
Runtime imports preserve PNG bytes. Building visibility uses cached alpha masks
and view depth for local eased hero reveal; foreground houses stay opaque.

## Observable acceptance

Verify real category/card selection, existing tower hotkeys, mutual cancellation,
UI click/wheel exclusion and minimap access. Rule and live-scene checks cover
one-time payment, wrong/occupied points, missing visual, hero exclusion,
production timing/overflow, pause/terminal, restart and regeneration. Retain
small-node E, tower and district movement/reveal regressions. Inspect all three
built generators and the bottom HUD, with the hero in front and behind each.
No new attributable Console errors; final evidence/handoff is recorded in
`production/WORKSTREAMS.md`. Windows target validation remains separate from
the Mac development checks.

Final Mac verification: 33/33 EditMode, 26/26 combined PlayMode and 10/10 final
focused PlayMode; after caching read-only views, 9/9 production rule checks
passed again. Native UI clicks selected a generator, bought one sawmill at its
pad, observed the exact price once and automatic wood growth, then switched to
Rapid preview and cancelled. All three HUD/front/behind views were independently
inspected. Quarry/mine foreground depth boundaries were corrected without PNG
changes. Fresh ordinary Play has Generators open, no QA grants/buildings and
Console 0 warnings/0 errors. Evidence: Artifacts/S3-RESOURCE-PRODUCTION-20261016/;
shared frames/results: art/review/resource_production_*.

D-059 acceptance adds all three free stamps at normal/near/overview zoom,
both tower silhouettes and all generators in actual green/red Game frames,
pixel-level background transmission, unaffordable UI/public/hotkey gating,
matching/wrong/occupied/blocked positions, UI hiding and stamp lifecycle.
Evidence: `Artifacts/S3-PLACEMENT-READABILITY-20261016/`.

D-059 final Mac checks: 19/19 EditMode and 21/21 PlayMode, zero skips. Actual
native clicks verified an unfunded card cannot select, a funded card starts
placement, one pad purchase spends once and hides its mark, and tower switching
and cancellation work. Console has no new attributable errors. Shared actual
frames and hashes: `art/review/placement_readability_*`.

D-060 final Mac checks: 3/3 EditMode and 6/6 PlayMode, no failures/skips. Samples
verify actual holes/solids, bounds and uniform fit; live scene checks cover mark
lifecycle, actual preview colors/transparency, input/UI, paid purchase and old E.
All three normal/close final pad frames were independently inspected. No new
scene/Inspector wiring or changes to existing data/artwork are required.
Evidence: `Artifacts/S3-PLACEMENT-ICONS-20261017/`; shared final frames and
verification: `art/review/placement_icons_*2026-10-17*`. Fresh ordinary Play had
Console 0/0; after the owner resumed playing, a system audio-device reset warning
appeared, with zero errors and no attributable runtime exception.
