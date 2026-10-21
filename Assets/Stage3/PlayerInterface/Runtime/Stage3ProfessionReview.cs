using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit development-only fixture. Normal starts never award mastery or change resolution.</summary>
    public static class Stage3ProfessionReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            Directory.CreateDirectory(directory);
            var game = owner.GetComponent<Stage2GameComposition>();
            var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var hud = owner.GetComponent<Stage3FantasyHud>();
            var deadline = Time.realtimeSinceStartup + 15;
            while (owner.GetComponent<Stage2ProfessionDriver>() == null && Time.realtimeSinceStartup < deadline) yield return null;
            var driver = owner.GetComponent<Stage2ProfessionDriver>();
            if (driver == null) throw new InvalidOperationException("Profession review binding did not initialize.");
            game.RunDriver.enabled = false; game.HeroCombat.AutomaticTick = false; game.RunDriver.ResetRun();
            var xp = 40;
            for (var level = 1; level < 20; level++) xp += driver.Catalog.RequiredXp(level);
            // Declared QA awards for approved-layout review, never a normal-player balance change.
            driver.Rules.AwardMaterial(MapResourceKind.Stone, xp);
            driver.Rules.AwardMaterial(MapResourceKind.Wood, 125);
            driver.Rules.TryUpgrade(Stage2Profession.Miner, Stage2ProfessionTalent.Strength);
            driver.Rules.TryUpgrade(Stage2Profession.Miner, Stage2ProfessionTalent.Strength);
            driver.Rules.TryUpgrade(Stage2Profession.Miner, Stage2ProfessionTalent.Tempo);
            hud.SetWindow(Stage3PlayerWindow.Hero); hud.ShowProfessions(true);
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080) })
            {
                Screen.SetResolution(size.x, size.y, false); yield return new WaitForSecondsRealtime(.45f);
                hud.SelectProfession(0); hud.SelectProfessionTalent(0);
                yield return Shot(directory, $"miner-{size.x}x{size.y}");
                hud.SelectProfession(1); hud.SelectProfessionTalent(1);
                yield return Shot(directory, $"logger-{size.x}x{size.y}");
                hud.ShowProfessions(false); yield return Shot(directory, $"combat-{size.x}x{size.y}");
                hud.ShowProfessions(true);
            }
            Screen.SetResolution(1600, 900, false); yield return new WaitForSecondsRealtime(.3f);
            hud.SelectProfession(0); hud.SelectProfessionTalent(0);
            File.WriteAllText(Path.Combine(directory, "verification.txt"),
                "Actual native Mac development-player captures. Explicit QA mastery awards and frozen clock/combat, normal start unchanged.\n" +
                $"miner={driver.Rules.Mastery(Stage2Profession.Miner)} XP={driver.Rules.XpWithin(Stage2Profession.Miner)}/{driver.Rules.RequiredXp(Stage2Profession.Miner)} points={driver.Rules.AvailablePoints(Stage2Profession.Miner)}\n" +
                $"logger={driver.Rules.Mastery(Stage2Profession.Logger)} XP={driver.Rules.XpWithin(Stage2Profession.Logger)}/{driver.Rules.RequiredXp(Stage2Profession.Logger)} points={driver.Rules.AvailablePoints(Stage2Profession.Logger)}\n" +
                $"tab={hud.ProfessionTabRect(true)} miner={hud.ProfessionCardRect(0)} logger={hud.ProfessionCardRect(1)} strength={hud.ProfessionTalentRect(0)} tempo={hud.ProfessionTalentRect(1)} reach={hud.ProfessionTalentRect(2)} upgrade={hud.ProfessionUpgradeRect}\n");
            Debug.Log("S3_PROFESSION_NATIVE_READY " + directory);
            // Remain interactive for actual native mouse/E verification and report only changed facts.
            driver.Rules.Changed += () => Debug.Log($"S3_PROFESSION_NATIVE_CHANGED miner={driver.Rules.Mastery(Stage2Profession.Miner)}/{driver.Rules.XpWithin(Stage2Profession.Miner)} points={driver.Rules.AvailablePoints(Stage2Profession.Miner)} ranks={driver.Rules.Rank(Stage2Profession.Miner, Stage2ProfessionTalent.Strength)},{driver.Rules.Rank(Stage2Profession.Miner, Stage2ProfessionTalent.Tempo)},{driver.Rules.Rank(Stage2Profession.Miner, Stage2ProfessionTalent.Reach)} logger={driver.Rules.Mastery(Stage2Profession.Logger)}/{driver.Rules.XpWithin(Stage2Profession.Logger)} points={driver.Rules.AvailablePoints(Stage2Profession.Logger)} ranks={driver.Rules.Rank(Stage2Profession.Logger, Stage2ProfessionTalent.Strength)},{driver.Rules.Rank(Stage2Profession.Logger, Stage2ProfessionTalent.Tempo)},{driver.Rules.Rank(Stage2Profession.Logger, Stage2ProfessionTalent.Reach)}");
        }
        private static IEnumerator Shot(string directory, string name)
        {
            yield return null; yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png")); yield return null;
        }
    }
}
