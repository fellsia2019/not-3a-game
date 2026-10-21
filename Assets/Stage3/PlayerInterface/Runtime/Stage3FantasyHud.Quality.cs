using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        // One shared imported source, never a generated plate or a per-item texture.
        private Texture2D equipmentQualityAtlas;
        private bool equipmentQualityAtlasLoaded;
        private Rect? equipmentTextureClip;

        // Measured top-left pixel bounds in the original 1774 x 887 ImageGen source.
        // The artwork is deliberately NOT sampled as an equal 4 x 2 grid.
        // SSS owns its wider local halo; the nearly transparent seam separates S.
        private static readonly Rect[] EquipmentQualitySkinPixels =
        {
            new Rect(40,30,420,400), new Rect(465,30,420,400),
            new Rect(890,30,416,400), new Rect(1315,30,420,400),
            new Rect(40,443,420,405), new Rect(465,443,420,405),
            new Rect(890,443,416,405), new Rect(1307,430,449,440)
        };

        private void DrawEquipmentCell(Rect rect, Stage3EquipmentItemView item, bool selected = false)
        {
            if (item == null) return;
            if (item.Blueprint != null) { DrawBlueprintCell(rect, item.Blueprint, selected); return; }
            DrawEquipmentQualityFrame(rect, item.QualityIndex);
            DrawEquipmentIcon(EquipmentIconSafeRect(rect), item);
            if (selected) DrawEquipmentSelection(rect);
        }

        private Rect EquipmentIconSafeRect(Rect rect)
        {
            var side=Mathf.Min(rect.width,rect.height);
            var frameEdge=Mathf.Min(14f*UiScale,side*.22f);
            // Keep the visible silhouette beyond the inner bevel and corner ornaments.
            // Proportional padding also protects large details/forge previews.
            var padding=Mathf.Max(side*.10f,frameEdge*.85f+3f*UiScale);
            padding=Mathf.Min(padding,side*.30f);
            return Inset(rect,padding);
        }

        private void DrawEquipmentQualityFrame(Rect rect, int quality)
        {
            if (Event.current.type != EventType.Repaint || rect.width <= 0 || rect.height <= 0) return;
            if (!equipmentQualityAtlasLoaded)
            {
                equipmentQualityAtlasLoaded = true;
                equipmentQualityAtlas = Resources.Load<Texture2D>("Stage3EquipmentQualitySkins");
            }
            if (equipmentQualityAtlas == null) return;
            quality = Mathf.Clamp(quality, 0, 7);
            var source = EquipmentQualitySkinPixels[quality];
            var sourceEdge = quality == 7 ? 76f : 60f;
            var edge = Mathf.Min(14f * UiScale, Mathf.Min(rect.width, rect.height) * .22f);

            // Preserve the complete painted interior (gradient, engraving and vignette).
            // Only the surrounding eight frame patches are nine-sliced; there is no
            // flat colour, sampled centre pixel, synthetic rail or generated sparkle.
            var centre = Rect.MinMaxRect(Mathf.Round(rect.x + edge), Mathf.Round(rect.y + edge),
                Mathf.Round(rect.xMax - edge), Mathf.Round(rect.yMax - edge));
            DrawEquipmentSkinPatch(centre, Inset(source, sourceEdge));
            for (var row = 0; row < 3; row++)
            for (var col = 0; col < 3; col++)
            {
                if (row == 1 && col == 1) continue;
                // Shared physical pixel edges prevent fractional GPU quad seams.
                var x = Mathf.Round(col == 0 ? rect.x : col == 1 ? rect.x + edge : rect.xMax - edge);
                var y = Mathf.Round(row == 0 ? rect.y : row == 1 ? rect.y + edge : rect.yMax - edge);
                var xMax = Mathf.Round(col == 0 ? rect.x + edge : col == 1 ? rect.xMax - edge : rect.xMax);
                var yMax = Mathf.Round(row == 0 ? rect.y + edge : row == 1 ? rect.yMax - edge : rect.yMax);
                var sx = col == 0 ? source.x : col == 1 ? source.x + sourceEdge : source.xMax - sourceEdge;
                var sy = row == 0 ? source.y : row == 1 ? source.y + sourceEdge : source.yMax - sourceEdge;
                var sw = col == 1 ? source.width - 2f * sourceEdge : sourceEdge;
                var sh = row == 1 ? source.height - 2f * sourceEdge : sourceEdge;
                DrawEquipmentSkinPatch(Rect.MinMaxRect(x,y,xMax,yMax), new Rect(sx,sy,sw,sh));
            }
        }

        private void DrawEquipmentSkinPatch(Rect target, Rect sourcePixels)
        {
            // Source metadata uses image top-left; Unity UVs use bottom-left.
            var uv = new Rect(sourcePixels.x / equipmentQualityAtlas.width,
                1f - sourcePixels.yMax / equipmentQualityAtlas.height,
                sourcePixels.width / equipmentQualityAtlas.width,
                sourcePixels.height / equipmentQualityAtlas.height);
            DrawEquipmentTexture(target, equipmentQualityAtlas, uv);
        }

        private void DrawEquipmentSelection(Rect rect)
        {
            // The cyan selection lives outside the quality bevel and never replaces it.
            var line = Mathf.Max(1f, 1.4f * UiScale);
            var r = Inset(rect, 1f * UiScale);
            var cyan = new Color32(86,235,255,255);
            Stroke(r, new Color(86f/255f,235f/255f,1f,.65f), line);
            var length = Mathf.Min(12f * UiScale, Mathf.Min(r.width,r.height) * .22f);
            for (var i=0;i<4;i++)
            {
                var right = (i & 1) != 0; var bottom = (i & 2) != 0;
                var x = right ? r.xMax - length : r.x;
                var y = bottom ? r.yMax - line * 2f : r.y;
                Fill(new Rect(x,y,length,line * 2f),cyan);
                Fill(new Rect(right ? r.xMax-line*2f : r.x,bottom ? r.yMax-length : r.y,line*2f,length),cyan);
            }
        }

        private void DrawEquipmentTexture(Rect target, Texture texture, Rect uv)
        {
            // Graphics.DrawTexture does not inherit IMGUI scroll clipping on Metal.
            // Clip both geometry and UVs in the same local scroll coordinates as labels.
            if (equipmentTextureClip.HasValue)
            {
                var clip = equipmentTextureClip.Value;
                var visible = Rect.MinMaxRect(Mathf.Max(target.x,clip.x),Mathf.Max(target.y,clip.y),
                    Mathf.Min(target.xMax,clip.xMax),Mathf.Min(target.yMax,clip.yMax));
                if (visible.width <= 0f || visible.height <= 0f) return;
                uv = new Rect(uv.x+(visible.x-target.x)/target.width*uv.width,
                    uv.y+(target.yMax-visible.yMax)/target.height*uv.height,
                    visible.width/target.width*uv.width,visible.height/target.height*uv.height);
                target = visible;
            }
            DrawUiTexture(target,texture,uv);
        }

        private void ReleaseEquipmentQualityTextures()
        {
            // Resources owns the single imported atlas; this HUD owns no texture allocations.
            equipmentQualityAtlas = null;
            equipmentQualityAtlasLoaded = false;
        }
    }
}
