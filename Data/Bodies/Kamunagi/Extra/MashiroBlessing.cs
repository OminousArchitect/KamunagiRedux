using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class MashiroBlessingState : BaseTwinState
	{
		public override int meterGain => 0;

		public static GameObject muzzleEffect = Asset.GetGameObject<MashiroBlessing, IEffect>();
		private EffectManagerHelper muzzleInstanceLeft;
		private EffectManagerHelper muzzleInstanceRight;
		private float stopwatch;
		private float duration = 2f;

		public override void OnEnter()
		{
			base.OnEnter();
			muzzleInstanceLeft =
				EffectManagerKamunagi.GetAndActivatePooledEffect(muzzleEffect,
					new EffectData() { rootObject = FindModelChild("MuzzleLeft").gameObject });
			muzzleInstanceRight =
				EffectManagerKamunagi.GetAndActivatePooledEffect(muzzleEffect,
					new EffectData() { rootObject = FindModelChild("MuzzleRight").gameObject });
		}

		public override void OnExit()
		{
			base.OnExit();
			if (muzzleInstanceLeft != null) muzzleInstanceLeft.ReturnToPool();
			if (muzzleInstanceRight != null) muzzleInstanceRight.ReturnToPool();
			var chargeFraction = fixedAge / duration;
			var amountToDecrease = healthComponent.fullHealth * chargeFraction;
			
			if (!NetworkServer.active || !healthComponent) return;
			/*var damageInfo = new DamageInfo
			{
				damage = (healthComponent.combinedHealth * 0.25f) * chargeFraction,
				position = characterBody.corePosition,
				damageColorIndex = MashiroBlessing.damageColorIndex,
				damageType = DamageType.BypassArmor,
				procCoefficient = 0f
			};*/
			//healthComponent.TakeDamage(damageInfo);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (isAuthority && !characterBody.outOfDanger || fixedAge > duration)
			{
				if (fixedAge > duration)
				{
					characterBody.AddTimedBuffAuthority(Asset.GetAsset<MashiroBlessing>(), 10f);
				}
				outer.SetNextStateToMain();
				return;
			}

			
			if (!IsKeyDownAuthority())
			{
				outer.SetNextStateToMain();
				return;
			}

			stopwatch += Time.deltaTime;
			if (stopwatch < 0.075f) return;
			//0.2 frequency is equal to 5 times per second
			//0.1 would be 10 times per second
			//0.075 is 24 times in 2 seconds
			stopwatch = 0;
		}
	}
	
	public class MashiroBlessing : Asset, IEffect, ISkill, IBuff, IOverlay
	{
		public static DamageColorIndex damageColorIndex;

		public MashiroBlessing()
		{
			damageColorIndex = ColorsAPI.RegisterDamageColor(new Color(0.98f, 1, 0.58f));
		}

		GameObject IEffect.BuildObject()
		{
			var effect = LoadAsset<GameObject>("bundle:ShadowFlame.prefab")!;
			var vfx = effect.AddComponent<VFXAttributes>();
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;
			vfx.DoNotPool = false;
			effect.transform.localPosition = Vector3.zero;
			effect.transform.localScale = Vector3.one * 0.6f;
			return effect;
		}

		public IEnumerable<Type> GetEntityStates() => new []{typeof(MashiroBlessingState)};
		
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Extra Skill 6";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA6_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix +  "EXTRA6_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle2:Mashiro");
			skill.activationStateMachineName = "Spell";
			skill.baseRechargeInterval = 3f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			skill.mustKeyPress = true;
			return skill;
		}

		BuffDef IBuff.BuildObject()
		{
			var buffDef = ScriptableObject.CreateInstance<BuffDef>();
			buffDef.name = "LordMashiroBlessing";
			buffDef.buffColor = Color.yellow;
			buffDef.canStack = false;
			buffDef.isDebuff = false;
			buffDef.iconSprite = LoadAsset<Sprite>("RoR2/Base/LunarGolem/texBuffLunarShellIcon.tif");
			buffDef.isHidden = false;
			return buffDef;
		}

		Material IOverlay.BuildObject()
		{
			return LoadAsset<Material>("RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat")!;
		}

		bool IOverlay.CheckEnabled(CharacterModel model)
		{
			return model.body && model.body.HasBuff(this);
		}
	}

	public class MashiroBlessingRespawn : Asset, IEffect
	{
		public GameObject BuildObject()
		{
			var effect = LoadAsset<GameObject>("legacy:Prefabs/Effects/HippoRezEffect")!
				.InstantiateClone("MashiroBlessingRespawnEffect", false);
			return effect;
		}
	}
}