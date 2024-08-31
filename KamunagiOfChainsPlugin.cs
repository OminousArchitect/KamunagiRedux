using System;
using BepInEx;
using HarmonyLib;
using R2API;
using R2API.Utils;
using UnityEngine;

namespace KamunagiOfChains
{
    [BepInDependency(DotAPI.PluginGUID), BepInDependency(DamageAPI.PluginGUID), BepInDependency(ColorsAPI.PluginGUID), BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [NetworkCompatibility]
    [BepInPlugin(Guid, Name, Version)]
    [HarmonyPatch]
    public class KamunagiOfChainsPlugin : BaseUnityPlugin
    {
        public const string AssetBundleName = "kamunagiassets";
        public const string SoundBankName = "KamunagiMusic.bnk";
        public static AssetBundle? Bundle;

        public const string Guid = "com.Nines.Kamunagi";
        public const string Name = "KamunagiOfChains";
        public const string Version = "1.0.0";

        public void Awake()
        {
            // Hook all the harmony attributes
            new Harmony(Info.Metadata.GUID).PatchAll();

            // Get the path of the dll
            var pluginPath = System.IO.Path.GetDirectoryName(Info.Location);
            if (pluginPath is null) return;

            // Load Assets
            Bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(pluginPath, AssetBundleName));
            
            // Load Soundbank
            if (Application.isBatchMode) return;
            try
            {
                AkSoundEngine.AddBasePath(pluginPath);
                var result = AkSoundEngine.LoadBank(SoundBankName, out _);
                if (result != AKRESULT.AK_Success)
                    Logger.LogError("SoundBank Load Failed: " + result);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            
            Assets.RegisterContent();
        }
    }
}