using System;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3BlueprintInstaller
    {
        public static void Install()
        {
            Stage3BlueprintArtImporter.BuildAssets();
            const string path="Assets/Stage3/PlayerInterface/Resources/Stage3Blueprints.asset";
            var data=AssetDatabase.LoadAssetAtPath<Stage2BlueprintCatalog>(path);
            if(data==null){data=ScriptableObject.CreateInstance<Stage2BlueprintCatalog>();AssetDatabase.CreateAsset(data,path);}
            if(!data.TryValidate(out var reason))throw new InvalidOperationException(reason);
            AssetDatabase.SaveAssets();Debug.Log("S3_BLUEPRINTS_INSTALLED:4 accepted documents, trial gold prices serialized, target building stages only.");
        }
    }
}
