using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Console = System.Console;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
	public class VoidPortalSpawnState : BaseState
	{
		public bool earlyBufferDone;
		public bool fuck;
		public static GameObject spawnEffectPrefab;
		private CharacterModel charModel;

		public override void OnEnter()
		{
			base.OnEnter();
			if (NetworkServer.active)
			{
				characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 3f);
			}
			charModel = GetModelTransform().GetComponent<CharacterModel>();
			if (charModel)
			{
				charModel.invisibilityCount++;
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= 1f && !earlyBufferDone)
			{
				earlyBufferDone = true;
				Util.PlaySound("Play_nullifier_spawn", gameObject);
				EffectManager.SimpleMuzzleFlash(spawnEffectPrefab, gameObject, "MuzzleRear", false);
			}

			if (fixedAge >= 1.3f && !fuck)
			{
				fuck = true;
				charModel.invisibilityCount--;
			}

			if (fixedAge >= 3.2f)
			{
				outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;
	}

	public class VoidPortalSpawn : Concentric, IEntityStates
	{
		public IEnumerable<Type> GetEntityStates() => new[] { typeof(VoidPortalSpawnState) };
		public override async Task Initialize()
		{
			await base.Initialize();
			VoidPortalSpawnState.spawnEffectPrefab = await LoadAsset<GameObject>("addressable:RoR2/Base/Nullifier/NullifierSpawnEffect.prefab");
		}
	}
}