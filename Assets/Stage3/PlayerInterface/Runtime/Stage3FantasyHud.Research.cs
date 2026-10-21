using System;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed class Stage3ResearchEntryView
    {
        public int KindIndex;
        public Stage3EquipmentItemView Item;
        public bool Owned, Studied, CanStudy;
        public string Reason;
    }

    public sealed class Stage3ResearchView
    {
        public Stage3ResearchEntryView[] Entries = Array.Empty<Stage3ResearchEntryView>();
        public int SelectedKind = -1;
        public bool CanStudy, NearWorkshop, WorkshopBuilt, TeleportStudied, TeleportDiscovered;
        public string Reason;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3ResearchView research;
        private bool researchShowStudied;
        public bool ResearchShowingStudied => researchShowStudied;
        public Stage3ResearchView ResearchView => research;
        public event Action<int> ResearchSelected;
        public event Action<int> ResearchStudyRequested;
        public void SetResearch(Stage3ResearchView view)
        {
            var oldKind=research?.SelectedKind??-1;
            var wasStudied=SelectedResearchEntry?.Studied==true;
            research=view;
            if(view==null){researchShowStudied=false;return;}
            var selected=SelectedResearchEntry;
            if(selected!=null&&(oldKind!=view.SelectedKind||(!wasStudied&&selected.Studied)))
                researchShowStudied=selected.Studied;
        }

        private bool ResearchEntryVisible(Stage3ResearchEntryView entry) =>
            entry?.Item?.Blueprint!=null&&(researchShowStudied?entry.Studied:entry.Owned&&!entry.Studied);

        public void SelectResearchFilter(bool studied)
        {
            researchShowStudied=studied;
            if(ResearchEntryVisible(SelectedResearchEntry))return;
            if(research?.Entries!=null)foreach(var entry in research.Entries)
                if(ResearchEntryVisible(entry)){SelectResearchEntry(entry.KindIndex);return;}
            SelectResearchEntry(-1);
        }

        public void SelectResearchEntry(int kind)
        {
            if(kind==-1){ResearchSelected?.Invoke(-1);return;}
            if(research?.Entries!=null)foreach(var entry in research.Entries)
                if(entry!=null&&entry.KindIndex==kind){ResearchSelected?.Invoke(kind);return;}
        }

        private Stage3WorldTravel researchTravel;
        public string ResearchStatusText
        {
            get
            {
                if (researchTravel == null && game != null) researchTravel=game.GetComponent<Stage3WorldTravel>();
                return researchTravel == null || !researchTravel.IsReady ? "Недоступно" : researchTravel.IsResearched ? "Изучено" :
                    researchTravel.WorkshopBuilt ? "Доступно у мастерской" : "Нужна мастерская";
            }
        }

        private Rect ResearchPanelRect(int column)
        {
            var r=WindowContentRect;var s=UiScale;var gap=20f*s;
            var usable=r.width-2f*gap;var left=usable*.35f;var center=usable*.29f;
            var x=column==0?r.x:column==1?r.x+left+gap:r.x+left+center+2f*gap;
            return new Rect(x,r.y+62f*s,column==0?left:column==1?center:usable-left-center,r.height-62f*s);
        }

        private Rect ResearchGridCell(int index)
        {
            var r=Inset(ResearchPanelRect(0),24f*UiScale);var s=UiScale;
            var cell=Mathf.Min((r.width-3f*12f*s)/4f,(r.height-210f*s)/3f-21f*s);
            var pitchX=(r.width-cell)/3f;
            return new Rect(r.x+(index%4)*pitchX,r.y+92f*s+(index/4)*(cell+21f*s),cell,cell);
        }

        private Rect ResearchTeleportRect
        {
            get {var r=Inset(ResearchPanelRect(0),24f*UiScale);return new Rect(r.x,r.yMax-76f*UiScale,r.width,76f*UiScale);}
        }

        public Rect ResearchFilterRect(bool studied)
        {
            var r=Inset(ResearchPanelRect(0),24f*UiScale);
            return new Rect(r.x+(studied?r.width*.5f:0),r.y+42f*UiScale,r.width*.5f,36f*UiScale);
        }

        public Rect ResearchEntryRect(int kind)
        {
            if(ActiveWindow!=Stage3PlayerWindow.Research)return Rect.zero;
            if(kind==-1)return ResearchTeleportRect;
            var index=0;
            if(research?.Entries!=null)foreach(var entry in research.Entries)
            {
                if(!ResearchEntryVisible(entry))continue;
                if(index>=12)break;
                if(entry.KindIndex==kind)return ResearchGridCell(index);
                index++;
            }
            return Rect.zero;
        }

        public Rect ResearchStudyRect
        {
            get {var r=Inset(ResearchPanelRect(2),30f*UiScale);return new Rect(r.x,r.yMax-85f*UiScale,r.width,64f*UiScale);}
        }

        private Stage3ResearchEntryView SelectedResearchEntry
        {
            get
            {
                if(research?.Entries!=null)foreach(var entry in research.Entries)
                    if(entry!=null&&entry.KindIndex==research.SelectedKind)return entry;
                return null;
            }
        }

        private void DrawResearch(Rect content)
        {
            var s=UiScale;
            Label(new Rect(content.x,content.y,content.width,46f*s),"Исследовательская мастерская",34);
            for(var i=0;i<3;i++)DrawSliced(ResearchPanelRect(i),art.windowFrame,false,.13f*s);
            DrawResearchCatalogue();
            DrawResearchPreview();
            DrawResearchConditions();
        }

        private void DrawResearchCatalogue()
        {
            var s=UiScale;var inner=Inset(ResearchPanelRect(0),24f*s);
            Label(new Rect(inner.x,inner.y-7f*s,inner.width,38f*s),"Ваши чертежи",25,TextAnchor.MiddleCenter);
            var filter=ResearchFilterRect(false);var other=ResearchFilterRect(true);
            DrawSliced(filter,art.panelFrame,true,.07f*s);
            DrawSliced(other,art.panelFrame,true,.07f*s);
            Label(filter,"Не изучено",17,TextAnchor.MiddleCenter,researchShowStudied);
            Label(other,"Изучено",17,TextAnchor.MiddleCenter,!researchShowStudied);
            var active=researchShowStudied?other:filter;
            Fill(new Rect(active.x,active.yMax-2f*s,active.width,2f*s),Accent);
            if(GUI.Button(filter,GUIContent.none,GUIStyle.none))SelectResearchFilter(false);
            if(GUI.Button(other,GUIContent.none,GUIStyle.none))SelectResearchFilter(true);
            for(var i=0;i<12;i++)DrawBlueprintNeutralFrame(ResearchGridCell(i));
            var index=0;
            if(research?.Entries!=null)foreach(var entry in research.Entries)
            {
                if(!ResearchEntryVisible(entry))continue;
                if(index>=12)break;
                var cell=ResearchGridCell(index++);
                DrawBlueprintCell(cell,entry.Item.Blueprint,research.SelectedKind==entry.KindIndex);
                FitText(new Rect(cell.x-3f*s,cell.yMax,cell.width+6f*s,21f*s),entry.Item.Blueprint.ShortLabel??"Чертёж",13,false);
                if(GUI.Button(cell,GUIContent.none,GUIStyle.none))SelectResearchEntry(entry.KindIndex);
            }
            var teleport=ResearchTeleportRect;
            DrawSliced(teleport,art.panelFrame,true,.09f*s);
            if(research!=null&&research.SelectedKind==-1)DrawEquipmentSelection(teleport);
            var icon=new Rect(teleport.x+10f*s,teleport.y+9f*s,58f*s,58f*s);
            DrawDisc(icon,PlumInk);DrawIcon(icon,art.teleportIcon);
            FitText(new Rect(icon.xMax+12f*s,teleport.y+9f*s,teleport.width-90f*s,30f*s),"Телепортационный круг",20,false);
            Label(new Rect(icon.xMax+12f*s,teleport.y+39f*s,teleport.width-90f*s,24f*s),"Стартовый чертёж",14,TextAnchor.UpperLeft,true);
            if(GUI.Button(teleport,GUIContent.none,GUIStyle.none))SelectResearchEntry(-1);
        }

        private void DrawResearchPreview()
        {
            var s=UiScale;var r=Inset(ResearchPanelRect(1),30f*s);var entry=SelectedResearchEntry;
            var teleport=research==null||research.SelectedKind==-1;
            var studied=teleport?research?.TeleportStudied==true:entry?.Studied==true;
            Label(new Rect(r.x,r.y-13f*s,r.width,44f*s),"Выбранный чертёж",24,TextAnchor.MiddleCenter);
            var preview=new Rect(r.x+15f*s,r.y+54f*s,r.width-30f*s,r.height*.57f);
            if(teleport)
            {
                var side=Mathf.Min(preview.width,preview.height);
                var icon=new Rect(preview.center.x-side*.5f,preview.center.y-side*.5f,side,side);
                DrawDisc(Inset(icon,8f*s),PlumInk);DrawIcon(icon,art.teleportIcon);
            }
            else if(entry?.Item?.Blueprint!=null)DrawBlueprintIcon(preview,entry.Item.Blueprint);
            var titleY=r.y+r.height*.70f;
            FitText(new Rect(r.x,titleY,r.width,40f*s),teleport?"Телепортационный круг":entry?.Item?.Name??"Выберите чертёж",26,false);
            Label(new Rect(r.x,titleY+42f*s,r.width,42f*s),teleport?"Стартовый чертёж":
                entry?.Item?.Blueprint!=null?BlueprintCategoryCaption(entry.Item.Blueprint):"",17,TextAnchor.UpperCenter,true);
            DrawRule(new Rect(r.x,r.yMax-65f*s,r.width,s));
            if(studied)DrawBlueprintKnownPill(new Rect(r.x,r.yMax-55f*s,r.width,28f*s));
            Label(new Rect(r.x,r.yMax-(studied?24f:46f)*s,r.width,(studied?24f:42f)*s),
                teleport?(studied?"Знание сохранено":"Всегда доступен в мастерской"):studied?"Чертёж израсходован":"Чертёж расходуется при изучении",15,TextAnchor.MiddleCenter,true);
        }

        private void DrawResearchConditions()
        {
            var s=UiScale;var r=Inset(ResearchPanelRect(2),30f*s);var entry=SelectedResearchEntry;
            var teleport=research==null||research.SelectedKind==-1;var paper=entry?.Item?.Blueprint;
            var studied=teleport?research?.TeleportStudied==true:entry?.Studied==true;
            Label(new Rect(r.x,r.y-13f*s,r.width,44f*s),"Результат",25,TextAnchor.MiddleCenter);
            var result=teleport?"Открывает строительство телепортационного круга. Постройте его на базе для переходов к открытым точкам.":paper?.Description??"Выберите чертёж в списке слева.";
            if(paper!=null&&paper.TargetStage>0)
                result=(studied?"Открыто улучшение кузницы до ступени ":"Открывает улучшение кузницы до ступени ")+BlueprintStage(paper.TargetStage)+".\nСамо улучшение выполняется в кузнице.";
            Label(new Rect(r.x+8f*s,r.y+63f*s,r.width-16f*s,113f*s),result,19,TextAnchor.UpperLeft,true);
            var conditionsY=r.y+208f*s;
            DrawRule(new Rect(r.x,conditionsY-22f*s,r.width,s));
            Label(new Rect(r.x,conditionsY,r.width,38f*s),"Условия",25);
            if(studied)
            {
                DrawResearchCondition(new Rect(r.x,conditionsY+45f*s,r.width,45f*s),true,"Знание сохранено");
                Label(new Rect(r.x,conditionsY+99f*s,r.width,45f*s),"Повторно изучать не нужно.",17,TextAnchor.MiddleCenter,true);
            }
            else
            {
                DrawResearchCondition(new Rect(r.x,conditionsY+45f*s,r.width,45f*s),teleport||entry?.Owned==true,
                    teleport?"Всегда доступен в мастерской":"Чертёж в инвентаре");
                DrawResearchCondition(new Rect(r.x,conditionsY+99f*s,r.width,45f*s),research?.NearWorkshop==true,
                    research?.WorkshopBuilt==true?"Вы у мастерской":"Нужна мастерская");
            }
            var button=ResearchStudyRect;
            if(Button(button,studied?"Изучено":"Изучить",false,!studied&&research?.CanStudy==true))ResearchStudyRequested?.Invoke(research.SelectedKind);
            Label(new Rect(r.x,button.yMax+2f*s,r.width,30f*s),studied?"":research?.CanStudy==true?"Готово к изучению":research?.Reason??"Недоступно",13,TextAnchor.MiddleCenter,true);
        }

        private void DrawResearchCondition(Rect r,bool met,string text)
        {
            var s=UiScale;DrawSliced(r,art.panelFrame,true,.055f*s);
            var icon=new Rect(r.x+9f*s,r.y+10f*s,25f*s,25f*s);
            DrawDisc(icon,met?new Color(.13f,.55f,.59f):Silver);
            PaleLabel(icon,met?"✓":"—",18,TextAnchor.MiddleCenter);
            Label(new Rect(icon.xMax+12f*s,r.y,r.width-icon.width-33f*s,r.height),text,17,TextAnchor.MiddleLeft,true);
        }
    }
}
