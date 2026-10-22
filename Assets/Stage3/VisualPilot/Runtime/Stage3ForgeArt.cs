using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    [CreateAssetMenu(menuName="Not 3A/Stage 3/Forge Art")]
    public sealed class Stage3ForgeArt : ScriptableObject
    {
        public Sprite worldSprite;
        public Material material;
        public Texture2D bowPickaxeHelmet, wandAxe, chestArmsLegs;
        public Texture2D staffAccessories,potions;
        public Rect[] potionVisibleBounds;
        private static readonly float[] AccessoryRowTops={0f,225f,450f,653f};
        private static readonly float[] AccessoryRowBottoms={225f,450f,653f,887f};
        // Grip and axis are measured on the original 1774x887 source. Sprite projection
        // rotates the long diagonal shaft upright; no bitmap or source pixel is changed.
        public Vector2 StaffGripPixels(int quality)=>new Vector2((Mathf.Clamp(quality,0,7)*221.75f+88f)*staffAccessories.width/1774f,
            (887f-140f)*staffAccessories.height/887f);
        public float StaffPixelsPerUnit=>283f/1.15f*staffAccessories.height/887f;
        public const float StaffUprightAngle=44f;
        public Stage3HeroEquipmentArt heldEquipment;
        // UI-only alpha bounds, indexed type * 8 + quality. Baked in Editor;
        // held sprite pivots/rects and world registration remain independent.
        public Rect[] uiVisibleBounds;
        public Rect UiVisibleBounds(int type,int quality,Rect fallback)
        {
            if(type>=11&&type<=13&&potionVisibleBounds!=null&&type-11<potionVisibleBounds.Length)
            {
                var potion=potionVisibleBounds[type-11];
                return potion.width>0&&potion.height>0?potion:fallback;
            }
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
            if(type<0||type>13||quality<0||quality>7) return false;
            if(type>=11)
            {
                texture=potions;uv=new Rect((type-11)/3f,0f,1f/3f,1f);
                return texture!=null;
            }
            if((type==1||type>=8)&&staffAccessories!=null)
            {
                var row=type==1?0:type-7;
                texture=staffAccessories;uv=new Rect(quality/8f,1f-AccessoryRowBottoms[row]/887f,1f/8f,(AccessoryRowBottoms[row]-AccessoryRowTops[row])/887f);
                return true;
            }
            if(type>=8)return false;
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
