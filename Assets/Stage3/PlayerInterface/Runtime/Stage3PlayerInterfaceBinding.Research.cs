using System;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage3WorldTravel blueprintTravel;
        private int selectedResearchKind=-1,researchStamp;
        private bool researchDirty=true;
        public Stage2Blueprints Blueprints=>equipmentDriver?.Rules.Blueprints;
        public Stage2BlueprintResult LastResearchResult {get;private set;}
        private void BindBlueprintResearch()
        {
            if(equipmentDriver==null||!game.RunDriver.Run.UsesDaytimeGates)return;
            var catalog=Resources.Load<Stage2BlueprintCatalog>("Stage3Blueprints");
            if(catalog==null||!catalog.TryValidate(out _))
            {Debug.LogError("S3_BLUEPRINT_BINDING_ERROR: valid Stage3Blueprints data required.");return;}
            equipmentDriver.Rules.EnableBlueprintResearch(catalog);
            blueprintTravel=GetComponent<Stage3WorldTravel>();
            Blueprints.Changed+=BlueprintsChanged;
            if(blueprintTravel!=null)
            {blueprintTravel.WorkshopOpenRequested+=OpenWorkshopResearch;blueprintTravel.OpeningsChanged+=BlueprintsChanged;}
            presentation.ResearchSelected+=SelectResearch;
            presentation.ResearchStudyRequested+=StudyResearch;
            presentation.BlueprintResearchRequested+=OpenResearchForBlueprint;
            game.Building.SetRoleAvailability(role=>role!=1||Blueprints.IsStudied(Stage2BlueprintKind.Longshot));
            BlueprintsChanged();
            Debug.Log("S3_BLUEPRINTS_READY:4 gold-traded documents, local single-use study, permanent knowledge; single-level workshop.");
        }
        private void BlueprintsChanged()
        {researchDirty=true;MarkEquipmentDirty();MarkMerchantDirty();InvalidateBuildingSelection();}
        private void SelectResearch(int kind)
        {if(kind< -1||kind>=4)return;selectedResearchKind=kind;researchDirty=true;RefreshResearch();}
        private void StudyResearch(int kind)=>TryStudyResearch(kind);
        public void OpenResearchForBlueprint(int kind)
        {
            if(!CanManageEquipment||Blueprints==null||kind<0||kind>=4)return;
            selectedResearchKind=kind;OpenResearchWindow();
        }
        private void OpenWorkshopResearch()
        {
            if(!CanManageEquipment||Blueprints==null||!(blueprintTravel?.CanStudyAtWorkshop??false))return;
            selectedResearchKind=!blueprintTravel.IsResearched?-1:Blueprints.Items.Count>0?(int)Blueprints.Items[0].Kind:-1;OpenResearchWindow();
        }
        private void OpenResearchWindow()
        {
            ClearBuildingSelection();CancelBlink();CancelPlacement();game.Gathering.CancelGatherUntilRelease();
            presentation.SetWindow(Stage3PlayerWindow.Research);researchDirty=true;RefreshResearch();
        }
        private bool ResearchModeAllowed=>CanManageEquipment&&isActiveAndEnabled&&Time.timeScale>0f&&!IsDraggingBuilding&&
            presentation.ActiveWindow==Stage3PlayerWindow.Research&&(blueprintTravel?.CanStudyAtWorkshop??false);
        public bool TryStudyResearch(int kind)
        {
            if(Blueprints==null||kind!=selectedResearchKind||kind< -1||kind>=4||!ResearchModeAllowed)
            {LastResearchResult=Stage2BlueprintResult.Unavailable;researchDirty=true;return false;}
            if(kind== -1)
                LastResearchResult=blueprintTravel.TryStudyTeleport()?Stage2BlueprintResult.Ready:
                    blueprintTravel.IsResearched?Stage2BlueprintResult.AlreadyStudied:Stage2BlueprintResult.Unavailable;
            else
            {
                var key=(Stage2BlueprintKind)kind;var item=Blueprints.Find(key);
                LastResearchResult=Blueprints.IsStudied(key)?Stage2BlueprintResult.AlreadyStudied:
                    item==null?Stage2BlueprintResult.InvalidItem:Blueprints.TryStudy(item.Id);
            }
            BlueprintsChanged();RefreshResearch();return LastResearchResult==Stage2BlueprintResult.Ready;
        }
        private void UpdateBlueprintResearch()
        {
            if(Blueprints==null)return;
            var stamp=HashCode.Combine(presentation.ActiveWindow,CanManageEquipment,blueprintTravel?.WorkshopBuilt??false,
                blueprintTravel?.NearWorkshop??false,ResearchModeAllowed,blueprintTravel?.IsResearched??false,blueprintTravel?.RemoteDiscovered??false);
            if(stamp!=researchStamp){researchStamp=stamp;researchDirty=true;}
            if(researchDirty)RefreshResearch();
        }
        private string ResearchReason(Stage2BlueprintKind? kind)
        {
            if(kind.HasValue&&Blueprints.IsStudied(kind.Value)||!kind.HasValue&&(blueprintTravel?.IsResearched??false))return "Изучено";
            if(!(blueprintTravel?.WorkshopBuilt??false))return "Сначала постройте исследовательскую мастерскую";
            if(!(blueprintTravel?.NearWorkshop??false))return "Подойдите к исследовательской мастерской";
            if(!ResearchModeAllowed)return "Исследование сейчас недоступно";
            if(!kind.HasValue)return null;
            var paper=Blueprints.Find(kind.Value);
            if(paper==null)return "Нет чертежа · приобретите его у торговца";
            return BlueprintReason(Blueprints.EvaluateStudy(paper.Id));
        }
        public static string BlueprintReason(Stage2BlueprintResult result)=>result switch
        {
            Stage2BlueprintResult.Ready=>null,
            Stage2BlueprintResult.AlreadyStudied=>"Изучено",
            Stage2BlueprintResult.InvalidItem=>"Нет чертежа · приобретите его у торговца",
            Stage2BlueprintResult.PreviousStudyRequired=>"Сначала изучите чертёж кузницы II",
            _=>"Исследование сейчас недоступно"
        };
        private void RefreshResearch()
        {
            if(Blueprints==null)return;researchDirty=false;
            var entries=new Stage3ResearchEntryView[Blueprints.Catalog.Count];
            for(var i=0;i<entries.Length;i++)
            {
                var kind=(Stage2BlueprintKind)i;var paper=Blueprints.Find(kind);var reason=ResearchReason(kind);
                entries[i]=new Stage3ResearchEntryView{KindIndex=i,Item=BlueprintItemView(kind,paper?.Id??-1),Owned=paper!=null,
                    Studied=Blueprints.IsStudied(kind),CanStudy=reason==null,Reason=reason};
            }
            var selectedReason=ResearchReason(selectedResearchKind<0?(Stage2BlueprintKind?)null:(Stage2BlueprintKind)selectedResearchKind);
            presentation.SetResearch(new Stage3ResearchView{Entries=entries,SelectedKind=selectedResearchKind,
                WorkshopBuilt=blueprintTravel?.WorkshopBuilt??false,NearWorkshop=blueprintTravel?.NearWorkshop??false,
                TeleportStudied=blueprintTravel?.IsResearched??false,TeleportDiscovered=blueprintTravel?.RemoteDiscovered??false,
                CanStudy=selectedReason==null,Reason=selectedReason});
        }
        private Stage3EquipmentItemView BlueprintItemView(Stage2BlueprintItem paper)=>BlueprintItemView(paper.Kind,paper.Id);
        private Stage3EquipmentItemView BlueprintItemView(Stage2BlueprintKind kind,int id)
        {
            var data=Blueprints.Catalog.Get(kind);
            return new Stage3EquipmentItemView{Id=id,Name=data.displayName,CanEquip=false,
                Blueprint=new Stage3BlueprintItemView{KindIndex=(int)kind,ArtIndex=data.ArtIndex,TargetStage=data.TargetStage,
                    ShortLabel=data.shortLabel,Description=data.result,CategoryLabel=data.Category==Stage2BlueprintCategory.Opening?"Открытие нового":
                    "Улучшение · целевая ступень "+(data.TargetStage==2?"II":"III"),Studied=Blueprints.IsStudied(kind)}};
        }
        private void UnbindBlueprintResearch()
        {
            if(Blueprints==null)return;
            Blueprints.Changed-=BlueprintsChanged;
            if(blueprintTravel!=null)
            {blueprintTravel.WorkshopOpenRequested-=OpenWorkshopResearch;blueprintTravel.OpeningsChanged-=BlueprintsChanged;}
            presentation.ResearchSelected-=SelectResearch;presentation.ResearchStudyRequested-=StudyResearch;
            presentation.BlueprintResearchRequested-=OpenResearchForBlueprint;presentation.SetResearch(null);
            game.Building.SetRoleAvailability(null);
        }
    }
}
