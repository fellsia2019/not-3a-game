# Feature: Stage 2 Enemy Differences

Status: S2-AI-01 brief and consolidated contract review delivered, 2026-09-24.
Proposed breadth is pending product
acceptance, not implemented or added to the runtime catalogs.
Under D-015 this proposal is parked until a complete basic run reveals a need
for another enemy. It is not on the first-playable critical path.
Owner: Design/Waves; historical planning preset was GPT-6 Astra / high.
Inputs: D-009/D-014, S2-AI-02 movement/siege, S2-WAV-02 run baseline and the
S2-MAP-03/S2-WAV-01 decision drafts.

## Question And Minimum Scope

Does a threat that breaches a fully blocked path faster make the player
prefer an open, extended route over a sealed wall? Can the player predict
that tradeoff from the approaching group and choose a different layout?

Propose two ground profiles for the next slice candidate: the existing
baseline enemy and one siege specialist. No third normal type or final boss
is needed to answer this question. Both retain the same four-neighbor path
rules, throne objective and protected movement endpoints. Neither attacks,
aggros or damages the hero. No flying, teleporting, ranged tower attack,
armor/resistance, debuff, hidden targeting preference or special currency.

The baseline provides the existing reference for open-route travel and
ordinary siege. The specialist changes the time available to recover from
complete blockage through a stronger siege output. Start comparison with
the same travel speed, health and throne damage so the cause is identifiable;
do not combine speed, toughness and siege changes in the first experiment.
Numeric variants belong in owned catalog assets once the human accepts the
brief. A temporary doubled-siege diagnostic is sensitivity evidence only.

## Observable Behavior And Layout Choice

- With an open path, both profiles follow it to the throne and do not attack
  unrelated nearby towers. The specialist is not a wall-seeking enemy.
- With full blockage, both approach the currently reachable blocker and
  attack only while adjacent and still owning a valid blocker plan. The
  specialist breaches sooner; the player can leave a route open or lengthen
  it, rather than rely on indefinite full-wall tanking.
- Destruction releases the exact tower owner once; enemies finish any
  active protected step and replan at the next center. Replacement towers
  cannot receive delayed damage from the destroyed instance's stale plan.
- The specialist must have a distinct, simple silhouette and an explicit
  upcoming-wave cue before deployment. Color alone is insufficient. Art and
  UI own that later delivery; no detailed Blender model is required.

## Contracts To Implement After Acceptance

Waves owns the enemy profile chosen at spawn and the finite wave composition.
Existing live enemies keep the profile they spawned with; switching the next
wave must not mutate their shared ScriptableObject. `Stage2EnemyGroup` needs
an explicit supplied spawn profile when mixed groups become approved; avoid
a service locator, global type lookup or new damage interface.

Defense owns tower health, confirmed damage, removal and identity-safe
occupancy release. AI consumes NavigationPlan's blocker cell/owner and the
existing narrow damage call. Run state owns throne damage and final outcome.
The current integer damage contract is sufficient for this brief. New
slow/control/area-effect contracts belong to S2-DEF-02 only after an actual
tower-choice need is identified and both owners agree on pause/reset behavior.

Integration alone changes scene references. AI hands off profile assets,
inactive prefabs, spawn data, silhouette requirements and exact wiring;
prototype stats are not duplicated in this Markdown file.

## Review And Acceptance Scenarios

Use the same affordable map layout, inventory, tower catalog and wave timing
when comparing profiles. Record arrival time, blocker health/destruction,
throne outcome and tower spend. Keep synthetic full-map walls separate from
player-affordable evidence.

Verify identical open-path plans; no siege with an open route; earlier breach
under full blockage; post-breach continuation; active-step exclusion during
mixed groups; pause freezes both siege timers; restart clears profile/population
state; no attack reaches the hero. A fresh player must identify which threat
punishes complete blockage and name a layout change without coaching.

Reject the added role if its only observable effect is an arbitrary faster
loss, if the affordable map cannot support both choices, or if the player
cannot distinguish it. In that case improve topology/feedback before adding
more enemies. A diagnostic time-to-breach difference alone is not proof of
player comprehension or of a meaningful tower choice.

## Handoff And Limits

A fixed-step diagnostic on the real map purchased three towers at the entrance
for three of each resource and compared the baseline catalog with a transient
copy having doubled siege damage; all other fields stayed equal. Tower fire
was suppressed to isolate siege. The first blocker fell after 5.0 versus 2.5
simulated seconds, and both cases reopened a `ReachesThrone` route. The source
catalog was unchanged. This confirms the current damage/removal/navigation contracts
can express the proposed difference without a new damage abstraction; it
does not validate the doubled value as balance or the role as player-readable.

This brief proposes the smallest next enemy comparison and its contracts.
It does not implement profiles, change current catalogs, promise a new boss,
or close the Stage 2 external-player gate. S2-MAP-03 identifies affordable
compact layouts on the existing map; their player-facing tradeoff still needs
observation. S2-WAV-01 must
define the next cadence candidate; S2-DEF-02 owns corresponding tower choices.
