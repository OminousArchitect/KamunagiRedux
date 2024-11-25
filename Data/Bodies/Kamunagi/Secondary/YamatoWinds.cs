using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using RoR2.Skills;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	internal class YamatoWindsState : BaseTwinState
	{
		public override int meterGain => 5;
		private float damageCoefficient = 2f;
		private const float maxChargeTime = 1.5f;
		private EffectManagerHelper? chargeEffectInstance;
		private CameraTargetParams.AimRequest? aimRequest;
		public static GameObject boomerangPrefab;
		public static BuffDef parryBuff;
		private float stopwatch;
		private bool hasFired;
		private bool condition;

		public override void OnEnter()
		{
			base.OnEnter();
			var muzzleTransform = FindModelChild("MuzzleCenter");
			var effect = Concentric.GetEffect<YamatoWinds>().WaitForCompletion();
			if (muzzleTransform)
			{
				chargeEffectInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(effect, muzzleTransform, true,
					new EffectData() { rootObject = muzzleTransform.gameObject });
			}

			aimRequest = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
			characterBody.SetBuffCount(parryBuff.buffIndex, 1);
			condition = true;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate(); 
			// this is some real spaghetti because I'm tired of
			// reworking this skill for the 6th time, please forgive
			if (!isAuthority) return;
			if (!IsKeyDownAuthority() || stopwatch >= 0.2f)
			{
				outer.SetNextStateToMain();
			}

			if (inputBank.skill1.down)
			{
				condition = false;
				stopwatch += Time.fixedDeltaTime;
				characterBody.SetBuffCount(Concentric.GetBuffIndex<YamatoWinds>().WaitForCompletion(), 0);
				if (!hasFired)
				{
					Fire();
				}
			}
			
			if (fixedAge > maxChargeTime && condition)
			{
				skillLocator.DeductCooldownFromAllSkillsAuthority(2f);
				characterBody.AddTimedBuffAuthority(RoR2.DLC1Content.Buffs.KillMoveSpeed.buffIndex, 2f);
				outer.SetNextStateToMain();
			}
		}

		private void Fire()
		{
			hasFired = true;
			if (chargeEffectInstance != null)
			{
				chargeEffectInstance.ReturnToPool();
			}
			var aimRay = GetAimRay();

			if (isAuthority)
			{
				var fireProjectileInfo = new FireProjectileInfo
				{
					crit = RollCrit(),
					damage = characterBody.damage * damageCoefficient,
					damageTypeOverride = DamageType.Generic,
					damageColorIndex = DamageColorIndex.Default,
					force = 500,
					owner = gameObject,
					position = aimRay.origin,
					procChainMask = default,
					projectilePrefab = boomerangPrefab,
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
			aimRequest?.Dispose();
			if (chargeEffectInstance != null)
			{
				chargeEffectInstance.ReturnToPool();
			}
			characterBody.SetBuffCount(parryBuff.buffIndex, 0);
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
	}

	[HarmonyPatch]
	public class YamatoWinds : Concentric, IProjectile, IProjectileGhost, IEffect, ISkill, IBuff
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			YamatoWindsState.parryBuff = await this.GetBuffDef();
			YamatoWindsState.boomerangPrefab = await this.GetProjectile();
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(YamatoWindsState) };

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Secondary 2";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SECONDARY2_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SECONDARY2_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("bundle:windpng"));
			skill.activationStateMachineName = "Weapon";
			skill.fullRestockOnAssign = true;
			skill.baseMaxStock = 2;
			skill.baseRechargeInterval = 8f;
			skill.stockToConsume = 1;
			skill.beginSkillCooldownOnSkillEnd = false;
			skill.canceledFromSprinting = false;
			skill.interruptPriority = InterruptPriority.Any;
			skill.isCombatSkill = false;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = true;
			skill.keywordTokens = new[] { KamunagiAsset.tokenPrefix + "TWINSWIND_KEYWORD" };
			return skill;
		}

		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj= (await LoadAsset<GameObject>("RoR2/Base/Saw/Sawmerang.prefab"))!.InstantiateClone("TwinsWindBoomerang",
				true);
			Object.Destroy(proj.GetComponent<BoomerangProjectile>());
			Object.Destroy(proj.GetComponent<ProjectileOverlapAttack>());
			var windDamage = proj.GetComponent<ProjectileDotZone>();
			windDamage.damageCoefficient = 0.5f;
			windDamage.overlapProcCoefficient = 0.3f;
			windDamage.fireFrequency = 25f;
			windDamage.resetFrequency = 10f;
			windDamage.impactEffect = await GetEffect<WindHitEffect>();
			var itjustworks = proj.AddComponent<WindBoomerangProjectileBehaviour>();
			//haha hopefully
			var windSounds = proj.GetComponent<ProjectileController>();
			windSounds.startSound = "Play_merc_m2_uppercut";
			windSounds.flightSoundLoop =
				await LoadAsset<LoopSoundDef>("RoR2/Base/LunarSkillReplacements/lsdLunarSecondaryProjectileFlight.asset");
			proj.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost= (await LoadAsset<GameObject>("RoR2/Base/LunarSkillReplacements/LunarSecondaryGhost.prefab"))!
				.InstantiateClone("TwinsWindBoomerangGhost", false);
			var windPsr = ghost.GetComponentsInChildren<ParticleSystemRenderer>();
			windPsr[0].material.SetColor("_TintColor", Colors.windyGreen);
			windPsr[2].enabled = false;
			windPsr[3].enabled = false;
			windPsr[3].material.SetColor("_TintColor", Colors.windyGreen);
			windPsr[3].material.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampHealing.png"));
			windPsr[4].enabled = false;
			windPsr[5].enabled = false;
			var boomerangTrail = ghost.GetComponentInChildren<TrailRenderer>();
			boomerangTrail.material = new Material(boomerangTrail.material);
			boomerangTrail.material.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampHealing.png"));
			boomerangTrail.material.SetColor("_TintColor", Colors.windyGreen);
			var windLight = ghost.GetComponentInChildren<Light>();
			windLight.color = Colors.windyGreen;
			windLight.intensity = 20f;
			var windMR = ghost.GetComponentsInChildren<MeshRenderer>();
			windMR[0].material.SetColor("_TintColor", Colors.windyGreen);
			windMR[1].material.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampHealing.png"));
			return ghost;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect =
				(await this.GetProjectileGhost())!.InstantiateClone("TwinsWindChargeEffect", false);
			Object.Destroy(effect.GetComponent<ProjectileGhostController>());
			var attributes = effect.AddComponent<VFXAttributes>();
			attributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
			attributes.DoNotPool = false;
			var spaghet = (await LoadAsset<GameObject>("RoR2/Base/Merc/EvisProjectile.prefab"))!.InstantiateClone("Scuffed", false);
			UnityEngine.Object.Destroy(spaghet.GetComponent<NetworkIdentity>());
			UnityEngine.Object.Destroy(spaghet.GetComponent<ProjectileController>());
			UnityEngine.Object.Destroy(spaghet.GetComponent<Rigidbody>());
			UnityEngine.Object.Destroy(spaghet.GetComponent<ProjectileNetworkTransform>());
			UnityEngine.Object.Destroy(spaghet.GetComponent<ProjectileSimple>());
			UnityEngine.Object.Destroy(spaghet.GetComponent<ProjectileDamage>());
			UnityEngine.Object.Destroy(spaghet.GetComponent<TeamFilter>());
			UnityEngine.Object.Destroy(spaghet.GetComponent<ProjectileImpactExplosion>());
			UnityEngine.Object.Destroy(spaghet.GetComponent<BoxCollider>());
			spaghet.transform.SetParent(effect.transform);
			return effect;
		}

		async Task<BuffDef> IBuff.BuildObject()
		{
			var buffDef = ScriptableObject.CreateInstance<BuffDef>();
			buffDef.name = "TwinsWindDeflect";
			buffDef.buffColor = Colors.earlyZeal;
			buffDef.canStack = false;
			buffDef.isDebuff = false;
			buffDef.iconSprite= (await LoadAsset<Sprite>("RoR2/Junk/Common/texBuffBodyArmorIcon.tif"));
			buffDef.isHidden = false;
			return buffDef;
		}
		
		[HarmonyPrefix, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamageProcess))]
		private static void TakeDamageProcessParry(HealthComponent __instance, DamageInfo damageInfo)
		{
			if (__instance.body.bodyIndex != Concentric.GetBodyIndex<KamunagiAsset>().WaitForCompletion()) return;
			if (!__instance.body.HasBuff(Concentric.GetBuffDef<YamatoWinds>().WaitForCompletion())) return;
			EffectData effectData = new EffectData
			{
				origin = damageInfo.position,
				rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
			};
			damageInfo.rejected = true;
			EffectManager.SpawnEffect(GetEffect<WindBlockEffect>().WaitForCompletion(), effectData, transmit: true);
		}
	}

	public class WindBlockEffect : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("RoR2/Base/Bear/BearProc.prefab"))!.InstantiateClone("TwinsParryEffect", false);
			effect.EffectWithSound("Play_merc_m1_hard_swing");
			return effect;
		}
	}
	
	public class WindHitEffect : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect =
				(await LoadAsset<GameObject>("RoR2/Base/Merc/MercExposeConsumeEffect.prefab"))!.InstantiateClone(
					"TwinsWindHitEffect", false);
			Object.Destroy(effect.GetComponent<OmniEffect>());
			foreach (var r in effect.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				r.gameObject.SetActive(true);
				r.material.SetColor("_TintColor", Color.green);
				if (r.name == "PulseEffect, Ring (1)")
				{
					var mat = r.material;
					mat.mainTexture= (await LoadAsset<Texture2D>("RoR2/Base/Common/VFX/texArcaneCircleProviMask.png"));
				}
			}

			effect.EffectWithSound("Play_huntress_R_snipe_shoot");
			return effect;
		}
	}


	[RequireComponent(typeof(ProjectileController))]
	internal class WindBoomerangProjectileBehaviour : NetworkBehaviour, IProjectileImpactBehavior
	{
		public float travelSpeed = 60f;

		public float transitionDuration = 0.75f;

		private float attackScale = 4f;

		public float distanceMultiplier = 0.3f;

		private float maxFlyStopwatch;

		public GameObject impactSpark;

		public GameObject crosshairPrefab;

		public bool canHitCharacters;

		public bool canHitWorld;

		private ProjectileController projectileController;
		[SyncVar] private WindBoomerangState windboomerangState;

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
			rigidbody = GetComponent<Rigidbody>();
			projectileController = GetComponent<ProjectileController>();
			projectileDamage = GetComponent<ProjectileDamage>();

			if (projectileController && projectileController.owner)
			{
				ownerTransform = projectileController.owner.transform;
			}

			maxFlyStopwatch = distanceMultiplier;
		}

		private void Start() => transform.localScale = Vector3.one * attackScale;

		public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
		{
			if (!canHitWorld)
			{
				return;
			}

			NetworkboomerangState = WindBoomerangState.FlyBack;
			var unityEvent = onWindFlyBack;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}

			EffectManager.SimpleImpactEffect(impactSpark, impactInfo.estimatedPointOfImpact, -transform.forward, true);
		}

		private bool Reel()
		{
			var vector = projectileController.owner.transform.position - transform.position;
			var normalized = vector.normalized;
			return vector.magnitude <= 2f;
		}

		public void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				if (!setScale)
				{
					setScale = true;
				}

				if (!projectileController.owner)
				{
					Destroy(gameObject);
					return;
				}

				switch (windboomerangState)
				{
					case WindBoomerangState.FlyOut:
						if (NetworkServer.active)
						{
							rigidbody.velocity = travelSpeed * transform.forward;
							stopwatch += Time.deltaTime;
							if (stopwatch >= maxFlyStopwatch)
							{
								stopwatch = 0f;
								NetworkboomerangState = WindBoomerangState.Transition;
								return;
							}
						}

						break;
					case WindBoomerangState.Transition:
						{
							stopwatch += Time.deltaTime;
							var num = stopwatch / transitionDuration;
							var thisVector = NetworkedVector();
							rigidbody.velocity = Vector3.Lerp(travelSpeed * transform.forward, travelSpeed * thisVector,
								num);
							if (num >= 1f)
							{
								NetworkboomerangState = WindBoomerangState.FlyBack;
								var unityEvent = onWindFlyBack;
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
							var flag = Reel();
							if (NetworkServer.active)
							{
								canHitWorld = false;
								var thisVector = NetworkedVector();
								rigidbody.velocity = travelSpeed * thisVector;
								if (flag)
								{
									Destroy(gameObject);
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
			if (projectileController.owner)
			{
				return (projectileController.owner.transform.position - transform.position).normalized;
			}

			return transform.forward;
		}

		protected WindBoomerangState NetworkboomerangState
		{
			get => windboomerangState;
			[param: In]
			set
			{
				var newValueAsUlong = (ulong)(long)value;
				var fieldValueAsUlong = (ulong)(long)windboomerangState;
				SetSyncVarEnum<WindBoomerangState>(value, newValueAsUlong, ref windboomerangState, fieldValueAsUlong,
					1U);
			}
		}

		// Token: 0x06004361 RID: 17249 RVA: 0x0011785C File Offset: 0x00115A5C
		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.Write((int)windboomerangState);
				return true;
			}

			var flag = false;
			if ((syncVarDirtyBits & 1U) != 0U)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(syncVarDirtyBits);
					flag = true;
				}

				writer.Write((int)windboomerangState);
			}

			if (!flag)
			{
				writer.WritePackedUInt32(syncVarDirtyBits);
			}

			return flag;
		}

		// Token: 0x06004362 RID: 17250 RVA: 0x001178C8 File Offset: 0x00115AC8
		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
			{
				windboomerangState = (WindBoomerangState)reader.ReadInt32();
				return;
			}

			var num = (int)reader.ReadPackedUInt32();
			if ((num & 1) != 0)
			{
				windboomerangState = (WindBoomerangState)reader.ReadInt32();
			}
		}

		// Token: 0x06004363 RID: 17251 RVA: 0x00002225 File Offset: 0x00000425
		public override void PreStartClient()
		{
		}
	}
}