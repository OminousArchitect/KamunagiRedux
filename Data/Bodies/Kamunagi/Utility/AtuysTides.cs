using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using ThreeEyedGames;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class AtuysTidesState : BaseTwinState
	{
		public int totalProjectileCount;
		public int projectilesFired;

		public GameObject projectilePrefab = Concentric.GetProjectile<AtuysTides>().WaitForCompletion();
		public GameObject luckyProjectilePrefab = Concentric.GetProjectile<AtuysTidesLucky>().WaitForCompletion();
		private float chanceToSweep;

		public override void OnEnter()
		{
			base.OnEnter();
			//bubbetMath no idea wtf is going on here
			totalProjectileCount = Mathf.RoundToInt(10f/3f * characterBody.attackSpeed - 0.3f);
			chanceToSweep = critStat * 0.85f;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;
			const float duration = 1.4f;
			if (fixedAge >= duration || projectilesFired >= totalProjectileCount)
			{
				outer.SetNextStateToMain();
				return;
			}

			if (projectilesFired > Mathf.FloorToInt(fixedAge / (duration / totalProjectileCount))) return;
			FireTide();
		}

		private void FireTide()
		{
			var aimRay = GetAimRay();
			FireProjectileInfo unluckyInfo = new FireProjectileInfo
			{
				crit = false,
				damage = damageStat * 2.5f,
				force = 20,
				owner = gameObject,
				position = aimRay.origin,
				projectilePrefab = Concentric.GetProjectile<AtuysTides>().WaitForCompletion(),
				rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
				speedOverride = 80f
			};
			FireProjectileInfo luckyInfo = new FireProjectileInfo
			{
				crit = false,
				damage = damageStat * 2.5f,
				force = 20,
				owner = gameObject,
				position = aimRay.origin,
				projectilePrefab = Concentric.GetProjectile<AtuysTidesLucky>().WaitForCompletion(),
				rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
				speedOverride = 80f
			};
			if (Util.CheckRoll(chanceToSweep))
			{
				ProjectileManager.instance.FireProjectile(luckyInfo);
			}
			else
			{
				ProjectileManager.instance.FireProjectile(unluckyInfo);
			}
			projectilesFired++;
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class AtuysTides : Concentric, IProjectileGhost, IProjectile, ISkill
	{
		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var tidalProjectileGhost = (await LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelGhost.prefab"))!.InstantiateClone("TidalProjectileGhost", false);
			tidalProjectileGhost.transform.localScale = Vector3.one * 0.5f;
			var gPsr = tidalProjectileGhost.GetComponentInChildren<ParticleSystemRenderer>();
			var material = new Material(await LoadAsset<Material>("RoR2/Base/Common/VFX/matBloodClayLarge.mat"));
			material.SetTexture("_RemapTex", await LoadAsset<Texture2D>("kamunagiassets:geyserRemapTex"));
			material.SetColor("_Color", Colors.oceanColor);
			material.SetFloat("_AlphaCutoff", 0.13f);
			gPsr.material = material;
			var gMr = tidalProjectileGhost.GetComponentInChildren<MeshRenderer>(true);
			gMr.material = await LoadAsset<Material>("RoR2/Base/Common/VFX/matDistortion.mat");
			var blueLight = tidalProjectileGhost.AddComponent<Light>();
			blueLight.type = LightType.Point;
			blueLight.range = 8.43f;
			blueLight.color = Colors.oceanColor;
			blueLight.intensity = 20f;
			blueLight.lightShadowCasterMode = LightShadowCasterMode.NonLightmappedOnly;
			tidalProjectileGhost.GetComponent<VFXAttributes>().optionalLights = new[] { blueLight };
			return tidalProjectileGhost;
		}

		async Task<GameObject> IProjectile.BuildObject()
		{
			var tidalProjectile =
				(await LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelProjectile.prefab")).InstantiateClone(
					"TidalProjectile", true);
			var projectileController = tidalProjectile.GetComponent<ProjectileController>();
			projectileController.ghostPrefab = await this.GetProjectileGhost();
			projectileController.startSound = "Play_TidalProjectileStart";
			projectileController.procCoefficient = 0.9f;
			tidalProjectile.GetComponent<Rigidbody>().useGravity = false;
			tidalProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 80f;
			var impact = tidalProjectile.GetComponent<ProjectileImpactExplosion>();
			impact.impactEffect = await GetEffect<AtuysTidesImpact>();
			impact.falloffModel = BlastAttack.FalloffModel.None;
			tidalProjectile.GetComponent<ProjectileDamage>().damageType = DamageTypeCombo.GenericUtility | DamageType.SlowOnHit;
			return tidalProjectile;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(AtuysTidesState) };

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Utility 1";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "UTILITY1_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "UTILITY1_DESCRIPTION";
			skill.activationState = new SerializableEntityStateType(typeof(AtuysTides));
			skill.icon = await LoadAsset<Sprite>("kamunagiassets2:Atuy2"); 
			skill.activationStateMachineName = "Spell";
			skill.baseRechargeInterval = 6f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.keywordTokens = new[] { KamunagiAsset.tokenPrefix + "TWINSBLESSING_KEYWORD" };
			return skill;
		}
	}

	public class AtuysTidesLucky : Concentric, IProjectile
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			var luckyTidalProjectile = (await LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelProjectile.prefab"))!.InstantiateClone("LuckyTidalProjectile", true);
			var gImpact = luckyTidalProjectile.GetComponent<ProjectileImpactExplosion>();
			gImpact.impactEffect = await GetEffect<AtuysTidesImpact>();
			gImpact.falloffModel = BlastAttack.FalloffModel.None;
			gImpact.fireChildren = true;
			gImpact.childrenCount = 1;
			gImpact.childrenDamageCoefficient = 0.9f;
			gImpact.childrenProjectilePrefab = await GetProjectile<AtuysTidesEruption>();
			luckyTidalProjectile.GetComponent<ProjectileController>().ghostPrefab = await GetProjectileGhost<AtuysTides>();
			luckyTidalProjectile.GetComponent<ProjectileController>().startSound = "Play_TidalProjectileStart";
			luckyTidalProjectile.GetComponent<Rigidbody>().useGravity = false;
			luckyTidalProjectile.GetComponent<ProjectileDamage>().damageType = DamageTypeCombo.GenericUtility | DamageType.SlowOnHit;
			return luckyTidalProjectile;
		}
	}

	public class AtuysTidesEruption : Concentric, IProjectile, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var tidalEruptionEffect =
				(await LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierMortarExplosion.prefab"))!.InstantiateClone(
					"TidalEruptionEffect", false);
			var eruptionDecal = tidalEruptionEffect.GetComponentInChildren<Decal>();
			var eruptionDecalMaterial = new Material(await LoadAsset<Material>("RoR2/DLC1/ClayGrenadier/matClayGooDecalMediumSplat.mat"));
			eruptionDecalMaterial.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
			eruptionDecal.Material = eruptionDecalMaterial;
			tidalEruptionEffect.GetComponentInChildren<Light>().color = Colors.oceanColor;
			var sharedMat1 = new Material(await LoadAsset<Material>("RoR2/DLC1/ClayGrenadier/matClayGrenadierShockwave.mat"));
			sharedMat1.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampBombOrb.png"));
			foreach (var r in tidalEruptionEffect.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				switch (r.name)
				{
					case "Omni, Directional":
						r.enabled = false;
						break;
					case "Billboard, Big Splash":
						//r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
						r.enabled = false;
						break;
					case "Billboard, Splash":
						r.material.SetTexture("_RemapTex",
							await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
						r.material.SetFloat("_AlphaBoost", 2.45f);
						r.material.SetFloat("_NormalStrength", 5f);
						r.material.SetFloat("_Cutoff", 0.45f);
						break;
					case "Billboard, Dots":
						r.material.SetTexture("_RemapTex",
							await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
						break;
					case "Sparks, Collision":
					case "Lightning, Spark Center":
						r.enabled = false;
						break;
					case "Ring":
						r.material.SetTexture("_RemapTex",
							await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
						break;
					case "Debris":
					case "Dust,Edge":
						r.enabled = false;
						break;
					case "Ring, Out":
						r.material.SetTexture("_RemapTex",
							await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
						break;
				}
			}
			foreach (var p in tidalEruptionEffect.GetComponentsInChildren<ParticleSystem>())
			{
				switch (p.name)
				{
					case "Billboard, Directional":
					case "Billboard, Big Splash":
					case "Billboard, Splash":
					case "Dust,Edge":
					case "Ring, Out":
						var main = p.main;
						main.startColor = Colors.oceanColor;
						break;
				}
			}
			tidalEruptionEffect.EffectWithSound("Play_TidalImpact");
			return tidalEruptionEffect;
		}

		async Task<GameObject> IProjectile.BuildObject()
		{
			var tidalEruptionProjectile =
				(await LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierMortarProjectile.prefab"))!.InstantiateClone("TidalEruptionProjectile", true);
			/*geyserEruptionProjectile.GetComponent<ProjectileController>().ghostPrefab = geyserEruptionEffect;*/
			//These kinds of projectiles don't have ghosts?????? Why are they even projectiles then???? This is just functionally a blast attack???
			tidalEruptionProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = await GetEffect<AtuysTidesEruption>();
			var gParticles = tidalEruptionProjectile.GetComponentsInChildren<ParticleSystemRenderer>();
			gParticles[0].enabled = false;
			gParticles[1].enabled = false;
			var healImpact = tidalEruptionProjectile.AddComponent<ProjectileHealOwnerOnDamageInflicted>();
			healImpact.fractionOfDamage = 0.5f;
			var geyserImpact = tidalEruptionProjectile.GetComponent<ProjectileImpactExplosion>();
			geyserImpact.falloffModel = BlastAttack.FalloffModel.None;
			geyserImpact.blastDamageCoefficient = 2.1f;
			tidalEruptionProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.SlowOnHit;
			tidalEruptionProjectile.GetComponent<ProjectileController>().startSound = "";
			return tidalEruptionProjectile;
		}
	}

	public class AtuysTidesImpact : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var tidalImpactEffect = (await LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelExplosion.prefab"))!.InstantiateClone("TidalImpactEffect", false);
			tidalImpactEffect.transform.localScale = Vector3.one * 3f;
			var giDecal = tidalImpactEffect.GetComponentInChildren<Decal>();
			var giDecalMaterial = new Material(await LoadAsset<Material>("RoR2/DLC1/ClayGrenadier/matClayGooDecalMediumSplat.mat"));
			giDecalMaterial.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
			giDecal.Material = giDecalMaterial;
			var geyserParticles = tidalImpactEffect.GetComponentsInChildren<ParticleSystemRenderer>();
			geyserParticles[0].enabled = false;

			var geyserParticlesMat = new Material(await LoadAsset<Material>("RoR2/Base/Common/VFX/matBloodClayLarge.mat"));
			geyserParticlesMat.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
			geyserParticlesMat.SetFloat("_Cutoff", 0.38f);
			geyserParticles[1].material = geyserParticlesMat;

			var geyserParticles2Mat = new Material(await LoadAsset<Material>("RoR2/Base/ClayBoss/matGooTrailLegs.mat"));
			geyserParticles2Mat.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
			geyserParticles[2].material = geyserParticles2Mat;
			geyserParticles[3].enabled = false;

			tidalImpactEffect.EffectWithSound("Play_acrid_m2_explode");
			return tidalImpactEffect;
		}
	}
}