using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KamunagiOfChains.Data {
    public abstract class Asset {       
        public static Dictionary<string, object> Objects = new Dictionary<string, object>();
        public static Dictionary<Type, Asset> Assets = new Dictionary<Type, Asset>();

        public static ContentPack BuildContentPack()
        {
            var result = new ContentPack();
            
            var iItems = new List<IItem>();
            var iUnlockables = new List<IUnlockable>();
            var iNetworkedObjects = new List<INetworkedObject>();
            var iBodies = new List<IBody>();
            var iBodyDisplays = new List<IBodyDisplay>();
            var iModels = new List<IModel>();
            var iSurvivors = new List<ISurvivor>();
            

            
            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
            {
                if (!typeof(Asset).IsAssignableFrom(type) || type.IsAbstract) continue;
                var instance = (Asset) Activator.CreateInstance(type);
                Assets[type] = instance;
                if (instance is IUnlockable unlockable)
                {
                    iUnlockables.Add(unlockable);
                }
                if (instance is IItem item)
                {
                   iItems.Add(item);
                }
                if (instance is INetworkedObject networkedObject)
                {
                    iNetworkedObjects.Add(networkedObject);
                }
                if (instance is IModel model)
                {
                    iModels.Add(model);
                }
                if (instance is IBodyDisplay display)
                {
                    iBodyDisplays.Add(display);
                }
                if (instance is IBody body)
                {
                    iBodies.Add(body);
                }
                if (instance is ISurvivor survivor)
                {
                    iSurvivors.Add(survivor);
                }
            }

            // The order of this is important, so other objects can reference existing objects when being built.
            foreach (var model in iModels)
            {
                var obj = model.BuildObject();
                Objects[model.GetType().Name + "_" + nameof(IModel)] = obj;
            }
            foreach (var display in iBodyDisplays)
            {
                var obj = display.BuildObject();
                Objects[display.GetType().Name + "_" + nameof(IBodyDisplay)] = obj;
            }

            result.unlockableDefs.Add(iUnlockables.Select(x =>
            {
                var obj = x.BuildObject();
                Objects[x.GetType().Name + "_" + nameof(IUnlockable)] = obj;
                return obj;
            }).ToArray());
            result.itemDefs.Add(iItems.Select(x =>
            {
                var obj = x.BuildObject();
                Objects[x.GetType().Name + "_" + nameof(IItem)] = obj;
                return obj;
            }).ToArray());
            result.networkedObjectPrefabs.Add(iNetworkedObjects.Select(x =>
            {
                var obj = x.BuildObject();
                Objects[x.GetType().Name + "_" + nameof(INetworkedObject)] = obj;
                return obj;
            }).ToArray());
            result.bodyPrefabs.Add(iBodies.Select(x =>
            {
                var obj = x.BuildObject();
                Objects[x.GetType().Name + "_" + nameof(IBody)] = obj;
                return obj;
            }).ToArray());
            result.survivorDefs.Add(iSurvivors.Select(x =>
            {
                var name = x.GetType().Name;
                var obj = x.BuildObject();
                obj.cachedName = name + nameof(SurvivorDef);
                Objects[name + "_" + nameof(ISurvivor)] = obj;
                return obj;
            }).ToArray());
            return result;
        }
        
        public static T? LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            if (assetPath.StartsWith("addressable:"))
            {
                return Addressables.LoadAssetAsync<T>(assetPath["addressable:".Length..]).WaitForCompletion();
            }

            if (assetPath.StartsWith("bundle:"))
            {
                return !KamunagiOfChainsPlugin.Bundle ? null : KamunagiOfChainsPlugin.Bundle!.LoadAsset<T>(assetPath["bundle:".Length..]);
            }
            
            if (assetPath.StartsWith("legacy:"))
            {
                return LegacyResourcesAPI.Load<T>(assetPath["legacy:".Length..]);
            }

            return null;
        }

        public static bool TryGetAsset<T>(out T asset) where T : Asset
        {
            if (Assets.TryGetValue(typeof(T), out var foundAsset))
            {
                asset = (T)foundAsset;
                return true;
            }

            asset = default!;
            return false;
        }
        
        public static bool TryGetGameObject<T, T2>(out GameObject asset) where T2 : IGameObject
        {
            if (Objects.TryGetValue(typeof(T).Name + "_" + typeof(T2).Name, out var foundAsset))
            {
                asset = (GameObject) foundAsset;
                return true;
            }
            asset = default!;
            return false;
        }
        
        private static object GetObjectOrThrow<T>(Asset asset)
        {
            var name = asset.GetType().Name;
            var targetTypeName = typeof(T).Name;
            if (Objects.TryGetValue(name + "_" + targetTypeName, out var result))
            {
                return result!;
            }
            throw new AssetTypeInvalidException($"{name} is not of type {targetTypeName}");
        }
        
        public static implicit operator ItemDef(Asset asset) => (ItemDef) GetObjectOrThrow<IItem>(asset);
        public static implicit operator UnlockableDef(Asset asset) => (UnlockableDef) GetObjectOrThrow<IUnlockable>(asset);
        public static implicit operator SurvivorDef(Asset asset) => (SurvivorDef) GetObjectOrThrow<ISurvivor>(asset);
    }

    public class AssetTypeInvalidException : Exception
    {
        public AssetTypeInvalidException(string message): base(message) {}
    }

    public interface IGameObject {}
    public interface INetworkedObject : IGameObject
    {
        public abstract GameObject BuildObject();
    }
    public interface IBody : IGameObject
    {
        public abstract GameObject BuildObject();
    }
    public interface IBodyDisplay : IGameObject
    {
        public abstract GameObject BuildObject();
    }
    public interface IModel : IGameObject
    {
        public abstract GameObject BuildObject();
    }
    public interface ISurvivor
    {
        public abstract SurvivorDef BuildObject();
    }
    public interface IItem {
        public abstract string nameToken { get; }
        public virtual ItemDef BuildObject()
        {
            var item = ScriptableObject.CreateInstance<ItemDef>();
            item.nameToken = nameToken;
            return item;
        }
    }
    public interface IUnlockable
    {
        public abstract UnlockableDef BuildObject();
    }
}