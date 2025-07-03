using EntityStates;
using EntityStates.LunarWisp;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	#region BodyAndMaster
	public class DecaySpirit : Concentric, IBody, IMaster, ISkin, IModel //2
	{
		async Task<SkinDef> ISkin.BuildObject()
		{
			var model = await this.GetModel();
			var theRenderer = model.GetComponentInChildren<MeshRenderer>();
			var particles = model.GetComponentInChildren<ParticleSystemRenderer>();

			var sdParams = ScriptableObject.CreateInstance<SkinDefParams>();
			sdParams.name = "DecaySkinDefParams";
			sdParams.rendererInfos = new RoR2.CharacterModel.RendererInfo[2];
			sdParams.rendererInfos[0].renderer = theRenderer;
			sdParams.rendererInfos[0].defaultMaterial = await LoadAsset<Material>("RoR2/Junk/AncientWisp/matAncientWisp.mat");
			sdParams.rendererInfos[1].renderer = particles;
			sdParams.rendererInfos[1].defaultMaterial = await LoadAsset<Material>("RoR2/Base/GreaterWisp/matGreaterWispFire.mat");
			
			return (SkinDef)ScriptableObject.CreateInstance(typeof(SkinDef), obj =>
			{
				var skinDef = (SkinDef)obj;
				ISkin.AddDefaults(ref skinDef);
				skinDef.name = "DecayDefaultSkinDef";
				skinDef.nameToken = "DecaySkin";
				skinDef.icon = null;
				skinDef.skinDefParams = sdParams;
				skinDef.rootObject = model;
			});
		}

		async Task<GameObject> IModel.BuildObject()
		{
			var bodyPrefab = await LoadAsset<GameObject>("RoR2/Base/Wisp/WispBody.prefab");
			var wispModelCopy = bodyPrefab.transform.Find("Model Base/mdlWisp1Mouth").gameObject;
			
			var decayModel = wispModelCopy.InstantiateClone("mdlDecay", false);
			decayModel.GetComponent<CharacterModel>().baseLightInfos[0].defaultColor = Colors.jachdwaltColor;
			return decayModel;
		}
		
		IEnumerable<Concentric> IModel.GetSkins() => new Concentric[] { this };
		
		async Task<GameObject> IBody.BuildObject()
		{
			var nugwisoBody= (await LoadAsset<GameObject>("RoR2/Base/Wisp/WispBody.prefab"))!.InstantiateClone("Nugwiso4", true);
			var decayModelObject = await this.GetModel();
			
			var thePSR = decayModelObject.GetComponentInChildren<ParticleSystemRenderer>();
			var hBox = decayModelObject.GetComponentInChildren<HurtBox>();
			hBox.transform.SetParent(decayModelObject.transform); //set parent of the hurtbox outside of the armature, so we don't destroy it, too
			hBox.healthComponent = nugwisoBody.GetComponent<HealthComponent>();
			thePSR.transform.SetParent(decayModelObject.transform); //do the same to the fire particles
			var sphere0 = decayModelObject.transform.Find("Sphere.000").gameObject;
			UnityEngine.Object.Destroy(sphere0.GetComponent<SkinnedMeshRenderer>());
			UnityEngine.Object.Destroy(decayModelObject.transform.GetChild(1).gameObject); //destroy armature, we don't need it 
			sphere0.AddComponent<MeshFilter>().mesh = (await LoadAsset<Mesh>("kamunagiassets2:IceMask"));
			sphere0.AddComponent<MeshRenderer>().material = (await LoadAsset<Material>("RoR2/Junk/AncientWisp/matAncientWisp.mat"));
			sphere0.transform.localPosition = new Vector3(0, -2.4f, 0.4f);
			sphere0.transform.localScale = Vector3.one * 3;

			nugwisoBody.GetComponent<Rigidbody>().mass = 300f;
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI4_BODY_NAME";
			cb.baseMaxHealth = 400;
			cb.levelMaxHealth = 130;
			cb.baseDamage = 13f;
			cb.levelDamage = 1.5f;
			cb.baseMoveSpeed = 4f;
			
			var modelLocator = nugwisoBody.GetComponent<ModelLocator>();
			UnityEngine.Object.Destroy(modelLocator.modelTransform.gameObject);
			decayModelObject.transform.parent = modelLocator.modelBaseTransform;
			decayModelObject.transform.localPosition = Vector3.zero;
			decayModelObject.GetComponent<CharacterModel>().body = cb;
			modelLocator.modelTransform = decayModelObject.transform;

			var secondary = nugwisoBody.AddComponent<GenericSkill>();
			secondary.skillName = "NugwisoSkill2";
			secondary._skillFamily = await GetSkillFamily<DecayPrimaryFamily>();
			secondary.baseSkill = await GetSkillDef<DecaySecondary>();
			nugwisoBody.GetComponent<SkillLocator>().secondary = secondary;
			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = await GetSkillFamily<DecayPrimaryFamily>();
			return nugwisoBody;
		}

		async Task<GameObject> IMaster.BuildObject()
		{
			var master= (await LoadAsset<GameObject>("RoR2/Base/Wisp/WispMaster.prefab"))!.InstantiateClone("Nugwiso4Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();
			return master;
		}
	}
	#endregion

	public class WeakenAOE : BaseState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			var search = new SphereSearch { origin = characterBody.corePosition, radius = 25, mask = LayerIndex.entityPrecise.mask }
				.RefreshCandidates()
				.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamComponent.teamIndex))
				.FilterCandidatesByDistinctHurtBoxEntities()
				.GetHurtBoxes();
			for (int i = 0; i < search.Length; i++)
			{
				search[i].healthComponent.body.AddTimedBuffAuthority(RoR2Content.Buffs.Weak.buffIndex, 10f);
			}
		}
	}
	
	public class DecayPrimary : Concentric, ISkill
	{
		public override async Task Initialize()
		{
			await base.Initialize();
		}
		
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 5f;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(WeakenAOE) };
	}
	public class DecayPrimaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<DecayPrimary>() };
	}

	public class TBDstate : BaseState
	{
		public override void OnEnter()
		{
			base.OnEnter();
		}
	}
	public class DecaySecondary : Concentric, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 3f;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(TBDstate) };
	}
	public class DecaySecondaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<DecaySecondary>() };
	}
}