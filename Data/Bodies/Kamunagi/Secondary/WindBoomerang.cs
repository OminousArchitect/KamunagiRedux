using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using Rewired.UI.ControlMapper;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
    public class WindBoomerangState : BaseTwinState
    {
        public override int meterGain => 5;
        private float damageCoefficient = 2.8f;
        private float distanceMult;
        private float maxChargeTime = 1.5f;
        private float minDistance = 0.05f;
        private float maxDistance = 0.6f;
        private Transform muzzleTransform;
        private string effectMuzzleString = "MuzzleCenter";
        public EffectManagerHelper? chargeEffectInstance;
        
        public override void OnEnter()
        {
            base.OnEnter();
            if (characterMotor.isGrounded)
            {
                base.StartAimMode();
            }
            muzzleTransform = base.FindModelChild("MuzzleCenter");
            if (muzzleTransform && Asset.TryGetGameObject<WindBoomerang, IEffect>(out var muzzleEffect))
            {
                chargeEffectInstance = EffectManager.GetAndActivatePooledEffect(muzzleEffect, muzzleTransform, true);
                ObjectScaleCurve scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if (scale)
                {
                    scale.baseScale = Vector3.one;
                    scale.timeMax = maxChargeTime;
                }
            }
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            distanceMult = Util.Remap(fixedAge, 0, maxChargeTime, minDistance, maxDistance);

            if (base.isAuthority && fixedAge >= maxChargeTime)
            {
                Fire();
                outer.SetNextStateToMain();
            }
            
            if (base.isAuthority && !inputBank.skill2.down)
            {
                Fire();
                outer.SetNextStateToMain();
            }
        }
        
        void Fire()
        {
            Ray aimRay = base.GetAimRay();
            Asset.GetGameObject<WindBoomerang, IProjectile>().GetComponent<WindBoomerangProjectile>().distanceMultiplier = distanceMult;
            
            if (base.isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * damageCoefficient,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 500,
                    owner = base.gameObject,
                    position = aimRay.origin,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = Asset.GetGameObject<WindBoomerang, IProjectile>(),
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useFuseOverride = false,
                    useSpeedOverride = true,
                    speedOverride = 50,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
        
        public override void OnExit()
        {
            base.OnExit();
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }

    public class WindBoomerang : Asset, IProjectile, IProjectileGhost, IEffect, ISkill
    {
        GameObject IProjectile.BuildObject()
        {
            var proj = LoadAsset<GameObject>("addressable:RoR2/Base/Saw/Sawmerang.prefab")!.InstantiateClone( "TwinsWindBoomerang", true);
            UnityEngine.Object.Destroy(proj.GetComponent<BoomerangProjectile>());
            UnityEngine.Object.Destroy(proj.GetComponent<ProjectileOverlapAttack>());
            var windDamage = proj.GetComponent<ProjectileDotZone>();
            windDamage.damageCoefficient = 0.5f;
            windDamage.overlapProcCoefficient = 0.2f;
            windDamage.fireFrequency = 25f;
            windDamage.resetFrequency = 10f;
            windDamage.impactEffect = GetGameObject<WindHitEffect, IEffect>();
            var itjustworks = proj.AddComponent<WindBoomerangProjectile>(); //ugh we gotta add this later
            var windSounds = proj.GetComponent<ProjectileController>();
            windSounds.startSound = "Play_merc_m2_uppercut";
            windSounds.flightSoundLoop = LoadAsset<LoopSoundDef>("addressable:RoR2/Base/LunarSkillReplacements/lsdLunarSecondaryProjectileFlight.asset");
            proj.GetComponent<ProjectileController>().ghostPrefab = GetGameObject<WindBoomerang, IProjectileGhost>();
            return proj;
        }

        GameObject IProjectileGhost.BuildObject()
        {
            var windyGreen = new Color(0.175f, 0.63f, 0.086f);
            
            var ghost = LoadAsset<GameObject>("addresable:RoR2/Base/LunarSkillReplacements/LunarSecondaryGhost.prefab")!.InstantiateClone( "TwinsWindBoomerangGhost", false);
            var windPsr = ghost.GetComponentsInChildren<ParticleSystemRenderer>();
            windPsr[0].material.SetColor("_TintColor", windyGreen);
            windPsr[2].enabled = false;
            windPsr[3].enabled = false;
            windPsr[3].material.SetColor("_TintColor", windyGreen);
            windPsr[3].material.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampHealing.png"));
            windPsr[4].enabled = false;
            windPsr[5].enabled = false;
            var boomerangTrail = ghost.GetComponentInChildren<TrailRenderer>();
            boomerangTrail.material = new Material(boomerangTrail.material);
            boomerangTrail.material.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampHealing.png"));
            boomerangTrail.material.SetColor("_TintColor", windyGreen);
            var windLight = ghost.GetComponentInChildren<Light>();
            windLight.color = windyGreen;
            windLight.intensity = 20f;
            var windMR = ghost.GetComponentsInChildren<MeshRenderer>();
            windMR[0].material.SetColor("_TintColor", windyGreen);
            windMR[1].material.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampHealing.png"));
            return ghost;
        }

        GameObject IEffect.BuildObject()
        {
            var effect = GetGameObject<WindBoomerang, IProjectile>()!.InstantiateClone("WindChargeEffect", false);
            UnityEngine.Object.Destroy(effect.GetComponent<ProjectileGhostController>());
            return effect;
        }
    }

    public class WindHitEffect : Asset, IEffect
    {
        GameObject IEffect.BuildObject()
        {
            var effect = LoadAsset<GameObject>("addressable:RoR2/Base/Merc/MercExposeConsumeEffect.prefab")!.InstantiateClone( "TwinsWindHitEffect", false);
            UnityEngine.Object.Destroy(effect.GetComponent<OmniEffect>());
            foreach (ParticleSystemRenderer r in effect.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.gameObject.SetActive(true);
                r.material.SetColor("_TintColor", Color.green);
                if (r.name == "PulseEffect, Ring (1)")
                {
                    var mat = r.material;
                    mat.mainTexture = LoadAsset<Texture2D>("RoR2/Base/Common/VFX/texArcaneCircleProviMask.png");
                }
            }
            return effect;
        }
    }
}