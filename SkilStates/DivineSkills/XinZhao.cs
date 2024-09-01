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
    class XinZhao : BaseTwinState
    {
        private float duration;
        private float baseDuration = 0.35f;
        private float damageCoefficient = 4;
        private float knockbackCoefficient = 55;
        private float radius = 30;
        public static float forcefieldDuration = 6;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / base.attackSpeedStat;
            if (NetworkServer.active)
            {
                foreach (HurtBox hurtBox in
                new SphereSearch()
                {
                    origin = base.characterBody.corePosition,
                    radius = radius,
                    mask = LayerIndex.entityPrecise.mask
                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex)).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes())
                {
                    if (hurtBox.healthComponent != base.healthComponent && hurtBox.healthComponent.body)
                    {
                        float mass = 0;
                        var rigidMotor = hurtBox.healthComponent.body.GetComponent<RigidbodyMotor>();
                        if (hurtBox.healthComponent.body.characterMotor)
                        {
                            mass = hurtBox.healthComponent.body.characterMotor.mass;
                        }
                        else if (rigidMotor)
                        {
                            mass = rigidMotor.mass;
                        }
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = damageCoefficient;
                        damageInfo.force = (hurtBox.healthComponent.body.footPosition - base.characterBody.footPosition).normalized * mass * knockbackCoefficient;
                        damageInfo.canRejectForce = false;
                        damageInfo.position = hurtBox.transform.position;
                        damageInfo.procChainMask = default(ProcChainMask);
                        damageInfo.inflictor = base.gameObject;
                        damageInfo.canRejectForce = base.gameObject;
                        damageInfo.crit = base.RollCrit();
                        hurtBox.healthComponent.TakeDamage(damageInfo);
                    }
                };
                NetworkServer.Spawn(UnityEngine.Object.Instantiate(Prefabs.forceField, base.transform.position, Quaternion.identity));
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
