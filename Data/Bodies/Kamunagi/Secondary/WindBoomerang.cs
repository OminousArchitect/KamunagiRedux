using System;
using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using Rewired.UI.ControlMapper;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using RoR2.Skills;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
    public class WindBoomerangState : BaseTwinState
    {
        public override int meterGain => 5;
        private float damageCoefficient = 2.8f;
        private float maxChargeTime = 1.5f;
        private float minDistance = 0.05f;
        private float maxDistance = 0.6f;
        public EffectManagerHelper? chargeEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            if (characterMotor.isGrounded)
            {
                base.StartAimMode();
            }

            var muzzleTransform = base.FindModelChild("MuzzleCenter");
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
            if (!isAuthority || (fixedAge < maxChargeTime && IsKeyDownAuthority())) return;
            var aimRay = GetAimRay();
            var prefab = Asset.GetGameObject<WindBoomerang, IProjectile>();
            prefab.GetComponent<WindBoomerangProjectileBehaviour>().distanceMultiplier = Util.Remap(fixedAge, 0, maxChargeTime, minDistance, maxDistance);
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
            {
                crit = RollCrit(),
                damage = characterBody.damage * damageCoefficient,
                force = 500,
                owner = gameObject,
                position = aimRay.origin,
                projectilePrefab = prefab,
                rotation = Quaternion.LookRotation(aimRay.direction),
                useFuseOverride = false,
                useSpeedOverride = true,
                speedOverride = 50,
            });
            outer.SetNextStateToMain();
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
	    SkillDef ISkill.BuildObject()
	    {
		    var skill = ScriptableObject.CreateInstance<SkillDef>();
		    skill.skillName = "Secondary 0";
		    skill.skillNameToken = "SECONDARY0_NAME";
		    skill.skillDescriptionToken = "SECONDARY0_DESCRIPTION";
		    skill.icon = LoadAsset<Sprite>("bundle:firepng");
		    skill.activationStateMachineName = "Weapon";
		    skill.baseMaxStock = 1;
		    skill.baseRechargeInterval = 2f;
		    skill.beginSkillCooldownOnSkillEnd = false;
		    skill.canceledFromSprinting = false;
		    skill.interruptPriority = InterruptPriority.Any;
		    skill.isCombatSkill = true;
		    skill.mustKeyPress = true; 
		    skill.cancelSprintingOnActivation = false;
		    return skill;
	    }

	    Type[] ISkill.GetEntityStates() => new[] {typeof(WindBoomerangState) };

	    GameObject IProjectile.BuildObject()
        {
            var proj =
                LoadAsset<GameObject>("addressable:RoR2/Base/Saw/Sawmerang.prefab")!.InstantiateClone(
                    "TwinsWindBoomerang", true);
            foreach (var boom in proj.GetComponents<BoomerangProjectile>())
            {
                Object.Destroy(boom);
            }
            UnityEngine.Object.Destroy(proj.GetComponent<ProjectileOverlapAttack>());
            var windDamage = proj.GetComponent<ProjectileDotZone>();
            windDamage.damageCoefficient = 0.5f;
            windDamage.overlapProcCoefficient = 0.2f;
            windDamage.fireFrequency = 25f;
            windDamage.resetFrequency = 10f;
            windDamage.impactEffect = GetGameObject<WindHitEffect, IEffect>();
            var itjustworks = proj.AddComponent<WindBoomerangProjectileBehaviour>(); //ugh we gotta add this later
            var windSounds = proj.GetComponent<ProjectileController>();
            windSounds.startSound = "Play_merc_m2_uppercut";
            windSounds.flightSoundLoop = LoadAsset<LoopSoundDef>("addressable:RoR2/Base/LunarSkillReplacements/lsdLunarSecondaryProjectileFlight.asset");
            proj.GetComponent<ProjectileController>().ghostPrefab = GetGameObject<WindBoomerang, IProjectileGhost>();
            return proj;
        }

        GameObject IProjectileGhost.BuildObject()
        {
            var windyGreen = new Color(0.175f, 0.63f, 0.086f);
            
            var ghost = LoadAsset<GameObject>("addressable:RoR2/Base/LunarSkillReplacements/LunarSecondaryGhost.prefab")!.InstantiateClone( "TwinsWindBoomerangGhost", false);
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
            effect.EffectWithSound("Play_huntress_R_snipe_shoot");
            return effect;
        }
    }
    

    [RequireComponent(typeof(ProjectileController))]
	class WindBoomerangProjectileBehaviour : NetworkBehaviour, IProjectileImpactBehavior
	{
		public float travelSpeed = 60f;

		public float transitionDuration = 0.75f;
		
		private float attackScale = 4f;
		
		public float distanceMultiplier = 0.05f;
		
		private float maxFlyStopwatch;

		public GameObject impactSpark;
		
		public GameObject crosshairPrefab;
		
		public bool canHitCharacters;
		
		public bool canHitWorld;
		
		private ProjectileController projectileController;
		[SyncVar]
		private WindBoomerangState windboomerangState;
		
		private Transform ownerTransform;
		
		private ProjectileDamage projectileDamage;

		private Rigidbody rigidbody;

		private float stopwatch;

		private float fireAge;

		private float fireFrequency;

		[FormerlySerializedAs("onFlyBack")] public UnityEvent onWindFlyBack;

		
		private bool setScale;

		protected enum WindBoomerangState
		{
			FlyOut,
			Transition,
			FlyBack
		}
		
		private void Awake()
		{
			this.rigidbody = base.GetComponent<Rigidbody>();
			this.projectileController = base.GetComponent<ProjectileController>();
			this.projectileDamage = base.GetComponent<ProjectileDamage>();
			if (this.projectileController && this.projectileController.owner)
			{
				this.ownerTransform = this.projectileController.owner.transform;
			}
			this.maxFlyStopwatch = distanceMultiplier;
		}
		
		private void Start()
		{
			base.transform.localScale = Vector3.one * attackScale;
		}

		public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
		{
			if (!this.canHitWorld)
			{
				return;
			}
			this.NetworkboomerangState = WindBoomerangState.FlyBack;
			UnityEvent unityEvent = this.onWindFlyBack;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			EffectManager.SimpleImpactEffect(this.impactSpark, impactInfo.estimatedPointOfImpact, -base.transform.forward, true);
		}

		private bool Reel()
		{
			Vector3 vector = this.projectileController.owner.transform.position - base.transform.position;
			Vector3 normalized = vector.normalized;
			return vector.magnitude <= 2f;
		}

		public void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				if (!this.setScale)
				{
					this.setScale = true;
				}
				if (!this.projectileController.owner)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
				switch (this.windboomerangState)
				{
				case WindBoomerangState.FlyOut:
					if (NetworkServer.active)
					{
						this.rigidbody.velocity = this.travelSpeed * base.transform.forward;
						this.stopwatch += Time.deltaTime;
						if (this.stopwatch >= this.maxFlyStopwatch)
						{
							this.stopwatch = 0f;
							this.NetworkboomerangState = WindBoomerangState.Transition;
							return;
						}
					}
					break;
				case WindBoomerangState.Transition:
				{
					this.stopwatch += Time.deltaTime;
					float num = this.stopwatch / this.transitionDuration;
					Vector3 thisVector = NetworkedVector();
					this.rigidbody.velocity = Vector3.Lerp(this.travelSpeed * base.transform.forward, this.travelSpeed * thisVector, num);
					if (num >= 1f)
					{
						this.NetworkboomerangState = WindBoomerangState.FlyBack;
						UnityEvent unityEvent = this.onWindFlyBack;
						if (unityEvent == null)
						{
							return;
						}
						unityEvent.Invoke();
						return;
					}
					break;
				}
				case WindBoomerangState.FlyBack:
				{
					bool flag = this.Reel();
					if (NetworkServer.active)
					{
						this.canHitWorld = false;
						Vector3 thisVector = NetworkedVector();
						this.rigidbody.velocity = this.travelSpeed * thisVector;
						if (flag)
						{
							UnityEngine.Object.Destroy(base.gameObject);
						}
					}
					break;
				}
				default:
					return;
				}
			}
		}
		
		private Vector3 NetworkedVector()
		{
			if (this.projectileController.owner)
			{
				return (this.projectileController.owner.transform.position - base.transform.position).normalized;
			}
			return base.transform.forward;
		}
		
		protected WindBoomerangState NetworkboomerangState
		{
			get
			{
				return this.windboomerangState;
			}
			[param: In]
			set
			{
				ulong newValueAsUlong = (ulong)((long)value);
				ulong fieldValueAsUlong = (ulong)((long)this.windboomerangState);
				base.SetSyncVarEnum<WindBoomerangState>(value, newValueAsUlong, ref this.windboomerangState, fieldValueAsUlong, 1U);
			}
		}

		// Token: 0x06004361 RID: 17249 RVA: 0x0011785C File Offset: 0x00115A5C
		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.Write((int)this.windboomerangState);
				return true;
			}
			bool flag = false;
			if ((base.syncVarDirtyBits & 1U) != 0U)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.Write((int)this.windboomerangState);
			}
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
			}
			return flag;
		}

		// Token: 0x06004362 RID: 17250 RVA: 0x001178C8 File Offset: 0x00115AC8
		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
			{
				this.windboomerangState = (WindBoomerangState)reader.ReadInt32();
				return;
			}
			int num = (int)reader.ReadPackedUInt32();
			if ((num & 1) != 0)
			{
				this.windboomerangState = (WindBoomerangState)reader.ReadInt32();
			}
		}

		// Token: 0x06004363 RID: 17251 RVA: 0x00002225 File Offset: 0x00000425
		public override void PreStartClient()
		{
		}
	}
}