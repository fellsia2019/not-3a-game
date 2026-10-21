using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    [CreateAssetMenu(menuName="Not 3A/Stage 3/Forge Art")]
    public sealed class Stage3ForgeArt : ScriptableObject
    {
        public Sprite worldSprite;
        public Material material;
        public Texture2D bowPickaxeHelmet, wandAxe, chestArmsLegs;
        public Stage3HeroEquipmentArt heldEquipment;
        // UI-only alpha bounds, indexed type * 8 + quality. Baked in Editor;
        // held sprite pivots/rects and world registration remain independent.
        public Rect[] uiVisibleBounds;
        public Rect UiVisibleBounds(int type,int quality,Rect fallback)
        {
            var index=type*8+quality;
            if(uiVisibleBounds==null||index<0||index>=uiVisibleBounds.Length)return fallback;
            var bounds=uiVisibleBounds[index];
            return bounds.width>0f&&bounds.height>0f?bounds:fallback;
        }
        // The diagonal axe silhouettes overlap column bounds, but occupy disjoint
        // ranges in these two horizontal bands. Metadata preserves every source pixel.
        private static readonly int[,] AxeBands={
            {41,271,0,212},{291,525,246,463},{545,782,497,724},{802,1034,746,978},
            {1056,1300,1003,1242},{1312,1555,1260,1495},{1566,1815,1514,1750},{1819,2048,1769,1989}
        };
        public Rect AxeIconBand(int quality,int band)
        {
            var q=Mathf.Clamp(quality,0,7);var offset=band==0?0:2;
            var left=AxeBands[q,offset];var right=AxeBands[q,offset+1];
            var top=band==0?384f:576f;var bottom=band==0?576f:704f;
            return new Rect(left/2048f,1f-bottom/768f,(right-left)/2048f,(bottom-top)/768f);
        }

        // Source sheets retain their original bytes. Rows follow the approved, nonuniform artwork bounds.
        public bool TryGetIcon(int type,int quality,out Texture2D texture,out Rect uv)
        {
            texture=null;uv=default;
            if(type<0||type>7||quality<0||quality>7) return false;
            if(type==0&&heldEquipment!=null&&heldEquipment.HasBows)
            {
                var sprite=heldEquipment.Bow(quality,0);texture=sprite.texture;
                uv=new Rect(sprite.rect.x/texture.width,sprite.rect.y/texture.height,sprite.rect.width/texture.width,sprite.rect.height/texture.height);
                return true;
            }
            float top,bottom;
            switch(type)
            {
                case 0: texture=bowPickaxeHelmet;top=0;bottom=347;break;
                case 6: texture=bowPickaxeHelmet;top=347;bottom=597;break;
                case 2: texture=bowPickaxeHelmet;top=597;bottom=887;break;
                case 1: texture=wandAxe;top=0;bottom=texture!=null?texture.height*.5f:0;break;
                case 7:
                    texture=wandAxe;
                    var upper=AxeIconBand(quality,0);var lower=AxeIconBand(quality,1);
                    uv=Rect.MinMaxRect(lower.xMin,lower.yMin,upper.xMax,upper.yMax);
                    return texture!=null;
                case 3: texture=chestArmsLegs;top=0;bottom=333;break;
                case 4: texture=chestArmsLegs;top=333;bottom=562;break;
                default:texture=chestArmsLegs;top=562;bottom=887;break;
            }
            if(texture==null) return false;
            // Fractions reference the 887px original even if the importer downsamples a sheet.
            var referenceHeight=type==1||type==7?texture.height:887f;
            uv=new Rect(quality/8f,1f-bottom/referenceHeight,1f/8f,(bottom-top)/referenceHeight);
            return true;
        }
    }
}
