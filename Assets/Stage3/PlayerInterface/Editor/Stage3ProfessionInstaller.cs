using System;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3ProfessionInstaller
    {
        public static void Install()
        {
            const string path = "Assets/Stage3/PlayerInterface/Resources/Stage3Professions.asset";
            var data = AssetDatabase.LoadAssetAtPath<Stage2ProfessionCatalog>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<Stage2ProfessionCatalog>();
                AssetDatabase.CreateAsset(data, path);
            }
            if (!data.TryValidate(out var reason)) throw new InvalidOperationException(reason);
            AssetDatabase.SaveAssets(); Debug.Log("S3_PROFESSIONS_INSTALLED: separate mastery100, actual material XP,3 talents each; trial tuning serialized.");
        }
    }
}
