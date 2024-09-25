﻿using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class TwinsChildTeleportState : BaseTwinState
	{
		public CharacterModel? charModel;
		public HurtBoxGroup? hurtBoxGroup;
		public EffectManagerHelper? veilEffect;
		private GameObject childTpFx;
		private Vector3 teleportPosition;
		public override int meterGain => 0;
		private float duration = 0.45f;
		private bool teleported;
		private NodeGraph? availableNodes;

		public override void OnEnter()
		{
			base.OnEnter();
			childTpFx = LoadAsset<GameObject>("RoR2/DLC2/Child/FrolicTeleportVFX.prefab")!;
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

			NodeGraph airNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Air);
			NodeGraph groundNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
			if (twinBehaviour)
			{
				availableNodes = characterMotor.isGrounded ? groundNodes : airNodes;
			}
			else
			{
				availableNodes = airNodes;
			}
			var nodesInRange = availableNodes.FindNodesInRange(characterBody.footPosition, 25f, 37f, HullMask.Human);
			NodeGraph.NodeIndex nodeIndex = nodesInRange.ElementAt(UnityEngine.Random.Range(1, nodesInRange.Count));
			availableNodes.GetNodePosition(nodeIndex, out var footPosition);
			footPosition += Vector3.up * 1.5f;
			teleportPosition = footPosition;
			EffectManager.SpawnEffect(LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflash.prefab"), new EffectData
			{
				origin = Util.GetCorePosition(base.gameObject),
				rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward)
			}, false);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;

			if (fixedAge > 0.2f && !teleported)
			{
				teleported = true;
				Vector3 effectPos = FindModelChild("MuzzleCenter").transform.position;
				EffectManager.SpawnEffect(childTpFx, new EffectData
				{
					origin = effectPos,
					scale = 1f
				}, transmit: true);
				Util.PlaySound("Play_child_attack2_reappear", base.gameObject);
				TeleportHelper.TeleportBody(base.characterBody, teleportPosition);
			}
			
			if (base.fixedAge >= duration)
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
			EffectManager.SpawnEffect(LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflash.prefab"), new EffectData
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

	public class TwinsChildTeleport : Asset, ISkill, IEffect
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
			skill.mustKeyPress = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.keywordTokens = new[] { KamunagiAsset.tokenPrefix + "TWINSBLESSING_KEYWORD" };
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(TwinsChildTeleportState) };

		GameObject IEffect.BuildObject()
		{
			var impBoss = LoadAsset<GameObject>("RoR2/Base/ImpBoss/ImpBossBody.prefab")!;
			var dustCenter = impBoss.transform.Find("ModelBase/mdlImpBoss/DustCenter");

			var effect = dustCenter.gameObject.InstantiateClone("VeilParticles", false);
			UnityEngine.Object.Destroy(effect.transform.GetChild(0).gameObject);
			var distortion = effect.AddComponent<ParticleSystem>();
			var coreR = effect.GetComponent<ParticleSystemRenderer>();
			Material decalMaterial = new Material(LoadAsset<Material>("RoR2/Base/Common/VFX/matInverseDistortion.mat"));
			decalMaterial.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
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
				renderer.material.SetTexture("_RemapTex", LoadAsset<Texture2D>("bundle:purpleramp"));
				renderer.material.SetFloat("_AlphaBias", 0.1f);
				renderer.material.SetColor("_TintColor", new Color(0.42f, 0f, 1f));
			}
			return effect;
		}
	}
}