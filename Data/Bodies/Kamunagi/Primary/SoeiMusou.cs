using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
    class SoeiMusouState : BaseTwinState
    {
        public override int meterGain => 0;

        public static GameObject muzzlePrefab =
            Asset.LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab")!;
        public override void OnEnter()
        {
            base.OnEnter();
            if (characterMotor.isGrounded) StartAimMode();
            AkSoundEngine.PostEvent("Play_voidman_m2_shoot", base.gameObject);
            EffectManager.SimpleMuzzleFlash(muzzlePrefab, base.gameObject, twinMuzzle, false);
            var aimRay = base.GetAimRay();
            if (!isAuthority || !Asset.TryGetGameObject<SoeiMusou, IProjectile>(out var projectile)) return;
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
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }
    class SoeiMusou : Asset, ISkill
    {
        
    }
}