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
    class WohsisZone : RaycastedSpell
    {
        public override float cooldownReduction => 0f;
        public override float projectileDamageCoefficient => 3.1f;
        public override GameObject projectilePrefab => Prefabs.reaverMusouProjectile;
        public override string projectilePrefabPath => null;
        public override bool useIndicator => true;
        public override float blastRadius => 5f;
        public override DamageType GetDamageType()
        {
            return DamageType.Nullify;
        }
        public override bool ButtonDown()
        {
            return base.inputBank.skill3.down;
        }

        protected override void FireAttack()
        {
            if (twinBehaviour.activeBuffWard)
            {
                NetworkServer.Destroy(twinBehaviour.activeBuffWard);
            }
            var ward = UnityEngine.Object.Instantiate(Prefabs.woshisWard, areaIndicator.transform.position, Quaternion.identity);
            UnityEngine.Object.Destroy(ward.GetComponent<NetworkedBodyAttachment>());
            ward.GetComponent<TeamFilter>().teamIndex = TeamIndex.Monster;
            twinBehaviour.activeBuffWard = ward.gameObject;
            NetworkServer.Spawn(ward);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
