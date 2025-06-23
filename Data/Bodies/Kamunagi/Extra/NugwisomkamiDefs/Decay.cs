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
	public class DecaySpirit : Concentric, IBody, IMaster, ISkin //4
	{
		async Task<SkinDef> ISkin.BuildObject()
		{
			var icon = await LoadAsset<Sprite>("kamunagiassets:TwinsSkin");
			var model = (await this.GetBody()).GetComponent<ModelLocator>().modelTransform.gameObject;
			var mask = model.GetComponentInChildren<MeshRenderer>();
			var theRenderer = mask;

			model.GetComponent<ModelSkinController>().skins[0] = await this.GetSkinDef();
			
			var sdParams = ScriptableObject.CreateInstance<SkinDefParams>();
			sdParams.rendererInfos = new RoR2.CharacterModel.RendererInfo[1];
			sdParams.rendererInfos[0].renderer = theRenderer;
			sdParams.rendererInfos[0].defaultMaterial = (await LoadAsset<Material>("RoR2/Junk/AncientWisp/matAncientWisp.mat"));
			//sdParams.rendererInfos[1].renderer = thePSR 
			//sdParams.baseRendererInfos[1].defaultMaterial = fireMat; //todo check if PSR is on model //I think it still is

			sdParams.meshReplacements = new SkinDefParams.MeshReplacement[1];
			sdParams.meshReplacements[0].renderer = theRenderer;
			sdParams.meshReplacements[0].meshAddress = new AssetReferenceT<Mesh>("9fada769ddd7edf48b5af5380c459134");
			sdParams.meshReplacements[0].meshAddress.m_AssetGUID = "9fada769ddd7edf48b5af5380c459134";
			sdParams.meshReplacements[0].meshAddress.m_SubObjectName = "DecaySpiritModel";
			sdParams.meshReplacements[0].meshAddress.m_SubObjectType = "UnityEngine.Mesh, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

			return (SkinDef)ScriptableObject.CreateInstance(typeof(SkinDef), obj =>
			{
				var skinDef = (SkinDef)obj;
				ISkin.AddDefaults(ref skinDef);
				skinDef.name = "KamunagiSpirit2DefaultSkinDef";
				skinDef.nameToken = "AssassinSpirit2Skin";
				skinDef.icon = icon;
				skinDef.skinDefParams = sdParams;
				skinDef.rootObject = model;
				
				Debug.Log("trail mix");
			});
		}
		async Task<GameObject> IBody.BuildObject()
		{
			Material fireMat = new Material(await LoadAsset<Material>("RoR2/Base/Wisp/matWispFire.mat"));
			fireMat.SetFloat("_BrightnessBoost", 2.63f);
			fireMat.SetFloat("_AlphaBoost", 1.2f);
			fireMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
			fireMat.SetColor("_TintColor", new Color(0, 0.32f, 1f));
			
			var nugwisoBody= (await LoadAsset<GameObject>("RoR2/Base/Wisp/WispBody.prefab"))!.InstantiateClone("Nugwiso4", true);
			var charModel = nugwisoBody.GetComponentInChildren<CharacterModel>();
			charModel.baseLightInfos[0].defaultColor = Colors.jachdwaltColor;
			//charModel.baseRendererInfos[0].ignoreOverlays = true;
			var mdl = nugwisoBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			var thePSR = mdl.GetComponentInChildren<ParticleSystemRenderer>();
			mdl.GetComponentInChildren<HurtBox>().transform.SetParent(mdl.transform); //set parent of the hurtbox outside of the armature, so we don't destroy it, too
			thePSR.transform.SetParent(mdl.transform); //do the same to the fire particles
			UnityEngine.Object.Destroy(mdl.transform.Find("Sphere.000")); //todo when you destroy this you're not really destroying it, my guess is because it isn't loaded into memory yet
			//UnityEngine.Object.Destroy(mdl.transform.GetChild(1).gameObject); //destroy armature, we don't need it //todo you might not be allowed to do this anymore because the game seems to crash

			var blankObject = await LoadAsset<GameObject>("kamunagiassets2:DecaySpiritModel");
			var meshObject = blankObject; //did this spaghetti to prevent refactoring 
			meshObject.transform.localPosition = new Vector3(0, -2.4f, 0.4f);
			meshObject.transform.localScale = Vector3.one * 3;
			meshObject.AddComponent<MeshFilter>().mesh = (await LoadAsset<Mesh>("kamunagiassets2:IceMask"));
			var theRenderer = meshObject.AddComponent<MeshRenderer>();
			theRenderer.material = (await LoadAsset<Material>("RoR2/Junk/AncientWisp/matAncientWisp.mat"));
			//meshObject.transform.SetParent(mdl.transform); //todo does the game crash because I'm setting parent here or because it can't find the renderer in ISkin? it stops crashing when I comment this out 
			
			//
			nugwisoBody.GetComponent<Rigidbody>().mass = 300f;
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI4_BODY_NAME";
			cb.baseMaxHealth = 400;
			cb.levelMaxHealth = 130;
			cb.baseDamage = 13f;
			cb.levelDamage = 1.5f;
			cb.baseMoveSpeed = 4f;

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