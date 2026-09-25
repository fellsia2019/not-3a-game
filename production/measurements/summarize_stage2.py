"""Derive a compact report from the completed Unity probes; standard library only."""
import json
import math
import statistics
from pathlib import Path

root = Path(__file__).resolve().parents[2]
folder = root / "Artifacts" / "S2-DecisionBatch"
live = json.loads((folder / "live-measurements.json").read_text(encoding="utf-8-sig"))
load = json.loads((folder / "live-load.json").read_text(encoding="utf-8-sig"))
nav = json.loads((folder / "navigation-role-measurements.json").read_text(encoding="utf-8-sig"))
if live["status"] != "completed" or load["status"] != "completed":
    raise SystemExit("Incomplete probe: no completed-run conclusions are permitted.")


def stats(values):
    values = sorted(values)
    return {"n": len(values), "median": statistics.median(values),
            "p95": values[math.ceil(0.95 * len(values)) - 1], "max": max(values)}


navigation = []
for name in dict.fromkeys(row["scenario"] for row in nav["samples"]):
    rows = [row for row in nav["samples"] if row["scenario"] == name]
    navigation.append({"scenario": name, "occupied": rows[0]["occupied"],
                       "steps": rows[0]["steps"], "status": rows[0]["status"],
                       "searchMs": stats([r["searchMs"] for r in rows]),
                       "replanMs": stats([r["replanMs"] for r in rows]),
                       "managedAllocation": "unavailable: Mono counter returned zero" if
                       not all(r.get("allocationCounterAvailable", False) for r in rows)
                       else stats([r["managedAllocatedBytes"] for r in rows])})
frame_stats = []
for name in dict.fromkeys(row["scenario"] for row in load["frames"]):
    # Discard the first second of each ten-second scenario, including spawn stalls.
    rows = [r for r in load["frames"] if r["scenario"] == name and r["seconds"] % 10 >= 1]
    memories = [r["allocatedBytes"] / (1024 ** 2) for r in load["memory"] if r["scenario"] == name]
    frame_stats.append({"scenario": name, "sampledFrameMs": stats([r["ms"] for r in rows]),
                        "editorAllocatedMiB": stats(memories), "warmupSeconds": 1})
gathered = {resource: sum(e["amount"] for e in live["events"] if
                         e["fact"] == "gather" and e["resource"] == resource)
            for resource in ["Wood", "Stone", "Metal"]}
wave = next(e for e in live["events"] if e["stage"] == "timed-unopposed" and
            e["fact"] == "phase" and e["phase"] == "Wave")
lost = next(e for e in live["events"] if e["stage"] == "timed-unopposed" and
            e["fact"] == "phase" and e["phase"] == "Lost")
unopposed_wave_seconds = lost["t"] - wave["t"]
summary = {"utc": live["utc"], "mapHash": live["mapHash"],
           "hardware": {k: live[k] for k in ["cpu", "gpu", "ramMB", "os"]},
           "replaysPerPolicy": 1, "travelAndActions": live["segments"],
           "runs": live["runs"], "gatheredInPreparation": gathered,
           "paidTowerCount": sum(e["fact"] == "build" for e in live["events"]),
           "unopposedWaveSeconds": unopposed_wave_seconds,
           "unopposedPreparationSeconds": live["runs"][1]["seconds"] - unopposed_wave_seconds,
           "navigation": navigation, "load": frame_stats,
           "loadFullSearchDelta": load["fullSearchDelta"], "loadRequestDelta": load["requestDelta"],
           "loadRetainedPlanDelta": load["retainedPlanDelta"],
           "skippedFrames": {"live": live["skippedFrames"], "load": load["skipped"]},
           "roles": nav["roles"],
           "limits": [live["limits"], load["mode"],
                      "Per-policy n=1; no fresh player observation or statistical claim about human timing.",
                      "A temporary doubled-siege variant is sensitivity analysis, not approved runtime tuning."]}
(folder / "summary.json").write_text(json.dumps(summary, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
print(json.dumps(summary, indent=2, ensure_ascii=False))
