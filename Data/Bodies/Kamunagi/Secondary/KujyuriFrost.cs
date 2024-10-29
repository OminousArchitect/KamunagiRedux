using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.Events;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class KujyuriFrostState : IndicatorSpellState
	{
		public EffectManagerHelper muzzleEffectInstance;

		public EffectManagerHelper iceMagicInstance;
		public override float duration => 2f;
		public override bool requireFullCharge => false;

		public override void OnEnter() {
			base.OnEnter();
			muzzleEffectInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(Asset.GetEffect<KujyuriFrost>().WaitForCompletion(), GetModelChildLocator().FindChild(twinMuzzle), true);
			var toggling = twinMuzzle;
			iceMagicInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(Asset.GetEffect<IceMagicEffect>().WaitForCompletion(), GetModelChildLocator().FindChild(twinMuzzle), true); 
		}

		public override void OnExit()
		{
			base.OnExit();
			if (muzzleEffectInstance != null) muzzleEffectInstance.ReturnToPool();
			if (iceMagicInstance != null) iceMagicInstance.ReturnToPool();
		}

		public override void Fire(Vector3 targetPosition) { 
			base.Fire(targetPosition);
			ProjectileManager.instance.FireProjectile(
				Asset.GetProjectile<KujyuriFrost>().WaitForCompletion(),
				indicator.transform.position,
				indicator.transform.rotation,
				gameObject,
				damageStat,
				10f,
				false
			);
		}
	}

	public class IceHitEffect : Asset, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			Material iceImpact = new Material(await LoadAsset<Material>("RoR2/Base/Common/VFX/matGenericFlash.mat"));
			
			var effect= (await LoadAsset<GameObject>("RoR2/Base/Huntress/HuntressFireArrowRain.prefab"))!.InstantiateClone("TwinsIceHitspark", false);
			var sparksLarge = effect.transform.GetChild(0).gameObject;
			Material sparksOne = sparksLarge.GetComponent<ParticleSystemRenderer>().material;
			return effect;
		}
	}
	public class KujyuriFrost : Asset, ISkill, IEffect, IProjectile
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Secondary 3";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SECONDARY3_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SECONDARY3_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("bundle:waterpng"));
			skill.activationStateMachineName = "Weapon";
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = false;
			skill.beginSkillCooldownOnSkillEnd = false;
			return skill;
		}

		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/Base/LunarExploder/LunarExploderProjectileDotZone.prefab"))!.InstantiateClone("TwinsIceDotZone", true);
			proj.transform.localScale = Vector3.one * 0.75f;
			var parent = proj.transform.GetChild(0).gameObject.transform;
			parent.transform.localScale = new Vector3(13f, 6f, 13f);
			//parent.transform.localScale = Vector3.one * 10f;
			var dotZone = proj.GetComponent<ProjectileDotZone>();
			dotZone.onEnd.m_PersistentCalls = new UnityEngine.Events.PersistentCallGroup();
			dotZone.fireFrequency = 0.7f;
			dotZone.lifetime = 5.7f;
			var stockParticles = proj.transform.Find("FX/ScaledOnImpact").gameObject;
			stockParticles.transform.GetChild(0).gameObject.SetActive(false);
			stockParticles.transform.GetChild(2).gameObject.SetActive(false);
			stockParticles.transform.GetChild(3).gameObject.SetActive(false);
			stockParticles.transform.GetChild(4).gameObject.SetActive(false);
			var icePillar = (await LoadAsset<GameObject>("RoR2/Base/bazaar/Bazaar_GenericIce.prefab"))!.InstantiateClone("IcePillar", false);
			icePillar.transform.localScale = Vector3.one; //new Vector3(0.2f, 0.1f, 0.2f);
			icePillar.transform.localPosition = Vector3.zero;
			icePillar.transform.SetParent(parent);
			
			var icyFx = (await LoadAsset<GameObject>("RoR2/Base/Icicle/IcicleAura.prefab")).transform.GetChild(0).gameObject!.InstantiateClone("IceFX", false);
			icyFx.transform.localScale = Vector3.one;
			UnityEngine.Object.Destroy(icyFx.transform.Find("Area").gameObject);
			UnityEngine.Object.Destroy(icyFx.transform.GetChild(0).gameObject);
			UnityEngine.Object.Destroy(icyFx.transform.GetChild(1).gameObject);
			foreach (ParticleSystem p in icyFx.GetComponentsInChildren<ParticleSystem>())
			{
				p.transform.localScale = Vector3.one * 10;
				var main = p.main;
				main.loop = true;
				main.playOnAwake = true;
				var r = p.GetComponent<ParticleSystemRenderer>();
				r.materials = new Material[] { r.material, r.material, r.material, r.material, r.material, r.material, r.material, r.material }; //whatisthis
			}
			var changeRate = icyFx.transform.Find("Ring, Procced").gameObject;
			icyFx.transform.SetParent(parent);
			return proj;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var effect= (await LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab"))!.InstantiateClone("TwinsFrostMuzzleFlash", false);
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
			var remapTex= (await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
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
			effect.SetActive(false);
			return effect;
		}

		public IEnumerable<Type> GetEntityStates() => new []{typeof(KujyuriFrostState)};
	}

	public class KujyuriFrostBlast : Asset, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect= (await LoadAsset<GameObject>("RoR2/Base/EliteIce/AffixWhiteExplosion.prefab"))!.InstantiateClone("TwinsFrostNovaEffect", false);
			effect.transform.localScale = Vector3.one * 10f;
			effect.EffectWithSound("Play_item_proc_iceRingSpear");
			return effect;
		}
	}

	public class IceMagicEffect : Asset, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect= (await LoadAsset<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbGhost.prefab"))!.InstantiateClone("TwinsIceHandEnergy", false);
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