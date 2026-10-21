using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage3.VisualPilot;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit opt-in disposable QA of the actual equipped renderers and native input.</summary>
    public static class Stage3HeldEquipmentReview
    {
        public static IEnumerator Capture(GameObject owner,string directory)
        {
            Directory.CreateDirectory(directory);
            var game=owner.GetComponent<Stage2GameComposition>();var scene=owner.GetComponent<Stage2SceneBootstrap>();
            var hud=owner.GetComponent<Stage3FantasyHud>();var binding=owner.GetComponent<Stage3PlayerInterfaceBinding>();
            while(binding.Equipment==null||!hud.IsReady)yield return null;
            for(var i=0;i<12;i++)yield return null;
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;game.RunDriver.ResetRun();
            var rules=binding.Equipment.Rules;var avatar=scene.PlayerMover.GetComponent<Stage3HeroAvatar>();
            var camera=scene.CameraFollow.BoundCamera;scene.CameraFollow.ZoomByPinchScale(camera.orthographicSize/2.5f);
            var starter=rules.Equipped(Stage2ItemSlot.Weapon);
            rules.TryReceiveLoot(Stage2ItemType.Bow,Stage2ItemQuality.S,1,out var bow);
            rules.TryReceiveLoot(Stage2ItemType.Pickaxe,Stage2ItemQuality.S,1,out var pick);
            rules.TryReceiveLoot(Stage2ItemType.Axe,Stage2ItemQuality.S,1,out var axe);
            File.WriteAllText(Path.Combine(directory,"verification.txt"),
                $"Actual development player {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceType}.\n"+
                "Declared disposable QA: stopped automatic clock/combat, granted S1bow/pick/axe through future-loot API only; no live drop source or ordinary startup grants. Position/camera changes below are capture fixtures. Same actual HUD, owned equip rules, gather facts and renderer.\n");
            hud.SetWindow(Stage3PlayerWindow.Inventory);yield return Shot(directory,"00-F-bow-inventory-spares");
            hud.SetWindow(Stage3PlayerWindow.None);
            yield return Shot(directory,"01-starter-bow-close");Debug.Log("S3_HELD_NATIVE_READY_EQUIP_BOW");
            while(rules.Equipped(Stage2ItemSlot.Weapon)!=bow)yield return null;
            Require(hud.ActiveWindow==Stage3PlayerWindow.Inventory,"native inventory equip");
            yield return Shot(directory,"02-native-S-bow-inventory");
            while(hud.ActiveWindow!=Stage3PlayerWindow.None)yield return null;
            Require(avatar.DisplayedEquipmentType==0&&avatar.DisplayedEquipmentQuality==6,"actual S bow renderer");
            yield return Shot(directory,"03-native-S-bow-world");
            var start=scene.PlayerMover.transform.position;
            foreach(var item in new[]{starter,bow})
            {
                rules.TryEquip(item.Id);yield return null;yield return null;
                var directions=new[]{Vector3.right,Vector3.up,Vector3.left,Vector3.down};
                for(var i=0;i<4;i++)
                {
                    scene.PlayerMover.transform.position=start+directions[i]*.2f;yield return null;yield return null;
                    scene.CameraFollow.SnapToTarget();yield return Shot(directory,$"bow-{item.Quality}-direction-{i}");
                    scene.PlayerMover.transform.position=start;yield return null;
                }
            }
            rules.TryEquip(pick.Id);rules.TryEquip(axe.Id);yield return null;yield return null;
            foreach(var kind in new[]{MapResourceKind.Wood,MapResourceKind.Metal})
            {
                var economy=game.Gathering.Economy;var collisions=owner.GetComponent<Stage3ManualResourceCollisions>();
                Not3A.Stage2.Economy.Stage2ResourceNode target=null;Vector2 approach=default;
                foreach(var node in economy.Nodes.Where(n=>n.Resource==kind&&!n.IsDepleted&&(kind==MapResourceKind.Wood?n.GroupId>=0:n.Size==Not3A.Stage2.Economy.Stage2ResourceSize.Large)))
                {
                    var radius=collisions.ColliderRadius(node)+scene.PlayerMover.CollisionRadius+.08f;
                    foreach(var side in new[]{Vector2.left,Vector2.down,Vector2.right,Vector2.up})
                    {
                        var point=node.WorldPosition+side*radius;
                        if(!scene.PlayerMover.Obstacles.IsBlocked(point,scene.PlayerMover.CollisionRadius)&&economy.NearestAvailableNode(point)==node)
                        {target=node;approach=point;break;}
                    }
                    if(target!=null)break;
                }
                Require(target!=null,"clear physical approach");
                scene.PlayerMover.transform.position=approach;yield return null;yield return null;scene.CameraFollow.SnapToTarget();
                yield return Shot(directory,$"{kind}-native-ready");Debug.Log("S3_HELD_NATIVE_READY_E "+kind);
                var before=avatar.ConfirmedStrikes;
                while(!avatar.IsHarvesting)yield return null;
                Require(avatar.UsesDetachedHarvestTool&&avatar.DisplayedEquipmentQuality==6,"equipped S tool shown");
                yield return Shot(directory,$"{kind}-native-anticipation",false);
                while(avatar.HarvestFrame<2)yield return null;
                yield return Shot(directory,$"{kind}-native-raised",false);
                while(avatar.ConfirmedStrikes==before)yield return null;
                Require(avatar.UsesDetachedHarvestTool,"detached tool contact");
                yield return Shot(directory,$"{kind}-native-contact",false);
                while(Keyboard.current.eKey.isPressed)yield return null;
                for(var i=0;i<20;i++)yield return null;
                Require(!avatar.UsesDetachedHarvestTool&&avatar.DisplayedEquipmentType==0,"bow returns after tool use");
                File.AppendAllText(Path.Combine(directory,"verification.txt"),$"PASS native held E {kind}: S tool visible, raised/contact recorded, actual strike, bow restored.\n");
            }
            File.AppendAllText(Path.Combine(directory,"verification.txt"),"PASS native inventory S bow selection/equip and world renderer. F/S four-facing screenshots are declared automatic pose/equip QA.\n");
            game.RunDriver.ResetRun();yield return null;yield return null;
            hud.SetWindow(Stage3PlayerWindow.Inventory);yield return Shot(directory,"final-empty-bag-starters-only");
            hud.SetWindow(Stage3PlayerWindow.None);
            Debug.Log("S3_HELD_NATIVE_REVIEW_OK "+directory);
        }
        private static void Require(bool value,string reason)
        {if(!value)throw new InvalidOperationException("Held equipment QA: "+reason);}
        private static IEnumerator Shot(string directory,string name,bool settle=true)
        {
            if(settle)yield return new WaitForSecondsRealtime(.18f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));yield return null;
        }
    }
}
