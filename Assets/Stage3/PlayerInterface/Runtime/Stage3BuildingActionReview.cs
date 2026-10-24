using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Explicit development-player QA; temporary stock/objects are reset on completion.</summary>
    public static class Stage3BuildingActionReview
    {
        public static IEnumerator Capture(GameObject owner, string directory)
        {
            var game = owner.GetComponent<Stage2GameComposition>();
            var scene = owner.GetComponent<Stage2SceneBootstrap>();
            var hud = owner.GetComponent<Stage3FantasyHud>();
            var binding = owner.GetComponent<Stage3PlayerInterfaceBinding>();
            while (!game.IsInitialized || !hud.IsReady) yield return null;
            for (var i=0;i<12;i++) yield return null;
            Directory.CreateDirectory(directory);
            var report = new StringBuilder($"Actual development player {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceType}.\nDisposable scripted setup, real transactions and presentation. Not a natural playthrough.\n");
            void Require(bool value,string message)
            { if(!value) throw new InvalidOperationException("Building action QA: "+message);report.AppendLine("PASS "+message); }
            var constructionReview=Environment.GetEnvironmentVariable("NOT3A_BUILDING_ACTIONS_CONSTRUCTION_REVIEW")=="1";
            var defense=game.Building.Defense;var stock=game.Gathering.Economy.Inventory;
            var camera=scene.CameraFollow.BoundCamera;var previousPosition=camera.transform.position;
            var previousSize=camera.orthographicSize;var previousFollow=scene.CameraFollow.enabled;
            var previousCombat=game.HeroCombat.AutomaticTick;var previousRate=Time.captureFramerate;
            void Fund() => stock.TryCredit(new Stage2ResourceCost(500,500,500,500));
            void Frame(Vector3 position,float zoom=5f)
            {
                camera.orthographicSize=zoom;camera.transform.position=position-camera.transform.forward*30;
                var projection=camera.WorldToScreenPoint(position);var wanted=new Vector2(Screen.width*.54f,Screen.height*.69f);
                camera.transform.position+=position-camera.ScreenToWorldPoint(new Vector3(wanted.x,wanted.y,projection.z));
            }
            SpriteRenderer Body(Stage2Tower tower) => tower.GetComponentInChildren<Stage3TowerSprite>()?.Body ??
                tower.GetComponentsInChildren<SpriteRenderer>().First(r=>r.enabled&&r.gameObject.activeInHierarchy);
            Vector2 Point(Stage2Tower tower) => camera.WorldToScreenPoint(Body(tower).bounds.center);
            Rect BadgeRect(Stage2Tower tower)
            {
                var left=float.PositiveInfinity;var right=float.NegativeInfinity;var top=float.PositiveInfinity;
                var native=tower.GetComponentInChildren<Stage3TowerSprite>(true);
                var renderers=native!=null?native.GetComponentsInChildren<SpriteRenderer>(true):tower.GetComponentsInChildren<SpriteRenderer>(true);
                foreach(var renderer in renderers)
                {
                    if(!renderer.enabled||!renderer.gameObject.activeInHierarchy||renderer.sprite==null||renderer.color.a<=.01f)continue;
                    var bounds=renderer.sprite.bounds;
                    for(var corner=0;corner<4;corner++)
                    {
                        var local=new Vector3((corner&1)==0?bounds.min.x:bounds.max.x,(corner&2)==0?bounds.min.y:bounds.max.y,0);
                        if(renderer.flipX)local.x=-local.x;if(renderer.flipY)local.y=-local.y;
                        var point=camera.WorldToScreenPoint(renderer.transform.TransformPoint(local));
                        if(point.z<=0)continue;
                        left=Mathf.Min(left,point.x);right=Mathf.Max(right,point.x);top=Mathf.Min(top,Screen.height-point.y);
                    }
                }
                return new Rect((left+right)*.5f-24f*hud.UiScale,top-21f*hud.UiScale,48f*hud.UiScale,17f*hud.UiScale);
            }
            void Select(Stage2Tower tower)
            { Frame(Body(tower).bounds.center);Require(binding.TrySelectBuildingAtScreen(Point(tower)),"real visual selection"); }
            Stage2Tower Build(int role,Vector2Int near)
            {
                if(role==1&&!defense.IsRoleAvailable(role))
                {
                    var paper=binding.Merchant.BlueprintItems.First(p=>p.Kind==Stage2BlueprintKind.Longshot);
                    Require(binding.Merchant.TryTradeBlueprint(true,paper.Id)==Stage2TradeResult.Ready,"QA buys actual Longshot prerequisite");
                    Require(binding.Blueprints.TryStudy(paper.Id)==Stage2BlueprintResult.Ready,"QA studies actual Longshot prerequisite");
                }
                binding.ClearBuildingSelection();defense.SelectRole(role);Require(defense.BeginPlacement(),"placement begins");
                for(var radius=1;radius<25;radius++)
                for(var y=-radius;y<=radius;y++) for(var x=-radius;x<=radius;x++)
                {
                    var cell=near+new Vector2Int(x,y);
                    if(defense.Evaluate(cell)!=PlacementResult.Ready) continue;
                    defense.SetPreviewCell(cell);
                    Require(defense.TryConfirm(out var result)==PlacementResult.Ready,"paid authored tower built");return result;
                }
                throw new InvalidOperationException("No legal tower site.");
            }
            Vector2Int EmptyCell(Stage2Tower source)
            {
                for(var radius=1;radius<15;radius++) for(var y=0;y<=radius;y++) for(var x=-radius;x<=radius;x++)
                {
                    var cell=source.Cell+new Vector2Int(x,y);
                    if(defense.EvaluateMove(source,cell)!=Stage2TowerActionResult.Ready||cell==source.Cell)continue;
                    var screen=(Vector2)camera.WorldToScreenPoint(scene.MapDefinition.CellToWorld(cell));
                    if(game.ContainsScreenPoint(screen))continue;
                    var ray=camera.ScreenPointToRay(screen);var covered=false;
                    for(var i=0;i<defense.TowerCount&&!covered;i++)
                    {
                        var other=defense.GetTower(i);if(other==source)continue;
                        foreach(var visual in other.GetComponentsInChildren<Renderer>())
                            if(visual.enabled&&visual.gameObject.activeInHierarchy&&visual.bounds.IntersectRay(ray)) {covered=true;break;}
                    }
                    if(!covered)return cell;
                }
                throw new InvalidOperationException("No empty drag site.");
            }
            try
            {
                game.SetPaused(false);game.RunDriver.ResetRun();hud.CloseWindow();
                Time.captureFramerate=60;scene.CameraFollow.enabled=false;game.HeroCombat.AutomaticTick=false;Fund();
                if(constructionReview)
                {
                    hud.SetWindow(Stage3PlayerWindow.Construction);
                    for(var group=0;group<3;group++)
                    { hud.SetBuildGroup(group);yield return Shot(directory,"00-construction-group-"+group); }
                    hud.SetBuildGroup(0);
                }
                var tower=Build(0,scene.MapDefinition.Throne);yield return null;Select(tower);
                Require(hud.SelectedBuildingView.UpgradeAvailable&&hud.SelectedBuildingView.SellAvailable,"affordable card actions enabled");
                yield return Shot(directory,"01-affordable");
                tower.TakeDamage(2);Require(binding.TryUpgradeSelectedBuilding()==Stage2TowerActionResult.Ready&&tower.Level==2,"card upgrade applies next level");
                yield return Shot(directory,"02-upgraded-damaged");
                stock.Reset();yield return null;Require(!hud.SelectedBuildingView.UpgradeAvailable,"unaffordable action disabled with price visible");
                yield return Shot(directory,"03-unaffordable");Fund();yield return null;
                var towerCatalog=defense.GetTowerCatalog(tower);
                Require(towerCatalog.UnlimitedUpgrades,"tower progression has no authored level cap");
                while(tower.Level<6)
                {
                    Require(towerCatalog.TryGetUpgrade(tower.Level,out var nextLevel),"next paid level exists");
                    Require(stock.TryCredit(nextLevel.Cost),"fund exact next upgrade");
                    Require(binding.TryUpgradeSelectedBuilding()==Stage2TowerActionResult.Ready,"paid next level");
                }
                Require(towerCatalog.TryGetUpgrade(tower.Level,out var seventh),"level seven remains available");
                Require(stock.TryCredit(seventh.Cost),"fund seventh level preview");yield return null;
                Require(hud.SelectedBuildingView.UpgradeAvailable&&hud.SelectedBuildingView.Subtitle=="Уровень 6","uncapped card remains available without huge denominator");
                yield return Shot(directory,"04-level-six");
                var badgeBefore=BadgeRect(tower);var dialCenter=hud.CycleDialRect.center;
                var depth=camera.WorldToScreenPoint(Body(tower).bounds.center).z;
                camera.transform.position+=camera.ScreenToWorldPoint(new Vector3(badgeBefore.center.x,Screen.height-badgeBefore.center.y,depth))-
                    camera.ScreenToWorldPoint(new Vector3(dialCenter.x,Screen.height-dialCenter.y,depth));
                var clockBadge=BadgeRect(tower);
                Require(hud.CycleDialRect.Contains(clockBadge.center),"LVL badge deliberately overlaps clock for native stacking review");
                Require(!clockBadge.Overlaps(hud.ResourcesRect)&&!clockBadge.Overlaps(hud.StatusRect),"clock overlap is not hidden by other badge exclusions");
                yield return Shot(directory,"04-clock-over-level-"+Screen.width+"x"+Screen.height);
                Select(tower);
                var refund=defense.GetSaleRefund(tower);var before=stock.Get(Stage2.Map.MapResourceKind.Wood);
                Require(binding.TrySellSelectedBuilding()==Stage2TowerActionResult.Ready,"card sale succeeds");
                Require(stock.Get(Stage2.Map.MapResourceKind.Wood)==before+refund.Wood&&!hud.HasSelectedBuilding,"exact refund and card cleared");
                Require(binding.TrySellSelectedBuilding()==Stage2TowerActionResult.InvalidTarget,"stale sale cannot pay twice");
                var donor=Build(0,scene.MapDefinition.Throne);var receiver=Build(0,donor.Cell);yield return null;
                var donorCell=donor.Cell;var receiverCell=receiver.Cell;
                var donorHealth=donor.Health;var receiverHealth=receiver.Health;
                var beforeDrop=stock.CaptureState();var beforeCount=defense.TowerCount;
                Select(donor);Require(binding.TryBeginBuildingDrag(donor),"occupied-cell drag begins");
                binding.UpdateBuildingDragTarget(Point(receiver));Require(!binding.DragDropValid,"same role and level occupied preview is invalid");
                Require(!binding.DragWillMerge,"occupied cell never shows merge feedback");
                yield return Shot(directory,"05-invalid-occupied",()=>binding.PreviewBuildingDragAtScreen(Point(receiver)));
                Require(binding.TryDropBuildingAtScreen(Point(receiver))==Stage2TowerActionResult.Occupied,"occupied drop rejected");
                Require(donor.Level==1&&receiver.Level==1&&defense.TowerCount==beforeCount&&
                    donor.Cell==donorCell&&receiver.Cell==receiverCell&&donor.Health==donorHealth&&receiver.Health==receiverHealth&&
                    beforeDrop.Equals(stock.CaptureState()),"rejected drop preserves both towers and stock");
                yield return Shot(directory,"06-rejected-drop");
                // Observe screen-sized level tags at both extremes of the existing world zoom.
                foreach(var zoom in new[]{2.5f,10f})
                {
                    Frame(Body(receiver).bounds.center,zoom);
                    yield return Shot(directory,"06-level-zoom-"+zoom.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture));
                }
                Select(receiver);var empty=EmptyCell(receiver);Require(binding.TryBeginBuildingDrag(receiver),"empty-cell drag begins");
                var emptyPoint=(Vector2)camera.WorldToScreenPoint(scene.MapDefinition.CellToWorld(empty));
                binding.UpdateBuildingDragTarget(emptyPoint);Require(binding.DragDropValid,"empty-cell move preview valid");
                Require(!binding.DragWillMerge,"free movement does not show merge feedback");
                yield return Shot(directory,"07-valid-empty",()=>binding.PreviewBuildingDragAtScreen(emptyPoint));
                Require(binding.TryDropBuildingAtScreen(emptyPoint)==Stage2TowerActionResult.Ready&&receiver.Cell==empty,"move preserves actual tower");
                var artillery=Build(1,receiver.Cell);yield return null;Select(receiver);
                Require(binding.TryBeginBuildingDrag(receiver),"incompatible drag begins");
                binding.UpdateBuildingDragTarget(Point(artillery));Require(!binding.DragDropValid,"different role preview invalid");
                Require(!binding.DragWillMerge,"incompatible occupied cell never shows merge feedback");
                yield return Shot(directory,"08-invalid-merge",()=>binding.PreviewBuildingDragAtScreen(Point(artillery)));
                Require(binding.TryDropBuildingAtScreen(Point(artillery))==Stage2TowerActionResult.Occupied,"invalid drop preserves both towers");
                Select(artillery);empty=EmptyCell(artillery);Require(binding.TryBeginBuildingDrag(artillery),"layered Longshot drag begins");
                binding.UpdateBuildingDragTarget(camera.WorldToScreenPoint(scene.MapDefinition.CellToWorld(empty)));
                Require(binding.DragDropValid,"Longshot empty-cell preview valid");yield return Shot(directory,"09-longshot-ghost",()=>binding.PreviewBuildingDragAtScreen(camera.WorldToScreenPoint(scene.MapDefinition.CellToWorld(empty))));
                binding.ClearBuildingSelection();
                var barrier=Build(2,receiver.Cell);yield return null;Select(barrier);
                var barrierHealth=barrier.Health;var barrierStock=stock.CaptureState();
                Require(!hud.SelectedBuildingView.ShowUpgrade,"barricade has no upgrade button");
                Require(binding.TryUpgradeSelectedBuilding()==Stage2TowerActionResult.MaximumLevel&&barrier.Level==1&&!barrier.CanAttack&&
                    barrier.Health==barrierHealth&&barrierStock.Equals(stock.CaptureState()),"barricade upgrade rejected without mutation");
                yield return Shot(directory,"10-barricade");
                if(Environment.GetEnvironmentVariable("NOT3A_BUILDING_ACTIONS_NATIVE_CLICK")=="1")
                {
                    var native=Build(0,receiver.Cell);yield return null;
                    var deadline=Time.realtimeSinceStartup+120;
                    if(constructionReview)
                    {
                        var buddy=Build(0,native.Cell);yield return null;
                        Frame(Body(native).bounds.center);binding.ClearBuildingSelection();
                        var nativeCount=defense.TowerCount;var nativeStock=stock.CaptureState();
                        void WritePoints(string phase,Vector2? target=null)
                        {
                            var start=Point(native);var end=target??Point(buddy);
                            File.WriteAllText(Path.Combine(directory,"native-points.txt"),
                                $"{phase} game={Screen.width}x{Screen.height} sourceGUI=({start.x:0.0},{Screen.height-start.y:0.0}) targetGUI=({end.x:0.0},{Screen.height-end.y:0.0})\n");
                        }
                        yield return Shot(directory,"11a-native-card-ready");Debug.Log("S3_BUILDING_ACTIONS_NATIVE_CARD_READY");
                        deadline=Time.realtimeSinceStartup+120;
                        while(!defense.IsPlacing&&Time.realtimeSinceStartup<deadline)yield return null;
                        Require(defense.IsPlacing&&defense.TowerCount==nativeCount,"one native catalogue card click arms placement without purchase");
                        WritePoints("select");Debug.Log("S3_BUILDING_ACTIONS_NATIVE_SELECT_READY");deadline=Time.realtimeSinceStartup+120;
                        while((!hud.HasSelectedBuilding||defense.IsPlacing)&&Time.realtimeSinceStartup<deadline)yield return null;
                        Require(hud.HasSelectedBuilding&&!defense.IsPlacing&&hud.ActiveWindow==Stage3PlayerWindow.Construction,
                            "native owned-building selection cancels draft and displays strip with B open");
                        var afterSelection=stock.CaptureState();
                        Require(nativeStock.Equals(afterSelection)&&defense.TowerCount==nativeCount,"native selection never buys under silhouette");
                        yield return Shot(directory,"11b-native-selected");
                        var moveCell=EmptyCell(native);var movePoint=(Vector2)camera.WorldToScreenPoint(scene.MapDefinition.CellToWorld(moveCell));
                        WritePoints("move",movePoint);Debug.Log("S3_BUILDING_ACTIONS_NATIVE_MOVE_READY");deadline=Time.realtimeSinceStartup+120;
                        while(native.Cell!=moveCell&&Time.realtimeSinceStartup<deadline)yield return null;
                        Require(native.Cell==moveCell&&!binding.IsDraggingBuilding&&hud.ActiveWindow==Stage3PlayerWindow.Construction,
                            "native world drag moves tower while B remains open");
                        yield return Shot(directory,"11c-native-moved");
                        var rejectedCell=native.Cell;var buddyCell=buddy.Cell;
                        var nativeHealth=native.Health;var buddyHealth=buddy.Health;var rejectedStock=stock.CaptureState();
                        WritePoints("occupied-drop");Debug.Log("S3_BUILDING_ACTIONS_NATIVE_OCCUPIED_DROP_READY");deadline=Time.realtimeSinceStartup+120;
                        while(!binding.IsDraggingBuilding&&Time.realtimeSinceStartup<deadline)yield return null;
                        Require(binding.IsDraggingBuilding,"native occupied drag begins");
                        while(binding.IsDraggingBuilding&&Time.realtimeSinceStartup<deadline)yield return null;
                        Require(!binding.IsDraggingBuilding&&native.Level==1&&buddy.Level==1&&defense.TowerCount==nativeCount&&
                            native.Cell==rejectedCell&&buddy.Cell==buddyCell&&native.Health==nativeHealth&&buddy.Health==buddyHealth&&
                            rejectedStock.Equals(stock.CaptureState()),"native occupied drop leaves both towers and stock unchanged");
                        Require(hud.HasSelectedBuilding,"rejected native drop retains source selection");
                        yield return Shot(directory,"11d-native-rejected");
                    }
                    else Select(native);
                    var initialLevel=native.Level;
                    yield return Shot(directory,"11-native-upgrade-ready");Debug.Log("S3_BUILDING_ACTIONS_NATIVE_UPGRADE_READY");
                    deadline=Time.realtimeSinceStartup+120;
                    while(native.Level==initialLevel&&Time.realtimeSinceStartup<deadline)yield return null;
                    Require(native.Level==initialLevel+1,"native upgrade button changes actual tower level");yield return Shot(directory,"12-native-upgraded");
                    var saleRefund=defense.GetSaleRefund(native);var saleBefore=stock.Get(Stage2.Map.MapResourceKind.Wood);
                    Debug.Log("S3_BUILDING_ACTIONS_NATIVE_SELL_READY");deadline=Time.realtimeSinceStartup+120;
                    while(native!=null&&native.IsAlive&&Time.realtimeSinceStartup<deadline)yield return null;
                    Require(defense.GetTowerCatalog(native)==null&&!hud.HasSelectedBuilding,"native sale removes tower and selection");
                    Require(stock.Get(Stage2.Map.MapResourceKind.Wood)==saleBefore+saleRefund.Wood,"native sale pays exact refund");
                    yield return Shot(directory,"13-native-sold");
                }
                File.WriteAllText(Path.Combine(directory,"verification.txt"),report.ToString());Debug.Log("S3_BUILDING_ACTIONS_REVIEW_OK "+directory);
            }
            finally
            {
                game.SetPaused(false);game.RunDriver.ResetRun();binding.ClearBuildingSelection();hud.CloseWindow();
                camera.transform.position=previousPosition;camera.orthographicSize=previousSize;
                scene.CameraFollow.enabled=previousFollow;game.HeroCombat.AutomaticTick=previousCombat;Time.captureFramerate=previousRate;
            }
        }
        private static IEnumerator Shot(string directory,string name,Action beforeRender=null)
        {
            for(var i=0;i<4;i++)yield return null;beforeRender?.Invoke();yield return new WaitForEndOfFrame();
            var frame=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(directory,name+".png"),frame.EncodeToPNG());
            UnityEngine.Object.Destroy(frame);yield return null;
        }
    }
}
