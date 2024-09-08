using KamunagiOfChains.Data.Bodies.Kamunagi.Extra;
using RoR2;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi
{
	public class TwinBehaviour : MonoBehaviour
	{
		private bool _muzzleToggle;
		public GameObject activeBuffWard;
		public bool componentAddedToMaster;
		public CharacterBody body;

		public string twinMuzzle
		{
			get
			{
				_muzzleToggle = !_muzzleToggle;
				return _muzzleToggle ? "MuzzleRight" : "MuzzleLeft";
			}
		}

		public void Awake()
		{
			body = GetComponent<CharacterBody>();
		}

		public void FixedUpdate()
		{
			if (!componentAddedToMaster && body.masterObject)
			{
				body.masterObject.GetOrAddComponent<MasterTwinBehaviour>();
				componentAddedToMaster = true;
			}
		}
	}

	public class MasterTwinBehaviour : MonoBehaviour
	{
		public CharacterMaster master;

		public void Awake()
		{
			master = GetComponent<CharacterMaster>();
			master.onBodyDeath.AddListener(OnDeath);
		}

		public void OnDeath()
		{
			if (master.GetBody().HasBuff(Asset.GetAsset<MashiroBlessing>()))
			{
				master.preventGameOver = true;
				Invoke(nameof(MashiroRebirth), 1.5f);
			}
		}

		public void MashiroRebirth()
		{
			master.preventGameOver = false;
			var positionAtDeath = master.deathFootPosition;
			if (master.killedByUnsafeArea)
			{
				positionAtDeath =
					TeleportHelper.FindSafeTeleportDestination(positionAtDeath, master.GetBody(),
						RoR2Application.rng) ?? master.deathFootPosition;
			}

			var body = master.Respawn(positionAtDeath, Quaternion.identity);
			body.AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
			var rezEffect = Asset.GetGameObject<MashiroBlessingRespawn, IEffect>();
			if (!master.bodyInstanceObject) return;
			var array = master.bodyInstanceObject.GetComponents<EntityStateMachine>();
			foreach (var esm in array)
			{
				esm.initialStateType = esm.mainStateType;
			}

			if (!rezEffect) return;
			EffectManager.SpawnEffect(rezEffect,
				new EffectData
				{
					origin = positionAtDeath, rotation = master.bodyInstanceObject.transform.rotation
				}, transmit: true);
		}
	}
}