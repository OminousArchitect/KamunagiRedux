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
		public static string fireAirstrikeSoundString = "Play_seeker_skill1_fire_orb";
		public static GameObject star;
		public static GameObject line;
		
		public override void OnEnter()
		{
			projectilePrefab = star;
			arcVisualizerPrefab = line;
			base.OnEnter();
			base.characterBody.SetSpreadBloom(0.4f);
			airstrikeRadius = 20f;
			maxDistance = 1000f;
			rayRadius = 0.2f;
			endpointVisualizerPrefab = EntityStates.Huntress.ArrowRain.areaIndicatorPrefab;
			damageCoefficient = 9f;
			baseMinimumDuration = 1f;

			if (NetworkServer.active && healthComponent.alive)
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.damage = base.healthComponent.combinedHealth * 0.1f;
				damageInfo.position = base.characterBody.corePosition;
				damageInfo.force = Vector3.zero;
				damageInfo.damageColorIndex = DamageColorIndex.Default;
				damageInfo.crit = false;
				damageInfo.attacker = null;
				damageInfo.inflictor = null;
				damageInfo.damageType = DamageType.NonLethal | DamageType.BypassArmor;
				damageInfo.procCoefficient = 0f;
				damageInfo.procChainMask = default(ProcChainMask);
				base.healthComponent.TakeDamage(damageInfo);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
		}

		public override void Update()
		{
			base.Update();
		}

		public override void OnExit()
		{
			Util.PlaySound(fireAirstrikeSoundString, base.gameObject);
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
	public class HeavenStarfall : Concentric, ISkill, IEffect, IProjectile, IProjectileGhost, IMaterial
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			HeavenStarfallState.star = await this.GetProjectile();
			HeavenStarfallState.line = await LoadAsset<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab");
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(HeavenStarfallState) };

		async Task<Material> IMaterial.BuildObject()
		{
			var material = new Material(await LoadAsset<Material>("RoR2/Base/Captain/matCaptainAirstrikeCore.mat"));
			material.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampVoidRaidPlanet2.png"));
			material.SetFloat("_SrcBlend", 1f);
			material.SetFloat("_DstBlend", 1f);
			material.SetFloat("_Boost", 20f);
			material.SetFloat("_AlphaBoost", 2.05f);
			material.SetFloat("_AlphaBias", 0f);
			material.name = "bright";
			return material;
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.icon= (await LoadAsset<Sprite>("kamunagiassets:lightpng"));
			skill.skillName = "Secondary 4";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SECONDARY4_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SECONDARY4_DESCRIPTION";
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 8f;
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
			proj.GetComponent<ProjectileImpactExplosion>().lifetime = 1.3f;
			proj.GetComponent<ProjectileImpactExplosion>().impactEffect = await GetEffect<StarImpact>();
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await LoadAsset<GameObject>("RoR2/Base/Captain/CaptainAirstrikeGhost1.prefab"))!.InstantiateClone("TwinsCometGhost", false);
			var fallingStarParent = ghost.transform.GetChild(2).gameObject; //airstrike
			var fallingStar = fallingStarParent.transform.GetChild(0).gameObject; //fallingprojectile
			fallingStar.GetComponent<ObjectTransformCurve>().timeMax = 1.3f;
			//I'm being lazy again and copy-pasting this :haha: 
			var darkStarCore = (await GetProjectileGhost<PrimedStickyBomb>())!.InstantiateClone("DarkStarCore", false);
			var trash = darkStarCore.transform.Find("Scaler/GameObject").gameObject;
			var trash2 = ghost.transform.Find("Expander/Sphere, Inner Expanding").gameObject;
			UnityEngine.Object.Destroy(trash);
			UnityEngine.Object.Destroy(trash2);
			UnityEngine.Object.Destroy(darkStarCore.GetComponent<ProjectileGhostController>());
			UnityEngine.Object.Destroy(darkStarCore.GetComponent<VFXAttributes>());
			UnityEngine.Object.Destroy(darkStarCore.GetComponent<EffectManagerHelper>());
			darkStarCore.transform.SetParent(fallingStar.transform);
			darkStarCore.transform.localPosition = Vector3.zero;
			Material rings = new Material(await LoadAsset<Material>("RoR2/DLC2/Child/matChildStarGlow.mat"));
			rings.SetFloat("_DstBlend", 10f);
			rings.SetColor("_TintColor", new Color32(0, 150, 255, 160));
			Material glow = new Material(await LoadAsset<Material>("RoR2/Base/Common/VFX/matGlow1.mat"));
			glow.SetColor("_TintColor", new Color32(35, 0, 255, 128));
			glow.name = "glow";
			foreach (ParticleSystemRenderer r in fallingStar.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				switch (r.name)
				{
					case "Rings":
						r.material = rings;
						break;
					case "BrightFlash":
						r.material = await this.GetMaterial();
						break;
					case "BrightFlash (1)":
						r.material = await this.GetMaterial();
						break;
					case "Soft Glow":
						r.material = glow;
						break;
				}
			}
			var line = fallingStarParent.transform.Find("FallingProjectile/Trail/TrailRenderer").gameObject;
			TrailRenderer trailR = line.GetComponent<TrailRenderer>();
			trailR.material = new Material(trailR.material);
			trailR.material.SetColor("_TintColor", new Color32(1, 16, 232, 255));
			trailR.material.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampGhost.png"));
			trailR.material.SetFloat("_Boost", 6.2f);
			var sofuckingtired = ghost.transform.Find("Expander/AreaIndicatorCenter/Rings").gameObject;
			Material ugh = new Material(await LoadAsset<Material>("RoR2/Base/Nullifier/matNullifierStarParticle.mat"));
			ugh.DisableKeyword("VERTEXCOLOR");
			ugh.SetColor("_TintColor", new Color32(127, 0, 255, 255));
			ugh.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampBottledChaos.png"));
			ugh.name = "holyfuck";
			sofuckingtired.GetComponent<ParticleSystemRenderer>().material = ugh;
			return ghost;
		}
		
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeMegaBlaster.prefab"))!.InstantiateClone("TwinsLight5Muzzle", false);
			effect.EffectWithSound("");
			return effect;
		}
	}

	public class StarImpact : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("RoR2/Base/Captain/CaptainAirstrikeImpact1.prefab"))!.InstantiateClone("StarImpactEffect", false);
			var particles = effect.transform.GetChild(1).gameObject;
			UnityEngine.Object.Destroy(particles.transform.GetChild(2).gameObject);
			UnityEngine.Object.Destroy(particles.transform.GetChild(1).gameObject);
			UnityEngine.Object.Destroy(particles.transform.GetChild(3).gameObject);
			UnityEngine.Object.Destroy(particles.transform.GetChild(5).gameObject);
			UnityEngine.Object.Destroy(particles.transform.GetChild(7).gameObject);
			var remapped1 = particles.transform.GetChild(4).gameObject;
			var remapped2 = remapped1.transform.GetChild(0).gameObject;
			remapped1.GetComponent<ParticleSystemRenderer>().material = await GetMaterial<HeavenStarfall>();
			remapped2.GetComponent<ParticleSystemRenderer>().material = await GetMaterial<HeavenStarfall>();
			effect.transform.GetChild(0).gameObject.GetComponent<Light>().color = Color.blue;
			return effect;
		}
	}
}