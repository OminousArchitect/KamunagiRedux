using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Special;
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
		public static GameObject? missilePrefab;
		public static GameObject? muzzleFlash;
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

			if (timeBetweenShots > twinBehaviour.firingDelay && isAuthority)
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
			float d = twinBehaviour.radius + characterBody.radius * 1f;
			Quaternion rotation = Util.QuaternionSafeLookRotation(vector);
			Vector3 b = Quaternion.AngleAxis((remainingMissilesToFire - 1) * intervalDegrees - intervalDegrees * (missiles - 1) / 2f, vector) * Vector3.up * (d - 0.5f);
			Vector3 position = characterBody.aimOrigin + b;
			FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
			{
				projectilePrefab = missilePrefab,
				position = position,
				rotation = rotation,
				owner = base.gameObject,
				damage = characterBody.damage * 1f,
				crit = RollCrit(),
				force = 200f
			};
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			EffectManager.SimpleMuzzleFlash(muzzleFlash, gameObject, twinMuzzle, false);
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	[HarmonyPatch]
	public class MultiMusou : Concentric, ISkill, IProjectile, IProjectileGhost, IEffect
	{
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(MultiMusouState) };

		[HarmonyPrefix, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamageProcess))]
		private static void TakeDamageProcess(HealthComponent __instance, DamageInfo damageInfo)
		{
			if (damageInfo.HasModdedDamageType(CurseFlames))
			{
				DotController.InflictDot(
					__instance.gameObject,
					damageInfo.attacker,
					NaturesAxiom.CurseIndex, 
					2.4f, 
					damageInfo.damage * 0.2f
				);
			}
		}
		
		public override async Task Initialize()
		{
			await base.Initialize();
			MultiMusouState.missilePrefab = await this.GetProjectile();
			MultiMusouState.muzzleFlash = await (LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab"))!;
		}
		
		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/Base/EliteLunar/LunarMissileProjectile.prefab"))!.InstantiateClone("MultiMusouProjectile", true);
			UnityEngine.Object.Destroy(proj.GetComponent<BoxCollider>());
			proj.AddComponent<SphereCollider>().radius = 0.75f;
			var controller = proj.GetComponent<ProjectileController>();
			controller.ghostPrefab = await this.GetProjectileGhost();
			controller.startSound = "Play_item_use_molotov_throw";
			proj.GetComponent<ProjectileSimple>().desiredForwardSpeed = 150f;
			UnityEngine.Object.Destroy(proj.GetComponent<ProjectileSteerTowardTarget>());
			UnityEngine.Object.Destroy(proj.GetComponent<ProjectileDirectionalTargetFinder>());
			proj.GetComponent<ProjectileSingleTargetImpact>().impactEffect = await this.GetEffect();
			proj.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(CurseFlames);
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			Material fireMat = new Material(await LoadAsset<Material>("RoR2/Base/Common/VFX/matFireStaticBlueLarge.mat"));
			fireMat.name = "MusouFire";
			fireMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/texRampDeathBomb.png"));
			fireMat.SetFloat("_SrcBlend", 1f);
			fireMat.SetFloat("_InvFade", 0.893f);
			fireMat.SetFloat("_Boost", 3.7f);
			fireMat.SetFloat("_AlphaBoost", 3.7f);
			fireMat.SetFloat("_DistortionStrength", 0.82f);
			fireMat.SetColor("_TintColor", new Color32(193, 0, 255, 255));
			
			var ghost = (await LoadAsset<GameObject>("RoR2/Base/bazaar/Bazaar_Light.prefab"))!.transform.Find("FireLODLevel/BlueFire").gameObject!.InstantiateClone("MultiMusouGhost", false);
			ghost.GetComponent<ParticleSystemRenderer>().material = fireMat;
			ghost.AddComponent<ProjectileGhostController>();
			var l = ghost.AddComponent<Light>();
			l.intensity = 70f;
			l.range = 3f;
			l.color = new Color32(195, 0, 255, 255);
			return ghost;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			Material puffMat = new Material(await LoadAsset<Material>("RoR2/Base/Common/VFX/matOmniExplosion1.mat"));
			puffMat.name = "MusouPuff";
			puffMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampNullifierOffset.png"));
			
			var effect = (await LoadAsset<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab"))!.InstantiateClone("DarkFlameImpact", false);
			var spark = effect.transform.GetChild(0).gameObject; //hitspark
			spark.GetComponent<ParticleSystemRenderer>().material = await LoadAsset<Material>("RoR2/DLC1/Common/Void/matOmniHitspark1Void.mat");
			
			var puff = effect.transform.GetChild(9).gameObject; //flame impact
			puff.GetComponent<ParticleSystemRenderer>().material = puffMat;
			
			var light = effect.transform.GetChild(11).gameObject;
			light.GetComponent<Light>().color = Colors.twinsLightColor;
			return effect;
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 3";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "PRIMARY3_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "PRIMARY3_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("kamunagiassets:darkpng"));
			skill.activationStateMachineName = "Weapon";
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = false;
			skill.cancelSprintingOnActivation = false;
			skill.beginSkillCooldownOnSkillEnd = false;
			skill.baseRechargeInterval = 2.5f;
			skill.keywordTokens = new[] { KamunagiAsset.tokenPrefix + "TWINSCURSE1_KEYWORD" };
			return skill;
		}
	}
}