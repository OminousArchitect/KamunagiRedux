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
	public class LightPillar5State : BaseTwinState
	{
		private float chargeTime = 0.45f;
		private Vector3 hitPoint;
		private HealthComponent? enemyHc;
		private bool charged;
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (isAuthority && fixedAge >= chargeTime)
			{
				charged = true;
				outer.SetNextStateToMain();
			}

			if (!IsKeyDownAuthority())
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			if (!isAuthority || !charged) return;
			var aimRay = GetAimRay();
			var testFlying = new BulletAttack
			{
				maxDistance = 1000f,
				stopperMask = LayerIndex.entityPrecise.mask | LayerIndex.world.mask,
				owner = gameObject,
				weapon = gameObject,
				origin = aimRay.origin,
				aimVector = aimRay.direction,
				minSpread = 0,
				maxSpread = 0f,
				bulletCount = 1U,
				damage = damageStat * 2f,
				force = 100f,
				tracerEffectPrefab = null,
				muzzleName = twinMuzzle,
				hitEffectPrefab = null,
				isCrit = RollCrit(),
				radius = 0.8f,
				procCoefficient = 0.75f,
				smartCollision = true,
				damageType = DamageType.Generic,
				hitCallback = (BulletAttack bulletAttack, ref BulletAttack.BulletHit info) =>
				{
					hitPoint = info.point;
					if (info.hitHurtBox && info.hitHurtBox.healthComponent)
						enemyHc = info.hitHurtBox.healthComponent;
					return false;
				}
			};
			testFlying.Fire();
			
			SpawnPuddle(hitPoint);
			if (enemyHc && enemyHc.body.isFlying)
				SpawnPillar(hitPoint);
		}

		void SpawnPuddle(Vector3 hitPos)
		{
			if (!Physics.Raycast(hitPos + Vector3.up * 1f, Vector3.down, out var hitInfo, 100f, LayerIndex.world.mask)) return;
			GameObject voidGooPuddle = UnityEngine.Object.Instantiate( Concentric.GetProjectile<LightPool5>().WaitForCompletion(), hitInfo.point, Quaternion.identity);
			ProjectileController controller = voidGooPuddle.GetComponent<ProjectileController>();
			if (controller)
			{
				controller.procChainMask = default;
				controller.procCoefficient = 1f;
				controller.Networkowner = gameObject;
			}
			var filter = voidGooPuddle.GetComponent<TeamFilter>();
			filter.teamIndex = teamComponent.teamIndex;
			ProjectileDamage projectileDamage = voidGooPuddle.GetComponent<ProjectileDamage>();
			if (projectileDamage)
			{
				projectileDamage.damage = damageStat * 1f; //pillar damage
				projectileDamage.crit = base.RollCrit();
				projectileDamage.force = 0f;
				projectileDamage.damageColorIndex = LightPillar5.damageColorIndex;
			}
			NetworkServer.Spawn(voidGooPuddle);
		}

		void SpawnPillar(Vector3 hitPos)
		{
			if (Physics.Raycast(hitPos + Vector3.up * 1f, Vector3.down, out var hitInfo, 100f, LayerIndex.world.mask))
			{
				ProjectileManager.instance.FireProjectile(
					Concentric.GetProjectile<LightPillar5>().WaitForCompletion(),
					hitInfo.point,
					Quaternion.identity,
					gameObject,
					damageStat * 1.5f,
					0f,
					false,
					LightPillar5.damageColorIndex
				);
			}
		}
	}

	[HarmonyPatch]
	public class LightPillar5 : Concentric, ISkill, IEffect, IProjectile, IProjectileGhost
	{
		public static DamageColorIndex damageColorIndex;
		public override async Task Initialize()
		{
			await base.Initialize();
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
			skill.keywordTokens = new[] { KamunagiAsset.tokenPrefix + "TWINSRAYCAST_KEYWORD" };
			return skill;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeMegaBlaster.prefab"))!.InstantiateClone("TwinsLight5Muzzle", false);
			effect.EffectWithSound("");
			return effect;
		}
		
		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/Base/Brother/BrotherFirePillar.prefab"))!.InstantiateClone("TwinsLight5Pillar", true);
			proj.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			proj.GetComponent<HitBoxGroup>().hitBoxes[0].gameObject.transform.localScale = new Vector3(4f, 150, 4f);
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await LoadAsset<GameObject>("RoR2/Base/Brother/BrotherFirePillarGhost.prefab"))!.InstantiateClone("TwinsLight5PillarGhost", false);
			foreach (ParticleSystem p in ghost.GetComponentsInChildren<ParticleSystem>())
			{
				switch (p.name)
				{
					case "Glow, Looping":
						var shape = p.shape;
						shape.scale = new Vector3(1f, 1f, 15f); //3rd attempt at scaling this up, shape.scale also doesn't make a difference and I'm gonna give up on this for now
						break;
				}
			}
			foreach (ParticleSystemRenderer r in ghost.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				switch (r.name)
				{
					case "Glow, Looping":
						r.material = new Material(r.material);
						r.material.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampHippoVoidEye.png"));
						r.material.SetFloat("_DstBlendFloat", 1f);
						break;
					case "Glow, Initial":
						r.material = new Material(r.material);
						r.material.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampHippoVoidEye.png"));
						r.material.SetFloat("_DstBlendFloat", 1f);
						break;
				}
			}
			ghost.GetComponentInChildren<Light>().color = Colors.twinsDarkColor;
			return ghost;
		}
	}

	public class LightPool5 : Concentric, IProjectile
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			Material poolMat = new Material(await LoadAsset<Material>("RoR2/DLC1/VoidRaidCrab/matVoidRaidCrabTripleBeamDotZoneDecal.mat"));
			//poolMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>(""));
			
			var proj = (await LoadAsset<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMultiBeamDotZone.prefab"))!.InstantiateClone("TwinsLight5Zone", true);
			proj.GetComponentInChildren<Light>().color = Color.red;
			proj.transform.localScale = Vector3.one * 0.7f;
			var dotZone = proj.GetComponent<ProjectileDotZone>();
			dotZone.damageCoefficient = 0.5f;
			dotZone.resetFrequency = 10f;
			dotZone.lifetime = 5f;

			var vfxChild = proj.transform.Find("FX/ScaledOnImpact").gameObject;
			Decal decal = vfxChild.transform.GetChild(0).gameObject.GetComponent<Decal>();
			decal.Material = new Material("RoR2/DLC1/VoidRaidCrab/matVoidRaidCrabTripleBeamDotZoneDecal.mat");
			return proj;
		}
	}
}