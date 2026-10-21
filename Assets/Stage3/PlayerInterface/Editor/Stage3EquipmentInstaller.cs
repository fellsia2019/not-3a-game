using System;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3EquipmentInstaller
    {
        public static void Install()
        {
            const string folder = "Assets/Stage3/PlayerInterface/Resources/";
            var items = AssetDatabase.LoadAssetAtPath<Stage2EquipmentCatalog>(folder+"Stage3Equipment.asset");
            if (items == null) { items = ScriptableObject.CreateInstance<Stage2EquipmentCatalog>(); AssetDatabase.CreateAsset(items,folder+"Stage3Equipment.asset"); }
            var forge = AssetDatabase.LoadAssetAtPath<Stage3ForgeCatalog>(folder+"Stage3ForgeBuilding.asset");
            if (forge == null) { forge = ScriptableObject.CreateInstance<Stage3ForgeCatalog>(); AssetDatabase.CreateAsset(forge,folder+"Stage3ForgeBuilding.asset"); }
            if (!items.TryValidate(out var reason) || !forge.TryValidate(out reason)) throw new InvalidOperationException(reason);
            Stage3ForgeArtImporter.BuildAssets(); AssetDatabase.SaveAssets();
            Debug.Log("S3_EQUIPMENT_INSTALLED: trial tuning serialized, D090 caps D/B/S, scroll-only building upgrades, no scene/GUID migration.");
        }
    }
}
