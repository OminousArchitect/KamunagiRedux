using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using RoR2;
using RoR2.Navigation;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class TrackingWispsState : EntityStates.GravekeeperMonster.Weapon.GravekeeperBarrage
	{
		
	}
	
	public class AssassinSpiritPrimary : Asset, ISkill
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 8f;
			skill.icon = LoadAsset<Sprite>("n");
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(TrackingWispsState) };
	}
	
	public class AssassinSpiritPrimaryFamily : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<AssassinSpiritPrimary>() };
	}
	
	public class SpiritTeleportState : BaseState
	{
		public CharacterModel? charModel;
		public HurtBoxGroup? hurtBoxGroup;
		public EffectManagerHelper? veilEffect;
		private GameObject childTpFx;
		private Vector3 teleportPosition;
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
			Vector3 effectPos = characterBody.corePosition;
			EffectManager.SpawnEffect(childTpFx, new EffectData
			{
				origin = effectPos,
				scale = 1f
			}, transmit: true);
			Util.PlaySound("Play_imp_attack_blink", gameObject);
			NodeGraph airNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Air);
			NodeGraph groundNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
			availableNodes = airNodes;
			var nodesInRange = availableNodes.FindNodesInRange(characterBody.footPosition, 25f, 37f, HullMask.Human);
			NodeGraph.NodeIndex nodeIndex = nodesInRange.ElementAt(UnityEngine.Random.Range(1, nodesInRange.Count));
			availableNodes.GetNodePosition(nodeIndex, out var footPosition);
			footPosition += Vector3.up * 1.5f;
			teleportPosition = footPosition;
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

		public override void OnExit()
		{
			base.OnExit();
			if (NetworkServer.active) characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
			if (veilEffect != null) veilEffect.ReturnToPool();
			Util.PlaySound("Play_imp_attack_blink", gameObject);
			if (charModel != null && charModel && hurtBoxGroup != null && hurtBoxGroup)
			{
				charModel.invisibilityCount--;
				hurtBoxGroup.hurtBoxesDeactivatorCounter--;
			}
			Vector3 effectPos = characterBody.corePosition;
			EffectManager.SpawnEffect(childTpFx, new EffectData
			{
				origin = effectPos,
				scale = 1f
			}, transmit: true);
		}
	}
	
	internal class AssassinSpiritSecondary : Asset, ISkill
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 8f;
			skill.icon = LoadAsset<Sprite>("n");
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(SpiritTeleportState) };
	}
	
	public class AssassinSpiritSecondaryFamily : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<AssassinSpiritSecondary>() };
	}
}