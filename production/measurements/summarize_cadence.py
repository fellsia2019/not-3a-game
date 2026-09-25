"""Summarize completed cadence replays without treating them as human observations."""
import json
from pathlib import Path


def summarize(path):
    report = json.loads(path.read_text(encoding="utf-8-sig"))
    if report["status"] != "completed" or not report["replacementComplete"]:
        raise ValueError(f"Incomplete cadence probe: {path}")
    result = {"source": str(path), "mapHash": report["mapHash"], "runs": []}
    for run in report["runs"]:
        policy = run["policy"]
        events = [e for e in report["events"] if e.get("state", {}).get("policy") == policy]
        phases = [e["state"] for e in events if e["fact"] == "phase"]
        durations = []
        for current, following in zip(phases, phases[1:]):
            if current["phase"] in ("Preparation", "Wave"):
                durations.append({"phase": current["phase"], "wave": current["wave"],
                                  "seconds": following["runSeconds"] - current["runSeconds"]})
        entry = {"policy": policy, "outcome": run["phase"], "seconds": run["runSeconds"],
                 "throne": run["throne"], "towers": run["towers"],
                 "gathered": run["gathered"], "remaining": [run[k] for k in ("wood", "stone", "metal")],
                 "paidBuilds": run["buildCount"], "phaseDurations": durations}
        loss = next((e["state"] for e in events if e["fact"] == "controlled-paid-tower-loss"), None)
        replacement = next((e["state"] for e in events if e["fact"] == "replacement-complete"), None)
        if loss and replacement:
            entry["controlledReplacement"] = {
                "seconds": replacement["t"] - loss["t"],
                "preparationRemaining": replacement["remaining"],
                "phase": replacement["phase"], "wave": replacement["wave"],
                "throne": replacement["throne"], "nodes": replacement["nodes"]}
        result["runs"].append(entry)
    result["limits"] = report["limits"]
    return result


if __name__ == "__main__":
    folder = Path("Artifacts/S2-Cadence")
    status = json.loads((folder / "status.json").read_text(encoding="utf-8-sig"))
    if status["status"] != "completed":
        raise ValueError("The current cadence replay has not completed; old reports are not current evidence.")
    paths = [folder / "live-cadence.json"]
    entrance = folder / "entrance-cadence.json"
    if entrance.exists():
        paths.append(entrance)
    summary = {"trials": [summarize(path) for path in paths]}
    (folder / "summary.json").write_text(json.dumps(summary, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(summary, indent=2))
