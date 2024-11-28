using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.Networking;
using Console = System.Console;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class HeavenStarfallState : AimThrowableBase
	{
		public float airstrikeRadius = 20f;
		public static string fireAirstrikeSoundString;
		public static GameObject star;

		public override void OnEnter()
		{
			projectilePrefab = star;
			base.OnEnter();
			base.characterBody.SetSpreadBloom(0.4f);
			airstrikeRadius = 20f;
			maxDistance = 1000f;
			rayRadius = 0.2f;
			endpointVisualizerPrefab = EntityStates.Huntress.ArrowRain.areaIndicatorPrefab;
			damageCoefficient = 10f;
			baseMinimumDuration = 1f;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.characterBody.SetAimTimer(4f);
		}

		public override void OnExit()
		{
			Util.PlaySound(fireAirstrikeSoundString, base.gameObject);
			AddRecoil(0f, 0f, -1f, -1f);
			base.OnExit();
		}

		public override void ModifyProjectile(ref FireProjectileInfo fireProjectileInfo)
		{
			base.ModifyProjectile(ref fireProjectileInfo);
			fireProjectileInfo.position = currentTrajectoryInfo.hitPoint;
			fireProjectileInfo.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
			fireProjectileInfo.speedOverride = 0f;
		}

		public override bool KeyIsDown()
		{
			return base.inputBank.skill2.down;
		}

		public override EntityState PickNextState()
		{
			return new Idle();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}

	[HarmonyPatch]
	public class HeavenStarfall : Concentric, ISkill, IEffect, IProjectile, IProjectileGhost
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			HeavenStarfallState.star = await this.GetProjectile();
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(HeavenStarfallState) };
		
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.icon= (await LoadAsset<Sprite>("bundle:lightpng"));
			skill.skillName = "Secondary 4";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SECONDARY4_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SECONDARY4_DESCRIPTION";
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 3f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = false;
			skill.keywordTokens = new[]
			{
				KamunagiAsset.tokenPrefix + "TWINSRAYCAST_KEYWORD",
				"KEYWORD_PERCENT_HP"
			};
			return skill;
		}

		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/Base/Captain/CaptainAirstrikeProjectile1.prefab"))!.InstantiateClone("TwinsCometProjectile", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await LoadAsset<GameObject>("RoR2/Base/Captain/CaptainAirstrikeGhost1.prefab"))!.InstantiateClone("TwinsCometGhost", false);
			return ghost;
		}
		
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeMegaBlaster.prefab"))!.InstantiateClone("TwinsLight5Muzzle", false);
			effect.EffectWithSound("");
			return effect;
		}
	}
}