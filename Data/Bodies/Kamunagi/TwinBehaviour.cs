using EntityStates;
using ExtraSkillSlots;
using KamunagiOfChains.Data.Bodies.Kamunagi.Extra;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using RoR2;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi
{
	public class TwinBehaviour : MonoBehaviour
	{
		private bool _muzzleToggle;
		public bool componentAddedToMaster;
		public CharacterBody body;
		public ExtraSkillLocator extraLocator;
		public ModelLocator mdlL;
		public ChildLocator childL;
		private int _zealMeter;
		public int maxZeal = 80;
		public bool alternateSkills;
		public MasterTwinBehaviour masterBehaviour;
		public float firingDelay = 0.15f;
		public float radius = 1.25f;
		//public float runtimeNumber = 9f;
		//public float runtimeNumber2 = 7.5f;
		
		public int zealMeter
		{
			get => _zealMeter;
			set
			{
				_zealMeter = Math.Min(value, maxZeal);
				if (_zealMeter != maxZeal) return;
				if (alternateSkills)
					UnsetOverrides();
				else
					SetOverrides();
				alternateSkills = !alternateSkills;
				_zealMeter = 0;
			}
		}

		private void SetOverrides()
		{
			for (var index = 0; index < body.skillLocator.allSkills.Length; index += 2)
			{
				var skill = body.skillLocator.allSkills[index];
				var slot = body.skillLocator.FindSkillSlot(skill);
				if (slot != SkillSlot.None && slot <= SkillSlot.Special)
					skill.SetSkillOverride(this, body.skillLocator.allSkills[index + 1].skillDef,
						GenericSkill.SkillOverridePriority.Contextual);
			}
			var extra = extraLocator.extraFourth;
			extra.SetSkillOverride(this, body.skillLocator.allSkills[9].skillDef, GenericSkill.SkillOverridePriority.Contextual);
			body.skillLocator.ResetSkills();
			extraLocator.extraFourth.Reset();
			
			Vector3 ururuu = childL.transformPairs[22].transform.position;
			EffectManager.SpawnEffect(Concentric.GetEffect<Overzeal>().WaitForCompletion(), new EffectData
			{
				origin = ururuu,
				scale = 1f
			}, transmit: true);
		}

		private void UnsetOverrides()
		{
			for (var index = 0; index < body.skillLocator.allSkills.Length; index += 2)
			{
				var skill = body.skillLocator.allSkills[index];
				var slot = body.skillLocator.FindSkillSlot(skill);
				if (slot != SkillSlot.None && slot <= SkillSlot.Special)
					skill.UnsetSkillOverride(this, body.skillLocator.allSkills[index + 1].skillDef,
						GenericSkill.SkillOverridePriority.Contextual);
			}
			var extra = extraLocator.extraFourth;
			extra.UnsetSkillOverride(this, body.skillLocator.allSkills[9].skillDef, GenericSkill.SkillOverridePriority.Contextual);
			body.skillLocator.ResetSkills();
			extraLocator.extraFourth.Reset();

			Vector3 saraana = childL.transformPairs[23].transform.position;
			EffectManager.SpawnEffect(Concentric.GetEffect<Overzeal>().WaitForCompletion(), new EffectData
			{
				origin = saraana,
				scale = 1
			}, transmit: true);
		}

		public string twinMuzzle
		{
			get
			{
				_muzzleToggle = !_muzzleToggle;
				return _muzzleToggle ? "MuzzleRight" : "MuzzleLeft";
			}
		}

		public float zealMeterNormalized => (float) zealMeter / (float) maxZeal;

		public void Awake()
		{
			body = GetComponent<CharacterBody>();
			extraLocator = GetComponent<ExtraSkillLocator>();
			mdlL = GetComponent<ModelLocator>();
			childL = mdlL.modelTransform.GetComponent<ChildLocator>();
			foreach (var esm in body.GetComponents<EntityStateMachine>())
			{
				esm.nextStateModifier += ModifyNextState;
			}
		}

		public void ModifyNextState(EntityStateMachine entitystatemachine, ref EntityState nextState)
		{
			if (entitystatemachine.state is IZealState currentZealState)
			{
				zealMeter += currentZealState.meterGainOnExit;
			}
			if (nextState is IZealState zealState)
			{
				zealMeter += zealState.meterGain;
			}
		}

		public void FixedUpdate()
		{
			if (!componentAddedToMaster && body.masterObject)
			{
				masterBehaviour = body.masterObject.GetOrAddComponent<MasterTwinBehaviour>();
				componentAddedToMaster = true;
			}
		}
	}

	public class MasterTwinBehaviour : NetworkBehaviour
	{
		public CharacterMaster master;
		public Dictionary<GameObject, CharacterMaster?> NugwisoSpiritDefs = null!;

		public void SpawnedNugwiso(CharacterMaster characterMaster, int key)
		{
			if (!NetworkServer.active)
			{
				log.LogError("Called SpawnedNugwiso not on server.");
				return;
			}

			var writer = new NetworkWriter();
			writer.StartMessage(1345);
			writer.Write(netIdentity);
			writer.Write(key);
			writer.Write(characterMaster.networkIdentity);
			writer.FinishMessage();
			NetworkServer.SendWriterToReady(gameObject, writer, GetNetworkChannel());
		}

		[NetworkMessageHandler(msgType = 1345, client = true, server = false)]
		public static void HandleSpawnedNugwiso(NetworkMessage message)
		{
			var reader = message.reader;
			var netId = reader.ReadNetworkIdentity();
			var masterTwin = netId.gameObject.GetComponent<MasterTwinBehaviour>();
			masterTwin.NugwisoSpiritDefs[SummonNugwisomkamiState.NugwisoEliteDefs.Keys.ElementAt(reader.ReadInt32())] = reader.ReadNetworkIdentity().GetComponent<CharacterMaster>();
		}

		public void Awake()
		{
			NugwisoSpiritDefs = SummonNugwisomkamiState.NugwisoEliteDefs.Keys.ToDictionary(x => x, x => null as CharacterMaster);
			
			master = GetComponent<CharacterMaster>();
			master.onBodyDeath.AddListener(OnDeath);
		}

		public void OnDeath()
		{
			if (master.GetBody().HasBuff(Concentric.GetBuffIndex<MashiroBlessing>().WaitForCompletion()))
			{
				master.preventGameOver = true;
				Invoke(nameof(MashiroRebirth), 1.5f);
			}
		}

		public void MashiroRebirth()
		{
			master.preventGameOver = false;
			var positionAtDeath = master.deathFootPosition;
			if (master.killedByUnsafeArea)
			{
				positionAtDeath =
					TeleportHelper.FindSafeTeleportDestination(positionAtDeath, master.GetBody(),
						RoR2Application.rng) ?? master.deathFootPosition;
			}

			var body = master.Respawn(positionAtDeath, Quaternion.identity);
			body.AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
			var rezEffect = Concentric.GetEffect<MashiroBlessingRespawn>().WaitForCompletion();
			if (!master.bodyInstanceObject) return;
			var array = master.bodyInstanceObject.GetComponents<EntityStateMachine>();
			foreach (var esm in array)
			{
				esm.initialStateType = esm.mainStateType;
			}

			if (!rezEffect) return;
			EffectManager.SpawnEffect(rezEffect,
				new EffectData { origin = positionAtDeath, rotation = master.bodyInstanceObject.transform.rotation },
				transmit: true);
		}
	}
}