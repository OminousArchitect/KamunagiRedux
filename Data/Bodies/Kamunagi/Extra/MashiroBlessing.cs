using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class MashiroBlessing : Asset, IEffect
	{
		GameObject IEffect.BuildObject()
		{
			var effect = LoadAsset<GameObject>("bundle:ShadowFlame.prefab")!;
			effect.transform.localPosition = Vector3.zero;
			effect.transform.localScale = Vector3.one * 0.6f;
			return effect;
		}
	}
}