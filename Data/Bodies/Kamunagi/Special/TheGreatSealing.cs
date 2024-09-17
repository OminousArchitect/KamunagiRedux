using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Special
{
	public class TheGreatSealingState : IndicatorSpellState
	{
		public EffectManagerHelper? chargeEffectInstance;
		public override int meterGain => 0;
		public override float duration => 10f;
		public override float failedCastCooldown => 1f;

		public override void Fire(Vector3 targetPosition)
		{
			base.Fire(targetPosition);

			ProjectileManager.instance.FireProjectile(Asset.GetGameObject<PrimedObelisk, IProjectile>(),
				targetPosition,
				Quaternion.identity,
				gameObject,
				damageStat,
				1f,
				RollCrit()
			);
		}

		public override void OnEnter()
		{
			base.OnEnter();
			if (isAuthority) characterMotor.useGravity = false;
			var muzzleTransform = FindModelChild("MuzzleCenter");
			if (!muzzleTransform || !Asset.TryGetGameObject<TheGreatSealing, IEffect>(out var muzzleEffect)) return;
			chargeEffectInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(muzzleEffect, muzzleTransform,
				true, new EffectData() { rootObject = muzzleTransform.gameObject, scale = 0.0625f });
		}

		public override void Update()
		{
			base.Update();
			if (!isAuthority) return;
			((IPhysMotor)characterMotor).velocityAuthority = Vector3.zero;
		}

		public override void OnExit()
		{
			base.OnExit();
			if (chargeEffectInstance != null) chargeEffectInstance.ReturnToPool();
			if (isAuthority) characterMotor.useGravity = true;
		}
	}
	
	public class TheGreatSealing : Asset, ISkill, IEffect
	{
		public static Material[] onKamiMats;

		static TheGreatSealing()
		{
			var onkami1 =
				new Material(LoadAsset<Material>("addressable:RoR2/Base/artifactworld/matArtifactPortalCenter.mat"));
			onkami1.SetFloat("_AlphaBoost", 1.3f);
			onkami1.SetColor("_TintColor", new Color(0f, 0.1843f, 1f));
			;
			var onkami2 =
				new Material(LoadAsset<Material>("addressable:RoR2/Base/artifactworld/matArtifactPortalEdge.mat"));
			onkami2.SetColor("_TintColor", new Color(0f, 0.1843f, 1f));
			onkami2.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonArenaWall.png"));
			onkami2.SetFloat("_BrightnessBoost", 4.67f);
			onkami2.SetFloat("_AlphaBoost", 1.2f);
			onKamiMats = new[] { onkami1, onkami2 };
		}

		GameObject IEffect.BuildObject()
		{
			var effect =
				LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonGhost.prefab")!
					.InstantiateClone("AntimatterMuzzleEffect", false);

			var comp = effect.GetOrAddComponent<EffectComponent>();
			comp.applyScale = true;
			comp.parentToReferencedTransform = true;
			comp.positionAtReferencedTransform = true;
			comp.effectData = new EffectData() { };
			effect.SetActive(false); // Required for pooled effects or you get a warning about effectData not being set
			var vfx = effect.GetOrAddComponent<VFXAttributes>();
			vfx.DoNotPool = false;
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;

			Object.Destroy(effect.GetComponent<ProjectileGhostController>());
			var scaler = effect.transform.GetChild(0).gameObject;
			var blackSphere = scaler.transform.GetChild(1).gameObject;
			var (emissionMat, (rampMat, _)) = blackSphere.GetComponent<MeshRenderer>().materials;
			emissionMat.SetTexture("_Emission",
				LoadAsset<Texture2D>("addressable:RoR2/Base/ElectricWorm/ElectricWormBody.png"));
			rampMat.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampLunarElectric.png"));
			scaler.GetComponentInChildren<Light>().range = 20f;
			Object.Destroy(scaler.transform.GetChild(3).gameObject);
			return effect;
		}

		Type[] ISkill.GetEntityStates() => new[] { typeof(TheGreatSealingState) };

		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Special 1";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SPECIAL1_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SPECIAL1_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle:Special1");
			skill.activationStateMachineName = "Body";
			skill.baseMaxStock = 2;
			skill.baseRechargeInterval = 3f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.canceledFromSprinting = false;
			skill.fullRestockOnAssign = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.isCombatSkill = true;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = true;
			return skill;
		}
	}

	// first
	public class PrimedObelisk : Asset, IProjectile, IProjectileGhost
	{
		GameObject IProjectile.BuildObject()
		{
			var projectile =
				LoadAsset<GameObject>("addressable:RoR2/Base/Nullifier/NullifierPreBombProjectile.prefab")!
					.InstantiateClone("OnkamiSealPhase1", true);
			projectile.GetComponent<ProjectileController>().ghostPrefab =
				GetGameObject<PrimedObelisk, IProjectileGhost>();
			Object.Destroy(projectile.transform.GetChild(0).gameObject);
			projectile.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			var onkamiImpact = projectile.GetComponent<ProjectileImpactExplosion>();
			onkamiImpact.blastRadius = 0.01f;
			onkamiImpact.fireChildren = true;
			onkamiImpact.blastDamageCoefficient = 1f;
			onkamiImpact.childrenProjectilePrefab = GetGameObject<TickingFuseObelisk, IProjectile>();
			onkamiImpact.childrenDamageCoefficient = 6.5f;
			onkamiImpact.impactEffect = null;
			onkamiImpact.lifetimeExpiredSound = null;
			return projectile;
		}

		GameObject IProjectileGhost.BuildObject()
		{
			var ghost = GetGameObject<ExplodingObelisk, IEffect>().InstantiateClone("OnkamiSealPhase1Ghost", false);
			Object.Destroy(ghost.GetComponent<EffectComponent>());
			Object.Destroy(ghost.GetComponent<VFXAttributes>());
			ghost.transform.GetChild(0).gameObject.SetActive(false);
			ghost.transform.GetChild(2).gameObject.SetActive(false);
			ghost.transform.GetChild(4).gameObject.SetActive(false);
			ghost.transform.GetChild(8).gameObject.SetActive(false);
			ghost.transform.GetChild(9).gameObject.SetActive(false);
			ghost.transform.GetChild(10).gameObject.SetActive(false);
			ghost.transform.GetChild(11).gameObject.SetActive(false);
			ghost.transform.localScale = Vector3.one * 13.6f;
			var onkamiMesh = ghost.transform.GetChild(7).gameObject;
			Object.Destroy(onkamiMesh.GetComponent<ObjectScaleCurve>());
			onkamiMesh.GetComponent<MeshRenderer>().materials = TheGreatSealing.onKamiMats;
			ghost.AddComponent<ProjectileGhostController>();
			return ghost;
		}
	}

	// second
	[HarmonyPatch]
	public class TickingFuseObelisk : Asset, IProjectile, IProjectileGhost
	{
		public static DamageAPI.ModdedDamageType Uitsalnemetia;

		public override void Initialize()
		{
			Uitsalnemetia = DamageAPI.ReserveDamageType();
		}
		GameObject IProjectile.BuildObject()
		{
			var projectile =
				LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab")!
					.InstantiateClone("OnkamiSealPhase2", true);
			projectile.GetComponent<ProjectileController>().ghostPrefab =
				GetGameObject<TickingFuseObelisk, IProjectileGhost>();
			var sealingImpact = projectile.GetComponent<ProjectileImpactExplosion>();
			sealingImpact.lifetime = 1.5f;
			sealingImpact.blastRadius = 15f;
			sealingImpact.fireChildren = false;
			sealingImpact.impactEffect = GetGameObject<ExplodingObelisk, IEffect>();
			sealingImpact.blastDamageCoefficient = 1f; //todo you dont set it here
			sealingImpact.blastProcCoefficient = 1f;
			projectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
			var onkamiDamage = GetAsset<TheGreatSealing>(); //uhhhhh
			projectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Uitsalnemetia);
			return projectile;
		}

		GameObject IProjectileGhost.BuildObject()
		{
			var sealingProjectileMat =
				new Material(
					LoadAsset<Material>("addressable:RoR2/Base/Common/matVoidDeathBombAreaIndicatorFront.mat"));
			sealingProjectileMat.SetTexture("_Cloud1Tex",
				LoadAsset<Texture2D>("addressable:RoR2/DLC1/MajorAndMinorConstruct/texMajorConstructShield.png"));
			sealingProjectileMat.SetTexture("_Cloud2Tex",
				LoadAsset<Texture2D>("addressable:RoR2/Base/Common/VFX/texArcaneCircleWispMask.png"));
			sealingProjectileMat.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonPreBoss.png"));
			sealingProjectileMat.SetTexture("_MainTex",
				LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texLunarWispTracer 1.png"));
			sealingProjectileMat.SetFloat("_SrcBlendFloat", 5f);
			sealingProjectileMat.SetFloat("_DstBlendFloat", 1f);
			sealingProjectileMat.SetFloat("_IntersectionStrength", 0.4f);
			sealingProjectileMat.SetFloat("_AlphaBoost", 9.041705f);
			sealingProjectileMat.SetFloat("_RimStrength", 9.041705f);
			sealingProjectileMat.SetFloat("_RimPower", 0.1f);
			sealingProjectileMat.SetColor("_TintColor", Colors.sealingColor);

			var ghost = LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombGhost.prefab")!
				.InstantiateClone("OnkamiSealPhase2Ghost", false);
			ghost.transform.localScale = Vector3.one * 2f;
			var sealingScale = ghost.transform.GetChild(0).gameObject;
			sealingScale.transform.localScale = Vector3.one * 5.625f;
			foreach (var r in ghost.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				var name = r.name;

				if (name == "AreaIndicator, Front")
				{
					r.material = sealingProjectileMat;
				}

				if (name == "AreaIndicator, Back")
				{
					r.material.SetColor("_TintColor", new Color(0f, 0.01960784f, 1f));
				}

				if (name == "Vacuum Stars")
				{
					r.material.SetTexture("_RemapTex",
						LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonArenaWall.png"));
				}
			}

			foreach (var p in ghost.GetComponentsInChildren<ParticleSystem>())
			{
				var name = p.name;
				var main = p.main;
				var sizeLife = p.sizeOverLifetime;

				if (name == "AreaIndicator, Front")
				{
					sizeLife.enabled = false;
				}

				if (name == "Vacuum Radial")
				{
					sizeLife.sizeMultiplier = 1.6f;
				}

				if (name == "AreaIndicator, Back")
				{
					sizeLife.enabled = false;
				}

				if (name == "Vacuum Stars, Trails")
				{
					main.startColor = Colors.sealingColor;
				}
			}

			var scaleChild = ghost.transform.GetChild(0);
			scaleChild.transform.position = new Vector3(0f, 4f, 0f);
			var frontIndicator = scaleChild.GetChild(8).gameObject;
			var backIndicator = scaleChild.GetChild(9).gameObject;
			frontIndicator.transform.localScale = Vector3.one * 1.3f; // sealing frontIndicator
			backIndicator.transform.localScale = Vector3.one * 1.3f; // sealing backIndicator

			var sealingMeshObject = GetGameObject<ExplodingObelisk, IEffect>().transform.GetChild(7).gameObject;
			var onkamiObelisk = PrefabAPI.InstantiateClone(sealingMeshObject, "OnkamiObelisk", false);
			Object.Destroy(onkamiObelisk.GetComponent<ObjectScaleCurve>());
			onkamiObelisk.transform.localScale = Vector3.one;
			onkamiObelisk.transform.SetParent(scaleChild);
			onkamiObelisk.transform.localScale = Vector3.one * 0.6f;
			onkamiObelisk.transform.localPosition = ghost.transform.position;
			onkamiObelisk.transform.position = new Vector3(0f, -10f, 0f);

			var onkamiMeshR = onkamiObelisk.GetComponent<MeshRenderer>();
			onkamiMeshR.materials = TheGreatSealing.onKamiMats; //this is how you make a completely new array

			return ghost;
		}
		[HarmonyPrefix, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamageProcess))]
		private static void TakeDamageProcess(HealthComponent __instance, DamageInfo damageInfo)
		{
			if (damageInfo.HasModdedDamageType(Uitsalnemetia))
			{
				var fractionOfHealth = __instance.fullHealth * 0.3f;
				var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
				if (!(__instance.health <= fractionOfHealth)) return;
				damageInfo.damageType = DamageType.VoidDeath;
				EffectManager.SpawnEffect(
					LoadAsset<GameObject>("RoR2/DLC1/CritGlassesVoid/CritGlassesVoidExecuteEffect.prefab"),
					new EffectData
					{
						origin = __instance.body.corePosition,
						rotation = Util.QuaternionSafeLookRotation(attackerBody.characterDirection.forward)
					}, false);
			}
		}
	}


	// third
	public class ExplodingObelisk : Asset, IEffect
	{
		GameObject IEffect.BuildObject()
		{
			var effect =
				LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathExplosion.prefab")!
					.InstantiateClone("OnkamiSealPhase3BlastEffect", false);
			foreach (var r in effect.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				var name = r.name;

				if (name == "AreaIndicator")
				{
					r.material.SetTexture("_Cloud1Tex",
						LoadAsset<Texture2D>(
							"addressable:RoR2/DLC1/MajorAndMinorConstruct/texMajorConstructShield.png"));
					r.material.SetTexture("_Cloud2Tex",
						LoadAsset<Texture2D>("addressable:RoR2/DLC1/ancientloft/texAncientLoft_TempleDecal.tga"));
					r.material.SetTexture("_RemapTex",
						LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonArenaWall.png"));
					r.material.SetColor("_TintColor", Colors.sealingColor); //new Color(0f, 0.9137255f, 1f));

					r.material.SetFloat("_IntersectionStrength", 0.08f);
					r.material.SetFloat("_AlphaBoost", 20f);
					r.material.SetFloat("_RimStrength", 1.050622f);
					r.material.SetFloat("_RimPower", 1.415718f);
				}

				if (name == "Vacuum Radial")
				{
					r.material.SetTexture("_MainTex",
						LoadAsset<Texture2D>("addressable:RoR2/DLC1/ancientloft/texAncientLoft_TempleDecal.tga"));
					r.material.SetTexture("_RemapTex",
						LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampBrotherPillar.png"));
					r.material.SetFloat("_AlphaBoost", 6.483454f);
					r.material.SetColor("_TintColor", new Color(0f, 0.1843f, 1f));
				}

				if (name == "Vacuum Stars, Trails")
				{
					r.enabled = false;
				}

				if (name == "Goo, Medium")
				{
					r.enabled = false;
				}

				if (name == "AreaIndicator (1)")
				{
					r.enabled = false;
				}

				if (name == "Vacuum Stars")
				{
					r.enabled = false;
				}
			}

			effect.transform.localScale = Vector3.one * 10f;
			effect.GetComponentInChildren<Light>().color = Colors.sealingColor;
			//effect.transform.position = new Vector3(effect.transform.position.x, 4f, effect.transform.position.z);
			var sealMeshR = effect.GetComponentInChildren<MeshRenderer>();
			sealMeshR.materials = TheGreatSealing.onKamiMats;


			//obtaining the perfect obelisk mesh
			var loftPrefab = LoadAsset<GameObject>("addressable:RoR2/DLC1/ancientloft/AL_LightStatue_On.prefab")!;
			var obelisk = loftPrefab.transform.GetChild(4).gameObject;
			// TODO??
			var theObelisk = PrefabAPI.InstantiateClone(obelisk, "TwinsObelisk", false);
			theObelisk.transform.position = Vector3.zero;
			var obeliskChildRotation = theObelisk.transform.rotation;
			var sealingObelisk = theObelisk.GetComponent<MeshFilter>().mesh;
			//
			var sealingMeshObject = effect.transform.GetChild(7).gameObject;
			sealingMeshObject.GetComponent<MeshFilter>().mesh = sealingObelisk;
			sealingMeshObject.transform.rotation = obeliskChildRotation; //I should get an award for this
			sealingMeshObject.transform.position = new Vector3(sealingMeshObject.transform.position.x, -8f,
				sealingMeshObject.transform.position.z);
			//the detonation and priming obelisk use the same Vector3
			sealingMeshObject.GetComponent<MeshRenderer>().materials = TheGreatSealing.onKamiMats;
			sealingMeshObject.GetComponent<ObjectScaleCurve>().baseScale = Vector3.one * 0.7f;

			var blastIndicator = effect.transform.GetChild(10).gameObject;
			blastIndicator.transform.localScale = Vector3.one * 1.4f; //blast indicator
			blastIndicator.transform.position = new Vector3(blastIndicator.transform.position.x, 0.5f,
				blastIndicator.transform.position.z);

			effect.EffectWithSound("Play_item_void_bleedOnHit_explo");

			return effect;
		}
	}
}