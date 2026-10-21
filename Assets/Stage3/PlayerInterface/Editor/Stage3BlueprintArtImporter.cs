using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3BlueprintArtImporter
    {
        public const string TexturePath = "Assets/Stage3/PlayerInterface/Resources/Stage3BlueprintItems.png";
        public const string ArtPath = "Assets/Stage3/PlayerInterface/Resources/Stage3BlueprintArt.asset";

        [MenuItem("Tools/Stage 3/Blueprints/Import Approved Art")]
        public static void BuildAssets()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before importing blueprint art.");
            AssetDatabase.ImportAsset(TexturePath,ImportAssetOptions.ForceSynchronousImport);
            var importer=AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Missing approved blueprint PNG: "+TexturePath);
            importer.textureType=TextureImporterType.Default;
            importer.sRGBTexture=true;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency=true;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=4096;
            importer.isReadable=false;
            importer.mipmapEnabled=false;
            importer.filterMode=FilterMode.Bilinear;
            importer.wrapMode=TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            var decoded=new Texture2D(2,2,TextureFormat.RGBA32,false);
            RectInt[] bounds;
            try
            {
                if(!ImageConversion.LoadImage(decoded,File.ReadAllBytes(TexturePath),false))
                    throw new InvalidOperationException("Cannot decode approved blueprint PNG.");
                bounds=FindPaperIslands(decoded.GetPixels32(),decoded.width,decoded.height);
            }
            finally { UnityEngine.Object.DestroyImmediate(decoded); }
            var art=AssetDatabase.LoadAssetAtPath<Stage3BlueprintArt>(ArtPath);
            if(art==null){art=ScriptableObject.CreateInstance<Stage3BlueprintArt>();AssetDatabase.CreateAsset(art,ArtPath);}
            if(art.Pages==null||art.Pages.Length!=3)art.Pages=new Sprite[3];
            var names=new[]{"Blueprint tower","Blueprint wand","Blueprint forge"};
            var existing=AssetDatabase.LoadAllAssetsAtPath(ArtPath);
            for(var i=0;i<3;i++)
            {
                Sprite page=null;
                foreach(var candidate in existing)
                    if(candidate is Sprite sprite&&sprite.name==names[i]){page=sprite;break;}
                var rect=new Rect(bounds[i].x,bounds[i].y,bounds[i].width,bounds[i].height);
                if(page!=null&&(page.texture!=texture||page.rect!=rect))
                    throw new InvalidOperationException("Blueprint source bounds changed; explicitly migrate existing sprite "+names[i]);
                if(page==null)
                {
                    page=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100f,0,SpriteMeshType.FullRect);
                    page.name=names[i];AssetDatabase.AddObjectToAsset(page,art);
                }
                art.Pages[i]=page;
                Debug.Log("S3_BLUEPRINT_BOUNDS: "+names[i]+" "+bounds[i]+"; alpha>=16 dominant connected paper, 2px padding.");
            }
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("S3_BLUEPRINT_ART_READY: 3/3 measured isolated sheets; no equal-tile assumption; original PNG unchanged; no runtime alpha scans.");
        }

        private static RectInt[] FindPaperIslands(Color32[] pixels,int width,int height)
        {
            var seen=new bool[pixels.Length];var queue=new Queue<int>();
            var islands=new List<RectInt>();
            for(var start=0;start<pixels.Length;start++)
            {
                if(seen[start]||pixels[start].a<16)continue;
                seen[start]=true;queue.Enqueue(start);
                var xMin=width;var yMin=height;var xMax=0;var yMax=0;var count=0;
                while(queue.Count>0)
                {
                    var index=queue.Dequeue();var x=index%width;var y=index/width;count++;
                    xMin=Mathf.Min(xMin,x);yMin=Mathf.Min(yMin,y);xMax=Mathf.Max(xMax,x);yMax=Mathf.Max(yMax,y);
                    void Visit(int next)
                    {if(!seen[next]&&pixels[next].a>=16){seen[next]=true;queue.Enqueue(next);}}
                    if(x>0)Visit(index-1);if(x+1<width)Visit(index+1);
                    if(y>0)Visit(index-width);if(y+1<height)Visit(index+width);
                }
                if(count<10000)continue; // Ignore isolated anti-alias/shadow specks.
                var left=Mathf.Max(0,xMin-2);var bottom=Mathf.Max(0,yMin-2);
                islands.Add(new RectInt(left,bottom,Mathf.Min(width,xMax+3)-left,Mathf.Min(height,yMax+3)-bottom));
            }
            if(islands.Count!=3)throw new InvalidOperationException("Expected three isolated blueprint papers; found "+islands.Count);
            islands.Sort((a,b)=>a.x.CompareTo(b.x));
            for(var i=1;i<islands.Count;i++)
                if(islands[i-1].xMax>=islands[i].xMin)
                    throw new InvalidOperationException("Blueprint paper bounds overlap; source needs review.");
            return islands.ToArray();
        }
    }
}
