﻿using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Events;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class SummonFriendlyEnemyState : BaseTwinState
	{
		public override int meterGain => 0;
		public override int meterGainOnExit => possibleElites.Length != 0 || deadElites.Length != 0 ? 10 : 0;
		public GameObject[] possibleElites;
		public CharacterMaster[] deadElites;

		public static Dictionary<GameObject, List<string>> NugwisomkamiEliteDef = new Dictionary<GameObject, List<string>>()
		{
			{
				Asset.GetGameObject<NugwisomkamiOne, IMaster>(),
				new List<string>() { "EliteIceEquipment", "EliteFireEquipment" }
			},
			{
				Asset.GetGameObject<NugwisomkamiTwo, IMaster>(),
				new List<string>() { "EliteLightningEquipment", "EliteLunarEquipment" }
			}
			
			//mending or malachite goes here
		};

		public override void OnEnter()
		{
			base.OnEnter();
			//collect the dictionary values,
			//then put the keys in an array, if they are not null
			possibleElites = twinBehaviour.masterBehaviour.NugwisomkamiSpiritDefs.Where(x => x.Value == null).Select(x => x.Key).ToArray(); 
			
			deadElites = twinBehaviour.masterBehaviour.NugwisomkamiSpiritDefs.Where(x => x.Value != null && x.Value && x.Value.lostBodyToDeath).Select(x => x.Value!).ToArray();
			if (possibleElites.Length == 0 && deadElites.Length == 0) outer.SetNextStateToMain();
			var position = characterBody.corePosition + Vector3.up * 3;
			SpawnSpirit(position);
		}

		public void SpawnSpirit(Vector3 targetPosition)
		{
			var rollBody = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
			var rollEquip = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
			var spawnPosition = targetPosition +
			               Vector3.up;
			if (possibleElites.Length != 0)
			{
				var whichWisp = possibleElites[possibleElites.Length > 1 ? rollBody.RangeInt(0, possibleElites.Length - 1) : 0];
				var summon = new MasterSummon
				{
					masterPrefab = whichWisp,
					position = spawnPosition,
					summonerBodyObject = gameObject,
					ignoreTeamMemberLimit = true
				};
				summon.preSpawnSetupCallback += master =>
				{
					var equipList = NugwisomkamiEliteDef[whichWisp];
					var whichEquip = equipList[equipList.Count > 1 ? rollEquip.RangeInt(0, equipList.Count - 1) : 0];
					master.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex(whichEquip));
					twinBehaviour.masterBehaviour.NugwisomkamiSpiritDefs[whichWisp] = master;
				};
				var characterMaster = summon.Perform();
				var deployable = characterMaster.gameObject.AddComponent<Deployable>();
				deployable.onUndeploy ??= new UnityEvent();
				deployable.onUndeploy.AddListener(characterMaster.TrueKill);
				
				characterBody.master.AddDeployable(deployable, SummonFriendlyEnemy.deployableSlot);
				
			} else if (deadElites.Length != 0)
			{
				var nextMaster = deadElites[deadElites.Length > 1 ? rollBody.RangeInt(0, deadElites.Length - 1) : 0];
				nextMaster.Respawn(spawnPosition, Quaternion.identity);
				var equipList = NugwisomkamiEliteDef.First(x => x.Key.GetComponent<CharacterMaster>().masterIndex == nextMaster.masterIndex).Value;
				var whichEquip = equipList[equipList.Count > 1 ? rollEquip.RangeInt(0, equipList.Count - 1) : 0];
				nextMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex(whichEquip));
			}
		}
	}

	public class SummonFriendlyEnemy : Asset, ISkill
	{
		public static DeployableSlot deployableSlot;

		public SummonFriendlyEnemy()
		{
			deployableSlot = DeployableAPI.RegisterDeployableSlot((_,_) => 3);
		}

		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA5_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA5_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("n");
			// TODO i dont know what else to put here
			return skill;
		}

		public Type[] GetEntityStates() => new []{typeof(SummonFriendlyEnemyState)};
	}
	
	public class NugwisomkamiOne : Asset, IBody, IMaster
	{
		GameObject IBody.BuildObject()
		{
			Material wispMat = new Material(LoadAsset<Material>("RoR2/Base/Wisp/matWispFire.mat"));
			wispMat.SetFloat("_BrightnessBoost", 2.63f);
			wispMat.SetFloat("_AlphaBoost", 1.2f);
			wispMat.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
			wispMat.SetColor("_TintColor", Colors.wispNeonGreen);

			var wispBody = LoadAsset<GameObject>("RoR2/Base/Wisp/WispBody.prefab")!.InstantiateClone("Nugwiso1", true);
			wispBody.GetComponent<CharacterBody>().baseNameToken = "NUGWISOMKAMI1_BODY_NAME";
			var charModel = wispBody.GetComponentInChildren<CharacterModel>();
			charModel.baseRendererInfos[0].defaultMaterial = LoadAsset<Material>("RoR2/DLC1/Assassin2/matAssassin2.mat"); //mesh
			charModel.baseRendererInfos[1].defaultMaterial = wispMat; //fire particle system
			charModel.baseLightInfos[0].defaultColor = Colors.wispNeonGreen;
			var mdl = wispBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			mdl.GetComponentInChildren<HurtBox>().transform.SetParent(mdl.transform); //set parent of the hurtbox outside of the armature, so we don't destroy it, too
			mdl.GetComponentInChildren<ParticleSystemRenderer>().transform.SetParent(mdl.transform); //do the same to the fire particles
			UnityEngine.Object.Destroy(mdl.transform.GetChild(1).gameObject); //destroy armature, we don't need it
			var meshObject = mdl.transform.GetChild(0).gameObject;
			UnityEngine.Object.Destroy(meshObject.GetComponent<SkinnedMeshRenderer>());
			UnityEngine.Object.Destroy(mdl.GetComponentInChildren<SkinnedMeshRenderer>());
			meshObject.AddComponent<MeshFilter>().mesh = LoadAsset<Mesh>("bundle2:TheMask");
			meshObject.AddComponent<MeshRenderer>().material = LoadAsset<Material>("RoR2/DLC1/Assassin2/matAssassin2.mat");
			meshObject.transform.localPosition = new Vector3(0, -4.8f, 0);
			return wispBody;
		}
		
		GameObject IMaster.BuildObject()
		{
			var master = LoadAsset<GameObject>("RoR2/Base/Wisp/WispMaster.prefab")!.InstantiateClone("Nugwiso1Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<NugwisomkamiOne, IBody>();
			return master;
		}
	}
	
	public class NugwisomkamiTwo : Asset, IBody, IMaster
	{
		GameObject IBody.BuildObject()
		{
			Material wispMat = new Material(LoadAsset<Material>("RoR2/Base/LunarWisp/matLunarWispFlames.mat"));
			//wispMat.SetFloat("_BrightnessBoost", 2.63f);
			//wispMat.SetFloat("_AlphaBoost", 1.2f);
			wispMat.SetTexture("_RemapTex", LoadAsset<Texture2D>("bundle:purpleramp"));
			wispMat.SetColor("_TintColor", Color.white);

			var wispBody = LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispBody.prefab")!.InstantiateClone("Nugwiso2", true);
			var mdl = wispBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			Vector3 smaller = new Vector3(0.2f, 0.2f, 0.2f);
			mdl.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
			mdl.transform.GetChild(1).localScale = smaller;
			mdl.transform.GetChild(2).localScale = smaller;
			mdl.transform.GetChild(4).localScale = smaller; //particle center
			mdl.transform.GetChild(5).localScale = smaller; //particle left
			mdl.transform.GetChild(6).localScale = smaller; //particle right
			var light = mdl.transform.Find("StandableSurface/Point light").gameObject;
			light.GetComponent<Light>().color = Colors.twinsLightColor;
			var cb = wispBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI2_BODY_NAME";
			cb.moveSpeed = 15f;
			cb.acceleration = 14f;
			return wispBody;
		}

		GameObject IMaster.BuildObject()
		{
			var master = LoadAsset<GameObject>("RoR2/Base/Wisp/WispMaster.prefab")!.InstantiateClone("Nugwiso2Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<NugwisomkamiTwo, IBody>();
			return master;
		}
	}

	/*public class NugwisomkamiThree : Asset, IBody
	{
		GameObject IBody.BuildObject()
		{
			var wispBody
		}
	}*/
}