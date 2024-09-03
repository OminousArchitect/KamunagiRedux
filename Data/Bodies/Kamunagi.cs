using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Rendering;

namespace KamunagiOfChains.Data.Bodies {
    public class Kamunagi : Asset, IBody, IBodyDisplay, ISurvivor, IModel {
        GameObject IModel.BuildObject()
        {
            var model = LoadAsset<GameObject>("bundle:mdlKamunagi");
            var characterModel = model.GetComponent<CharacterModel>();
            if (!characterModel) characterModel = model.AddComponent<CharacterModel>();
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
            characterModel.baseRendererInfos = new []
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

            var modelHurtBoxGroup = model.GetComponent<HurtBoxGroup>();
            if (!modelHurtBoxGroup) modelHurtBoxGroup = model.AddComponent<HurtBoxGroup>();
            var mainHurtBox = childLocator.FindChild("MainHurtbox").gameObject;
            mainHurtBox.layer = LayerIndex.entityPrecise.intVal;
            var mainHurtBoxComponent = mainHurtBox.AddComponent<HurtBox>();
            mainHurtBoxComponent.isBullseye = true;
            modelHurtBoxGroup.hurtBoxes = new []{mainHurtBoxComponent};

            return model;
        }
        
        GameObject IBodyDisplay.BuildObject()
        {
            if (!TryGetGameObject<Kamunagi, IModel>(out var model)) throw new Exception("Model not loaded.");
            var displayModel = PrefabAPI.InstantiateClone(model, "KamunagiDisplay");
            displayModel.GetComponent<Animator>().runtimeAnimatorController = LoadAsset<RuntimeAnimatorController>("bundle:animHenryMenu");
            return displayModel;
        }
        
        GameObject IBody.BuildObject()
        {
            if (!TryGetGameObject<Kamunagi, IModel>(out var model)) throw new Exception("Model not loaded.");
            var bodyPrefab = LoadAsset<GameObject>("legacy:Prefabs/CharacterBodies/CommandoBody").InstantiateClone("NinesKamunagiBody");

            var bodyHealthComponent = bodyPrefab.GetComponent<HealthComponent>();

            #region Setup Model
            var bodyHurtBoxGroup = model.GetComponentInChildren<HurtBoxGroup>();
            foreach (var hurtBox in bodyHurtBoxGroup.hurtBoxes)
            {
                hurtBox.healthComponent = bodyHealthComponent;
            }

            var bodyModelLocator = bodyPrefab.GetComponent<ModelLocator>();
            UnityEngine.Object.Destroy(bodyModelLocator.modelTransform.gameObject);
            model.transform.parent = bodyModelLocator.modelBaseTransform;
            bodyModelLocator.modelTransform = model.transform;
            #endregion

            #region Setup StateMachines
            
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
    }
}