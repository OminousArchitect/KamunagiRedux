using System;
using EntityStates;
using KamunagiOfChains.Data.States;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Projectiles
{
    class AltSoeiMusou : BaseTwinState
    {
        public static GameObject megaBlaster =
            Asset.LoadAsset<GameObject>(
                "addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterSmallProjectile.prefab")!;

        public float maxChargeTime = 3f;
        public Transform muzzleTransform = null!;
        public EffectManagerHelper? chargeEffectInstance;
        public float projectileFireFrequency = 0.4f;
        public float ballDamageCoefficient = 6f;
        public float stopwatch;
        public bool charged;
        public override int meterGain => 0;

        public override void OnEnter()
        {
            base.OnEnter();
            maxChargeTime *= attackSpeedStat;
            muzzleTransform = FindModelChild("MuzzleCenter");
            if (!muzzleTransform || !Asset.TryGetGameObject<AltMusou, IEffect>(out var muzzleEffect)) return;
            chargeEffectInstance = EffectManager.GetAndActivatePooledEffect(muzzleEffect, muzzleTransform, true);
            var scale = chargeEffectInstance.effectComponent.GetComponent<ObjectScaleCurve>();
            scale.baseScale = Vector3.one * 0.7f;
            scale.timeMax = projectileFireFrequency;
            scale.Reset();
        }

        public void FireProjectiles()
        {
            if (!isAuthority || !Asset.TryGetGameObject<AltMusou, IProjectile>(out var projectile)) return;
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
            {
                crit = RollCrit(),
                damage = characterBody.damage * 1.2f,
                damageTypeOverride = DamageTypeCombo.Generic,
                damageColorIndex = DamageColorIndex.Default,
                force = 120,
                owner = gameObject,
                position = muzzleTransform.position,
                projectilePrefab = projectile,
                rotation = Quaternion.LookRotation(GetAimRay().direction),
            });
        }

        public void FireBall()
        {
            if (!isAuthority || !Asset.TryGetGameObject<AltMusouChargeBall, IProjectile>(out var projectile)) return;
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
            {
                crit = RollCrit(),
                damage = characterBody.damage * ballDamageCoefficient,
                damageTypeOverride = DamageTypeCombo.Generic,
                damageColorIndex = DamageColorIndex.Default,
                force = 100 * ballDamageCoefficient,
                owner = gameObject,
                position = muzzleTransform.position,
                projectilePrefab = fixedAge < maxChargeTime ? megaBlaster : projectile,
                rotation = Quaternion.LookRotation(GetAimRay().direction),
            });
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && !inputBank.skill1.down)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (fixedAge >= maxChargeTime && chargeEffectInstance != null && !charged)
            {
                var scale = chargeEffectInstance.effectComponent.GetComponent<ObjectScaleCurve>();
                scale.baseScale = Vector3.one;
                scale.timeMax = projectileFireFrequency;
                scale.Reset();
                charged = true;
            }

            stopwatch += Time.deltaTime;
            if (stopwatch < projectileFireFrequency) return;
            //0.2 frequency is equal to 5 times per second
            //0.1 would be 10 times per second
            stopwatch = 0;
            FireProjectiles();
        }

        public override void OnExit()
        {
            StartAimMode();
            if (chargeEffectInstance != null) chargeEffectInstance.ReturnToPool();
            if (fixedAge >= maxChargeTime) FireBall();
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }

    public class AltMusou : Asset, IProjectile, IProjectileGhost, IEffect, ISkill
    {
        SkillDef ISkill.BuildObject()
        {
            var skill = ScriptableObject.CreateInstance<SkillDef>();
            skill.skillName = "Primary 2";
            skill.skillNameToken = "NINES_KAMUNAGI_BODY_PRIMARY1_NAME";
            skill.skillDescriptionToken = "NINES_KAMUNAGI_BODY_PRIMARY1_DESCRIPTION";
            skill.icon = LoadAsset<Sprite>("bundle:darkpng");
            skill.activationStateMachineName = "Weapon";
            skill.baseMaxStock = 0;
            skill.baseRechargeInterval = 2f;
            skill.beginSkillCooldownOnSkillEnd = true;
            skill.interruptPriority = InterruptPriority.Any;
            skill.cancelSprintingOnActivation = false;
            skill.rechargeStock = 0;
            skill.requiredStock = 0;
            skill.stockToConsume = 0;
            skill.keywordTokens = new[] { "KEYWORD_AGILE" };
            return skill;
        }

        Type[] ISkill.GetEntityStates() => new[] { typeof(AltSoeiMusou) };

        GameObject IProjectile.BuildObject()
        {
            var projectile =
                LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab")!
                    .InstantiateClone("TwinsTrackingProjectile");
            if (TryGetGameObject<AltMusou, IProjectileGhost>(out var ghost))
                projectile.GetComponent<ProjectileController>().ghostPrefab = ghost;
            //projectile.GetComponent<ProjectileDirectionalTargetFinder>().lookRange = 15f;
            projectile.GetComponent<ProjectileDamage>().damage = 1f;
            return projectile;
        }

        GameObject IProjectileGhost.BuildObject()
        {
            var ghost = LoadAsset<GameObject>(
                    "addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterSmallGhost.prefab")!
                .InstantiateClone("TwinsTrackingGhost", false);
            ghost.GetComponentInChildren<Light>().color = Colors.twinsLightColor;
            return ghost;
        }

        GameObject IEffect.BuildObject()
        {
            var effect =
                LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/VoidMegacrabBlackSphere.prefab")!
                    .InstantiateClone("ChargedMusouEffect", false);
            effect.transform.localScale = Vector3.one * 0.5f;

            effect.SetActive(false); // Required for pooled effects or you get a warning about effectData not being set
            var vfx = effect.GetOrAddComponent<VFXAttributes>();
            vfx.DoNotPool = false;
            vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;

            var scale = effect.AddComponent<ObjectScaleCurve>();
            scale.useOverallCurveOnly = true;
            scale.timeMax = 0.5f;
            scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);

            var altPP = effect.AddComponent<PostProcessVolume>();
            altPP.profile = LoadAsset<PostProcessProfile>("addressable:RoR2/Base/title/ppLocalBrotherImpact.asset");
            altPP.sharedProfile = altPP.profile;

            var musouInstance =
                new Material(LoadAsset<Material>("addressable:RoR2/Base/Brother/matBrotherPreBossSphere.mat"));
            musouInstance.SetTexture("_RemapTex",
                LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonPreBoss.png"));
            musouInstance.SetColor("_TintColor", Colors.twinsLightColor);
            var coolSphere = effect.GetComponent<MeshRenderer>();
            coolSphere.materials = new[] { musouInstance };
            coolSphere.shadowCastingMode = ShadowCastingMode.On;

            var pointLight = LoadAsset<GameObject>("addressable:RoR2/Base/bazaar/Bazaar_Light.prefab")!.transform
                .GetChild(1).gameObject.InstantiateClone("Point Light", false);
            pointLight.transform.parent = effect.transform;
            pointLight.transform.localPosition = Vector3.zero;
            pointLight.transform.localScale = Vector3.one * 0.5f;
            pointLight.GetComponent<Light>().range = 0.5f;
            var altSparks = LoadAsset<GameObject>("addressable:RoR2/Base/Blackhole/GravSphere.prefab")!.transform
                .GetChild(1).gameObject.InstantiateClone("Sparks, Blue", false);
            var altP = altSparks.GetComponent<ParticleSystem>();
            var altPMain = altP.main;
            altPMain.simulationSpeed = 2f;
            altPMain.startColor = Colors.twinsLightColor;
            altSparks.GetComponent<ParticleSystemRenderer>().material =
                LoadAsset<Material>("addressable:RoR2/Base/Common/VFX/matTracerBrightTransparent.mat");
            altSparks.transform.parent = effect.transform;
            altSparks.transform.localPosition = Vector3.zero;
            altSparks.transform.localScale = Vector3.one * 0.05f;

            var altCoreP = effect.AddComponent<ParticleSystem>();
            var coreR = effect.GetComponent<ParticleSystemRenderer>();
            var decalMaterial =
                new Material(LoadAsset<Material>("addressable:RoR2/Base/Brother/matLunarShardImpactEffect.mat"));
            decalMaterial.SetTexture("_RemapTex",
                LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
            coreR.material = decalMaterial;
            coreR.renderMode = ParticleSystemRenderMode.Billboard;
            var coreM = altCoreP.main;
            coreM.duration = 1f;
            coreM.simulationSpeed = 1.1f;
            coreM.loop = true;
            coreM.startLifetime = 0.13f;
            coreM.startSpeed = 5f;
            coreM.startSize3D = false;
            coreM.startSizeY = 0.6f;
            coreM.startRotation3D = false;
            coreM.startRotationZ = 0.1745f;
            coreM.startSpeed = 0f;
            coreM.maxParticles = 30;
            var coreS = altCoreP.shape;
            coreS.enabled = false;
            coreS.shapeType = ParticleSystemShapeType.Circle;
            coreS.radius = 0.67f;
            coreS.arcMode = ParticleSystemShapeMultiModeValue.Random;
            var sparkleSize = altCoreP.sizeOverLifetime;
            sparkleSize.enabled = true;
            sparkleSize.separateAxes = true;
            //sparkleSize.sizeMultiplier = 0.75f;
            sparkleSize.xMultiplier = 1.3f;

            #region UnusedLightFlickerValues

            /*var altLight = pointLight.GetComponent<FlickerLight>();
            var flicker0 = altLight.sinWaves[0];
            flicker0.period = 0.08333334f;
            flicker0.amplitude = 0.2f;
            flicker0.frequency = 12f;
            flicker0.cycleOffset = 61.35653f; 
            var flicker1 = altLight.sinWaves[1];
            flicker1.period = 0.1666667f;
            flicker1.amplitude = 0.1f;
            flicker1.frequency = 6f;
            flicker1.cycleOffset = 96.17653f;
            var flicker2 = altLight.sinWaves[2];
            flicker2.period = 0.1111111f;
            flicker2.amplitude = 0.1f;
            flicker2.frequency = 9f;
            flicker2.cycleOffset = 51.90653f;*/

            #endregion

            return effect;
        }
    }

    public class AltMusouChargeBall : Asset, IProjectile, IProjectileGhost
    {
        GameObject IProjectile.BuildObject()
        {
            var projectile =
                LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab")!
                    .InstantiateClone("TwinsAltChargeBallProjectile");
            if (TryGetGameObject<AltMusouChargeBall, IProjectileGhost>(out var ghost))
                projectile.GetComponent<ProjectileController>().ghostPrefab = ghost;
            return projectile;
        }

        GameObject IProjectileGhost.BuildObject()
        {
            if (!TryGetGameObject<AltMusou, IEffect>(out var effect)) throw new Exception("Effect not present");
            var ghost = effect.InstantiateClone("TwinsAltChargeBallGhost", false);
            Object.Destroy(ghost.GetComponent<ObjectScaleCurve>());
            Object.Destroy(ghost.GetComponent<EffectComponent>());
            ghost.AddComponent<ProjectileGhostController>();
            return ghost;
        }
    }
}