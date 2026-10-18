using System;
using System.IO;
using System.Linq;
using Not3A.Stage3.VisualPilot;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    /// <summary>Copies original ImageGen PNG bytes; sprite rectangles/nine-slice borders are Unity metadata only.</summary>
    public static class Stage3FantasyHudArtImporter
    {
        public const string Root="Assets/Stage3/PlayerInterface";
        public const string ArtPath=Root+"/LunarInterfaceArt.asset";
        private const string Source="art/concepts/interface_runtime_2026-10-18/";

        [MenuItem("Tools/Stage 3/Player Interface/Import Lunar Art")]
        public static void Import()
        {
            Directory.CreateDirectory(Root+"/Art");AssetDatabase.Refresh();
            var frames=CopyTexture("lunar_frames_v1.png","LunarFrames.png");
            var icons=CopyTexture("lunar_icons_v1.png","LunarIcons.png");
            var portrait=CopyTexture("fullwidth_hud_C_v3.png","ApprovedHudPortraitSource.png");
            if(frames.width!=1403||frames.height!=1121||icons.width!=1774||icons.height!=887)
                throw new InvalidOperationException("Lunar atlas dimensions changed; inspect the new art and update source rectangles first.");
            var art=AssetDatabase.LoadAssetAtPath<Stage3FantasyHudArt>(ArtPath);
            if(art==null) {art=ScriptableObject.CreateInstance<Stage3FantasyHudArt>();AssetDatabase.CreateAsset(art,ArtPath);}
            art.windowFrame=Slice(art,frames,"Window frame",27,29,793,379,new Vector4(155,85,155,85));
            art.panelFrame=Slice(art,frames,"Pearl inset",818,129,1383,274,new Vector4(68,40,68,40));
            art.buttonNormal=Slice(art,frames,"Plum button",114,405,676,575,new Vector4(95,40,95,40));
            art.buttonSelected=Slice(art,frames,"Selected button",756,403,1339,576,new Vector4(100,42,100,42));
            art.minimapFrame=Slice(art,frames,"Map bezel",114,597,641,1068,Vector4.zero);
            art.medallionFrame=Slice(art,frames,"Portrait bezel",820,595,1304,1074,Vector4.zero);
            art.resourceIcons=new[]{
                Slice(art,icons,"Gold",15,191,299,411,Vector4.zero),
                Slice(art,icons,"Wood",309,164,621,426,Vector4.zero),
                Slice(art,icons,"Stone",620,200,888,418,Vector4.zero),
                Slice(art,icons,"Metal",901,193,1188,414,Vector4.zero)};
            art.menuIcons=new[]{
                Slice(art,icons,"Inventory",1190,146,1466,430,Vector4.zero),
                Slice(art,icons,"Construction",1469,133,1753,449,Vector4.zero),
                Slice(art,icons,"Research",6,555,377,823,Vector4.zero),
                Slice(art,icons,"Hero",369,520,654,791,Vector4.zero),
                Slice(art,icons,"Map",646,515,987,790,Vector4.zero)};
            art.bowIcon=Slice(art,icons,"Bow",986,463,1200,826,Vector4.zero);
            art.sunIcon=Slice(art,icons,"Sun",1220,511,1501,784,Vector4.zero);
            art.closeIcon=Slice(art,icons,"Close",1536,554,1742,753,Vector4.zero);
            art.heroPortrait=Slice(art,portrait,"Approved hero portrait",342,765,475,898,Vector4.zero);
            var hero=AssetDatabase.LoadAssetAtPath<Stage3HeroArt>("Assets/Stage3/DayNight/HeroArt.asset");
            art.inventoryHero=hero!=null&&hero.idleDirections!=null&&hero.idleDirections.Length>3?hero.idleDirections[3]:null;
            art.defenseIcons=new[]{TowerIcon("Assets/Stage3/CartoonWorldPilot/RapidSprites.asset"),TowerIcon("Assets/Stage3/CartoonWorldPilot/LongshotSprites.asset"),TowerIcon("Assets/Stage3/Barricades/WoodenBarricadeSprites.asset")};
            var home=AssetDatabase.LoadAssetAtPath<Stage3DwellingCatalog>("Assets/Stage3/DayNight/DwellingCatalog.asset");
            art.dwellingIcon=home!=null?home.sprite:null;
            var travel=AssetDatabase.LoadAssetAtPath<Stage3WorldTravelData>("Assets/Stage3/WorldTravel/WorldTravelData.asset");
            art.workshopIcon=travel!=null?travel.workshopSprite:null;
            var seal=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Stage3/WorldTravel/TeleportSeal.png");
            if(seal!=null) art.teleportIcon=Slice(art,seal,"Approved ground seal",0,0,seal.width,seal.height,Vector4.zero);
            art.circularMapShader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/LunarCircularMap.shader");
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("S3_LUNAR_ART_READY: original PNG bytes preserved; inspected source rectangles and native sliced sprites imported.");
        }

        private static Sprite TowerIcon(string path)
        {var set=AssetDatabase.LoadAssetAtPath<Stage3TowerSpriteSet>(path);return set!=null?set.Frame(0,0):null;}

        private static Texture2D CopyTexture(string sourceName,string assetName)
        {
            var source=Path.GetFullPath(Source+sourceName);var destination=Root+"/Art/"+assetName;
            if(!File.Exists(source)) throw new FileNotFoundException("ImageGen source is missing",source);
            var bytes=File.ReadAllBytes(source);
            if(!File.Exists(destination)||!bytes.SequenceEqual(File.ReadAllBytes(destination))) File.WriteAllBytes(destination,bytes);
            AssetDatabase.ImportAsset(destination,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(destination);
            importer.textureType=TextureImporterType.Default; importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency=true; importer.mipmapEnabled=true; importer.sRGBTexture=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.maxTextureSize=4096;
            importer.filterMode=FilterMode.Trilinear; importer.wrapMode=TextureWrapMode.Clamp;
            importer.npotScale=TextureImporterNPOTScale.None; importer.isReadable=false;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(destination);
        }

        // Bounds are inspected image coordinates, top-left origin; asset pixels are never cropped/repainted.
        private static Sprite Slice(Stage3FantasyHudArt art,Texture2D texture,string name,int left,int top,int right,int bottom,Vector4 border)
        {
            var rect=new Rect(left,texture.height-bottom,right-left,bottom-top);
            if(rect.xMin<0||rect.yMin<0||rect.xMax>texture.width||rect.yMax>texture.height)
                throw new InvalidOperationException("Invalid inspected sprite rectangle: "+name);
            var existing=AssetDatabase.LoadAllAssetsAtPath(ArtPath).OfType<Sprite>().FirstOrDefault(s=>s.name==name&&s.texture==texture&&s.rect==rect&&s.border==border);
            if(existing!=null) return existing;
            var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100f,0,SpriteMeshType.FullRect,border);
            sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,art);return sprite;
        }
    }
}
