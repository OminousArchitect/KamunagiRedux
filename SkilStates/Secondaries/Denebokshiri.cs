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
using R2API.ContentManagement;
using Rewired.UI.ControlMapper;
using UnityEngine.AddressableAssets;
using Console = System.Console;

namespace Kamunagi
{
    class DenebokshiriBrimstone : BaseTwinState
    {
        private float remapMin = 1f;
        private float remapMax = 2f;
        private float maxChargeTime = 2f;
        private float damageCoefficient;
        private Transform muzzleTransform;
        private GameObject chargeEffectInstance;
        private string effectMuzzleString = "MuzzleCenter";
        private int onRails;
        private uint soundID;

        public override void OnEnter()
        {
            base.OnEnter();
            var whatTheFuck = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
            maxChargeTime *= base.attackSpeedStat;
            muzzleTransform = base.FindModelChild(effectMuzzleString);
            if (muzzleTransform)
            {
                chargeEffectInstance = UnityEngine.Object.Instantiate(Prefabs.miniSunChargeEffect, muzzleTransform.position, muzzleTransform.rotation);
                chargeEffectInstance.transform.parent = muzzleTransform;
                ObjectScaleCurve scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if (scale)
                {
                    scale.baseScale = Vector3.one * 0.35f;
                    scale.timeMax = maxChargeTime;
                }
            }

            soundID = AkSoundEngine.PostEvent("Play_fireballsOnHit_pool_aliveLoop", base.gameObject);
        }

        void FireProjectile()
        {
            var zapDamage = Prefabs.miniSunProjectile.GetComponent<ProjectileProximityBeamController>();
            zapDamage.damageCoefficient = damageCoefficient;
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = damageCoefficient * 100,
                    owner = base.gameObject,
                    position = aimRay.origin,  //aimRay.origin + aimRay.direction * 2,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = Prefabs.miniSunProjectile,
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
            damageCoefficient = Util.Remap(base.fixedAge, 0, maxChargeTime, remapMin, remapMax);

            if (base.isAuthority && base.fixedAge > maxChargeTime && inputBank.skill2.down)
            {
                FireProjectile();
                this.outer.SetNextStateToMain();
            }

            if (base.isAuthority && base.fixedAge < maxChargeTime && !inputBank.skill2.down)
            {
                FireProjectile();
                outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }
            AkSoundEngine.StopPlayingID(soundID);
            
            //var baseD = base.damageStat;
            //Debug.Log($"{fixedAge} fixedAge");
            //Debug.Log($"{damageCoefficient} remap");
            //Debug.Log($" is {damageCoefficient} * {baseD} = {damageCoefficient * baseD}"); //example of a dynamic log output
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
