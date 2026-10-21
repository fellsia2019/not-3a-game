using System;
using Not3A.Stage2.Economy;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3ProductionUpgradeInstaller
    {
        public static void Install()
        {
            const string path = "Assets/Stage3/ResourceDistricts/Production/ProductionCatalog.asset";
            var data = AssetDatabase.LoadAssetAtPath<Stage2ProductionCatalog>(path);
            if (data == null) throw new InvalidOperationException("Existing Stage3 production catalog is required.");
            var gold = new[] { 10, 25, 60, 140 };
            foreach (var entry in data.Entries)
            {
                entry.upgrades = new Stage2ProductionUpgrade[4];
                for (var i = 0; i < 4; i++)
                {
                    var factor = 1 << (i + 1);
                    entry.upgrades[i] = new Stage2ProductionUpgrade {
                        cost = new Stage2ResourceCost(checked(entry.WoodCost * factor), checked(entry.StoneCost * factor),
                            checked(entry.MetalCost * factor), gold[i]), outputAmount = i + 2
                    };
                }
            }
            if (!data.TryValidate(out var reason)) throw new InvalidOperationException(reason);
            EditorUtility.SetDirty(data); AssetDatabase.SaveAssets();
            Debug.Log("S3_PRODUCTION_UPGRADES_INSTALLED:5 stages1..5 output, original intervals/art/build costs; trial paid upgrades, no research gate.");
        }
    }
}
