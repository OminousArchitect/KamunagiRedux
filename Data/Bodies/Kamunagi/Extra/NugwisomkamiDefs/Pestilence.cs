﻿using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	#region BodyAndMaster
	public class VirusArchWisp : Concentric, IBody, IMaster //3
	{
		async Task<GameObject> IBody.BuildObject()
		{
			Material fireMat = new Material(await LoadAsset<Material>("RoR2/Base/Gravekeeper/matArchWispFire.mat"));
			fireMat.SetColor("_TintColor", new Color(0, 0.89f, 0.03f));
			
			var nugwisoBody= (await LoadAsset<GameObject>("RoR2/Junk/ArchWisp/ArchWispBody.prefab"))!.InstantiateClone("Nugwiso3", true);
			var charModel = nugwisoBody.GetComponentInChildren<CharacterModel>();
			charModel.baseRendererInfos[1].ignoreOverlays = false;
			charModel.baseRendererInfos[1].defaultMaterial = fireMat;
			charModel.baseLightInfos[0].defaultColor = new Color(0f, 1, 0.03f);
			var mdl = nugwisoBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			var thePSR = mdl.GetComponentInChildren<ParticleSystemRenderer>();
			mdl.transform.localScale = Vector3.one * 1f;
			var firePS = mdl.transform.Find("ArchWispArmature/ROOT/Mask/Fire").gameObject;
			firePS.transform.localScale = Vector3.one * 0.6f;
			firePS.transform.localPosition = new Vector3(0, -0.1f, 0.51f);
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI3_BODY_NAME";
			
			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = await GetSkillFamily<LargeArchWispPrimaryFamily>();
			return nugwisoBody;
		}

		async Task<GameObject> IMaster.BuildObject()
		{
			var master= (await LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab"))!.InstantiateClone("Nugwiso3Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();
			return master;
		}
	}
	#endregion
	
	public class LargeArchWispPrimaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<LargeArchWispPrimary>() };
	}

	internal class LargeArchWispPrimary : Concentric, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 7f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(EntityStates.Idle /*GravekeeperMonster.Weapon.GravekeeperBarrage*/) }; //todo Gravekeeper state is broken, fix later
	}
}