#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3HeldEquipmentTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2EquipmentDriver items;
        private Component binding,hud,avatar;
        private Camera camera;
        private int previousRate;
        private const string Evidence="Artifacts/S3-EQUIPMENT-VISIBLE-20261020/playmode";
        private static T Read<T>(object owner,string name)=>(T)owner.GetType().GetProperty(name).GetValue(owner);
        private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).Invoke(owner,args);
        private void Window(string name)=>Call(hud,"SetWindow",Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType,name));
        private IEnumerator Equip(Stage2EquipmentItem item)
        {
            Window("Inventory");Assert.That(Call(binding,"TryEquipItem",item.Id),Is.True);Window("None");yield return null;yield return null;
        }
        [UnitySetUp] public IEnumerator Load()
        {
            previousRate=Time.captureFramerate;Time.captureFramerate=60;Time.timeScale=1;
            input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<14;i++)yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            items=game.GetComponent<Stage2EquipmentDriver>();binding=game.GetComponent("Stage3PlayerInterfaceBinding");hud=game.GetComponent("Stage3FantasyHud");
            avatar=scene.PlayerMover.GetComponent("Stage3HeroAvatar");camera=scene.CameraFollow.BoundCamera;
            game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;game.RunDriver.ResetRun();
            scene.CameraFollow.enabled=false;camera.orthographicSize=2.5f;Directory.CreateDirectory(Evidence);yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;yield return null;input.TearDown();Time.captureFramerate=previousRate;Time.timeScale=1;
        }
        [UnityTest] public IEnumerator BowQualityChangesActualHeldSpriteInAllFacingsWithoutReplacingArmor()
        {
            var starter=items.Rules.Equipped(Stage2ItemSlot.Weapon);
            Assert.That(items.Rules.TryReceiveLoot(Stage2ItemType.Bow,Stage2ItemQuality.S,1,out var gold),Is.True);
            var directions=new[]{Vector3.right,Vector3.up,Vector3.left,Vector3.down};
            var bodyFrames=new Sprite[4];var weaponFrames=new Sprite[4];
            for(var pass=0;pass<2;pass++)
            {
                yield return Equip(pass==0?starter:gold);
                for(var dir=0;dir<4;dir++)
                {
                    scene.PlayerMover.transform.position+=directions[dir]*.2f;yield return null;yield return null;
                    var body=Read<SpriteRenderer>(avatar,"Body");var weapon=Read<SpriteRenderer>(avatar,"Equipment");
                    Assert.That(Read<int>(avatar,"Direction"),Is.EqualTo(dir));
                    Assert.That(weapon.enabled&&weapon.sprite!=null,Is.True);
                    Assert.That(Read<int>(avatar,"DisplayedEquipmentType"),Is.Zero);
                    Assert.That(Read<int>(avatar,"DisplayedEquipmentQuality"),Is.EqualTo(pass==0?0:6));
                    Assert.That(weapon.sprite.bounds.size.magnitude,Is.InRange(.4f,1.8f),"Bow must retain a hero-sized physical projection.");
                    Assert.That(Vector3.Distance(weapon.transform.position,body.bounds.center),Is.LessThan(1.5f));
                    if(pass==0){bodyFrames[dir]=body.sprite;weaponFrames[dir]=weapon.sprite;}
                    else {Assert.That(body.sprite,Is.SameAs(bodyFrames[dir]),"Changing weapon quality must not redress the hero.");Assert.That(weapon.sprite,Is.Not.SameAs(weaponFrames[dir]));}
                    Capture($"bow-{(pass==0?"F":"S")}-direction-{dir}");
                }
            }
            Window("Inventory");Assert.That(Call(binding,"TryUnequipSlot",4),Is.True);Window("None");yield return null;
            Assert.That(Read<SpriteRenderer>(avatar,"Equipment").enabled,Is.False);
            yield return Equip(starter);Assert.That(Read<int>(avatar,"DisplayedEquipmentQuality"),Is.Zero);
        }
        [UnityTest] public IEnumerator EquippedToolQualityFollowsRealGatherContactAndReturnsToBow()
        {
            foreach(var kind in new[]{MapResourceKind.Wood,MapResourceKind.Metal})
            {
                var type=kind==MapResourceKind.Wood?Stage2ItemType.Axe:Stage2ItemType.Pickaxe;
                var slot=Stage2EquipmentCatalog.Slot(type);var starter=items.Rules.Equipped(slot);
                Assert.That(items.Rules.TryReceiveLoot(type,Stage2ItemQuality.S,1,out var gold),Is.True);
                var site=Approach(kind,Vector2.left);var node=site.node;
                Sprite initialTool=null;
                foreach(var tool in new[]{starter,gold})
                {
                    input.Release(Keyboard.current.eKey);yield return null;yield return Equip(tool);
                    scene.PlayerMover.transform.position=site.position;yield return null;
                    var stock=node.Remaining;input.Press(Keyboard.current.eKey);
                    for(var i=0;i<14;i++)yield return null;
                    Assert.That(Read<bool>(avatar,"UsesDetachedHarvestTool"),Is.True);
                    Assert.That(Read<int>(avatar,"DisplayedEquipmentType"),Is.EqualTo((int)type));
                    Assert.That(Read<int>(avatar,"DisplayedEquipmentQuality"),Is.EqualTo((int)tool.Quality));
                    var renderer=Read<SpriteRenderer>(avatar,"Equipment");Assert.That(renderer.enabled,Is.True);
                    if(tool==starter)initialTool=renderer.sprite;else Assert.That(renderer.sprite,Is.Not.SameAs(initialTool));
                    Capture($"{type}-{tool.Quality}-anticipation");
                    for(var i=0;i<180&&node.Remaining==stock;i++)yield return null;
                    Assert.That(node.Remaining,Is.LessThan(stock));
                    yield return null; // Let LateUpdate apply the confirmed contact before camera.Render.
                    Assert.That(Read<bool>(avatar,"UsesDetachedHarvestTool"),Is.True,"Confirmed strike must keep the actual tool in its contact pose.");
                    Capture($"{type}-{tool.Quality}-contact");
                    game.SetPaused(true);yield return null;var position=renderer.transform.position;var rotation=renderer.transform.rotation;
                    for(var i=0;i<5;i++)yield return null;
                    Assert.That(renderer.transform.position,Is.EqualTo(position));Assert.That(Quaternion.Angle(renderer.transform.rotation,rotation),Is.LessThan(.001f));
                    game.SetPaused(false);input.Release(Keyboard.current.eKey);for(var i=0;i<18;i++)yield return null;
                    Assert.That(Read<bool>(avatar,"UsesDetachedHarvestTool"),Is.False);Assert.That(Read<int>(avatar,"DisplayedEquipmentType"),Is.Zero);
                }
                Window("Inventory");Call(binding,"TryUnequipSlot",(int)slot);Window("None");yield return null;
                input.Press(Keyboard.current.eKey);for(var i=0;i<12;i++)yield return null;
                Assert.That(Read<bool>(avatar,"UsesDetachedHarvestTool"),Is.False);input.Release(Keyboard.current.eKey);
            }
        }
        [UnityTest] public IEnumerator ToolPhasesHaveVisibleQualityAtClearSideBackAndFrontApproaches()
        {
            foreach(var kind in new[]{MapResourceKind.Wood,MapResourceKind.Metal})
            foreach(var quality in new[]{Stage2ItemQuality.F,Stage2ItemQuality.S})
            foreach(var side in new[]{Vector2.left,Vector2.down,Vector2.up})
            {
                var type=kind==MapResourceKind.Wood?Stage2ItemType.Axe:Stage2ItemType.Pickaxe;
                Assert.That(items.Rules.TryReceiveLoot(type,quality,1,out var item),Is.True);
                input.Release(Keyboard.current.eKey);yield return null;yield return Equip(item);
                var site=Approach(kind,side);scene.PlayerMover.transform.position=site.position;
                yield return null;yield return null;var phases=new System.Collections.Generic.HashSet<int>();
                input.Press(Keyboard.current.eKey);
                for(var tick=0;tick<180&&phases.Count<6;tick++)
                {
                    yield return null;
                    if(!Read<bool>(avatar,"UsesDetachedHarvestTool"))continue;
                    var frame=Read<int>(avatar,"HarvestFrame");
                    if(!phases.Add(frame))continue;
                    Assert.That(Read<int>(avatar,"DisplayedEquipmentType"),Is.EqualTo((int)type));
                    Assert.That(Read<int>(avatar,"DisplayedEquipmentQuality"),Is.EqualTo((int)quality));
                    Capture($"phase-{type}-{quality}-dir{Read<int>(avatar,"Direction")}-{frame}");
                }
                foreach(var required in new[]{0,2,3,5})
                    Assert.That(phases.Contains(required),Is.True,"Actual gather must visibly wind up, hit and recover: phase "+required);
                input.Release(Keyboard.current.eKey);for(var i=0;i<20;i++)yield return null;
            }
        }
        private (Stage2ResourceNode node,Vector2 position) Approach(MapResourceKind kind,Vector2 side)
        {
            var economy=game.Gathering.Economy;var collisions=game.GetComponent("Stage3ManualResourceCollisions");
            foreach(var node in economy.Nodes.Where(n=>n.Resource==kind&&!n.IsDepleted&&
                (kind==MapResourceKind.Wood?n.GroupId>=0:n.Size==Stage2ResourceSize.Large)))
            {
                var radius=(float)Call(collisions,"ColliderRadius",node)+scene.PlayerMover.CollisionRadius+.08f;
                var point=node.WorldPosition+side*radius;
                if(!scene.PlayerMover.Obstacles.IsBlocked(point,scene.PlayerMover.CollisionRadius)&&economy.NearestAvailableNode(point)==node)
                    return(node,point);
            }
            Assert.Fail("No physically clear approach for "+kind+" "+side);return default;
        }
        private void Capture(string name)
        {
            var target=new RenderTexture(1280,720,24);var old=camera.targetTexture;var active=RenderTexture.active;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try
            {
                camera.transform.position=scene.PlayerMover.transform.position+Vector3.back*.55f-camera.transform.forward*30;
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(Evidence,name+".png"),image.EncodeToPNG());
            }
            finally{camera.targetTexture=old;RenderTexture.active=active;Object.Destroy(target);Object.Destroy(image);}
        }
    }
}
#endif
