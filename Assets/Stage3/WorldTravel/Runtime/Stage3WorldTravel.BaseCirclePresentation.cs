using Not3A.Stage2.Defense;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed partial class Stage3WorldTravel
    {
        private GameObject baseCirclePreviewArt;
        private Mesh baseCirclePreviewMesh;
        private Stage2PlacementMark baseCirclePreviewOutline;
        private Material baseCirclePreviewOutlineMaterial;
        private MaterialPropertyBlock baseCirclePreviewTint;
        private MeshRenderer baseCirclePreviewRenderer;
        private Vector2 baseCircleDrawnPosition;

        partial void RefreshBaseCirclePreview(bool visible)
        {
            if(!visible)
            {
                if(baseCirclePreviewArt!=null)baseCirclePreviewArt.SetActive(false);
                if(baseCirclePreviewOutline!=null)baseCirclePreviewOutline.gameObject.SetActive(false);
                return;
            }
            // Snap-position changes rebuild the existing seal mesh; no bitmap or per-frame material allocation.
            if(baseCirclePreviewArt==null||baseCircleDrawnPosition!=BaseCirclePreviewPosition)
            {
                RebuildCircle(ref baseCirclePreviewArt,ref baseCirclePreviewMesh,"Base circle placement preview",BaseCirclePreviewPosition);
                baseCircleDrawnPosition=BaseCirclePreviewPosition;
                baseCirclePreviewRenderer=baseCirclePreviewArt.GetComponent<MeshRenderer>();
            }
            if(baseCirclePreviewOutline==null)
            {
                baseCirclePreviewOutlineMaterial=Stage2PlacementVisuals.Material(false);
                baseCirclePreviewOutline=Stage2PlacementVisuals.Outline(transform,BaseCirclePlacementSize,
                    baseCirclePreviewOutlineMaterial,"Teleport circle reserved footprint");
                baseCirclePreviewTint=new MaterialPropertyBlock();
            }
            baseCirclePreviewArt.SetActive(true);baseCirclePreviewOutline.gameObject.SetActive(true);
            var color=BaseCirclePreviewResult==WorkshopPlacementResult.Ready?Stage2PlacementVisuals.Valid:Stage2PlacementVisuals.Invalid;
            baseCirclePreviewTint.SetColor("_ContactColor",new Color(color.r,color.g,color.b,1f));
            baseCirclePreviewTint.SetFloat("_Opacity",.85f);
            baseCirclePreviewRenderer.SetPropertyBlock(baseCirclePreviewTint);
            baseCirclePreviewOutline.SetTint(new Color(color.r,color.g,color.b,.9f));
            baseCirclePreviewOutline.transform.position=new Vector3(BaseCirclePreviewPosition.x,BaseCirclePreviewPosition.y,
                Stage3TacticalPresentation.GrassSurfaceZ(BaseCirclePreviewPosition)-.025f);
        }

        partial void ClearBaseCirclePreview()
        {
            if(baseCirclePreviewArt!=null){baseCirclePreviewArt.SetActive(false);Destroy(baseCirclePreviewArt);baseCirclePreviewArt=null;}
            if(baseCirclePreviewMesh!=null){Destroy(baseCirclePreviewMesh);baseCirclePreviewMesh=null;}
            if(baseCirclePreviewOutline!=null){baseCirclePreviewOutline.gameObject.SetActive(false);Destroy(baseCirclePreviewOutline.gameObject);baseCirclePreviewOutline=null;}
            if(baseCirclePreviewOutlineMaterial!=null){Destroy(baseCirclePreviewOutlineMaterial);baseCirclePreviewOutlineMaterial=null;}
            baseCirclePreviewRenderer=null;baseCirclePreviewTint=null;
        }
    }
}
