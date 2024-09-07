using System;
using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
    class SoeiMusouState : BaseTwinState
    {
        public override int meterGain => 0;

        public static GameObject MuzzlePrefab = LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab")!;

        public float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            AkSoundEngine.PostEvent("Play_voidman_m2_shoot", gameObject);
            EffectManager.SimpleMuzzleFlash(MuzzlePrefab, gameObject, twinMuzzle, false);
            if (isAuthority && Asset.TryGetGameObject<SoeiMusou, IProjectile>(out var projectile))
            {
                duration = 0.45f / attackSpeedStat;
                var aimRay = GetAimRay();
                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    crit = RollCrit(),
                    damage = characterBody.damage * 2.9f,
                    force = 500,
                    owner = gameObject,
                    position = aimRay.origin,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    projectilePrefab = projectile,
                    useSpeedOverride = true,
                    speedOverride = 105f,
                });
            }
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority || fixedAge < duration) return;
            outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }

    class SoeiMusou : Asset, ISkill, IProjectile, IProjectileGhost
    {
        SkillDef ISkill.BuildObject()
        {
            var skill = ScriptableObject.CreateInstance<SkillDef>();
            skill.skillName = "Primary 0";
            skill.skillNameToken = KamunagiAsset.tokenPrefix + "PRIMARY0_NAME";
            skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "PRIMARY0_DESCRIPTION";
            skill.icon = LoadAsset<Sprite>("bundle:darkpng");
            skill.activationStateMachineName = "Weapon";
            skill.baseMaxStock = 4;
            skill.baseRechargeInterval = 2f;
            skill.interruptPriority = InterruptPriority.Any;
            skill.cancelSprintingOnActivation = false;
            skill.rechargeStock = 2;
            return skill;
        }

        Type[] ISkill.GetEntityStates() => new []{typeof(SoeiMusouState)};

        GameObject IProjectile.BuildObject()
        {
            var projectile = LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab")!.InstantiateClone("VoidProjectileSimple");
            if (!TryGetGameObject<SoeiMusou, IProjectileGhost>(out var ghost)) throw new Exception("Ghost not loaded");
            projectile.GetComponent<ProjectileController>().ghostPrefab = ghost;
            var rb = projectile.GetComponent<Rigidbody>();
            rb.useGravity = true;
            var antiGrav = projectile.AddComponent<AntiGravityForce>();
            antiGrav.rb = rb;
            antiGrav.antiGravityCoefficient = 0.7f;
            return projectile;
        }

        GameObject IProjectileGhost.BuildObject()
        {
            var ghost = LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigGhost.prefab")!.InstantiateClone("VoidProjectileSimpleGhost", false);
            var (solidParallax, (cloudRemap, _)) = ghost.GetComponentInChildren<MeshRenderer>().materials;
            solidParallax.SetTexture("_EmissionTex", LoadAsset<Texture2D>("addressable:RoR2/DLC1/voidraid/texRampVoidRaidSky.png"));
            solidParallax.SetFloat("_EmissionPower", 1.5f);
            solidParallax.SetFloat("_HeightStrength", 4.1f);
            solidParallax.SetFloat("_HeightBias", 0.35f);
            solidParallax.SetFloat("_Parallax", 1f);
            solidParallax.SetColor("_Color", Colors.twinsLightColor);

            cloudRemap.SetTexture("_RemapTex", LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampIce.png"));
            cloudRemap.SetColor("_TintColor", Colors.twinsTintColor);
            cloudRemap.SetFloat("_AlphaBoost", 3.88f);

            var scale = ghost.AddComponent<ObjectScaleCurve>();
            scale.useOverallCurveOnly = true;
            scale.timeMax = 0.12f;
            scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
            scale.baseScale = Vector3.one * 0.6f;
            return ghost;
        }
    }
}