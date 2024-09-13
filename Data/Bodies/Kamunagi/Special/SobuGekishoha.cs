﻿using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Special
{
	public class SobuGekishohaState : BaseTwinState
	{
		private float duration = 5;
		private float stopwatch;
		private float damageCoefficient = 3;
		private Transform centerFarMuzzle;
		private Transform centerMuzzle;
		private EffectManagerHelper voidSphereMuzzle;
		private EffectManagerHelper darkSigilEffect;
		private EffectManagerHelper tracerInstance;
		private CameraTargetParams.AimRequest request;

		public override void OnEnter()
		{
			base.OnEnter();
			StartAimMode(duration + 1);
			Util.PlaySound(EntityStates.VoidRaidCrab.SpinBeamAttack.enterSoundString, gameObject);
			characterMotor.useGravity = false;
			centerFarMuzzle = FindModelChild("MuzzleCenterFar");
			centerMuzzle = FindModelChild("MuzzleCenter");
			Vector3 additive = characterDirection.forward * 0.5f;
			Vector3 vectorMath = centerMuzzle.position + additive;
			darkSigilEffect = EffectManager.GetAndActivatePooledEffect(Asset.GetGameObject<SobuGekishoha.DarkSigil, IEffect>(), centerFarMuzzle, true);
			tracerInstance = EffectManager.GetAndActivatePooledEffect(Asset.GetGameObject<SobuGekishoha, IEffect>(), centerFarMuzzle, true);
			tracerInstance.transform.localScale = new Vector3(1, 1, 0.03f * 180);
			voidSphereMuzzle = EffectManager.GetAndActivatePooledEffect(Asset.GetGameObject<SobuGekishoha.VoidSphere, IEffect>(), centerFarMuzzle, true);
			voidSphereMuzzle.transform.localRotation = Quaternion.identity;
			voidSphereMuzzle.transform.localScale = Vector3.one;
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
			if (tracerInstance && centerFarMuzzle)
			{
				var aimRay = GetAimRay();
				tracerInstance.transform.position = centerFarMuzzle.position;
				tracerInstance.transform.forward = aimRay.direction;
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.deltaTime;
			if (stopwatch >= 0.15f)
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
					maxDistance = 180f,
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
					procCoefficient = 0.35f,
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
				tracerInstance.ReturnToPool();
			}

			if (voidSphereMuzzle)
			{
				voidSphereMuzzle.ReturnToPool();
			}

			if (darkSigilEffect)
			{
				darkSigilEffect.ReturnToPool();
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
			var tracer = LoadAsset<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamVFX.prefab")!.InstantiateClone("VoidTracer", false);
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