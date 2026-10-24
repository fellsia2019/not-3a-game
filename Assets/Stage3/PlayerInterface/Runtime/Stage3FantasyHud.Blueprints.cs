using System;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed class Stage3BlueprintItemView
    {
        public int KindIndex, ArtIndex, TargetStage;
        public string ShortLabel, CategoryLabel, Description;
        public bool Studied;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3BlueprintArt blueprintArt;
        private bool blueprintArtLoaded;
        public event Action<int> BlueprintResearchRequested;

        private Stage3BlueprintArt BlueprintArt
        {
            get
            {
                if (!blueprintArtLoaded)
                {
                    blueprintArtLoaded = true;
                    blueprintArt = Resources.Load<Stage3BlueprintArt>("Stage3BlueprintArt");
                }
                return blueprintArt;
            }
        }

        private static string BlueprintStage(int stage) => stage == 2 ? "II" : stage == 3 ? "III" : "";
        private static string ItemCellCaption(Stage3EquipmentItemView item) => item.Blueprint != null
            ? item.Blueprint.ShortLabel ?? "Чертёж" : item.Consumable ? (item.TypeIndex==11?"HP":item.TypeIndex==12?"MP":"HP + MP") : $"{QualityName(item.QualityIndex)} · LVL {item.Level}";
        private static string BlueprintCategoryCaption(Stage3BlueprintItemView item) => item.TargetStage > 0
            ? "Улучшение · целевая ступень " + BlueprintStage(item.TargetStage)
            : item.CategoryLabel ?? "Открытие";

        private void DrawBlueprintCell(Rect rect, Stage3BlueprintItemView item, bool selected)
        {
            DrawBlueprintNeutralFrame(rect);
            DrawBlueprintIcon(Inset(rect, Mathf.Min(rect.width,rect.height)*.14f), item);
            if (selected) DrawEquipmentSelection(rect); else DrawEquipmentHover(rect);
            if (item.Studied)
            {
                var size=Mathf.Min(18f*UiScale,Mathf.Min(rect.width,rect.height)*.205f);
                DrawBlueprintKnownBadge(new Rect(rect.xMax-size-4f*UiScale,rect.y+4f*UiScale,size,size));
            }
        }

        private void DrawBlueprintKnownBadge(Rect r)
        {
            // GUI rounded textures inherit the scroll viewport, including on Metal.
            // This tiny native medallion owns no textures and never tints the paper.
            void Circle(Rect box,Color color) => GUI.DrawTexture(box,Texture2D.whiteTexture,
                ScaleMode.StretchToFill,true,0f,NativeGuiColor(color),0f,box.width*.5f);
            Circle(r,new Color(.66f,.72f,.74f));
            Circle(Inset(r,Mathf.Max(.7f,r.width*.055f)),new Color(.96f,.96f,.88f));
            Circle(Inset(r,Mathf.Max(1.5f,r.width*.13f)),new Color(.12f,.47f,.52f));
            PaleLabel(r,"✓",Mathf.Max(10,Mathf.RoundToInt(r.height*.72f/UiScale)),TextAnchor.MiddleCenter);
        }

        private void DrawBlueprintKnownPill(Rect r)
        {
            var s=UiScale;var width=Mathf.Min(r.width,148f*s);
            var pill=new Rect(r.center.x-width*.5f,r.y,width,r.height);
            GUI.DrawTexture(pill,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0f,
                NativeGuiColor(Silver),0f,pill.height*.5f);
            GUI.DrawTexture(Inset(pill,s),Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0f,
                NativeGuiColor(new Color(.95f,.93f,.87f)),0f,pill.height*.5f);
            var icon=new Rect(pill.x+3f*s,pill.y+3f*s,pill.height-6f*s,pill.height-6f*s);
            DrawBlueprintKnownBadge(icon);
            Label(new Rect(icon.xMax+6f*s,pill.y,pill.xMax-icon.xMax-12f*s,pill.height),"Изучено",16,TextAnchor.MiddleCenter);
        }

        private void DrawBlueprintNeutralFrame(Rect rect)
        {
            var sprite = art?.controlSlot??art?.panelFrame;
            if (sprite == null || Event.current.type != EventType.Repaint) return;
            var src = sprite.rect; var border = sprite.border;
            var factor = Mathf.Min(.09f*UiScale,Mathf.Min(rect.width/(border.x+border.z+1f),
                rect.height/(border.y+border.w+1f)));
            // Use the existing neutral ceramic slot, with the same explicit Metal
            // viewport clipping as item skins. No quality colour or owned texture.
            for(var row=0;row<3;row++)
            for(var col=0;col<3;col++)
            {
                var x0=col==0?rect.x:col==1?rect.x+border.x*factor:rect.xMax-border.z*factor;
                var x1=col==0?rect.x+border.x*factor:col==1?rect.xMax-border.z*factor:rect.xMax;
                var y0=row==0?rect.y:row==1?rect.y+border.w*factor:rect.yMax-border.y*factor;
                var y1=row==0?rect.y+border.w*factor:row==1?rect.yMax-border.y*factor:rect.yMax;
                var sx0=col==0?src.x:col==1?src.x+border.x:src.xMax-border.z;
                var sx1=col==0?src.x+border.x:col==1?src.xMax-border.z:src.xMax;
                var sy0=row==0?src.yMax-border.w:row==1?src.y+border.y:src.y;
                var sy1=row==0?src.yMax:row==1?src.yMax-border.w:src.y+border.y;
                var target=Rect.MinMaxRect(Mathf.Round(x0),Mathf.Round(y0),Mathf.Round(x1),Mathf.Round(y1));
                if(target.width>0&&target.height>0)
                    DrawEquipmentTexture(target,sprite.texture,new Rect(sx0/sprite.texture.width,sy0/sprite.texture.height,
                        (sx1-sx0)/sprite.texture.width,(sy1-sy0)/sprite.texture.height));
            }
        }

        private void DrawBlueprintIcon(Rect rect, Stage3BlueprintItemView item)
        {
            var pages=BlueprintArt?.Pages;
            var sprite=pages!=null&&item.ArtIndex>=0&&item.ArtIndex<pages.Length?pages[item.ArtIndex]:null;
            if(sprite==null) {Label(rect,"Чертёж",12,TextAnchor.MiddleCenter,true);return;}
            var src=sprite.rect;var aspect=src.width/src.height;
            var width=Mathf.Min(rect.width,rect.height*aspect);var height=width/aspect;
            var target=new Rect(rect.center.x-width*.5f,rect.center.y-height*.5f,width,height);
            DrawEquipmentTexture(target,sprite.texture,new Rect(src.x/sprite.texture.width,src.y/sprite.texture.height,
                src.width/sprite.texture.width,src.height/sprite.texture.height));
            if(item.TargetStage==2||item.TargetStage==3)
            {
                // Source forge sheet reserves this corner; only target building
                // stage appears here, never equipment level or quality.
                var caption=new Rect(target.x+target.width*.77f,target.y+target.height*.81f,
                    target.width*.19f,target.height*.15f);
                var size=Mathf.Clamp(Mathf.RoundToInt(target.height*.12f/UiScale),9,32);
                Label(caption,BlueprintStage(item.TargetStage),size,TextAnchor.MiddleCenter);
            }
        }

        private void DrawBlueprintInventoryDetails(Rect r,Stage3EquipmentItemView item)
        {
            var s=UiScale;var blueprint=item.Blueprint;
            FitText(new Rect(r.x,r.y,r.width,52f*s),item.Name??"Чертёж",30,false);
            DrawRule(new Rect(r.x,r.y+72f*s,r.width,s));
            var side=Mathf.Min(r.width*.88f,r.height*.39f);
            DrawBlueprintIcon(new Rect(r.center.x-side*.5f,r.y+91f*s,side,side),blueprint);
            Label(new Rect(r.x,r.y+r.height*.54f,r.width,40f*s),BlueprintCategoryCaption(blueprint),20,TextAnchor.MiddleCenter);
            if(blueprint.Studied)DrawBlueprintKnownPill(new Rect(r.x,r.y+r.height*.62f,r.width,28f*s));
            Label(new Rect(r.x+5f*s,r.y+r.height*(blueprint.Studied ? .70f : .65f),r.width-10f*s,r.height*(blueprint.Studied ? .13f : .18f)),
                blueprint.Description??"",17,TextAnchor.UpperLeft,true);
            var requested=Button(EquipmentActionRect,"К исследованиям");
            Label(new Rect(r.x,EquipmentActionRect.yMax+3f*s,r.width,28f*s),
                blueprint.Studied?"Уже изучено":"Изучается в исследовательском центре",12,TextAnchor.MiddleCenter,true);
            if(requested)BlueprintResearchRequested?.Invoke(blueprint.KindIndex);
        }
    }
}
