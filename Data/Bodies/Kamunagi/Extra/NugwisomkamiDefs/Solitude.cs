using EntityStates.LunarWisp;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	#region BodyAndMaster
	public class IceTank : Asset, IBody, IMaster //4
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
			charModel.baseLightInfos[0].defaultColor = Colors.wispNeonGreen;
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
			cb.baseMaxHealth = 200f;
			cb.baseDamage = 12f;
			cb.baseMoveSpeed = 13f;

			#region itemdisplays
			var idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
			idrs.keyAssetRuleGroups = charModel.itemDisplayRuleSet.keyAssetRuleGroups;

			var haunted = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/EliteHaunted/EliteHauntedEquipment.asset"));
			var hauntedRules = new ItemDisplayRule[haunted.rules.Length];
			Array.Copy(haunted.rules, hauntedRules, haunted.rules.Length);
			haunted.rules = hauntedRules;
			haunted.rules[0].localPos = new Vector3(-0.38F, 0.52741F, 0.16226F);
			haunted.rules[0].localAngles = new Vector3(36.87725F, 211.2191F, 2.35356F);
			haunted.rules[0].localScale = Vector3.one * 0.3f;
			haunted.rules[0].childName = "Head";

			var icy = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/EliteLightning/EliteIceEquipment.asset"));
			var icyRules = new ItemDisplayRule[icy.rules.Length];
			Array.Copy(icy.rules, icyRules, icy.rules.Length);
			icy.rules = icyRules;
			icy.rules[0].localPos = new Vector3(0.02568F, 0.77985F, 0.25021F);
			icy.rules[0].localAngles = new Vector3(339.2211F, 359.8235F, 182.5763F);
			icy.rules[0].localScale = Vector3.one * 0.7f;
			icy.rules[0].childName = "Head";
			/*icy.rules[1].localPos = new Vector3(0.04052F, -0.26059F, 0.40294F);
			icy.rules[1].localAngles = new Vector3(0f, 0f, 0f);
			icy.rules[1].localScale = Vector3.one * 0.5f;
			icy.rules[1].childName = "Child";*/
			charModel.itemDisplayRuleSet = idrs;
			#endregion
			
			var secondary = nugwisoBody.AddComponent<GenericSkill>();
			secondary.skillName = "NugwisoSkill2";
			secondary._skillFamily = await GetSkillFamily<IceTankPrimaryFamily>();
			secondary.baseSkill = await GetSkillDef<IceTankSecondary>();
			nugwisoBody.GetComponent<SkillLocator>().secondary = secondary;
			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = await GetSkillFamily<IceTankPrimaryFamily>();
			return nugwisoBody;
		}

		async Task<GameObject> IMaster.BuildObject()
		{
			var master= (await LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab"))!.InstantiateClone("Nugwiso4Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();
			return master;
		}
	}
	#endregion

	public class IceBeam : EntityStates.Wisp1Monster.ChargeEmbers
	{
		
	}
	
	public class IceTankPrimary : Asset, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 6f;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(IceBeam) };
	}
	
	public class IceTankPrimaryFamily : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<IceTankPrimary>() };
	}
	
	//secondary state goes here
	
	public class IceTankSecondary : Asset, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 6f;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(IceBeam) };
	}
	
	public class IceTankSecondaryFamily : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<IceTankSecondary>() };
	}
}