using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Special
{
    public class TheGreatSealingState : IndicatorSpellState
    {
        public EffectManagerHelper? chargeEffectInstance;
        public override int meterGain => 0;
        public override float duration => 10f;
        public override float failedCastCooldown => 1f;

        public override void Fire(Vector3 targetPosition)
        {
            base.Fire(targetPosition);

            ProjectileManager.instance.FireProjectile(Asset.GetGameObject<PrimedObelisk, IProjectile>(),
                targetPosition,
                Quaternion.identity,
                gameObject,
                1f,
                1f,
                RollCrit()
            );
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (isAuthority) characterMotor.useGravity = false;
            var muzzleTransform = FindModelChild("MuzzleCenter");
            if (!muzzleTransform || !Asset.TryGetGameObject<TheGreatSealing, IEffect>(out var muzzleEffect)) return;
            chargeEffectInstance = EffectManager.GetAndActivatePooledEffect(muzzleEffect, muzzleTransform, true);
        }

        public override void Update()
        {
            base.Update();
            if (!isAuthority) return;
            characterMotor.velocityAuthority = Vector3.zero;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeEffectInstance != null) chargeEffectInstance.ReturnToPool();
            if (isAuthority) characterMotor.useGravity = true;
        }
    }

    public class TheGreatSealing : Asset, ISkill, IEffect
    {
        GameObject IEffect.BuildObject()
        {
            var effect =
                LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonGhost.prefab")!
                    .InstantiateClone("AntimatterMuzzleEffect", false);

            var comp = effect.GetOrAddComponent<EffectComponent>();
            comp.applyScale = false;
            comp.parentToReferencedTransform = true;
            comp.positionAtReferencedTransform = true;
            comp.effectData = new EffectData() { };
            effect.SetActive(false); // Required for pooled effects or you get a warning about effectData not being set
            var vfx = effect.GetOrAddComponent<VFXAttributes>();
            vfx.DoNotPool = false;
            vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;

            Object.Destroy(effect.GetComponent<ProjectileGhostController>());
            effect.transform.localScale = Vector3.one * 0.04f;
            var scaler = effect.transform.GetChild(0).gameObject;
            var blackSphere = scaler.transform.GetChild(1).gameObject;
            var (emissionMat, (rampMat, _)) = blackSphere.GetComponent<MeshRenderer>().materials;
            emissionMat.SetTexture("_Emission",
                LoadAsset<Texture2D>("addressable:RoR2/Base/ElectricWorm/ElectricWormBody.png"));
            rampMat.SetTexture("_RemapTex",
                LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampLunarElectric.png"));
            scaler.GetComponentInChildren<Light>().range = 20f;
            Object.Destroy(scaler.transform.GetChild(3).gameObject);
            return effect;
        }
    }

   
}