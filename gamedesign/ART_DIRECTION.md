# Art Direction

## D-085 approved progression / D-086 level-up revision, 2026-10-19

Keep the existing fullscreen O shell and approved Lunar Porcelain HUDv4, with
original heroC. Proposed page: hero/level/XP/points left, four Blink rank rows
center, three personal-passive cards right. Maintain ten HUD-cell capacity,
separate nav ribbon and all dock geometry; one Blink slot, tiny portrait LVL,
warm-orange numeric XP. HP/MP remain explicitly inactive0/0.

Astra selects one silver-crescent/cyan-directed-streak glyph for Blink at both
scales. Cyan/silver effects are local and brief; amber XP text is restrained. Owner supersedes the old level-up sparks with
a divine light imprint: unusual symmetrical polygonal shield-seal, sharp varied
facets and broad lateral ornament. The centre is crisp, outer effects are
blurred feathered ENERGY WAVES rather than hard lines. Text contains only the
new-level heading, numeral and awarded points; no O/«Развитие» prompt. Show this revised effect
in the approved polygon_v3 ImageGen concept. The owner explicitly approved
implementation after shape/blur/text revisions. Sound direction:
the accepted ethereal female three-layer choir and echo. The owner postponed
hammer work after rejecting the impact attempts: current runtime is the reward
voice/echo only. The owner selected Elden Ring Player Level Up (the third
supplied link) as the specific target sound direction. Original v5 was rejected
as entirely unlike it; technical PCM checks were not an auditory comparison.
No rejected candidate or imported/sampled reference-game bytes are in runtime. Valid landing ghost/ellipse and red blocked
ellipse+X distinguish target states without relying only on color. No damage
shockwave, large beam or full-screen white fracture. These are not long-range
teleport gates or the deferred D-084 road runes.

Three final built-in ImageGen images: hero_progression_fullscreen_v1.png,
hero_progression_hud_v2.png and hero_progression_fx_v2.png under
art/concepts/hero_progression_2026-10-19/. Full prompts/provenance/hashes:
art/source/hero_progression_2026-10-19_generation.json. XP/point/cooldown numbers
are explicitly illustrative. Owner approved the three concepts except the old level-up panel. Astra owns
visual implementation of all approved parts, including the polygon_v3 level-up. Numbers are initial tuning, not fixed balance.

## D-084 deferred landscape and invasion-road revision, 2026-10-19

Owner direction for a distant pass AFTER necessary gameplay functionality:
rework the landscape and replace the invasion route's ordinary road treatment
with scorched, undead-corrupted earth. Etched rune markings follow the full
length of the enemy road; at night they glow red. These environmental markings
are separate from collectible runes. Preserve fixed-isometric readability of
enemies, construction and terrain boundaries when choosing the glow.

No current asset replacement, route-geometry change or immediate art production.
Exact landscape treatment, rune motif and glow strength require later ImageGen
sketches, owner approval, then GPT-6 Astra implementation. Sequence lives in
DEVELOPMENT_PLAN.md; this section records visual intent only.

## D-083 first-frame presentation repair, 2026-10-19

No new appearance or art: preserve the approved world/HUD while preventing
legacy graybox sprites/debug ground and unbound HUD from reaching the first
render. The owner explicitly keeps direct prototype startup without menu or
interactive loading. Astra owns existing TacticalPresentation lifecycle only;
actual first-render frames are required for qualification, without warm-up skips.
Final Astra visual PASS: actual first/eighth frames at720p/900p/1080p already
contain the finished world/hero/minimap/HUD, without legacy graphics, duplicate
UI or new overlaps. Evidence: art/source/direct_start_2026-10-19_verification.json.

## D-082 smaller silver LVL tags and generated upgrade icon, 2026-10-19

Owner rejects the large D-081 Level tags and asks for shorter LVL, smaller text/
padding and reflective silver. Current Astra revision: 48×17 UI units, font 11
(minimum 9px at 720p), 4-unit gap above silhouette; bright bands/white highlight/
dark lower edge, eight fills with cached live-level labels. No tier classes.

The owner explicitly approved the original ImageGen silver/cyan chevron:
«Да, внедряй». Source and full prompt: art/source/upgrade_icon_2026-10-19_generation.json.
Runtime consumes the unchanged PNG as a max256 sprite, inside the existing upgrade
medallion; layout/availability remain v4. No tracing or third-party source.

Measured HUD costs also justify an appearance-preserving bounded cache for the
existing bevels. Pixel colors retain the Metal conversion; resize/destruction
releases textures. Actual normal-HUD frames must verify color/edges before closure.
Final Astra actual-frame PASS: ruby HP, deep cyan-blue MP and warm orange XP
retain their color and beveled silver edges. Approved PNG and compact LVL tags
pass card states/zoom 2.5–10 at 720p/900p/1080p; review is recorded in
art/source/tower_lvl_performance_2026-10-19_verification.json. One representative
actual frame is retained; intermediate captures remain ignored QA artifacts.

## D-081 merge feedback and silver level tags, 2026-10-19

The owner directly requests light blue for a compatible tower-on-tower merge;
green remains ordinary construction/movement and red remains invalid. Finished
towers carry compact gray/silver metal tags with Level 1, Level 2, etc. No tier
classification or per-level tower artwork. Reuse the existing native HUD palette
and code geometry; tags remain readable at fixed screen size above the actual
visible silhouette through zoom, without capturing input or drawing over modals.
Astra owns this concrete visual correction to approved v4 and actual-frame review.
Final Astra visual PASS: blue/green/red remain distinct; silver Level 1/2/5
tags read at 720p and keep their screen size at zoom 2.5–10. Rapid, Longshot
and barricade anchors clear the HUD. No further revisions required. Evidence:
art/source/tower_merge_levels_2026-10-19_verification.json.

## D-080 construction-selection correction, 2026-10-19

Direct owner correction removes the redundant Select button from all B groups;
existing card clicks already choose blueprints. The former button space contains
the existing control hint. The approved v4 selected strip also sits above the
construction dock, preserving frame geometry and native art. No new sketch or
raster asset is needed for this requested removal and restoration of the existing
card behavior. Astra owns these two HUD partials and final actual-frame review.
Final Astra review PASS: all B groups at720p/900p/1080p and selected strip/native
move/merge/upgrade above B; no overlap or clipping, no visual revisions required.
Evidence: art/source/construction_selection_fix_2026-10-19_verification.json.

## D-078 navigation ribbon and vivid numeric bars, 2026-10-19

Current owner revision: selected-building strip above a separate narrow
full-width I/B/P/O/M ribbon above the main dock. All three touch and share
continuous Lunar Porcelain framing. Main center reserves room for potentially
ten spells, illustrated by 2×5 empty slots; equipped-skill count is unapproved.
No Move button. Reuse original hero, bow, Rapid and native ornaments.

Owner follow-up: labels must read HP, MP, XP. First runtime simplified the
approved image too far: restore adjoining native ornamental frames, larger
minimap/portrait/spell medallions, beveled status tracks and illustrated card
stats/actions. Keep the common backing and actual game-art identity.

HP is ruby red, mana cyan-blue, XP warm orange, with restrained gradients and
crisp highlights. Current/maximum numbers remain legible on full/empty track.
Concept examples are 80/100 at80%, 45/60 at75%, 350/1000 at35%, explicitly
labelled illustrative outside the HUD. These are not approved gameplay values.

Two built-in ImageGen v4 frames: hud_ribbon_normal_v4.png and
hud_ribbon_selected_v4.png in art/concepts/building_selection_2026-10-19/.
Astra reviewed both: correct stack, ten reserves, readable color/numbers,
consistent dock positions, no blocking defects. Owner explicitly approved v4
on 2026-10-19. Astra implemented the native runtime and corrected the first
simplified version: three adjoining ornamented frames, larger medallions,
HP/MP/XP bevels and card stat/action illustrations. Hero values remain an
explicit disabled 0/0 state until gameplay sources exist. No raster artwork
is imported from the approval screenshot; new bevels/glyphs are code-native.
Prompts/references/hashes/review: art/source/hud_ribbon_2026-10-19_generation.json.
Actual frames, qualification and Astra review: art/source/hud_v4_2026-10-19_verification.json.
Astra final actual-frame review passed at all three resolutions; no blockers.
Two representative player images are in art/review/hud_v4_game_2026-10-19.png
and art/review/building_card_v4_game_2026-10-19.png; remaining QA stays in Artifacts.

## D-077 historical unified HUD, revised by D-078, 2026-10-19

Owner replaces the compact/floating D-076 candidates with a single full-width
bottom dock: minimap, hero/weapon, I/B/P/O/M, five existing ability reserves
and status placeholders share one continuous porcelain backing/outer frame.
The selected-building strip spans the same width immediately above and touches
the lower dock; no world gap or detached navigation. Remove the Move button;
dragging the world building handles movement/compatible merging.

Astra supplied the new brief and reviewed two built-in ImageGen v3 states:
hud_unified_normal_v3.png and hud_unified_selected_v3.png in
art/concepts/building_selection_2026-10-19/. Both are suitable to show, with no
blocking visual defect. Selected stack is taller than the brief's guide and
needs owner judgment on world coverage; it is not an approved pixel layout.
Keep the Lunar Porcelain palette/current identity, use original assets in
implementation. No native UI, gameplay or data is changed by these pictures.

Source/prompts/hashes/review: art/source/unified_hud_2026-10-19_generation.json.
Owner approval is pending; Astra implementation follows that approval.

## D-076 selection-card ImageGen candidates, 2026-10-19

Historical alternatives, sent back for D-077 revision; not the active layout.

Two final in-context layout candidates and a content/state board are saved in
art/concepts/building_selection_2026-10-19/. They follow Astra's brief and use
the existing actual Lunar Porcelain HUD as the primary reference. A is a low
horizontal card above the left navigation; B is a compact right card. Both
are proposals awaiting owner approval, not approved runtime appearances.

The board illustrates the shared chrome for tower/workshop/forge/sawmill/
throne and disabled-upgrade state. Existing Rapid and soul/dome identity
references are preserved as direction; final native thumbnails should use
original project art. No palace replacement or second world building is
authorized by discarded generation attempts.

Record: art/source/building_selection_2026-10-19_generation.json, including
full prompts, references, corrections and final file hashes. Only three final
requested PNGs are retained in the repository. Generated screenshot pixels
are illustrative, not evidence of exact HUD geometry or live behavior.
Owner chooses layout/content before Astra implements; no Unity changes now.

## D-075 owner rune and building-card brief, 2026-10-19

Owner requests stone fantasy shield-like rune shapes. The quality sequence
is F/E/D/C/B/A/S/SSS, with no SS. F is plain grey; intermediate qualities
include red, without an assigned rank yet; S is gold and SSS is exceptionally
rich gold. Other colors and exact SSS treatment remain open. This records
the owner's brief, not a new approved asset or generated reference.

Astra's unapproved SSS proposal retains stone/gold with a three-ray crown in
the silhouette and light gold inlay, making the top quality distinct beyond
brightness. Owner approval of an ImageGen sheet is still needed before this
proposal becomes runtime art. Do not infer approval from the wording here.

Owner also requests a compact selected-building card above the HUD with
relevant stats, available upgrades and a link to its full-screen section.
Astra proposes the existing lunar-porcelain palette and a horizontal card
to the right of the minimap, above the HUD/navigation. Functional contents,
input and proposed page destinations are in features/PLAYER_INTERFACE.md.
This is a design brief only. ImageGen approval precedes Astra's visual Unity
implementation; preserve existing sprites, camera and native UI stack.

## D-062 throne damage and defeat rewind, 2026-10-19

Owner approved the ImageGen storyboard with a transient-feedback correction:
white radial light fissures originate at the visible dome's soul on each actual
HP loss, fade away completely, and grow stronger on subsequent lower-HP hits.
Offscreen damage gives only a small short render-view shake. Opaque fullscreen
pages count as hiding the dome. Logical camera transform/follow/picking stay
stable. No permanent fracture overlay or forced camera jump.

Fatal damage grows bright angular screen fragments into solid white wherever
the camera is; then the existing lunar palette and native ornaments present
«Ещё одна попытка» and «Повторить волну». GPT-6 Astra owns appearance and actual
frame review. Concept is illustrative only; original throne/enemies stay in
runtime. Procedural shader/UI reuse existing materials rather than shipping
the storyboard as a screen texture. Timings live in Stage3DefeatSettings.asset.
Source/approval/prompt: art/source/defeat_rewind_2026-10-19_generation.json.

## D-072 overhead harvest marker, 2026-10-18

Reuse the approved sawmill/quarry native SVG tool silhouettes (tool-only mask)
and existing lunar HUD pearl/plum/turquoise palette. One axe or pickaxe appears
above the actual E target's sprite, independent of foreground tree alpha.
Keep a constant screen footprint across world zoom; no labels or new controls.
Implementation and actual-frame review belong to GPT-6 Astra. These are native
derivatives of existing project-authored icons, with no new raster source.
GPT-6 Astra accepted all 15 final Mac player frames at zoom2.5/5/10. The top
anchor uses source alpha extents, avoiding FullRect transparent padding; source
pixels, pivots and sprite geometry remain unchanged. Verification/provenance:
art/source/resource_interaction_2026-10-18_verification.json.

## D-070 sampling correction and approved D-071 implementation, 2026-10-18

D-070 keeps original artwork, fixed camera and style. PC Forward+ uses4xMSAA
at renderScale1 with no added post-AA blur. Dedicated landmark/production
images gain mipmaps/trilinear sampling; crowded hero animation atlases keep
no-mips to avoid pose bleed. Production pads retain one outer contour and
existing glyph fit; their inner dashed layout guide is removed by owner request.
Image sampling improves reduction/edges, not the source detail at extreme zoom.

D-071 requests review sheets for stone/iron sizes and depletion, small conifer
groups, axe/pickaxe equipped by the existing hero C, and natural hero/enemy
walk/harvest keyposes. Preserve current character identity and cartoon materials.
Static pose sheets are concepts, not a proven smooth animation atlas. They do
not by themselves prove production readiness. The owner subsequently approved
all six sheets and authorized Astra's visual implementation. Tree layouts must
be natural random groups of 1–8 separate ordinary trees, without rows/grids;
the owner waived another tree sketch. Preserve approved tree identity. Current generation/provenance:
art/source/manual_resource_motion_2026-10-18_generation.json.

The implemented D-071 derivative preserves ordinary tree scale and uses native
size/depletion sprites, six harvest poses per direction and eight walk phases.
Left poses mirror the right through the existing URP sprite-instance pathway;
the shared sprite shader now honors renderer flip/tint/alpha. Foreground ordinary
trees fade locally when covering the hero. Back/front harvest rows were repaired
after actual player contact review; sole pivots and mineral footprint centers
match the gameplay stock-stage radii. Original PNG pixels are retained, with
Unity-native sprite geometry only. Production prompts/source hashes:
art/source/manual_resources_motion_runtime_2026-10-18_generation.json.

## Full-screen interface — D-069, implemented direction, 2026-10-18

The owner approved the revised full-screen Inventory/Research/Hero/Map concepts
and compact HUD buttons. Inventory V2 is the shared shell: opaque pearl ceramic,
restrained plum header, thin silver separators, turquoise selection and Cyrillic
sans-serif. The four pages fill the screen; content and icons keep proportions.
Use the same header/wallet/footer across them, without a Construction tab.
Construction remains the current in-world ribbon. The world's menu row contains
only bag I, hammer B, book P, hero O and map M, without word labels.
The selected PNGs guide layout; illustrative hero/bow/seal rendering does not
replace approved world artwork. Keep research local and development unavailable.
Sources/prompts: art/source/fullscreen_interface_2026-10-18_generation.json.
GPT-6 Astra implemented the layout and reviews actual player frames at
1280x720, 1600x900 and 1920x1080. Current Mac results and Windows limitation:
art/source/fullscreen_interface_2026-10-18_verification.json. D-068's Windows
checks remain historical. Original runtime artwork is reused unchanged.

## Player interface — D-066, approved implementation, 2026-10-18

D-068 correction: the owner rejected the first runtime frame proportions and
serif typography. Keep C materials, but use Cyrillic sans-serif, proportional
jewels/corners/icons, decoration-safe content padding, screen margins and clear
interactive states. The earlier runtime visual PASS missed these defects.
Use separate stretchable connectors and fixed-aspect decorative bands.

The owner rejected the old charcoal panels/white outlines and authorized a
complete fantasy redesign, including minimap and large map. They selected
ImageGen direction C, «Лунный фарфор»: pearl ceramic, muted plum enamel,
silver edges, restrained turquoise accents, curved crescent/petal silhouettes.
GPT-6 Astra owns layout and visual implementation after concrete concept review.
Do not reinterpret this as another recolor of generic rectangular panels.
Preserve readable content and compact HUD scale; reduce oversized empty surfaces.
The owner approved implementation of the window sheets and both updated
fullwidth HUD sketches. The normal lower console contains a narrow vertical
weapon-stat block, future abilities and red/blue/orange status reserves. B
replaces it with a nonmodal construction ribbon. The local minimap and all
five windows use actual world/catalog data. HP/mana/XP/ability mechanics remain
unimplemented. See features/PLAYER_INTERFACE.md and the original PNGs in
art/concepts/interface_2026-10-18/. Generated window thumbnails are illustrative;
existing approved building/teleport artwork keeps its identity in implementation.
Runtime component atlas sources and full prompts are recorded in
art/source/interface_runtime_2026-10-18_generation.json. Original pixels are
preserved; Unity-native sprite rectangles, nine-slice borders and circular
sampling supply the scalable UI. Font rendering uses installed Windows fonts,
with no copied font files. Daylight follows D-067 clock time, including an
uncleared horde after 08:00; spawning/rest remain governed by run phase.

Hero C idle/bow correction preserves the accepted character: four planted-foot
neutral directional sprites plus restrained breathing, separate physically
correct side bows with a straight undrawn string, and grip registration for
walking/standing. No newly drawn multi-frame idle or string-draw attack cycle.
Sources and provenance: art/source/hero_motion_2026-10-18_generation.json.

Status: target look clarified by the player, 2026-09-25. Current Stage 2
sprites and markers are temporary gameplay placeholders.

## Target

Owner workflow, 2026-10-18: all visual decisions, appearance and visual Unity
implementation belong to GPT-6 Astra after the owner approves ImageGen sketches.
The main agent generates the sketches; an image proposal alone is not approval.
For D-064 the owner approved wooden barricade A, explicitly facing the camera
like current towers. Its original approved sheet and transparent front sprite
are in art/concepts/wooden_barricade_2026-10-18/. Astra uses the existing billboard,
one-cell fit and matched preview/built front-foot registration. No new camera
angle is selected. D-065 below selects the hero appearance.
See features/WOODEN_BARRICADES.md.

## Hero C, dwelling B and night — D-065, 2026-10-18

Owner approved hero C (blue travel clothing, tied dark hair, short beard),
dwelling B (cream plaster/timber/terracotta cottage, front facing the camera)
and the cool-blue readable night comparison. Sources are in
art/concepts/day_night_and_home_2026-10-18/; full prompts, approvals, rights and
hashes are in art/source/day_night_and_home_2026-10-18_generation.json.

C is a versatile developing fantasy hero. The starter bow is separate equipment,
not a class identity or a permanent part of the body sprite. Do not introduce
unapproved magic effects, skill UI or weapon progression. Astra imports the
unchanged transparent body atlas, separate equipment and approved house, fits
the existing camera/feet and supplies readable motion. Keep the house's actual
door approach aligned with its collision and matching preview/built form.

Night changes world lighting/color while preserving road/enemy silhouettes,
HUD contrast and localized portal/objective/window glow. The concept is visual
direction, not implementation evidence. Astra verifies actual day/night/dawn
frames; no new camera angle or environment replacement is approved.

Мягкая мультяшная fantasy-графика в изометрическом виде с читаемыми объёмами;
ориентир по настроению — Dota 2 и её пользовательские TD-карты. Не копировать
их модели, текстуры, персонажей или UI. Pixel/near-pixel больше не является
целевым стилем. Камера остаётся фиксированной и ортографической для читаемости
маршрута и размещения; поворот камеры не нужен для первого играбельного билда.

По D-027 игрок выбрал полноценные runtime 3D-объекты в Unity на общей
поверхности и принял текущие башню и камеру в играбельной сцене. Первый
играбельный забег и малый Blender-пилот уже проверены. Спрайтовая версия
снова используется для выбранных объектов по D-039–D-041. Вид
каждой следующей модели и бюджет массового производства согласуются отдельно.
Не закладывать pixel art как обязательство в новые ассеты. Правила сохраняют
плоские координаты, а камера, размещение и визуалы уже адаптированы вместе.

## Research workshop and ground teleport circles — D-063, 2026-10-17

The owner accepted research_workshop_v1's appearance: warm timber/pale stone/
terracotta architecture, books, plans and measuring instruments. Preserve that
approved concept. Its unchanged PNG is imported as the runtime workshop sprite,
with a visible-bottom pivot, front-foot registration and separate ground collision.
The player now places it through the Research build tab; preview and completed
building share that exact registration. It is not preplaced on the base.

The owner rejected teleport_pair_v1's upright arches and explicitly requested
teleportation circles drawn directly on the ground. All rune/line geometry is
horizontal and flush with the surface, with an open center for the hero.
Both endpoints use the SAME appearance. Large circles and symmetric round nodes
remain the visual structure. The owner found v3's circle-only treatment too bare
and supplied a decorative magic-seal reference: add geometric connections, a
few bold invented glyphs and arc accents while retaining clear large shapes
at ordinary zoom. Tiny inscriptions and excessive bloom remain unsuitable.
No upright portal membrane, arch, pillar or platform.
Approved for the game: teleport_circles_v4_seal, one original ornamental master pattern.
The unchanged source is projected by an XY ground mesh that conforms to grass
height. The Ground Teleport Seal shader removes the source's opaque dark background
and supplies a thin contact outline; no solid square is used beneath the markings.
Do not make separate fresh/weathered variants for base and found locations.

Preserve the fixed orthographic view, upright supports, readable ground contact
and silhouette/footprint fit. Runtime source copies/materials and footprint/scale
data are in Assets/Stage3/WorldTravel/. The base seal appears after research;
the remote seal uses exactly the same material. Both are static markings.
Actual gameplay renders at normal/close/overview zoom are verification evidence
in art/review/. The city and NPCs are future context;
the owner explicitly said not to create the city now.

References: `../art/concepts/research_and_travel_2026-10-17/`.
Full prompts, original sources, rights and hashes:
`../art/source/research_and_travel_2026-10-17_generation.json`.
User composition reference: `../art/references/teleport_seal_user_reference_2026-10-17.png`.
External source/license is not supplied: inspiration only, never runtime-ready
or traced artwork. Generated v4 uses original geometry and invented symbols.

## Throne destruction and rewind — D-062, 2026-10-17

The defended object remains the blue throne/soul objective; the red portal
remains the enemy entrance. For the approved defeat transition, rays emerge
from the destroyed throne and intensify until the entire screen is white by
the end of the destruction. A readable «Повторить» action follows. Keep the
fixed tactical view and accepted landmark identity. The animation duration and
rendering method are not chosen yet. This is an approved target, not an
implemented effect; see `features/STAGE2_RUN_STATE.md` for retry semantics.

## Resource landmarks — current D-057 correction, 2026-10-16

The owner rejected the D-056/Astra smooth terrain core with edge sprites.
Match the original D-053 images in composition, volume, materials and detail:
pale angular quarry benches, a dark rust-veined descending iron excavation,
and overlapping coherent conifer groups with clearings. Authored fixed compound
artwork is allowed; safe placement may vary by seed. Procedural shape variety
must not dilute the reference. Coherent layered bitmaps are valid under the
fixed camera; never leave the rejected generic mesh underneath them. Production
buildings are separate assets; reserve one typed local point per landmark.
Scale the large objects and spatial contracts together by about 0.75; original
small E-gathering nodes are unchanged. The following D-054/D-056 sections are
historical implementation notes, superseded by this direction.

The owner accepted the D-057 Game appearance on 2026-10-16. D-058 now authorizes
the three separate buildable production buildings from those same original
references: timber sawmill, pale-stone quarry workshop and iron headframe/station.
Their full source prompts and rights are in
`art/source/resource_production_buildings_2026-10-16_generation.json`. Preserve
landmark pixels/scale and the fixed view; place each detailed transparent building
on its existing pad. The bottom menu reuses the current HUD with simple category
tabs/cards, prices and output intervals; it does not select a new UI stack.

## Tower placement — D-061, 2026-10-17

The owner requested cell divisions on the gray zone during tower selection and
a 45-degree turn of both current towers. Use actual ground cell bounds with
restrained contrasting lines, only while tower construction is active. Depicted
Rapid/Longshot bodies face along the grid axes; never lean the billboard or rotate
the camera to approximate object yaw. Preview and built forms share the same
edited sprite set. Keep original V2 art and Longshot spoon/stone. Source, complete
prompts and rights: `art/source/tower_grid_facing_2026-10-17.json`.

Owner follow-up: register the visible base at the front/lower edge of its cell,
with a small inset; transparent PNG padding must not lift it away from that edge.
Uniformly fit the full body silhouette, including roof eaves, inside the cell's
width so immediately adjacent towers have a visible gap. The shared preview and
built artwork, contact shadow and articulated parts must agree. Preserve upright
proportions and the loaded stone's size when it becomes a flying projectile.

## Construction readability — D-059, 2026-10-16

Current acceptance, D-060 (2026-10-17): the owner approved Astra's corrected
`approval_sheet_astra_v2` and requested runtime integration. Use the existing
Astra glyph geometry with transparent cutouts, thin complete neutral pad bounds,
a subtle dashed inner safe border and approved 12% icon margins. All elements
share the actual ground-plane camera projection. Implement as vector mesh
geometry; the approved metal bar is a pictogram only. Runtime integration is
complete and all three actual pad views were independently inspected. Provenance:
`art/source/placement_icons_runtime_2026-10-17.json`. This supersedes the
pending-approval status in the historical review notes below.

Free resource-building sites carry persistent neutral, outlined ground stamps:
axe and log, pick and fractured stone, or pick and metal bar. They identify the exact
locked point before the player selects construction. Hide an occupied stamp;
restore free ones on reset/regeneration. These are original project vector
meshes in Stage2PlacementVisuals, not imported or traced reference artwork.

During construction, show the actual tower/building silhouette with transparent
green tint for a valid position and red tint for a rejected position, together
with a ground footprint outline. Do not add yellow/orange placement states.
Preview materials are separate from built-object materials. Unaffordable build
cards are dimmed and disabled, with each price readable and a hover explanation.
Preserve D-057/D-058 artwork, scale, camera and pad positions.

2026-10-17 owner feedback: the current pad glyphs are hard to recognize; their
appearance is not accepted. Prepare a minimal vector replacement for approval
before runtime integration. The first original SVG proposal is under
`art/concepts/placement_icons_2026-10-17/`: axe/log, pick/stone and pick/ingot.
The sheet includes 32/48/64 px views and a schematic ground projection. The
ingot is a proposed metal pictogram, not a new resource or mechanic. These are
unapproved concepts; the ground row is not a Unity screenshot. The owner then
rejected approval_sheet_v1: unrealistic bent forms and incorrect proportions.
The owner explicitly delegated the next revision to GPT-6 Astra. Require
recognizable tool construction and credible proportions, with restrained vector
detail; v1 is rejected history, not the baseline to reproduce.
The Astra glyph sheet is `art/concepts/placement_icons_2026-10-17/approval_sheet_astra_v1.png`:
straight shafts, a constructed axe/pick head, a fractured irregular rock and a
regular metal bar. Original vector sources/rights and hashes are recorded in
`art/source/placement_icons_astra_2026-10-17.json`. Parent inspected the final
render independently; at that review step, owner approval and integration were pending.

The owner then flagged the arbitrary ground-diagram tilt and missing safe
areas. Current review is `approval_sheet_astra_v2.png` in the same directory.
Keep v1 glyph geometry; project the real scaled point clearance, proposed inset
and glyph with the same actual orthographic XY transform (no decorative diamond
or independent skew). Full solid bounds mark the reserved pad; a distinct dashed
inner rectangle proposes 12% icon-layout insets per edge. This is a review-only
margin, not a new collision or construction rule. Source measurements and
checks: `art/source/placement_icons_astra_v2_2026-10-17.json`.

## Procedural resource sprite kit — D-054, 2026-10-16

D-056 revision, owner-selected 2026-10-16: a procedural 3D geological base now
provides actual stone terraces, mine depths, walls and connected stair tiles.
Painted tree/rock accents remain sprites. Reuse original project stone pigment
and the exact grass pigment/UV phase; no source PNG edits. Ground aprons follow
the real grass triangles. Trees have scale-aware crown gaps, varied ages and
an irregular opening. Local partial obstruction reveal keeps the rest of each
object visible with eased transitions and an exit hold. This supersedes the
flat illustrated excavation and dense sprite-stack representation below.

The owner authorized procedural assembly and selected initial 200–300-cell
footprints. Runtime uses original separate sprite parts, not the full D-053
pictures. Assets/Stage3/ResourceDistricts/ holds three 1536x1024 transparent
six-part sheets: Trees, RockModules and MineModules. Text-only built-in imagegen
produced distinct conifer silhouettes, pale/dark rock ledges, worked shaft rim,
stairs, shoring and a small rim hoist. No third-party input images or models.
Exact prompts, original generated paths, runtime hashes and rights:
art/source/procedural_resource_districts_2026-10-16_generation.json.

The generator varies connected district shapes, object positions, module
selection, scale/reflection and restrained tint under the existing fixed camera.
Mine depth is painted through procedural ground shading/ledges and separate
sprite pieces on the current gameplay plane. Overlapping canopies fade together to preserve
hero readability through the district-only sprite material, including real
per-instance tint/flip support. Grass/flowers omit rock/excavation footprints, including when
a new seed is generated. Appearance and density await owner's live judgement;
production buildings, renewal and zone harvesting still need their own brief.
By D-055, collision follows ground footprints: separate scaled trunk circles
leave forest gaps open; stone/iron footprints and protruding rock bases are
solid. Sprite alpha, canopy size and camera-facing offsets do not define walls.

## Pending resource district concepts — D-053, 2026-10-16

Three original preview-only illustrations under
`art/concepts/resource_districts_2026-10-16/` explore the owner's next idea:
- `forest_sawmill.png`: dominant mature conifer forest, small edge sawmill,
  worked clearing and young conifer nursery/regrowth;
- `stone_quarry.png`: broad pale granite mass, open stepped extraction benches,
  cut blocks and a small workshop/derrick;
- `iron_mine.png`: dark rust-veined rock, a clearly descending stone-hewn shaft,
  visible stairs and a compact timber hoist/extraction station.

All use consistent painted cartoon materials and elevated orthographic framing.
They are environmental concept references, not final production sprites: exact
map footprint, modular pieces, occlusion/collision and building placement must
be resolved when implementation is authorized. The mine depicts depth visually;
it does not introduce vertical hero movement or underground gameplay.
Art was generated by the built-in imagegen tool using text descriptions after
inspecting the project's own cartoon B board and approved resource previews.
No third-party image input. Exact prompts, original paths and SHA256 hashes:
`art/source/resource_districts_2026-10-16_generation.json`.
Appearance is unapproved; no Unity asset, renderer, scene or gameplay changed.

## Current cartoon revision — D-042

The first D-041 tower/terrain scene was rejected by the owner. The target is
specifically the original B board's lower gameplay strip, including its earth
pockets, short pointed turf, weeds between chipped paving, small flowers,
shrubs and broken masonry at the road. These decorative props are now authorized;
they must not obscure the objective/resources or consume buildable cells.
The fixed camera and map geometry remain unchanged.

`Assets/Stage3/CartoonWorldPilot/*V2.png` are original built-in imagegen artwork.
Exact prompts and byte-identical source/runtime hashes are recorded in
`art/source/astra_cartoon_rework_prompts.json` and
`art/source/astra_cartoon_rework_provenance.json`. Earlier art is retained.
Roadside props are shared transparent sprites without collision, placed only
on grass or the nonwalkable visual border with objective/resource clearances.
The old bright triangle fringe is removed. Paving UVs compensate for the fixed
camera so stone courses agree more closely with the drawn tower foundations.

Rapid stays static, with contact marks and sparse sprouts at its stone feet.
Longshot uses one fixed masonry/support image and a separate spoon/stone;
only the spoon articulates during the throw. Bowl dimensions remain constant,
loaded stone disappears at the rule-clock release and returns after recovery.
It is a 2D articulated drawing: no continuous 3D support rotation is claimed.
Approved portal/soul art is preserved. Runtime acceptance remains open.

## Stage 3 — Shared Tactical View (D-019)

2026-10-15, D-048: the owner requested previews for cartoon resource sprites,
retaining D-037/D-038's approved conifer, pale granite and rust-seamed iron
outcrop. Match B's painted volumes, dark outlines and fixed tactical view.
Original transparent ImageGen previews go to
`art/concepts/resource_sprite_review_2026-10-15/`; prompts/provenance:
`art/source/resource_sprite_preview_2026-10-15_*`. The owner approved all three
and requested runtime use. Exact PNG copies and new sprite prefabs are under
`Assets/Stage3/ResourceSpritePilot/`; keep the original 3D sources separately.
Use the fixed camera, grounded contact pivot and existing unlit sprite treatment.
Terrain and resource rules remain existing behavior. Integration passed Mac
PlayMode 46/46 and live review of all three resources. Actual Game-camera frames:
`art/review/resource_sprite_*2026-10-15*`; exact runtime/source hashes:
`art/source/resource_sprites_2026-10-15_provenance.json`. Owner approval of the
preview art and feedback on the live scale/readability are separate observations.

2026-10-15, D-039: the owner authorized a direct cartoon-sprite walk trial
using skeleton B from `art/concepts/sprite_style_review_2026-10-15/`.
`Assets/Stage3/SkeletonSpritePilot/SkeletonWalk.png` is original built-in
imagegen RGBA art, referenced only from the project's original concept board.
Exact generation brief: `art/source/skeleton_walk_cartoon_prompt.txt`;
provenance: `art/source/skeleton_walk_cartoon_provenance.json`. No third-party
image/mesh is used. A 6x4 sheet supplies one unit's right/away/left/toward walk;
feet/torso registration is stored in Sprite pivots without repainting the PNG.
Static Game-camera checks remain in `art/review/skeleton_sprite_portal_*.png`;
the animated QA recording was removed at the owner's request.
This narrow mixed 2D/3D trial awaits owner review; it does not approve a final
pipeline for the towers, landscape or remaining actors.

2026-10-15, D-040: the owner extended the direct cartoon-sprite trial to the
B4 portal and the selected soul-sphere objective. Original project concepts
`entrance_portal_option_b4_dark_red_ring.png` and
`throne_soul_sphere_ice_dome.png`, plus cartoon style board B, are the only
image inputs. Original RGBA outputs are `PortalCartoon.png` and
`SoulObjectiveCartoon.png` under `Assets/Stage3/LandmarkSpritePilot/`.
Exact prompts and source/rights are in `art/source/portal_cartoon_sprite_prompt.txt`,
`soul_objective_cartoon_sprite_prompt.txt` and `landmark_sprites_provenance.json`.
The PNGs are copied unchanged; Unity imports scale/pivots and animates only the
vortex interior and blue energy highlights. New models/Blender conversion are
not part of this delivery. The previous sources/prefabs are retained; live
appearance was approved by the owner; the final production pipeline remains open.

2026-10-15, D-041: the same cartoon B board now guides both tower sprites and
the terrain. Original built-in imagegen PNGs are in `Assets/Stage3/CartoonWorldPilot/`:
`RapidCartoon.png`, `LongshotFire.png`, `CartoonGrass.png`, `CartoonPaving.png`.
Exact prompts: `art/source/cartoon_map_tower_prompts.json`; source/rights and
file hashes: `art/source/cartoon_map_tower_provenance.json`. The only image
input was this project's original generated board. Source PNGs are unchanged;
Unity stores sprite rectangles/pivots and frame selection separately.
Rapid is stationary; Longshot has four drawn aiming directions and firing
poses. The sheet's detached-stone pose is excluded from playback because the
existing projectile view supplies the moving stone. No copied illustration
patch, new heraldry or third-party runtime asset is used. Unlit ground colors,
diagonal slab courses and an alpha-faded grass verge bring the material style
closer to the reference while keeping the existing map geometry/camera.
Tower/terrain runtime appearance still needs the owner's assessment.

Новые примеры игрока от 2026-09-25 задают композицию: достаточно крутой взгляд
сверху, квадратные клетки на поверхности, вертикальные башни с видимыми
основаниями, контактные тени и промежутки между объектами. Камера общая для
земли, башен и акторов. Отдельный поворот меша для имитации своей камеры снят.
Текущий пилот использует фиксированную ортографическую камеру с наклоном;
колесо мыши плавно меняет масштаб до 2× в обе стороны от стартового вида;
WASD не меняет масштаб, герой остаётся в центре. Вокруг игровых клеток есть
непроходимая
видимая рамка земли, поэтому при отдалении кадр остаётся заполнен;
любой ненулевой шаг учитывается независимо от единиц устройства, а его сила
настраивается игроком ползунком Inspector (1,05×–3×,
начально 1,4×) и сохраняется между запусками;
свободное вращение камеры не добавляется. Кровля не должна превращать
соседние клетки в непросматриваемую стену; проверять стройку рядом с башней,
героя, волну и рестарт в реальном игровом кадре.

Runtime 3D — активный способ проверки этого вида. Остальные акторы пока
представлены простыми объёмами. Поверхность получает лёгкую фактуру и линии
клеток серой зоны; это техническое оформление пилота, не утверждённый биом.
Референсы Dota/TD описывают ракурс и масштаб; их карты, персонажи и модели
не копируются. Финальные новые объекты по-прежнему сначала согласуются по
картинке с игроком.

## Historical Placeholder Boundary

До первого играбельного билда использовать существующие блоки, спрайты и
простые оригинальные маркеры. Новая графика нужна только тогда, когда игрок
не может найти ресурс, отличить башни, увидеть маршрут/врага или прочитать
состояние забега. Звук, финальные модели, анимации и законченный UI идут после
проверки цикла.

## Readability Rules

- Трон, вход врагов и ресурсные источники различимы в игровом масштабе.
- Серая зона вторжения и зелёная ресурсная зона различаются с первого взгляда;
  цвет не должен быть единственной подсказкой для интерактивных объектов.
  Preview боевой башни ясно показывает запрет на зелёной зоне.
- Портал и трон читаются как единый оборонительный угол; три направления
  исследования и добычи различимы уже на временной карте.
- Дерево, камень и металл различаются силуэтом; башни — силуэтом и действием.
- Preview показывает допустимость и цену размещения; враги и удары не теряются
  на фоне окружения.
- Высокие объекты и декор не закрывают героя, врагов и важные клетки.
- Одна карта и один согласованный набор форм важнее разнообразия декора.

## References And Rights

The player's `Без имени.png` is a layout mockup for D-034: green land fills
most of the 100×100 map, while the gray invasion zone runs diagonally from
the upper right corner toward the lower left. The red and blue circles mark
portal and objective positions. It is used for composition only, never as a
runtime texture or traced visual asset.

Stage 3 entrance portal: `art/concepts/entrance_portal_option_a_violet_arch.png`
and `entrance_portal_option_b_ember_ring.png` are original built-in imagegen
review concepts made for this project from text prompts, without third-party
image inputs. They compare a violet slate arch with a lower warm stone ring.
Neither is a runtime texture, mesh source to trace, or an approved model.
These first alternatives are historical; B4 is approved below.

The player chose direction B but rejected its broken-ring appearance.
`art/concepts/entrance_portal_option_b2_intact_ember_ring.png` is an original
built-in imagegen edit of the project concept, showing a continuous intact
stone ring. This revision was superseded by the darker B4 below.
The player clarified that the baseline enemy emerging from the portal is
undead and asked for a darker mood. The original imagegen review revision
`art/concepts/entrance_portal_option_b3_undead_ring.png` keeps the intact ring
and replaces the warm opening with a dark spectral vortex. It remains a concept
not approved and not a runtime asset.
The player then restored the original red interior. The latest original
imagegen review edit is `art/concepts/entrance_portal_option_b4_dark_red_ring.png`:
intact dark stone and a red vortex. The B3 spectral-color option is historical.
B4 was explicitly approved for modeling (D-033). Original runtime geometry,
baked procedural slate and analytic red vortex are authored by
`art/source/build_entrance_portal.py`, with editable source in
`art/source/entrance_portal.blend`. No third-party asset or traced texture is
used. The owner subsequently requested a 90-degree placement turn and 3x3
size matching the objective. After seeing the actual tactical screenshot,
the owner confirmed keeping the exit toward the objective, even though that
bearing made the red aperture edge-on to the fixed camera. On the subsequent
D-034 diagonal map the owner requested a 45-degree gate angle (D-035). The
upright arch and red aperture now turn within the grid-aligned 3x3 stone base;
the original B4 color and complete silhouette remain the visual source.

Stage 3 resource source options are original built-in imagegen review concepts
generated from text with the project's actual Game screenshot as a palette and
camera reference. The approved stone A concept was also used only as a style
and size reference for the wood and iron-ore images. No third-party input or
traced image is used. Stone A,
`art/concepts/stone_source_option_a_granite_boulder.png`, is a compact pale
granite boulder with small stones and is approved for modeling; stone B,
`art/concepts/stone_source_option_b_limestone_outcrop.png`, is unselected.
Wood A `art/concepts/wood_source_option_a_broadleaf.png` is unselected;
wood B `art/concepts/wood_source_option_b_conifer.png` is the approved compact
conifer. Iron ore A `art/concepts/iron_ore_source_option_a_jagged_outcrop.png`
is approved dark angular rock with rust seams; iron ore B
`art/concepts/iron_ore_source_option_b_upright_slabs.png` has three upright
ironstone slabs and is unselected. All six images remain
review concepts, not runtime textures or mesh sources to trace.
The owner rejected both resource model iterations: first the coarse faceted
models, then the smoothed and textured replacements shown in the Game view.
Neither implementation is an accepted visual baseline. Increasing polygon
counts and smoothing alone did not resolve the mismatch with the concepts.
The A/B/A selections remain approved. Broad natural rock forms, soft worn
edges, surface texture and cracks, rusty mineral seams, and separate rounded
conifer boughs are required; a faceted sphere or stacked cones do not meet the
concepts. Technical tests alone do not constitute visual acceptance.
Runtime source meshes are original procedural geometry created by
`art/source/build_resource_sources.py`, with the conifer and iron-ore geometry
in `resource_wood_model.py` and `resource_ore_model.py`, and saved as editable
`StoneResourceSource.blend`, `WoodResourceSource.blend` and
`IronOreResourceSource.blend` in `art/source/`. FBXs, project-created URP
materials and visual prefabs are under `Assets/Stage3/VisualPilot/`. Each
source has an original 2048×2048 albedo atlas. Wood uses procedural foliage,
bark, vertex tint and local occlusion. The current stone revision uses a new
original imagegen granite surface, box-projected
and baked with vertex tint and local occlusion. Its source is
`art/source/stone_granite_painted_albedo.png`; exact built-in generation brief
and provenance are in `art/source/stone_granite_painted_albedo_prompt.txt`.
The selected stone concept was a style/palette reference only. No concept
pixels were sampled or traced. Ore uses an original imagegen slate surface,
`art/source/ore_slate_painted_albedo.png`, with its exact brief/provenance in
`ore_slate_painted_albedo_prompt.txt`; exposed rusty mineral is procedural and
recessed between the dark rock masses. This texture was generated from text
without image input or third-party assets. Stone and ore also use a 2048×2048 tangent normal
map for shallow fractures and surface grain. No external meshes or third-party
textures are included. After the stone review the owner requested corrected
smoothing and explicitly authorized continuing wood/ore. Stone tint is now
continuous across smooth vertex normals. The conifer has individual closed
bough volumes and separate upper/lower normals at their perimeter, retaining
smooth surfaces without folding the upper edge normals downward. The current
mesh also orders the radial contour monotonically around each bough center:
the earlier backtracking contour formed inward narrow strips despite smooth
normals. All 22 bough shells and the crown are checked for outward winding
in the source generator. The current set still requires the owner's visual
acceptance in the actual game.
At the game's tactical camera the light boulder, tapered conifer and dark
rust-seamed ore must remain distinct. Their actual Game frames are
`art/review/resource_{stone,wood,metal}_game_{default,close}.png`.

Soul objective (D-031): `art/concepts/throne_soul_sphere_ice_dome.png` is the
selected original imagegen concept, reattached by the player to clarify its
layering. It is review material only. Runtime geometry and procedural slate
albedo are authored by `art/source/build_soul_sphere.py`; smooth ice, soul and
flowing energy use original project shaders. No third-party mesh, copied image
patch, traced texture or unlicensed runtime material is included. Preserve the
distinct stone bowl, inner sphere/flame, transparent outer dome and horizontal
external flow. The first uniform luminous-shell implementation was rejected.

Существующие изображения в `concepts_visual_scene/` — только вдохновение;
их старые подписи о pixel art или отклонённом 3D не задают новый target.
Перед включением любого стороннего ассета в игру записать источник и лицензию.

Stage 3 landscape: `art/references/landscape_player_grass_reference.png` is a
player-supplied visual reference (rights/license not specified), for inspiration
only, never a runtime texture or trace source. Original concept previews
`art/concepts/landscape_option_a_stone_paving.png` and
`art/concepts/landscape_option_b_packed_earth.png` were generated with the
built-in imagegen tool from the current gameplay screenshot and that reference.
They are review images, not Unity assets. Both retain the green resource zone
and flat gray defense zone; their gray surface and decoration density are
alternatives. The player selected B: soft bright turf and compacted gray earth.
Bushes, rocks and other raised decoration must be separate 3D props if added;
they can be omitted from the first terrain pass. The selected concept is not
a runtime texture or geometry source. The player rejected the first playable
pass as blurry and too distant from the concept. The current grass and gray
earth albedos are original imagegen results using the approved concept B as a
style/palette reference. The concept screenshot itself
is not cropped or included as runtime art. No bushes, stones or flowers are
painted into the grass texture.
The player rejected the generated bushes, which have been removed. Small
grass blades and flowers remain separate procedural 3D meshes with no
collision. The player then rejected the dark straight ledge and asked for the
grass texture and transition of concept B around every side of the gray zone.
The green surface starts 0.08 cell above gray, has small real 3D rolling relief,
and slopes over a broad irregular textured verge with separate 3D grass fringe.
This is presentation
geometry only; movement and placement stay on one plane. The
current runtime scene still awaits the player's visual review; the hero
remains the basic marker until the end of Stage 3 content.
Runtime ground images: original built-in imagegen outputs created for this
project, with concept B as an appearance reference, stored at
`Assets/Stage3/VisualPilot/LandscapeGrass.png` and
`Assets/Stage3/VisualPilot/LandscapeDefenseEarth.png`. Exact generation briefs:
`art/source/landscape_grass_prompt.txt` and
`art/source/landscape_earth_prompt.txt`. Source/rights record: project-generated
outputs; no third-party image or separate third-party asset license. The old
procedural earth generator remains a fallback outside the runtime asset path.
3D grass/flowers are generated by project code. Player reference and concept
images are review material only, not included in runtime assets.

Stage 3 Rapid: предоставленное игроком изображение деревянной вышки использовано
только как референс настроения; его источник и лицензия не установлены, поэтому
само изображение не становится игровым ассетом. Одобренный оригинальный
сгенерированный концепт — `art/concepts/rapid_watchtower_approved.png`;
Первую упрощённую модель игрок отклонил за несоответствие концепту. Ревизия 2
сохраняет его пропорции и основные детали: толстые опоры, раскосы, перила,
изогнутые деревянные рёбра и отдельные красные кровельные дощечки, железные
обоймы с заклёпками, каменные основания. Геометрия и процедурная текстура
созданы для проекта; сторонние модели и текстуры не использованы.
Источник — `art/source/rapid_watchtower.blend` и воспроизводимый скрипт рядом;
рендер фактической модели — `art/review/rapid_watchtower_r2_blender.png`.
Концепт одобрен; новая модель и общий вид ещё ожидают оценки игрока. Runtime
3D проверяется по D-019; финальный бюджет производства ещё не утверждён.

Stage 3 Longshot: оригинальный концепт каменной артиллерии с метательным
рычагом и чашей `art/concepts/longshot_stone_catapult_option_a.png` выбран
игроком по D-028. Второй показанный вариант — требюше с противовесом — не
выбран. Изображения созданы для проекта при помощи imagegen с одобренным
оригинальным концептом Rapid в качестве стилевого референса; сторонние модели
или текстуры не использованы. После отдельного разрешения игрока модель
создана в `art/source/longshot_catapult.blend`; воспроизводимый скрипт рядом,
рендер фактической геометрии — `art/review/longshot_catapult_blender.png`.
Основание с кладкой и угловыми опорами, деревянные перила и A-образная рама,
ось, железные обоймы, канат, рычаг и чаша с камнем повторяют выбранные формы.
Геометрия и текстура оригинальные. По запросу анимации D-029 runtime разделён
на основание, поворотную раму, рычаг/чашу и камень, с общим атласом/материалом.
Концепт принят; оценка готовой модели в игре остаётся за игроком.

Эффекты боя D-029: стрела, камень/осколки и следы построены оригинальным
кодом проекта; сторонние модели, текстуры и эффекты не использованы.
Попадание Longshot обозначает фактический радиус, затем быстро затухает,
чтобы сохранять видимость врагов, маршрута и клеток.

D-042 final close review: Longshot flight uses the same painted stone as the
loaded cup, including foreground ordering at release. The paved map edge gets
an irregular grass apron against the nonwalkable border; border turf uses the
same tint. The 60 Hz throw was checked with 90 rendered frames over 1.5 s;
the optional recording was removed at the owner's request.
