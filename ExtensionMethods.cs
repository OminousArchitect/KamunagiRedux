using HarmonyLib;
using Newtonsoft.Json.Utilities;
using R2API;
using Rewired.UI.ControlMapper;
using RoR2;
using RoR2.Navigation;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KamunagiOfChains
{
	public static class ExtensionMethods
	{
		private static List<Material> _cachedMaterials = new List<Material>();
		private static Shader hgStandard = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/HGStandard");

		public static Material SetHopooMaterial(this Material tempMat)
		{
			if (_cachedMaterials.Contains(tempMat))
				return tempMat;

			float? bumpScale = null;
			Color? emissionColor = null;

			//grab values before the shader changes
			if (tempMat.IsKeywordEnabled("_NORMALMAP"))
			{
				bumpScale = tempMat.GetFloat("_BumpScale");
			}

			if (tempMat.IsKeywordEnabled("_EMISSION"))
			{
				emissionColor = tempMat.GetColor("_EmissionColor");
			}

			//set shader
			tempMat.shader = hgStandard;

			//apply values after shader is set
			tempMat.SetColor("_Color", tempMat.GetColor("_Color"));
			tempMat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
			tempMat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
			tempMat.EnableKeyword("DITHER");

			if (bumpScale != null)
			{
				tempMat.SetFloat("_NormalStrength", (float)bumpScale);
			}

			if (emissionColor != null)
			{
				tempMat.SetColor("_EmColor", (Color)emissionColor);
				tempMat.SetFloat("_EmPower", 1);
			}

			//set this keyword in unity if you want your model to show backfaces
			//in unity, right click the inspector tab and choose Debug
			if (tempMat.IsKeywordEnabled("NOCULL"))
			{
				tempMat.SetInt("_Cull", 0);
			}

			//set this keyword in unity if you've set up your model for limb removal item displays (eg. goat hoof) by setting your model's vertex colors
			if (tempMat.IsKeywordEnabled("LIMBREMOVAL"))
			{
				tempMat.SetInt("_LimbRemovalOn", 1);
			}

			_cachedMaterials.Add(tempMat);
			return tempMat;
		}

		public static void Deconstruct<T>(this T[] array, out T first, out T[] rest)
		{
			first = array[0];
			rest = array[1..];
		}

		public static Task<T> LoadAssetAsyncTask<T>(this AssetBundle bundle, string path) where T : UnityEngine.Object
		{
			var source = new TaskCompletionSource<T>();
			var handle = bundle.LoadAssetAsync<T>(path);
			handle.completed += _ =>
			{
				source.SetResult((T)handle.asset);
			};
			return source.Task;
		}

		public static DamageTypeCombo AddModdedDamageType(ref this DamageTypeCombo combo, DamageAPI.ModdedDamageType moddedType)
		{
			DamageAPI.AddModdedDamageType(ref combo, moddedType);
			return combo;
		}

		public static T WaitForCompletion<T>(this Task<T> task)
		{
			task.Wait();
			return task.Result;
		}

		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
		{
			var result = gameObject.GetComponent<T>();
			if (!result) result = gameObject.AddComponent<T>();
			return result;
		}

		public static V GetOrSet<T, V>(this Dictionary<T, V> dict, T key, Func<V> valueGetter)
		{
			if (dict.TryGetValue(key, out var value)) return value;
			value = valueGetter();
			dict[key] = value;
			return value;
		}
		
		public static EffectComponent EffectWithSound(this GameObject gameObject, string soundName)
		{
			var comp = gameObject.GetComponent<EffectComponent>();
			if (!comp)
			{
				comp = gameObject.AddComponent<EffectComponent>();
				comp.parentToReferencedTransform = true;
				comp.positionAtReferencedTransform = true;
			}

			comp.soundName = soundName;
			return comp;
		}
		
		public static Vector3 FindNearestNodePosition(Vector3 targetPosition, MapNodeGroup.GraphType nodeGraphType)
		{
			SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
			spawnCard.hullSize = HullClassification.Human;
			spawnCard.nodeGraphType = nodeGraphType;
			spawnCard.prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/DirectorSpawnProbeHelperPrefab.prefab").WaitForCompletion();
			Vector3 result = targetPosition;
			GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
				position = targetPosition
			}, RoR2Application.rng));
			if (gameObject)
			{
				result = gameObject.transform.position;
			}
			if (gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			UnityEngine.Object.Destroy(spawnCard);
			return result;
		}

		public static void SetChild(this ChildLocator locator, string key, Transform transform)
		{
			var index = locator.transformPairs.IndexOf(x => x.name == key);
			if (index < 0)
			{
				locator.transformPairs = locator.transformPairs.AddItem(new ChildLocator.NameTransformPair
				{
					name = key, transform = transform
				}).ToArray();
			}
			else
			{
				locator.transformPairs[index].transform = transform;
			}
		}
	}
}