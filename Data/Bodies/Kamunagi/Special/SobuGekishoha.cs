using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Special
{
	public class SobuGekishohaState : BaseTwinState
	{
		private float duration = 5;
		private float fireFrequency = 0.15f;
		private float stopwatch;
		private float maxRange = 180;
		private float damageCoefficient = 3;
		private Transform sphereTransform;
		private Transform sigilTransform;
		private Vector3 spherePosition;
		private Vector3 sigilPosition;

		private GameObject voidSphereMuzzle;
		private GameObject darkSigilEffect;
		private GameObject tracerInstance;
		private CameraTargetParams.AimRequest request;
		
		public EffectManagerHelper? voidSphere;
		public EffectManagerHelper? voidTracer;
		public EffectManagerHelper? darkSigil;

		public override void OnEnter()
		{
			base.OnEnter();
			StartAimMode(duration + 1);
			Util.PlaySound(EntityStates.VoidRaidCrab.SpinBeamAttack.enterSoundString, gameObject);
			characterMotor.useGravity = false;
			sphereTransform = FindModelChild("MuzzleCenterFar");
			sigilTransform = FindModelChild("MuzzleCenter");
			spherePosition = sphereTransform.position;
			sigilPosition = sigilTransform.position;
			
			tracerInstance = UnityEngine.Object.Instantiate(Asset.GetGameObject<SobuGekishoha, IEffect>(), spherePosition, Quaternion.identity, null);
			tracerInstance.transform.localScale = new Vector3(1, 1, 0.03f * maxRange);
			
			voidSphereMuzzle = UnityEngine.Object.Instantiate(Asset.GetGameObject<SobuGekishoha.VoidSphere, IEffect>(), spherePosition, Quaternion.identity, sphereTransform);
			voidSphereMuzzle.transform.localRotation = Quaternion.identity;
			voidSphereMuzzle.transform.localScale = Vector3.one;
			//transform.position + Vector3.back * 2f; example
			var additive = characterDirection.forward * 0.5f;
			darkSigilEffect = UnityEngine.Object.Instantiate(Asset.GetGameObject<SobuGekishoha.DarkSigil, IEffect>(), (sigilPosition + additive), Util.QuaternionSafeLookRotation(characterDirection.forward, Vector3.up), sigilTransform);
			
			/*if (sphereTransform)
			{
				voidSphere = EffectManagerKamunagi.GetAndActivatePooledEffect(sphere, sphereTransform, true,
					new EffectData() { rootObject = sphereTransform.gameObject });
				darkSigil = EffectManagerKamunagi.GetAndActivatePooledEffect(sigil, sigilTransform, true,
					new EffectData() { rootObject = sigilTransform.gameObject });
				darkSigil = EffectManagerKamunagi.GetAndActivatePooledEffect(sigil, sphereTransform, true,
					new EffectData() { rootObject = sphereTransform.gameObject });
			}*/

			var component = voidSphereMuzzle.GetComponent<ScaleParticleSystemDuration>();
			if (component)
			{
				component.newDuration = 1;
				for (var i = 0; i < component.particleSystems.Length; i++)
				{
					var p = component.particleSystems[i];
					if (p && i != 2 && i != 3)
					{
						var main = p.main;
						main.duration = duration;
						main.startLifetime = duration;
						var scale = p.sizeOverLifetime;
						scale.enabled = false;
					}
				}
			}

			if (cameraTargetParams)
			{
				request = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
			}
		}

		public override void Update()
		{
			base.Update();
			(characterMotor as IPhysMotor).velocityAuthority = Vector3.zero;
			if (tracerInstance && sphereTransform)
			{
				var aimRay = GetAimRay();
				tracerInstance.transform.position = sphereTransform.position;
				tracerInstance.transform.forward = aimRay.direction;
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.deltaTime;
			if (stopwatch >= fireFrequency)
			{
				stopwatch = 0;
				Fire();
				//Debug.Log($"{bulletCount}");
			}

			var timeUp = fixedAge >= duration;
			if (timeUp || (!inputBank.skill4.down && isAuthority))
			{
				outer.SetNextStateToMain();
			}
		}

		private void Fire()
		{
			if (isAuthority)
			{
				var aimRay = GetAimRay();
				new BulletAttack
				{
					maxDistance = maxRange,
					stopperMask = LayerIndex.noCollision.mask,
					owner = gameObject,
					weapon = gameObject,
					origin = aimRay.origin,
					aimVector = aimRay.direction,
					minSpread = 0,
					maxSpread = 0.4f,
					bulletCount = 1U,
					damage = damageStat * damageCoefficient,
					force = 155,
					tracerEffectPrefab = null,
					muzzleName = twinMuzzle,
					hitEffectPrefab = LoadAsset<GameObject>("RoR2/DLC1/MissileVoid/VoidImpactEffect.prefab"),
					isCrit = RollCrit(),
					radius = 0.2f,
					procCoefficient = 0.3f,
					smartCollision = true,
					damageType = DamageType.Generic
				}.Fire();
			}
		}

		public override void OnExit()
		{
			if (request != null)
			{
				request.Dispose();
			}

			Util.PlaySound(EntityStates.VoidRaidCrab.SpinBeamWindDown.enterSoundString, gameObject);
			characterMotor.useGravity = true;
			if (tracerInstance)
			{
				Destroy(tracerInstance);
			}

			if (voidSphereMuzzle)
			{
				Destroy(voidSphereMuzzle);
			}

			if (darkSigilEffect)
			{
				Destroy(darkSigilEffect);
			}

			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
	}

	public class SobuGekishoha : Asset, ISkill, IEffect
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Special 0";
			skill.skillNameToken = "SPECIAL0_NAME";
			skill.icon = LoadAsset<Sprite>("bundle:SobuGekishoha");
			skill.skillDescriptionToken = "SPECIAL0_DESCRIPTION";
			skill.activationStateMachineName = "Weapon";
			skill.baseMaxStock = 1;
			skill.baseRechargeInterval = 10f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.canceledFromSprinting = false;
			skill.fullRestockOnAssign = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.isCombatSkill = true;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = false;
			return skill;
		}

		Type[] ISkill.GetEntityStates() => new[] { typeof(SobuGekishohaState) };

		GameObject IEffect.BuildObject()
		{
			var tracer =
				LoadAsset<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamVFX.prefab")!.InstantiateClone(
					"VoidTracer", false);
			var particles = tracer.GetComponentsInChildren<ParticleSystemRenderer>();
			particles[particles.Length - 1].transform.localScale = new Vector3(0, 0, 0.25f);
			UnityEngine.Object.Destroy(tracer.GetComponentInChildren<ShakeEmitter>());
			var sofuckingbright = tracer.transform.GetChild(3).gameObject;
			sofuckingbright.SetActive(false);
			var laserC = tracer.transform.GetChild(4).gameObject;
			var rarted = laserC.transform.GetChild(0).gameObject;
			rarted.SetActive(true);
			rarted.GetComponent<PostProcessVolume>().blendDistance = 19f;
			return tracer;
		}

		public class DarkSigil : Asset, IEffect
		{
			GameObject IEffect.BuildObject()
			{
				var effect = LoadAsset<GameObject>("bundle:LaserMuzzle.prefab");
				effect.transform.localScale = Vector3.one * 0.6f;
				return effect;
			}
		}

		public class VoidSphere : Asset, IEffect
		{
			GameObject IEffect.BuildObject()
			{
				var effect =
					LoadAsset<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamChargeUp.prefab")!
						.InstantiateClone("VoidTracerSphere", false);
				effect.GetOrAddComponent<Light>().range = 30f;
				return effect;
			}
		}
	}
}