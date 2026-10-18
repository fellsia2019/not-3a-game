using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Not3A.Stage2.Integration;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Cool readable moonlight on world materials; player HUD and luminous landmarks retain their contrast.</summary>
    public sealed class Stage3DayNightPresentation : MonoBehaviour
    {
        private Stage2GameComposition game;
        private Material groundTemplate;
        private readonly Dictionary<Material,Material> groundCopies = new Dictionary<Material,Material>();
        private readonly Dictionary<Renderer,Material[]> originals = new Dictionary<Renderer,Material[]>();
        private Light[] lights;
        private Color[] colors;
        private float[] intensities;
        private Color ambient;
        private float nextRefresh;
        public float NightAmount { get; private set; }
        public Func<bool> NightCondition { get; set; }

        public void Initialize(Stage2GameComposition owner, Material ground)
        {
            game=owner; groundTemplate=ground; ambient=RenderSettings.ambientLight;
            lights=FindObjectsByType<Light>(FindObjectsSortMode.None);
            colors=new Color[lights.Length]; intensities=new float[lights.Length];
            for(var i=0;i<lights.Length;i++) { colors[i]=lights[i].color; intensities[i]=lights[i].intensity; }
            BindGround();
            NightAmount=game.RunDriver.Run.IsNight?1f:0f; Apply();
            if(Debug.isDebugBuild && !Application.isBatchMode)
            {
                var capture=Environment.GetEnvironmentVariable("NOT3A_DAY_NIGHT_CAPTURE");
                if(!string.IsNullOrWhiteSpace(capture) && Path.IsPathRooted(capture))
                    StartCoroutine(CaptureFreshPlayer(capture));
            }
        }

        private IEnumerator CaptureFreshPlayer(string path)
        {
            // Opt-in evidence of ordinary player startup. Never grants resources or changes simulation/input/camera.
            var home=game.GetComponent<Stage3Dwelling>();
            while(game!=null && (!game.IsInitialized || game.GetComponent<Stage2GameHud>()==null ||
                (home!=null && !home.IsReady))) yield return null;
            if(game==null) yield break;
            yield return new WaitForSecondsRealtime(2f);
            yield return new WaitForEndOfFrame();
            var directory=Path.GetDirectoryName(path);
            if(!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("S3_DAY_NIGHT_PLAYER_CAPTURE_REQUESTED: "+path);
        }

        private void BindGround()
        {
            var shader=groundTemplate==null ? null : groundTemplate.shader;
            if(shader==null) return;
            foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(originals.ContainsKey(renderer)) continue;
                var materials=renderer.sharedMaterials; var changed=false;
                var replacement=(Material[])materials.Clone();
                for(var i=0;i<materials.Length;i++)
                {
                    var source=materials[i];
                    if(source==null || source.shader.name!="Universal Render Pipeline/Unlit" ||
                        source.renderQueue>=3000 || (source.HasProperty("_Surface") && source.GetFloat("_Surface")>.5f)) continue;
                    if(!groundCopies.TryGetValue(source,out var copy))
                    { copy=new Material(source) { name=source.name+" day/night runtime",shader=shader }; groundCopies.Add(source,copy); }
                    replacement[i]=copy; changed=true;
                }
                if(changed) { originals.Add(renderer,materials); renderer.sharedMaterials=replacement; }
            }
        }

        private void LateUpdate()
        {
            if(game==null || game.RunDriver.Run==null) return;
            var run=game.RunDriver.Run;
            if(!run.IsPaused && !run.IsTerminal)
                NightAmount=Mathf.MoveTowards(NightAmount,(NightCondition?.Invoke() ?? run.IsNight)?1f:0f,Time.deltaTime*2f);
            Apply();
            // Landscape regeneration can replace renderer instances, but never their authored materials.
            if(Time.unscaledTime>=nextRefresh) { nextRefresh=Time.unscaledTime+1f; BindGround(); }
        }

        private void Apply()
        {
            Shader.SetGlobalFloat("_Not3ANightAmount",NightAmount);
            RenderSettings.ambientLight=Color.Lerp(ambient,new Color(.13f,.19f,.35f),NightAmount);
            if(lights==null) return;
            for(var i=0;i<lights.Length;i++) if(lights[i]!=null && lights[i].type==LightType.Directional)
            { lights[i].color=Color.Lerp(colors[i],new Color(.48f,.61f,1f),NightAmount);
                lights[i].intensity=Mathf.Lerp(intensities[i],intensities[i]*.42f,NightAmount); }
        }

        private void OnDestroy()
        {
            Shader.SetGlobalFloat("_Not3ANightAmount",0f);
            RenderSettings.ambientLight=ambient;
            if(lights!=null) for(var i=0;i<lights.Length;i++) if(lights[i]!=null)
            { lights[i].color=colors[i]; lights[i].intensity=intensities[i]; }
            foreach(var entry in originals) if(entry.Key!=null) entry.Key.sharedMaterials=entry.Value;
            foreach(var copy in groundCopies.Values) Destroy(copy);
        }
    }
}
