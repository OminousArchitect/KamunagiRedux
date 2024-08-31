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
    class HonokasVeil : BaseTwinState
    {
        public override int meterGain => 0;
        private float maxDuration = 99;
        private Vector3 position;
        private GameObject particles1;
        private GameObject particles2;
        private Transform mdl;
        private CharacterModel charModel;
        private HurtBoxGroup turnHurtboxesOff;

        public override void OnEnter()
        {
            base.OnEnter();
            twinBehaviour.chainsVfx1.SetActive(false);
            EffectManager.SpawnEffect(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflash.prefab"), new EffectData
            {
                origin = Util.GetCorePosition(base.gameObject),
                rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward)
            }, false);
            mdl = GetModelTransform();
            if (mdl)
            {
                charModel = mdl.GetComponent<CharacterModel>();
                turnHurtboxesOff = mdl.GetComponent<HurtBoxGroup>();
            }
            
            if (charModel)
            {
                charModel.invisibilityCount++;
                turnHurtboxesOff.hurtBoxesDeactivatorCounter++;
            }
            Util.PlaySound("Play_imp_attack_blink", base.gameObject);
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
            }
            particles1 = UnityEngine.Object.Instantiate(Prefabs.ImpOverlordParticles, Util.GetCorePosition(base.gameObject), Util.QuaternionSafeLookRotation(base.characterDirection.forward));
            particles1.transform.parent = base.FindModelChild("MuzzleCenter");
            particles2 = UnityEngine.Object.Instantiate(Prefabs.mithrixPreBossBillboard, Util.GetCorePosition(base.gameObject), Util.QuaternionSafeLookRotation(base.characterDirection.forward));
            particles2.transform.parent = base.FindModelChild("MuzzleCenter");
            //var lolLmao = particles1.GetComponentInChildren<ParticleSystemRenderer>(true);
            //lolLmao.material.SetTexture("_RemapTex", Prefabs.purpleRamp);
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            var buttonDown = inputBank.skill3.down;
            if (base.isAuthority && base.fixedAge >= maxDuration || !buttonDown)
            {
                this.outer.SetNextStateToMain();
            } 
        }
        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
            }
            twinBehaviour.chainsVfx1.SetActive(false);
            twinBehaviour.chainsVfx2.SetActive(false);
            EffectManager.SpawnEffect(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflash.prefab"), new EffectData
            {
                origin = Util.GetCorePosition(base.gameObject),
                rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward)
            }, false);
            Util.PlaySound("Play_imp_attack_blink", base.gameObject);
            if (particles1) Destroy(particles1);
            if (particles2) Destroy(particles2);

            if (charModel) charModel.invisibilityCount--;

            if (turnHurtboxesOff) turnHurtboxesOff.hurtBoxesDeactivatorCounter--;
            
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
