using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
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
		private float stopwatch;

		void SowSeeds()
		{
			if (!Asset.TryGetGameObject<ReaverMusou, IEffect>(out var tracer))
				throw new Exception("Effect failed to load.");
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
				tracerEffectPrefab = tracer,
				hitCallback = (BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo) =>
				{
					ProjectileManager.instance.FireProjectile(new FireProjectileInfo
					{
						position = hitInfo.point,
						crit = RollCrit(),
						projectilePrefab = Asset.GetGameObject<ReaverMusou, IProjectile>(),
						owner = gameObject,
						damage = characterBody.damage * 2.8f,
						force = 200
					});
					return false;
				}
			};
			testForTarget.Fire();
			EffectManager.SimpleMuzzleFlash(Asset.GetGameObject<ReaverMusou, IEffect>(), gameObject, twinMuzzle, false);
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
						projectilePrefab = Asset.GetGameObject<StickyBombDetonator, IProjectile>(),
						owner = gameObject,
						damage = characterBody.damage * 2.8f,
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
			//if (fixedAge <= 0.3f) return;
			HarvestSeeds();
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	internal class ReaverMusou : Asset, ISkill, IEffect, IProjectile, IProjectileGhost
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 2";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "PRIMARY2_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "PRIMARY2_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle:darkpng");
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 0f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(ReaverMusouState) };

		GameObject IEffect.BuildObject()
		{
			var effect = LoadAsset<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab")!.InstantiateClone("ReaverMusouMuzzleFlash", false);
			UnityEngine.Object.Destroy(effect.GetComponent<ShakeEmitter>());
			//UnityEngine.Object.Destroy(effect.GetComponent<Rigidbody>());
			effect.transform.GetChild(1).gameObject.SetActive(false);
			effect.transform.GetChild(4).gameObject.SetActive(false);
			//effect.transform.GetChild(5).gameObject.SetActive(false);
			effect.transform.GetChild(6).gameObject.SetActive(false);
			effect.transform.localScale = Vector3.one * 0.4f;
			var dist = effect.transform.GetChild(3).gameObject;
			var distP = dist.GetComponentInChildren<ParticleSystem>().shape;
			distP.scale = Vector3.one * 0.25f;
			var comp = effect.GetComponent<EffectComponent>();
			//comp.parentToReferencedTransform = true;
			//comp.positionAtReferencedTransform = true;
			return effect;
		}

		GameObject IProjectile.BuildObject()
		{
			var proj = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabWhiteCannonStuckProjectile.prefab")!.InstantiateClone("TwinsReaverProjectile", true);
			ProjectileImpactExplosion impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.lifetime = 12f;
			//impact.impactEffect = GetGameObject<ReaverMusou, IEffect>();
			//impact.blastRadius = 5f;
			var controller = proj.GetComponent<ProjectileController>();
			controller.procCoefficient = 0.8f;
			controller.ghostPrefab = GetGameObject<ReaverMusou, IProjectileGhost>();
			//proj.transform.GetChild(0).gameObject.SetActive(false);
			return proj;
		}

		GameObject IProjectileGhost.BuildObject()
		{
			var ghost = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabWhiteCannonStuckGhost.prefab")!.InstantiateClone("TwinsReaverGhost", false);
			/*ghost.transform.localScale = Vector3.one;
			var reaveMesh = ghost.GetComponentInChildren<MeshRenderer>();
			reaveMesh.material.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampBanditSplatter.png"));
			reaveMesh.material.SetColor("_TintColor", new Color(0.5411765f, 0.1176471f, 1f));
			var flickerPurple = ghost.transform.GetChild(1).gameObject;
			var lightC = flickerPurple.GetComponent<Light>();
			lightC.color = new Color(0.4333f, 0.0726f, 0.8925f);
			flickerPurple.transform.localPosition = new Vector3(0f, 0.7f, 0f);*/
			return ghost;
		}
	}
	
	internal class StickyBombDetonator : Asset, IProjectile, IProjectileGhost, IEffect
	{
		GameObject IProjectile.BuildObject()
		{
			var proj = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonProjectile.prefab")!.InstantiateClone("TwinsDetonatorProjectile", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = GetGameObject<StickyBombDetonator, IProjectileGhost>();
			var simple = proj.GetComponent<ProjectileSimple>();
			simple.desiredForwardSpeed = 0f;
			simple.lifetime = 10f;
			simple.velocityOverLifetime = null;
			ProjectileImpactExplosion impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.falloffModel = BlastAttack.FalloffModel.None;
			impact.blastRadius = 
			impact.lifetime = 0.1f;
			impact.destroyOnEnemy = false;
			impact.destroyOnWorld = false;
			impact.impactEffect = GetGameObject<StickyBombDetonator, IEffect>();
			var crabController = proj.GetComponent<MegacrabProjectileController>(); //this is where the transformation happens
			crabController.whiteToBlackTransformedProjectile = GetGameObject<PrimedStickyBomb, IProjectile>();
			crabController.whiteToBlackTransformationRadius = 5f;
			return proj;
		}

		GameObject IProjectileGhost.BuildObject()
		{
			var ghost = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonGhost.prefab")!.InstantiateClone("TwinsDetonatorGhost", false);
			ghost.transform.localScale = Vector3.one * 0.1f;
			return ghost;
		}

		GameObject IEffect.BuildObject()
		{
			var effect = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegacrabAntimatterExplosion.prefab")!.InstantiateClone("TwinsDetonatorExplosion", false);
			effect.transform.localScale = Vector3.one * 5f;
			effect.GetComponent<EffectComponent>().applyScale = false;
			var discBillboard = effect.transform.GetChild(6).gameObject;
			discBillboard.transform.localScale = Vector3.one * 0.3f;
			var discCircular = effect.transform.GetChild(5).gameObject;
			discCircular.transform.localScale = Vector3.one * 0.5f;
			var indicator = effect.transform.GetChild(4).gameObject;
			indicator.transform.localScale = Vector3.one * 0.75f;
			return effect;
		}
	}

	internal class PrimedStickyBomb : Asset, IProjectile, IProjectileGhost, IEffect
	{
		GameObject IProjectile.BuildObject()
		{
			var proj = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonStuckProjectile1.prefab")!.InstantiateClone("TwinsPrimedReaverProjectile", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = GetGameObject<PrimedStickyBomb, IProjectileGhost>();
			ProjectileImpactExplosion impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.impactEffect = GetGameObject<PrimedStickyBomb, IEffect>();
			var crabController = proj.GetComponent<MegacrabProjectileController>();
			crabController.whiteToBlackTransformedProjectile = GetGameObject<ChainReactionProjectile, IProjectile>(); //you could swap this for different chain-reaction visuals if you wanted
			crabController.whiteToBlackTransformationRadius = 7.5f;
			return proj;
		}

		GameObject IProjectileGhost.BuildObject()
		{
			var ghost = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonStuckGhost.prefab")!.InstantiateClone("TwinsPrimedReaverGhost", false);
			ghost.transform.localScale = Vector3.one * 0.5f;
			var scaler = ghost.transform.GetChild(0).gameObject;
			var sphere = ghost.transform.Find("Scaler/VoidMegacrabBlackSphere").gameObject;
			UnityEngine.Object.Destroy(sphere.GetComponent<SetRandomScale>());
			sphere.transform.localScale = Vector3.one * 4;
			var indicator = scaler.transform.GetChild(4).gameObject;
			indicator.transform.localScale = Vector3.one * 15f;
			return ghost;
		}

		GameObject IEffect.BuildObject()
		{
			var effect = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegacrabAntimatterExplosionSimple.prefab")!.InstantiateClone("TwinsPrimedReaverExplosion", false);
			effect.transform.localScale = Vector3.one * 5f;
			effect.GetComponent<EffectComponent>().applyScale = false;
			var discCircular = effect.transform.GetChild(5).gameObject;
			discCircular.transform.localScale = Vector3.one * 0.5f;
			var discBillboard = effect.transform.GetChild(6).gameObject;
			discBillboard.transform.localScale = Vector3.one * 0.4f;
			var indicator = effect.transform.GetChild(4).gameObject;
			indicator.transform.localScale = Vector3.one * 0.75f;
			return effect;
		}
	}

	internal class ChainReactionProjectile : Asset, IProjectile // you need this because you can't set the crabcontroller transformation projectile to itself, it crashes CodedAssets
	{
		GameObject IProjectile.BuildObject()
		{
			var proj = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonStuckProjectile1.prefab")!.InstantiateClone("ChainReactionProjectile", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = GetGameObject<PrimedStickyBomb, IProjectileGhost>();
			ProjectileImpactExplosion impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.impactEffect = GetGameObject<PrimedStickyBomb, IEffect>();
			var crabController = proj.GetComponent<MegacrabProjectileController>();
			crabController.whiteToBlackTransformedProjectile = proj; //you could swap this for different chain-reaction visuals if you wanted
			crabController.whiteToBlackTransformationRadius = 7.5f;
			return proj;
		}
	}
}