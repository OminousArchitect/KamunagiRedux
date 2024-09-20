using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Orbs;
using RoR2.Skills;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	internal class DenebokshiriBrimstoneState : BaseTwinState
	{
		private float remapMin = 1.5f;
		private float remapMax = 2.5f;
		private float maxChargeTime = 2f;
		private float damageCoefficient;

		private EffectManagerHelper? chargeEffectInstance;
		private uint soundID;

		public override void OnEnter()
		{
			base.OnEnter();
			var muzzleTransform = FindModelChild("MuzzleCenter");
			var effect = Asset.GetGameObject<DenebokshiriBrimstone, IEffect>();
			if (muzzleTransform)
			{
				chargeEffectInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(effect, muzzleTransform, true,
					new EffectData() { rootObject = muzzleTransform.gameObject });
			}

			soundID = AkSoundEngine.PostEvent("Play_fireballsOnHit_pool_aliveLoop", gameObject);
		}

		private void FireProjectile()
		{
			var prefab = Asset.GetGameObject<DenebokshiriBrimstone, IProjectile>();
			var zapDamage = prefab.GetComponent<ProjectileProximityBeamController>();
			zapDamage.damageCoefficient = damageCoefficient;
			if (isAuthority)
			{
				var aimRay = GetAimRay();
				var fireProjectileInfo = new FireProjectileInfo
				{
					crit = RollCrit(),
					damage = characterBody.damage,
					damageColorIndex = DamageColorIndex.Default,
					force = damageCoefficient * 100,
					owner = gameObject,
					position = aimRay.origin, //aimRay.origin + aimRay.direction * 2,
					procChainMask = default,
					projectilePrefab = prefab,
					rotation = Quaternion.LookRotation(aimRay.direction)
				};
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			damageCoefficient = Util.Remap(fixedAge, 0, maxChargeTime, remapMin, remapMax);

			if (!isAuthority || (fixedAge < maxChargeTime && IsKeyDownAuthority())) return;
			FireProjectile();
			outer.SetNextStateToMain();
		}

		public override void OnExit()
		{
			if (chargeEffectInstance != null)
			{
				chargeEffectInstance.ReturnToPool();
			}

			AkSoundEngine.StopPlayingID(soundID);
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	[HarmonyPatch]
	public class DenebokshiriBrimstone : Asset, IProjectile, IProjectileGhost, IEffect, ISkill
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Secondary 0";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SECONDARY0_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SECONDARY0_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle:firepng");
			skill.activationStateMachineName = "Weapon";
			skill.baseMaxStock = 1;
			skill.baseRechargeInterval = 2f;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			skill.baseMaxStock = 1;
			skill.baseRechargeInterval = 2f;
			skill.beginSkillCooldownOnSkillEnd = false;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			skill.mustKeyPress = true;
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(DenebokshiriBrimstoneState) };

		GameObject IProjectile.BuildObject()
		{
			var hitEffect = GetGameObject<FireHitEffect, IEffect>();

			var proj = GetGameObject<WindBoomerang, IProjectile>()!.InstantiateClone("TwinsMiniSun", true);
			UnityEngine.Object.Destroy(proj.GetComponent<WindBoomerangProjectileBehaviour>());
			UnityEngine.Object.Destroy(proj.GetComponent<BoomerangProjectile>()); //bro what is this spaghetti
			UnityEngine.Object.Destroy(proj.GetComponent<ProjectileOverlapAttack>());
			UnityEngine.Object.Destroy(proj.GetComponent<ProjectileDotZone>());
			var minisunController = proj.GetComponent<ProjectileController>();
			minisunController.ghostPrefab = GetGameObject<DenebokshiriBrimstone, IProjectileGhost>();
			minisunController.flightSoundLoop = null;
			minisunController.startSound = "Play_fireballsOnHit_impact";
			proj.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Denebokshiri);
			var minisunSimple = proj.AddComponent<ProjectileSimple>();
			minisunSimple.desiredForwardSpeed = 20f;
			minisunSimple.lifetime = 5f;
			minisunSimple.lifetimeExpiredEffect = hitEffect;
			var singleI = proj.AddComponent<ProjectileSingleTargetImpact>();
			singleI.impactEffect = hitEffect;
			singleI.destroyOnWorld = true;
			var proxBeam = proj.AddComponent<ProjectileProximityBeamController>();
			proxBeam.attackFireCount = 1;
			proxBeam.attackInterval = 0.15f;
			proxBeam.listClearInterval = 1;
			proxBeam.attackRange = 15f; //radius
			proxBeam.minAngleFilter = 0;
			proxBeam.maxAngleFilter = 180;
			proxBeam.procCoefficient = 0.9f;
			proxBeam.damageCoefficient = 0.1f;
			proxBeam.bounces = 0;
			proxBeam.lightningType = (LightningOrb.LightningType)204957; // some random constant bullshit
			proxBeam.inheritDamageType = true;
			return proj;
		}

		GameObject IProjectileGhost.BuildObject()
		{
			var ghost = LoadAsset<GameObject>("RoR2/Base/Grandparent/ChannelGrandParentSunHands.prefab")!
				.InstantiateClone("TwinsChargeMiniSun", false);
			var minisunMesh = ghost.GetComponentInChildren<MeshRenderer>(true);
			minisunMesh.gameObject.SetActive(true);
			minisunMesh.material.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/Base/FireballsOnHit/texFireballsOnHitIcon.png"));
			minisunMesh.material.SetFloat("_AlphaBoost", 6.351971f);
			ghost.AddComponent<ProjectileGhostController>();
			ghost.AddComponent<MeshFilter>().mesh = LoadAsset<Mesh>("RoR2/Base/Common/VFX/mdlVFXIcosphere.fbx");
			var miniSunIndicator = PrefabAPI.InstantiateClone(
				LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphere.prefab").transform.GetChild(0)
					.gameObject, "MiniSunIndicator", false);
			miniSunIndicator.transform.parent = ghost.transform; //this was the first time I figured this out
			miniSunIndicator.transform.localPosition = Vector3.zero;
			miniSunIndicator.transform.localScale = Vector3.one * 25f;

			var gravSphere = PrefabAPI.InstantiateClone(
				LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphereGhost.prefab").transform.GetChild(0)
					.gameObject, "Indicator", false);
			var gooDrops = PrefabAPI.InstantiateClone(gravSphere.transform.GetChild(3).gameObject, "MiniSunGoo", false);
			gooDrops.transform.parent = ghost.transform; // adding the indicator sphere to DenebokshiriBrimstone
			gooDrops.transform.localPosition = Vector3.zero;
			return ghost;
		}

		GameObject IEffect.BuildObject()
		{
			var effect =
				LoadAsset<GameObject>("RoR2/Base/Grandparent/ChannelGrandParentSunHands.prefab")!.InstantiateClone(
					"TwinsChargeMiniSun", false);
			var scale = effect.AddComponent<ObjectScaleCurve>();
			effect.transform.localScale = Vector3.one * 0.35f;
			scale.timeMax = 1f;
			scale.useOverallCurveOnly = true;
			scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
			effect.transform.GetChild(1).gameObject.SetActive(true);
			return effect;
		}

		public static DamageAPI.ModdedDamageType Denebokshiri;

		public override void Initialize()
		{
			Denebokshiri = DamageAPI.ReserveDamageType();
		}
		[HarmonyPrefix, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamageProcess))]
		private static void TakeDamageProcess(HealthComponent __instance, DamageInfo damageInfo)
		{
			if (damageInfo.HasModdedDamageType(Denebokshiri))
			{
				DotController.InflictDot(
					__instance.gameObject,
					damageInfo.attacker,
					DotController.DotIndex.StrongerBurn,
					2f,
					damageInfo.damage * 0.2f
				);
			}
		}
	}

	public class FireHitEffect : Asset, IEffect
	{
		GameObject IEffect.BuildObject()
		{
			var effect =
				LoadAsset<GameObject>("addressable:RoR2/Base/Merc/MercExposeConsumeEffect.prefab")!.InstantiateClone(
					"TwinsFireHitEffect", false);
			UnityEngine.Object.Destroy(effect.GetComponent<OmniEffect>());
			foreach (var r in effect.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				r.gameObject.SetActive(true);
				r.material.SetColor("_TintColor", new Color(0.7264151f, 0.1280128f, 0f));
				if (r.name == "PulseEffect, Ring (1)")
				{
					var mat = r.material;
					mat.mainTexture = LoadAsset<Texture2D>("RoR2/Base/Common/VFX/texArcaneCircleProviMask.png");
				}
			}

			effect.EffectWithSound("Play_item_use_molotov_throw");
			return effect;
		}
	}

	[HarmonyPatch]
	public class LightningEffect : Asset, IEffect
	{
		[HarmonyILManipulator]
		[HarmonyPatch(typeof(LightningOrb), nameof(LightningOrb.Begin))]
		public static void ReplaceEffect(ILContext il)
		{
			var c = new ILCursor(il);
			if (!c.TryGotoNext(x =>
				    x.MatchCallOrCallvirt(typeof(OrbStorageUtility), nameof(OrbStorageUtility.Get)))) return;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<string, LightningOrb, string>>((path, orb) =>
				orb.lightningType == (LightningOrb.LightningType)204957
					? "NINES_LIGHTNING_ORB"
					: path);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(OrbStorageUtility), nameof(OrbStorageUtility.Get))]
		public static bool ReplaceEffect(string path, ref GameObject __result)
		{
			if (path == "NINES_LIGHTNING_ORB")
			{
				__result = GetGameObject<LightningEffect, IEffect>();
				return false;
			}

			return true;
		}

		public GameObject BuildObject()
		{
			var effect =
				LoadAsset<GameObject>("legacy:Prefabs/Effects/OrbEffects/MageLightningOrbEffect")!.InstantiateClone(
					"BrimstoneLightning", false);
			effect.GetComponent<OrbEffect>().endEffect = GetGameObject<LightningEndEffect, IEffect>();
			effect.GetComponentInChildren<LineRenderer>().material =
				LoadAsset<Material>("RoR2/Base/Common/VFX/mageMageFireStarburst.mat");
			return effect;
		}
	}

	public class LightningEndEffect : Asset, IEffect
	{
		public GameObject BuildObject()
		{
			var effect =
				LoadAsset<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab")!.InstantiateClone(
					"LightningImpactEffect", false);
			return effect;
		}
	}
}