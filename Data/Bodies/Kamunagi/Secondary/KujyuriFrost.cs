using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class ExperimentalWallState : BaseTwinState
	{
		public float maxDistance = 600f;
        public float maxSlopeAngle = 100f;
        public float baseDuration = 0.5f;
        public float duration;
        public string prepWallSoundString = "Play_mage_shift_start";
        public string fireSoundString = "Play_mage_shift_stop";
        public bool goodPlacement;
        public RoR2.UI.CrosshairUtils.OverrideRequest crosshairOverrideRequest;
        public static GameObject indicatorPrefab;
        public GameObject goodCrosshairPrefab = EntityStates.Mage.Weapon.PrepWall.goodCrosshairPrefab;
        public GameObject badCrosshairPrefab =  EntityStates.Mage.Weapon.PrepWall.badCrosshairPrefab;
        public GameObject indicatorPrefabInstance; //declared in the entitystate, intialized in the Concentric class
        public static GameObject muzzleFlash;
        private float damageCoefficient = 1f;
        private bool shouldInvert;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(duration + 2f);
            Util.PlaySound(prepWallSoundString, gameObject);
            indicatorPrefabInstance = UnityEngine.Object.Instantiate(indicatorPrefab);
            UpdateAreaIndicator();
        }

        private void UpdateAreaIndicator()
        {
            var wasGoodPlacement = goodPlacement;
            goodPlacement = false;
			if (isAuthority)
            {
	            indicatorPrefabInstance.SetActive(true);
            }
            if (indicatorPrefabInstance)
            {
                var aimRay = GetAimRay();
                if (Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(aimRay, gameObject, out var extraRayDistance), out var raycastHit, maxDistance + extraRayDistance, LayerIndex.world.mask))
                {
	                shouldInvert = Vector3.Angle(Vector3.up, raycastHit.normal) > maxSlopeAngle;
                    indicatorPrefabInstance.transform.position = raycastHit.point;
                    indicatorPrefabInstance.transform.forward = -aimRay.direction;
                    
                    if (shouldInvert)
                    {
	                    indicatorPrefabInstance.transform.up = -raycastHit.normal;
                    }
                    else
                    {
	                    indicatorPrefabInstance.transform.up = raycastHit.normal;
                    }
                    //goodPlacement = Vector3.Angle(Vector3.up, raycastHit.normal) " maxSlopeAngle;
                    goodPlacement = true;
                }
                if (wasGoodPlacement != goodPlacement || crosshairOverrideRequest == null)
                {
                    if (crosshairOverrideRequest != null) crosshairOverrideRequest.Dispose();
                    var newCrosshairPrefab = goodPlacement ? goodCrosshairPrefab : badCrosshairPrefab;
                    crosshairOverrideRequest = RoR2.UI.CrosshairUtils.RequestOverrideForBody(characterBody, newCrosshairPrefab, RoR2.UI.CrosshairUtils.OverridePriority.Skill);
                }
            }
            indicatorPrefabInstance.SetActive(goodPlacement);
        }

        public override void Update()
        {
            base.Update();
            UpdateAreaIndicator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration && !inputBank.skill2.down)
            {
                outer.SetNextStateToMain();
            }
        }
        
        public override void OnExit()
        {
            if (!outer.destroying)
            {
                if (goodPlacement)
                {
                    Util.PlaySound(fireSoundString, gameObject);
                    if (indicatorPrefabInstance && base.isAuthority)
                    {
	                    var forward = indicatorPrefabInstance.transform.forward;
                        forward.y = 0f;
                        forward.Normalize();
                        var crossproduct = Vector3.Cross(Vector3.up, forward);
                        var position = indicatorPrefabInstance.transform.position + Vector3.up;
                        
                        ProjectileManager.instance.FireProjectile
                        (
	                        Concentric.GetProjectile<KujyuriFrost>().WaitForCompletion(),
                            position, // position:
                            Util.QuaternionSafeLookRotation(crossproduct), // rotation:
                            base.gameObject, 
                            damageStat * damageCoefficient,
                            0f,
                            false,
                            DamageColorIndex.Default
                        );
                        var blastAttack = new BlastAttack //need this as a disguise for the first loop of the particle system
                        {
	                        attacker = gameObject,
	                        baseDamage = characterBody.damage * 1f,
	                        baseForce = 0,
	                        crit = false,
	                        damageType = DamageTypeCombo.GenericSecondary | DamageType.Freeze2s,
	                        falloffModel = BlastAttack.FalloffModel.None,
	                        procCoefficient = 1,
	                        radius = 13f,
	                        position = position,
	                        attackerFiltering = AttackerFiltering.NeverHitSelf,
	                        teamIndex = teamComponent.teamIndex
                        };
                        blastAttack.Fire();
                    }
                }
                else
                {
                    skillLocator.utility.AddOneStock();
                }
            }
            EntityState.Destroy(indicatorPrefabInstance.gameObject);
            if (crosshairOverrideRequest != null)
            {
                crosshairOverrideRequest.Dispose();
            }
            EffectManager.SimpleMuzzleFlash(muzzleFlash, base.gameObject, twinMuzzle, false);
            base.OnExit();
        }
	}

	public class PrepIceWallIndicator : Concentric, IGenericObject
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			ExperimentalWallState.indicatorPrefab = await this.GetGenericObject();
		}
		
		async Task<GameObject> IGenericObject.BuildObject()
		{
			var indicator = (await LoadAsset<GameObject>("RoR2/Base/Mage/FirewallAreaIndicator.prefab"))!.InstantiateClone("TwinsIceWallIndicator", false);
			indicator.transform.localScale = new Vector3(3f, 3f, 3f);
			return indicator;
		}
	}
	
	public class KujyuriFrost : Concentric, ISkill, IEffect, IProjectile
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			ExperimentalWallState.muzzleFlash = await this.GetEffect();
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Secondary 3";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SECONDARY3_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SECONDARY3_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("kamunagiassets:waterpng"));
			skill.activationStateMachineName = "Weapon";
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = false;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.baseRechargeInterval = 10f;
			skill.keywordTokens = new[] { "KEYWORD_FREEZING" };
			return skill;
		}

		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("RoR2/Base/LunarExploder/LunarExploderProjectileDotZone.prefab"))!.InstantiateClone("TwinsIceDotZone", true);
			proj.transform.localScale = Vector3.one * 0.75f;
			var parent = proj.transform.GetChild(0).gameObject.transform;
			parent.transform.localScale = new Vector3(10f, 4f, 10f);
			var dotZone = proj.GetComponent<ProjectileDotZone>();
			dotZone.fireFrequency = 0.7f;
			dotZone.lifetime = 2.9f;
			dotZone.overlapProcCoefficient = 1f;
			dotZone.damageCoefficient = 1f;
			var pdzef = proj.AddComponent<ProjectileDotZoneEndEffect>();
			pdzef.effect = await LoadAsset<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXFrozen.prefab");
			dotZone.onEnd.AddListener(pdzef.OnDestroy);
			proj.GetComponent<ProjectileDamage>().damageType = DamageTypeCombo.GenericSecondary | DamageType.Freeze2s;
			UnityEngine.Object.Destroy(proj.GetComponent<AkGameObj>());

			var hitboxResize = Vector3.one * 2.9f;
			proj.GetComponent<HitBoxGroup>().hitBoxes[0].gameObject.transform.localScale = hitboxResize;
			proj.GetComponent<HitBoxGroup>().hitBoxes[0].gameObject.transform.localPosition = new Vector3(0f, 2f, 0f);
			proj.GetComponent<HitBoxGroup>().hitBoxes[1].gameObject.transform.localScale = hitboxResize;

			var stockParticles = proj.transform.Find("FX/ScaledOnImpact").gameObject;
			stockParticles.transform.GetChild(0).gameObject.SetActive(false);
			stockParticles.transform.GetChild(2).gameObject.SetActive(false);
			stockParticles.transform.GetChild(3).gameObject.SetActive(false);
			stockParticles.transform.GetChild(4).gameObject.SetActive(false);
			var icePillar = (await LoadAsset<GameObject>("RoR2/Base/bazaar/Bazaar_GenericIce.prefab"))!.InstantiateClone("IcePillar", false);
			icePillar.transform.localScale = Vector3.one; //touching this appears to break normalizing to floor, so I'll leave it alone
			icePillar.transform.localPosition = Vector3.zero;
			icePillar.transform.SetParent(parent);
			var icyFx = (await LoadAsset<GameObject>("RoR2/Base/Icicle/IcicleAura.prefab")).transform.GetChild(0).gameObject!.InstantiateClone("IceFX", false);
			icyFx.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
			UnityEngine.Object.Destroy(icyFx.transform.Find("Area").gameObject);
			UnityEngine.Object.Destroy(icyFx.transform.Find("Ring, Outer").gameObject);
			UnityEngine.Object.Destroy(icyFx.transform.GetChild(0).gameObject);
			UnityEngine.Object.Destroy(icyFx.transform.GetChild(1).gameObject); 
			foreach (ParticleSystem p in icyFx.GetComponentsInChildren<ParticleSystem>())
			{
				p.transform.localScale = Vector3.one * 18f;
				var main = p.main;
				var name = p.name;
				if (name == "Ring, Procced")
				{
					main.simulationSpeed = 0.7f;
				}
				
				main.loop = true;
				main.playOnAwake = true;
			}
			
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

		public IEnumerable<Type> GetEntityStates() => new []{typeof(ExperimentalWallState)};
	}

	public class ProjectileDotZoneEndEffect : MonoBehaviour
	{
		public GameObject effect;

		public void OnDestroy()
		{
			var effectData = new EffectData { rotation = Quaternion.identity, origin = transform.position };
			EffectManager.SpawnEffect(effect, effectData, false);
		}
	}

	public class KujyuriFrostBlast : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect= (await LoadAsset<GameObject>("RoR2/Base/EliteIce/AffixWhiteExplosion.prefab"))!.InstantiateClone("TwinsFrostNovaEffect", false);
			effect.transform.localScale = Vector3.one * 10f;
			effect.EffectWithSound("Play_item_proc_iceRingSpear");
			return effect;
		}
	}
	
	public class PillarSpawn : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect= (await LoadAsset<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXFrozen.prefab"))!.InstantiateClone("TwinsIceHitspark", false);
			return effect;
		}
	}

	public class IceMagicEffect : Concentric, IEffect
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