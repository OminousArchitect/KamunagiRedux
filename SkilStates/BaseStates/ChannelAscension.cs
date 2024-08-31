using UnityEngine;
using RoR2;
using EntityStates;
using Kamunagi;
using RoR2.Projectile;
using Console = System.Console;

namespace Kamunagi
{
    class ChannelAscension : BaseTwinState
    {
        public float duration = 0.8f;
        private Vector3 origin;
        protected float ascendSpeedMult;
        public override int meterGain => 0;
        private Vector3 thePosition;
        
        public override void OnEnter()
        {
            base.OnEnter();
            origin = this.transform.position;
            thePosition = characterBody.footPosition + Vector3.up * 0.15f;
            if (base.isAuthority)
            {
                FireVacuum();
            }
            //StartAimMode(duration);
        }

        private void FireVacuum()
        {
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                crit = false,
                damage = this.characterBody.damage * 0.5f,
                damageTypeOverride = DamageType.Generic,
                damageColorIndex = DamageColorIndex.Void,
                force = 10,
                owner = base.gameObject,
                position = thePosition,
                procChainMask = default(RoR2.ProcChainMask),
                projectilePrefab = Prefabs.VacuumProjectile,
                rotation = Quaternion.identity,
                useFuseOverride = true,
                _fuseOverride = fixedAge,
                target = null
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority)
            {
                this.characterMotor.Motor.SetPosition(this.origin);
                this.characterMotor.velocity = Vector3.zero;

                if (base.fixedAge >= this.duration || !inputBank.jump.down)
                { 
                    characterBody.isSprinting = true; 
                    ascendSpeedMult = Util.Remap(fixedAge, 0, duration, 0.4f, 1.5f);

                    if (!MainPlugin.variableDashDirection.Value)
                    {
                        outer.SetNextState(new DarkAscension
                        {
                            flyRay = GetAimRay(),
                            speedMult = ascendSpeedMult,
                            effectPosition = thePosition,
                        });
                    }
                    else
                    { 
                        outer.SetNextState(new DarkAscension
                        { 
                            speedMult = ascendSpeedMult,
                            effectPosition = thePosition 
                        });
                    }
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }

    class DarkAscension : BaseTwinState 
    {
        public override int meterGain => 0;
        public float speedMult;
        public Ray flyRay;
        private Vector3 flyVector;
        public Vector3 effectPosition;
        public Vector3 rayDir;
        private float duration = 1.4f;
        public bool passedBool;
        public static AnimationCurve flyCurve = EntityStates.Mage.FlyUpState.speedCoefficientCurve;

        public override void OnEnter() 
        {
            base.OnEnter();
            //Debug.Log($"{speedMult} remapped");
            PlayAnimation("Saraana", "FlyUp");
            PlayAnimation("Ururuu", "FlyUp");
            CreateBlinkEffect(effectPosition);
            EffectManager.SimpleMuzzleFlash(Prefabs.yamatoWinds, base.gameObject, "MuzzleRight", false);
            EffectManager.SimpleMuzzleFlash(Prefabs.yamatoWinds, base.gameObject, "MuzzleLeft", false);
            base.characterMotor.Motor.ForceUnground();
        }

        private void CreateBlinkEffect(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(this.flyVector); 
            effectData.origin = origin;
            EffectManager.SpawnEffect(Prefabs.Load<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerCaptureCharge.prefab"), effectData, false);
            Util.PlaySound("Play_voidJailer_m2_shoot", base.gameObject);
        }
        
        public override void FixedUpdate()
        { 
            base.FixedUpdate();
            if (base.isAuthority && fixedAge >= duration) 
            {
                this.outer.SetNextStateToMain(); 
            } 
            HandleMovements();
        }
        
        public void HandleMovements()
        {
            if (MainPlugin.variableDashDirection.Value)
            {
                flyRay = GetAimRay();
            }

            flyVector = inputBank.interact.wasDown ? -flyRay.direction * speedMult : flyRay.direction * speedMult;
            base.characterMotor.rootMotion += flyVector * (moveSpeedStat * flyCurve.Evaluate(fixedAge / duration) * Time.deltaTime);
            base.characterMotor.velocity.y = 0f;
        }

        public override InterruptPriority GetMinimumInterruptPriority() 
        { 
            return InterruptPriority.Skill; 
        } 
    }
}
