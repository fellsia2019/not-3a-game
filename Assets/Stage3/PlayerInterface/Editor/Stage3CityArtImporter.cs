using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Not3A.Stage3.VisualPilot;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    /// <summary>Original PNG bytes remain untouched; only native sprite bounds/material bindings are imported.</summary>
    public static class Stage3CityArtImporter
    {
        public const string Folder = "Assets/Stage3/WorldTravel/Resources/";
        public const string ArtPath = Folder + "Stage3CityArt.asset";
        [MenuItem("Tools/Stage 3/City/Import Approved Modular Art")]
        public static void BuildAssets()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before importing city art.");
            var art=AssetDatabase.LoadAssetAtPath<Stage3CityArt>(ArtPath);
            if(art==null){art=ScriptableObject.CreateInstance<Stage3CityArt>();AssetDatabase.CreateAsset(art,ArtPath);}
            art.shop=ImportStandalone(art,"Stage3CityShop.png","City trading house",9);
            art.temple=ImportStandalone(art,"Stage3CityTemple.png","City temple",3);
            var kit=ImportTexture("Stage3CityKit.png",false);
            var source=Decode("Stage3CityKit.png");
            try
            {
                if(art.kit==null || art.kit.Length!=12)art.kit=new Sprite[12];
                if(source.width!=1448 || source.height!=1086)
                    throw new InvalidOperationException("City kit dimensions changed; remeasure isolated source regions.");
                // Measured full silhouettes in the approved source (top-left pixels).
                // Roofs cross nominal thirds; no equal-cell slicing or PNG rewrite.
                var measured=new RectInt[]
                {
                    new RectInt(16,29,328,360),new RectInt(373,61,344,347),
                    new RectInt(738,9,358,407),new RectInt(1114,29,312,394),
                    new RectInt(70,411,263,305),new RectInt(439,522,226,189),
                    new RectInt(788,497,273,218),new RectInt(1181,508,191,202),
                    new RectInt(101,746,167,311),new RectInt(472,749,129,304),
                    new RectInt(830,750,117,295),new RectInt(1112,746,268,291)
                };
                for(var i=0;i<12;i++)
                {
                    var m=measured[i];
                    var bounds=new RectInt(m.x,source.height-m.yMax,m.width,m.height);
                    var padded=Pad(bounds,new RectInt(0,0,source.width,source.height),4);
                    art.kit[i]=CreateOrReuse(art,kit,"City kit "+i,padded,bounds.yMin);
                    Debug.Log("S3_CITY_KIT_BOUNDS: "+i+" "+bounds+"; alpha>=128 full connected silhouette, 4px fringe, isolated measured region.");
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(source);}
            var paving=ImportTexture("Stage3CityPaving.png",true);
            art.spriteMaterial=Material("Stage3CityInk.mat","Not3A/Resource District Sprite");
            art.shopMaterial=Material("Stage3CityShopInk.mat","Not3A/City Sprite");
            art.shopMaterial.SetColor("_OutlineColor",new Color(.48f,.90f,.87f,.95f));
            art.shopMaterial.SetFloat("_OutlinePixels",5f);
            art.pavingMaterial=Material("Stage3CityPaving.mat","Not3A/City Paving");
            art.pavingMaterial.SetTexture("_BaseMap",paving);
            art.pavingMaterial.SetColor("_BaseColor",Color.white);
            var districtArt=AssetDatabase.LoadAssetAtPath<Stage3ResourceDistrictArt>("Assets/Stage3/ResourceDistricts/ResourceDistrictArt.asset");
            art.gardenTrees=districtArt!=null?districtArt.trees:Array.Empty<Sprite>();
            ImportGardens(art);
            ImportArrivalPreviews(art);
            EditorUtility.SetDirty(art.shopMaterial);EditorUtility.SetDirty(art.pavingMaterial);
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("S3_CITY_ART_READY: 2 buildings + 12 isolated kit sprites, native tiled ground, original PNGs unchanged.");
        }

        private static void ImportGardens(Stage3CityArt art)
        {
            const string file="Stage3CityGardenKit.png";
            if(!File.Exists(Folder+file))return;
            var texture=ImportTexture(file,false);var source=Decode(file);
            try
            {
                if(source.width!=1536||source.height!=1024)
                    throw new InvalidOperationException("Garden sheet changed; remeasure its isolated components.");
                // Full alpha-component bounds, top-left source coordinates. The two shrubs
                // overlap in rectangular X bounds; native silhouette meshes exclude their neighbours.
                var regions=new[]{new RectInt(24,120,522,344),new RectInt(542,113,519,348),
                    new RectInt(1084,120,420,325),new RectInt(74,572,494,354),
                    new RectInt(709,527,117,402),new RectInt(931,623,566,315)};
                var pixels=source.GetPixels32();art.gardens=new Sprite[6];
                for(var i=0;i<regions.Length;i++)
                {
                    var r=regions[i];var bounds=new RectInt(r.x,source.height-r.yMax,r.width,r.height);
                    var padded=Pad(bounds,new RectInt(0,0,source.width,source.height),2);
                    var sprite=CreateOrReuse(art,texture,"City garden "+i,padded,bounds.yMin);
                    ApplyGardenSilhouette(sprite,pixels,source.width,bounds,padded);
                    art.gardens[i]=sprite;EditorUtility.SetDirty(sprite);
                    Debug.Log("S3_CITY_GARDEN_BOUNDS: "+i+" "+bounds+"; alpha>=16 component mesh; unchanged PNG.");
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(source);}
        }

        private static void ApplyGardenSilhouette(Sprite sprite,Color32[] pixels,int width,RectInt bounds,RectInt padded)
        {
            var visited=new HashSet<int>();var queue=new Queue<int>();var largest=new List<int>();
            for(var y=bounds.yMin;y<bounds.yMax;y++)for(var x=bounds.xMin;x<bounds.xMax;x++)
            {
                var start=y*width+x;if(pixels[start].a<16||visited.Contains(start))continue;
                var component=new List<int>();queue.Enqueue(start);visited.Add(start);
                while(queue.Count>0)
                {
                    var k=queue.Dequeue();component.Add(k);var px=k%width;var py=k/width;
                    Visit(px-1,py);Visit(px+1,py);Visit(px,py-1);Visit(px,py+1);
                }
                if(component.Count>largest.Count)largest=component;
            }
            void Visit(int x,int y)
            {
                if(x<bounds.xMin||x>=bounds.xMax||y<bounds.yMin||y>=bounds.yMax)return;
                var k=y*width+x;if(pixels[k].a<16||!visited.Add(k))return;queue.Enqueue(k);
            }
            var points=new HashSet<Vector2>();var own=new HashSet<int>(largest);
            foreach(var k in largest)
            {
                if(own.Contains(k-1)&&own.Contains(k+1)&&own.Contains(k-width)&&own.Contains(k+width))continue;
                var x=k%width;var y=k/width;
                points.Add(new Vector2(x,y));points.Add(new Vector2(x+1,y));
                points.Add(new Vector2(x,y+1));points.Add(new Vector2(x+1,y+1));
            }
            var sorted=new List<Vector2>(points);sorted.Sort((a,b)=>a.x==b.x?a.y.CompareTo(b.y):a.x.CompareTo(b.x));
            var hull=new List<Vector2>();
            foreach(var p in sorted){while(hull.Count>=2&&Cross(hull[hull.Count-2],hull[hull.Count-1],p)<=0)hull.RemoveAt(hull.Count-1);hull.Add(p);}
            var lower=hull.Count;
            for(var n=sorted.Count-2;n>=0;n--){var p=sorted[n];while(hull.Count>lower&&Cross(hull[hull.Count-2],hull[hull.Count-1],p)<=0)hull.RemoveAt(hull.Count-1);hull.Add(p);}
            hull.RemoveAt(hull.Count-1);
            var vertices=new Vector2[hull.Count];var triangles=new ushort[(hull.Count-2)*3];
            for(var n=0;n<hull.Count;n++)vertices[n]=(hull[n]-new Vector2(padded.x,padded.y)-sprite.pivot)/sprite.pixelsPerUnit;
            for(var n=0;n<hull.Count-2;n++){triangles[n*3]=0;triangles[n*3+1]=(ushort)(n+1);triangles[n*3+2]=(ushort)(n+2);}
            sprite.OverrideGeometry(vertices,triangles);
        }
        private static float Cross(Vector2 o,Vector2 a,Vector2 b)=>(a.x-o.x)*(b.y-o.y)-(a.y-o.y)*(b.x-o.x);

        private static void ImportArrivalPreviews(Stage3CityArt art)
        {
            const string file="Stage3TeleportPreviews.png";
            if(!File.Exists(Folder+file))return;
            var texture=ImportTexture(file,false);
            if(texture.width!=1774 || texture.height!=887)
                throw new InvalidOperationException("Teleport preview source changed; remeasure illustration bounds.");
            // Opaque illustrated interiors; exclude the transparent sheet margins and uneven AA fringe.
            var left=new RectInt(80,107,768,670);
            var right=new RectInt(926,107,768,670);
            art.baseArrivalPreview=CreateOrReuse(art,texture,"Base arrival illustration",left,left.y);
            art.cityArrivalPreview=CreateOrReuse(art,texture,"City arrival illustration",right,right.y);
        }

        private static Sprite ImportStandalone(Stage3CityArt owner,string file,string label,int padding)
        {
            var texture=ImportTexture(file,false);var source=Decode(file);
            try
            {
                var full=new RectInt(0,0,source.width,source.height);
                var bounds=VisibleBounds(source.GetPixels32(),source.width,full);
                return CreateOrReuse(owner,texture,label,Pad(bounds,full,padding),bounds.yMin);
            }
            finally{UnityEngine.Object.DestroyImmediate(source);}
        }
        private static RectInt VisibleBounds(Color32[] pixels,int width,RectInt within)
        {
            var x0=within.xMax;var y0=within.yMax;var x1=-1;var y1=-1;
            for(var y=within.yMin;y<within.yMax;y++)for(var x=within.xMin;x<within.xMax;x++)
            {
                if(pixels[y*width+x].a<16)continue;
                x0=Mathf.Min(x0,x);y0=Mathf.Min(y0,y);x1=Mathf.Max(x1,x);y1=Mathf.Max(y1,y);
            }
            if(x1<x0)throw new InvalidOperationException("City artwork contains an empty sprite region.");
            return new RectInt(x0,y0,x1-x0+1,y1-y0+1);
        }
        private static RectInt Pad(RectInt r,RectInt limit,int padding)
        {
            var x=Mathf.Max(limit.xMin,r.xMin-padding);var y=Mathf.Max(limit.yMin,r.yMin-padding);
            return new RectInt(x,y,Mathf.Min(limit.xMax,r.xMax+padding)-x,Mathf.Min(limit.yMax,r.yMax+padding)-y);
        }
        private static Sprite CreateOrReuse(Stage3CityArt owner,Texture2D texture,string label,RectInt pixels,int baseline)
        {
            var rect=new Rect(pixels.x,pixels.y,pixels.width,pixels.height);
            foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(ArtPath))
                if(asset is Sprite existing && existing.name==label)
                {
                    if(existing.texture!=texture || existing.rect!=rect)
                        throw new InvalidOperationException("Existing city sprite source changed: "+label+". Explicit migration required.");
                    return existing;
                }
            var pivot=new Vector2(.5f,(baseline-pixels.y)/(float)pixels.height);
            var sprite=Sprite.Create(texture,rect,pivot,100f,0,SpriteMeshType.FullRect);
            sprite.name=label;AssetDatabase.AddObjectToAsset(sprite,owner);return sprite;
        }
        private static Texture2D Decode(string file)
        {
            var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
            if(!ImageConversion.LoadImage(source,File.ReadAllBytes(Folder+file),false))
            {UnityEngine.Object.DestroyImmediate(source);throw new InvalidOperationException("Invalid city PNG: "+file);}
            return source;
        }
        private static Texture2D ImportTexture(string file,bool repeat)
        {
            var path=Folder+file;
            if(!File.Exists(path))throw new FileNotFoundException("Approved city source is missing.",path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=!repeat;
            importer.mipmapEnabled=repeat;importer.isReadable=false;importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.wrapMode=repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        private static Material Material(string file,string shaderName)
        {
            var path=Folder+file;var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader=Shader.Find(shaderName);
            if(shader==null)throw new InvalidOperationException("City shader missing: "+shaderName);
            if(material!=null){if(material.shader!=shader){material.shader=shader;EditorUtility.SetDirty(material);}return material;}
            material=new Material(shader){name=Path.GetFileNameWithoutExtension(file)};
            AssetDatabase.CreateAsset(material,path);return material;
        }
    }
}
