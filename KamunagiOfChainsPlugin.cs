global using static KamunagiOfChains.KamunagiOfChainsPlugin;
global using ConcentricContent;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using ExtraSkillSlots;
using HarmonyLib;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]

namespace KamunagiOfChains
{
	//[BepInDependency(DotAPI.PluginGUID), BepInDependency(DamageAPI.PluginGUID), BepInDependency(ColorsAPI.PluginGUID), BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
	[BepInDependency(RecalculateStatsAPI.PluginGUID)]
	[BepInDependency(PrefabAPI.PluginGUID)]
	[BepInDependency(ColorsAPI.PluginGUID)]
	[BepInDependency(DeployableAPI.PluginGUID)]
	[BepInDependency(DotAPI.PluginGUID)]
	[BepInDependency(DamageAPI.PluginGUID)]
	[BepInDependency(ExtraSkillSlotsPlugin.GUID)]
	[NetworkCompatibility]
	[BepInPlugin(Guid, Name, Version)]
	[HarmonyPatch]
	public class KamunagiOfChainsPlugin : BaseUnityPlugin
	{
		public const string AssetBundleName = "kamunagiassets";
		public const string SoundBankName = "KamunagiMusic.bnk";
		public static Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
		public static string? pluginPath;
		public static KamunagiOfChainsPlugin instance = null!;
		public static ManualLogSource log = null!;
		public static bool soundBankQueued;

		public const string Guid = "com.Nines.Kamunagi";
		public const string Name = "KamunagiOfChains";
		public const string Version = "1.0.0";

		public static BodyIndex vultureIndex;
		public static BodyIndex pestIndex;
		public static DamageAPI.ModdedDamageType Denebokshiri;
		public static DamageAPI.ModdedDamageType TwinsReaver;
		public static DamageAPI.ModdedDamageType Uitsalnemetia;
		public static DamageAPI.ModdedDamageType CurseFlames;

		public void Awake()
		{
			instance = this;
			log = Logger;
			log.LogDebug("Harmony Patching");
			// Hook all the harmony attributes
			new Harmony(Info.Metadata.GUID).PatchAll();

			log.LogDebug("Getting Plugin Path");
			// Get the path of the dll
			pluginPath = System.IO.Path.GetDirectoryName(Info.Location) ??
			             throw new InvalidOperationException("Failed to find path of plugin.");


			Language.collectLanguageRootFolders +=
				folders => folders.Add(System.IO.Path.Combine(pluginPath, "Language"));

			log.LogDebug("Caching BodyIndexes");
			RoR2Application.onLoad += () =>
			{
				vultureIndex = BodyCatalog.FindBodyIndex("VultureBody");
				pestIndex = BodyCatalog.FindBodyIndex(
					"FlyingVerminBody"); //cache these like KatarinaMod, because I need all flying enemies, and these guys have isFlying set to false
			};

			Denebokshiri = DamageAPI.ReserveDamageType();
			TwinsReaver = DamageAPI.ReserveDamageType();
			Uitsalnemetia = DamageAPI.ReserveDamageType();
			CurseFlames = DamageAPI.ReserveDamageType();


			log.LogDebug("Loading Concentric Bundle");

			var assetsPath = System.IO.Path.Join(pluginPath, "Assets");
			var bundlePaths = Directory.EnumerateFiles(assetsPath).Where(x => !x.EndsWith("manifest")).ToArray();
			foreach (var path in bundlePaths)
			{
				AssetBundle.LoadFromFileAsync(path).completed += operation =>
				{
					var name = System.IO.Path.GetFileName(path);
					log.LogDebug(name + " Bundle Loaded");
					bundles[name] = ((AssetBundleCreateRequest)operation).assetBundle;
					if (bundles.Count != bundlePaths.Length) return;
					log.LogDebug("Loading ContentPack");
					ContentPackProvider.Initialize(Info.Metadata.GUID,
						Concentric.BuildContentPack(Assembly.GetExecutingAssembly()));
				};
			}

			log.LogDebug("Finished Awake");
		}

		public static Task<T> LoadAsset<T>(string assetPath) where T : UnityEngine.Object
		{
			if (assetPath.StartsWith("addressable:"))
			{
				return Addressables.LoadAssetAsync<T>(assetPath["addressable:".Length..]).Task;
			}

			if (assetPath.StartsWith("legacy:"))
			{
				return LegacyResourcesAPI.LoadAsync<T>(assetPath["legacy:".Length..]).Task;
			}

			var colinIndex = assetPath.IndexOf(":", StringComparison.Ordinal);
			if (colinIndex <= 0) return Addressables.LoadAssetAsync<T>(assetPath).Task;

			var source = new TaskCompletionSource<T>();
			var handle = bundles[assetPath[..colinIndex]].LoadAssetAsync<T>(assetPath[(colinIndex + 1)..]);
			handle.completed += _ => source.SetResult((T)handle.asset);
			return source.Task;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RoR2.WwiseUtils.SoundbankLoader), nameof(RoR2.WwiseUtils.SoundbankLoader.Start))]
		public static void AddSoundbankToLoader(RoR2.WwiseUtils.SoundbankLoader __instance)
		{
			// Ensure the soundbank isn't added to each loader, but only one.
			if (soundBankQueued) return;
			log.LogDebug("Soundbank Added To Queue");
			AkSoundEngine.AddBasePath(pluginPath);
			//AkSoundEngine.LoadBank(SoundBankName, out var bankID);
			__instance.soundbankStrings = __instance.soundbankStrings
				.AddItem(SoundBankName).ToArray();
			soundBankQueued = true;
		}

		private class ContentPackProvider : RoR2.ContentManagement.IContentPackProvider
		{
			private static Task<ContentPack> _contentPack = null!;
			private static string _identifier = null!;
			public string identifier => _identifier;

			public IEnumerator LoadStaticContentAsync(RoR2.ContentManagement.LoadStaticContentAsyncArgs args)
			{
				//ContentPack.identifier = identifier;
				args.ReportProgress(1f);
				yield break;
			}

			public IEnumerator GenerateContentPackAsync(RoR2.ContentManagement.GetContentPackAsyncArgs args)
			{
				while (!_contentPack.IsCompleted)
					yield return null;
				if (_contentPack.IsFaulted)
					throw _contentPack.Exception!;

				ContentPack.Copy(_contentPack.Result, args.output);
				//Log.LogError(ContentPack.identifier);
				args.ReportProgress(1f);
				yield break;
			}

			public IEnumerator FinalizeAsync(RoR2.ContentManagement.FinalizeAsyncArgs args)
			{
				args.ReportProgress(1f);
				log.LogInfo("Contentpack finished");
				yield break;
			}

			internal static void Initialize(string identifier, Task<ContentPack> pack)
			{
				_identifier = identifier;
				_contentPack = pack;
				ContentManager.collectContentPackProviders +=
					provider => provider(new ContentPackProvider());
			}
		}
	}
}