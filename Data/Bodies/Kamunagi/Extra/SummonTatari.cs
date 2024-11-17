using BepInEx.Configuration;
using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Special;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class SpawnTatariState : BaseState
	{
		public Vector3 position;

		public override void OnEnter()
		{
			base.OnEnter();
			if (!NetworkServer.active) return;
			var tatariSummon = new MasterSummon()
			{
				masterPrefab = Concentric.GetMaster<TatariBody>().WaitForCompletion(),
				position = position,
				summonerBodyObject = gameObject,
				ignoreTeamMemberLimit = true,
				useAmbientLevel = true,
			};
			tatariSummon.preSpawnSetupCallback += master =>
			{
				master.inventory.GiveItem(RoR2Content.Items.HealWhileSafe, 6);
				master.inventory.GiveItem(RoR2Content.Items.StunChanceOnHit, 4);
			};
			var tatariMaster = tatariSummon.Perform();
			var deployable = tatariMaster.gameObject.AddComponent<Deployable>();
			deployable.onUndeploy ??= new UnityEvent();
			deployable.onUndeploy.AddListener(tatariMaster.TrueKill);
			characterBody.master.AddDeployable(deployable, SummonTatari.deployableSlot);
		}

		public override void OnSerialize(NetworkWriter writer)
		{
			 base.OnSerialize(writer);
			 writer.Write(position);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			position = reader.ReadVector3();
		}
	}
	public class SummonTatariState : IndicatorSpellState
	{
		public override void Fire(Vector3 position)
		{
			outer.SetNextState(new SpawnTatariState() { position = position + Vector3.up * 4 });
		}
	}

	public class SummonTatari : Concentric, ISkill
	{
		public static DeployableSlot deployableSlot;

		public SummonTatari()
		{
			deployableSlot = DeployableAPI.RegisterDeployableSlot((_,_) => 1);
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 6";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA7_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA7_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("bundle:Uitsalnemetia"));
			return skill;
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(SummonTatariState), typeof(SpawnTatariState) };
	}
}