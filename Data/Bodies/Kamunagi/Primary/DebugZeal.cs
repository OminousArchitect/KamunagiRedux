using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	public class DebugZealState : BaseTwinState
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

	public class DebugZeal : Concentric, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 0";
			skill.skillNameToken = "? ? ?";
			skill.skillDescriptionToken = "? ? ?";
			skill.icon = await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png");
			skill.activationStateMachineName = "Weapon";
			skill.baseMaxStock = 1;
			skill.baseRechargeInterval = 1f;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			skill.rechargeStock = 1;
			skill.mustKeyPress = true;
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(DebugZealState) };
	}
}