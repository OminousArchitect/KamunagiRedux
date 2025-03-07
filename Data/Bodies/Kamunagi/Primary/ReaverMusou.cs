using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	internal class ReaverMusouState : BaseTwinState
	{
		public override int meterGain => 0;
		public override void OnEnter()
		{
			base.OnEnter();
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

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (fixedAge >= 0.45f && isAuthority)
			{
				outer.SetNextStateToMain();
			}

			if (!inputBank.skill1.down)
			{
				outer.SetNextStateToMain();
			}
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
			var effect =
				LoadAsset<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab")!.InstantiateClone(
					"ReaverMusouMuzzleFlash", false);
			UnityEngine.Object.Destroy(effect.GetComponent<ShakeEmitter>());
			//UnityEngine.Object.Destroy(effect.GetComponent<Rigidbody>());
			effect.transform.GetChild(1).gameObject.SetActive(false);
			effect.transform.GetChild(4).gameObject.SetActive(false);
			//effect.transform.GetChild(5).gameObject.SetActive(false);
			effect.transform.GetChild(6).gameObject.SetActive(false);
			effect.transform.localScale = Vector3.one * 0.4f;
			var dist = effect.transform.GetChild(3).gameObject;
			var distP = dist.GetComponentInChildren<ParticleSystem>().shape;
			distP.scale = Vector3.one * 0.5f;
			var comp = effect.GetComponent<EffectComponent>();
			//comp.parentToReferencedTransform = true;
			//comp.positionAtReferencedTransform = true;
			return effect;
		}

		GameObject IProjectile.BuildObject()
		{
			var proj =
				LoadAsset<GameObject>("RoR2/Base/Nullifier/NullifierPreBombProjectile.prefab")!.InstantiateClone(
					"ReaverMusouProjectile", true);
			var impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.lifetime = 0.5f;
			impact.impactEffect = GetGameObject<ReaverMusou, IEffect>();
			//impact.blastRadius = 5f;
			var controller = proj.GetComponent<ProjectileController>();
			controller.procCoefficient = 0.8f;
			controller.ghostPrefab = GetGameObject<ReaverMusou, IProjectileGhost>();
			proj.transform.GetChild(0).gameObject.SetActive(false);
			return proj;
		}

		GameObject IProjectileGhost.BuildObject()
		{
			var ghost = LoadAsset<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsGhost.prefab")!
				.InstantiateClone("ReaverMusouGhost", false);
			ghost.transform.localScale = Vector3.one;

			var reaveMesh = ghost.GetComponentInChildren<MeshRenderer>();
			reaveMesh.material.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampBanditSplatter.png"));
			reaveMesh.material.SetColor("_TintColor", new Color(0.5411765f, 0.1176471f, 1f));
			var flickerPurple = ghost.transform.GetChild(1).gameObject;
			var lightC = flickerPurple.GetComponent<Light>();
			lightC.color = new Color(0.4333f, 0.0726f, 0.8925f);

			flickerPurple.transform.localPosition = new Vector3(0f, 0.7f, 0f);
			return ghost;
		}
	}
}