using UnityEngine;
using Not3A.Stage2.Integration;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Only depicts confirmed actor state; root position, damage and lifetime belong to the guild driver.</summary>
    public sealed class Stage3GuildEnemyVisual : MonoBehaviour
    {
        private Stage3GuildEnemyArt art;
        private SpriteRenderer body;
        private Camera view;
        private Stage2GameComposition composition;
        private Stage2GameHud legacyHud;
        private Stage3GuildWorldDriver worldDriver;
        private int kind;
        private Vector2 velocity,facing=Vector2.down;
        private bool attacking,dead;
        private float stride,deathSeconds;
        public SpriteRenderer Body=>body;
        public void Initialize(int enemyKind,Camera camera)
        {
            kind=enemyKind;view=camera;art=Resources.Load<Stage3GuildEnemyArt>("Stage3GuildEnemyArt");
            composition=GetComponentInParent<Stage2GameComposition>();
            worldDriver=GetComponentInParent<Stage3GuildWorldDriver>();
            legacyHud=composition!=null?composition.GetComponent<Stage2GameHud>():null;
            if(body==null){var child=new GameObject("Exploration enemy sprite");child.transform.SetParent(transform,false);body=child.AddComponent<SpriteRenderer>();}
            body.spriteSortPoint=SpriteSortPoint.Pivot;
            if(art!=null)body.sharedMaterial=art.material;
            Apply();
        }
        public void SetState(Vector2 movement,Vector2 look,bool isAttacking,bool isDead)
        {
            velocity=movement;if(look.sqrMagnitude>.0001f)facing=look;
            attacking=isAttacking;if(isDead&&!dead)deathSeconds=0;dead=isDead;Apply();
        }
        private void LateUpdate()
        {
            if(worldDriver!=null&&!worldDriver.AutomaticTick||composition?.RunDriver?.Run!=null&&
                (composition.RunDriver.Run.IsPaused||composition.RunDriver.Run.IsTerminal)||legacyHud!=null&&legacyHud.DebugOpen)return;
            if(dead)deathSeconds+=Time.deltaTime;
            else if(velocity.sqrMagnitude>.001f)stride=Mathf.Repeat(stride+velocity.magnitude*Time.deltaTime/.85f,1f);
            Apply();
        }
        private void Apply()
        {
            if(body==null||art==null||view==null)return;
            var rear=Vector3.Dot((Vector3)facing,view.transform.up)>.05f;
            var pose=dead?0:attacking?3:velocity.sqrMagnitude>.001f?(stride<.5f?1:2):0;
            var sprite=art.Frame(kind,(rear?4:0)+pose);if(sprite==null){body.enabled=false;return;}
            body.enabled=true;body.sprite=sprite;
            body.flipX=Vector3.Dot((Vector3)facing,view.transform.right)<0;
            body.color=new Color(1,1,1,dead?1-Mathf.Clamp01(deathSeconds/.4f):1);
            var world=transform.position;world.z=Stage3TacticalPresentation.GrassSurfaceZ(world);
            var padding=sprite.pivot.y/sprite.pixelsPerUnit;
            var clearance=padding*Mathf.Abs(view.transform.up.z)/Mathf.Max(.1f,Mathf.Abs(view.transform.forward.z))+.03f;
            body.transform.SetPositionAndRotation(world-view.transform.forward*clearance,
                view.transform.rotation*Quaternion.Euler(0,0,dead?-Mathf.Min(70,deathSeconds*220):0));
            body.sortingOrder=-Mathf.RoundToInt(world.y*100)+1;
        }
    }
}
