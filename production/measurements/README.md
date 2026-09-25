# Stage 2 Decision Measurements

## Current Multi-Wave Cadence Trial

After the S2-WAV-01 follow-up, run `Stage2CadenceTrial.cs` through `eval_file`
in the clean Stage 2 Editor Play scene at timeScale 1. It writes
`Artifacts/S2-Cadence/live-cadence.json` and `status.json`; await `completed`.
It travels with WASD, gathers three of each resource with E, buys three towers
through the real transaction API and starts the first wave with Space. After
wave 1 it deliberately destroys one paid tower through TakeDamage, then travels
again, gathers one of each from the naturally recovered sources and buys a
replacement. It early-starts the remaining waves, records the outcome, checks
restart, then observes an unopposed automatic-start loss. The controlled damage
isolates funding/recovery; it does not establish natural siege pressure.

The report records per-resource gather totals, paid-build facts, phase/wave
transitions, source stock/timers, preparation remaining, throne health and
outcomes. There are no injected resources, rewards, repairs, source resets or
accelerated time within a run. Known-coordinate movement and fixed build
spacing are not human discovery or decision times. No performance conclusion
should be drawn from this cadence probe. It temporarily enables background
updates and removes its keyboard/callback at completion; stop Play to abort.

Default tower cells match the earlier defense near the throne. The recorded
entrance sensitivity control used `towerStartX = 48`, `towerStepX = -4` in the
probe; it was saved as `entrance-cadence.json` before the default replay.
These coordinates change only paid placements during the replay, not map data.
Run `python production/measurements/summarize_cadence.py` after completion to
derive phase durations and replacement funding time in `summary.json`.
Use a fresh Play session for each placement variant. A replay in a reused
Editor session stopped making normal movement progress and failed before
construction; `reused-session-failed.json` preserves that excluded attempt.
Do not count an incomplete input replay as timing evidence for the intended
defense policy.

## Historical One-Wave Decision Batch

The original `Stage2DecisionMeasurement.cs` below targets the recorded one-wave
baseline. Do not interpret its policies or old outputs as a replay of the new
multi-wave catalog; use the cadence script above for current run evidence.

These scripts are statement bodies for the project's existing Unity CLI
`eval_file` command, not Unity assets or C# assemblies. They produce local
evidence in Git-ignored `Artifacts/S2-DecisionBatch/`. No script saves a scene,
changes an authored catalog or requires the optional Player Pipeline server.

Use the clean, saved `Stage2VerticalSlice` scene. Stop other measurement or
test sessions first. Enter Editor Play at `Time.timeScale = 1`, then run:

```powershell
unity command eval_file --file 'D:/Unity/Games/Not_3A_game/production/measurements/Stage2DecisionMeasurement.cs' --json
```

Wait until `status.json` says `completed`, not merely until the CLI returns.
The live replay temporarily enables `Application.runInBackground` and adds
one isolated keyboard. It records six source-return legs, then starts a fresh
run, travels to the three actual anchors, holds E for three units each,
returns, confirms three paid towers and presses Space. It next observes a
fresh unopposed run through the automatic timer and terminal outcome. Both
runs advance through normal frame updates. It removes its keyboard/callback
and resets the run on completion. Exiting Play aborts the replay; an aborted
or timeout result is not evidence of a completed run.

After completion, run `Stage2NavigationDecisionProbe.cs` using the same
`eval_file` command. This synchronous probe uses isolated services, three
warmups and thirty samples per layout. Its eight cases include open,
three/nine-tower transverse walls, 100-token wall, 297-token zigzag, partial
and complete throne rings, and a three-cell entrance cap. The cap is also
purchased through actual Defense transactions to compare baseline siege with
a temporary doubled-siege profile, suppressing tower fire for isolation.
That fixed-step sensitivity experiment is not approved tuning or a human
observation of the new role.

Then run `Stage2LiveLoadProbe.cs` and wait for `live-load.json` with
`status: completed`. It samples ten seconds idle, ten with 64 real moving
enemy components and ten with the same population and occupancy changes at
1 Hz. It resets the scene's run when finished. Do not run the scripts or
Test Runner concurrently. Finally stop Play and inspect Console.

Frame measurements are sampled Editor frame intervals, not GPU timings or
Windows Player performance claims. Each probe reports skipped frames because
Editor callbacks can sample fewer frames than the Player loop produces.
Unity allocated/reserved memory includes the Editor, probe and project; it
cannot be interpreted as standalone build RAM. Managed Navigation allocation
includes constructing the isolated service and its first query when the
runtime supplies that counter. A zero Mono counter is recorded as unavailable,
not zero allocation. Run `python production/measurements/summarize_stage2.py`
after all three probes complete to derive `summary.json`; incomplete probe
results are rejected. It reports nearest-rank p95 and sample counts explicitly.

The travel policy knows source coordinates and uses eight-direction WASD.
It cannot establish discoverability, player comprehension or subjective pace.
Tower confirmation uses the real public transaction API with fixed one-second
spacing; that spacing is a scripted allowance, not measured human build time.
Record all resource gather/build facts and outcomes before interpreting the
results. A full-map token wall proves neither affordability nor a good maze.

Canonical conclusions belong in `gamedesign/features/STAGE2_MAP.md`,
`STAGE2_RUN_STATE.md`, `STAGE2_ENEMY_ROLES.md`, and the single handoff register
in `production/WORKSTREAMS.md`. This directory is the reproduction method,
not a parallel design plan or source of runtime balance values.
