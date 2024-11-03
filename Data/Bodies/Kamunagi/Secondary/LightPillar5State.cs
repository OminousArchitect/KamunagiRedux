using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using Console = System.Console;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class LightPillar5State : BaseTwinState
	{
		private float duration => 0.5f;
		private Vector3 hitpoint;
		public void Shoot()
		{
			var aimRay = GetAimRay();
			BulletAttack bullet = new BulletAttack();
			bullet.maxDistance = 1000;
			bullet.stopperMask = LayerIndex.entityPrecise.mask | LayerIndex.world.mask;
			bullet.owner = base.gameObject;
			bullet.weapon = base.gameObject;
			bullet.origin = aimRay.origin;
			bullet.aimVector = aimRay.direction;
			bullet.minSpread = 0;
			bullet.maxSpread = 0.4f;
			bullet.bulletCount = 1U;
			bullet.damage = base.damageStat * 1f;
			bullet.force = 155;
			bullet.tracerEffectPrefab = null;
			bullet.muzzleName = twinMuzzle;
			bullet.hitEffectPrefab = null;
			bullet.isCrit = base.RollCrit();
			bullet.radius = 1.5f;
			bullet.procCoefficient = 0.1f;
			bullet.smartCollision = true;
			bullet.damageType = DamageType.BypassArmor;
			bullet.falloffModel = BulletAttack.FalloffModel.None;
			bullet.hitCallback = delegate(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
			{
				hitpoint = hitInfo.point;
				return true;
			};
			bullet.AddModdedDamageType(LightPillar5.Onkamiyamukai);
			bullet.Fire();
			
			if (hitpoint == Vector3.zero) return;
			if (!Physics.Raycast(hitpoint + Vector3.up * 1f, Vector3.down, out var hitInfo, 100f, LayerIndex.world.mask)) return; //this is technically not even a child anymore
			
			ProjectileManager.instance.FireProjectile(
				Concentric.GetProjectile<NightshadePrison>().WaitForCompletion(),
				hitInfo.point,
				Quaternion.identity,
				base.gameObject,
				damageStat * 1f,
				0f,
				false,
				target: null
			);
			
			#region uhhhh
			/*
			GameObject dotZone = UnityEngine.Object.Instantiate( Concentric.GetProjectile<NightshadePrison>().WaitForCompletion(), hitInfo.point, Quaternion.identity);
			ProjectileController controller = dotZone.GetComponent<ProjectileController>();
			if (controller)
			{
				controller.procChainMask = default;
				controller.procCoefficient = 1f;
				controller.Networkowner = gameObject;
			}
			var filter = dotZone.GetComponent<TeamFilter>();
			filter.teamIndex = teamComponent.teamIndex;
			ProjectileDamage projectileDamage = dotZone.GetComponent<ProjectileDamage>();
			if (projectileDamage)
			{
				projectileDamage.damage = damageStat * 1f; //dotzone "parent" damage
				projectileDamage.crit = base.RollCrit();
				projectileDamage.force = 0f;
				projectileDamage.damageColorIndex = LightPillar5.damageColorIndex;
			}*/
			#endregion uhhhhh
			//NetworkServer.Spawn(dotZone);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;
			if (fixedAge >= 0.25f)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			Shoot();
			base.OnExit();
		}
	}

	[HarmonyPatch]
	public class LightPillar5 : Concentric, ISkill, IEffect
	{
		public static DamageColorIndex damageColorIndex;
		public static DamageAPI.ModdedDamageType Onkamiyamukai;
		public override async Task Initialize()
		{
			base.Initialize();
			damageColorIndex = ColorsAPI.RegisterDamageColor(new Color(0.0989387f, 0.0249644f, 0.4811321f));
			Onkamiyamukai = DamageAPI.ReserveDamageType();
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(LightPillar5State) };
		
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
			return skill;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeMegaBlaster.prefab"))!.InstantiateClone("TwinsLight5Muzzle", false);
			effect.EffectWithSound("");
			return effect;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamageProcess))]
		private static void TakeDamageProcess(HealthComponent __instance, DamageInfo damageInfo)
		{
			if (damageInfo.HasModdedDamageType(Onkamiyamukai))
			{
				if (!__instance.body.isFlying && __instance.body.bodyIndex != pestIndex && __instance.body.bodyIndex != vultureIndex) return;
				var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
				if (!Physics.Raycast(damageInfo.position + Vector3.up * 1f, Vector3.down, out var hitInfo, 100f,
					    LayerIndex.world.mask)) return;

				GameObject projectile = UnityEngine.Object.Instantiate(
					Concentric.GetProjectile<LightPillar5Projectile>().WaitForCompletion(), hitInfo.point,
					Quaternion.identity);
				ProjectileController controller = projectile.GetComponent<ProjectileController>();
				if (controller)
				{
					controller.procChainMask = default;
					controller.procCoefficient = 1f;
					controller.Networkowner = damageInfo.attacker;
				}

				var filter = projectile.GetComponent<TeamFilter>();
				filter.teamIndex = attackerBody.teamComponent.teamIndex;
				ProjectileDamage projectileDamage = projectile.GetComponent<ProjectileDamage>();
				if (projectileDamage)
				{
					projectileDamage.damage = attackerBody.damage * 1f; //pillar damage
					projectileDamage.crit = false;
					projectileDamage.force = 0f;
					projectileDamage.damageColorIndex = LightPillar5.damageColorIndex;
					projectileDamage.damageType = DamageType.Generic;
				}

				NetworkServer.Spawn(projectile); //spawn projectile without ProjectileManager
			}
		}
	}

	public class LightPillar5Projectile : Concentric, IProjectile, IProjectileGhost
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/Base/Brother/BrotherFirePillar.prefab"))!.InstantiateClone("TwinsLight5Pillar", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await LoadAsset<GameObject>("RoR2/Base/Brother/BrotherFirePillarGhost.prefab"))!.InstantiateClone("TwinsLight5PillarGhost", false);
			return ghost;
		}
	}

	public class NightshadePrison : Concentric, IProjectile
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMultiBeamDotZone.prefab"))!.InstantiateClone("TwinsLight5Zone", true);
			proj.GetComponentInChildren<Light>().color = Color.red;
			proj.transform.localScale = Vector3.one * 0.7f;
			var dotZone = proj.GetComponent<ProjectileDotZone>();
			dotZone.damageCoefficient = 0.5f;
			dotZone.resetFrequency = 10f;
			dotZone.lifetime = 5f;
			return proj;
		}
	}
}