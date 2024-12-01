using BepInEx.Configuration;
using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Special;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	[HarmonyPatch]
	public class TatariBody : Concentric, IBody, IMaster
	{
		private static ConfigEntry<string> debuffBlacklist;
		
		private static BuffIndex[] _eligibleDebuffs;
		public static BuffIndex[] EligibleDebuffs
		{
			get
			{
				if (_eligibleDebuffs == null)
				{
					DebuffBlacklistOnSettingChanged(null, null);
				}
				return _eligibleDebuffs;
			}
		}
		public static BodyIndex _tatariIndex;
		public static BodyIndex tatariIndex
		{
			get
			{
				if (_tatariIndex == (BodyIndex) 0)
				{
					var tatari = GetBody<TatariBody>().WaitForCompletion();
					_tatariIndex = tatari.GetComponent<CharacterBody>().bodyIndex;
				}
				return _tatariIndex;
			}
		}

		[HarmonyPrefix, HarmonyPatch(typeof(FootstepHandler), nameof(FootstepHandler.Footstep), typeof(string), typeof(GameObject))]
		public static void FootStepReplacer(FootstepHandler __instance, ref GameObject footstepEffect)
		{
			if (__instance.body.bodyIndex == tatariIndex)
			{
				footstepEffect = __instance.footstepDustPrefab;
			}
		}

		async Task<GameObject> IBody.BuildObject()
		{
			Material tatariMat = new Material(await LoadAsset<Material>("RoR2/DLC1/Gup/matGupBodySimple.mat"));
			tatariMat.SetColor("_Color", new Color(0.33f, 0.22f, 0.78f));
			tatariMat.SetColor("_EmColor", new Color(1f, 0f, 0.87f));
			tatariMat.SetFloat("_EmPower", 0.67f);
			tatariMat.SetFloat("_SpecularStrength", 0.05f);
			tatariMat.SetFloat("_SpecularExponent", 3.2f);
			tatariMat.SetFloat("_FlowSpeed", 15f);
			//mat.SetFloat("");
			
			var gupBody = (await LoadAsset<GameObject>("RoR2/DLC1/Gup/GupBody.prefab")).InstantiateClone("TatariBody", true);
			gupBody.GetComponent<CharacterDeathBehavior>().deathState =
				new SerializableEntityStateType(typeof(VoidDeathState));
			var legs = gupBody.transform.Find("ModelBase/mdlGup/mdlGup.003").gameObject;
			legs.GetComponent<SkinnedMeshRenderer>().enabled = false; //attempt #1
			GameObject model = gupBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			//var omfg = model.AddComponent<FootstepReplacer>();
			//omfg.dust = await (GetGenericObject<TatariStepDust>());

			var handler = model.GetComponent<FootstepHandler>();
			handler.enableFootstepDust = false; // we dont actually want to use this, because we're gonna use it in the hook
			handler.footstepDustPrefab = await GetEffect<TatariStepDust>();

			CharacterModel mdl = model.GetComponent<CharacterModel>();
			mdl.baseRendererInfos[0].defaultMaterial = tatariMat;
			mdl.baseRendererInfos[1].renderer.enabled = false; //attempt #2
			var cb = gupBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "TATARI_BODY_NAME";
			cb.baseDamage = 14f;
			cb.portraitIcon= (await LoadAsset<Texture>("kamunagiassets2:TatariIcon"));

			var lr = model.AddComponent<LegRemover>();
			foreach (SkinnedMeshRenderer m in model.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				switch (m.name)
				{
					case "mdlGup.003":
						lr.legs = m.gameObject;
						break;
				}
			}
			return gupBody;
		}

		async Task<GameObject> IMaster.BuildObject()
		{
			var tatariMaster= (await LoadAsset<GameObject>("RoR2/DLC1/Gup/GupMaster.prefab"))!.InstantiateClone("LargeTatariMaster", true);
			tatariMaster.AddComponent<SetDontDestroyOnLoad>();
			tatariMaster.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();;
			return tatariMaster;
		}

		public override async Task Initialize()
		{
			await base.Initialize();
			debuffBlacklist = KamunagiOfChains.KamunagiOfChainsPlugin.instance.Config.Bind("Tatari", "DebuffBlacklist",
				$"bdBleeding bdBlight bdOnFire bdFracture bdDisableAllSkills bdSuperBleed bdLunarSecondaryRoot bdlunarruin bdNullifyStack bdNullified bdOverheat bdPoisoned bdPulverizeBuildup bdLunarDetonationCharge bdSoulCost bdStrongerBurn " +
				$"{(await GetBuffDef<NaturesAxiom>()).name} {(await GetBuffDef<AxiomBurn>()).name} {(await GetBuffDef<NaturesAxiom>()).name} {(await GetBuffDef<SobuGekishoha>()).name} {(await GetBuffDef<WoshisZone>()).name}" +
				$"{(await GetBuffDef<MashiroBlessing>()).name}",
				"description");

			RoR2.GlobalEventManager.onServerDamageDealt += GlobalEventManagerOnonServerDamageDealt;
			debuffBlacklist.SettingChanged += DebuffBlacklistOnSettingChanged;
		}

		private static void DebuffBlacklistOnSettingChanged(object sender, EventArgs eventArgs)
		{
			var blacklist = debuffBlacklist.Value.Split(' ')
				.Select(str => BuffCatalog.FindBuffIndex(str))
				.Where(index => index != BuffIndex.None);
			_eligibleDebuffs = BuffCatalog.debuffBuffIndices.Except(blacklist).ToArray();
		}

		private static void GlobalEventManagerOnonServerDamageDealt(DamageReport damageReport)
		{
			if (damageReport.attackerBody == null) return;
			if (damageReport.victimBody.bodyIndex != tatariIndex) return;
			if (!Util.CheckRoll((1 - damageReport.victimBody.healthComponent.health / damageReport.victimBody.healthComponent.fullHealth) * 100, damageReport.victimMaster)) return;
			var theDebuff = EligibleDebuffs[UnityEngine.Random.Range(0, EligibleDebuffs.Length)];
			damageReport.attackerBody.AddTimedBuff(theDebuff, 3f);
		}

		public class LegRemover : MonoBehaviour
		{
			public GameObject legs;

			public void Awake()
			{
				legs.SetActive(false);
			}
		}
		
		public class FootstepReplacer : MonoBehaviour
		{
			private Animator animator;
			public GameObject dust;
			public void Start()
			{
				animator = GetComponent<Animator>();

				foreach (var clip in animator.runtimeAnimatorController.animationClips)
				{
					foreach (var animEvent in clip.events)
					{
						if (animEvent.stringParameter == "StepPosition")
						{
							animEvent.objectReferenceParameter = dust;
						}
					}
				}
			}
		}
	}

	public class TatariStepDust : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			Material purpleGooMat = new Material(await LoadAsset<Material>("RoR2/DLC1/Gup/matGupBlood.mat"));
			purpleGooMat.name = "TatariBlood";
			purpleGooMat.SetColor("_EmissionColor", new Color32(30, 0, 10, 255)); //255 color casting!!!!
			purpleGooMat.SetColor("_TintColor", new Color32(109, 14, 31, 255));
			
			var dust = (await LoadAsset<GameObject>("RoR2/DLC1/Gup/GupStep.prefab"))!.InstantiateClone("TatariStepDust", false);
			var gooParticles = dust.transform.Find("Particles/Goo").gameObject;
			gooParticles.GetComponent<ParticleSystemRenderer>().material = purpleGooMat;
			return dust;
		}
	}

	public class DeprecatedAttack : BasicMeleeAttack
	{
		public static GameObject customSpikeEffect; //swingeffectprefab
		private string playbackRateParam = "SpikeAttack.playbackRate";
		private string initialHitboxName = "InitialHitBox";
		private string initialHitboxActiveParameter = "SpikeAttack.initialHitBoxActive";
		public static NetworkSoundEventDef sound;
		private float crossfadeDuration = 0.2f;
		private string animationStateName = "SpikeAttack";
		private string animationLayerName = "Body";
		public float baseDuration = 2f;
		public float damageCoefficient = 3f;
		public string hitBoxGroupName = "Spikes";
		public static GameObject prefab;
		public float procCoefficient = 1f;
		public float pushAwayForce = 2500f;
		public float hitPauseDuration = 0.05f;
		public string swingEffectMuzzleString = "Root";
		public string mecanimHitboxActiveParameter = "SpikeAttack.innerHitBoxActive";
		public string beginStateSoundString = "Play_gup_attack1_charge";
		public string beginSwingSoundString = "Play_gup_attack1_shoot";
		protected Animator animator;
		private Run.FixedTimeStamp meleeAttackStartTime = Run.FixedTimeStamp.positiveInfinity;
		private bool forceFire;
		protected EffectManagerHelper _emh_swingEffectInstance;
		private HitBox initialHitBox;

		public override void OnEnter()
		{
			impactSound = sound;
			swingEffectPrefab = customSpikeEffect;
			hitEffectPrefab = prefab;
			animator = GetModelAnimator();
			base.OnEnter();
			var mdl = GetModelTransform().gameObject;
			var uhh = mdl.GetComponent<HitBoxGroup>();
			if (uhh)
			{
				hitBoxGroup = uhh;
			}
			if (characterDirection)
			{
				base.characterDirection.moveVector = base.characterDirection.forward;
			}
			if (!hitBoxGroup)
			{
				return;
			}
			HitBox[] hitBoxes = hitBoxGroup.hitBoxes;
			foreach (HitBox hitBox in hitBoxes)
			{
				if (hitBox.gameObject.name == initialHitboxName)
				{
					initialHitBox = hitBox;
					break;
				}
			}
		}
		
		public override void BeginMeleeAttackEffect()
		{
			if (meleeAttackStartTime != Run.FixedTimeStamp.positiveInfinity)
			{
				return;
			}
			meleeAttackStartTime = Run.FixedTimeStamp.now;
			Util.PlaySound(beginSwingSoundString, base.gameObject);
			if (customSpikeEffect)
			{
				Transform transform = base.FindModelChild(swingEffectMuzzleString);
				if (transform)
				{
					if (!EffectManager.ShouldUsePooledEffect(customSpikeEffect))
					{
						swingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(customSpikeEffect, transform);
					}
					else
					{
						_emh_swingEffectInstance = EffectManager.GetAndActivatePooledEffect(customSpikeEffect, transform, true);
						swingEffectInstance = _emh_swingEffectInstance.gameObject;
					}
					ScaleParticleSystemDuration component = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
					if (component)
					{
						component.newDuration = component.initialDuration;
					}
				}
			}
		}

		public override void PlayAnimation()
		{
			PlayCrossfade(animationLayerName, animationStateName, playbackRateParam, 2, crossfadeDuration);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (initialHitBox)
			{
				initialHitBox.enabled = animator.GetFloat(initialHitboxActiveParameter) > 0.5f;
			}
		}
	}

	public class ThisDoesntWorkEither : EntityStates.Gup.GupSpikesState
	{
		public static GameObject customSpikeEffect;
		public static NetworkSoundEventDef sound;
		public static GameObject prefab;
		public override void OnEnter()
		{
			swingEffectPrefab = customSpikeEffect;
			base.OnEnter();
		}
	}
	
	public class TatariPrimary : Concentric, ISkill, IEffect
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			ThisDoesntWorkEither.customSpikeEffect = await this.GetEffect();
			ThisDoesntWorkEither.sound = await LoadAsset<NetworkSoundEventDef>("RoR2/Base/Bandit2/nseBandit2ShivHit.asset");
			ThisDoesntWorkEither.prefab = await LoadAsset<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXMedium.prefab");
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Body";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 6f;
			skill.icon = (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}

		async Task<GameObject> IEffect.BuildObject()
		{ 
			Material purpleGooMat = new Material(await LoadAsset<Material>("RoR2/DLC1/Gup/matGupBlood.mat"));
			purpleGooMat.name = "TatariBlood";
			purpleGooMat.SetColor("_EmissionColor", new Color32(30, 0, 10, 255)); //255 color casting!!!!
			purpleGooMat.SetColor("_TintColor", new Color32(109, 14, 31, 255));
			Material otherGoo = new Material(await LoadAsset<Material>("RoR2/DLC1/Gup/matGupBloodSphere.mat"));
			otherGoo.SetColor("_EmissionColor", new Color32(30, 0, 10, 255)); //255 color casting!!!!
			otherGoo.SetColor("_TintColor", new Color32(109, 14, 31, 255));
			
			var effect = (await LoadAsset<GameObject>("RoR2/DLC1/Gup/GupSpikeAttack.prefab"))!.InstantiateClone("TatariSpikeEffect", false);
			foreach (ParticleSystemRenderer r in effect.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				switch (r.name)
				{
					case "BigBlood":
						r.material = purpleGooMat;
						break;
					case "BloodSphere":
						r.material = otherGoo;
						break;
				}
			}
			return effect;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(EntityStates.Gup.GupSpikesState) };
	}
	
	public class TatariPrimaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<TatariPrimary>() };
	}
}