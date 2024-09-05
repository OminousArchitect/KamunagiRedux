using System;
using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using Rewired.UI.ControlMapper;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Object = UnityEngine.Object;

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
        }

        public override void Update()
        {
            base.Update();
            if (!isAuthority) return;
            if (!inputBank.GetAimRaycast(float.PositiveInfinity, out var hit)) return;
            if (indicator == null || !indicator)
            {
                indicator = Object.Instantiate(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
                indicator.transform.localScale = Vector3.one * 3f;
            }

            indicator.transform.position = hit.point;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority || (fixedAge < 5f && IsKeyDownAuthority())) return;
            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeEffectInstance != null)
                chargeEffectInstance.ReturnToPool();
            if (!isAuthority) return;
            if (indicator != null && indicator)
            {
                var indicatorPosition = indicator.transform.position;
                var blastAttack = new BlastAttack
                {
                    attacker = gameObject,
                    baseDamage = damageStat * 8.5f,
                    baseForce = 1800,
                    crit = RollCrit(),
                    damageType = DamageType.Shock5s,
                    falloffModel = BlastAttack.FalloffModel.None,
                    procCoefficient = 1,
                    radius = 3f,
                    position = indicatorPosition,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    teamIndex = teamComponent.teamIndex
                };
                blastAttack.Fire();
                if (Asset.TryGetGameObject<MikazuchiStrike, IEffect>(out var effect))
                    EffectManager.SpawnEffect(effect, new EffectData()
                    {
                        origin = indicatorPosition,
                        scale = blastAttack.radius
                    }, true);

                var xoro = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
                var spacingDegrees = 360f / projectileCount;
                var forward = Vector3.ProjectOnPlane(inputBank.aimDirection, Vector3.up);
                var centerPoint = indicatorPosition + Vector3.up * 2.5f;
                for (var i = 0; i < projectileCount; i++)
                {
                    ProjectileManager.instance.FireProjectile(Asset.GetGameObject<Mikazuchi, IProjectile>(),
                        centerPoint,
                        Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(spacingDegrees * i, Vector3.up) *
                                                        forward),
                        gameObject,
                        damageStat,
                        10f,
                        RollCrit(),
                        speedOverride: xoro.RangeInt(13, 28)
                    );
                }

                Destroy(indicator);
            }
            else
            {
                activatorSkillSlot.rechargeStopwatch = activatorSkillSlot.finalRechargeInterval - 2f;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }

    public class Mikazuchi : Asset, IEffect, IProjectile, IProjectileGhost
    {
    }
}