# Feature: Fantasy City And First Merchant

## D117 fixed authored city — renewed correction

Owner returned D115 arrangement/structure despite local review. Implement the
approved reference relationships directly, using a fixed authored layout. No
random residential filler or shuffled streets/planting. Future cities each get
individual designed plans/styles. Whole-city safe world-origin selection can
remain; internal relative placement is fixed. Separate Astra owns this pass;
service behavior and original shop/guild art remain. Centred native overview,
civic grouping, guild, portal court and merchant require strict comparison.

D117 implemented:shared civic paving without grassbar, larger guild left,
nearby temple/portal right, foreground well, stepped housing quarters. Separate
Astra PASS on15 centred service/civic frames720/900/1080; actual entrance-to-
services path checks. This is local qualification, not owner appearance approval
or a pixel-identical illustration. Evidence:art/source/world_camps_2026-10-21_verification.json.

## D115 current city fidelity correction

Match the approved city_v4_open_temple layout/density: organic residential groups,
connected walkable streets, gardens and appropriate props instead of isolated
rows on empty grass. Existing merchant/guild sprites stay; temple portal remains
in a separate open public court, notice board small/right/decorative. Ground
footprints, access anchors and visible placement must share one layout.
D113 removes automatic offscreen city guidance from minimap; M/local icon remain.
Implemented with15 homes, planted garden masses/private borders, narrower linked
streets and three compact public courts. Original service art retained. Shared
layout supplies125 collision footprints; path checks cover entry to services/
portal at.3/.6 clearance. Native Mac720/900/1080 Astra FINAL VISUAL PASS;
verification:art/source/iteration_2026-10-21_verification.json. Both wide-open
camp v2 concepts approved; runtime camp scenery is the next separate step.


D112: город отмечен с начала мира на M и миникарте независимо от ещё не открытого телепорта. M впервые показывает весь мир, далее сохраняет zoom/pan. Историческое направление к городу вне локального обзора удалено D113. CityOrigin обновляется при генерации; ResetRun сохраняет отметку существующего города. По уточнению владельца на M остаются значки без подписей «Город»/«Вы здесь». Красные точки — живые враги, а не статичные значки лагерей. Это не открывает телепорт. Внедрено:29 distinct latest PlayMode PASS,28 native720/900/1080 и обычный Mac M. Windows непроверен. Evidence: art/source/city_map_2026-10-21_verification.json.


D111 latest owner correction: the small world notice board is decoration only,
placed substantially farther right of the guild door with a free approach,
smaller and muted to avoid attracting attention. Click the guild BUILDING
to open the contract-board window; D114 local actions use the actual
GuildFootprint and existing interaction reach.


D111 implementation update,2026-10-21: owner approved city_v4_open_temple. Keep green-roof guild and its clickable outdoor contract board plus the original small temple and separate open public teleport forecourt; remove front fences/gates, keep streets free. Mage quest NPC canceled. Portal remains a normal shared-network endpoint; board uses OBJECTIVES.md contract lifecycle. Native first slice implemented; local qualification in art/source/guild_2026-10-21_verification.json.


## Historical D110 concept — superseded by approved D111 above

Owner cancels the temple elder and his quest window. Propose an adventurers'
guild hall in that city quarter, matching the orange-roof town but distinct
with dark-green roof, timber/ivory walls, compass crest and physical paper-order
board beside its public entrance. No new guild NPC is assigned in this request.
The existing ground portal remains nearby with a separate open approach. Owner
returned city_v1 because its seal sits directly at the guild entrance; move it
aside in the revised placement sketch. This was the historical concept stage; D111 above now owns the implemented
guild plus separate temple/portal layout. The existing merchant is
unaffected. OBJECTIVES.md owns board/ranks; art/concepts/guild_2026-10-21/ holds
new sketches. Previous mage/site approvals below are superseded for quest use;
approved orc/ogre appearances remain.

## 2026-10-21 — historical temple / approved world-enemy appearance concepts

The owner now requests a new quest-giving NPC in the existing city, initially
suggested as a senior temple mage/exorcist, and orc/ogre enemy sketches.
«Хорад/Хород» was a voice-input error: no city name is approved. This extends
the earlier merchant-only concept scope; no new service is implemented yet.

Main generates original ImageGen concepts using our hero C, city A_v6 and native
world as references; Astra supplies visual direction/review. Owner returned v1
for revision: larger orc, purple ogre, unmistakable temple-cleric mage with long
hair/beard, ceremonial robes and cross-shaped staff instead of the lantern.
V2 shows an ivory/plum/gold robed mage at human scale beside the temple steps
outside the teleport seal. The comparison sheet enlarges the olive orc and gives
the heavier ogre matte violet/lavender skin. Hero C remains the scale anchor;
both changes also appear in the small tactical strip. These are world-enemy
alternatives, not replacements for portal skeletons. V1 remains revision history.

- [Temple mage v2](../../art/concepts/quest_npc_world_enemies_2026-10-21/temple_mage_v2.png).
- [Final slightly smaller orc / hero / approved purple ogre](../../art/concepts/quest_npc_world_enemies_2026-10-21/orc_ogre_comparison_v5.png).

Owner response to v2: «Орку чуть-чуть меньше сделать. остальное ок». Mage v2
and purple ogre v2 appearance are approved (D108). Follow-up «Утверждаю
размещение и размер орка» also approves final orc v5 and courtyard placement v1.
The owner requested a separate view of placement with
temple/portal rather than relying on the small character-sheet inset.
Existing D099 city code/art already supplies the temple and nearby portal;
the new placement concept shows the mage beside the steps, outside the seal,
with open street/door/portal approaches. This approved placement remains concept
art, not an implemented quest-giver or a redesign of the existing city.

- [Temple courtyard placement](../../art/concepts/quest_npc_world_enemies_2026-10-21/temple_courtyard_placement_v1.png).

Scale, equipment and poses are
illustrative art proposals, not runtime stats, attack contracts or content counts.
D109 activates the missing NPC quest-window concept and confirms orc/ogre spots
are separate. OBJECTIVES.md owns that window/quest brief; exact objectives,
dialogue/rewards and later gameplay rules remain open. Source prompts/hashes/rights:
art/source/quest_npc_world_enemies_2026-10-21_generation.json, *_prompts.json
and *_v2_prompts.json through *_v5_prompts.json. Owner clarified the publishing concern refers to Steam.
Its [published onboarding rules](https://partner.steamgames.com/doc/gettingstarted/onboarding#5)
prohibit religious hate speech; no blanket prohibition on a cleric/cross is
listed. Respectful original fantasy imagery is the art direction, not a claim
that this concept or the eventual full game has passed Valve review.
No Unity assets, scene, UI or runtime rules change in this concept-only step.

## Current city and trade implementation

D102 extends the current finite shop with four blueprint documents beside the
eight equipment types. Same shared ID/atomic gold transfer,50% sale and no
restock. Documents have neutral fixed cells, no equipment rank/LVL; known copies
carry «Изучено» but remain tradable. RESEARCH.md owns consumption/unlock rules.
Mac trade/study UI and actual mouse verified in
art/source/research_blueprints_2026-10-20_verification.json.

Status: D-099 modular city and shared teleport menu implemented; local Mac
qualification recorded in art/source/city_runtime_2026-10-20_verification.json.
D-100 authorizes and integrates the first finite gold buy/sell slice below;
local Mac qualification passed41/41PlayMode plus21native frames, Astra PASS.
Concept phase authorized by the owner2026-10-20, D-095. Three alternative
ImageGen studies completed; the owner selected A with orange terracotta roofs,
then requested a larger city and a distinctive permanent merchant shop instead
of the tent. A_v2's shop blended into housing and was rejected for readability;
A_v3 with a contrasting shop was approved: «Пойдет». The merchant window is a
separate concept surface, D-096; D-100 below implements its first gold trade. D-098 additionally requests service markers and
a city endpoint for the ground seal. D-099 moves that point beside a modest
temple off the square and introduces the shared destination menu/network;
the owner approved A_v6/menu_v1 and requested implementation.
These are alternative
directions for one first city, not three promised settlements or Unity prototypes.
Main owns brief/generation/provenance; GPT-6 Astra owns visual direction/review
and approved visual implementation. Main owns world rules/binding and QA.

## D-100 first gold trade — implemented and locally qualified

Clicking the visible city shop opens the approved fullscreen player/merchant
inventory layout. The two grids share current D097 painted quality skins and
item-safe fitting; equipped instances stay only in loadout and cannot be sold.
The centre shows the selected owner's item, actual stats and gold price, with
one Buy or Sell action. Only gold appears in the trade wallet. Existing global
window navigation and Esc close keep the current UI conventions; no remote HUD
shop shortcut or automatic pause is introduced.

The owner accepted eight initial items, one per existing type, all F/LVL1;
sale is50% of the current item's purchase value, rounded down. Existing numerical
level and independent quality affect trial valuation in Resources/Stage3Merchant;
prices are data-owned and adjustable. No scroll/rune/resource/new-family stock
or automatic restock is inferred from the old illustration. Purchased and sold
instances move between the player's bag and finite merchant stock with the same
ID/type/quality/level. Player-sold equipment can be bought again at the ordinary
purchase price. No separate merchant-gold budget is modeled in this first slice.

The shop can be inspected by clicking its visible building. Transactions require
the hero near its entrance/merchant (catalog radius), a live local service and
no pause/loss/debug/draft/other modal. The window explains missing gold or local
proximity. Revalidate the selected side/ID, ownership, equipped state, price,
funds and credit capacity on action; failures never move items or money.
Wallet observers see the completed ownership/balance, with nested item operations
blocked during payment. Source click/release cannot reopen after closing.

Closing/reopening does not replenish stock. D087 retry keeps CURRENT purchases,
sales, wallet and merchant stock, closing unfinished trade UI; full Reset restores
8F1stock and starter loadout, without reusing previous item IDs. This is current
world lifetime only; no disk-save format is introduced. Stage3MerchantInstaller
creates the new validated data asset; existing scene/world city art stay bound.

Qualification:41/41 focused PlayMode, final Mac/Metal build,21native frames at
720/900/1080 and actual mouse buy20/sell10 (105→85→95gold), Esc/reopen. Astra
final visual PASS; normal startup restored without QA grants. Windows remains
unverified. Evidence: art/source/merchant_2026-10-20_verification.json.

## Selected appearance — A

Owner: «Мне нравится первый концепт с обычными, оранжевыми точками» — the first
image with orange roofs. A is the approved appearance direction: cream plaster
and warm timber, terracotta gables, stone foundations, open cobblestone square
and small connected streets. The sole merchant is visible at the near-right
edge of the square from the entrance. Residents have no additional services.

Owner revision: expand the town into a visibly larger city layout with additional
quarters/streets, preserving ordinary human/building scale. Replace the tent with
a recognizable permanent trading-house building. It must stand out from housing
and keep the sole merchant/approach readable. No additional merchants/services
are implied by the request. The original A palette/architecture remains selected.

Earlier approved architecture: [A_v3 — expanded city and cobalt trading house](../../art/concepts/city_2026-10-20/a_cobalt_trading_house_v3.png).
The residential roofs remain orange. The shop alone has one broad cobalt-blue
hipped tile roof with brass fittings, dark timber open commercial frontage,
two display bays, a large attached plum/gold scales crest and clean paved apron.
One visible merchant remains. Astra's final concept review passed with no blockers:
the unique blue roof/large sign/open frontage solve the previous visual blending.
The owner approved A_v3. Architectural contrast remains essential. D-098 now
adds an interaction contour and illustrated service badge as well.

The [A_v2 orange-roof shop](../../art/concepts/city_2026-10-20/a_expanded_trading_house_v2.png)
is retained as revision history, not the chosen merchant appearance. Owner:
«У торговой лавку всё ещё не видно, она сливается с общим фоном».

- [A — selected square town](../../art/concepts/city_2026-10-20/a_market_square_v1.png).
- [B — alternative stone courtyards](../../art/concepts/city_2026-10-20/b_stone_courtyards_v1.png).
- [C — alternative garden loops](../../art/concepts/city_2026-10-20/c_garden_loops_v1.png).

All three passed Astra concept review. A best matches existing buildings and reads
as a small city. Fine detail will need simplification at actual game scale; exact
layout/counts are not runtime data. Edge water is decorative concept scenery.
Generation/prompts/hashes: art/source/city_2026-10-20_generation.json and
city_2026-10-20_prompts.json. No external artwork was copied or traced.

## Player intent

The player can discover a fantasy city in the larger world. It has inhabitants;
the initial active NPC is exactly one merchant. Other NPCs supply city life only;
their services, quests, dialogue and progression are undecided. Do not infer an
inn, bank, healing, crafting service or additional currencies from their presence.

The established future technology route remains: buy a research item from a
merchant with gold, study it in the player's research workshop, then pay materials
to build. RESEARCH.md owns that route; EQUIPMENT_AND_RUNES.md owns item/forge
limits. City sketches do not implement stock, prices, scroll sources or research
unlocks and do not turn the city into a substitute player forge/workshop.

## Service visibility — D-098; temple-side network point — D-099

The owner requires local interactive city elements to read as clickable. The
initial merchant shop gets a visible colored contour and one small attractive,
colored painted role icon above it. Keep the cobalt roof and scales shop identity.
The marker is anchored to that service, with comfortable spacing from roof/sign;
it must be readable at the game camera without obscuring the building. Generic
flat geometric placeholders are not the intended artwork. Other residents and
decorative homes gain no fake service markers. Future active services will have
their own role icons when approved.

Place the first remote travel point in the city, reusing the approved ornamental
ground seal and a clear walkable approach/arrival area. D-099 explicitly rejects
the square/market placement: put it beside a modest temple in a quiet side court.
The temple is a setting, with no priest/new service/religion implied. All travel
points will open the same menu of opened destinations, including base and future
cities/world points; the product is no longer restricted to one pair. Existing
research/discovery/local validation stays in WORLD_TELEPORTS.md. No third current
runtime point or enemy portal is added. D-099's approved layout now supplies
the modular city; site/footprint wiring is described below.
Trading stock/prices/access transactions remain a separate ready gameplay brief.

[A_v5 — service contour, merchant badge and paired seal](../../art/concepts/city_2026-10-20/a_interactive_shop_and_portal_v5.png)
is superseded in portal placement by the owner's D-099 correction. A thin light
turquoise contour follows the shop's outside silhouette. A compact plum/gold
painted scales medallion floats above the blue roof with an air gap. The flat
silver/cyan/lilac seal lies on free paving off the entrance route. Only the shop
is marked. A_v4 is retained as scale/placement history: the badge and seal were
too large, and the seal occupied the gate axis. A_v5 reduces both and moves the
arrival area right/up. Exact prompts, original outputs and reference hashes are
recorded in art/source/city_2026-10-20_generation.json. This is not a runtime
city or an alteration of current portal data.

[A_v6 — quiet temple courtyard and portal](../../art/concepts/city_2026-10-20/a_temple_courtyard_portal_v6.png)
is the approved implementation reference. A modest pale-stone temple replaces one house on the
left town edge; the single fully visible seal lies in its walkable side court.
The original square is ordinary paving again. Main street, orange/cobalt
architecture, merchant outline and compact scales badge stay recognizable.
Astra concept PASS; owner approved A_v6/menu_v1 for implementation. No new temple service
marker or active NPC is shown. The same travel-menu proposal is in
../../art/concepts/teleport_menu_2026-10-20/teleport_menu_v1.png. Travel rules belong
to WORLD_TELEPORTS; interface behavior to PLAYER_INTERFACE. Generation records:
art/source/city_2026-10-20_generation.json and
art/source/teleport_menu_2026-10-20_generation.json.

## Historical concept deliverables

- Three original full-city studies that differ substantially in silhouette,
  building materials and street arrangement, not only roof color.
- Each has a readable entrance, clear walkable approach and one recognizable
  merchant in an open trading area; other residents have no interaction markers.
- All use the existing cartoonB painted-volume style and fixed elevated
  orthographic tactical view. Humans/buildings share the current hero scale;
  entrances and merchant must stay readable without a foreground roof hiding them.
- A city has enough visible streets/building mass to read as a settlement, while
  leaving navigable space rather than a tightly packed decorative diorama.
- Show the alternatives to the owner before selecting appearance or authoring
  Unity assets/scenes. Record prompts, source files and review limits.

Concept names and decorative architecture do not establish lore or promised
content counts. D-099 implements the city site, world-circle connection and
collision described below; NPC movement and merchant transactions remain separate.

## Merchant window — D-096, dual-inventory revision active

Owner requests adding the merchant window after approving city A_v3. Use the
existing fullscreen Lunar Porcelain visual language and local-service identity,
not a small popup. Existing global tabs/close convention stay consistent with
inventory/forge; no permanent remote-trade HUD shortcut is introduced by the sketch.

Owner rejected v1's large portrait/single-product storefront. The explicit new
requirement is a familiar RPG trading layout with the player's inventory AND
the merchant's inventory visible together. Both item grids dominate the screen;
portraits are compact identity markers. Main inspected BG3 and Divinity: Original
Sin2 trade screens as inspiration, without copying their art or adding their
barter/attitude/party systems. References/rights are in merchant generation.json.

Concept must show two item grids, selected-item details, owned gold and clear
contextual purchase/sale action according to which inventory owns the selected
item. Player bag excludes equipped items D091. Mixed existing item-family icons
can illustrate the grids; they do not establish the merchant's actual stock.
Trade uses existing gold; no additional currency. The established research-item
purchase route is the first content reference, not an approved stock/pricing table.
Owner correction: merchant top wallet shows ONLY gold. Hide wood/stone/metal
there; ordinary HUD/other windows keep current resource displays. A possible
future trading resource is undecided, so add no currency/placeholder now.
Equipment cells use the global quality treatment D097; research scrolls have no
equipment rank/LVL and empty slots remain neutral.
Sample goods/prices are illustrative pending a ready gameplay brief. Buying a
research item must not depict automatic study or free building construction.

Exact sale values, buyback and stock refresh remain undecided; tower half-refund
does not define merchant prices. Do not infer discounts/reputation/dialogue quests,
equipment stock or a forge-scroll source. Current phase
creates an ImageGen interface proposal for owner review. Astra owns composition
and later approved visuals; main owns generation and future rules/binding. Runtime
city/NPC/service access and safe one-time payment remain implementation work.

## Historical concept acceptance

The initial comparison is complete: three saved images visually inspected,
Astra checked style/readability/distinction, and the owner selected A. The requested
larger-city/permanent-shop A_v3 revision was approved. D-100 now implements the merchant UI.
No Unity build or gameplay test is needed for this art-only phase;
runtime wiring and performance qualification remain implementation work.

## D-099 runtime integration and data

The town uses isolated shop/temple/12-kit sprites and a tiled XY street mesh,
not the full concept painting. Shared Stage3CityPresentation layout reports
footprints/portal/merchant offsets; WorldTravel finds a clear district around
WorldTravelData.cityOrigin and registers ground footprints in HeroObstacleMap.
Manual nodes, landscape grass/flowers and new workshop/dwelling/forge placement
are excluded from the city bounds. Existing tree sprites and low walls frame
rounded connected streets/courts. Reset keeps the town and clears discoveries;
D087 retry keeps current openings. The initial city point replaces the old remote
point, with no third current location. Ambient sprites have no new AI/service.

Local qualification: 16/16 focused PlayMode checks, final Mac/Metal build,
16 native city/menu frames at720/900/1080p and actual mouse/E roundtrip.
Astra's final appearance review is recorded with the evidence in
art/source/city_runtime_2026-10-20_verification.json; Windows is unverified.
Source/prompt/rights/import wiring:
art/source/city_runtime_2026-10-20_generation.json. Shop role/merchant placement
is present; D-100 above supplies the separate authorized equipment buy/sell slice.
No extra currency, temple service or scroll source.
