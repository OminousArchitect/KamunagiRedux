using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
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
		public EffectManagerHelper? veilEffect;
		public static GameObject muzzleEffect;
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
			var effect = Concentric.GetEffect<HonokasVeil>().WaitForCompletion();
			if (NetworkServer.active) characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
			EffectManager.SpawnEffect(muzzleEffect, new EffectData
			{
				origin = Util.GetCorePosition(base.gameObject),
				rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward)
			}, false);
			veilEffect = EffectManagerKamunagi.GetAndActivatePooledEffect(effect, characterBody.coreTransform, true);
			Util.PlaySound("Play_imp_attack_blink", gameObject);
			characterMotor.useGravity = false;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;
			if (!IsKeyDownAuthority() || fixedAge > 1f)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			if (NetworkServer.active) characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
			if (veilEffect != null) veilEffect.ReturnToPool();
			Util.PlaySound("Play_imp_attack_blink", gameObject);
			characterMotor.useGravity = true;
			EffectManager.SpawnEffect(muzzleEffect, new EffectData
			{
				origin = Util.GetCorePosition(base.gameObject),
				rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward)
			}, false);
			if (charModel != null && charModel && hurtBoxGroup != null && hurtBoxGroup)
			{
				charModel.invisibilityCount--;
				hurtBoxGroup.hurtBoxesDeactivatorCounter--;
			}
		}
	}

	public class HonokasVeil : Concentric, ISkill, IEffect
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			HonokasVeilState.muzzleEffect = await LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflash.prefab");
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Utility 9";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA1_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA1_DESCRIPTION";
			skill.icon = await LoadAsset<Sprite>("bundle:HonokasVeil");
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 1.5f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.canceledFromSprinting = true;
			skill.mustKeyPress = true;
			skill.interruptPriority = InterruptPriority.Any;
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(HonokasVeilState) };

		async Task<GameObject> IEffect.BuildObject()
		{
			var impBoss = await LoadAsset<GameObject>("RoR2/Base/ImpBoss/ImpBossBody.prefab")!;
			var dustCenter = impBoss.transform.Find("ModelBase/mdlImpBoss/DustCenter");

			var effect = dustCenter.gameObject.InstantiateClone("VeilParticles", false);
			UnityEngine.Object.Destroy(effect.transform.GetChild(0).gameObject);
			var distortion = effect.AddComponent<ParticleSystem>();
			var coreR = effect.GetComponent<ParticleSystemRenderer>();
			Material decalMaterial = new Material(await LoadAsset<Material>("RoR2/Base/Common/VFX/matInverseDistortion.mat"));
			decalMaterial.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
			coreR.material = decalMaterial;
			coreR.renderMode = ParticleSystemRenderMode.Billboard;
			var coreM = distortion.main;
			coreM.duration = 1f;
			coreM.simulationSpeed = 1.1f;
			coreM.loop = true;
			coreM.startLifetime = 0.13f;
			coreM.startSpeed = 5f;
			coreM.startSize3D = false;
			coreM.startSizeY = 0.6f;
			coreM.startRotation3D = false;
			coreM.startRotationZ = 0.1745f;
			coreM.startSpeed = 0f;
			coreM.maxParticles = 30;
			var coreS = distortion.shape;
			coreS.enabled = false;
			coreS.shapeType = ParticleSystemShapeType.Circle;
			coreS.radius = 0.67f;
			coreS.arcMode = ParticleSystemShapeMultiModeValue.Random;
			var sparkleSize = distortion.sizeOverLifetime;
			sparkleSize.enabled = true;
			sparkleSize.separateAxes = true;
			//sparkleSize.sizeMultiplier = 0.75f;
			sparkleSize.xMultiplier = 1.3f;
			effect.transform.localScale = Vector3.one * 1.5f;
			effect.GetOrAddComponent<EffectComponent>().applyScale = true;
			effect.GetComponentInChildren<Light>().color = Colors.twinsLightColor;
			var spikyImpStuff = effect.transform.Find("LocalRing").gameObject;
			if (spikyImpStuff)
			{
				var pMain = spikyImpStuff.GetComponent<ParticleSystem>().main;
				pMain.startColor = Colors.twinsLightColor;
				var renderer = spikyImpStuff.GetComponent<ParticleSystemRenderer>();
				renderer.material = new Material(renderer.material);
				renderer.material.SetTexture("_RemapTex", await LoadAsset<Texture2D>("bundle:purpleramp"));
				renderer.material.SetFloat("_AlphaBias", 0.1f);
				renderer.material.SetColor("_TintColor", new Color(0.42f, 0f, 1f));
			}
			return effect;
		}
	}
}