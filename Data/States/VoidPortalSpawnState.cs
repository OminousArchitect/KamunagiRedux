using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.States
{
    public class VoidPortalSpawnState : BaseState
    {
        public static float minimumIdleDuration = 1.5f;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, minimumIdleDuration);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= minimumIdleDuration)
            {
                outer.SetNextState(new BufferPortal());
            }
        }
    }

    public class BufferPortal : BaseState
    {
        private static GameObject _spawnEffectPrefab =
            Asset.LoadAsset<GameObject>("addressable:RoR2/Base/Nullifier/NullifierSpawnEffect.prefab")!;

        public static float duration = 2f;

        public override void OnEnter()
        {
            base.OnEnter();

            if (_spawnEffectPrefab)
            {
                //Util.PlaySound(EntityStates.NullifierMonster.SpawnState.spawnSoundString, gameObject);
                Util.PlaySound("Play_nullifier_spawn", gameObject);
                EffectManager.SimpleMuzzleFlash(_spawnEffectPrefab, gameObject, "MuzzleRear", false);
            }

            if (NetworkServer.active)
                characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, duration * 1.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}