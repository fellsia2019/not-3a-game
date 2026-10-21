using System;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3GuildInstaller
    {
        public static void Install()
        {
            Stage3GuildArtImporter.BuildAssets();
            Stage3GuildEnemyArtImporter.BuildAssets();
            const string path="Assets/Stage3/PlayerInterface/Resources/Stage3Guild.asset";
            var data=AssetDatabase.LoadAssetAtPath<Stage2GuildCatalog>(path);
            if(data==null){data=ScriptableObject.CreateInstance<Stage2GuildCatalog>();AssetDatabase.CreateAsset(data,path);}
            if(!data.TryValidate(out var reason))throw new InvalidOperationException(reason);
            AssetDatabase.SaveAssets();Debug.Log("S3_GUILD_INSTALLED: F–S orders and six trials; provisional balance serialized.");
        }
    }
}
