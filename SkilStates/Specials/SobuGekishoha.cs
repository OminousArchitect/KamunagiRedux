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
using Kamunagi.Modules;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;

namespace Kamunagi
{
    class SobuGekishoha : BaseTwinState
    {
        public override int meterGain => 0;
        private float duration = 5;
        private float fireFrequency = 0.15f;
        private float stopwatch;
        private float maxRange = 180;
        private float damageCoefficient = 3;
        private GameObject tracerInstance;
        private Transform sphereTransform;
        private Transform runeTransform;
        private GameObject muzzleEffect;
        private GameObject runeMuzzleEffect;
        private CameraTargetParams.AimRequest request;
        private Vector3 spherePosition;
        private Vector3 runePosition;
        private float bulletCount = 0;

        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode(duration + 1);
            Util.PlaySound(EntityStates.VoidRaidCrab.SpinBeamAttack.enterSoundString, base.gameObject);
            base.characterMotor.useGravity = false;
            sphereTransform = base.FindModelChild("MuzzleCenterFar");
            runeTransform = FindModelChild("MuzzleCenter");
            spherePosition = sphereTransform.position;
            runePosition = runeTransform.position;
            tracerInstance = UnityEngine.Object.Instantiate(Prefabs.voidTracer, spherePosition, Quaternion.identity, null);
            tracerInstance.transform.localScale = new Vector3(1, 1, 0.03f * maxRange);
            muzzleEffect = UnityEngine.Object.Instantiate(Prefabs.voidTracerSphere, spherePosition, Quaternion.identity, sphereTransform);
            muzzleEffect.transform.localRotation = Quaternion.identity;
            muzzleEffect.transform.localScale = Vector3.one;
            //transform.position + Vector3.back * 2f; example
            var additive = characterDirection.forward * 0.5f;
            runeMuzzleEffect = UnityEngine.Object.Instantiate(Prefabs.laserSigil, (runePosition + additive), Util.QuaternionSafeLookRotation(characterDirection.forward, Vector3.up), runeTransform);
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(Buffs.twinsArmor);
            }
            ScaleParticleSystemDuration component = muzzleEffect.GetComponent<ScaleParticleSystemDuration>();
            if (component)
            {
                component.newDuration = 1;
                for (int i = 0; i < component.particleSystems.Length; i++)
                {
                    var p = component.particleSystems[i];
                    if (p && i != 2 && i != 3)
                    {
                        var main = p.main;
                        main.duration = duration;
                        main.startLifetime = duration;
                        var scale = p.sizeOverLifetime;
                        scale.enabled = false;
                    }
                }
            }
            if (base.cameraTargetParams)
            {
                request = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
        }
        public override void Update()
        {
            base.Update();
            base.characterMotor.velocity = Vector3.zero;
            if (tracerInstance && sphereTransform)
            {
                Ray aimRay = base.GetAimRay();
                tracerInstance.transform.position = sphereTransform.position;
                tracerInstance.transform.forward = aimRay.direction;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.deltaTime;
            if (stopwatch >= fireFrequency)
            {
                stopwatch = 0;
                Fire();
                //Debug.Log($"{bulletCount}");
            }

            bool timeUp = base.fixedAge >= this.duration;
            if (timeUp || !inputBank.skill4.down && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        void Fire()
        {
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                new BulletAttack
                {
                    maxDistance = maxRange,
                    stopperMask = LayerIndex.noCollision.mask,
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0,
                    maxSpread = 0.4f,
                    bulletCount = 1U,
                    damage = base.damageStat * damageCoefficient,
                    force = 155,
                    tracerEffectPrefab = null,
                    muzzleName = muzzleString,
                    hitEffectPrefab = Prefabs.Load<GameObject>("RoR2/DLC1/MissileVoid/VoidImpactEffect.prefab"),
                    isCrit = base.RollCrit(),
                    radius = 0.2f,
                    procCoefficient = 0.3f,
                    smartCollision = true,
                    damageType = DamageType.Generic
                }.Fire();
                bulletCount++;
            }
        }
        public override void OnExit()
        {
            if (request != null)
            {
                request.Dispose();
            }
            Util.PlaySound(EntityStates.VoidRaidCrab.SpinBeamWindDown.enterSoundString, base.gameObject);
            base.characterMotor.useGravity = true;
            if (tracerInstance)
            {
                Destroy(tracerInstance);
            }
            if (muzzleEffect)
            {
                Destroy(muzzleEffect);
            }

            if (runeMuzzleEffect)
            {
                Destroy(runeMuzzleEffect);
            }

            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(Buffs.twinsArmor);
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
