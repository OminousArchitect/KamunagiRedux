using EntityStates;
using RoR2;

namespace KamunagiOfChains.Data.States
{
    public class VoidDeathState : GenericCharacterDeath
    {
        public override void OnEnter()
        {
            base.OnEnter();
            EffectManager.SpawnEffect(voidDeathEffect, new EffectData
            {
                origin = characterBody.corePosition,
                scale = characterBody.bestFitRadius
            }, transmit: true);
            Destroy(cachedModelTransform.gameObject);
            cachedModelTransform = null;
        }
    }
}