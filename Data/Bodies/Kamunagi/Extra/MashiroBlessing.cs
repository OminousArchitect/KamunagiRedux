using EntityStates;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class MashiroBlessingState : BaseTwinState
	{
		public static GameObject muzzleEffect;
		private EffectManagerHelper muzzleInstanceLeft;
		private EffectManagerHelper muzzleInstanceRight;
		private float stopwatch;
		public float duration = 2f;

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
			var amountToDecrease = Mathf.Max(1f,healthComponent.fullHealth * chargeFraction * 0.25f); // by mathf max you ensure people die when at 1hp ie trancendence

			if (!NetworkServer.active || !healthComponent) return;
			healthComponent.Networkhealth -= amountToDecrease;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (isAuthority && !characterBody.outOfDanger || fixedAge > duration)
			{
				if (fixedAge > duration)
				{
					characterBody.AddTimedBuffAuthority(Asset.GetBuffIndex<MashiroBlessing>().WaitForCompletion(), 10f);
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

	public class MashiroBarSegment : BarData
	{
		public override HealthBarStyle.BarStyle GetStyle()
		{
			if (Bar == null) return default;
			var style = Bar.style.curseBarStyle;
			style.baseColor = Colors.twinsLightColor;
			return style;
		}

		public override void UpdateInfo(ref HealthBar.BarInfo inf, HealthComponent.HealthBarValues healthBarValues, ExtraHealthBarSegments.ExtraHealthBarInfoTracker extraHealthBarInfoTracker)
		{
			base.UpdateInfo(ref inf, healthBarValues, extraHealthBarInfoTracker);
			inf.enabled = false;
			if (!spellStateMachine || spellStateMachine!.state is not MashiroBlessingState blessingState) return;
			inf.enabled = true;
			inf.normalizedXMin = Mathf.Max(0f,healthBarValues.healthFraction - blessingState.fixedAge / blessingState.duration * 0.25f);
			inf.normalizedXMax = healthBarValues.healthFraction;
		}
		
		private EntityStateMachine? _spellStateMachine;
		public EntityStateMachine? spellStateMachine
		{
			get
			{
				if (_spellStateMachine == null || !_spellStateMachine)
				{
					_spellStateMachine = Bar!.source.body.skillLocator
						.FindSkillByDef(Asset.GetSkillDef<MashiroBlessing>().WaitForCompletion())?.stateMachine;
				}

				return _spellStateMachine;
			}
		}
	}
	
	public class MashiroBlessing : Asset, IEffect, ISkill, IBuff, IOverlay
	{
		public static DamageColorIndex damageColorIndex;

		public MashiroBlessing()
		{
			damageColorIndex = ColorsAPI.RegisterDamageColor(new Color(0.98f, 1, 0.58f));
		}

		public override async Task Initialize()
		{
			await base.Initialize();
			MashiroBlessingState.muzzleEffect = await this.GetEffect();
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = await LoadAsset<GameObject>("bundle:ShadowFlame.prefab")!;
			var vfx = effect.AddComponent<VFXAttributes>();
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;
			vfx.DoNotPool = false;
			effect.transform.localPosition = Vector3.zero;
			effect.transform.localScale = Vector3.one * 0.6f;
			return effect;
		}

		public IEnumerable<Type> GetEntityStates() => new []{typeof(MashiroBlessingState)};

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<BlessingDef>();
			skill.skillName = "Extra Skill 6";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA6_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix +  "EXTRA6_DESCRIPTION";
			skill.icon = await LoadAsset<Sprite>("bundle2:Mashiro");
			skill.activationStateMachineName = "Spell";
			skill.baseRechargeInterval = 3f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.cancelSprintingOnActivation = false;
			skill.mustKeyPress = true;
			return skill;
		}

		async Task<BuffDef> IBuff.BuildObject()
		{
			var buffDef = ScriptableObject.CreateInstance<BuffDef>();
			buffDef.name = "LordMashiroBlessing";
			buffDef.buffColor = Color.yellow;
			buffDef.canStack = false;
			buffDef.isDebuff = false;
			buffDef.iconSprite = await LoadAsset<Sprite>("RoR2/Base/LunarGolem/texBuffLunarShellIcon.tif");
			buffDef.isHidden = false;
			return buffDef;
		}

		Task<Material> IOverlay.BuildObject()
		{
			return LoadAsset<Material>("RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat")!;
		}

		bool IOverlay.CheckEnabled(CharacterModel model)
		{
			return model.body && model.body.HasBuff(this.GetBuffIndex().WaitForCompletion());
		}
	}

	public class BlessingDef : SkillDef
	{
		public override bool IsReady(GenericSkill skillSlot) => base.IsReady(skillSlot) && skillSlot.characterBody.outOfDanger;
	}

	public class MashiroBlessingRespawn : Asset, IEffect
	{
		public async Task<GameObject> BuildObject()
		{
			var effect = (await LoadAsset<GameObject>("legacy:Prefabs/Effects/HippoRezEffect"))!
				.InstantiateClone("MashiroBlessingRespawnEffect", false);
			return effect;
		}
	}
}