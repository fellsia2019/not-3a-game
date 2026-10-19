using System.Collections.Generic;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        private enum BuildingGlyph { Sword, Range, Hourglass, Heart, Upgrade }
        private readonly Dictionary<(Vector2 size,float cut,Color top,Color bottom,bool metal),Texture2D> bevelTextures=
            new Dictionary<(Vector2 size,float cut,Color top,Color bottom,bool metal),Texture2D>();
        private Vector2Int bevelScreenSize;

        private void DrawStatusBevel(Rect r,Color tint)
        {
            var s=UiScale;
            DrawBevelGradient(r,PlumInk,PlumInk,11f*s);
            DrawBevelGradient(Inset(r,1f*s),new Color(.91f,.85f,.76f),Silver,10f*s);
            DrawBevelGradient(Inset(r,3f*s),new Color(.24f,.14f,.29f),new Color(.10f,.05f,.14f),8f*s);
            var track=Inset(r,5f*s);
            DrawBevelGradient(track,Color.Lerp(tint,Pearl,.25f)*new Color(.72f,.72f,.72f,1),
                Color.Lerp(PlumInk,tint,.30f),7f*s);
            Fill(new Rect(track.x+8f*s,track.y+1f*s,track.width-16f*s,1f*s),new Color(1,1,1,.35f));
            DrawLunarJewel(new Vector2(r.x+2f*s,r.center.y),10f*s);
            DrawLunarJewel(new Vector2(r.xMax-2f*s,r.center.y),10f*s);
        }

        private void DrawBevelGradient(Rect r,Color top,Color bottom,float cut)
        {
            if(Event.current.type!=EventType.Repaint || r.width<=0 || r.height<=0) return;
            var screen=new Vector2Int(Screen.width,Screen.height);
            if(bevelScreenSize!=screen) { ReleaseBevelTextures();bevelScreenSize=screen; }
            var key=(r.size,cut,top,bottom,UsesMetalLinearTarget);
            if(!bevelTextures.TryGetValue(key,out var texture))
            {
                if(bevelTextures.Count>=16) ReleaseBevelTextures();
                var width=Mathf.Max(1,Mathf.CeilToInt(r.width));var height=Mathf.Max(1,Mathf.CeilToInt(r.height));
                texture=new Texture2D(width,height,TextureFormat.RGBA32,false,false)
                { name="HUD bevel cache",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave };
                var pixels=new Color[width*height];
                for(var y=0;y<height;y++)
                {
                    var t=(y+.5f)/height;var inset=Mathf.Abs(t-.5f)*2f*cut;
                    // Store the former GUI tint in sRGB: texture sampling must perform the
                    // same color-space conversion that GUI applied to that vertex tint.
                    var color=NativeGuiColor(Color.Lerp(top,bottom,t));
                    for(var x=0;x<width;x++)
                    {
                        var pixel=color;var localX=(x+.5f)*r.width/width;
                        if(localX<inset || localX>r.width-inset) pixel.a=0;
                        pixels[(height-1-y)*width+x]=pixel;
                    }
                }
                texture.SetPixels(pixels);texture.Apply(false,true);
                bevelTextures.Add(key,texture);
            }
            GUI.DrawTexture(r,texture,ScaleMode.StretchToFill,true);
        }

        private void ReleaseBevelTextures()
        {
            foreach(var texture in bevelTextures.Values) if(texture!=null) Destroy(texture);
            bevelTextures.Clear();
        }

        private void DrawLunarJewel(Vector2 center,float side)
        {
            var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(45f,center);
            var r=new Rect(center.x-side*.5f,center.y-side*.5f,side,side);
            Fill(r,PlumInk);Fill(Inset(r,1f*UiScale),Silver);Fill(Inset(r,2f*UiScale),Accent);
            Fill(new Rect(r.x+2f*UiScale,r.y+2f*UiScale,side*.35f,1f*UiScale),new Color(.72f,1f,1f));
            GUI.matrix=matrix;
        }

        private void DrawGlyphLine(Vector2 a,Vector2 b,float width,Color color)
        {
            var matrix=GUI.matrix;var delta=b-a;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,a);
            Fill(new Rect(a.x,a.y-width*.5f,delta.magnitude,width),color);GUI.matrix=matrix;
        }

        private void DrawBuildingGlyph(Rect r,BuildingGlyph glyph,Color color)
        {
            var c=r.center;var u=r.width/24f;var line=2.6f*u;
            Vector2 P(float x,float y) => c+new Vector2(x*u,y*u);
            switch(glyph)
            {
                case BuildingGlyph.Range:
                    DrawDisc(new Rect(c.x-8*u,c.y-8*u,16*u,16*u),color);
                    DrawDisc(new Rect(c.x-5.5f*u,c.y-5.5f*u,11*u,11*u),Pearl);
                    DrawDisc(new Rect(c.x-2*u,c.y-2*u,4*u,4*u),color);
                    DrawGlyphLine(P(-11,0),P(-6,0),line,color);DrawGlyphLine(P(6,0),P(11,0),line,color);
                    DrawGlyphLine(P(0,-11),P(0,-6),line,color);DrawGlyphLine(P(0,6),P(0,11),line,color);break;
                case BuildingGlyph.Hourglass:
                    DrawGlyphLine(P(-7,-9),P(7,-9),line,color);DrawGlyphLine(P(-7,9),P(7,9),line,color);
                    DrawGlyphLine(P(-6,-7),P(6,7),line,color);DrawGlyphLine(P(6,-7),P(-6,7),line,color);
                    DrawGlyphLine(P(-6,-7),P(-6,-4),line,color);DrawGlyphLine(P(6,-7),P(6,-4),line,color);
                    DrawGlyphLine(P(-6,7),P(-6,4),line,color);DrawGlyphLine(P(6,7),P(6,4),line,color);break;
                case BuildingGlyph.Heart:
                    DrawDisc(new Rect(c.x-10*u,c.y-8*u,12*u,12*u),color);
                    DrawDisc(new Rect(c.x-2*u,c.y-8*u,12*u,12*u),color);
                    for(var i=0;i<10;i++) Fill(new Rect(c.x-(10-i)*u,c.y+i*u,(20-2*i)*u,1.1f*u),color);
                    break;
                case BuildingGlyph.Upgrade:
                    DrawGlyphLine(P(0,9),P(0,-8),4*u,color);
                    DrawGlyphLine(P(-7,-1),P(0,-8),4*u,color);DrawGlyphLine(P(7,-1),P(0,-8),4*u,color);
                    DrawGlyphLine(P(-7,11),P(7,11),2*u,color);break;
                default:
                    DrawGlyphLine(P(-7,8),P(7,-8),4*u,color);
                    DrawGlyphLine(P(-8,1),P(-1,8),3*u,color);
                    DrawGlyphLine(P(4,-8),P(8,-9),3*u,color);break;
            }
        }

        private bool TryDrawStatGlyph(Rect r,string label)
        {
            BuildingGlyph glyph;
            switch(label)
            {
                case "Урон": glyph=BuildingGlyph.Sword;break;
                case "Дальность": glyph=BuildingGlyph.Range;break;
                case "Интервал": glyph=BuildingGlyph.Hourglass;break;
                case "Здоровье": glyph=BuildingGlyph.Heart;break;
                default:return false;
            }
            DrawDisc(Inset(r,r.width*.16f),Pearl);DrawMedallion(r);
            DrawBuildingGlyph(Inset(r,r.width*.24f),glyph,PlumInk);return true;
        }
    }
}
