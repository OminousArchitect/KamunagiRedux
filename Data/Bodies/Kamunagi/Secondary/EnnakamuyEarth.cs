using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class EnnakamuyEarthState : DelayedFireWithEffectState<EnnakamuyEarth>
	{
		public override float baseDuration => 1.25f;
		public override float baseDelay => 0.4f;

		public override void Fire()
		{
			base.Fire();
			if (chargeEffectInstance) chargeEffectInstance.ReturnToPool();
			var aimRay = GetAimRay();
			ProjectileManager.instance.FireProjectile(new FireProjectileInfo
			{
				crit = RollCrit(),
				damage = damageStat * 6,
				force = 500,
				owner = gameObject,
				position = aimRay.origin + (aimRay.direction * 2),
				projectilePrefab = Concentric.GetProjectile<EnnakamuyEarth>().WaitForCompletion(),
				rotation = Quaternion.LookRotation(aimRay.direction)
			});
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
	}

	public class EnnakamuyEarth : Concentric, ISkill, IEffect, IProjectile, IProjectileGhost
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.icon= (await LoadAsset<Sprite>("bundle:earthpng"));
			skill.skillName = "Secondary 1";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SECONDARY1_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SECONDARY1_DESCRIPTION";
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 2f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = false;
			skill.keywordTokens = new[] { "KEYWORD_STUNNING" };
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(EnnakamuyEarthState) };

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect =(await this.GetProjectileGhost())
				.InstantiateClone("BoulderChargeEffect", false);
			UnityEngine.Object.Destroy(effect.GetComponent<ProjectileGhostController>());
			var scale = effect.AddComponent<ObjectScaleCurve>();
			scale.useOverallCurveOnly = true;
			scale.timeMax = 0.2f;
			scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
			return effect;
		}

		async Task<GameObject> IProjectile.BuildObject()
		{
			var projectile =
				(await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab"))!.InstantiateClone("BoulderProjectile", true);
			projectile.transform.localScale = Vector3.one * 0.3f;
			projectile.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			projectile.GetComponent<ProjectileController>().procCoefficient = 1f;
			projectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 83f;
			var boulderImpact = projectile.GetComponent<ProjectileImpactExplosion>();
			boulderImpact.bonusBlastForce = new Vector3(20, 20, 20);
			boulderImpact.blastRadius = 5f;
			boulderImpact.childrenProjectilePrefab = await GetProjectile<EnnakamuyEarthChild>();
			boulderImpact.blastDamageCoefficient = 1f;
			boulderImpact.childrenDamageCoefficient = 0.43f;
			boulderImpact.falloffModel = BlastAttack.FalloffModel.None;
			projectile.GetComponent<Rigidbody>().useGravity = true;
			projectile.GetComponent<SphereCollider>().radius = 3.5f;
			return projectile;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost =
				(await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab"))!.InstantiateClone(
					"BoulderProjectileGhost", false);
			ghost.transform.localScale = Vector3.one * 0.3f;

			return ghost;
		}
	}

	public class EnnakamuyEarthChild : Concentric, IProjectile, IProjectileGhost
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			var projectile= (await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentMiniBoulder.prefab"))!.InstantiateClone("BoulderChild", true);
			projectile.GetComponent<ProjectileImpactExplosion>().falloffModel = BlastAttack.FalloffModel.None;
			projectile.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			return projectile;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost= (await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab"))!.InstantiateClone("BoulderChildGhost", false);
			var childMesh = ghost.GetComponentInChildren<MeshFilter>();
			var theRock= (await LoadAsset<Mesh>("RoR2/Base/skymeadow/SMRockAngular.fbx"));
			childMesh.mesh = theRock;
			ghost.transform.localScale = new Vector3(0.16f, -0.2f, 0.2f);
			return ghost;
		}
	}
}