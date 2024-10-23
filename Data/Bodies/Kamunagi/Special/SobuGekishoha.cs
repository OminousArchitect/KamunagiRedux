using EntityStates;
using HarmonyLib;
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
		private EffectManagerHelper voidSphereMuzzle;
		private EffectManagerHelper darkSigilEffect;
		private EffectManagerHelper tracerInstance;
		private CameraTargetParams.AimRequest request;
		private TemporaryOverlayInstance overlay;
		private CharacterModel charModel;

		public override void OnEnter()
		{
			base.OnEnter();
			StartAimMode(duration + 1);
			Util.PlaySound(EntityStates.VoidRaidCrab.SpinBeamAttack.enterSoundString, gameObject);
			characterMotor.useGravity = false;
			centerFarMuzzle = FindModelChild("MuzzleCenterFar");
			charModel = GetModelTransform().GetComponent<CharacterModel>();

			Vector3 additive = characterDirection.forward * 0.5f;
			darkSigilEffect = EffectManagerKamunagi.GetAndActivatePooledEffect(Asset.GetEffect<DarkSigil>().WaitForCompletion(), centerFarMuzzle, true);
			darkSigilEffect.transform.localScale = Vector3.one * 0.7f;
			tracerInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(Asset.GetEffect<SobuGekishoha>().WaitForCompletion(), centerFarMuzzle, true);
			tracerInstance.transform.localScale = new Vector3(1, 1, 0.03f * 180);
			voidSphereMuzzle = EffectManagerKamunagi.GetAndActivatePooledEffect(Asset.GetEffect<VoidSphere>().WaitForCompletion(), centerFarMuzzle, true);
			voidSphereMuzzle.transform.localRotation = Quaternion.identity;
			voidSphereMuzzle.transform.localScale = Vector3.one;
			
			overlay = TemporaryOverlayManager.AddOverlay(gameObject);
			overlay.originalMaterial = LoadAsset<Material>("RoR2/DLC1/voidstage/matVoidCrystal.mat");
			overlay.AddToCharacterModel(charModel);
			characterBody.AddBuff(Asset.GetAsset<SobuGekishoha>());
			var component = voidSphereMuzzle.GetComponent<ScaleParticleSystemDuration>();
			if (component)
			{
				component.newDuration = 1;
				for (var i = 0; i < component.particleSystems.Length; i++)
				{
					var p = component.particleSystems[i];
					if (!p || i == 2 || i == 3) continue;
					var main = p.main;
					main.duration = duration;
					main.startLifetime = duration;
					var scale = p.sizeOverLifetime;
					scale.enabled = false;
				}
			}

			if (!cameraTargetParams) return;
			request = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
		}

		public override void Update()
		{
			base.Update();
			if (!tracerInstance || !centerFarMuzzle) return;
			var aimRay = GetAimRay();
			tracerInstance.transform.position = centerFarMuzzle.position;
			tracerInstance.transform.forward = aimRay.direction;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.deltaTime;
			(characterMotor as IPhysMotor).velocityAuthority = Vector3.zero;
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
			if (!isAuthority) return;
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

		public override void OnExit()
		{
			if (request != null)
			{
				request.Dispose();
			}

			characterBody.RemoveBuff(Asset.GetAsset<SobuGekishoha>());
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
			
			overlay.RemoveFromCharacterModel();
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
	}

	[HarmonyPatch]
	public class SobuGekishoha : Asset, ISkill, IEffect, IBuff, IOverlay
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Special 0";
			skill.icon = LoadAsset<Sprite>("bundle:SobuGekishoha");
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SPECIAL0_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SPECIAL0_DESCRIPTION";
			skill.activationStateMachineName = "Weapon";
			skill.baseMaxStock = 1;
			skill.baseRechargeInterval = 10f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.isCombatSkill = true;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = false;
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(SobuGekishohaState) };

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
		
		BuffDef IBuff.BuildObject()
		{
			var buffDef = ScriptableObject.CreateInstance<BuffDef>();
			buffDef.name = "TwinsArmorBuff";
			buffDef.buffColor = Colors.twinsDarkColor;
			buffDef.canStack = false;
			buffDef.isDebuff = false;
			buffDef.iconSprite = LoadAsset<Sprite>("RoR2/Junk/Common/texBuffBodyArmorIcon.tif");
			buffDef.isHidden = false;
			return buffDef;
		}

		Material IOverlay.BuildObject()
		{
			var material = new Material(LoadAsset<Material>("RoR2/Base/Brother/maBrotherGlassOverlay.mat"));
			
			return material;
		}

		bool IOverlay.CheckEnabled(CharacterModel model)
		{
			return model.body && model.body.HasBuff(GetAsset<SobuGekishoha, IBuff>());
		} 
		
		[HarmonyPostfix, HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.RecalculateStats))]
		private static void RecalcStats(CharacterBody __instance)
		{
			if (!__instance) return;
			if (__instance.HasBuff(GetAsset<SobuGekishoha>()))
			{
				__instance.armor += 80f;
			}
		}
	}

	public class DarkSigil : Asset, IEffect
	{
		GameObject IEffect.BuildObject()
		{
			var effect = LoadAsset<GameObject>("bundle:LaserMuzzle.prefab");
			effect.transform.localScale = Vector3.one * 0.6f;
			effect.GetOrAddComponent<EffectComponent>().applyScale = false;
			effect.GetOrAddComponent<VFXAttributes>().DoNotPool = false;
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
	
