using EntityStates;
using RoR2;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
	public class VoidDeathState : GenericCharacterDeath
	{
		public override void OnEnter()
		{
			base.OnEnter();
			EffectManager.SpawnEffect(voidDeathEffect,
				new EffectData { origin = characterBody.corePosition, scale = characterBody.bestFitRadius }, true);
			Destroy(cachedModelTransform.gameObject);
			cachedModelTransform = null;
		}
	}
}