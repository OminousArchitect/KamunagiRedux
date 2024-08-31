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
using UnityEngine.AddressableAssets;

namespace Kamunagi
{
    class WindBoomerang : BaseTwinState
    {
        public override int meterGain => 5;
        private float damageCoefficient = 2.8f;
        private float distanceMult;
        private float maxChargeTime = 1.5f;
        private float minDistance = 0.05f;
        private float maxDistance = 0.6f;
        private GameObject projectilePrefab = Prefabs.windBoomerang;
        private Transform muzzleTransform;
        private string effectMuzzleString = "MuzzleCenter";
        private GameObject chargeEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            if (characterMotor.isGrounded)
            {
                base.StartAimMode();
            }
            muzzleTransform = base.FindModelChild(effectMuzzleString);
            if (muzzleTransform)
            {
                chargeEffectInstance = UnityEngine.Object.Instantiate(Prefabs.windBoomerangChargeEffect, muzzleTransform.position, muzzleTransform.rotation);
                chargeEffectInstance.transform.parent = muzzleTransform;
                ObjectScaleCurve scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if (scale)
                {
                    scale.baseScale = Vector3.one;
                    scale.timeMax = maxChargeTime;
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            distanceMult = Util.Remap(fixedAge, 0, maxChargeTime, minDistance, maxDistance);

            if (base.isAuthority && fixedAge >= maxChargeTime)
            {
                Fire();
                outer.SetNextStateToMain();
            }
            
            if (base.isAuthority && !inputBank.skill2.down)
            {
                Fire();
                outer.SetNextStateToMain();
            }
        }

        void Fire()
        {
            //AkSoundEngine.PostEvent("Play_voidman_m2_shoot", base.gameObject);
            //EffectManager.SimpleMuzzleFlash(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab"), base.gameObject, muzzleString, false);
            Ray aimRay = base.GetAimRay();
            projectilePrefab.GetComponent<WindBoomerangProjectile>().distanceMultiplier = distanceMult;
            
            if (base.isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * damageCoefficient,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 500,
                    owner = base.gameObject,
                    position = aimRay.origin,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = projectilePrefab,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useFuseOverride = false,
                    useSpeedOverride = true,
                    speedOverride = 50,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }
            //Debug.Log($"{fixedAge} fixedAge");
            //Debug.Log($"{distanceMult} distance");
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
