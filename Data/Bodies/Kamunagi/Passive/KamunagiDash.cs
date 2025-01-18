using BepInEx.Configuration;
using EntityStates;
using EntityStates.Mage;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Special;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Passive
{
	public class KamunagiChannelDashState : BaseTwinState
	{
		
		public float duration = 0.6f;
		private Vector3 origin;
		protected float ascendSpeedMult;
		private Vector3 thePosition;
		public static GameObject projectilePrefab;
		private TwinBehaviour behaviour;

		public override void OnEnter()
		{
			base.OnEnter();
			//log.LogDebug("entering channeldash");
			behaviour = characterBody.GetComponent<TwinBehaviour>();
			duration = behaviour.chargeDuration;
			origin = this.transform.position;
			thePosition = characterBody.footPosition + Vector3.up * 0.15f;
			characterBody.SetBuffCount(Concentric.GetBuffIndex<SobuGekishoha>().WaitForCompletion(), 1);
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
				damageTypeOverride = DamageTypeCombo.GenericPrimary,
				damageColorIndex = DamageColorIndex.Void,
				force = 10,
				owner = base.gameObject,
				position = thePosition,
				procChainMask = default(RoR2.ProcChainMask),
				projectilePrefab = projectilePrefab,
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

			if (fixedAge < duration && inputBank.interact.down) return;
			characterBody.isSprinting = true;
			ascendSpeedMult = Util.Remap(fixedAge, 0, duration, behaviour.minDistance, behaviour.maxDistance);

			outer.SetNextState(new KamunagiDashState
			{
				flyRay = GetAimRay(), speedMult = ascendSpeedMult, effectPosition = thePosition
			});
		}

		public override void OnExit()
		{
			//log.LogDebug("exiting channeldash");
			characterBody.SetBuffCount(Concentric.GetBuffIndex<SobuGekishoha>().WaitForCompletion(), 0);
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
		
		public override int meterGain => 0;
	}

	class KamunagiDashState : BaseTwinState
	{
		public override int meterGain => 0;
		public float speedMult;
		public Ray flyRay;
		private Vector3 flyVector;
		public Vector3 effectPosition;
		public Vector3 rayDir;
		private float duration = 1.3f;
		public static AnimationCurve flyCurve;
		public bool wasJumpDown;
		public static GameObject blinkPrefab;
		public static GameObject muzzlePrefab;

		public override void OnEnter()
		{
			base.OnEnter();
			//Debug.Log($"{speedMult} remapped");
			duration = twinBehaviour.flyDuration;
			
			PlayAnimation("Saraana", "FlyUp");
			PlayAnimation("Ururuu", "FlyUp");
			CreateBlinkEffect(effectPosition);
			EffectManager.SimpleMuzzleFlash(muzzlePrefab, base.gameObject, "MuzzleRight", false);
			EffectManager.SimpleMuzzleFlash(muzzlePrefab, base.gameObject, "MuzzleLeft", false);
			if (!isAuthority) return;
			characterMotor.Motor.ForceUnground();
			characterBody.AddTimedBuffAuthority(Concentric.GetBuffIndex<SobuGekishoha>().WaitForCompletion(), 0.5f);
			//log.LogDebug("dash is entering");
		}

		public override void OnExit()
		{
			base.OnExit();
			//log.LogDebug("dash exiting" + new StackTrace());
		}

		private void CreateBlinkEffect(Vector3 origin)
		{
			var effectData = new EffectData { rotation = Util.QuaternionSafeLookRotation(flyVector), origin = origin };
			EffectManager.SpawnEffect(blinkPrefab, effectData, false);
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

			if (KamunagiDash.fixedDashDir.Value)
			{
				flyRay = GetAimRay();
			}
			
			characterBody.isSprinting = true; //magic
			flyVector = flyRay.direction * speedMult;

			//log.LogDebug("flyVector: " + flyVector);
			characterMotor.rootMotion += flyVector * (moveSpeedStat * flyCurve.Evaluate(fixedAge / duration) * Time.deltaTime);

			//log.LogDebug("rootMotion: " + characterMotor.rootMotion);

			var motor = characterMotor as IPhysMotor;
			var motorVelocityAuthority = motor.velocityAuthority;
			motorVelocityAuthority.y = 0f;
			motor.velocityAuthority = motorVelocityAuthority;

			//log.LogDebug("Velo:" + motor.velocityAuthority);
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
	}

	public class KamunagiDash : Concentric, ISkill, IProjectile, IProjectileGhost, IEffect
	{
		public static ConfigEntry<bool> fixedDashDir = instance.Config.Bind("General", "Variable Boost Direction", false, "False: Proxy Apotheosis goes in a fixed direction just like Artificer Ion Surge. True: Proxy Apotheosis goes where you're looking, allowing you to dash around corners and such. Defaults to false.");
		public override async Task Initialize()
		{
			await base.Initialize();
			var curve = await LoadAsset<EntityStateConfiguration>(
				"RoR2/Base/Mage/EntityStates.Mage.FlyUpState.asset");
			var field = typeof(KamunagiDashState).GetField(nameof(KamunagiDashState.flyCurve)); 
			KamunagiDashState.flyCurve =
				// ReSharper disable once SuspiciousTypeConversion.Global
				(AnimationCurve) curve.serializedFieldsCollection.GetOrCreateField(nameof(FlyUpState.speedCoefficientCurve)).fieldValue.GetValue(field);
			KamunagiDashState.blinkPrefab = await this.GetEffect();
			KamunagiDashState.muzzlePrefab = await GetEffect<FlyEffect>();
			KamunagiChannelDashState.projectilePrefab = await this.GetProjectile();
		}

		async Task<GameObject> IEffect.BuildObject()
		{	//await LoadAsset<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerCaptureCharge.prefab");
			var variable = (await LoadAsset<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerCaptureCharge.prefab"))!.InstantiateClone("TwinsVortex", false);
			var vtex = variable.transform.Find("GrowingSphere").gameObject;
			ParticleSystemRenderer r = vtex.GetComponent<ParticleSystemRenderer>();
			r.material = new Material(r.material);
			r.material.SetColor("_Tint", new Color(0.074f, 0f, 1f));
			return variable;
		}
		
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Hover";
			skill.icon = (await LoadAsset<Sprite>("kamunagiassets2:DarkAscension"));
			skill.baseRechargeInterval = 5f;
			skill.cancelSprintingOnActivation = false;
			skill.skillName = "Other 1";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "OTHERPASSIVE_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "OTHERPASSIVE_DESCRIPTION";
			return skill;
		}

		public IEnumerable<Type> GetEntityStates() =>
			new[] { typeof(KamunagiChannelDashState), typeof(KamunagiDashState) };

		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphere.prefab"))!.InstantiateClone("TwinsVacuumSphere", true);
			var vacuumSimple = proj.GetComponent<ProjectileSimple>();
			vacuumSimple.desiredForwardSpeed = 0f;
			vacuumSimple.lifetime = 1f;
			UnityEngine.Object.Destroy(proj.GetComponent<TetherVfxOrigin>());
			UnityEngine.Object.Destroy(proj.transform.GetChild(0).gameObject);
			proj.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			proj.GetComponent<RadialForce>().radius = 30f;
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var sparks = (await LoadAsset<GameObject>("RoR2/Base/Blackhole/GravSphere.prefab"))!.transform.GetChild(1)
				.gameObject.InstantiateClone("Sparks, Blue", false);
			var altP = sparks.GetComponent<ParticleSystem>();
			var altPMain = altP.main;
			altPMain.simulationSpeed = 2f;
			altPMain.startColor = Colors.twinsLightColor;
			var ascP = sparks.GetComponent<ParticleSystem>();
			var sparkEmit = ascP.emission;
			sparkEmit.rate = new ParticleSystem.MinMaxCurve(120f); //minmaxcurve example
			sparks.GetComponent<ParticleSystemRenderer>().material =
				await LoadAsset<Material>("RoR2/Base/Common/VFX/matTracerBrightTransparent.mat");
			sparks.transform.localScale = Vector3.one * 0.25f;

			var ghost = PrefabAPI.InstantiateClone(
				(await LoadAsset<GameObject>("RoR2/Base/Blackhole/GravSphere.prefab")).transform.GetChild(2).gameObject,
				"TwinsVacuumGhost", false);
			sparks.transform.SetParent(ghost.transform);
			var innerSphere = ghost.transform.GetChild(0).gameObject;
			innerSphere.GetComponent<MeshRenderer>().material =
				await LoadAsset<Material>("RoR2/Base/Nullifier/matNullifierGemPortal.mat");
			ghost.AddComponent<ProjectileGhostController>();
			return ghost;
		}
	}

	public class TetherVFX : Concentric, IGenericObject
	{
		async Task<GameObject> IGenericObject.BuildObject()
		{
			Material vacuum = new Material(await LoadAsset<Material>("RoR2/Base/Grandparent/matGrandParentSunChannelStartBeam.mat"));
			//vacuum.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampHippoVoidEye.png"));
			vacuum.SetFloat("_Boost", 18f);
			vacuum.SetFloat("_AlphaBoost", 2.8f);
			vacuum.SetColor("_TintColor", new Color32(105, 0, 229, 255));
			
			var tetherLine = (await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphereTether.prefab"))!.InstantiateClone("TwinsTether", false);
			tetherLine.GetComponent<LineRenderer>().materials = new[] { vacuum };
			return tetherLine;
		}
	}

	public class FlyEffect : Concentric, IEffect
	{
		public async Task<GameObject> BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("RoR2/Base/Mage/MuzzleflashMageLightningLargeWithTrail.prefab"))!
				.InstantiateClone(
					"TwinsFlyUpEffect", false);
			foreach (ParticleSystemRenderer r in effect.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				var name = r.name;

				if (name == "Matrix, Billboard")
				{
					r.material.SetTexture("_RemapTex",
						await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
				}

				if (name == "Matrix, Mesh")
				{
					r.material.SetTexture("_RemapTex",
						await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
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
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
			return effect;
		}
	}
}