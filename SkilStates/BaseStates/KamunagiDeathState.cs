using System.Runtime.CompilerServices;
using EntityStates;
using RoR2;

namespace Kamunagi
{
    public class KamunagiDeathState : GenericCharacterDeath
    {
        public override void OnEnter()
        {
            base.OnEnter();
            EffectManager.SpawnEffect(voidDeathEffect, new EffectData
            {
                origin = base.characterBody.corePosition,
                scale = base.characterBody.bestFitRadius
            }, transmit: true);
            EntityState.Destroy(cachedModelTransform.gameObject);
            cachedModelTransform = null;
        }
    }
}