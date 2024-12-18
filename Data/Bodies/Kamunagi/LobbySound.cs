using UnityEngine;

namespace KamunagiOfChains.Data.Bodies
{
	public class LobbySound : MonoBehaviour
	{
		private void OnEnable()
		{
			//AkSoundEngine.PostEvent(2023966543, base.gameObject);
		}

		private void OnDisable()
		{
			AkSoundEngine.StopPlayingID(2023966543);	
		}
	}
}