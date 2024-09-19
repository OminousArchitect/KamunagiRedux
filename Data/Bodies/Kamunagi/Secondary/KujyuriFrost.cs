using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class KujyuriFrostState : RaycastedSpellState
	{
		public EffectManagerHelper muzzleEffectInstance;

		public EffectManagerHelper iceMagicInstance;
		public override float failedCastCooldown => 2f;
		public override float duration => 0.8f;
		public override bool requireFullCharge => true;

		public override void OnEnter() {
			base.OnEnter();
			muzzleEffectInstance = EffectManager.GetAndActivatePooledEffect(Asset.GetGameObject<KujyuriFrost, IEffect>(), GetModelChildLocator().FindChild(twinMuzzle), true);
			var toggling = twinMuzzle;
			iceMagicInstance = EffectManager.GetAndActivatePooledEffect(Asset.GetGameObject<IceMagicEffect, IEffect>(), GetModelChildLocator().FindChild(twinMuzzle), true); 
		}

		public override void OnExit()
		{
			base.OnExit();
			if (muzzleEffectInstance != null) muzzleEffectInstance.ReturnToPool();
			if (iceMagicInstance != null) iceMagicInstance.ReturnToPool();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;
			if (fixedAge >= duration || IsKeyDownAuthority()) return;
			var aimRay = GetAimRay();
			
			new BulletAttack
			{
				maxDistance = 1000,
				owner = gameObject,
				weapon = gameObject,
				origin = aimRay.origin,
				aimVector = aimRay.direction,
				maxSpread = 0.4f,
				damage = damageStat * 2.5f,
				force = 155,
				muzzleName = twinMuzzle,
				isCrit = RollCrit(),
				radius = 0.8f,
				procCoefficient = 1f,
				smartCollision = true,
				falloffModel = BulletAttack.FalloffModel.None,
				damageType = DamageType.Freeze2s
			}.Fire();
		}

		public override void Fire(Vector3 targetPosition) {
			base.Fire(targetPosition);
			new BlastAttack
			{
				attacker = gameObject,
				baseDamage = damageStat * 1.75f,
				baseForce = 200f,
				crit = RollCrit(),
				damageType = DamageType.SlowOnHit | DamageType.Stun1s | DamageType.Freeze2s,
				falloffModel = BlastAttack.FalloffModel.None,
				procCoefficient = 1f,
				radius = 10f,
				position = targetPosition,
				attackerFiltering = AttackerFiltering.NeverHitSelf,
				teamIndex = teamComponent.teamIndex
			}.Fire();
			EffectManager.SpawnEffect(Asset.GetGameObject<KujyuriFrostBlast, IEffect>(), new EffectData
			{
				origin = targetPosition,
				rotation = Util.QuaternionSafeLookRotation(GetAimRay().direction),
				scale = 10f
			}, true);
		}
	}
	public class KujyuriFrost : Asset, ISkill, IEffect
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Secondary 3";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SECONDARY3_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SECONDARY3_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle:waterpng");
			skill.activationStateMachineName = "Weapon";
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = false;
			return skill;
		}

		GameObject IEffect.BuildObject()
		{
			var effect = LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab")!.InstantiateClone("TwinsFrostMuzzleFlash", false);
			var transform = effect.transform;
			transform.localPosition = Vector3.zero;
			transform.rotation = Quaternion.identity;
			transform.localScale = Vector3.one * 12.5f;
			var vfx = effect.GetOrAddComponent<VFXAttributes>();
			vfx.DoNotPool = false;
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;
			var curve = effect.AddComponent<ObjectScaleCurve>();
			curve.useOverallCurveOnly = true;
			curve.timeMax = 0.8f;
			curve.overallCurve = AnimationCurve.Linear(0.03f, 0.15f, 0.8f, 0.08f);
			var remapTex = LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png");
			foreach (var r in effect.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				var name = r.name;

				switch (name)
				{
					case "Ring":
					case "OmniSparks":
						r.material.SetTexture("_RemapTex", remapTex);
						break;
				}
			}
			effect.GetComponentInChildren<Light>().color = Color.cyan;
			effect.EffectWithSound("Play_mage_m2_iceSpear_charge");
			return effect;
		}

		public IEnumerable<Type> GetEntityStates() => new []{typeof(KujyuriFrostState)};
	}

	public class KujyuriFrostBlast : Asset, IEffect
	{
		GameObject IEffect.BuildObject()
		{
			var effect = LoadAsset<GameObject>("RoR2/Base/EliteIce/AffixWhiteExplosion.prefab")!.InstantiateClone("TwinsFrostNovaEffect", false);
			effect.transform.localScale = Vector3.one * 10f;
			effect.EffectWithSound("Play_item_proc_iceRingSpear");
			return effect;
		}
	}

	public class IceMagicEffect : Asset, IEffect
	{
		GameObject IEffect.BuildObject()
		{
			var effect = LoadAsset<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbGhost.prefab")!.InstantiateClone("TwinsIceHandEnergy", false);
			UnityEngine.Object.Destroy(effect.GetComponent<ProjectileGhostController>());
			var iceChild = effect.transform.GetChild(0);
			iceChild.transform.localScale = Vector3.one * 0.1f;
			var iceTransform = iceChild.transform.GetChild(0);
			iceTransform.transform.localScale = Vector3.one * 0.25f;
			var iceAdditive = new Color(0.298039216f, 0.443137255f, 0.767741935f);
			var spitCore = effect.GetComponentInChildren<ParticleSystemRenderer>();
			spitCore.material.SetColor("_TintColor", iceAdditive);
			var iceTrails = effect.GetComponentsInChildren<TrailRenderer>();
			iceTrails[0].enabled = false;
			iceTrails[1].enabled = false;
			return effect;
		}
	}
}