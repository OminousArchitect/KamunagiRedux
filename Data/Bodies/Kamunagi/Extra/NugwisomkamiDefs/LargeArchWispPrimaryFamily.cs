using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	#region BodyAndMaster
	public class TankyArchWisp : Asset, IBody, IMaster
	{
		GameObject IBody.BuildObject()
		{
			var nugwisoBody = LoadAsset<GameObject>("RoR2/Junk/ArchWisp/ArchWispBody.prefab")!.InstantiateClone("Nugwiso3", true);
			nugwisoBody.GetComponent<CharacterBody>().baseNameToken = "NUGWISOMKAMI3_BODY_NAME";
			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = GetAsset<LargeArchWispPrimaryFamily>();
			return nugwisoBody;
		}

		GameObject IMaster.BuildObject()
		{
			var master = LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab")!.InstantiateClone("Nugwiso3Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<TankyArchWisp, IBody>();
			return master;
		}
	}
	#endregion
	
	public class LargeArchWispPrimaryFamily : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<LargeArchWispPrimary>() };
	}

	internal class LargeArchWispPrimary : Asset, ISkill
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 7f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.icon = LoadAsset<Sprite>("n");
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(EntityStates.Idle /*GravekeeperMonster.Weapon.GravekeeperBarrage*/) }; //todo Gravekeeper state is broken, fix later
	}
}