using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.WavesAI;
using Not3A.Stage3.PlayerInterface;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Read-only local spell feedback. Gameplay calls this only after a confirmed effect.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(255)]
    public sealed class Stage3HeroSkillPresentation : MonoBehaviour
    {
        public const int MaximumImpactCount = 12;
        public const float ImpactLifetime = .38f;
        private struct Impact { public int Skill; public Vector3 Position; public float Radius, Age; }
        private readonly List<Impact> impacts = new List<Impact>();
        private struct PersistentArea { public int Skill; public Vector3 Position; public float Radius,Duration,Age; }
        private readonly List<PersistentArea> areas=new List<PersistentArea>();
        private Stage3GuildWorldDriver guild;
        private float animationTime;
        public int ActivePersistentAreaCount=>areas.Count;
        private Stage2GameComposition game;
        private Stage3FantasyHud hud;
        private Stage2GameHud legacy;
        private Camera view;
        private bool bound, preview, previewValid;
        private int previewSkill;
        private Vector3 previewPosition;
        private float previewRadius;
        public int ActiveImpactCount => impacts.Count;
        public bool HasTargetPreview => preview;
        public int PreviewSkillId => previewSkill;
        public float PreviewRadius => previewRadius;

        public void Initialize(Stage2GameComposition owner)
        {
            Release(); game = owner;
            if (game?.RunDriver?.Run == null) return;
            hud = game.GetComponent<Stage3FantasyHud>();
            legacy = game.GetComponent<Stage2GameHud>();guild=game.GetComponent<Stage3GuildWorldDriver>();
            view = game.GetComponent<Stage2SceneBootstrap>()?.CameraFollow?.BoundCamera;
            game.RunDriver.Run.Reset += ClearPresentation;
            game.RunDriver.Run.Rewound += ClearPresentation;
            game.Enemies.ShotsCleared += ClearPresentation;
            bound = true;
        }

        private bool Hidden => hud != null && hud.IsModalOpen || legacy != null && legacy.DebugOpen;
        private bool Frozen => game?.RunDriver?.Run == null || game.RunDriver.Run.IsPaused || Hidden;
        private bool CanPresent => bound && isActiveAndEnabled && !Hidden && !game.RunDriver.Run.IsTerminal;

        public void PresentSkillImpact(int skillId, Vector3 position, float radius)
        {
            if (!CanPresent || skillId < 0 || skillId > 10 || !Finite(position) || !float.IsFinite(radius)) return;
            if (impacts.Count >= MaximumImpactCount) impacts.RemoveAt(0);
            impacts.Add(new Impact { Skill = skillId, Position = position, Radius = Mathf.Max(0, radius) });
        }

        public void PresentPersistentSkillArea(int id,Vector2 centre,float radius,float duration)
        {
            if(!CanPresent||(id!=4&&id!=9)||!Finite(centre)||!float.IsFinite(radius)||radius<=0||!float.IsFinite(duration)||duration<=0)return;
            if(areas.Count>=MaximumImpactCount)areas.RemoveAt(0);
            areas.Add(new PersistentArea{Skill=id,Position=new Vector3(centre.x,centre.y,Stage3TacticalPresentation.GrassSurfaceZ(centre)),Radius=radius,Duration=duration});
        }

        /// <summary>Radius is the actual affected world radius; an invalid cast previews a muted red boundary.</summary>
        public void SetTargetPreview(int skillId, Vector3 position, float radius, bool valid)
        {
            if (!CanPresent || skillId < 0 || skillId > 10 || !Finite(position) || !float.IsFinite(radius) || radius <= 0)
            { ClearTargetPreview(); return; }
            preview = true; previewSkill = skillId; previewPosition = position; previewRadius = radius; previewValid = valid;
        }
        public void ClearTargetPreview() { preview = false; previewRadius = 0; }
        public void ClearPresentation() { impacts.Clear();areas.Clear();animationTime=0; ClearTargetPreview(); }
        private static bool Finite(Vector3 p) => float.IsFinite(p.x) && float.IsFinite(p.y) && float.IsFinite(p.z);
        private void LateUpdate()
        {
            if (!bound) return;
            if (game.RunDriver.Run.IsTerminal) { ClearPresentation(); return; }
            // Persistent gameplay areas expire while a modal hides their rendering.
            if (game.RunDriver.Run.IsPaused || (legacy?.DebugOpen ?? false)) return;
            animationTime+=Time.deltaTime;
            for(var i=areas.Count-1;i>=0;i--)
            {var area=areas[i];area.Age+=Time.deltaTime;if(area.Age>=area.Duration)areas.RemoveAt(i);else areas[i]=area;}
            if (Frozen) return; // Existing short impact/target-preview semantics.
            for (var i = impacts.Count - 1; i >= 0; i--)
            {
                var effect = impacts[i]; effect.Age += Time.deltaTime;
                if (effect.Age >= ImpactLifetime) impacts.RemoveAt(i); else impacts[i] = effect;
            }
        }
        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || !CanPresent || view == null) return;
            var depth = GUI.depth; var matrix = GUI.matrix; var color = GUI.color;
            GUI.depth = 1; GUI.matrix = Matrix4x4.identity;
            try
            {
                var scale = hud != null ? hud.UiScale : Mathf.Clamp(Screen.height / 900f, .55f, 2f);
                if (preview && !Frozen)
                    Ring(previewPosition, previewRadius, previewValid ? new Color(.23f, .86f, .91f, .85f) : new Color(.84f, .25f, .28f, .75f), 1.6f * scale);
                foreach (var effect in impacts)
                {
                    if (!Project(effect.Position, out var center)) continue;
                    var fraction = Mathf.Clamp01(effect.Age / ImpactLifetime);
                    var tint = effect.Skill == 4 ? new Color(1f,.18f,.27f,1f-fraction) : effect.Skill==5 ? new Color(.49f,.92f,.20f,1f-fraction) : effect.Skill == 2 || effect.Skill == 9 ? new Color(1f, .65f, .25f, 1f - fraction) :
                        effect.Skill == 3 || effect.Skill == 10 ? new Color(.51f, .83f, 1f, 1f - fraction) :
                        effect.Skill == 8 ? new Color(.48f, .94f, .65f, 1f - fraction) : new Color(.86f, .77f, 1f, 1f - fraction);
                    // The same restrained local star language as confirmed field damage; no new numbers or camera motion.
                    var extent = (5f + 11f * fraction) * scale;
                    for (var ray = 0; ray < 6; ray++)
                    {
                        var angle = ray * Mathf.PI / 3f + effect.Skill * .13f;
                        var axis = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        Line(center + axis * extent * .33f, center + axis * extent, 1.7f * scale, tint);
                    }
                    if (effect.Radius > 0 && (effect.Skill == 2 || effect.Skill == 3 || effect.Skill == 6 || effect.Skill == 9 || effect.Skill == 10))
                        Ring(effect.Position, effect.Radius * (.72f + .28f * fraction), new Color(tint.r, tint.g, tint.b, tint.a * .45f), scale);
                }
                foreach(var area in areas)DrawPersistentArea(area,scale);
                DrawEnemyStatuses(scale);
            }
            finally { GUI.depth = depth; GUI.matrix = matrix; GUI.color = color; }
        }
        private void DrawPersistentArea(PersistentArea area,float scale)
        {
            var fade=Mathf.Clamp01((area.Duration-area.Age)/.3f);
            var red=new Color(1f,.16f,.23f,fade);var orange=new Color(1f,.42f,.08f,fade);
            Ring(area.Position,area.Radius,area.Skill==4?new Color(.95f,.17f,.25f,.28f*fade):new Color(1f,.43f,.12f,.30f*fade),scale);
            if(area.Skill==4)
            {
                var phase=area.Age*7f;
                var offset=new Vector3(Mathf.Cos(phase)*.70f,Mathf.Sin(phase*2f)*.42f,0)*area.Radius;
                var ahead=new Vector3(Mathf.Cos(phase+.12f)*.70f,Mathf.Sin((phase+.12f)*2f)*.42f,0)*area.Radius;
                var point=area.Position+offset;point.z=Stage3TacticalPresentation.GrassSurfaceZ(point)-.18f;
                if(!Project(point,out var centre)||!Project(area.Position+ahead,out var next))return;
                var axis=(next-centre).normalized;if(axis.sqrMagnitude<.1f)axis=Vector2.right;
                var normal=new Vector2(-axis.y,axis.x);var tip=centre+axis*15f*scale;var tail=centre-axis*16f*scale;
                Line(tail,tip,4f*scale,new Color(red.r,red.g,red.b,fade*.35f));
                Line(tail,tip,1.7f*scale,red);
                Line(tip,tip-axis*9f*scale+normal*5f*scale,2f*scale,red);
                Line(tip,tip-axis*9f*scale-normal*5f*scale,2f*scale,red);
                Line(tail,tail+axis*6f*scale+normal*4f*scale,1.2f*scale,new Color(1f,.75f,.65f,fade));
                Line(tail,tail+axis*6f*scale-normal*4f*scale,1.2f*scale,new Color(1f,.75f,.65f,fade));
            }
            else
            {
                for(var i=0;i<13;i++)
                {
                    var angle=i*2.399963f;var radius=Mathf.Sqrt(i/13f)*area.Radius*.82f;
                    var world=area.Position+new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*radius;
                    world.z=Stage3TacticalPresentation.GrassSurfaceZ(world)-.03f;
                    if(Project(world,out var centre))Flame(centre,scale*(1.1f+.25f*Mathf.Sin(area.Age*9f+i)),orange);
                }
            }
        }

        private static void Flame(Vector2 foot,float scale,Color tint)
        {
            // Broad ember body and bright tapered tongues remain readable over patterned grass.
            var ember=foot+Vector2.down*3f*scale;var mid=foot+new Vector2(-2,-11)*scale;
            var tip=foot+new Vector2(3,-24)*scale;
            OutlinedLine(ember+Vector2.left*5f*scale,ember+Vector2.right*5f*scale,7f*scale,new Color(.68f,.12f,.025f,tint.a));
            OutlinedLine(ember,mid,10f*scale,new Color(.97f,.24f,.025f,tint.a));
            OutlinedLine(mid,tip,5f*scale,tint);
            Line(ember,mid+Vector2.up*2f*scale,5f*scale,new Color(1f,.79f,.20f,tint.a));
            Line(ember,foot+new Vector2(2,-12)*scale,2.5f*scale,new Color(1f,.96f,.67f,tint.a));
        }

        private void DrawEnemyStatuses(float scale)
        {
            for(var i=0;i<game.Enemies.ActiveCount;i++)
            {
                var enemy=game.Enemies.GetEnemy(i);
                if(enemy==null||!enemy.IsAlive||!HasVisibleStatus(enemy.Statuses))continue;
                var body=enemy.GetComponentInChildren<Stage3SkeletonSprite>()?.Body;
                DrawStatus(enemy.transform.position,enemy.Statuses,body,scale);
            }
            if(guild==null)guild=game.GetComponent<Stage3GuildWorldDriver>();
            if(guild!=null)foreach(var enemy in guild.Enemies)
            {
                if(!enemy.Alive||!HasVisibleStatus(enemy.Statuses))continue;
                guild.TryGetVisual(enemy,out var visual);
                DrawStatus(enemy.Position,enemy.Statuses,visual?.Body,scale);
            }
        }
        private static bool HasVisibleStatus(Stage2CombatStatuses status)=>status!=null&&
            (status.FrozenRemaining>0||status.StunnedRemaining>0||status.BurningRemaining>0||status.PoisonRemaining>0||
             status.SlowMultiplier<1||status.PhysicalVulnerability>0||status.MagicVulnerability>0);

        private Rect StatusSilhouette(Vector2 foot,SpriteRenderer body,float scale)
        {
            if(body==null)return new Rect(foot.x-13f*scale,foot.y-45f*scale,26f*scale,45f*scale);
            var bounds=body.bounds;var minimum=new Vector2(float.PositiveInfinity,float.PositiveInfinity);
            var maximum=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
            for(var i=0;i<8;i++)
            {
                var point=new Vector3((i&1)==0?bounds.min.x:bounds.max.x,(i&2)==0?bounds.min.y:bounds.max.y,(i&4)==0?bounds.min.z:bounds.max.z);
                var projected=view.WorldToScreenPoint(point);var pixel=new Vector2(projected.x,Screen.height-projected.y);
                minimum=Vector2.Min(minimum,pixel);maximum=Vector2.Max(maximum,pixel);
            }
            return Rect.MinMaxRect(minimum.x,minimum.y,maximum.x,maximum.y);
        }
        private void DrawStatus(Vector3 position,Stage2CombatStatuses status,SpriteRenderer body,float scale)
        {
            if(!Project(position,out var foot))return;
            var shape=StatusSilhouette(foot,body,scale);
            var middle=shape.center;var halfWidth=Mathf.Clamp(shape.width*.45f,12f*scale,46f*scale);
            var cyan=new Color(.56f,.94f,1f,.96f);
            if(status.FrozenRemaining>0)
            {
                // Ice brackets surround the lower body; the actor and its health bar stay uncovered.
                var top=new Vector2(middle.x,Mathf.Lerp(shape.yMin,shape.yMax,.42f));
                var left=new Vector2(middle.x-halfWidth,shape.yMax-2f*scale);
                var right=new Vector2(middle.x+halfWidth,shape.yMax-2f*scale);
                OutlinedLine(left,top,2f*scale,cyan);OutlinedLine(top,right,2f*scale,cyan);
                OutlinedLine(left,right,1.6f*scale,cyan);
                Line(left,top+Vector2.right*5f*scale,1.1f*scale,new Color(.8f,1f,1f,.65f));
            }
            else if(status.SlowMultiplier<1)
            {OutlinedLine(foot+new Vector2(-12,2)*scale,foot+new Vector2(-4,2)*scale,2f*scale,cyan);OutlinedLine(foot+new Vector2(4,2)*scale,foot+new Vector2(12,2)*scale,2f*scale,cyan);}
            if(status.StunnedRemaining>0)
            {
                var top=new Vector2(middle.x,shape.yMin-25f*scale);
                for(var i=0;i<3;i++)
                {
                    var a=animationTime*2f+i*Mathf.PI*2f/3f;var point=top+new Vector2(Mathf.Cos(a)*14f,Mathf.Sin(a)*4f)*scale;
                    OutlinedLine(point+new Vector2(-3,-4)*scale,point+new Vector2(3,0)*scale,2f*scale,cyan);
                    OutlinedLine(point+new Vector2(3,0)*scale,point+new Vector2(-2,5)*scale,2f*scale,cyan);
                }
            }
            if(status.BurningRemaining>0)Flame(new Vector2(middle.x+halfWidth+8f*scale,shape.yMax-4f*scale),scale*.8f,new Color(1f,.44f,.10f,.94f));
            if(status.PoisonRemaining>0)
            {
                var green=new Color(.68f,1f,.25f,1f);
                var origin=new Vector2(middle.x-halfWidth-10f*scale,Mathf.Lerp(shape.yMin,shape.yMax,.40f));
                for(var i=0;i<3;i++)
                {
                    var rise=Mathf.Repeat(animationTime*9f+i*7f,21f);var point=origin+new Vector2((i-1)*8f,-rise)*scale;
                    OutlinedLine(point+new Vector2(0,-5)*scale,point+new Vector2(-3,1)*scale,3f*scale,green);
                    OutlinedLine(point+new Vector2(-3,1)*scale,point+new Vector2(2,3)*scale,3f*scale,green);
                    Line(point+Vector2.down*3f*scale,point,1.3f*scale,new Color(.94f,1f,.75f,1f));
                }
            }
            // School-specific broken guards sit on opposite hips, away from poison, stun and damage text.
            if(status.PhysicalVulnerability>0)BrokenGuard(new Vector2(middle.x-halfWidth-12f*scale,Mathf.Lerp(shape.yMin,shape.yMax,.75f)),scale*1.2f,new Color(1f,.68f,.32f,1f));
            if(status.MagicVulnerability>0)BrokenGuard(new Vector2(middle.x+halfWidth+12f*scale,Mathf.Lerp(shape.yMin,shape.yMax,.60f)),scale*1.2f,new Color(.84f,.60f,1f,1f));
        }

        private static void BrokenGuard(Vector2 point,float scale,Color tint)
        {
            OutlinedLine(point+new Vector2(-4,-5)*scale,point+new Vector2(-4,1)*scale,2f*scale,tint);
            OutlinedLine(point+new Vector2(4,-5)*scale,point+new Vector2(4,1)*scale,2f*scale,tint);
            OutlinedLine(point+new Vector2(-4,1)*scale,point+new Vector2(-1,5)*scale,2f*scale,tint);
            OutlinedLine(point+new Vector2(4,1)*scale,point+new Vector2(1,5)*scale,2f*scale,tint);
        }
        private static void OutlinedLine(Vector2 a,Vector2 b,float width,Color tint)
        {
            Line(a,b,width+2.4f*Mathf.Clamp(Screen.height/900f,.75f,1.5f),new Color(.10f,.055f,.12f,tint.a*.90f));
            Line(a,b,width,tint);
        }

        private void Ring(Vector3 position, float radius, Color tint, float width)
        {
            Vector2 previous = default; var havePrevious = false;
            for (var i = 0; i <= 48; i++)
            {
                var angle = i * Mathf.PI * 2f / 48f;
                var point = position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                point.z = Stage3TacticalPresentation.GrassSurfaceZ(point);
                var projected = Project(point, out var next);
                if (projected && havePrevious) Line(previous, next, width, tint);
                previous = next; havePrevious = projected;
            }
        }
        private bool Project(Vector3 world, out Vector2 point)
        {
            var screen = view.WorldToScreenPoint(world); point = new Vector2(screen.x, Screen.height - screen.y);
            if (screen.z <= 0 || point.x < 0 || point.x > Screen.width || point.y < 0 || point.y > Screen.height) return false;
            return hud == null || !hud.WorldHudRect.Contains(point) && !hud.ResourcesRect.Contains(point) &&
                !hud.StatusRect.Contains(point) && !hud.CycleDialRect.Contains(point) && !hud.CyclePhaseRect.Contains(point);
        }
        private static void Line(Vector2 a, Vector2 b, float width, Color color)
        {
            var matrix = GUI.matrix; var delta = b - a;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, a);
            GUI.color = color; GUI.DrawTexture(new Rect(a.x, a.y - width * .5f, delta.magnitude, width), Texture2D.whiteTexture);
            GUI.matrix = matrix;
        }
        private void Release()
        {
            if (bound && game != null)
            {
                game.RunDriver.Run.Reset -= ClearPresentation;
                game.RunDriver.Run.Rewound -= ClearPresentation;
                game.Enemies.ShotsCleared -= ClearPresentation;
            }
            bound = false; ClearPresentation();
        }
        private void OnDisable() => Release();
        private void OnDestroy() => Release();
    }
}
