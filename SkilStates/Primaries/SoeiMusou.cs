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
    class ReaverMusou : RaycastedSpell
    {
        public override float cooldownReduction => 0f;
        public override int meterGain => 0;
        public override float projectileDamageCoefficient => 3.1f;
        public override GameObject projectilePrefab => Prefabs.reaverMusouProjectile;
        public override string projectilePrefabPath => null;
        public override DamageType GetDamageType()
        {
            return DamageType.Nullify;
        }
        public override bool ButtonDown()
        {
            return true;
        }

        protected override void FireAttack()
        {
            if (raycastHitPoint == Vector3.zero)
            {
                return;
            }
            if (base.isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * projectileDamageCoefficient,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 200,
                    owner = base.gameObject,
                    position = raycastHitPoint,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = projectilePrefab ? projectilePrefab : Prefabs.Load<GameObject>(projectilePrefabPath),
                    rotation = Quaternion.identity,
                    useFuseOverride = false,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            EffectManager.SimpleMuzzleFlash(Prefabs.reaverMuzzleFlash, base.gameObject, muzzleString, false);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
