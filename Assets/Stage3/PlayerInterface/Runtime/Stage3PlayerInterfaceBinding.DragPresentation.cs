using Not3A.Stage2.Defense;
using Not3A.Stage3.VisualPilot;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private GameObject buildingDragGhost;
        private Stage2Tower buildingDragVisualSource;
        private SpriteRenderer[] buildingDragSources, buildingDragSprites;
        private Stage2PlacementMark buildingDragOutline;
        private Material buildingDragSpriteMaterial, buildingDragGroundMaterial;

        // The input owner supplies the target and validity; these renderers never move the source.
        private void UpdateBuildingDragPresentation()
        {
            if(!IsDraggingBuilding || DraggedTower==null || !DraggedTower.IsAlive || scene?.MapDefinition==null)
            { ReleaseBuildingDragPresentation();return; }
            if(!DragTargetCell.HasValue)
            { if(buildingDragGhost!=null) buildingDragGhost.SetActive(false);return; }

            if(buildingDragGhost==null || buildingDragVisualSource!=DraggedTower)
            {
                ReleaseBuildingDragPresentation();
                buildingDragVisualSource=DraggedTower;
                var towerArt=DraggedTower.GetComponentInChildren<Stage3TowerSprite>(true);
                // Preserve layered artillery parts, while excluding the hidden legacy root sprite.
                buildingDragSources=towerArt!=null
                    ? towerArt.GetComponentsInChildren<SpriteRenderer>(true)
                    : DraggedTower.GetComponentsInChildren<SpriteRenderer>(true);
                buildingDragSpriteMaterial=Stage2PlacementVisuals.Material(true);
                buildingDragGroundMaterial=Stage2PlacementVisuals.Material(false);
                buildingDragGhost=new GameObject("Owned building drag preview");
                buildingDragGhost.transform.SetParent(transform,true);
                buildingDragSprites=new SpriteRenderer[buildingDragSources.Length];
                for(var i=0;i<buildingDragSources.Length;i++)
                {
                    var part=new GameObject("Drag artwork "+i);
                    part.transform.SetParent(buildingDragGhost.transform,false);
                    var renderer=part.AddComponent<SpriteRenderer>();
                    renderer.sharedMaterial=buildingDragSpriteMaterial;
                    renderer.shadowCastingMode=ShadowCastingMode.Off;
                    renderer.receiveShadows=false;
                    buildingDragSprites[i]=renderer;
                }
                buildingDragOutline=Stage2PlacementVisuals.Outline(buildingDragGhost.transform,
                    Vector2.one*.96f,buildingDragGroundMaterial,"Dragged cell footprint");
            }

            buildingDragGhost.SetActive(true);
            var target=(Vector3)scene.MapDefinition.CellToWorld(DragTargetCell.Value);
            var offset=target-DraggedTower.transform.position;
            var tint=DragWillMerge?new Color(.15f,.75f,1f,.58f):
                DragDropValid?Stage2PlacementVisuals.Valid:Stage2PlacementVisuals.Invalid;
            for(var i=0;i<buildingDragSources.Length;i++)
            {
                var source=buildingDragSources[i];var ghost=buildingDragSprites[i];
                ghost.enabled=source!=null && source.enabled && source.gameObject.activeInHierarchy && source.sprite!=null;
                if(!ghost.enabled) continue;
                ghost.sprite=source.sprite;
                ghost.transform.SetPositionAndRotation(source.transform.position+offset,source.transform.rotation);
                ghost.transform.localScale=source.transform.lossyScale;
                ghost.flipX=source.flipX;ghost.flipY=source.flipY;
                ghost.drawMode=source.drawMode;ghost.size=source.size;
                ghost.sortingLayerID=source.sortingLayerID;
                ghost.sortingOrder=30000+i;
                ghost.color=tint;
            }
            buildingDragOutline.transform.position=target+Vector3.back*.06f;
            buildingDragOutline.SetTint(new Color(tint.r,tint.g,tint.b,.9f));
        }

        private void ReleaseBuildingDragPresentation()
        {
            // Hide immediately: Destroy completes at frame end, after a cancel/drop can occur.
            if(buildingDragGhost!=null) { buildingDragGhost.SetActive(false);Destroy(buildingDragGhost); }
            if(buildingDragSpriteMaterial!=null) Destroy(buildingDragSpriteMaterial);
            if(buildingDragGroundMaterial!=null) Destroy(buildingDragGroundMaterial);
            buildingDragGhost=null;buildingDragVisualSource=null;
            buildingDragSources=null;buildingDragSprites=null;buildingDragOutline=null;
            buildingDragSpriteMaterial=null;buildingDragGroundMaterial=null;
        }
    }
}
