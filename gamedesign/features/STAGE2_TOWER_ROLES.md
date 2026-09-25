# Feature: Stage 2 Tower Choice

Status: S2-DEF-02 Defense delivery complete, 2026-09-25; scene integration
and player-choice observation remain open.
Owner: Defense. Integration exclusively owns the Stage 2 scene.
Preset: GPT-6 Sol / high, selected by the human.
Inputs: S2-DEF-01 and DEF-01-FIX, D-009/D-014, S2-AI-01 enemy-role brief.

## Question

Can a player choose between guarding a compact choke and covering a longer
open route, while both towers still shape the enemy path as one-cell blockers?
The Stage 2 experiment uses two single-target roles. It does not add area,
slow, resistance, projectile or new enemy-damage contracts.

## Prototype Contract

- Rapid is the existing inexpensive, short-range, frequent-shot tower.
- Longshot reaches farther and deals a heavier hit, but fires less often and
  requires an additional metal unit. Stats and prices
  are authored only in the two Defense catalog assets.
- Both towers use the existing nearest-living-enemy callback, occupancy,
  siege health, pause and restart rules. Switching the selected recipe during
  preview does not spend or reserve resources. Confirmation rechecks the
  selected recipe's full price before creating and activating its own prefab.
- B opens/cancels placement. Number keys 1/2 select Rapid/Longshot; Escape or
  right click cancels. The role silhouette changes with selection. UI handoff
  must show the selected name, exact price and build validity before a player
  comprehension test. These prototype inputs are subject to that test.

## Acceptance And Handoff

Focused checks: invalid or unaffordable role selection cannot spend or occupy;
the paid tower has the selected prefab and price; Rapid cannot hit a comparison
target beyond its radius while Longshot can; pause, destruction and reset keep
the pre-existing contracts. Integration must pass the Longshot catalog as the
second role to Stage2BuildDriver.Configure, show the selection/price in the
development readout, and then run an affordable live-map comparison. No scene
file is changed by Defense.

The authored numbers are diagnostic candidates, not final balance. A fresh
player must be able to name a layout where each role helps. If the difference
is invisible or only makes one tower strictly better, adjust the two catalog
assets or the feedback before expanding tower breadth.

## Verification, 2026-09-25

The Defense EditMode assembly passed 10/10 and its PlayMode assembly passed
3/3 on Unity 6000.6.2f1 in an isolated project copy with the active Editor's
import cache. The cold copy compiled but did not resolve existing ScriptableObject
assets; those initial asset-null failures were an environment artifact and were
not counted as gameplay failures. The final focused logs have no C# compiler
errors or new exceptions. The main Stage 2 scene, Windows build and full
regression have not yet been changed or checked for this delivery.

The baseline catalog keeps its GUID and prefab reference. New data:
`Assets/Stage2/Defense/Data/Stage2DefenseLongshot.asset` and inactive
`Assets/Stage2/Defense/Prefabs/Stage2LongshotTower.prefab`. The latter uses the
project's existing graybox sprite with a narrower, taller silhouette; no
external art is included. Integration passes `new[] { longshotCatalog }` to
the new `Stage2BuildDriver.Configure` overload. The old overload remains valid
for existing tests and scenes; it exposes only Rapid until Integration updates
its composition. No data migration or map change is needed.
