using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class MikazuchiState : RaycastedSpellState
	{
		public EffectManagerHelper? chargeEffectInstance;
		public float projectileCount = 3f;
		public override float duration => 0.7f;
		public override bool requireFullCharge => true;
		public override float failedCastCooldown => 2f;

		public override void OnEnter()
		{
			base.OnEnter();
			var muzzleTransform = FindModelChild("MuzzleCenter");
			if (!muzzleTransform) return;
			chargeEffectInstance =
				EffectManagerKamunagi.GetAndActivatePooledEffect(Concentric.GetEffect<Mikazuchi>().WaitForCompletion(),
					muzzleTransform, true);
		}

		public override void Fire(Vector3 targetPosition)
		{
			base.Fire(targetPosition);
			var blastAttack = new BlastAttack
			{
				attacker = gameObject,
				baseDamage = damageStat * twinBehaviour.runtimeNumber,
				baseForce = 1800,
				crit = RollCrit(),
				damageType = DamageType.Shock5s,
				falloffModel = BlastAttack.FalloffModel.None,
				procCoefficient = 1,
				radius = 3f,
				position = targetPosition,
				attackerFiltering = AttackerFiltering.NeverHitSelf,
				teamIndex = teamComponent.teamIndex
			};
			blastAttack.Fire();
			EffectManager.SpawnEffect(Concentric.GetEffect<MikazuchiLightningStrike>().WaitForCompletion(),
				new EffectData() { origin = targetPosition, scale = blastAttack.radius }, true);

			var xoro = new Xoroshiro128Plus(RoR2Application.rng.nextUlong);
			var spacingDegrees = 360f / projectileCount;
			var forward = Vector3.ProjectOnPlane(inputBank.aimDirection, Vector3.up);
			float multiplier = UnityEngine.Random.Range(3f, 5f);
			var centerPoint = targetPosition + (Vector3.up * multiplier);
			for (var i = 0; i < projectileCount; i++)
			{
				ProjectileManager.instance.FireProjectile(
					Concentric.GetProjectile<MikazuchiLightningOrb>().WaitForCompletion(),
					centerPoint,
					Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(spacingDegrees * i, Vector3.up) * forward),
					gameObject,
					damageStat * twinBehaviour.runtimeNumber2,
					10f,
					RollCrit(),
					speedOverride: xoro.RangeInt(20, 35)
				);
			}
		}

		public override void FixedUpdate()
		{
			if (fixedAge < 0.2f && !IsKeyDownAuthority())
			{
				outer.SetNextStateToMain();
			}
			base.FixedUpdate();
		}

		public override void OnExit()
		{
			base.OnExit();
			if (chargeEffectInstance != null)
				chargeEffectInstance.ReturnToPool();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class Mikazuchi : Concentric, IEffect, ISkill
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("kamunagiassets:MikazuchiMuzzle.prefab"))!;

			var comp = effect.GetOrAddComponent<EffectComponent>();
			comp.applyScale = false;
			comp.parentToReferencedTransform = true;
			comp.positionAtReferencedTransform = true;
			comp.effectData = new EffectData() { };
			effect.SetActive(false); // Required for pooled effects or you get a warning about effectData not being set
			var vfx = effect.GetOrAddComponent<VFXAttributes>();
			vfx.DoNotPool = false;
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;

			return effect;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(MikazuchiState) };

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Utility 0";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "UTILITY0_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "UTILITY0_DESCRIPTION";
			skill.icon = (await LoadAsset<Sprite>("kamunagiassets:Mikazuchi"));
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 2f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.isCombatSkill = false;
			skill.cancelSprintingOnActivation = false;
			skill.keywordTokens = new[]
			{
				"KEYWORD_SHOCKING",
				KamunagiAsset.tokenPrefix + "TWINSRAYCAST_KEYWORD",
			};
			return skill;
		}
	}

	public class MikazuchiLightningStrike : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect =
				(await LoadAsset<GameObject>("addressable:RoR2/Base/Lightning/LightningStrikeImpact.prefab"))!
				.InstantiateClone(
					"MikazuchiLightningStrikeImpact", false);
			effect.GetComponentInChildren<Light>().color = Color.yellow;
			var comp = effect.GetOrAddComponent<EffectComponent>();
			comp.parentToReferencedTransform = true;
			comp.positionAtReferencedTransform = true;
			comp.soundName = "Play_item_use_lighningArm";

			var postProcess = effect.transform.Find("PostProcess").gameObject;
			var pp = postProcess.GetComponent<PostProcessVolume>();
			pp.profile =
				(await LoadAsset<PostProcessProfile>("RoR2/Base/title/PostProcessing/ppLocalGrandparent.asset"));
			pp.sharedProfile = pp.profile;
			postProcess.GetComponent<PostProcessDuration>().destroyOnEnd = false;

			var rampTeleport =
				await LoadAsset<Texture2D>(
					"addressable:RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png");
			var sphereMat = (await LoadAsset<Material>("addressable:RoR2/Base/Loader/matLightningLongYellow.mat"))!;
			foreach (var particleSystemRenderer in effect.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				switch (particleSystemRenderer.name)
				{
					case "Ring":
						particleSystemRenderer.material.SetTexture("_RemapTex", rampTeleport);
						particleSystemRenderer.material.SetColor("_TintColor", Color.yellow);
						break;
					case "LightningRibbon":
						particleSystemRenderer.trailMaterial = new Material(particleSystemRenderer.trailMaterial);
						particleSystemRenderer.trailMaterial.SetTexture("_RemapTex", rampTeleport);
						break;
					case "Sphere":
						particleSystemRenderer.material = sphereMat;
						break;
					case "Flash":
						particleSystemRenderer.material.DisableKeyword("VERTEXCOLOR");
						break;
				}
			}

			return effect;
		}
	}

	public class MikazuchiLightningStrikeSilent : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = (await GetEffect<MikazuchiLightningStrike>()).InstantiateClone("MikazuchiSilentImpact", false);
			effect.GetOrAddComponent<EffectComponent>().soundName = "";
			return effect;
		}
	}

	public class MikazuchiLightningOrb : Concentric, IProjectile, IProjectileGhost
	{
		async Task<GameObject> IProjectile.BuildObject()
		{	//(await LoadAsset<GameObject>("addressable:RoR2/Base/ElectricWorm/ElectricOrbProjectile.prefab")!).InstantiateClone("MikazuchiLightningOrbProjectile");
			var projectile = (await LoadAsset<GameObject>("RoR2/DLC1/VoidBarnacle/VoidBarnacleBullet.prefab")!).InstantiateClone("MikazuchiLightningOrbProjectile");
			var controller = projectile.GetComponent<ProjectileController>();
			controller.ghostPrefab = await this.GetProjectileGhost();
			controller.procCoefficient = 0.8f;
			projectile.GetComponent<ProjectileDamage>().damageType = DamageType.Shock5s;
			var lightningImpact = projectile.GetOrAddComponent<ProjectileImpactExplosion>();
			lightningImpact.impactEffect = await GetEffect<MikazuchiLightningStrike>();
			lightningImpact.childrenProjectilePrefab = await GetProjectile<MikazuchiLightningSeeker>();
			lightningImpact.childrenDamageCoefficient = 0.5f;
			var lightpact = projectile.GetOrAddComponent<ProjectileImpactExplosion>();
			lightpact.falloffModel = BlastAttack.FalloffModel.None;
			lightpact.blastDamageCoefficient = 4.5f;
			projectile.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			projectile.GetComponent<ProjectileController>().procCoefficient = 0.6f;
			projectile.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 145f;
			projectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 80f;
			var target = projectile.GetComponent<ProjectileDirectionalTargetFinder>();
			target.lookRange = 45f;
			target.lookCone = 180f;
			target.targetSearchInterval = 0.30f;
			target.allowTargetLoss = false;
			return projectile;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await LoadAsset<GameObject>("addressable:RoR2/Base/ElectricWorm/ElectricOrbGhost.prefab"))!
				.InstantiateClone("MikazuchiLightningOrbGhost", false);
			var warbannerRamp =
				(await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampWarbanner2.png"));
			foreach (var r in ghost.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				var name = r.name;
				if (name != "SpitCore") continue;
				r.material.SetTexture("_RemapTex", warbannerRamp);
				r.material.SetColor("_TintColor", Color.yellow);
			}

			var teleporterRamp =
				await LoadAsset<Texture2D>(
					"addressable:RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png");
			var (firstTrail, (secondTrail, _)) = ghost.GetComponentsInChildren<TrailRenderer>();
			firstTrail.material.SetTexture("_RemapTex", teleporterRamp);
			secondTrail.material.SetTexture("_RemapTex", teleporterRamp);
			ghost.GetComponentInChildren<Light>().color = Color.yellow;

			return ghost;
		}
	}

	public class MikazuchiLightningSeeker : Concentric, IProjectile, IProjectileGhost
	{
		async Task<GameObject> IProjectile.BuildObject()
		{
			var projectile =
				(await LoadAsset<GameObject>("addressable:RoR2/Base/ElectricWorm/ElectricWormSeekerProjectile.prefab")!
				).InstantiateClone("MikazuchiLightningSeekerProjectile", true);
			projectile.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();

			projectile.GetComponent<ProjectileImpactExplosion>().impactEffect =
				await GetEffect<MikazuchiStakeNova>();

			return projectile;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await LoadAsset<GameObject>(
					"addressable:RoR2/Base/ElectricWorm/ElectricWormSeekerGhost.prefab"))!
				.InstantiateClone("Mikazuch iLightningSeekerGhost", false);
			ghost.GetComponentInChildren<TrailRenderer>().startColor = Color.yellow;
			return ghost;
		}
	}

	public class MikazuchiStakeNova : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect =
				(await LoadAsset<GameObject>("addressable:RoR2/Base/EliteLightning/LightningStakeNova.prefab"))!
				.InstantiateClone("MikazuchiStakeNova", false);
			effect.transform.localScale = Vector3.one * 2;
			var (novaPr, _) = effect.GetComponentsInChildren<ParticleSystemRenderer>();
			novaPr.material.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>(
					"addressable:RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png"));
			var indicator = effect.transform.GetChild(1).gameObject;
			var yellow = indicator.GetComponent<ParticleSystemRenderer>().material;
			yellow.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>(
					"addressable:RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png"));
			foreach (ParticleSystem p in effect.GetComponentsInChildren<ParticleSystem>())
			{
				var name = p.name;
				switch (name)
				{
					case "Nova Sphere":
					case "AreaIndicatorRing, Billboard":
					case "UnscaledHitsparks 1":
					case "Flash":
						var main = p.main;
						main.startColor = Color.yellow;
						break;
				}
			}

			effect.GetComponentInChildren<Light>().color = Color.yellow;

			var comp = effect.GetOrAddComponent<EffectComponent>();
			comp.parentToReferencedTransform = true;
			comp.positionAtReferencedTransform = true;
			comp.soundName = "Play_mage_m1_impact";

			return effect;
		}
	}
}