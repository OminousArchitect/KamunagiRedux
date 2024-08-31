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
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;
using EntityStates.GrandParent;
using Kamunagi;

namespace Kamunagi
{
    class LightOfNaturesAxiom : BaseTwinState
    {
        public override int meterGain => 0;
        private float chargeDuration = 1;
        private Vector3 sunPos;
        private GameObject left;
        private GameObject right;
        private GameObject sun;
        private CameraTargetParams.AimRequest request;
        private TeamIndex team;
        
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(ChannelSunStart.beginSoundName, base.gameObject);
            characterMotor.velocity = Vector3.zero;
            characterMotor.useGravity = false;
            sunPos = (Vector3.up * 1.25f) + childLocator.FindChild("MuzzleCenter").position + base.characterDirection.forward * 5;
            team = base.GetComponent<CharacterBody>().teamComponent.teamIndex;
            left = NewHandEffect(childLocator.FindChild("MuzzleLeft"));
            right = NewHandEffect(childLocator.FindChild("MuzzleRight"));

            if (base.cameraTargetParams)
            {
                request = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
        }
        private GameObject NewHandEffect(Transform muzzle)
        {
            var chargeEffect = UnityEngine.Object.Instantiate<GameObject>(Prefabs.chargeSunEffect, muzzle);
            /*ChildLocator component = UnityEngine.Object.Instantiate<GameObject>(Prefabs.sunStreamEffect, muzzle).GetComponent<ChildLocator>();
            var end = component.FindChild("EndPoint");
            end.SetPositionAndRotation(sunPos, Quaternion.identity);
            end.SetParent(chargeEffect.transform);*/
            return chargeEffect;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active && base.fixedAge >= chargeDuration && !sun)
            {
                sun = UnityEngine.Object.Instantiate(Prefabs.naturesAxiom, sunPos, Quaternion.identity);
                sun.GetComponent<GenericOwnership>().ownerObject = base.gameObject;
                sun.GetComponent<UmbralSunController>().bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(team);
                NetworkServer.Spawn(sun);
                EffectManager.SimpleEffect(Prefabs.sunExplosion, sunPos, Quaternion.identity, true);
            }
            if (base.isAuthority && !base.IsKeyDownAuthority())
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (request != null)
            {
                request.Dispose();
            }
            if (sun)
            {
                Destroy(sun);
                EffectManager.SimpleEffect(Prefabs.sunExplosion, sunPos, Quaternion.identity, false);
            }
            if (left)
            {
                Destroy(left.gameObject);
            }
            if (right)
            {
                Destroy(right.gameObject);
            }
            characterMotor.useGravity = true;
            twinBehaviour.Invoke("ToddHoward", 2f);
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
