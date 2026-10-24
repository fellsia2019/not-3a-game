# Project Agent Rules

These rules are the project-specific source of truth for every agent or chat.
Explicit human instructions win. Accepted decisions in
`production/DECISIONS.md` win over older plans and drafts.

## Read Before Work

1. Read `gamedesign/GAME_VISION.md` and `gamedesign/DEVELOPMENT_PLAN.md`.
2. Read the relevant feature spec, `gamedesign/ART_DIRECTION.md`, and
   `production/WORKSTREAMS.md` when they affect the task.
3. Inspect current files, packages, scenes, prefabs, assets, console state, and
   version-control status before changing anything.
4. State the active workstream, owned surface, dependencies, and observable
   acceptance criteria for nontrivial work.

Do not ask for facts that are discoverable in the repository. Ask about
player-facing intent, expensive architecture choices, destructive actions,
missing assets, credentials, or external coordination. If the user's proposal
creates a game-design or production problem, explain the evidence and recommend
a better option instead of implementing it silently.

## Project Facts And Scope

- Unity `6000.6.2f1`, URP, Input System; Windows/Steam is the only MVP target.
- Product intent is tower defense combined with survival, exploration of a huge
  world and base development (D-062). The core is explore/gather -> invest ->
  defend -> recover/develop -> repeat. The current short run is a prototype.
- The throne is the loss-critical objective. Resources are wood, stone, metal.
- D-095 opens first-city concept work: three contrasting fantasy-city studies,
  NPC inhabitants and only one initial active merchant. Owner selected A:
  timber/cream plaster, orange terracotta roofs, open square. Location,
  trade catalog and runtime were pending at that concept gate; D099/D100 now
  implement city and first trade. This supersedes D063's old city deferral
  for the current concept phase; other NPC services remain undecided.
  Owner then requested a larger city and distinctive permanent shop instead of
  a tent. A_v2 blended into housing; owner approved A_v3 expanded city/blue-roof shop.
- D-096 requests the merchant window. Use the current fullscreen Lunar Porcelain
  shell; ImageGen appearance approval precedes Astra visual implementation.
  Catalogue/prices/sale/restock rules are not inferred from a UI sketch.
- D-096 merchant window must show player and merchant inventory grids together,
  with compact portraits; merchant wallet shows gold only, no wood/stone/metal.
- D-101 requests a shared medieval magical engineering BLUEPRINT item base for
  future research/building upgrades (workshop/forge etc), new towers/weapons.
  Owner rejected tilted/unfolded shapes and the later modern white-sheet mockup.
  Reuse the earlier aged ivory/amber paper COLOR, texture and plum drafting style
  in a straight rectangular sheet that fits a fixed cell. Two primary categories:
  opening new content (including construction access) and improving existing content.
  No equipment F–SSS or independent item LVL1–100 on blueprints: an upgrade number
  is the TARGET BUILDING LEVEL. Examples4–5 do not change approved building caps.
  Every inventory/trade cell and row stays fixed; content never shrinks/reflows
  lower rows. Owner approved rect_categories_shop_v3: cyan spark for opening,
  muted brass chevron and target II/III for upgrade. This is appearance approval,
  not runtime stock/price/usage approval. Possible spell-scroll magic use is
  future only. No current runtime/source/price/consumption/stock migration or
  blanket prerequisite for all upgrades is implied. RESEARCH.md owns scope.
- D-102 activates blueprint trade/study: owner accepted Longshot construction,
  wand forge recipe and forge target stages II/III as the first four documents.
  Research workshop always has ONE level and is NOT upgradable, superseding
  D075's workshop-level proposal. Buying a document, studying it locally and
  building/upgrading the unlocked object are separate actions. Use approved D101
  item art; study-window and known/history appearance have owner approval after
  ImageGen review, with native implementation by Astra. Studying consumes one
  paper; known duplicates stay tradable and marked. RESEARCH.md owns the brief.
- D-100 implements first finite gold trade through the approved dual-inventory
  shop window:8existing types,F/LVL1,one each;50% sale,no automatic restock.
  Trial prices live in Stage3Merchant. Same item instances transfer; equipped
  items are excluded. Local action validation and D087 current trade/stock retry
  remain; full Reset restores finite stock. Locally qualified41/41PlayMode and
 21Mac native frames, Astra PASS; Windows unverified. Evidence:
  art/source/merchant_2026-10-20_verification.json. No scroll,
  rune, resource trade or new item family. CITY_AND_TRADE owns the contract.
- D-098 adds visible colored interaction contours and small painted role icons
  to active city services, initially only the merchant. The city contains the
  remote endpoint of the existing base/world ground-seal pair (no third portal).
  A_v5 concept passes Astra review and awaits owner; existing research/discovery rules
  and current runtime placement stay until city integration.
- D-099 supersedes D-098 square placement and pair-only product scope. City
  circle belongs in a quiet side courtyard beside a modest temple. Any travel
  portal opens one shared menu of opened destinations; explicit selection/confirm,
  current point disabled, existing research/discovery/local arrival checks kept.
  A_v6 city/menu_v1 are owner-approved for implementation. Modular city and
  common destination menu are integrated and locally Mac-qualified16/16PlayMode;
  evidence: art/source/city_runtime_2026-10-20_verification.json; Windows unverified;
  no temple NPC/service, new currency or invented extra current destination.
- D-097 requires rank-colored background/border consistently across item UI
  (bag/loadout/forge/trading/actual tool previews), separate from cyan selection.
  F/E/D/C/B/A/S/SSS only. Owner approved64cell board: Dgreen/Cblue, polishedgold S,
  radiantsharpwhite-gold SSS with containedglow. Astra implemented shared frames in
  Equipment/Forge/Professions/Dock; D100 merchant runtime reuses them. Empty slots/
  ungraded scrolls stay neutral, world art/stats unchanged. Local qualification:
  art/source/item_quality_cells_2026-10-20_verification.json; Windows unverified.
  Owner subsequently rejected the flat dark fills/thick gold rails: the approved
  original requires painted multitone gradients/texture and thin shaped luminous
  framing. Astra replaced the procedural approximation with the actual ImageGen
  source-derived skin atlas (measured UVs, textured centres/thin frame patches).
  Correction passes17PlayMode and strict-reference27Macframe review; prior local
  appearance PASS did not mean owner acceptance. Windows remains unverified.
  Safe-area repair bakes64UI-only full-source silhouette bounds into ForgeArt,
  validates neighbours and centres with proportional padding. Axe bands share
  rounded pixel edges. Source PNGs/world pivots unchanged;33Mac frames plus large
  Aaxe pass Astra. art/source/item_safe_areas_2026-10-20_verification.json.
- D-087 supersedes the active Stage3 full-world defeat snapshot: retry keeps
  CURRENT world/inventory/XP/ranks/research/buildings and returns morning for
  the same pending horde, restoring throne and clearing failed enemies/shots.
  Legacy finite Stage2 retains D-062 snapshot behavior. Full reset stays separate.
- D-064 adds nearest-target automatic hero attacks. Portal enemies target
  the throne/blockers; D111 world enemies target the hero. D103/D104 implement
  hero vitals and death/recovery; D120 adds active bow/staff and learned moving
  attacks. The original D064 slice is historical.
- D-074 continuing Stage3 uses clock-driven mandatory nights and repeatable
  daytime gates at the last cleared tier. Closing stops new spawns, not live
  creeps; mandatory hordes wait for all overlapping threats to clear.
- D-105 clarifies daytime repeats only last WON night difficulty (interval/HP/
  current type), never a failed next tier or finite night enemy quota. Continuous
  spawns until manual close or19:30; obsolete24-live cap removed. Retry keeps
  previous cleared tier. First horde buff is trial HordeRun data; no new types.
- D-088 restores explicit dwelling Sleep until19:50, ten game minutes before
  the20:00 horde, blocked by an open daytime gate, living creeps/shots or an
  active horde. Early victory never skips to dawn. No skipped-time income.
- Tutorial flow remains post-MVP. D-074 confirms hero cap 100 and low-splash
  exploration combat; dense matched-tier hordes require towers. Larger
  gathering/rune proposals remain in the feature specs. D120 implements
  11 actives, 20 combat passives and 3 adventurer talents; HERO_PROGRESSION.md
  distinguishes current runtime data from superseded proposals.
  Equipment levels 1–100/ranks F–SSS and separate hero/tower runes are the
  target; rune counts, stacking and gathering category remain proposals in
  EQUIPMENT_AND_RUNES.md. Do not treat design tables as runtime data.
- D-075 target ends the campaign after three distinct strong bosses. Boss
  venue is undecided. TD is the main combat XP/gold source; world camps offer
  less XP and better loot chances as a target. D111 world combat is implemented;
  bosses and enemy loot tables remain future work.
- D-090 equipment has five combat slots (head/chest/arms/legs/weapon) plus
  pickaxe/axe, levels1–100 and8qualities F/E/D/C/B/A/S/SSS. Weapons: bow and
  wand only; no sword/shield/SS. Forge stages1/2/3 cap at D/B/S and upgrade
  with special scrolls only in legacy profiles; D102 opt-in uses studied forge
  targetII/III blueprints from trade instead, with no second scroll spend. SSS is ultra-rare drop only,
  never forged. Production advancement is D-093; D102 fixes workshop at one level.
- D-094 implements exactly three paid throne upgrades of +5 current/max HP,
  preserving existing missing HP. The existing selected card uses HordeRun prices.
  D-087 retry keeps current upgrades/spending and fills the upgraded maximum;
  full Reset clears them. Legacy finite snapshots restore count/HP/wallet together.
- D-091 excludes equipped items from the inventory bag; show them only in
  loadout slots. Equip/replace/unequip moves the same instance between views.
  Actual bow/pickaxe/axe quality is visible on the hero; clothing/armor remains
  unchanged. Tools replace the baked starter only during harvesting.
- D-092 adds separate miner/logger mastery1–100, XP from actual manual material
  grants and independent profession points. O has approved combat/profession
  tabs; strength/tempo/reach talents combine with equipped tools without extra
  stock or enabling missing tools. Retry preserves current mastery; Reset clears
  it. Stage3Professions data owns trial tuning. Larger gathering trees remain
  later work; D120 adds three working adventurer talents. D093 owns generator
  advancement.
- D-093 gives each generator5paid stages with1/2/3/4/5units per existing cycle.
  Upgrade an individual building for materials+gold through its existing card;
  all stages are available without research in the current prototype. Upgrade
  preserves partial cycle and never pays income immediately. Retry preserves
  current level/progress; Reset clears buildings. Art/footprint/pads stay unchanged;
  new construction still costs only materials. Research gating remains future.
- D-075 new construction costs only wood/stone/metal. Upgrades/research may
  also use gold; trade is another sink. D-079 removes Stage3 Rapid gold.
  Historical Stage2 Rapid and the free workshop remain legacy data.
- D-078 selects owned buildings in the world and displays a long card above
  the HUD with live stats; the workshop links to the existing research page.
  D-079 activates tower/barricade resource upgrades, matching-type/equal-level
  merging and half-investment sale. Empty-cell movement is preparation-only;
  other building upgrades remain planned. See TOWER_UPGRADES.md.
- D-080 removes the redundant build Select button: a catalogue card chooses its
  blueprint directly. Finished buildings can be selected/dragged with B open;
  their press cancels an armed draft before any payment. The selected strip
  appears on release/drag, above the construction dock as well as the hero dock.
- D-077 supersedes the compact-card geometry: one continuous bottom HUD
  integrates minimap/hero/nav/abilities/status; a full-width selected-building
  strip docks directly above. No Move button: world drag handles movement/
  compatible merging. The v3 appearance was returned for D-078 revision.
- D-078 puts a narrow full-width nav ribbon between selected strip and main
  dock. Reserve room for potentially ten spells; final equipped count is open.
  Labels are HP/MP/XP, with colored numeric tracks. Until real hero sources
  exist, 0/0 + «Не открыто» is an inactive UI fallback, not game balance.
  Owner-approved v4 is implemented by Astra using native assets; live selection/
  input binding is integrated. Local Mac verification is recorded in
  art/source/hud_v4_2026-10-19_verification.json; Windows remains unverified.
- D-085 adds world-owned hero XP from hero/tower confirmed kills, levels1–100,
  cursor Blink (Q or HUD targeting) and three personal passives. The opt-in
  Stage3 binding loads Stage3HeroProgression data and preserves current XP/points/
  ranks/cooldown at D-087 retry. HP/MP and extra attacks remain deferred;
  gathering professions are implemented by D-092.
- D-086 approves polygon_v3 level-up: sharp symmetrical shield, soft outer
  energy waves, only heading/level/awarded points. Reset/retry/loss clears it;
  pause/modal UI freezes without replay. A CC0 recorded female
  three-part choir with echo is integrated;
  audio source/license and qualification are in art/source. Voice is accepted;
  the owner postponed the hammer. Runtime contains no impact layer.
- Factorio-style automation/logistics and persistent roguelite progression are
  post-MVP until the basic loop is proven.
- Follow `gamedesign/GAME_VISION.md` for the current scope and non-goals. Do not
  add content counts, currencies, controls, UI, lore, progression, or tuning
  that have not been approved or explicitly delegated.

- D-103 enables100HP/100MP and1HP/s +5MP/s regeneration; BlinkI–IV spends100/75/50/25MP
  and has5/4/3/2s cooldown. Existing ranges/points unchanged; invalid casts spend
  nothing. Retry keeps current vitals, Reset fills. F3 explicit test-wave command
  advances only clock to next night, keeps living threats and credits no skipped
  income. Ordinary Stage3 start stays clock-gated. Tactical max15, start5/min2.5.
  Owner requests Debug WTF: no mana cost and ZERO cooldown for current Blink,
  preserving landing/pause/UI checks; Reset/HUD disable clears cheat.
- D-106: current Stage3 day600active seconds/night180; night spawns until dawn,
  then keeps living creeps/shots until cleared. No authored-count quota or early
  victory; no overlapping mandatory nights. D105 last-WON daytime flow, cutoff
 19:30, sleep19:50 and progress-preserving retry stay. Zero timed-night duration
  preserves finite legacy Stage2. Existing density remains trial balance.
- D-107 approved thin shaped top-edge HUD and centered complete small analog
  dialA smoke-glass. Day10/13 and night3/13circle follow Run.DayNightCycleProgress;
  noHH:MM or separate timer. Softgreyolive/ivoryday to nearblack/bloodredmiasma,
  no sharp sector seam/scenery illustration. Locally qualified, AstraPASS25native
  frames; Windows unverified. Evidence:cycle_top_hud_2026-10-21_verification.json.
- D-104 death/recovery locally qualified: exploration-only hero damage,
  free base-portal respawn, half XP within current level (no lost levels), drop
  only bag, keep equipped gear/tools; recoverable world/map/body direction marker.
  Owner chose next separate step and marker concepts before implementation.
  Final label «Выпавшие вещи» / «E · Забрать вещи»; offscreen cue is a compact
  tooltip with small arrow on destination side at8bearings; latest3approved.
  Keep every nonempty death drop until recovery, including across later deaths
  and throne retry. Losefloor(currentXP/2), retainlevels/points/ranks/cooldown,
  restoreHP/MP at safe base-anchor respawn without unlocking travel. Reset clears.

- D-113/D-114:workshop-pinned starter teleport knowledge unlocks manual circle
  construction; no automatic base circle. Automatic hero attacks require stopping
  until the D120 moving-attack talent is learned.
  Guild local actions use the actual GuildFootprint, not one tiny entry point.
- D-116/D-117:approved separate orc/ogre camps have broad open fields/retreats,
  reserved before manual resources, shared scenery collision, unchanged combat/
  quest data. Current city has a fixed manually authored internal layout; future
  cities get individual layouts/styles. Never replace it with random filling or
  shuffled housing. Whole-city safe world-origin selection remains allowed.

- D118:quest direction on M/minimap after accept, one tracked order; cleared
  targets local guild turn-in. HP/actual damage/hit effects cover BOTH portal
  and neutral enemies; MP only from a real source. Manual UI shows actual+yield.
  Wild camp trees/rocks are harvestable, cities/auto districts exempt. Camp group
  near authored props shares aggro; existing leash/populations remain. Seal
  rim opens explicit common travel menu. Empty future sites/depleted nodes do
  not block buildings; occupied building ground prevents resource regrowth.
- D119 is the historical ten-active/about-twenty-passive brief, superseded by
  the implemented D120 catalogue below. Бой/Добыча/Авантюрист and the branching
  O icon remain; preserve learned hero/profession ranks and other dock icons.

- D-120 implements Q+5bow+5staff,20combat passives,3actual adventurer talents,
  the mixed prerequisite tree and twelve fixed gear slots in current Stage3.
  Both weapons are equipped, Z switches active stats/1–5; F1/F2/F3 use finite
  bag potions, F4 reserved, debugF10. Staff retains Wand/WandRecipe identity.
  Amulet/rings/«Защитная печать» use equipment quality/level; seal is constant
  damage reduction without MP. Potions cost100/100/250gold and restore100HP/
  100MP/100each; stock5each, shared20sCD, no restock/freeReset bottles.
  HERO_PROGRESSION/EQUIPMENT_AND_RUNES and D120 own the current contracts,
  superseding older counts/single weapon slot. No current enemy loot drops.

- D-121 supersedes independent profession points: all three O branches spend
  hero-level points; gathering mastery no longer awards points. Stage3 towers
  upgrade beyond5 only through their own card; barricades cannot upgrade and
  current drag/direct merging is disabled. D121 reduces portal XP/gold.
- D-122 requires studied construction documents for sawmill/quarry/iron mine/forge,
  separate from buying, building, recipes and existing upgrades. Clock overlays
  world LVL labels. D-123 sets current portal HP5, first spawn interval4s and
  unchanged HP at later existing shorter intervals; broader balance is deferred.

## Change Rules

- Make the smallest coherent change that satisfies a ready feature brief.
- Prefer plain C# rules plus narrow MonoBehaviours for lifecycle and scene
  binding. Do not introduce global managers, service locators, event buses, DI,
  pooling, or an interface layer without a present measured need.
- Preserve serialized names and existing scenes, prefabs, ScriptableObjects,
  packages, and user changes. Record migrations when a format must change.
- Never edit `Library/`, `Temp/`, `Logs/`, `obj/`, generated `.csproj`/`.slnx`,
  package-cache files, or third-party sources unless the task explicitly owns
  them.
- Keep Unity `.meta` files with their assets. Do not move or delete assets via
  raw filesystem operations when Unity references may exist.
- Install or remove a package only for an approved current requirement. Treat
  `com.unity.pipeline` and [Unity CLI](https://docs.unity.com/en-us/unity-cli)
  as optional automation with batchmode and manual fallbacks because CLI is
  experimental and [Production Pipeline](https://docs.unity.com/en-us/unity-production-pipeline)
  is beta.
- Use asmdefs only when a real compile/test boundary exists; do not create one
  assembly per feature.

## Documentation Contract

- `gamedesign/GAME_VISION.md`: stable player/product intent and MVP boundary.
- `gamedesign/DEVELOPMENT_PLAN.md`: canonical stage checklist and current state.
- `gamedesign/features/`: one spec per nontrivial mechanic/system, created only
  when that work becomes active.
- `production/DECISIONS.md`: accepted or superseded durable decisions.
- `production/WORKSTREAMS.md`: ownership, dependencies, and cross-chat handoffs.
- Architecture documents are created only after a durable boundary exists.

Do not create parallel roadmaps, changelogs, or duplicate GDDs. When behavior
changes, update the existing source of truth in the same change. Runtime content
values belong in project data assets once implemented, not in competing Markdown
tables.

## UI And Visuals

- D124 owner explicitly forbids generic website/account/profile bust or person
  silhouette icons in game UI. Do not use the HUD portrait again as the Player
  navigation icon either. Owner selected the original crossed bow/staff with a
  thin crescent (navigation study variant1). World-HUD navigation shows only
  pictograms and existing hotkey badges; full names appear on hover. Player is
  first and returns to the skill dock through Tab, distinct from O hero development.
  Construction now pages at most3 studied cards with contextual1–3 (revising
  the initial5), hides weapon previews in that mode. Owner approved the FIRST
  HUD composition: quick items left/nav right in the upper servicebar; category
  tabs stay BELOW in the main dock above cards. Prices centre their actual
  contents and wrap only when needed; fixed2×2/empty price slots are rejected.
  D124 adds two actual world-combat adventurer passives to the original3,
  for5nodes with precise orthogonal links. Owner approved implementation of
  the proposed development/inventory/trade/silver-LVL/gold-upgrade appearances,
  with the latest HUD corrections above. Locally Mac-qualified2026-10-23:
  37EditMode/82PlayMode PASS,1headlessviewportSKIP; final69native captures,
  Astra PASS on9v5+29reusedv4frames. Evidence:art/source/hud_refactor_2026-10-22_verification.json.
  Windows and sustained manual balance remain unverified.
- D125 moves moving-fire talent14 to late bow training after5, retaining its
  both-weapon effect and learned ranks. Strength/tempo have5ranks, reach3;
  each adventurer talent5. Two paid one-rank gathering capstones require
  StrengthV:10% full medium STONE node or one extra ordinary tree within the
  hero's normal gathering range, no recursive chain. Current data owns values.
  All branches share99earned hero points,156possible paid ranks, requiring choices.
- D126 removes artificial building level labels when no upgrade capability
  exists, and requires measured foreground tooltips. F10 gains explicit XP/level
  presets through normal progression/shared points/cap; no ordinary startup grants.
  Owner requests all old players closed and exactly one latest build. Weapon
  switch appearance is accepted with thin rims,50%horizontal overlap and active
  foreground short turning/depth exchange; updated weapon_switch_v2 records it.
  Owner subsequently requires reference-shaped painted Z control/ivory keycap,
  patterned neutral empty surface,and FOUR cardinal gems only on actual active
  weapon. weapon_switch_v3/weapon_fidelity evidence own the correction;prior
  native appearance PASS did not establish owner acceptance. Active turn rises
  clear of its caption;resting overlap and gameplay/input remain.
- D127 renames research workshop to Исследовательский центр (30wood/40stone),
  dwelling20wood/20stone. Both materials are checked atdraft andcommit; serialized
  Workshop identities and one-level/localstudy/retryreset stay. Latestowner picks
  sharedleft buildingpreview replacing hero part ONLY duringConstruction, keeping
  minimap/3cards and categoriesbelow. Servicebar matches hud_polish_v1: quickleft,
  navright, paddedbottles/containedplumbadges/ivorynavkeys. RealassignedF1–F3
  attempts pulsecyan/red by actualresult;F4empty/silent, no+/count/CD. Unfunded
  card art remains fullcolour with coralwell/rubyalert/actualdeficit. Astraowns
  visualmatch/nativequalification; conceptapproval doesnot establishnativeacceptance.
- D127 owner returned native-v2 appearance: prior native PASS is not acceptance.
  Match selected hud_polish_v1 ribbon proportions/contours, round HUD flasks,
  shaped luminous states and bold plum type. Construction hover popups are
  forbidden; actual stats/description/placement/shortage/hints stay LEFT. Native
  review must compare appearance against source, not just rectangle geometry.
- D127 reference repair locally passes39 final native assertions; Astra reviewed
  21 changed v2 frames plus18 reused v1 layouts, retaining the colour/bevel RETURN.
  Evidence:art/source/hud_reference_repair_2026-10-23_verification.json. Local
  reference qualification is not owner acceptance; Windows remains unverified.
- D128 owner directly revises the oversized servicebar: quick/navigation controls
  near skill-icon size, multiline modal labels, ivory edge-only header states,
  inset minimap terrain with a less intrusive rim, and continuous upper joins.
  Reuse current artwork; this explicit correction supersedes D127's large sizes.
  Astra owns native rendering; main owns capture/build/docs. Gameplay stays intact.
- D129 makes all F1–F4 configurable from inventory by actual active-action
  capability, preserving bag ownership/counts/shared CD; empty cells silent.
  Existing WandRecipe is Владение посохом: consuming local study gates staff
  equip/skills and recipe at ForgeII, Небесная кузница. Merchant staff remains
  buyable before study. Unknown staff nodes are obscured/noninspectable.
- D130 adds selective RMB autocast for learned AimedShot/Arcane; manual use,
  stationary aim, ordinary target/range/mana/CD stay. Fire/Ice arrows gain small
  areas; Arcane smaller splash. Skill4 is a red persistent piercing-arrow area,
  skill5 poison DoT/slow/physical vulnerability. Ice freezes; Fireball has larger
  burst plus ground fire; Lightning stuns and adds magic vulnerability. No enemy
  baseline armor is invented. All areas keep bounded per-tick target counts.
  HERO_PROGRESSION owns roles/limits, assets own current tuning; Windows and
  sustained full-run balance require separate qualification.
- D128–D130 local qualification completed2026-10-24:88relevantPlayModePASS,
  1oldheadlessviewportSKIP corroborated by native;45finalframeassertions,
  Astra15cleanv3+30reusedv1UI after retained RETURNs. Evidence:
  art/source/active_actions_2026-10-23_verification.json and native_review.json.
  Current build MacActiveActions849979653bytes; ordinary first8rendersready.
  Local qualification does not imply owner acceptance or Windows/full-run balance.
- All visual decisions/appearance and visual implementation are assigned to
  GPT-6 Astra. First create sketches through ImageGen, obtain the owner's
  appearance approval, then delegate their visual Unity implementation to Astra.
  The main agent performs ImageGen and nonvisual gameplay integration.

- Treat reference images as inspiration only; never ship or trace unlicensed
  material. Record source and license before any asset becomes runtime-ready.
- Preserve the fixed isometric readability rules in
  `gamedesign/ART_DIRECTION.md`.
- Use `production/pipelines/UI_TOOLKIT_PIPELINE.md` only if UI Toolkit is the
  chosen stack. Its presence does not authorize changing the UI stack.

## Verification And Handoff

A change is done only when the requested behavior works, relevant EditMode or
PlayMode checks pass (or are marked degraded with a reason), the project has no
new attributable console errors, required scene/Inspector wiring is documented,
and the canonical plan/spec/handoff is updated.

Before closing cross-system work, add one concise entry to the handoff register
in `production/WORKSTREAMS.md`: behavior, contracts/files changed, verification,
and explicitly remaining work. Do not overwrite another chat's work or modify a
surface owned by another active workstream without coordination.
