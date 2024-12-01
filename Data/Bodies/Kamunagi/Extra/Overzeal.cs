using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	public class OverzealState : BaseTwinState
	{
		public override int meterGain => 85;
		private float duration = 0.2f;

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= duration)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public class Overzeal : Concentric, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 0";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA8_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA8_DESCRIPTION";
			skill.icon = await LoadAsset<Sprite>("kamunagiassets2:Overzeal");
			skill.activationStateMachineName = "Weapon";
			skill.baseMaxStock = 1;
			skill.baseRechargeInterval = 1f;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			skill.rechargeStock = 1;
			skill.mustKeyPress = true;
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(OverzealState) };
	}
}