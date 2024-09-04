using System.Numerics;
using KamunagiOfChains.Data.States;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace KamunagiOfChains.Data.Projectiles
{
    class AltSoeiMusou : BaseTwinState
    {
        public float maxChargeTime = 3f;
        public Transform muzzleTransform;
        public GameObject chargeEffectInstance;
        public float projectileFireFrequency = 0.4f;
        public override int meterGain => 0;

        public override void OnEnter()
        {
            base.OnEnter();
            maxChargeTime *= attackSpeedStat;
            muzzleTransform = FindModelChild("MuzzleCenter");
            if (!muzzleTransform || !Asset.TryGetGameObject<AltMusou, IEffect>(out var muzzleEffect)) return;
            chargeEffectInstance = Object.Instantiate(muzzleEffect, muzzleTransform);
            var scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
            scale.baseScale = Vector3.one * 0.7f;
            scale.timeMax = projectileFireFrequency;
        }

        public void FireProjectiles()
        {
            if (!isAuthority || !Asset.TryGetGameObject<AltMusou, IProjectile>(out var projectile)) return;
            var aimRay = GetAimRay();
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
                rotation = Quaternion.LookRotation(aimRay.direction),
            });
        }
    }
    public class AltMusou : Asset, IProjectile, IProjectileGhost, IEffect
    {
        GameObject IProjectile.BuildObject()
        {
            var projectile =
                LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterSmallGhost.prefab")!.InstantiateClone("TwinsTrackingProjectile");
            if (TryGetGameObject<AltMusou, IProjectileGhost>(out var ghost))
                projectile.GetComponent<ProjectileController>().ghostPrefab = ghost;
            //projectile.GetComponent<ProjectileDirectionalTargetFinder>().lookRange = 15f;
            projectile.GetComponent<ProjectileDamage>().damage = 1f;
            return projectile;
        }

        GameObject IProjectileGhost.BuildObject()
        { 
            var ghost = LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterSmallGhost.prefab")!
            .InstantiateClone("TwinsTrackingGhost");
            ghost.GetComponentInChildren<Light>().color = Colors.twinsLightColor;
            return ghost;
        }

        GameObject IEffect.BuildObject()
        {
            var effect = LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/VoidMegacrabBlackSphere.prefab")!.InstantiateClone("ChargedMusouEffect");
            effect.transform.localScale = Vector3.one * 0.5f;

            var scale = effect.AddComponent<ObjectScaleCurve>();
            scale.useOverallCurveOnly = true;
            scale.timeMax = 0.5f;
            scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
            
            var altPP = effect.AddComponent<PostProcessVolume>();
            altPP.profile = LoadAsset<PostProcessProfile>("addressable:RoR2/Base/title/ppLocalBrotherImpact.asset");
            altPP.sharedProfile = altPP.profile;
            
            var musouInstance = new Material(LoadAsset<Material>("addressable:RoR2/Base/Brother/matBrotherPreBossSphere.mat"));
            musouInstance.SetTexture("_RemapTex", LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonPreBoss.png"));
            musouInstance.SetColor("_TintColor", Colors.twinsLightColor);
            var coolSphere = effect.GetComponent<MeshRenderer>();
            coolSphere.materials = new[] { musouInstance };
            coolSphere.shadowCastingMode = ShadowCastingMode.On;
            
            var pointLight = LoadAsset<GameObject>("addressable:RoR2/Base/bazaar/Bazaar_Light.prefab")!.transform.GetChild(1).gameObject.InstantiateClone("Point Light");
            pointLight.transform.parent = effect.transform;
            pointLight.transform.localPosition = Vector3.zero;
            pointLight.transform.localScale = Vector3.one * 0.5f;
            pointLight.GetComponent<Light>().range = 0.5f;
            var altSparks = LoadAsset<GameObject>("addressable:RoR2/Base/Blackhole/GravSphere.prefab")!.transform.GetChild(1).gameObject.InstantiateClone("Sparks, Blue");
            var altP = altSparks.GetComponent<ParticleSystem>();
            var altPMain = altP.main;
            altPMain.simulationSpeed = 2f;
            altPMain.startColor = Colors.twinsLightColor;
            altSparks.GetComponent<ParticleSystemRenderer>().material = LoadAsset<Material>("addressable:RoR2/Base/Common/VFX/matTracerBrightTransparent.mat");
            altSparks.transform.parent = effect.transform;
            altSparks.transform.localPosition = Vector3.zero;
            altSparks.transform.localScale = Vector3.one * 0.05f;
            
            var altCoreP = effect.AddComponent<ParticleSystem>();
            var coreR = effect.GetComponent<ParticleSystemRenderer>();
            var decalMaterial = new Material(LoadAsset<Material>("addressable:RoR2/Base/Brother/matLunarShardImpactEffect.mat"));
            decalMaterial.SetTexture("_RemapTex", LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
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

    public class AltMusouChargeBall : Asset
    {
        
    }
}