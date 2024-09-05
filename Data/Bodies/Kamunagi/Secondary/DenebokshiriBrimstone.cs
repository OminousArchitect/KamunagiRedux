using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using HarmonyLib;
using RoR2.Skills;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
    
    class DenebokshiriBrimstoneState : BaseTwinState 
    { 
        private uint soundID;
        private Transform muzzleTransform;
        private EffectManagerHelper? chargeEffectInstance;
        
        public override void OnEnter()
        {
            var confusingAsFuck = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
            muzzleTransform = base.FindModelChild("MuzzleCenter");
            if (muzzleTransform && TryGetGameObject<DenebokshiriBrimstone, IEffect>(out var muzzleEffect))
            {
                chargeEffectInstance = EffectManager.GetAndActivatePooledEffect(//UnityEngine.Object.Instantiate(Prefabs.miniSunChargeEffect, muzzleTransform.position, muzzleTransform.rotation);
                    ObjectScaleCurve scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if (scale)
                {
                    scale.baseScale = Vector3.one * 0.35f;
                    scale.timeMax = maxChargeTime;
                }
            }
            soundID = AkSoundEngine.PostEvent("Play_fireballsOnHit_pool_aliveLoop", base.gameObject);
        }
    }
    public class DenebokshiriBrimstone  :    Asset, IProjectile, IProjectileGhost, IEffect, ISkill
    {
        SkillDef ISkill.BuildOject()
        {
            var skill = ScriptableObject.CreateInstance<SkillDef>();
            skill.skillName = "Secondary 0";
            skill.skillNameToken = "NINES_KAMUNAGI_BODY_SECONDARY0_NAME";
            skill.skillDescriptionToken = "NINES_KAMUNAGI_BODY_SECONDARY0_DESCRIPTION";
            skill.icon = LoadAsset<Sprite>("bundle:firepng");
            skill.activationStateMachineName = "Weapon";
            skill.baseMaxStock = 1;
            skill.baseRechargeInterval = 2f;
            skill.interruptPriority = InterruptPriority.Any;
            skill.cancelSprintingOnActivation = false;
            skill.baseMaxStock = 1;
            skill.baseRechargeInterval = 2f;
            skill.beginSkillCooldownOnSkillEnd = false;
            skill.interruptPriority = InterruptPriority.Any;
            skill.cancelSprintingOnActivation = false;
            return skill;
        }
        
        GameObject IProjectile.BuildObject()
        {
            var proj = GetGameObject<WindBoomerang, IProjectile>()!.InstantiateClone(windBoomerang, "TwinsMiniSun", true);
            UnityEngine.Object.Destroy(miniSunProjectile.GetComponent<WindBoomerangProjectile>());
            UnityEngine.Object.Destroy(miniSunProjectile.GetComponent<BoomerangProjectile>()); //bro what is this spaghetti
            UnityEngine.Object.Destroy(miniSunProjectile.GetComponent<ProjectileOverlapAttack>());
            UnityEngine.Object.Destroy(miniSunProjectile.GetComponent<ProjectileDotZone>());
            var minisunController = miniSunProjectile.GetComponent<ProjectileController>();
            minisunController.ghostPrefab = miniSunGhost;
            minisunController.flightSoundLoop = null;
            minisunController.startSound = "Play_fireballsOnHit_impact";
            miniSunProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Denebokshiri);
            var minisunSimple = miniSunProjectile.AddComponent<ProjectileSimple>();
            minisunSimple.desiredForwardSpeed = 20f;
            minisunSimple.lifetime = 5f;
            minisunSimple.lifetimeExpiredEffect = fireHitEffect;
            var singleI = miniSunProjectile.AddComponent<ProjectileSingleTargetImpact>();
            singleI.impactEffect = fireHitEffect;
            singleI.destroyOnWorld = true;
            var proxBeam = miniSunProjectile.AddComponent<ProjectileProximityBeamController>();
            proxBeam.attackFireCount = 1;
            proxBeam.attackInterval = 0.15f;
            proxBeam.listClearInterval = 1;
            proxBeam.attackRange = 15f; //radius
            proxBeam.minAngleFilter = 0;
            proxBeam.maxAngleFilter = 180;
            proxBeam.procCoefficient = 0.3f;
            proxBeam.damageCoefficient = 0.1f;
            proxBeam.bounces = 0;
            proxBeam.lightningType = LightningOrb.LightningType.Loader;
            proxBeam.inheritDamageType = true;
        }
    }
}