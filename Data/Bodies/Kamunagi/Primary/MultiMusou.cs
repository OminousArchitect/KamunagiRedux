using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	public class MultiMusouState : BaseTwinState
	{
		public override int meterGain => 0;
		
		private float timeBetweenShots;
		public static GameObject? lunarMissilePrefab;
		private int remainingMissilesToFire;

		public override void OnEnter()
		{
			base.OnEnter();
			remainingMissilesToFire = 4;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			timeBetweenShots += Time.fixedDeltaTime;

			if (timeBetweenShots > 0.15f)
			{
				timeBetweenShots = 0f;
				MissileBarrage();
				remainingMissilesToFire--;
				if (remainingMissilesToFire == 0)
				{
					outer.SetNextStateToMain();
				}
			}
		}
		
		void MissileBarrage()
		{
			int missiles = 4;
			
			Vector3 vector = inputBank ? inputBank.aimDirection : transform.forward;
			float intervalDegrees = 180f / missiles;
			float d = 0.5f + characterBody.radius * 1f;
			Quaternion rotation = Util.QuaternionSafeLookRotation(vector);
			Vector3 b = Quaternion.AngleAxis((remainingMissilesToFire - 1) * intervalDegrees - intervalDegrees * (missiles - 1) / 2f, vector) * Vector3.up * d;
			Vector3 position = characterBody.aimOrigin + b;
			FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
			{
				projectilePrefab = lunarMissilePrefab,
				position = position,
				rotation = rotation,
				owner = base.gameObject,
				damage = characterBody.damage * 2f,
				crit = RollCrit(),
				force = 200f
			};
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
		}

		public override void OnExit()
		{
			base.OnExit();
		}
		
		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class MultiMusou : Concentric, ISkill, IProjectile, IProjectileGhost
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			MultiMusouState.lunarMissilePrefab = await this.GetProjectile();
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(MultiMusouState) };

		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/Base/EliteLunar/LunarMissileProjectile.prefab"))!.InstantiateClone("MultiMusouProjectile", true);
			proj.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 300;
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await LoadAsset<GameObject>("RoR2/Base/EliteLunar/LunarMissileGhost.prefab"))!.InstantiateClone("MultiMusouGhost", false);
			return ghost;
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 3";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "PRIMARY3_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "PRIMARY3_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("bundle:darkpng"));
			skill.activationStateMachineName = "Weapon";
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = false;
			skill.beginSkillCooldownOnSkillEnd = false;
			skill.baseRechargeInterval = 2f;
			return skill;
		}
	}
}