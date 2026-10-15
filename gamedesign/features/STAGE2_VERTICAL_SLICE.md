# Feature: First Playable Stage 2

Status: first-playable gate complete, 2026-09-25; later content remains outside
this slice.
This file defines the current playability check. `DEVELOPMENT_PLAN.md` is the
canonical stage state; `production/WORKSTREAMS.md` retains completed package
evidence and ownership. Earlier detailed Stage 2 design/measurement specs
remain historical contracts for implemented mechanics, not an active backlog.

Owner: Integration owns the Stage 2 scene and composition. A system owner
changes only the narrow runtime/data surface of a confirmed defect. Unity
6000.6.2f1, URP, Input System, Windows x64 Development Build.

## Intended Player Experience

On one compact map with entrance and throne near one corner and resource land
extending in three directions, the player finds wood, stone and metal, gathers enough to
buy a defense, chooses between two towers, shapes the route, survives or loses
the finite waves, then restarts. The hero does not fight. Invalid placement
does not spend resources; combat towers can occupy only gray corridor cells,
never green forest cells; full blockage triggers siege rather than a stuck run.
The player can understand the result from a temporary HUD and simple markers.

## Current Evidence

- Stage 1 graybox was accepted 2026-09-21. Stage 2 has the integrated gather,
  build, movement/siege, attack, wave/throne and restart loop on a 100×100 map.
- Resource-marker fixes were confirmed by the player 2026-09-25.
- Rapid and Longshot are wired in the scene. The full automated run purchases
  both roles, wins, and restarts; their cost/reach/fire differences have focused
  checks.
- Automated map/cadence measurements describe a viable candidate but do not
  prove player comprehension, a compact feel or worthwhile tower choice.
- The playable scene now uses the D-016 32×32 corner map. The historical
  100×100 candidate remains available for older navigation measurements.
- EditMode 74/74 and PlayMode 38/38 pass, with an additional 16/16 Integration
  check after the startup-visibility assertion. The final Windows Development
  Build starts on hash `48A2358B440C3C65` without attributable startup error.
  A human finished and restarted a run, then confirmed the revised start
  shows portal/throne and clear resource directions. No blocker was reported.

## Completed Playability Check

1. Verify the current two-tower scene integration. Confirm role
   selection, full-price spending, attacks, different reach and restart.
2. Update the authored map, its builder and affected scene tests to D-016;
   verify gray-only placement, route/siege, resource access and camera framing.
3. Run the Windows build from start through one natural win or loss and restart.
   Observe resource finding, travel, tower placement, wave warning, route
   change and siege. Record only observed blockers or confusion.
4. Adjust map distances, provisional run data or temporary feedback only
   where those observations show a problem. Reuse current contracts; do not
   add a new system for a presentation defect.
5. Give one fresh player the corrected build without instructions. Record
   whether they can find resources, build, understand the wave and finish the
   run. Repair a blocking issue and repeat only the affected check.

Acceptance reached: one complete, understandable, repeatable short run; relevant
EditMode/PlayMode checks pass, Windows launch works and there are no new
attributable Console/Player errors. If a check cannot run, record the reason.
The resulting map size and cadence are provisional until this observation.

Not required for this gate: secondary objective, specialist enemy, boss,
additional tower/resource, long run duration target, save system, final HUD,
sound, VFX or finished art. The current graybox is a valid test surface.
Do not silently change the accepted active-wave building, resource renewal
or maze/siege rules; if they fail in play, record the failure first.

After this gate, test one small asset set to choose runtime 3D versus rendered
sprites for the soft cartoon target. Only then schedule production art/UI/audio
and later content. Keep game rules and values independent of the presentation.
