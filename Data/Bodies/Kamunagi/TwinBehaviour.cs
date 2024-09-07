using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi
{
    public class TwinBehaviour : MonoBehaviour
    {
        private bool _muzzleToggle;
        public GameObject activeBuffWard;

        public string twinMuzzle
        {
            get
            {
                _muzzleToggle = !_muzzleToggle;
                return _muzzleToggle ? "MuzzleRight" : "MuzzleLeft";
            }
        }
    }
}