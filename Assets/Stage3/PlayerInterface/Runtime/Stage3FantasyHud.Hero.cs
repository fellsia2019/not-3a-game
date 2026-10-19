using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private void DrawHero(Rect content)
        {
            var s=UiScale;
            var figure=new Rect(content.x,content.y,content.width*.45f,content.height);
            if(art.inventoryHero!=null) DrawIcon(figure,art.inventoryHero);
            else if(art.heroPortrait!=null) DrawRoundSprite(figure,art.heroPortrait);
            var message=new Rect(content.x+content.width*.49f,content.y,content.width*.51f,content.height);
            var diameter=Mathf.Min(200f*s,message.height*.34f);
            var moon=new Rect(message.center.x-diameter*.5f,message.y+message.height*.10f,diameter,diameter);
            // A decorative crescent, deliberately not a progression node or interactive control.
            DrawDisc(moon,new Color(.67f,.60f,.63f,.22f));
            DrawDisc(new Rect(moon.x+diameter*.25f,moon.y-diameter*.08f,diameter*.90f,diameter*.90f),Pearl);
            var titleY=message.y+message.height*.49f;
            Label(new Rect(message.x,titleY,message.width,65f*s),"Развитие героя",38,TextAnchor.MiddleCenter);
            DrawRule(new Rect(message.x+32f*s,titleY+87f*s,message.width-64f*s,1f*s));
            Label(new Rect(message.x,titleY+121f*s,message.width,56f*s),"Пока недоступно",32,TextAnchor.MiddleCenter,true);
            Label(new Rect(message.x,titleY+188f*s,message.width,36f*s),"Этот раздел появится позже.",21,TextAnchor.MiddleCenter,true);
        }
    }
}
