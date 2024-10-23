using AK.Wwise;
using HarmonyLib;
using RoR2;
using RoR2.WwiseUtils;
using System.Diagnostics;
using UnityEngine;

namespace KamunagiOfChains.Data
{
	[HarmonyPatch]
	public class BossMusic : Asset, IMusicTrack //
	{
		Task<MusicTrackDef> IMusicTrack.BuildObject()
		{
			var musicTrackDef = ScriptableObject.CreateInstance<MusicTrackDef>();
			var group = ScriptableObject.CreateInstance<WwiseStateGroupReference>();
			group.id = 1533728782;
			var state = ScriptableObject.CreateInstance<WwiseStateReference>();
			state.id = 1852808225;
			state.GroupObjectReference = group;

			
			musicTrackDef.states = new[]
			{
				new State { WwiseObjectReference = state }
			};
			musicTrackDef.cachedName = "kamunagiCustomMusic";
			return Task.FromResult(musicTrackDef);
		}

		public override Task Initialize()
		{
			// golem plains,  sundered grove, sirens call

			MusicController.pickTrackHook += PickTrack;
			return base.Initialize();
		}

		private void PickTrack(MusicController musicController, ref MusicTrackDef newTrack)
		{
			if (!musicController.enableMusicSystem) return;
			var isBossMusic = TeleporterInteraction.instance && !TeleporterInteraction.instance.isIdle;
			if (SceneCatalog.mostRecentSceneDef == null) return;
			var currentScene = SceneCatalog.mostRecentSceneDef.baseSceneName;
			if (isBossMusic && (currentScene == "golemplains" || currentScene == "shipgraveyard" || currentScene == "rootjungle"))
			{
				newTrack = this.GetMusicTrackDef().WaitForCompletion();
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(MusicController), nameof(MusicController.StartIntroMusic))]
		// ReSharper disable once InconsistentNaming
		public static void PlayMusic(MusicController __instance)
		{
			AkSoundEngine.PostEvent("Play_KamunagiBossMusic", __instance.gameObject);
		}
		
		[HarmonyPrefix, HarmonyPatch(typeof(StateSetter), nameof(StateSetter.FlushIfChanged))]
		// ReSharper disable once InconsistentNaming
		public static void StateSwap(StateSetter __instance)
		{
			// bossStatus ID
			if (__instance.id != 549431000 || __instance.expectedEngineValueId.Equals(__instance.valueId)) return;
			AkSoundEngine.SetState(GetMusicTrackDef<BossMusic>().WaitForCompletion().states[0].GroupId, 0); // could also be a seperate state for exiting the track
		}
	}
}