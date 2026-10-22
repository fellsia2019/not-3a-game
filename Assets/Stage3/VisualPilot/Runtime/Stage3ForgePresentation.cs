using Not3A.Stage2.Defense;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Approved forge artwork only; placement, payment and service access belong to Stage3Forge.</summary>
    public sealed class Stage3ForgePresentation : MonoBehaviour
    {
        private Stage3Forge owner;
        private Stage3ForgeArt art;
        private Camera view;
        private Stage3FantasyHud hud;
        private SpriteRenderer prepared,built,preview;
        private Stage2PlacementMark footprint;
        private Material previewMaterial,outlineMaterial;
        private float worldScale=1f;
        public SpriteRenderer BuiltVisual=>built;
        public void Initialize(Stage3Forge forge,Stage3ForgeArt artwork,Camera camera)
        {
            owner=forge;art=artwork;view=camera;hud=GetComponent<Stage3FantasyHud>();
            if(art!=null&&art.worldSprite!=null)
            {
                var visibleWidth=art.material!=null&&art.material.HasProperty("_VisibleRect")?art.material.GetVector("_VisibleRect").z:1f;
                worldScale=owner.Catalog.WorldWidth/(art.worldSprite.bounds.size.x*Mathf.Max(.1f,visibleWidth));
            }
        }
        public bool PrepareBuild(Vector2 position)
        {
            CancelPreparedBuild();
            if(owner==null||art==null||art.worldSprite==null||art.material==null||view==null)return false;
            prepared=CreateSprite("Forge",art.material);Align(prepared,position);prepared.gameObject.SetActive(false);return true;
        }
        public void CompleteBuild()
        {
            if(prepared==null)return;
            if(built!=null){built.gameObject.SetActive(false);Destroy(built.gameObject);}
            built=prepared;prepared=null;built.gameObject.SetActive(true);
        }
        public void CancelPreparedBuild()
        {
            if(prepared==null)return;
            prepared.gameObject.SetActive(false);Destroy(prepared.gameObject);prepared=null;
        }
        public void Clear()
        {
            CancelPreparedBuild();
            if(built!=null){built.gameObject.SetActive(false);Destroy(built.gameObject);built=null;}
            if(preview!=null)preview.gameObject.SetActive(false);
            if(footprint!=null)footprint.gameObject.SetActive(false);
            if(hud!=null)hud.ForgeInteractionHint=null;
        }
        private SpriteRenderer CreateSprite(string name,Material material)
        {
            var root=new GameObject(name);root.transform.SetParent(transform,false);
            var sprite=root.AddComponent<SpriteRenderer>();sprite.sprite=art.worldSprite;sprite.sharedMaterial=material;return sprite;
        }
        private void LateUpdate()
        {
            if(owner==null||!owner.IsReady||art==null||view==null)return;
            if(hud==null)hud=GetComponent<Stage3FantasyHud>();
            if(built!=null)Align(built,owner.Position);
            var visible=owner.IsPlacing&&owner.PreviewVisible;
            if(visible&&preview==null)
            {
                previewMaterial=Stage2PlacementVisuals.Material(true);outlineMaterial=Stage2PlacementVisuals.Material(false);
                preview=CreateSprite("Forge preview",previewMaterial);
                footprint=Stage2PlacementVisuals.Outline(transform,owner.Catalog.Footprint,outlineMaterial,"Forge footprint");
            }
            if(preview!=null)preview.gameObject.SetActive(visible);
            if(footprint!=null)footprint.gameObject.SetActive(visible);
            if(visible)
            {
                Align(preview,owner.PreviewPosition);
                var color=owner.PreviewResult==Stage3ForgePlacementResult.Ready?Stage2PlacementVisuals.Valid:Stage2PlacementVisuals.Invalid;
                preview.color=color;footprint.SetTint(new Color(color.r,color.g,color.b,.9f));
                var p=owner.PreviewPosition;footprint.transform.position=new Vector3(p.x,p.y,Stage3TacticalPresentation.GrassSurfaceZ(p)-.03f);
            }
            if(hud!=null)hud.ForgeInteractionHint=owner.IsPlacing?
                visible?PlacementHint(owner.PreviewResult):"Выберите место в мире · Esc / ПКМ · отменить":
                owner.IsBuilt&&owner.NearEntrance?owner.CanUse?"E · кузница":"Кузница сейчас недоступна":null;
        }
        private static string PlacementHint(Stage3ForgePlacementResult result) => result switch
        {
            Stage3ForgePlacementResult.Ready => "ЛКМ · построить кузницу · Esc / ПКМ · отменить",
            Stage3ForgePlacementResult.HeroOverlap => "Герой стоит на месте постройки · отойдите · Esc / ПКМ · отменить",
            Stage3ForgePlacementResult.Occupied => "Место занято · выберите свободный участок · Esc / ПКМ · отменить",
            Stage3ForgePlacementResult.InvalidGround => "Нужен ровный свободный участок зелёной земли · Esc / ПКМ · отменить",
            Stage3ForgePlacementResult.InsufficientResources => "Недостаточно ресурсов для кузницы · Esc / ПКМ · отменить",
            Stage3ForgePlacementResult.CreationFailed => "Не удалось построить кузницу · выберите другое место · Esc / ПКМ · отменить",
            _ => "Выберите свободную зелёную землю · Esc / ПКМ · отменить"
        };
        private void Align(SpriteRenderer renderer,Vector2 position)
        {
            var scale=worldScale;
            renderer.transform.localScale=Vector3.one*scale;
            var ground=new Vector3(position.x,position.y-owner.Catalog.Footprint.y*.5f,Stage3TacticalPresentation.GrassSurfaceZ(position));
            var padding=renderer.sprite.pivot.y/renderer.sprite.pixelsPerUnit*scale;
            var depth=padding*Mathf.Abs(view.transform.up.z)/Mathf.Max(.1f,Mathf.Abs(view.transform.forward.z))+.03f;
            renderer.transform.SetPositionAndRotation(ground-view.transform.forward*depth,view.transform.rotation);
            renderer.sortingOrder=-Mathf.RoundToInt(position.y*100f);
        }
        private void OnDestroy()
        {
            if(hud!=null)hud.ForgeInteractionHint=null;
            if(previewMaterial!=null)Destroy(previewMaterial);
            if(outlineMaterial!=null)Destroy(outlineMaterial);
        }
    }
}
