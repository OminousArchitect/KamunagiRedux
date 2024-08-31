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
using Console = System.Console;

namespace Kamunagi
{
    class KujyuriFrost : BaseTwinState
    {
        private Vector3 raycastHitPoint;
        private float stopwatch;
        private float frostblastDamageCoefficient = 1.65f;
        private Transform muzzleTransform;
        private GameObject chargeEffectInstance;
        private GameObject fullChargeEffectInstance;
        private float maxChargeDuration = 0.8f;
        private float muzzleFlashTimer;
        private uint soundID;

        public override void OnEnter()
        {
            base.OnEnter();
            stopwatch = 0f;
            muzzleFlashTimer = 0f;
            soundID = AkSoundEngine.PostEvent("Play_mage_m2_iceSpear_charge", base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float scalingMuzzleFX = Util.Remap(fixedAge, 0.03f, 0.8f, 0.15f, 0.08f);
            if (base.inputBank && base.inputBank.skill2.down)
            {
                stopwatch += Time.deltaTime;
                muzzleFlashTimer += Time.deltaTime;
            }
            if (muzzleFlashTimer >= scalingMuzzleFX)
            {
                EffectManager.SimpleMuzzleFlash(Prefabs.frostMuzzleFlash, base.gameObject, muzzleString, false);
                muzzleFlashTimer = 0;
            }
            if (base.isAuthority && stopwatch >= maxChargeDuration /*&&!inputBank.skill3.down*/)
            {
                StageTwoAttack();
                outer.SetNextStateToMain();
            }
            else if (base.isAuthority && stopwatch < maxChargeDuration && !inputBank.skill2.down)
            {
                StageOneAttack();
                outer.SetNextStateToMain();
            }
        }

        private void StageOneAttack()
        {
            //Debug.Log("stage one");
            Ray aimRay = GetAimRay();
            BulletAttack frostShot = new BulletAttack();
            frostShot.maxDistance = 1000;
            frostShot.stopperMask = LayerIndex.entityPrecise.mask | LayerIndex.world.mask;
            frostShot.owner = base.gameObject;
            frostShot.weapon = base.gameObject;
            frostShot.origin = aimRay.origin;
            frostShot.aimVector = aimRay.direction;
            frostShot.minSpread = 0;
            frostShot.maxSpread = 0.4f;
            frostShot.bulletCount = 1U;
            frostShot.damage = base.damageStat * 1f;
            frostShot.force = 155;
            frostShot.tracerEffectPrefab = null;
            frostShot.muzzleName = muzzleString;
            frostShot.hitEffectPrefab = null;
            frostShot.isCrit = base.RollCrit();
            frostShot.radius = 0.8f;
            frostShot.procCoefficient = 0.4f;
            frostShot.smartCollision = true;
            frostShot.falloffModel = BulletAttack.FalloffModel.None;
            frostShot.damageType = DamageType.Freeze2s;
            frostShot.Fire();
        }
        private void StageTwoAttack()
        {
            //Debug.Log("stage two");
            Ray aimRay = GetAimRay();
            float num = 0f;
            RaycastHit raycastHit;
            if (Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(aimRay, base.gameObject, out num), out raycastHit, 1000 + num, LayerIndex.world.mask | LayerIndex.entityPrecise.mask))
            {
                raycastHitPoint = raycastHit.point;
            }
            new BlastAttack
            {
                attacker = base.gameObject,
                baseDamage = damageStat * frostblastDamageCoefficient,
                baseForce = 200f,
                crit = base.RollCrit(),
                damageType = DamageType.SlowOnHit | DamageType.Stun1s | DamageType.Freeze2s,
                falloffModel = BlastAttack.FalloffModel.None,
                procCoefficient = 1f,
                radius = 10f,
                position = raycastHitPoint,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                teamIndex = base.teamComponent.teamIndex
            }.Fire();
            EffectManager.SpawnEffect(Prefabs.customFrostNova, new EffectData
            {
                origin = raycastHitPoint,
                rotation = Util.QuaternionSafeLookRotation(aimRay.direction)
            }, false);
        }
        public override void OnExit()
        {
            EffectManager.SimpleMuzzleFlash(Prefabs.frostMuzzleFlash, base.gameObject, muzzleString, false);
            EffectManager.SimpleMuzzleFlash(Prefabs.frostMuzzleFlash, base.gameObject, muzzleString, false);
            AkSoundEngine.StopPlayingID(soundID);
            //Debug.Log($"stopwatch {stopwatch}");
            //Debug.Log($"{fixedAge} fixedAge");
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}