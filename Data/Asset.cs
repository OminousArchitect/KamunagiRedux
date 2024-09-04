using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EntityStates;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable SuspiciousTypeConversion.Global

namespace KamunagiOfChains.Data
{
    public abstract class Asset
    {
        public static Dictionary<string, object> Objects = new Dictionary<string, object>();
        public static Dictionary<Type, Asset> Assets = new Dictionary<Type, Asset>();

        public static ContentPack BuildContentPack()
        {
            var result = new ContentPack();
            
            var assets = Assembly.GetCallingAssembly().GetTypes()
                .Where(x => typeof(Asset).IsAssignableFrom(x) && !x.IsAbstract);
            Assets = assets.ToDictionary(x => x, x => (Asset)Activator.CreateInstance(x));

            var instances = Assets.Values;
            var entityStates = instances.Where(x => x is IEntityStates).SelectMany(x =>
                (Type[])(Objects.GetOrSet(x.GetType().Name + "_EntityStates", () => ((IEntityStates)x).GetEntityStates())));
            
            result.unlockableDefs.Add(instances.Where(x => x is IUnlockable).Select(x => (UnlockableDef)x).ToArray());
            result.itemDefs.Add(instances.Where(x => x is IItem).Select(x => (ItemDef)x).ToArray());
            result.skillDefs.Add(instances.Where(x => x is ISkillDef).Select(x => (SkillDef)x).ToArray());
            result.entityStateTypes.Add(instances.Where(x => x is ISkillDef)
                .SelectMany(x => (Type[])Objects[x.GetType().Name + "_" + nameof(ISkillDef) + "_EntityStates"])
                .Concat(entityStates).Distinct().ToArray());
            result.skillFamilies.Add(instances.Where(x => x is ISkillFamily).Select(x => (SkillFamily)x).ToArray());
            result.networkedObjectPrefabs.Add(instances.Where(x => x is INetworkedObject)
                .Select(x => (GameObject)GetObjectOrThrow<INetworkedObject>(x))
                .ToArray());
            result.bodyPrefabs.Add(instances.Where(x => x is IBody).Select(x => (GameObject)GetObjectOrThrow<IBody>(x))
                .ToArray());
            result.survivorDefs.Add(instances.Where(x => x is ISurvivor).Select(x => (SurvivorDef)x).ToArray());
            result.projectilePrefabs.Add(instances.Where(x => x is IProjectile)
                .Select(x => (GameObject)GetObjectOrThrow<IProjectile>(x)).ToArray());
            result.effectDefs.Add(instances.Where(x => x is IEffect)
                .Select(x => new EffectDef((GameObject)GetObjectOrThrow<IEffect>(x))).ToArray());

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
                return !KamunagiOfChainsPlugin.bundle
                    ? null
                    : KamunagiOfChainsPlugin.bundle!.LoadAsset<T>(assetPath["bundle:".Length..]);
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
            try
            {
                asset = (GameObject)GetObjectOrThrow<T2>(Assets[typeof(T)]);
                return true;
            }
            catch (Exception)
            {
                asset = default!;
                return false;
            }
        }

        private static object GetObjectOrThrow<T>(Asset asset)
        {
            var name = asset.GetType().Name;
            var targetType = typeof(T);
            var targetTypeName = targetType.Name;
            var key = name + "_" + targetTypeName;
            var notOfType = new AssetTypeInvalidException($"{name} is not of type {targetTypeName}");
            if (Objects.TryGetValue(key, out var result))
            {
                return result!;
            }

            object? returnedObject = null;
            switch (targetTypeName)
            {
                case nameof(IUnlockable):
                    var unlockable = (asset as IUnlockable)?.BuildObject() ?? throw notOfType;
                    unlockable.cachedName = name + nameof(UnlockableDef);
                    returnedObject = unlockable;
                    break;
                case nameof(IItem):
                    var item = (asset as IItem)?.BuildObject() ?? throw notOfType;
                    item.name = name + nameof(ItemDef);
                    returnedObject = item;
                    break;
                case nameof(ISkillDef):
                    var skill = (asset as ISkillDef)?.BuildObject() ?? throw notOfType;
                    var entityStates = ((ISkillDef)asset).GetEntityStates();
                    Objects[key + "_EntityStates"] = entityStates;
                    skill.skillName = name + nameof(SkillDef);
                    skill.activationState = new SerializableEntityStateType(entityStates[0]);
                    returnedObject = skill;
                    break;
                case nameof(ISurvivor):
                    var survivor = (asset as ISurvivor)?.BuildObject() ?? throw notOfType;
                    survivor.cachedName = name + nameof(SurvivorDef);
                    returnedObject = survivor;
                    break;
                case nameof(IEffect):
                    var effect = (asset as IEffect)?.BuildObject() ?? throw notOfType;
                    if (!effect.GetComponent<VFXAttributes>())
                    {
                        var attributes = effect.AddComponent<VFXAttributes>();
                        attributes.vfxPriority = VFXAttributes.VFXPriority.Always;
                        attributes.DoNotPool = true;
                    }
                    if (!effect.GetComponent<EffectComponent>())
                    {
                        var comp = effect.AddComponent<EffectComponent>();
                        comp.applyScale = false;
                        comp.parentToReferencedTransform = true;
                        comp.positionAtReferencedTransform = true;
                    }
                    returnedObject = effect;
                    break;
                case nameof(IVariant):
                    var variant = (asset as IVariant)?.BuildObject();
                    if (variant is null)
                    {
                        var skillDef = (SkillDef) asset;
                        variant = new SkillFamily.Variant
                        {
                            skillDef = skillDef,
                            viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false)
                        };
                    }
                    returnedObject = variant;
                    break;
                default:
                    returnedObject = targetType.GetMethod("BuildObject")?.Invoke(asset, null) ?? throw notOfType;
                    break;
                /*
                case nameof(ISkillFamily):
                    var skillFamily = (asset as ISkillFamily)?.BuildObject() ?? throw notOfType;
                    returnedObject = skillFamily;
                    break;
                case nameof(IVariant):
                    var variant = (asset as IVariant)?.BuildObject() ?? throw notOfType;
                    returnedObject = variant;
                    break;
                case nameof(INetworkedObject):
                    var networkedObject = (asset as INetworkedObject)?.BuildObject() ?? throw notOfType;
                    returnedObject = networkedObject;
                    break;
                case nameof(IProjectile):
                    var projectile = (asset as IProjectile)?.BuildObject() ?? throw notOfType;
                    returnedObject = projectile;
                    break;
                case nameof(IProjectileGhost):
                    var ghost = (asset as IProjectileGhost)?.BuildObject() ?? throw notOfType;
                    returnedObject = ghost;
                    break;
                case nameof(IModel):
                    var model = (asset as IModel)?.BuildObject() ?? throw notOfType;
                    returnedObject = model;
                    break;
                case nameof(IBodyDisplay):
                    var display = (asset as IBodyDisplay)?.BuildObject() ?? throw notOfType;
                    returnedObject = display;
                    break;
                case nameof(IBody):
                    var body = (asset as IBody)?.BuildObject() ?? throw notOfType;
                    returnedObject = body;
                    break;
                    */
            }

            Objects[key] = returnedObject ??
                           throw new InvalidOperationException(
                               $"How did you get here, maybe {targetTypeName} isn't a asset?");
            return returnedObject;
        }

        public static implicit operator ItemDef(Asset asset) => (ItemDef)GetObjectOrThrow<IItem>(asset);

        public static implicit operator UnlockableDef(Asset asset) =>
            (UnlockableDef)GetObjectOrThrow<IUnlockable>(asset);

        public static implicit operator SurvivorDef(Asset asset) => (SurvivorDef)GetObjectOrThrow<ISurvivor>(asset);

        public static implicit operator SkillDef(Asset asset) => (SkillDef)GetObjectOrThrow<ISkillDef>(asset);

        public static implicit operator SkillFamily(Asset asset) => (SkillFamily)GetObjectOrThrow<ISkillFamily>(asset);

        public static implicit operator SkillFamily.Variant(Asset asset) => (SkillFamily.Variant)GetObjectOrThrow<IVariant>(asset);
    }

    public class AssetTypeInvalidException : Exception
    {
        public AssetTypeInvalidException(string message) : base(message)
        {
        }
    }

    public interface IGameObject
    {
    }

    public interface INetworkedObject : IGameObject
    {
        public abstract GameObject BuildObject();
    }

    public interface IProjectile : IGameObject
    {
        public abstract GameObject BuildObject();
    }

    public interface IProjectileGhost : IGameObject
    {
        public abstract GameObject BuildObject();
    }

    public interface IEffect : IGameObject
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

    public interface IItem
    {
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

    public interface ISkillFamily
    {
        public abstract SkillFamily BuildObject();
    }

    public interface ISkillDef
    {
        public abstract SkillDef BuildObject();

        public abstract Type[] GetEntityStates();
    }

    public interface IEntityStates
    {
        public abstract Type[] GetEntityStates();
    }

    public interface IVariant
    {
        public abstract SkillFamily.Variant BuildObject();
    }
}