using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage2.Integration
{
    /// <summary>Opt-in exploration damage, safe reincarnation and exclusive local-E recovery.</summary>
    [DisallowMultipleComponent,DefaultExecutionOrder(-200)]
    public sealed class Stage2HeroRecoveryDriver : MonoBehaviour
    {
        private Stage2GameComposition game;
        private Stage2PlayerMover mover;
        private Stage2HeroProgressionDriver progression;
        private Func<Vector2?> basePortal;
        private InputAction interact;
        private bool requiresRelease=true;
        private int claimedFrame=-1;
        public Stage2HeroRecovery Rules {get;private set;}
        public Func<bool> ControlBlocked {get;set;}
        public Func<float,float> IncomingDamagePreview {get;set;}
        public Action<float> IncomingDamageConfirmed {get;set;}
        public float InteractionRadius=>2f;
        public bool InteractionClaimedThisFrame=>claimedFrame==Time.frameCount;
        public bool Active=>isActiveAndEnabled&&Rules!=null&&game!=null&&!game.RunDriver.Run.IsPaused&&
            !game.RunDriver.Run.IsTerminal&&Time.timeScale>0;
        public Stage2HeroDrop NearbyDrop
        {
            get
            {
                if(Rules==null||mover==null)return null;
                Stage2HeroDrop nearest=null;var distance=InteractionRadius*InteractionRadius;
                foreach(var drop in Rules.Drops)
                {var d=((Vector2)mover.transform.position-drop.Position).sqrMagnitude;if(d<=distance){nearest=drop;distance=d;}}
                return nearest;
            }
        }
        public bool CanRecover=>Active&&!(ControlBlocked?.Invoke()??false)&&NearbyDrop!=null;
        public event Action Respawned;
        public void Configure(Stage2GameComposition owner,Stage2PlayerMover player,Stage2HeroProgressionDriver hero,
            Stage2Equipment equipment,Func<Vector2?> portal)
        {
            Unbind();game=owner;mover=player;progression=hero;basePortal=portal;
            Rules=new Stage2HeroRecovery(hero.Rules,equipment);game.HeroRecovery=this;
            game.RunDriver.Run.Reset+=ResetRules;game.RunDriver.Run.Rewound+=Latch;
            requiresRelease=true;
        }
        public bool TryApplyExplorationDamage(float amount)
        {
            if(!Active||!float.IsFinite(amount)||amount<=0)return false;
            var raw=amount;amount=IncomingDamagePreview?.Invoke(amount)??amount;
            if(!float.IsFinite(amount)||amount<0)return false;
            var lethal=amount>=progression.Rules.Health;
            var spawn=Vector2.zero;if(lethal&&!TryFindRespawn(out spawn))return false;
            IncomingDamageConfirmed?.Invoke(raw);
            if(amount==0)return true;
            var death=(Vector2)mover.transform.position;
            if(!Rules.TryApplyExplorationDamage(amount,death))return false;
            if(lethal)
            {
                mover.TryTeleportTo(spawn);
                game.Gathering.CancelGatherUntilRelease();Latch();Respawned?.Invoke();
            }
            return true;
        }
        public bool TryFindRespawn(out Vector2 target)
        {
            target=default;var anchor=basePortal?.Invoke();if(!anchor.HasValue)return false;
            target=anchor.Value;if(progression.CanLand(target))return true;
            for(var ring=1;ring<=24;ring++)for(var direction=0;direction<16;direction++)
            {
                var a=direction*Mathf.PI/8f;
                var candidate=anchor.Value+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(ring*.5f);
                if(!progression.CanLand(candidate))continue;target=candidate;return true;
            }
            return false;
        }
        public bool TryRecover(int id)
        {
            var drop=Rules?.Find(id);
            if(!CanRecover||drop==null||Vector2.Distance(mover.transform.position,drop.Position)>InteractionRadius)return false;
            claimedFrame=Time.frameCount;
            if(!Rules.TryRecover(id))return false;
            game.Gathering.CancelGatherUntilRelease();requiresRelease=true;return true;
        }
        private void Update()
        {
            var held=interact?.IsPressed()??false;if(!held)requiresRelease=false;
            if(!CanRecover){requiresRelease|=held;return;}
            if(!requiresRelease&&interact.WasPressedThisFrame())TryRecover(NearbyDrop.Id);
        }
        private void OnEnable()
        {interact=new InputAction("Recover dropped hero items",InputActionType.Button,"<Keyboard>/e");interact.Enable();requiresRelease=true;}
        private void OnDisable(){interact?.Dispose();interact=null;requiresRelease=true;}
        private void Latch()=>requiresRelease=true;
        private void ResetRules(){Rules.Reset();Latch();}
        private void Unbind()
        {
            if(game?.RunDriver?.Run!=null)
            {game.RunDriver.Run.Reset-=ResetRules;game.RunDriver.Run.Rewound-=Latch;if(game.HeroRecovery==this)game.HeroRecovery=null;}
        }
        private void OnDestroy()=>Unbind();
    }
}
