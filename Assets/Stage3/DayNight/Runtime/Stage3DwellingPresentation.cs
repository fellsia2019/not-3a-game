using Not3A.Stage2.Defense;
using Not3A.Stage2.Integration;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Approved house artwork and HUD binding; transactions and interaction remain in Stage3Dwelling.</summary>
    public sealed class Stage3DwellingPresentation : MonoBehaviour
    {
        private Stage3Dwelling dwelling;
        private Stage3DwellingCatalog data;
        private Camera view;
        private Stage2GameHud hud;
        private Stage2GameComposition game;
        private GameObject prepared, built;
        private SpriteRenderer preview;
        private Stage2PlacementMark footprint;
        private Material previewMaterial, outlineMaterial;

        public SpriteRenderer BuiltVisual => built != null ? built.GetComponent<SpriteRenderer>() : null;

        public void Initialize(Stage3Dwelling owner, Stage3DwellingCatalog catalog, Camera camera)
        {
            dwelling = owner; data = catalog; view = camera;
            hud = owner.GetComponent<Stage2GameHud>();
            game = owner.GetComponent<Stage2GameComposition>();
            if (hud != null)
                hud.BindDwellingConstruction(data.sprite, () => dwelling.BuildStatus,
                    () => dwelling.CanBeginPlacement, dwelling.BeginPlacement, dwelling.CancelPlacement, data.woodCost);
        }

        public bool PrepareBuild(Vector2 position)
        {
            CancelPreparedBuild();
            if (data == null || data.sprite == null || data.material == null || view == null) return false;
            prepared = new GameObject("Hero dwelling B");
            prepared.transform.SetParent(transform, false);
            var sprite = prepared.AddComponent<SpriteRenderer>();
            sprite.sprite = data.sprite; sprite.sharedMaterial = data.material;
            Align(sprite, position);
            prepared.SetActive(false);
            return true;
        }

        public void CompleteBuild()
        {
            if (prepared == null) return;
            if (built != null) { built.SetActive(false); Destroy(built); }
            built = prepared; prepared = null; built.SetActive(true);
        }

        public void CancelPreparedBuild()
        {
            if (prepared == null) return;
            prepared.SetActive(false); Destroy(prepared); prepared = null;
        }

        public void Clear()
        {
            CancelPreparedBuild();
            if (built != null) { built.SetActive(false); Destroy(built); built = null; }
            if (preview != null) preview.gameObject.SetActive(false);
            if (footprint != null) footprint.gameObject.SetActive(false);
            if (hud != null) hud.DwellingInteractionHint = null;
        }

        private void LateUpdate()
        {
            if (dwelling == null || !dwelling.IsReady || data == null || view == null) return;
            if (built != null) Align(built.GetComponent<SpriteRenderer>(), dwelling.Position);
            var visible = dwelling.IsPlacing && dwelling.PreviewVisible;
            if (visible && preview == null)
            {
                previewMaterial = Stage2PlacementVisuals.Material(true);
                outlineMaterial = Stage2PlacementVisuals.Material(false);
                var root = new GameObject("Hero dwelling preview"); root.transform.SetParent(transform, false);
                preview = root.AddComponent<SpriteRenderer>();
                preview.sprite = data.sprite; preview.sharedMaterial = previewMaterial;
                footprint = Stage2PlacementVisuals.Outline(transform, data.footprint, outlineMaterial, "Dwelling footprint");
            }
            if (preview != null) preview.gameObject.SetActive(visible);
            if (footprint != null) footprint.gameObject.SetActive(visible);
            if (visible)
            {
                Align(preview, dwelling.PreviewPosition);
                var valid = dwelling.PreviewResult == DwellingPlacementResult.Ready;
                var color = valid ? Stage2PlacementVisuals.Valid : Stage2PlacementVisuals.Invalid;
                preview.color = color;
                footprint.SetTint(new Color(color.r,color.g,color.b,.9f));
                var p = dwelling.PreviewPosition;
                footprint.transform.position = new Vector3(p.x,p.y,Stage3TacticalPresentation.GrassSurfaceZ(p)-.03f);
            }
            if (hud == null) return;
            hud.DwellingInteractionHint = dwelling.IsPlacing ? visible ? PlacementHint(dwelling.PreviewResult) :
                "Выберите место в мире · Esc / ПКМ · отменить" :
                dwelling.IsBuilt && dwelling.NearEntrance ? game != null && game.RunDriver.Run.UsesDaytimeGates ?
                    dwelling.CanSkipDay ? "E · сон до начала ночи" : GetSleepUnavailableReason(dwelling, game) : dwelling.CanSkipDay ?
                    "E · дождаться ночи в жилище" : game != null && game.RunDriver.Run.IsNight ?
                    "Сначала отбейте орду · ждать пока нельзя" : "Жилище героя" : null;
        }

        private static string PlacementHint(DwellingPlacementResult result) => result switch
        {
            DwellingPlacementResult.Ready => "ЛКМ · построить жилище · Esc / ПКМ · отменить",
            DwellingPlacementResult.HeroOverlap => "Герой стоит на месте постройки · отойдите · Esc / ПКМ · отменить",
            DwellingPlacementResult.Occupied => "Место занято · выберите свободный участок · Esc / ПКМ · отменить",
            DwellingPlacementResult.InvalidGround => "Нужен ровный свободный участок зелёной земли · Esc / ПКМ · отменить",
            DwellingPlacementResult.InsufficientResources => "Недостаточно ресурсов для жилища · Esc / ПКМ · отменить",
            DwellingPlacementResult.CreationFailed => "Не удалось построить жилище · выберите другое место · Esc / ПКМ · отменить",
            _ => "Выберите свободную зелёную землю · Esc / ПКМ · отменить"
        };

        public static string GetSleepUnavailableReason(Stage3Dwelling dwelling, Stage2GameComposition game)
        {
            if (dwelling == null || !dwelling.IsBuilt) return "Сначала постройте жилище";
            if (!dwelling.NearEntrance) return "Подойдите ко входу в жилище";
            var run = game.RunDriver.Run;
            if (run.IsPaused) return "Сначала продолжите игру";
            if (run.IsTerminal) return "Забег завершён";
            if (run.DaytimeGateOpen || run.ActiveEnemies > 0 || game.Enemies.PendingShotCount > 0 || run.IsNight)
                return "Во время боя сон недоступен";
            if (run.IsNightTime) return "Ночью сон недоступен";
            if (!run.CanSleepUntilNight) return "Уже пора готовиться к ночи";
            return dwelling.CanSkipDay ? null : "Сначала завершите размещение";
        }

        private void Align(SpriteRenderer renderer, Vector2 position)
        {
            var ground = new Vector3(position.x,position.y-data.footprint.y*.5f,
                Stage3TacticalPresentation.GrassSurfaceZ(position));
            var padding = renderer.sprite.pivot.y / renderer.sprite.pixelsPerUnit;
            var depth = padding * Mathf.Abs(view.transform.up.z) / Mathf.Max(.1f,Mathf.Abs(view.transform.forward.z)) + .03f;
            renderer.transform.SetPositionAndRotation(ground-view.transform.forward*depth,view.transform.rotation);
            renderer.sortingOrder = -Mathf.RoundToInt(position.y*100f);
        }

        private void OnDestroy()
        {
            if (hud != null) hud.DwellingInteractionHint = null;
            if (previewMaterial != null) Destroy(previewMaterial);
            if (outlineMaterial != null) Destroy(outlineMaterial);
        }
    }
}
