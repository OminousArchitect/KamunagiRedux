﻿using EntityStates.LunarWisp;
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

			var cPairs = mdl.GetComponent<ChildLocator>().transformPairs;
			cPairs[0].transform = mdl.transform;
			cPairs[0].name = "Muzzle";
			
			#region itemdisplays
			var idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
			idrs.keyAssetRuleGroups = charModel.itemDisplayRuleSet.keyAssetRuleGroups;
			
			var hauntedDisplay = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/EliteHaunted/EliteHauntedEquipment.asset"));
			hauntedDisplay.rules[0].childName = "Muzzle";
			hauntedDisplay.rules[0].localPos = new Vector3(-0.02014F, 0.18649F, -0.16408F);
			hauntedDisplay.rules[0].localAngles = new Vector3(270F, 0F, 0F);
			hauntedDisplay.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			/*hauntedDisplay.rules[1].childName = "Muzzle";
			hauntedDisplay.rules[1].localPos = new Vector3(-0.34832F, 0.26794F, 0.14957F);
			hauntedDisplay.rules[1].localAngles = new Vector3(52.33278F, 60.16898F, 218.7332F);
			hauntedDisplay.rules[1].localScale = new Vector3(-0.40586F, 0.40586F, 0.40586F);*/
			
			var iceDisplay = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/EliteIce/EliteIceEquipment.asset"));
			iceDisplay.rules[0].childName = "Muzzle";
			iceDisplay.rules[0].localPos = new Vector3(0.0189F, 1.05928F, 0.03792F);
			iceDisplay.rules[0].localAngles = new Vector3(270F, 0F, 0F);
			iceDisplay.rules[0].localScale = new Vector3(0.08F, 0.08F, 0.08F);
			/*iceDisplay.rules[1].childName = "Muzzle";
			iceDisplay.rules[1].localPos = new Vector3(0.04168F, 0.95129F, 0.15072F);
			iceDisplay.rules[1].localAngles = new Vector3(335.6771F, 357.8F, 180F);
			iceDisplay.rules[1].localScale = new Vector3(-0.40586F, 0.40586F, 0.40586F);*/
			
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