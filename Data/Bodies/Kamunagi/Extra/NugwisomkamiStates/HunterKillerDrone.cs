using EntityStates.GolemMonster;
using EntityStates.LunarWisp;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class FireWispGuns : ChargeLunarGuns
	{
		public override void OnEnter()
		{
			this.duration = 0.8f;
			this.muzzleTransformRoot = base.FindModelChild(ChargeLunarGuns.muzzleNameRoot);
			this.muzzleTransformOne = base.FindModelChild(ChargeLunarGuns.muzzleNameOne);
			this.muzzleTransformTwo = base.FindModelChild(ChargeLunarGuns.muzzleNameTwo);
			this.loopedSoundID = Util.PlaySound(ChargeLunarGuns.windUpSound, base.gameObject);
			base.PlayCrossfade("Gesture", "MinigunSpinUp", 0.2f);
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(this.duration);
			}
		}
	}
	
	internal class ReplaceHKPrimary : Asset, ISkill
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 8f;
			skill.icon = LoadAsset<Sprite>("n");
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(FireWispGuns) };
	}
	
	public class HKPrimaryFamily : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<ReplaceHKPrimary>() };
	}
}