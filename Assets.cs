using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KamunagiOfChains
{
    public static class Assets {
        public static DeployableSlot WispSlot;
        public static DeployableSlot GupSlot;
        
        public static DamageColorIndex MashiroPrayerIndex;
        
        public static ItemDef CustomGhostItem;
        
        public static Material woshisGhostOverlay;

        public static void RegisterContent()
        {
            WispSlot = DeployableAPI.RegisterDeployableSlot((master,count) => 4);
            GupSlot = DeployableAPI.RegisterDeployableSlot((master, count) => 1);
            
            MashiroPrayerIndex = ColorsAPI.RegisterDamageColor(new Color(0.98f, 1, 0.58f));
            
            CustomGhostItem = BuildGhostItem();
            ItemAPI.Add(new CustomItem(CustomGhostItem, null as ItemDisplayRule[]));
            
            woshisGhostOverlay = new Material(Load<Material>("RoR2/Base/Common/VFX/matGhostEffect.mat"));
            woshisGhostOverlay.SetTexture("_RemapTex", KamunagiOfChainsPlugin.Bundle!.LoadAsset<Texture2D>("texRampWoshis"));
        }
        
        internal static T Load<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }
        
        public static ItemDef BuildGhostItem()
        {
            var customGhostItem = ScriptableObject.CreateInstance<ItemDef>();
            customGhostItem.name = "WOSHISGHOST_NAME";
            customGhostItem.nameToken = "WOSHISGHOST_NAME";
            customGhostItem.pickupToken = "WOSHISGHOST_PICKUP";
            customGhostItem.descriptionToken = "WOSHISGHOST_DESC";
            customGhostItem.loreToken = "WOSHISGHOST_LORE";
            customGhostItem.tier = ItemTier.NoTier;
            customGhostItem.pickupIconSprite = Load<Sprite>("RoR2/Base/Beetle/texBuffBeetleJuiceIcon.tif");
            customGhostItem.pickupModelPrefab = Load<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab");
            customGhostItem.canRemove = false;
            customGhostItem.hidden = true;
            return customGhostItem;
        }
    }
}