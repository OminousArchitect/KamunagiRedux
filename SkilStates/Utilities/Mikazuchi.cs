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
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using Rewired.ComponentControls.Effects;

namespace Kamunagi
{
    class Mikazuchi : RaycastedSpell
    {
        public override float cooldownReduction => 5f;
        public override float duration => 5f;
        public override int randomChildProjectiles => 2;
        public override float blastRadius => 3f;
        public override GameObject blastEffectPrefab => Prefabs.MikazuchiLightningStrike;
        public override GameObject projectilePrefab => Prefabs.MikazuchiLightningOrb;
        public override bool useIndicator => true;
        public override float blastDamageCoefficient => 8.5f;
        
        private float childProjectileCount = 3f;

        public override bool ButtonDown()
        {
            return base.inputBank.skill3.down;
        }
        public override DamageType GetDamageType()
        {
            return DamageType.Shock5s;
        }

        private GameObject chargeEffectInstance;
        private Transform muzzleTransform;
        public override void OnEnter()
        {
            base.OnEnter();
            muzzleTransform = base.FindModelChild("MuzzleCenter");
            chargeEffectInstance = UnityEngine.Object.Instantiate(Prefabs.lightningMuzzle, muzzleTransform.position, muzzleTransform.rotation);
            chargeEffectInstance.transform.parent = muzzleTransform;
            ObjectScaleCurve scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
            if (scale)
            {
                scale.baseScale = Vector3.one;
                scale.timeMax = 1f;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }
        }

        protected override void FireAttack()
        {
            if (raycastHitPoint == Vector3.zero)
            {
                base.skillLocator.utility.rechargeStopwatch = cooldownReduction;
                return;
            }
            base.FireAttack();
            
            float num = 360f / childProjectileCount;
            Vector3 point = Vector3.ProjectOnPlane(base.inputBank.aimDirection, Vector3.up);
            Vector3 centerPoint = areaIndicator.transform.position + (Vector3.up * 2.5f);
            for (int i = 0; i < childProjectileCount; i++)
            {
                Vector3 forward = Quaternion.AngleAxis(num * i, Vector3.up) * point;
                var velocity = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);

                ProjectileManager.instance.FireProjectile(
                    Prefabs.MikazuchiLightningOrb,
                    centerPoint,
                    Util.QuaternionSafeLookRotation(forward),
                    base.gameObject,
                    base.characterBody.damage * 1f, //total damage = ProjectileDamage * blastDamageCoefficient
                    10f,
                    Util.CheckRoll(base.characterBody.crit, base.characterBody.master),
                    DamageColorIndex.Default,
                    null,
                    velocity.RangeInt(13, 28)
                );
            }
        }
    }
}
