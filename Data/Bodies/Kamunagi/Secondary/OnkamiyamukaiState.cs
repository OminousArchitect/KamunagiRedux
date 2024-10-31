using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class OnkamiyamukaiState : RaycastedSpellState
	{
		public override bool requireFullCharge => true;
		public override float duration => 0.5f;
		public override float failedCastCooldown => 3f;

		public override void Fire(Vector3 targetPostion)
		{
			new BlastAttack
			{
				attacker = base.gameObject,
				baseDamage = damageStat * 2f, //blast on raycast damage
				baseForce = 200f,
				crit = false,
				damageType = DamageType.Generic,
				falloffModel = BlastAttack.FalloffModel.None,
				procCoefficient = 1f,
				radius = 10f,
				position = targetPostion,
				attackerFiltering = AttackerFiltering.NeverHitSelf,
				teamIndex = base.teamComponent.teamIndex
			}.Fire();

			if (Physics.Raycast(targetPostion + Vector3.up * 1f, Vector3.down, out var hitInfo, 100f, LayerIndex.world.mask))
			{ //this is technically not even a child anymore
				GameObject pillarChild = UnityEngine.Object.Instantiate( Asset.GetProjectile<OnkamiyamukaiLightPillar>().WaitForCompletion(), hitInfo.point, Quaternion.identity);
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
					projectileDamage.damageColorIndex = Onkamiyamukai.damageColorIndex;
				}
				NetworkServer.Spawn(pillarChild);
			}
		}
	}

	public class Onkamiyamukai : Asset, ISkill, IEffect
	{
		public static DamageColorIndex damageColorIndex;

		public override async Task Initialize()
		{
			base.Initialize();
			damageColorIndex = ColorsAPI.RegisterDamageColor(new Color(0.0989387f, 0.0249644f, 0.4811321f));
		}

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
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(OnkamiyamukaiState) };
	}

	public class OnkamiyamukaiLightPillar : Asset, IProjectile, IProjectileGhost
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
}