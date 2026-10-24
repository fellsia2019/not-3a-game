# Workstreams And Chat Handoffs

### 2026-10-24 — D128–D130 local completion and handoff

- Main: actual quick-item capabilities/bindings, mastery/equip/ForgeII gates,
  autocast, physical/magic hit routing, timed states/DoT/areas and data/tests.
  Astra: compact HUD, assignment/locks/autocast/detail layout and status readability.
- Contracts: allF1–F4 configurable, empty silent; one local consuming Владение
  посохом study gates equip/skills and recipe at Небесная кузницаII. IDs/items/
  ranks/current retry preserved. RMB supported1/6 plus manual casts;4persistent
  physical arrow,5poison/slow/physical vulnerability, Ice freeze, Fireball ground
  fire, Lightning stun/magic vulnerability. No baseline enemy armor invented.
- Main files: Equipment.QuickAccess/catalog/Blueprints, CombatStatuses/Enemy/Guild,
  HeroSkills/catalog/driver, interface binding and existing Resources. Visuals:
  FantasyHud partials/CircularMap shader/HeroSkillPresentation. Existing scene
  Resources wiring; no package/Inspector dependency or disk-save migration.
- Verification:88unique relevant PlayModePASS/1oldheadlessviewportSKIP; final45
  native assertions at3sizes; Astra15clean v3+30reusedv1UI after four recorded
  RETURNs. D128 compact33assertions/v2PASS retained. V3 fixes QA-only state
  contamination;120Hz final-tick and modal autocast preferences tested.
- Native mouse assignment/clear/RMB1/6/Z and final keyboardF4empty/I/O/Tab pass;
  mapped F4 consumption uses real InputSystem QA/tests. One final ordinary Mac
  849979653-byte player, first8renders ready, LVL1/XP0/gold5/no QA grants. Build
  and raw evidence in Artifacts/ACTIVE-ACTIONS-2026-10-23; durable report:
  art/source/active_actions_2026-10-23_verification.json, native_review.json and
  active_skills_2026-10-23_audit.json. HERO_PROGRESSION owns full active-role audit.
- Remaining:Windows/Steam, sustained balance and owner appearance acceptance.
  PortalHP5 can die before late statuses show; broader horde retuning stays deferred.

### 2026-10-23 — D129/D130 follow-up: quick assignment and staff/combat actions (accepted brief; completed above)

- Main owns inventory action capability/quick-slot rules, shared consumption/CD,
  staff study/equip/craft/progression gates, combat targeting/autocast/persistent
  damage, data and tests. Astra owns inventory assignment UI, locked skill
  presentation, autocast indication and established-art effect readability.
- Quick F1–F4 must be configurable through inventory, limited by actual active
  action capability; preserve owned items/counts, no grants, retry keeps current
  bindings. Existing defaults may remain; empty slots stay silent.
- Existing WandRecipe identity is to become Владение посохом: one studied paper
  opens staff equipment/skills and staff recipe at ForgeII. TierII display name
  Небесная кузница. Buying a staff remains possible, equipping requires knowledge.
  Before knowledge, staff nodes cannot be upgraded, cast or inspected; obscure
  them with the existing desaturated/locked treatment. Preserve bought papers,
  research/local study transactions and known duplicates.
- Owner requests RMB autocast toggles for AimedShot1/Arcane6 only, while preserving
  manual use, normal target validation, mana/CD and stationary aiming. Fire2 and
  Ice3 become bounded small AoE; Arcane6 gains smaller splash. Fireball9 has a
  larger burst area followed by damaging fire at the same area. Replace Focus4
  with a piercing magic arrow that stays at impact area for3seconds, attacking
  enemies there. Gameplay data owns trial radius/cadence/power.
- Latest owner selected poison for skill5: DoT, slow and physical-defense
  weakening; Lightning gains short numbness/stun and magic-defense weakening.
  Ice must gain actual freeze (currently only slows). After implementation,
  audit every active skill's element/buff/debuff/targeting/duration/cadence so
  bow and staff have balanced distinct roles rather than visual/name clones.
  Existing enemies currently have no separate armor/resistance stats: main must
  document the exact implemented school-specific weakening rather than invent
  hidden baseline armor. Skill IDs/ranks remain stable; corresponding passive
  descriptions/effects must follow skill4/5 replacements.
- D128 native-v1 RETURN (Tab/crown/backing scraps) was repaired and locally
  passed in v2; latest qualified build/evidence are recorded in the24Oct handoff.


### 2026-10-23 — D128 compact HUD correction (accepted brief; completed above)

- Main: opt-in QA capture, build, evidence and canonical docs. Astra: HUD geometry,
  modal states/text fit, minimap aperture/ornament and upper corner joins.
- Dependencies: current D127 assets/results, D124 controls/three cards, current
  minimap/clock delegates. Preserve transactions, state, world markers and timing.
- Acceptance: near-skill-size quick/nav controls, contained small badges, full
  multiline modal labels, edge-only ivory hover/active, no map leakage or heavy
  occlusion, continuous top joins at720/900/1080, one latest ordinary build.
- Locally completed after v2 repair; no new raster generation/package/save
  migration. Subsequent D129/D130 gameplay is qualified in the24Oct handoff.

### 2026-10-23 — D127 reference repair after owner RETURN

- Main owns ImageGen/provenance, QA capture, build and docs; Astra owns the
  reference audit and visual implementation. Dependencies: existing paid builds,
  potion transactions, studied three-card catalogue and shared left preview.
- Replaced service ribbon with one connected painted32/23/45 surface; large round
  flask family, contained keys/counts, stronger plum type and shaped silver/cyan/
  red border-only states. All building descriptions, placement, traits, shortages
  and hints stay left; construction popups are removed. Category tabs remain below.
  Files:FantasyHud.cs/.Dock/.SkillDock/.HeroLoadout/.Construction, four new Resources
  PNG/meta pairs and opt-in HudRefactorReview. Item identity/art outside quick HUD,
  gameplay, costs, input and serialized scene references are preserved. Resources
  loads need no Inspector wiring or package/save migration.
- Final39 native frame/assertions PASS at720/900/1080. Astra inspected all39 v1
  frames, returned weak pulse/bevel, then passed21 changed v2 frames plus18 reused
  unchanged layouts. Prior evidence is preserved; all six defects closed. Previous
  65 functional PASS remain the baseline; no redundant gameplay rerun for art only.
  Compile/player logs have no new attributable exceptions; diff whitespace check clean.
- Build849904711 bytes:Builds/MacHudReferenceRepair/Not3AHudReferenceRepair.app.
  Old27278/interim29751 closed; QA autoquit; exactly one ordinary PID30606. All
  first8 renders ready, LVL1/XP0/gold5, no QA grants. Actual native mouse/key/F4/
  B/Base/left dwelling smoke passes; final F3 refusal visibly red. Source/prompts:
  art/source/hud_reference_repair_2026-10-23_generation.json. Qualification:
  art/source/hud_reference_repair_2026-10-23_verification.json and native_review.json.
- Remaining:Windows/Steam and sustained manual feel. This local appearance PASS
  does not infer owner acceptance; the old appearance RETURN remains history.

### 2026-10-23 — D127 HUD fidelity and paid base construction handoff

- Main owns catalogs/payment/attempt-result binding/tests/build/capture/docs;
  Astra owns selected native appearance and review. Dependencies:D120 actualbag/
  cooldown,D124 firstcomposition/3cards,D126 controls,D102 localstudy.
- Latest owner picks shared left building stats replacing hero part ONLY in
  Construction, retainingminimap/3cards/categoriesbelow. Unfundedcards use full-
  colour art/coralwell/rubyalert/actualdeficit. Servicebar matches selected
  hud_polish_v1 with paddedquicktiles/containedbadges/ivorynavkeys. Assigned
  mouse/F1–F3attempt shareactualtransaction andcyan/redresult;F4empty/silent.
- Researchbuilding renamed Исследовательский центр,30wood40stone; dwelling20/20.
  Bothmaterials checked atdraft/commit; actualpaidbuild/sleep/retry/reset remain.
  Files:WorldTravelData/Construction,DwellingCatalog/driver/presentation,legacyHUD,
  bindingHeroLoadout/research/selection;FantasyHud Dock/SkillDock/Construction/
  Research/HeroLoadout/nav. NeworiginalResources/Stage3HudPolishSkins PNG/meta
  isbyteidenticalsourceatlas withmeasuredUV. QAReview addsopt-in30statecapture;
  olderdisposableprerequisitesexplicitlyfund paidcentre. SerializedWorkshop IDs,
  artGUIDs andscene wiring preserved; additivecostfields only, no package/save
  migration. Resources import requires no newInspector hookup.
- Verification:65unique latestPlayModePASS. Firstnographicsattempt crashed in
  oldURP Camera.Rendersealtest; Metalrun60PASS/5staletestfixtures, repaired4+1
  actualmanual20/20harvestapproachPASS. Finalspacing/type/copy-onlyv2compiled
  withoutredundantgameplayrerun. Nativev1RETURN HP-01bottleoverlap/02tiny nav/
 03grammar retained; all30v2frames720/900/1080 AstraPASS. Geometry/actualalpha
  provezerointersections; newlogs no attributable compiler/playerexceptions.
- FinalMac Builds/MacHudPolish/Not3AHudPolish.app,816359833bytes,oneordinaryPID27278;
  previous22538closed, QAautoquit. Allfirst8actualrendersready,nofreegrants. CUA
  actualmouseF2red/keyF3red/F4silent/B/Base30-40and20-20/PlayericonreturnPASS.
  Source/prompts:art/source/hud_polish_2026-10-23_generation.json. Evidence:
  art/source/hud_polish_2026-10-23_verification.json andnative_review.json.
  Remaining:Windows/Steam andcontinuousmanualfeel/sustainedbalance. Current
  requestedimplementation complete; nativePASSdoesnotinferowneracceptance.

### 2026-10-23 — D126 weapon reference-fidelity follow-up

- Main:ImageGen/provenance/build/capture/docs. Astra:weapon-only appearance/
  measured atlas import/review. Dependencies:actual equipment/ranks and retained
  thin pair/50%overlap/turn/Z. Owner returned flat-grey empty slot,flat switch
  and missing4active-only gems;v3 consolidates corrections to selected references.
- Original atlas supplies textured neutral disc,active/disabled shaped button
  and ivory keycap. Native text/gems remain separate;equipped quality skins and
  gameplay untouched. Provenance:art/source/weapon_fidelity_2026-10-23_generation.json.
- First15native review RETURN WF-01:lower active staff gem overlapped caption
  during turn. Astra reverses/reduces only vertical arc (active rises3logicalpx),
  retaining resting geometry/input.18native states including peak.536 at3sizes
  are ready/error-free;Astra reviewed all18 and PASSED,WF-01 closed at early/peak/
  end poses on all3sizes. No repeated gameplay tests for visual-only change;
  earlier20functionalPASS remain baseline.
- Build807952391bytes at Builds/MacHudCorrections/Not3AHudCorrections.app;
  one ordinary player PID20305,first8renders ready,noQA grants. Old17480/19511
  closed;QA autoquit. Resources PNG/meta and SkillDock/HeroLoadout changed;
  opt-in capture adds weapon-only/peak,ordinary runtime unaffected. No new scene/
  Inspector/package/save wiring. Evidence:art/source/weapon_fidelity_2026-10-23_verification.json.
  Final review:art/source/weapon_fidelity_2026-10-23_native_review.json.
  Remaining:Windows and continuous animation/manual balance;current requested
  implementation complete. Native PASS is scoped qualification,not owner acceptance.

### 2026-10-23 — D126 HUD correction and developer grants handoff

- Main owns debug progression/rules, selection text, tests/build/docs/ImageGen;
  Astra owns native tooltip/plate/debug-row fixes and any approved weapon block.
  Dependencies:D120 equipment/progression,D121 shared points,D124/D125 HUD/capstones.
- Implemented: no level plates/subtitle for nonupgradable barricade/workshop;
  wrapped hover measured and painted after normal panels; F10 +100/+1000XP,
  +1/+10levels using normal points/cap, invalid/open-menu/live-run checks, Reset.
  No ordinary-startup grants or free learned skills. Files:Stage2GameHud,
  FantasyHud/.Selection/.SkillDock,Binding.Selection,focused progression/interface
  tests and opt-in Stage3HudRefactorReview native evidence. Existing
  Binding.Progression→BindHeroProgressionTools wiring is reused; no new scene/
  Inspector/package/save migration or runtime bitmap asset is required.
- Owner explicitly supersedes prior preserve-instance preference: old players
  4243/11668/12461 were all closed. Leave exactly one latest build after verification.
- Owner accepts weapon direction with thin rims,50%overlap and short active-
  foreground turn; weapon_switch_v2 records it. Astra implements the two actual
  equipment medallions/shared footer. Generic footer9slice RETURN corrected;
  missing staff stays neutral and cannot switch. Pause/modal/hidden settles.
- Qualification:20unique latestPlayModePASS,0fail,1explicit batch viewportSKIP
  replaced by102ready native720/900/1080frames. Actual InputSystem capstone
  hover,normal Z transitions and6padded tooltip geometries pass. Native-v6 and
  final ordinary logs have no attributable compiler/player exceptions. Prior
  locked-host/hover/QA-role/selected-strip failures and footer RETURN are retained.
- Final Mac:Builds/MacHudCorrections/Not3AHudCorrections.app,799565960bytes.
  Exactly one ordinary player PID17480;first8renders ready,no QA grants,
  LVL1/XP0/5gold. CUA actual4presets pass in disposable run; final F10/scroll/
  visible4presets/return/empty-staff-Z smoke passes without grants. No commit/push.
  Astra native PASS covers36explicit frames (17v4+19v6), not all102. Final
  report:art/source/hud_corrections_2026-10-23_native_review.json. Remaining:
  Windows/Steam and continuous manual balance; current requested corrections
  are complete. Evidence:art/source/hud_corrections_2026-10-23_verification.json.

### 2026-10-23 — D124/D125 interface, input and specialization handoff

- Ownership/dependencies: main data/rules/input/bindings/tests/docs and ImageGen;
  Astra native visuals/review. D120 loadout, D121 shared points, D102/D122 local
  study gates, D079 paid upgrades and D104/D111 world combat are preserved.
- Implemented owner-approved first HUD: F1–F4 left/nav right, Player crossed
  bow/staff/crescent with Tab, categories below, max3known build cards, actual
  centred flowing prices. Unknown papers remain hidden until studied. Contextual
 1–3/1–5 have no conflict; pager arrows/wheel/swipe, paid Shift repeat and Alt
  one-upgrade/no-selection work. Silver LVL/green-red readiness and common paid
  upgrade light use the actual near footprint, freeze during pause/modal and clear
  on Reset/retry/loss. Shared tab/control/item hover/selection layout is native.
- D125: talent14 late via5 at50/54/58, learned ranks preserved; profession5/5/3,
  all five adventurer nodes5ranks. Two owner-selected paid one-rank capstones
  after StrengthV:10% mediumSTONEfinisher / one extra ordinary in-range tree,
  no chain/extra stock. Full cost156 versus99earned hero points. New world-only
  damage/resistance IDs34/35 retain IDs0–33 and fractional actual-hit rules.
- Contracts/files: Stage2BuildDriver/Economy/HeroSkills/Catalog/Professions,
  Stage3FantasyHud partials/Art/Importer, Binding.Selection/.Professions/
  .HeroLoadout, HeroSkillsDriver/GuildWorldDriver and Stage3 data resources.
  Original runtime atlas/skin/light and UI-only alpha bounds are documented in
  art/source/hud_refactor_2026-10-22_generation.json. Existing PNGs/pivots/D097
  quality skins, packages/scenes and GUIDs are preserved; prior D123 scene
  PortalEnemy reference remains. No global service or disk-save migration.
- Qualification:37EditMode +82unique latest PlayMode PASS,1explicit batch
  viewportSKIP replaced by native720/900/1080captures. Last5focused checks pass;
  final unavailable color-only fix compiled/captured without redundant gameplay
  rerun. Capture5ready69/error-free; Astra PASS on9new v5frames plus29unchanged
  v4frames, after retained RETURN corrections. Current logs have no attributable
  compiler/player exceptions; Unity licensing/Cloud symbol credentials are
  environment-only warnings. Empty-YAML whitespace normalization keeps semantics.
- Final Mac build:Builds/MacHudRefactorFinal/Not3AHudRefactorFinal.app,
 799549978bytes; ordinary first8renders ready/no grants (LVL1,5gold). Actual CUA
  I/Tab/B/1 and Player-icon click pass. Final PID12461 is running; earlier
 4243/11668 instances preserved. Evidence:art/source/hud_refactor_2026-10-22_verification.json
  and final native_review.json. No commit/push requested. Remaining:Windows/Steam,
  continuous VFX smoothness/manual full-run balance and unpictured combinations;
  no current requested implementation remains open.

### 2026-10-22 — D121–D123 progression, defense, research and portal corrections

- Ownership: main rules/data/binding/tests/docs; GPT-6 Astra existing UI text,
  action visibility, render order and native review. Dependencies: D120 hero
  points/skills, D079 defense investments, D102 paper ownership/study, D093
  generators, D106 stream and D087 current-progress retry. Scope is current Stage3.
- Behavior: ONE hero-point balance for all three O branches; manual mastery
  retains XP without minting points. Rapid/Longshot continue paid card upgrades
  beyond5 by their last authored increases; no barricade upgrades/current merge.
  Legal empty-cell preparation movement, ratios/refunds/retry remain. Upper clock
  draws after world badges. Four appended finite opening papers gate production3
  and forge construction at rules/draft entry, separate from purchase/study/build.
- Data/wiring: Stage3HeroProgression/ManualEconomy/current tower catalogs and
  Stage3Blueprints; new PortalEnemy.asset5HP uses existing prefab/art and replaces
  only the current scene's enemyCatalog reference. HordeRun first interval4s,
  later intervals retained, HP tier growth0 by owner clarification. Legacy Stage2
  enemy/economy/defense content stays intact. No package/global service/disk save.
- Acceptance/verification:105EditModePASS +99unique latest PlayModePASS. Covers
  shared spending across all three branches/reentry, extraction, paid levels8+,
  direct/drag merge rejection, retry/reset, trade/local study/knowledge gates and
  invalid production IDs. Actual five timed nights:8298confirmed kills/XP,
  LVL8,224earned gold (chance-based); mean forecast~249, no day farming/guild XP.
  Ideal target-following bow throughput passes first night; stationary-at-portal
  policy loses with5HP, so natural manual balance is not claimed.
- Mac build:Builds/MacProgressionRefactor/Not3AProgressionRefactor.app,
  774338804bytes.78latest native frames captured across720/900/1080, including
  deliberate clock/LVL overlap and ordinary first8renders all ready/no grants.
  Astra FINAL VISUAL PASS on33representative frames plus6caption replacements.
  Initial IronMine caption clipping fixed by «Жел. шахта»; full name preserved.
  Forge hover-detail was code-reviewed rather than separately captured.
- Initial no-graphics Camera.Render crash and two old fixture assumptions are
  retained in evidence; Metal rerun/latest-per-case results pass. No attributable
  final compiler/player exceptions; diff check PASS. New asset has metadata;
  source PNGs/prefabs/packages/settings unchanged. Scene migration is only the
  PortalEnemy catalog reference. Evidence:art/source/progression_refactor_2026-10-22_verification.json.
- Remaining:Windows/Steam, natural manual full-run playtest and later economy/
  difficulty tuning (owner explicitly deferred deeper balance). No current
  implementation work remains from D121–D123; no commit/push requested here.

### 2026-10-22 — D118–D120 repository save and push handoff

- Owner requests saving all accumulated work, refreshing documentation,
  committing and pushing the current main branch to origin/main. Main owns
  this repository snapshot; gameplay and visual runtime stay as qualified.
- Includes source/data, Unity .meta files, original concepts/runtime art,
  provenance/reviews and qualification summaries. Four new Unity metadata files
  have whitespace-only empty-YAML cleanup; GUIDs/import values stay unchanged.
  Original/normalized hashes are retained in the qualification summary.
  Plan, agent guidance and
  pending-decision notes now distinguish implemented D120 from D119 history.
- Verification: current runtime source hashes match the D120 evidence, all new
  Unity assets have metadata, package/settings/scenes are unchanged, and no
  file exceeds the remote size limit. Existing D118: 118 PASS + 2 documented
  skips/45 native frames; D120: 131 PASS + 1 headless viewport skip/80 native
  frames, Astra final PASS. No runtime changes require another test run here.
- Build outputs, Unity caches and detailed Artifacts stay local through the
  existing ignore rules. One earlier unpublished commit (1cc39d7) is included
  in the requested push. Remaining product/Windows/balance work is unchanged.

### 2026-10-22 — D120 hero loadout/progression integration

- Ownership: main rules/data/bindings/qualification/docs; Astra native art/layout/
  review. Dependencies: equipment ownership, finite merchant/blueprints/local
  study/forge, shared combat cadence, XP/vitals/recovery, guild rewards and input.
  Owner approved tree_v1/hud_v2/equipment_v2,5+5catalogue and controls.
- Behavior: Q+5bow+5staff contextual1–5, Z active weapon with both retained;
  mixed11large/20small prerequisite graph and3working adventurer reward talents.
  Moving automatic attacks are learned; aimed shot still stops. Bounded4-target
  fire/lightning25/30→15s.12gear slots, amulet/rings/no-MP «Защитная печать»;
  staff preserves Wand/WandRecipe. F1/F2/F3 actual bag potions/F4reserved/F10debug,
  owner100/100/250gold and100HP/100MP/100each,5stock/shared20sCD/no restock.
- Contracts/files: new plain Stage2HeroSkills/Catalog + Resources/Stage3HeroSkills,
  scene-owned Stage3HeroSkillsDriver, binding.HeroLoadout and Astra SkillDock/
  SkillTree/HeroLoadout/VFX. Equipment/catalog/trading/merchant append IDs/slots,
  default legacy behavior preserved; active vs fixed-slot lookup is explicit.
  Guild turn-in bonuses apply once; world-only fractional XP excludes portal.
  Original ImageGen PNGs/importers/measured bounds and painted quality skin;
  no scene/package/global-manager/disk-save migration or current world drops.
- Acceptance/verification: actual invalid-cast/points/mana/weapon/cadence/potion/
  death/retry/reset/quest paths checked; native720/900/1080HUD/tree/equipment/
  forge/merchant/heldstaff and both enemy-family hit feedback. Initial visual
  RETURNs (caption overlap, link crossings, duplicated details, mixed-potion
  clipping) are corrected.131distinctPlayModePASS+1headlessviewportSKIP
  (640×480Editor refuses720p);80finalMacframes720/900/1080 AstraFINALPASS.
  Physical CUA O/node/tab/I/F10 and F3-no-debug smoke PASS; no attributable
  compile/player errors, diff check PASS. Final test/build/recapture evidence is in
  art/source/hero_loadout_2026-10-22_verification.json. Build:
  Builds/MacHeroLoadout/Not3AHeroLoadout.app. No ordinary QA grants; prior user
  players/checkouts retained. Install/import: Stage3HeroLoadoutInstaller.Install.
- Ordinary final build startsLVL1/XP0/gold5/HP100/MP100/no potions or staff,
  no capture environment or grants; existing user players stay intact.
- Remaining: Windows/Steam, sustained combat/economy balance, full manual
  weapon/potion/research playthrough, future loot/additional accessory effects.
  Earlier D119 larger gathering/adventurer lists are proposals; no fake buttons.

### 2026-10-21 — D118 world UX/corrections; D119 progressive skill-tree review

- Main owns camp grouping/aggro, harvestable natural props, confirmed damage/
  grant facts, quest tracking, seal picking/range, placement/recovery rules,
  qualification and canonical docs. Astra field owns HP/numbers/impact/yield/O
  icon and unchanged-art depletion adapter; Astra quest owns map/minimap skin.
  Dependencies:existing GuildWorld/stock/mastery, shared HeroObstacleMap,
  existing travel network/native Lunar Porcelain shell and approved camp art.
- D118 behavior:both portal/world enemies share actual HP/damage feedback; actual
  material grants show+N.46 authored camp trees/rocks enter ordinary economy,
  deplete/recover without duplicate art/permanent colliders. Compact camp pack
  beside shelter/ruins alerts its generation together, preserving leash/rewards.
  Accept tracks a quest, clear targets guild, turn-in chooses next/no arrow.
  Seal rim opens existing explicit menu on actual painted ground plane. Empty
  future sites/depleted nodes/disabled zero-radius circles do not block base
  buildings; occupied solid ground holds regrowth. Clear existing-shell reasons.
- Files/contracts:Stage2Enemy/Group damage facts, Economy.ResourceGranted and
  optional scene-owned CanRecoverNode; GuildEnemy.Alert; GuildWorld(.Camps),
  CampPresentation/ResourceSprite adapter; ManualResourceCollisions; travel
  Network/Construction and dwelling/forge; Guild binding/Hud.GuildMap; new
  FieldFeedbackPresentation. No scene/Inspector migration, package or save
  format. Runtime reuses current data values; extra skills have not shipped.
- Verification:120 distinct latest outcomes,118 PASS+2 explicit native-IMGUI
  skips (PlayMode89 PASS+2 skips;EditMode29 PASS). Final field7/7 covers density
  at3 resolutions, injured priority and lifecycle. Fresh separate Mac build succeeds. Quest23 corrected native
  frames720/900/1080 Astra FINAL PASS; scripted production pointer handler PASS,
  real OS mouse/full-pan DEGRADED(CUA noWindowsAvailable). Initial native check
  pins/city overlap was RETURN, fixed and recaptured. Field 22 final native frames
  Astra FINAL PASS after maximumzoom adjacentHP merging RETURN and density
  correction (24..50px,4pxgaps, injuredpriority) plus actual 900p night close/wide/ogre.
  Evidence:art/source/world_ux_2026-10-21_verification.json;
  logs/captures:Artifacts/WORLD-UX-2026-10-21/. Original user game stays intact;
  separate Builds/MacWorldUX/Not3AWorldUX.app is used for disposable captures.
- D119:owner now wants 10 active/about 20 passive combat skills, fire+lightning
  splash with 25–30s CD down to 15s, learned movingfire underMobility. V2 grid and
  v3 left info/legend returned. Current v5 has ONE mixed prerequisite tree,
  large actives/small passives, no sidebar, compact LVL/XP/points header and full
  reclaimed working width. ImageGen concept awaits owner approval before full
  Astra page/skill implementation. HERO_PROGRESSION.md owns named proposal.
- Ordinary latest Mac process restarted without capture environment/fixture or debug
  grants; automatic simulation at baseline. No attributable final runtime/compile
  errors, git diff --check PASS; previous failed QA captures retained as RETURN.
- Remaining:Windows/Steam and physical mouse qualification; ownerv5 appearance, new skill
  rules/data/tree implementation and sustained balance. No invented current
  mana/crit/newloot/service or progression grants. No commit/push requested.

### 2026-10-21 — Shared prototype save / commit handoff

- Owner requests saving and committing all current work, including accumulated
  gates/cycle, equipment/forge/professions, trade/research, recovery/vitals,
  guild/world combat, starter travel, camp scenery and authored city corrections.
  Canonical plan/vision/agent guidance updated to current D117 status.
- Runtime remains the already verified build; this save adds documentation only.
  Latest relevant evidence:82/82 distinct PlayMode outcomes, final Mac build,
  camp/city Astra reviews and ordinary restart; Windows remains unverified.
- Source/assets/.meta/concepts/provenance are versioned together. Existing
  ignore rules keep build outputs, Unity caches and Artifacts local. No cleanup
  or deletion of prior work, no remote push requested in this save step.

### 2026-10-21 — D116/D117 camps and authored city -> Locally verified handoff

- Owner authorizes runtime orc/ogre environments from approved camp_v2 sketches.
  Main owns ImageGen, deterministic site reservation, obstacle/lifecycle
  integration, tests/native captures/docs. Astra owns new modular camp artwork,
  importer, presentation/layout and ground visuals. Other surfaces stay frozen.
- Dependencies:existing GuildWorld/catalog/quests/enemies, resource districts,
  manual generation, shared HeroObstacleMap, original approved camp_v2 art.
- Acceptance:large open battle ground, scenery concentrated on one side,
  at least three broad independent escape routes; generated resources never
  fill reserved ground; colliders follow substantial visible props, low details
  remain walkable. Existing acceptance/kill/turn-in/retry/reset work without
  population/reward tuning. Native720/900/1080 plus actual movement/combat.
- Owner also returned city arrangement/structure. Separate city_layout_astra
  owns CityPresentation only (Art/Importer if needed), preserves LocalBounds;
  fixed authored composition, no random filler. Camp Astra touches new files
  only; main integrates/captures both and coordinates one Unity process.
- Completed contracts/files:GuildWorldDriver.Camps reserves44×40 before manual
  generation; main integrates bounds/obstacles/relocation; CampArt/Presentation/
  Importer provide40/41 props and30/31 solids, no colliding lowflowers. City only
  changes authored CityPresentation layout/anchors; old bounds/scene preserved.
- Verification:77/77 initial +30/30 final focused;82 distinct latest PASS.
  New seed/retreat/collision/lifecycle checks and actual city entrance→services
  BFS at.35/.6 pass. Fresh import/build success,30native frames720/900/1080,
  camp/city Astra FINAL VISUAL PASS; actual A-key retreat and clean ordinary
  restart verified. First native was RETURN and corrected; no attributable final
  runtime/compile errors, git diff --check clean. Evidence:
  art/source/world_camps_2026-10-21_verification.json.
- Wiring:import Stage3CampArtImporter.BuildAssets; no scene/Inspector migration,
  package or new global manager. Ordinary Mac player left running at baseline.
- Remaining:Windows/Steam, night beyond900p, sustained balance and owner actual
  city appearance review. No new NPC/quest/type/currency/loot/save or package.

### 2026-10-21 — D113/D114/D115 corrections -> Locally verified handoff

- Main: ImageGen open camp proposals, integration, captures/tests/docs. Existing
  Astra: native clock/minimap/research/buildcard/base preview/avatar and city
  fidelity to city_v4_open_temple. guild_bow_fix: Guild binding, stationary shared
  bow, mover flag, regressions. starter_teleport_rules: Travel state/placement/
  cancellation/binding and regressions. User explicitly requested parallel work.
- Contracts: pinned workshop starter kind-1, local study -> manual one-circle
  construction; current free prototype, no automatic destination discovery.
  NearGuild uses actual GuildFootprint, no remote rewards. Ground movement blocks
  new bow launches while cooldown/previous arrows continue. Minimap no city rim
  cue, clock120px at900p with brighter source hand, same10:3. Original shop/guild
  art retained; denser natural city and public portal court, synced obstacles.
- Acceptance: real local study/build/route, invalid placement/modal/retry/reset
  checks; real guild accept click updates objective; WASD/backward/diagonal bow
  checks; native720/900/1080 clock/city/mini/research/build and ordinary restart.
  Camp v2 has broad open combat ground; owner approved both appearances.
  Runtime camp scenery is a later step, beyond the current sketch request.
- Verification:81 distinct latest PlayMode outcomes PASS; after final garden
  import/layout and four-card caption correction,37/37 relevant checks PASS.
  Fresh Mac build succeeds.28 native frames cover720/900/1080; Astra FINAL
  VISUAL PASS for city/reference, clock/hand, minimap, starter study and cards.
  Actual native mouse rejects hero footing, builds one circle at a valid site,
  opens guild by building click and accepts the order (Выполняется,0/3).
  Scene/Inspector wiring unchanged; Stage3CityArtImporter.BuildAssets updates the
  existing Resources asset with six garden sprites. No attributable runtime/
  compile error in final capture or ordinary process; git diff --check passes.
- Ordinary process restarted without any capture environment:LVL1/XP0/gold5/
  HP100/MP100, automatic simulation active, no modal/debug grants. M shows city
  from initialization. Evidence:art/source/iteration_2026-10-21_verification.json.
- Remaining:Windows/Steam, sustained play/balance, a separately observed green
  valid preview and later approved camp-environment implementation. No new
  gameplay price, item rank, service, package or save format.

### 2026-10-21 — City map visibility -> Locally verified handoff

- Owner refinement: remove «Город» and «Вы здесь» captions on M, keep icons
  and safe hero/city separation. Main capture/docs; Astra native draw removal.
  Verified: fresh Mac build,28 native frames and ordinary M; icons remain visible
  before discovery, no captions. Astra PASS6 overview/overlap frames720/900/1080.
- D112 corrects missing city navigation: independent City marker at actual
  CityOrigin, visible before research/discovery. M first fits the complete map,
  then retains user zoom/pan; the historical rim bearing is removed by D113 above.
  Only the local house remains on the minimap.
  Navigation does not grant discovery, research or teleport access.
- Main owns Stage2Minimap marker data, WorldTravel lifecycle, regressions and
  opt-in capture; Astra owns FantasyHud.CityMap and narrow Map/HUD hooks using
  approved city house art. Existing bindings/scene need no Inspector migration.
  Reset preserves the city; regeneration rebinds it; ClearCity removes it.
- Verification:29 distinct latest PlayMode checks pass (full29 + focused3 after
  initial projection correction).28 native frames at720/900/1080 cover overview,
  retained view, local/rim bearings, hero overlap and clear; ordinary Mac input M
  opens the map with city visible. No attributable runtime/compile errors.
  Evidence: art/source/city_map_2026-10-21_verification.json.
- Remaining: Windows/Steam verification. No new art, shortcut, package, gameplay
  unlock, scene migration or inventory/world-state grant.

### 2026-10-21 — Adventurers' guild / ranked contracts -> Locally verified handoff

- D110/D111 approved board_f_v1/trial_e_v1/city_v4_open_temple. Final owner
  correction: world board is SMALL, muted, decorative only, well right of the
  entry and parallel to its frontage. Guild building click opens the menu;
  GuildInteractionPosition is the free entrance anchor. Temple court stays open.
- Main owns Stage2Guild/catalog/enemy rules, Stage3GuildWorldDriver, binding,
  one shared hero bow targeting bridge, installer/tests/docs. Astra owns native
  Guild HUD/art/importers, City layout and enemy/arrow/map presentation. Existing
  XP/equipment/recovery/obstacles/travel reused; no scene migration or package.
- Repeat successes count; ACCEPT resets/refills its typed spot. CLEAR marks
  ready; TURN-IN at guild awards gold/XP/count/rank once. Three current-rank
  turn-ins unlock one trial; trial turn-in promotes. Seven ranks F…S, one active
  order per spot, two independent orc/ogre spots with provisional data profiles.
- Hero melee damage uses recovery/death API; shared bow chooses nearest portal/
  world target. World actors never enter horde counters. Sleep blocks during
  actual world combat. Current contracts/camps survive throne retry; full reset
  returns F/baseline camps. Guild closes/transient input clears as other windows.
- Verification:89distinct latest PlayMode outcomes PASS, including full F→S
  rules, repeated acceptance/turn-in, stale IDs, reward reentry/overflow, real
  scene click/range/modal gates, bow/melee/death, current-progress retry/reset,
  existing merchant/travel/research/progression/day gates. Headless portal
  RenderTexture limitation resolved by repeated PASS with graphics.
- Native720/900/1080: actual F repeats→trial→E, disabled/turn-in states, third
  paper scroll, both species front/rear/strike, map and real mid-flight arrow.
  Astra PASS all surfaces including final farther-right muted decorative board.
  Actual CUA: decoration no-op, guild door opens, F reward355→405gold and
  122→202XP, next E acceptance0/3. Source/evidence:
  art/source/guild_2026-10-21_generation.json and verification.json.
- Fresh ordinary Mac process restored LVL1/XP0/gold5/100HP/100MP, all automatic
  simulation enabled, no fixture grants. No new attributable console errors.
- Remaining: Windows, final balance/sustained playtest; more spots/environment kits/loot are later content.
  No disk persistence or extra NPC/monster kind. Existing dirty tree preserved.

### 2026-10-21 — Temple NPC quest window / separate world spots -> Superseded by D110

- Owner now requests the missing quest-selection window on clicking the temple
  mage. Main owns ImageGen/provenance/brief/docs; Astra owns visual direction
  and review. Existing fullscreen Lunar Porcelain UI and approved mage/enemy art
  are dependencies; no Unity/runtime edits in this step.
- Acceptance: clear available-quest list, selected objective/location and one
  accept action, readable fixed panel geometry matching current fullscreen UI.
  Show orc camp and ogre spot separately; no mixed encounter, goblin/undead
  assets or gameplay counts inferred. Exact rewards/respawn/quest rules open.
- Latest owner approval completes D108 character/placement appearance: mage v2,
  purple ogre v2, orc scale v5 and temple_courtyard_placement_v1 all approved.
- Delivered art/concepts/temple_quests_2026-10-21/window_v1.png. Astra CONCEPT
  PASS: equal separate cards, clear selected detail/goal/place/action, safe text
  and shared fullscreen style. Camp scenery/banners in preview are illustrative,
  not approved modular environment/faction assets. Prompt/source/hash/review:
  art/source/temple_quests_2026-10-21_generation.json. No code tests/build needed
  for concept-only work; Unity unchanged. Remaining: owner appearance approval,
  then ready quest/world-enemy rules before gameplay and native UI implementation.
- Owner returned v1 with «Нужна доработка»; asked for specific appearance/layout
  changes before generating another version. V1 is not approved; D108 remains
  approved. No runtime/Unity work started on the quest surface.

### 2026-10-21 — Temple quest NPC / world-enemy concepts -> Appearance approved

- Owner requests sketches of a new city temple mage/quest giver and orc/ogre
  alternatives. «Хорад/Хород» was dictation error, not an approved city name.
- Main owns ImageGen, own-project references/provenance and documentation;
  Astra owns visual brief/review. Concept files only, no Unity/runtime changes.
- Dependencies: approved city A_v6 temple court, hero C and painted tactical art.
  Acceptance: readable matching silhouettes, NPC at human scale, visibly distinct
  orc/ogre sizes beside the same hero; no portal/city redesign. Appearance and
  later quest/combat brief await owner review; no rewards/stats/factions inferred.
- Delivered temple_mage_v1 and orc_ogre_comparison_v1 in
  art/concepts/quest_npc_world_enemies_2026-10-21/. Astra CONCEPT PASS both:
  NPC outside seal, distinct enemy silhouettes/materials. Ogre drawn about1.4–1.5
  hero heights rather than illustrative1.7; runtime scale remains open. Small
  tactical strip is generated art, not native QA. Prompts/hashes/rights in
  art/source/quest_npc_world_enemies_2026-10-21_*.json. No code tests/build needed;
  Unity/scene/runtime unchanged. Remaining: owner review and later gameplay brief.
- Owner returned v1 for revision: larger orc, purple ogre; long-haired/bearded
  ceremonial temple mage with cross staff, not a travelling elder. V2 generated
  as temple_mage_v2 / orc_ogre_comparison_v2; v1 retained as history. Current
  appearance approval remains pending. Owner's publishing concern is Steam;
  official rules checked, no asserted blanket cross ban or Valve review approval.
- Astra V2 CONCEPT PASS both sheets: complete coherent silhouettes, clear temple
  identity, larger orc and matte purple ogre, free court/seal approach. V2 prompt
  file: art/source/quest_npc_world_enemies_2026-10-21_v2_prompts.json. PNG copies/
  hashes and JSON validated; no runtime testing applicable. Owner review remains.
- Owner approved mage v2 and purple ogre v2, requested only slightly smaller orc
  (D108). Separate temple_courtyard_placement_v1 now answers the placement question:
  existing D099 temple/portal retained; mage stands beside steps with free paths.
  Main owns ImageGen/provenance, Astra owns correction/review; approved characters
  preserved. Orc scale/placement review remains, no quest/UI/runtime work added.
- Final candidates: orc_ogre_comparison_v5 and temple_courtyard_placement_v1.
  Astra CONCEPT PASS both; intermediate orc scale mismatch corrected locally.
  Eight documented PNG source/copy/hash/dimension checks and five prompt JSONs
  validated; diff check clean. Mage v2 / ogre v2 owner approval recorded in D108;
  only final orc size and separate placement await owner review. No Unity changes.
- Owner final reply «Утверждаю размещение и размер орка» approves both pending
  candidates, completing D108 appearance. Next surface is the NPC quest window
  above; these review sheets do not implement characters, encounters or quests.

### 2026-10-21 — D-104 hero death/recovery -> Locally verified handoff

- Owner approved final world/map/compact-tooltip concepts, label «Выпавшие вещи».
  Main owns plainRecovery/driver, bag instance transfers, progression penalty,
  binding/E/debug/test contracts; Astra owns native art/map/minimap/tooltip.
- Death losesfloor(currentlevelXP/2), keeps levels/points/ranks/cooldown and all
  equipped gear/tools/knowledge/wallet. Immediate safe base-anchor respawn fills
  HP/MP without unlocking travel. Bag gear/documents retain same IDs/instances.
  All nonempty drops persist until pickup, including further deaths/throne retry;
  empty bag creates no marker; fullReset clears. Fresh exclusive E pickup prevents
  same-frame harvesting/teleport/sleep/gate activation. Portal creeps never hurt hero.
- Existing Stage3binding opts in after progression/equipment/blueprints/travel;
  no scene migration. Existing F3 adds death test without grants. New ArtSO/importer
  uses generated source byte-identically; .meta retained, no new package/manager.
- Verification:75shared PlayMode checks PASS with D107, including10new recovery
  cases and inventory/trade/research/progression regressions. Astra visual PASS
  all34nativeframes at720/900/1080,8bearings, world/E/map/minimap/debug. Actual
  native E removes markers and I confirms helmet returned; same-instance rules
  additionally tested. Repaired orthographic negativeZ arrow flip and overlapping
  hero/map markers before final qualification. Evidence:
  art/source/death_recovery_2026-10-20_verification.json.
- Remaining: Windows and future exploration-enemy damage-source integration.
  API/F3 available now; no new enemies added. Mouse clicks not claimed.

### 2026-10-21 — D-107 top day/night dial and shaped upper HUD -> Locally verified handoff

- Owner approved thinshapedv2upperHUD and chose A smoke-glass after10:3ratio
  correction. Main owns readonly Run.DayNightCycleProgress/delegate binding,
  optional legacyF3hitrect/test/capture/docs; Astra owns native HUD/art/importer.
- Whole centered dial48/60/72px at720/900/1080, thinflushcornice with openworld
  gaps, compact resourcesleft/day+F3right, noHH:MM. Abstract soft day/night fade,
  no scenery/pie seam. Exact day10/13/night3/13; dawnat3clock and uniform13minute
  clockwise hand follows pause/sleep/launch/retry/reset, no independent HUDtimer.
- Shader rotates the separate generated hand; bounded textures/material released.
  SourcePNG byte-identical; importer measures alpha islands/pivot, no runtime scan.
  Existing UIstack retained, modal nav/trade-onlygold unchanged. F3 hit rectangle
  follows visible button through optional binding, legacy fallback preserved.
- Verification:36EditMode+75sharedPlayMode PASS; Astra PASS all25nativephase/UI
  frames, no clipping/color fringe at actual size, correct hand/phase at night
  entry/midnight/dawn. Ordinary Mac startup restored x1/WTFoff, native F3 toggles
  debug, then closed. Evidence:art/source/cycle_top_hud_2026-10-21_verification.json.
- Remaining: Windows and owner balance feedback; coordinate mouse clicks not
  claimed. No commit/push requested. Existing dirty workspace preserved.

### 2026-10-21 — D-106 10-minute day / 3-minute streaming night -> Locally verified handoff

- Main owns Run/catalog/HordeRun/builders/tests/docs; Astra preserves D107 visual
  binding. Day600active seconds, night180; stream until dawn regardless oldcount.
  Live creeps/shots remain and tier clears only after dawn+allthreats. No stacked
  mandatory nights, last-WON dayfarm/cutoff19:30/sleep19:50/progressretry retained.
- Optional nightDurationSeconds=0 preserves finitelegacyStage2. HordeRun and
  builders explicitly set600/180; no scene/package migration. Existing profile
  intervals/HP retained, so values remain trial balance; old count only forecast.
- Verification:36EditMode+26PlayMode PASS including pause/retry/large clocksteps,
  cutoff/localE/sleep, firstnight solo bow and tiers2/3 boundedpaiddefenses. D107
  nativephase fixture confirms dawn retains live threats. Combined D104/D106/D107
  union137distinctchecks PASS,59nativeframes AstraPASS. Mac build694646886bytes,
  clean final native logs/ordinary startup. Evidence:
  art/source/timed_nights_2026-10-20_verification.json.
- Remaining: Windows, natural fullcycle/sustained later-tier load and owner balance
  playtest. No new creep types or tower stats invented.

### 2026-10-20 — D-105 unbounded last-cleared daytime flow -> Locally verified handoff

- Main owns Run cadence/stream logic, first-horde data/builder, relevant
  rules/scene integration checks and canonical docs. Existing scene/art/approved
  UI remain. Dependencies: D074 last-cleared gates/cutoff, D087 current-progress
  retry, D103 explicit debug launch and existing night profile data.
- Owner clarification: repeat last WON tier after retry, never the failed next
  night. Daytime copies difficulty (interval/HP/current type), not night count;
  it continues until manual close or19:30. Initial gates locked before first
  victory; day kills never clear/promote pending night. First horde buff delegated.
- Acceptance: reproduce clears/failure/retry at authored tiers2/3, same cleared
  HP/timing/hint, keep spawning beyond2xnight count and beyond obsolete24-live
  cap; local E close/cutoff retain living threats; reopening keeps cadence;
  retry keeps current world/XP/economy, Reset clears access. Finite legacy
  Stage2 and instant finite profiles unaffected. No art/scene migration.
- Contracts/files: Stage2Run removes count throttle and reads cleared-profile
  cadence; RunCatalog retains obsolete serialized fields and validates positive
  flow intervals. HordeRun first profile/builder buff; existing gate hint stays
  LastClearedTier. First-night test/QA helpers follow actual spawn completion.
- Verification:59distinct checks passed (31EditMode,28PlayMode, latest-case
  merge). Actual InputTestFixture E verifies failed-night retry and endless
  cleared-tier stream/close; solo bow and paid-defense balance checks pass.
  Mac development build681995748bytes; ordinary standalone startup/F3 keyboard
  smoke passes, no attributable console errors, game left running without QA
  flags/grants/debug. Evidence:art/source/daytime_flow_2026-10-20_verification.json.
- Remaining: Windows qualification, owner balance/sustained-load playtest and
  future creep types. Standalone manual full-cycle playthrough not claimed.
  Old combat fixture now buys/studies D102 Longshot with exact-price fixture
  funding, asserting unchanged defense gold budget; production gates unchanged.

### 2026-10-20 — D-103 vitals/Blink/debug wave/zoom -> Locally verified handoff

- Main owns existing HeroProgression rules/catalog/driver binding, explicit
  Run test-launch and development HUD command, camera limits/data, tests/docs.
  Astra owns FantasyHud Dock/Hero/Progression partials for real HP/MP and Blink
  rank/tooltip numeric facts, preserving existing approved art/layout.
- Dependencies: existing opt-in Stage3 progression, run clock/daytime gates,
  world camera and numeric HUD. Accepted100HP/100MP,5MP/s,4rank costs/CD.
- Acceptance: failed cast spends nothing; successful cast spends proper mana/CD,
  active-time regen/cap and retry/reset; first-day F3 starts pending night once,
  no skipped income and living threats retained; wheel/pinch clamp15 consistently;
  numeric HUD/rank values legible and actual controls work in local player.
- WTF added on explicit owner request: no MP/CD (zero, not0.5s), with original
  footprint/pause/modal checks and no XP/rank grants; Reset/HUD disable clears.
  Scene migration: planarEdgeClearance1→1.5 only, via Unity installer, preserving
  full hero at max zoom and ground frustum inside world; builders retain it.
- Contracts/files: HeroProgression/catalog data owns vitals and per-rank costs;
  explicit Run.StartNextWaveForTesting changes only test-clock semantics;
  Stage2GameHud binds one progression owner for development WTF; narrow binding
  supplies numeric DTO. Camera settings store multiplier3, Inspector exposes it.
- Verification:27EditMode+27PlayMode pass, Mac development build681995703bytes,
 26native frames Astra visual PASS. Actual Q consumes100MP/starts5s, F3 opens.
  Automated same-HUD-command native fixture verifies2zero-mana WTF casts and
  first-day launch. Mouse qualification DEGRADED: computer-use coordinate clicks
  fail with noWindowsAvailable; do not claim actual button clicks passed. No new
  attributable console errors; ordinary launch restored with WTFoff/full vitals
  and advancing clock. Evidence: art/source/hero_vitals_2026-10-20_verification.json.
- Remaining: Windows qualification and ordinary mouse acceptance; HUD-safe
  hero visibility at extreme map corners remains separate (strict viewport test
  passes, existing timecard/dock can overlay). No hero damage/death added here.
- D104 is accepted NEXT work: exploration-only hero damage, base respawn,
  half current-level XP, recoverable bag only, world/map/body indicator. Owner
  explicitly deferred implementation to next separate step with marker concept.

### 2026-10-20 — D-102 blueprint trade/study -> Locally verified handoff

- Scope: owner requests buyable/studiable documents and permanently single-level
  workshop. Explicitly accepted first4: Longshot construction, wand recipe,
  forgeII/III. No new content type, equipment ranks on blueprints or workshop ap.
- Main owns Stage2BlueprintCatalog/Blueprints, narrow Equipment/merchant rule
  extensions, Defense availability guard, research/trade/equipment bindings,
  workshop interaction, installer, focused tests and canonical docs/provenance.
  Astra owns blueprint art/import/Resources and FantasyHud DTO/renderers/pages;
  main does not edit those surfaces while Astra works. D101 item art, new research
  screen and known/history concepts approved; Astra native implementation ready.
- Contract: local one-paper study records knowledge, forgeIII depends on studied
  forgeII, no automatic building upgrade/placement, no double scroll spending.
  Current-world paper/knowledge/trades survive retry; Reset clears/restocks with
  monotonic shared identities. Old free teleport study remains. Trial prices in
  runtime data, fixed grid geometry, no blueprint quality/LVL, no new UI stack.
- Observable gate: buy/sell same instances atomically; study cannot bypass
  ownership/locality/mode/prerequisite; actual Longshot/wand/forge actions enforce
  access. UI research ready/missing/known/far/wrong-order, inventory and trade at
 720/900/1080; focused rules/PlayMode and native review.
- Verification:83distinct checks pass (rules/trade/forge/research/travel/build/
  selection/defeat). Mac build681976675bytes;18native frames Astra visual PASS.
  Actual mouse merchant click/buy200/sell100 and E→ForgeII→Study consume/history
  pass; no attributable runtime errors. Scene unchanged: existing opt-in binding
  loads Resources/Stage3Blueprints + Stage3BlueprintArt; installer imports both.
  Ordinary launch restored without QA grants/frozen clock. Detailed evidence:
  art/source/research_blueprints_2026-10-20_verification.json.
- Remaining: Windows qualification, trial economy tuning and future documents/
  drops/restock, additional content/research remain separate work. No disk-save
  format, workshop level-up or blanket prerequisite for all upgrades was added.

### 2026-10-20 — D-101 rectangular blueprint -> Appearance approved, implementation brief next

- Behavior/direction: owner asks for a common medieval magical fantasy BLUEPRINT
  item/icon, distinct from rolled spell scroll, with two examples first. Future
  research/new towers/weapons and upgrades of workshop/forge/other buildings may
  use derivatives. Possible magic-acquisition scrolls remain a separate idea.
- Owners/dependencies: main ImageGen/provenance/art-only QA/docs; GPT-6 Astra
  visual direction and concept review. Uses cartoonB painted volume, ivory/plum
  material palette and existing visible-bounds safe-fit principle. Source surface:
  art/concepts/research_blueprints_2026-10-20 and art/source records; RESEARCH,
  EQUIPMENT_AND_RUNES, ART_DIRECTION, vision/plan/D101/AGENTS updated together.
- Historical result (owner rejected): A_v2 broad creased single sheet/folded corner; B_v2 connected3panel
  unfolded plan/subdued brass clips. Engineering ink diagrams plus restrained
  cyan magic, no rolled ends/modern blue paper/rank/LVL/external quality frame.
  Both original1254RGBA PNGs with true alpha; workspace/original byte-identical.
  Exact original/edit prompts, source/rights/hash and bounds in
  art/source/research_blueprints_2026-10-20_generation.json. Earlier v1 retained
  as framing history. ImageGen v2 scales whole items without manual pixel edits.
- Historical context revision (owner rejected): owner deferred A/B choice until seeing actual inventory/
  sale context. A_shop_v2/B_shop_v2 are ImageGen edits of the D100 native merchant
  capture, each with one selected row2/col3 blueprint and a large central detail.
  Empty player bag, existing8equipment and gold-only purchase context remain;
  blueprint has no rank/LVL/weapon stats/equipment-quality frame. Narrow v2 edits
  reduce small icons and B's central preview for safe margins/comparable scale.
  Price100 and QA wallet10005 are illustrative only. ImageGen re-renders some UI/
  equipment details: these concepts are not pixel-identical runtime captures or
  proof that blueprint trade exists. No new Unity asset/stock/transaction added.
  Astra final CONCEPT PASS both shop_v2: silhouettes readable in ordinary cells,
  safe margins, preserved proportions and no equipment attributes. All1276
  protected runtime files rechecked unchanged after the context revision.
- Historical white-sheet attempt (owner rejected): owner rejects both tilted/unfolded shapes and asks for an
  ordinary upright WHITE sheet. Visual types must distinguish tower unlock and
  building construction, with class/level/category information. Main prepares
  one new ImageGen merchant-context example using current native UI and owned
  D097 skin reference; Astra directs/reviews the white-sheet category treatment.
  Schematics/category marks separate uses; class/LVL stay readable outside paper,
  quality belongs to the cell while paper stays white. C/D, LVL10/5 and100gold are
  illustrative only; no numerical blueprint mechanics or new item stock approved.
- Historical correction: owner also rejects modern white-sheet drawing and C/D,
  LVL10/5 interpretation. Reuse earlier light paper texture/drafting pattern in a
  straight rectangle fitted to fixed cells. No equipment grades/independent item
  levels on blueprints: an upgrade number means target BUILDING level. Examples
  4–5/forgeIV do not extend D090's current3stages. Separate unlock/construction/
  upgrade categories; actual marking remains unapproved. Every cell, row pitch
  and metadata area is fixed; icons/labels never reflow/squeeze cells below.
  White v1/v2 retained as rejected history; no final visual PASS/owner approval.
  That generation stop was temporary; the owner then resumed with explicit
  SAME aged-paper colour/style as A_v2. Main now prepares a rectangular variant
  using A_v2 material and native D100 UI references; Astra proposes/reviews cyan
  spark versus muted brass chevron in the same spot for the two primary uses:
  opening new content and upgrading existing content. Four blueprint-only
  illustrative slots compare tower/wand openings and forge-II/III; no quality
  ranks/LVL/skins, one fixed metadata line and equal regular6×5grid. Actual shop
  equipment stock, imports, forge counters/rules and current game stay untouched.
- Current result: rect_categories_shop_v3 uses the original aged paper style in
  upright rectangles. v2 repaired equal neutral cells/safe insets/short captions
  (Astra geometry PASS); v3 replaces copied ring devices with one recognisable
  forge furnace/chimney/anvil drawing forII/III and central preview. Spark versus
  chevron and target numerals remain; no quality ranks/LVL/skins. Astra final
  CONCEPT PASS v3: warm original style, rectangular shell, recognisable forge,
  equal cells/safe margins and correct category/target-stage semantics. Owner
  approved rect_categories_shop_v3 with «апрув» on2026-10-20. Appearance gate is
  complete: shared rectangular aged-paper shell, spark versus chevron, target
  II/III only, fixed cells. Catalogue/100gold/source/usage are still illustrative.
  All1276protectedruntime files unchanged; no Unity
  tests/build needed/rerun, no runtime integration or wiring in this slice.
- Historical verification: Astra CONCEPT PASS both v2 plus native browser256/96/64px proof
  on ivory/plum, visible-bounds uniform fit76%cell (12%inset). A recommended for
  square cells; B side diagrams become texture at64px. No visible coloured
  fringe in normal composition. B source still has6.7%side margin instead of
  requested14%; do not claim raw-margin compliance. Safe cell composition uses
  measured visible bounds. Original images are concepts, not runtime-ready assets.
  All1276 Assets/Packages/ProjectSettings files match pre-work SHA256; no Unity
  test/build needed or rerun, no scene/Inspector/import/package/asmdef/running
  game changes. QA page/capture/baseline remain ignored Artifacts.
- Remaining: standalone runtime art/import/safe-fit and ready
  item/source/price/usage/consumption brief. Same appearance needs no reapproval.
  No new stock/unlock catalogue or
  blanket blueprint gate for all current paid upgrades. Existing generic forge
  scroll-counter naming is not silently migrated by concept preparation.

### 2026-10-20 — D-100 merchant buy/sell -> Runtime handoff

- Behavior: clicking the existing city shop opens the approved fullscreen dual
  inventory. Player bag excludes equipped instances; select either side for one
  live-price Buy/Sell action. Only gold is shown/spent/credited. Initial stock is
  8 existing types, F/LVL 1, one each; owner accepted 50% sale and no auto restock.
  Same item/ID/type/quality/level transfers; no automatic equip or duplicated loot.
  Sold equipment remains finite merchant stock for ordinary repurchase.
- Ownership/dependencies: main owns Stage2Merchant/Catalog, narrow partial
  Equipment.Trading, PlayerInterfaceBinding.Merchant, installer, opt-in QA/tests/
  docs. GPT-6 Astra owns native FantasyHud.Merchant and narrow HUD hooks/review.
  Uses D099 city shop, D090/D091 instances, existing gold wallet and D097 actual
  painted skins/64visible bounds. No new PNG, scene/prefab/package/asmdef or global
  service. Existing hero portrait/city merchant head/scales are reused with masks.
- Contracts: inspect by visible shop click; transaction requires local proximity
  and live service, correct selected side/ID, owned unequipped item, current price,
  funds/credit capacity, no pause/loss/debug/draft/other modal. Failures preserve
  items/gold/materials; wallet observers see completed ownership and balance, with
  nested item transactions blocked. Modal blocks hero/build input without freezing
  the world. Esc/navigation closes and requires release before a fresh shop click.
  Reopen/D087 retry preserve current stock/trades/wallet; full Reset restores8F1
  and starter loadout without recycling IDs. No disk-save format introduced.
- Wiring: Stage3MerchantInstaller.Install creates validated Resources/Stage3Merchant
  data. Ordered8type prices/quality multipliers/level growth/local radius are trial
  data; sale50% is accepted. Existing binding Start/Update/OnDestroy owns the service;
  Merchant enum appended, serialized old names/IDs/art references preserved.
- Verification:41/41 focused PlayMode in 76.960s (10 new rules/integration plus
  equipment/forge/travel/startup regression); final Mac/Metal development build
  675630467 bytes. 21 native frames at720/900/1080 cover buy/sell/unfunded/far/empty
  and both scroll grids; Astra final visual PASS all21. Actual final mouse shop
  click, buy20/sell10,105→85→95gold, same item moves and Esc returns to city;
  earlier actual reopen also retained changed stock. No new attributable errors.
  First retry test lacked a night checkpoint; fixture fixed, no retry rule change.
  First native portrait corners/720p scrollbar/cut rows repaired before final PASS.
  Normal player restored5gold/0materials, running clock and empty bag/three equipped
  F1starters, no QA grant or frozen clock. Captures/logs/XML stay ignored Artifacts.
  Evidence/rights/reused assets: art/source/merchant_2026-10-20_verification.json;
  art/source/merchant_window_2026-10-20_generation.json. CITY_AND_TRADE owns rules.
- Remaining: Windows/Steam qualification (module unavailable), price playtest,
  owner response to native appearance; research/forge scroll sources/stock, runes,
  resource trade, restock systems and NPC budget/quests remain separate briefs.
  No new content family/currency/loot source or armor/world-appearance change.

### 2026-10-20 — D-099 city and shared teleport menu -> Runtime handoff

- Behavior: approved A_v6 modular orange-roof city, cobalt shop with turquoise
  contour/painted scales badge, ambient residents and a modest left-side temple.
  The sole remote seal is in its quiet walkable court. Nearby click/E opens the
  same opened-destination menu at every point; selection plus confirmation
  replaces the old instant counterpart hop. Current point is disabled, close
  cancels, blocked/unknown targets fail without movement/payment, arrival needs
  fresh input. Research, personal discovery and D087 current-progress retry stay.
- Ownership/dependencies: main owns ImageGen/provenance, plain travel rules,
  world integration, binding/tests/docs; GPT-6 Astra owns all city/UI appearance
  and visual review. Depends on approved A_v6/menu_v1, ground seal v4, existing
  Lunar Porcelain shell, HeroObstacleMap and resource/placement clearances.
  Observable acceptance covers free arrival/approach, city collision, no nodes
  inside streets, click/E/current/close/held input and reset/retry; additional
  test points use the same menu without inventing current product towns.
- Contracts/files: Stage3TravelState registers IDs/openings/snapshots; WorldTravel
  Network/City partials bind shared routing, city footprints and point events.
  PlayerInterfaceBinding.Teleports mediates validated modal confirmation;
  FantasyHud.Teleports is native existing-stack UI, not a flat concept screenshot.
  Stage3CityArt/Presentation use five original ImageGen PNG sources, isolated
  sprites/shared materials, tiled curved streets, green pockets and existing
  garden trees. Manual resources/grass/flowers and new workshop/dwelling/forge
  placement exclude city bounds. Reset keeps physical city but clears openings;
  D087 retry preserves current research/discovery and closes unfinished UI.
- Wiring/migration: run Stage3CityTravelInstaller.Install (art importer plus
  SaveConfiguration) to reproduce Resources/Stage3CityArt and serialize additive
  WorldTravelData.cityOrigin/empty additionalPoints. Existing art links/offsets,
  scenes, prefabs, packages, asmdefs and project settings are preserved. Initial
  registry has only base/city; legacy worldCirclePosition stays a fallback.
- Verification:16/16 focused PlayMode in48.665s, including configured third
  endpoint rendering/menu, unknown/blocked/current targets, research, modes,
  retry/reset/regeneration, collision and startup. Final Mac/Metal development
  build675598347bytes succeeded;16 city/menu frames at720/900/1080p, plus actual
  mouse city→base→city, E opening, Esc cancellation and disabled-current click.
  First grid paving, terrain holes and preview crop/frame were repaired before
  final native review. Early capture setup paused its own run; QA now explicitly
  resets/unpauses after freezing the clock, without weakening gameplay gates.
  No new attributable console errors. Normal player restored with running clock,
  5gold/0materials, no QA workshop/openings or grants. Screenshots/logs/XML remain
  ignored Artifacts, not duplicated into runtime. Astra final visual PASS on all16frames plus actual city arrival; evidence:
  art/source/city_runtime_2026-10-20_verification.json; source/prompts/rights:
  art/source/city_runtime_2026-10-20_generation.json.
- Remaining: Windows/Steam qualification (module unavailable), owner response
  to native appearance, merchant interaction/stock/prices/buy-sell transactions
  as a separate ready slice. No trade source, temple service, new currency,
  third current city, extra combat/quest/NPC AI or disk-save system added.

### 2026-10-20 — D-097 item safe areas and visible centring qualified

- Owner reports bow touching the frame and helmet/arms/legs vertically displaced.
  Scope: all 8 item types × 8 ranks and every existing item UI surface, preserving
  the approved painted skins, source icons and world-held sprite registration.
- Astra owns shared UI fitting, baked visible bounds in existing ForgeArt plus
  narrow importer, Equipment/Quality/Forge/Professions/Dock. Main owns opt-in
  Stage3ItemIconSafeAreaReview/InterfaceCapture QA, checks/build/native/docs.
- Dependencies: actual atlas alpha bounds (axe uses two allowed bands), current
  source skins and scroll/input contracts. Acceptance: proportional inset from
  frame ornaments, actual visible bounds centred without distortion/clipping,
  all 64 combinations plus actual windows at720/900/1080p. No pixel scanning or
  texture allocation per frame; no new appearance proposal/gameplay scope.
- Result/handoff: complete source connected-component bounds replace unequal
  tile padding, with foreign-silhouette validation64/64 and fallback for legacy
  data; original PNGs and held pivots unchanged. Equipment/Quality/Forge share
  centred proportional fitting; masked axe destinations share rounded edges.
  Initial tile-bound audit exposed neighbours/clipped own edges; corrected bake
  fixed these. Final1080 axe seam fixed separately. Main's opt-in QA calls exact
  product cell renderer for64DTOs plus existing real-window fixture, no new
  product controls/grants. No scene/Inspector/package migration; rebake ForgeArt
  using Stage3ForgeArtImporter.BakeUiIconBounds.
- Verification:17/17PlayMode after safe fit, before final Editor/pixel corrections;
  final bake64/64/build644077799bytes successful.33native frames at3sizes plus
  actual mouse helmet/arms/legs/equip/forge scroll and large Aaxe after5paid
  reforges. No new attributable errors, normal player restored without QA grants.
  Final Astra visual PASS all33frames plus largeAaxe, no blockers; evidence in
  art/source/item_safe_areas_2026-10-20_verification.json.
  Remaining: Windows, owner response to repaired appearance. City D098 stays
  separate and concept-only pending owner approval.

### 2026-10-20 — D-098 city service concept; portal placement superseded by D099

- Owner adds colored interactive contours and small painted role badges to
  local city services, initially only the merchant; city also contains the
  remote endpoint of the existing paired ground seal. Main owns ImageGen/docs;
  Astra owns composition/review and later approved Unity appearance.
- Dependencies: selected A_v3 cobalt-roof shop and approved teleport seal v4,
  existing research/discovery travel contract. No extra service, third portal,
  merchant stock or new currency. Preserve current runtime world placement.
- Acceptance: clear clickable shop contour, one beautiful compact colored role
  icon with roof/sign safe spacing, same ground seal in a clear city approach/
  arrival area; original style/composition retained. Save/show reviewed concept
  before visual implementation. Item safe-area repair remains active separately.
- Result/handoff: ImageGen A_v5 preserves A_v3 architecture and adds one thin
  turquoise shop outline, compact plum/gold scales badge above roof and one flat
  paired seal off the gate axis. A_v4 oversized badge/seal retained as history;
  final A_v5 Astra concept PASS, no blockers. Prompt/output/reference hashes and
  rights in art/source/city_2026-10-20_generation.json. CITY_AND_TRADE,
  WORLD_TELEPORTS, ART_DIRECTION/DECISIONS/plan record D-098. No scene/runtime
  city, merchant transaction or WorldTravelData changed; no gameplay test needed
  for this concept-only surface. Remaining: owner appearance approval, modular
  runtime assets/site/collision/arrival/interaction qualification and a ready
  trade stock/pricing brief; existing paired-circle research/discovery retained.
  Owner's D099 rejects A_v5 square placement and replaces pair-only product
  target with a shared opened-destination menu; latest result is above.

### 2026-10-20 — D-097 quality-cell reference fidelity correction qualified

- Owner rejected the shipped appearance: flat opaque dark fills and thick gold
  bars do not match the approved all_equipment_qualities_v2 sheet. Prior local
  checks establish functioning input/clipping only, not owner appearance approval.
- Main owns source inspection, ImageGen asset preparation/provenance and native
  qualification. Astra owns visual matching and the common quality renderer plus
  Equipment/Forge/Professions/Dock/import wiring. Dependencies: approved original
  raster, existing item icon atlases and preserved scroll/selection contracts.
- Acceptance: reference-like multitone gradients, texture, thin shaped luminous
  frames and rich S/SSS materials in actual windows at720/900/1080p. Preserve item
  actions, rank colors, clipping and bounded texture lifetime. No gameplay or
  merchant/city scope change. The existing approved reference authorizes this
  repair; a new concept/appearance decision is not required.
- Result: eight painted skins prepared by ImageGen editing the approved original,
  Astra asset PASS. Imported PNG byte-identical, measured UVs and full textured
  centres/thin frame patches now replace procedural plates/rails/stars. One
  borrowed Resources atlas, no per-item/per-frame texture allocation. Narrow
  importer only; scene/Inspector/packages/input geometry unchanged.
- Verification: import/build successful, 17/17 PlayMode checks after correction.
  Mac build644070019bytes; Astra strict-reference PASS all27native frames at720/
  900/1080p, no seams/cut ornaments/overlap. Actual mouse equip/forge scroll pass,
  ordinary game restored without QA grants, no new attributable console errors.
  Tiny HUD skins lose some engraving detail but retain rank/light. Windows and
  final owner response remain open. Source/prompt/rights:
  art/source/item_quality_cells_2026-10-20_skins_generation.json; qualification:
  art/source/item_quality_cells_2026-10-20_verification.json.

### 2026-10-20 — D-097 initial implementation; appearance rejected by owner

- Owner requires colored background/border for quality across all item UI, not
  just merchant. Main owns brief/ImageGen/provenance; Astra owns visual tokens
  and approved shared UI treatment. No gameplay balance or world art change.
- Scope:8ranks, bag/loadout/forge/actual tool and weapon previews; rank letters/LVL
  remain, selected cyan separate from persistent quality color. Neutral empty/
  generic cells; future trading reuses the same renderer. The final64-cell board
  is approved; corrected merchant concepts/runtime remain a separate D096 surface.
- Owner approved final64-cell all_equipment_qualities_v2 after rejecting weakD/C
  and mutedgold. FinalDgreen/Cblue, polishedS/radiantsharpSSS. Astra owns HUD
  Equipment/Forge/Professions/Dock +sharedQuality partial and narrowDestroycleanup;
  main owns opt-in Stage3ItemQualityReview/InterfaceCapture, checks/build/docs.
  Existing itematlases retained; no gameplay/scene/package/stock change. Acceptance:
  consistent frames across actual itemviews, visible quality while selected,
  readable720/900/1080p, boundedcachedtextures, unchanged equip/craft/profession
  behavior, no newerrors; nativeAstrareview then normalgame restored.
- Runtime implemented by Astra; final Mac build637778927bytes after initial
  native review found fractional frame seams, escaped custom scroll draws and
  weakSSSradiance. Pixel-rounded parts, explicit UV/geometry clipping and a shared
  SSSglint fixed these. Cache bound8×128²+64²RGBA32 with HUD cleanup.17PlayMode
  regressions passed before final visual-only repair; final build compiles.
  Main native actual selection/equip/forge scroll pass. Astra visual PASS all
  27 final frames at720/900/1080p and two extra forge scroll frames, no blockers.
  Ordinary game restored without QA grants. Windows remains unverified.
  Evidence: art/source/item_quality_cells_2026-10-20_verification.json.

### 2026-10-20 — D-096 merchant window concept active

- Owner approved city A_v3 and requested the merchant window. Main owns brief,
  generation/provenance and docs; Astra owns composition/review and later approved
  visual implementation. Current surface: concept files, no Unity edits yet.
- Dependencies: existing fullscreen inventory/forge Lunar Porcelain shell, approved
  merchant/scales shop identity, shared gold and planned research-item purchase
  route. Stock/prices/sellback/refresh are not fixed by this visual request.
- Acceptance: readable fullscreen catalogue/selection, owned gold, item detail,
  explicit purchase price/action and close/back; original ImageGen proposal
  saved/reviewed/shown before implementation. Preserve uncommitted runtime work.
- Owner rejected v1's portrait/single-product storefront. Active V2 requires
  simultaneous player/merchant inventory grids, compact identities and contextual
  buy/sell. Main visually inspected BG3/DOS2 trade references; no external image
  downloaded/generated/copied/shipped. Astra revises composition; prices/stock
  remain illustrative and equipment worn by player stays out of bag D091.
- New owner requirement: gold-only wallet for merchant, hide material counters.
  Final buy/sell concepts also apply global quality colors D097; no future trade
  currency/finite merchant wallet or stock rule is inferred.

### 2026-10-20 — D-095 expanded city A_v3 approved

- Scope: three substantially different fantasy-city ImageGen studies for owner
  selection. One first active merchant; all other NPCs are ambient/future roles.
  Main owns CITY_AND_TRADE brief, generation/provenance and canonical docs;
  GPT-6 Astra owns art direction and visual review. No Unity surface is claimed.
- Dependencies: approved cartoonB/equipment-forge style, fixed tactical camera,
  existing hero scale and future merchant→research workshop route. D095 replaces
  the older D063 defer-city restriction only for concept preparation now.
- Acceptance: three distinct silhouettes/materials/street arrangements, visible
  walkable entrance and merchant, ambient residents without extra services;
  saved original images/prompts, Astra review, owner shown before implementation.
  Preserve all accumulated uncommitted runtime work and the open game.
- Result:3original concepts saved under art/concepts/city_2026-10-20/ with exact
  prompts/hashes in art/source/city_2026-10-20_*.json. Astra concept PASS all3;
  owner selected A with orange roofs, timber/cream houses and open square.
  Runtime unchanged; next implementation needs site/layout/NPC/merchant binding.
- New owner revision: enlarge city with more quarters/streets and replace the
  tent with a distinctive permanent merchant shop. A_v2 still blended into housing
  per owner. Astra designed A_v3: unique cobalt hipped roof/brass ridge, open
  commercial front, attached large scales crest and clear apron. Main generated
  and inspected A_v3; one merchant unchanged. Astra concept PASS, no blockers;
  owner approved A_v3: «Пойдет». Next surface is the merchant window D096.

### 2026-10-20 — D-094 throne reinforcement locally qualified

- Scope: the accepted D075 three max-HP upgrades of+5, through the existing D078
  throne card. Main owns Run/Catalog, binding/data/installer/tests/docs; Astra
  reviews existing card/defeat presentation. No new visual asset/HUD/scene change.
- Dependencies: shared material/gold wallet, live throne health/max, D087 retry
  and existing damage/defeat effects. New capacity is filled by+5current HP too,
  preserving the amount of previous damage rather than fully healing it.
- Acceptance: three paid upgrades and no fourth; exact one-time prices; live
  current/max card and max/unfunded/pause/loss gating; damage still ends at zero,
  retry restores upgraded maximum and keeps current investment; full Reset
  clears upgrades. Legacy finite catalogs retain their existing behavior.
- Result:30EditMode +9PlayMode pass; Mac build637762906bytes, Astra visual
  PASS9native frames. Actual mouse0→3, exact costs and no fourth payment checked
  above normal/B docks. Normal unfunded game relaunched; Windows unverified.

### 2026-10-20 — D-093 production advancement locally qualified

- Owner approved5generator stages,1/2/3/4/5units per cycle, resource+gold upgrades
  per building, all stages available in the current prototype without research.
- Main owns Production rules/catalog/data/driver, selected-building binding,
  regression/native verification and docs. Astra reviews existing D078 card
  presentation; building art/HUD geometry/packages/scenes stay unchanged.
- Dependencies: existing generator points/shared wallet/active-time production,
  selected card fields and D087 preservation. No merge/sale/movement of production
  buildings, new structure, manual-profession multiplier or skipped-time income.
- Acceptance: real paid local level1→5, exact one-time prices, growing real yield,
  retained cycle progress, no upgrade-time reward, rejected/max/pause gating,
  current level/progress preserved by retry and cleared by full Reset; native
  card above regular/build HUD remains readable. Existing uncommitted work kept.
- Result:66unique checks pass (31EditMode/35PlayMode), Mac build succeeds,
  Astra visual PASS17frames; actual mouse2→5/max verified. Normal build is
  relaunched without QA funds/buildings. Windows remains unverified.

### 2026-10-20 — D-092 gathering professions locally qualified

- Owner approved miner_v3/logger_v2 and the first slice: independent mastery100,
  actual-material XP/points, strength/tempo/reach talents; asked Astra to implement.
- Main owns Stage2ProfessionCatalog/Professions/Driver, Economy modifiers,
  Stage3 binding/installer, tests, ImageGen runtime icon atlas and documentation.
  Astra owns HUD Professions DTO/native layout, inner O tabs and art importer.
- Dependencies: actual Gathered grants and equipped tool stats, shared interaction
  range for E/marker, D-087 preservation, existing fullscreen shell. Existing
  uncommitted gates/items/held art preserved; no scene/package/GUID migration.
- Acceptance: real held-E progress, separate point spend, bounded stock, combined
  tool/talent effects, pause/modal/tool-swap cancellation, reset versus retry,
  native O controls and720/900/1080p Astra appearance review. Tuning stays trial.
- Result:38PlayMode +17EditMode passed, final Mac build637740179bytes. Actual
  native clicks bought miner strength2→3 and logger tempo0→1 with separate points.
  Nine final native frames passed Astra visual review; no new attributable
  errors. Windows unverified. Remaining base/scroll/explorer work is separate.

### 2026-10-20 — Gathering professions concept -> approved for implementation

- Surface: existing HERO_PROGRESSION spec, plan/art/handoff and ImageGen
  concepts only. Main owns rules/provenance; GPT-6 Astra owns composition/review.
  No runtime, scene, package or serialized equipment changes in this slice.
- Dependencies: Economy.Gathered actual grants, current equipped tool stats,
  D-087 current-progress retry and existing Lunar Porcelain fullscreen O.
- Proposal: separate miner/logger mastery/XP/points, first strength/tempo/reach
  talents. Numbers/budget require review; no new accepted decision implied.
- Selected miner_v3/logger_v2: Astra final concept visual PASS; owner approval
  pending. Tool icon, point/XP ownership and current→next values reviewed.
- Acceptance: two readable consistent states, independent numeric progress,
  same equipped item and clear current→next harvest results, preserved shell;
  show owner before Astra native implementation. Existing uncommitted gate/
  equipment work retained; building/scroll implementation remains later.

### 2026-10-20 — D-091 equipped appearance and inventory bag locally qualified

- Owner reports crooked bow and equipped items missing on actual hero. Current
  scope: bow/pickaxe/axe only; preserve hero clothing/armor appearance.
- Main owns ImageGen repair derivatives, equipment tool-quality binding, gameplay
  tests/native QA and canonical docs. Astra owns HeroAvatar/Art/ForgeArt, narrow
  import and held-item presentation. Earlier uncommitted gates/equipment retained.
- Dependencies: actual equipped item instances/qualities, existing tool harvest
  facts/six poses, four facing directions and approved heroC/quality palette.
- Owner added D091: left bag contains only unequipped items; equipped items
  appear only in loadout slots. Astra owns BagItems DTO/draw; main filters binding
  and verifies equip/replacement/unequip without duplication/loss.
- Acceptance: correct bow perspective/grip in idle/walk/shot, visibly different
  actual tool quality in all six work phases, no baked duplicate tool, correct
  hide/restore on unequip/harvest/pause/reset/retry. Same armor/body identity;
  no item stats/cost, new spells or package/scene/GUID migration.

### 2026-10-19 — D-089/D-090 equipment/forge locally qualified

- Main owns Stage2 item catalog/rules/lifetime and combat/manual-harvest integration,
  Stage3 forge placement/local service and binding, tests, ImageGen and docs.
  Astra owns inventory/forge HUD DTO/presentation, forge art importer/world visuals
  and final native visual review. Surface ownership is coordinated before edits.
- Dependencies: existing shared resource wallet, hero combat/progression, local
  building occupancy/input, D090 five combat slots plus tools, approved four concepts,
  D087 preservation of current progress. Preserve earlier uncommitted gates work.
- Acceptance: real owned item instances/equip,8qualities/levels1–100, one base type
  each, atomic resource-paid craft/level/reforge, local paid forge and upgrade cap,
  live weapon/tool effects without extra splash, actual new screens/actions, reset
  versus loss semantics. Existing scenes/packages/GUIDs remain; data tuning trial.
- Owner answered D090:3forge stages cap D/B/S; special-scroll building upgrades
  only (no current source), SSS ultra-rare drop only. Weapons bow/wand, no shield.
  Rules/scene and regression unique latest results91/91 passed (67PlayMode,24EditMode).
  Actual Mac/Metal E/mouse craft→level→reforge→equip passed. Layout720/900/1080p
  reviewed; rebuilt ordinary-start inventory has corrected full axe UV. See handoff.

### 2026-10-19 — D-089 equipment/forge concepts approved; implementation next

- Main owns concept generation/provenance and canonical brief. Astra owns visual
  direction/review. Surfaces: existing EQUIPMENT_AND_RUNES, ART_DIRECTION, plan,
  decisions and concept images only; preserve uncommitted gates/runtime changes.
- Dependencies: approved fullscreen inventory shell/Lunar Porcelain, heroC and
  house/workshop architecture, six combat slots, axe/pickaxe, eight qualities.
- Acceptance: show inventory, fixed-view forge building, fullscreen forge and
  readable item-quality progression. One basic item family per type; actual
  art changes by quality, numerical1–100 stays separate. No elemental variants,
  SS or new currency/extra gear slots. Owner approved all4; values illustrative.
- This is concept work. No gameplay/UI implementation or scene/package changes.

### 2026-10-19 — D-087/D-088 gates, time-only retry and Sleep locally qualified

- Main owns Run/clock/gates, enemy source/tier facts, retry-owner integration,
  local portal E binding, tests and canonical documentation. Existing approved
  UI/art and scene references remain; no new visual layout is introduced.
- Latest owner instruction supersedes the requested live-population snapshot:
  defeat keeps CURRENT inventory/XP/ranks/research/buildings/world progress.
  Restore throne, clear the failed enemies/shots, return morning and retry the
  same pending night tier. Do not restore old live creeps or old world owners.
- Dependencies: shared EnemyGroup, continuous Stage3 catalog, existing Rewound
  cleanup/defeat readiness, D085 XP and existing local interaction hint surface.
- Acceptance: no early dawn; D088 local dwelling Sleep stops at19:50 and
  refuses any live/pending fight; day gates repeatedly farm the last
  cleared tier, preserve cadence/cap, auto-close before night without despawning
  existing enemies; mandatory nights grow and never overlap. Retry preserves
  earned/spent progress and repeated real kills award normal XP/gold.

### 2026-10-19 — D-085/D-086 progression and visual PASS / accepted reward cue without hammer

- Owner approved the three progression concepts and first passives. Only the
  level-up panel is superseded: divine gold full-screen stamped seal, new level
  in its centre, magic waves (no sparks), metallic impact and female choir/echo.
- Main owns Stage2 progression catalog/rules, combat and mover integration,
  Stage3 progression controller/binding, tests, ImageGen and canonical docs.
  Astra owns FantasyHud Hero/Dock/Progression partials, progression FX and art.
  Coordinate contracts before touching each other's files. Existing changes preserved.
- Dependencies: v4 fullscreen/HUD, original heroC, confirmed enemy WasKilled,
  D-062 BeforeWaveStarted/Rewound/Reset. Main binds a read-only HUD DTO and
  action callbacks; no new global manager, package or UI stack.
- Acceptance: XP once from hero/tower kills, cap100, legal point spending,
  personal damage/cadence/speed, safe cursor Blink, live numeric XP and UI, and
  checkpoint restoration of XP/ranks/points/cooldown. HP/MP remain inactive.
- D-086 polygon_v3 is explicitly approved: more varied sharp central shield,
  blurred outer energy waves, only heading/level/points. Astra now owns its
  separate level-up component; main binds event/checkpoint/QA. Main found a CC0 recorded female three-part chord and prepared the initial
  impact/echo mix, with preserved source/license and technical audio checks.
  Gameplay13/13 PlayMode and final native visual Astra PASS at720/900/1080p.
  Owner accepted voice but rejected every impact attempt. Latest instruction
  keeps reward choir/echo without hammer; current runtime has no impact layer.
  Owner selected Elden Ring Player Level Up (third supplied link). Candidate v5
  was rejected as entirely unlike the selected reference; never imported. PCM
  checks were technical only, with no auditory comparison. Current tool audit
  found no connected reference-audio listening/conditioning SFX capability; no
  further blind candidate is qualified. Owner reconfirmed accepted voice/echo without impact and requested the
  all-changes Git checkpoint. Impact work is deferred. No Unity asset/code
  change from the rejected preview; no game audio downloaded/sampled/shipped.

### 2026-10-19 — D-084 future landscape direction recorded, deferred

- Owner places this after necessary gameplay functionality, far beyond current
  work: landscape revision, scorched/undead-corrupted enemy road, etched runes
  along its full length, glowing red at night. No current art/runtime changes.
- Main owns this accepted brief/sequence in vision, plan, decisions and art
  direction. Later visual production requires ImageGen, owner approval and Astra.
  Existing navigation/route geometry remain unchanged until a future brief.

### 2026-10-19 — D-083 direct-start first frame implemented

- Owner asks to keep immediate prototype startup: no main menu, loading flow or
  extra Play click. Remove visible legacy primitives and incomplete HUD frames.
- Astra owns TacticalPresentation lifecycle only; main owns first-frame tests,
  opt-in player observation, ordinary build entry, build and canonical evidence. Existing uncommitted
  work is preserved; current approved artwork/layout is unchanged.
- Dependencies: bootstrap Awake, idempotent Stage2 composition initialization,
  synchronous world/presentation construction, then existing HUD binding.
- Acceptance: first rendered game frame has final hero/landmarks/world/HUD,
  hidden legacy actor/resource/debug-ground renderers and suppressed legacy UI;
  direct reload also works, Stage2 fixture retains its original presentation.
  Ordinary Editor Build Settings enters current Stage3; older entries preserved.
- Verification:5/5 PlayMode;2/2 startup cases requalified after default build entry
  correction; Mac/Metal build. Three fresh processes at720p/900p/1080p pass
  first8 game-camera frames from Unity frame1. Astra final visual PASS.

### 2026-10-19 — D-082 compact silver LVL / upgrade icon / walking performance qualified

- Root owns movement/frame profiling, measured fixes, opt-in QA, regression tests
  and canonical handoff. Astra owns the corrected small silver LVL tags and the
  approved generated upgrade icon, preserving the v4 card/action layout.
- Owner rejects the large gray Level tags and code-arrow appearance. Direct
  revision: shorter LVL, smaller text/padding and reflective silver finish.
  New silver/cyan ImageGen chevron is explicitly approved: «Да, внедряй».
- Dependencies: D-079 live levels/actions and D-080 input; existing scene/resource
  movement. Preserve all earlier uncommitted changes and runtime rules/prices.
- Acceptance: smaller readable metallic LVL N tags, approved icon in actual card;
  reproduce sustained hero traversal, record frame/CPU/allocation counters and
  compare a measured fix on the same route. No speculative streaming rewrite.
- Result: compact LVL and approved PNG implemented; 11 unique PlayMode pass.
  Same uncapped walk p50 6.00→3.00ms, p95 7.69→3.40ms, allocations 20.1→5.4KB.
  PC quality vSync=1; final native ordinary-WASD 227m/three passes has moving
  p95 16.97ms/max17.68ms on this60Hz screen and no overlap recovery.
  One earlier1s Metal presentation wait remains an unreproduced qualification
  limit, not a proven streaming/GC defect. See final handoff/evidence below.

### 2026-10-19 — D-081 tower merge feedback / level badges implemented

- Root owns merge-state binding, focused regression/player QA and canonical
  documentation. GPT-6 Astra owns the requested blue merge preview and compact
  silver Level badges. Existing uncommitted D-062–080 work is preserved.
- Dependencies: D-079 actual tower level/merge eligibility, D-080 nonmodal drag
  and approved v4 world-HUD clipping. No gameplay, price or tier changes.
- Acceptance: a compatible same-type/equal-level occupied target previews blue;
  ordinary construction stays green, invalid drops stay red. Live Level 1/2/etc.
  silver badges sit above built towers, retain readable screen size through zoom,
  update after upgrade/merge, and neither intercept input nor leak over modal HUD.

### 2026-10-19 — D-080 construction/world-selection repair implemented

- Root owns selection/input arbitration, actual pointer regression tests and QA;
  Astra owns removing the redundant Build Select button and rendering the existing
  selected strip above the nonmodal construction dock. Preserve prior changes.
- Reproduction: construction stays open after a purchase, but both binding and
  selected-strip visibility require Window.None. Previous D-079 tests picked
  towers with the dock closed, missing the normal build-to-selection path.
- Acceptance: one build-card click arms placement; no separate Select button.
  Own-building clicks select while B stays open, including when a draft is armed;
  selection cancels the draft before placement can charge/create. World drag
  move/merge and card upgrade/sale work with B open, UI remains click-safe,
  modal/pause/loss/reset cancel. Verify actual native select/move/merge clicks.
- Native requalification exposed a second issue: FastMouse merges down with held
  motion before onEvent, losing its location. The binding now captures event origin
  with merging disabled only while it owns input, restoring the original setting
  on disable/unbind. Three-event one-update drag and setting cleanup regressions
  pass. Final native retest passes catalogue→select→move→merge→upgrade→sale
  with B open. Earlier failed gesture attempts remain diagnostic evidence only.

### 2026-10-19 — D-079 implemented; local verification complete

- Owner: main chat owns Defense/Economy rules, data, binding, tests and handoff;
  Astra owns activation of approved v4 card actions and existing-art drag preview.
  User clarified this slice upgrades only towers and barricades. Preserve all
  previous uncommitted changes. Production/workshop/dwelling/throne upgrades are
  outside this slice.
- Dependencies: existing selection card, shared navigation/inventory and pre-wave
  snapshot. Acceptance: resource-paid upgrade, same-type/same-level drag merge,
  exact 50% invested-cost sale once, safe cancel/invalid drops, actual price/stat
  preview, resource/occupancy/combat/checkpoint consistency; local tests/build.
- Provisional tuning lives in active Stage3 catalogs; legacy Stage2 catalogs stay
  compatible. No new tower art or research gates; movement uses world drag and
  is limited to preparation to preserve siege play.
- Delivered: live card actions, tier data/ledger, transaction and snapshot rules,
  safe drag/ghost, 76 EditMode + 28 unique PlayMode cases, native Upgrade/Sell,
  Mac build and 30 final frames at three resolutions; Astra final visual PASS.
  No scene/Inspector rewire or PNG/prefab/package/settings changes. Windows
  remains unverified; full record is in the latest D-079 Handoff Register entry.

### 2026-10-19 — D-078 implemented; local verification complete

- Owner approved v4 explicitly: «Утверждаю v4». Astra owns HUD visual code/
  selection-strip drawing and final frame review; main chat owns building
  selection, input/data binding, tests and canonical handoff. Preserve all
  prior uncommitted D-062/D-073 work. No gameplay upgrade/merge/sell mechanics
  or invented hero values in this UI slice.
- Dependencies: existing native HUD art, camera/projected building visuals,
  actual tower/production/run/travel data and full-screen pages. Acceptance:
  connected full-width main dock/nav ribbon/selected strip, 2×5 reserves,
  numeric inactive hero bars until real systems exist; own-building picking,
  live stats/research link, no click-through, stale selection cleared on
  removal/modal/pause/loss/reset/rewind; tests and actual review frames.
- Delivered: Astra native HUD/ornaments/selection presentation; main live-data
  binding/picking, lifecycle, read-only building accessors and regression QA.
  Owner's HP/MP/XP/conformity correction implemented. 25/25 PlayMode, two native
  buttons, Mac build and 27 final frames at 720p/900p/1080p verified. No scene/
  Inspector/data migration. Details and remaining slices in runtime handoff below.

### 2026-10-19 — D-078 ribbon/numeric-bar concept evidence (approved)

- Owner: main chat ImageGen/provenance; Astra visual brief/review. User puts
  I/B/P/O/M in a narrow full-width ribbon between building strip and main HUD,
  reserving the main center for potentially ten spells. Colorful numeric
  HP/mana/XP bars requested; example numbers must remain concept-only.
- Surface: two v4 concept states, metadata and canonical UI/progression notes;
  runtime/scenes/assets excluded. Dependencies: D-077 unified dock/drag input,
  original game identity and current bow/Rapid data.
- Acceptance: normal and selected states share dock+ribbon, no nav inside the
  spell area, capacity for ten reserves, vivid red/cyan/orange bars with readable
  current/max examples. Present before owner approval/Astra implementation.

- Delivered: two v4 PNGs in art/concepts/building_selection_2026-10-19/;
  prompts/hashes/review in art/source/hud_ribbon_2026-10-19_generation.json.
  Astra found both suitable; owner then approved v4 and requested HP/MP/XP and
  closer runtime conformity. This block records the concept stage; implementation
  and qualification are above and in the latest D-078 runtime register entry.

### 2026-10-19 — D-077 historical unified HUD; revised by D-078

- Owner: main chat ImageGen/provenance/handoff; GPT-6 Astra visual brief/review.
  Two v3 states join minimap/hero/nav/abilities/status in one dock; selected
  strip is full-width directly above. No Move button; world drag handles
  movement/compatible merging. Old compact A/B returned for this revision.
- Final files: art/concepts/building_selection_2026-10-19/hud_unified_{normal,selected}_v3.png.
  Source/prompts/hashes/review: art/source/unified_hud_2026-10-19_generation.json.
  Astra finds both suitable; owner still judges height/appearance. No runtime
  changes. Detailed verification/remaining work: D-077 Handoff Register below.

### 2026-10-19 — D-076 historical compact candidates; replaced by D-077

- Current owner: main chat, ImageGen/provenance/handoff; GPT-6 Astra owns
  appearance and later implementation. User prioritizes building selection
  and above-HUD card before the previous progression-first sequence.
- Three final PNGs: art/concepts/building_selection_2026-10-19/; full prompts,
  references/hashes and Astra review: art/source/building_selection_2026-10-19_generation.json.
  Horizontal A, right B and common content/disabled-state sheet. Astra finds
  both suitable to show and recommends A. Owner requested the D-077 revision instead.
- No runtime/scene/asset migration or price/upgrade implementation. Old HUD
  pixels in generated mockups are illustrative, original assets remain source.
  Detailed checks/remaining work: D-076 Handoff Register entry below.

### 2026-10-19 — D-075 delivered: campaign and building-progression concept

- Current owner: Stage 3 Game Design. Canonical design now includes three-boss
  victory, six equipment slots/eight qualities, forge/tools, building selection,
  tower/base advancement and construction-versus-upgrade currency rules.
- Main contracts: BOSSES_AND_VICTORY.md, EQUIPMENT_AND_RUNES.md,
  TOWER_UPGRADES.md; supporting research/production/objectives/economy/UI specs.
  DEVELOPMENT_PLAN.md is the sole current implementation sequence.
- No gameplay, data asset, scene or UI implementation in this pass. Astra
  supplied text-only card/rune proposals. Detailed verification and remaining
  choices are in the D-075 entry of the Handoff Register below.

### 2026-10-19 — D-074 delivered: progression and day-gate concept

- Owner: current Stage 3 chat, Game Design. Owned surface: canonical vision,
  plan, hero progression, portal/day-night and equipment/rune feature contracts,
  decisions and handoff. Existing D-062/D-073 code, assets and qualification are
  preserved; this task does not implement the proposed gameplay or appearance.
- Dependencies: automatic bow/tower balance, continuous clock, D-062 snapshots,
  current gathering/tools and future open-world enemies/hero survival.
- Acceptance: distinguish owner requirements from proposed counts; define a
  coherent level-100 scope, low-splash hero role, repeatable daytime gate flow,
  reward/rune limits, checkpoint consequences and one ordered implementation
  sequence in DEVELOPMENT_PLAN.md. Identify unresolved intent and runtime gaps.
- Delivered contracts: HERO_PROGRESSION.md replaces the prior AoE roster with
  level-100 exploration combat and proposed 12 active / 48 passive / 36
  profession nodes. PORTAL_CONTROL.md separates cleared-tier farming from
  required nights, closes only new spawns and identifies live-actor checkpoint
  work. EQUIPMENT_AND_RUNES.md defines future automatic rewards, item level/
  quality and proposed 40 rune types with best-quality limits. Vision, plan,
  D-074, AGENTS, README, day/night, bow and retry specs point to these contracts.
- Owner clarification: remove both dwelling day skip and early dawn; separate
  hero/tower runes, with gathering runes possible. Runtime still has both old
  skips and no progression/gate/items implementation. Counts, rune levels/
  stacking and farm-loss timing are recommendations, not accepted gameplay.
- Verification: document references/count budgets and unchanged runtime/project
  files checked; no Unity tests/build/console session needed for prose-only
  edits. Existing D-062/D-073 qualification is not presented as testing the new
  design; no scene/Inspector wiring or asset migration.
- Remaining: choose proposed scope/point budgets/rune limits and gate cutoff/
  farm checkpoint; test continuous-night pacing; decide hero death before ruins.
  Implement ordered slices from DEVELOPMENT_PLAN.md, with approved ImageGen/
  Astra appearance. Commercial MVP extent and content pacing remain open.

### 2026-10-19 — D-073 delivered: spawn stalls, mineral reach and progression concept

- Parent owns WavesAI spawn diagnosis/configuration, manual-resource reach
  data/rules, tests/player QA and canonical documents. Prior uncommitted D-062
  work preserved. Astra reviews existing resource artwork at the new distance;
  no appearance/pose/PNG changes or new layouts in this repair.
- Diagnosis/contracts: the complete actual winding road already passes at
  30/60/240 Hz. Every Enemy.Configure redundantly validated the whole 250×250
  map on the movement thread. Eight-spawn local Editor observation: 40.807ms
  mean before, 0.275ms after; full validation alone 40.539ms. EnemyGroup keeps
  its existing one-time valid map/hash boundary and uses an internal trusted
  configuration path, retaining local cell/catalog checks. Standalone public
  Configure keeps full validation. Navigation, movement speed, siege, enemy
  counts and pause rules unchanged; no pooling/manager/package introduced.
- Mineral reach: ManualEconomy.asset and Stage2ManualResourceSettings add
  separate medium/large interaction gaps. E and the marker share the existing
  exact nearest-surface target. Collision/stock stages/site protection/+1
  remain coherent; small minerals, trees and legacy distance stay unchanged.
  No scene/prefab/Inspector migration. Existing builder preserves both fields.
- Verification: 91/91 scoped EditMode, 31/31 scoped PlayMode, no failures/skips;
  includes actual full road, 120-actor shared search, rejected spawn cells,
  every stock stage/cardinal reach, real E before collision and marker, siege,
  day/night and D-062 retry. Final Mac Development/Metal succeeds. Actual
  1600×900 player reaches the authored second horde through declared QA setup,
  then observes 16 active seconds of ordinary Enemy.Update/cadence: 120 enemies,
  all valid throne plans, lead displacement 35.263, longest stationary sample 0 s.
  Eight actual-player spawns average 0.104 ms, max 0.771 ms; timing observations,
  not a hardware-independent frame budget. Four resource pairs pay +1 via
  synthetic E with 0.6 free units beyond hero collision. Final log has no
  warnings/errors/exceptions; normal game left open. Astra accepts all 8 actual
  resource frames, restricted to shown full-state/bottom/zoom5 approaches.
- Progression: HERO_PROGRESSION.md is the single requested concept. Owner
  confirms level-one cursor Blink and four example range ranks 300/500/800/1000.
  Six actives, passive branches, independent mining/logging/exploration, XP,
  scale conversion and first implementation slice are proposals. Any future
  XP/ranks/cooldowns join the D-062 snapshot. No new progression gameplay/UI
  claims; no new city/crafting tiers/hero health.
- Evidence: art/source/movement_reach_2026-10-19_verification.json. One selected
  resource proof frame retained; raw 10 PNGs/logs/XML/local build ignored.
  Prior D-062 qualified source hashes match; existing runtime PNGs unchanged.
- Remaining: Windows build/runtime unavailable (module absent); review proposed
  progression scope and ImageGen appearance before new UI/spell implementation.
  Existing crowd overlap, camera-edge diagnostic, balance and Stage3 external
  player gate remain outside these two repairs.

### 2026-10-19 — D-062 delivered: pre-wave rewind and throne loss presentation

- Parent owns snapshots/restoration in existing Run/Defense/Economy and hero,
  production, dwelling, travel owners; input/scene binding, tests/build/capture
  and canonical docs. GPT-6 Astra owns the approved transient white fracture,
  render-only shake and lunar retry UI. Owner approved ImageGen storyboard with
  timed fade and offscreen shake-only clarification. No new global manager,
  package, disk save system or enemy behavior.
- Dependencies: same StartWave boundary for timer/local rest/manual calls;
  saved health/occupancy/cooldowns, exact individual stock/recovery, inventory,
  production progress and built dwelling/workshop/research/discovery. Existing
  logical camera, input blockers and unscaled UI transition.
- Acceptance: retry the failed wave with full preparation and matching snapshot,
  discard all failed-future state/effects/held actions, support later/repeated
  defeats and both start causes; verify nonfatal decay, offscreen signal,
  render-view restoration, fatal whiteout and working retry in an actual player.
- Delivered: in-memory snapshots in plain C# Economy/Defense/Production and
  scene-owned hero/dwelling/travel restoration. Run.BeforeWaveStarted captures
  before spawn; Rewound restores bindings without full ResetRun. RetryFailedWave
  restores the failed wave, checkpoint health/stock/occupancy/cooldowns, full
  preparation and checkpoint-day dawn without skipped-time income. Failed-future
  buildings/research/discovery, enemies/shots/effects and held E are cleared.
  R/button readiness is gated by the unscaled white transition; development full
  reset remains explicit. Stage3DefeatPresentation + ThroneFracture.shader and
  Stage3FantasyHud.Defeat use the approved existing dome/lunar UI; nonfatal
  pulses fully expire, offscreen damage shakes only. First-hit/fatal timing
  survives a preceding slow frame without losing its initial presentation.
- Verification: 60/60 scoped EditMode, 46/46 existing regression PlayMode and
  final 4/4 cross-system rewind cases (50 unique PlayMode; original 3 also passed
  before timing correction). Includes manual/timed, later/repeated failures,
  every world owner, exact stock/progress/health and real arrivals after 800 ms
  stalls. Final Mac Development/Metal build succeeds. Actual 1600×900 player
  QA passes 14 assertions and captures 17 frames; R and a native CUA button click
  restore the checkpoint. Final player log has no warnings/errors/exceptions.
  GPT-6 Astra accepts the final actual frames after the timing patch; exact
  reviewed frames and source hashes are recorded in the verification JSON.
- Wiring/preservation: existing TacticalPresentation binds after the original
  dome; Resources/Stage3DefeatSettings supplies timings/shader. No Inspector
  migration, scene/prefab/package changes or raster runtime additions; all 66
  existing runtime PNGs byte-identical. Two selected proof frames retained in
  art/review; raw captures/XML/logs/local build stay ignored. Full record:
  art/source/defeat_rewind_2026-10-19_verification.json.
- Remaining: Windows build/shader/runtime unverified because this Mac lacks
  its module. Extra-preparation balance, huge-world/commercial-slice decisions,
  existing camera-edge diagnostic and Stage3 external-player gate remain open.
  No disk save or permanent roguelite progression introduced.

### 2026-10-18/19 — D-072 delivered: manual resource footing and E marker

- Ownership: parent owns gathering target contract, resource footing in the
  existing hero obstacle map, binding, tests/build/capture and documentation.
  GPT-6 Astra owns marker appearance and actual-frame review, reusing approved
  native axe/pick silhouettes and lunar palette. D-069/070/071 edits preserved.
- Dependencies: live Economy.Nodes and stock stages, current sprite binding,
  swept hero movement, gathering/UI/pause blockers and world regeneration.
- Acceptance: every tree/stone/ore size stops the hero at its footing and can
  be harvested there; one overhead tool indicates the exact E target, including
  faded trees. Depletion opens the site, recovery cannot trap/teleport the hero,
  and reset/regeneration do not retain stale footings/targets.
- Delivered/contracts: stable updatable circles in HeroObstacleMap; narrow
  Stage3ManualResourceCollisions binds all live nodes, shrinks mineral footings,
  disables depleted ones and defers restoration around the hero. GatheringDriver
  exposes the exact InteractionTarget used by E and the single overhead marker.
  TacticalPresentation clears/rebinds both on regeneration. ManualEconomy keeps
  the tree footing parameter; ManualResourceArt adds 25 visible-crest anchors.
  Marker position follows painted pixels rather than transparent atlas padding.
  No new managers/packages/scene components requiring Inspector wiring.
- Verification: 33/33 EditMode, 12/12 manual-resource PlayMode and21/21
  UI/build/world-travel cases. After silhouette-anchor polish,3/3 targeted checks
  pass, including an independent original-PNG alpha-crest projection comparison
  at all3zooms. Final Mac Development/Metal build succeeds; actual1600x900 player
  verifies all7resource/size contacts, E+1, no overlap-recovery teleport, fade,
  UI/pause and depletion. Astra accepted all15final actual frames. Native CUA
  WASD/E afterward approached an ordinary tree and increased wood0→1; player
  remains open there. Final player log has no warnings/errors/exceptions.
- Preservation/evidence: all47pre-existing PNGs unchanged; existing scene exact
  after serializer whitespace cleanup; ManualResourceArt is byte-identical after
  removing only25added anchors. Prior dirty D-069/070/071 work retained. Two
  selected actual frames in art/review; raw15PNGs/logs/XML/local build ignored.
  Full record: art/source/resource_interaction_2026-10-18_verification.json.
- Wiring/remaining: existing BuildAndWire saves the added tree catalog default;
  RefreshInteractionAnchors updates only the25top anchors and restores texture
  readability. Windows build/runtime remains unverified on this Mac. Existing
  camera-edge diagnostic and D-071 deferred progression/world work remain outside
  this correction; no enemy navigation/tool-tier change.

### 2026-10-18 — D-071 delivered: manual resources, natural trees and tools

- Owners: parent owns multi-node economy/distribution, scene/map/HUD/placement,
  tests/build/capture/docs and ImageGen; GPT-6 Astra owns resource/character art,
  native imports/registration, shared sprite shader and actual-frame review.
  All six concepts were approved. Owner explicitly required separate ordinary
  trees in close random groups 1–8 without rows, waiving another tree sketch.
- Delivered: independent sources throughout the test world; per-hit +1, retained
  partial reserves, individual active-time recovery, reset and seed replay.
  Stone/ore have three sizes and coherent depletion states; original small scale
  retained. Trees remain ordinary separate nodes. Axe/pick actions use confirmed
  gather facts; moving cancels unfinished work. Hero/skeleton use eight walk
  phases, with registered soles and mirrored left. Foreground trees reveal hero.
- Contracts/files: Economy.Nodes/CurrentNode plus identity-targeted gathering;
  HarvestRadius is maximum reserved footprint, InteractionRadius follows authored
  stock stages. ManualEconomy.asset is Stage3-only; original Stage2 data remains.
  ManualResourceArt and HeroArt/SkeletonWalks preserve legacy references; prefab
  binding, TacticalPresentation, minimap/HUD and construction consume these nodes.
  Shared shader now honors native URP flip/tint/alpha. Rewire via
  Stage3ManualResourceBuilder.BuildAllApproved; no packages or new managers.
- Verification: 22/22 EditMode;32 relevant PlayMode cases covered successfully
  (31 release-pass plus targeted 1/1 after replacing a frame-count wait with a
  real-time fade wait). Earlier broad HUD/daynight/combat cases also passed,
  before final reach/shader polish; not claimed as a fresh whole-suite run.
  Mac Development/Metal build and actual 1600x900 player capture pass. QA uses
  synthetic E/WASD and model stock setup, resets the disposable run. Captured
  tree groups 1/4/8, all mineral sizes/stages, 20 four-side full/low contacts,
  all walking phases and 180 consecutive motion frames. Source PNGs unchanged;
  all 15 selected production sources match generator/runtime hashes.
- Evidence: art/source/manual_resources_motion_2026-10-18_verification.json and
  manual_resources_motion_runtime_2026-10-18_generation.json; two selected actual
  frames in art/review. Raw media/logs/XML/local build remain ignored in Artifacts
  and Builds. Existing D-069/D-070 dirty code/data retained.
- Remaining: Windows build/runtime and physical trackpad unverified here;
  existing D-070 camera-edge diagnostic unchanged. Animation is the improved
  prototype cycle, not final photorealistic motion. City/trader/research locks,
  tool tiers, huge-world expansion, D-062 rewind and Stage3 external-player gate
  remain outside this slice. Existing stripped unused GaussianDOF warnings
  predate D-071; no new attributable runtime/font/shader errors.

### 2026-10-18 — D-070 delivered / D-071 concepts awaiting owner approval

- Owners/surfaces: Astra owns render/import/pad appearance and actual-frame
  review; parent owns native Mac routing, existing binding/map/tests, capture,
  ImageGen/provenance and records. Complete uncommitted D-069 work preserved.
  Dependencies: PC URP Forward+, original resource images, AppKit bridge,
  camera/UI blockers and anchored regional map math.
- Delivered D-070: 4xMSAA at renderScale 1 without added post-AA; six dedicated
  landmark/production images gain mipmaps/trilinear in metadata and builders.
  Original PNGs are unchanged. Crowded hero atlases stay no-mips. Only inner
  dashed pad guide removed; outer clearance and glyph margins/footprints stay.
  Camera polls native magnify/precise-scroll once, routing map-owned gestures
  through existing binding: pinch/spread zoom, two-finger translation pan,
  coarse wheel zoom. Header/focus/disable/ownership cannot replay pending input.
  No scene/Inspector migration, package change or gameplay/economy change.
- Verification: 18/18 relevant PlayMode on Unity 6000.6.2f1/Mac/Metal; native
  monitor/ratio/precise-vs-coarse/backing-scale/reset checks pass. New route case
  covers anchored map pinch, pan without zoom, no world-camera mutation, header
  exclusion, invalid input and returning pinch to the world. Mac Development
  build succeeds; native keyboard M opens the map in the final player. Astra
  accepted 18 actual static frames at zoom 2.5/5/10 for forest/quarry/mine and
  their pads; 13 more UI frames cover 1280x720. Comparison
  mode approximates former sampling; temporal shimmer reduction and constructed
  production sprites were not separately measured/reviewed. No attributable
  new game/font/shader errors; licensing refresh warning remains environmental.
- Limits/remaining D-070: physical trackpad hardware and Windows runtime are
  unverified (only MacStandaloneSupport installed). Additional broad camera-edge
  test fails its full-hero bounds assertion; camera clamp/hero dimensions are
  unchanged, temporary quad-only investigation did not resolve it, original
  assertion restored. Separate edge visibility work remains; no weakened test.
- Delivered D-071: six selected ImageGen sheets in
  art/concepts/manual_resources_motion_2026-10-18/. Astra reviewed directions:
  stone/ore sizes+depletion, tree groups 1/2/4/8, axe/pickaxe with hero C, hero
  walking/mining/chopping keyposes, skeleton walk keyposes. Matte iron veins,
  one pick point+flat adze and exact eight-tree count corrected before handoff.
  These are pending OWNER appearance approval, not imported runtime assets or
  proven animation loops. Percent labels are illustrative; eight-tree rows
  are a counting diagram, not the future world grid. Registration, limb
  alternation, tool grip and actual motion remain future implementation checks.
- Evidence: art/source/quality_gestures_2026-10-18_verification.json and
  art/source/manual_resource_motion_2026-10-18_generation.json include results,
  limits, selected hashes and full prompt history. Raw logs/frames/builds stay
  ignored in Artifacts/S3-QUALITY-GESTURES-20261018/ and Builds/. One selected
  actual pad frame is retained. New manual nodes/tools/animations, city/trader,
  research gates, D-062 rewind and Stage3 external-player gate remain unimplemented.

### 2026-10-18 — D-069 full-screen interface / local Mac handoff

- Owners/surfaces: GPT-6 Astra owns existing PlayerInterface visual runtime and
  review-builder code. Parent owns approved concept/provenance preservation,
  relevant PlayMode assertions, Unity verification and canonical documentation.
  Dependencies: current lunar art, scene-local HUD binding, wallet, local-E
  research, actual map and existing in-world construction. No other active
  surface is claimed; existing packages, scene references and gameplay data stay.
- Accepted: owner approved four full-screen pages I/P/O/M, explicitly kept B
  construction in the world, and selected icon+hotkey-only I/B/P/O/M HUD access.
  Approved original concept PNGs/prompts are in art/concepts/fullscreen_interface_2026-10-18/
  and art/source/fullscreen_interface_2026-10-18_generation.json. D-069 owns
  the superseding presentation contract; no auto-pause/new mechanics added.
- Delivered/contracts: four opaque full-backbuffer pages share four-tab header,
  wallet/clock and footer. The existing runtime partials reuse approved art;
  research stays local-E, development unavailable, B stays a nonmodal world
  ribbon. Public header/wallet/footer/tab/HUD rects and old MenuButtonRect match
  the visible navigation. Existing scene/binding/data/assets/meta need no
  migration. Installed single-face sans-serif fallback and Metal/Linear output
  fix avoid unreadable Mac labels/washed-out art; prior Windows encoding stays.
- Verification: PlayMode 16/16 on Unity 6000.6.2f1 / Mac / Metal covers full-screen
  corner protection, exclusive keys/continuing simulation, local-E re-arming,
  construction purchase, HUD/debug and anchored/synthetic-touch map regressions.
  Saved-scene Mac Development build passed; actual captures cover 1280x720,
  1600x900 and 1920x1080. Native keyboard I/P/O/M, Esc and B were observed.
  Astra accepted 18 main V3 frames and all nine V4 HUD/construction/hover
  frames; V4 only strengthens the HUD hover/selected tint. Final V4 has 39
  native PNGs and is left open at 1600x900 on Inventory for the owner.
  No new attributable game/font/shader errors; Unity licensing refresh reports
  an unavailable access token but tests/build complete successfully.
- Limits/remaining: this host has only MacStandaloneSupport, so D-069 Windows
  build/runtime validation is degraded; old D-068 Windows V4 is historical.
  Native coordinate-click automation returns noWindowsAvailable despite the
  visible keyboard-operable player. No complete mouse playthrough is claimed.
  Hover uses actual rendering with pointer-only Input System events; pressed
  and physical touch have no separate capture. Windows QA, future gameplay
  reserves, D-062 rewind and Stage 3 external-player gate remain.
- Evidence: art/source/fullscreen_interface_2026-10-18_verification.json;
  raw local logs/frames in Artifacts/S3-FULLSCREEN-INTERFACE-20261018/.
  Five approved concepts and three selected actual review frames are preserved;
  raw capture batches/builds stay ignored. README, canonical spec/plan/vision,
  art direction, decisions and existing HTML summaries reflect D-069.

### 2026-10-18 — D-068 interface proportions and map interaction correction

- Owners: Astra core owns scalable frame drawing, sans-serif font, HUD/build
  padding and shared interactive states; Astra windows owns safe content and
  clear hero marker. Parent owns cursor/pinch map math/input, capture/build/docs.
  QA owns focused map behavior and narrow safety assertions. Dependencies:
  existing D-066 interface art, actual minimap, UI input protection and scene.
- The owner's annotated runtime frames invalidate the prior presentation PASS
  as a quality gate: stretched jewels, displaced icons, serif font, no hover
  and insufficient safe areas require correction within the approved C style.
- Acceptance: proportional ornaments/icons; sans-serif Cyrillic; content and
  screen padding at 1280x720, 1600x900 and 1920x1080; visible control states;
  distinct hero marker; anchored cursor/pinch zoom with unchanged world camera;
  existing UI placement/input regressions pass and fresh Windows frames reviewed.
- Delivered/contracts: core uses Segoe UI/Arial, proportional five-band frame
  drawing with shared pixel edges, safe content/screen bounds and shared states.
  All windows and build/HUD content use safe interiors; inventory preview is
  static and empty reserve slots no longer contain clipped dashes. Map marker
  is a distinct ring/triangle with a large-map label. MapInput.cs adds inverse
  projection, anchored zoom and Touchscreen pan/pinch; wheel routes to the
  cursor anchor while +/- stay centered. Existing click/local-E/economy rules
  remain. Existing scene binding requires no manual Inspector migration.
- Verification: PlayMode 16/16. Cursor anchor error <0.000008 world units,
  bounds/invalid values/no camera movement and synthetic moving pinch/pan pass.
  Windows V4 built and started at all three exact backbuffer sizes; 39 final
  native screenshots retained. Astra accepted core states/geometry and all
  windows, including final Inventory on every size. V1/V2 evidence was rejected
  for seams, description/empty-slot clipping and unproven hover, then corrected.
  Hover evidence uses pointer-only Input System events and actual renderer,
  with requested/actual position logs; pressed has no separate down capture,
  and no physical touch/manual complete wheel playthrough is claimed.
- Handoff/evidence: Artifacts/S3-INTERFACE-CORRECTION-20261018/ and
  art/source/interface_correction_2026-10-18_verification.json. Final 1600 player
  left open on normal HUD. Original PNG bytes and prior reviews preserved;
  packages/settings unchanged, asset/meta pairs complete, no Git mutation.
  Future gameplay reserves, D-062 rewind and Stage3 external-player gate remain.
- Repository handoff: the owner requested documentation update and publication
  of the accumulated D-064–D-068 work. README/vision and the existing HTML
  summaries now point to the current interface/day-night behavior. Runtime
  sources, assets/meta, original-art provenance and selected review images are
  included; Builds/ and raw Artifacts/ remain local ignored output.

### 2026-10-18 — D-066 runtime lunar interface / D-067 clock clarification

- Owners/surfaces: GPT-6 Astra owns PlayerInterface presentation, construction
  ribbon, four window partials, original-art importer and circular shader.
  Parent owns optional Stage 2 input/legacy-render bindings, clock, saved scene,
  verification/build/capture and docs; QA agent owns focused UI integration
  scenarios and old assertions updated to the approved B/fullwidth contract.
  Dependencies: current hero C, wallet/build catalogs, live minimap, workshop,
  production/home local input and continuous Run state.
- Accepted/delivered: owner approved actual window implementation and both
  updated fullwidth ImageGen sketches. I/B/P/O/M toggle one panel; B replaces
  lower hero contents with a nonmodal build ribbon and retains world placement.
  Narrow vertical bow stats, future ability slots and red/blue/orange status
  reserves fill the normal dock. Menus remain accessible above open windows.
  Inventory reads current wallet/equipment; research reads actual local-E
  Teleportation state; the real map pans/zooms/recenters without moving camera.
  Item storage, skills, HP/mana/XP progression are explicitly inactive reserves.
- Contracts/files: PlayerInterfaceBinding opts in through narrow blockers and
  legacy Rect/render delegates. Old minimap drawing is suppressed while terrain/
  markers keep updating; existing development/outcome controls remain. Original
  generated PNGs are copied byte-for-byte; native sprite bounds/borders and
  circular/keyed shader sampling render the UI. Existing world sprites are reused.
  BuildAndWire binds three scene-local components; BuildWindowsPlayer targets
  Builds/WindowsInterface/Not3AInterface.exe without rebuilding the world.
- D-067: Run.WorldMinutes/CalendarDay/ClockHour/ClockMinute/IsNightTime display
  ordinary hours. Early victory advances to dawn; a still-active horde can pass
  08:00 into daylight. Clearing after dawn preserves forward time and grants
  full preparation. No wave overlap or in-horde home skip. Pause/reset retain
  their existing meanings. NightCondition optionally binds lighting to clock.
- Verification: EditMode Run 13/13; PlayMode latest results cover 35 distinct
  cases, all passing across broad/focused runs (broad31/34; targeted12/13;
  corrected legacy B expectation1/1; final navigation4/4). Broad failures were
  stale geometry/B assumptions and map initialization/regeneration cleanup,
  now corrected and checked. Actual lighting/rest test proves daylight during
  an unfinished horde without time skip/overlap. XML/logs:
  Artifacts/S3-PLAYER-INTERFACE-20261018/. Final UI 4/4 also checks late-bound
  legacy minimap suppression. First Windows frames were rejected for duplicate
  minimap, dark circular output, clipped small controls, square portrait and
  double map frame; all corrected. Astra accepted V2 HUD/four windows and V3
  construction after the last disabled-label contrast-only fix. V3 Windows
  build/startup succeeded, player responds and readiness markers pass without
  compiler/shader/game exceptions. Six new immutable full-player review PNGs:
  art/review/player_interface_*_2026-10-18.png. Pan/zoom/recenter APIs are tested;
  separate human IMGUI drag/scroll playthrough is not claimed.
- Sources: art/source/interface_runtime_2026-10-18_{generation,verification}.json;
  features/PLAYER_INTERFACE.md, DAY_NIGHT.md and D-066/D-067 own current rules.
  Remaining: later real item/ability/status/progression mechanics, world
  throne-health indicator, manual
  survival pacing/crowd behavior and D-062 rewind including WorldMinutes.

### 2026-10-18 — Hero C motion correction / D-066 interface concepts

- Owners/surfaces: GPT-6 Astra owns HeroAvatar/HeroArt, hero-only art import,
  focused visual tests and interface art/layouts. Primary agent owns ImageGen,
  verification/build orchestration and canonical docs. Dependencies: approved
  hero C, separate bow/grip, fixed camera, existing HUD/catalogs/workshop/map.
- Delivered motion: four planted directional idle sprites with restrained
  breathing and corrected right/left bows with straight undrawn strings;
  walking and standing hand registration remain separate. BuildHeroMotion
  adds only two PNGs/subassets to the existing HeroArt; scene binding remains
  valid. BuildHeroMotionPlayer writes a separate WindowsHeroMotion build so
  the user's already-running day/night player is preserved.
- Verification: import succeeded; focused D3D11 PlayMode 2/2, with 8 walking
  frames, 8 idle extremes, pause/sole-pivot checks and an actual hero arrow
  alongside the authored second horde (120 live / 82 visible). Astra inspected
  all frames and accepted the scoped correction. Windows x64 Development
  build succeeded and startup/readiness markers passed without game errors.
  Hidden-player screenshot was black and rejected; visual evidence is actual
  PlayMode camera rendering, excluding HUD. No full manual survival run claimed.
  Six new immutable review PNGs: art/review/hero_motion_*_2026-10-18.png.
  Prompts/hashes/limits: art/source/hero_motion_2026-10-18_{generation,verification}.json;
  local XML/logs/frames: Artifacts/S3-HERO-MOTION-20261018/.
- Interface: old charcoal style explicitly rejected; owner selected C lunar
  porcelain from three original fantasy directions. Compact-HUD revision and
  all five separate-window concepts generated; full minimap redesign allowed.
  Astra reviewed all sheets. PLAYER_INTERFACE.md / D-066 own requirements;
  art/concepts/interface_2026-10-18/ contains originals and
  art/source/interface_2026-10-18_generation.json contains full provenance.
  The C theme is approved; individual layouts/scales await owner feedback.
  No runtime HUD, inventory, progression or map controls changed in this task.
- Remaining: concrete UI layout approval, smaller HUD proportions/cartographic
  simplification and later Astra implementation with approved mechanics/clock
  contract. Inventory/equipment/development content is schematic. Motion uses
  planted sprites + breathing; an actual string-draw attack cycle is separate.
  Existing enemy overlap, rewind and future hero health/death remain unchanged.

### 2026-10-18 — D-065 day/night / dwelling / versatile hero C

- Owner/surfaces: current chat owns Run cadence/endless growth, dwelling
  placement/payment/local E, scene wiring and canonical docs. GPT-6 Astra owns
  approved C/B/lighting presentation, HUD, shaders, asset importers and visual QA.
  Dependencies: D-064, shared Run/EnemyGroup/Defense, gathering/production,
  green-ground collision, workshop/teleport sites and fixed camera.
- Accepted: twenty active minutes per day; night ends only after its horde is
  resolved. Days continue past the initial three hordes. A player-built home
  costs ten wood; E at its door skips only day, without skipped-time income.
  C is a versatile fantasy hero, with a separate starter bow; magic/other weapon
  mechanics are future work. Owner approved hero C, dwelling B and readable blue
  night ImageGen references before Astra implementation.
- Contracts/files: RunCatalog.ContinuousDays + later-night growth; Run.IsDay/
  IsNight/PreparationDuration and legacy Space gating. Stage3Dwelling/Catalog
  own placement/collision/input and prepared-art payment; DwellingPresentation
  binds a Home card and local hint. Hero body/equipment and day/night shaders
  use unchanged generated PNGs under Stage3/DayNight. DayNightSliceBuilder wires
  optional presentation references and the dwelling in Stage3Rapid3DGamePilot.
  Legacy finite Stage 2 scenes keep old behavior. Survival starts bypass the
  old automatic two-tower showcase grants. Contract: features/DAY_NIGHT.md.
- Verification: RunState/Defense EditMode 30/30, focused PlayMode 39/39 including
  actual E-gathered purchase, atomic payment/collision, UI/held/cancel input,
  local day-only rest with no income, five continuing nights and resets. After
  the final visual-only blue-tint/window-mask correction, visual PlayMode 1/1.
  Astra approved real day/night/dawn, four hero directions and grip-based arrow;
  original generated pixels preserved, Sprite metadata retains subasset IDs.
  Windows x64 Development build succeeded; responsive startup beyond 60s with
  all four readiness markers, no showcase grants and no game errors. Opt-in
  full-player fresh-start screenshot includes IMGUI without grants/framing/time
  jumps. Protected packages/settings/prior review and asset/meta pairs checked.
  Local XML/logs: Artifacts/S3-DAY-NIGHT-20261018/. Exact method/limits and hashes:
  art/source/day_night_2026-10-18_verification.json. Timer/long-run checks use
  controlled ticks/QA kills; material gathering is actual E input at dev x10.
- Remaining: manual survival pacing/balance and later-night crowd/performance
  (existing single-file overlap remains); D-062 rewind, world combat/hero death,
  magic/other weapons, ruins/blueprints/tutorial. Current R resets to day1;
  Stage 3 external player gate remains open.

### 2026-10-18 — D-064 hero bow / passive barricade / horde slice

- Owner/surfaces: current chat owns gameplay, optional composition bindings,
  separate Stage 3 run/bow data and canonical docs. GPT-6 Astra owns approved
  A barricade assets, HUD/presentation and visual QA; ImageGen supplied the
  owner-approved front-facing artwork. Existing Defense/Navigation/EnemyGroup,
  Run lifecycle, gathering and protected HUD are the dependencies.
- Delivered: automatic nearest-target bow while moving; pause, E, construction,
  debug and terminal states suppress firing. Portal enemies retain throne/blocker
  targets. Paid passive road walls redirect routes, never shoot and are breached
  on full blockage. First wave is solo-clearable; later hordes exceed solo bow
  capacity. Scene Stage3Rapid3DGamePilot binds HeroBow/HordeRun and the barricade
  catalog through Stage3HeroCombatSliceBuilder; no manual Inspector wiring.
  Legacy Stage 2 data, packages, camera and existing tower artwork are preserved.
- Contracts: Stage2HeroCombat{,Catalog,Driver}, optional GameComposition fields,
  DefenseCatalog/Stage2Tower.CanAttack, Gathering.IsInteracting; assets under
  Stage3/CombatSlice and Stage3/Barricades. HERO_COMBAT.md, WOODEN_BARRICADES.md,
  D-064 and the canonical plan describe the accepted slice.
- Verification: Defense EditMode 19/19; final focused PlayMode 9/9 and corrected
  real-camera visual check 1/1. Broad regression 94/95; the sole failing old
  paid-defense policy was adapted to dense hordes and passed in final 9/9.
  Solo and six-wall-only policies lose; 2 Rapid + 2 Longshot + 6 walls win both
  later hordes with throne intact, using a declared post-first-wave QA allowance
  of 20 wood / 14 stone and ordinary earned gold. This is policy evidence,
  not a universal balance or gathering/travel-time result.
  Astra verified preview/built cell fit and visible actual hero arrow with
  120 live enemies / 82 visible. Camera renders omit HUD; HUD regressions pass.
  Windows x64 Development build succeeded; D3D11 live startup remained responsive
  beyond 60 seconds, all three readiness markers present, no game errors.
  Existing obsolete-API/package warnings remain. Art/meta/hash and whitespace
  checks pass; earlier tracked review images were restored with owner approval.
  Summary: art/source/hero_combat_slice_2026-10-18_verification.json;
  local XML/logs: Artifacts/S3-HERO-COMBAT-20261018/.
- Remaining: manual play/balance, overlapping single-file enemy movement, final
  archer art, world enemies/hero health/death, day/night, ruins/loot/blueprints,
  research progression and tutorial. D-062 rewind remains unimplemented; full
  restart is still the runtime behavior. Stage 3 external player gate remains open.

### 2026-10-17 — Documentation and repository handoff

- Owner/workstream: this chat, docs / Git handoff. Owner requested documentation
  update and push of the accumulated session changes to the existing main branch.
- README and both HTML overviews now match D-062/D-063: survival/world identity,
  current tower readability, generators, player-built free workshop, E research
  and paired ground circles. Durable docs distinguish working runtime from the
  pending defeat rewind, research tuning, future city/NPC/trade and Windows QA.
- Verification: existing final runtime baseline remains macOS/Metal PlayMode
  46/46; this handoff changes documentation only. HTML nesting, unique IDs,
  navigation anchors and local source/image links checked statically; whitespace,
  new Unity asset/meta pairs and retained image hashes checked before commit.
  No new build or Unity gate claimed. Local Artifacts/build/cache output stays
  excluded; original art, runtime data, source provenance and review frames are
  part of the repository handoff. Earlier handoffs remain historical evidence.

### 2026-10-17 — D-063 player-built Research Workshop

- Owner/workstream: this chat, Research / construction menu / travel. Requested
  order: build the workshop personally, then study Teleportation; owner explicitly
  selected free construction for now. Existing research/travel and art are dependencies.
- Delivered: Research tab and workshop card in the existing HUD. No workshop,
  collision or marker exists at start. LMB places one workshop on free green
  ground, with a matching sprite/footprint preview; Esc/RMB cancels. Placement
  rejects road/borders, hero, resource districts/pads, solids, manual resources
  and reserved circle/landmark sites. Safe front approach is checked. Construction
  does not research automatically; E at the actual building creates the base seal.
  Building/tower/generator modes are exclusive; pause/debug/terminal cancel the
  preview, UI clicks cannot place through panels, held clicks cannot build twice.
  ResetRun/regeneration remove the workshop and clear research/discovery.
- Contracts/files: Stage3TravelState owns WorkshopBuilt; Stage3WorldTravel and
  its Construction partial own lifecycle/placement/visuals. HUD receives narrow
  scene callbacks, retaining the existing assembly boundary/UI stack. Production
  exposes PlacementStarting for cancellation, and tactical presentation refreshes
  ground clutter after placement. WorldTravelBuilder live review opens Research
  with no QA workshop/research/resources granted. No scene/data/package/art changes.
- Verification: macOS/Metal PlayMode **46/46** passed (101.29 s), including nine
  construction/travel scenarios plus HUD/menu/minimap, existing placement,
  production, resource-world collision/regeneration and gather/win/loss/restart.
  Logs/results: Artifacts/S3-WORKSHOP-BUILD-20261017/final.*. Protected scene,
  WorldTravelData, packages, tower sprite sets and source/runtime art hashes match
  the pre-task baseline. New Unity source keeps its meta. No new attributable
  compiler/shader/runtime errors in the passing run.
  Interactive Editor shows Research/card/free construction with no ready building;
  Console 0 warnings / 0 errors. Fresh Game view left open for owner construction;
  actual menu frame: art/review/workshop_build_menu_2026-10-17.png.
- Remaining: research prices/durations and new technologies are separate design
  choices. D-062 snapshot/rewind must later include workshop placement and unlocks.
  Windows player validation remains degraded: this installation has only Mac
  standalone support. City/NPC/trade remain future.

### 2026-10-17 — D-063 research-gated ground-circle pair in game

- Owner/workstream: this chat, Research / World travel / Stage 3 presentation.
  Owned surface: Assets/Stage3/WorldTravel/, narrow hooks in tactical presentation,
  resource-world regeneration, E gathering cancellation, HUD hint and minimap.
  Dependencies: existing run lifecycle, safe green ground, fixed camera, approved
  workshop/v4 source art. Other active surfaces and package/tower data preserved.
- Delivered: ready base workshop; E at its front studies «Телепортация» once.
  Base circle and marker appear only after research, as explicitly required by
  the owner. Physically find the remote circle, then E at either center travels
  both ways; release E before another action. Shared v4 material conforms to
  ground height, removes opaque source background, has no circle collision.
  Pause/terminal/construction/debug block interaction; blocked arrivals reject
  travel. ResetRun/regeneration clear research/discovery; workshop owns one solid
  footprint. Current research/travel are instant/free prototype rules.
- Wiring: Stage3Rapid3DGamePilot > Stage3TacticalPresentation.worldTravelData
  references Assets/Stage3/WorldTravel/WorldTravelData.asset. Data owns the art,
  layout and radii. Tools > Stage 3 > World Travel > Build and Wire restores the
  art/data reference outside Play; Open Live Review starts beside the workshop
  without granting research/resources. Only one serialized scene field added.
- Verification: macOS/Metal PlayMode **38/38** passed (87.83 s), including six
  travel scenarios, minimap, placement/build menu, production, resource-world
  regeneration/collision and gather/full-win/loss/restart regressions. Actual
  camera renders before/after research and at three zooms passed contrast/no
  opaque-square checks; saved as art/review/world_travel_*.png. Logs/results:
  Artifacts/S3-WORLD-TRAVEL-20261017/regression.*. Source/runtime PNG hashes match;
  protected packages/tower prefabs/sprite sets/resource art match pre-task hashes.
  No new attributable runtime/shader/compiler errors in the passing run.
  Interactive Editor E confirmed the base seal appears after study; Console
  0 warnings / 0 errors. Game view left open beside the workshop after research.
- Remaining: player construction of the workshop, prices/durations and further
  research are separate design choices. D-062 wave snapshot/rewind is still
  pending and must include this progress when implemented. Windows player/shader
  validation is degraded: this Unity installation has only MacStandaloneSupport;
  no Windows module was installed for this change. City/NPC/trade remain future.

### 2026-10-17 — D-063 ornamental teleport seal from owner reference

- Owner/workstream: this chat, Art concepts / docs. Owner found circle-only v3
  too bare and supplied a magic-seal image. Keep the shared ground-circle role
  and distant readability; add a more attractive geometric composition.
- Delivered: original teleport_circles_v4_seal.png with round hierarchy, links,
  arcs and a few large glyphs. User reference archived as inspiration only with
  unknown external source/license; no tracing/runtime reference-pixel use.
  Canonical spec/art/vision/plan and D-063 updated, older images retained.
- Verification: generated through built-in imagegen; subject/composition/margins
  inspected. Original selected bytes, dimensions and all prior hashes checked;
  prompt, input-role and rights recorded in the source manifest. Docs/links
  checked. No runtime/scene/Inspector changes or Unity tests for this concept.
- Remaining: owner review of v4; exact ground projection, symmetry and actual
  gameplay-scale readability during asset preparation. Research/travel runtime
  and D-062 rewind remain pending; city remains future.

### 2026-10-17 — D-063 universal simple-circle teleport reference

- Owner/workstream: this chat, Art concepts / design docs. Owner requested one
  identical appearance for both endpoints and a simpler pattern readable from
  the distant isometric camera: large outer circle, symmetric circles of varying
  diameter around it, circles at the center. Existing style/camera are dependencies.
- Delivered: teleport_circles_v3_simple.png, one circle-only design for both
  endpoints. No fresh/weathered variation. A first generated attempt had excess
  bloom; the selected revision uses separated outlines and restrained glow on
  an opaque quiet preview background. Canonical spec/art/vision/plan and D-063
  record the current direction; source manifest preserves both prompts and status.
- Verification: selected image visually inspected for simple circular hierarchy,
  horizontal marking and no tiny glyphs; original PNG/hash/mode/dimensions checked.
  Earlier selected art bytes unchanged; docs/links checked. Concept only, no
  runtime asset/scene/code/Inspector changes or new Unity tests.
- Remaining: owner review of v3; exact symmetry, footprint and actual gameplay
  scale/readability to verify during asset preparation, then research/travel
  integration. Workshop appearance remains accepted; city remains future.

### 2026-10-17 — D-063 appearance correction: ground teleport circles

- Owner/workstream: this chat, Art concepts / design docs. Owner accepted the
  Research Workshop appearance and rejected the vertical teleport arches;
  requested teleport circles drawn flat on the ground. Scope depends on the
  existing cartoon style and fixed tactical view; no runtime migration.
- Delivered: teleport_circles_v2.png shows a fresh base diagram and matching
  weathered world diagram, both entirely horizontal with ground-level glow,
  visible centers and no arch/pedestal. Renamed current concept to «Круги
  телепортации». Canonical briefs/vision/art/plan and D-063 record the correction;
  generation manifest records workshop approval and v1 rejection explicitly.
- Verification: generated through built-in imagegen, visually inspected for
  flat surface markings and complete diagrams. Original RGBA v2 bytes, prompt,
  source path, transparency and SHA-256 saved; approved workshop and rejected
  v1 hashes unchanged. Docs/links checked. No Unity code/scene/Inspector changes
  or tests; existing game session and previous work preserved.
- Remaining: owner review of circle v2; final asset preparation and research/
  teleport runtime briefs/implementation remain pending. City remains future.

### 2026-10-17 — D-063 research and world travel (concepts prepared)

- Owner/workstream: this chat, Game Design / Art concepts. Owned surface: two
  feature briefs, canonical docs and original concept references/provenance.
  Dependencies: D-062 world/rewind direction and current cartoon art vocabulary.
- Authorization/acceptance: name and design a research building for future
  buildings/towers; generate travel-gate references for a research-opened base
  endpoint and a discovered world endpoint. The owner clarified that the city
  does not exist and must not be created now; merchant/NPCs are future context.
  Both proposals must be readable in the current style without changing runtime.
- Delivered: «Исследовательская мастерская» with books/plans/instruments;
  matching base/world «Путевые врата» reference sheet. First research
  «Телепортация» and discovering the remote endpoint before travel are proposed
  first-slice rules. RESEARCH.md and WORLD_TELEPORTS.md distinguish those from
  accepted intent and open implementation choices. Vision/plan/art/D-063 updated.
- Verification: both built-in imagegen outputs inspected for subject, style,
  full silhouettes and absence of a city/NPC scene. Original RGBA PNG bytes
  copied unchanged into art/concepts/research_and_travel_2026-10-17/; alpha,
  dimensions and SHA-256 checked. Prompts/rights/source paths are recorded in
  art/source/research_and_travel_2026-10-17_generation.json. Docs checked for
  local references and whitespace. No Unity runtime, package, scene or Inspector
  changes/tests; live game and previous work preserved.
- Remaining: owner visual review, resolve the minimal research/travel rules and
  parameters before implementation, then verify research/access/placement/safe
  arrival/retry integration. D-062 rewind remains pending. City art/scene/NPCs
  and trade remain explicitly future work; no city was generated here.

### 2026-10-17 — D-062 survival/world identity and defeat rewind (design recorded)

- Owner/workstream: this chat, Game Design / Run state contract. Owned surface:
  canonical vision/plan/decisions, agent facts, existing run-state spec and art
  direction. Dependencies: current finite Run state, snapshot consumers and HUD.
- Accepted intent: tower defense combined with survival, huge-world exploration
  and base development. Exact world size and additional survival systems remain
  open. Owner corrected the wording: red portal spawns enemies; blue throne is
  defended. Destroying the throne emits light rays, ending in a white screen
  and «Повторить». Retry restores the state immediately before the failed wave
  started and gives its full preparation timer for either manual or timed start;
  repeated attempts are unlimited. Owner selected this snapshot boundary over
  restoring the beginning of the preceding preparation.
- Deliverable/acceptance: confirmed intent and current runtime are distinguished;
  D-062 supersedes the old peripheral-world framing and whole-run defeat retry.
  Existing STAGE2_RUN_STATE.md defines restoration scope, integration surfaces
  and observable runtime checks; no duplicate feature or architecture document.
- Verification: inspected StartWave/ResetRun, driver, production Reset consumer
  and HUD; checked documentation links and whitespace. This pass changes docs
  only. No new Unity run or functional retry validation is claimed; live Unity
  and previous tower-grid/fit changes are preserved. No scene/Inspector wiring.
- Remaining: runtime snapshot/restore plus defeat presentation/HUD implementation
  and EditMode/PlayMode validation; define the minimum survival/world slice and
  scale approach before commercial scope is fixed. No new packages, content
  counts, automation chain or persistent meta system are approved here.

### 2026-10-17 — D-061 follow-up: tower cell registration (implemented)

- Owner/workstream: this chat, Stage 3 Integration/Art. User reported a large
  vertical base offset and overlap between adjacent towers in the first result.
  Owned surface: shared tower sprite fit, contact shadows and stone handoff.
- Dependencies/acceptance: existing unit-cell anchors and camera, shared paid/
  preview art and layered catapult. Visible feet meet the front cell edge, full
  body/roof width leaves a gap between immediate neighbors at normal/near zoom,
  preview matches purchase and projectile release preserves stone size.
- Behavior/contracts: Stage3TowerSpriteSet stores robust visible body bounds and
  fit settings; the cartoon builder updates both existing sets/prefabs and their
  shadows. Stage3TowerSprite applies uniform fit and front-edge registration;
  Stage3CombatPresentation carries loaded stone scale into flight. Original PNGs,
  logical anchors, gameplay data and scene wiring are unchanged. Existing sets
  were refreshed; no manual Inspector action or scene migration is needed.
- Verification: Mac/Metal PlayMode 14/14, no failures/skips or new attributable
  errors. Actual pixel bounds of a dense four-tower row and both previews meet
  cell sides/front at zoom 5/2.5; final frames inspected. Placement lifecycle,
  paid build/reset, HUD, four bearings, combat and stone release all pass.
  Initial run was 13/14: the new near-zoom capture placed its cursor over HUD;
  corrected the test setup, then the complete final suite passed.
- Evidence: Artifacts/S3-TOWER-CELL-FIT-20261017/{final-playmode.xml,final/,
  verification.json}; art/review/tower_cell_fit_*2026-10-17.png. Canonical
  tactical spec, art direction, plan and D-061 reflect the corrected contract.
- Remaining: owner visual review and Windows target validation. No push or
  commit requested; changes remain local.
- Live handoff: ordinary Stage3Rapid3DGamePilot Play reopened without QA grants;
  Game view visible, startup Console checked at zero errors and zero warnings.

### 2026-10-17 — D-061 tower construction grid and 45-degree facing (implemented)

- Owner/workstream: this chat, continuing Stage 3 Integration/Art. Owns the
  tactical tower grid, Rapid/Longshot sprite facing/import and relevant checks.
- Authorization: owner requested visible cells on the gray zone when selecting
  a tower and a 45-degree turn of both existing towers because placement looks
  sideways. Existing billboards require the depicted object to change yaw;
  rotating the flat sprite would lean the tower. Use edited sibling artwork.
- Dependencies: current planar map/buildability masks, Defense placement state,
  shared preview/built sprite sets and existing layered Longshot animation.
- Acceptance: grid matches actual gray build cells only and follows selection,
  cancellation, pause/reset and generator switching; both tower bodies face
  along the grid in preview and after purchase, with upright posts, matching
  scale/contact and working shots. No new attributable Console errors. Preserve
  gameplay data, camera, map, pad art and original tower sources.
- Behavior/files: Stage3TacticalPresentation creates one cached ground mesh and
  material over real buildable road cells, with shared edges drawn once and
  no marks on green/reserved cells. It follows Defense placement state even
  over HUD and hides on cancel/pause/reset/generator selection. The cartoon
  builder's narrow Refresh Tower Facing imports sibling RapidGridFacing and
  LongshotGridFacing PNGs into existing sets/prefabs; original opaque height
  and front-foot offsets were retained initially (superseded by the cell-fit
  follow-up above after owner feedback). Stage3TowerSprite's initial aim follows
  the new resting bearing; actual targeting and separate spoon/stone remain.
- Verification: final Mac/Metal PlayMode 13/13, zero failures/skips. Legal-cell
  vertices and actual render contrast; both preview/built identities, bounds
  and upright alignment; menu/HUD/affordability/exclusive modes/pause/reset;
  repeated paid construction, four bearings and both attacks/releases/impacts.
  Final normal/close/overview grid and both built-tower renders independently
  inspected. No new attributable Console errors. Scene/catalog/package values
  and original PNGs remain unchanged; no scene or Inspector migration.
- Evidence: Artifacts/S3-TOWER-GRID-20261017/{final-playmode.xml,final/,
  verification.json}; shared art/review/*grid*2026-10-17.png. Full original-edit
  prompts/source rights/PNG hashes: art/source/tower_grid_facing_2026-10-17.json.
- Remaining: owner visual review and Windows target validation. Technical checks
  do not pre-approve the edited artwork. Changes remain local; no push requested.
- Live handoff: Stage3Rapid3DGamePilot reopened in ordinary Play with normal
  starting inventory, both paid showcase towers and no review grants. Game view
  is visible; Console shows zero errors and zero warnings after startup.

### 2026-10-17 — D-060 approved Astra pad icons (implemented)

- Authorization: owner approved approval_sheet_astra_v2 and explicitly requested
  implementation plus one commit of all accumulated changes. No push requested.
- Ownership: continuing GPT-6 Astra owns the narrow runtime vector glyph/neutral
  frame conversion, meaningful checks and Unity verification/captures. Parent
  owns canonical acceptance/docs, full pending-change audit and final commit.
- Dependencies/surface: D-059 mark lifecycle/material and D-057 typed locked
  point.Clearance, current orthographic camera. Approved Astra v1 glyph shapes
  plus v2 uniform world fit, 12% icon-layout insets, full neutral outer contour
  and subtle dashed inner safe border. No bitmap icon import or new package.
- Observable acceptance: all three actual pad marks match approved geometry,
  retain transparent negative space and fit their safe bounds at normal/near
  view. Built/reset/regenerate and UI/preview transitions keep the existing
  lifecycle; red/green ghosts, affordability gates, small-node E, points,
  collision/economy and accepted landmark/building art remain intact. Relevant
  checks pass, no new attributable Console errors, and the commit includes all
  intended pending source/assets/docs with their metadata.
- Files/wiring: Stage2PlacementVisuals now builds the neutral full rectangle,
  inner dashed safe border and uniformly fitted approved glyph. Original SVG
  masks are tessellated offline by build_runtime_mesh.py into the plain generated
  Stage2ApprovedPadGeometry.cs; negative space is actual mesh holes. No runtime
  SVG parser, raster icon, package, scene migration or Inspector wiring.
- Verification: Mac/Metal 3/3 EditMode (holes/solid samples, bounds and uniform
  fit) and 6/6 PlayMode (lifecycle, actual red/green renders, pointer/input,
  purchase/cancel and old E suppression/recovery), no failures/skips. Parent
  independently inspected all three final normal/close pad frames; the approved
  outline, safe fields and proportions hold. Six relevant scene/data/package
  baselines and all 38 protected art files are unchanged. All new Assets have
  metadata; GUIDs/source-runtime artwork/provenance hashes were checked.
- Final live state: fresh ordinary Play started without QA funds/buildings,
  Console initially 0/0. Owner then began playing, so automation stopped touching
  UI. Latest Console has zero errors and one external audio-device reset warning;
  no attributable glyph/runtime exception. Preserve this live user session.
- Evidence: Artifacts/S3-PLACEMENT-ICONS-20261017/{editmode,playmode}.xml, final/
  and ordinary-play.log. Runtime-ready source/geometry provenance:
  art/source/placement_icons_runtime_2026-10-17.json. Six shared review frames,
  hashes and independent verification: art/review/placement_icons_*2026-10-17*.
- Remaining: Windows target validation. This completes approved Mac integration;
  all accumulated intended source/assets/docs are saved together in the local
  D-060 integration commit. Standard Unity .meta empty-field whitespace is
  preserved; source whitespace checks pass. No push was requested. Rejected
  concepts remain explicit history.

### 2026-10-17 — Resource pad icon concepts (v2 approved for implementation)

- Owner/surface: owner explicitly delegated the new concept revision to GPT-6
  Astra after rejecting the parent's v1 for unrealistic bent forms and wrong
  proportions. Astra owns its new SVG/PNG/source/provenance; parent owns canonical
  art-status notes. Scope is a review sheet, following owner feedback that the
  current D-059 glyphs are difficult to recognize. No runtime/scene edits.
- Dependency/acceptance: existing three production categories and fixed 55°
  camera; inspect axe/log, pick/stone and pick/ingot at large and 32/48/64 px
  sizes plus schematic ground compression. Owner subsequently approved v2/D-060.
- Files: art/concepts/placement_icons_2026-10-17/ now has three new
  *_astra_v1.svg, build_astra_preview.py and approval_sheet_astra_v1.svg/png.
  Corrected straight shafts, axe poll/eye/bit and asymmetric pick/chisel head;
  irregular fractured stone differs from the regular metal bar at small sizes.
  Provenance/hashes: art/source/placement_icons_astra_2026-10-17.json. Original
  vector paths, no third-party assets or bitmap source; PNG is a review render.
  Rejected parent v1 is retained, its manifest marks rejection explicitly.
- Projection follow-up: owner flagged v1 ground tilt/missing safe areas. Current
  approval_sheet_astra_v2.svg/png uses full actual point.Clearance and the current
  no-yaw orthographic camera for outer bounds, a dashed proposed 12% icon inset
  and glyph together. Same px/world scale retains actual relative pad sizes;
  complete glyph artboards fit inside safe bounds. No arbitrary diamond/skew.
  Source: build_astra_preview_v2.py and projection_astra_v2.json; provenance:
  art/source/placement_icons_astra_v2_2026-10-17.json. V1 glyphs are unchanged.
- Verification: all four new SVG XML parsed, no bitmap layers, unique mask IDs;
  Astra and parent independently inspected the final native-rendered 1260×1030
  sheet including small/projected rows. Eleven baseline runtime/rejected-source
  files remain byte-identical; new provenance hashes match. No
  Unity import/wiring, runtime changes or new gameplay tests apply. The ground
  examples are explicitly schematic, not claimed as live Unity evidence.
- Follow-up verification: v2 PNG independently inspected by Astra and parent,
  SVG/unique IDs and metadata hashes verified, nested bounds checked. Eighteen
  baseline files including runtime, previous concepts and Astra v1 sources are
  byte-identical. The 12% inset is proposed icon padding only, not a gameplay rule.
- Acceptance: owner approved v2 and explicitly requested implementation/commit;
  the active D-060 handoff above now owns runtime conversion and verification.

### 2026-10-16 — D-059 construction readability (implemented)

- Owner/surface: continuing GPT-6 Astra owns runtime pad glyphs, preview shader,
  tower/production drivers, tactical preview and Unity verification. Parent owns
  existing HUD, build-menu tests, canonical docs and independent frame review.
- Behavior: six free typed pads show neutral axe/log, pick/stone or pick/ore
  ground stamps without entering construction. Occupied marks hide; reset and
  regeneration restore free ones. Matching hover hides the neutral mark beneath
  one preview outline. Actual tower/building silhouettes are transparent green
  only for Ready and red otherwise; UI hover hides the ghost. No orange/yellow
  placement state. Each price is visible; unaffordable cards are dimmed, do not
  select, and explain the deficit on hover. B/1/2 and public actions share the
  full-cost gate, with commit-time revalidation retained.
- Dependencies/contracts: D-058 inventory/run/pointer/purchase contracts and
  D-057 locked points. Existing costs, output, built materials/artwork, scale,
  map/road and points stay unchanged. Marks have no collider or E interaction.
- Files/wiring: Stage2PlacementVisuals owns original code-native glyph geometry;
  Defense/Resources/PlacementPreview.shader owns isolated preview materials.
  BuildDriver.CanBeginPlacement and ProductionDriver.CanBeginPlacement gate
  input; ProductionDriver binds/owns mark lifecycle; TacticalPresentation owns
  actual tower ghosts. Existing Stage2GameHud cards use the same gates. No new
  package/UI stack or scene/Inspector migration is required.
- Verification: Mac/Metal 19/19 EditMode and 21/21 PlayMode, zero failures/skips;
  suites cover Defense, production, HUD/menu, rendered ghost transparency/color,
  stamp lifecycle, invalid/wrong/blocked locations, pointer and late balance.
  Parent inspected actual Game frames for all three markers/buildings and both
  towers in green/red, plus disabled HUD frames. All 38 protected existing art
  files remain unchanged; three production PNGs still match source bytes.
- Native UI: actual disabled-card click did not select/spend/build. With funds,
  card click began placement; pad interaction built one sawmill, spent exactly
  its price once and hid that mark. Towers/Rapid and Esc also worked; Console
  showed 0 warnings/0 errors. Fresh ordinary Play has normal follow/default
  inventory, no QA buildings, Generators open and placement off; Console 0/0.
  Native purchase happened before a stationary green capture; all five green
  silhouettes are verified by final actual renders and pixel checks instead.
  Manual details: manual-ui-verification.json; startup: ordinary-play.log.
- Evidence: Artifacts/S3-PLACEMENT-READABILITY-20261016/final-{editmode,playmode}.xml,
  game/ and live/; 22 shared frames and hashes in art/review/placement_readability_*.
  Intermediate pilot/candidate failures are retained; final results supersede
  constructor-time MaterialPropertyBlock and overbroad informational-log fixture
  assertions. Specs: RESOURCE_PRODUCTION_BUILDINGS.md and STAGE2_BUILDING.md.
- Remaining: owner appearance/feel judgement and Windows target validation.
  No economy/content expansion, commit or push.

### 2026-10-16 — D-058 production buildings and bottom build menu (implemented)

- Owner/surface: Astra owns plain production rules/catalog in Economy, Integration
  driver/composition, existing tower/run/gathering coordination, Stage 3 binding,
  Unity import and runtime tests. Parent owns existing HUD, menu tests, source
  building artwork/provenance and canonical docs. No shared-file concurrent edits.
- Authorization: owner accepted D-057 appearance and asked for three buildable
  generators plus bottom Towers/Generators categories. Owner explicitly delegated
  temporary construction prices and production rates; Unity data owns the values.
- Contract: exact locked resource/type point, one generator per point, atomic
  shared-inventory payment, hero-safe collision and active-time passive output.
  Pause/terminal freeze; restart/regenerate clear owned state. Existing tower
  policy, small-node E, landmark images/scale and map/road remain authoritative.
- Dependencies/acceptance: existing D-057 points, inventory and run phases; actual
  menu selection, pointer exclusion, three built visuals, foreground/behind hero
  visibility, payment/type/occupied/pause/reset/production checks and no new errors.
- Files/wiring: Stage2Production/Catalog in Economy, Stage2ProductionDriver and
  optional productionCatalog binding in GameComposition; Stage2GameHud categories,
  narrow BuildDriver.PlacementStarting and Run.Reset coordination, independent
  gathering placement flags, point/collision rebinding in ResourceDistrictWorld.
  Stage3 scene adds exactly one serialized catalog reference. Explicit Import
  Production Buildings menu owns new sprite/catalog import; no additional manual
  Inspector wiring. Original landmark PNGs and 0.75 scale are preserved; only
  quarry/mine painted-ground depth regions extend over the observed approaches.
- Verification: Mac/Metal 33/33 EditMode, 26/26 combined PlayMode, 10/10 final
  focused PlayMode, no skips. Cached read-only views then passed 9/9 pure checks.
  Failed intermediate batch capture/input fixtures are retained and identified;
  final input test uses actual mouse edges, without direct-purchase substitution.
  Root independently inspected all nine final HUD/front/behind frames; quarry
  foreground hero clipping is fixed. Source/runtime PNGs match byte-for-byte;
  all 38 protected pre-production art files remain unchanged.
- Native UI verification: actual category/card click showed the sawmill ghost;
  actual pad click spent its complete price once and produced a visible building,
  then wood rose automatically. Towers/Rapid preview and Esc were also clicked.
  Fresh ordinary Play has Generators open, default inventory, no QA buildings,
  normal camera and Console 0 warnings/0 errors. Evidence and manual record:
  Artifacts/S3-RESOURCE-PRODUCTION-20261016/. Shared nine frames and hashes:
  art/review/resource_production_* and resource_production_2026-10-16_verification.json.
- Remaining: owner's appearance/feel judgement, provisional balance iteration and
  Windows target validation. No logistics/research/upgrades/selling/persistence.
  No commit/push or new package/UI stack.
- Spec: gamedesign/features/RESOURCE_PRODUCTION_BUILDINGS.md. Source art/prompts:
  art/source/resource_production_buildings_2026-10-16_generation.json.

### 2026-10-16 — D-057 authored resource landmarks (implemented, owner appearance accepted)

- Owner: GPT-6 Astra owns runtime/scene/canonical docs; parent owns generated
  artwork and provenance outside Assets. No concurrent edits to those surfaces.
- Human correction: replace smooth grey core plus edge sprites with coherent
  compound forest/quarry/iron excavation closely matching original D-053.
  Fixed authored composition is allowed; safe seeded placement remains. Shared
  data scale 0.75 covers visual/footprint/collision/local building point.
- Boundary: one typed locked local future-building point per landmark; large
  deposits are not E-gatherable. Existing small nodes/E, road/map/objectives,
  camera and economy remain. No production rules, prices, rates or build UI.
- Implemented: all three original-reference-derived RGBA compositions imported
  into Landmarks, serialized authored contours/trunks and scaled locked typed
  points. Seeded placement retains reservations; no old core/apron/cutout is
  rendered. Ground-depth regions preserve pad access without erasing the object.
  Local grass exclusion avoids grass crossing painted clearings. Small E nodes
  and production economy remain unchanged.
- Contracts/files: ResourceDistrictArt/World, ResourceDistrictGenerator.CutsTerrain,
  ResourceProductionPoint, ResourceDistrictSprite depth regions, LandscapeProps,
  tactical terrain and existing district/rapid-pilot tests. Shared HeroObstacleMap
  now rejects a float-rounded overlapping contact endpoint, retaining the last
  clear swept position instead of triggering next-frame teleport recovery.
- Intermediate verification retained: first focused PlayMode 10/12; old lateral
  head-on fixture hit a slanted authored contour, and a real dense-trunk wedge
  produced one overlap recovery. Fixture now approaches a flat actual contour;
  the wedge has a dedicated pure regression. Earlier hot-reload NullReferences
  in stone-iron.log are intermediate failures, not final startup evidence.
- Final checks: Mac/Metal 27/27 EditMode, 12/12 PlayMode, no skips. XML/logs
  are final-editmode/final-playmode under Artifacts/S3-AUTHORED-RESOURCE-LANDMARKS-20261016.
  All six pad clearances sampled with hero radius and verified approach; matching
  types, old-node E, reset/regenerate, accelerated contact and partial reveal pass.
  2000-step profile: 0 bytes, 0 recoveries, max step 0.070002, mean CPU 0.0564ms.
  Final whole/5/2.5 and pad frames: final-capture/ in that evidence root. Source
  artwork/full prompts: art/source/resource_landmarks_2026-10-16_generation.json.
  Parent independently inspected final Game frames and saved ten selected normal,
  pad and actual canopy-reveal PNGs as art/review/resource_landmark_*; results and
  frame hashes: art/review/resource_landmarks_2026-10-16_verification.json.
  All 240 protected original images/manual-node assets/scene/map/packages/settings
  remain unchanged; original references match origin/main and runtime PNGs match
  the generated source bytes. This is independent review, not owner acceptance.
  Fresh final-live.log and live Unity inspection: ordinary Play restored, follow
  camera/normal zoom, Console 0 warnings/0 errors after complete capture. Startup
  reports authored representation, six points and actual reserved/collision counts.
- Remaining: owner artistic review, future production/building brief and Windows
  target validation. No production gameplay or underground walking is implemented.
- Acceptance: reference fidelity at whole/normal5/close2.5, both fixture seeds,
  coherent ground/collision/point, lifecycle and old gathering regression,
  hero reveal and Console. Prior Astra 13/9/3 passes are technical only;
  its artistic result is REJECTED, historical evidence is retained.

### 2026-10-16 — Astra artifact correction (appearance REJECTED by D-057)

- Owner/surface: current Stage 3 Map/Art/Integration chat; only D-054–D-056
  large forest, stone and iron district presentation. User explicitly narrowed
  review and correction to these districts. Dependencies: existing fixed camera,
  XY rules, original PNGs/materials and seeded district footprints.
- Behavior/contracts: clean mirrored triplanar atlas pigment removes repeated
  baked wedges/seams; better sampled bevel keeps the existing vertex count.
  Ground/reveal transparent layers precede painted accents. Rock anchors follow
  actual relief, with view-depth clearance only against their own opaque pixel
  area; collision anchors remain on the ground. The mine retains readable lower
  values and a connected descent from its visible rear lip. Forest canopy
  opacity is restored and only its local floor is muted; tree layout is retained.
- Files: Stage3DistrictGeometry/Stage3ResourceDistrictWorld/DistrictGeology,
  ResourceDistrictLayout stair approach, existing feature/plan. Explicit
  Play-mode Capture Live Districts menu restores prior seed/hero/camera after
  review; no persistent request polling. Tests accept an evidence directory
  override and report the real platform instead of hard-coded Windows.
- Verification: Mac/Metal EditMode 13/13 (including 80-seed structural check),
  district PlayMode 9/9, two-seed overview/game/close renders independently
  reviewed. Final contact/reveal/capture recheck 3/3 passed after correcting
  transparent geology ordering; no new attributable final-run errors. 2,000 controlled
  steps: 0 managed bytes and 0 recovery jumps; 120 local walking frames p95
  0.4933 ms, maximum 3.7328 ms (Editor sample, not a GPU/FPS guarantee).
  Evidence: Artifacts/S3-ASTRA-RESOURCE-VISUAL-20261016/; shared new frames:
  art/review/astra_resource_district_* and the matching verification JSON.
  Independent SHA256 check preserved all 240 original images, old gathering
  assets, scene/map files, packages and project settings. Historical evidence preserved.
- Outcome: owner rejected the appearance; D-057 replaces this representation.
  Technical passes above do not constitute visual acceptance. Wider Stage 3 gate remains; no production,
  renewal, underground traversal or new assets. No new Windows build/run in
  this Mac correction. No additional Inspector wiring. No commit or push.

### 2026-10-16 — Windows to Mac source handoff

- Owner/surface: current Stage 3 chat; README setup instructions and the full
  pending Stage 3 source/assets/documentation handoff on main. The owner
  explicitly requested committing and pushing all current work to continue on Mac.
- Contract: both machines use Editor 6000.6.2f1, revision 770e33f6875c, from
  ProjectSettings/ProjectVersion.txt. README names the exact version and the
  existing-scene workflow; Packages lock and asset .meta files travel in Git,
  while each OS regenerates its own Library. No Editor/package upgrade.
- Verification: version checked against ProjectVersion.txt; existing final
  Windows evidence remains 69/69 EditMode and 61/61 PlayMode. This handoff only
  clarifies documentation; these new district changes have not yet been
  revalidated on Mac. The earlier Mac check below predates D-050–D-056.
- Remaining: import on Mac with the exact Editor, check Console/Game and the
  hybrid district appearance. Owner appearance review and the wider Stage 3
  gate remain open as described below.

### 2026-10-16 — D-056 structural district refinement (verified)

- Owner/surface: current Stage 3 Map/Integration/Art chat; structural layout,
  world-owned relief/ground meshes and shaders, visibility cache/transitions,
  hero movement diagnostics, terrain openings and existing tests/canonical docs.
  Dependencies: fixed camera/XY movement, original sprite kit and original
  painted stone/grass pigments. No package or assembly change.
- Authorization: owner explicitly chose hybrid procedural relief + painted
  sprites after reporting flicker, ground seams, unclear pit/disconnected stairs,
  uniform stone and stuck-together trees. Production and underground traversal
  are still outside this brief.
- Acceptance: visible real cavity/connected stair landings, several stone
  levels, scale-aware forest gaps, conforming ground blend, local partial fade
  with easing/hold, and measured movement/visibility without recovery jumps.
- Contracts: ResourceDistrictLayout drives meshes/decor; terrain and outside
  ring omit relief interiors. Pixel masks are cached once, not texture-read
  per frame. Geometry is sorted once for the fixed camera; no moving-camera
  generalization. Run reset retains the world; regeneration replaces owned
  geometry/collision/minimap/landscape. Prior local work and source PNGs retained.
- Verification: Map/Navigation EditMode 69/69; final Stage3/Integration/RunState
  PlayMode 61/61 including nine district scenarios, no failures/skips or new
  attributable errors. Structural stair/landing checks cover 80 seeds; two-seed
  overview/close frames and partial canopy/paired hero renders inspected. The
  terrain count test now excludes exactly relief cells while keeping world
  bounds and WASD/minimap assertions. Initial render/capture/alpha and obsolete
  vertex-count failures are retained in evidence. 2,000 controlled steps: 0
  managed bytes, 0 walking recoveries, max 0.070002 for 0.07 movement, mean
  movement+visibility CPU 0.0616 ms. 120 normal post-warmup walking frames in
  Windows PlayMode: p50 0.6387 ms / p95 0.8861 ms / max 2.2926 ms, visibility
  p95 0.0611 ms, movement p95 0.0248 ms, no recoveries. Local Editor sample,
  not an FPS promise for other hardware. Windows build succeeded (305294365
  bytes); visible player PID 21564, seed 1103560765, six districts/138 sprites,
  clean Gameplay/Tactical Ready, map hash ACB42D4354F114AE. All 123 preexisting
  review/settings files and source sprite PNGs hash-verified unchanged.
  Evidence: Artifacts/S3-DISTRICT-REFINEMENT-20261016/;
  art/review/resource_district_hybrid_*. Git remained read-only.
- Remaining: owner appearance review of this hybrid art iteration and wider
  Stage 3 gate. Production/renewal/large-zone harvesting and underground hero
  traversal still require separate briefs. No extra Inspector wiring; material
  setup is owned by ResourceDistrictBuilder.UpdateHybridData.

### 2026-10-16 — D-055 procedural district hero collision

- Owner/surface: current Stage 3 Map/Integration/Art chat; HeroObstacleMap in
  the existing Map assembly, Stage2PlayerMover binding/sweep, district runtime
  ground anchors and ResourceDistrictArt collision data, tests/canonical docs.
  Dependencies: accepted generated cells, seeded sprite placement, XY movement
  and existing Input System. No package, assembly or Inspector wiring change.
- Request/acceptance: owner wants packed stone/iron to stop the hero and forest
  paths between separate trunk obstacles. Ground contours, sliding, accelerated
  movement, regeneration cleanup/recovery and unchanged road/reset are checked.
- Behavior/contracts: solid district strips plus protruding rock/hoist ground
  circles; scaled individual trunk circles, no canopy/alpha walls. Mover sweeps
  the whole displacement and slides; replacement recovers an overlapped hero
  to nearby in-bounds free ground. Run restart retains the same obstacle data.
  Scenery owns/detaches its binding; scenes without the kit keep prior movement.
  Original map/art PNGs, economy, enemies and earlier local work are preserved.
- Verification: Map/Navigation EditMode 63/63; final Stage3/Integration/RunState
  PlayMode 58/58 (six district scenarios), no failures/skips or new attributable
  console errors. Head-on hold with x10/x10, smooth sliding/round corners,
  separate trunks/forest gaps, 2,000 mixed movements, deterministic collision,
  safe regeneration/replacement, run reset and clear road pass. Contact and
  forest captures inspected; readable-atlas alpha sampling now excludes
  transparent margins from occlusion fading, preserving the hero render check.
  Windows Development build succeeded (299653025 bytes); player PID 9976 is
  visible, seed -828002414, six districts/79 strips/345 ground circles, clean
  Gameplay/Tactical Ready and map hash ACB42D4354F114AE. All 117 preexisting
  art/settings files and source PNGs hash-verified unchanged. Evidence:
  Artifacts/S3-RESOURCE-COLLISION-20261016/; art/review/resource_collision_*.
- Remaining: owner contact feel/appearance review and the wider Stage 3 gate
  stay open; production,
  renewal and underground travel remain separate briefs. Git stays read-only.

### 2026-10-16 — D-054 procedural modular resource districts

- Owner/surface: current Stage 3 Art/Map/Integration chat; new pure map generator,
  sprite kit/data/builder and runtime scenery, minimap overlay, one scene binding,
  landscape exclusion/teardown, tests and existing canonical documents.
  Dependencies: current fixed planar map/camera, URP sprite renderer and Input
  System. District-only material handles per-instance color/flip; baseline actor
  shaders/materials are preserved.
- Authorization/acceptance: owner clarified random varied objects at world
  creation and approved sprite assembly. Explicit initial size: 200–300 cells.
  Connected, distinct and separated footprints must protect road, objectives,
  hero and the three old gathering anchors. Same seed reproduces, new seed varies;
  real scene captures must show each resource kind and preserve hero readability.
- Behavior/contracts: ResourceDistrictGenerator returns read-only footprint data
  without modifying map/global Random. Stage3ResourceDistrictWorld builds
  seeded sprite combinations and cached generated ground; run restart retains
  world. Optional -worldSeed reproduces a scene. Landscape grass exclusion and
  minimap overlay rebuild alongside a regenerated layout. No new economy,
  production building, collision or underground movement rule. Art/settings:
  Assets/Stage3/ResourceDistricts/ResourceDistrictArt.asset; current scene gets
  one resourceDistrictArt reference via the Editor builder. Other serialized
  bindings, map/catalogs/packages and prior local changes remain existing.
- Source/rights: 18 original sprite parts across three transparent sheets;
  prompts/original paths/hashes/rights in
  art/source/procedural_resource_districts_2026-10-16_generation.json. The full
  D-053 concepts remain references. Brief: PROCEDURAL_RESOURCE_DISTRICTS.md.
- Verification: Map/Navigation EditMode 54/54; final Stage3/Integration/RunState
  PlayMode 55/55, no failures/skips. Same seed reproduces footprint and sprite
  choices/placement; other seeds vary; connectivity, strict cell area, clearances,
  size increase, map/global-Random preservation, run reset, minimap and resource
  cleanup pass. Both seed variants rendered for all three kinds. Initial alpha
  inspection missed a legacy shader limitation; an actual rendered cyan-pixel
  check caught the hidden hero. A district-only shader now applies URP tint/flip
  and all overlapping canopies fade together; the final pixel check passes.
  Regenerated grass exclusion and mesh teardown stay aligned with the new world.
  Initial import/compile/render failures are retained in task evidence.
  Windows Development build succeeded (299641912 bytes); two normal launches
  generated seeds 1273994254 / 401322775 with six districts and clean Gameplay/
  Tactical Ready. Real 1600x900 Game confirms the normal HUD/minimap overlay;
  second player (PID 26588) remains visible. All 108 preexisting art/settings
  files and original source PNG bytes preserved and hash-verified. Evidence:
  Artifacts/S3-PROCEDURAL-DISTRICTS-20261016/; art/review/procedural_district*.
- Remaining: owner appearance review; passive-production/large-zone harvesting
  and forest renewal remain separate briefs, wider Stage 3 gate stays open.
  Git stayed read-only; prior local D-050–D-053 work is retained.

### 2026-10-16 — D-053 large resource district visual references

- Owner/surface: current Stage 3 Art/Design chat; three concept PNGs,
  generation/source record and existing plan/art direction/decision/handoff.
  Dependencies: accepted cartoon B direction and current resource identities.
- Request/acceptance: owner requested references before implementation for a
  large forest + sawmill with forest renewal, a broad stone mass + quarry, and
  a descending iron mine + extraction station. Each district must visually
  dominate its small building and read through shape/material without UI labels.
- Outcome/files: three original 1536x1024 concepts in
  art/concepts/resource_districts_2026-10-16/. Text-only built-in imagegen;
  project's own prior art inspected for style, no third-party image inputs.
  Exact prompts, original generated paths, source/rights and SHA256 hashes:
  art/source/resource_districts_2026-10-16_generation.json. Original generated
  files retained; all three final PNGs copied into the workspace and displayed.
- Verification: all images inspected for full framing, common style, forest
  renewal cues, open granite benches and distinct downward mine/hoist identity;
  PNG signatures/dimensions/hashes verified. Unity EditMode/PlayMode/build not
  rerun: only preview art and documentation changed; no runtime/scene/asset/
  package/wiring change. Prior local work preserved; Git read-only.
- Remaining: owner's appearance choice; exact footprint (50x50 = 2,500 cells,
  not the other spoken 200–300 estimate); before runtime, agree placement near
  the matching deposit, prices/production/recovery, hero's economic role and
  resource-zone representation. One sawmill pilot before scaling production,
  then one enemy/tower counter pair, are recommendations only. D-032 MVP scope
  and the Stage 3 gate remain unchanged; no passive-production work activated.

### 2026-10-16 — D-052 development menu and player HUD cleanup

- Owner/surface: current Stage 3 chat, UI/UX + Integration; scene-owned HUD,
  movement runtime multiplier, build input binding, union of HUD/minimap pointer
  exclusions, existing run/economy contracts and canonical documentation.
  Dependencies: initialized composition, current IMGUI/Input System, minimap.
- Authorization/behavior: owner requested wave launch, pause, x10, gold/all
  material grants and removal/polish of diagnostic windows; explicitly chose
  independent hero/game speed multipliers. F3/button opens hidden development
  tools. Normal player UI retains resources, wave/throne, tower choice/price,
  contextual E/build hints and result/restart, including non-development builds.
  Technical source/distance tables and old readout/terminal boxes are removed.
- Contracts/files: Stage2GameHud is bound by Stage2GameComposition without scene
  rewiring. Stage2PlayerMover retains authored speed with a runtime x1–x10 factor;
  Stage2BuildDriver.SelectAndBeginPlacement synchronizes the existing preview.
  Composition owns combined pointer exclusions; minimap no longer overwrites or
  clears HUD input protection. Pause combines run pause and scaled-time freeze;
  F3/commands still work. Resume keeps chosen simulation speed; restart clears
  pause/inventory; scene exit/disable restores prior timeScale and hero x1.
  Grants use overflow-safe inventory transactions, wave start uses normal phase
  rules. Existing opt-in showcase capture additionally captures the real menu.
  New brief: gamedesign/features/STAGE3_DEBUG_MENU.md. No packages, map/scene/
  prefab/catalog changes; the final commercial UI stack remains open.
- Verification: EditMode 29/29 (Economy/Defense/RunState), PlayMode 52/52
  (Stage3/Integration/RunState), final focused menu 5/5, no skips. Real purchase,
  inventory/reset/overflow, wave refusal, paused hero/enemies, independent speed,
  keyboard at zero timeScale, release guard, global clock teardown and real
  Input System click/wheel blocking pass alongside existing minimap/full-run
  regressions. Windows build succeeded (274435971 bytes), real HUD/menu frames
  inspected at 1600×900; layout tightened so all commands fit. Native window
  automation kernel failed to initialize (OS error 3); direct manual clicking
  is degraded, covered instead by command/state and Input System checks.
  Initial test capture failure (batchmode WaitForEndOfFrame) is retained; visual
  evidence uses the actual standalone rendered frame. Prior review/settings
  files preserved byte-for-byte; fresh regression captures retained separately.
  Evidence: Artifacts/S3-DEBUG-HUD-20261016/; shared art/review/debug_hud_*.
- Remaining: owner judgement of appearance/usability, final commercial UI stack,
  wider Stage 3 content/audio/performance gate. Updated Windows game left open.
  Git read-only; prior local D-050/D-051 changes are preserved.

### 2026-10-16 — D-051 curved invasion road from the player's sketch

- Owner/surface: current Stage 3 chat; existing map builder/asset, layout-specific
  map/navigation/tactical/minimap checks and canonical documentation. Preserve
  the preexisting uncommitted D-050 correction and all art/reference evidence.
- Authorization: owner confirmed red left portal / blue right throne, smooth
  sketched road, seven-cell width and lower-left district in the 250×250 world.
- Behavior/contracts: Editor cubic geometry bakes into the existing gray surface
  and build mask. Two valleys surround a central rise, endpoints above them.
  Portal/throne reposition through map data; hero/resources, world/camera bounds,
  visual assets, prices, waves, combat and gray-only detour/siege rules remain.
  No runtime spline, waypoint system, package or Inspector rewiring.
- Files/migration: existing `Stage3DiagonalMapBuilder` and `Stage3PlanarCorner`,
  layout-specific existing tests; README, vision/plan/decisions, tactical/minimap/
  navigation briefs. Portal moves to (-42,12), throne to (55,12), clear of metal;
  Bootstrap updates markers from map data. Existing scene/prefab/GUID/data format
  and Inspector wiring remain. Original sketch is outside `Assets/`, with
  source/rights recorded in `art/source/curved_road_2026-10-16_reference.json`.
- Verification: Windows Map/Navigation EditMode 47/47; Stage3/Integration/
  RunState PlayMode 47/47; final focused route/render 1/1, no skips. All nine
  sampled straight widths, ordered valleys/rise, six-tower detour/seven-tower
  siege, resource lifecycle, paid full run/restart and minimap/camera/input pass.
  Baked road has 1,626 cells and a 225-step free route. Curve builder reproduces
  the saved asset byte-for-byte. World/camera/walk masks, resources and hero start
  preserved. Initial 45/47 PlayMode retained: two superseded grass-cell fixtures
  were corrected; no gameplay rule was changed to satisfy them.
  Windows build succeeded (274420675 bytes), actual player reached Gameplay
  Ready with map hash `ACB42D4354F114AE`, no attributable runtime errors. The old
  owned player was replaced; new visible game stays open for the owner.
  Actual Windows Game screenshot includes HUD/minimap; route overview uses only
  a disposable QA camera. Original art/settings evidence (102 files) preserved
  byte-for-byte after retaining new captures separately. Evidence:
  `Artifacts/S3-CURVED-ROAD-20261016/`; shared overview/live frame/result:
  `art/review/curved_road_*2026-10-16*`.
- Remaining: owner judgement of the route and longer-route pacing; wider Stage 3
  visual/performance gate. Speeds, waves and prices unchanged. Git stayed
  read-only; the earlier local D-050 changes are retained.

### 2026-10-16 — D-050 combat district in the lower-left world corner

- Owner/surface: current Stage 3 chat; tactical map bounds, existing map builder,
  related map/navigation/camera/minimap checks and canonical documentation.
- Authorization/acceptance: owner requires the road from portal to throne in
  the world's lower-left corner. Keep the old 100×100 lower/left bounds and
  extend green land to the right/up within the same 250×250 size. Entire gray
  footprint must fit in the lower-left quadrant. Preserve road cells/hash,
  build masks, landmarks, resources, hero start, 195-step route and game rules.
  Camera/minimap follow authored bounds; rebuilds reproduce this composition.
- Files: `Stage3PlanarCorner.asset`, `Stage3DiagonalMapBuilder`, existing
  invasion-road and tactical PlayMode checks; README, vision, plan, decisions,
  tactical/minimap briefs. No scene/prefab references, catalogs or packages change.
- Verification: Windows Map/Navigation EditMode 47/47; Stage3/Integration/
  RunState PlayMode 47/47, no skips. All 1,582 gray cells retain the old SHA256;
  relative road extents x=5..92, y=17..65 fit wholly in the lower-left quadrant.
  Route remains 195 steps. Builder round trip differs from D-049 only in the
  playable/camera bounds. Ground, input across the old edge, camera clamp/zoom,
  minimap terrain/markers/input exclusion, full run/restart and resources pass.
  Windows build succeeded (274420387 bytes); the previous owned player was
  closed for rebuilding, then the new visible player reached Gameplay Ready
  with map hash `7445A7082A36130A`, no attributable errors. Game stays open.
  Test-overwritten preexisting review PNGs restored byte-for-byte; new overview
  kept separately. QA overview changes zoom/far clip only in a disposable test
  camera. Evidence: `Artifacts/S3-WORLD-CORNER-20261016/`; shared overview/result:
  `art/review/world_corner_*2026-10-16*`. No new Inspector wiring or migration.
- Remaining: owner review and the wider Stage 3 visual/performance gate. Git
  remained read-only for this correction.

### 2026-10-16 — D-049 green world expansion to 250×250

- Owner/surface: current Stage 3 chat; saved tactical map, existing map builder,
  affected map/navigation/camera/minimap checks and canonical documentation.
- Authorization: owner selected 250×250 and expansion of green land only around
  the existing road. Depend on accepted D-045 layout and D-047 minimap.
- Contract/acceptance: extend playable/camera bounds 75 cells on each side;
  preserve all gray/buildable cells, reservations, anchors, hero start, seven-wide
  road and 195-step enemy route. New land is hero-walkable forest, forbidden to
  towers/enemies. Keep camera/zoom, resource count, art, economy and combat rules.
  Ordinary rebuilds retain the size; minimap/edge clamp cover the larger world.
- Files: `Stage3PlanarCorner.asset`, `Stage3DiagonalMapBuilder`, existing
  map/navigation and tactical/minimap tests; README, vision, plan, decisions
  and tactical/minimap briefs. The saved scene, data catalogs, art and packages
  keep their references; no new Inspector wiring or migration.
- Verification: Windows Map/Navigation EditMode 47/47; Stage3/Integration/
  RunState PlayMode 47/47, no skips. Road SHA256 matches the original 1,582-cell
  set; route stays 195 steps. Builder round trip preserves all serialized map
  fields except playable/camera bounds. New land renders, WASD crosses the old
  edge, grass rejects paid placement without spending, minimap covers all cells,
  and new camera edges/full run/restart/resource lifecycle pass.
  Windows Development build succeeded (274420387 bytes); standalone reached
  Gameplay Ready with map hash `D47CE64E44A4C38A`, no attributable runtime errors.
  Existing obsolete-API/Production Pipeline build warnings remain outside scope.
  Ground mesh: 250000 vertices, about 20.4 MB reported runtime memory; foliage:
  230296 triangles at unchanged density. Its old 100×100 total-count test was
  updated to retain a density ceiling; raw initial 46/47 result is preserved.
  All 14 test-overwritten preexisting review PNGs restored from byte-exact local
  copies; fresh captures kept separately. Evidence: `Artifacts/S3-WORLD-250-20261016/`;
  shared frames/result: `art/review/world_250_*2026-10-16*`. Full-world overview
  uses QA-only zoom/far clip; the saved player camera remains unchanged.
- Remaining: owner play feedback and wider Stage 3 visual/performance gate.
  New resource placements/content are not part of the expansion. Git remained
  read-only; the owned startup smoke process was closed after verification.

### 2026-10-15 — Approved resource sprites in the game

- Owner/surface: current Stage 3 chat; three sprite imports/prefabs, narrow
  camera-facing resource view, existing tactical resource binding/builders,
  actual-scene resource checks and canonical art/spec docs.
- Authorization: owner approved all three D-048 previews and explicitly asked
  to put them in the game. Use those exact PNGs, preserving their source files.
- Dependencies/acceptance: keep the existing wood/stone/metal anchors, gather
  interaction, stocks/recovery, minimap and run reset; visible grounded sprites
  at opening/close zoom, no gameplay colliders, no duplicated 3D model; preserve
  the current map, terrain, camera and earlier changes. Standard builders must
  reproduce sprite wiring. Verify lifecycle/real rendering and live Console.
- Implementation: exact approved PNG copies and three new prefabs in
  `Assets/Stage3/ResourceSpritePilot/`, shared existing unlit sprite treatment,
  alpha-based visible scale/custom footing pivots and ground-anchor sorting.
  Existing scene changed only its three resource visual references. Normal
  Stage 3/Cartoon World rebuilds retain the sprites; 3D sources stay preserved.
- Verification: Mac/Metal Stage 3 + Integration + RunState PlayMode 46/46 passed
  (42.62 s, no skips). Updated legacy mesh checks cover one visible sprite/no
  duplicate mesh or collider, original anchors/grass relief, size/pivot/camera
  alignment, depletion/recovery/reset and actual rendered pixels at size 5/2.5.
  Existing minimap, input/trackpad, building, combat, full run/restart checks pass.
  All six default/close Game-camera captures inspected. Interactive Game review
  visited each resource with the nearby E-gather prompt; Console 0 errors and
  0 warnings. All 90 preexisting review/settings files restored/hash-verified;
  fresh captures kept separately. Accepted road/map remains byte-identical.
  Evidence: `Artifacts/S3-RESOURCE-SPRITES-20261015/`; shared frames/result:
  `art/review/resource_sprite_*2026-10-15*`. PNG copies/source provenance:
  `art/source/resource_sprites_2026-10-15_provenance.json`.
- Remaining: owner live appearance feedback; no implementation blocker. Editor
  is running beside the ore node. No Windows build/run performed on this Mac;
  wider Stage 3 gate remains open.

### 2026-10-15 — Remove resource direction labels; resource sprite previews

- Owner/surface: current Stage 3 chat; resource-direction rendering in
  `Stage2GameComposition`, original ImageGen previews and canonical art/spec docs.
  Depends on the current cartoon B scene and approved D-037/D-038 resource shapes.
- Authorization/acceptance: remove floating `Stone W+A` / `Wood W+D` labels;
  prepare previews now (owner clarified the voice-transcribed timing). Keep the
  conifer, pale granite boulder and rust-seamed iron outcrop identities, rendered
  as cartoon sprites for review. This request is previews, not runtime replacement.
- Implementation: remove floating resource boxes and keyboard-direction suffixes
  from the temporary source readout. Resource counts/distances, nearby gathering
  prompt, minimap markers and economy remain existing behavior.
- Verification: refreshed/compiled in Unity and inspected the real live Game;
  floating resource boxes and direction suffixes are gone, minimap/resource
  counters remain, Console 0 errors/0 warnings. Actual frame:
  `art/review/resource_direction_labels_removed_2026-10-15.png`.
  No new tests or broad rerun for this label-only deletion. Three distinct
  original RGBA previews saved unchanged under
  `art/concepts/resource_sprite_review_2026-10-15/`; transparent corners and
  visible content verified. Stone/ore had a separate background-cleanup pass;
  prompts/source/output hashes: `art/source/resource_sprite_preview_2026-10-15_*`.
  Existing runtime resource prefabs match pre-change HEAD. Summary/evidence:
  `art/review/resource_preview_2026-10-15_verification.json`,
  `Artifacts/S3-RESOURCE-PREVIEW-20261015/`. Earlier minimap capture restored
  byte-identically after the new frame was saved separately.
- Remaining: owner assessment of the three previews. No runtime sprite
  replacement in this request. Wider UI and Stage 3 gate remain open.

### 2026-10-15 — Bottom-left minimap

- Owner/surface: current Stage 3 chat; narrow minimap presentation in Integration,
  Stage 3 binding, pointer exclusion in build/camera input and relevant tests.
  Depends on authored map cells, existing actor/run state and tactical camera.
- Request/acceptance: owner authorized this UI element now while the wider UI
  remains deferred. Bottom-left map must show the whole road/world, live hero,
  portal/throne, resources, towers/enemies and current camera footprint; follow
  movement/zoom/reset, resize within the screen and prevent building through it.
- Approach: cached schematic terrain and live markers in the existing immediate
  HUD path; no extra world-rendering camera, packages or final UI stack migration.
  Passive map only: no new movement command, camera pan, fog or exploration rule.
- Implementation: `Stage2Minimap` caches authored terrain and reads live marker
  state; `Stage3TacticalPresentation` creates/binds it at runtime. Existing
  build/camera drivers take a scene-owned pointer blocker, with teardown cleanup.
  No scene/material/map asset or package changed for this feature.
- Verification: Mac/Metal Stage 3 + Integration + RunState PlayMode 46/46 passed
  (42.26 s, no skips). Three new tests cover every terrain cell, live movement/
  actor removal/resource depletion/recovery/reset, viewport layout/edge zoom and
  an otherwise valid purchase blocked through the panel, with outside purchase
  still succeeding. Existing 43 camera/trackpad/gameplay checks passed.
  Interactive Game captures at 819×556 and 1206×614 were visually checked;
  real WASD movement moved the hero/camera outline and ordinary wave creeps
  appeared along the zigzag. Final observed Console: 0 errors, 0 warnings.
  The first batch screenshot attempt had no displayed Game surface; visual
  capture moved to the interactive Editor and passed. Raw initial evidence kept.
  All 85 preexisting review/settings files restored and hash-verified after
  retaining fresh test captures separately. Evidence: `Artifacts/S3-MINIMAP-20261015/`;
  shared image/summary: `art/review/minimap_{game,2026-10-15_verification}*`.
- Remaining: owner appearance feedback; no implementation blocker. Editor is
  open in a fresh ordinary Play review, original panel layout restored. Windows
  build/run not performed on this Mac. Wider UI and Stage 3 gate remain open.

### 2026-10-15 — Mac trackpad pinch zoom

- Owner/surface: current Stage 3 chat; `Stage2CameraFollow`, narrow AppKit
  adapter/native library + source/build script, importer metadata and camera
  regression test. Depends on existing tactical zoom, camera clamp and Input System.
- Authorization/acceptance: owner clarified two-finger pinch/spread on the Mac
  trackpad. Continuous zoom shares wheel smoothing and 0.5×..2× bounds; wheel
  remains usable. Native monitor acts only over the focused live Game view and
  clears on focus loss, Editor pause, Play exit/domain reload. Windows excludes
  this binary and never calls the native adapter.
- Verification: native source compiled universal ARM64/x86_64 and ad-hoc signed;
  native AppKit monitor/ratio/reset/invalid-input checks passed. Unity importer
  configured macOS only. Stage 3 + Integration + RunState PlayMode passed 43/43
  (39.38 s, no skips), including actual native loading, zoom bounds, disable/
  resume and existing wheel/movement/combat regressions. All 84 preexisting
  art/review/settings files restored and hash-verified; fresh test captures kept
  separately. Live Mac/Metal Game view is open in Play; observed Console has
  0 errors and 0 warnings. Evidence: `Artifacts/S3-TRACKPAD-20261015/`;
  shared summary: `art/review/trackpad_zoom_2026-10-15_verification.json`.
  Owner physically tested pinch/spread in the live Game view and confirmed
  "Да, работает"; the live log also records native `S3_PINCH_INPUT` samples.
- Remaining: no open work for this input request. Intel slice compiled/signed
  only; Windows build/run not performed. The wider Stage 3 appearance/pacing
  review stays open.

### 2026-10-15 — Accepted invasion road and lower-left portal

- Owner/surface: current Stage 3 chat; existing Stage 3 map builder/asset,
  opt-in map/navigation contract, live review framing and affected tests/docs.
  Depends on I's owner-accepted shape, existing hero movement, paid tower
  occupancy, enemy detours/siege and camera-facing landmark sprites.
- Authorization: keep I's horizontal double rectangular zigzag, seven tower
  footprints across. The owner allowed a lower-left portal if relocation is
  straightforward; spawning and landmarks already consume authored map anchors,
  so use that option. No new drawing or upper-right redraw is necessary.
- Implementation: existing `Stage3PlanarCorner` in the 100×100 world, nine
  connected seven-cell strips/eight right-angle bends, updated endpoint
  reservations and gray-only building mask. `EnemiesStayOnInvasionCorridor`
  restricts enemy navigation while keeping hero/resource walkability intact.
  Default false preserves earlier Stage 2 routing/hashes. Existing Stage 3
  rebuilds reproduce the new layout. Cartoon World live review frames the portal.
- Observable acceptance: width seven on all nine reaches; one connected road;
  enemy traverses both detours without grass shortcuts, six towers leave a
  passage and seven cause siege, release/reset restore navigation; portal
  spawns enemies, faces the Game camera and renders above the paving.
- Verification: Mac/Metal Map+Navigation EditMode 46/46 (0.55 s) and Stage 3,
  Integration+RunState PlayMode 42/42 (33.17 s), no skips. Existing short-road
  capture timing/off-screen pointer setup updated; capture pre-roll uses normal
  run/enemy ticks before combat, and full-run guard allows the longer route.
  Ordinary combat, victory/restart and the complete unopposed enemy traversal
  passed. Width tested transversely on all nine reaches; portal front/base
  visibility checked by the actual Game camera. No combat tuning changed.
  Actual route: 196 visited cells/195 steps, direct endpoint distance 91.24 cells;
  initial 17.20-cell concept target is not the scale of this blockout. At the
  existing 2.2-cell/s movement this is roughly 89 s before frame rounding/detours.
  Evidence: `Artifacts/S3-INVASION-ROAD-20261015/`; shareable captures and summary:
  `art/review/invasion_route_{game_overview,portal_lower_left}_2026-10-15.png`,
  `art/review/invasion_route_2026-10-15_verification.json`. All 81 preexisting
  reference/settings files were restored and hash-verified after fresh captures
  were retained separately. Windows was not rebuilt/retested on this Mac.
- Owner follow-up, 2026-10-15: tested the changed road and confirmed it is fine.
  Road review accepted; grass appearance raised separately. Hash comparison to
  pre-road HEAD confirms identical CartoonMeadowV2 PNG/import settings,
  TacticalGrass/GrassVerge materials, terrain shader/UV code and scene wiring.
  Live review now frames the lower-left portal instead of the old midpoint.
- Remaining: longer travel/pacing assessment; grass appearance clarification.
  Unity live review is open in Play Mode at the lower-left portal; observed
  Console: 0 errors, 0 warnings, portal front fully visible. Stage 3 stays open.

### 2026-10-15 — Winding invasion route concept review (historical exploration)

Historical concept handoff, superseded by the accepted-road implementation above.

- Owner/surface: current chat; ImageGen layout references under `art/concepts/`
  and the current design/handoff documents. Depends on the current Stage 3
  cartoon scene, original B board and existing map/navigation contracts.
- Request: explore a winding gray invasion/building zone, with roughly twice
  the existing portal-to-objective straight-line distance. Large explorable
  world is the longer-term direction; final world bounds are not selected.
- Current acceptance: ONE horizontal DOUBLE RECTANGULAR ZIGZAG road of uniform
  about SEVEN tower ground footprints width, no branches, portal and throne
  at its two ends. Use the B gameplay strip's gray paving/grass style. Preserve
  room for player-built mazes, original art and prompts/provenance. The earlier
  organic/variable-width and single vertical studies below are superseded.
  No runtime map, scene, camera or mechanic change in this task.
- Verification: all three original reference PNGs were visually inspected and
  copied unchanged into `art/concepts/invasion_route_review_2026-10-15/`.
  Exact generation/refinement prompts, input/output SHA256 and provenance are
  in `art/source/invasion_route_concepts_2026-10-15_{prompts,provenance}.json`.
  C was refined once to remove an unintended road spur beyond the objective.
  Current endpoints are (46,44)/(39,39): 8.60 cells direct, giving 17.20 cells
  for a 2x comparison. Image geometry is illustrative, not a measured map.
  Current navigation allows green terrain; future routing must prevent that
  shortcut while preserving hero resource travel and local tower detour/siege.
  Runtime/scene/package files are unchanged; gameplay tests were not rerun for
  this concept-only work.
- Owner correction: first A/B/C were too road-like. The original TD uses towers
  as blockers; the player, not narrow terrain, creates the repeated zigzag.
  A's earlier recommendation is withdrawn. D1 shows the wide, gently curved
  construction field; D2 edits that same field with alternating tower barriers
  and an illustrative gold route through their open ends. All output PNGs are
  unchanged ImageGen originals. New prompts/hashes/provenance:
  `art/source/maze_field_width_review_2026-10-15_{prompts,provenance}.json`.
  12–16 usable construction cells across is a starting hypothesis for blockout,
  not accepted tuning or measured image geometry. Example tower counts do not
  select economy. Current full-block/siege behavior stays unchanged.
- Further owner correction: D1/D2 rejected as oval/plaza-like. The gray area
  must remain an organically winding wide road, not a straight construction
  arena. E restores an asymmetrical S/meander and irregular worn shoulders;
  it is a new candidate, not an accepted result. Original output is retained as
  `art/concepts/invasion_route_review_2026-10-15/e_natural_wide_road.png`.
  Prompts/source hashes: `art/source/natural_wide_road_2026-10-15_*`.
- Quantified owner correction: E is still too narrow. Broad sections must fit
  7–8 tower ground footprints transversely; short necks may fit about 3. This
  supersedes the earlier agent-proposed 12–16-cell hypothesis. New F candidate
  `f_variable_width_road.png` has countable 8/3 scale rows, with unchanged PNG
  copy and prompts/source hashes in `art/source/variable_width_road_2026-10-15_*`.
  Initial generated attempt was discarded for an unwanted branch and a row
  along the reach; refinement uses the continuous E road as its edit target.
  Painted perspective/row direction do not certify usable grid width. No Unity
  tests rerun for concept-only work. Owner next requested the original B board:
  `art/concepts/sprite_style_review_2026-10-15/b_cartoon_2d.png`, lower gameplay
  strip road; found and opened. This does not select a new runtime layout.
- Latest owner correction: B road style confirmed, then uniform five-wide
  rectangular road requested; H single vertical zigzag was corrected to width
  seven, DOUBLE zigzag and HORIZONTAL composition. Current I original PNG:
  `art/concepts/invasion_route_review_2026-10-15/i_horizontal_double_zigzag_7wide.png`.
  Exact original SVG guide, prompts and hashes:
  `art/source/horizontal_double_zigzag_7wide_2026-10-15_*`. Guide has one open
  nine-segment polyline, eight square turns, width 7 and seven transverse
  footprint markers. Initial I generation was discarded for projection/count
  drift; final uses only the exact guide as edit input. Final route visually
  inspected, PNG copied byte-identically, JSON parses and diff whitespace
  checked. Generated paving/markers are illustrative, not certified Unity cells.
- Remaining: owner review of I, then measured width/layout/shortest-route
  blockout before map/navigation implementation. Stage 3 remains open;
  Stage 4 is not started. No gameplay tests rerun for concept-only changes.

### 2026-10-15 — Mac Editor startup and shared Windows development

- Owner/surface: chat `Запуск и продолжение Not 3A Game`; local Mac Unity
  installation, import, existing tests, live Stage 3 review and this handoff.
  Depends on clean `main` at `649938c4aa81404739ca10e4495ea01aef12b60d`,
  Unity 6000.6.2f1 ARM64 and the existing Personal license.
- Contract: develop the same project on Mac and Windows, retaining Windows/Steam
  as the release target. Preserve current mechanics, visuals, scene bindings and
  package versions; only fix an observed startup/compatibility blocker.
- Acceptance: import and compile, relevant existing tests, no new attributable
  Console errors, then `Stage3Rapid3DGamePilot` running in Editor Play through
  `Tools > Stage 3 > Cartoon World > Open Live Review`.
- Verification: Unity 6000.6.2f1 ARM64 imported packages/FBX and compiled on
  Apple M3/Metal. Existing Stage3/Integration/RunState PlayMode passed 41/41,
  45.87 s, including paid full run/restart, both tower roles and camera bounds.
  XML/log: `Artifacts/S3-MAC-20261015/playmode.{xml,log}`. Mac-rendered review
  PNGs are in its `mac-review/`; original tracked review PNGs were restored.
  Import-only URP runtime-list serialization was restored to the clean baseline.
  No new compiler/shader/test error; existing deprecated-API warnings remain.
  Initial repository status was clean. No Blender dependency in Unity `Assets/`.
  Owner accepted the first-launch Editor terms. First GUI startup produced one
  UnityEditor.Search indexing exception; an ordinary Editor restart resolved it,
  with no package/cache/source edits. Final GUI Console: 0 errors, 0 warnings.
  `Open Live Review` runs the actual `Stage3Rapid3DGamePilot` in Play, with
  `S2_INT_GAMEPLAY_READY`, map hash `996FF3F8BC05F95C`, both tower readiness
  markers and `S3_CARTOON_LIVE_REVIEW_READY`. Log: `editor-reopen.log` in the
  same evidence folder. Editor remains open in Game view for the owner.
  On the owner's follow-up, the project was re-cloned with ordinary
  `git clone https://github.com/fellsia2019/not-3a-game.git`: complete 14-commit
  remote history, no shallow/partial clone, `git fsck --full` passed. This full
  clone now occupies the original project path. The documentation commit was
  reapplied with `git am` and all three document hashes matched. At the owner's
  subsequent request, `not-3a-game-backup-20261015` was moved from `game-dev` to
  the Mac Trash. Local Unity cache and Mac test evidence remain in the active
  full clone; all 17 stable evidence files (XML/log/summary and 14 PNGs) retained
  their SHA256 hashes. GitHub CLI login/setup is complete
  for `fellsia2019`, with repository push permission verified.
  After the replacement, the same scene was reopened through Open Live Review:
  both towers/gameplay ready, unchanged map hash, GUI Console 0 errors/0 warnings.
  Startup log: `Artifacts/S3-MAC-20261015/editor-full-clone.log`.
  The earlier long Editor session also logged two native JobTempAlloc warning
  messages during first-time indexing; no project error accompanied them.
- Files changed: this handoff, canonical plan and README instructions for
  alternating Windows/Mac Git work. Runtime code, scenes, art and packages are
  unchanged.
- Remaining: owner visual review and Stage 3 gate stay open; Stage 4 is not
  started. No macOS player or new Windows build was requested or produced.

### 2026-10-15 — Remove unneeded animated QA captures

- Owner/surface: current chat; eight agent-generated GIF/MP4 recordings under
  `art/review/` and their documentation links. No runtime Unity asset changed.
- Behavior: remove motion recordings that are unnecessary for playing or
  continuing the project on Mac; retain static review PNGs, original sprite
  artwork, prompts/provenance and gameplay verification results.
- Verification: no GIF/MP4 remains in `art/review/`, no documentation link
  points to a deleted recording, and the Git working tree is synced after the
  removal. The historical commit still contains the recordings; no history
  rewrite was requested.
- Remaining: Stage 3 visual acceptance and actual Unity import/Play on Mac.

### 2026-10-15 — Portable Mac development handoff

- Owner/surface: current chat; README, reference inventory and GitHub handoff.
  No gameplay or Unity scene behavior changed.
  Depends on the current Stage 3 cartoon scene and its existing art provenance.
- Contract: Unity Editor 6000.6.2f1 opens the cloned project root; `Assets/` and
  all `.meta`, `Packages/`, `ProjectSettings/`, `art/`, canonical docs and the
  historical reference folder are saved in public GitHub `main`. Windows remains the approved
  build target; Mac is for continuing development and visual review.
- Verification: project-owned B board and both attached B PNGs are pixel-equal;
  all `Assets/` entries have `.meta` and portable paths have no case-fold name
  collisions. Historic third-party reference files under
  `concepts_visual_scene/` and `art/references/` stay outside `Assets/` and
  have explicit unknown-rights documentation. The owner approved placing all
  references in the existing public repository (D-044). GitHub `main` was
  verified at `8b156ffd8a5e655e73b217269c35e7c22428c735`; a local ZIP is
  an older snapshot, not the Mac transfer path. `art/review` GIF/MP4
  files were agent-generated QA captures, not player-provided references; the
  owner subsequently requested their removal (handoff above).
- Remaining: open/import and Play in Unity on the actual Mac; owner review of
  Stage 3 visuals and the open Stage 3 gate.

### 2026-10-15 — D-043 tactical camera viewport clamp

- Owner/surface: current chat; `Stage2CameraFollow` tactical branch, existing
  Stage 3 camera checks and tactical-view docs. Depends on current Stage 3
  100×100 map, fixed view angle and the D-042 playable scene.
- Behavior: camera follows the hero centrally until a visible ground corner
  reaches a playable map edge. It then stops translating on that axis so the
  hero moves through the viewport. The clamp tracks orthographic zoom and
  Game view aspect; Stage 2 non-tactical following and map rules stay intact.
- Acceptance: all four viewport corners remain over playable ground at map
  corners and zoom extremes; center follow, clicks, movement and zoom work.
- Verification/handoff: Stage3/Integration/RunState PlayMode 41/41; focused
  edge capture 1/1. Ground corners checked at 2.5/5/10 zoom and two aspects;
  edge-follow lock and center follow checked. Actual Game frames:
  `art/review/camera_clamp_{southwest,northeast}.png`. Windows Development
  build succeeded, 277524877 bytes, 0 errors/3 warnings; standalone reached
  Gameplay Ready with unchanged map hash `996FF3F8BC05F95C`. Reports/logs:
  `Artifacts/S3-CAMERA-CLAMP/`. Outer visual border remains unseen backing;
  owner visual review remains. No Git mutations performed.
- Follow-up from owner: the complete hero silhouette must stay on screen at
  the stopped camera edge. Stage 3 planar movement now has one cell of inner
  clearance, serialized into its current scene and preserved by both builders;
  Stage 2 movement is unchanged. Full PlayMode 41/41 passed again, including
  hero renderer bounds at the map extremes. Updated edge frames:
  `art/review/camera_clamp_{southwest,northeast}.png`. Final Development build
  succeeded, 277525565 bytes, 0 errors/7 warnings; standalone reached
  Gameplay Ready with unchanged map hash. Logs: `Artifacts/S3-HERO-EDGE/`.
  Owner visual judgement of the clearance remains open.

### 2026-10-15 — D-042 Astra cartoon rework -> owner visual review

- Owner/surface: human-requested Astra implementation agent in current chat;
  `Assets/Stage3/CartoonWorldPilot/**`, narrow tactical terrain/landscape/shot
  presentation, narrow/main builders, existing Stage 3 tests and canonical docs.
  Depends on original B gameplay reference and existing map/build/combat contracts.
- Behavior: V2 short turf with earth pockets, worn paving with joint weeds,
  67 bounded roadside ruin/shrub/flower sprites including the visible nonwalkable
  border. No collision or buildable-cell occupation; objective/resource clearance.
  Old bright triangle fringe removed, irregular grass apron and perspective-aware
  paving UVs retained. Map 100x100, anchors, resources and camera unchanged.
- Towers: grounded red-roof Rapid stays static. Longshot uses ONE stationary
  masonry/support sprite plus smoothly articulated spoon and separate loaded
  stone. The cup has fixed dimensions; projectile starts at its sampled actual
  release position on the existing clock. Pause and reset restore expected state.
  Four bearings are still observable; the support is a 2D fixed drawing, not a
  continuously rotating 3D mechanism. Preview and paid build share artwork.
- Source/rights: five original built-in imagegen PNGs under CartoonWorldPilot;
  exact prompts/provenance/SHA256 in `art/source/astra_cartoon_rework_*.json`.
  Original PNGs and metadata retained; V2 files match generation outputs exactly.
- Verification: full Stage3/Integration/RunState PlayMode 40/40, 33.00 s,
  including paid full run/restart, 24 builds, foundation-stable aiming/fire,
  actual cup release, pause and loaded-stone state. After final fringe removal,
  capture/landscape 2/2 (13.03 s); extra decor-cell/landmark-clearance check 1/1.
  After border-seam and painted-projectile fixes, final focused capture/combat/
  landscape checks passed 4/4 (17.32 s), including 90 actual 60 Hz throw frames.
  Windows Development build succeeded, 277524044 bytes, 0 errors/7 pre-existing
  warnings. Standalone startup reached gameplay and 67-prop readiness without a
  new attributable error; owned smoke process terminated after startup verification.
- Evidence: `art/review/astra_world_game_overview.png`, tower closeups and
  the retained PlayMode results; motion recordings were later removed at the
  owner's request.
  Final close-frame review found and corrected legacy cube-like flight artwork:
  released Longshot stone now shares the loaded painted sprite. The outside
  world edge also receives the same irregular grass apron; its former dark
  border tint is removed to avoid a straight color seam.
  Logs/XML/build report/frames: `Artifacts/S3-ASTRA-VISUAL/`.
  Build: `Builds/Windows/Stage3CartoonWorld/Not3A.exe`.
- Rebuild: `Stage3CartoonWorldBuilder.BuildAndWire`; main builder preserves V2
  art and roadside reference. No new scene Inspector work required. Live review
  frames ordinary preparation once; Space begins the wave.
- Remaining: owner's judgement of closeness to reference, wider Stage 3 visuals
  and stable standalone performance gate. First D-041 result was rejected;
  automated checks do not imply visual acceptance. Git was read-only throughout.

### 2026-10-15 — Stage 3 cartoon towers/terrain -> live review

- Owner/surface: current root; `Assets/Stage3/CartoonWorldPilot/**`, narrow
  tactical presentation/shot-motion/scene-builder changes, existing Stage 3
  tests and canonical documents. Depends on original cartoon B board and the
  existing building/combat/map contracts. D-041 authorizes two tower visuals
  and terrain; the owner's clarification explicitly retains map/camera.
- Behavior: Rapid and Longshot are transparent sprite towers with shared
  authored artwork in the placement preview. Longshot uses four drawn aiming
  directions and shot-clock windup/release/return; the actual projectile starts
  at its drawn cup socket. Rapid stays still. Pause, prices, damage, occupancy,
  paid building, waves and restart remain unchanged. Soft contact shadows have
  no collision. Four original source PNGs and exact prompts/hashes are saved.
- Terrain: cartoon grass and gray stone paving replace the old pigment/grid
  texture. Paving courses and slab aspect match the reference more closely;
  an alpha-faded visual grass apron softens the existing edge. The 100x100
  map, diagonal invasion zone, anchors, resource positions and camera stay.
  Original 3D assets remain available; no package or assembly added.
- Integration: existing Stage 3 scene uses copied catalogs with unchanged
  gameplay values. `Stage3CartoonWorldBuilder.BuildAndWire` rebuilds only this
  surface; the main builder retains it. `Open Live Review` frames the two
  towers/objectives once during ordinary preparation, with Space starting
  the wave. It saves no player-position/timing change and does not loop/reset.
- Verification: final Stage 3/Integration/RunState PlayMode 40/40 in 29.69 s,
  including 24 paid sprite towers, four bearings, windup/release/impact/pause,
  reset, the paid full-run check and actual first-wave firing of both roles.
  Windows Development Build succeeded, 269135061 bytes, 0 errors/7 warnings.
  Logs/reports are under `Artifacts/S3-CARTOON-WORLD/`.
  Final standalone startup reached gameplay/tactical readiness without a new
  error; the owned hidden smoke process was closed. The Editor opens the
  framed ordinary preparation scene for live inspection.
- Evidence: overview and both tower close-ups under `art/review/`; the
  animated QA capture was later removed at the owner's request.
  Source/rights: `art/source/cartoon_map_tower_provenance.json`; exact prompts:
  `cartoon_map_tower_prompts.json`. All four PNG hashes match imagegen originals.
- Remaining: owner review of new tower/terrain appearance and drawn animation,
  remaining resource/hero visuals and other Stage 3 gates. D-040 landmark
  appearance was approved in the current request. No full-project art approval
  or stable performance claim. Pre-existing work preserved; Git read-only.

### 2026-10-15 — Stage 3 portal/soul sprites -> live review

- Owner/surface: current root; `Assets/Stage3/LandmarkSpritePilot/**`, narrow
  presentation/scene-builder binding, skeleton ground sorting, existing Stage 3
  tests and canonical documents. Depends on saved B4/soul-sphere references,
  cartoon B style and existing map/run/portal contracts. D-040 authorizes these
  two replacements; full-project pipeline/art approval remains open.
- Behavior: original transparent cartoon PNGs replace the portal and soul
  objective visuals in the existing Stage 3 scene. The portal retains its red
  circulation clock; cyan highlights travel around the objective's horizontal
  arc band and its soul flame shimmers. Explicit pause freezes both. Root map
  anchors/bearing, 3x3 build exclusions, spawns, health/loss, combat/economy and
  waves remain unchanged. Old 3D prefabs/Blender sources are preserved.
- Integration: Sprite prefabs reuse the existing serialized visual fields;
  deterministic import/build and the main scene rebuilder retain this choice.
  View-ray clearance corrects observed foreground base clipping by opaque
  terrain without moving screen registration. Ground-anchor sorting keeps
  skeletons in front after leaving the portal, while opaque meshes depth-test
  normally. No collider, package, assembly boundary or gameplay system added.
- Verification: final Stage 3/Integration/RunState PlayMode 39/39 in 22.38 s.
  Checks retain paid three-wave win/restart and ordinary spawn/reservations,
  compare foreground steps/anchors against rendered ground, confirm changing
  red/blue pixels, pause stability and skeleton walking/feedback/reset.
  Windows Development Build succeeded: 268557149 bytes, 0 errors, 7 warnings.
  Final compile/test/build logs show no attributable compiler/runtime error.
  Standalone startup logged gameplay/tactical readiness and responded without
  a new runtime error; the owned hidden smoke process was closed afterward.
  The interactive Editor scene remains open for the owner's live inspection.
- Evidence: `Artifacts/S3-LANDMARK-SPRITES/` reports/logs and 60 actual camera
  frames; overview and individual close stills remain under `art/review/`.
  The animated QA capture was later removed at the owner's request.
  Exact prompts/source rights are in `art/source/landmark_sprites_provenance.json`
  and referenced prompt files; both PNGs match original imagegen outputs.
- Review: the existing explicit Editor preview now frames both landmarks and
  starts prepared waves early, repeating after outcome until Play Mode stops.
  It saves no player-position or timing change. Remaining: owner review of
  artwork, perceived facing, scale and flow; the rest of Stage 3 and stable
  standalone performance remain open. No Stage 4 or whole-project sprite
  conversion is claimed. Pre-existing work is preserved; Git read-only.

### 2026-10-15 — Stage 3 cartoon skeleton walk -> live review

- Owner/surface: current root; `Assets/Stage3/SkeletonSpritePilot/**`, narrow
  enemy-visual binding in `Stage3TacticalPresentation`/its scene builder and
  existing Stage 3 PlayMode tests/spec. Depends on chosen-for-trial cartoon B
  and the existing enemy/run/portal contracts. D-039 authorizes this one actor
  trial, not a full project pipeline switch.
- Behavior: one original transparent 1536x1024 imagegen sheet supplies six
  walk frames in each of four directions. Shared Sprite subassets register
  feet/torso from alpha; a small view changes frames with actual distance,
  faces the tactical camera, freezes on pause/outcome, stops at blockers and
  forwards the existing hit flash. A ground shadow has no collider. Normal
  portal spawning, navigation, combat, economy and wave values are unchanged;
  Stage 2 prefab/catalog remain intact. Scene regeneration retains the binding.
- Verification: Stage 3/Integration/RunState PlayMode 39/39 passed in 37.81 s,
  including normal portal spawns, real walking, four directions, alpha,
  pause/hit/reset and the full paid three-wave win/restart. Windows Development
  Build succeeded, 255543777 bytes, 0 errors, 7 warnings (full build log retained).
  The initial builder hit Unity's missing-component fake-null behavior with
  `??`; explicit Unity null checks corrected it before final import/tests/build.
  Final compile/test/build logs have no new attributable compiler/runtime error.
- Evidence: `Artifacts/S3-SKELETON-SPRITE/` retains import pivots, reports,
  logs and 60 actual Game-camera frames; `art/review/skeleton_sprite_portal_*`
  includes size-5/3.2 stills. The looping GIF was later removed. Prompts/provenance and the
  recording assembler are in `art/source/skeleton_walk_cartoon_*` and
  `assemble_skeleton_walk_review.cjs`. Source art is original built-in imagegen;
  no third-party image or mesh input. The PNG itself is preserved unchanged.
- Review: Tools > Stage 3 > Skeleton Sprites > Open Looping Live Review opens
  the existing scene near the portal and repeats ordinary first waves after
  outcome, only inside Editor Play Mode. Stop Play Mode ends this temporary
  inspection; normal game startup/countdown and saved hero spawn are unchanged.
- Remaining: owner's assessment of gait, style, scale and scene occlusion.
  No attack/death animation, final pipeline approval, resource-art acceptance
  or standalone performance claim. Stage 3 gate stays open; Stage 4 not started.
  Pre-existing uncommitted work is preserved; Git was used read-only.

### 2026-10-15 — Stage 3 sprite style comparison -> awaiting owner choice

- Owner/surface: current root; original visual review concepts under
  `art/concepts/sprite_style_review_2026-10-15/` and current plan/handoff only.
  Depends on the existing Rapid arrow / Longshot bowl-catapult roles and
  baseline undead identity. No new tower or enemy role is introduced.
- Intent/outcome: owner rejected the current 3D tower quality relative to
  production cost and requested pixel-art and smooth cartoon sprite options
  before deciding. Inspected current tower/resource Game frames and project
  tower concepts; generated two original comparison boards with the built-in
  imagegen tool. Each shows both towers, the same proposed skeleton in several
  poses, and an illustrative gameplay-scale strip. These strips are not Unity
  captures; scenery/heraldry do not approve new mechanics or lore.
- Files/provenance: `a_pixel_art.png`, `b_cartoon_2d.png` and
  `generation_manifest.json` retain outputs, exact prompts and source notes.
  Cartoon is a style edit of the original pixel board; no third-party image
  input was supplied. Both outputs were visually inspected before handoff.
- Remaining: owner selects or revises the direction. No style/pipeline switch,
  runtime asset, animation delivery or model approval is implied. Actual sprite
  animation/import/readability still need validation after the choice. No Unity
  gameplay/scene change, so tests/build are not rerun. Stage 3 stays open.

### 2026-09-26 — Stage 3 resources -> smooth stone, then continue wood/ore

- Latest owner instruction after the stone review: "поправь сглаживание и го
  дальше". This supersedes the temporary stone-only hold below. Preserve the
  approved A/B/A concepts and proceed with their implementation, without a
  new concept-selection loop. Stage 3 gate remains open; hero remains last.
- Ownership: root owns the shared Blender driver, stone assets, Unity import,
  actual-scene checks and canonical docs. Conifer subtask owns only
  `art/source/resource_wood_model.py` and wood candidate/source files; ore
  subtask owns only `art/source/resource_ore_model.py` and ore candidate/source
  files. They return geometry/materials for root integration and do not modify
  shared runtime assets or run the Unity test/build request watcher.
- Stone smoothing: confirmed false faceting came from CORNER albedo tint
  computed from each triangle's normal. Changed to POINT tint computed from
  the smooth vertex normal, then rebaked/exported. Imported mesh normals already
  preserve Blender smoothing; no gameplay or camera change is required.
  Its actual-scene verification passed 36/36 in 35.39 s; evidence is
  `Artifacts/S3-RESOURCE-SOURCES/stone-smoothing-playmode.xml` and the
  `stone-smoothed-game-close.png` capture there. The all-resource final
  test/build results are recorded below.
- Wood/ore are integrated at the existing asset paths. Wood has 33236
  triangles, closed boughs and a closed crown; ore has 9464 triangles with
  dark fractured masses surrounding recessed iron mineral. Ore's original
  generated slate surface and its provenance/prompt are in `art/source/`.
  Each resource still has one renderer/material and no collider.
- The actual wood frame exposed a second geometry defect beyond smooth
  normals: some radial contour samples went backwards around the center,
  folding thin top strips inward. A/B renders with a constant texture and
  disabled shadow reception retained the marks; unlit rendering did not.
  Ordered contour samples retain the same outline points and prevent the
  folds. The generator now checks outward upper/lower faces on all 22 boughs
  and the crown. No production lighting, shadow or shader workaround is used.
- Final verification: corrected stone and replacement wood/ore were captured
  in the actual scene at zoom 5/2.5. Stage 3/Integration/RunState PlayMode
  passed 36/36 in 35.50 s, including footprint/placement, material imports,
  resource depletion/recovery and the paid three-wave win/restart. Evidence:
  `Artifacts/S3-RESOURCE-SOURCES/playmode.xml`, `status.txt` and
  `art/review/resource_{stone,wood,metal}_game_{default,close}.png`.
  Editor render/readback on/off medians: stone 2.449/2.433 ms, wood
  2.457/2.455 ms, ore 2.595/2.424 ms; on p95 3.138/2.935/3.225 ms.
  These 1280×720 GTX 1060 observations do not establish standalone FPS.
- Final Windows Development Build succeeded: 249623372 bytes, 0 errors,
  3 warnings (`Artifacts/S3-RESOURCE-SOURCES/build.txt`). The playable scene
  was reopened after build. Asset/meta audit preserves existing GUIDs and
  verifies source/FBX/2048 atlases. The final suites/build log has no new
  compiler or runtime exception/error lines. A temporary SSAO diagnostic
  lacked a URP test assembly reference; it was removed before the final
  passing suites/build. No diagnostic material or rendering change remains.
- Remaining: owner's visual review of the actual resource set and stable
  standalone performance. The ore's front rust seam is still broader than
  its concept; technical checks do not constitute art approval. Then D-032
  basic undead, finished HUD/audio and hero last. Stage 3 gate stays open;
  Stage 4 is not started. All pre-existing uncommitted work is preserved;
  replaced wood/ore artifacts were copied into `rejected-r2/` before export.

### 2026-09-26 — Stage 3 resource visuals -> second implementation rejected

- Owner/surface: current Stage 3 agent; resource appearance and production
  approach. The owner rejected all three replacement models after seeing
  actual Game frames. Neither runtime iteration is accepted art.
- State/contracts: existing assets, uncommitted work and technical evidence
  are preserved. No asset deletion, rollback, gameplay change or new model
  production accompanies this status update. D-037/D-038 remain the selected
  concepts unless the owner revises them.
- Owner clarification: the A/B/A concepts are correct; the in-game execution
  is the failure. Active surface is now stone A only: authored major planes,
  front-facing fragment composition in the actual camera, sparse structural
  fractures and a light granite palette. Independent review produced an unused
  isolated experiment under `Artifacts/S3-RESOURCE-SOURCES/`; that reviewer
  did not edit runtime assets, the main generator or project documents.
- Stone review candidate: corrected the authored front for the unchanged
  FBX/prefab pivot chain, compacted/widened/lowered the composition and flattened
  its pointed crown. The existing stone FBX, `.blend`, albedo and normal paths
  now contain the new candidate; their `.meta` GUIDs and Economy bindings are
  preserved. New original imagegen granite source/prompt/provenance are in
  `art/source/stone_granite_painted_albedo{.png,_prompt.txt}`; only its baked
  atlas ships. No concept pixels or third-party assets are copied.
- Verification: 5832 triangles, one renderer/material, footprint about
  1.44×1.05 and height 1.03 cell; actual camera frames at unchanged zoom 5/2.5
  are `art/review/resource_stone_game_{default,close}.png`. Final PlayMode
  36/36 passed in 35.50 s, including paid full run/restart and resource lifecycle.
  Evidence is `Artifacts/S3-RESOURCE-SOURCES/playmode.xml`. The new candidate
  has not been visually accepted; wood and ore were not regenerated this pass.
- Remaining: resolve the visual mismatch on one representative object before repeating
  production across all three. Existing 36/36 tests establish behavior only.
  Resource art acceptance and standalone performance remain open; D-032
  undead, HUD/audio and hero last follow later. Stage 3 gate open, Stage 4
  not started.

### 2026-09-26 — Stage 3 second resource implementation -> technically checked, art rejected

- Owner/surface: current Stage 3 agent; three resource Blender sources,
  exported geometry/textures, resource prefab materials and actual Game review.
  Dependencies: approved D-037/D-038 concepts and existing Economy bindings.
- Owner feedback: first runtime models look excessively low-poly and do not
  meet the approved concepts. Their technical pass below is not art approval.
  The approved A/B/A choices remain in force. Replacements now have broad
  weathered rock surfaces, softened edges, baked cracks/rust and individual
  rounded conifer boughs with bark. The source script, editable Blender files,
  FBXs and prefabs retain the existing asset paths and gameplay contracts.
- Implementation: stone 28172, wood 51620, ore 37478 triangles; one renderer
  and one material each. Original 2048 albedo atlases for all three and 2048
  tangent normal maps for stone/ore. `Stage3ResourceSourceBuilder` configures
  sRGB/mipmap/normal imports. Foliage atlas padding and background prevent
  black mip bleed; ordinary receiving/casting shadows remain enabled.
- Verification: final PlayMode 36/36 passed in 43.70 s, including material
  import, source dimensions/anchors, depletion/recovery, paid three-wave win
  and restart (`Artifacts/S3-RESOURCE-SOURCES/playmode.xml`). Actual playable
  camera frames at zoom 5/2.5 are
  `art/review/resource_{stone,wood,metal}_game_{default,close}.png`.
  Final Editor render/readback medians were 8.932–10.866 ms with the source,
  9.641–11.167 ms without; p95 outliers reached 87.258 ms. This noisy local
  comparison does not establish release FPS; standalone performance remains
  degraded/pending for the Stage 3 gate. Updated Windows Development Build
  succeeded with 0 errors and 3 warnings
  (`Artifacts/S3-RESOURCE-SOURCES/build.txt`); final asset/meta checks passed
  and no new attributable Editor errors were found. The playable scene was
  reopened for review after build.
- Remaining: owner review of the actual replacement models, stable standalone
  performance review, then the D-032 undead, HUD/audio and hero last. Stage 3
  gate open; Stage 4 not started. Earlier uncommitted work is preserved;
  rejected resource artifacts were also copied under
  `Artifacts/S3-RESOURCE-SOURCES/rejected-r1/` before replacement.

### 2026-09-26 — Stage 3 first resource models -> technically integrated, art rejected

- Owner/surface: current Stage 3 agent; D-032 resource visuals in
  `art/source/build_resource_sources.py`, `Assets/Stage3/VisualPilot/**`,
  playable Stage 3 scene, related PlayMode tests and documentation. Depends
  on D-037/D-038 selections, existing Economy lifecycle and D-034 map.
- Behavior/contracts: the approved stone A granite boulder, wood B conifer
  and iron-ore A source are separate Blender FBX/prefabs under the existing
  wood/stone/metal markers. `Stage3ResourceSourceBuilder` creates materials and
  prefabs; `Stage3TacticalPresentation` binds and grass-aligns them. Each has
  one renderer and no collider. Source cells, gathering, stock recovery,
  navigation, buildability and run rules are unchanged. Source concepts remain
  review images only; original geometry is reproducible from the Blender
  script and editable `.blend` files.
- Verification: Blender export reports 1280/634/640 triangles for stone/wood/
  ore. Actual Game camera captures at zoom 5 and 2.5 are
  `art/review/resource_{stone,wood,metal}_game_{default,close}.png` and show
  distinct silhouettes in the playable scene. PlayMode 36/36 passed, including
  source dimensions/anchors, depletion/recovery, full paid three-wave win and
  restart (`Artifacts/S3-RESOURCE-SOURCES/playmode.xml`). The 1280×720 Editor
  render/readback comparison is in
  `Artifacts/S3-RESOURCE-SOURCES/render-comparison.txt`; local medians varied
  by less than 0.5 ms on/off per source after warmups, and are not release FPS.
  Windows Development Build succeeded with 0 errors and 3 warnings
  (`Artifacts/S3-RESOURCE-SOURCES/build.txt`); no new attributable Editor
  errors were found after build.
- Remaining: owner can review the three actual Game frames; complete the
  D-032 baseline undead, HUD, audio and hero last, then final full-run and
  performance review. Stage 3 gate open; Stage 4 not started.

### 2026-09-26 — Stage 3 D-036 former zoom scale -> implemented, gate open

- Owner/surface: current Stage 3 agent; tactical Stage 3 scene camera setup,
  its zoom regression and current Game capture. Depends on D-026 wheel
  behavior and the D-034 100×100 map.
- Behavior/contracts: `Stage3RapidPilotBuilder` again configures the Stage 3
  camera with opening orthographic size 5. `Stage2CameraFollow` retains its
  0.5×..2× clamps, making the actual bounds 2.5–10. Angle, centered follow,
  smooth wheel, Inspector step setting, map cells, gameplay rules and all
  existing uncommitted work remain preserved. The player chose this scale
  after being told the portal and objective begin outside the resource-area
  hero's frame.
- Verification: full Stage 3/Integration/RunState PlayMode 34/34 passed,
  including exact near/far bounds, WASD zoom stability, paid three-wave win
  and restart. Evidence: `Artifacts/S3-ENTRANCE-PORTAL/zoom-restored-playmode.xml`.
  Windows Development Build succeeded with 0 errors and 3 warnings. Actual
  starting Game-camera view: `art/review/stage3_zoom_restored_default_game.png`.
- Remaining: owner assesses the close framing and whether the defense corner
  can be found without explanation. Then continue only the D-032 accepted
  model/UI/audio set, with image approval before each new model; hero last.
  Stage 3 gate is open, Stage 4 has not started.

### 2026-09-26 — Stage 3 portal circulation -> implemented, gate open

- Owner/surface: current Stage 3 agent; portal shader/material, narrow
  presentation clock, actual-render regression and explicit review recording.
  Dependency: the approved B4 red opening and D-035 placement. The owner
  reported a static-looking interior and requested visible circulation.
- Behavior/contracts: `EntrancePortalVortex.shader` rotates the red strands
  with bounded inward distortion; speed lives in `EntrancePortalVortex.mat`
  and is reproduced by `Stage3EntrancePortalBuilder`. `Stage3EntrancePortal`
  honors explicit pause and keeps the ambient vortex alive after an outcome
  or in an unbound Play-mode preview. Game rules, 45-degree ring and 3x3
  foundation remain independent of the effect.
- Verification: all 34 Stage 3/Integration/RunState PlayMode checks passed,
  including full paid victory/restart. Actual red-pixel comparisons detected
  2,870 changed pixels over 0.4 seconds, zero on pause, and continued motion
  after victory. Evidence: `Artifacts/S3-ENTRANCE-PORTAL/portal-circulation-playmode.xml`.
  Windows Development Build succeeded with 0 errors and 7 warnings; the
  visible Unity Console has no errors. The explicit `motion` review captures
  90 actual Game camera frames over six simulation seconds, preserving time
  and camera settings afterward. The resulting GIF/MP4 recordings were later
  removed as unnecessary for the project handoff.
- Remaining: owner review of presentation and overall map composition, then
  the D-032 baseline undead, three resource models, hero last, HUD and sounds.
  Each new model needs its specific image approval. Stage 3 gate is open;
  Stage 4 has not started. Existing uncommitted work is retained.

### 2026-09-26 — Stage 3 D-035 portal at 45 degrees -> implemented, gate open

- Owner/surface: current Stage 3 agent; approved B4 portal Blender source,
  exported FBX/prefab, tactical placement and portal PlayMode regression.
- Dependency/intent: after the D-034 diagonal map, the player requested the
  enemy gate at 45 degrees. Its exit remains toward the objective, with the
  red vortex and intact dark ring already approved in D-033.
- Behavior/contracts: `art/source/build_entrance_portal.py` and the editable
  `.blend` turn the upright ring, buttresses, steps and aperture 45 degrees
  inside the unchanged 2.945x2.945 square foundation. The FBX, baked stone
  texture and `EntrancePortal.prefab` were rebuilt without changing asset
  GUIDs, spawn/navigation rules or the nine-cell building exclusion.
  `Stage3RapidPilotTests` checks the diagonal aperture extents, foundation,
  spawn/movement, pause/reset and exclusion. The actual Game frames are
  `art/review/stage3_diagonal_portal_45_game_overview.png` and
  `art/review/stage3_diagonal_portal_45_game_close.png`.
- Verification: focused PlayMode 1/1 and full Stage 3/Integration/RunState
  PlayMode 34/34 passed, including the paid three-wave win and restart.
  Evidence: `Artifacts/S3-ENTRANCE-PORTAL/portal-45-playmode.xml`.
  Windows Development Build succeeded with 0 errors and 3 warnings. Model:
  3 meshes, 6,860 triangles, 0 colliders, world ground extents 2.945x2.945.
  Local Editor 1600x900 Camera.Render/readback medians on GTX 1060 were
  14.850ms on and 15.237ms off; these are not standalone FPS. No new
  attributable console errors observed.
- Remaining: owner reviews the combined D-034 map composition. Then continue
  only the D-032 enemy, three resources, hero last, HUD and sound set, with
  specific image approval before each new model. Stage 3 gate stays open;
  Stage 4 has not started.

### 2026-09-26 — Stage 3 D-034 diagonal 100×100 map -> active

- Owner/surface: Stage 3 planar map data, playable 3D scene composition,
  tactical camera start, temporary actor placement, and relevant PlayMode
  checks. Existing Stage 2 map/scene assets and approved model shapes stay
  with their owners.
- Dependency/intent: player supplied a screen composition and confirmed a
  100×100 map with a gray strip about 30 cells long and 12 across, descending
  from the upper right corner. Portal and objective sit along it; three
  resource types remain in green. The short three-wave run remains the check.
- Acceptance: actual game frame reflects the diagonal corner; both 3×3
  landmarks fit and remain legible, hero/resources are reachable, gray-only
  construction agrees with rendered ground, enemies reach the objective,
  paid defense can win all waves and restart, and map/render overhead is
  measured. Preserve unrelated uncommitted work.
- Outcome/contracts: `Stage3PlanarCorner.asset` holds the new map;
  `Stage3DiagonalMapBuilder` reproduces it. `Stage3RapidPilotBuilder` positions
  the hero and opens a wider fixed camera. The portal foundation turns by a
  grid-aligned quarter turn toward the new objective, keeping its 3x3 square
  base within the reserved cells; D-035 angles the aperture within it.
  `Stage3TacticalPresentation` uses one grass
  quad per cell on this large map and extends only the non-walkable visual
  border for maximum zoom. Stage 2 assets are retained.
- Verification: 34/34 Stage 3/Integration/RunState PlayMode tests pass,
  including the map, resources, gray-only placement, enemy movement, paid
  three-wave victory and restart, and zoom limits. Evidence:
  `Artifacts/S3-ENTRANCE-PORTAL/diagonal-map-final-playmode.xml`.
  Windows Development build succeeded with 0 errors and 7 obsolete-API
  warnings. Actual 1600x900 Game camera capture:
  `art/review/stage3_diagonal_map_game_overview.png`. Local Editor render and
  readback median is 17.7 ms on GTX 1060, a bounded observation rather than a
  standalone frame budget. The initial far-zoom border test failure was fixed
  and rerun; no new attributable console errors remain.
- Remaining: owner reviews the actual composition against the supplied
  mockup. If accepted, continue the agreed Stage 3 model/UI/audio set with
  specific image approval before each new model; hero stays last. Stage 3
  gate is open; no Stage 4 work started.

### 2026-09-26 — Stage 3 entrance portal B4 -> 3x3 placement corrected

- Owner/surface: Stage 3 portal Blender source, FBX/materials/prefab, tactical
  presentation binding and focused regression; other uncommitted work retained.
- Dependency/intent: player approved intact dark stone/red vortex B4 and
  explicitly requested modeling (D-033). Existing map entrance and run rules
  remain the source of spawn and navigation behavior.
- Acceptance: recognizable portal at tactical camera sizes, intact masonry,
  red animated center, owner-requested 90-degree turn and 3x3 base/exclusion,
  unchanged spawn/path, valid placement outside the base, pause/reset, full run and no new
  errors in a fresh Play session; model overhead measured locally.
- Contracts/files: `build_entrance_portal.py`/`.blend`, portal FBX, textures,
  shader and prefab; `Stage3EntrancePortalBuilder`, tactical presentation,
  `Stage3PlanarCorner.asset` and portal/full-run PlayMode checks. Base is
  2.945x2.945, with a centered nine-cell building exclusion in authored map
  data. The quarter turn faces the +X objective route; no collider or game-rule
  dependency on the mesh was added.
- Verification: 33/33 PlayMode checks passed after rebuilding the new map,
  including all three waves to victory with paid defense and restart,
  spawn/movement, all nine excluded cells, walkability, bounds, pause/reset
  and existing integration/run-state regressions. Evidence:
  `Artifacts/S3-ENTRANCE-PORTAL/placement-3x3-playmode.xml`.
  An earlier stale-map run failed the exclusion assertion; two zero-test
  runner results were discarded. Explicit script reload fixed test discovery.
  Windows development build succeeded (0 errors, 7 warnings); the fresh
  tactical capture confirms 3 meshes/6,860 triangles, no colliders and a
  2.945x2.945 footprint. The exact 90-degree turn makes the aperture edge-on
  to the fixed camera. After seeing that exact tactical screenshot and the
  alternative directions, the owner explicitly chose to retain the exit
  toward the objective. D-035 subsequently changed the exact bearing to 45
  degrees while keeping that destination.
  Local Editor render/readback medians: portal on 9.845ms, off 10.056ms;
  this noisy bounded comparison is not a minimum-target frame-budget result.
  Standalone startup logged gameplay and tactical readiness with no error or
  exception. Its hidden-window screenshot was black and was discarded as
  visual evidence; the reviewed images are from the actual Editor Play scene.
  Remaining: baseline undead enemy, three resources, hero last, accepted HUD
  and sounds, with specific image approvals before each new model and playable
  validation. The requested portal placement correction is complete; Stage 3
  gate stays open and Stage 4 has not started.

### 2026-09-26 — Stage 3 minimal MVP scope accepted -> visual/UI/audio work

- Active owner/surface: Stage 3 agent; remaining visual models in the playable
  `Stage3Rapid3DGamePilot`, its HUD and sound presentation. Dependencies are
  D-027/D-030/D-031, current map/economy/defense/run contracts and the
  individually approved concept for each new model.
- Outcome: player accepted D-032's minimal set: current short run, existing two
  towers and soul objective, then entrance portal, one baseline enemy, three
  resource sources, and hero last; finished run HUD and short cues for gather,
  build, both attacks/impacts, objective danger, wave and outcome. No new game
  mechanic was accepted. Existing hero, enemy, portal and resources are still
  temporary solids; the only HUD is development-only IMGUI; no gameplay audio
  assets or playback were found. Portal is the next concept candidate.
- Contracts/files: `production/DECISIONS.md` D-032 and Stage 3 in
  `gamedesign/DEVELOPMENT_PLAN.md`; runtime contracts and scene untouched.
- Verification: repository asset/code/scene/package and Git-status audit;
  earlier accepted soul-objective 11/11 PlayMode evidence remains recorded
  above. No runtime edit in this handoff, so tests/build were not rerun.
- Remaining: get portal concept approval before Blender/Unity; repeat that
  approval and playable-scene scale/silhouette/performance check for each
  remaining model, implement accepted HUD and sounds, verify full run and
  relevant regressions, then seek player acceptance of the Stage 3 gate.
  Stage 4 remains closed and belongs to a separate agent.
  Portal review options: `art/concepts/entrance_portal_option_a_violet_arch.png`
  and `entrance_portal_option_b_ember_ring.png`; no model was built from them.
  Player selected B's direction but rejected its broken stonework; intact
  revision `art/concepts/entrance_portal_option_b2_intact_ember_ring.png` is
  awaiting specific approval before production.
  Player then specified a darker entrance for an undead baseline enemy. The
  intermediate review candidate was
  `art/concepts/entrance_portal_option_b3_undead_ring.png`; no new enemy rules
  or runtime art have been added.
  The player restored the red interior; current review candidate is
  `art/concepts/entrance_portal_option_b4_dark_red_ring.png`, pending approval.

### 2026-09-26 — Stage 3 soul objective revision 3 after owner rejection

- Active owner/surface: Stage 3 objective model, shaders, prefab, tactical scene
  binding and the owner-requested 3x3 building exclusion. Depends on the D-031
  approved concept and the existing Stage 2 center-cell throne contract.
- Acceptance: separate stone bowl/anchors, soul sphere with readable blue flame,
  transparent smooth protective dome and horizontal external flowing strips;
  about 3x3 cells, readable from the existing tactical camera, no visual-driven
  combat rules. Initial small uniformly lit result and vertical orbit rejected.
- Changes: original `build_soul_sphere.py`, Blender/FBX, baked 1024² slate atlas,
  `SoulSphereIceDome.prefab`, smooth-ice/flame/wave shaders and materials,
  `Stage3SoulSphereMotion`, builder and scene bloom profile. Raised bowl and
  four anchors have real geometry. Stone face winding corrected. Orbit uses
  world -Z, independent of imported FBX local axes; solids remain stationary.
  Nine cells are reserved in `Stage3PlanarCorner.asset`; target, health and
  movement contracts remain unchanged. All earlier uncommitted work preserved.
- Verification: 11/11 Stage 3 PlayMode tests pass, including full run/win/restart,
  building rejection across all nine cells and more than a complete orbit with
  constant wave altitude. Evidence: `Artifacts/S3-SOUL-SPHERE/playmode.xml`.
  Final material tuning inspected in actual Unity captures
  `art/review/soul_sphere_game_close.png` and `soul_sphere_game_overview.png`.
  Console 0 errors/0 warnings after fresh Play start; shaders supported.
  Runtime objective: 26,796 triangles, six renderers, one shadowless point light.
  One local Editor Stats observation: 317.9 FPS, total 3.1 ms/GPU 1.9 ms,
  442.5k scene triangles, 113 draws at roughly 1100x506. No controlled build
  baseline; standalone/minimum-target performance remains a Stage 3 gate item.
- Review tooling: `Stage3SoulSphereReview` consumes explicit local
  `Artifacts/S3-SOUL-SPHERE/review.request` (`validate` or `open`), rebuilds the
  pilot, optionally runs its PlayMode suite, then opens Play and captures only
  after the model exists and shader compilation finishes. No background task
  remains after the request. Sources and generated review files stay local.
- Owner visually approved revision 3 after the playable scene review.
- Remaining: agree the minimal commercial MVP content; produce only its needed
  UI/audio and remaining models with individual visual approvals, then check
  target-build performance and the complete playable run. Hero remains last.
  Stage 4 stays closed until the Stage 3 gate is accepted by the owner.

### 2026-09-26 — Stage 3 soul sphere model in playable scene (rejected revision 1)

- Owner/surface: loss-critical objective visual only. D-031 selected the fourth
  concept and authorized modeling. No health, occupancy or rule changes.
- Output: original project geometry from `art/source/build_soul_sphere.py` and
  `soul_sphere_ice_dome.blend`; `Assets/Stage3/VisualPilot/SoulSphereIceDome.fbx`,
  six URP materials, `SoulSphereIceDome.prefab`, `Stage3SoulSphereMotion`, and
  the `soulSpherePrefab` binding in `Stage3Rapid3DGamePilot.unity`. Source is
  project-owned original Blender geometry derived from the AI-generated
  approved concept; no third-party mesh, texture or material was shipped.
- Verification: Blender export/render succeeded (4183 vertices, 7452 triangles,
  six mesh renderers); interactive Game Play Mode shows the sphere, dome and
  rotating orbit at the authored throne cell beside both towers. Unity Console
  showed zero errors and warnings in that review. Stats showed about 383k scene
  triangles and 300 FPS on the local machine; no controlled baseline measured.
- Remaining: owner review of the in-game model; full run, relevant PlayMode
  regression and final screenshot after shader warm-up before Stage 3 gate.

### 2026-09-26 — Stage 3 central soul sphere correction

- Owner/surface: visual concepts for the loss-critical objective only. The
  player specified a sphere containing the hero's soul in the center.
- Output: `art/concepts/throne_soul_sphere_energy_dome.png` and
  `art/concepts/throne_soul_sphere_ice_dome.png` show that sphere beneath a
  transparent blue dome with circular energy strips. Prompts are in
  `art/source/throne_soul_sphere_concept_prompts.txt`. Previous dome-throne
  concepts are rejected historical sketches.
- Verification: both generated images visually inspected and saved. No Blender
  model, Unity asset, scene, gameplay rule or test changed.
- Remaining: player visual approval of the corrected concept before modeling
  and playable-scene integration. Stage 3 gate remains open.

### 2026-09-26 — Stage 3 dome-throne concept replaces smooth obelisk

- Owner/surface: visual concepts for the player's loss-critical throne only.
  The player redirected the design before model production.
- Output: two original built-in imagegen concepts in `art/concepts/`:
  `throne_dome_option_energy.png` has a transparent cyan force dome;
  `throne_dome_option_ice.png` has a smooth translucent ice dome. Both place a
  smooth blue throne on a compact circular dais with broken orbiting energy
  arcs and no front-facing soul emblem. Exact prompts are in
  `art/source/throne_dome_concept_prompts.txt`. The supplied gameplay screenshot
  is a motion/composition reference only; its architecture, statues and UI are
  not reused. The A4 obelisk remains a historical review image.
- Verification: images visually inspected. An unfinished unapproved Blender
  script started before the direction change was removed. No runtime Unity
  asset, scene, gameplay rule or test changed.
- Remaining: player chooses the dome and central form before Blender modeling,
  Unity integration and a playable-scene check. Stage 3 gate remains open.

### 2026-09-26 — Stage 3 ice throne smooth-monolith revision

- Owner/surface: visual concept only for the player's loss-critical objective.
- Output: `art/concepts/throne_ice_option_a4_smooth_monolith.png` removes the
  previous monolith's small facets, cracks and busy ice texture. It keeps one
  continuous softly sculpted cone, with the embedded soul prism as the sole
  detailed focal point. Exact built-in imagegen edit prompt is appended to
  `art/source/throne_ice_concept_prompts.txt`.
- Verification: image-only visual review. No Blender, Unity asset, scene,
  gameplay rule or test changed.
- Remaining: player visual approval before model production or integration.

### 2026-09-26 — Stage 3 ice throne homogeneous-monolith revision

- Owner/surface: visual concept only for the player's loss-critical objective.
- Output: `art/concepts/throne_ice_option_a3_monolith.png` revises A1 into one
  continuous tapered ice body. All satellite, front and crown-cluster shards
  are removed; the soul prism remains embedded in the monolith, held by the
  existing stone altar and metal retainers. Exact built-in imagegen edit prompt
  is appended to `art/source/throne_ice_concept_prompts.txt`.
- Verification: image-only visual review. No Blender, Unity asset, scene,
  gameplay rule or test changed.
- Remaining: player visual approval before model production or integration.

### 2026-09-26 — Stage 3 ice-obelisk narrative revisions

- Owner/surface: visual concept only for the player's loss-critical objective.
  The player selected original option A as the closest direction and proposed
  a possible story motivation: an evil sorceress imprisoned the hero's soul in
  the ice, so breaking it kills the hero. This remains a visual premise until
  explicitly accepted as canon.
- Output: `throne_ice_option_a1_soul_prism.png` strengthens hierarchy with a
  clean visible soul prism, metal seals and one energy ring;
  `throne_ice_option_a2_witch_reliquary.png` adds a cursed violet fracture,
  clamps and chains. Both preserve the chosen conical obelisk and one-cell base.
  Exact built-in imagegen edit prompts are appended to
  `art/source/throne_ice_concept_prompts.txt`.
- Verification: image-only visual review. No Blender, Unity asset, scene,
  gameplay rule or test changed.
- Remaining: player selects one revision or requests a blend before any model
  production or playable-scene integration.

### 2026-09-26 — Stage 3 ice-throne concepts awaiting selection

- Owner/surface: visual concept only for the player's loss-critical throne;
  its gameplay role and rules are unchanged.
- Output: three original imagegen options in `art/concepts/`: A is an ice-crown
  obelisk, B is a recognizable empty throne with a high tapered crystal back,
  and C is a glacial core shrine. Approved Rapid and landscape images were
  used only as style and mood references. Exact prompts are recorded in
  `art/source/throne_ice_concept_prompts.txt`.
- Verification: image-only review; each option is a single readable object
  with a broad base and isometric presentation. No Unity assets, Blender file,
  scene, rules or tests changed.
- Remaining: player selects or revises one concept. Blender production and
  playable-scene integration require that visual approval first.

### 2026-09-26 — Stage 3 landscape parked by player

- Owner/surface: Stage 3 content order; no Unity asset or gameplay edit.
- Outcome: the player chose to leave the current landscape in place for now
  and move on. `DEVELOPMENT_PLAN.md` records this as a pause, not final visual
  acceptance. Existing build/capture and prior 49/49 verification remain the
  latest evidence; no tests rerun for this documentation-only update.
- Remaining: agree the minimum commercial MVP models, UI and sound before
  further production; review the landscape again before Stage 3 gate. The hero
  model remains last.

### 2026-09-25 — Stage 3 landscape closer to concept B

- Owner/surface: Stage 3 playable landscape only. The player said the previous
  runtime frame improved but remained far from approved concept B.
- Behavior/contracts: original grass and compacted-earth albedos regenerated
  from concept B as a style reference; exact prompts in `art/source/`. The
  earth keeps fine cell guides and flush mineral flecks. Grass now has shallow
  deterministic visual relief with smooth normals, marker/foliage height
  alignment, and a wider irregular slope into gray earth. `Stage3TacticalPresentation.cs`,
  `Stage3LandscapeProps.cs`, two runtime textures, the material builder, and
  the focused PlayMode check changed. Bushes stay removed; no colliders or
  gameplay rules changed. The old procedural earth script produces only a
  fallback outside runtime Assets.
- Verification: isolated Unity 6000.6.2f1 PlayMode 49/49 including complete
  run, two paid towers, no-collider terrain and map rule checks. Windows x64
  Development build succeeded and is running from
  `Builds/Windows/Stage3LandscapeConceptB`; capture:
  `art/review/landscape-concept-b-game.png`. Player log has no attributable
  errors; runtime reports 0 bushes, 10,099 combined foliage triangles and 0
  foliage colliders. `git diff --check` passed.
- Remaining: player visual acceptance and quantitative frame cost. Commercial
  MVP model/UI/audio scope and Stage 3 gate remain open; final hero model last.

### 2026-09-25 — Stage 3 concept B grass and soft gray-zone verge

- Owner/surface: Stage 3 playable landscape. The player pointed back to the
  approved concept B image and requested that grass texture and its soft,
  irregular transition on every gray-zone side, with no bushes.
- Behavior/contracts: a new original `LandscapeGrass.png` was generated with
  concept B as a style reference (prompt at
  `art/source/landscape_grass_prompt.txt`). `TacticalGrass.mat` repeats it at an
  eight-cell scale. `Stage3TacticalPresentation.cs` lowers the visual grass
  lift to 0.08 cell and replaces the straight dark sidewall with a textured
  sloping verge on each forest/gray boundary. `Stage3LandscapeProps.cs` puts
  real grass fringe at the outer edge; no bush meshes or materials return.
  The shared rules plane and gray-only tower placement stay unchanged.
- Verification: isolated Unity 6000.6.2f1 PlayMode 49/49 including full run,
  grass-verge geometry, no foliage colliders, two paid towers and existing map
  rules; 1/1 focused check passed after the final grass material scale edit.
  Windows x64 Development build succeeds. Running build:
  `Builds/Windows/Stage3LandscapeConceptB`; actual game capture:
  `art/review/landscape-concept-b-scale-game.png`; player log has no new
  attributable errors. Main/built scene, scripts, material and texture hashes
  match; `git diff --check` passes.
- Remaining: player visual acceptance and quantitative frame-cost check if
  accepted. Commercial MVP model/UI/audio scope and overall Stage 3 gate are
  open. The final hero model remains last.

### 2026-09-25 — Stage 3 raised grass edge, rejected bushes removed

- Owner/surface: Stage 3 playable landscape presentation. The player rejected
  the procedural bushes and asked for grass visibly above compacted gray earth
  as in concept B. No new model or gameplay system is introduced.
- Behavior/contracts: `Stage3LandscapeProps.cs` now generates only grass blades
  and flowers, with denser real 3D blades along the green/gray boundary. Bush
  methods, scene bindings and their unreferenced material assets were removed.
  `Stage3TacticalPresentation.cs` renders green cells 0.18 cell above gray and
  builds a shallow soil sidewall, while lifting hero/resource scale markers to
  match; the scene and builder use the reduced material set. Navigation,
  movement, ray clicks, combat and tower placement still use the unchanged
  flat map coordinates. D-011/D-030 and tactical/art specs record the visual
  exception; no collider was added.
- Verification: isolated Unity 6000.6.2f1 PlayMode 49/49 including full run,
  followed by 1/1 focused edge geometry check after the fringe density edit.
  Windows x64 Development build succeeded. The actual game capture is
  `art/review/landscape-raised-grass-final.png`; running player
  `Builds/Windows/Stage3LandscapeRaisedGrass` has no attributable log errors.
  Runtime reports 0 bushes, 3 shared foliage meshes, 7487 triangles and 0
  foliage colliders. `git diff --check` and main/isolated input hashes checked.
- Remaining: player visual review and quantitative frame cost if the terrain
  is accepted. Stage 3 commercial MVP model/UI/audio scope and gate remain
  open; hero model remains last.

### 2026-09-25 — Stage 3 landscape visual correction

- Owner/surface: Stage 3 landscape presentation in the playable 3D scene.
  The player rejected the first texture-only pass as blurry and distant from
  selected concept B. Hero, towers, combat rules and map masks remain owned by
  their existing surfaces.
- Behavior/contracts: `LandscapeGrass.png` now carries original text-prompt
  generated painted turf, imported uncompressed with full NPOT resolution.
  `Stage3LandscapeProps.cs` adds separate rounded bush, leaf, grass-blade and
  flower meshes on green terrain/border; five combined renderers, no colliders
  or rule references. `Stage3TacticalPresentation` and the scene bind the new
  materials. The procedural source script generates only gray earth now;
  `STAGE3_TACTICAL_VIEW.md` and `ART_DIRECTION.md` record provenance.
- Verification: isolated Unity 6000.6.2f1 PlayMode 49/49 including full run,
  map-rule and no-collider checks; Windows x64 Development build succeeded.
  `Builds/Windows/Stage3LandscapePolished` runs and responds, with both paid
  towers visible; `Artifacts/S3-RAPID-PILOT/landscape-polished-player.log` has
  no attributable errors. Actual capture: `art/review/landscape-polished-game.png`.
  Five shared meshes contain 44,245 triangles total, 0 colliders;
  `git diff --check` passes.
- Remaining: the player must judge the revised look in the game. Measure
  frame cost after visual acceptance and adjust only if needed. Commercial MVP
  model/UI/audio scope and the overall Stage 3 gate remain open; the final
  hero model is still last.

### 2026-09-25 — Stage 3 landscape concept selection

- Owner/surface: Stage 3 visual landscape planning. The player deferred the
  final hero model until the end; the current basic hero marker remains.
- Input: player-supplied bright grass, low bushes and small white flower
  reference at `art/references/landscape_player_grass_reference.png`; unknown
  source license, inspiration only. Current game screenshot provided scale and
  camera composition. No external image is runtime-ready.
- Output: two imagegen review concepts in `art/concepts/landscape_option_a_stone_paving.png`
  and `landscape_option_b_packed_earth.png`. A uses weathered stone paving in
  the gray zone; B uses compacted earth/slate chips. Both explore the grassy
  green zone. Player selected B and required real 3D props for any raised
  bushes/rocks; omitting these in the first pass is accepted.
- First pass (subsequently rejected): original seamless turf and compacted earth textures via
  `art/source/build_landscape_textures.py`, continuous four-cell UVs in
  `Stage3TacticalPresentation`, updated materials and builder bindings. No
  fake-relief props are painted into the textures. Scene/map rules and prefabs
  are unchanged; the existing terrain mesh and draw-call structure are reused.
- Verification: PlayMode 48/48 including full run, catapult aiming, camera and
  map bounds. Main and tested landscape inputs matched by hash. Windows x64
  Development build `Builds/Windows/Stage3LandscapeB` launched with both
  purchased towers; `Artifacts/S3-RAPID-PILOT/landscape-player.log` has no
  attributable errors. Actual-player capture: `art/review/landscape-b-game.png`.
  Two 512-square maps replace two 128-square maps; no raised prop geometry was
  added. Release GPU timing was not separately measured.
- Historical remaining work: the texture-only pass awaited review; the player
  rejected it, and the newer entry above records the visual correction.

### 2026-09-25 — Stage 3 centered rotation of the catapult inner mechanism

- Owner/surface: Stage 3 visual aiming, following the player's annotated
  screenshot. Existing model, textures, material, camera and prior uncommitted
  changes are preserved. No additional content is produced.
- Behavior: an explicit `Catapult centered yaw` pivot lies on the vertical
  line through the occupied cell center. Imported frame/axle/arm/cup move
  together below that pivot; masonry and parapet remain outside it. Forward
  comes from the imported cup and axle positions. Between throws the mechanism
  continuously tracks the nearest living enemy; the throwing arm retains its
  separate axle motion. Pause freezes tracking and reset cleans it up.
- Contracts/files: `Stage3TowerShotMotion` and combat presentation; read-only
  `Stage2Tower.AttackRange` and `Stage2EnemyGroup.FindNearest` reuse combat's
  existing target selection. Aiming cannot fire, reserve targets or deal damage.
  Catalog values, damage timing and shots are unchanged; no Inspector migration.
- Verification: EditMode 79/79 and complete PlayMode 48/48, including full run/restart and a new
  four-bearing check of fixed pivot, frame height/radius, fixed foundation and
  parapet, live-target tracking without shots, and pause/reset. Preview versus
  placed bounds allow 0.0001 world units for float rounding during the
  world-preserving reparent. Evidence: `Artifacts/S3-RAPID-PILOT/catapult-yaw-*`.
  Windows build `Builds/Windows/Stage3CatapultYaw` launched; player startup and
  both tower bindings are ready, with no attributable errors. Actual gameplay
  captures `art/review/catapult-yaw-stone-windup.png`, `catapult-yaw-stone-release.png`
  and `catapult-yaw-stone-flight.png` show the mechanism inside the fixed base.
  Main Assets/Packages/ProjectSettings match the tested build inputs by hash.
- Acceptance: player inspected the restarted playable build and accepted the
  revised aiming on 2026-09-25.
- Remaining: agree commercial MVP model/UI/audio scope, produce only approved
  content, then verify the full run. Stage 3 gate remains open; Stage 4 waits.

### 2026-09-25 — Stage 3 player corrections to tower animation

- Owner/surface: Stage 3 combat presentation and removal of obsolete tower
  shot tint from `Stage2Tower`. Existing assets, camera, catalogs and all prior
  uncommitted changes are preserved. No new model or package was introduced.
- Behavior: both towers retain their authored colors; Rapid's model remains
  completely still. Catapult aims from its current bearing, winds back with
  the stone still loaded, releases at the cup socket, follows through and
  smoothly returns/reloads. The existing shot clock drives the pose and release;
  windup fits within the original flight duration, so damage timing is unchanged.
  Enemy damage feedback remains. No Inspector reassignment is required.
- Contracts/files: `Stage3TowerShotMotion`, `Stage3CombatPresentation`, tactical
  binding, `Stage2Tower`, focused tests, tactical spec, D-029 and canonical plan.
- Verification: Unity 6000.6.2f1 EditMode 79/79 and PlayMode 47/47, including
  the full tactical run/restart. The presentation check asserts loaded stone
  during windup, no visible duplicate projectile, release within 0.002 world
  units of the cup, unchanged Rapid position/rotation/color, pause and reset.
  Evidence: `Artifacts/S3-RAPID-PILOT/combat-r2-*`.
  Windows build `Builds/Windows/Stage3CombatR2` launched with a clean player
  log. Actual player frames `art/review/combat-r2-stone-windup.png`,
  `combat-r2-stone-release.png`, `combat-r2-arrow-flight.png` verified distinct
  loaded/released phases and unchanged tower colors. `git diff --check` passes.
- Remaining: player evaluates corrected motion in the open build; Stage 3 gate and
  commercial MVP model/UI/audio scope remain open. Stage 4 awaits that gate.

### 2026-09-25 — Stage 3 tower firing and impact animation (D-029)

- Owner/surface: Stage 3 combat presentation with a narrow Defense/WavesAI/Run
  timing contract. The player requested firing and impact animation for both
  existing towers. Prior uncommitted changes and approved camera are preserved.
- Behavior: Rapid arrow/trail/recoil/impact sparks; Longshot aims the upper
  frame, releases its arm/bowl, throws a stone along an arc, returns and reloads.
  Stone impact emits fragments and a ring at the real area boundary. The
  catapult uses four shared mesh parts with the original atlas/material and
  unchanged triangle count; no new tower/character production was started.
- Contracts/files: `Stage2DefenseCatalog.projectileFlightSeconds` is authored
  in baseline and visual catalogs; Tower forwards it to EnemyGroup. The group
  owns pending shots and launch/impact/clear facts; Run advances them, freezes
  them on pause and clears them on reset. Damage resolves on arrival without
  any renderer/collider/animation event dependency. Dead targets are not
  retargeted; arrows do no extra damage, stone resolves at last known position.
  Wave completion waits for outstanding shots. Range/cadence/damage/costs stay.
  `Stage3CombatPresentation`/`Stage3TowerShotMotion` bind automatically through
  the tactical presentation; no manual Inspector step or package change.
- Verification: EditMode 79/79 and complete PlayMode 47/47, including full
  tactical run and visual flight/pause/impact/restart. A preview test exposed an
  extra 0.338-cell vertical offset in the aiming pivot; fixed in source and
  verified against the original bounds without relaxing the test. Shared mesh
  placement was checked with 24 catapults. This Editor diagnostic does not
  substitute for release GPU profiling. Logs/XML: `Artifacts/S3-RAPID-PILOT/combat-*`.
  Final Windows x64 Development Build is `Builds/Windows/Stage3Combat`; source
  Assets/Packages/ProjectSettings hashes matched the tested isolated copy.
  Final player launched; clean gameplay-ready log and refreshed actual-player
  captures `art/review/combat-arrow-flight.png`, `combat-stone-flight.png`,
  `combat-stone-impact.png` confirm trails, articulated pose and area impact.
  The player remains open; Space starts the next wave. `git diff --check` passes.
- Remaining: player review of animation readability/timing; commercial MVP
  model/UI/audio roster and representative release performance pass remain
  open. Stage 3 gate stays open; Stage 4 is reserved for a new agent after it.

### 2026-09-25 — Stage 3 bowl catapult model -> playable review

- Owner/surface: Stage 3 art and visual integration; dependencies are approved
  concept A, explicit Blender authorization and existing D-028 Longshot rules.
  Added original `art/source/longshot_catapult.blend`, reproducible script,
  atlas/FBX, material, prefab and visual catalog in `Assets/Stage3/VisualPilot`.
  Scene changes only bind the new Longshot catalog; prior camera settings and
  all pre-existing working-tree changes are preserved.
- Behavior: the player-owned second tower now shows masonry, timber frame,
  axle/rope, throwing arm and bowl with a stone. Showcase buys both roles
  through normal placement, Longshot left of Rapid. The visual catalog copies
  rule values from the baseline catalog; no geometry-based combat/navigation.
  Added `BuildCurrentGamePlayer` to build the saved scene without regenerating
  accepted camera/model settings. No manual Inspector wiring remains.
- Verification: isolated Unity 6000.6.2f1 PlayMode 46/46, including complete
  tactical run/restart, matching mesh preview/placement bounds and 24 paid
  copies sharing one mesh/material. Footprint 0.918 x 0.916 cells, height 1.581;
  22,968 triangles, one 2048-square atlas. Batchmode Editor frame diagnostics
  are not release GPU performance evidence. Prior rule EditMode 77/77 remains
  the unchanged gameplay baseline. Final Windows Development build succeeds;
  visible player logs startup, both towers and tactical ready without errors.
  Actual playable camera inspection shows both distinct silhouettes, correct
  one-cell bases and shadows; screenshot `art/review/longshot_in_game.png`.
  Evidence: `Artifacts/S3-RAPID-PILOT/catapult-*`; build/player in
  `Builds/Windows/Stage3Longshot`. `git diff --check` passes.
- Remaining: owner evaluates the finished model in the launched game; this
  does not imply visual acceptance. Arm/projectile animation is not implemented;
  existing attack flash remains. Agree commercial MVP model/UI/audio roster
  before further production; profile representative release load later.
  Existing interactive Editor hot-reload errors were not used as validation;
  the fresh build/test environment is clean. Stage 3 gate remains open;
  Stage 4 starts with a new agent only after that gate closes.

### 2026-09-25 — Stage 3 Longshot concept and D-028 rules checkpoint

- Owner/surface: Stage 3 visual-content proposal and Longshot catalog/attack
  contract; `art/concepts/`, Defense/WavesAI rules and focused tests, canonical
  Stage 3 plan, tower-role spec, art direction and D-028.
- Behavior: player chose the original stone catapult concept A, price 2 wood/5
  stone/0 metal and a two-cell blast dealing the current two damage to each
  enemy. Rapid remains single-target. Metal stays gatherable on the map.
  Blender modeling is explicitly held until separate player authorization.
- Verification: original working-tree changes preserved. Unity 6000.6.2f1
  isolated-copy EditMode 77/77 and PlayMode 45/45 passed, including the
  tactical-scene full two-role run and restart. Test logs/XML are under
  `Artifacts/S3-RAPID-PILOT/longshot-*`. No attributable C# errors; two
  expected invalid-map diagnostics came from negative tests. Windows x64
  Development Build succeeded; hidden headless player smoke logged Stage 3
  gameplay/tactical ready without attributable errors.
- Remaining: agree proposed commercial MVP model/UI/audio scope, then await explicit permission
  before Blender work. Once a model is accepted, check its scale, silhouette,
  camera readability, placement and many-tower cost in the playable scene.
  Stage 3 gate remains open; Stage 4 belongs to a new agent.

### 2026-09-25 — Stage 3 owner accepted runtime 3D (D-027)

- Owner/surface: Stage 3 product decision and visual-production handoff;
  `production/DECISIONS.md`, `gamedesign/GAME_VISION.md`,
  `gamedesign/ART_DIRECTION.md`, `gamedesign/DEVELOPMENT_PLAN.md` and this entry.
- Behavior: player accepted the playable Rapid watchtower and tactical camera,
  and explicitly chose runtime 3D over sprites rendered from 3D. Each future
  model still requires a role/look discussion and image concept approval
  before Blender modeling and Unity integration.
- Verification: player confirmation in chat; documentation-only decision,
  with no scene, runtime, asset or test changes in this entry.
- Remaining: agree commercial MVP asset scope and the next model (Longshot is
  the likely next tower), review its concepts with the player, then implement
  only accepted content. Stage 3 gate stays open; Stage 4 belongs to a new
  agent after that gate closes.

### 2026-09-25 — Stage 3 closer-view limit from player screenshots (D-026)

- Owner/surface: Stage 3 tactical camera near clamp, matching PlayMode check,
  and current visual/plan handoff. The player supplied two real game frames;
  their cell and tower sizes indicate a closest view about 2× less magnified.
- Behavior: closest zoom is 2× the opening view (orthographic size 0.5×)
  instead of 4× (0.25×). D-025 movement behavior, far 2× limit and the saved
  1.205 Inspector step remain.
- Verification: isolated PlayMode 45/45 passed, including the new near bound,
  unchanged far bound, movement/corners, full run and restart. Windows build
  succeeded and was opened from `Builds/Windows/Stage3CloserZoom/`. Player log
  has `S2_INT_GAMEPLAY_READY`, `S3_RAPID_GAME_READY` and
  `S3_TACTICAL_VIEW_READY` without a new attributable error. `git diff --check`
  passed. Stage 3 remains open for visual acceptance by the player.

### 2026-09-25 — Stage 3 fixed movement zoom and non-walkable border (D-025)

- Owner/surface: Stage 3 tactical camera/presentation and their pilot tests.
  The player selected a finite non-walkable visual border after weighing the
  conflicting center, map-edge and 2× zoom requirements.
- Behavior: WASD no longer changes orthographic size. Wheel alone eases toward
  a size from 0.25× to 2× the opening view; the hero remains centered. A
  darkened terrain border surrounds the unchanged 32×32 gameplay map, with no
  walkable/buildable/navigation cells or new gameplay content. The saved
  Inspector multiplier remains 1.205.
- Verification: isolated PlayMode 45/45 passed, including WASD without zoom,
  smooth wheel steps, 2× retreat, centered hero and all viewport corners at
  old gameplay-map edges; the complete run and restart also pass. Windows build
  succeeded and was opened from `Builds/Windows/Stage3CameraSteady/`.
  Player log has `S2_INT_GAMEPLAY_READY`, `S3_RAPID_GAME_READY` and
  `S3_TACTICAL_VIEW_READY` without a new attributable error. The current
  build screenshot is `art/review/stage3_tactical_game.png`; `git diff --check`
  passed. Human camera/tower acceptance and Stage 3 gate remain open.

### 2026-09-25 — Stage 3 map-bound, hero-centered smooth camera (D-024)

- Owner/surface: Stage 3 tactical presentation and opt-in camera behavior in
  `Stage2CameraFollow`; the Stage 2 camera path and gameplay rules are unchanged.
- Behavior: the hero's ground point is the exact screen center. Mouse wheel
  updates a target orthographic size and the image eases toward it. Maximum
  size follows the nearest playable map edge and Game view aspect; the camera
  approaches automatically near borders. The outer grass skirt was removed.
  The player's persistent `TacticalZoomSettings.asset` value of 1.205 is kept.
- Verification: isolated PlayMode 45/45 passed, including live Inspector
  setting, smooth wheel zoom, map corners, ground picking, the full run and
  restart. Windows build succeeded; launched player logged
  `S2_INT_GAMEPLAY_READY`, `S3_RAPID_GAME_READY`, `S3_TACTICAL_VIEW_READY` with
  no new attributable error. `git diff --check` passed. Build:
  `Builds/Windows/Stage3CameraBounds/Not3AStage3RapidPilot.exe`.
- Remaining: the owner assesses the visible camera behavior and tower style in
  the actual game. Stage 3 gate and runtime-3D-versus-sprites production choice
  remain open; no mass content production or Stage 4 handoff yet.

### 2026-09-25 — Stage 3 Inspector zoom tuning (D-023)

- Owner/surface: Stage 3 pilot scene/Editor opening flow and a narrow Stage 2
  camera settings asset. Player highlighted the empty Inspector and asked to
  tune the abrupt wheel strength there.
- Behavior: `TacticalZoomSettings.asset` stores a 1.05..3 multiplier (default
  1.4), which the tactical camera reads live; its custom Inspector presents a
  Russian slider and saves edits during Play Mode. The game opening command
  selects this asset in the right-hand Inspector. Wheel normalization and
  near/far limits remain as D-022; rules have no asset dependency.
- Verification/build: isolated PlayMode 45/45 passed, including live setting
  changes via a scratch ScriptableObject plus small/standard wheel deltas,
  range clamps and ground picking. Windows build succeeded and startup/gameplay/
  tower/tactical-ready markers passed. The Editor had one native crash during
  script reload in `com.unity.pipeline` startup; after restart the scene entered
  Play Mode without a new attributable error. The main scene's serialized
  camera reference points to `TacticalZoomSettings.asset`; Editor status
  confirms `zoomSettingsSelected=True`, scene ready and step 1.4. The Inspector
  was opened and the temporary standalone window closed so the user can tune
  in Unity. `git diff --check` passed. Player chooses final strength; Stage 3
  gate remains open.

### 2026-09-25 — Stage 3 wheel delta correction (D-022)

- Owner/surface: same Stage 3 tactical camera and its input regression test.
  Player still saw almost no zoom after D-021. The camera divided every wheel
  value by 120, so devices reporting small deltas could barely move it.
- Behavior: each nonzero wheel event counts as at least one 2.5× scale step;
  large deltas retain multi-step behavior. D-021 limits and fixed angle remain.
  First four wheel events log raw delta and resulting size for a real-device
  follow-up. No change to building, navigation or playable map cells.
- Verification/build: full isolated PlayMode suite 45/45 passed, including the
  regression with raw wheel deltas 1 and 120, both directions, range clamps,
  ground coverage and picking. Windows build succeeded and was copied to
  `Builds/Windows/Stage3TacticalWheelFix/`. The previous standalone player was
  closed and this build launched; its log has startup, gameplay, tower and
  tactical-ready markers without attributable errors. The main Editor also
  refreshed and resumed the playable scene (`editor-game-status.txt`: ready).
  `git diff --check` passed. Actual wheel units on the player's device will
  be visible as `S3_ZOOM_INPUT` after their next scroll; owner feel review remains.

### 2026-09-25 — Stage 3 stronger tactical zoom (D-021)

- Owner/surface: same Stage 3 tactical camera and noninteractive surrounding
  ground; user found the previous wheel change too weak in both directions.
- Behavior: each wheel notch changes orthographic scale 1.8×; limits are 4×
  closer and 8× farther than opening scale 5. Fixed angle, rules-plane clicks,
  map cell bounds and gameplay rules stay as before. Background ground now
  covers the wider view.
- Verification/build: full isolated PlayMode suite 45/45 passed after correcting
  wide-view ray/ground intersection. Windows build succeeded and is open from
  `Builds/Windows/Stage3TacticalZoomStrong/`. Its Player log has Stage 2
  startup/gameplay and Stage 3 tower/tactical-ready markers, with no attributable
  error. The main interactive Editor also reloaded the change and returned to
  `Stage3Rapid3DGamePilot` in Play Mode (`editor-game-status.txt`: ready=true).
  Owner visual review remains open.

### 2026-09-25 — Stage 3 tactical mouse zoom

- Owner/surface: Stage 3 camera follow, surrounding visual ground and focused
  PlayMode coverage. Depends on D-019 tactical scene and input system.
- Player request: mouse wheel zooms relative to the current opening view;
  `orthographicSize` 5 is clamped to 2.5..20. Fixed angle, navigation, build
  rules and the 32×32 playable boundary stay intact. The visual ground skirt
  fills the wide camera view but adds no cells or collision.
- Verification: focused wheel/boundary/picking check 1/1 and full PlayMode
  suite 45/45 passed in the isolated Unity project. Updated Windows build
  succeeded (`zoom-build.log`) and was copied to
  `Builds/Windows/Stage3TacticalZoom/`. The main interactive Editor reloaded
  the changed scripts, resumed `Stage3Rapid3DGamePilot` in Play Mode and logged
  startup/tactical/tower readiness; status confirms `playing=True, ready=True`.
  Actual Editor game capture `art/review/stage3_editor_game.png` was inspected.
  `git diff --check` passed. Owner visual approval remains open.

### 2026-09-25 — Stage 3 playable scene in the user's existing Editor

- Fixed review delivery: the Windows player had started, but the visible Editor
  still showed the standalone model probe. Opened `Stage3Rapid3DGamePilot` in
  that existing Editor, entered Play Mode and selected the Game tab.
- Surface: `Stage3RapidPilotBuilder` accepts an explicit local request at
  `Artifacts/S3-RAPID-PILOT/open-game.request` on script reload. Requests expire
  after five minutes, are consumed once, skip batch mode, and use Unity's save
  prompt before changing scenes. No automatic launch without a request.
- Verified in the main interactive Editor: successful compilation, startup,
  gameplay, tower and tactical-ready markers; `editor-game-status.txt` records
  the intended scene with `playing=True`, `ready=True`. No new attributable
  exception. Desktop screenshot verification was unavailable because the
  computer-use tool failed to initialize; runtime status is verified via Editor
  APIs. Existing 76 EditMode / 44 PlayMode results apply to unchanged gameplay.
- Remaining: owner's visual review and the previously recorded Stage 3 work.

### 2026-09-25 — Stage 3 D-019 tactical camera/ground -> playable review

- Owner/surface: current Stage 3 agent; shared camera follow, ground coordinate
  conversion and mouse picking plus isolated Stage 3 scene/presentation. Depends
  on accepted Stage 2 loop and the original Rapid R2 Blender output. User
  explicitly authorized camera adaptation now, after supplying more TD views.
- Behavior: one fixed elevated orthographic camera over a unit square ground
  grid; upright Rapid with foundations/shadows, temporary solid hero/objective/
  resource/enemy markers, matching mesh build preview and existing tint feedback.
  Gray-only placement, paid construction, navigation, damage and run rules stay
  authoritative. No final new actor/environment models were approved or made.
- Contracts/files: map adds opt-in planar conversion/hash/clamp; mover uses map
  clamp; camera has opt-in tactical follow and bootstrap validates its fixed
  orientation; build input intersects Z=0. `Stage3PlanarCorner` is a separate
  map asset. `Stage3TacticalPresentation` and `Stage3MeshFeedback` own visuals.
  Source Stage 2 assets keep their legacy view. Updated vision/art/plan, D-019,
  tactical feature brief, related map/build specs, both requested HTML reports.
- Verification: EditMode 76/76; final PlayMode 44/44, including upright FBX,
  real mouse placement/grass rejection, planar movement, full viewport within
  map edges, paid two-role full run/restart and legacy regressions. Windows
  build/startup/pilot/tactical-ready markers passed, final build/player logs
  have no new attributable error. Visually checked actual player capture
  `art/review/stage3_tactical_game.png`. Build is open from
  `Builds/Windows/Stage3TacticalView/`. Evidence: `Artifacts/S3-RAPID-PILOT/tactical-*`.
  Both HTML reports now identify Stage 3 as current and embed that game capture;
  local image links/unique IDs were checked.
- Remaining: owner evaluates the new view; mass art/production budget, sustained
  many-tower performance, final actor art/UI/audio and the Stage 3 gate remain
  open. Stage 4 starts with a new agent. Working implementation is runtime 3D;
  the previous sprite-first recommendation is superseded by D-019.

### 2026-09-25 — Stage 3 Rapid model revision 2 -> owner visual review

- Behavior: the original concept (wood, no visible archer) is approved, but the
  player rejected the first overly simple model. R2 rebuilds the silhouette,
  layered curved shingles, hip caps, heavy beams, rails/slats, riveted collars
  and stone feet. Original procedural wood/stone color and AO are baked into
  one 2048 atlas; FBX has one renderer/material and 31,668 triangles. Actual
  Blender render: `art/review/rapid_watchtower_r2_blender.png`. Model approval
  is still pending; do not confuse concept approval with model acceptance.
- Playable wiring: `Stage3Rapid3DGamePilot` buys one Rapid through normal paid
  placement for immediate review. Its prefab keeps the imported FBX axis
  conversion inside a separate visual orientation parent for the XY camera;
  a dedicated scene light makes the mesh readable. A small depth offset keeps
  feet above the flat map surface. Reimport texture settings before prefab
  mutation (reimporting during mutation can reset imported-root overrides).
  Sprite preview and separate sprite pilot remain available. Gameplay rules
  and the original Stage 2 scene/catalog were not changed by this revision.
- Contracts/files: `art/concepts/rapid_watchtower_approved.png`,
  `art/source/rapid_watchtower.blend` and build scripts,
  `Assets/Stage3/VisualPilot/**`, `Assets/Scenes/Stage3Rapid*`, review captures,
  this register, art direction and the canonical plan. Other uncommitted
  changes were preserved. Build: `Builds/Windows/Stage3RapidPilotR2/`.
- Verification: Blender source exported and rendered; Unity imported the atlas,
  model and sprite. Windows build emitted `S3_RAPID_GAME_BUILD_OK`; actual
  player emitted startup/gameplay/pilot-ready markers and was visually checked
  in `art/review/rapid_r2_in_game.png`. Final build/player logs have no new
  attributable error. Earlier full PlayMode suite passed 40/40; final revision
  checks passed 3/3: both visual pilots plus the existing complete winning
  run/restart (`revision2-verified-tests.xml`). One test launch hit a transient
  Unity IL postprocessor startup failure; the unchanged retry completed.
  Evidence: `Artifacts/S3-RAPID-PILOT/revision2-*`. Immediate editor Camera.Render
  capture has an unreliable ground-material preview; use the actual player
  screenshot and Blender render for visual judgment.
- Remaining: owner reviews R2 and chooses runtime 3D or Blender-rendered sprites.
  Current runtime mesh pilot does not show the existing SpriteRenderer attack
  flash, and uses a sprite placement preview. Actor overlap, picking/surface
  consistency and many-tower performance/LOD still need evaluation before
  adopting runtime 3D. Geometry count is pilot data, not an accepted production
  budget. No broader art, UI or audio production yet; Stage 3 gate remains
  open. Stage 4 belongs to a new agent after Stage 3 closes.

### 2026-09-25 — Stage 2 Rapid hit visibility -> Stage 3

- Behavior: a live corner-map check confirmed one Rapid kills multiple first-wave
  enemies. Each successful shot now flashes the tower briefly, and nonlethal
  enemy damage flashes the enemy; attack rules and values stay the same.
- Contracts/files: `Stage2Tower`, `Stage2Enemy`, focused Defense and Integration
  tests, tower-role spec and canonical plan. No scene or Inspector migration.
- Verification: EditMode 76/76 and PlayMode 39/39 passed. Windows player build
  emitted `S2_INT_BUILD_OK`, launched with `S2_INT_STARTUP_OK` and
  `S2_INT_GAMEPLAY_READY`, and has no new attributable startup error.
  Evidence: `Artifacts/S2-RAPID-FEEDBACK/`.
- Human observation: player confirmed the new Windows build: «теперь
  работает». Repeated attacks are readable; Stage 3 remains a separate new
  task.

### 2026-09-25 — Stage 2 D-018 economy and first-wave pacing -> Stage 3

- Behavior: preparation before every authored wave is 180 s; Rapid costs
  5 gold, 2 wood, 1 stone;
  each run starts with 5 gold and each killed enemy grants 1. Throne contact
  and restart cleanup grant none. Longshot price remains.
- Contracts/files: Run catalog/builder, Economy inventory/catalog/asset,
  Defense payment/catalog/asset/builder, enemy death fact, run reward, temporary
  HUD, focused tests and vision/plan/spec/decision docs. Stage 2 scene uses
  the existing asset references; no Inspector migration is needed.
- Verification: Unity 6000.6.2f1 EditMode 76/76, PlayMode 38/38; the full
  automated run buys a second Rapid after first-wave kills, wins and restarts.
  Windows Development Build emitted `S2_INT_BUILD_OK` and the launched player
  emitted `S2_INT_STARTUP_OK` and `S2_INT_GAMEPLAY_READY`, responsive with no
  new attributable startup error. The later extension to 180 s for waves 2/3
  passed EditMode 76/76 and PlayMode 38/38; its new Windows build started
  cleanly. Evidence: `Artifacts/S2-ECONOMY-REVISION/` and
  `Artifacts/S2-ALL-WAVES-180/`.
- Remaining: no new human pacing observation since D-018; values are
  provisional. Stage 3 still starts in a separate task with a new agent.

### 2026-09-25 — Stage 2 first playable gate -> new Stage 3 agent

- Behavior: D-016 corner defense now runs on a 32×32 map; Rapid and Longshot
  are both wired, selectable, purchasable and active in one automated winning
  run. Gray-only combat placement remains enforced. The old 100×100 asset is
  retained for historical navigation/performance fixtures.
- Contracts/files: new `Stage2Corner32x32.asset` and Map builder, Integration
  scene/builder/tests, map test and current map/vertical-slice/plan/decision
  docs. The existing uncommitted Longshot wiring was preserved. Scene map
  references, entrance/throne/hero/camera positions and resource anchors now
  agree with the corner asset; no new Inspector assignment is required.
- Verification: Unity 6000.6.2f1 EditMode 74/74, PlayMode 38/38, then focused
  Integration 16/16 after adding a viewport assertion. Windows x64 Development
  Build `S2_INT_BUILD_OK` launched with `S2_INT_STARTUP_OK` and
  `S2_INT_GAMEPLAY_READY` for final hash `48A2358B440C3C65`, responsive and
  without a new attributable startup error. A human completed and restarted
  a run, then confirmed the final start shows portal/throne and clear resource
  directions. Screenshot, test reports and logs are under
  `Artifacts/S2-CORNER/`; `git diff --check` passes.
- Remaining: Stage 2 gate is closed. Map size and tempo are provisional; no
  specific blocker was reported. Stage 3 starts only in a separate new task
  with another agent and begins with the small Blender visual pilot.

### 2026-09-25 — Stage agent prompts -> future stage owners

- Behavior: roadmap now contains a standalone starter prompt for each Stage
  2–7 and regression-only prompts for completed Stage 0–1. Each new stage uses
  a new Codex agent/task after the previous gate; one agent carries its stage.
- Contracts/files: `reports/development-roadmap.html`,
  `gamedesign/DEVELOPMENT_PLAN.md`, D-017 in `production/DECISIONS.md` and this
  handoff. Existing Unity scene/runtime/data changes were left untouched.
- Verification: HTML parses; all eight prompts sit in their matching stage,
  and all 18 links/anchors resolve. `git diff --check` passes. Documentation
  only, so Unity tests and build were not rerun for this change.
- Remaining: current Stage 2 agent still needs to complete its playable gate;
  create the Stage 3 agent only after that gate is accepted.

### 2026-09-25 — Roadmap stage navigation

- Behavior/files: `reports/development-roadmap.html` now switches Stage 0–1 and
  Stage 2–7 through accessible button tabs; only the chosen stage panel shows.
  Stage prompts, task-level model guidance and the canonical stage gates remain.
- Verification: HTML structure/links and seven tab-panel bindings pass; a DOM
  interaction check covers click, Arrow, Home/End and the current-stage shortcut.
  `git diff --check` passes. No Unity behavior changed in this edit.
- Remaining: Stage 2 playable work and gate are unchanged.

### 2026-09-25 — Model settings in stage prompts

- Behavior/files: all eight starter prompts in
  `reports/development-roadmap.html` now begin with an explicit Codex model and
  reasoning level; the setting is also visible above the copyable text.
  `gamedesign/DEVELOPMENT_PLAN.md` clarifies that the user selects that setting
  in Codex before creating the stage task, with any later switch described in
  the prompt.
- Verification: parsed every prompt and checked its first line against the
  stage recommendation; HTML links resolve and `git diff --check` passes.
- Remaining: stage gates and Unity work are unchanged.

## Completed Work — First Playable (D-015)

Active workstream: Integration, owner of
`Assets/Scenes/Stage2VerticalSlice.unity`, its scene builder, composition and
temporary readout. Defense owns its existing tower data/runtime. The
uncommitted Longshot scene binding has been preserved and automatically
verified. Dependencies: completed gather/build/path/siege/run loop and the
two delivered Defense roles. Exit evidence: both roles work in the same
automated full run; a player completed and restarted a Windows run, then
confirmed the corrected startup view. Relevant checks pass and no new
attributable startup errors appear.

Next action: a new Stage 3 agent begins the small visual pilot when requested.
`DEVELOPMENT_PLAN.md` is the current short
path. The earlier task schedule, token estimates and proposed content below
are retained only as historical evidence; they are not current assignments.

D-016 update, 2026-09-25: Defense owns the gray-only combat placement check in
`Assets/Stage2/Defense/Runtime/Stage2Defense.cs` and its focused tests. Map owns
the authored map and Editor asset builder; Integration alone owns the scene,
composition and its tests. The corner layout, three resource directions,
gray-only paid towers, route/siege and camera now pass automated checks. The
100×100 asset remains an older opposite-end candidate. Human Windows-run
acceptance and final startup-visibility confirmation are recorded above.

### 2026-09-25 — Defense gray-zone rule -> Map/Integration/QA

- Behavior: Defense preview and confirm reject combat towers on green Forest
  cells, even when the general buildable/walkable masks allow them; no tower,
  payment or occupancy is produced. Gray InvasionCorridor placement remains
  subject to all existing actor, economy and map checks.
- Contracts/files: `Stage2Defense.cs`, focused Defense EditMode/PlayMode tests,
  D-016, current vision/plan/map/building/vertical-slice docs and both HTML
  reports. No Stage 2 scene, map asset or Integration work was overwritten.
- Verification: `git diff --check` passes; both HTML files parse, SVG tags
  balance and local links/anchors resolve. Focused Unity tests are degraded:
  the project is open in an Editor process, and a second batchmode instance
  cannot use the same project. Run Defense EditMode/PlayMode checks in that
  Editor when the current Integration scene work is ready. Map geometry has
  not yet been changed.
- Remaining: Map and Integration implement the selected corner composition,
  update coordinate-dependent tests and qualify the Windows player run.

## Historical Stage 2 Package Notes

### Former active package — S2-DEF-02 (2026-09-25)

- Owner: Defense, `Assets/Stage2/Defense/**` and
  `gamedesign/features/STAGE2_TOWER_ROLES.md`. Integration retains exclusive
  ownership of the Stage 2 scene and its composition/HUD files.
- Inputs: completed S2-DEF-01/DEF-01-FIX, D-009/D-014 and the delivered
  S2-AI-01 brief. Human selected GPT-6 Sol / high.
- Acceptance: two paid one-cell tower roles use distinct catalog data and
  prefabs; role selection changes the preview and price without spending;
  focused EditMode/PlayMode checks cover reach, damage, purchase, pause and
  reset. Scene wiring, build and player choice observation follow as separate
  Integration/QA checkpoints.

### 2026-09-25 — Defense S2-DEF-02 -> Integration/QA

- Behavior: keys 1/2 select Rapid/Longshot; B opens placement; selected role
  controls ghost silhouette, full-price validation, instantiated prefab and
  attack profile. Both roles retain one-cell occupancy, siege and reset.
- Contracts/files: `Assets/Stage2/Defense/Runtime/Stage2Defense.cs`,
  `Stage2BuildDriver.cs`, `Stage2DefenseCatalog.cs`, two catalog assets,
  two inactive prefabs and focused Defense tests. Existing baseline asset
  retains its GUID and references. See `STAGE2_TOWER_ROLES.md` for the exact
  `Configure` overload and data paths; Integration owns scene/HUD changes.
- Verification: Unity 6000.6.2f1 isolated-copy Defense EditMode 10/10,
  PlayMode 3/3, no failures/skips, compile errors or new exceptions in final
  focused logs. A cold import initially resolved pre-existing data assets as
  null; copying the current Editor import cache made both suites pass.
  `git diff --check` clean. No scene or Windows build claim for this delivery.
- Remaining: Integration binds Longshot and shows role/price, then QA checks
  Editor/Windows and the player tests whether the choice changes a real-map
  layout. P-004/P-008 and S2-AI-01 product acceptance remain separate.

Human verification preference, 2026-09-24: keep agent verification focused on
changed logic and necessary regression. The human can perform one manual game
check at the end of a task or task batch. Do not spend further agent turns on
repeated live placement/timing comparisons unless a concrete defect or an
explicit measurement request requires them. Present a short manual checklist.

Historical sequence before D-015: Stage 1 is complete. For Stage 2, reserve the scarce Integration/scene slot
for a running loop before polishing content. The next work is:

1. S2-ECO-01 closed P-003 as D-013; S2-BLD-01 closed P-002 as D-014.
   S2-ECO-02, S2-DEF-01, S2-AI-02, S2-WAV-02, S2-DEF-01-FIX and S2-INT-02 are complete.
   The human subsequently authorized S2-MAP-03, S2-WAV-01 and S2-AI-01 as a
   serial decision batch, with checks/corrections after all three drafts.
   Their analyses/brief are delivered. The human delegated the next cadence
   choice and implementation on 2026-09-24 ("делай как считаешь правильно и
   лучше всего"). S2-WAV-01/P-004 now owns one diagnostic cadence follow-up;
   S2-WAV-02 remains complete. Map and enemy-role proposals remain separate.
2. Economy supplies three-resource gathering/inventory; Defense supplies paid
   placement/occupancy; Waves/AI supplies the Stage 2 enemy and run loop.
   Reuse accepted Stage 1 rules where they survive the new map contract.
3. Integration connects each delivery to the scene; Platform/QA checks that
   checkpoint before the next content expansion.
4. Design + Platform/QA use the live loop to decide map size P-008 and pacing
   P-004. UI/UX and Art/content may prepare wireframes, scale and provenance
   concurrently, but final assets follow the measured scale and state hooks.
5. Defense/AI add only variation needed for a meaningful choice; UI/UX,
   Art/content and Audio make the tested loop legible with simple 2D assets.
   QA qualifies a full run, then Design conducts the external comprehension
   gate. Detailed Blender models are deferred by D-012.

The confirmed capacity is one developer: keep one implementation package
active at a time, with brief decision/document review only while builds or
external reviews are waiting. If capacity changes later, independently owned
surfaces may overlap after a new WIP decision. This is not a requirement to
create agents or chats. A human product decision cannot be replaced by a
model's guess.

## Stage 2 Ownership Rules

Current playability check is in
`gamedesign/features/STAGE2_VERTICAL_SLICE.md`.

- **Integration — единственный владелец
  `Assets/Scenes/Stage2VerticalSlice.unity`.** Только этот owner создаёт,
  редактирует, сохраняет и меняет build wiring Stage 2 scene. Map, Economy,
  Defense, Waves/AI, UI/UX, Art/Audio и Platform/QA передают ему prefabs, data,
  tests и wiring notes; они не открывают сцену для сохранения.
- Stage 1 scene/assets остаются regression baseline и не мигрируются скрыто.
- Параллельная работа разрешена только для task IDs с готовыми входными
  контрактами и непересекающимися surfaces. Активный owner до начала добавляет в
  этот файл task ID, owned files/contracts, dependencies и acceptance.
- Task-specific feature spec создаётся при фактической активации задачи, а не
  заранее. Integration возвращает дефект владельцу системы вместо
  opportunistic изменения чужого runtime/prefab surface.
- S2-INT-02 и S2-QA-01 выполняются на каждом подключённом checkpoint, а
  S2-QA-02 — после квалифицированной representative сборки. Только принятый
  внешний playtest переводит Stage 2 в complete.

## Historical Stage 2 Claims

### S2-WAV-01 / P-004 — diagnostic cadence follow-up

- Owner: Design/Waves in this dialogue; recommended GPT-6 Astra / high for
  S2-WAV-01. The human delegated choosing and implementing the trial after
  reviewing the three-wave, early-start and persistent-state proposal.
- Surface: existing RunState runtime/catalog/tests; Integration regression
  verifies the existing scene binding. Its existing development readout also
  displays current/total wave and group count; no scene rebuild is required.
- Inputs: completed S2-WAV-02, S2-INT-02, S2-QA-01 and measured one-wave runs.
- Acceptance: finite wave sequence, recovery preserving world/resources/throne
  health, final-only victory, loss priority, pause/early-start/restart, a live
  paid-tower replacement circuit, regression and current Windows build.
- Status: implementation delivered; EditMode 70/70 and PlayMode 34/34 passed.
  Windows Development Build succeeded 2026-09-25 with `S2_INT_BUILD_OK`;
  Player/manual run remains outstanding. Live probes ended after the human
  objected to excessive measurement. Final pacing and external-player gate
  stay open.

### S2-MAP-03 -> S2-WAV-01 -> S2-AI-01 — authorized decision batch

- Status: analyses/brief delivered and consolidated verification complete.
  Decision drafts were delivered for review. The later delegated cadence
  follow-up above supersedes this batch's original implementation pause for
  P-004 only; map acceptance and enemy breadth remain separate.
- Human instruction, 2026-09-24: prepare these three packages serially;
  run consolidated checks/tests/corrections only after all three deliveries
  are drafted. This supersedes the prior stop before S2-MAP-03. It does not
  authorize subsequent implementation packages or declare P-008/P-004 accepted.
- Routing: S2-MAP-03 Design/Platform/Map, GPT-6 Astra / high; S2-WAV-01
  Design/Waves, GPT-6 Astra / high; S2-AI-01 Design/Waves, GPT-6 Astra / high.
  Model/effort remain selected by the human in this dialogue.
- Owned surfaces: existing map/run/pathing/vertical-slice specs, a bounded
  enemy-difference brief, canonical plan/decision proposals/handoffs; after
  all three drafts, reproducible Editor-only measurement tooling and evidence.
  No authored scene, map, economy, enemy, tower or run tuning changes are
  implicit in these decision packages.
- Inputs: S2-INT-02/S2-QA-01 integrated baseline, D-009 through D-014,
  current prototype catalogs and S2-MAP-02 diagnostics.
- Acceptance: distinguish observed live timings from geometric estimates,
  report frame/memory/replan evidence with hardware/mode, propose P-008 and
  P-004 conclusions without invented human observations, and provide a minimal
  enemy brief with testable layout choices and downstream ownership. Final
  decision acceptance stays explicit; drafts are not stage closure.

### S2-QA-01 — Platform/QA / integrated checkpoint

- Status: integration checkpoint complete under the human's sequential
  authorization, 2026-09-24; recurs after future integrated deliveries.
  Plan recommends GPT-6 Sol / high, 38–70k soft allocation; UI model choice
  remains with the human.
- Owned surface: regression/build commands, build output, QA evidence and
  canonical status/handoff only. Integration retains sole ownership of the
  Stage 2 scene/build wiring; system runtime/data remain read-only unless a
  concrete defect is returned to its owner.
- Inputs: complete S2-INT-02 real-map scene, S2-ECO-02/S2-DEF-01(-FIX)/
  S2-AI-02/S2-WAV-02 and existing Stage 1 regression.
- Observable acceptance: complete EditMode/PlayMode suites pass; fresh Editor
  scene entry has no attributable Console errors; Windows x64 Development
  Build succeeds and its executable launches without attributable errors.

### S2-INT-02 — Integration / Stage 2 scene

- Status: complete under the human's sequential authorization, 2026-09-24.
  Plan recommends GPT-6 Sol / high, 50–90k soft allocation; UI model choice
  remains with the human.
- Owned surface: `Assets/Stage2/Integration/**`,
  `Assets/Scenes/Stage2VerticalSlice.unity` and its saved Inspector wiring,
  existing Stage 2 Integration tests plus package status/handoff. Only this
  owner edits/saves the scene. Stage 1, Map, Navigation, Economy, Defense,
  WavesAI and RunState runtime/data remain read-only inputs.
- Inputs: S2-MAP-01/02, S2-INT-01 shell, S2-ECO-02, S2-DEF-01 plus baseline
  attack correction, S2-AI-02 and S2-WAV-02; accepted D-013/D-014. P-004/P-008
  remain measurement decisions, not scene assumptions.
- Observable acceptance: the existing authored map/player/camera scene binds
  exactly one shared Navigation, Economy, Defense, enemy group and Run state
  with the supplied catalogs/prefabs. PlayMode on that scene proves gather,
  paid build, route/siege, throne loss, natural win and restart without missing
  references or attributable Console errors. Inspector wiring and handoff are
  explicit; Windows Development Build belongs to S2-QA-01.

### S2-DEF-01-FIX — Defense baseline attack

- Status: complete, corrective follow-up under the human's sequential
  authorization, 2026-09-24. Parent S2-DEF-01 recommends GPT-6 Sol / high.
- Owned surface: narrow extension of `Assets/Stage2/Defense/**` tower combat
  data/runtime/tests and explicit target callback in `Assets/Stage2/WavesAI/`
  and `Assets/Stage2/RunState/`; existing scene remains Integration-owned.
- Inputs: S2-DEF-01 tower/placement, S2-AI-02 live enemies, S2-WAV-02 win
  contract, Stage 1 one-tower attack behavior. No new tower role or final
  balance choice is authorized.
- Observable acceptance: one placed prototype tower can select and damage a
  nearby living enemy at a stable interval, not hit the hero, respect pause,
  and enable a natural one-wave win without test-injected enemy death. Prior
  placement/siege/reset tests remain passing, with clean Console and handoff.
- Exit: EditMode 66/66 and PlayMode 29/29, no failures/skips; a new moving
  wave test wins from the paid tower's own fire. Prototype combat data was
  migrated in place with cost/health preserved. After script reload and
  clearing test-session logs, Stage 2 Play entry/exit had 0 errors/0 warnings
  and no compile failure. Scene/Windows build still belongs to Integration/QA.

### S2-WAV-02 — Waves/AI / Run state

- Status: complete under the human's sequential authorization, 2026-09-24.
  Plan recommends GPT-6 Sol / high, 24–44k soft allocation; UI model choice
  remains with the human.
- Owned surface: `Assets/Stage2/RunState/**` plain run rules, narrow input
  driver, provisional data and tests, task implementation notes in
  `gamedesign/features/STAGE2_VERTICAL_SLICE.md`, status/handoff. The Stage 2
  scene/build settings remain Integration-owned; Economy, Defense, Navigation,
  Map and Stage 1 are inputs. A narrow WavesAI reset fix was needed to make
  terminal cleanup safe during throne-contact callbacks.
- Inputs: S2-AI-02 live enemy group/throne fact; S2-DEF-01 phase/reset contract;
  S2-ECO-02 inventory/recovery/reset; S2-MAP-02 plan generation; Stage 1
  preparation/wave/win/loss/restart semantics. P-004 pacing remains undecided;
  prototype values stay in one Unity data asset.
- Observable acceptance: preparation timer or accepted Space action starts a
  bounded one-wave prototype; spawning/clear and throne damage yield exactly
  one terminal win or loss; pause freezes time/actors/economy and discards
  preview; restart clears enemies, towers, inventory, node timers, plans and
  input latches, restoring preparation without prior-run callbacks. Focused
  EditMode/PlayMode checks plus clean Console and Integration wiring handoff.
- Exit: EditMode 65/65, PlayMode 28/28, no failures/skips. Two full-suite
  negative Integration validation logs identified. An Editor Input System
  monitor error occurred after stopping a test-created Play session; a script
  domain recompile, cleared Console and subsequent Stage 2 Play entry/exit
  reported 0 errors/0 warnings/no compile failure. No scene/build edits.
  Missing Stage 2 tower damage is a concrete Integration blocker; Defense
  owns the narrow correction before a live win/build claim.

### S2-AI-02 — Waves/AI

- Status: complete under the human's sequential authorization, 2026-09-24.
  Plan recommends GPT-6 Sol / high, 32–58k soft allocation; UI model choice
  remains with the human.
- Owned surface: `Assets/Stage2/WavesAI/**` enemy runtime, provisional data,
  prefab and tests, `gamedesign/features/DYNAMIC_ENEMY_PATHING.md` Stage 2
  implementation notes, package status/handoff. Defense, Navigation, Map,
  Economy, Stage 1 and Integration scene are read-only inputs.
- Inputs: D-009 deterministic route/siege, D-014 active-step placement rule,
  S2-MAP-02 shared navigation API and S2-DEF-01 tower identity/health plus
  `SetEnemyProtection` callback.
- Observable acceptance: a live enemy moves continuously from explicit cell
  to cell using current plans; an active step protects both endpoints without
  Navigation Revision changes; mid-step edits finish the step then replan;
  invalidated blocker identity cannot be damaged; reachable blocker is attacked
  at a stable interval and death opens a route; multiple enemies and reset
  release protections. Focused/full EditMode and PlayMode checks, clean Console
  and exact handoff to S2-WAV-02/Integration.
- Exit: EditMode 61/61, PlayMode 26/26, no failures/skips; full PlayMode's two
  negative Integration logs identified. Cleared Console and subsequent Stage 2
  Play entry/exit: 0 errors/0 warnings, compile successful. AI data/prefab/
  tests and wiring notes delivered; no scene or build settings edits.

### S2-DEF-01 — Defense

- Status: complete, human authorized 2026-09-24. Plan recommends GPT-6 Sol /
  high, 32–58k soft allocation; model selection remains in the user's UI.
- Owned surface: `Assets/Stage2/Defense/**` runtime, prototype data/prefab,
  tests and metas; implementation notes in `gamedesign/features/STAGE2_BUILDING.md`,
  package status here and in DEVELOPMENT_PLAN.md/STAGE2_VERTICAL_SLICE.md.
  Integration alone owns the Stage 2 scene; Map, Navigation, Economy and
  Stage 1 surfaces are read-only.
- Inputs: D-014 paid active-wave placement; D-013 shared three-resource
  inventory; S2-ECO-02 `Stage2Inventory`; S2-MAP-02 identity-checked occupancy;
  Stage 2 map masks and anchors.
- Observable acceptance: one valid confirmation yields one usable tower, one
  full spend, one occupancy revision and one built fact; preview/cancel/failure
  leaves no payment or lasting occupancy. Phase, hero and current enemy step
  protect placement; destruction releases by original identity, replacement
  survives stale callbacks, and reset discards preview/towers. Focused and
  full EditMode/PlayMode checks plus clean Console; explicit Integration/AI
  wiring handoff. Sequential subsequent ready packages are authorized up to
  the S2-MAP-03 boundary.
- Exit: EditMode 54/54, PlayMode 24/24, no failures/skips; the two PlayMode
  Console errors are expected negative Integration tests. Cleared Console and
  Stage 2 Play entry: 0 errors/0 warnings, compilation successful. Scene
  remains clean and Integration-owned. Windows build/live-loop evidence awaits
  scene wiring.

### S2-ECO-02 — Economy

- Status: complete, 2026-09-24; human authorized this package. Plan recommends
  GPT-6 Sol / high, 28–50k soft allocation; model selection remains in the
  user's UI. No later implementation package is authorized.
- Owned surface: `Assets/Stage2/Economy/**` runtime, authored prototype data,
  tests and metas; S2-ECO-02 contract in `gamedesign/features/STAGE2_ECONOMY.md`,
  current status in DEVELOPMENT_PLAN.md and STAGE2_VERTICAL_SLICE.md, this
  claim and handoff. Stage 2 scene/build settings, Stage 1, Map, Navigation,
  Packages and other workstream assets remain read-only.
- Inputs: human-accepted D-013 node lifecycle, completed S2-MAP-01 resource
  regions/anchors, Stage 1 held-interaction semantics, D-014 placement boundary.
- Observable acceptance: each of wood/stone/metal can be gathered only in
  range after a completed interaction; depletion and finite active-time
  recovery work without rewards; inventory adds and atomically spends all three
  resources; pause/terminal freeze and restart reset timers/progress/balances;
  relevant focused and full EditMode/PlayMode regression plus clean Console.
  Data and exact Integration wiring notes are delivered without scene edits.
- Entry: main `87fdb1b`; pre-existing documentation/ignore changes preserved.
  Assets/Packages/ProjectSettings are clean. Unity 6000.6.2f1 Editor connected,
  Console 0 errors/0 warnings, no compilation failure; retained historical
  EditMode 42/42 and PlayMode 20/20.
- Exit: EditMode 47/47, PlayMode 22/22, no skips or failures; clean subsequent
  Stage 2 Play entry and Console 0 errors/0 warnings, no compile failure.
  Economy source/data/metas are delivered; scene/build wiring remains with
  Integration. This was the S2-ECO-02 exit state; S2-DEF-01 has since completed.

### S2-BLD-01 — Defense / Design

- Status: complete, human-accepted 2026-09-24; P-002 resolved by D-014.
  Human selected GPT-6 Astra / high (original plan envelope 14–26k).
  This was the S2-BLD-01 exit state; later authorization is recorded above.
- Owned surface: `gamedesign/features/STAGE2_BUILDING.md` placement-policy
  brief, P-002/D-014 record, current package status in DEVELOPMENT_PLAN.md
  and STAGE2_VERTICAL_SLICE.md, accepted consumer notes in
  DYNAMIC_ENEMY_PATHING.md, GAME_VISION.md, this claim/handoff register. No runtime, scene,
  prefab, map, navigation, package or build-wiring changes are owned here.
- Inputs: D-009 route/siege, D-013 renewable/reserved resources, accepted Stage 1
  placement/enemy code and S2-MAP-02 occupancy/plan API and discrete consumers.
- Acceptance: human accepts allowed build phases, current-enemy/continuous-step
  exclusion, future-path placement, full-block siege and cancel/failure without
  spending; rule-level scenarios cover stale preview, repeated confirm,
  phase transitions, destruction/replacement and restart. Implementation and
  continuous-movement tests remain S2-DEF-01/S2-AI-02/Integration work.
- Entry: `main` at `87fdb1b`; existing documentation/ignore changes and accepted
  S2-ECO-01 spec preserved. No Assets/Packages/ProjectSettings diff. Unity
  6000.6.2f1 is now open on this project; stored baseline still reports
  EditMode 42/42 and PlayMode 20/20. Current Console checked separately below.

### S2-ECO-01 — Economy / Design

- Status: complete, human-accepted 2026-09-24; P-003 resolved as D-013.
  Human selected GPT-6 Astra / high; original plan envelope 14–26k.
  Decision surface released for handoff. S2-BLD-01 was subsequently completed;
  later packages required separate approval at this handoff; the human has
  since authorized sequential ready packages up to S2-MAP-03.
- Owned surface: `gamedesign/features/STAGE2_ECONOMY.md` resource lifecycle
  brief, P-003 in `production/DECISIONS.md`, S2-ECO-01 status in
  `gamedesign/DEVELOPMENT_PLAN.md` and `STAGE2_VERTICAL_SLICE.md`, this register.
- Inputs: accepted Stage 1 gather/inventory/restart behavior, D-002/D-006/D-008,
  completed S2-MAP-01 resource regions/anchors and S2-INT-01 scene shell.
  Runtime, tests, data, scenes, packages and build wiring are read-only here.
- Observable acceptance: human accepts depletion/recovery/restart rules;
  rule-level review covers zero inventory, all nodes depleted, lost defense,
  absent optional rewards, pause/terminal states and restart. Distinguish
  permanent resource starvation from a survivable recovery window, which
  requires later live-loop evidence. Exact runtime values remain data-owned.
- Entry verification: `main` at `87fdb1b` matches the local `origin/main` ref;
  no remote fetch claimed. Seven pre-existing modified documentation/ignore
  files are preserved; Assets/Packages/ProjectSettings have no Git diff.
  Stored reports at that commit: EditMode 42/42, PlayMode 20/20, no skips;
  fresh-Play Console snapshot has 0 errors/0 warnings. Unity is currently
  closed; these are historical results, not a new run. Existing Windows build
  qualifies S2-INT-01's shell only. `git diff --check` passes at entry.

### S2-MAP-02 — Foundation / Waves-AI Navigation

- Status: complete, 2026-09-22; surface released for the handoffs below.
  Claimed against clean `main` at `f0d151b`, equal
  to freshly fetched `origin/main`. Editor preflight: 0 errors/0 warnings;
  retained baseline reports: EditMode 20/20, PlayMode 13/13.
- Owned surface: `Assets/Stage2/Navigation/**` (runtime, diagnostics, navigation
  EditMode/PlayMode tests and fixtures, required assembly boundaries and metas),
  Stage 2 navigation section in `DYNAMIC_ENEMY_PATHING.md`, S2-MAP-02 status in
  `STAGE2_VERTICAL_SLICE.md`, current `DEVELOPMENT_PLAN.md` status, this register.
- Dependencies: completed S2-MAP-01 read-only map definition/candidate asset,
  S2-INT-01 composition contracts, accepted Stage 1 dynamic pathing. Map,
  Integration, both scenes, Stage 1, ProjectSettings and Packages stay read-only.
- Observable acceptance: candidate 100x100 open/longer-zigzag/stable reachable
  blocker routes respect authored masks; identity-checked atomic occupancy and
  exact revision/no-op semantics; destruction/reset/replanning recover; simulated
  consumers share unchanged-state searches; measured search/cache/replan counts
  and timings with hardware/map hash; full EditMode/PlayMode regression and
  fresh-domain Console pass. No scene wiring, combat, P-008 or Stage 2 gate change.

### S2-MAP-01 — Foundation / Map

- Status: complete, 2026-09-21; surface released for the handoffs below.
- Owned surface: `Assets/Stage2/Map/**`, its EditMode tests, S2-MAP-01 status in
  `gamedesign/features/STAGE2_VERTICAL_SLICE.md`, current Stage 2 status in
  `gamedesign/DEVELOPMENT_PLAN.md`, and this claim/handoff register.
- Dependencies: D-003 and D-009–D-011; accepted Stage 1 `IsoGrid`, dynamic
  pathing, scene-builder and test contracts. Stage 2 scene/build wiring remains
  exclusively owned by Integration.
- Acceptance: one authored 100x100 map asset exposes cell bounds, green/gray
  surfaces, walkable/buildable masks, one entrance, one throne, wood/stone/metal
  placement regions and camera bounds; deterministic validation rejects invalid
  bounds, missing/out-of-bounds landmarks, invalid regions/camera bounds and
  incompatible masks; isometric cell/world round-trip and deterministic reads
  pass EditMode tests without changing Stage 1 or either scene.

### S2-INT-01 — Integration

- Status: complete, 2026-09-22; claimed 2026-09-21. Handoff ready below;
  further Stage 2 scene edits remain exclusively owned by Integration.
- Owned surface: `Assets/Scenes/Stage2VerticalSlice.unity` and its `.meta`,
  `Assets/Stage2/Integration/**`, Stage 2 integration PlayMode tests,
  `ProjectSettings/EditorBuildSettings.asset`, S2-INT-01 status in
  `gamedesign/features/STAGE2_VERTICAL_SLICE.md`, current Stage 2 status in
  `gamedesign/DEVELOPMENT_PLAN.md`, and this claim/handoff register.
- Dependencies: completed S2-MAP-01 and its read-only
  `Stage2Map100x100.asset`; accepted Stage 1 movement, camera, isometric grid,
  scene-builder and regression-test contracts. `Assets/Stage2/Map/**` and the
  Stage 1 scene/assets remain read-only inputs.
- Acceptance: a builder-authored Stage 2 scene explicitly binds and validates
  the 100x100 map, renders distinct forest/corridor surfaces and readable
  entrance/throne landmarks, moves a non-combat WASD player inside `CellBounds`,
  follows it with a fixed orthographic camera inside `CameraBounds`, reports
  missing/invalid map references with code/reason, preserves Stage 1 regression,
  and is the first enabled Windows Development Build scene.

## Handoff Entry Format

```md
### YYYY-MM-DD — <source> -> <target/status>
- Outcome: <что теперь работает>.
- Contract/files: <что изменено и на что можно опираться>.
- Verification: <tests/build/manual; degraded reason if any>.
- Remaining: <явно вне завершённой задачи>.
```

## Handoff Register

### 2026-10-20 — D-097 reference fidelity repair -> Runtime handoff

- Behavior: owner-rejected flat opaque fills and thick gold rails replaced with
  the actual ImageGen source-derived painted skin atlas: multitone gradients,
  engraved texture, thin shaped rims, amber/gold S and plum/radiant-gold SSS.
  Same shared treatment covers bag/loadout/details, forge, professions and HUD.
- Contract/files: Astra changed Quality renderer and removed obsolete Forge
  glint overlay; added Stage3EquipmentQualityArtImporter and one shared Resources
  Stage3EquipmentQualitySkins.png (original bytes, measured Rect[8], no generated
  textures). Rounded shared edges/scroll UV clipping/selection/input retained.
  No scene/Inspector/package migration or gameplay/item-icon change. Main owns
  ImageGen/provenance, checks/build/native evidence and canonical docs.
- Verification: import successful, 17/17 PlayMode, Mac644070019bytes; Astra
  strict-reference PASS27frames at720/900/1080p. Main actual mouse D/C/equipC,
  SSS details and forge horizontal scroll/select pass. Ordinary game restored
  5gold/0materials, no QA grants/frozen clock, no new attributable console errors.
  art/source/item_quality_cells_2026-10-20_verification.json preserves the rejected
  procedural attempt explicitly; skins_generation.json records source/rights.
- Remaining: final owner appearance response, Windows qualification. Small HUD
  cells simplify engraving detail. Merchant/city runtime is still separate and
  should reuse this artwork. No new stock, price, loot or research source implied.

### 2026-10-20 — D-097 initial procedural equipment quality -> Appearance superseded

- Behavior: eight approved persistent quality frames across bag/loadout/details,
  forge source/result/owned chips/rank chain, actual profession tools and HUD
  weapon. Dgreen/Cblue, polishedS/radiantSSS; cyan selection stays distinct.
  Existing rank-specific item atlases, gameplay/stats/input geometry unchanged.
- Contract/files: Astra owns Stage3FantasyHud.Quality/Equipment/Forge/Professions/
  Dock, narrow HUD destruction cleanup. Eight128px textures plus one64pxglint,
  lazy bounded cache, nine-slice shared pixel edges and explicit scroll target/UV
  clipping. Main owns Debug-only opt-in Stage3ItemQualityReview/InterfaceCapture,
  checks/build/provenance/docs. No new scene/Inspector/package/asmdef wiring.
- Verification:17existing PlayMode equip/forge/held-tool/profession/startup checks
  pass; final visual-only repair compiles/builds. Mac637778927bytes,27finalnative
  frames at720/900/1080p. Main actual mouse choosesD/C/equipsC, oldSSSreturnsbag,
  then drags forge scrollbar and selectsSSS with readable labels/correct cap.
  Astra visual PASS all27finalframes plus two forge scroll frames, no blockers.
  No new attributable errors; old unusedURP
  shader warnings match D094baseline. Normal game restored5gold/0materials.
  Evidence: art/source/item_quality_cells_2026-10-20_verification.json.
- Remaining: Windows/Steam qualification. City/merchant still concept-phase;
  final merchant window, world site/NPC access, stock/prices/sell rules and scroll
  sources require their own ready brief. Reuse D097 there, no fake scroll rank.

### 2026-10-20 — D-095 city concepts -> A appearance selected

- Outcome: three original fantasy-city alternatives generated; owner chose A:
  orange terracotta gables, timber/cream houses, open stone square and one merchant
  visible from entrance. Other NPCs are ambient; future functions undecided.
- Contract/files: CITY_AND_TRADE.md owns active concept brief/selection; accepted
  scope/appearance D095; ART_DIRECTION/vision/plan updated. Three saved original
  PNGs in art/concepts/city_2026-10-20/, exact prompts/source/rights/hashes in
  art/source/city_2026-10-20_prompts.json and generation.json. No Assets/scene/
  prefab/package/code/console mutation; accumulated runtime work retained.
- Verification: all3PNG copies match originals; visually inspected, Astra concept
  PASS all3, no blockers. A has strongest small-city identity/clear merchant.
  No Unity tests/build required for this art-only concept phase.
- Owner revision: expand city and replace tent with a distinctive permanent
  shop. A_v2 shop contrast rejected. Latest a_cobalt_trading_house_v3.png keeps
  expanded orange-roof neighbourhoods, gives one shop a cobalt roof/open frontage/
  scales crest and clean forecourt. Main generation/inspection complete;
  Astra concept PASS, no blockers; owner approved A_v3 («Пойдет»). Exact edit targets/prompts/
  hashes in generation.json and city_2026-10-20_a_v2/v3_prompt.txt.
- Remaining: selected-city modular assets and actual world site/footprints,
  collision/occlusion, ambient NPC behavior, merchant interaction/fullscreen UI,
  stock/prices/research-item transactions. Fine detail needs scale simplification;
  depicted counts/water are illustrative, not approved gameplay.

### 2026-10-20 — D-094 throne reinforcement -> Runtime handoff

- Outcome: exactly3paid upgrades add5current/max HP without repairing old
  damage; current base5→10→15→20. Existing selected-throne card shows count,
  liveHP/next price/result and disabled cap. No fourth payment or resurrection.
- Contract/files: Stage2Run owns upgrade count/effective max/paid transaction
  and reentry guard; legacy checkpoint includes count. Stage2RunCatalog owns
  three costs/+5, with empty-array legacy fallback. Existing HordeRun updated
  additively by Stage3ThroneUpgradeInstaller.Install; Binding.Selection exposes
  TryUpgradeSelectedThrone and existing HUD event dispatch. D087 keeps CURRENT
  count/spending and refills effective max; full Reset clears count. No scene,
  Inspector, package, asmdef or new visual wiring; existing damage uses maxHP.
- Verification:30EditMode +9PlayMode pass (7new checks, run/gates/defeat/card
  regressions). Mac build637762906bytes; native CUA bought3/5→8/10→13/15→18/20,
  gold5005→4980→4920→4780, exact material prices, no spend on extra max click.
  Astra visual PASS9frames: base/damage/upgrade/unfunded at900p, cap above B
  additionally720/1080p. No new attributable errors. Qualification:
  art/source/throne_upgrades_2026-10-20_verification.json.
- Remaining: Windows/Steam qualification, price tuning, workshop advancement
  and forge scroll sources; bosses/camps/runes remain separate future slices.

### 2026-10-20 — D-093 generator advancement -> Runtime handoff

- Behavior: each sawmill/quarry/mine owns levels1–5 and output1/2/3/4/5 per
  original5/7/10s cycle. Four paid material+gold transitions use the existing
  selected-card action; prototype has no research gate. Constructor remains
  materials-only/level1. Upgrade preserves partial cycle and gives no immediate
  output; D087 keeps current level/progress/spend, full Reset clears buildings.
- Contracts/files: ProductionEntry additive upgrade table; ProductionBuilding
  owns Level/derived OutputAmount; plain Production validates paid transactions
  with reentry guard and snapshots carry level. Driver/binding expose production
  upgrade and live card facts. ProductionCatalog updated additively by
  Stage3ProductionUpgradeInstaller.Install; GUID/art/footprint/scene/packages kept.
- Verification:31EditMode +29affectedPlayMode +6selection/startupPlayMode pass
  (66unique;7new upgrade checks). Native Mac build637752035bytes. Scripted
  declared-QA setup captures3types at1/2/5 and max above B720/1080 plus unfunded
  state. Actual CUA clicks upgraded an individual sawmill2→3→4→5 with exact gold
  debits25/60/140; extra max click did not spend. Native review recorded in
  art/source/production_upgrades_2026-10-20_verification.json.
- Astra final visual PASS17frames: all3types/1/2/5, next prices/unfunded at900p,
  max over construction HUD720/1080p. No new attributable console errors.
- Remaining: Windows/Steam qualification, price tuning, future technology gate,
  workshop/throne advancement and forge scroll sources. No generator sale/merge/
  move, new image, profession bonus/XP or skipped-time/offline income introduced.

### 2026-10-20 — D-092 gathering professions -> Runtime handoff

- Behavior: O combat/profession tabs; independent miner/logger mastery1–100,
  actual manually granted material XP, own milestone points and strength/tempo/
  reach3ranks. Talents combine with equipped items and finite stock, never enable
  absent tools. Rank/tool changes cancel partial work until release. D-087
  preserves current mastery/points/ranks/stock; full Reset clears progression.
- Contracts/files: Stage2ProfessionCatalog/Professions/Driver in Integration;
  Economy separates item stats from profession modifiers and shares edge range
  across E/target/marker. Binding.Professions owns validated buys/dirty UI facts;
  Astra owns HUD.Professions DTO/layout, inner O navigation and art importer.
  Existing scene/GUID/packages retained. Resource data Stage3Professions.asset;
  installer and importer entry points recorded in runtime provenance JSON.
- Verification:38PlayMode (9new profession checks +29affected regressions),
  17legacy economy EditMode, final Mac build637740179bytes. Actual held-E grants,
  bounded remainder, item+talent composition, independent spend, Reset/retry and
  startup covered. CUA clicks in final native build bought miner2→3 and logger0→1;
  nine final720/900/1080p miner/logger/combat frames passed Astra review after
  repairing panel insets/tool-card height. No attributable console error.
- Evidence: art/source/gatherer_professions_2026-10-20_runtime.json and
  verification.json; Artifacts/S3-PROFESSIONS-20261020/. Explicit QA XP/resolution
  fixture is development opt-in only; ordinary startup has mastery1/XP0/points0.
- Remaining: Windows/Steam qualification, tuning, larger trees/explorer and
  production/workshop/throne upgrades plus forge scroll source. No disk save,
  new building, combat spell, hero HP/MP or armor redress added by this slice.

### 2026-10-20 — Gatherer rules / ImageGen concepts -> Owner approval

- Behavior: concept only. Two independent gathering professions with proposed
  actual-material XP, separate points, three first talents each and actual
  equipment+talent harvest comparison. D-087 keeps current mastery on retry.
- Contracts/files: HERO_PROGRESSION.md contains proposal/acceptance;
  DEVELOPMENT_PLAN/ART_DIRECTION track status; selected miner_v3/logger_v2 PNGs
  in art/concepts/gatherer_professions_2026-10-20/. Source prompts/hashes/rights
  and Astra review: art/source/gatherer_professions_2026-10-20_generation.json.
- Verification: main inspected extraction/tool contracts and current native
  UI; selected miner_v3/logger_v2 have Astra concept visual PASS. No C#/scene/package/runtime
  art changes, so Unity tests/build are not rerun for this concept-only step.
- Remaining: owner approval, XP calibration/data, gameplay implementation and
  Astra native UI with actual held-E/Reset/retry/720p verification. Larger trees,
  explorer, auto-production/building upgrades and scroll sources remain later.


### 2026-10-20 — D-091 bow/tools and bag correction -> Runtime handoff

- Behavior: equipped bow quality now uses correct upright carrying projection,
  straight string and facing/grip in world and I hero preview. Axe/pick actually
  show equipped quality during existing six-phase harvesting, via separate tool
  over additive clean work poses. Old duplicate baked tools removed from active
  layer; clothing/armor identity, original HeroArt/idle/walk images unchanged.
  Equip/replace/unequip moves the same ID between left bag and loadout. Bag
  excludes every equipped item, full lookup still serves slot selection/details.
- Contracts/files: Astra owns HeroAvatar, new HeroEquipmentArt/importer, additive
  Equipment textures/Resources, ForgeArt link and Equipment HUD/paperdoll. Main
  binds actual tool qualities, builds BagItems on state changes and owns tests/
  native fixture/docs. No gameplay stats/cost, package, scene or existing GUID
  migration. BuildAssets entry: Stage3HeroEquipmentArtImporter; source/rights/
  prompts/hashes art/source/equipment_visible_2026-10-20_generation.json.
- Verification:20/20 PlayMode incl quality/held E,6phases, clear physical approach,
  pause, unequip/return, motion, forge/retry, bag lifecycle/no loss, startup/combat.
  Final upright carry refinement reran4/4 subset,20unique latest PASS. Native
  macOS/Metal I mouse equip/unequip/re-equip and E wood/metal passed; both F/S
  inventory/world and tool raised/contact captured. Astra final visual PASS.
  Body side cropping adjusted to transparent gutter y280, sole pivot/PPU retained.
  Horizontal front bow candidate rejected on native review; final carry is upright.
  Mac build success629319823bytes; no new attributable compile/runtime errors.
  Verification art/source/equipment_visible_2026-10-20_verification.json.
- Fixture limits: QA explicitly grants S items and positions/camera, stops clock/
  combat; no natural loot proof. Final fullReset snapshot shows3equipped F1,
  empty bag; ordinary fresh game relaunched after QA. Artifacts captures/logs stay
  ignored. Windows unverified (local module absent). Remaining: earlier deferred
  loot/scroll sources/runes/disk saving, hero HP/armor protection; armor appearance
  explicitly remains unchanged. Prior uncommitted gates/equipment work retained.

### 2026-10-19 — D-089/D-090 equipment/forge -> Runtime handoff

- Behavior:8single-family item types, independent1–100 and8qualities, actual
  owned instances,5combat slots+2tools. Bow/wand single-target profiles and
  manual-tool power/tempo affect existing combat/economy. StarterF1bow/axe/pick
  retain playability; unequip blocks its action. Armor slots/stats work but hero
  protection/HP and armor redressing remain deferred, explicitly shown in UI.
- Local forge: resource-paid B/Base third blueprint, collision/clear entrance,
  E or selected-card service; CreateF1/Level/Reforge pay once, preserve item ID
  and independent axis.3building stages capD/B/S, upgrade only special scrolls;
  no source yet. SSS exclusively future ultra-rare loot, never forged.
- Contracts/files: Stage2EquipmentCatalog/Equipment/Driver, existing combat and
  Economy tool parameters; Stage3ForgeCatalog/Forge/ForgePresentation/ForgeArt;
  Equipment binding and Astra Inventory/Forge/Selection/Dock/Avatar. Resources
  assets wire existing scene at startup; no scene/package/GUID migration.
  Installer Stage3EquipmentInstaller.Install, importer Stage3ForgeArtImporter.
  D087 retry preserves current bag/equipped/stage/building/cost; fullReset resets.
- Verification: initial23/23, regression66/67 with one old clock test1ms precision
  failure; corrected tolerance and entire affected8-test class8/8. Unique latest
  PlayMode67/67 +EditMode24/24. Actual native Mac/Metal E and mouse creation,
  level2, reforgeE2, equip/world/card passed; explicit disposable QA stock/site,
  clock/combat stop and later loot grants documented (not natural-play evidence).
  Layout720/900/1080p reviewed. Final build success570544627bytes; ordinary
  unmodified starting wallet/items captured after axe UV fix. No new attributable
  console errors. Evidence art/source/equipment_forge_2026-10-19_verification.json;
  original art/prompts/hashes runtime.json. Windows unverified: no build module.
- Remaining: loot tables/source/rates (including SSS/forge-scroll source), rune
  effects, hero HP/protection/world enemies, armor redressing, disk saving and
  fine balance. TryReceiveLoot/TryReceiveForgeScrolls are future-source APIs only;
  ordinary game grants neither armor nor scrolls. Existing gates changes retained.

### 2026-10-19 — D-089 owner acceptance -> Equipment/forge implementation

- Outcome: direct human «Утверждаю все четыре» approves inventory v2, forge
  building v1, forge interface v2 and item-quality sheet v1. Layout, appearance
  and shown quality palette/lineage are accepted; no repeat approval needed.
- Contract/files: generation manifest approval/version hashes, D089, feature
  spec, ART_DIRECTION and canonical plan now agree. Existing runtime preserved.
- Verification: selected PNG hashes unchanged; JSON parses and diff whitespace
  checks pass. Documentation-only acceptance, no Unity tests/build rerun.
- Remaining: implement approved visuals through Astra and item/forge gameplay;
  prices/stats/stage budgets and shield compatibility still require concrete rules.


### 2026-10-19 — D-089 item/forge ImageGen concepts -> Owner approval

- Outcome: four coherent concepts saved: item inventory v2, fixed-view forge
  building v1, contextual fullscreen forge v2 and eight-quality item sheet v1.
  One basic type has eight actual appearance variants, separately from level1–100.
  Inventory has six combat slots plus axe/pickaxe, empty shield with bow. Forge
  E→D keepsLVL12, shows unspecified cap/prices; sheet compares all atLVL1.
- Contract/files: D089, EQUIPMENT_AND_RUNES, vision/plan/ART_DIRECTION updated.
  Source/prompts/owned reference roles/hashes/selected versus superseded revisions:
  art/source/equipment_forge_2026-10-19_generation.json. Latest D087 item retention
  replaces the old full-world-rollback paragraph in the feature spec.
- Verification: original generated PNGs copied unchanged and dimensions/hash
  checked; visual review covers slot count, item-quality independence, noSS,
  coherent existing shell/world style. Astra requested and ImageGen corrected
  accidental item-XP/occupied shield, contextual header/footer and forge cap.
  Astra final visual PASS on all4 selected images is recorded in the generation
  manifest. No new runtime
  art import, code/scene/package changes; Unity tests/build not rerun for concepts.
- Remaining: explicit owner appearance approval. Prices/stats/forge-stage budgets,
  shield compatibility and concrete gameplay implementation still need the active
  feature brief. Concepts do not approve new weapon mechanics or elemental items.


### 2026-10-19 — D-074/D-087/D-088 gates, recovery and Sleep -> Player review

- Outcome: local portal E opens/closes continuous daytime farming of the last
  cleared tier. Cadence/cap survive toggles; cutoff keeps existing creeps. Night
  tiers strengthen, unfinished hordes never overlap, early victory keeps night.
  Dwelling E/selected-card «Сон» ends at19:50 only outside any live/pending fight.
  Retry restores throne/morning for the same pending tier, keeping current world,
  XP/skills/resources/research/buildings/spending/damage. Fresh kills reward again.
- Contract/files: Stage2 Run/Catalog and Enemy/Group, retry-owner guards, HordeRun,
  local Stage3DaytimeGate, existing FantasyHud/Selection/Dwelling bindings.
  D087 cancels full-world/live-creep snapshot restoration for active Stage3;
  D088 restores intentional dwelling day advancement. Legacy Stage2 stays opt-out.
  Resource-population changes invalidate only legacy snapshots, not time anchors.
  No scene/prefab/GUID/package migration; gate is added by existing scene binding.
- Verification:37/37 EditMode and43/43 unique targeted PlayMode (latest per case),
  Mac/Metal development build; real native E open/close and selected Sleep click,
  actual08:00→19:50 and unchanged rewards. Explicit native fixture constructs one
  paid home/clears first night/advances dawn for setup; no natural-day pacing claim.
  Astra PASS five1600x900 hint/Sleep frames; no new attributable code/runtime errors.
  Evidence: art/source/gates_cycle_2026-10-19_verification.json. Temporary QA images
  stay ignored in Artifacts. Disposable player was closed after review.
- Remaining: Windows/Steam module absent; full-day/long-night balance and late
  tiers still need playtesting. Next canonical slice: equipment/forge. No new
  item/rune/profession/trader/boss content or save-to-disk system in this change.


### 2026-10-19 — D-085/D-086 all-changes checkpoint -> Next: D-074 gates/clock

- Outcome: owner confirms the accepted level-up choir/echo without any impact;
  sound experiments are deferred. All current progression code/assets/tests and
  canonical documentation are included in the requested Git commit/push.
- Verification: unchanged runtime WAV byte-matches accepted voice-only source;
  existing13/13 PlayMode, successful final Mac build, native audio binding and
  three-resolution Astra visual PASS remain applicable. No new runtime edits.
- Next: remove both time skips and implement repeatable daytime gates/mandatory
  stronger night hordes. First extend retry snapshots for already-live enemies
  and settle daytime-defeat/cutoff behavior per PORTAL_CONTROL.md. Equipment/
  forge, professions/base, trade/research, world loot and bosses follow the
  canonical DEVELOPMENT_PLAN order. This checkpoint does not start that work.



### 2026-10-19 — D-085/D-086 playable hero progression -> Player review

- Behavior: global XP once for hero/owned-tower confirmed kills, level cap100,
  point spending, free BlinkI plus four cursor ranges and three personal passives.
  Fullscreen O/live v4 XP/level/points/CD and approved polygon_v3 celebration work.
  Latest accepted audio is female three-part choir/echo only; hammer deferred.
- Contracts/files: existing Stage2 Integration adds catalog/plain rules/opt-in
  driver; combat/mover apply personal bonuses. Stage3 binding/HUD/FX/level-up
  integrate without new managers/packages/UI stack or scene/Inspector migration.
  D-062 snapshot restores XP/points/ranks/CD and derived stats without reward
  replay. Runtime tuning is Stage3HeroProgression.asset, not Markdown examples.
- Verification:13/13 targeted PlayMode including tower/hero XP, safe cursor Blink,
  ranks/cap/pause and full retry restoration plus bow/startup regressions. Actual
  native720p/900p/1080p final visual PASS Astra; actual O purchase/HUD targeting
  clicks pass. Final voice-only Mac/Metal build succeeds; native source/listener/
  clip binding passes. No new attributable console errors. Windows is unverified.
  One retained actual image and hashes/contracts/limits are in
  art/source/hero_progression_2026-10-19_verification.json. Full transient captures
  stay ignored in Artifacts; original prompts/rights remain in generation JSON.
- Remaining: fine balance, HP/MP and broader skills/professions/equipment/runes,
  D-074 gates/time-cycle and world combat remain separate future slices. Owner
  asks to compare Skyrim/Elden Ring audio references; no proprietary game sound
  is imported and no replacement impact is approved by that reference request.


### 2026-10-19 — D-085 first progression ImageGen concepts -> Owner / Astra

- Outcome: three coherent proposed concepts for fullscreen O, existing v4 HUD
  with level/XP/Blink and local Blink/XP/level-up states. Personal passives proposed:
  single-target damage, attack speed, movement speed. Cap100 and Blink ranges
  follow accepted intent; XP/point/cooldown values remain illustrative.
- Contracts/files: HERO_PROGRESSION first-slice scope supersedes the older extra-
  attack-spell start, plan/decisions/PLAYER_INTERFACE/ART_DIRECTION updated.
  Original final PNGs in art/concepts/hero_progression_2026-10-19/; full built-in
  prompts/input roles/rights/hashes: art/source/hero_progression_2026-10-19_generation.json.
- Verification: main/Astra reviewed all3; final Astra PASS after live XP label,
  blocked landing mark and shared glyph corrections. LVL3/light-step1/3/one
  point accounting consistent; ten HUD cells, inactiveHPMP and restrained FX.
  Assets/Packages/ProjectSettings unchanged. No Unity tests/build needed for
  these concepts; no approval inferred from internal review or generation.
- Remaining: explicit owner appearance approval, then Astra visual runtime and
  main XP/level/passive/Blink integration, including D-062 checkpoint. Balance/
  hotkey and broader skill/profession budgets not settled by concept numbers.

### 2026-10-19 — D-084 future landscape brief -> Deferred art pass

- Outcome: recorded the owner's distant post-functionality landscape revision:
  scorched/undead-corrupted invasion road, etched runes throughout, red glow at night.
- Contract/files: DECISIONS D-084, ART_DIRECTION, GAME_VISION and canonical
  DEVELOPMENT_PLAN. Environmental road runes are distinct from loot runes.
- Verification: documentation consistency; no runtime/art changes or Unity run
  needed for this textual direction. Included in the current all-changes checkpoint.
- Remaining: activate only after necessary gameplay work, then ImageGen appearance
  approval and Astra implementation; no new route layout or tuning selected now.

### 2026-10-19 — D-083 direct startup without visible graybox -> Player review

- Behavior: immediate prototype entry, no menu/loading interaction or extra Play
  click. Final world/hero/landmarks/HUD exist before the first game-camera render;
  saved graybox/resource/debug-ground graphics and legacy HUD/minimap do not flash.
- Contracts/files: Astra TacticalPresentation replaces delayed coroutine Start
  with synchronous Start(-500), explicitly calls idempotent composition.Initialize,
  sets IsInitialized only after completion. Existing HUD binding(-300) then binds
  within that first frame. Root adds opt-in first-render observation in existing
  InterfaceCapture and independent first-LateUpdate/reload tests. EditorBuildSettings
  prepends current Stage3, retaining both older enabled scene entries. Scene/prefab/
  art/data/package references unchanged; no Inspector migration or new manager.
- Verification:5/5 focused PlayMode (first-frame/reload, legacy Stage2, resource
  binding, keyboard UI, defeat restoration), then2/2 startup cases after ordinary
  build-entry change. Mac/Metal build passes. Three fresh player processes at
  1280x720/1600x900/1920x1080, fixed and random seeds: all24 first8 camera renders
  report ready from Unity frame1, with no skipped warm-up frames. Astra accepts
  first/eighth actual frames at all resolutions; no graybox/duplicate HUD/overlap.
  No new attributable console errors.170/171 protected baseline files unchanged;
  sole intentional setting change is the prepended current build scene. Retain
  one first-frame PNG and compact evidence; other captures/logs/XML/build ignored.
  Evidence: art/source/direct_start_2026-10-19_verification.json.
- Remaining: Windows first-frame qualification unavailable without build module.
  Main menu/loading UI and general shader warm-up remain deferred; this does not
  claim to fix the separate unreproduced Metal presentation wait from D-082.

### 2026-10-19 — D-082 compact LVL / approved icon / walking profile -> Player review

- Behavior: smaller reflective silver LVL N labels and the owner-approved
  ImageGen chevron replace the oversized Level tags and code upgrade arrow.
  Existing blue merge, prices, availability and v4 card layout remain intact.
- Contracts/files: Astra owns HUD Selection/Ornaments and runtime PNG import;
  main owns cached GUIContent/slice arrays/resource/clock/bow labels, minimap
  glyph content, resource sprite state/affine foreground fade, and opt-in player
  profiling. QualitySettings changes only PC vSyncCount 0→1 for display-paced
  output. No scene/Inspector/prefab/package or gameplay migration. PNG loads
  once through the existing Resources mechanism; original/prompt/approval kept.
- Verification: 11 unique PlayMode pass, final Mac/Metal development build;
  45 actual action frames at720p/900p/1080p plus normal HUD, zoom2.5/5/10.
  Astra final visual PASS at all three resolutions; code frozen after review.
  Same45s seed/route/full-HUD uncapped comparison: p50 6.00→3.00ms, p95
  7.69→3.40ms, median GC allocation20,076→5,404B/frame, collections131→74.
  Final ordinary native WASD three passes226.9m: moving p95 16.97/max17.68ms,
  no overlap recovery. Entire85s observation also retains four stationary/input-
  boundary outliers33–100ms; these are not removed from evidence. One earlier
  native run contained a1.02s Gfx.WaitForPresentOnGfxThread wait without GC;
  later uncapped/paced runs did not reproduce it. No full-elimination claim.
  No new attributable console errors.169/170 protected baseline files remain
  unchanged; the sole intentional change is PC vSync. New runtime PNG/meta only.
  Evidence: art/source/tower_lvl_performance_2026-10-19_verification.json.
  One representative actual frame and generated original/prompt are retained;
  other screenshots/profiles/XML/logs/build remain ignored. Normal player reopened.
- Remaining: rare Metal presentation-wait root cause and Windows qualification
  (module absent); broader hardware/performance budgets are not established.
  No resource streaming rewrite, tower tiers or additional gameplay in this slice.

### 2026-10-19 — D-081 merge feedback and tower levels -> Player review

- Behavior: compatible equal-type/equal-level merge ghost/footprint is light blue;
  ordinary movement/construction remains green and invalid drops red. Compact
  gray/silver Level tags above built towers/barricades show live levels after
  paid upgrades/merges. Screen size is stable through zoom; tags neither block
  input nor leak over HUD/modal pages. No tiers or gameplay/price changes.
- Contracts/files: binding Selection partial adds read-only DragWillMerge;
  Astra updates DragPresentation and HUD Selection/OnGUI. Renderer arrays cache
  per tower, dead entries prune; actual visible sprite corners anchor the tag.
  Existing opt-in action QA adds zoom captures and semantic checks; existing
  PlayMode drag case verifies distinct state/cleanup. No new scene/Inspector
  wiring, raster assets, data, packages or serialized settings.
- Verification: 4/4 focused PlayMode; final Mac/Metal development build; 45 actual
  frames at 1280x720/1600x900/1920x1080, zoom 2.5/5/10. Actual API transactions
  verify paid levels, matching merge, move, invalid drop and sale; screenshots
  verify presentation. Native mouse is not requalified in this visual-only slice.
  No attributable runtime exceptions; all 170 protected baseline files unchanged.
  Evidence: art/source/tower_merge_levels_2026-10-19_verification.json; one real
  merge frame retained, other captures/logs/XML/build ignored. Normal player reopened.
  Astra final visual PASS: blue/green/red, live Level 1/2/5, stable zoom sizing,
  all three tower silhouettes and 720p readability; no revisions required.
- Remaining: Windows qualification unavailable without module; per-level tower
  artwork, new tiers and other building progression remain outside this request.

### 2026-10-19 — D-080 construction-selection correction -> Player review

- Behavior: no separate Select button in any B category; each card directly arms
  its blueprint. Finished owned buildings select with B open, show the existing
  strip above it, and permit world move/merge plus card upgrade/sale. World press
  on a built object cancels tower/production/workshop/dwelling drafts before any
  purchase. Card appears on release/drag so the initial press cannot hit its actions.
- Cause/fix: D-079 binding/card incorrectly required Window.None, omitting the
  normal post-build state. Native retest also revealed FastMouse merging a press
  into held motion before onEvent. Binding captures press origin with redundant
  merging disabled while it owns input; disable/unbind restores the prior runtime
  setting. No serialized settings, scene, prefab, tier or economy migration.
- Files: Astra FantasyHud.Construction/Selection partials; root Binding.Selection
  and lifecycle, capture/review helper and PlayerInterfaceTests. Canonical UI/tower
  specs/vision/plan/AGENTS updated. D-079 tower/combat/checkpoint rules unchanged.
- Verification:24/24 unique PlayMode cases, final targeted4/4 after buffered input
  changes; explicit down+held-motion+up in one update and original-setting cleanup.
  Actual native catalogue click, built selection, move, merge, upgrade and sale
  all PASS with B open. Final Mac/Metal build;46 actual frames at720p/900p/1080p;
  no attributable errors. Astra final frame review recorded in evidence.
  Evidence: art/source/construction_selection_fix_2026-10-19_verification.json;
  representative: art/review/construction_selection_fix_game_2026-10-19.png.
- Remaining: Windows build/player unverified (no local module); future gameplay
  slices unchanged. Prior D-079 proof did not cover this ordinary construction
  path and is superseded for input qualification. No commit/push requested.


### 2026-10-19 — D-079 tower/barricade actions -> Next gameplay slice

- Behavior: card pays resource upgrades and sells for floor(50% actual costs)
  per currency; world drag merges matching type/level into recipient. Preparation
  permits free empty-cell movement. Source stays until commit, UI/cancel/invalid
  drop never consumes it; pause/modal/loss/reset/rewind clears drag/selection.
  HP fraction/cooldown readiness persist; sale pays only once and releases cell.
- Contracts/files: DefenseCatalog serialized upgrade rows (legacy empty/base-only),
  Tower Level/Invested, Defense action transactions and snapshot, Economy atomic
  four-currency credit/overflow checks; Selection binding, Astra HUD action DTO/
  deferred events and native ghost partial. D-062 rewind restores costs/levels/
  HP/cooldown/cells together. No scene/Inspector wiring changes.
- Data migration: only existing active Stage3 Rapid/Longshot/barricade catalogs gain
  editable prototype tiers; Stage3 Rapid five-gold build surcharge explicitly
  removed. Historical Stage2 assets/prices, prefabs, scenes, artwork, packages and
  ProjectSettings preserved. Five current levels are tuning, not a permanent cap.
- Verification: 76/76 EditMode. Full relevant PlayMode first26/28; stale legacy
  gold expectation and test drop under HUD corrected; targeted rerun2/2, final
  unique28/28. Actual pre-wave retry includes paid upgrades/merge/sale. Native
  Upgrade/Sell clicks and exact resource payout passed; final Mac/Metal build,
  30 actual frames at1280x720/1600x900/1920x1080 and Astra final visual PASS.
  Evidence/hashes: art/source/building_actions_2026-10-19_verification.json;
  representative frame: art/review/building_actions_game_2026-10-19.png.
- Remaining: Windows build/player unverified (local module absent); production/
  workshop/dwelling/throne upgrades, permanent balance/caps, per-level art and
  future research unlocks remain separate slices. No commit/push requested.


### 2026-10-19 — D-078 implemented HUD / owned-building selection -> Next gameplay slice

- Behavior: approved v4 implemented by Astra, then corrected for closer
  ornament/proportion conformity and HP/MP/XP labels. Connected main dock,
  navigation ribbon, ten empty reserves, native selected-building strip.
  Own-building picking/live stats/thumbnail/highlight, research overview link,
  stale-selection cleanup and complete world-pointer protection are integrated.
- Contracts/files: FantasyHud main/Construction plus Dock/Selection/Ornaments
  partials; Binding.Selection and narrow read-only tower/production/workshop/
  dwelling accessors. Existing UI scene wiring suffices, no serialized migration.
  Docs and art/source/hud_v4_2026-10-19_verification.json describe current state.
- Verification: 25/25 PlayMode, Mac/Metal development build, 27 actual frames at
  1280×720/1600×900/1920×1080, two ordinary native clicks (× and Research).
  After tests only production text baselines changed; rebuilt/reviewed all
  resolutions. No attributable errors in final player QA. Windows module absent.
  186 protected assets/settings files, including 66 runtime PNGs, unchanged.
  Astra final visual review PASS: latest 1920 full set plus updated 1600 production/
  1280 normal+tower; no blocking defects or further visual work in this slice.
- Limits/remaining: HP/MP/XP0/0+«Не открыто» is disabled UI, not hero balance;
  progression sources/loadout cap and gameplay paid-upgrade/sale/drag/merge
  remain planned slices. Research still needs local E; dwelling retains current
  D-065 behavior until day-cycle revision. Prices/investment rules need data.
  Full natural playthrough/Windows acceptance is not claimed by these captures.

### 2026-10-19 — D-078 navigation ribbon/numeric bars -> Owner / Astra

- Behavior: separate narrow full-width navigation between selected strip and
  main dock; center has 2×5 empty reserves; vivid numeric HP/mana/XP examples.
  Ten-slot capacity does not confirm ten equipped skills. Move remains absent.
- Contracts/files: two hud_ribbon_*_v4.png concepts, generation metadata;
  PLAYER_INTERFACE/HERO_PROGRESSION, vision/plan/art direction/decisions,
  AGENTS and README. v3 source marked superseded; original assets preserved.
- Verification: Astra reviewed both frames; stack, ten reserves, readable
  colors/numbers and consistent dock pass, no blocking visual defect. PNG
  hashes/local references/JSON checked; 1031 runtime/project files unchanged.
  No Unity tests/build/console session or Inspector wiring for concept-only work.
- Remaining: owner v4 appearance approval, then Astra visuals/main selection,
  input/data integration and actual-frame verification. HP/mana/XP need real
  gameplay sources; mockup values are not balance. Resolve skill loadout cap
  and gameplay upgrades/drag in their ready briefs before implementation.

### 2026-10-19 — D-077 unified HUD revision -> Owner / Astra

- Behavior: one continuous bottom HUD and same-width selected strip immediately
  above; no Move button, world drag handles move/merge. Two ImageGen v3 states
  replace unapproved compact A/B. Owner approval remains pending.
- Contracts/files: two final PNGs and unified_hud_2026-10-19_generation.json;
  updated PLAYER_INTERFACE/TOWER_UPGRADES/RESEARCH, vision/plan/AGENTS/README,
  ART_DIRECTION and D-077. Old generation record marked superseded; no asset
  overwrite or new gameplay prices/values.
- Verification: Astra/main visual inspection checks continuous frame/navigation,
  five reserves/current numbers, no Move and readable actions. No blocking
  concept defect. Height/world coverage remains an owner judgment. File hashes,
  local links and unchanged runtime/project content checked; no Unity test/build
  or Inspector migration for concept-only work.
- Remaining: approve v3, then Astra visual implementation plus main selection/
  data/input integration and actual-frame checks. Preserve original art; resolve
  gameplay drag/upgrade rules in their own ready brief, not by a mockup.

### 2026-10-19 — D-076 ImageGen selection-card concepts -> Owner / Astra

- Delivered: horizontal A, right-card B and shared tower/workshop/forge/
  sawmill/throne/disabled-state sheet, pending owner choice. User explicitly
  requests approval before Astra implementation; no approval inferred.
- Contracts/files: three final PNGs in art/concepts/building_selection_2026-10-19/;
  full prompts/inputs/corrections/hashes/review in generation JSON. Updated
  PLAYER_INTERFACE, ART_DIRECTION, DEVELOPMENT_PLAN and D-076. Only final
  requested images retained; discarded attempts remain outside repository.
- Verification: main and Astra inspected outputs, Cyrillic/data, card coverage,
  accessible HUD/navigation/minimap and corrected dome identity. Astra recommends
  A; no blocking concept defect. +5 throne means maximum HP, not healing.
  Generated world/HUD drift is nonbinding; no actual UI/drag/upgrade behavior
  is qualified. Runtime/project hashes unchanged; no Unity tests/build needed
  for concept-only output and no scene/Inspector wiring.
- Remaining: owner approves layout/content, then Astra implements visual UI and
  main integrates selection/real parameters/input. Keep future upgrades/merges/
  prices in separate ready briefs; no fake working actions or new forge/world
  building from a concept. Qualify native resolutions and actual runtime later.

### 2026-10-19 — D-075 design -> Stage 3 gameplay/UI owners

- Behavior/requirements: three distinct bosses end the campaign; TD main XP/
  gold and world better loot chances; six gear slots, levels 1–100 and eight
  qualities with no SS; forge/crafted tools; tower paid upgrades AND merging,
  move/50% sale; any own building has a compact above-HUD card/full-page link.
  New construction uses resources only; upgrades/research may also use gold.
  Workshop/production advance; throne has three +5-maxHP upgrades. Hut purpose
  and boss venue remain open.
- Contracts/files: BOSSES_AND_VICTORY.md and TOWER_UPGRADES.md added;
  EQUIPMENT_AND_RUNES.md revised. Vision, plan, D-075, AGENTS, README, art brief
  and hero/research/production/objectives/economy/building/tower/UI/portal/retry
  specs agree. Counts for forge/workshop/production/tower levels and HP/refund/
  merge-type rules are proposals; owner's direct requirements are labelled.
- Visual review: GPT-6 Astra proposed card layout and optional SSS silhouette
  treatment in text only. No ImageGen/asset/render/UI implementation produced;
  sketches still precede owner approval and Astra implementation.
- Verification: 20 touched documents checked for local Markdown links/conflict
  markers, quality/slot/HP arithmetic and git diff whitespace. All 1,031
  Assets/Packages/ProjectSettings files match the pre-design hashes. No Unity
  tests/build rerun for prose; prior D-062/D-073 qualification is preserved,
  not evidence for new features. No Inspector wiring or migration performed.
- Remaining: choose boss encounter/death rules, shield compatibility, tower
  merge-type/move timing and refund/health/cooldown details; tune prices/level
  caps and farm checkpoint/night pacing. Follow the one ordered D-075 slice
  sequence, migrate actual Rapid gold/free workshop data explicitly, then
  qualify new runtime. Commercial MVP extent remains open.

### 2026-09-25 — Current vision HTML and roadmap cleanup -> All workstreams

- Behavior: the current game design is presented in `reports/game-design.html`;
  `reports/development-roadmap.html` now follows Stage 0–7 with a short active
  path, a model/effort recommendation per task or stage, and the Stage 3
  Blender trigger immediately after the first playable gate.
- Contracts/files: updated `README.md`, reference index, `.gitignore`,
  `DEVELOPMENT_PLAN.md`, `GAME_VISION.md`, D-015 and the two HTML reports.
  The old 55-package roadmap was replaced; historical feature specs,
  measurements and original local reference images were preserved.
- Verification: both HTML documents parse with balanced tags, unique IDs and
  working local links/anchors; `git diff --check` passes. Browser preview of
  local `file://` pages was blocked by the browser URL policy. No Unity
  runtime/scene/asset changed in this documentation task, so Unity tests were
  not rerun.
- Remaining: Stage 2 Integration verifies the in-progress two-tower scene and
  complete Windows run. After acceptance, start one Blender/Unity pilot before
  mass model production.

### 2026-09-25 — Design audit / D-015 -> Integration and later Art

- Behavior: first playable is now one short understandable gather/build/defend
  run on a compact map with two towers and the existing three resources.
  Pixel art is no longer the target; soft cartoon isometric presentation is
  deferred until the core run works. Optional objectives, more enemy/content
  breadth, final UI/audio, fine balance and speculative token packages are
  removed from the first-playable gate.
- Contracts/files: `GAME_VISION.md`, `ART_DIRECTION.md`, `DEVELOPMENT_PLAN.md`,
  `STAGE2_VERTICAL_SLICE.md`, D-015 in `DECISIONS.md`, and status notes in map,
  run, economy, enemy and objective specs. Existing Unity scene/code changes
  by Integration were inspected and left untouched.
- Verification: repository and Unity 6000.6.2f1 scene/data inspected;
  documentation references checked and `git diff --check` passed. No runtime
  behavior changed by this audit, so Unity tests/build were not rerun.
- Remaining: Integration verifies the in-progress Longshot scene wiring and
  one full Windows run; live play decides whether 100×100 feels compact and
  which temporary feedback is still unclear. After that gate, Art tests a
  small target-look asset before choosing runtime 3D or rendered sprites.

### 2026-09-25 — Integration S2-INT-02 depleted markers and metal input -> accepted

- Behavior: wood, stone and metal markers disappear at zero stock and return
  after active-time recovery or run restart. The Development Build shows the
  remaining recovery time, prompts hold E in range, and explains that gathering
  requires R restart after win/loss. The reported screenshot showed Lost and
  metal 18 m away; active-run held E at the metal anchor gathers successfully.
- Contracts/files: `Stage2GameComposition.cs`, Integration PlayMode tests,
  `STAGE2_ECONOMY.md`, vertical-slice spec and canonical plan. Economy rules,
  map asset and scene wiring are unchanged.
- Verification: focused Integration PlayMode 15/15, including input-driven metal
  gathering and all three marker lifecycles. Windows Development Build logged
  `S2_INT_BUILD_OK` (188083804 bytes); refreshed Integration.dll timestamp
  2026-09-25 06:38 UTC. The relaunched Player is responsive and logged both
  Stage 2 startup messages with no matching Player error/exception.
  `git diff --check` passed. Unity CLI reported its five-second main-thread
  timeout while Unity completed the build; the existing optional Pipeline
  warning remains. The player checked the relaunched build and confirmed
  "все гуд" on 2026-09-25.
- Remaining: Stage 2 external playtest and gate evidence remain open.

### 2026-09-25 — Integration S2-INT-02 resource visibility correction -> human check

- Behavior: after the user saw no resource points in the current 100x100
  Development Build, Integration added visible, nonblocking wood/stone/metal
  markers at the existing Economy anchors. The temporary HUD now shows stock,
  distance and WASD direction, plus a screen-edge label for hidden sources.
- Contracts/files: `Stage2GameComposition.cs`, its PlayMode integration test,
  `STAGE2_ECONOMY.md`, vertical-slice spec and canonical plan. Runtime markers
  reuse project scene sprites; scene/map/economy data and GUIDs stay unchanged.
- Verification: PlayMode 35/35; one Editor screenshot confirms directions and
  offscreen labels. Windows Development Build logged `S2_INT_BUILD_OK`
  (188082452 bytes); updated Integration.dll timestamp 2026-09-25 06:29 UTC.
  The refreshed Player opened a responsive window and logged both
  `S2_INT_STARTUP_OK` and `S2_INT_GAMEPLAY_READY` with unchanged map hash
  `77A23824A9BF24DA`, no matching Player error/exception. `git diff --check`
  passed. Unity CLI itself reported its known five-second main-thread timeout
  while Unity completed the build; optional Pipeline warning remains.
- Remaining: human check of source readability and gameplay in the opened
  Player; final S2-ART-02/S2-UI-02 visuals and external Stage 2 gate.

### 2026-09-24 — S2-WAV-01 cadence implementation -> human manual check

- Behavior: finite waves, persistent world/inventory/health/recovery between
  waves, final-only win and full-run restart. Legacy one-wave catalog fallback
  is preserved; the existing development readout displays wave number/count.
- Files/contracts: RunState catalog/runtime/builder/tests, prototype data and
  Integration regression/readout. The saved scene requires no rewiring.
- Verification: EditMode 70/70, PlayMode 34/34. Local `Artifacts/S2-Cadence/`
  holds completed near-throne and entrance probe reports plus an excluded
  reused-session input failure. Repeated live comparisons exceeded the useful
  scope of this implementation; future manual game checks belong to the human
  at task/batch completion per their instruction. On 2026-09-25 the clipped
  control-help line was split; a new Windows Development Build logged
  `S2_INT_BUILD_OK` (188079016 bytes), and the Integration/RunState Player
  assemblies carry the new build timestamp. Unity CLI returned a five-second
  main-thread timeout error while the Editor continued and completed the
  build; the other three warnings match the earlier optional Pipeline/hidden
  shader build warnings. `git diff --check` passed.
- Remaining: human launch and one manual run of the new Windows build; final
  pacing and external-player gate. S2-WAV-02 remains complete.

### 2026-09-24 — Human map direction -> Design/Map future revision

- Outcome: current focus remains proving the playable core. Rectangular
  expansion, an offset throne with deeper resource territory, a winding gray
  area and possible gray-only tower placement are tentative future options.
- Contracts/files: clarification in `STAGE2_MAP.md`, linked from P-008 in
  `DECISIONS.md` and Checkpoint C in `DEVELOPMENT_PLAN.md`. Current scene,
  runtime, masks and accepted flat-map/Blender scope are unchanged.
- Verification: documentation consistency and diff whitespace check; no new
  runtime behavior, so Unity tests are not repeated for this clarification.
- Remaining: human acceptance of pending slice proposals; any future map
  revision needs its own concrete scope and checks when activated. This note
  does not accept cadence/enemy roles or close the Stage 2 gate.

### 2026-09-24 — Design/Platform/Map S2-MAP-03 -> human P-008 review

- Outcome: recommends retaining 100x100/current masks for Stage 2, with no
  200x200 or compulsory topology rebuild. Actual WASD source travel is
  1.588/4.654/9.221 s. Three paid entrance-neighbor towers cause full siege;
  the earlier transverse-wall-only rework hypothesis was withdrawn after
  checking compact layouts. Human comprehension and entrance-camping balance
  remain unproven.
- Contracts/files: expanded `STAGE2_MAP.md`; `production/measurements/`
  supplies reproducible, scene-unsaved Editor probes and a standard-library
  summary script. Evidence is local in `Artifacts/S2-DecisionBatch/`.
  Authored map hash stays `77A23824A9BF24DA`; scene/data/runtime unchanged.
- Verification: three warmups + 30 samples for eight layouts; search medians
  1.018–1.044 ms, p95 <=1.450 ms. Recorded i5-12400F/GTX1060/Windows11
  Editor Play, 1101x506; 64 movers + 1 Hz revisions: sampled frame p95
  2.351 ms, Editor allocated-memory median 839.47 MiB. This is not Player
  GPU/RAM qualification; the Mono allocation counter was unavailable.
- Remaining: accept/revise P-008 explicitly. The proposal is a Stage 2 size
  selection, not a permanent MVP lock; external player/layout observation
  and representative-content performance remain later gates.

### 2026-09-24 — Design/Waves S2-WAV-01 -> human P-004 review

- Outcome: measured full live policies: 29.489 s preparation + 35.594 s wave
  wins at 65.083 s; no-action automatic start loses at 219.484 s, including
  180 s idle preparation. Defended preparation gathers/spends 3 of each
  resource, buys 3 towers, leaves zero inventory and full throne health;
  no wave gathering/spend or objective rewards. Recommend rework into a
  three-round diagnostic candidate, 45 s first/30 s later preparation,
  early start and surviving run state preserved between rounds.
- Contracts/files: updated `STAGE2_RUN_STATE.md`, canonical plan/spec and
  pending P-004 entry. Current one-wave catalog and runtime are unchanged.
- Verification: actual WASD/E and normal run updates at timeScale 1; one
  replay per policy. Build transactions use the real API with fixed one-second
  spacing, not human build timing. No fresh-player observation claimed.
- Remaining: human cadence approval plus implementation/trial. Lost-tower
  recovery under pressure and the unchanged 15–25 minute target remain
  unproven; do not close P-004 or Checkpoint C from the one-wave measurements.

### 2026-09-24 — Design/Waves S2-AI-01 -> human scope / Defense / Integration

- Outcome: delivered `STAGE2_ENEMY_ROLES.md`: baseline ground enemy plus
  one siege specialist, identical routing/hero exclusion and initially only
  siege output differing. The observable choice is open detour versus full
  blockage; no third type, boss or new damage abstraction is needed yet.
- Contracts/files: brief specifies immutable spawn profiles, existing
  tower damage/identity release, wave-owned composition, required silhouette
  and next-wave cue, negative cases and rejection criteria. New profiles
  and scene references were not implemented.
- Verification: a paid three-tower entrance cap, with tower fire suppressed,
  breaches in 5.0 vs 2.5 simulated seconds under a temporary double-siege
  catalog copy; both reopen a throne route. Consolidated post-draft regression:
  EditMode 66/66, PlayMode 33/33, no skips. Live probes and a subsequent fresh
  Play session with advancing frames ended with 0 Console errors/warnings.
  The input device and callbacks were removed, background execution restored,
  and scene plus five authored data assets matched the recorded SHA256 values.
  A first probe-script compile attempt was corrected for
  eval_file statement syntax; no project C# compiler failure occurred.
- Remaining: human acceptance of the proposed breadth; live comparative
  wave/layout and external readability tests. Stage 2 remains active.
  S2-DEF-02 and later packages have not been started by this batch.

### 2026-09-24 — Platform/QA S2-QA-01 -> Design/Map S2-MAP-03 boundary

- Outcome: the first integrated Stage 2 checkpoint is qualified in the
  Editor and as a Windows x64 Development Player. Stage 2 remains active;
  this is not a map-size, pacing or external-player gate decision.
- Contracts/files: no gameplay/data contract changed. Development build at
  `Builds/Windows/Not3AStage2.exe` uses Stage 2 first and Stage 1 second;
  local smoke log is `Artifacts/S2-QA-01/PlayerLong.log` (both paths Git-ignored).
  Canonical plan/spec/handoff record this checkpoint. Unity's saved scene
  empty-name whitespace was normalized without changing serialized values.
- Verification: EditMode 66/66 and PlayMode 33/33, no skips. One intermediary
  PlayMode invocation yielded 0 tests due Test Runner state and was discarded;
  domain reload followed by the full 33-test pass. Fresh Editor Play after
  reload/Console clear: 0 errors, 0 warnings, no compilation failure. Build
  logged `S2_INT_BUILD_OK`, 188076750 bytes; Player ran 30 seconds on Windows
  11/GTX 1060, logging `S2_INT_STARTUP_OK` and `S2_INT_GAMEPLAY_READY` with
  hash `77A23824A9BF24DA`, no error/exception lines. Build-only warnings
  from optional Pipeline configuration and a stripped hidden shader were
  observed; neither prevented startup. `git diff --check` passes.
- Remaining: S2-MAP-03 needs measured player travel, frame/memory/replans
  and explicit P-008 size decision. The human authorized autonomous packages
  only up to this boundary, so no S2-MAP-03 work starts until they direct it.
  P-004 pacing, representative UI/art/audio, external fresh-player playtest
  and final continue/rework/stop gate remain open.

### 2026-09-24 — Integration S2-INT-02 -> Platform/QA S2-QA-01

- Outcome: the saved 100x100 Stage 2 scene now runs the connected gather ->
  paid tower -> route/wave/siege -> throne outcome -> restart loop. One
  scene-owned composition shares Navigation, Economy inventory, Defense,
  enemy group and Run state; the Stage 1 scene is unchanged.
- Contracts/files: `Assets/Scenes/Stage2VerticalSlice.unity` keeps its six
  roots. `Stage 2 Scene Composition` holds the existing bootstrap plus
  `Stage2GatheringDriver`, `Stage2BuildDriver`, `Stage2RunDriver` and
  `Stage2GameComposition`. Gathering serializes the authored map, Economy
  prototype catalog and hero transform. Composition serializes bootstrap,
  Economy/Defense/Enemy/Run prototype catalogs and all three drivers; it
  initializes one shared Navigation and enemy group, binds enemy protection
  and attack/run state, and exposes a development-build-only readout.
  `Assets/Stage2/Integration/Editor/Stage2IntegrationSceneBuilder.cs` can
  wire the existing clean scene or rebuild with these links; Integration
  runtime/test asmdefs and PlayMode tests reflect the new contracts.
- Verification: Unity compilation passed; fresh scene Play entry logged
  `S2_INT_GAMEPLAY_READY` with map hash `77A23824A9BF24DA`, 0 current Console
  errors/warnings. Full PlayMode 33/33: authored anchors yield all three
  resources, a paid tower spends each once and lengthens the route, a complete
  x=0 wall causes a real-map enemy to destroy the blocker and reopen the route,
  paid towers naturally win a moving wave, an unopposed wave destroys the
  throne, and restart clears population/towers/inventory. The wall test uses
  temporary occupancy tokens outside the paid blocker to isolate siege; it
  is a system verification, not a claim that one run can purchase 100 towers.
- Remaining: Platform/QA runs full EditMode/PlayMode regression, checks fresh
  Console, builds and launches Windows x64 Development. S2-MAP-03 still needs
  measured travel/performance and a human size decision; P-004, UI clarity,
  external player observation and the Stage 2 gate remain open.

### 2026-09-24 — Defense S2-DEF-01-FIX -> Integration/Run state/QA

- Outcome: one paid prototype tower now targets and damages the nearest
  living enemy at a stable interval; pause freezes its timer. A moving
  one-wave PlayMode run wins naturally, while differentiated roles remain
  S2-DEF-02 work.
- Contracts/files: extended `Assets/Stage2/Defense/` tower runtime/catalog,
  prototype data and EditMode test; explicit nearest-target method in
  `Assets/Stage2/WavesAI/Runtime/Stage2EnemyGroup.cs` and run binding in
  `Assets/Stage2/RunState/Runtime/Stage2Run.cs`; natural-win PlayMode test.
  Existing catalog's attack fields were added in place through the Defense
  Editor migration; costs, health, prefab GUID and scene are unchanged.
- Verification: EditMode 66/66, PlayMode 29/29, no skips/failures. Full-suite
  expected Integration negative logs distinguished from an Input System
  test-session monitor issue; script reload and cleared Console then yielded
  0 errors/0 warnings on Stage 2 Play entry/exit. `git diff --check` clean.
- Remaining: Integration binds delivered Economy/Defense/AI/Run state into
  the authored scene, then QA verifies both natural outcomes and Windows
  build. Full-run pacing and human observation are still open.

### 2026-09-24 — Waves/AI S2-WAV-02 -> Defense/Integration/QA

- Outcome: preparation timer/Space leads to a bounded prototype wave;
  confirmed throne arrivals apply damage, completed surviving wave wins,
  zero throne health loses, and R restarts terminal runs. Pause freezes run
  time/enemies and cancels preview. Restart clears enemy protection, towers,
  navigation generation, resource inventory/nodes and prior input progress.
- Contracts/files: `Assets/Stage2/RunState/` plain rules, narrow input driver,
  prototype data, EditMode/PlayMode tests and metas; a narrow nested-callback
  reset fix in `Assets/Stage2/WavesAI/Runtime/Stage2EnemyGroup.cs`.
  `gamedesign/features/STAGE2_RUN_STATE.md` records exact Integration wiring.
- Verification: EditMode 65/65, PlayMode 28/28, no skips/failures. Expected
  negative Integration logs identified. After a test-session Input System
  monitor error, script-domain recompile and cleared Console yielded 0
  errors/0 warnings on Stage 2 Play entry/exit, with successful compilation.
  `git diff --check` clean. No Windows build or live-run claim.
- Remaining: Defense must add one basic tower attack; otherwise the integrated
  wave has no source of enemy damage and cannot win. Integration then binds
  Economy/Defense/AI/Run state to the map scene; QA qualifies a Windows build
  and observes both outcomes. P-004 remains open for measured cadence.

### 2026-09-24 — Waves/AI S2-AI-02 -> Run state/Integration/Defense/QA

- Outcome: one continuously moving ground enemy retains explicit current/step
  cells, protects both step endpoints, replans at centers, sieges the current
  tower owner at a stable interval, resumes after destruction and emits one
  throne-damage fact on arrival. A run-owned group preserves shared protection
  across multiple enemies and clears it on death/reset.
- Contracts/files: `Assets/Stage2/WavesAI/` runtime, prototype enemy data/
  prefab, EditMode/PlayMode tests and metas; exact group/Defense/run-state
  binding in `gamedesign/features/DYNAMIC_ENEMY_PATHING.md`. It consumes the
  shared Navigation and Defense `Stage2Tower`; no scene or Stage 1 edits.
- Verification: EditMode 61/61, PlayMode 26/26; no skips/failures. Expected
  negative Integration logs identified; cleared Console and Stage 2 Play
  entry/exit yielded 0 errors/0 warnings/no compile failure. `git diff --check`
  clean. Windows build remains after scene wiring.
- Remaining: S2-WAV-02 schedules waves, applies `ReachedThrone` damage,
  owns preparation/win/loss/restart; Integration connects the group and driver
  on the real map; QA qualifies the integrated loop and build. No timing,
  variation or full-run gate claim is made by this package.

### 2026-09-24 — Defense S2-DEF-01 -> Waves/AI/Integration/Run state/QA

- Outcome: B/click paid placement on authored Stage 2 cells yields one active
  blocker tower, one three-resource spend, one navigation registration and one
  built fact. Cancel/stale preview/phase/actor/cost failures do not buy; tower
  death releases by identity and allows safe replacement.
- Contracts/files: `Assets/Stage2/Defense/` runtime, prototype catalog/prefab,
  EditMode/PlayMode tests and metas; exact wiring and reset order in
  `gamedesign/features/STAGE2_BUILDING.md`. Defense reads shared Economy
  inventory and Navigation; Integration owns the scene. Wave placement requires
  AI's live current-cell/active-step protection callback.
- Verification: EditMode 54/54, PlayMode 24/24; no skips/failures. Expected
  negative Integration validation logs identified; clear Console then Stage 2
  Play entry yielded 0 errors/0 warnings/no compile failure. `git diff --check`
  clean. No Stage 2 scene or Windows build change in this package.
- Remaining: AI supplies continuous movement protection and siege; run state
  sends phase/pause/restart and invalidates Navigation generation; Integration
  binds prefab/catalog/driver on the real map. QA then observes the full loop,
  Windows build and gate evidence. Hero footprint must track any later collider.

### 2026-09-24 — Economy S2-ECO-02 -> Defense/Integration/Run state/QA

- Outcome: map-anchor wood, stone and metal have held, in-range one-unit gather
  with one confirmed fact; per-node depletion and active-time recovery follow
  D-013. The three-resource inventory adds and atomically spends complete
  costs. Pause/terminal freeze recovery; interruption, build mode and restart
  clear progress; restart resets balances and node timers/stock.
- Contracts/files: `Assets/Stage2/Economy/` runtime, prototype data asset,
  EditMode/PlayMode tests and metas; the S2-ECO-02 section of
  `gamedesign/features/STAGE2_ECONOMY.md` records exact Integration bindings.
  Defense reads `Stage2Inventory` and pays through `TrySpend`; Run state sends
  activity/pause/reset; confirmed `Gathered` facts feed objectives/UI later.
  Integration alone attaches the driver and simple markers to the Stage 2
  scene. Stage 1, Map, Navigation, Integration scene and packages unchanged.
- Verification: Unity 6000.6.2f1 compile succeeded after adding the missing
  Input System asmdef reference. Full EditMode 47/47, PlayMode 22/22, no skips.
  Initial compiler errors are resolved. Two existing Integration negative tests
  intentionally logged validation errors; after clearing retained entries,
  the Stage 2 scene entered Play with 0 new errors/0 warnings, then exited.
  Final Console ground truth: 0 errors, 0 warnings, compilationFailed=false.
  Stage 2 scene remains clean/not dirty; `git diff --check` passes.
- Remaining: S2-DEF-01 binds full cost and depleted-source placement exclusion;
  S2-WAV-02 binds phase/pause/restart; S2-INT-02 adds visible markers and
  serialized references; S2-QA-01 qualifies integrated Windows build.
  Checkpoint C measures travel/rebuild timing and economy flows without reward.
  No complete scene loop, build qualification, or Stage 2 gate is claimed.
  S2-DEF-01 required separate human approval at this handoff and has since
  been authorized and completed.

### 2026-09-24 — Defense/Design S2-BLD-01 -> accepted decision / package complete

- Outcome: human accepted P-002 as D-014; S2-BLD-01 is closed. This supersedes
  the earlier pending proposal status below. Building remains available in
  Preparation/Wave, protects actors and active-step endpoints, allows other
  route cells/full walls and spends only on successful complete confirmation.
- Contracts/files: DECISIONS.md records D-014 and removes P-002 from pending;
  STAGE2_BUILDING.md is accepted. DYNAMIC_ENEMY_PATHING.md now specifies
  finishing a protected continuous step before replanning, without enemy
  protection entering tower occupancy. GAME_VISION.md, DEVELOPMENT_PLAN.md,
  STAGE2_VERTICAL_SLICE.md and this register reflect acceptance.
- Verification: human acceptance received; documentation/status/link review
  and `git diff --check` pass. No Assets/Packages/ProjectSettings changes.
  Read-only live Console check: 0 errors/0 warnings, no compilation failure.
  No Unity tests/builds rerun for this documentation-only acceptance;
  retained 42/42 EditMode and 20/20 PlayMode do not verify continuous placement.
- Remaining: S2-DEF-01 implements/tests transactions and actor exclusion;
  S2-AI-02 supplies/tests active-step state, movement and siege; Integration/QA
  qualify composed behavior. No Stage 2 gate is closed. Next by plan:
  S2-ECO-02 (GPT-6 Sol / high), not started; separate human approval required.

### 2026-09-24 — Defense/Design S2-BLD-01 -> human decision pending

- Outcome: prepared P-002 recommendation: build in unpaused Preparation/Wave,
  protect live enemy current cells and both endpoints of started steps, allow
  other future-route cells/full walls, revalidate paid confirmation and cancel
  without spending. Rules remain proposed; S2-BLD-01 is not closed.
- Contracts/files: `gamedesign/features/STAGE2_BUILDING.md` records code evidence,
  alternatives, phase/continuous-movement/payment rules and scenario review;
  DECISIONS.md retains P-002 as pending; DEVELOPMENT_PLAN.md,
  STAGE2_VERTICAL_SLICE.md and this register name the sole active package.
  Existing user/planning changes and accepted S2-ECO-01 are preserved.
- Verification: reviewed Stage 1 build/movement and S2-MAP-02 API/tests;
  discrete consumers do not prove continuous overlap safety. Historical tests
  remain EditMode 42/42, PlayMode 20/20. Read-only live Unity CLI confirms
  Stage2VerticalSlice loaded/active/not dirty (6 roots), Console 0 errors and
  0 warnings, no compilation failure. Documentation/status/link review and
  `git diff --check` pass. No runtime/scene/package/build-wiring changes;
  no fresh test or Windows build is claimed for this decision-only package.
- Remaining: human accepts/revises P-002. Defense S2-DEF-01 implements atomic
  placement/payment and actor checks; Waves/AI S2-AI-02 supplies active-step
  state and tests continuous movement/siege; Integration alone wires the scene;
  QA qualifies composed behavior. Accepted policy must also be reflected in
  the existing DYNAMIC_ENEMY_PATHING.md consumer notes before closing S2-BLD-01.
  No next package starts without separate human approval; Stage 2 stays active.

### 2026-09-24 — Economy/Design S2-ECO-01 -> accepted decision / package complete

- Outcome: human accepted the prepared P-003 policy; D-013 closes S2-ECO-01.
  This entry supersedes the pending status in the earlier proposal handoff
  below. Finite node stock recovers on active game time independently of
  wave completion/rewards; restart restores nodes and zeroes inventory.
- Contracts/files: DECISIONS.md records D-013 and removes P-003 from pending;
  STAGE2_ECONOMY.md contains the accepted lifecycle and rule-level review;
  GAME_VISION.md, DEVELOPMENT_PLAN.md, STAGE2_VERTICAL_SLICE.md and this
  register reflect acceptance and the separate approval boundary.
- Verification: human acceptance received; documentation/status/link review
  and `git diff --check` pass. No Assets/Packages/ProjectSettings changes.
  Documentation only: Unity tests, Console and Windows build are not rerun;
  retained 42/42 EditMode and 20/20 PlayMode results do not test this lifecycle.
- Remaining: S2-ECO-02 implements/tests the rules; Defense/Integration checks
  spatial access, placement and reset; Checkpoint C measures timely recovery
  after lost defenses without optional rewards. No Stage 2 gate is closed.
  Next by plan: S2-BLD-01, not started and awaiting explicit human approval.

### 2026-09-24 — Economy/Design S2-ECO-01 -> human decision pending

- Outcome: prepared P-003 timed node-recovery proposal with depletion,
  pause/terminal, restart and resource-access rules; reviewed zero inventory,
  full depletion and lost-tower cases without optional rewards. Proposal is
  not accepted and S2-ECO-01 remains open.
- Contracts/files: new `gamedesign/features/STAGE2_ECONOMY.md` records evidence,
  alternatives, proposed rules and downstream checks; P-003 pending entry,
  DEVELOPMENT_PLAN.md, STAGE2_VERTICAL_SLICE.md and this register identify the
  sole active package and the human's approval boundary for the next package.
  Existing uncommitted planning changes are preserved.
- Verification: inspected code/scene/map data and baseline reports at
  `87fdb1b` (EditMode 42/42, PlayMode 20/20, clean fresh-Play Console).
  Documentation/link/status and `git diff --check` review pass. Unity is closed;
  no new runtime/test/build/Console pass is claimed. Assets, Packages and
  ProjectSettings are unchanged by this task; existing build is the scene shell.
- Remaining: human accepts or revises P-003 before closing this decision
  package. Runtime lifecycle tests belong to S2-ECO-02, spatial access and
  placement/reset to system integration, and survivable recovery timing to
  Checkpoint C. These are unverified and no Stage 2 gate pass is implied.
  S2-BLD-01 and all other next packages require separate human approval.

### 2026-09-20 — Project setup -> All workstreams

- Outcome: создана базовая документация, scope MVP и правила работы чатов.
- Contract/files: `AGENTS.md`, `gamedesign/`, `production/`, `.cursor/rules/`.
- Verification: структура и ссылки проверяются до закрытия Stage 0.
- Remaining: настроить version control и реализовать Stage 1 graybox spec.

### 2026-09-20 — Design approval -> Foundation

- Outcome: GAME_VISION утверждён; герой не сражается, обучение и progression
  инструментов перенесены после MVP.
- Contract/files: `gamedesign/GAME_VISION.md`, `production/DECISIONS.md`,
  `gamedesign/features/GRAYBOX_CORE_LOOP.md`,
  `gamedesign/features/OBJECTIVES.md`.
- Verification: решения отражены в canonical vision/plan; Git baseline
  `0016bd5` опубликован в `origin/main`.
- Remaining: реализация интеграционного Unity graybox одним Foundation owner.

### 2026-09-20 — Foundation/integration -> Stage 1 complete

- Outcome: реализован полный graybox loop: WASD/fixed isometric camera, конечные
  tree/stone nodes с автоматическими топором/киркой, одна размещаемая башня,
  фиксированный маршрут и одна волна, throne damage, win/lose/restart и one-shot
  objective «добыть 5 дерева» с наградой существующим ресурсом.
- Contract/files: `Assets/Scenes/Stage1Graybox.unity`, `Assets/Stage1/Runtime/`,
  `Assets/Stage1/Prefabs/`, `Assets/Stage1/Tests/`,
  `ProjectSettings/EditorBuildSettings.asset`; integration scene ownership
  остаётся у Foundation до начала следующего согласованного workstream. Для
  discoverability добавлены `Tools > Stage 1 > Open Graybox Scene` и безопасное
  автопереключение только с неизменённой `SampleScene` при первом импорте.
- Verification: EditMode 7/7, PlayMode 3/3 (включая cell-space map bounds,
  Input System, gathering,
  invalid/valid placement, tower win, unopposed throne loss и restart), Windows
  x64 Development Build создан в `Builds/Windows/Not3AGraybox.exe`; headless
  player smoke запущен без attributable exceptions/errors.
- Readability follow-up: поле расширено с 15x11 до 21x17 клеток, внешний ряд
  получил контрастный boundary treatment, а hero/resources ограничены общей
  изометрической областью вместо несовпадающего world-space прямоугольника.
- Scale follow-up: hero, enemy, tree, stone, tower и throne переведены на
  простые grounded rectangular blocks; PlayMode фиксирует относительную высоту,
  физическую блокировку героя и наличие blocking footprints у ресурсов, трона и
  построенной башни. Встроенный UI sprite заменён на проектный solid sprite 1x1
  world unit; PlayMode проверяет точное совпадение нижней грани с ground origin.
- Visual cleanup: runtime-заливки `Resource Zone`/`Build Zone` удалены после
  обнаружения неверного масштаба встроенного UI sprite; PlayMode запрещает их
  повторное появление, enemy entrance использует изометрический marker.
- Remaining: финальный art/audio/UI, tutorial, metal, дополнительные
  towers/enemies/waves, tool progression, save/Steam и production balance вне
  Stage 1; P-002–P-006 не становятся durable решениями из-за prototype values.

### 2026-09-21 — Product/design pivot -> Stage 1 reopened

- Outcome: принято D-009 — башни формируют динамический grid route; длинный
  зигзаг является основной стратегией, а при полном перекрытии враги разрушают
  blocking towers и продолжают путь. Fixed-route build остаётся исторической
  проверенной базой, но больше не закрывает Stage 1.
- Contract/files: `gamedesign/GAME_VISION.md`,
  `gamedesign/features/DYNAMIC_ENEMY_PATHING.md`,
  `gamedesign/features/GRAYBOX_CORE_LOOP.md`, `gamedesign/DEVELOPMENT_PLAN.md`,
  `production/DECISIONS.md`. Foundation сохраняет integration/scene ownership;
  navigation grid/path result принадлежит Foundation/Waves, tower occupancy и
  health — Defense.
- Verification: canonical документы согласованы с maze-building direction;
  implementation verification ещё не выполнялась и прежние 7/7 EditMode,
  3/3 PlayMode не покрывают новый gate.
- Remaining: заменить `FixedRoute` и route-clearance rejection на grid search,
  occupancy/replanning, tower health и blocker siege; затем заново пройти
  EditMode, PlayMode, Windows build и ручной acceptance. Stage 2 не активен.

### 2026-09-21 — Foundation/integration -> Stage 1 human acceptance

- Outcome: D-009/D-011 реализованы на плоском 21x17 representative slice:
  широкий серый corridor и зелёная территория coplanar; башни занимают клетки
  обеих зон, меняют кратчайший путь и могут полностью закрыть corridor. При
  блокировке враг подходит к доступной стороне башни, разрушает её, освобождает
  occupancy и продолжает к трону. Старый `FixedRoute` больше не связан со
  сценой.
- Contract/files: `Assets/Stage1/Runtime/DynamicNavigationGrid.cs`,
  `Stage1Rules.GridPathfinder`, обновлённые `BuildPlacement`, `EnemyController`,
  `TowerController`, `WaveSpawner`, builder, prefabs и
  `Assets/Scenes/Stage1Graybox.unity`. Prototype tower cost — 2 wood/1 stone;
  значения остаются tuning, не durable balance.
- Verification: scene rebuild/compile passed; EditMode 8/8; PlayMode 4/4,
  включая route-cell placement, deterministic longer zigzag, full wall,
  blocker destruction, route resume, tower win, throne loss и restart. Windows
  x64 Development Build собран в `Builds/Windows/Not3AGraybox.exe`, запущен и
  отвечает без attributable startup errors.
- Remaining: человеку проверить читаемость серого corridor, ощущение
  maze-building/siege и prototype balance в Windows build. До этого Stage 1
  gate остаётся pending; production map 100x100 и сравнение 200x200 относятся
  к Stage 2/P-008, финальный арт и остальные out-of-scope системы не начаты.

### 2026-09-21 — Human playtest -> Stage 1 complete

- Outcome: человек подтвердил, что динамическое движение и разрушение полной
  стены работают корректно; Stage 1 gate закрыт. Временный siege-test profile
  удалён: стартовые ресурсы снова 0/0, здоровье врага снова 8.
- Contract/files: gameplay contracts не изменены; canonical status обновлён в
  `gamedesign/DEVELOPMENT_PLAN.md`, `GRAYBOX_CORE_LOOP.md` и
  `DYNAMIC_ENEMY_PATHING.md`.
- Verification: human acceptance получен; после возврата значений повторно
  пройдены EditMode 8/8 и PlayMode 4/4, Windows x64 Development Build успешно
  пересобран, запущен и не показывает attributable startup errors.
- Remaining: Stage 2 не активирован. Перед ним требуется отдельное решение о
  старте работ; P-002–P-006 и P-008 остаются открытыми, production-scale map и
  контент Stage 2 ещё не реализуются.

### 2026-09-21 — Stage 2 planning coordination -> Stage 2 workstreams

- Outcome: Stage 2 разложен на atomic task backlog от map/economy decisions до
  qualified build и external comprehension gate; отмечены dependency graph,
  parallel windows и единоличное владение Stage 2 scene Integration owner'ом.
- Contract/files: `gamedesign/features/STAGE2_VERTICAL_SLICE.md`, Stage 2 status
  в `gamedesign/DEVELOPMENT_PLAN.md` и ownership rules в этом файле. План не
  создаёт task-specific feature specs или Unity assets заранее.
- Verification: Stage 1 baseline `37178e2` перед планированием совпадал с
  `origin/main`, working tree был чист; документы сверены с D-001–D-011,
  P-002–P-006/P-008, Stage 1 contracts и art direction; Unity code/scenes не
  изменялись.
- Remaining: Stage 2 implementation не начата. Первый owner должен явно claim
  ready task и surface; P-002–P-006/P-008 закрываются соответствующими decision
  tasks до зависящей durable реализации.

### 2026-09-21 — Foundation/Map S2-MAP-01 -> Integration/Navigation/Economy

- Outcome: одна data asset описывает первый плоский 100x100 candidate: green
  forest и gray invasion corridor, entrance/throne, walkable/buildable masks,
  wood/stone/metal placement regions/anchors, camera bounds и разные debug
  colors; runtime больше не требует Stage 1 bounds 21x17.
- Contract/files: `Assets/Stage2/Map/` предоставляет read-only
  `Stage2MapDefinition`, deterministic validation/content hash, generated debug
  mesh renderer, Editor asset builder, candidate asset and EditMode tests;
  `gamedesign/features/STAGE2_MAP.md` содержит точные wiring notes.
- Verification: clean Unity 6000.6.2f1 compile without C# warnings/errors;
  S2-MAP-01 EditMode 12/12 and full EditMode 20/20 pass; `git diff --check`
  passes; no `.unity`, ProjectSettings or package change. Windows build deferred
  by task contract to S2-INT-01.
- Remaining: S2-INT-01 wires the asset/debug renderer in the Stage 2 scene;
  S2-MAP-02 consumes masks for scalable navigation/occupancy; S2-ECO-02 places
  nodes in resource regions. P-008 remains open; no 200x200 variant exists.

### 2026-09-22 — Integration S2-INT-01 -> Map/Economy/system owners/Platform-QA

- Outcome: `Assets/Scenes/Stage2VerticalSlice.unity` is a runnable 100x100 scene
  shell with green forest, gray corridor, orange entrance, gold throne, cyan
  non-combat WASD player and fixed orthographic follow camera. Startup validates
  the explicit map reference and reports `S2_INT_VALIDATION_ERROR` with code and
  reason on missing/invalid data; valid startup logs `S2_INT_STARTUP_OK` and the
  deterministic map hash. Runtime never writes authored map data.
- Contract/files: `Assets/Stage2/Integration/Runtime/Stage2SceneBootstrap.cs` on
  `Stage 2 Scene Composition` owns serialized `mapDefinition`, `mapDebugRenderer`,
  `playerMover`, `cameraFollow`, `entranceMarker`, `throneMarker`. The same
  `Stage2Map100x100.asset` is assigned to all map consumers; player uses
  `CellBounds`, camera target/position uses `CameraBounds` in cell space.
  Landmarks resolve from data at startup. Narrow movement/camera components
  reuse Stage 1 `IsoGrid` without a `Stage1GameController` dependency.
- Build/wiring: enabled order is `Stage2VerticalSlice` (0), `Stage1Graybox` (1).
  `Tools > Stage 2 > Integration` exposes Open Vertical Slice Scene, Rebuild
  Scene Shell and Build Windows Development; the Editor builder creates/saves
  the scene through Unity APIs and refuses to overwrite unsaved open scenes.
  Output is `Builds/Windows/Not3AStage2.exe`. Stage 1 scene/assets/tests, Map
  assets and packages are unchanged; automatic build-time settings serialization
  was restored to the original Git contents.
- Verification: full EditMode 20/20 (Stage 1 8/8, Map 12/12); full PlayMode 13/13
  (Stage 1 4/4 including restart, Integration 9/9). Integration covers all WASD
  directions, four extreme bounds, actual camera following, validation
  diagnostics and a transient changed definition with independent camera bounds.
  Clean Editor startup has 0 errors/0 warnings. Windows x64 Development Build
  succeeded with 0 errors; 56-second batchmode smoke with Direct3D 11 enabled
  logged `S2_INT_STARTUP_OK`, responded and produced no errors/exceptions.
  `git diff --check` passes; protected Stage 1/Map/package surfaces have no diff.
  Local evidence: `Artifacts/S2-INT-01/` contains EditMode/PlayMode reports,
  startup/entrance screenshots, Windows build report and `Windows-player.log`.
- QA environment notes: build emitted three package warnings: disabled optional
  Pipeline runtime and stripped `Hidden/Core/DebugOccluder` / `DebugOcclusionTest`
  shaders. Existing Stage 1 tests alone reproduce stale Input System mouse
  monitor errors on the next Editor Play entry with Domain Reload disabled;
  reload the script domain or restart Editor after those tests. Integration
  alone passes 9/9 and the subsequent Play entry stays clean. No tests are
  skipped or replaced with degraded manual movement checks.
- Remaining/handoffs: map composition/serialized binding points -> S2-MAP-02;
  map/player/camera shell -> S2-ECO-02. Defense, Waves/AI, UI/UX and Art/Audio
  owners deliver prefabs/data plus wiring notes to Integration rather than
  editing the scene. Platform/QA receives the Windows build and smoke evidence.
  Navigation scaling, resource nodes, economy, combat/waves/objectives, final
  UI/art/audio and overall Stage 2 acceptance remain unimplemented by this task;
  P-002–P-006/P-008 remain open and no 200x200 variant was created.

### 2026-09-22 — Foundation/Waves-AI S2-MAP-02 -> Defense/AI/Integration/Platform-QA

- Outcome: data-driven navigation on the actual 100x100 candidate produces
  deterministic open/zigzag/blocker routes; removing blockers and resetting a
  run recover safely. Occupancy validates authored masks and reference identity;
  each real transaction changes revision once. Consumers share a reverse search
  field and retain immutable plans until revision/run generation changes.
- Contracts/files: `Assets/Stage2/Navigation/Runtime/Stage2Navigation.cs`, its
  test assemblies/fixtures and the Stage 2 section of `DYNAMIC_ENEMY_PATHING.md`.
  Defense S2-DEF-01 receives `TryOccupy`/`TryRelease` ownership-token lifecycle;
  S2-AI-01/S2-AI-02 receive `NavigationPlan` path/blocker/diagnostic output and
  `ReplanIfStale`. Integration receives explicit shared-instance injection and
  map-reference/reset instructions; no prefab or scene edit is needed here.
- Verification: EditMode 42/42 = Stage 1 8 + Map 12 + Navigation 22; PlayMode
  20/20 = Stage 1 4 + Integration 9 + Navigation 7; no skips. Fresh Editor
  restart and subsequent Stage 2 Play entry: 0 errors/0 warnings, no compile
  failure or recurring stale Input System monitor error. Integration's two
  intentional validation errors were expected by its negative tests. Scene
  remains clean/unsaved; protected surfaces unchanged; `git diff --check` passes.
- Measurements -> S2-MAP-03/Platform-QA: map hash `77A23824A9BF24DA`, Unity
  6000.6.2f1 Editor/Editor PlayMode, i5-12400F, 12 logical CPUs, 32581 MB RAM,
  GTX 1060 6GB, Windows 11 build 26100. Three warmups and 30 samples per case
  and mode. Search median ms (open/zigzag/full wall): EditMode
  1.0835/1.1217/1.0958; PlayMode 1.0741/1.0448/1.1208. Full p95/max and replan/
  cache timings are recorded in the existing pathing spec. Routes: 89/385 steps;
  wall approach: 48 steps to blocker `(0,0)`. 64 moving consumers/128 frames:
  1 search, 64 requests, 8128 retained-plan hits; 24 consumers with shared
  next-cell placement: 2 searches total and all reach the throne. Local raw
  samples, reports, Console evidence and exact diff: `Artifacts/S2-MAP-02/`.
- Remaining: S2-BLD-01 owns placement-phase/current-cell overlap policy;
  S2-DEF-01 owns tower lifecycle/economy binding; S2-AI-02 owns continuous enemy
  movement/siege; Integration alone wires the shared instance into the scene.
  S2-MAP-03 still decides scale/travel time/P-008, S2-QA-01 qualifies the Windows
  build. No universal timing budget, 200x200 variant, combat/spawning/economy or
  Stage 2 completion is implied. Only S2-MAP-02 is closed by this handoff.

### 2026-09-24 — Design replanning -> Stage 2 workstreams

- Outcome: Stage 2 now advances through a connected loop, measured
  scale/pacing, representative presentation and external comprehension gate.
  The MVP content ceiling is no longer interpreted as a Stage 2 task quota.
- Contract/files: DEVELOPMENT_PLAN.md owns the revised stage gates;
  STAGE2_VERTICAL_SLICE.md keeps existing task IDs but changes their readiness
  order and makes S2-INT-02/S2-QA-01 iterative checkpoints. This file owns
  capacity, model/effort and soft token routing. ART_DIRECTION.md now depicts
  dynamic maze paths per D-009. D-001–D-011 and completed task evidence are
  unchanged; earlier planning entries remain historical.
- Verification: documentation cross-check against GAME_VISION.md,
  DECISIONS.md, Stage 2 map/navigation/integration specs and current Git/Unity
  files; diff whitespace/link/status review. No Unity runtime files or scene
  changed, so EditMode/PlayMode/build checks are not rerun for this handoff.
- Remaining: P-002–P-006/P-008 remain open; no new implementation task is
  claimed here. Next owner claims S2-ECO-01 or S2-BLD-01 with exact surface
  before changing gameplay. S2-MAP-03 still needs live-loop travel/performance
  evidence; Stage 2 requires an external player gate.

### 2026-09-24 — Human asset-scope decision -> Art/Stage 2/Post-MVP

- Outcome: D-012 removes detailed Blender/3D model production from the
  Stage 2–6 critical path. MVP art remains simple, readable 2D isometric work;
  a later Blender pass requires its own scope and visual-direction decision.
- Contract/files: GAME_VISION.md and ART_DIRECTION.md state the MVP asset
  boundary; DEVELOPMENT_PLAN.md places a Blender evaluation after MVP;
  STAGE2_VERTICAL_SLICE.md limits S2-ART-01–03 to representative 2D assets;
  DECISIONS.md records D-012; this file lowers the default model/effort for
  MVP art tasks and reserves GPT-6 Astra for scoped post-MVP Blender work.
- Verification: checked D-003/D-005, current Stage 2 gate and model routing;
  git diff --check passes. Documentation-only change: no Unity scene, prefab,
  runtime, package or Console change; Unity tests/build are not rerun.
- Remaining: no Blender asset or new 3D Unity pipeline is authorized for MVP.
  The later use of Blender source assets versus runtime 3D objects is unresolved
  and must be decided before such production starts.

### 2026-09-24 — Solo-developer allocation -> Stage 2–7 owners

- Outcome: the active Stage 2 backlog and future Stage 3–7 gates now have 55
  named packages with an assigned GPT-6 model, reasoning level, dependency,
  observable exit and soft token envelope. Stage 0–1 remain complete; one
  developer keeps one implementation package active at a time.
- Contract/files: `gamedesign/DEVELOPMENT_PLAN.md` owns the per-task and
  per-stage allocations; this register points to that table as the sole
  routing source. A derived, self-contained dark HTML view is saved locally
  in the Git-ignored `reports/` directory.
- Verification: all 24 original Stage 2 task IDs are accounted for, including
  three completed IDs at zero forward allocation; the 55 new IDs are unique
  and stage totals equal the sum of task bands. HTML was generated from the
  canonical plan and visually checked at desktop and narrow widths;
  `git diff --check` passes. No Unity runtime, scene, asset or package changed,
  so Unity tests/build were not rerun for this planning handoff.
- Remaining: track actual Codex usage and rework after three comparable tasks,
  then revise estimates. P-002–P-006/P-008 and the Stage 2 external player
  gate remain open. S7-VISUAL-01 must settle Blender sprite-source versus
  runtime-3D direction before detailed post-MVP asset production.

### 2026-09-24 — Stage dialogue prompts -> Stage 2–7 owners

- Outcome: each Stage 2–7 has one copyable opening prompt for a single Codex
  dialogue, placed ahead of its task list in the local HTML report. The list
  keeps dependency order, task-level model/effort and token envelopes.
- Contract/files: `gamedesign/DEVELOPMENT_PLAN.md` owns the six prompt texts;
  the Git-ignored `reports/development-roadmap.html` is generated from them.
  Model and reasoning settings remain user-selected in the same dialogue when
  the next package calls for a different setting.
- Verification: all six prompt blocks parse, Stage 2–7 switching shows the
  matching prompt and task count, and the desktop layout was visually checked.
  Documentation/report-only change: Unity tests/build are not rerun.
- Remaining: stage entry gates and human scope/release decisions still apply;
  the Stage 2 gameplay packages remain unimplemented by this handoff.
