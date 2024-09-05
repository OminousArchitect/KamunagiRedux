using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
    public class MikazuchiState : BaseTwinState
    {
        public Transform? muzzleTransform;
        public EffectManagerHelper? chargeEffectInstance;
        public GameObject? indicator;
        public float projectileCount = 3f;

        public override void OnEnter()
        {
            base.OnEnter();
            muzzleTransform = FindModelChild("MuzzleCenter");
            if (!muzzleTransform || !Asset.TryGetGameObject<Mikazuchi, IEffect>(out var muzzleEffect)) return;
            chargeEffectInstance = EffectManager.GetAndActivatePooledEffect(muzzleEffect, muzzleTransform, true);
            if (!isAuthority) return;
            indicator = Object.Instantiate(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
            indicator.transform.localScale = Vector3.one * 3f;
        }

        public override void Update()
        {
            base.Update();
            if (indicator == null || !indicator) return;
            if (inputBank.GetAimRaycast(float.PositiveInfinity, out var hit))
                indicator.transform.position = hit.point;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeEffectInstance != null)
                chargeEffectInstance.ReturnToPool();
            if (isAuthority && indicator != null && indicator)
            {
                var xoro = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
                var spacingDegrees = 360f / projectileCount;
                var pointOffset = Vector3.ProjectOnPlane(inputBank.aimDirection, Vector3.up);
                var centerPoint = indicator.transform.position + Vector3.up * 2.5f;
                for (var i = 0; i < projectileCount; i++)
                {
                    ProjectileManager.instance.FireProjectile(Asset.GetGameObject<Mikazuchi, IProjectile>(),
                        centerPoint,
                        Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(spacingDegrees * i, Vector3.up) *
                                                        pointOffset),
                        gameObject,
                        damageStat,
                        10f,
                        RollCrit(),
                        speedOverride: xoro.RangeInt(13, 28)
                    );
                }
            }

            if (indicator) Destroy(indicator);
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }

    public class Mikazuchi : Asset, IEffect, IProjectile, IProjectileGhost
    {
    }
}