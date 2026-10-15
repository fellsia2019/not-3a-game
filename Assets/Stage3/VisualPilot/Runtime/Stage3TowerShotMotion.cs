using Not3A.Stage2.WavesAI;
using Not3A.Stage2.Defense;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Articulated catapult pose. Rapid and both tower foundations stay still.</summary>
    public sealed class Stage3TowerShotMotion : MonoBehaviour
    {
        private static readonly Vector3 AxleAxis = new Vector3(.913545f,.406737f,0f);
        private Transform arm, loadedStone, aim;
        private Quaternion restingArm, restingAim, aimStart, aimGoal;
        private Vector3 aimForwardLocal, stoneCenter;
        private Stage2Shot shot;
        private Stage2Tower tower;
        private Stage3TowerSprite sprite;
        private float age = 2f;
        public float ReleaseSeconds { get; private set; }
        public Vector3 ReleasePosition { get; private set; }

        public void Initialize()
        {
            tower=GetComponent<Stage2Tower>();
            sprite=GetComponentInChildren<Stage3TowerSprite>();
            if (sprite != null) return;
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "CatapultArm") arm = child;
                if (child.name == "LoadedStone") loadedStone = child;
                if (child.name == "CatapultAim") aim = child;
            }
            if (arm != null) restingArm = arm.localRotation;
            if (loadedStone != null) stoneCenter = loadedStone.GetComponent<MeshFilter>().sharedMesh.bounds.center;
            if (aim != null)
            {
                // Explicit world-aligned vertical pivot through the occupied cell's center.
                // Keep FBX conversion transforms below it; only the complete inner mechanism moves.
                var importedMechanism=aim;
                aim=new GameObject("Catapult centered yaw").transform;
                aim.SetParent(transform.Find("Pilot ground-aligned mesh"),false);
                aim.SetPositionAndRotation(transform.position,Quaternion.identity);
                importedMechanism.SetParent(aim,true);
                restingAim = aim.localRotation;
                aimStart = aimGoal = aim.rotation;
                // Derive forward from the actual cup and axle after import, not Blender axis guesses.
                var forward=arm.position-loadedStone.TransformPoint(stoneCenter);
                forward.z=0f;
                aimForwardLocal=aim.InverseTransformDirection(forward.normalized);
            }
        }

        public void TrackTarget(Stage2EnemyGroup enemies)
        {
            if (sprite != null)
            {
                if (tower == null || !tower.IsAlive || (shot != null && age < 1.05f)) return;
                var spriteTarget = enemies.FindNearest(transform.position,tower.AttackRange);
                if (spriteTarget != null) sprite.Face(spriteTarget.transform.position);
                return;
            }
            if (aim == null || tower == null || !tower.IsAlive || (shot != null && age < 1.05f)) return;
            var target=enemies.FindNearest(transform.position,tower.AttackRange);
            if (target != null) aimGoal=Facing(target.transform.position);
        }

        private Quaternion Facing(Vector2 target)
        {
            var forward=aim.TransformDirection(aimForwardLocal); forward.z=0f;
            var direction=(Vector3)target-transform.position;
            return Quaternion.AngleAxis(Vector3.SignedAngle(forward,direction,Vector3.back),Vector3.back)*aim.rotation;
        }

        public Vector3 Fire(Stage2Shot value)
        {
            shot=value; age=0f;
            ReleaseSeconds=0f;
            ReleasePosition=transform.position+Vector3.back*1.1f;
            if (sprite != null)
            {
                sprite.Face(value.Position);
                sprite.SetPose(0);
                ReleaseSeconds=sprite.IsCatapult ? Mathf.Min(.22f,value.Duration*.45f) : 0f;
                if(sprite.IsLayered) sprite.SetThrow(0f,ReleaseSeconds);
                ReleasePosition=sprite.IsLayered ? sprite.SampleRelease(ReleaseSeconds) : sprite.ReleasePosition;
                return ReleasePosition;
            }
            if (value.SplashRadiusCells <= 0f || aim == null || arm == null || loadedStone == null)
                return ReleasePosition;

            // Aim from the current bearing, without resetting or snapping the mechanism.
            aimStart=aim.rotation;
            aimGoal=Facing(value.Position);
            ReleaseSeconds=Mathf.Min(.22f,value.Duration*.45f);

            // Sample the cup at release, then restore the visible start pose. The flight
            // starts at this same socket even if a frame steps across the release time.
            var initialArm=arm.localRotation;
            aim.rotation=aimGoal;
            arm.localRotation=ArmPose(ReleaseSeconds);
            ReleasePosition=loadedStone.TransformPoint(stoneCenter);
            aim.rotation=aimStart;
            arm.localRotation=initialArm;
            loadedStone.gameObject.SetActive(true);
            return ReleasePosition;
        }

        private Quaternion ArmPose(float seconds)
        {
            float angle=seconds < .1f ? Mathf.Lerp(0f,-32f,Mathf.SmoothStep(0f,1f,seconds/.1f)) :
                seconds < .29f ? Mathf.Lerp(-32f,48f,Mathf.SmoothStep(0f,1f,(seconds-.1f)/.19f)) :
                seconds < .42f ? 48f : Mathf.Lerp(48f,0f,Mathf.SmoothStep(0f,1f,(seconds-.42f)/.63f));
            return restingArm*Quaternion.AngleAxis(angle,AxleAxis);
        }

        public void Advance(float elapsed)
        {
            if (sprite != null)
            {
                if (shot == null || age >= 1.05f) return;
                age=shot.Progress < 1f ? shot.Elapsed : Mathf.Max(age+elapsed,shot.Elapsed);
                if(sprite.IsLayered) { sprite.SetThrow(age,ReleaseSeconds); return; }
                sprite.SetPose(age < ReleaseSeconds*.35f ? 0 : age < ReleaseSeconds ? 1 :
                    age < ReleaseSeconds+.20f ? 3 : age < .82f ? 4 : 0);
                return;
            }
            if (aim == null || arm == null) return;
            if (shot == null || age >= 1.05f)
            {
                aim.rotation=Quaternion.RotateTowards(aim.rotation,aimGoal,360f*elapsed);
                return;
            }
            age=shot.Progress < 1f ? shot.Elapsed : Mathf.Max(age+elapsed,shot.Elapsed);
            aim.rotation=Quaternion.Slerp(aimStart,aimGoal,Mathf.SmoothStep(0f,1f,age/.12f));
            arm.localRotation=ArmPose(age);
            if (loadedStone != null)
                loadedStone.gameObject.SetActive(age < ReleaseSeconds || age >= 1.05f);
        }

        public void ResetPose()
        {
            shot=null; age=2f;
            if (sprite != null) sprite.SetPose(0);
            if (sprite != null && sprite.IsLayered) sprite.SetThrow(2f,.22f);
            if (arm != null) arm.localRotation=restingArm;
            if (aim != null) aim.localRotation=restingAim;
            if (aim != null) aimStart=aimGoal=aim.rotation;
            if (loadedStone != null) loadedStone.gameObject.SetActive(true);
        }
    }
}
