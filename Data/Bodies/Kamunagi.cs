using System;
using System.Collections.Generic;
using ExtraSkillSlots;
using KamunagiOfChains.Data.GameObjects;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies
{
    public class Kamunagi : Asset, IBody, IBodyDisplay, ISurvivor, IModel
    {
        GameObject IModel.BuildObject()
        {
            var model = LoadAsset<GameObject>("bundle:mdlKamunagi");
            var characterModel = model.GetOrAddComponent<CharacterModel>();
            var childLocator = model.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo RenderInfoFromChild(Component child, bool dontHopoo = false)
            {
                var renderer = child.GetComponent<Renderer>();
                return new CharacterModel.RendererInfo()
                {
                    renderer = renderer,
                    defaultMaterial = !dontHopoo ? renderer.material.SetHopooMaterial() : renderer.material,
                    ignoreOverlays = false,
                    defaultShadowCastingMode = ShadowCastingMode.On,
                };
            }

            var voidCrystalMat = LoadAsset<Material>("addressable:RoR2/DLC1/voidstage/matVoidCrystal.mat");
            characterModel.baseRendererInfos = new[]
            {
                new CharacterModel.RendererInfo
                {
                    renderer = childLocator.FindChild("S Cloth01").GetComponent<Renderer>(),
                    defaultMaterial = voidCrystalMat,
                    ignoreOverlays = false,
                    defaultShadowCastingMode = ShadowCastingMode.On,
                },
                new CharacterModel.RendererInfo()
                {
                    renderer = childLocator.FindChild("U Cloth01").GetComponent<Renderer>(),
                    defaultMaterial = voidCrystalMat,
                    ignoreOverlays = false,
                    defaultShadowCastingMode = ShadowCastingMode.On,
                },
                RenderInfoFromChild(childLocator.FindChild("S Body")),
                RenderInfoFromChild(childLocator.FindChild("U Body")),
                RenderInfoFromChild(childLocator.FindChild("S Cloth02"), true),
                RenderInfoFromChild(childLocator.FindChild("U Cloth02"), true),
                RenderInfoFromChild(childLocator.FindChild("S Hair")),
                RenderInfoFromChild(childLocator.FindChild("U Hair")),
                RenderInfoFromChild(childLocator.FindChild("S Jewelry")),
                RenderInfoFromChild(childLocator.FindChild("U Jewelry")),
                RenderInfoFromChild(childLocator.FindChild("S HandItems")),
                RenderInfoFromChild(childLocator.FindChild("U HandItems")),
                RenderInfoFromChild(childLocator.FindChild("S Shoe")),
                RenderInfoFromChild(childLocator.FindChild("U Shoe")),
            };

            var modelHurtBoxGroup = model.GetOrAddComponent<HurtBoxGroup>();
            var mainHurtBox = childLocator.FindChild("MainHurtbox").gameObject;
            mainHurtBox.layer = LayerIndex.entityPrecise.intVal;
            var mainHurtBoxComponent = mainHurtBox.GetOrAddComponent<HurtBox>();
            mainHurtBoxComponent.isBullseye = true;
            modelHurtBoxGroup.hurtBoxes = new[] { mainHurtBoxComponent };

            return model;
        }

        GameObject IBodyDisplay.BuildObject()
        {
            if (!TryGetGameObject<Kamunagi, IModel>(out var model)) throw new Exception("Model not loaded.");
            var displayModel = model.InstantiateClone("KamunagiDisplay");
            displayModel.GetComponent<Animator>().runtimeAnimatorController =
                LoadAsset<RuntimeAnimatorController>("bundle:animHenryMenu");
            return displayModel;
        }

        GameObject IBody.BuildObject()
        {
            if (!TryGetGameObject<Kamunagi, IModel>(out var model)) throw new Exception("Model not loaded.");
            var bodyPrefab = LoadAsset<GameObject>("legacy:Prefabs/CharacterBodies/CommandoBody")
                .InstantiateClone("NinesKamunagiBody");

            var bodyHealthComponent = bodyPrefab.GetComponent<HealthComponent>();

            #region Setup Model

            var bodyHurtBoxGroup = model.GetComponentInChildren<HurtBoxGroup>();
            foreach (var hurtBox in bodyHurtBoxGroup.hurtBoxes)
            {
                hurtBox.healthComponent = bodyHealthComponent;
            }

            var bodyModelLocator = bodyPrefab.GetComponent<ModelLocator>();
            Object.Destroy(bodyModelLocator.modelTransform.gameObject);
            model.transform.parent = bodyModelLocator.modelBaseTransform;
            bodyModelLocator.modelTransform = model.transform;

            #endregion

            #region Setup StateMachines

            #endregion

            #region Setup Skills

            foreach (var toDestroy in bodyPrefab.GetComponents<GenericSkill>())
            {
                Object.Destroy(toDestroy);
            }

            var skillLocator = bodyPrefab.GetComponent<SkillLocator>();
            var extraSkillLocator = bodyPrefab.AddComponent<ExtraSkillLocator>();
            if (TryGetAsset<KamunagiSkillFamilyExtraFourth>(out var skillFamily))
            {
                var skill = bodyPrefab.AddComponent<GenericSkill>();
                skill._skillFamily = skillFamily;
                skillLocator.primary = skill;
            }

            #endregion

            return bodyPrefab;
        }

        SurvivorDef ISurvivor.BuildObject()
        {
            var survivor = ScriptableObject.CreateInstance<SurvivorDef>();
            survivor.primaryColor = new Color(0.592156863f, 0f, 0.964705882f);
            survivor.displayNameToken = "NINES_KAMUNAGI_BODY_NAME";
            survivor.descriptionToken = "NINES_KAMUNAGI_BODY_DESCRIPTION";
            survivor.outroFlavorToken = "NINES_KAMUNAGI_BODY_OUTRO_FLAVOR";
            survivor.mainEndingEscapeFailureFlavorToken = "NINES_KAMUNAGI_BODY_OUTRO_FAILURE";
            survivor.desiredSortPosition = 100f;

            if (TryGetGameObject<Kamunagi, IBody>(out var body))
                survivor.bodyPrefab = body;

            if (TryGetGameObject<Kamunagi, IBodyDisplay>(out var display))
                survivor.displayPrefab = display;

            return survivor;
        }

        public class KamunagiSkillFamilyExtraFourth : Asset, ISkillFamily
        {
            public SkillFamily BuildObject()
            {
                var family = ScriptableObject.CreateInstance<SkillFamily>();
                if (TryGetAsset<MothMoth>(out var variant))
                    family.variants = new[] { (SkillFamily.Variant)variant };
                return family;
            }
        }
    }
}