using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Not3A.Stage3.VisualPilot;

namespace Not3A.Stage3.PlayerInterface.Editor
{
    /// <summary>Measures approved source islands at import time; keeps all original bitmap bytes.</summary>
    public static class Stage3CampArtImporter
    {
        public const string Folder="Assets/Stage3/Guild/Resources/";
        public const string ArtPath=Folder+"Stage3CampArt.asset";
        [MenuItem("Tools/Stage 3/Guild/Import Approved Open Camp Art")]
        public static void BuildAssets()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before camp art import.");
            if(!AssetDatabase.IsValidFolder(Folder.TrimEnd('/')))
                AssetDatabase.CreateFolder("Assets/Stage3/Guild","Resources");
            var art=AssetDatabase.LoadAssetAtPath<Stage3CampArt>(ArtPath);
            if(art==null){art=ScriptableObject.CreateInstance<Stage3CampArt>();AssetDatabase.CreateAsset(art,ArtPath);}
            art.orc=Import(art,"Stage3CampOrcKit.png","Orc camp");
            art.ogre=Import(art,"Stage3CampOgreKit.png","Ogre camp");
            var district=AssetDatabase.LoadAssetAtPath<Stage3ResourceDistrictArt>("Assets/Stage3/ResourceDistricts/ResourceDistrictArt.asset");
            if(district==null||district.material==null||district.groundBlendMaterial==null)
                throw new InvalidOperationException("Existing district sprite and ground materials must be installed first.");
            art.spriteMaterial=district.material;art.trees=district.trees;art.rocks=district.rocks;
            var city=AssetDatabase.LoadAssetAtPath<Stage3CityArt>("Assets/Stage3/WorldTravel/Resources/Stage3CityArt.asset");
            art.lowPlants=city!=null&&city.gardens!=null&&city.gardens.Length>5?city.gardens[5]:null;
            const string groundPath=Folder+"Stage3CampGround.mat";
            var ground=AssetDatabase.LoadAssetAtPath<Material>(groundPath);
            if(ground==null){ground=new Material(district.groundBlendMaterial){name="Camp ground beneath sprites"};AssetDatabase.CreateAsset(ground,groundPath);}
            else ground.CopyPropertiesFromMaterial(district.groundBlendMaterial);
            // Transparent mesh sortingOrder=0 otherwise wins over world sprite orders,
            // making perfectly opaque tents appear translucent beneath the ground wash.
            ground.renderQueue=2450;EditorUtility.SetDirty(ground);art.groundMaterial=ground;
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("S3_CAMP_ART_READY: twelve measured original modular props; shared existing materials; no runtime alpha scan.");
        }
        private static Sprite[] Import(Stage3CampArt owner,string file,string prefix)
        {
            var path=Folder+file;
            if(!File.Exists(path))throw new FileNotFoundException("Approved camp source copy is missing.",path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
            importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;
            importer.isReadable=false;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var decoded=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                if(!ImageConversion.LoadImage(decoded,File.ReadAllBytes(path),false))throw new InvalidOperationException("Invalid camp PNG: "+path);
                if(decoded.width!=1536||decoded.height!=1024)
                    throw new InvalidOperationException("Camp sheet dimensions changed; remeasure the approved group regions.");
                // Measured groups in source top-left coordinates, not nominal atlas cells.
                // Ogre logs are two separate alpha islands; shelter also has detached stones.
                var regions=prefix=="Orc camp"?new[]{
                    new RectInt(34,55,650,458),new RectInt(733,147,380,363),new RectInt(1190,169,310,327),
                    new RectInt(90,620,431,309),new RectInt(719,562,225,380),new RectInt(1135,667,357,250)}:
                    new[]{new RectInt(28,45,638,489),new RectInt(707,153,464,372),new RectInt(1217,145,285,365),
                    new RectInt(63,661,406,268),new RectInt(538,637,475,303),new RectInt(1108,640,385,294)};
                var pixels=decoded.GetPixels32();var bounds=new RectInt[6];
                for(var i=0;i<regions.Length;i++)
                {
                    var r=regions[i];var region=new RectInt(r.x,decoded.height-r.yMax,r.width,r.height);
                    bounds[i]=VisibleBounds(pixels,decoded.width,region);
                    for(var j=0;j<i;j++)if(bounds[i].Overlaps(bounds[j]))
                        throw new InvalidOperationException("Camp groups overlap; source review required.");
                }
                var result=new Sprite[6];
                for(var i=0;i<result.Length;i++)
                {
                    var b=bounds[i];
                    var x=Mathf.Max(0,b.x-3);var y=Mathf.Max(0,b.y-3);
                    var rect=new Rect(x,y,Mathf.Min(decoded.width,b.xMax+3)-x,Mathf.Min(decoded.height,b.yMax+3)-y);
                    var name=prefix+" prop "+i;Sprite sprite=null;
                    foreach(var existing in AssetDatabase.LoadAllAssetsAtPath(ArtPath))
                        if(existing is Sprite found&&found.name==name)
                        {
                            if(found.texture!=texture||found.rect!=rect)throw new InvalidOperationException("Camp source bounds changed; explicit migration required: "+name);
                            sprite=found;break;
                        }
                    if(sprite==null)
                    {
                        sprite=Sprite.Create(texture,rect,new Vector2(.5f,(b.y-y)/rect.height),100f,0,SpriteMeshType.FullRect);
                        sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,owner);
                    }
                    result[i]=sprite;
                    Debug.Log("S3_CAMP_ART_BOUNDS: "+name+" "+b+"; alpha>=16 measured full group, 3px margin; separate ogre logs retained.");
                }
                return result;
            }
            finally{UnityEngine.Object.DestroyImmediate(decoded);}
        }
        private static RectInt VisibleBounds(Color32[] pixels,int width,RectInt region)
        {
            var minX=region.xMax;var minY=region.yMax;var maxX=-1;var maxY=-1;
            for(var y=region.yMin;y<region.yMax;y++)for(var x=region.xMin;x<region.xMax;x++)
            {
                if(pixels[y*width+x].a<16)continue;
                minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);
            }
            if(maxX<minX)throw new InvalidOperationException("Empty camp source group.");
            return new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);
        }
    }
}
