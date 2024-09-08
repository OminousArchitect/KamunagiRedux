using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Passive
{
	public class KamunagiDashState : KamunagiHoverState
	{
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			log.LogInfo("oh god we're dashing");
		}
	}

	public class KamunagiDash : Asset, ISkill
	{
		public SkillDef BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Hover";
			skill.baseRechargeInterval = 5f;
			skill.cancelSprintingOnActivation = false;
			return skill;
		}

		public Type[] GetEntityStates() => new[] { typeof(KamunagiDashState) };
	}
}