using HarmonyLib;
using RoR2;
using UnityEngine;
using UnityEngine.Rendering;

namespace KamunagiOfChains
{
	[HarmonyPatch]
	public class ModelAttachedEffect : MonoBehaviour
	{

		[HarmonyPostfix, HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.UpdateMaterials))]
		public static void UpdateModelAttachedEffects(CharacterModel __instance)
		{
			foreach (var effect in __instance.GetComponentsInChildren<ModelAttachedEffect>())
			{
				effect.UpdateRenderers(__instance);
			}
		}
		
		private (Renderer renderer, RendererInfo info)[] defaults;

		private void Start()
		{
			var renderers = GetComponentsInChildren<Renderer>(true);
			var defaultValues = renderers.Select(x => new RendererInfo(x));
			defaults = renderers.Zip(defaultValues, (renderer, info) => (renderer, info)).ToArray();
		}
		
		private struct RendererInfo
		{
			public readonly ShadowCastingMode shadowcastingMode;
			public readonly bool wasEnabled;

			public RendererInfo(Renderer renderer)
			{
				wasEnabled = renderer.enabled;
				shadowcastingMode = renderer.shadowCastingMode;
			}
		}

		public void UpdateRenderers(CharacterModel characterModel)
		{
			var invisible = characterModel.visibility == VisibilityLevel.Invisible;
			foreach (var (renderer, defaults) in defaults)
			{
				if (invisible)
				{
					renderer.shadowCastingMode = ShadowCastingMode.Off;
					renderer.enabled = false;
				}
				else
				{
					renderer.shadowCastingMode = defaults.shadowcastingMode;
					renderer.enabled = defaults.wasEnabled;
				}
			}
		}
	}
}