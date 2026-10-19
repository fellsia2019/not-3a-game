using System.Collections.Generic;
using Not3A.Stage2.Defense;
using Not3A.Stage3.VisualPilot;
using UnityEngine;
using UnityEngine.Rendering;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Local, noninteractive Blink artwork and XP feedback. Rules own all target checks.</summary>
    public sealed class Stage3HeroProgressionEffects : MonoBehaviour
    {
        private Camera view;
        private Transform hero;
        private Stage3FantasyHud hud;
        private Stage3HeroAvatar avatar;
        private GameObject visualRoot;
        private Material groundMaterial, spriteMaterial;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private readonly List<Vector3> vertices=new List<Vector3>(2048);
        private readonly List<Color> colors=new List<Color>(2048);
        private readonly List<int> indices=new List<int>(3072);
        private readonly SpriteRenderer[] ghosts=new SpriteRenderer[6];
        private bool aiming,valid;
        private Vector2 landing,departure,arrival;
        private float range,blinkTime,xpTime;
        private int xpAmount;
        private string xpLabel;
        private static readonly Color Cyan=new Color(.36f,.91f,1f,.85f);
        private static readonly Color Red=new Color(1f,.18f,.22f,.9f);
        public bool IsShowingBlinkAim => aiming;
        public bool BlinkAimValid => aiming&&valid;
        public Vector2 BlinkAimPosition => landing;
        public bool IsShowingBlinkTransition => blinkTime>0;
        public bool IsShowingExperience => xpTime>0;

        public void Initialize(Camera camera,Transform player,Stage3FantasyHud presentation)
        {
            view=camera;hero=player;hud=presentation;avatar=hero!=null?hero.GetComponent<Stage3HeroAvatar>():null;
            if(visualRoot!=null)return;
            visualRoot=new GameObject("Hero progression local effects");visualRoot.transform.SetParent(transform,false);
            groundMaterial=new Material(Resources.Load<Shader>("Stage3HeroBlinkMarks")){name="Local Blink target overlay",hideFlags=HideFlags.HideAndDontSave};
            spriteMaterial=Stage2PlacementVisuals.Material(true);
            mesh=new Mesh{name="Blink rings and local wisps"};mesh.MarkDynamic();
            visualRoot.AddComponent<MeshFilter>().sharedMesh=mesh;
            meshRenderer=visualRoot.AddComponent<MeshRenderer>();meshRenderer.sharedMaterial=groundMaterial;
            meshRenderer.shadowCastingMode=ShadowCastingMode.Off;meshRenderer.receiveShadows=false;meshRenderer.sortingOrder=30010;
            for(var i=0;i<ghosts.Length;i++)
            {
                var part=new GameObject("Blink silhouette "+i);part.transform.SetParent(visualRoot.transform,false);
                ghosts[i]=part.AddComponent<SpriteRenderer>();ghosts[i].sharedMaterial=spriteMaterial;
                ghosts[i].shadowCastingMode=ShadowCastingMode.Off;ghosts[i].receiveShadows=false;ghosts[i].enabled=false;
                ghosts[i].sortingOrder=30011+i;
            }
        }

        public void SetBlinkAim(bool visible,Vector2 target,float worldRange,bool isValid)
        {aiming=visible;landing=target;range=worldRange;valid=isValid;if(!visible)HideGhostPair(0);}

        public void PlayBlink(Vector2 from,Vector2 to)
        {
            departure=from;arrival=to;blinkTime=.65f;aiming=false;HideGhostPair(0);
            CopyGhostPair(2,from,.4f);CopyGhostPair(4,to,.25f);
        }

        public void ShowExperience(int amount)
        {
            if(amount<=0)return;
            xpAmount=xpTime>0?xpAmount+amount:amount;xpTime=1.25f;
            xpLabel="+"+xpAmount+" XP";hud?.ShowExperienceFeedback(xpLabel,1.25f);
        }

        public void ClearEffects()
        {
            aiming=false;blinkTime=0;xpTime=0;xpAmount=0;hud?.ClearExperienceFeedback();
            foreach(var ghost in ghosts)if(ghost!=null)ghost.enabled=false;
            if(mesh!=null)mesh.Clear();if(meshRenderer!=null)meshRenderer.enabled=false;
        }

        private void LateUpdate()
        {
            if(visualRoot==null||view==null||hero==null||hud==null)return;
            blinkTime=Mathf.Max(0,blinkTime-Time.deltaTime);xpTime=Mathf.Max(0,xpTime-Time.deltaTime);
            if(xpTime==0)xpAmount=0;
            var visible=!hud.IsModalOpen&&hud.isActiveAndEnabled;
            visualRoot.SetActive(visible);
            if(!visible)return;
            vertices.Clear();colors.Clear();indices.Clear();
            if(aiming)
            {
                Ring(landing,.40f,valid?Cyan:Red,!valid,PixelWidth(2.1f));
                if(range>0)Ring(hero.position,range,new Color(.30f,.88f,1f,.72f),true,PixelWidth(1.4f),96);
                if(valid)CopyGhostPair(0,landing,.48f);
                else
                {
                    HideGhostPair(0);
                    Segment(Ground(landing+new Vector2(-.20f,-.20f)),Ground(landing+new Vector2(.20f,.20f)),PixelWidth(3f),Red);
                    Segment(Ground(landing+new Vector2(-.20f,.20f)),Ground(landing+new Vector2(.20f,-.20f)),PixelWidth(3f),Red);
                }
            }
            else HideGhostPair(0);
            if(blinkTime>0)
            {
                var t=1f-blinkTime/.65f;var alpha=1f-t;
                TintGhostPair(2,alpha*.4f);TintGhostPair(4,alpha*.22f);
                BlinkWisps(departure,t,alpha);BlinkWisps(arrival,t*.85f,alpha);
            }
            else {HideGhostPair(2);HideGhostPair(4);}
            meshRenderer.enabled=vertices.Count>0;
            if(vertices.Count==0&&mesh.vertexCount==0)return;
            mesh.Clear();
            if(vertices.Count>0){mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();}
        }

        private void BlinkWisps(Vector2 position,float progress,float alpha)
        {
            var tint=new Color(.63f,.95f,1f,alpha*.8f);
            Ring(position,.32f+progress*.20f,tint,false,.022f);
            // Two rising ribbons remain local to the figure, never an attack wave.
            for(var ribbon=0;ribbon<2;ribbon++)
                for(var i=0;i<17;i++)
                {
                    var a=i/17f;var b=(i+1)/17f;
                    Vector3 Point(float t)
                    {
                        var angle=t*Mathf.PI*2+progress*4+ribbon*Mathf.PI;
                        return Ground(position)+new Vector3(Mathf.Cos(angle)*.33f,Mathf.Sin(angle)*.18f,-(.10f+t*.95f));
                    }
                    Segment(Point(a),Point(b),.018f,tint);
                }
            for(var i=0;i<7;i++)
            {
                var angle=i*2.39996f;var center=Ground(position)+new Vector3(Mathf.Cos(angle)*.43f,Mathf.Sin(angle)*.30f,-(.20f+((i*.17f+progress)%1f)*1.1f));
                var radius=.025f*(1f-progress*.6f);
                Segment(center-view.transform.up*radius,center+view.transform.up*radius,.012f,tint);
                Segment(center-view.transform.right*radius*.65f,center+view.transform.right*radius*.65f,.012f,tint);
            }
        }

        private static Vector3 Ground(Vector2 point)=>new Vector3(point.x,point.y,-.12f);
        private void Ring(Vector2 center,float radius,Color color,bool dashed,float width,int steps=48)
        {
            for(var i=0;i<steps;i++)
            {
                if(dashed&&i%4>=2)continue;
                var a=i*Mathf.PI*2/steps;var b=(i+1)*Mathf.PI*2/steps;
                Segment(Ground(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius),Ground(center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius),width,color);
            }
        }
        private float PixelWidth(float pixels)=>view.orthographicSize*2f/Mathf.Max(1,view.pixelHeight)*pixels*Mathf.Max(1f,hud.UiScale);
        private void Segment(Vector3 a,Vector3 b,float width,Color color)
        {
            // One mesh draw; a faint wide stroke separates the bright core from painted terrain.
            Quad(a,b,width+PixelWidth(3f),new Color(color.r,color.g,color.b,color.a*.14f));
            Quad(a,b,width,color);
        }
        private void Quad(Vector3 a,Vector3 b,float width,Color color)
        {
            var normal=Vector3.Cross(b-a,view.transform.forward).normalized*width*.5f;var n=vertices.Count;
            vertices.Add(a-normal);vertices.Add(a+normal);vertices.Add(b+normal);vertices.Add(b-normal);
            for(var i=0;i<4;i++)colors.Add(color);
            indices.Add(n);indices.Add(n+1);indices.Add(n+2);indices.Add(n);indices.Add(n+2);indices.Add(n+3);
        }
        private void CopyGhostPair(int index,Vector2 point,float alpha)
        {
            if(avatar==null)return;
            CopyGhost(ghosts[index],avatar.Body,point,alpha);
            CopyGhost(ghosts[index+1],avatar.Equipment,point,alpha);
        }
        private void CopyGhost(SpriteRenderer ghost,SpriteRenderer source,Vector2 point,float alpha)
        {
            if(ghost==null)return;
            ghost.enabled=source!=null&&source.enabled&&source.sprite!=null;
            if(!ghost.enabled)return;
            ghost.sprite=source.sprite;ghost.flipX=source.flipX;ghost.flipY=source.flipY;
            ghost.transform.SetPositionAndRotation(source.transform.position+(Vector3)point-hero.position,source.transform.rotation);
            ghost.transform.localScale=source.transform.lossyScale;ghost.color=new Color(.50f,.95f,1f,alpha);
        }
        private void TintGhostPair(int index,float alpha)
        {for(var i=index;i<index+2;i++)if(ghosts[i]!=null)ghosts[i].color=new Color(.50f,.95f,1f,alpha);}
        private void HideGhostPair(int index)
        {for(var i=index;i<index+2;i++)if(ghosts[i]!=null)ghosts[i].enabled=false;}

        private void OnDisable()=>ClearEffects();
        private void OnDestroy()
        {
            if(visualRoot!=null)Destroy(visualRoot);if(mesh!=null)Destroy(mesh);
            if(groundMaterial!=null)Destroy(groundMaterial);if(spriteMaterial!=null)Destroy(spriteMaterial);
        }
    }
}
