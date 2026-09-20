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
- The core is gather -> invest -> defend -> recover/upgrade -> repeat.
- The throne is the loss-critical objective. Resources are wood, stone, metal.
- The hero has no combat role; they gather resources and construct defenses.
- Tutorial flow and tool-tier progression are post-MVP.
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
