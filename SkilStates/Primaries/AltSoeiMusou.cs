using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using Kamunagi;
using Kamunagi.Modules;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;

namespace Kamunagi
{
    class AltSoeiMusou : BaseTwinState
    {
        public override int meterGain => 0;
        private float stopwatch;
        private float maxChargeTime = 3f;
        private float projectileFireFrequency = 0.4f;
        private float barrageProjectileDamage = 1.2f;
        private float ballDamageCoefficient = 6f;
        private Transform muzzleTransform;
        private GameObject chargeEffectInstance;
        private GameObject fullChargeEffectInstance;
        private string effectMuzzleString = "MuzzleCenter";
        private bool charged;

        public override void OnEnter()
        {
            base.OnEnter();
            maxChargeTime *= base.attackSpeedStat;
            muzzleTransform = base.FindModelChild(effectMuzzleString);
            if (muzzleTransform)
            {
                chargeEffectInstance = UnityEngine.Object.Instantiate(Prefabs.altMusouMuzzle, muzzleTransform.position, muzzleTransform.rotation);
                chargeEffectInstance.transform.parent = muzzleTransform;
                ObjectScaleCurve scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if (scale)
                {
                    scale.baseScale = Vector3.one * 0.7f;
                    scale.timeMax = projectileFireFrequency;
                }
            }
        }
        void FireProjectiles()
        {
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * barrageProjectileDamage,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 120,
                    owner = base.gameObject,
                    position = childLocator.FindChild("MuzzleCenter").transform.position,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = Prefabs.altMusouProjectile,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useFuseOverride = false,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
        void FireBall()
        {
            if (base.isAuthority)
            {
                /*var ballExplosion = Prefabs.altMusouChargeballProjectile.GetComponent<ProjectileImpactExplosion>();
                ballExplosion.blastDamageCoefficient = ballDamageCoefficient;*/
                
                Ray aimRay = base.GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * ballDamageCoefficient,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = ballDamageCoefficient * 100,
                    owner = base.gameObject,
                    position = childLocator.FindChild("MuzzleCenter").transform.position,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = base.fixedAge < maxChargeTime ? Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterSmallProjectile.prefab") : Prefabs.altMusouChargeballProjectile,
                    //if fixedAge is less than maxChargeTime, then fizzle
                    //else don't and fire the ball
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useFuseOverride = false,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
        
        public override void FixedUpdate() 
        {
            base.FixedUpdate();
            //base.StartAimMode();

            if (base.isAuthority && !base.inputBank.skill1.down)
            {
                base.outer.SetNextStateToMain();
                return;
            }
            if (base.fixedAge >= maxChargeTime)
            {
                charged = true;
                if (!fullChargeEffectInstance && chargeEffectInstance)
                {
                    Destroy(chargeEffectInstance);
                    fullChargeEffectInstance = UnityEngine.Object.Instantiate(Prefabs.altMusouMuzzle, muzzleTransform.position, muzzleTransform.rotation);
                    fullChargeEffectInstance.transform.parent = muzzleTransform;
                    fullChargeEffectInstance.GetComponent<ObjectScaleCurve>().baseScale = Vector3.one;
                }
            }
            stopwatch += Time.deltaTime;
            if (stopwatch >= projectileFireFrequency)
            {
                //0.2 frequency is equal to 5 times per second
                //0.1 would be 10 times per second
                stopwatch = 0;
                FireProjectiles();
            }
        }
        public override void OnExit()
        {
            StartAimMode();
            if (fullChargeEffectInstance)
            {
                Destroy(fullChargeEffectInstance); 
            }

            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }

            if (charged)
            {
                FireBall();
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
