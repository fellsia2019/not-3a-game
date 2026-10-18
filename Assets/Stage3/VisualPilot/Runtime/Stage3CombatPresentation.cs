using System;
using System.Collections;
using System.Collections.Generic;
using Not3A.Stage2.Defense;
using Not3A.Stage2.Integration;
using Not3A.Stage2.WavesAI;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Observes run-owned shots; never applies damage, selects targets or uses physics.</summary>
    public sealed class Stage3CombatPresentation : MonoBehaviour
    {
        private sealed class Flight
        {
            public Stage2Shot Shot;
            public Transform Body;
            public LineRenderer Trail;
            public Vector3 Start;
            public float ReleaseSeconds;
            public SpriteRenderer Sprite;
        }
        private sealed class Impact
        {
            public Transform Root;
            public LineRenderer Ring;
            public Transform[] Chips;
            public Vector3 Position;
            public float Age, Radius;
        }
        private Stage2GameComposition composition;
        private Stage3HeroAvatar heroAvatar;
        private Material material;
        private Mesh arrowMesh, stoneMesh;
        private readonly Dictionary<Stage2Shot, Flight> flights = new Dictionary<Stage2Shot, Flight>();
        private readonly List<Impact> impacts = new List<Impact>();
        private readonly List<Stage3TowerShotMotion> motions = new List<Stage3TowerShotMotion>();
        private string capturePrefix;
        private bool capturedArrow, capturedStone, capturedImpact;
        public int ActiveFlights => flights.Count;
        public int ActiveImpacts => impacts.Count;

        public void Initialize(Stage2GameComposition owner, Material source)
        {
            composition = owner;
            heroAvatar = owner.GetComponent<Stage2SceneBootstrap>().PlayerMover.GetComponent<Stage3HeroAvatar>();
            capturePrefix = Environment.GetEnvironmentVariable("NOT3A_COMBAT_CAPTURE");
            material = new Material(source) { name = "Shared combat effects" };
            material.SetColor("_BaseColor", Color.white);
            arrowMesh = MakeArrow();
            stoneMesh = MakeStone();
            foreach (var tower in FindObjectsByType<Stage2Tower>(FindObjectsSortMode.None)) Bind(tower);
            composition.Building.Defense.Built += Bind;
            composition.Enemies.ShotLaunched += Launch;
            composition.Enemies.ShotImpacted += Land;
            composition.Enemies.ShotsCleared += Clear;
        }

        private void Bind(Stage2Tower tower)
        {
            if (!tower.CanAttack) return;
            var motion = tower.GetComponent<Stage3TowerShotMotion>();
            if (motion != null) return;
            motion = tower.gameObject.AddComponent<Stage3TowerShotMotion>();
            motion.Initialize();
            motions.Add(motion);
        }

        private void Launch(Stage2Shot shot)
        {
            var stone = shot.SplashRadiusCells > 0f;
            // Hero equipment and tower motion each supply their visible release socket.
            var start = (Vector3)shot.Origin + Vector3.back * (stone ? 1.45f : .43f);
            if (!stone && heroAvatar != null && Vector2.Distance(shot.Origin,heroAvatar.transform.position)<.01f)
                start = heroAvatar.ReleasePosition;
            float releaseSeconds=0f;
            Stage3TowerSprite towerArt=null;
            foreach (var motion in motions)
            {
                if (motion == null || ((Vector2)motion.transform.position - shot.Origin).sqrMagnitude > .001f) continue;
                start = motion.Fire(shot);
                releaseSeconds=motion.ReleaseSeconds;
                towerArt=motion.GetComponentInChildren<Stage3TowerSprite>();
                break;
            }
            var body = MeshObject(stone ? "Stone in flight" : "Arrow in flight", stone ? stoneMesh : arrowMesh,
                stone ? new Color(.63f,.58f,.48f) : new Color(1f,.79f,.30f));
            body.localScale = Vector3.one * (stone ? .28f : .72f);
            var trail = Line("Shot trail", stone ? new Color(.78f,.69f,.5f) : new Color(1f,.86f,.4f), stone ? .055f : .035f, 7);
            var flight = new Flight { Shot = shot, Body = body, Trail = trail, Start = start, ReleaseSeconds=releaseSeconds };
            if(stone && towerArt!=null && towerArt.IsLayered)
            {
                // Preserve the very same painted stone at the cup-to-flight handoff.
                body.GetComponent<MeshRenderer>().forceRenderingOff=true;
                var painted=new GameObject("Painted stone projectile");
                painted.transform.SetParent(body,false);
                painted.transform.localScale=towerArt.LoadedStoneScale/.28f;
                flight.Sprite=painted.AddComponent<SpriteRenderer>();
                flight.Sprite.sprite=towerArt.LoadedStoneSprite;
                flight.Sprite.sharedMaterial=towerArt.Body.sharedMaterial;
            }
            flights.Add(shot, flight);
            DrawFlight(flight);
            if (!string.IsNullOrEmpty(capturePrefix) && !(stone ? capturedStone : capturedArrow))
            {
                if (stone) capturedStone = true; else capturedArrow = true;
                StartCoroutine(CaptureFlight(shot,stone ? "stone-flight" : "arrow-flight"));
            }
        }

        private Vector3 FlightPoint(Flight flight, float t)
        {
            bool stone = flight.Shot.SplashRadiusCells > 0f;
            var end = (Vector3)flight.Shot.Position + Vector3.back * (stone ? .08f : .35f);
            return Vector3.Lerp(flight.Start, end, t) + Vector3.back * (stone ? 1.65f : .12f) * 4f * t * (1f-t);
        }

        private void DrawFlight(Flight flight)
        {
            bool released=flight.Shot.Elapsed >= flight.ReleaseSeconds;
            flight.Body.GetComponent<MeshRenderer>().enabled=released;
            flight.Trail.enabled=released;
            if(flight.Sprite!=null) flight.Sprite.enabled=released;
            if (!released) return;
            var duration=flight.Shot.Duration-flight.ReleaseSeconds;
            var t=duration <= 0f ? 1f : Mathf.Clamp01((flight.Shot.Elapsed-flight.ReleaseSeconds)/duration);
            flight.Body.position = FlightPoint(flight,t);
            var direction = FlightPoint(flight, Mathf.Min(1f,t+.01f)) - FlightPoint(flight,Mathf.Max(0f,t-.01f));
            if (direction.sqrMagnitude > .000001f)
                flight.Body.rotation = Quaternion.LookRotation(direction, Vector3.back);
            if(flight.Sprite!=null)
            {
                var camera=composition.GetComponent<Stage2SceneBootstrap>().CameraFollow.BoundCamera;
                flight.Sprite.transform.SetPositionAndRotation(flight.Body.position-camera.transform.forward*.01f,camera.transform.rotation);
                flight.Sprite.sortingOrder=-Mathf.RoundToInt(Mathf.Min(flight.Shot.Origin.y,flight.Shot.Position.y)*100f)+10;
            }
            for (int i=0;i<7;i++) flight.Trail.SetPosition(i,FlightPoint(flight,Mathf.Max(0f,t-i*.025f)));
        }

        private void Land(Stage2Shot shot)
        {
            if (flights.TryGetValue(shot,out var flight))
            {
                Destroy(flight.Body.gameObject); Destroy(flight.Trail.gameObject); flights.Remove(shot);
            }
            bool stone = shot.SplashRadiusCells > 0f;
            var root = new GameObject(stone ? "Stone area impact" : "Arrow impact").transform;
            root.SetParent(transform,false);
            var ring = Line("Impact ring", stone ? new Color(.95f,.75f,.4f) : new Color(1f,.88f,.5f), stone ? .09f : .04f, 49);
            ring.transform.SetParent(root,false);
            var impact = new Impact { Root = root, Ring = ring, Position = (Vector3)shot.Position + Vector3.back*(stone ? 0f : .32f),
                Radius = stone ? shot.SplashRadiusCells : .28f, Chips = new Transform[stone ? 10 : 5] };
            for (int i=0;i<impact.Chips.Length;i++)
            {
                impact.Chips[i] = MeshObject("Impact fragment",stoneMesh,
                    stone ? new Color(.58f,.49f,.37f) : new Color(1f,.82f,.32f));
                impact.Chips[i].SetParent(root,false);
            }
            impacts.Add(impact);
            DrawImpact(impact);
            if (stone && !capturedImpact && !string.IsNullOrEmpty(capturePrefix))
            {
                capturedImpact = true;
                StartCoroutine(CaptureFrame("stone-impact"));
            }
        }

        private IEnumerator CaptureFlight(Stage2Shot shot, string label)
        {
            if (shot.SplashRadiusCells > 0f)
            {
                while (flights.ContainsKey(shot) && shot.Progress < .1f) yield return null;
                if (flights.ContainsKey(shot)) yield return CaptureFrame("stone-windup");
                while (flights.TryGetValue(shot,out var view) && shot.Elapsed < view.ReleaseSeconds) yield return null;
                if (flights.ContainsKey(shot)) yield return CaptureFrame("stone-release");
            }
            while (flights.ContainsKey(shot) && shot.Progress < .45f) yield return null;
            if (flights.ContainsKey(shot)) yield return CaptureFrame(label);
        }

        private IEnumerator CaptureFrame(string label)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(capturePrefix + "-" + label + ".png");
            Debug.Log("S3_COMBAT_CAPTURE " + label);
        }

        private void DrawImpact(Impact impact)
        {
            var t = Mathf.Clamp01(impact.Age / .55f);
            // Outer stone ring marks the actual two-cell boundary from the first impact frame.
            var radius = impact.Radius > 1f ? impact.Radius : impact.Radius * (.4f + .6f*t);
            impact.Ring.widthMultiplier = .085f*(1f-t);
            for (int i=0;i<49;i++)
            {
                float a = i*Mathf.PI*2f/48;
                impact.Ring.SetPosition(i,impact.Position + new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,-.045f));
            }
            for (int i=0;i<impact.Chips.Length;i++)
            {
                float a = i*Mathf.PI*2f/impact.Chips.Length;
                float spread = impact.Radius*(.12f+.7f*t);
                impact.Chips[i].position = impact.Position + new Vector3(Mathf.Cos(a)*spread,Mathf.Sin(a)*spread,
                    -.08f - Mathf.Sin(t*Mathf.PI)*(impact.Radius > 1f ? .6f : .3f));
                impact.Chips[i].localScale = Vector3.one * (impact.Radius > 1f ? .15f : .065f)*(1f-t);
                impact.Chips[i].localRotation = Quaternion.Euler(i*31f+t*360f,i*17f,0);
            }
        }

        private void LateUpdate()
        {
            if (composition == null || composition.RunDriver?.Run == null) return;
            if (composition.RunDriver.Run.IsPaused) return;
            foreach (var motion in motions) if (motion != null)
            {
                motion.TrackTarget(composition.Enemies);
                motion.Advance(Time.deltaTime);
            }
            foreach (var flight in flights.Values) DrawFlight(flight);
            for (var i=impacts.Count-1;i>=0;i--)
            {
                var impact = impacts[i]; impact.Age += Time.deltaTime;
                if (impact.Age >= .55f) { Destroy(impact.Root.gameObject); impacts.RemoveAt(i); }
                else DrawImpact(impact);
            }
        }

        private Transform MeshObject(string label, Mesh mesh, Color color)
        {
            var obj = new GameObject(label);
            obj.transform.SetParent(transform,false);
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = obj.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            var tint = new MaterialPropertyBlock(); tint.SetColor("_BaseColor",color); renderer.SetPropertyBlock(tint);
            return obj.transform;
        }

        private LineRenderer Line(string label, Color color, float width, int count)
        {
            var obj = new GameObject(label); obj.transform.SetParent(transform,false);
            var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = material;
            line.useWorldSpace = true; line.positionCount = count; line.widthMultiplier = width;
            line.startColor = line.endColor = color; line.numCapVertices = 2;
            line.generateLightingData = true;
            line.shadowCastingMode = ShadowCastingMode.Off;
            var tint = new MaterialPropertyBlock(); tint.SetColor("_BaseColor",color); line.SetPropertyBlock(tint);
            return line;
        }

        private void Clear()
        {
            foreach (var flight in flights.Values)
            {
                if (flight.Body != null) Destroy(flight.Body.gameObject);
                if (flight.Trail != null) Destroy(flight.Trail.gameObject);
            }
            flights.Clear();
            foreach (var impact in impacts) if (impact.Root != null) Destroy(impact.Root.gameObject);
            impacts.Clear();
            foreach (var motion in motions) if (motion != null) motion.ResetPose();
            motions.RemoveAll(motion => motion == null);
        }

        private void OnDestroy()
        {
            if (composition != null)
            {
                if (composition.Building?.Defense != null) composition.Building.Defense.Built -= Bind;
                if (composition.Enemies != null)
                {
                    composition.Enemies.ShotLaunched -= Launch;
                    composition.Enemies.ShotImpacted -= Land;
                    composition.Enemies.ShotsCleared -= Clear;
                }
            }
            Clear(); Destroy(material); Destroy(arrowMesh); Destroy(stoneMesh);
        }

        private static Mesh MakeStone()
        {
            var mesh = new Mesh { name = "Original low-poly projectile and debris" };
            mesh.vertices = new[] {Vector3.up*.5f,Vector3.down*.5f,Vector3.left*.5f,Vector3.right*.5f,Vector3.forward*.5f,Vector3.back*.5f};
            mesh.triangles = new[] {0,4,3,0,3,5,0,5,2,0,2,4,1,3,4,1,5,3,1,2,5,1,4,2};
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        private static Mesh MakeArrow()
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int ring=0;ring<3;ring++) for (int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3; float radius=ring == 2 ? .09f : .032f;
                vertices.Add(new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,ring == 0 ? -.45f : .23f));
            }
            vertices.Add(new Vector3(0,0,.5f));
            for (int i=0;i<6;i++)
            {
                int j=(i+1)%6;
                triangles.AddRange(new[]{i,j,6+i,j,6+j,6+i,12+i,12+j,18});
            }
            // Two white/gold feather planes at the tail give a readable arrow silhouette.
            foreach (var axis in new[]{Vector3.right,Vector3.up})
            {
                int k=vertices.Count;
                vertices.AddRange(new[]{axis*.1f+Vector3.back*.42f,-axis*.1f+Vector3.back*.42f,Vector3.back*.18f});
                triangles.AddRange(new[]{k,k+1,k+2,k+2,k+1,k});
            }
            var mesh = new Mesh { name = "Original arrow shaft head and feathers" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
    }
}
