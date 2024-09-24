using EntityStates;
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
		public override int meterGain => 0;
		
		public static GameObject projectilePrefab;

		public static GameObject effectPrefab;

		public static GameObject tpEffectPrefab;

		public static float baseDuration = 2f;

		public static float damageCoefficient = 1.2f;

		public static float force = 20f;

		public static string attackString;

		public static float tpDuration = 0.5f;

		public static float fireFrolicDuration = 0.3f;

		public static float frolicCooldownDuration = 18f;

		private float duration;

		private bool frolicFireFired;

		private bool tpFired;

		private Transform position;

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
			var effect = Asset.GetGameObject<HonokasVeil, IEffect>();
			if (NetworkServer.active) characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
			EffectManager.SpawnEffect(LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflash.prefab"), new EffectData
			{
				origin = Util.GetCorePosition(base.gameObject),
				rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward)
			}, false);
			//veilEffect = EffectManager.GetAndActivatePooledEffect(effect, characterBody.coreTransform, true);
			var hurtBoxes = new SphereSearch
				{
					origin = characterBody.corePosition, radius = 30, mask = LayerIndex.entityPrecise.mask
				}
				.RefreshCandidates()
				.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamComponent.teamIndex))
				.OrderCandidatesByDistance()
				.FilterCandidatesByDistinctHurtBoxEntities()
				.GetHurtBoxes();
			if (hurtBoxes[0]) position = hurtBoxes[0].transform;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;
			if (base.fixedAge > tpDuration && !tpFired)
			{
				FireTPEffect();
				tpFired = true;
				TeleportAroundPlayer();
			}
			if (base.fixedAge > fireFrolicDuration && !frolicFireFired)
			{
				frolicFireFired = true;
				FireTPEffect();
				Transform position1 = position;
				base.characterBody.transform.LookAt(position1.position);
			}
			if (base.fixedAge >= duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}
		
		public void TeleportAroundPlayer()
		{
			GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>();
			_ = base.characterBody.corePosition;
			NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
			Vector3 variable = position.position;
			List<NodeGraph.NodeIndex> list = nodeGraph.FindNodesInRange(variable, 25f, 37f, HullMask.Human);
			Vector3 nodePosition = default(Vector3);
			bool flag = false;
			int num = 35;
			while (!flag)
			{
				NodeGraph.NodeIndex nodeIndex = list.ElementAt(UnityEngine.Random.Range(1, list.Count));
				nodeGraph.GetNodePosition(nodeIndex, out nodePosition);
				float num2 = Vector3.Distance(base.characterBody.coreTransform.position, nodePosition);
				num--;
				if (num2 > 55f || num < 0)
				{
					flag = true;
				}
			}
			if (num < 0)
			{
				Debug.LogWarning("Twins.Frolic state entered a loop where it ran more than 35 times without getting out");
			}
			nodePosition += Vector3.up * 1.5f;
			TeleportHelper.TeleportBody(base.characterBody, nodePosition);
		}

		public void FireTPEffect()
		{
			Vector3 position = FindModelChild("Chest").transform.position;
			EffectManager.SpawnEffect(tpEffectPrefab, new EffectData
			{
				origin = position,
				scale = 1f
			}, transmit: true);
			Util.PlaySound("Play_child_attack2_reappear", base.gameObject);
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