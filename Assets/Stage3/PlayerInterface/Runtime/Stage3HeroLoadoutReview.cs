using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Not3A.Stage2.Integration;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Invoked explicitly by development review after its real gameplay fixture is prepared.</summary>
    public static class Stage3HeroLoadoutReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            if (!Debug.isDebugBuild && !Application.isEditor) yield break;
            Directory.CreateDirectory(directory);
            var hud = owner.GetComponent<Stage3FantasyHud>();
            var game = owner.GetComponent<Stage2GameComposition>();
            var deadline = Time.realtimeSinceStartup + 30f;
            while ((hud == null || !hud.IsReady || hud.HeroLoadoutView == null) && Time.realtimeSinceStartup < deadline)
            { yield return null; hud = owner.GetComponent<Stage3FantasyHud>(); }
            if (hud == null || !hud.IsReady || hud.HeroLoadoutView == null)
                throw new InvalidOperationException("Hero loadout review has no bound actual loadout.");
            var frames = new List<string>(); var facts = new List<string>();
            foreach (var size in new[] { new Vector2Int(1280,720), new Vector2Int(1600,900), new Vector2Int(1920,1080) })
            {
                Screen.SetResolution(size.x, size.y, false);
                yield return new WaitForSecondsRealtime(.3f);
                hud.CloseWindow(); game.SetPaused(false);
                for (var weapon = 0; weapon < 2; weapon++)
                {
                    if (hud.HeroLoadoutView.ActiveWeapon != weapon) hud.RequestWeaponSwitch();
                    yield return null; yield return null;
                    if (hud.HeroLoadoutView.ActiveWeapon != weapon)
                        throw new InvalidOperationException("Hero review fixture must equip both weapons before capture.");
                    yield return Shot(weapon == 0 ? "hud-bow" : "hud-staff", size);
                }
                hud.SetWindow(Stage3PlayerWindow.Hero); hud.ShowHeroDevelopmentTab(0);
                foreach (var skill in new[] { 0, 9, 14 })
                {
                    hud.SelectSkill(skill); yield return null; yield return null;
                    yield return Shot("tree-" + skill, size);
                }
                hud.ShowHeroDevelopmentTab(1); yield return null;
                yield return Shot("gathering", size);
                if (hud.HeroLoadoutView.AdventurerNodes?.Length > 0)
                {
                    hud.ShowHeroDevelopmentTab(2); yield return null;
                    yield return Shot("adventurer", size);
                }
            }
            hud.CloseWindow();
            File.WriteAllLines(Path.Combine(directory, "frames.txt"), frames);
            File.WriteAllLines(Path.Combine(directory, "facts.txt"), facts);
            File.WriteAllText(Path.Combine(directory, "qualification.txt"), "native frames=" + frames.Count + "\nActual bound skills/items; geometry and appearance require Astra review.\n");
            Debug.Log("S3_HERO_LOADOUT_NATIVE_READY: " + directory);

            IEnumerator Shot(string name, Vector2Int size)
            {
                yield return new WaitForEndOfFrame();
                var texture = ScreenCapture.CaptureScreenshotAsTexture();
                if (texture == null || texture.width != size.x || texture.height != size.y)
                    throw new InvalidOperationException("Unexpected hero review screen size.");
                var path = Path.Combine(directory, size.x + "x" + size.y + "-" + name + ".png");
                File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.Destroy(texture);
                frames.Add(path);
                facts.Add(name + " " + size + " weapon=" + hud.HeroLoadoutView.ActiveWeapon + " selected=" + hud.SelectedSkillId +
                    " nodes=" + (hud.HeroLoadoutView.Nodes?.Length ?? 0) + " commonBlink=" + hud.BlinkSlotRect +
                    " fire=" + hud.SkillNodeRect(9) + " upgrade=" + hud.SkillUpgradeRect);
                yield return null;
            }
        }
    }
}
