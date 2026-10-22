#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3HeroLoadoutInstaller
    {
        [MenuItem("Tools/Stage 3/Hero Loadout/Install Approved Expansion")]
        public static void Install()
        {
            const string path="Assets/Stage3/PlayerInterface/Resources/Stage3HeroSkills.asset";
            AssetDatabase.Refresh();
            var catalog=AssetDatabase.LoadAssetAtPath<Stage2HeroSkillCatalog>(path);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<Stage2HeroSkillCatalog>();AssetDatabase.CreateAsset(catalog,path);}
            catalog.MigrateAdventurerDefaults();EditorUtility.SetDirty(catalog);
            if(!catalog.TryValidate(out var reason))throw new InvalidOperationException(reason);
            Stage3ForgeArtImporter.BuildAssets();
            Stage3HeroSkillArtImporter.Import();
            EditorUtility.SetDirty(Resources.Load<Stage2MerchantCatalog>("Stage3Merchant"));
            EditorUtility.SetDirty(Resources.Load<Stage2EquipmentCatalog>("Stage3Equipment"));
            AssetDatabase.SaveAssets();
            Debug.Log("S3_HERO_LOADOUT_INSTALLED: data, original item/skill/potion art, existing scene binding.");
        }
        private static TestRunnerApi runner;
        public static void BuildMacReview()
        {CleanQualificationSceneAssets();Install();Stage3PlayerInterfaceBuilder.BuildMacReviewPlayer();}
        public static void CleanQualificationSceneAssets()
        {
            // TestRunner's synchronous batch exit can leave its own generated init scene.
            // Only remove scenes proven by this qualification's logs, through AssetDatabase.
            const string directory="Artifacts/HERO-LOADOUT-2026-10-22";
            if(!Directory.Exists(directory))return;
            var paths=new HashSet<string>();
            foreach(var log in Directory.GetFiles(directory,"*.log"))
                foreach(Match match in Regex.Matches(File.ReadAllText(log),@"Assets/InitTestScene[0-9a-f-]+\.unity"))
                    paths.Add(match.Value);
            foreach(var path in paths)
            {
                if(Array.Exists(EditorBuildSettings.scenes,scene=>scene.path==path))continue;
                if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)!=null)AssetDatabase.DeleteAsset(path);
            }
        }
        public static void RunChecks()
        {
            Directory.CreateDirectory("Artifacts/HERO-LOADOUT-2026-10-22");
            runner=ScriptableObject.CreateInstance<TestRunnerApi>();
            runner.RegisterCallbacks(new Results());
            var requested=Environment.GetEnvironmentVariable("NOT3A_HERO_LOADOUT_TEST_FILTER");
            runner.Execute(new ExecutionSettings(new Filter{
                testMode=TestMode.PlayMode,
                testNames=string.IsNullOrWhiteSpace(requested)?new[]{
                    "Not3A.Stage3.VisualPilot.Tests.Stage3HeroLoadoutRulesTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3HeroLoadoutCombatTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3HeroLoadoutPresentationTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3LoadoutEquipmentPresentationTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3HeroProgressionTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3HeroProgressionRulesTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3HudTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3HeroRecoveryRulesTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3EquipmentRulesTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3GuildRulesTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3EquipmentForgeTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3MerchantRulesTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3MerchantTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3BlueprintRulesTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3BlueprintResearchTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3HeroRecoveryTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3GuildTests",
                    "Not3A.Stage3.VisualPilot.Tests.Stage3FieldFeedbackTests"
                }:requested.Split(';')
            }));
        }
        private sealed class Results:ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun){}
            public void TestStarted(ITestAdaptor test){}
            public void TestFinished(ITestResultAdaptor result){}
            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result,"Artifacts/HERO-LOADOUT-2026-10-22/playmode.xml");
                File.WriteAllText("Artifacts/HERO-LOADOUT-2026-10-22/tests-status.txt",$"{result.ResultState}:passed={result.PassCount};failed={result.FailCount};skipped={result.SkipCount}");
                EditorApplication.Exit(result.FailCount==0?0:1);
            }
        }
    }
}
#endif
