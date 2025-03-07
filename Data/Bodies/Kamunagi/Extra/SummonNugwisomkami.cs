﻿using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Secondary;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Events;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class SummonNugwisomkamiState : IndicatorSpellState
	{
		public override int meterGain => 0;
		public override int meterGainOnExit => possibleSpirits.Length != 0 || deadSpirits.Length != 0 ? 10 : 0;
		public GameObject[] possibleSpirits;
		public CharacterMaster[] deadSpirits;

		public static Dictionary<GameObject, List<string>> NugwisoEliteDefs = new Dictionary<GameObject, List<string>>()
		{
			{
				Asset.GetGameObject<AssassinSpirit, IMaster>(),
				new List<string>() { "EliteLightningEquipment", "EliteFireEquipment" } //needs recoloring, then done(?)
			}, //Mischief
			
			{
				Asset.GetGameObject<WarMachine, IMaster>(),
				new List<string>() { "EliteVoidEquipment", "EliteLunarEquipment" } //completely done
			}, //Hubris

			/*{
				Asset.GetGameObject<VirusArchWisp, IMaster>(),
				new List<string>() { "EliteEarthEquipment", "ElitePoisonEquipment" }
			}, //Pestilence*/

			{
				Asset.GetGameObject<IceTank, IMaster>(),
				new List<string>() { "EliteIceEquipment", "EliteHauntedEquipment" }
			} //Solitude
		};
		public override void OnEnter()
		{
			base.OnEnter();
			//collect dictionary
			//then put the keys with null value into an array
			possibleSpirits = twinBehaviour.masterBehaviour.NugwisoSpiritDefs.Where(x => x.Value == null).Select(x => x.Key).ToArray();
			deadSpirits = twinBehaviour.masterBehaviour.NugwisoSpiritDefs.Where(x => x.Value != null && x.Value && x.Value.lostBodyToDeath).Select(x => x.Value!).ToArray();
			
			if (possibleSpirits.Length == 0 && deadSpirits.Length == 0) outer.SetNextStateToMain();
			var pos = characterBody.corePosition + Vector3.up * 4;
		}

		public override void Fire(Vector3 targetPosition)
		{
			SpawnSpirit(targetPosition);
		}

		public void SpawnSpirit(Vector3 spawnPosition)
		{
			
			if (possibleSpirits.Length != 0)
			{
				var index = Mathf.RoundToInt(UnityEngine.Random.Range(0, possibleSpirits.Length));
				log.LogInfo("Index: " + index);
				var whichSpirit = possibleSpirits[index];
				var summon = new MasterSummon
				{
					masterPrefab = whichSpirit,
					position = spawnPosition,
					summonerBodyObject = gameObject,
					ignoreTeamMemberLimit = true,
					useAmbientLevel = true
				};
				summon.preSpawnSetupCallback += master =>
				{
					var equipList = NugwisoEliteDefs[whichSpirit];
					var whichEquip = equipList[Mathf.RoundToInt(UnityEngine.Random.Range(0, equipList.Count))];
					master.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex(whichEquip));
					twinBehaviour.masterBehaviour.NugwisoSpiritDefs[whichSpirit] = master;
				};
				var characterMaster = summon.Perform();
				var deployable = characterMaster.gameObject.AddComponent<Deployable>();
				deployable.onUndeploy ??= new UnityEvent();
				deployable.onUndeploy.AddListener(characterMaster.TrueKill);
				
				characterBody.master.AddDeployable(deployable, SummonNugwisomkami.deployableSlot);
				
			} else if (deadSpirits.Length != 0)
			{
				var nextSpiritMaster = deadSpirits[Mathf.RoundToInt(UnityEngine.Random.Range(0, deadSpirits.Length))];
				nextSpiritMaster.Respawn(spawnPosition, Quaternion.identity);
				var equipList = NugwisoEliteDefs.First(x => x.Key.GetComponent<CharacterMaster>().masterIndex == nextSpiritMaster.masterIndex).Value;
				var whichEquip = equipList[Mathf.RoundToInt(UnityEngine.Random.Range(0, equipList.Count))];
				nextSpiritMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex(whichEquip));
			}
		}
	}

	public class SummonNugwisomkami : Asset, ISkill
	{
		public static DeployableSlot deployableSlot;

		public SummonNugwisomkami()
		{
			deployableSlot = DeployableAPI.RegisterDeployableSlot((_,_) => 1);
		}

		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA5_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA5_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle2:Nugwisomkami");
			// TODO i dont know what else to put here
			return skill;
		}

		public IEnumerable<Type> GetEntityStates() => new []{typeof(SummonNugwisomkamiState)};
	}
}