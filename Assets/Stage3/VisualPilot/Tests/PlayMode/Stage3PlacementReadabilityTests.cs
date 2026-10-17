#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3PlacementReadabilityTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Camera camera;
        private Component world;
        private Stage2ProductionDriver Production => game.Production;
        private static string Evidence => Environment.GetEnvironmentVariable("NOT3A_PLACEMENT_EVIDENCE") ?? "Artifacts/S3-PLACEMENT-READABILITY-20261016/game";
        [UnitySetUp] public IEnumerator Load()
        {
            input=new InputTestFixture(); input.Setup(); InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>(); scene=game.GetComponent<Stage2SceneBootstrap>();
            world=game.GetComponent("Stage3ResourceDistrictWorld");
            world.GetType().GetMethod("Regenerate").Invoke(world,new object[]{164032});
            camera=scene.CameraFollow.BoundCamera; scene.CameraFollow.enabled=false; camera.orthographicSize=5;
            game.RunDriver.ResetRun(); Directory.CreateDirectory(Evidence); yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; yield return null; input.TearDown(); Time.timeScale=1;
        }
        private void Fund()
        {
            var inventory=game.Gathering.Economy.Inventory;
            foreach(MapResourceKind kind in Enum.GetValues(typeof(MapResourceKind))) inventory.TryAdd(kind,100);
            inventory.TryAddGold(100);
        }
        private Stage2PlacementMark[] Marks() => Production.GetComponentsInChildren<Stage2PlacementMark>(true)
            .Where(m=>m.name.StartsWith("Free production pad ")).ToArray();
        private void Point(Vector2 p)
        {
            camera.transform.position=(Vector3)p-camera.transform.forward*30;
            input.Set(Mouse.current.position,(Vector2)camera.WorldToScreenPoint(p));
        }
        [UnityTest] public IEnumerator FreeTypedMarksExistBeforeSelectionAndFollowBuildResetRegenerate()
        {
            Assert.That(Production.IsPlacing,Is.False);
            Assert.That(Marks().Length,Is.EqualTo(6));
            foreach(ResourceProductionBuilding kind in Enum.GetValues(typeof(ResourceProductionBuilding)))
                Assert.That(Marks().Count(m=>m.name.EndsWith(kind.ToString())),Is.EqualTo(2));
            foreach(var mark in Marks())
            {
                Assert.That(mark.gameObject.activeSelf,Is.True);
                Assert.That(mark.GetComponentsInChildren<Collider>().Length,Is.Zero);
                Assert.That(mark.GetComponentsInChildren<Collider2D>().Length,Is.Zero);
                Assert.That(Production.Rules.Points.Any(p=>Vector2.Distance(p.Position,mark.transform.position)<.001f),Is.True);
            }
            Fund();
            foreach(var kind in new[]{ResourceProductionBuilding.Sawmill,ResourceProductionBuilding.Quarry,ResourceProductionBuilding.IronMine})
            {
                var point=Production.Rules.Points.First(p=>p.Building==kind); Point(point.Position);
                yield return null; Save(camera,Path.Combine(Evidence,kind+"-marker-normal.png"));
                camera.orthographicSize=2.5f; Save(camera,Path.Combine(Evidence,kind+"-marker-close.png"));
                camera.orthographicSize=10; Save(camera,Path.Combine(Evidence,kind+"-marker-overview.png")); camera.orthographicSize=5;
            }
            var first=Production.Rules.Points[0]; Production.SelectAndBeginPlacement(first.Building);
            Assert.That(Production.TryBuildAt(0),Is.EqualTo(ProductionPlacementResult.Built));
            Assert.That(Marks().Count(m=>m.gameObject.activeSelf),Is.EqualTo(5));
            for(var i=1;i<Production.Rules.Points.Count;i++)
            { Production.SelectAndBeginPlacement(Production.Rules.Points[i].Building); Assert.That(Production.TryBuildAt(i),Is.EqualTo(ProductionPlacementResult.Built)); }
            Assert.That(Marks().Count(m=>m.gameObject.activeSelf),Is.Zero);
            foreach(var built in Production.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.StartsWith("Production ")))
                Assert.That(built.sharedMaterial,Is.SameAs(Production.Catalog.SpriteMaterial),"Preview material must never leak into constructed art.");
            game.RunDriver.ResetRun(); Assert.That(Marks().Count(m=>m.gameObject.activeSelf),Is.EqualTo(6));
            var old=Marks(); world.GetType().GetMethod("Regenerate").Invoke(world,new object[]{164033});
            yield return null; yield return null;
            Assert.That(old.All(m=>m==null),Is.True); Assert.That(Marks().Length,Is.EqualTo(6));
        }
        [UnityTest] public IEnumerator GeneratorGhostsRenderGreenReadyRedWrongOrBlockedAndHideOverUi()
        {
            Fund();
            foreach(var entry in Production.Catalog.Entries)
            {
                var index=Enumerable.Range(0,Production.Rules.Points.Count).First(i=>Production.Rules.Points[i].Building==entry.Building);
                var point=Production.Rules.Points[index]; Point(point.Position);
                Production.SelectAndBeginPlacement(entry.Building); yield return null; yield return null;
                Assert.That(Production.PreviewResult,Is.EqualTo(ProductionPlacementResult.Ready));
                var ghost=Production.GetComponentsInChildren<SpriteRenderer>().Single(r=>r.name=="Production placement preview");
                AssertGhost(ghost,true); Assert.That(Marks().Count(m=>m.gameObject.activeSelf),Is.EqualTo(5));
                Save(camera,Path.Combine(Evidence,entry.Building+"-green.png"));
                AssertTransparency(ghost.gameObject);
                scene.PlayerMover.transform.position=point.Position; yield return null; yield return null;
                Assert.That(Production.PreviewResult,Is.EqualTo(ProductionPlacementResult.HeroBlocked));
                AssertGhost(ghost,false); Save(camera,Path.Combine(Evidence,entry.Building+"-red-blocked.png"));
                Assert.That(Production.TryBuildAt(index),Is.EqualTo(ProductionPlacementResult.HeroBlocked));
                scene.PlayerMover.transform.position=new Vector3(10,10,0);
                var wrong=Production.Rules.Points.First(p=>p.Building!=entry.Building); Point(wrong.Position);
                yield return null; yield return null;
                Assert.That(Production.PreviewResult,Is.EqualTo(ProductionPlacementResult.WrongType)); AssertGhost(ghost,false);
                Point(scene.MapDefinition.CellToWorld(scene.MapDefinition.EnemyEntrance)); yield return null; yield return null;
                Assert.That(Production.PreviewResult,Is.EqualTo(ProductionPlacementResult.InvalidPoint)); AssertGhost(ghost,false);
                Assert.That(Production.TryBuildAt(-1),Is.EqualTo(ProductionPlacementResult.InvalidPoint));
                var hud=game.GetComponent<Stage2GameHud>(); var ui=hud.BuildRect.center*hud.Scale;
                input.Set(Mouse.current.position,new Vector2(ui.x,Screen.height-ui.y)); yield return null;
                Assert.That(ghost.gameObject.activeSelf,Is.False);
                Assert.That(Marks().Count(m=>m.gameObject.activeSelf),Is.EqualTo(6));
                Production.CancelPlacement();
            }
            Assert.That(Production.Rules.Buildings.Count,Is.Zero);
            Assert.That(game.Gathering.Economy.Inventory.Get(MapResourceKind.Wood),Is.EqualTo(100));
        }
        [UnityTest] public IEnumerator BothTowerRolesHaveTransparentGreenAndRedModelPlusGroundOutline()
        {
            Fund();
            var map=scene.MapDefinition;
            var cell=map.MinimumCell;
            var found=false;
            for(var y=map.MinimumCell.y;y<=map.MaximumCell.y&&!found;y++)
            for(var x=map.MinimumCell.x;x<=map.MaximumCell.x;x++)
            {
                var test=new Vector2Int(x,y);
                if(map.Contains(test)&&map.GetSurface(test)==MapSurfaceKind.InvasionCorridor&&map.IsBuildable(test)&&map.IsWalkable(test)&&test!=map.EnemyEntrance&&test!=map.Throne)
                {cell=test;found=true;break;}
            }
            Assert.That(found,Is.True);
            foreach(var role in new[]{0,1})
            {
                Point(map.CellToWorld(cell)); game.Building.SelectAndBeginPlacement(role);
                yield return null; yield return null;
                Assert.That(game.Building.PreviewResult,Is.EqualTo(PlacementResult.Ready));
                var preview=GameObject.Find("Tactical mesh placement preview"); Assert.That(preview,Is.Not.Null);
                var sprites=preview.GetComponentsInChildren<SpriteRenderer>(); Assert.That(sprites.Length,Is.GreaterThan(0));
                foreach(var sprite in sprites) AssertGhost(sprite,true);
                Assert.That(preview.GetComponentInChildren<Stage2PlacementMark>(),Is.Not.Null);
                Save(camera,Path.Combine(Evidence,"Tower"+role+"-green.png")); AssertTransparency(preview);
                Assert.That(sprites[0].sprite.texture.name,Is.EqualTo(role==0?"RapidGridFacing":"LongshotGridFacing"));
                var previewSprite=sprites[0].sprite;
                var previewSize=sprites[0].bounds.size;
                Point(Production.Rules.Points[0].Position); yield return null; yield return null;
                Assert.That(game.Building.PreviewResult,Is.EqualTo(PlacementResult.InvalidCell));
                foreach(var sprite in sprites) AssertGhost(sprite,false);
                Save(camera,Path.Combine(Evidence,"Tower"+role+"-red.png"));
                Point(map.CellToWorld(cell)); yield return null; yield return null;
                Assert.That(game.Building.Defense.TryConfirm(out var built),Is.EqualTo(PlacementResult.Ready));
                yield return null;
                var builtBody=built.transform.Find("Tower artwork").GetComponent<SpriteRenderer>();
                Assert.That(builtBody.sprite,Is.SameAs(previewSprite));
                Assert.That(builtBody.bounds.size,Is.EqualTo(previewSize));
                Assert.That(Quaternion.Angle(builtBody.transform.rotation,camera.transform.rotation),Is.LessThan(.001f));
                game.Building.CancelPlacement(); yield return null;
                Save(camera,Path.Combine(Evidence,"Tower"+role+"-built.png"));
                game.RunDriver.ResetRun(); Fund(); yield return null;
            }
        }
        [UnityTest] public IEnumerator TowerGridCoversOnlyGrayBuildCellsAndFollowsConstructionLifecycle()
        {
            Fund();
            var grid=scene.transform.Find("Tower construction cell grid");
            Assert.That(grid,Is.Not.Null);
            Assert.That(grid.gameObject.activeSelf,Is.False);
            Assert.That(grid.GetComponentsInChildren<Collider>().Length,Is.Zero);
            Assert.That(grid.GetComponentsInChildren<Collider2D>().Length,Is.Zero);
            var map=scene.MapDefinition;
            var mesh=grid.GetComponent<MeshFilter>().sharedMesh;
            Assert.That(mesh.vertexCount,Is.GreaterThan(0));
            foreach(var vertex in mesh.vertices)
            {
                var cell=map.WorldToCell(grid.TransformPoint(vertex));
                Assert.That(map.Contains(cell)&&map.GetSurface(cell)==MapSurfaceKind.InvasionCorridor&&
                    map.IsWalkable(cell)&&map.IsBuildable(cell)&&cell!=map.EnemyEntrance&&cell!=map.Throne,Is.True,
                    "Grid strokes must remain inside real legal road cells, excluding grass and reserved landmarks.");
            }
            var focus=map.WorldToCell(mesh.vertices[0]); Point(map.CellToWorld(focus));
            game.Building.SelectAndBeginPlacement(0); yield return null; yield return null;
            Assert.That(grid.gameObject.activeSelf,Is.True);
            var visible=Render(camera,640,450);
            grid.gameObject.SetActive(false); var hidden=Render(camera,640,450);
            Assert.That(visible.Zip(hidden,(a,b)=>Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b))
                .Count(delta=>delta>.12f),Is.GreaterThan(100),"Actual construction lines must be readable over painted paving.");
            yield return null;
            Save(camera,Path.Combine(Evidence,"Tower-grid-normal.png"));
            camera.orthographicSize=2.5f; Save(camera,Path.Combine(Evidence,"Tower-grid-close.png"));
            camera.orthographicSize=10; Save(camera,Path.Combine(Evidence,"Tower-grid-overview.png"));
            camera.orthographicSize=5;
            var hud=game.GetComponent<Stage2GameHud>(); var ui=hud.BuildRect.center*hud.Scale;
            input.Set(Mouse.current.position,new Vector2(ui.x,Screen.height-ui.y)); yield return null;
            Assert.That(game.Building.PointerOverUi,Is.True);
            Assert.That(grid.gameObject.activeSelf,Is.True,"Selecting a card already reveals the whole grid.");
            game.Building.CancelPlacement(); yield return null;
            Assert.That(grid.gameObject.activeSelf,Is.False);
            Production.SelectAndBeginPlacement(ResourceProductionBuilding.Sawmill); yield return null;
            Assert.That(grid.gameObject.activeSelf,Is.False,"Generators use their own locked pads.");
            game.Building.SelectAndBeginPlacement(1); yield return null;
            Assert.That(Production.IsPlacing,Is.False); Assert.That(grid.gameObject.activeSelf,Is.True);
            game.RunDriver.SetPaused(true); yield return null;
            Assert.That(grid.gameObject.activeSelf,Is.False);
            game.RunDriver.SetPaused(false); game.Building.SelectAndBeginPlacement(0); yield return null;
            Assert.That(grid.gameObject.activeSelf,Is.True);
            game.RunDriver.ResetRun(); yield return null;
            Assert.That(grid.gameObject.activeSelf,Is.False);
        }
        [UnityTest] public IEnumerator AdjacentTowerSilhouettesFitCellsAndMeetTheirFrontEdgeAtBothZooms()
        {
            Fund();
            var map=scene.MapDefinition;
            var row=new Vector2Int[5]; var found=false;
            for(var y=map.MinimumCell.y;y<=map.MaximumCell.y&&!found;y++)
            for(var x=map.MinimumCell.x;x<=map.MaximumCell.x-4;x++)
            {
                var cells=Enumerable.Range(0,5).Select(i=>new Vector2Int(x+i,y)).ToArray();
                if(!cells.All(c=>map.Contains(c)&&map.GetSurface(c)==MapSurfaceKind.InvasionCorridor&&
                    map.IsBuildable(c)&&map.IsWalkable(c)&&c!=map.EnemyEntrance&&c!=map.Throne&&
                    map.Contains(c+Vector2Int.down)&&map.GetSurface(c+Vector2Int.down)==MapSurfaceKind.InvasionCorridor)) continue;
                row=cells; found=true; break;
            }
            Assert.That(found,Is.True,"A dense five-cell construction row must exist in the fixed map seed.");
            scene.PlayerMover.transform.position=map.CellToWorld(row[0])+Vector2.down*3;
            var built=new GameObject[4]; var roles=new[]{1,0,0,1};
            for(var i=0;i<4;i++)
            {
                Fund(); Point(map.CellToWorld(row[i+1])); game.Building.SelectAndBeginPlacement(roles[i]);
                yield return null; yield return null;
                Assert.That(game.Building.Defense.TryConfirm(out var tower),Is.EqualTo(PlacementResult.Ready));
                built[i]=tower.gameObject;
            }
            foreach(var zoom in new[]{5f,2.5f})
            {
                camera.orthographicSize=zoom;
                // Pick away from HUD, then frame the row synchronously for offscreen renders.
                Point(map.CellToWorld(row[0]));
                Fund(); game.Building.SelectAndBeginPlacement(0); yield return null; yield return null;
                var preview=GameObject.Find("Tactical mesh placement preview");
                Assert.That(preview,Is.Not.Null);
                Assert.That(game.Building.PreviewResult,Is.EqualTo(PlacementResult.Ready));
                camera.transform.position=(Vector3)map.CellToWorld(row[2])+Vector3.up*.5f-camera.transform.forward*30;
                Save(camera,Path.Combine(Evidence,"Tower-adjacent-row-"+zoom+".png"));
                var previous=AssertCellSilhouette(preview,map.CellToWorld(row[0]),"preview "+zoom);
                for(var i=0;i<4;i++)
                {
                    var pixels=AssertCellSilhouette(built[i],map.CellToWorld(row[i+1]),"tower "+i+" zoom "+zoom);
                    Assert.That(pixels.xMin-previous.xMax,Is.GreaterThan(1f),"Adjacent rendered silhouettes must have a visible gap.");
                    previous=pixels;
                }
                Point(map.CellToWorld(row[0])); game.Building.SelectAndBeginPlacement(1); yield return null; yield return null;
                preview=GameObject.Find("Tactical mesh placement preview");
                Assert.That(preview,Is.Not.Null);
                camera.transform.position=(Vector3)map.CellToWorld(row[2])+Vector3.up*.5f-camera.transform.forward*30;
                AssertCellSilhouette(preview,map.CellToWorld(row[0]),"Longshot preview "+zoom);
            }
        }
        [UnityTest] public IEnumerator PublicAndShortcutAffordabilityGatesAndLateBalanceCheckPreventPurchase()
        {
            foreach(var entry in Production.Catalog.Entries)
            { Assert.That(Production.CanBeginPlacement(entry.Building),Is.False); Production.SelectAndBeginPlacement(entry.Building); Assert.That(Production.IsPlacing,Is.False); }
            foreach(var role in new[]{0,1}) { Assert.That(game.Building.CanBeginPlacement(role),Is.False); game.Building.SelectAndBeginPlacement(role); Assert.That(game.Building.Defense.IsPlacing,Is.False); }
            foreach(var key in new[]{Keyboard.current.bKey,Keyboard.current.digit1Key,Keyboard.current.digit2Key})
            { input.Press(key); yield return null; input.Release(key); yield return null; Assert.That(game.Building.Defense.IsPlacing,Is.False); }
            Fund(); var point=Production.Rules.Points[0]; Point(point.Position); Production.SelectAndBeginPlacement(point.Building);
            yield return null; var inventory=game.Gathering.Economy.Inventory;
            Assert.That(inventory.TrySpend(inventory.Get(MapResourceKind.Wood),inventory.Get(MapResourceKind.Stone),inventory.Get(MapResourceKind.Metal),inventory.Gold),Is.True);
            yield return null;
            Assert.That(Production.PreviewResult,Is.EqualTo(ProductionPlacementResult.InsufficientResources));
            Assert.That(Production.TryBuildAt(0),Is.EqualTo(ProductionPlacementResult.InsufficientResources));
            Assert.That(Production.Rules.Buildings.Count,Is.Zero);
        }
        private static void AssertGhost(SpriteRenderer ghost,bool valid)
        {
            Assert.That(ghost.sharedMaterial.shader.name,Is.EqualTo("Not3A/Placement Preview"));
            Assert.That(ghost.color,Is.EqualTo(valid?Stage2PlacementVisuals.Valid:Stage2PlacementVisuals.Invalid));
            Assert.That(ghost.color.a,Is.InRange(.45f,.55f));
        }
        private Rect AssertCellSilhouette(GameObject root,Vector3 anchor,string label)
        {
            const int width=1280,height=900;
            var sprites=root.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.enabled).ToArray();
            var layers=sprites.Select(r=>r.gameObject.layer).ToArray();
            var mask=camera.cullingMask; var clear=camera.clearFlags; var background=camera.backgroundColor; var aspect=camera.aspect;
            try
            {
                foreach(var sprite in sprites) sprite.gameObject.layer=31;
                camera.cullingMask=1<<31; camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=Color.black;
                camera.aspect=(float)width/height;
                var colors=Render(camera,width,height);
                var rows=new int[height]; var columns=new int[width];
                for(var y=0;y<height;y++) for(var x=0;x<width;x++)
                    if(colors[y*width+x].maxColorComponent>.12f) { rows[y]++; columns[x]++; }
                var left=Array.FindIndex(columns,n=>n>=3); var right=Array.FindLastIndex(columns,n=>n>=3)+1;
                var bottom=Array.FindIndex(rows,n=>n>=3); var top=Array.FindLastIndex(rows,n=>n>=3)+1;
                Assert.That(left,Is.GreaterThanOrEqualTo(0),label+" must render visible artwork.");
                var cellLeft=camera.WorldToViewportPoint(anchor+Vector3.left*.5f).x*width;
                var cellRight=camera.WorldToViewportPoint(anchor+Vector3.right*.5f).x*width;
                var foot=camera.WorldToViewportPoint(anchor+Vector3.down*.48f).y*height;
                Assert.That(left,Is.GreaterThanOrEqualTo(cellLeft+1f),label+" crosses the left cell boundary.");
                Assert.That(right,Is.LessThanOrEqualTo(cellRight-1f),label+" crosses the right cell boundary.");
                Assert.That(Mathf.Abs(bottom-foot),Is.LessThan(3f),label+" must touch the front edge without a vertical gap.");
                File.AppendAllText(Path.Combine(Evidence,"cell-silhouettes.txt"),
                    $"{label}: pixels=[{left},{bottom},{right},{top}] cellX=[{cellLeft},{cellRight}] expectedFoot={foot}\n");
                return Rect.MinMaxRect(left,bottom,right,top);
            }
            finally
            {
                for(var i=0;i<sprites.Length;i++) sprites[i].gameObject.layer=layers[i];
                camera.cullingMask=mask; camera.clearFlags=clear; camera.backgroundColor=background; camera.aspect=aspect;
            }
        }
        private void AssertTransparency(GameObject root)
        {
            var transforms=root.GetComponentsInChildren<SpriteRenderer>().Select(r=>r.transform).ToArray(); var layers=transforms.Select(t=>t.gameObject.layer).ToArray();
            var mask=camera.cullingMask; var clear=camera.clearFlags; var background=camera.backgroundColor;
            try
            {
                foreach(var t in transforms) t.gameObject.layer=31;
                camera.cullingMask=1<<31; camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=Color.black; var black=Render(camera,320,225);
                camera.backgroundColor=Color.white; var white=Render(camera,320,225);
                var samples=0; var opacities=new System.Collections.Generic.List<float>();
                for(var i=0;i<black.Length;i++) if(black[i].g>.25f&&black[i].g>black[i].r*1.5f)
                { Assert.That(white[i].r-black[i].r,Is.GreaterThan(.30f),"Actual rendered ghost must transmit the background."); samples++;
                    opacities.Add(1-(Mathf.GammaToLinearSpace(white[i].r)-Mathf.GammaToLinearSpace(black[i].r))); }
                Assert.That(samples,Is.GreaterThan(10),"Actual preview silhouette must be visibly green, not source brown or invisible.");
                opacities.Sort();
                File.AppendAllText(Path.Combine(Evidence,"pixel-opacity.txt"),root.name+" samples="+samples+" median="+opacities[samples/2]+" p90="+opacities[samples*9/10]+"\n");
            }
            finally
            { for(var i=0;i<transforms.Length;i++) transforms[i].gameObject.layer=layers[i]; camera.cullingMask=mask; camera.clearFlags=clear; camera.backgroundColor=background; }
        }
        private static Color[] Render(Camera camera,int width,int height,string path=null)
        {
            var target=new RenderTexture(width,height,24); var old=camera.targetTexture; var active=RenderTexture.active;
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,width,height),0,0); texture.Apply();
                if(path!=null) File.WriteAllBytes(path,texture.EncodeToPNG()); return texture.GetPixels();
            }
            finally {camera.targetTexture=old;RenderTexture.active=active;Object.Destroy(target);Object.Destroy(texture);}
        }
        private static void Save(Camera camera,string path)=>Render(camera,1280,900,path);
    }
}
#endif
