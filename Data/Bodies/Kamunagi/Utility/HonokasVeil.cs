using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class HonokasVeilState : BaseTwinState
	{
		public CharacterModel? charModel;
		public HurtBoxGroup? hurtBoxGroup;
		public override int meterGain => 0;

		public override void OnEnter()
		{
			base.OnEnter();
			var mdl = GetModelTransform();
			if (mdl)
			{
				charModel = mdl.GetComponent<CharacterModel>();
				hurtBoxGroup = mdl.GetComponent<HurtBoxGroup>();
				if (charModel && hurtBoxGroup)
				{
					charModel.invisibilityCount++;
					hurtBoxGroup.hurtBoxesDeactivatorCounter++;
				}
			}

			Util.PlaySound("Play_imp_attack_blink", gameObject);
			if (NetworkServer.active) characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
			//TODO effects and chains bullshit
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;
			if (!IsKeyDownAuthority()) outer.SetNextStateToMain();
		}

		public override void OnExit()
		{
			base.OnExit();
			if (NetworkServer.active) characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
			Util.PlaySound("Play_imp_attack_blink", gameObject);
			if (charModel != null && charModel && hurtBoxGroup != null && hurtBoxGroup)
			{
				charModel.invisibilityCount--;
				hurtBoxGroup.hurtBoxesDeactivatorCounter--;
			}
		}
	}

	public class HonokasVeil : Asset, ISkill
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Utility 9";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA1_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA1_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle:HonokasVeil");
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 0f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.canceledFromSprinting = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.keywordTokens = new[] { KamunagiAsset.tokenPrefix + "TWINSBLESSING_KEYWORD" };
			return skill;
		}

		Type[] ISkill.GetEntityStates() => new[] { typeof(HonokasVeilState) };
	}
}