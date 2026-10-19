using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.RunState;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Transient hit feedback and stopped-game loss presentation; owns no simulation state.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(200)]
    public sealed class Stage3DefeatPresentation : MonoBehaviour
    {
        private struct Hit
        {
            public float age, strength;
            public bool visible, sampled;
            public double sampleStartedAt;
            public Vector2 origin;
            public int serial;
        }
        private readonly List<Hit> hits = new();
        private readonly Vector4[] shaderHits = new Vector4[8];
        private readonly float[] shaderSeeds = new float[8];
        private Camera viewCamera;
        private Stage2RunDriver driver;
        private Stage2Run run;
        private Stage3LandmarkSprite objective;
        private Stage3DefeatSettings settings;
        private Stage3FantasyHud hud;
        private Stage2GameHud legacyHud;
        private Material material;
        private float lossAge = -1f;
        private double lossStartedAt;
        private Vector2 lossOrigin = new Vector2(.5f, .5f);
        private Matrix4x4 savedView;
        private bool viewOverridden;
        private bool savedAutomaticView;
        public int HitSequence { get; private set; }
        public int ActiveHitCount => hits.Count;
        public float LastHitDamageFraction { get; private set; }
        public Vector2 ShakePixels { get; private set; }
        public bool OwnsLossPresentation => isActiveAndEnabled && run != null && run.Phase == Stage2RunPhase.Lost;
        public float LossElapsed => Mathf.Max(0, lossAge);
        public float Whiteout => !OwnsLossPresentation || settings == null ? 0 :
            Mathf.SmoothStep(0, 1, Mathf.InverseLerp(settings.whiteoutSeconds * .18f, settings.whiteoutSeconds, LossElapsed));
        public float RetryOpacity => !OwnsLossPresentation || settings == null ? 0 :
            Mathf.SmoothStep(0, 1, Mathf.InverseLerp(settings.whiteoutSeconds + settings.whiteHoldSeconds,
                settings.whiteoutSeconds + settings.whiteHoldSeconds + settings.retryFadeSeconds, LossElapsed));
        public bool RetryReady => OwnsLossPresentation && RetryOpacity >= .999f;
        public bool HasVisibleFractures { get; private set; }
        public bool RenderViewOverridden => viewOverridden;

        public void Bind(Camera camera, Stage2RunDriver runDriver, Stage3LandmarkSprite dome)
        {
            Unbind();
            viewCamera = camera; driver = runDriver; objective = dome; run = driver != null ? driver.Run : null;
            settings = Resources.Load<Stage3DefeatSettings>("Stage3DefeatSettings");
            if (settings == null || settings.fractureShader == null)
            { Debug.LogError("S3_DEFEAT_BINDING_ERROR: authored defeat settings/shader missing.", this); enabled = false; return; }
            if (material != null) Destroy(material);
            material = new Material(settings.fractureShader) { name = "Transient throne fracture", hideFlags = HideFlags.HideAndDontSave };
            hud = GetComponent<Stage3FantasyHud>();
            legacyHud = GetComponent<Stage2GameHud>();
            hud?.BindDefeatPresentation(this);
            legacyHud?.BindLossPresentation(() => OwnsLossPresentation);
            driver?.SetRetryReadiness(() => !OwnsLossPresentation || RetryReady);
            if (run != null)
            {
                run.ThroneDamaged += OnDamage;
                run.Changed += OnRunChanged;
                run.Reset += ResetPresentation;
                run.Rewound += ResetPresentation;
            }
            ResetPresentation(); OnRunChanged();
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += BeginCamera;
            RenderPipelineManager.endCameraRendering += EndCamera;
            if (run != null) OnRunChanged();
        }
        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCamera;
            RenderPipelineManager.endCameraRendering -= EndCamera;
            RestoreView(); ResetPresentation();
        }
        private void OnDestroy()
        {
            Unbind();
            if (material != null) Destroy(material);
        }
        private void Unbind()
        {
            RestoreView();
            if (run == null) return;
            run.ThroneDamaged -= OnDamage; run.Changed -= OnRunChanged;
            run.Reset -= ResetPresentation; run.Rewound -= ResetPresentation;
            driver?.SetRetryReadiness(null);
            legacyHud?.BindLossPresentation(null);
            hud?.BindDefeatPresentation(null);
            run = null;
        }
        private void ResetPresentation()
        {
            RestoreView(); hits.Clear(); ShakePixels = Vector2.zero;
            lossAge = -1; lossStartedAt = 0; HasVisibleFractures = false; HitSequence = 0;
            LastHitDamageFraction = run == null ? 0 : 1f - run.ThroneHealth / (float)Mathf.Max(1, run.ThroneMaximum);
        }
        private void OnDamage(int amount)
        {
            if (!isActiveAndEnabled || settings == null || amount <= 0 || run == null) return;
            LastHitDamageFraction = Mathf.Clamp01(1f - run.ThroneHealth / (float)Mathf.Max(1, run.ThroneMaximum));
            var visible = ProjectFlame(out var origin);
            HitSequence++;
            // Bounded transient draw budget. Every hit still resets/strengthens the shake.
            if (hits.Count == shaderHits.Length) hits.RemoveAt(0);
            hits.Add(new Hit { age = 0, strength = LastHitDamageFraction, visible = visible, origin = origin, serial = HitSequence });
            if (visible) lossOrigin = origin;
        }
        private void OnRunChanged()
        {
            if (!isActiveAndEnabled || !OwnsLossPresentation || lossAge >= 0) return;
            lossAge = 0;
            lossStartedAt = Time.realtimeSinceStartupAsDouble;
            if (!ProjectFlame(out lossOrigin)) lossOrigin = new Vector2(.5f, .5f);
        }
        private bool ProjectFlame(out Vector2 origin)
        {
            origin = new Vector2(.5f, .5f);
            if (hud != null && hud.IsModalOpen) return false;
            var body = objective != null ? objective.Body : null;
            if (viewCamera == null || body == null || body.sprite == null || !body.enabled) return false;
            var sprite = body.sprite;
            // Approved soul flame center, in normalized pixels of the current original sprite.
            var local = (new Vector2(sprite.rect.width * .5f, sprite.rect.height * .53f) - sprite.pivot) / sprite.pixelsPerUnit;
            var projected = viewCamera.WorldToViewportPoint(body.transform.TransformPoint(local));
            origin = projected;
            return projected.z > 0 && projected.x >= 0 && projected.x <= 1 && projected.y >= 0 && projected.y <= 1;
        }
        private void Update()
        {
            if (settings == null) return;
            var now = Time.realtimeSinceStartupAsDouble;
            var shake = Vector2.zero;
            for (var i = hits.Count - 1; i >= 0; i--)
            {
                var hit = hits[i];
                // A hit received after a slow frame has not been presented yet. Start its
                // first sample now, then expire strictly by real elapsed time, not stale delta.
                if (!hit.sampled) { hit.sampled = true; hit.sampleStartedAt = now; }
                hit.age = (float)(now - hit.sampleStartedAt);
                if (hit.age >= settings.hitSeconds) { hits.RemoveAt(i); continue; }
                var envelope = Mathf.Pow(Mathf.Clamp01(1 - hit.age / settings.shakeSeconds), 2);
                var phase = hit.age * 68f + hit.serial * 2.4f;
                shake += new Vector2(Mathf.Sin(phase), Mathf.Cos(phase * 1.17f)) *
                    (settings.shakePixels * Mathf.Lerp(.6f, 1f, hit.strength) * envelope);
                hits[i] = hit;
            }
            ShakePixels = Vector2.ClampMagnitude(shake, settings.shakePixels * 1.3f);
            if (OwnsLossPresentation && lossAge >= 0) lossAge = (float)(now - lossStartedAt);
        }
        private void LateUpdate() => RefreshProjection();
        private void RefreshProjection()
        {
            // Follow and landmark alignment have completed. Refresh again on Repaint so the
            // projected origin and the actual rendered dome always share the final view.
            var visible = ProjectFlame(out var origin);
            HasVisibleFractures = false;
            for (var i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];
                hit.visible &= visible;
                if (hit.visible) { hit.origin = origin; HasVisibleFractures = true; }
                hits[i] = hit;
            }
            if (OwnsLossPresentation && visible) lossOrigin = origin;
        }
        private void BeginCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera != viewCamera || ShakePixels.sqrMagnitude < .0001f || settings == null) return;
            RestoreView(); savedView = camera.worldToCameraMatrix;
            var automatic = Matrix4x4.Scale(new Vector3(1, 1, -1)) * camera.transform.worldToLocalMatrix;
            savedAutomaticView = true;
            for (var i = 0; i < 16; i++)
                if (Mathf.Abs(savedView[i] - automatic[i]) > .0001f) { savedAutomaticView = false; break; }
            var unitsPerPixel = 2f * camera.orthographicSize / Mathf.Max(1, camera.pixelHeight);
            camera.worldToCameraMatrix = Matrix4x4.Translate(new Vector3(ShakePixels.x, ShakePixels.y, 0) * unitsPerPixel) * savedView;
            viewOverridden = true;
        }
        private void EndCamera(ScriptableRenderContext context, Camera camera) { if (camera == viewCamera) RestoreView(); }
        private void RestoreView()
        {
            if (viewOverridden && viewCamera != null)
            {
                // Reset restores transform-driven follow on subsequent frames. Merely assigning
                // the old default matrix would leave Unity in custom-view mode permanently.
                if (savedAutomaticView) viewCamera.ResetWorldToCameraMatrix();
                else viewCamera.worldToCameraMatrix = savedView;
            }
            viewOverridden = false;
        }
        public void DrawOverlay()
        {
            if (!isActiveAndEnabled || material == null || Event.current.type != EventType.Repaint) return;
            RefreshProjection();
            if (!OwnsLossPresentation && !HasVisibleFractures) return;
            for (var i = 0; i < shaderHits.Length; i++)
            {
                shaderHits[i] = Vector4.zero; shaderSeeds[i] = 0;
                if (i >= hits.Count || !hits[i].visible) continue;
                var hit = hits[i];
                shaderHits[i] = new Vector4(hit.origin.x + ShakePixels.x / Mathf.Max(1, Screen.width),
                    hit.origin.y + ShakePixels.y / Mathf.Max(1, Screen.height), hit.age / settings.hitSeconds,
                    Mathf.Lerp(.23f, .9f, hit.strength));
                shaderSeeds[i] = hit.serial;
            }
            material.SetVectorArray("_Hits", shaderHits); material.SetFloatArray("_Seeds", shaderSeeds);
            material.SetVector("_Origin", new Vector4(lossOrigin.x, lossOrigin.y, 0, 0));
            material.SetFloat("_Fatal", OwnsLossPresentation ? Mathf.Clamp01(LossElapsed / settings.whiteoutSeconds) : 0);
            material.SetFloat("_Whiteout", Whiteout);
            material.SetFloat("_Aspect", Screen.width / (float)Mathf.Max(1, Screen.height));
            material.SetFloat("_Pixel", 1f / Mathf.Max(1, Screen.height));
            Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture,
                new Rect(0, 0, 1, 1), 0, 0, 0, 0, Color.white, material);
        }
        public bool Retry() => RetryReady && driver != null && driver.RetryFailedWave();
        public bool CanRetry => RetryReady && driver != null && driver.CanRetry;
    }
}
