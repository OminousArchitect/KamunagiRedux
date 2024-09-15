using R2API;
using RoR2;
using UnityEngine;

namespace KamunagiOfChains.Data
{
	public class BossMusic : Asset, IMusicTrack //
	{
		MusicTrackDef IMusicTrack.BuildObject()
		{
			var customMusicData = new SoundAPI.Music.CustomMusicData();
			customMusicData.BanksFolderPath = pluginPath;
			customMusicData.BepInPlugin = instance.Info.Metadata;
			customMusicData.InitBankName = "KamunagiMusic";
			customMusicData.PlayMusicSystemEventName = "KamunagiPlayMusic";
			customMusicData.SoundBankName = "KamunagiMusic";

			customMusicData.SceneDefToTracks = new Dictionary<SceneDef, IEnumerable<SoundAPI.Music.MainAndBossTracks>>();
			
			var musicTrackDef = ScriptableObject.CreateInstance<SoundAPI.Music.CustomMusicTrackDef>();
			musicTrackDef.cachedName = "kamunagiCustomMusic";
			musicTrackDef.SoundBankName = customMusicData.SoundBankName;
			musicTrackDef.CustomStates = new List<SoundAPI.Music.CustomMusicTrackDef.CustomState>();
			var myCustomState = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
			myCustomState.GroupId = 1378295094u; // The Kamunagi State Group ID
			myCustomState.StateId = 1560169506u; // The Kamunagi State ID
			musicTrackDef.CustomStates.Add(myCustomState);
			var hopooMusicState = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
			hopooMusicState.GroupId = 792781730U; // gathered from the Init bank txt file. Vanilla Group ID Music_system
			hopooMusicState.StateId = 2607556080; // gathered from the Init bank txt file. Denote where the track will be played, in this case it uses the vanilla state Bossfight
			musicTrackDef.CustomStates.Add(hopooMusicState);
			
			var sceneDef = LoadAsset<SceneDef>("RoR2/DLC1/ancientloft/ancientloft.asset");
			customMusicData.SceneDefToTracks.Add(sceneDef, new List<SoundAPI.Music.MainAndBossTracks>() { new SoundAPI.Music.MainAndBossTracks(null, musicTrackDef) });
			return musicTrackDef;
		}
	}
}