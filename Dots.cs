using RoR2;
using R2API;
using static R2API.DotAPI;
using static RoR2.DotController;
using UnityEngine;

namespace Kamunagi.Modules
{
    public static class Dots
    {
        public static DotIndex KamunagiCurse;
        public static CustomDotBehaviour kamunagicurseBehaviour;
        public static CustomDotVisual kamunagicurseEffects;
        //public static KamunagiEffectController kamunagiEffectController;
        public static GameObject stupidEffect;
        public static DestroyOnDestroy destroyComponent;

        internal static void Initialize()
        {
            kamunagicurseEffects = UpdateCustomDotVisuals;
            kamunagicurseBehaviour = CalculateCurse;
            InitializeDotDefs();
        }

        internal static void UpdateCustomDotVisuals(DotController self)
        {
            ModelLocator modelLocator = self.victimObject.GetComponent<ModelLocator>();
            if (modelLocator && modelLocator.modelTransform) 
            {
                if (!self.GetComponent<KamunagiEffectController>())
                {
                    var kamunagiEffectController = self.gameObject.AddComponent<KamunagiEffectController>();
                    kamunagiEffectController.effectParams = KamunagiEffectController.defaultEffect;
                    kamunagiEffectController.target = modelLocator.modelTransform.gameObject;
                    Debug.LogWarning("added Kamunagi Controller");
                }
            }
        }
        internal static void CalculateCurse(DotController self, DotStack dotStack) //AddDot
        {
            if (dotStack.dotIndex == KamunagiCurse)
            {
                var pos = self.victimBody.corePosition;
                Debug.Log("A stack was added");
            }
        }

        internal static void InitializeDotDefs()
        {
            KamunagiCurse = DotAPI.RegisterDotDef(new DotController.DotDef
            {
                interval = 0.2f,
                damageCoefficient = 0.1f,
                damageColorIndex = DamageColorIndex.Void,
                associatedBuff = Buffs.KamunagiCurseDebuff
            }, kamunagicurseBehaviour, kamunagicurseEffects);
        }
    }
}