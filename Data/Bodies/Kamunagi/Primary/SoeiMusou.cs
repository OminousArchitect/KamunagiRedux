using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	internal class SoeiMusouState : BaseTwinState
	{
		public override int meterGain => 0;
		public static GameObject MuzzlePrefab;
		public const float duration = 0.2f;

		public override void OnEnter()
		{
			base.OnEnter();
			AkSoundEngine.PostEvent("Play_voidman_m2_shoot", gameObject);
			EffectManager.SimpleMuzzleFlash(MuzzlePrefab, gameObject, twinMuzzle, false);
			if (isAuthority)
			{
				var aimRay = GetAimRay();
				ProjectileManager.instance.FireProjectile(new FireProjectileInfo
				{
					crit = RollCrit(),
					damage = characterBody.damage * 3.2f,
					force = 500,
					owner = gameObject,
					position = aimRay.origin,
					rotation = Quaternion.LookRotation(aimRay.direction),
					projectilePrefab = Concentric.GetProjectile<SoeiMusou>().WaitForCompletion(),
					useSpeedOverride = true,
					speedOverride = 105f
				});
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority || fixedAge < duration) return;
			outer.SetNextStateToMain();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	internal class SoeiMusou : Concentric, ISkill, IProjectile, IProjectileGhost
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			SoeiMusouState.MuzzlePrefab = await (LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab"))!;
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 0";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "PRIMARY0_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "PRIMARY0_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("kamunagiassets:darkpng"));
			skill.activationStateMachineName = "Weapon";
			skill.baseMaxStock = 4;
			skill.baseRechargeInterval = 1.3f;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			skill.rechargeStock = 1;
			skill.attackSpeedBuffsRestockSpeed = true;
			skill.mustKeyPress = true;
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(SoeiMusouState) };

		async Task<GameObject> IProjectile.BuildObject()
		{
			var projectile =
				(await LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab")!
					).InstantiateClone("VoidProjectileSimple");
			var controller = projectile.GetComponent<ProjectileController>();
			controller.ghostPrefab = await this.GetProjectileGhost();
			controller.procCoefficient = 1;
			var rb = projectile.GetComponent<Rigidbody>();
			rb.useGravity = true;
			var antiGrav = projectile.AddComponent<AntiGravityForce>();
			antiGrav.rb = rb;
			antiGrav.antiGravityCoefficient = 0.7f;
			return projectile;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost =
				(await LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigGhost.prefab")!
				)	.InstantiateClone("VoidProjectileSimpleGhost", false);
			var (solidParallax, (cloudRemap, _)) = ghost.GetComponentInChildren<MeshRenderer>().materials;
			solidParallax.SetTexture("_EmissionTex",
				await LoadAsset<Texture2D>("addressable:RoR2/DLC1/voidraid/texRampVoidRaidSky.png"));
			solidParallax.SetFloat("_EmissionPower", 1.5f);
			solidParallax.SetFloat("_HeightStrength", 4.1f);
			solidParallax.SetFloat("_HeightBias", 0.35f);
			solidParallax.SetFloat("_Parallax", 1f);
			solidParallax.SetColor("_Color", Colors.twinsLightColor);

			cloudRemap.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampIce.png"));
			cloudRemap.SetColor("_TintColor", Colors.twinsTintColor);
			cloudRemap.SetFloat("_AlphaBoost", 3.88f);

			var scale = ghost.AddComponent<ObjectScaleCurve>();
			scale.useOverallCurveOnly = true;
			scale.timeMax = 0.12f;
			scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
			scale.baseScale = Vector3.one * 0.6f;
			return ghost;
		}
	}
}