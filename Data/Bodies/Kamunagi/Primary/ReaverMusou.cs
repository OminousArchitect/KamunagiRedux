using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Special;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using Path = RoR2.Path;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	internal class ReaverMusouState : BaseTwinState
	{
		public override int meterGain => 0;
		private float fireRate = 0.3f;
		private const float fizzle = 0.3f;
		private float stopwatch;

		void SowSeeds()
		{
			
			var aimRay = GetAimRay();
			var testForTarget = new BulletAttack()
			{
				owner = gameObject,
				weapon = gameObject,
				origin = aimRay.origin,
				aimVector = aimRay.direction,
				maxDistance = 1000,
				damage = 0,
				force = 0,
				radius = 0.3f,
				procCoefficient = 0,
				smartCollision = true,
				muzzleName = "MuzzleRight",
				tracerEffectPrefab = Concentric.GetEffect<ReaverMusou>().WaitForCompletion(),
				hitCallback = (BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo) =>
				{
					ProjectileManager.instance.FireProjectile(new FireProjectileInfo
					{
						position = hitInfo.point,
						crit = RollCrit(),
						projectilePrefab = Concentric.GetProjectile<ReaverMusou>().WaitForCompletion(),
						owner = gameObject,
						damage = characterBody.damage,
						force = 1
					});
					return false;
				}
			};
			testForTarget.Fire();
			EffectManager.SimpleMuzzleFlash(Concentric.GetEffect<ReaverMusou>().WaitForCompletion(), gameObject, twinMuzzle, false);
		}

		void HarvestSeeds()
		{
			var aimRay = GetAimRay();
			var testForTarget = new BulletAttack()
			{
				owner = gameObject,
				weapon = gameObject,
				origin = aimRay.origin,
				aimVector = aimRay.direction,
				maxDistance = 1000,
				damage = 0,
				force = 0,
				radius = 0.3f,
				procCoefficient = 0,
				smartCollision = true,
				muzzleName = "MuzzleRight",
				tracerEffectPrefab = null,
				hitCallback = (BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo) =>
				{
					ProjectileManager.instance.FireProjectile(new FireProjectileInfo
					{
						position = hitInfo.point,
						crit = RollCrit(),
						projectilePrefab = Concentric.GetProjectile<StickyBombDetonator>().WaitForCompletion(),
						owner = gameObject,
						damage = characterBody.damage,
						force = 200
					});
					return false;
				}
			};
			testForTarget.Fire();
		}
		
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.deltaTime;
			
			if (stopwatch >= twinBehaviour.runtimeNumber2 && isAuthority)
			{
				SowSeeds();
				stopwatch = 0;
			}

			if (!inputBank.skill1.down)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			if (fixedAge <= fizzle) return;
			HarvestSeeds();
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	internal class ReaverMusou : Concentric, ISkill, IEffect, IProjectile, IProjectileGhost
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 2";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "PRIMARY2_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "PRIMARY2_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("bundle:darkpng"));
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 0f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(ReaverMusouState) };

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect= (await LoadAsset<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab"))!.InstantiateClone("ReaverMusouMuzzleFlash", false);
			UnityEngine.Object.Destroy(effect.GetComponent<ShakeEmitter>());
			//UnityEngine.Object.Destroy(effect.GetComponent<Rigidbody>());
			effect.transform.GetChild(1).gameObject.SetActive(false);
			effect.transform.GetChild(4).gameObject.SetActive(false);
			//effect.transform.GetChild(5).gameObject.SetActive(false);
			effect.transform.GetChild(6).gameObject.SetActive(false);
			effect.transform.localScale = Vector3.one * 0.4f;
			var dist = effect.transform.GetChild(3).gameObject;
			ParticleSystem p = dist.GetComponent<ParticleSystem>();
			var main = p.main;
			main.startSize = 1.4f;
			var comp = effect.GetComponent<EffectComponent>();
			//comp.parentToReferencedTransform = true;
			//comp.positionAtReferencedTransform = true;
			return effect;
		}

		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj= (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabWhiteCannonStuckProjectile.prefab"))!.InstantiateClone("TwinsReaverProjectile", true);
			ProjectileImpactExplosion impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.lifetime = 12f;
			var controller = proj.GetComponent<ProjectileController>();
			controller.procCoefficient = 0.8f;
			controller.ghostPrefab = await this.GetProjectileGhost();
			//proj.transform.GetChild(0).gameObject.SetActive(false);
			var crabController = proj.GetComponent<MegacrabProjectileController>();
			crabController.whiteToBlackTransformedProjectile = await GetProjectile<Recursion1Projectile>();
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			Material seedMaterial = new Material( await LoadAsset<Material>("RoR2/Base/Nullifier/matNullifierPortal.mat"));
			seedMaterial.SetFloat("_AlphaBoost", 1f);
			seedMaterial.SetFloat("_AlphaBias", 0.28f);
			seedMaterial.SetFloat("_Boost", 10f);
			seedMaterial.SetTexture("_MainTex", await LoadAsset<Texture2D>("RoR2/Base/Common/texCloudColor2.png"));
			seedMaterial.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampVoidRaidPlanet2.png"));
			seedMaterial.SetFloat("_SrcBlendFloat", 10f);
			seedMaterial.SetFloat("_DstBlendFloat", 1f);
			
			var ghost = (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabWhiteCannonStuckGhost.prefab"))!.InstantiateClone("TwinsReaverGhost", false);
			var flicker = (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsGhost.prefab")).transform.GetChild(1).gameObject!.InstantiateClone("FlickerTwins", false);
			var lightC = flicker.GetComponent<Light>();
			lightC.color = new Color(0.4333f, 0.0726f, 0.8925f);
			lightC.range = 10f;
			lightC.intensity = 40f;
			flicker.transform.parent = ghost.transform;
			UnityEngine.Object.Destroy(flicker.GetComponent<FlickerLight>());
			flicker.transform.localPosition = Vector3.zero;
			/*LightIntensityCurve curve = flicker.GetOrAddComponent<LightIntensityCurve>();
			curve.maxIntensity = 71f;
			curve.timeMax = 2f;*/
			var child = ghost.transform.Find("Scaler,Animated/Scaler, Random").gameObject;
			UnityEngine.Object.Destroy(child.transform.GetChild(0).gameObject);
			var sphere = child.transform.GetChild(1).gameObject;
			MeshRenderer material = sphere.GetComponent<MeshRenderer>();
			material.material = seedMaterial;
			return ghost;
		}
	}

	internal class ReaverExplosion : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect= (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegacrabAntimatterExplosion.prefab"))!.InstantiateClone("TwinsReaverExplosion", false);
			effect.transform.localScale = Vector3.one * 5f;
			effect.GetComponent<EffectComponent>().applyScale = false;
			var discBillboard = effect.transform.GetChild(6).gameObject;
			discBillboard.transform.localScale = Vector3.one * 0.3f;
			var discCircular = effect.transform.GetChild(5).gameObject;
			discCircular.transform.localScale = Vector3.one * 0.5f;
			var indicator = effect.transform.GetChild(4).gameObject;
			indicator.transform.localScale = Vector3.one * 0.75f;
			UnityEngine.Object.Destroy(effect.transform.GetChild(7).gameObject);
			foreach (var particleSystemRenderer in effect.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				switch (particleSystemRenderer.name)
				{  //particleSystemRenderer.material.SetTexture("_RemapText",LoadAsset<Texture2D>(""));
					case "Billboard, Long":
						particleSystemRenderer.material.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/DLC1/VoidRaidCrab/texRampVoidRaidCrabTripleBeam.png"));
						break;
					case "Billboard, Short":
						particleSystemRenderer.material.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/DLC1/VoidRaidCrab/texRampVoidRaidCrabTripleBeam.png"));
						particleSystemRenderer.material.SetFloat("_Boost", 2f);
						break;
					case "Mesh, Donut":
						particleSystemRenderer.material.SetTexture("_RemapTex",await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
						break;
					case "Mesh, Sphere":
						particleSystemRenderer.material.SetTexture("_RemapTex",await LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampVoidRing.png"));
						break;
					case "Trails":
						particleSystemRenderer.trailMaterial.SetColor("_TintColor", new Color(0.35f, 0.17f, 0.93f));
						particleSystemRenderer.trailMaterial.SetTexture("_RemapTex",await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
						particleSystemRenderer.trailMaterial.SetFloat("_NormalStrength", 1.41f);
						break;
					case "MeshIndicator":
						particleSystemRenderer.enabled = false;
						break;
				}
			}
			Light light = effect.AddComponent<Light>();
			light.range = 10f;
			light.intensity = 60f;
			light.color = Colors.twinsDarkColor;
			LightIntensityCurve curve = effect.GetOrAddComponent<LightIntensityCurve>();
			curve.maxIntensity = 71f;
			curve.timeMax = 0.6f;
			return effect;
		}
	}
	
	[HarmonyPatch]
	internal class PrimedStickyBomb : Concentric, IProjectile, IProjectileGhost
	{
		public static DamageAPI.ModdedDamageType TwinsReaver;
		public override Task Initialize()
		{
			TwinsReaver = DamageAPI.ReserveDamageType();
			return base.Initialize();
		}
		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonStuckProjectile1.prefab"))!.InstantiateClone("TwinsPrimedReaverProjectile", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			ProjectileImpactExplosion impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.impactEffect = await GetEffect<ReaverExplosion>();
			impact.bonusBlastForce = new Vector3(0f, 0f, 0f);
			impact.falloffModel = BlastAttack.FalloffModel.None;
			impact.blastDamageCoefficient = 1.25f;
			var crabController = proj.GetComponent<MegacrabProjectileController>();
			crabController.whiteToBlackTransformedProjectile = await GetProjectile<Recursion1Projectile>(); //this is so the bombs can blow up each other as well as blow up from
			crabController.whiteToBlackTransformationRadius = 7.5f;
			proj.GetComponent<ProjectileDamage>().damageType = DamageType.Nullify;
			proj.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(TwinsReaver);
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			Material primed = new Material(await LoadAsset<Material>("RoR2/DLC1/VoidMegaCrab/matVoidCrabMatterOpaque.mat"));
			primed.SetColor("_Color", new Color(1f, 0f, 0.66f));
			primed.SetTexture("_EmissionTex", await LoadAsset<Texture2D>("RoR2/Base/skymeadow/texSkymeadowPreview.png"));
			primed.SetFloat("_EmissionPower", 2f);
			primed.SetFloat("_HeightStrength", 10f);
			primed.SetFloat("_HeightBias", 0.11f);

			Material primedOutline = new Material(await LoadAsset<Material>("RoR2/DLC1/VoidMegaCrab/matVoidCrabMatterOverlay.mat"));
			primedOutline.SetColor("_TintColor", new Color(0.29f, 0f, 1f));
			primedOutline.SetFloat("_Boost", 12.15f);
			
			var ghost = (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonStuckGhost.prefab"))!.InstantiateClone("TwinsPrimedReaverGhost", false);
			ghost.transform.localScale = Vector3.one * 0.5f;
			var scaler = ghost.transform.GetChild(0).gameObject;
			var sphere = ghost.transform.Find("Scaler/VoidMegacrabBlackSphere").gameObject;
			UnityEngine.Object.Destroy(sphere.GetComponent<SetRandomScale>());
			var destroy = ghost.transform.Find("Scaler/Point Light, Flash").gameObject;
			UnityEngine.Object.Destroy(scaler.transform.GetChild(2).gameObject);
			UnityEngine.Object.Destroy(destroy);
			sphere.transform.localScale = Vector3.one * 4;
			sphere.GetComponent<MeshRenderer>().materials = new[] { primed, primedOutline };
			var indicator = scaler.transform.GetChild(4).gameObject;
			indicator.transform.localScale = Vector3.one * 15f;
			var light = scaler.transform.GetChild(0).gameObject;
			light.SetActive(true);
			UnityEngine.Object.Destroy(light.GetComponent<LightIntensityCurve>());
			Light l = light.GetComponent<Light>();
			l.range = 15f;
			l.intensity = 65f;
			l.color = Colors.twinsLightColor;
			return ghost;
		}
		[HarmonyPrefix, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamageProcess))]
		private static void TakeDamageProcess(HealthComponent __instance, DamageInfo damageInfo)
		{
			if (damageInfo.HasModdedDamageType(TwinsReaver))
			{
				if (damageInfo.damage >= __instance.health)
				{
					damageInfo.damageType = DamageType.VoidDeath;
				}
			}
		}
	}
	
	internal class StickyBombDetonator : Concentric, IProjectile, IProjectileGhost
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj= (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonProjectile.prefab"))!.InstantiateClone("TwinsDetonatorProjectile", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			var simple = proj.GetComponent<ProjectileSimple>();
			simple.desiredForwardSpeed = 0f;
			simple.lifetime = 10f;
			simple.velocityOverLifetime = null;
			ProjectileImpactExplosion impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.falloffModel = BlastAttack.FalloffModel.None;
			impact.bonusBlastForce = new Vector3(0f, 75f, 0f);
			impact.blastDamageCoefficient = 1.75f;
			impact.lifetime = 0.1f;
			impact.blastRadius = 5f;
			impact.destroyOnEnemy = false;
			impact.destroyOnWorld = false;
			impact.impactEffect = await GetEffect<ReaverExplosion>(); 
			var crabController = proj.GetComponent<MegacrabProjectileController>(); //this is where the transformation happens
			crabController.whiteToBlackTransformedProjectile = await GetProjectile<PrimedStickyBomb>();
			crabController.whiteToBlackTransformationRadius = 13f;
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			Material primed = new Material(await LoadAsset<Material>("RoR2/DLC1/VoidMegaCrab/matVoidCrabMatterOpaque.mat"));
			primed.SetColor("_Color", new Color(1f, 0f, 0.66f));
			primed.SetTexture("_EmissionTex", await LoadAsset<Texture2D>("RoR2/Base/skymeadow/texSkymeadowPreview.png"));
			primed.SetFloat("_EmissionPower", 2f);
			primed.SetFloat("_HeightStrength", 10f);
			primed.SetFloat("_HeightBias", 0.11f);

			Material primedOutline = new Material(await LoadAsset<Material>("RoR2/DLC1/VoidMegaCrab/matVoidCrabMatterOverlay.mat"));
			primedOutline.SetColor("_TintColor", new Color(0.29f, 0f, 1f));
			primedOutline.SetFloat("_Boost", 12.15f);
			
			var ghost = (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonGhost.prefab"))!.InstantiateClone("TwinsDetonatorGhost", false);
			Light light = ghost.GetComponentInChildren<Light>();
			light.range = 10f;
			light.intensity = 60f;
			light.color = Colors.twinsDarkColor;
			ghost.transform.localScale = Vector3.one * 0.1f;
			var scaler = ghost.transform.GetChild(0).gameObject;
			UnityEngine.Object.Destroy(scaler.transform.GetChild(2).gameObject);
			var sphere = ghost.transform.Find("Scaler/VoidMegacrabBlackSphere").gameObject;
			sphere.GetComponent<MeshRenderer>().materials = new[] { primed, primedOutline };
			return ghost;
		}
	}
	
	internal class Recursion1Projectile : Concentric, IProjectile 
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonStuckProjectile1.prefab"))!.InstantiateClone("Recursion1Projectile", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = await GetProjectileGhost<PrimedStickyBomb>();
			ProjectileImpactExplosion impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.impactEffect = await GetEffect<ReaverExplosion>();
			var crabController = proj.GetComponent<MegacrabProjectileController>();
			crabController.whiteToBlackTransformedProjectile = await GetProjectile<Recursion2Projectile>();
			crabController.whiteToBlackTransformationRadius = 12f;
			proj.GetComponent<ProjectileDamage>().damageType = DamageType.Nullify;
			proj.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(PrimedStickyBomb.TwinsReaver);
			return proj;
		}
	}

	internal class Recursion2Projectile : Concentric, IProjectile //this handles up to 3 recursive explosions, thats enough I guess. Vanilla component infinitely recurses and I couldn't manage that without stackoverflow because I'm 
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonStuckProjectile1.prefab"))!.InstantiateClone("Recursion2Projectile", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = await GetProjectileGhost<PrimedStickyBomb>();
			ProjectileImpactExplosion impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.impactEffect = await GetEffect<ReaverExplosion>();
			var crabController = proj.GetComponent<MegacrabProjectileController>();
			crabController.whiteToBlackTransformedProjectile = null; //have to null or else stack overflow
			crabController.whiteToBlackTransformationRadius = 12f;
			proj.GetComponent<ProjectileDamage>().damageType = DamageType.Nullify;
			proj.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(PrimedStickyBomb.TwinsReaver);
			return proj;
		}
	}
}