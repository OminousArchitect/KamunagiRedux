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
using EntityStates;

namespace Kamunagi
{
    public class TwinsSpawnState : BaseState
    {
        public static float minimumIdleDuration = 1.5f;
        private Animator modelAnimator;
        private CharacterModel characterModel;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, minimumIdleDuration);
            }
            
            characterModel = base.GetModelTransform().GetComponent<CharacterModel>();
            if (characterModel)
            {
                characterModel.invisibilityCount++;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= minimumIdleDuration)
            {
                outer.SetNextState(new BufferPortal());
            }
        }
    }
    
    public class BufferPortal : BaseState
    {
        private static GameObject spawnEffectPrefab = Prefabs.Load<GameObject>("RoR2/Base/Nullifier/NullifierSpawnEffect.prefab");
        public static float duration = 2f;
        private CharacterModel characterModel;
        public override void OnEnter()
        {
            base.OnEnter();
            this.characterModel = null;
            if (base.characterBody.modelLocator && base.characterBody.modelLocator.modelTransform)
            {
                this.characterModel = base.characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            }
            if (characterModel)
            {
                characterModel.invisibilityCount--;
            }
            
            if (spawnEffectPrefab)
            {
                //Util.PlaySound(EntityStates.NullifierMonster.SpawnState.spawnSoundString, gameObject);
                Util.PlaySound("Play_nullifier_spawn", base.gameObject);
                EffectManager.SimpleMuzzleFlash(spawnEffectPrefab, base.gameObject, "MuzzleRear", false);
            }

            if (NetworkServer.active) characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, duration * 1.2f);
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= BufferPortal.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}