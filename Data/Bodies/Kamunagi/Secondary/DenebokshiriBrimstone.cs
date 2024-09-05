using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using HarmonyLib;
using RoR2.Orbs;
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
            var proj = GetGameObject<WindBoomerang, IProjectile>()!.InstantiateClone("TwinsMiniSun", true);
            UnityEngine.Object.Destroy(proj.GetComponent<WindBoomerangProjectile>());
            UnityEngine.Object.Destroy(proj.GetComponent<BoomerangProjectile>()); //bro what is this spaghetti
            UnityEngine.Object.Destroy(proj.GetComponent<ProjectileOverlapAttack>());
            UnityEngine.Object.Destroy(proj.GetComponent<ProjectileDotZone>());
            var minisunController = proj.GetComponent<ProjectileController>();
            minisunController.ghostPrefab = GetGameObject<DenebokshiriBrimstone, IProjectileGhost>();
            minisunController.flightSoundLoop = null;
            minisunController.startSound = "Play_fireballsOnHit_impact";
            proj.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Denebokshiri);
            var minisunSimple = proj.AddComponent<ProjectileSimple>();
            minisunSimple.desiredForwardSpeed = 20f;
            minisunSimple.lifetime = 5f;
            minisunSimple.lifetimeExpiredEffect = GetGameObject<FireHitEffect, IEffect>();
            var singleI = proj.AddComponent<ProjectileSingleTargetImpact>();
            singleI.impactEffect = GetGameObject<FireHitEffect, IEffect>();
            singleI.destroyOnWorld = true;
            var proxBeam = proj.AddComponent<ProjectileProximityBeamController>();
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
            return proj;
        }

        GameObject IProjectileGhost.BuildObject()
        {
            var ghost = LoadAsset<GameObject>("addressable:RoR2/Base/Grandparent/ChannelGrandParentSunHands.prefab")!.InstantiateClone("TwinsChargeMiniSun", false);
            ObjectScaleCurve scale = ghost.AddComponent<ObjectScaleCurve>();
            scale.useOverallCurveOnly = true;
            scale.timeMax = 2f;
            scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
            var minisunMesh = ghost.GetComponentInChildren<MeshRenderer>();
            minisunMesh.material.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/FireballsOnHit/texFireballsOnHitIcon.png"));
            minisunMesh.material.SetFloat("_AlphaBoost", 6.351971f);
            ghost.AddComponent<ProjectileGhostController>();
            ghost.AddComponent<MeshFilter>().mesh = LoadAsset<Mesh>("RoR2/Base/Common/VFX/mdlVFXIcosphere.fbx");
            var miniSunIndicator = PrefabAPI.InstantiateClone(LoadAsset<GameObject>("addressable:RoR2/Base/Grandparent/GrandparentGravSphere.prefab").transform.GetChild(0).gameObject, "MiniSunIndicator", false);
            miniSunIndicator.transform.parent = ghost.transform; //this was the first time I figured this out
            miniSunIndicator.transform.localPosition = Vector3.zero;
            miniSunIndicator.transform.localScale = Vector3.one * 25f;
            
            var gravSphere = PrefabAPI.InstantiateClone(LoadAsset<GameObject>("addressable:RoR2/Base/Grandparent/GrandparentGravSphereGhost.prefab").transform.GetChild(0).gameObject, "Indicator", false);
            var gooDrops = PrefabAPI.InstantiateClone(gravSphere.transform.GetChild(3).gameObject, "MiniSunGoo", false);
            gooDrops.transform.parent = ghost.transform; // adding the indicator sphere to DenebokshiriBrimstone
            gooDrops.transform.localPosition = Vector3.zero;
            return ghost;
        }

        GameObject IEffect.BuildObject()
        {
            
        }

        public class FireHitEffect : Asset, IEffect
        {
            GameObject IEffect.BuildObject()
            {
                var effect = PrefabAPI.InstantiateClone(LoadAsset<GameObject>("addressable:RoR2/Base/Merc/MercExposeConsumeEffect.prefab"), "TwinsFireHitEffect", false);
                UnityEngine.Object.Destroy(effect.GetComponent<OmniEffect>());
                foreach (ParticleSystemRenderer r in effect.GetComponentsInChildren<ParticleSystemRenderer>(true))
                {
                    r.gameObject.SetActive(true);
                    r.material.SetColor("_TintColor", new Color(0.7264151f, 0.1280128f, 0f));
                    if (r.name == "PulseEffect, Ring (1)")
                    {
                        var mat = r.material;
                        mat.mainTexture = LoadAsset<Texture2D>("RoR2/Base/Common/VFX/texArcaneCircleProviMask.png");
                    }
                }
                effect.EffectWithSound("Play_item_use_molotov_throw");
                return effect;
            }
        }
    }
}