using System;
using System.Collections;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using ExtraSkillSlots;
using HarmonyLib;
using KamunagiOfChains.Data;
using R2API;
using R2API.Utils;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]

namespace KamunagiOfChains
{
    //[BepInDependency(DotAPI.PluginGUID), BepInDependency(DamageAPI.PluginGUID), BepInDependency(ColorsAPI.PluginGUID), BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(ExtraSkillSlotsPlugin.GUID)]
    [NetworkCompatibility]
    [BepInPlugin(Guid, Name, Version)]
    [HarmonyPatch]
    public class KamunagiOfChainsPlugin : BaseUnityPlugin
    {
        public const string AssetBundleName = "kamunagiassets";
        public const string SoundBankName = "KamunagiMusic.bnk";
        public static AssetBundle? Bundle;
        public static string PluginPath;
        public static KamunagiOfChainsPlugin Instance;

        public static ManualLogSource Log;
        public static bool soundBankQueued;

        public const string Guid = "com.Nines.Kamunagi";
        public const string Name = "KamunagiOfChains";
        public const string Version = "1.0.0";

        public void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogDebug("Harmony Patching");
            // Hook all the harmony attributes
            new Harmony(Info.Metadata.GUID).PatchAll();

            Log.LogDebug("Getting Plugin Path");
            // Get the path of the dll
            PluginPath = System.IO.Path.GetDirectoryName(Info.Location) ?? throw new InvalidOperationException("Failed to find path of plugin.");

            Log.LogDebug("Loading Asset Bundle");
            // Load Assets
            AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(PluginPath, AssetBundleName)).completed += operation =>
            {
                Log.LogDebug("Bundle Loaded");
                Bundle = (operation as AssetBundleCreateRequest)?.assetBundle;
                
                Log.LogDebug("Loading ContentPack");
                ContentPackProvider.Initialize(Info.Metadata.GUID, Asset.BuildContentPack());
            };

            Log.LogDebug("Finished Awake");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RoR2.WwiseUtils.SoundbankLoader), nameof(RoR2.WwiseUtils.SoundbankLoader.Start))]
        public static void AddSoundbankToLoader(RoR2.WwiseUtils.SoundbankLoader __instance)
        {
            // Ensure the soundbank isn't added to each loader, but only one.
            if (soundBankQueued) return;
            Log.LogDebug("Soundbank Added To Queue");
            AkSoundEngine.AddBasePath(PluginPath);
            __instance.soundbankStrings = __instance.soundbankStrings
                .AddItem(SoundBankName).ToArray();
            soundBankQueued = true;
        }
        
        private class ContentPackProvider : RoR2.ContentManagement.IContentPackProvider
        {
            private static RoR2.ContentManagement.ContentPack _contentPack = null!;
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
                RoR2.ContentManagement.ContentPack.Copy(_contentPack, args.output);
                //Log.LogError(ContentPack.identifier);
                args.ReportProgress(1f);
                yield break;
            }

            public IEnumerator FinalizeAsync(RoR2.ContentManagement.FinalizeAsyncArgs args)
            {
                args.ReportProgress(1f);
                Log.LogInfo("Contentpack finished");
                yield break;
            }

            internal static void Initialize(string identifier, RoR2.ContentManagement.ContentPack pack)
            {
                _identifier = identifier;
                _contentPack = pack;
                RoR2.ContentManagement.ContentManager.collectContentPackProviders += provider => provider(new ContentPackProvider());
            }
        }
    }
}