using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Not3A.Stage3.VisualPilot;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    /// <summary>Imports only the approved D089 source rasters. No scene or existing art asset is rewritten.</summary>
    public static class Stage3ForgeArtImporter
    {
        public const string ArtPath="Assets/Stage3/PlayerInterface/Resources/Stage3ForgeArt.asset";
        private const string Folder="Assets/Stage3/PlayerInterface/Resources/";
        [MenuItem("Tools/Stage 3/Equipment/Import Approved Forge Art")]
        public static void BuildAssets()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before importing art.");
            var art=AssetDatabase.LoadAssetAtPath<Stage3ForgeArt>(ArtPath);
            if(art==null){art=ScriptableObject.CreateInstance<Stage3ForgeArt>();AssetDatabase.CreateAsset(art,ArtPath);}
            art.bowPickaxeHelmet=Import("Stage3ItemsBowPickaxeHelmet.png",false);
            art.chestArmsLegs=Import("Stage3ItemsChestArmsLegs.png",false);
            art.wandAxe=Import("Stage3ItemsWandAxe.png",false);
            if(File.Exists(Folder+"Stage3ItemsStaffAccessories.png"))art.staffAccessories=Import("Stage3ItemsStaffAccessories.png",false);
            if(File.Exists(Folder+"Stage3Potions.png"))art.potions=Import("Stage3Potions.png",false);
            var world=Import("Stage3ForgeWorld.png",true);
            var bounds=Visible(world);
            if(art.worldSprite==null)
            {
                art.worldSprite=Sprite.Create(world,new Rect(0,0,world.width,world.height),
                    new Vector2(bounds.center.x/world.width,(float)bounds.yMin/world.height),100f,0,SpriteMeshType.FullRect);
                art.worldSprite.name="Approved forge";AssetDatabase.AddObjectToAsset(art.worldSprite,art);
            }
            const string materialPath=Folder+"Stage3ForgeInk.mat";
            art.material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(art.material==null)
            {
                var shader=Shader.Find("Not3A/Dwelling Sprite");
                if(shader==null)throw new InvalidOperationException("Native dwelling sprite shader is missing.");
                art.material=new Material(shader){name="Forge Ink"};AssetDatabase.CreateAsset(art.material,materialPath);
            }
            art.material.SetVector("_VisibleRect",new Vector4((float)bounds.x/world.width,(float)bounds.y/world.height,(float)bounds.width/world.width,(float)bounds.height/world.height));
            PopulateUiIconBounds(art);
            EditorUtility.SetDirty(art.material);EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("S3_FORGE_ART_READY: original PNGs, equipment/accessory quality atlas and neutral potions, native world material.");
        }
        [MenuItem("Tools/Stage 3/Equipment/Bake UI Visible Icon Bounds")]
        public static void BakeUiIconBounds()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before baking UI icon bounds.");
            var art=AssetDatabase.LoadAssetAtPath<Stage3ForgeArt>(ArtPath);
            if(art==null)throw new InvalidOperationException("Forge art must be imported first.");
            PopulateUiIconBounds(art);
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        }

        private const byte UiAlphaThreshold=16;
        private sealed class UiSourceComponents
        {
            public int Width,Height;
            public int[] Labels;
            public readonly List<RectInt> Bounds=new List<RectInt>{default};
            public readonly List<int> PixelCounts=new List<int>{0};
        }

        private static UiSourceComponents LabelUiSource(Texture2D source)
        {
            var pixels=source.GetPixels32();
            var data=new UiSourceComponents {Width=source.width,Height=source.height,Labels=new int[pixels.Length]};
            var queue=new int[pixels.Length];
            for(var seed=0;seed<pixels.Length;seed++)
            {
                if(pixels[seed].a<UiAlphaThreshold||data.Labels[seed]!=0)continue;
                var label=data.Bounds.Count;var start=0;var end=1;queue[0]=seed;data.Labels[seed]=label;
                var minX=source.width;var minY=source.height;var maxX=-1;var maxY=-1;
                while(start<end)
                {
                    var index=queue[start++];var x=index%source.width;var y=index/source.width;
                    minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);
                    for(var direction=0;direction<4;direction++)
                    {
                        var nx=x+(direction==0?-1:direction==1?1:0);
                        var ny=y+(direction==2?-1:direction==3?1:0);
                        if(nx<0||nx>=source.width||ny<0||ny>=source.height)continue;
                        var next=ny*source.width+nx;
                        if(data.Labels[next]!=0||pixels[next].a<UiAlphaThreshold)continue;
                        data.Labels[next]=label;queue[end++]=next;
                    }
                }
                data.Bounds.Add(new RectInt(minX,minY,maxX-minX+1,maxY-minY+1));
                data.PixelCounts.Add(end);
            }
            return data;
        }

        private static bool ContainsOtherUiSilhouette(UiSourceComponents source,Rect uv,int winner)
        {
            var left=Mathf.Clamp(Mathf.RoundToInt(uv.xMin*source.Width),0,source.Width);
            var right=Mathf.Clamp(Mathf.RoundToInt(uv.xMax*source.Width),0,source.Width);
            var bottom=Mathf.Clamp(Mathf.RoundToInt(uv.yMin*source.Height),0,source.Height);
            var top=Mathf.Clamp(Mathf.RoundToInt(uv.yMax*source.Height),0,source.Height);
            for(var y=bottom;y<top;y++)for(var x=left;x<right;x++)
            {
                var label=source.Labels[y*source.Width+x];
                // Tiny detached AA/specular islands belong to the authored image.
                // Every neighbouring equipment silhouette is well above this limit.
                if(label!=0&&label!=winner&&source.PixelCounts[label]>=256)return true;
            }
            return false;
        }

        private static void PopulateUiIconBounds(Stage3ForgeArt art)
        {
            // Decode original PNGs in Editor only. The old cell identifies an item,
            // but does NOT clip it: artwork crosses nominal column/row boundaries.
            var sources=new Dictionary<Texture2D,UiSourceComponents>();
            var typeCount=art.staffAccessories!=null?11:8;
            var result=new Rect[typeCount*8];
            for(var type=0;type<typeCount;type++)
            for(var quality=0;quality<8;quality++)
            {
                if(!art.TryGetIcon(type,quality,out var texture,out var region))
                    throw new InvalidOperationException($"Missing UI icon {type}/{quality}.");
                if(texture==art.staffAccessories)
                {
                    result[type*8+quality]=IsolatedCellBounds(texture,region);
                    continue;
                }
                if(!sources.TryGetValue(texture,out var source))
                {
                    var path=AssetDatabase.GetAssetPath(texture);
                    var decoded=new Texture2D(2,2,TextureFormat.RGBA32,false);
                    try
                    {
                        if(!ImageConversion.LoadImage(decoded,File.ReadAllBytes(path),false))
                            throw new InvalidOperationException("Cannot decode original icon PNG: "+path);
                        source=LabelUiSource(decoded);sources.Add(texture,source);
                    }
                    finally {UnityEngine.Object.DestroyImmediate(decoded);}
                }
                var counts=new int[source.Bounds.Count];
                var bands=type==7?2:1;
                for(var band=0;band<bands;band++)
                {
                    var allowed=type==7?art.AxeIconBand(quality,band):region;
                    var left=Mathf.Clamp(Mathf.RoundToInt(allowed.xMin*source.Width),0,source.Width);
                    var right=Mathf.Clamp(Mathf.RoundToInt(allowed.xMax*source.Width),0,source.Width);
                    var bottom=Mathf.Clamp(Mathf.RoundToInt(allowed.yMin*source.Height),0,source.Height);
                    var top=Mathf.Clamp(Mathf.RoundToInt(allowed.yMax*source.Height),0,source.Height);
                    for(var y=bottom;y<top;y++)for(var x=left;x<right;x++)
                        counts[source.Labels[y*source.Width+x]]++;
                }
                var winner=0;
                for(var label=1;label<counts.Length;label++)
                    if(winner==0||counts[label]>counts[winner])winner=label;
                if(winner==0||counts[winner]==0)
                    throw new InvalidOperationException($"Empty visible UI icon {type}/{quality}.");
                var bounds=source.Bounds[winner];
                // Preserve the complete primary silhouette, including portions outside
                // nominal tiles, plus two source pixels of antialias fringe. The axe
                // renderer still intersects this AABB with its two approved masks.
                for(var fringe=2;fringe>=0;fringe--)
                {
                    var uv=Rect.MinMaxRect(
                        Mathf.Max(0f,(bounds.xMin-fringe)/(float)source.Width),
                        Mathf.Max(0f,(bounds.yMin-fringe)/(float)source.Height),
                        Mathf.Min(1f,(bounds.xMax+fringe)/(float)source.Width),
                        Mathf.Min(1f,(bounds.yMax+fringe)/(float)source.Height));
                    var contaminated=false;
                    for(var band=0;band<bands;band++)
                    {
                        var allowed=uv;
                        if(type==7)
                        {
                            var mask=art.AxeIconBand(quality,band);
                            allowed=Rect.MinMaxRect(Mathf.Max(uv.xMin,mask.xMin),Mathf.Max(uv.yMin,mask.yMin),
                                Mathf.Min(uv.xMax,mask.xMax),Mathf.Min(uv.yMax,mask.yMax));
                        }
                        contaminated|=ContainsOtherUiSilhouette(source,allowed,winner);
                    }
                    if(!contaminated){result[type*8+quality]=uv;break;}
                    if(fringe==0)throw new InvalidOperationException(
                        $"UI icon {type}/{quality} needs an explicit projection mask: neighbour overlaps its visible bounds.");
                }
            }
            art.uiVisibleBounds=result;
            if(art.potions!=null)
            {
                art.potionVisibleBounds=new Rect[3];
                for(var potion=0;potion<3;potion++)
                {
                    art.TryGetIcon(11+potion,0,out var texture,out var region);
                    art.potionVisibleBounds[potion]=IsolatedCellBounds(texture,region);
                }
            }
            Debug.Log($"S3_UI_ICON_BOUNDS_READY: {result.Length}/{result.Length}; isolated new atlas cells include every alpha island; alpha>=16/255; legacy largest connected silhouette by cell overlap; "+
                "full-source component bounds (not clipped to nominal tile); up to2 source pixels fringe, reduced before neighbour; "+
                "legacy sheets checked for foreign silhouettes including axe masked bands; "+
                "axes retain two masked bands; bows use held Bow(q,0); uniform centred UI fit; world pivots unchanged.");
        }
        private static Rect IsolatedCellBounds(Texture2D source,Rect region)
        {
            var decoded=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                var path=AssetDatabase.GetAssetPath(source);
                if(!ImageConversion.LoadImage(decoded,File.ReadAllBytes(path),false))throw new InvalidOperationException("Cannot decode item atlas "+path);
                var pixels=decoded.GetPixels32();var width=decoded.width;var height=decoded.height;
                var left=Mathf.RoundToInt(region.xMin*width);var right=Mathf.RoundToInt(region.xMax*width);
                var bottom=Mathf.RoundToInt(region.yMin*height);var top=Mathf.RoundToInt(region.yMax*height);
                var minX=right;var maxX=left-1;var minY=top;var maxY=bottom-1;
                for(var y=bottom;y<top;y++)for(var x=left;x<right;x++)
                {
                    if(pixels[y*width+x].a<UiAlphaThreshold)continue;
                    minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);
                }
                if(maxX<minX)throw new InvalidOperationException("Empty isolated item cell in "+path+" "+region);
                return Rect.MinMaxRect(Mathf.Max(left,minX-2)/(float)width,Mathf.Max(bottom,minY-2)/(float)height,
                    Mathf.Min(right,maxX+3)/(float)width,Mathf.Min(top,maxY+3)/(float)height);
            }
            finally{UnityEngine.Object.DestroyImmediate(decoded);}
        }
        private static Texture2D Import(string file,bool readable)
        {
            var path=Folder+file;if(!File.Exists(path))throw new FileNotFoundException("Approved runtime artwork missing",path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
            importer.sRGBTexture=true;importer.alphaIsTransparency=true;importer.mipmapEnabled=true;
            importer.filterMode=FilterMode.Trilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.isReadable=readable;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        private static RectInt Visible(Texture2D texture)
        {
            var pixels=texture.GetPixels32();var minX=texture.width;var minY=texture.height;var maxX=0;var maxY=0;
            for(var y=0;y<texture.height;y++)for(var x=0;x<texture.width;x++)
                if(pixels[y*texture.width+x].a>=128){minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
            return new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);
        }
    }
}
