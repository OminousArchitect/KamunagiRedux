using RoR2;
using UnityEngine;

namespace KamunagiOfChains
{
	public static class EffectManagerKamunagi
	{
		public static EffectManagerHelper GetAndActivatePooledEffect(GameObject prefab, Transform parentTransform,
			bool inResetLocal = false, EffectData? data = null)
		{
			var pooledEffect = EffectManager.GetPooledEffect(prefab, parentTransform);
			var gameObject = pooledEffect.gameObject;
			if (inResetLocal)
			{
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
			}

			var effectComponent = gameObject.GetComponent<EffectComponent>();
			if (data != null && effectComponent)
			{
				effectComponent.effectData = data.Clone();
				pooledEffect.Reset(true);
				pooledEffect.gameObject.SetActive(true);
			}

			return pooledEffect;
		}
	}
}