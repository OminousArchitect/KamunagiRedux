using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class LightPillar5State : RaycastedSpellState
	{
		public override bool requireFullCharge => true;
		public override float duration => 0.5f;
		public override float failedCastCooldown => 3f;

		public override void Fire(Vector3 targetPostion)
		{ 
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.attacker = base.gameObject;
			blastAttack.baseDamage = damageStat * 2f; //blast on raycast damage
			blastAttack.baseForce = 200f;
			blastAttack.crit = false;
			blastAttack.damageType = DamageType.BypassArmor;
			blastAttack.falloffModel = BlastAttack.FalloffModel.None;
			blastAttack.procCoefficient = 1f;
			blastAttack.radius = 10f;
			blastAttack.position = targetPostion;
			blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blastAttack.teamIndex = base.teamComponent.teamIndex;
			blastAttack.Fire();//lol, lmao even
			
			if (Physics.Raycast(targetPostion + Vector3.up * 1f, Vector3.down, out var hitInfo, 100f, LayerIndex.world.mask))
			
			{ //this is technically not even a child anymore
				/*GameObject pillarChild = UnityEngine.Object.Instantiate( Asset.GetProjectile<Light5PillarAsset>().WaitForCompletion(), hitInfo.point, Quaternion.identity);
				ProjectileController controller = pillarChild.GetComponent<ProjectileController>();
				if (controller)
				{
					controller.procChainMask = default;
					controller.procCoefficient = 1f;
					controller.Networkowner = gameObject;
				}
				var filter = pillarChild.GetComponent<TeamFilter>();
				filter.teamIndex = teamComponent.teamIndex;
				ProjectileDamage projectileDamage = pillarChild.GetComponent<ProjectileDamage>();
				if (projectileDamage)
				{
					projectileDamage.damage = damageStat * 1f; //pillar damage
					projectileDamage.crit = base.RollCrit();
					projectileDamage.force = 0f;
					projectileDamage.damageColorIndex = LightPillar5.damageColorIndex;
				}
				NetworkServer.Spawn(pillarChild);*/
			}
		}
	}

	[HarmonyPatch]
	public class LightPillar5 : Asset, ISkill, IEffect
	{
		public static DamageColorIndex damageColorIndex;
		public override async Task Initialize()
		{
			base.Initialize();
			damageColorIndex = ColorsAPI.RegisterDamageColor(new Color(0.0989387f, 0.0249644f, 0.4811321f));
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
			if (damageInfo.damageType != DamageType.BypassArmor || damageInfo.attacker.GetComponent<CharacterBody>().bodyIndex != Asset.GetBodyIndex<KamunagiAsset>().WaitForCompletion()) return;
			
			if (__instance.body.isFlying || __instance.body.bodyIndex == pestIndex || __instance.body.bodyIndex == vultureIndex)
			{//spawn projectile without ProjectileManager
				var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
				if (!Physics.Raycast(damageInfo.position + Vector3.up * 1f, Vector3.down, out var hitInfo, 100f, LayerIndex.world.mask)) return;
				GameObject projectile = UnityEngine.Object.Instantiate(
					Asset.GetProjectile<Light5PillarAsset>().WaitForCompletion(), hitInfo.point,
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
				}
				NetworkServer.Spawn(projectile);
			}
			else
			{
				var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
				if (!Physics.Raycast(damageInfo.position + Vector3.up * 1f, Vector3.down, out var hitInfo, 100f, LayerIndex.world.mask)) return;
				GameObject projectile = UnityEngine.Object.Instantiate(
					Asset.GetProjectile<NightshadePrison>().WaitForCompletion(), hitInfo.point,
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
				}
				NetworkServer.Spawn(projectile);
			}
		}
	}

	public class Light5PillarAsset : Asset, IProjectile, IProjectileGhost
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

	public class NightshadePrison : Asset, IProjectile
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