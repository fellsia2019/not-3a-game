using System;
using Not3A.Stage2.Economy;
using Not3A.Stage2.RunState;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3ThroneUpgradeInstaller
    {
        public static void Install()
        {
            const string path = "Assets/Stage3/CombatSlice/HordeRun.asset";
            var data = AssetDatabase.LoadAssetAtPath<Stage2RunCatalog>(path);
            if (data == null) throw new InvalidOperationException("Existing Stage3 HordeRun catalog is required.");
            data.ConfigureThroneUpgrades(new[] {
                new Stage2ResourceCost(20, 15, 5, 25), new Stage2ResourceCost(40, 30, 10, 60), new Stage2ResourceCost(80, 60, 20, 140) });
            if (!data.TryValidate(out var reason)) throw new InvalidOperationException(reason);
            EditorUtility.SetDirty(data); AssetDatabase.SaveAssets();
            Debug.Log("S3_THRONE_UPGRADES_INSTALLED:3 paid+5HP steps, original base HP/waves/clock kept; trial prices.");
        }
    }
}
