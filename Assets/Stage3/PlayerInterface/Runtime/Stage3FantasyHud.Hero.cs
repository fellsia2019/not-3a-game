using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private void DrawHero(Rect content)
        {
            var s=UiScale;
            var profile=new Rect(content.x,content.y,content.width*.19f,content.height);
            var description=new Rect(content.xMax-content.width*.25f,content.y,content.width*.25f,content.height);
            var tree=new Rect(profile.xMax+18f*s,content.y,description.x-profile.xMax-36f*s,content.height);
            var portrait=new Rect(profile.x,profile.y+10f*s,profile.width,profile.width);
            if(art!=null && art.heroPortrait!=null) DrawRoundSprite(Inset(portrait,12f*s),art.heroPortrait);
            DrawMedallion(portrait);
            Label(new Rect(profile.x,portrait.yMax+15f*s,profile.width,35f*s),"Герой",24,TextAnchor.MiddleCenter);
            Label(new Rect(profile.x,portrait.yMax+63f*s,profile.width,76f*s),"Здоровье  —\nМана  —\nОпыт  —",17,TextAnchor.UpperCenter,true);
            Label(new Rect(profile.x,profile.yMax-30f*s,profile.width,26f*s),"Будущие параметры",13,TextAnchor.MiddleCenter,true);

            // The approved schematic is a visual reserve, not an actionable progression graph.
            var positions=new[]{new Vector2(.14f,.50f),new Vector2(.42f,.24f),new Vector2(.45f,.50f),new Vector2(.42f,.76f),
                new Vector2(.77f,.13f),new Vector2(.82f,.34f),new Vector2(.82f,.58f),new Vector2(.77f,.84f)};
            var links=new[]{new Vector2Int(0,1),new Vector2Int(0,2),new Vector2Int(0,3),new Vector2Int(1,4),new Vector2Int(1,5),new Vector2Int(2,6),new Vector2Int(3,7)};
            foreach(var link in links)
                DrawHeroLink(tree.position+Vector2.Scale(tree.size,positions[link.x]),tree.position+Vector2.Scale(tree.size,positions[link.y]),s);
            for(var i=0;i<positions.Length;i++)
            {
                var center=tree.position+Vector2.Scale(tree.size,positions[i]);
                var diameter=(i==0?70f:58f)*s;
                var node=new Rect(center.x-diameter*.5f,center.y-diameter*.5f,diameter,diameter);
                DrawMedallion(node);
                Label(node,i==0?"◇":"—",24,TextAnchor.MiddleCenter,true);
            }
            DrawPanel(description);
            var descriptionSafe=PanelContentRect(description);
            Label(new Rect(descriptionSafe.x,descriptionSafe.y,descriptionSafe.width,55f*s),"Развитие героя",23,TextAnchor.MiddleCenter);
            var sealSide=Mathf.Min(82f*s,descriptionSafe.width,descriptionSafe.height*.24f);
            var seal=new Rect(descriptionSafe.center.x-sealSide*.5f,descriptionSafe.y+65f*s,sealSide,sealSide);
            DrawMedallion(seal);Label(seal,"◇",36,TextAnchor.MiddleCenter,true);
            var noteTop=seal.yMax+16f*s;
            var noteBottom=descriptionSafe.yMax-65f*s;
            Label(new Rect(descriptionSafe.x,noteTop,descriptionSafe.width,Mathf.Max(0f,noteBottom-noteTop)),
                "Будущие способности\nпока недоступны.",16,TextAnchor.MiddleCenter,true);
            Label(new Rect(descriptionSafe.x,descriptionSafe.yMax-55f*s,descriptionSafe.width,55f*s),
                "Условия открытия\n—",16,TextAnchor.MiddleCenter,true);
        }

        private void DrawHeroLink(Vector2 from,Vector2 to,float scale)
        {
            var matrix=GUI.matrix;var color=GUI.color;
            try
            {
                GUI.color=new Color(MutedInk.r,MutedInk.g,MutedInk.b,.4f);
                GUIUtility.RotateAroundPivot(Mathf.Atan2(to.y-from.y,to.x-from.x)*Mathf.Rad2Deg,from);
                GUI.DrawTexture(new Rect(from.x,from.y,Vector2.Distance(from,to),Mathf.Max(1f,scale)),Texture2D.whiteTexture);
            }
            finally{GUI.matrix=matrix;GUI.color=color;}
        }
    }
}
