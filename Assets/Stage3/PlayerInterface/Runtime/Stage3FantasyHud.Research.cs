using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
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

        private void DrawResearch(Rect content)
        {
            var s=UiScale;
            var sidebar=new Rect(content.x,content.y,content.width*.29f,content.height);
            var detail=new Rect(sidebar.xMax+24f*s,content.y,content.xMax-sidebar.xMax-24f*s,content.height);
            var entry=new Rect(sidebar.x,sidebar.y+12f*s,sidebar.width,106f*s);
            DrawPanel(entry);
            var entrySafe=PanelContentRect(entry);
            var iconSide=Mathf.Min(48f*s,entrySafe.height);
            var iconRect=new Rect(entrySafe.x,entrySafe.center.y-iconSide*.5f,iconSide,iconSide);
            var textX=iconRect.xMax+12f*s;
            if (art!=null) DrawIcon(iconRect,art.teleportIcon);
            Label(new Rect(textX,entrySafe.y,entrySafe.xMax-textX,entrySafe.height*.45f),"Телепортация",16,TextAnchor.MiddleLeft);
            Label(new Rect(textX,entrySafe.y+entrySafe.height*.45f,entrySafe.xMax-textX,entrySafe.height*.55f),ResearchStatusText,13,TextAnchor.MiddleLeft,true);
            Label(new Rect(detail.x,detail.y+10f*s,detail.width,40f*s),"Телепортация",30,TextAnchor.MiddleCenter);
            var sealSide=Mathf.Min(180f*s,content.height*.34f);
            var medallion=new Rect(detail.center.x-sealSide*.5f,detail.y+56f*s,sealSide,sealSide);
            DrawMedallion(medallion,true);
            if (art!=null) DrawIcon(Inset(medallion,sealSide*.16f),art.teleportIcon);
            Label(new Rect(detail.x+16f*s,medallion.yMax+12f*s,detail.width-32f*s,60f*s),
                "Открывает базовый круг.\nСвязывает его с найденным кругом в мире.",17,TextAnchor.MiddleCenter);
            var status=ResearchStatusText;
            if (researchTravel != null && researchTravel.IsResearched)
                status += researchTravel.RemoteDiscovered ? " · круг в мире найден" : " · найдите круг в мире";
            Label(new Rect(detail.x,detail.yMax-102f*s,detail.width,35f*s),status,18,TextAnchor.MiddleCenter);
            var hint=new Rect(detail.x+12f*s,detail.yMax-60f*s,detail.width-24f*s,48f*s);
            DrawPanel(hint);
            Label(PanelContentRect(hint),researchTravel!=null && researchTravel.IsResearched ?
                "Исследование завершено" : "У мастерской · E",20,TextAnchor.MiddleCenter);
        }
    }
}
