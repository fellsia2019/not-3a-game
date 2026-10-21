using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    public static class Stage3CycleArtImporter
    {
        public const string TexturePath="Assets/Stage3/PlayerInterface/Resources/Stage3CycleDial.png";
        public const string ArtPath="Assets/Stage3/PlayerInterface/Resources/Stage3CycleArt.asset";
        [MenuItem("Tools/Stage 3/Cycle Dial/Import Approved Art")]
        public static void BuildAssets()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before cycle art import.");
            AssetDatabase.ImportAsset(TexturePath,ImportAssetOptions.ForceSynchronousImport);
            var importer=AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Missing approved cycle PNG.");
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
            importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=4096;importer.isReadable=false;importer.mipmapEnabled=false;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            var decoded=new Texture2D(2,2,TextureFormat.RGBA32,false);RectInt[] bounds;
            try
            {
                if(!ImageConversion.LoadImage(decoded,File.ReadAllBytes(TexturePath),false))throw new InvalidOperationException("Cannot decode cycle PNG.");
                bounds=FindIslands(decoded.GetPixels32(),decoded.width,decoded.height);
            }
            finally { UnityEngine.Object.DestroyImmediate(decoded); }
            var art=AssetDatabase.LoadAssetAtPath<Stage3CycleArt>(ArtPath);
            if(art==null){art=ScriptableObject.CreateInstance<Stage3CycleArt>();AssetDatabase.CreateAsset(art,ArtPath);}
            var existing=AssetDatabase.LoadAllAssetsAtPath(ArtPath);
            var sprites=new Sprite[2];var names=new[]{"Cycle smoky face","Cycle moving hand"};
            for(var i=0;i<2;i++)
            {
                var b=bounds[i];var rect=new Rect(b.x,b.y,b.width,b.height);
                foreach(var candidate in existing)if(candidate is Sprite sprite&&sprite.name==names[i]){sprites[i]=sprite;break;}
                if(sprites[i]!=null&&(sprites[i].texture!=texture||sprites[i].rect!=rect))
                    throw new InvalidOperationException("Cycle sprite bounds changed; explicit migration required: "+names[i]);
                if(sprites[i]==null)
                {
                    sprites[i]=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100f,0,SpriteMeshType.FullRect);
                    sprites[i].name=names[i];AssetDatabase.AddObjectToAsset(sprites[i],art);
                }
                Debug.Log("S3_CYCLE_ART_BOUNDS: "+names[i]+" "+b+"; alpha>=16 connected silhouette,2px padding.");
            }
            art.Face=sprites[0];art.Hand=sprites[1];
            // Separate upright hand ends in a round pivot bead: its centre is half its width from the bottom.
            art.HandPivot=new Vector2(.5f,bounds[1].width*.5f/bounds[1].height);
            art.HandShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Stage3/PlayerInterface/CycleHand.shader");
            if(art.HandShader==null)throw new InvalidOperationException("Cycle hand shader missing.");
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("S3_CYCLE_ART_READY:2/2 measured sprites; hand pivot="+art.HandPivot+"; byte-identical approved source, no runtime alpha scans.");
        }

        private static RectInt[] FindIslands(Color32[] pixels,int width,int height)
        {
            var seen=new bool[pixels.Length];var queue=new Queue<int>();var islands=new List<RectInt>();
            for(var start=0;start<pixels.Length;start++)
            {
                if(seen[start]||pixels[start].a<16)continue;
                seen[start]=true;queue.Enqueue(start);
                var minX=width;var minY=height;var maxX=0;var maxY=0;var count=0;
                while(queue.Count>0)
                {
                    var index=queue.Dequeue();var x=index%width;var y=index/width;count++;
                    minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);
                    void Visit(int next){if(!seen[next]&&pixels[next].a>=16){seen[next]=true;queue.Enqueue(next);}}
                    if(x>0)Visit(index-1);if(x+1<width)Visit(index+1);if(y>0)Visit(index-width);if(y+1<height)Visit(index+width);
                }
                if(count<10000)continue;
                var left=Mathf.Max(0,minX-2);var bottom=Mathf.Max(0,minY-2);
                islands.Add(new RectInt(left,bottom,Mathf.Min(width,maxX+3)-left,Mathf.Min(height,maxY+3)-bottom));
            }
            if(islands.Count!=2)throw new InvalidOperationException("Expected2 cycle art islands, found "+islands.Count);
            islands.Sort((a,b)=>a.x.CompareTo(b.x));
            for(var i=1;i<islands.Count;i++)if(islands[i-1].xMax>=islands[i].xMin)
                throw new InvalidOperationException("Cycle art bounding rectangles overlap; source needs review.");
            return islands.ToArray();
        }
    }
}
