using System;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3HeroVitalsInstaller
    {
        public static void Install()
        {
            var data=Resources.Load<Stage2HeroProgressionCatalog>("Stage3HeroProgression");
            if(data==null||!data.TryValidate(out var reason))throw new InvalidOperationException("Invalid hero vitals/progression data.");
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Stage3Rapid3DGamePilot.unity",OpenSceneMode.Single);
            var mover=UnityEngine.Object.FindAnyObjectByType<Stage2PlayerMover>();
            if(mover==null)throw new InvalidOperationException("Saved hero mover missing.");
            mover.ConfigurePlanarEdgeClearance(1.5f);EditorUtility.SetDirty(mover);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("S3_HERO_VITALS_INSTALLED: existing scene hero edge clearance1.5, vitals/rank data validated; art/camera angle unchanged.");
        }
    }
}
