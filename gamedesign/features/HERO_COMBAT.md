# Feature: Hero Bow And Horde Prototype

## D114 current correction — stationary-only automatic bow

Successful ground movement in the current frame prevents a NEW hero shot in
all directions, for the shared portal/world cadence. Cooldown still runs while
walking and already launched arrows keep flight/damage. Standing resumes normal
automatic target selection. Teleportation is not continuous ground walking.
No moving-attack skill or additional attack is implemented. Walking animation
faces its movement instead of holding the old shot pose.


D111 implementation update,2026-10-21: separate orc/ogre exploration spots target and melee-damage the hero through existing HeroRecovery; never join portal group/throne/wave counters. Shared automatic bow cadence chooses nearest live portal/world target; tied distance prefers portal. Camp actors chase locally with telegraphed melee and leash home without healing/refilling. Only new accepted order refills its spot. World per-kill XP and board contract XP are separate; trial values in Stage3Guild asset. Retry preserves current camp state. Local qualification:89distinct PlayMode PASS; native and source evidence in art/source/guild_2026-10-21_verification.json.


## D-109 planned world encounters,2026-10-21

Owner confirms distinct camps/ruins as open-world encounter spots, with separate
orc and ogre locations for the current enemy appearances. Goblins and other
undead are future examples; no new current types/counts or mixed camp is implied.
These future world enemies fight the hero, unlike throne-targeting portal creeps.
D104 already supplies hero damage/death/recovery APIs; no new enemy AI, spawn,
leash/respawn, loot or placement is implemented by this concept request.
OBJECTIVES.md owns the active temple-NPC quest-window concept and open quest rules.

## D-106 continuously spawning nights,2026-10-20

Stage3 night now spawns for180active seconds after a600second day. Authored
count no longer stops it; dawn plus resolved live threats ends the tier. First
night must still be manageable with the starting bow; later dense tiers require
offensive towers. Existing profile intervals/HP remain trial data; old finite
paid-defense policy now passes the full-night check for tiers2/3, without tower-
stat relaxation. These are declared automated combat fixtures, not owner balance
approval or a sustained native load playtest. Current cycle contract: PORTAL_CONTROL.md.

## D-105 initial-horde tuning,2026-10-20

Owner delegated a modest first-night buff: more enemies and shorter spacing in
HordeRun, mirrored in the existing builder. Later night profiles and bow stats
stay current. Trial values live in runtime data; the first-wave bow-only and
later tower-dependent checks qualify the changed balance. Day gates repeat
last cleared difficulty indefinitely, separately from night count; see
PORTAL_CONTROL.md.

## New progression target — D-074, not implemented

Герой до 100 уровня развивается для небольших боёв открытого мира. Магия
имеет умеренный одиночный урон и минимум сплеша; башни остаются необходимы
против сопоставимой плотной орды. Скорость атаки, предметы и постоянные руны
проверяются вместе, не только радиус спеллов. HERO_PROGRESSION.md заменяет
прошлый предложенный набор массовой магии. Здоровье/гибель героя и агро
противников мира ещё требуют реализации и отдельного решения о возвращении.
Текущий лук, его targeting/cadence и проверенный D-064 ниже не изменены.

## Implemented bow baseline

Status: D-064 implemented and verified as a provisional slice, 2026-10-18.
Owner: current Stage 3 chat, gameplay integration; Astra owns presentation.
Dependencies: shared enemy group/shots, run activity/reset, existing gathering,
construction, pause, debug HUD and approved tower/enemy art.

## Accepted behavior

The hero automatically shoots the nearest living enemy within bow range,
including while moving. Existing spawn order resolves exact-distance ties.
The bow uses one finite firing interval and the existing tracked projectile /
confirmed damage/kill reward path. No new ammunition, currency, weapon tiers,
manual aim control or equipment UI. Flight and cooldown use active game time.
Construction, held E interactions, debug menu, pause and terminal outcomes
suppress firing. Full restart clears bow cooldown and pending attacks through
the normal shared reset. D-065 replaces the temporary cyan marker with the
approved versatile hero C, keeping starter-bow artwork separate from the body.

Portal enemies retain their throne/blocker targets and gray-road restriction.
Open-world enemies, hero health/death, ruins and loot are not added now.
Respawning at the throne after hero death is only a follow-up recommendation.

The first wave must be clearable with the hero alone and no starting towers.
Later dense hordes must exceed solo bow capacity but admit victory through
paid defense construction. Do not make individual enemies arbitrarily immune
to bow damage or force a scripted defeat. No tutorial locks or research gating
of the existing towers are introduced. Day/night, blueprints and training
quests remain follow-up design; the finite prototype is not the full product.

## Data and integration

2026-10-18 visual correction: Stage3HeroArt optionally supplies four separate
planted idle sprites and idle grip offsets. Stage3HeroAvatar breathes subtly
around the sole pivot, follows distance-driven walking when moving and freezes
on pause. Corrected side bows replace the old triangular-string silhouette;
weapon/arrow grip registration remains separate from the body. This is not a
new string-draw attack animation. BuildHeroMotion imports only the new hero
art into the existing HeroArt asset, preserving scene references and other art.
Focused rendered checks pass 2/2; details/limits:
art/source/hero_motion_2026-10-18_verification.json.

Stage2HeroCombat is plain cadence logic; Stage2HeroCombatDriver binds it to
scene-owned hero/run/interactions and Stage2EnemyGroup.TryHitNearest.
Stage2HeroCombatCatalog owns tuning. The Stage 3 composition opts in with
heroCombatCatalog; old scenes without it retain their prior behavior.

Tools > Stage 3 > Hero Combat > Build And Wire Slice creates HeroBow.asset and
HordeRun.asset in Assets/Stage3/CombatSlice/, builds the barricade assets and
wires the existing Stage3Rapid3DGamePilot scene through Unity APIs. It preserves
later data tuning. Historical Stage 2 run data stays unchanged.
The implemented D-062 snapshot includes bow cooldown and clears failed-future
attacks; new progression state will extend that same boundary.

## Observable acceptance

- Nearest/range/cadence/flight checks use actual enemies and confirmed kills.
- Pause, construction, E, debug, terminal and repeated restart suppress or clear
  attacks correctly; tower/hero kills share one existing reward contract.
- Actual first-wave bow-only policy wins without materials or towers.
- An unrestricted-target-access solo policy loses the later horde; a bounded
  paid-defense policy wins later hordes. These are automated policies, not proof
  of universal player balance. Report material budgets and population evidence.
- Relevant regressions, real frames, Windows build/run and no new attributable
  console errors. Final evidence/status is recorded in WORKSTREAMS.md.

## Verified result

Actual first-wave bow-only policy clears with no material grants or towers.
Unrestricted access to targets still loses the second horde, including with six
paid passive walls. Two Rapid, two Longshot and six walls clear both later hordes
without throne damage. This policy receives a declared QA material allowance of
20 wood / 14 stone after the first wave and spends the ordinary kill-reward gold;
it does not prove gathering/travel pacing or every defense layout is balanced.
The existing paid-defense regression also gathers actual prices and wins.

EditMode 19/19, final focused PlayMode 9/9 and corrected visual check 1/1 pass.
The broad regression's sole failing defense policy was updated and passed in
the focused run; the other 94 cases passed in the broad run. Windows x64 build
and live startup smoke passed. Real D3D11 camera frames show the hero-origin
arrow and dense population. Current movement produces overlapping single-file
enemies, not a wide formation. The historical D-064 frames use the cyan marker;
D-065 owns its approved replacement and continuing night-horde cycle.
Evidence/limits: art/source/hero_combat_slice_2026-10-18_verification.json.
