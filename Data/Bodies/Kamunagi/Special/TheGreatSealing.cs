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
		public static GameObject muzzleEffect;
		public override float duration => 10f;
		public override float failedCastCooldown => 1f;
		private uint soundID;

		public override void Fire(Vector3 targetPosition)
		{
			base.Fire(targetPosition);

			ProjectileManager.instance.FireProjectile(Concentric.GetProjectile<PrimedObelisk>().WaitForCompletion(),
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
			var muzzleTransform = FindModelChild("MuzzleCenter");
			if (!muzzleTransform) return;
			soundID = AkSoundEngine.PostEvent(1275107278, base.gameObject);
			chargeEffectInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(muzzleEffect, muzzleTransform,
				true, new EffectData() { rootObject = muzzleTransform.gameObject, scale = 0.1f });
		}

		public override void OnExit()
		{
			base.OnExit();
			if (chargeEffectInstance != null) chargeEffectInstance.ReturnToPool();
			//AkSoundEngine.StopPlayingID(soundID);
			EffectManager.SimpleMuzzleFlash(Concentric.GetEffect<SealingMuzzleFlash>().WaitForCompletion(), gameObject, "MuzzleCenter", false);
		}
	}
	
	public class TheGreatSealing : Concentric, ISkill, IEffect
	{
		public static Material[] onKamiMats;
		public override async Task Initialize()
		{
			await base.Initialize();
			TheGreatSealingState.muzzleEffect = await this.GetEffect();
			var onkami1 = new Material(await LoadAsset<Material>("addressable:RoR2/Base/artifactworld/matArtifactPortalCenter.mat"));
			onkami1.SetFloat("_AlphaBoost", 1.3f);
			onkami1.SetColor("_TintColor", new Color(0f, 0.1843f, 1f));
			var onkami2 = new Material(await LoadAsset<Material>("addressable:RoR2/Base/artifactworld/matArtifactPortalEdge.mat"));
			onkami2.SetColor("_TintColor", new Color(0f, 0.1843f, 1f));
			onkami2.SetTexture("_RemapTex", await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonArenaWall.png"));
			onkami2.SetFloat("_BrightnessBoost", 4.67f);
			onkami2.SetFloat("_AlphaBoost", 1.2f);
			onKamiMats = new[] { onkami1, onkami2 };
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect= (await LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonGhost.prefab"))!.InstantiateClone("AntimatterMuzzleEffect", false);
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
			emissionMat.SetTexture("_Emission", await LoadAsset<Texture2D>("addressable:RoR2/Base/ElectricWorm/ElectricWormBody.png"));
			rampMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampLunarElectric.png"));
			Object.Destroy(scaler.transform.GetChild(3).gameObject);
			Object.Destroy(scaler.transform.GetChild(2).gameObject);
			Object.Destroy(scaler.transform.GetChild(0).gameObject);
			
			var warp = PrefabAPI.InstantiateClone(scaler.transform.GetChild(1).gameObject, "DistortionSphere", false);
			warp.transform.parent = effect.transform;
			warp.transform.localScale = Vector3.one * 10f;
			warp.transform.localPosition = Vector3.zero;
			
			Material distortion = new Material(await LoadAsset<Material>("RoR2/Base/Nullifier/matNullifierDeathDistortion.mat"));
			Material outline = new Material(await LoadAsset<Material>("RoR2/DLC1/VoidMegaCrab/matVoidCrabMatterOverlay.mat"));
			outline.SetTexture("_Emission", await LoadAsset<Texture2D>("addressable:RoR2/Base/ElectricWorm/ElectricWormBody.png"));
			outline.SetTexture("_RemapTex", await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampLunarElectric.png"));

			warp.GetComponent<MeshRenderer>().materials = new[] { distortion, outline}; 
			return effect;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(TheGreatSealingState) };

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Special 1";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SPECIAL1_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SPECIAL1_DESCRIPTION";
			skill.icon = await LoadAsset<Sprite>("kamunagiassets:Special1");
			skill.activationStateMachineName = "Weapon";
			skill.baseMaxStock = 1;
			skill.baseRechargeInterval = 8f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.canceledFromSprinting = false;
			skill.interruptPriority = InterruptPriority.Any;
			skill.isCombatSkill = true;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = true;
			skill.stockToConsume = 1;
			return skill;
		}
	}

	// first
	public class PrimedObelisk : Concentric, IProjectile, IProjectileGhost
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			var projectile =
				(await LoadAsset<GameObject>("addressable:RoR2/Base/Nullifier/NullifierPreBombProjectile.prefab"))!.InstantiateClone("OnkamiSealPhase1", true);
			projectile.GetComponent<ProjectileController>().ghostPrefab =
				await GetProjectileGhost<PrimedObelisk>();
			Object.Destroy(projectile.transform.GetChild(0).gameObject);
			projectile.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			var onkamiImpact = projectile.GetComponent<ProjectileImpactExplosion>();
			onkamiImpact.blastRadius = 0.01f;
			onkamiImpact.fireChildren = true;
			onkamiImpact.blastDamageCoefficient = 1f;
			onkamiImpact.childrenProjectilePrefab = await GetProjectile<TickingFuseObelisk>();
			onkamiImpact.childrenDamageCoefficient = 6.5f;
			onkamiImpact.impactEffect = null;
			onkamiImpact.lifetimeExpiredSound = null;
			return projectile;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await GetEffect<ExplodingObelisk>()).InstantiateClone("OnkamiSealPhase1Ghost", false);
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
	public class TickingFuseObelisk : Concentric, IProjectile, IProjectileGhost
	{

		async Task<GameObject> IProjectile.BuildObject()
		{
			var projectile =
				(await LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab")!
					).InstantiateClone("OnkamiSealPhase2", true);
			projectile.GetComponent<ProjectileController>().ghostPrefab =
				await this.GetProjectileGhost();
			var sealingImpact = projectile.GetComponent<ProjectileImpactExplosion>();
			sealingImpact.lifetime = 1.5f;
			sealingImpact.blastRadius = 22f;
			sealingImpact.fireChildren = false;
			sealingImpact.impactEffect = await GetEffect<ExplodingObelisk>();
			sealingImpact.blastDamageCoefficient = 1f;  
			sealingImpact.blastProcCoefficient = 1f;
			projectile.GetComponent<ProjectileDamage>().damageType = DamageTypeCombo.GenericSpecial
				.AddModdedDamageTypeChainable(Uitsalnemetia);
			return projectile;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var sealingProjectileMat =
				new Material(
					await LoadAsset<Material>("addressable:RoR2/Base/Common/matVoidDeathBombAreaIndicatorFront.mat"));
			sealingProjectileMat.SetTexture("_Cloud1Tex",
				await LoadAsset<Texture2D>("addressable:RoR2/DLC1/MajorAndMinorConstruct/texMajorConstructShield.png"));
			sealingProjectileMat.SetTexture("_Cloud2Tex",
				await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/VFX/texArcaneCircleWispMask.png"));
			sealingProjectileMat.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonPreBoss.png"));
			sealingProjectileMat.SetTexture("_MainTex",
				await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texLunarWispTracer 1.png"));
			sealingProjectileMat.SetFloat("_SrcBlendFloat", 5f);
			sealingProjectileMat.SetFloat("_DstBlendFloat", 1f);
			sealingProjectileMat.SetFloat("_IntersectionStrength", 0.4f);
			sealingProjectileMat.SetFloat("_AlphaBoost", 9.041705f);
			sealingProjectileMat.SetFloat("_RimStrength", 9.041705f);
			sealingProjectileMat.SetFloat("_RimPower", 0.1f);
			sealingProjectileMat.SetColor("_TintColor", Colors.sealingColor);

			var ghost = (await LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombGhost.prefab")!
				).InstantiateClone("OnkamiSealPhase2Ghost", false);
			ghost.transform.localScale = Vector3.one * 2f;
			var sealingScale = ghost.transform.GetChild(0).gameObject;
			sealingScale.transform.localScale = Vector3.one * 8.2f; //scale child
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
						await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonArenaWall.png"));
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
			frontIndicator.transform.localScale = Vector3.one * 1.5f; // sealing frontIndicator
			backIndicator.transform.localScale = Vector3.one * 1.5f; // sealing backIndicator
			var enableCulling = frontIndicator.GetComponent<ParticleSystemRenderer>();
			enableCulling.materials[0].SetFloat("_Cull", 2);

			var sealingMeshObject = (await GetEffect<ExplodingObelisk>()).transform.GetChild(7).gameObject;
			var onkamiObelisk = sealingMeshObject.InstantiateClone("OnkamiObelisk", false);
			Object.Destroy(onkamiObelisk.GetComponent<ObjectScaleCurve>());
			onkamiObelisk.transform.localScale = Vector3.one;
			onkamiObelisk.transform.SetParent(scaleChild);
			onkamiObelisk.transform.localScale = Vector3.one * 0.6f;
			onkamiObelisk.transform.localPosition = new Vector3(0, -1.25f, 0);
			var onkamiMeshR = onkamiObelisk.GetComponent<MeshRenderer>();
			onkamiMeshR.materials = TheGreatSealing.onKamiMats;

			return ghost;
		}
		[HarmonyPrefix, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamageProcess))]
		private static void TakeDamageProcess(HealthComponent __instance, DamageInfo damageInfo)
		{
			if (damageInfo.HasModdedDamageType(Uitsalnemetia))
			{
				var fractionOfHealth = __instance.fullCombinedHealth * 0.35f;
				var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
				if (__instance.health <= fractionOfHealth || damageInfo.damage >= __instance.health)
				{
					damageInfo.damageType = DamageType.VoidDeath;
					if (!attackerBody) return;
					EffectManager.SpawnEffect(
						GetEffect<CyanDamageNumbers>().WaitForCompletion(),
						new EffectData
						{
							origin = __instance.body.corePosition,
							rotation = Util.QuaternionSafeLookRotation(attackerBody.characterDirection.forward)
						}, false);
				}
			}
		}
	}

	public class CyanDamageNumbers : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect= (await LoadAsset<GameObject>("RoR2/DLC1/CritGlassesVoid/CritGlassesVoidExecuteEffect.prefab"))!.InstantiateClone("SealExecuteEffect", false);
			var numbers = effect.transform.GetChild(9).gameObject;
			var pRender = numbers.GetComponent<ParticleSystemRenderer>();
			pRender.material.SetColor("_TintColor", new Color(0f, 1f, 0.98f));
			effect.EffectWithSound("Play_Seal_Execute"); //execute sound
			return effect;
		}
	}
	
	public class SealingMuzzleFlash : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab"))!.transform.GetChild(3).gameObject.InstantiateClone("SealMuzzleFlash", false);
			ParticleSystem p = effect.GetComponent<ParticleSystem>();
			var main = p.main;
			main.startSize = 1f;
			return effect;
		}
	}

	// third
	public class ExplodingObelisk : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect =
				(await LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathExplosion.prefab")!
					).InstantiateClone("OnkamiSealPhase3BlastEffect", false);
			foreach (var r in effect.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				var name = r.name;

				if (name == "AreaIndicator")
				{
					r.material.SetTexture("_Cloud1Tex",
						await LoadAsset<Texture2D>(
							"addressable:RoR2/DLC1/MajorAndMinorConstruct/texMajorConstructShield.png"));
					r.material.SetTexture("_Cloud2Tex",
						await LoadAsset<Texture2D>("addressable:RoR2/DLC1/ancientloft/texAncientLoft_TempleDecal.tga"));
					r.material.SetTexture("_RemapTex",
						await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonArenaWall.png"));
					r.material.SetColor("_TintColor", Colors.sealingColor); //new Color(0f, 0.9137255f, 1f));

					r.material.SetFloat("_IntersectionStrength", 0.08f);
					r.material.SetFloat("_AlphaBoost", 20f);
					r.material.SetFloat("_RimStrength", 1.050622f);
					r.material.SetFloat("_RimPower", 1.415718f);
				}

				if (name == "Vacuum Radial")
				{
					r.material.SetTexture("_MainTex",
					await 	LoadAsset<Texture2D>("addressable:RoR2/DLC1/ancientloft/texAncientLoft_TempleDecal.tga"));
					r.material.SetTexture("_RemapTex",
						await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampBrotherPillar.png"));
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
			var sealMeshR = effect.GetComponentInChildren<MeshRenderer>();
			sealMeshR.materials = TheGreatSealing.onKamiMats;
			//obtaining the perfect obelisk mesh
			var loftPrefab= (await LoadAsset<GameObject>("addressable:RoR2/DLC1/ancientloft/AL_LightStatue_On.prefab"))!;
			var obelisk = loftPrefab.transform.GetChild(4).gameObject;
			//
			var theObelisk = PrefabAPI.InstantiateClone(obelisk, "TwinsObelisk", false);
			theObelisk.transform.position = Vector3.zero;
			var obeliskChildRotation = theObelisk.transform.rotation;
			var sealingObelisk = theObelisk.GetComponent<MeshFilter>().mesh;
			//
			var sealingMeshObject = effect.transform.GetChild(7).gameObject;
			sealingMeshObject.GetComponent<MeshFilter>().mesh = sealingObelisk;
			sealingMeshObject.transform.rotation = obeliskChildRotation; //I should get an award for this
			sealingMeshObject.transform.position = new Vector3(sealingMeshObject.transform.position.x, -8f, sealingMeshObject.transform.position.z);
			//the detonation and priming obelisk use the same Vector3
			sealingMeshObject.GetComponent<MeshRenderer>().materials = TheGreatSealing.onKamiMats;
			sealingMeshObject.GetComponent<ObjectScaleCurve>().baseScale = Vector3.one * 0.7f;

			var blastIndicator = effect.transform.GetChild(10).gameObject;
			blastIndicator.transform.localScale = Vector3.one * 1.15f; //blast indicator
			blastIndicator.transform.localPosition = new Vector3(0, 0.25f, 0);

			effect.EffectWithSound("Play_Seal_Detonate"); //detonation sound
			return effect;
		}
	}
}