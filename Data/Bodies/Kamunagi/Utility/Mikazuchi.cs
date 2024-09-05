using System;
using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
    public class MikazuchiState : IndicatorSpellState
    {
        public EffectManagerHelper? chargeEffectInstance;
        public float projectileCount = 3f;
        public override float duration => 5f;
        public override float failedCastCooldown => 2f;

        public override void OnEnter()
        {
            base.OnEnter();
            var muzzleTransform = FindModelChild("MuzzleCenter");
            if (!muzzleTransform || !Asset.TryGetGameObject<Mikazuchi, IEffect>(out var muzzleEffect)) return;
            chargeEffectInstance = EffectManager.GetAndActivatePooledEffect(muzzleEffect, muzzleTransform, true);
        }
        
        public override void Fire(Vector3 targetPosition)
        {
            base.Fire(targetPosition);
            var blastAttack = new BlastAttack
            {
                attacker = gameObject,
                baseDamage = damageStat * 8.5f,
                baseForce = 1800,
                crit = RollCrit(),
                damageType = DamageType.Shock5s,
                falloffModel = BlastAttack.FalloffModel.None,
                procCoefficient = 1,
                radius = 3f,
                position = targetPosition,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                teamIndex = teamComponent.teamIndex
            };
            blastAttack.Fire();
            if (Asset.TryGetGameObject<MikazuchiLightningStrike, IEffect>(out var effect))
                EffectManager.SpawnEffect(effect, new EffectData()
                {
                    origin = targetPosition,
                    scale = blastAttack.radius
                }, true);

            var xoro = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
            var spacingDegrees = 360f / projectileCount;
            var forward = Vector3.ProjectOnPlane(inputBank.aimDirection, Vector3.up);
            var centerPoint = targetPosition + Vector3.up * 2.5f;
            for (var i = 0; i < projectileCount; i++)
            {
                ProjectileManager.instance.FireProjectile(Asset.GetGameObject<MikazuchiLightningOrb, IProjectile>(),
                    centerPoint,
                    Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(spacingDegrees * i, Vector3.up) *
                                                    forward),
                    gameObject,
                    damageStat,
                    10f,
                    RollCrit(),
                    speedOverride: xoro.RangeInt(13, 28)
                );
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeEffectInstance != null)
                chargeEffectInstance.ReturnToPool();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }

    public class Mikazuchi : Asset, IEffect, ISkill
    {
        GameObject IEffect.BuildObject()
        {
            var effect = LoadAsset<GameObject>("bundle:MikazuchiMuzzle.prefab")!;

            var comp = effect.GetOrAddComponent<EffectComponent>();
            comp.applyScale = false;
            comp.parentToReferencedTransform = true;
            comp.positionAtReferencedTransform = true;
            comp.effectData = new EffectData() { };
            effect.SetActive(false); // Required for pooled effects or you get a warning about effectData not being set
            var vfx = effect.GetOrAddComponent<VFXAttributes>();
            vfx.DoNotPool = false;
            vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;

            return effect;
        }

        Type[] ISkill.GetEntityStates() => new[] { typeof(MikazuchiState) };

        SkillDef ISkill.BuildObject()
        {
            var skill = ScriptableObject.CreateInstance<SkillDef>();
            skill.skillName = "Utility 0";
            skill.skillNameToken = "NINES_KAMUNAGI_BODY_UTILITY0_NAME";
            skill.skillDescriptionToken = "NINES_KAMUNAGI_BODY_UTILITY0_DESCRIPTION";
            skill.icon = LoadAsset<Sprite>("bundle:Mikazuchi");
            skill.activationStateMachineName = "Weapon";
            skill.baseRechargeInterval = 2f;
            skill.beginSkillCooldownOnSkillEnd = true;
            skill.fullRestockOnAssign = false;
            skill.interruptPriority = InterruptPriority.Any;
            skill.isCombatSkill = false;
            skill.cancelSprintingOnActivation = false;
            skill.keywordTokens = new[] { "TWINSSHOCK_KEYWORD" };
            return skill;
        }
    }

    public class MikazuchiLightningStrike : Asset, IEffect
    {
        GameObject IEffect.BuildObject()
        {
            var effect =
                LoadAsset<GameObject>("addressable:RoR2/Base/Lightning/LightningStrikeImpact.prefab")!.InstantiateClone(
                    "MikazuchiLightningStrikeImpact", false);
            effect.GetComponentInChildren<Light>().color = Color.yellow;
            var comp = effect.GetOrAddComponent<EffectComponent>();
            comp.parentToReferencedTransform = true;
            comp.positionAtReferencedTransform = true;
            comp.soundName = "Play_item_use_lighningArm";

            var pp = effect.GetComponentInChildren<PostProcessVolume>();
            pp.profile = LoadAsset<PostProcessProfile>("addressable:RoR2/Base/title/ppLocalGrandparent.asset");
            pp.sharedProfile = pp.profile;

            var rampTeleport =
                LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png");
            var sphereMat = LoadAsset<Material>("addressable:RoR2/Base/Loader/matLightningLongYellow.mat");
            foreach (var particleSystemRenderer in effect.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                switch (particleSystemRenderer.name)
                {
                    case "Ring":
                        particleSystemRenderer.material.SetTexture("_RemapTex", rampTeleport);
                        particleSystemRenderer.material.SetColor("_TintColor", Color.yellow);
                        break;
                    case "LightningRibbon":
                        particleSystemRenderer.trailMaterial = new Material(particleSystemRenderer.trailMaterial);
                        particleSystemRenderer.trailMaterial.SetTexture("_RemapTex", rampTeleport);
                        break;
                    case "Sphere":
                        particleSystemRenderer.material = sphereMat;
                        break;
                    case "Flash":
                        particleSystemRenderer.material.DisableKeyword("VERTEXCOLOR");
                        break;
                }
            }

            return effect;
        }
    }

    public class MikazuchiLightningStrikeSilent : Asset, IEffect
    {
        GameObject IEffect.BuildObject()
        {
            var effect = GetGameObject<MikazuchiLightningStrike, IEffect>()
                .InstantiateClone("MikazuchiSilentImpact", false);
            effect.GetOrAddComponent<EffectComponent>().soundName = "";
            return effect;
        }
    }

    public class MikazuchiLightningOrb : Asset, IProjectile, IProjectileGhost
    {
        GameObject IProjectile.BuildObject()
        {
            var projectile =
                LoadAsset<GameObject>("addressable:RoR2/Base/ElectricWorm/ElectricOrbProjectile.prefab")!
                    .InstantiateClone("MikazuchiLightningOrbProjectile");
            projectile.GetComponent<ProjectileController>().ghostPrefab =
                GetGameObject<MikazuchiLightningOrb, IProjectileGhost>();
            projectile.GetComponent<ProjectileDamage>().damageType = DamageType.Shock5s;
            var lightningImpact = projectile.GetComponent<ProjectileImpactExplosion>();
            lightningImpact.impactEffect = GetGameObject<MikazuchiLightningStrikeSilent, IEffect>();
            lightningImpact.childrenProjectilePrefab = GetGameObject<MikazuchiLightningSeeker, IProjectile>();
            lightningImpact.childrenDamageCoefficient = 1.7f;
            var lightpact = projectile.GetComponent<ProjectileImpactExplosion>();
            lightpact.falloffModel = BlastAttack.FalloffModel.None;
            lightpact.blastDamageCoefficient = 5f;
            return projectile;
        }

        GameObject IProjectileGhost.BuildObject()
        {
            var ghost = LoadAsset<GameObject>("addressables:RoR2/Base/ElectricWorm/ElectricOrbGhost.prefab")!
                .InstantiateClone("MikazuchiLightningOrbGhost", false);
            var warbannerRamp = LoadAsset<Texture2D>("addressables:RoR2/Base/Common/ColorRamps/texRampWarbanner2.png");
            foreach (var r in ghost.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var name = r.name;
                if (name != "SpitCore") continue;
                r.material.SetTexture("_RemapTex", warbannerRamp);
                r.material.SetColor("_TintColor", Color.yellow);
            }

            var teleporterRamp =
                LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png");
            var (firstTrail, (secondTrail, _)) = ghost.GetComponentsInChildren<TrailRenderer>();
            firstTrail.material.SetTexture("_RemapTex", teleporterRamp);
            secondTrail.material.SetTexture("_RemapTex", teleporterRamp);
            ghost.GetComponentInChildren<Light>().color = Color.yellow;

            return ghost;
        }
    }

    public class MikazuchiLightningSeeker : Asset, IProjectile, IProjectileGhost
    {
        GameObject IProjectile.BuildObject()
        {
            var projectile =
                LoadAsset<GameObject>("addressable:RoR2/Base/ElectricWorm/ElectricWormSeekerProjectile.prefab")!
                    .InstantiateClone("MikazuchiLightningSeekerProjectile", true);
            projectile.GetComponent<ProjectileController>().ghostPrefab =
                GetGameObject<MikazuchiLightningSeeker, IProjectileGhost>();
            projectile.GetComponent<ProjectileImpactExplosion>().impactEffect =
                GetGameObject<MikazuchiStakeNova, IEffect>();
            return projectile;
        }

        GameObject IProjectileGhost.BuildObject()
        {
            var ghost = LoadAsset<GameObject>("addressable:RoR2/Base/ElectricWorm/ElectricWormSeekerGhost.prefab")!.InstantiateClone("Mikazuch iLightningSeekerGhost", false);
            ghost.GetComponentInChildren<TrailRenderer>().startColor = Color.yellow;
            return ghost;
        }
    }

    public class MikazuchiStakeNova : Asset, IEffect
    {
        GameObject IEffect.BuildObject()
        {
            var effect = LoadAsset<GameObject>("addressable:RoR2/Base/EliteLightning/LightningStakeNova.prefab")!.InstantiateClone("MikazuchiStakeNova", false);
            effect.transform.localScale = Vector3.one * 2;
            var (novaPr, _) = effect.GetComponentsInChildren<ParticleSystemRenderer>();
            novaPr.material.SetTexture("_RemapTex",
                LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png"));
            //novaPr[3].material.DisableKeyword("VERTEXCOLOR");
            foreach (var p in effect.GetComponentsInChildren<ParticleSystem>())
            {
                switch (p.name)
                {
                    case "AreaIndicatorRing, Billboard":
                    case "UnscaledHitsparks 1":
                    case "Flash":
                        var main = p.main;
                        main.startColor = Color.yellow;
                        break;
                }
            }

            effect.GetComponentInChildren<Light>().color = Color.yellow;
            
            var comp = effect.GetOrAddComponent<EffectComponent>();
            comp.parentToReferencedTransform = true;
            comp.positionAtReferencedTransform = true;
            comp.soundName = "Play_mage_m1_impact";

            return effect;
        }
    }
}