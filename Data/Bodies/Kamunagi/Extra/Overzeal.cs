using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	public class OverzealState : BaseTwinState
	{
		public override int meterGain => 85;
		private float duration = 0.2f;
		public static GameObject effect;

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= duration)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			
		}
	}

	public class Overzeal : Concentric, ISkill, IEffect
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			OverzealState.effect = await this.GetEffect();
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 0";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA8_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA8_DESCRIPTION";
			skill.icon = await LoadAsset<Sprite>("kamunagiassets2:Overzeal");
			skill.activationStateMachineName = "Weapon";
			skill.baseMaxStock = 1;
			skill.baseRechargeInterval = 10f;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			skill.rechargeStock = 1;
			skill.mustKeyPress = true;
			return skill;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			Material something = await LoadAsset<Material>("RoR2/Base/Common/VFX/matHealingCross.mat");
			something.SetTexture("_RemapTex", await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
			Material edited = new Material(await LoadAsset<Material>("RoR2/Base/Common/VFX/matHealTrail.mat"));
			edited.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/DLC1/VoidSurvivor/texRampVoidSurvivorBase2.png"));
			edited.SetFloat("_AlphaBoost", 3.1f);
			edited.SetFloat("_Boost", 20f);
			
			var effect = (await LoadAsset<GameObject>("RoR2/Base/Fruit/FruitHealEffect.prefab"))!.InstantiateClone("OverzealEffect", false);
			var first =effect.transform.GetChild(0).gameObject;
			first.GetComponent<ParticleSystemRenderer>().sharedMaterials = new[] { something, edited };
			UnityEngine.Object.Destroy(effect.transform.GetChild(2).gameObject);
			UnityEngine.Object.Destroy(effect.transform.GetChild(3).gameObject);
			effect.EffectWithSound("");
			effect.GetComponent<EffectComponent>().positionAtReferencedTransform = false;
			return effect;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(OverzealState) };
	}
}