using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Not3A.Stage3.VisualPilot;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    /// <summary>Imports unchanged authored pieces; no mockup text, runtime pixel scans or generated replacement artwork.</summary>
    public static class Stage3GuildArtImporter
    {
        public const string Folder="Assets/Stage3/PlayerInterface/Resources/";
        public const string ArtPath=Folder+"Stage3GuildArt.asset";
        [MenuItem("Tools/Stage 3/Guild/Import Approved Modular Art")]
        public static void BuildAssets()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before guild import.");
            var art=AssetDatabase.LoadAssetAtPath<Stage3GuildArt>(ArtPath);
            if(art==null){art=ScriptableObject.CreateInstance<Stage3GuildArt>();AssetDatabase.CreateAsset(art,ArtPath);}
            art.boardBackground=Texture("Stage3GuildBoardBackground.png");
            var parts=ImportIslands(art,"Stage3GuildParts.png",new[]{"Guild order paper","Guild detail paper","Guild rank shield","Guild wax seal","Guild action button","Guild title plaque"},3);
            art.orderPaper=parts[0];art.detailPaper=parts[1];art.rankShield=parts[2];art.waxSeal=parts[3];art.actionButton=parts[4];art.headerPlaque=parts[5];
            var world=ImportIslands(art,"Stage3GuildWorld.png",new[]{"Guild hall","Guild notice board"},2,true);
            art.guildHall=world[0];art.noticeBoard=world[1];
            var ink=ImportIslands(art,"Stage3GuildInk.png",new[]{"Guild orc illustration","Guild ogre illustration"},2);
            art.orcInk=ink[0];art.ogreInk=ink[1];
            var city=AssetDatabase.LoadAssetAtPath<Stage3CityArt>("Assets/Stage3/WorldTravel/Resources/Stage3CityArt.asset");
            if(city==null)throw new InvalidOperationException("Import existing city art before guild extension.");
            city.guild=art.guildHall;city.noticeBoard=art.noticeBoard;EditorUtility.SetDirty(city);
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("S3_GUILD_ART_READY: native wood/paper pieces, 2 ink illustrations, modular hall/board; measured alpha bounds and unchanged PNG bytes.");
        }
        private static Texture2D Texture(string file)
        {
            var path=Folder+file;if(!File.Exists(path))throw new FileNotFoundException("Missing approved guild source copy",path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.isReadable=false;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        private static Texture2D Decode(string file)
        {var source=new Texture2D(2,2,TextureFormat.RGBA32,false);if(!ImageConversion.LoadImage(source,File.ReadAllBytes(Folder+file),false))throw new InvalidOperationException("Cannot decode "+file);return source;}
        private static Sprite[] ImportIslands(Stage3GuildArt owner,string file,string[] names,int columns,bool groundPivot=false)
        {
            var texture=Texture(file);var decoded=Decode(file);
            try
            {
                var bounds=ConnectedBounds(decoded.GetPixels32(),decoded.width,decoded.height,names.Length);
                // Sort rows by the visual centre, then left to right; never assume the paint fits equal tiles.
                bounds.Sort((a,b)=>b.center.y.CompareTo(a.center.y));
                for(var row=0;row<names.Length/columns;row++)bounds.Sort(row*columns,columns,Comparer<RectInt>.Create((a,b)=>a.x.CompareTo(b.x)));
                var result=new Sprite[names.Length];
                for(var i=0;i<result.Length;i++)
                {
                    var b=bounds[i];var padded=Pad(b,decoded.width,decoded.height,3);
                    var pivot=groundPivot?new Vector2(.5f,(b.y-padded.y)/(float)padded.height):new Vector2(.5f,.5f);
                    result[i]=SpriteAsset(owner,texture,names[i],padded,pivot);
                    Debug.Log("S3_GUILD_ART_BOUNDS: "+names[i]+" "+b+" alpha>=16, connected component,3px padding.");
                }
                return result;
            }
            finally{UnityEngine.Object.DestroyImmediate(decoded);}
        }
        private static Sprite SpriteAsset(Stage3GuildArt owner,Texture2D texture,string name,RectInt bounds,Vector2 pivot)
        {
            var rect=new Rect(bounds.x,bounds.y,bounds.width,bounds.height);
            foreach(var candidate in AssetDatabase.LoadAllAssetsAtPath(ArtPath))
                if(candidate is Sprite sprite&&sprite.name==name)
                {if(sprite.texture!=texture||sprite.rect!=rect)throw new InvalidOperationException("Guild bounds changed; explicit migration required: "+name);return sprite;}
            var created=Sprite.Create(texture,rect,pivot,100f,0,SpriteMeshType.FullRect);created.name=name;AssetDatabase.AddObjectToAsset(created,owner);return created;
        }
        private static RectInt Pad(RectInt b,int width,int height,int padding)
        {var x=Mathf.Max(0,b.x-padding);var y=Mathf.Max(0,b.y-padding);return new RectInt(x,y,Mathf.Min(width,b.xMax+padding)-x,Mathf.Min(height,b.yMax+padding)-y);}
        internal static List<RectInt> ConnectedBounds(Color32[] pixels,int width,int height,int expected)
        {
            var seen=new bool[pixels.Length];var queue=new Queue<int>();var components=new List<(int count,RectInt bounds)>();
            for(var start=0;start<pixels.Length;start++)
            {
                if(seen[start]||pixels[start].a<16)continue;seen[start]=true;queue.Enqueue(start);
                var minX=width;var minY=height;var maxX=0;var maxY=0;var count=0;
                while(queue.Count>0)
                {
                    var at=queue.Dequeue();var x=at%width;var y=at/width;count++;
                    minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);
                    void Add(int p){if(!seen[p]&&pixels[p].a>=16){seen[p]=true;queue.Enqueue(p);}}
                    if(x>0)Add(at-1);if(x<width-1)Add(at+1);if(y>0)Add(at-width);if(y<height-1)Add(at+width);
                }
                if(count>500)components.Add((count,new RectInt(minX,minY,maxX-minX+1,maxY-minY+1)));
            }
            components.Sort((a,b)=>b.count.CompareTo(a.count));
            if(components.Count<expected)throw new InvalidOperationException("Not enough independent guild art islands: "+components.Count);
            var result=new List<RectInt>();for(var i=0;i<expected;i++)result.Add(components[i].bounds);
            for(var i=0;i<result.Count;i++)for(var j=i+1;j<result.Count;j++)if(result[i].Overlaps(result[j]))throw new InvalidOperationException("Overlapping guild art components; source review required.");
            return result;
        }
    }
}
