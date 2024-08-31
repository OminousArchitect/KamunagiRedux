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
using HG;
using System.Runtime.InteropServices;
using Kamunagi;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using Rewired.ComponentControls.Effects;

namespace Kamunagi
{
    class EnnakamuyEarth : BaseTwinState
    {
        //public override int meterGain => 0;
        private float duration;
        private float baseDuration = 1.25f;
        private float minFireDelay;
        private float baseFireDelay = 0.4f;
        private bool hasFired;
        private float damageCoefficient = 6;
        private GameObject chargeEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode();
            duration = baseDuration / base.attackSpeedStat;
            minFireDelay = baseFireDelay / base.attackSpeedStat;

            var muzzleTransform = base.FindModelChild("MuzzleCenter"); //TODO this is how to do MuzzleCenter
            chargeEffectInstance = UnityEngine.Object.Instantiate<GameObject>(Prefabs.boulderChargeEffect, muzzleTransform.position + base.characterDirection.forward * 2, muzzleTransform.rotation, muzzleTransform);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= minFireDelay && !hasFired)
            {
                hasFired = true;
                Fire();
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        void Fire()
        {
            Destroy(chargeEffectInstance);
            Ray aimRay = base.GetAimRay();
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
                    position = aimRay.origin + aimRay.direction * 2,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = Prefabs.boulderProjectile,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useFuseOverride = false,
                    useSpeedOverride = true,
                    speedOverride = 115,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
