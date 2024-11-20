using EntityStates;
using EntityStates.LunarWisp;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	#region BodyAndMaster
	public class DecaySpirit : Concentric, IBody, IMaster //4
	{
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
			UnityEngine.Object.Destroy(mdl.transform.GetChild(1).gameObject); //destroy armature, we don't need it
			var meshObject = mdl.transform.GetChild(0).gameObject;
			UnityEngine.Object.Destroy(meshObject.GetComponent<SkinnedMeshRenderer>());
			UnityEngine.Object.Destroy(mdl.GetComponentInChildren<SkinnedMeshRenderer>());
			meshObject.AddComponent<MeshFilter>().mesh= (await LoadAsset<Mesh>("bundle2:IceMask"));
			nugwisoBody.GetComponent<Rigidbody>().mass = 300f;
			var theRenderer = meshObject.AddComponent<MeshRenderer>();
			theRenderer.material= (await LoadAsset<Material>("RoR2/Junk/AncientWisp/matAncientWisp.mat"));
			charModel.baseRendererInfos[0].renderer = theRenderer;
			charModel.baseRendererInfos[0].defaultMaterial= (await LoadAsset<Material>("RoR2/Junk/AncientWisp/matAncientWisp.mat")); //mesh
			charModel.baseRendererInfos[1].renderer = thePSR;
			charModel.baseRendererInfos[1].defaultMaterial = fireMat;
			meshObject.transform.localPosition = new Vector3(0, -2.4f, 0.4f);
			meshObject.transform.localScale = Vector3.one * 3;
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI4_BODY_NAME";
			cb.baseMaxHealth = 400;
			cb.levelMaxHealth = 130;
			cb.baseDamage = 13f;
			cb.levelDamage = 1.5f;
			cb.baseMoveSpeed = 4f;
			
			#region itemdisplays
			var idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
			idrs.keyAssetRuleGroups = charModel.itemDisplayRuleSet.keyAssetRuleGroups;
			
			var fireDisplay = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/EliteHaunted/EliteHauntedEquipment.asset"));
			var fireRules = new ItemDisplayRule[fireDisplay.rules.Length];
			Array.Copy(fireDisplay.rules, fireRules, fireDisplay.rules.Length);
			fireDisplay.rules = fireRules;
			fireDisplay.rules[0].childName = "Head";
			fireDisplay.rules[0].localPos = new Vector3(0.37824F, 0.18649F, 0.31578F);
			fireDisplay.rules[0].localAngles = new Vector3(290.8627F, 338.1044F, 46.19113F);
			fireDisplay.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);

			var lightningDisplay = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/ElitePoison/ElitePoisonEquipment.asset"));
			var lightningRules = new ItemDisplayRule[lightningDisplay.rules.Length];
			Array.Copy(lightningDisplay.rules, lightningRules, lightningDisplay.rules.Length);
			lightningDisplay.rules = lightningRules;
			lightningDisplay.rules[0].childName = "Head";
			lightningDisplay.rules[0].localPos = new Vector3(0.06302F, -0.31085F, 0.46304F);
			lightningDisplay.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			lightningDisplay.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);

			charModel.itemDisplayRuleSet = idrs;
			#endregion
			
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