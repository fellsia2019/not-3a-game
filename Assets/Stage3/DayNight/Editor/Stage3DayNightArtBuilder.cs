using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>Imports unchanged approved ImageGen rasters; sprite metadata alone provides slicing and registration.</summary>
    public static class Stage3DayNightArtBuilder
    {
        public const string Folder = "Assets/Stage3/DayNight/";
        public const string CatalogPath = Folder + "DwellingCatalog.asset";
        public const string HeroArtPath = Folder + "HeroArt.asset";
        public const string GroundMaterialPath = Folder + "DayNightGround.mat";
        private const string Source = "art/concepts/day_night_and_home_2026-10-18/";
        private const string MotionSource = "art/concepts/hero_motion_2026-10-18/";

        [MenuItem("Tools/Stage 3/Day and Night/Build Approved Art")]
        public static void BuildAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before importing art.");
            var body = Import("hero_C_body_walk_v1.png", "HeroCBody.png");
            var bows = Import("hero_C_bow_directions_v1.png", "HeroCBows.png");
            var home = Import("dwelling_B_front_v1.png", "DwellingB.png");
            var art = LoadOrCreate<Stage3HeroArt>(HeroArtPath);
            var bodyRects = new RectInt[16]; var bodyVisible = new RectInt[16];
            var maxHeight = 0;
            // Transparent row gutters measured on the approved 1254px atlas. Equal quarters cut the last row's hair.
            var rowCuts=new[]{0f,316f,622f,931f,1254f};
            for (var row=0;row<4;row++) for(var col=0;col<4;col++)
            {
                var x0=Mathf.RoundToInt(col*body.width/4f); var x1=Mathf.RoundToInt((col+1)*body.width/4f);
                var y0=body.height-Mathf.RoundToInt(rowCuts[row+1]*body.height/1254f);
                var y1=body.height-Mathf.RoundToInt(rowCuts[row]*body.height/1254f);
                var index=row*4+col; bodyRects[index]=new RectInt(x0,y0,x1-x0,y1-y0);
                bodyVisible[index]=Visible(body,bodyRects[index]); maxHeight=Mathf.Max(maxHeight,bodyVisible[index].height);
            }
            var ppu=maxHeight/1.15f;
            art.bodyFrames=new Sprite[16]; art.gripOffsets=new Vector2[16];
            // Hand centers in each original 4x4 cell, authored from the approved atlas. Y is top-down here.
            var hands = new[] {
                new Vector2(.82f,.57f),new Vector2(.81f,.58f),new Vector2(.75f,.55f),new Vector2(.70f,.57f),
                new Vector2(.77f,.46f),new Vector2(.78f,.45f),new Vector2(.78f,.44f),new Vector2(.77f,.42f),
                new Vector2(.30f,.55f),new Vector2(.34f,.57f),new Vector2(.32f,.51f),new Vector2(.43f,.57f),
                new Vector2(.70f,.55f),new Vector2(.69f,.48f),new Vector2(.72f,.53f),new Vector2(.69f,.58f)
            };
            for(var i=0;i<16;i++)
            {
                var rect=bodyRects[i]; var visible=bodyVisible[i];
                var pivot=new Vector2(visible.center.x-rect.x,visible.yMin-rect.y);
                art.bodyFrames[i]=SpriteAsset(art,"Hero C direction "+i/4+" frame "+i%4,body,rect,pivot,ppu);
                var handY=body.height-(i/4+hands[i].y)*body.height/4f-rect.y;
                art.gripOffsets[i]=(new Vector2(hands[i].x*rect.width,handY)-pivot)/ppu;
            }
            art.bowDirections=new Sprite[4];
            // The generated strip has unequal spacing. These bounds encompass each complete silhouette.
            var starts=new[]{70f,795f,1180f,1608f}; var ends=new[]{465f,865f,1572f,2162f};
            var grips=new[]{new Vector2(308f,365f),new Vector2(830f,365f),new Vector2(1320f,365f),new Vector2(1885f,405f)};
            for(var i=0;i<4;i++)
            {
                var sx=bows.width/2172f; var sy=bows.height/724f;
                var rect=new RectInt(Mathf.FloorToInt(starts[i]*sx),0,
                    Mathf.CeilToInt(ends[i]*sx)-Mathf.FloorToInt(starts[i]*sx),bows.height);
                var visible=Visible(bows,rect);
                var grip=new Vector2(grips[i].x*sx-rect.x,(724f-grips[i].y)*sy);
                art.bowDirections[i]=SpriteAsset(art,"Starter bow direction "+i,bows,rect,grip,
                    Mathf.Max(visible.width,visible.height)/.78f);
            }
            art.material=MaterialAsset(Folder+"HeroInk.mat","Not3A/Skeleton Sprite Unlit");
            art.strideCells=.9f; EditorUtility.SetDirty(art);
            ApplyHeroMotion(art);
            var catalog=LoadOrCreate<Stage3DwellingCatalog>(CatalogPath);
            var homeRect=new RectInt(0,0,home.width,home.height); var bounds=Visible(home,homeRect);
            catalog.worldWidth=3.2f; catalog.footprint=new Vector2(3.2f,2f);
            catalog.entranceOffset=new Vector2(.3f,-1.4f); catalog.interactionRadius=1.4f; catalog.woodCost=10;
            catalog.sprite=SpriteAsset(catalog,"Approved dwelling B front",home,homeRect,
                new Vector2(bounds.center.x,bounds.yMin),bounds.width/catalog.worldWidth);
            catalog.material=MaterialAsset(Folder+"DwellingInk.mat","Not3A/Dwelling Sprite");
            catalog.material.SetVector("_VisibleRect",new Vector4((float)bounds.x/home.width,(float)bounds.y/home.height,
                (float)bounds.width/home.width,(float)bounds.height/home.height));
            EditorUtility.SetDirty(catalog.material); EditorUtility.SetDirty(catalog);
            MaterialAsset(GroundMaterialPath,"Not3A/Day Night Ground");
            if(!catalog.TryValidate(out var reason)) throw new InvalidOperationException(reason);
            AssetDatabase.SaveAssets();
            foreach(var name in new[]{"HeroCBody.png","HeroCBows.png","DwellingB.png","HeroCIdle.png","HeroCBowSides.png"})
            { var importer=(TextureImporter)AssetImporter.GetAtPath(Folder+name); importer.isReadable=false; importer.SaveAndReimport(); }
            Debug.Log("S3_DAY_NIGHT_ART_READY: approved hero C, separate starter bow, dwelling B, moonlit world materials.");
        }

        [MenuItem("Tools/Stage 3/Hero/Build Idle And Corrected Side Bows")]
        public static void BuildHeroMotion()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before importing hero art.");
            var art=AssetDatabase.LoadAssetAtPath<Stage3HeroArt>(HeroArtPath);
            if(art==null || !art.IsValid) throw new InvalidOperationException("Build the approved C hero art before adding idle.");
            ApplyHeroMotion(art); AssetDatabase.SaveAssets();
            foreach(var name in new[]{"HeroCIdle.png","HeroCBowSides.png"})
            { var importer=(TextureImporter)AssetImporter.GetAtPath(Folder+name); importer.isReadable=false; importer.SaveAndReimport(); }
            Debug.Log("S3_HERO_MOTION_ART_READY: planted C idle and physically correct right/left starter bows; source pixels unchanged.");
        }

        private static void ApplyHeroMotion(Stage3HeroArt art)
        {
            var idle=ImportPath(MotionSource+"hero_C_idle_v1.png","HeroCIdle.png");
            var sides=ImportPath(MotionSource+"hero_C_bow_sides_v1.png","HeroCBowSides.png");
            var rects=new RectInt[4]; var visible=new RectInt[4]; var maxHeight=0;
            for(var i=0;i<4;i++)
            {
                var x0=Mathf.RoundToInt(i%2*idle.width/2f); var x1=Mathf.RoundToInt((i%2+1)*idle.width/2f);
                var y0=Mathf.RoundToInt((1-i/2)*idle.height/2f); var y1=Mathf.RoundToInt((2-i/2)*idle.height/2f);
                rects[i]=new RectInt(x0,y0,x1-x0,y1-y0); visible[i]=Visible(idle,rects[i]);
                maxHeight=Mathf.Max(maxHeight,visible[i].height);
            }
            var ppu=maxHeight/1.15f;
            // Exact closed carrying-hand centers on the unchanged 1254px approval-derived idle sheet.
            var hands=new[]{new Vector2(419f,334f),new Vector2(1057f,352f),new Vector2(235f,928f),new Vector2(1054f,955f)};
            art.idleDirections=new Sprite[4]; art.idleGripOffsets=new Vector2[4];
            for(var i=0;i<4;i++)
            {
                var rect=rects[i]; var bounds=visible[i]; var pivot=new Vector2(bounds.center.x-rect.x,bounds.yMin-rect.y);
                art.idleDirections[i]=SpriteAsset(art,"Hero C idle direction "+i,idle,rect,pivot,ppu);
                var hand=new Vector2(hands[i].x*idle.width/1254f-rect.x,idle.height-hands[i].y*idle.height/1254f-rect.y);
                art.idleGripOffsets[i]=(hand-pivot)/ppu;
            }
            var grips=new[]{new Vector2(590f,435f),new Vector2(1183f,435f)};
            for(var i=0;i<2;i++)
            {
                var x0=Mathf.RoundToInt(i*sides.width/2f); var x1=Mathf.RoundToInt((i+1)*sides.width/2f);
                var rect=new RectInt(x0,0,x1-x0,sides.height); var bounds=Visible(sides,rect);
                var grip=new Vector2(grips[i].x*sides.width/1774f-rect.x,sides.height-grips[i].y*sides.height/887f);
                art.bowDirections[i==0?0:2]=SpriteAsset(art,i==0?"Starter bow corrected right":"Starter bow corrected left",
                    sides,rect,grip,bounds.height/.78f);
            }
            art.idlePeriodSeconds=3.6f; art.idleBreathScale=.007f;
            EditorUtility.SetDirty(art);
        }

        private static Texture2D Import(string source,string name)
            => ImportPath(Source+source,name);

        private static Texture2D ImportPath(string sourcePath,string name)
        {
            var destination=Folder+name;
            if(!File.Exists(destination)) File.Copy(sourcePath,destination);
            AssetDatabase.ImportAsset(destination,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(destination);
            importer.textureType=TextureImporterType.Default; importer.npotScale=TextureImporterNPOTScale.None;
            importer.alphaSource=TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency=true;
            importer.isReadable=true; importer.sRGBTexture=true;
            // The body atlas has narrow transparent gutters; mip averaging would blend adjacent poses at distant zoom.
            importer.mipmapEnabled=name=="DwellingB.png";
            importer.wrapMode=TextureWrapMode.Clamp; importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.maxTextureSize=4096;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Texture2D>(destination);
        }

        private static RectInt Visible(Texture2D texture,RectInt rect)
        {
            var pixels=texture.GetPixels32(); var minX=rect.xMax; var maxX=rect.xMin; var minY=rect.yMax; var maxY=rect.yMin;
            for(var y=rect.yMin;y<rect.yMax;y++) for(var x=rect.xMin;x<rect.xMax;x++)
                if(pixels[y*texture.width+x].a>=128)
                { minX=Mathf.Min(minX,x); maxX=Mathf.Max(maxX,x); minY=Mathf.Min(minY,y); maxY=Mathf.Max(maxY,y); }
            if(maxX<=minX || maxY<=minY) throw new InvalidOperationException("Empty approved sprite region.");
            return new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);
        }

        private static Sprite SpriteAsset(ScriptableObject owner,string name,Texture2D texture,RectInt rect,Vector2 pivot,float ppu)
        {
            // A rebuild retains each subasset's local ID so serialized references remain valid.
            var path=AssetDatabase.GetAssetPath(owner); Sprite existing=null;
            foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path)) if(asset is Sprite sprite && sprite.name==name) existing=sprite;
            var result=Sprite.Create(texture,new Rect(rect.x,rect.y,rect.width,rect.height),
                new Vector2(pivot.x/rect.width,pivot.y/rect.height),ppu,0,SpriteMeshType.FullRect);
            result.name=name;
            if(existing!=null)
            { EditorUtility.CopySerialized(result,existing); UnityEngine.Object.DestroyImmediate(result); EditorUtility.SetDirty(existing); return existing; }
            AssetDatabase.AddObjectToAsset(result,owner); return result;
        }

        private static T LoadOrCreate<T>(string path) where T:ScriptableObject
        { var asset=AssetDatabase.LoadAssetAtPath<T>(path); if(asset!=null) return asset;
            asset=ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset,path); return asset; }
        private static Material MaterialAsset(string path,string shaderName)
        {
            var shader=Shader.Find(shaderName); if(shader==null) throw new InvalidOperationException("Missing shader "+shaderName);
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null) { material=new Material(shader); AssetDatabase.CreateAsset(material,path); }
            material.shader=shader;
            if(material.HasProperty("_Color")) material.SetColor("_Color",Color.white);
            if(material.HasProperty("_BaseColor")) material.SetColor("_BaseColor",Color.white);
            EditorUtility.SetDirty(material); return material;
        }
    }
}
