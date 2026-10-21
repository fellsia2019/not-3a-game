using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Not3A.Stage3.VisualPilot.Editor
{
    /// <summary>New equipment-only layers; never rewrites the existing HeroArt or its textures.</summary>
    public static class Stage3HeroEquipmentArtImporter
    {
        public const string Folder="Assets/Stage3/DayNight/Equipment/";
        public const string ArtPath=Folder+"Resources/Stage3HeroEquipmentArt.asset";
        private const string Source="art/concepts/equipment_visible_2026-10-20/runtime/";
        [MenuItem("Tools/Stage 3/Equipment/Import Visible Held Equipment")]
        public static void BuildAssets()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before importing artwork.");
            Directory.CreateDirectory(Folder+"Resources");AssetDatabase.Refresh();
            var original=AssetDatabase.LoadAssetAtPath<Stage3HeroArt>("Assets/Stage3/DayNight/HeroArt.asset");
            var forge=AssetDatabase.LoadAssetAtPath<Stage3ForgeArt>("Assets/Stage3/PlayerInterface/Resources/Stage3ForgeArt.asset");
            if(original==null||forge==null)throw new InvalidOperationException("Existing hero/forge art is required.");
            var clean=new Dictionary<string,Texture2D>();
            foreach(var tool in new[]{"axe","pick"})foreach(var pose in new[]{"work","back","front"})
            {
                var name="hero_"+tool+"_"+pose;
                clean[name]=Import(name+"_clean_v1.png");
            }
            var bow=Import("bow_world_qualities_v2.png");
            var target=AssetDatabase.LoadAssetAtPath<Stage3HeroEquipmentArt>(ArtPath);
            if(target==null){target=ScriptableObject.CreateInstance<Stage3HeroEquipmentArt>();AssetDatabase.CreateAsset(target,ArtPath);}
            foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(ArtPath))if(asset is Sprite)Object.DestroyImmediate(asset,true);
            target.axeBodies=new Sprite[24];target.pickBodies=new Sprite[24];
            target.axePoses=new Stage3ToolGripPose[24];target.pickPoses=new Stage3ToolGripPose[24];
            for(var direction=0;direction<4;direction++)for(var frame=0;frame<6;frame++)
            {
                var index=direction*6+frame;
                foreach(var axe in new[]{true,false})
                {
                    var old=original.Work(axe,direction,frame);var texture=clean[old.texture.name];
                    var sx=(float)texture.width/old.texture.width;var sy=(float)texture.height/old.texture.height;
                    if(Mathf.Abs(sx-sy)>.001f)throw new InvalidOperationException("Clean sheet aspect ratio changed: "+texture.name);
                    // Clean actors fit their source cells. Preserve the old absolute sole
                    // anchor, not the old tool silhouette mesh (it clips repaired fingers).
                    var side=direction==0||direction==2;var columns=side?6:3;var rows=side?4:2;
                    var column=frame%columns;var row=side?0:frame/columns;
                    var rect=new Rect(column*texture.width/(float)columns,texture.height-(row+1)*texture.height/(float)rows,
                        texture.width/(float)columns,texture.height/(float)rows);
                    // The reviewed side actors' soles reach y273; y280 lies in the
                    // transparent gutter for every column, before the next row's hands.
                    if(side)rect.yMin=texture.height-280f*texture.height/1024f;
                    var anchor=(old.rect.min+old.pivot)*sx;
                    var sprite=Sprite.Create(texture,rect,new Vector2((anchor.x-rect.x)/rect.width,(anchor.y-rect.y)/rect.height),old.pixelsPerUnit*sx,0,SpriteMeshType.FullRect);
                    sprite.name=(axe?"Axe":"Pick")+" clean "+direction+" "+frame;
                    AssetDatabase.AddObjectToAsset(sprite,target);
                    var pose=WorkPose(axe,direction,frame,old);
                    if(axe){target.axeBodies[index]=sprite;target.axePoses[index]=pose;}
                    else {target.pickBodies[index]=sprite;target.pickPoses[index]=pose;}
                }
            }
            target.bows=BuildBows(target,bow);
            forge.heldEquipment=target;EditorUtility.SetDirty(forge);EditorUtility.SetDirty(target);AssetDatabase.SaveAssets();
            Debug.Log("S3_HELD_EQUIPMENT_ART_READY: eight bow qualities/four directions, detachable axe/pick with unchanged body identity.");
        }
        private static Texture2D Import(string file)
        {
            if(!File.Exists(Source+file))throw new FileNotFoundException("Approved equipment source is missing",Source+file);
            var destination=Folder+file;File.Copy(Source+file,destination,true);
            AssetDatabase.ImportAsset(destination,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(destination);
            importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
            importer.sRGBTexture=true;importer.alphaIsTransparency=true;importer.mipmapEnabled=true;
            importer.filterMode=FilterMode.Trilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.isReadable=true;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(destination);
        }

        // Coordinates are registered on the new 1536x1024 tool-free sheets. Shaft
        // directions/lengths retain the approved original work motion. Left mirrors right.
        private static readonly Vector2[][] AxeGrips={
            new[]{V(186,175),V(386,111),V(680,50),V(941,192),V(1197,184),V(1432,174)},
            new[]{V(403,278),V(682,152),V(1202,70),V(344,578),V(830,575),V(1344,752)},
            new[]{V(307,330),V(717,171),V(1238,66),V(304,827),V(779,803),V(1268,803)}
        };
        private static readonly Vector2[][] AxeAxes={
            new[]{V(58,20),V(-43,-51),V(-58,-22),V(61,27),V(64,28),V(55,23)},
            new[]{V(81,-17),V(-55,-57),V(48,-20),V(24,-62),V(24,-61),V(86,-18)},
            new[]{V(21,71),V(-37,-79),V(-43,-23),V(3,94),V(34,70),V(8,77)}
        };
        private static readonly Vector2[][] PickGrips={
            new[]{V(185,184),V(369,138),V(649,52),V(948,210),V(1212,211),V(1444,199)},
            new[]{V(377,330),V(718,143),V(1249,103),V(294,618),V(789,623),V(1349,820)},
            new[]{V(313,345),V(701,136),V(1221,65),V(332,807),V(776,802),V(1229,797)}
        };
        private static readonly Vector2[][] PickAxes={
            new[]{V(57,26),V(-49,-48),V(-53,-23),V(60,33),V(62,28),V(55,17)},
            new[]{V(82,37),V(-71,-62),V(57,-63),V(15,-57),V(17,-57),V(88,39)},
            new[]{V(-1,89),V(-37,-78),V(-5,-83),V(-4,101),V(14,85),V(1,99)}
        };
        private static Vector2 V(float x,float y)=>new Vector2(x,y);
        private static Stage3ToolGripPose WorkPose(bool axe,int direction,int frame,Sprite original)
        {
            var row=direction==1?1:direction==3?2:0;
            var point=(axe?AxeGrips:PickGrips)[row][frame];var axis=(axe?AxeAxes:PickAxes)[row][frame];
            var scale=original.texture.width/1536f;
            var pixel=new Vector2(point.x*scale,original.texture.height-point.y*scale);
            var grip=(pixel-original.rect.min-original.pivot)/original.pixelsPerUnit;
            var delta=new Vector2(axis.x,-axis.y);if(direction==2){grip.x=-grip.x;delta.x=-delta.x;}
            return new Stage3ToolGripPose{grip=grip,angleDegrees=Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,
                headDistance=axis.magnitude*scale/original.pixelsPerUnit};
        }
        private static Sprite[] BuildBows(ScriptableObject owner,Texture2D texture)
        {
            if(texture.width!=1448||texture.height!=1086)throw new InvalidOperationException("Bow registration requires the reviewed 1448x1086 v2 atlas.");
            var result=new Sprite[32];var pixels=texture.GetPixels32();
            var tops=new[]{20,450,890};var bottoms=new[]{450,870,1030};
            var rightGrip=new[]{156f,333f,501f,675f,856f,1036f,1215f,1382f};
            var backGrip=new[]{110f,281f,459f,633f,809f,989f,1174f,1336f};
            for(var quality=0;quality<8;quality++)for(var row=0;row<3;row++)
            {
                var left=quality*181;var right=left+181;var minX=right;var maxX=left;
                var minY=texture.height-tops[row];var maxY=texture.height-bottoms[row];
                for(var y=texture.height-bottoms[row];y<texture.height-tops[row];y++)
                    for(var x=left;x<right;x++)if(pixels[y*texture.width+x].a>96)
                    {minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x+1);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y+1);}
                if(minX>=maxX)throw new InvalidOperationException("Missing bow "+quality+"/"+row);
                var rect=new Rect(minX-3,minY-3,maxX-minX+6,maxY-minY+6);
                var grip=row==0?V(rightGrip[quality],texture.height-248):row==1?V(backGrip[quality],texture.height-653):
                    V((minX+maxX)*.5f,texture.height-980);
                var ppu=(row==2?maxX-minX:maxY-minY)/.78f;
                var sprite=Sprite.Create(texture,rect,new Vector2((grip.x-rect.x)/rect.width,(grip.y-rect.y)/rect.height),ppu,0,SpriteMeshType.FullRect);
                sprite.name="Bow "+quality+" "+(row==0?"right":row==1?"back":"front");
                AssetDatabase.AddObjectToAsset(sprite,owner);
                result[quality*4+(row==0?0:row==1?1:3)]=sprite;
                if(row==0)result[quality*4+2]=sprite;
            }
            return result;
        }
    }
}
