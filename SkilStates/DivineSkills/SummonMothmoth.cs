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
    class SummonMothmoth : BaseTwinState
    {
        private float duration = 0.55f;
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(EntityStates.BeetleQueenMonster.SpawnWards.attackSoundString, base.gameObject);
            if (NetworkServer.active)
            {
                var ward = UnityEngine.Object.Instantiate(Prefabs.mothMoth, base.characterBody.corePosition, Quaternion.identity);
                ward.GetComponent<TeamComponent>().teamIndex = base.teamComponent.teamIndex;
                ward.GetComponent<TeamFilter>().teamIndex = base.teamComponent.teamIndex;
                NetworkServer.Spawn(ward);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
