using BepInEx.Configuration;
using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Special;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
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
			
			var gupBody= (await LoadAsset<GameObject>("RoR2/DLC1/Gup/GupBody.prefab"))!.InstantiateClone("TatariBody", true);
			gupBody.GetComponent<CharacterDeathBehavior>().deathState = new SerializableEntityStateType(typeof(VoidDeathState));
			var legs = gupBody.transform.Find("ModelBase/mdlGup/mdlGup.003").gameObject;
			legs.GetComponent<SkinnedMeshRenderer>().enabled = false; //attempt #1
			GameObject model = gupBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			var disableOrangeStuff = model.GetComponent<FootstepHandler>();
			disableOrangeStuff.enableFootstepDust = false;
			disableOrangeStuff.footstepDustPrefab = null;
			CharacterModel mdl = model.GetComponent<CharacterModel>();
			mdl.baseRendererInfos[0].defaultMaterial = tatariMat;
			mdl.baseRendererInfos[1].renderer.enabled = false; //attempt #2
			var cb = gupBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "TATARI_BODY_NAME";
			cb.baseDamage = 14f;
			cb.portraitIcon= (await LoadAsset<Texture>("bundle2:TatariIcon"));

			var secondary = gupBody.AddComponent<GenericSkill>();
			secondary.skillName = "TatariSecondary";
			secondary._skillFamily = await GetSkillFamily<TatariSecondaryFamily>();
			secondary.baseSkill = await GetSkillDef<TatariSecondary>();
			gupBody.GetComponent<SkillLocator>().secondary = secondary;

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
	}

	public class TatariStunAOE : BaseState
	{
		public override void OnExit()
		{
			BlastAttack blast = new BlastAttack();
			blast.attacker = gameObject;
			blast.baseDamage = 0f;
			blast.baseForce = 50f;
			blast.crit = false;
			blast.damageType = DamageType.Stun1s;
			blast.falloffModel = BlastAttack.FalloffModel.None;
			blast.procCoefficient = 2f;
			blast.radius = 8f;
			blast.position = characterBody.corePosition;
			blast.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blast.teamIndex = teamComponent.teamIndex;
			
			base.OnExit();
		}
	}
	
	public class TatariSecondary : Concentric, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Body";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 6f;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(TatariStunAOE) };
	}
	
	public class TatariSecondaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<TatariSecondary>() };
	}
}