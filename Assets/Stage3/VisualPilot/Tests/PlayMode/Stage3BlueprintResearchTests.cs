#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Economy;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Tests
{
    public sealed class Stage3BlueprintResearchTests
    {
        private InputTestFixture input;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2EquipmentDriver equipment;
        private Component binding,hud,travel;
        private Stage2Merchant merchant;
        private Stage2Blueprints Rules=>equipment.Rules.Blueprints;
        private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).Invoke(owner,args);
        private static T Read<T>(object owner,string name)=>(T)owner.GetType().GetProperty(name).GetValue(owner);
        private static T Field<T>(object owner,string name)=>(T)owner.GetType().GetField(name).GetValue(owner);
        private void Window(string name)=>Call(hud,"SetWindow",Enum.Parse(hud.GetType().GetProperty("ActiveWindow").PropertyType,name));
        private bool Study(int kind)=>(bool)Call(binding,"TryStudyResearch",kind);
        private Stage2BlueprintItem Buy(Stage2BlueprintKind kind)
        {
            var paper=merchant.BlueprintItems.Single(p=>p.Kind==kind);
            Assert.That(merchant.TryTradeBlueprint(true,paper.Id),Is.EqualTo(Stage2TradeResult.Ready));return paper;
        }
        [UnitySetUp] public IEnumerator Load()
        {
            Time.timeScale=1;input=new InputTestFixture();input.Setup();InputSystem.AddDevice<Keyboard>();InputSystem.AddDevice<Mouse>();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Stage3Rapid3DGamePilot.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(var i=0;i<8;i++)yield return null;
            game=Object.FindAnyObjectByType<Stage2GameComposition>();scene=game.GetComponent<Stage2SceneBootstrap>();
            binding=game.GetComponent("Stage3PlayerInterfaceBinding");hud=game.GetComponent("Stage3FantasyHud");travel=game.GetComponent("Stage3WorldTravel");
            equipment=game.GetComponent<Stage2EquipmentDriver>();merchant=Read<Stage2Merchant>(binding,"Merchant");
            Assert.That(Rules,Is.Not.Null);game.RunDriver.enabled=false;game.HeroCombat.AutomaticTick=false;game.RunDriver.ResetRun();
            game.Gathering.Economy.Inventory.TryAddGold(10000);yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;yield return null;input.TearDown();Time.timeScale=1;
        }
        private IEnumerator BuildWorkshop()
        {
            Window("None");Stage3TestCycle.FundBaseConstruction(game);Call(travel,"BeginWorkshopPlacement");var origin=(Vector2)scene.PlayerMover.transform.position;Vector2? site=null;
            for(var ring=5;ring<70&&!site.HasValue;ring+=2)for(var x=-ring;x<=ring&&!site.HasValue;x+=2)for(var y=-ring;y<=ring;y+=2)
            {var p=origin+new Vector2(x,y);if(Call(travel,"EvaluateWorkshopAt",p).ToString()=="Ready"){site=p;break;}}
            Assert.That(site.HasValue,Is.True,"A legal workshop site must exist in the saved real scene.");
            Assert.That(Call(travel,"TryBuildWorkshopAt",site.Value).ToString(),Is.EqualTo("Built"));
            scene.PlayerMover.transform.position=Read<Vector2>(travel,"WorkshopInteractionPosition");yield return null;yield return null;
            Assert.That(Read<bool>(travel,"NearWorkshop"),Is.True);
        }
        [UnityTest] public IEnumerator ConstructionPagingRoutesVisibleChoicesToTheirActualRolesAndCancelsDrafts()
        {
            var rapid=game.Building.Defense.GetCatalog(0);
            // Eight ephemeral roles exercise pagination without adding persistent game content.
            Assert.That(game.Building.Configure(scene.MapDefinition,rapid,
                Enumerable.Repeat(rapid,7).ToArray(),game.Gathering,scene.PlayerMover.transform,
                scene.CameraFollow.BoundCamera,game.Navigation),Is.True);
            game.Building.SetRoleAvailability(_=>true);
            game.Gathering.Economy.Inventory.TryCredit(new Stage2ResourceCost(100,100,100));
            Window("Construction");Call(hud,"SetBuildGroup",0);yield return null;
            Assert.That(Read<int>(hud,"BuildCardCount"),Is.EqualTo(8));
            Assert.That(Read<int>(hud,"VisibleBuildCardCount"),Is.EqualTo(3));
            Assert.That(Read<int>(hud,"BuildPageCount"),Is.EqualTo(3));
            Assert.That((bool)Call(hud,"ActivateBuildChoice",2),Is.True);
            Assert.That(game.Building.Defense.SelectedRoleIndex,Is.EqualTo(2));
            Assert.That(game.Building.Defense.IsPlacing,Is.True);
            input.Press(Keyboard.current.rightArrowKey);yield return null;input.Release(Keyboard.current.rightArrowKey);yield return null;
            Assert.That(Read<int>(hud,"BuildPage"),Is.EqualTo(1),"Right arrow advances the visible build page.");
            Assert.That(game.Building.Defense.IsPlacing,Is.False,"Changing page cancels the former blueprint draft.");
            Assert.That(Read<int>(hud,"VisibleBuildCardCount"),Is.EqualTo(3));
            Assert.That((Rect)Call(hud,"BuildChoiceRect",3),Is.EqualTo(Rect.zero));
            Call(hud,"SetBuildPage",2);
            Assert.That(Read<int>(hud,"VisibleBuildCardCount"),Is.EqualTo(2));
            Assert.That((bool)Call(hud,"ActivateBuildChoice",1),Is.True);
            Assert.That(game.Building.Defense.SelectedRoleIndex,Is.EqualTo(7));
            input.Press(Keyboard.current.digit1Key);yield return null;yield return null;
            input.Release(Keyboard.current.digit1Key);yield return null;
            Assert.That(game.Building.Defense.SelectedRoleIndex,Is.EqualTo(6),"Digit1 targets the first card of the current page.");
            Call(hud,"SetBuildPage",99);Assert.That(Read<int>(hud,"BuildPage"),Is.EqualTo(2));
            Call(hud,"SetBuildPage",-1);Assert.That(Read<int>(hud,"BuildPage"),Is.Zero);
            Assert.That(game.Building.Defense.IsPlacing,Is.False);
            var content=Read<Rect>(hud,"ConstructionContentRect");
            input.Set(Mouse.current.position,new Vector2(content.center.x,Screen.height-content.center.y));
            input.Set(Mouse.current.scroll,new Vector2(0,-120));yield return null;
            input.Set(Mouse.current.scroll,Vector2.zero);yield return null;
            Assert.That(Read<int>(hud,"BuildPage"),Is.EqualTo(1),"Wheel over the catalogue advances the page.");
            Call(hud,"SetBuildPage",1);game.RunDriver.ResetRun();yield return null;
            Assert.That(Read<int>(hud,"BuildPage"),Is.Zero,"Reset returns construction to its first page.");
            Assert.That(Read<int>(hud,"VisibleBuildCardCount"),Is.EqualTo(3));
        }

        [UnityTest] public IEnumerator ConstructionShowsOnlyStudiedCardsWithDenseIndicesAndResetHidesThemAgain()
        {
            int Count(int group)
            {
                Call(hud,"SetBuildGroup",group);
                return Read<int>(hud,"VisibleBuildCardCount");
            }
            string Name(int index)
            {
                var cards=(System.Collections.IList)Call(hud,"GetBuildCards");
                return Field<string>(cards[index],"Name");
            }
            Window("Construction");
            Assert.That(Count(0),Is.EqualTo(2));Assert.That(Name(1),Is.EqualTo("Частокол"));
            Assert.That(Read<int>(hud,"BuildPageCount"),Is.EqualTo(1));
            Call(hud,"SetBuildPage",99);Assert.That(Read<int>(hud,"BuildPage"),Is.Zero);
            Assert.That((Rect)Call(hud,"BuildChoiceRect",2),Is.EqualTo(Rect.zero));
            Assert.That(Count(1),Is.Zero);Assert.That(Count(2),Is.EqualTo(2));
            foreach(var kind in new[]{Stage2BlueprintKind.Longshot,Stage2BlueprintKind.Sawmill,
                Stage2BlueprintKind.Quarry,Stage2BlueprintKind.IronMine,Stage2BlueprintKind.ForgeConstruction})Buy(kind);
            Assert.That(Count(0),Is.EqualTo(2));Assert.That(Count(1),Is.Zero);Assert.That(Count(2),Is.EqualTo(2),
                "Purchasing documents does not expose construction cards.");
            yield return BuildWorkshop();
            foreach(var kind in new[]{Stage2BlueprintKind.Longshot,Stage2BlueprintKind.Sawmill,
                Stage2BlueprintKind.Quarry,Stage2BlueprintKind.IronMine,Stage2BlueprintKind.ForgeConstruction})
            {
                Call(binding,"OpenResearchForBlueprint",(int)kind);yield return null;
                Assert.That(Study((int)kind),Is.True);
            }
            Window("Construction");
            Assert.That(Count(0),Is.EqualTo(3));Assert.That(Name(1),Is.EqualTo(game.Building.Defense.GetCatalog(1).RoleName));
            Assert.That(Count(1),Is.EqualTo(3));Assert.That(Count(2),Is.EqualTo(3));
            game.Gathering.Economy.Inventory.Reset();
            Assert.That(Count(1),Is.EqualTo(3),"Known but unaffordable buildings remain visible.");
            Assert.That((bool)Call(hud,"ActivateBuildChoice",0),Is.False);
            game.RunDriver.ResetRun();yield return null;Window("Construction");
            Assert.That(Count(0),Is.EqualTo(2));Assert.That(Count(1),Is.Zero);Assert.That(Count(2),Is.EqualTo(2));
            game.Gathering.Economy.Inventory.TryCredit(new Stage2ResourceCost(100,100,100));
            Count(0);
            input.Press(Keyboard.current.digit2Key);yield return null;yield return null;
            input.Release(Keyboard.current.digit2Key);
            Assert.That(game.Building.Defense.IsPlacing,Is.True);
            Assert.That(game.Building.Defense.SelectedRoleIndex,Is.EqualTo(2),"Digit2 selects the second visible card, not hidden Longshot.");
            Assert.That(Read<object>(hud,"ActiveWindow").ToString(),Is.EqualTo("Construction"));
        }

        [UnityTest] public IEnumerator ConsumedDocumentLeavesBagAndKnowledgeOpensActualDefenseRole()
        {
            var paper=Buy(Stage2BlueprintKind.Longshot);var defense=game.Building.Defense;
            Assert.That(defense.IsRoleAvailable(1),Is.False);Assert.That(defense.SelectRole(1),Is.False);
            yield return BuildWorkshop();Call(binding,"OpenResearchForBlueprint",0);yield return null;
            Assert.That(Read<object>(hud,"ActiveWindow").ToString(),Is.EqualTo("Research"));
            Assert.That(Study(0),Is.True);Assert.That(Rules.Find(paper.Id),Is.Null);Assert.That(Rules.IsStudied(paper.Kind),Is.True);
            Assert.That(defense.IsRoleAvailable(1),Is.True);Assert.That(Study(0),Is.False);
            Window("Inventory");yield return null;
            var bag=Field<Array>(Read<object>(hud,"EquipmentView"),"BagItems").Cast<object>().ToArray();
            Assert.That(bag.Any(row=>Field<int>(row,"Id")==paper.Id),Is.False);
            var entry=Field<Array>(Read<object>(hud,"ResearchView"),"Entries").Cast<object>().Single(e=>Field<int>(e,"KindIndex")==0);
            Assert.That(Field<bool>(entry,"Studied"),Is.True);Assert.That(Field<bool>(entry,"Owned"),Is.False);
        }
        [UnityTest] public IEnumerator ProductionAndForgeRequireTheirOwnBoughtAndLocallyStudiedConstructionPaper()
        {
            var production = game.Production; var wallet = game.Gathering.Economy.Inventory;
            foreach (MapResourceKind material in Enum.GetValues(typeof(MapResourceKind))) wallet.TryAdd(material, 1000);
            var forge = game.GetComponent("Stage3Forge");
            foreach (ResourceProductionBuilding building in Enum.GetValues(typeof(ResourceProductionBuilding)))
            {
                var index = Enumerable.Range(0, production.Rules.Points.Count).First(i => production.Rules.Points[i].Building == building);
                Assert.That(production.CanBeginPlacement(building), Is.False);
                Assert.That(production.CanBeginPlacement((ResourceProductionBuilding)99), Is.False);
                Assert.That(production.Rules.Evaluate(index, (ResourceProductionBuilding)99, Vector2.zero, .2f),
                    Is.EqualTo(ProductionPlacementResult.WrongType), "Unknown kinds reject without invoking the knowledge mapping.");
                var prepared = false;
                Assert.That(production.Rules.TryBuild(index, building, Vector2.zero, .2f, () => { prepared = true; return true; }, out _),
                    Is.EqualTo(ProductionPlacementResult.ResearchRequired));
                Assert.That(prepared, Is.False, "Locked rules must reject before visual creation/payment.");
            }
            Assert.That(Read<bool>(forge, "ConstructionUnlocked"), Is.False);
            Assert.That(Call(forge, "TryBuildAt", Vector2.zero).ToString(), Is.EqualTo("ResearchRequired"));
            yield return BuildWorkshop();
            foreach (var kind in new[] { Stage2BlueprintKind.Sawmill, Stage2BlueprintKind.Quarry, Stage2BlueprintKind.IronMine, Stage2BlueprintKind.ForgeConstruction })
            {
                var paper = Buy(kind); Assert.That(Rules.IsStudied(kind), Is.False, "Buying alone does not unlock construction.");
                Assert.That(Rules.Catalog.Get(kind).Category, Is.EqualTo(Stage2BlueprintCategory.Opening));
                Call(binding, "OpenResearchForBlueprint", (int)kind); yield return null;
                Assert.That(Study((int)kind), Is.True); Assert.That(Rules.Find(paper.Id), Is.Null);
                if (kind != Stage2BlueprintKind.ForgeConstruction)
                {
                    var building = (ResourceProductionBuilding)((int)kind - (int)Stage2BlueprintKind.Sawmill);
                    Assert.That(production.Rules.IsBuildingAvailable(building), Is.True);
                    Assert.That(production.Rules.Buildings.Count, Is.Zero, "Study never grants a free building.");
                }
                else Assert.That(Read<bool>(forge, "ConstructionUnlocked"), Is.True);
            }
            Window("None"); Assert.That(Read<bool>(forge, "CanBeginPlacement"), Is.True);
            Assert.That(Read<bool>(forge, "IsBuilt"), Is.False); Assert.That(equipment.Rules.ForgeLevel, Is.Zero);
            foreach (ResourceProductionBuilding building in Enum.GetValues(typeof(ResourceProductionBuilding)))
                Assert.That(production.CanBeginPlacement(building), Is.True);
            game.RunDriver.ResetRun(); yield return null;
            Assert.That(Read<bool>(forge, "ConstructionUnlocked"), Is.False);
            foreach (ResourceProductionBuilding building in Enum.GetValues(typeof(ResourceProductionBuilding)))
                Assert.That(production.Rules.IsBuildingAvailable(building), Is.False);
        }
        [UnityTest] public IEnumerator StudyRequiresLocalWorkshopSelectedDocumentAndLiveResearchWindow()
        {
            var paper=Buy(Stage2BlueprintKind.WandRecipe);Call(binding,"OpenResearchForBlueprint",1);yield return null;
            Assert.That(Study(1),Is.False);Assert.That(Rules.Find(paper.Id),Is.SameAs(paper));
            yield return BuildWorkshop();Call(binding,"OpenResearchForBlueprint",1);yield return null;
            Assert.That(Study(0),Is.False,"A stale/different selected kind cannot spend a paper.");
            Window("Inventory");Assert.That(Study(1),Is.False);Window("Research");
            scene.PlayerMover.transform.position=Read<Vector2>(travel,"WorkshopInteractionPosition")+Vector2.down*20;yield return null;
            Assert.That(Study(1),Is.False);scene.PlayerMover.transform.position=Read<Vector2>(travel,"WorkshopInteractionPosition");yield return null;
            game.SetPaused(true);Assert.That(Study(1),Is.False);game.SetPaused(false);Window("Research");yield return null;
            Assert.That(Rules.Find(paper.Id),Is.SameAs(paper));Assert.That(Study(1),Is.True);
        }
        [UnityTest] public IEnumerator ForgeOrderRejectsWithoutConsumptionAndLocalEOpensResearchInsteadOfAutoStudy()
        {
            var third=Buy(Stage2BlueprintKind.ForgeIII);var second=Buy(Stage2BlueprintKind.ForgeII);
            yield return BuildWorkshop();Window("None");Assert.That(Call(travel,"TryInteract"),Is.EqualTo(true));yield return null;
            Assert.That(Read<object>(hud,"ActiveWindow").ToString(),Is.EqualTo("Research"));Assert.That(Read<bool>(travel,"IsResearched"),Is.False);
            Call(binding,"SelectResearch",3);Assert.That(Study(3),Is.False);Assert.That(Rules.Find(third.Id),Is.SameAs(third));
            Call(binding,"SelectResearch",2);Assert.That(Study(2),Is.True);Assert.That(Rules.Find(second.Id),Is.Null);
            Call(binding,"SelectResearch",3);Assert.That(Study(3),Is.True);Assert.That(equipment.Rules.ForgeLevel,Is.Zero);
        }
        [UnityTest] public IEnumerator DefeatRetryPreservesKnowledgeCurrentPapersAndMerchantStockButFullResetClearsThem()
        {
            var learned=Buy(Stage2BlueprintKind.Longshot);var retained=Buy(Stage2BlueprintKind.ForgeII);
            yield return BuildWorkshop();Call(binding,"OpenResearchForBlueprint",0);Assert.That(Study(0),Is.True);Window("None");
            var gold=game.Gathering.Economy.Inventory.Gold;var run=game.RunDriver.Run;Assert.That(Stage3TestCycle.StartNight(run),Is.True);
            for(var i=0;i<10&&!run.IsTerminal;i++){var enemy=game.Enemies.SpawnAt(scene.MapDefinition.Throne);enemy.SetAutomaticTick(false);enemy.Tick(.01f);}
            Assert.That(run.Phase,Is.EqualTo(Stage2RunPhase.Lost));Assert.That(run.RetryFailedWave(),Is.True);yield return null;
            Assert.That(Rules.IsStudied(learned.Kind),Is.True);Assert.That(Rules.Find(retained.Id),Is.SameAs(retained));
            Assert.That(merchant.BlueprintItems.Count,Is.EqualTo(Rules.Catalog.Count-2));Assert.That(game.Building.Defense.IsRoleAvailable(1),Is.True);
            Assert.That(game.Gathering.Economy.Inventory.Gold,Is.EqualTo(gold));
            game.RunDriver.ResetRun();yield return null;
            Assert.That(Rules.StudiedCount,Is.Zero);Assert.That(Rules.Items,Is.Empty);Assert.That(merchant.BlueprintItems.Count,Is.EqualTo(Rules.Catalog.Count));
            Assert.That(game.Building.Defense.IsRoleAvailable(1),Is.False);Assert.That(Read<bool>(travel,"WorkshopBuilt"),Is.False);
        }
    }
}
#endif
