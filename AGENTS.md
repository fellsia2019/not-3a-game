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
- D-062 approves defeat rewind to the snapshot immediately before the failed
  wave starts, plus its full preparation timer, for manual and timed starts.
  Runtime retry restores that checkpoint; continuing days return to its dawn
  without skipped-time income. Full reset is a separate development operation.
- D-064 adds a nearest-target automatic hero bow. Portal enemies still target
  the throne/blockers; future open-world enemies target the hero. Hero health
  and death are outside the current bow/barricade/horde slice.
- Current D-065 runtime adds twenty-minute active days and night hordes until fully cleared.
  Days continue after the initial sequence. A paid green-ground dwelling skips
  only the remaining day via local E, with no skipped-time production.
  Hero C is versatile; the bow is initial equipment, not a fixed class.
- D-074 target supersedes both runtime time skips: no dwelling day skip and no
  early dawn after victory. Required night hordes grow; daytime gates repeatedly
  farm the last cleared tier. This revised cycle is design only; see
  gamedesign/features/PORTAL_CONTROL.md and its checkpoint prerequisites.
- Tutorial flow remains post-MVP. D-074 confirms hero cap 100 and low-splash
  exploration combat; dense matched-tier hordes require towers. Proposed
  ability/passive/profession counts are in HERO_PROGRESSION.md, not shipped.
  Equipment levels 1–100/ranks F–SSS and separate hero/tower runes are the
  target; rune counts, stacking and gathering category remain proposals in
  EQUIPMENT_AND_RUNES.md. Do not treat design tables as runtime data.
- D-075 target ends the campaign after three distinct strong bosses. Boss
  venue is undecided. TD is the main combat XP/gold source; world camps offer
  less XP and better loot chances. Bosses/world combat are not implemented.
- D-075 equipment has six combat slots (head/chest/arms/legs/weapon/shield),
  levels 1–100 and eight qualities F/E/D/C/B/A/S/SSS: no SS or current SSSS.
  Forge crafting/reforging includes pickaxes/axes; production buildings and
  workshop also advance. Throne has exactly three max-HP upgrades of +5.
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
- Factorio-style automation/logistics and persistent roguelite progression are
  post-MVP until the basic loop is proven.
- Follow `gamedesign/GAME_VISION.md` for the current scope and non-goals. Do not
  add content counts, currencies, controls, UI, lore, progression, or tuning
  that have not been approved or explicitly delegated.

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
