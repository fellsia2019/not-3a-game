using System;
using System.IO;
using System.Linq;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    public static class Stage3CartoonWorldBuilder
    {
        public const string Folder = "Assets/Stage3/CartoonWorldPilot/";
        public const string RapidCatalog = Folder+"RapidCartoonCatalog.asset";
        public const string LongshotCatalog = Folder+"LongshotCartoonCatalog.asset";
        public const string Evidence = "Artifacts/S3-ASTRA-VISUAL/";
        private const string FrameReview = "Not3A.CartoonWorld.FrameReview";

        [InitializeOnLoadMethod]
        private static void RegisterReview()
        {
            EditorApplication.update += () =>
            {
                if (!SessionState.GetBool(FrameReview,false) || !EditorApplication.isPlaying) return;
                var composition=Object.FindAnyObjectByType<Stage2GameComposition>();
                if(composition==null || !composition.IsInitialized || composition.Building.Defense.TowerCount<2) return;
                var bootstrap=composition.GetComponent<Stage2SceneBootstrap>();
                bootstrap.PlayerMover.transform.position=bootstrap.EntranceMarker.position+Vector3.up*3f;
                bootstrap.CameraFollow.SnapToTarget();
                SessionState.SetBool(FrameReview,false);
                Debug.Log("S3_CARTOON_LIVE_REVIEW_READY: lower-left portal, ordinary preparation; Space starts the wave.");
            };
            EditorApplication.playModeStateChanged += state =>
            {
                if(state==PlayModeStateChange.EnteredEditMode) SessionState.SetBool(FrameReview,false);
            };
        }

        [MenuItem("Tools/Stage 3/Cartoon World/Open Live Review")]
        public static void OpenReview()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the open scene before live review.");
            EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            SessionState.SetBool(FrameReview,true);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying=true;
        }

        [MenuItem("Tools/Stage 3/Cartoon World/Build and Wire")]
        public static void BuildAndWire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                Enumerable.Range(0,UnityEngine.SceneManagement.SceneManager.sceneCount)
                .Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
                throw new InvalidOperationException("Stop Play and save open scene changes first.");
            BuildAssets();
            if (Stage3ResourceSpriteBuilder.SourcesPresent) Stage3ResourceSpriteBuilder.BuildPrefabs();
            var scene = EditorSceneManager.OpenScene(Stage3SkeletonSpriteBuilder.ScenePath);
            var composition = Object.FindAnyObjectByType<Stage2GameComposition>();
            var building = Object.FindAnyObjectByType<Stage2BuildDriver>();
            var mover = composition.GetComponent<Stage2SceneBootstrap>().PlayerMover;
            mover.ConfigurePlanarEdgeClearance(1f);
            EditorUtility.SetDirty(mover);
            Set(composition,"defenseCatalog",AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(RapidCatalog));
            Set(composition,"longshotCatalog",AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(LongshotCatalog));
            Set(building,"catalog",AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(RapidCatalog));
            Set(Object.FindAnyObjectByType<Stage3TacticalPresentation>(),"grassVerge",
                AssetDatabase.LoadAssetAtPath<Material>(Folder+"GrassVerge.mat"));
            Set(Object.FindAnyObjectByType<Stage3TacticalPresentation>(),"roadsideArt",AssetDatabase.LoadAssetAtPath<Stage3RoadsideArt>(Folder+"RoadsideArt.asset"));
            if (Stage3ResourceSpriteBuilder.SourcesPresent)
                Stage3ResourceSpriteBuilder.Wire(Object.FindAnyObjectByType<Stage3TacticalPresentation>());
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_CARTOON_WORLD_WIRED: two sprite towers, new grass/paving; map and camera unchanged.");
        }

        public static void BuildAssets()
        {
            Directory.CreateDirectory(Evidence);
            Import(Folder+"RapidGroundedV2.png",true);
            Import(Folder+"LongshotPartsV2.png",true);
            Import(Folder+"RoadsidePropsV2.png",true);
            Import(Folder+"LongshotFire.png",true);
            Import(Folder+"CartoonMeadowV2.png",false);
            Import(Folder+"CartoonPavingV2.png",false);
            BuildRoadside();
            var rapid = BuildTower(false);
            var longshot = BuildTower(true);
            Catalog(RapidCatalog,"Assets/Stage3/VisualPilot/RapidWatchtower3DGameCatalog.asset",rapid);
            Catalog(LongshotCatalog,Stage3LongshotBuilder.CatalogPath,longshot);
            foreach (var surface in new[] {"Grass","Paving"})
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Stage3/VisualPilot/Tactical"+surface+".mat");
                if (material == null) throw new InvalidOperationException("Existing ground material missing.");
                material.shader = Shader.Find("Universal Render Pipeline/Unlit");
                material.SetColor("_BaseColor",Color.white);
                material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+(surface=="Grass"?"CartoonMeadowV2.png":"CartoonPavingV2.png")));
                material.SetTextureScale("_BaseMap",surface == "Grass" ? new Vector2(.5f,.5f) : new Vector2(.85f,1f));
                EditorUtility.SetDirty(material);
            }
            var vergePath=Folder+"GrassVerge.mat";
            var verge=AssetDatabase.LoadAssetAtPath<Material>(vergePath);
            if(verge==null) { verge=new Material(Shader.Find("Not3A/Cartoon Grass Verge")); AssetDatabase.CreateAsset(verge,vergePath); }
            verge.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"CartoonMeadowV2.png"));
            verge.SetTextureScale("_BaseMap",new Vector2(.5f,.5f));
            EditorUtility.SetDirty(verge);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Stage 3/Cartoon World/Refresh Tower Facing")]
        public static void RefreshTowerFacing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before refreshing tower artwork.");
            Directory.CreateDirectory(Evidence);
            BuildTower(false);
            BuildTower(true);
            AssetDatabase.SaveAssets();
            Debug.Log("S3_TOWER_FACING_READY: Rapid and Longshot face along grid axes; original art and gameplay data preserved.");
        }

        private static void BuildRoadside()
        {
            var path=Folder+"RoadsideArt.asset";
            var art=AssetDatabase.LoadAssetAtPath<Stage3RoadsideArt>(path);
            if(art==null) { art=ScriptableObject.CreateInstance<Stage3RoadsideArt>(); AssetDatabase.CreateAsset(art,path); }
            foreach(var old in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()) Object.DestroyImmediate(old,true);
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"RoadsidePropsV2.png");
            art.sprites=new Sprite[6];
            for(var i=0;i<6;i++)
            {
                var pivot=new Vector2(.5f,i<3?.20f:.27f);
                art.sprites[i]=Sprite.Create(texture,new Rect(i%3*512,(1-i/3)*512,512,512),pivot,512f/(i<2?2.1f:i==2?1.25f:i==3?1.35f:.9f),0,SpriteMeshType.FullRect);
                art.sprites[i].name="Roadside_"+i;
                AssetDatabase.AddObjectToAsset(art.sprites[i],art);
            }
            var materialPath=Folder+"RoadsideInk.mat";
            art.material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(art.material==null) { art.material=new Material(Shader.Find("Not3A/Skeleton Sprite Unlit")); AssetDatabase.CreateAsset(art.material,materialPath); }
            EditorUtility.SetDirty(art);
        }

        private static void Import(string path, bool readable)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency=readable;
            importer.isReadable=readable;
            importer.sRGBTexture=true;
            importer.mipmapEnabled=true;
            importer.wrapMode=readable ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=2048;
            importer.SaveAndReimport();
        }

        private static Stage2Tower BuildTower(bool catapult)
        {
            var name=catapult ? "Longshot" : "Rapid";
            var originalPath=Folder+(catapult?"LongshotPartsV2.png":"RapidGroundedV2.png");
            var facingPath=Folder+name+"GridFacing.png";
            Import(originalPath,true);
            Import(facingPath,true);
            var original=AssetDatabase.LoadAssetAtPath<Texture2D>(originalPath);
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(facingPath);
            var setPath=Folder+name+"Sprites.asset";
            var set=AssetDatabase.LoadAssetAtPath<Stage3TowerSpriteSet>(setPath);
            if (set == null) { set=ScriptableObject.CreateInstance<Stage3TowerSpriteSet>(); AssetDatabase.CreateAsset(set,setPath); }
            foreach(var previous in AssetDatabase.LoadAllAssetsAtPath(setPath).OfType<Sprite>()) Object.DestroyImmediate(previous,true);
            var count=1;
            var width=texture.width;
            var height=texture.height;
            var pixels=texture.GetPixels32();
            var originalWidth=catapult?original.width/3:original.width;
            var originalHeight=catapult?original.height/2:original.height;
            var originalPixels=original.GetPixels32();
            int originalBottom=originalHeight,originalTop=-1;
            for(var py=0;py<originalHeight;py++)
            for(var px=0;px<originalWidth;px++)
                if(originalPixels[((catapult?originalHeight:0)+py)*original.width+px].a>=128)
                { originalBottom=Mathf.Min(originalBottom,py); originalTop=Mathf.Max(originalTop,py); }
            if(originalTop<0) throw new InvalidOperationException("Empty original tower artwork.");
            var originalPixelsPerUnit=originalWidth/(catapult?1.65f:1.45f);
            var sprites=new Sprite[count];
            var report="";
            for(var index=0;index<count;index++)
            {
                int x=0, y=0;
                int bottom=height, top=-1;
                for(var py=0;py<height;py++)
                for(var px=0;px<width;px++)
                    if(pixels[(y+py)*texture.width+x+px].a>=128) { bottom=Mathf.Min(bottom,py); top=Mathf.Max(top,py); }
                if(top<0) throw new InvalidOperationException("Empty tower frame "+index);
                // Retain the authored local coordinates used by the articulated axle.
                // Final width and ground registration come from visible bounds below.
                var pixelsPerUnit=originalPixelsPerUnit*(top-bottom+1f)/(originalTop-originalBottom+1f);
                var footOffset=(catapult?95f:150f)/originalPixelsPerUnit;
                var pivot=new Vector2(.5f,(bottom+footOffset*pixelsPerUnit)/height);
                var sprite=Sprite.Create(texture,new Rect(x,y,width,height),pivot,pixelsPerUnit,0,SpriteMeshType.FullRect);
                sprite.name=name+"_"+index/6+"_"+index%6;
                AssetDatabase.AddObjectToAsset(sprite,set);
                sprites[index]=sprite;
                report+=$"{sprite.name} rect={sprite.rect} pivot={sprite.pivot} opaqueBottom={bottom}\n";
            }
            set.Configure(sprites,catapult ? new[] {
                new Vector2(185,215),new Vector2(65,218),new Vector2(69,94),new Vector2(185,125)
            } : new[] {new Vector2(width*.5f,height*.65f)});
            const float cellWidth=.92f, frontInset=.02f;
            var visibleBounds=VisibleBodyBounds(pixels,width,height,sprites[0]);
            set.ConfigureCellFit(visibleBounds,cellWidth,frontInset);
            report+=$"visibleBounds={visibleBounds} cellWidth={cellWidth} frontInset={frontInset}\n";
            if(catapult)
            {
                var spoon=Sprite.Create(original,new Rect(0,0,512,512),new Vector2(145f/512f,140f/512f),480f,0,SpriteMeshType.FullRect);
                spoon.name="Longshot separate spoon"; AssetDatabase.AddObjectToAsset(spoon,set);
                var stone=Sprite.Create(original,new Rect(512,0,512,512),new Vector2(250f/512f,260f/512f),740f,0,SpriteMeshType.FullRect);
                stone.name="Longshot loaded stone"; AssetDatabase.AddObjectToAsset(stone,set);
                set.ConfigureParts(spoon,stone);
            }
            EditorUtility.SetDirty(set);
            var materialPath=Folder+"TowerInk.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null) { material=new Material(Shader.Find("Not3A/Skeleton Sprite Unlit")); AssetDatabase.CreateAsset(material,materialPath); }
            material.SetColor("_Color",Color.white);
            var prefabPath=Folder+name+"Cartoon.prefab";
            var old=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var root=old!=null?PrefabUtility.LoadPrefabContents(prefabPath):new GameObject(name+" Cartoon Sprite");
            try
            {
                var tower=root.GetComponent<Stage2Tower>(); if(tower==null) tower=root.AddComponent<Stage2Tower>();
                var placeholder=root.GetComponent<SpriteRenderer>(); if(placeholder==null) placeholder=root.AddComponent<SpriteRenderer>();
                placeholder.enabled=false;
                var child=root.transform.Find("Tower artwork");
                if(child==null) { child=new GameObject("Tower artwork").transform; child.SetParent(root.transform,false); }
                var renderer=child.GetComponent<SpriteRenderer>(); if(renderer==null) renderer=child.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite=sprites[0]; renderer.sharedMaterial=material;
                renderer.spriteSortPoint=SpriteSortPoint.Pivot;
                renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
                var view=child.GetComponent<Stage3TowerSprite>(); if(view==null) view=child.gameObject.AddComponent<Stage3TowerSprite>();
                Set(view,"artwork",set); Set(view,"body",renderer);
                var shadow=root.transform.Find("Tower contact shadow");
                if(shadow==null) { shadow=new GameObject("Tower contact shadow").transform; shadow.SetParent(root.transform,false); }
                var shadowScale=cellWidth/(2f*.80f);
                shadow.localScale=Vector3.one*shadowScale;
                shadow.localPosition=new Vector3(0f,-(.5f-frontInset)+.47f*shadowScale,-.014f);
                var filter=shadow.GetComponent<MeshFilter>(); if(filter==null) filter=shadow.gameObject.AddComponent<MeshFilter>();
                filter.sharedMesh=ContactShadow();
                var shade=shadow.GetComponent<MeshRenderer>(); if(shade==null) shade=shadow.gameObject.AddComponent<MeshRenderer>();
                var shadeMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"ContactShadow.mat");
                if(shadeMaterial==null)
                {
                    shadeMaterial=new Material(material); shadeMaterial.SetColor("_Color",new Color(0,0,0,.24f));
                    AssetDatabase.CreateAsset(shadeMaterial,Folder+"ContactShadow.mat");
                }
                shade.sharedMaterial=shadeMaterial; shade.sortingOrder=-10000;
                shade.shadowCastingMode=ShadowCastingMode.Off; shade.receiveShadows=false;
                root.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root,prefabPath);
            }
            finally { if(old!=null) PrefabUtility.UnloadPrefabContents(root); else Object.DestroyImmediate(root); }
            File.WriteAllText(Evidence+name+"-import.txt",report);
            AssetDatabase.SaveAssets();
            var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
            importer.isReadable=false; importer.SaveAndReimport();
            var originalImporter=(TextureImporter)AssetImporter.GetAtPath(originalPath);
            originalImporter.isReadable=false; originalImporter.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Stage2Tower>(prefabPath);
        }

        private static Rect VisibleBodyBounds(Color32[] pixels,int width,int height,Sprite sprite)
        {
            var rows=new int[height]; var columns=new int[width];
            for(var y=0;y<height;y++)
            for(var x=0;x<width;x++)
                if(pixels[y*width+x].a>=128) { rows[y]++; columns[x]++; }
            // Ignore isolated alpha specks in otherwise transparent padding.
            var minRow=Mathf.Max(4,Mathf.CeilToInt(width*.005f));
            var minColumn=Mathf.Max(4,Mathf.CeilToInt(height*.005f));
            var left=Array.FindIndex(columns,n=>n>=minColumn);
            var right=Array.FindLastIndex(columns,n=>n>=minColumn)+1;
            var bottom=Array.FindIndex(rows,n=>n>=minRow);
            var top=Array.FindLastIndex(rows,n=>n>=minRow)+1;
            if(left<0||bottom<0) throw new InvalidOperationException("Empty tower body bounds.");
            return new Rect((left-sprite.pivot.x)/sprite.pixelsPerUnit,
                (bottom-sprite.pivot.y)/sprite.pixelsPerUnit,
                (right-left)/sprite.pixelsPerUnit,(top-bottom)/sprite.pixelsPerUnit);
        }

        private static Mesh ContactShadow()
        {
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"ContactShadow.asset");
            if(mesh!=null) return mesh;
            const int count=24;
            var vertices=new Vector3[count+1]; var colors=new Color[count+1];
            var triangles=new int[count*3]; colors[0]=Color.white;
            for(var i=0;i<count;i++)
            {
                var angle=i*Mathf.PI*2f/count;
                vertices[i+1]=new Vector3(Mathf.Cos(angle)*.80f,Mathf.Sin(angle)*.47f,0);
                colors[i+1]=new Color(1,1,1,0);
                triangles[i*3]=0; triangles[i*3+1]=i+1; triangles[i*3+2]=(i+1)%count+1;
            }
            mesh=new Mesh { name="Tower contact shadow",vertices=vertices,colors=colors,
                uv=new Vector2[count+1],triangles=triangles };
            mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh,Folder+"ContactShadow.asset");
            return mesh;
        }

        private static void Catalog(string path,string source,Stage2Tower prefab)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(path);
            if(catalog==null)
            {
                catalog=Object.Instantiate(AssetDatabase.LoadAssetAtPath<Stage2DefenseCatalog>(source));
                catalog.name=Path.GetFileNameWithoutExtension(path);
                AssetDatabase.CreateAsset(catalog,path);
            }
            Set(catalog,"towerPrefab",prefab); // Preserve all serialized gameplay values.
        }

        private static void Set(Object target,string property,Object value)
        {
            var serialized=new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue=value;
            serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(target);
        }

        public static void BuildWindows()
        {
            BuildAndWire();
            const string path="Builds/Windows/Stage3CartoonWorld/Not3A.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{Stage3SkeletonSpriteBuilder.ScenePath},locationPathName=path,
                target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development });
            File.WriteAllText(Evidence+"build.txt",$"{report.summary.result} bytes={report.summary.totalSize} errors={report.summary.totalErrors} warnings={report.summary.totalWarnings}");
            if(report.summary.result!=BuildResult.Succeeded) throw new InvalidOperationException("Cartoon build failed.");
        }
    }
}
