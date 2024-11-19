using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using Newtonsoft.Json.Utilities;
using R2API;
using RoR2;
using RoR2.Navigation;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class SpawnNugwisomkamiState : BaseState
	{
		public int whichSpirit;
		public EquipmentIndex whichEquip;
		public Vector3 spawnPosition;

		public override void OnEnter()
		{
			base.OnEnter();
			if (!NetworkServer.active) return;
			var summon = new MasterSummon
			{
				masterPrefab = SummonNugwisomkamiState.NugwisoEliteDefs.Keys.ElementAt(whichSpirit),
				position = spawnPosition,
				summonerBodyObject = gameObject,
				ignoreTeamMemberLimit = true,
				useAmbientLevel = true
			};
			summon.preSpawnSetupCallback += master =>
			{
				master.inventory.SetEquipmentIndex(whichEquip);
				log.LogDebug($"spirit was [ {whichSpirit} ]");
				/*if (whichSpirit == 3)
				{
					log.LogDebug("this was the 3 spirit");
					master.inventory.GiveItem(RoR2Content.Items.SiphonOnLowHealth, 3);
				}
				else
				{
					log.LogDebug("this wasn't the 3 spirit");
					master.inventory.GiveItem(RoR2Content.Items.SiphonOnLowHealth, 1);
				}*/
				outer.SetNextState(new NugwisomkamiSpawnedState() { master = master, whichSpirit = whichSpirit });
			};
			var characterMaster = summon.Perform();
			var deployable = characterMaster.gameObject.AddComponent<Deployable>();
			deployable.onUndeploy ??= new UnityEvent();
			deployable.onUndeploy.AddListener(characterMaster.TrueKill);

			characterBody.master.AddDeployable(deployable, SummonNugwisomkami.deployableSlot);
		}

		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write(whichSpirit);
			writer.Write(whichEquip);
			writer.Write(spawnPosition);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			whichSpirit = reader.ReadInt32();
			whichEquip = reader.ReadEquipmentIndex();
			spawnPosition = reader.ReadVector3();
		}
	}

	public class RespawnNugwisomkamiState : BaseTwinState
	{
		public CharacterMaster nextSpiritMaster;
		public Vector3 spawnPosition;
		public override int meterGain => 0;

		public override void OnEnter()
		{
			base.OnEnter();
			nextSpiritMaster.Respawn(spawnPosition, Quaternion.identity);
			var equipList = SummonNugwisomkamiState.NugwisoEliteDefs.First(x =>
				x.Key.GetComponent<CharacterMaster>().masterIndex == nextSpiritMaster.masterIndex).Value;
			var whichEquip = equipList[Mathf.RoundToInt(UnityEngine.Random.Range(0, equipList.Count))];
			nextSpiritMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex(whichEquip));
		}
		
		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write(nextSpiritMaster.netId);
			writer.Write(spawnPosition);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			nextSpiritMaster = Util.FindNetworkObject(reader.ReadNetworkId()).GetComponent<CharacterMaster>();
			spawnPosition = reader.ReadVector3();
		}
	}

	public class NugwisomkamiSpawnedState : BaseTwinState
	{
		public override int meterGain => 0;
		public int whichSpirit;
		public CharacterMaster master;

		public override void OnEnter()
		{
			base.OnEnter();
			twinBehaviour.masterBehaviour.NugwisoSpiritDefs[SummonNugwisomkamiState.NugwisoEliteDefs.Keys.ElementAt(whichSpirit)] = master;
		}
		
		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write(whichSpirit);
			writer.Write(master.netId);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			whichSpirit = reader.ReadInt32();
			master = Util.FindNetworkObject(reader.ReadNetworkId()).GetComponent<CharacterMaster>();
		}
	}

	public class SummonNugwisomkamiState : IndicatorSpellState
	{
		public override int meterGain => 0;
		public override int meterGainOnExit => didSpawn ? 5 : 0;

		public static Dictionary<GameObject, List<string>> NugwisoEliteDefs;
		private bool didSpawn;
		private int whichSpirit;
		private EquipmentIndex whichEquip;
		private bool didRespawn;
		private CharacterMaster whichRespawnedMaster;

		public override void OnEnter()
		{
			base.OnEnter();
			//collect dictionary
			//then put the keys with null value into an array
			var possibleSpirits = twinBehaviour.masterBehaviour.NugwisoSpiritDefs.Where(x => x.Value == null)
				.Select(x => x.Key).ToArray();
			var deadSpirits = twinBehaviour.masterBehaviour.NugwisoSpiritDefs
				.Where(x => x.Value != null && x.Value && x.Value.lostBodyToDeath).Select(x => x.Value!).ToArray();

			if (possibleSpirits.Length == 0 && deadSpirits.Length == 0)
			{
				outer.SetNextStateToMain();
				return;
			}

			if (possibleSpirits.Length > 0)
			{
				didSpawn = true;
				var index = Mathf.RoundToInt(UnityEngine.Random.Range(0, possibleSpirits.Length));
				log.LogInfo("Index: " + index);
				whichSpirit = NugwisoEliteDefs.Keys.IndexOf(x => x == possibleSpirits[index]);

				var equipList = NugwisoEliteDefs[possibleSpirits[index]];
				whichEquip =
					EquipmentCatalog.FindEquipmentIndex(
						equipList[Mathf.RoundToInt(UnityEngine.Random.Range(0, equipList.Count))]);
			}
			else
			{
				whichRespawnedMaster = deadSpirits[Mathf.RoundToInt(UnityEngine.Random.Range(0, deadSpirits.Length))];
			}
		}

		public override void Fire(Vector3 targetPosition)
		{
			var properNode = ExtensionMethods.FindNearestNodePosition(targetPosition, MapNodeGroup.GraphType.Air);
			if (didSpawn)
				outer.SetNextState(new SpawnNugwisomkamiState()
				{
					spawnPosition = properNode, whichSpirit = whichSpirit, whichEquip = whichEquip
				});
			else
				outer.SetNextState(new RespawnNugwisomkamiState()
				{
					spawnPosition = properNode, nextSpiritMaster = whichRespawnedMaster
				});
		}
	}

	public class SummonNugwisomkami : Concentric, ISkill
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			SummonNugwisomkamiState.NugwisoEliteDefs = new Dictionary<GameObject, List<string>>()
			{
				{ //Mischief 0
					await GetMaster<AssassinSpirit>(),
					new List<string>() { "EliteLightningEquipment", "EliteFireEquipment" }
				},

				{ //War 1
					await GetMaster<WarMachine>(),
					new List<string>() { "EliteVoidEquipment", "EliteLunarEquipment" }
				}, 
				
				{ //Decay 2
					await GetMaster<DecaySpirit>(), 
					new List<string>() { "ElitePoisonEquipment", "EliteHauntedEquipment" }
				} 

				/*
				{ //Pestilence
				    await GetMaster<VirusArchWisp>(), new List<string>() { "EliteEarthEquipment", "ElitePoisonEquipment" }
				}, 
				*/
			};
		}

		public static DeployableSlot deployableSlot;

		public SummonNugwisomkami()
		{
			deployableSlot = DeployableAPI.RegisterDeployableSlot((_, _) => 1);
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA5_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA5_DESCRIPTION";
			skill.icon = (await LoadAsset<Sprite>("bundle2:Nugwisomkami"));
			//skill.mustKeyPress = true; //this breaks things apparently
			return skill;
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(SummonNugwisomkamiState), typeof(SpawnNugwisomkamiState), typeof(RespawnNugwisomkamiState), typeof(NugwisomkamiSpawnedState) };
	}
}