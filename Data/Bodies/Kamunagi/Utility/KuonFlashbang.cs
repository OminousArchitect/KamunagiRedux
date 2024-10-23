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
	public class KuonFlashbangState : BaseTwinState
	{
		public CharacterModel? charModel;
		public HurtBoxGroup? hurtBoxGroup;
		public EffectManagerHelper? veilEffect;
		public static GameObject childTpFx;
		private Vector3 teleportPosition;
		public override int meterGain => 0;
		private float duration = 0.45f;
		private bool teleported;
		private NodeGraph? availableNodes;

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
			Util.PlaySound("Play_child_attack2_teleport", gameObject);

			NodeGraph airNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Air);
			NodeGraph groundNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
			availableNodes = characterMotor.isGrounded ? groundNodes : airNodes;
			var nodesInRange = availableNodes.FindNodesInRange(characterBody.footPosition, 25f, 37f, HullMask.Human);
			NodeGraph.NodeIndex nodeIndex = nodesInRange.ElementAt(UnityEngine.Random.Range(1, nodesInRange.Count));
			availableNodes.GetNodePosition(nodeIndex, out var footPosition);
			footPosition += Vector3.up * 1.5f;
			teleportPosition = footPosition;
			
			new BlastAttack
			{
				attacker = gameObject,
				baseDamage = damageStat * 1.75f,
				baseForce = 50f,
				crit = false,
				damageType = DamageType.Stun1s,
				falloffModel = BlastAttack.FalloffModel.None,
				procCoefficient = 2f,
				radius = 8f,
				position = characterBody.corePosition,
				attackerFiltering = AttackerFiltering.NeverHitSelf,
				teamIndex = teamComponent.teamIndex
			}.Fire();
			DoChildFx(characterBody.corePosition);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;

			if (fixedAge > 0.2f && !teleported)
			{
				teleported = true;
				Util.PlaySound("Play_child_attack2_reappear", base.gameObject);
				TeleportHelper.TeleportBody(base.characterBody, teleportPosition);
			}
			
			if (base.fixedAge >= duration)
			{
				outer.SetNextStateToMain();
			}
		}
		
		public void DoChildFx(Vector3 effectPos)
		{
			EffectManager.SpawnEffect(childTpFx, new EffectData
			{
				origin = effectPos,
				scale = 1f
			}, transmit: true);
		}

		public override void OnExit()
		{
			base.OnExit();
			if (NetworkServer.active) characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
			if (veilEffect != null) veilEffect.ReturnToPool();
			DoChildFx(characterBody.corePosition);
			if (charModel != null && charModel && hurtBoxGroup != null && hurtBoxGroup)
			{
				charModel.invisibilityCount--;
				hurtBoxGroup.hurtBoxesDeactivatorCounter--;
			}
		}
	}

	public class KuonFlashbang : Asset, ISkill, IEffect
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			KuonFlashbangState.childTpFx = await LoadAsset<GameObject>("RoR2/DLC2/Child/FrolicTeleportVFX.prefab");
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Utility 9";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA2_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA2_DESCRIPTION";
			skill.icon = await LoadAsset<Sprite>("bundle:Mikazuchi");
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 1f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.canceledFromSprinting = false;
			skill.mustKeyPress = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.keywordTokens = new[] { KamunagiAsset.tokenPrefix + "TWINSBLESSING_KEYWORD" };
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(KuonFlashbangState) };

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