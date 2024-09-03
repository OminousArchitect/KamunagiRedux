using System;
using System.Collections.Generic;
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
        
            var items = new List<ItemDef>();
            var bodies = new List<GameObject>();
            var survivors = new List<SurvivorDef>();
            var unlockables = new List<UnlockableDef>();
            
            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
            {
                if (!typeof(Asset).IsAssignableFrom(type) || type.IsAbstract) continue;
                var instance = (Asset) Activator.CreateInstance(type);
                Assets[type] = instance;
                if (instance is IUnlockable unlockable)
                {
                    var obj = unlockable.BuildObject();
                    Objects[type.Name + "_" + nameof(IUnlockable)] = obj;
                    unlockables.Add(obj);
                }
                if (instance is IItem item)
                {
                    var obj = item.BuildObject();
                    Objects[type.Name + "_" + nameof(IItem)] = obj;
                    items.Add(obj);
                }
                if (instance is IModel model)
                {
                    var obj = model.BuildObject();
                    Objects[type.Name + "_" + nameof(IModel)] = obj;
                }
                if (instance is IBodyDisplay display)
                {
                    var obj = display.BuildObject();
                    Objects[type.Name + "_" + nameof(IBodyDisplay)] = obj;
                }
                if (instance is IBody body)
                {
                    var obj = body.BuildObject();
                    Objects[type.Name + "_" + nameof(IBody)] = obj;
                    bodies.Add(obj);
                }
                if (instance is ISurvivor survivor)
                {
                    var name = type.Name;
                    var obj = survivor.BuildObject();
                    obj.cachedName = name + nameof(SurvivorDef);
                    Objects[name + "_" + nameof(ISurvivor)] = obj;
                    survivors.Add(obj);
                }
            }

            result.itemDefs.Add(items.ToArray());
            result.bodyPrefabs.Add(bodies.ToArray());
            result.survivorDefs.Add(survivors.ToArray());
            result.unlockableDefs.Add(unlockables.ToArray());
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