using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
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
    }
}