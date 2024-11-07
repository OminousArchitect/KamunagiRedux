using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.Special;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Orbs;
using RoR2.Skills;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class DenebokshiriBrimstoneFireState : BaseState
	{
		public Ray aimRay;
		public float damageCoefficient;

		public override void OnEnter()
		{
			base.OnEnter();
			var prefab = Concentric.GetProjectile<DenebokshiriBrimstone>().WaitForCompletion();
			var zapDamage = prefab.GetComponent<ProjectileProximityBeamController>();
			zapDamage.damageCoefficient = damageCoefficient;
			if (!NetworkServer.active) return;
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

		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write(aimRay);
			writer.Write(damageCoefficient);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			aimRay = reader.ReadRay();
			damageCoefficient = reader.ReadSingle();
		}
	}
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
			var effect = Concentric.GetEffect<DenebokshiriBrimstone>().WaitForCompletion();
			if (muzzleTransform)
			{
				chargeEffectInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(effect, muzzleTransform, true,
					new EffectData() { rootObject = muzzleTransform.gameObject });
			}

			soundID = AkSoundEngine.PostEvent("Play_fireballsOnHit_pool_aliveLoop", gameObject);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			damageCoefficient = Util.Remap(fixedAge, 0, maxChargeTime, remapMin, remapMax);

			if (!isAuthority || (fixedAge < maxChargeTime && IsKeyDownAuthority())) return;
			outer.SetNextState(new DenebokshiriBrimstoneFireState() {aimRay = GetAimRay(), damageCoefficient = damageCoefficient});
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
	public class DenebokshiriBrimstone : Concentric, IProjectile, IProjectileGhost, IEffect, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Secondary 0";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SECONDARY0_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SECONDARY0_DESCRIPTION";
			skill.icon = await LoadAsset<Sprite>("bundle:firepng");
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

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(DenebokshiriBrimstoneState), typeof(DenebokshiriBrimstoneFireState) };

		async Task<GameObject> IProjectile.BuildObject()
		{
			var hitEffect = await GetEffect<FireHitEffect>();

			var proj = (await GetProjectile<YamatoWinds>())!.InstantiateClone("TwinsMiniSun", true);
			UnityEngine.Object.Destroy(proj.GetComponent<WindBoomerangProjectileBehaviour>());
			UnityEngine.Object.Destroy(proj.GetComponent<BoomerangProjectile>()); //bro what is this spaghetti
			UnityEngine.Object.Destroy(proj.GetComponent<ProjectileOverlapAttack>());
			UnityEngine.Object.Destroy(proj.GetComponent<ProjectileDotZone>());
			var minisunController = proj.GetComponent<ProjectileController>();
			minisunController.ghostPrefab = await this.GetProjectileGhost();
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
			proxBeam.damageCoefficient = 0.05f;
			proxBeam.bounces = 0;
			proxBeam.lightningType = (LightningOrb.LightningType)204957; // some random constant bullshit
			proxBeam.inheritDamageType = true;
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await LoadAsset<GameObject>("RoR2/Base/Grandparent/ChannelGrandParentSunHands.prefab")!
				).InstantiateClone("TwinsChargeMiniSun", false);
			var minisunMesh = ghost.GetComponentInChildren<MeshRenderer>(true);
			minisunMesh.gameObject.SetActive(true);
			minisunMesh.material.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/FireballsOnHit/texFireballsOnHitIcon.png"));
			minisunMesh.material.SetFloat("_AlphaBoost", 6.351971f);
			ghost.AddComponent<ProjectileGhostController>();
			ghost.AddComponent<MeshFilter>().mesh = await LoadAsset<Mesh>("RoR2/Base/Common/VFX/mdlVFXIcosphere.fbx");
			var miniSunIndicator = (await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphere.prefab")).transform.GetChild(0)
				.gameObject.InstantiateClone("MiniSunIndicator", false);
			miniSunIndicator.transform.parent = ghost.transform; //this was the first time I figured this out
			miniSunIndicator.transform.localPosition = Vector3.zero;
			miniSunIndicator.transform.localScale = Vector3.one * 25f;

			var gravSphere = (await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphereGhost.prefab")).transform.GetChild(0)
				.gameObject.InstantiateClone("Indicator", false);
			var gooDrops = gravSphere.transform.GetChild(3).gameObject.InstantiateClone("MiniSunGoo", false);
			gooDrops.transform.parent = ghost.transform; // adding the indicator sphere to DenebokshiriBrimstone
			gooDrops.transform.localPosition = Vector3.zero;
			return ghost;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect =
				(await LoadAsset<GameObject>("RoR2/Base/Grandparent/ChannelGrandParentSunHands.prefab"))!.InstantiateClone(
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

		public override Task Initialize()
		{
			Denebokshiri = DamageAPI.ReserveDamageType();
			return base.Initialize();
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
					3.5f,
					damageInfo.damage * 0.1f
				);
			}
		}
	}

	public class FireHitEffect : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect =
				(await LoadAsset<GameObject>("addressable:RoR2/Base/Merc/MercExposeConsumeEffect.prefab"))!.InstantiateClone(
					"TwinsFireHitEffect", false);
			UnityEngine.Object.Destroy(effect.GetComponent<OmniEffect>());
			foreach (var r in effect.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				r.gameObject.SetActive(true);
				r.material.SetColor("_TintColor", new Color(0.7264151f, 0.1280128f, 0f));
				if (r.name == "PulseEffect, Ring (1)")
				{
					var mat = r.material;
					mat.mainTexture = await LoadAsset<Texture2D>("RoR2/Base/Common/VFX/texArcaneCircleProviMask.png");
				}
			}

			effect.EffectWithSound("Play_item_use_molotov_throw");
			return effect;
		}
	}

	[HarmonyPatch]
	public class LightningEffect : Concentric, IEffect
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
				__result = GetEffect<LightningEffect>().WaitForCompletion();
				return false;
			}

			return true;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect =
				(await LoadAsset<GameObject>("legacy:Prefabs/Effects/OrbEffects/MageLightningOrbEffect"))!.InstantiateClone(
					"BrimstoneLightning", false);
			effect.GetComponent<OrbEffect>().endEffect = await GetEffect<LightningEndEffect>();
			effect.GetComponentInChildren<LineRenderer>().material =
				await LoadAsset<Material>("RoR2/Base/Common/VFX/mageMageFireStarburst.mat");
			return effect;
		}
	}

	public class LightningEndEffect : Concentric, IEffect
	{
		public async Task<GameObject> BuildObject()
		{
			var effect =
				(await LoadAsset<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab"))!.InstantiateClone(
					"LightningImpactEffect", false);
			return effect;
		}
	}
}