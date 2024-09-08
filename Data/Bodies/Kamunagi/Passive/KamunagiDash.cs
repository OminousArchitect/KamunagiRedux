using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using Rewired.UI.ControlMapper;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Passive
{
	public class KamunagiChannelDashState : KamunagiHoverState
	{
		public float duration = 0.8f;
		private Vector3 origin;
		protected float ascendSpeedMult;
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
		}

		private void FireVacuum()
		{
			var fireProjectileInfo = new FireProjectileInfo
			{
				crit = false,
				damage = this.characterBody.damage * 0.5f,
				damageTypeOverride = DamageType.Generic,
				damageColorIndex = DamageColorIndex.Void,
				force = 10,
				owner = base.gameObject,
				position = thePosition,
				procChainMask = default(RoR2.ProcChainMask),
				projectilePrefab = Asset.GetGameObject<KamunagiDash, IProjectile>(),
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

			if (!isAuthority) return;
			characterMotor.Motor.SetPosition(this.origin);
			(characterMotor as IPhysMotor).velocityAuthority = Vector3.zero;

			if (fixedAge < duration && inputBank.jump.down) return;
			characterBody.isSprinting = true;
			ascendSpeedMult = Util.Remap(fixedAge, 0, duration, 0.4f, 1.5f);

			outer.SetNextState(new KamunagiDashState { flyRay = GetAimRay(), speedMult = ascendSpeedMult, effectPosition = thePosition });
		}

		public override void OnExit()
		{
			log.LogDebug("exiting channeldash");
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	class KamunagiDashState : BaseTwinState
	{
		public override int meterGain => 0;
		public float speedMult;
		public Ray flyRay;
		private Vector3 flyVector;
		public Vector3 effectPosition;
		public Vector3 rayDir;
		private float duration = 1.4f;
		public static AnimationCurve flyCurve = EntityStates.Mage.FlyUpState.speedCoefficientCurve;

		public override void OnEnter()
		{
			base.OnEnter();
			Debug.Log($"{speedMult} remapped");
			PlayAnimation("Saraana", "FlyUp");
			PlayAnimation("Ururuu", "FlyUp");
			CreateBlinkEffect(effectPosition);
			EffectManager.SimpleMuzzleFlash(Asset.GetGameObject<FlyEffect, IEffect>(), base.gameObject, "MuzzleRight",
				false);
			EffectManager.SimpleMuzzleFlash(Asset.GetGameObject<FlyEffect, IEffect>(), base.gameObject, "MuzzleLeft",
				false);
			if (!isAuthority) return;
			characterMotor.Motor.ForceUnground();
			log.LogDebug("dash is entering");
		}

		public override void OnExit()
		{
			base.OnExit();
			log.LogDebug("dash exiting" + new StackTrace());
		}

		private void CreateBlinkEffect(Vector3 origin)
		{
			var effectData = new EffectData
			{
				rotation = Util.QuaternionSafeLookRotation(flyVector), 
				origin = origin
			};
			EffectManager.SpawnEffect(LoadAsset<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerCaptureCharge.prefab"), effectData, false);
			Util.PlaySound("Play_voidJailer_m2_shoot", gameObject);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;
			if (fixedAge >= duration)
			{
				outer.SetNextStateToMain();
				return;
			}

			flyVector = inputBank.interact.wasDown ? -flyRay.direction * speedMult : flyRay.direction * speedMult;

			log.LogDebug("flyVector: " + flyVector);
			characterMotor.rootMotion +=
				flyVector * (moveSpeedStat * flyCurve.Evaluate(fixedAge / duration) * Time.deltaTime);

			log.LogDebug("rootMotion: " + characterMotor.rootMotion);


			var motor = characterMotor as IPhysMotor;
			var motorVelocityAuthority = motor.velocityAuthority;
			motorVelocityAuthority.y = 0f;
			motor.velocityAuthority = motorVelocityAuthority;

			log.LogDebug("Velo:" + motor.velocityAuthority);
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
	}

	public class KamunagiDash : Asset, ISkill, IProjectile, IProjectileGhost
	{
		public SkillDef BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Hover";
			skill.baseRechargeInterval = 5f;
			skill.cancelSprintingOnActivation = false;
			return skill;
		}

		public Type[] GetEntityStates() => new[] { typeof(KamunagiChannelDashState), typeof(KamunagiDashState) };

		GameObject IProjectile.BuildObject()
		{
			var proj =
				LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphere.prefab")!.InstantiateClone(
					"TwinsVacuumSphere", true);
			var vacuumSimple = proj.GetComponent<ProjectileSimple>();
			vacuumSimple.desiredForwardSpeed = 0f;
			vacuumSimple.lifetime = 1f;
			proj.GetComponent<TetherVfxOrigin>().tetherPrefab = GetGameObject<RequiredTetherVFX, IEffect>();
			UnityEngine.Object.Destroy(proj.transform.GetChild(0).gameObject);
			proj.GetComponent<ProjectileController>().ghostPrefab = GetGameObject<KamunagiDash, IProjectileGhost>();
			return proj;
		}

		GameObject IProjectileGhost.BuildObject()
		{
			var sparks = PrefabAPI.InstantiateClone(
				LoadAsset<GameObject>("RoR2/Base/Blackhole/GravSphere.prefab")!.transform.GetChild(1).gameObject,
				"Sparks, Blue", false);
			var altP = sparks.GetComponent<ParticleSystem>();
			var altPMain = altP.main;
			altPMain.simulationSpeed = 2f;
			altPMain.startColor = Colors.twinsLightColor;
			var ascP = sparks.GetComponent<ParticleSystem>();
			var sparkEmit = ascP.emission;
			sparkEmit.rate = new ParticleSystem.MinMaxCurve(120f); //minmaxcurve example
			sparks.GetComponent<ParticleSystemRenderer>().material =
				LoadAsset<Material>("RoR2/Base/Common/VFX/matTracerBrightTransparent.mat");
			sparks.transform.localScale = Vector3.one * 0.25f;

			var ghost = PrefabAPI.InstantiateClone(
				LoadAsset<GameObject>("RoR2/Base/Blackhole/GravSphere.prefab").transform.GetChild(2).gameObject,
				"TwinsVacuumGhost", false);
			sparks.transform.SetParent(ghost.transform);
			var innerSphere = ghost.transform.GetChild(0).gameObject;
			innerSphere.GetComponent<MeshRenderer>().material =
				LoadAsset<Material>("RoR2/Base/Nullifier/matNullifierGemPortal.mat");
			ghost.AddComponent<ProjectileGhostController>();
			return ghost;
		}
	}

	public class RequiredTetherVFX : Asset, IEffect
	{
		public GameObject BuildObject()
		{
			var mandatory =
				LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphereTether.prefab")!.InstantiateClone(
					"ImTooLazyToDoThis", false);
			mandatory.GetComponent<LineRenderer>().enabled = false;
			return mandatory;
		}
	}

	public class FlyEffect : Asset, IEffect
	{
		public GameObject BuildObject()
		{
			var effect =
				LoadAsset<GameObject>("RoR2/Base/Mage/MuzzleflashMageLightningLargeWithTrail.prefab")!.InstantiateClone(
					"TwinsFlyUpEffect", false);
			foreach (ParticleSystemRenderer r in effect.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				var name = r.name;

				if (name == "Matrix, Billboard")
				{
					r.material.SetTexture("_RemapTex",
						LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
				}

				if (name == "Matrix, Mesh")
				{
					r.material.SetTexture("_RemapTex",
						LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
				}
			}

			foreach (ParticleSystem p in effect.GetComponentsInChildren<ParticleSystem>(true))
			{
				var name = p.name;
				var main = p.main;

				if (name == "Flash")
				{
					main.startColor = Colors.twinsLightColor;
				}
			}

			effect.GetComponentInChildren<Light>().color = Colors.twinsLightColor;
			var trailRenderer = effect.GetComponentInChildren<TrailRenderer>();
			trailRenderer.materials[0].SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
			return effect;
		}
	}
}