using System;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3MerchantInstaller
    {
        public static void Install()
        {
            const string path="Assets/Stage3/PlayerInterface/Resources/Stage3Merchant.asset";
            var data=AssetDatabase.LoadAssetAtPath<Stage2MerchantCatalog>(path);
            if(data==null){data=ScriptableObject.CreateInstance<Stage2MerchantCatalog>();AssetDatabase.CreateAsset(data,path);}
            if(!data.TryValidate(out var reason))throw new InvalidOperationException(reason);
            AssetDatabase.SaveAssets();Debug.Log("S3_MERCHANT_INSTALLED:8 finite F/LVL1 items, gold-only trade,50% sale; trial prices serialized.");
        }
    }
}
