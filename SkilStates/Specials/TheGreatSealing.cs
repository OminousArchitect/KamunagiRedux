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
    class TheGreatSealing : RaycastedSpell
    {
        public override int meterGain => 0;
        
        public override float cooldownReduction => 1f;
        public override string projectilePrefabPath => null;
        public override GameObject projectilePrefab => Prefabs.primedObelisk;
        public override float blastDamageCoefficient => 1f;
        public override bool useIndicator => true;
        public override float duration => 10f;
        public override float blastRadius => 4f; //11.25

        private readonly string effectMuzzleString = "MuzzleCenter";
        private Transform muzzleTransform;
        private GameObject chargeEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            muzzleTransform = base.FindModelChild(effectMuzzleString);
            characterMotor.useGravity = false;
            if (muzzleTransform)
            {
                chargeEffectInstance = UnityEngine.Object.Instantiate(Prefabs.antimatterVoidballEffect, muzzleTransform.position, muzzleTransform.rotation);
                chargeEffectInstance.transform.parent = muzzleTransform;
                ObjectScaleCurve scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if (scale)
                {
                    scale.baseScale = Vector3.one * 2;
                    scale.timeMax = 2f;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            base.characterMotor.velocity = Vector3.zero;
        }

        public override bool ButtonDown()
        {
            return base.inputBank.skill4.down;
        }

        public override void OnExit()
        {
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }
            characterMotor.useGravity = true;
            base.OnExit();
        }

        protected override void FireAttack()
        {
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab ? projectilePrefab : Prefabs.Load<GameObject>(projectilePrefabPath),
                position = raycastHitPoint,
                rotation = Quaternion.identity,
                procChainMask = default(ProcChainMask),
                target = null,
                owner = base.gameObject,
                damage = blastDamageCoefficient,
                crit = base.RollCrit(),
                force = 1f,
                damageTypeOverride = DamageType.Generic,
                damageColorIndex = DamageColorIndex.Default,
                speedOverride = 0,
                useSpeedOverride = false
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }
    }
}
