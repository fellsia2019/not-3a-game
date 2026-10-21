using Not3A.Stage3.VisualPilot;
using UnityEditor;
using UnityEngine;
namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3CityTravelInstaller
    {
        public static void Install(){Stage3CityArtImporter.BuildAssets();SaveConfiguration();}
        public static void SaveConfiguration()
        {
            var data=AssetDatabase.LoadAssetAtPath<Stage3WorldTravelData>("Assets/Stage3/WorldTravel/WorldTravelData.asset");
            if(data==null)throw new System.InvalidOperationException("World travel data missing.");
            // Serialize additive city/optional-node fields without replacing existing art or point offsets.
            EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
            Debug.Log("S3_CITY_TRAVEL_CONFIGURATION_READY origin="+data.cityOrigin+" extraPoints="+(data.additionalPoints?.Length??0));
        }
    }
}
