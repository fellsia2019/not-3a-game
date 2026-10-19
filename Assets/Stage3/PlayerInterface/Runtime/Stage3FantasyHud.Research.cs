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
            var sidebar=new Rect(content.x,content.y,content.width*.26f,content.height);
            var visual=new Rect(content.x+content.width*.28f,content.y+60f*s,content.width*.37f,content.height-100f*s);
            var detail=new Rect(content.x+content.width*.68f,content.y,content.width*.32f,content.height);
            Label(new Rect(sidebar.x,sidebar.y,sidebar.width,56f*s),"Исследования",35);
            Label(new Rect(sidebar.x,sidebar.y+60f*s,sidebar.width,40f*s),"Возможности мастерской",20,TextAnchor.UpperLeft,true);
            var entry=new Rect(sidebar.x,sidebar.y+134f*s,sidebar.width,96f*s);
            Fill(entry,new Color(1f,1f,1f,.24f));Stroke(entry,Silver,1f*s);
            Fill(new Rect(entry.x,entry.y,3f*s,entry.height),Accent);
            var thumbnail=new Rect(entry.x+16f*s,entry.y+14f*s,68f*s,68f*s);
            DrawDisc(thumbnail,PlumInk);DrawIcon(thumbnail,art.teleportIcon);
            Label(new Rect(entry.x+96f*s,entry.y,entry.width-108f*s,entry.height),"Телепортация",21,TextAnchor.MiddleLeft);
            DrawRule(new Rect(sidebar.x,entry.yMax+58f*s,sidebar.width,1f*s));
            Label(new Rect(sidebar.x,entry.yMax+78f*s,sidebar.width,60f*s),"Доступно одно исследование",17,TextAnchor.UpperCenter,true);
            var sealSide=Mathf.Min(visual.width,visual.height);
            var seal=new Rect(visual.center.x-sealSide*.5f,visual.center.y-sealSide*.5f,sealSide,sealSide);
            DrawDisc(Inset(seal,12f*s),Silver);
            DrawDisc(Inset(seal,15f*s),new Color(.28f,.20f,.31f));
            DrawIcon(seal,art.teleportIcon);
            var titleY=detail.y+detail.height*.21f;
            DrawRule(new Rect(detail.x+20f*s,titleY-36f*s,detail.width-40f*s,1f*s));
            Label(new Rect(detail.x,titleY,detail.width,57f*s),"Телепортация",34,TextAnchor.MiddleCenter);
            Label(new Rect(detail.x,titleY+90f*s,detail.width,90f*s),
                "Открывает базовый круг.\nСвязывает его с найденным кругом в мире.",21,TextAnchor.MiddleCenter,true);
            DrawRule(new Rect(detail.x+20f*s,titleY+213f*s,detail.width-40f*s,1f*s));
            var status=ResearchStatusText;
            if(researchTravel!=null&&researchTravel.IsResearched)
                status+=researchTravel.RemoteDiscovered?" · круг в мире найден":" · найдите круг в мире";
            Label(new Rect(detail.x,titleY+242f*s,detail.width,66f*s),status,22,TextAnchor.MiddleCenter,true);
            Label(new Rect(detail.x,detail.yMax-74f*s,detail.width,62f*s),
                researchTravel!=null&&researchTravel.IsResearched?"Исследование завершено":"Подойдите к мастерской и нажмите E",19,TextAnchor.MiddleCenter,true);
        }
    }
}
