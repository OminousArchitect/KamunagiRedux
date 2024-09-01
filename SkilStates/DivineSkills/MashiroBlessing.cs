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
using ExtraSkillSlots;
using Kamunagi;
using Kamunagi.Modules;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;

namespace Kamunagi
{
    class MashiroBlessing : BaseTwinState
    {
        public override int meterGain => 0;
        private float stopwatch;
        private float prayerTime = 2f;
        private float decreaseHealthInterval = 0.075f;
        private float barrageProjectileDamage = 1.2f;
        private float ballDamageCoefficient = 6f;
        private string effectMuzzleString = "MuzzleCenter";
        
        private bool prayer;
        private Transform muzzleTransform;
        private GameObject fxLeft;
        private GameObject fxRight;
        private int tally;
        private ExtraInputBankTest extraInput;

        public override void OnEnter()
        {
            base.OnEnter();
            fxLeft = UnityEngine.Object.Instantiate(Prefabs.hoverMuzzleFlames, FindModelChild("MuzzleLeft"));
            fxRight = UnityEngine.Object.Instantiate(Prefabs.hoverMuzzleFlames, FindModelChild("MuzzleRight"));
            extraInput = outer.GetComponent<ExtraInputBankTest>();
        }
        
        public override void FixedUpdate() 
        {
            base.FixedUpdate();
            //base.StartAimMode();

            if (base.isAuthority && !extraInput.extraSkill4.down)
            {
                base.outer.SetNextStateToMain();
            }
            if (base.fixedAge >= prayerTime)
            {
                prayer = true;
                outer.SetNextStateToMain();
            }
            
            stopwatch += Time.deltaTime;
            if (stopwatch >= decreaseHealthInterval)
            {
                //0.2 frequency is equal to 5 times per second
                //0.1 would be 10 times per second
                
                //0.075 is 24 times in 2 seconds
                stopwatch = 0;
                //tally++;
                //if (tally < 26)
                //{
                ChipHealth();
                //}
            }
        }
        
        void ChipHealth()
        {
            if (NetworkServer.active && (bool)base.healthComponent)
            {
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.damage = base.healthComponent.combinedHealth * 0.01f;
                damageInfo.position = base.characterBody.corePosition;
                damageInfo.force = Vector3.zero;
                damageInfo.damageColorIndex = Prefabs.MashiroPrayer;
                damageInfo.crit = false;
                damageInfo.attacker = null;
                damageInfo.inflictor = null;
                damageInfo.damageType = DamageType.BypassArmor;
                damageInfo.procCoefficient = 0f;
                damageInfo.procChainMask = default(ProcChainMask);
                base.healthComponent.TakeDamage(damageInfo);
            }
        }
        void Bless()
        {
            if (base.isAuthority)
            {
                
            }
        }
        
        public override void OnExit()
        {
            StartAimMode();

            if (fxRight)
            {
                Destroy(fxRight);
            }
            if (fxLeft)
            {
                Destroy(fxLeft);
            }

            if (prayer)
            {
                characterBody.AddTimedBuffAuthority(Buffs.MashiroBlessing.buffIndex, 10f);
            }
            base.OnExit();
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
