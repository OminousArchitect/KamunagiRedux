using System;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
    class ReaverMusouState : BaseTwinState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (!Asset.TryGetGameObject<ReaverMusou, IEffect>(out var tracer)) throw new Exception("Effect failed to load.");
            var aimRay = GetAimRay();
            var testForTarget = new BulletAttack() {
                owner = gameObject,
                weapon = gameObject,
                origin = aimRay.origin,
                aimVector = aimRay.direction,
                maxDistance = 1000,
                damage = 0,
                force = 0,
                radius = 0.3f,
                procCoefficient = 0,
                muzzleName = "MuzzleRight",
                tracerEffectPrefab = tracer,
                hitCallback = (BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo) =>
                {
                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                    {
                        position = hitInfo.point,
                        crit = RollCrit(),
                        
                    });
                    return true;
                }
            };
            testForTarget.Fire();
        }
    }
    
    class ReaverMusou : Asset, ISkill, IEffect, IProjectile, IProjectileGhost
    {
        SkillDef ISkill.BuildObject()
        {
            throw new NotImplementedException();
        }

        Type[] ISkill.GetEntityStates()
        {
            throw new NotImplementedException();
        }

        GameObject IEffect.BuildObject()
        {
            throw new NotImplementedException();
        }

        GameObject IProjectile.BuildObject()
        {
            throw new NotImplementedException();
        }

        GameObject IProjectileGhost.BuildObject()
        {
            throw new NotImplementedException();
        }
    }
}