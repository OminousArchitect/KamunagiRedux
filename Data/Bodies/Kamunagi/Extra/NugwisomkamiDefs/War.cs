using EntityStates;
using EntityStates.GolemMonster;
using EntityStates.LunarWisp;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	#region BodyAndMaster

	public class WarMachine : Concentric, IBody, IMaster //2
	{
		async Task<GameObject> IBody.BuildObject()
		{
			Material wispMat = new Material(await LoadAsset<Material>("RoR2/Base/LunarWisp/matLunarWispFlames.mat"));
			//wispMat.SetFloat("_BrightnessBoost", 2.63f);
			//wispMat.SetFloat("_AlphaBoost", 1.2f);
			wispMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("bundle:purpleramp"));
			wispMat.SetColor("_TintColor", Color.white);

			var nugwisoBody = (await LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispBody.prefab"))!.InstantiateClone("Nugwiso2", true);
			var mdl = nugwisoBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			Vector3 particles = Vector3.one * 0.6f;
			mdl.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
			//mdl.transform.GetChild(1).localScale = smaller;
			mdl.transform.GetChild(2).localScale = new Vector3(0.4f, 0.4f, 0.4f);
			mdl.transform.GetChild(4).localScale = new Vector3(0.4f, 0.4f, 0.4f);
			mdl.transform.GetChild(5).localScale = new Vector3(0.6f, 0.6f, 0.6f);
			mdl.transform.GetChild(6).localScale = new Vector3(0.6f, 0.6f, 0.6f);
			mdl.GetComponentInChildren<Light>().color = Colors.twinsLightColor;
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI2_BODY_NAME";
			cb.baseMaxHealth = 200f;
			cb.levelMaxHealth = 70f;
			cb.baseDamage = 14f;
			cb.levelDamage = 3f;

			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = await GetSkillFamily<WarMachinePrimaryFamily>();
			return nugwisoBody;
		}

		async Task<GameObject> IMaster.BuildObject()
		{
			var master = (await LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab"))!.InstantiateClone("Nugwiso2Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();
			master.AddComponent<SetDontDestroyOnLoad>();
			return master;
		}
	}

	#endregion

	public class ChargeWarGuns : BaseState
	{
		public static string muzzleNameRoot = "Root";
		public static string muzzleNameOne = "MuzzleLeft";
		public static string muzzleNameTwo = "MuzzleRight";
		public static string windUpSound = "Play_Lunar_wisp_attack1_windUp";
		public static GameObject chargeEffectPrefab;
		private GameObject chargeInstance;
		private GameObject chargeInstanceTwo;
		private float duration;
		public static float spinUpDuration = 2.3f;
		public static float chargeEffectDelay = 0.2f;
		private bool chargeEffectSpawned;
		private bool upToSpeed;
		private uint loopedSoundID;
		protected Transform muzzleTransformRoot;
		protected Transform muzzleTransformOne;
		protected Transform muzzleTransformTwo;	
		public override void OnEnter()
		{
			base.OnEnter();
			duration = 0.8f;
			muzzleTransformRoot = FindModelChild(muzzleNameRoot);
			muzzleTransformOne = FindModelChild(muzzleNameOne);
			muzzleTransformTwo = FindModelChild(muzzleNameTwo);
			loopedSoundID = Util.PlaySound(windUpSound, base.gameObject);
			PlayCrossfade("Gesture", "MinigunSpinUp", 0.2f);
			if ((bool)base.characterBody)
			{
				base.characterBody.SetAimTimer(duration);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			StartAimMode(0.5f);
			if (base.fixedAge >= chargeEffectDelay && !chargeEffectSpawned)
			{
				chargeEffectSpawned = true;
				if (muzzleTransformOne && (bool)muzzleTransformTwo && (bool)chargeEffectPrefab)
				{
					chargeInstance = UnityEngine.Object.Instantiate(chargeEffectPrefab, muzzleTransformOne.position, muzzleTransformOne.rotation);
					chargeInstance.transform.parent = muzzleTransformOne;
					chargeInstanceTwo = UnityEngine.Object.Instantiate(chargeEffectPrefab, muzzleTransformTwo.position, muzzleTransformTwo.rotation);
					chargeInstanceTwo.transform.parent = muzzleTransformTwo;
					ScaleParticleSystemDuration component = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
					if (component)
					{
						component.newDuration = duration;
					}
				}
			}

			if (base.fixedAge >= spinUpDuration && !upToSpeed)
			{
				upToSpeed = true;
			}

			if (base.fixedAge >= duration && base.isAuthority)
			{
				outer.SetNextState(new FireWarGuns
					{
						muzzleTransformOne = muzzleTransformOne,
						muzzleTransformTwo = muzzleTransformTwo,
						muzzleNameOne = muzzleNameOne,
						muzzleNameTwo = muzzleNameTwo
					});
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			AkSoundEngine.StopPlayingID(loopedSoundID);
			if (chargeInstance)
			{
				EntityState.Destroy(chargeInstance);
			}

			if (chargeInstanceTwo)
			{
				EntityState.Destroy(chargeInstanceTwo);
			}
		}
	}

	class FireWarGuns : FireLunarGuns
	{
		public override void OnEnter()
		{
			base.OnEnter();
			baseDuration = 2.5f;
			baseDamagePerSecondCoefficient = 5f;
		}
	}

	internal class WarMachinePrimary : Concentric, ISkill, IEffect
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			FireWarGuns.bulletTracerEffectPrefab = await this.GetEffect();
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 8f;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			//Material tracer = new Material(await LoadAsset<Material>(""));
			
			var effect = (await LoadAsset<GameObject>("RoR2/Base/LunarWisp/TracerLunarWispMinigun.prefab"))!.InstantiateClone("Nugwiso2TracerEffect", false);
			var line = effect.transform.GetChild(2).gameObject;
			//line.GetComponent<LineRenderer>().materials = new Material[] { };
			return effect;
		}
		

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(ChargeWarGuns) };
	}
	
	public class WarMachinePrimaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<WarMachinePrimary>() };
	}
}