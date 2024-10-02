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

		public GameObject projectilePrefab = Asset.GetGameObject<AtuysTides, IProjectile>();
		public GameObject luckyProjectilePrefab = Asset.GetGameObject<AtuysTidesLucky, IProjectile>();

		public override void OnEnter()
		{
			base.OnEnter();
			//bubbetMath no idea wtf is going on here
			totalProjectileCount = Mathf.RoundToInt((6F * characterBody.attackSpeed) - 4.6f) + 1;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;
			const float duration = 2f;
			if (fixedAge >= duration || projectilesFired >= totalProjectileCount)
			{
				outer.SetNextStateToMain();
				return;
			}

			if (projectilesFired > Mathf.FloorToInt(duration / totalProjectileCount * fixedAge)) return;
			var aimRay = GetAimRay();
			bool wasLucky = Util.CheckRoll(critStat + 7f, characterBody.master);
			ProjectileManager.instance.FireProjectile(
				projectilePrefab = wasLucky ? luckyProjectilePrefab : projectilePrefab,
				aimRay.origin,
				Util.QuaternionSafeLookRotation(aimRay.direction),
				gameObject,
				damageStat * 3f,
				20f,
				RollCrit(),
				speedOverride: 80f
			);
			projectilesFired++;
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class AtuysTides : Asset, IProjectileGhost, IProjectile, ISkill
	{
		GameObject IProjectileGhost.BuildObject()
		{
			var tidalProjectileGhost =
				LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelGhost.prefab")!.InstantiateClone(
					"TwinsGeyserGhost", false);
			tidalProjectileGhost.transform.localScale = Vector3.one * 0.5f;
			var gPsr = tidalProjectileGhost.GetComponentInChildren<ParticleSystemRenderer>();
			var material = new Material(gPsr.material);
			material.SetTexture("_RemapTex", LoadAsset<Texture2D>("bundle:geyserRemapTex"));
			material.SetColor("_Color", Colors.oceanColor);
			material.SetFloat("_AlphaCutoff", 0.13f);
			gPsr.material = material;
			var gMr = tidalProjectileGhost.GetComponentInChildren<MeshRenderer>(true);
			gMr.material = LoadAsset<Material>("RoR2/Base/Common/VFX/matDistortion.mat");
			var blueLight = tidalProjectileGhost.AddComponent<Light>();
			blueLight.type = LightType.Point;
			blueLight.range = 8.43f;
			blueLight.color = Colors.oceanColor;
			blueLight.intensity = 20f;
			blueLight.lightShadowCasterMode = LightShadowCasterMode.NonLightmappedOnly;
			tidalProjectileGhost.GetComponent<VFXAttributes>().optionalLights = new[] { blueLight };
			return tidalProjectileGhost;
		}

		GameObject IProjectile.BuildObject()
		{
			var tidalProjectile =
				LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelProjectile.prefab")!.InstantiateClone(
					"TwinsGeyserProjectile", true);
			var projectileController = tidalProjectile.GetComponent<ProjectileController>();
			projectileController.ghostPrefab = GetGameObject<AtuysTides, IProjectileGhost>();
			projectileController.startSound = null;
			projectileController.procCoefficient = 1.2f;
			tidalProjectile.GetComponent<Rigidbody>().useGravity = false;
			tidalProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 80f;
			tidalProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect =
				GetGameObject<AtuysTidesImpact, IEffect>();
			tidalProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.SlowOnHit;
			return tidalProjectile;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(AtuysTidesState) };

		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Utility 1";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "UTILITY1_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "UTILITY1_DESCRIPTION";
			skill.activationState = new SerializableEntityStateType(typeof(AtuysTides));
			skill.icon = LoadAsset<Sprite>("bundle2:Atuy2");
			skill.activationStateMachineName = "Jet";
			skill.baseRechargeInterval = 4f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			return skill;
		}
	}

	public class AtuysTidesLucky : Asset, IProjectile
	{
		GameObject IProjectile.BuildObject()
		{
			var luckyTidalProjectile =
				LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelProjectile.prefab")!.InstantiateClone(
					"TwinsGeyserSpawnChild", true);
			var gImpact = luckyTidalProjectile.GetComponent<ProjectileImpactExplosion>();
			gImpact.impactEffect = GetGameObject<AtuysTidesImpact, IEffect>();
			gImpact.fireChildren = true;
			gImpact.childrenCount = 1;
			gImpact.childrenDamageCoefficient = 1.7f;
			gImpact.childrenProjectilePrefab = GetGameObject<AtuysTides, IProjectile>();
			luckyTidalProjectile.GetComponent<ProjectileController>().ghostPrefab =
				GetGameObject<AtuysTides, IProjectileGhost>();
			luckyTidalProjectile.GetComponent<Rigidbody>().useGravity = false;
			luckyTidalProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.SlowOnHit;
			return luckyTidalProjectile;
		}
	}

	public class AtuysTidesEruption : Asset, IProjectile, IEffect
	{
		GameObject IEffect.BuildObject()
		{
			var tidalEruptionEffect =
				LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierMortarExplosion.prefab")!.InstantiateClone(
					"TwinsGeyserEruptionEffect", false);
			var eruptionDecal = tidalEruptionEffect.GetComponentInChildren<Decal>();
			var eruptionDecalMaterial = new Material(eruptionDecal.Material);
			eruptionDecalMaterial.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
			eruptionDecal.Material = eruptionDecalMaterial;
			tidalEruptionEffect.GetComponentInChildren<Light>().color = Colors.oceanColor;
			var sharedMat1 = new Material(LoadAsset<Material>("RoR2/DLC1/ClayGrenadier/matClayGrenadierShockwave.mat"));
			sharedMat1.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampBombOrb.png"));
			foreach (var r in tidalEruptionEffect.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				switch (r.name)
				{
					case "Billboard, Big Splash":
						//r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
						r.enabled = false;
						break;
					case "Billboard, Splash":
						r.material.SetTexture("_RemapTex",
							LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
						r.material.SetFloat("_AlphaBoost", 2.45f);
						r.material.SetFloat("_NormalStrength", 5f);
						r.material.SetFloat("_Cutoff", 0.45f);
						break;
					case "Billboard, Dots":
						r.material.SetTexture("_RemapTex",
							LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
						break;
					case "Sparks, Collision":
					case "Lightning, Spark Center":
						r.enabled = false;
						break;
					case "Ring":
						r.material.SetTexture("_RemapTex",
							LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
						break;
					case "Debris":
					case "Dust,Edge":
						r.enabled = false;
						break;
					case "Ring, Out":
						r.material.SetTexture("_RemapTex",
							LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
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

			tidalEruptionEffect.EffectWithSound("Play_clayGrenadier_attack1_explode");
			return tidalEruptionEffect;
		}

		GameObject IProjectile.BuildObject()
		{
			var tidalEruptionProjectile =
				LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierMortarProjectile.prefab")!.InstantiateClone(
					"TwinsGeyserEruptionProjectile", true);
			/*geyserEruptionProjectile.GetComponent<ProjectileController>().ghostPrefab = geyserEruptionEffect;*/
			//These kinds of projectiles don't have ghosts?????? Why are they even projectiles then???? This is just functionally a blast attack???
			tidalEruptionProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = GetGameObject<AtuysTidesEruption, IEffect>();
			var gParticles = tidalEruptionProjectile.GetComponentsInChildren<ParticleSystemRenderer>();
			gParticles[0].enabled = false;
			gParticles[1].enabled = false;
			var healImpact = tidalEruptionProjectile.AddComponent<ProjectileHealOwnerOnDamageInflicted>();
			healImpact.fractionOfDamage = 0.5f;
			var geyserImpact = tidalEruptionProjectile.GetComponent<ProjectileImpactExplosion>();
			geyserImpact.falloffModel = BlastAttack.FalloffModel.None;
			geyserImpact.blastDamageCoefficient = 3f;
			tidalEruptionProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.SlowOnHit;
			return tidalEruptionProjectile;
		}
	}

	public class AtuysTidesImpact : Asset, IEffect
	{
		GameObject IEffect.BuildObject()
		{
			var tidalImpactEffect =
				LoadAsset<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelExplosion.prefab")!.InstantiateClone(
					"TwinsGeyserGhostImpact", false);
			tidalImpactEffect.transform.localScale = Vector3.one * 3f;
			var giDecal = tidalImpactEffect.GetComponentInChildren<Decal>();
			var giDecalMaterial = new Material(giDecal.Material);
			giDecalMaterial.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
			giDecal.Material = giDecalMaterial;
			var geyserParticles = tidalImpactEffect.GetComponentsInChildren<ParticleSystemRenderer>();
			geyserParticles[0].enabled = false;

			var geyserParticlesMat = new Material(geyserParticles[1].material);
			geyserParticlesMat.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
			geyserParticlesMat.SetFloat("_Cutoff", 0.38f);
			geyserParticles[1].material = geyserParticlesMat;

			var geyserParticles2Mat = new Material(geyserParticles[2].material);
			geyserParticles2Mat.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
			geyserParticles[2].material = geyserParticles2Mat;
			geyserParticles[3].enabled = false;

			tidalImpactEffect.EffectWithSound("Play_acrid_m2_explode");
			return tidalImpactEffect;
		}
	}
}