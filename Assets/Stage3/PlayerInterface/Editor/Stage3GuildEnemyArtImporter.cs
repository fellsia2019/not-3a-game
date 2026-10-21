using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Not3A.Stage3.VisualPilot;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3GuildEnemyArtImporter
    {
        private const string Folder="Assets/Stage3/PlayerInterface/Resources/";
        private const string ArtPath=Folder+"Stage3GuildEnemyArt.asset";
        [MenuItem("Tools/Stage 3/Guild/Import Approved Enemy Poses")]
        public static void BuildAssets()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before importing enemy poses.");
            var art=AssetDatabase.LoadAssetAtPath<Stage3GuildEnemyArt>(ArtPath);
            if(art==null){art=ScriptableObject.CreateInstance<Stage3GuildEnemyArt>();AssetDatabase.CreateAsset(art,ArtPath);}
            art.orcFrames=Import(art,"Stage3GuildOrc.png","Guild orc",1.15f*1.25f);
            art.ogreFrames=Import(art,"Stage3GuildOgre.png","Guild ogre",1.15f*1.55f);
            var hero=AssetDatabase.LoadAssetAtPath<Stage3HeroArt>("Assets/Stage3/DayNight/HeroArt.asset");
            if(hero==null||hero.material==null)throw new InvalidOperationException("Approved hero material required.");
            art.material=hero.material;EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("S3_GUILD_ENEMY_ART_READY: 16 isolated full poses; unchanged source PNG, grounded pivots, one scale per species, shared hero day/night shader.");
        }
        private static Sprite[] Import(Stage3GuildEnemyArt owner,string file,string label,float idleHeight)
        {
            var path=Folder+file;if(!File.Exists(path))throw new FileNotFoundException("Missing approved enemy pose sheet",path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.isReadable=false;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var decoded=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                if(!ImageConversion.LoadImage(decoded,File.ReadAllBytes(path),false))throw new InvalidOperationException("Cannot decode enemy pose source");
                var pixels=decoded.GetPixels32();
                var components=Stage3GuildArtImporter.ConnectedBounds(pixels,decoded.width,decoded.height,8);
                components.Sort((a,b)=>b.center.y.CompareTo(a.center.y));
                components.Sort(0,4,Comparer<RectInt>.Create((a,b)=>a.x.CompareTo(b.x)));
                components.Sort(4,4,Comparer<RectInt>.Create((a,b)=>a.x.CompareTo(b.x)));
                var bounds=components.ToArray();
                var ppu=bounds[0].height/idleHeight;var result=new Sprite[8];
                for(var i=0;i<8;i++)
                {
                    var b=bounds[i];
                    var left=Mathf.Max(0,b.x-3);var bottom=Mathf.Max(0,b.y-3);
                    var right=Mathf.Min(decoded.width,b.xMax+3);var top=Mathf.Min(decoded.height,b.yMax+3);
                    // The rear strike crosses a nominal row. Keep complete components, but never include its neighbour's pixels.
                    for(var j=0;j<bounds.Length;j++)if(j!=i)
                    {
                        var other=bounds[j];
                        if(other.x<b.xMax&&other.xMax>b.x)
                        {
                            if(other.yMax<=b.y)bottom=Mathf.Max(bottom,(other.yMax+b.y)/2);
                            else if(other.y>=b.yMax)top=Mathf.Min(top,(other.y+b.yMax)/2);
                        }
                        if(other.y<b.yMax&&other.yMax>b.y)
                        {
                            if(other.xMax<=b.x)left=Mathf.Max(left,(other.xMax+b.x)/2);
                            else if(other.x>=b.xMax)right=Mathf.Min(right,(other.x+b.xMax)/2);
                        }
                    }
                    var rect=new Rect(left,bottom,right-left,top-bottom);
                    // Sole midpoint is measured independently from weapon reach; pose art remains at a shared PPU.
                    var sole=FootCentre(pixels,decoded.width,b);
                    var pivot=new Vector2((sole-rect.x)/rect.width,(b.y-rect.y)/rect.height);
                    var name=label+" pose "+i;
                    foreach(var existing in AssetDatabase.LoadAllAssetsAtPath(ArtPath))if(existing is Sprite old&&old.name==name)
                    {
                        if(old.texture!=texture||old.rect!=rect||Mathf.Abs(old.pixelsPerUnit-ppu)>.001f)throw new InvalidOperationException("Enemy art changed; explicit migration required: "+name);
                        result[i]=old;break;
                    }
                    if(result[i]==null){var sprite=Sprite.Create(texture,rect,pivot,ppu,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,owner);result[i]=sprite;}
                    Debug.Log("S3_GUILD_ENEMY_BOUNDS: "+name+" "+b+" alpha>=16, sole="+sole+", ppu="+ppu);
                }
                return result;
            }
            finally{UnityEngine.Object.DestroyImmediate(decoded);}
        }
        private static float FootCentre(Color32[] pixels,int width,RectInt bounds)
        {
            var min=bounds.xMax;var max=bounds.x;
            var top=bounds.y+Mathf.Max(3,Mathf.RoundToInt(bounds.height*.06f));
            for(var y=bounds.y;y<top;y++)for(var x=bounds.x;x<bounds.xMax;x++)if(pixels[y*width+x].a>=128){min=Mathf.Min(min,x);max=Mathf.Max(max,x);}
            return min<=max?(min+max)*.5f:bounds.center.x;
        }
    }
}
