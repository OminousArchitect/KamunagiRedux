using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using HG;
using System.Runtime.InteropServices;
using Kamunagi.Modules;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using Rewired.ComponentControls.Effects;
using TMPro;
using UnityEngine.Serialization;

namespace Kamunagi
{
    public class TwinBehaviour : MonoBehaviour
    {
        public GenericSkill primary;
        public GenericSkill secondary;
        public GenericSkill utility;
        public GenericSkill special;
        private CharacterBody body;
        private SkillLocator skillLocator;
        private GameObject meterBar;
        private TextMeshProUGUI currentMeter;
        private TextMeshProUGUI fullMeter;
        private EntityStateMachine veilStateMachine;
        private EntityStateMachine hoverStateMachine;
        private Image barImage;
        [SerializeField] public int meterValue = 0;
        [SerializeField] public int maxMeterValue = 80;
        private bool barSetupDone;
        public bool canExecute = true;
        private Color barColor = new Color(0.9294118f, 0.654902f, 0.09019608f);
        private bool swapped;
        public GameObject activeBuffWard;
        public TwinsActiveSummonTracker activeSummonTracker;
        [FormerlySerializedAs("lastDarkAscensionTimer")] [FormerlySerializedAs("timeSinceLastAscent")] 
        public float secondsSinceLastAscension;
        public float timeSinceLastHover;
        public bool offCooldown;
        private float cooldown = 5f;
        [FormerlySerializedAs("chainsVfx")] 
        public GameObject chainsVfx1;
        public GameObject chainsVfx2;
        private Transform chainTransform;
        public bool isInVeil;
        public bool isHovering;
        private Transform bone1;
        private Transform bone2;
        private SceneDef currentSceneDef;
        private bool isthisbroken;
        public bool wasLongHover;
        public float timeSpentHovering;

        internal bool maxMeter
        {
            get
            {
                return meterValue >= maxMeterValue;
            }
        }
        public float currentMeterValue
        {
            get
            {
                return meterValue;
            }
        }
        private bool muzzleToggle = true;

        public string muzzleString
        {
            get
            {
                string s = muzzleToggle ? "MuzzleRight" : "MuzzleLeft";
                muzzleToggle ^= true;
                return s;
            }
        }

        public void AddMeter(int value)
        {
            if (value + meterValue >= maxMeterValue && skillLocator)
            {
                if (swapped)
                {
                    swapped = false;

                    skillLocator.primary.UnsetSkillOverride(this, primary.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.UnsetSkillOverride(this, secondary.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.UnsetSkillOverride(this, utility.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.special.UnsetSkillOverride(this, special.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                else
                {
                    swapped = true;

                    skillLocator.primary.SetSkillOverride(this, primary.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(this, secondary.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(this, utility.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.special.SetSkillOverride(this, special.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                meterValue = 0;
                return;
            }
            meterValue += value;
        }

        private void OnEnable()
        {
            body = base.GetComponent<CharacterBody>();
            skillLocator = body.skillLocator;
            currentSceneDef = SceneCatalog.GetSceneDefForCurrentScene();
            var theModel = body.GetComponent<ModelLocator>().modelTransform;
            
            var locator = theModel.GetComponent<ChildLocator>();
            bone1 = locator.FindChild("U Bone");
            bone2 = locator.FindChild("S Bone");

            veilStateMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Weapon");
            hoverStateMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Jet");
            
            var sulfurPoolsDef = Prefabs.Load<SceneDef>("RoR2/DLC1/sulfurpools/sulfurpools.asset");
            if (currentSceneDef != sulfurPoolsDef)
            {
                chainsVfx1 = UnityEngine.Object.Instantiate(Prefabs.kamunagiChains, bone1);
                chainsVfx2 = UnityEngine.Object.Instantiate(Prefabs.kamunagiChains, bone2);
            }
            else
            {
                Debug.Log("[Kamunagi] Chains effect doesn't work properly for Sulfur Pools, disabling....");
            }
        }

        private void FixedUpdate()
        {
            secondsSinceLastAscension += Time.deltaTime;
            timeSinceLastHover += Time.deltaTime;

            offCooldown = secondsSinceLastAscension >= cooldown;

            isInVeil = veilStateMachine.state.GetType() == typeof(HonokasVeil);
            isHovering = hoverStateMachine.state.GetType() == typeof(Hover);
            
            if (offCooldown && !isInVeil)
            {
                chainsVfx1.SetActive(true);
                chainsVfx2.SetActive(true);
            }
            //Debug.Log($"{timeSpentHovering}");
        }

        private void Start()
        {
            if (body.masterObject)
            {
                activeSummonTracker = body.masterObject.GetComponent<TwinsActiveSummonTracker>();
                if (!activeSummonTracker)
                {
                    activeSummonTracker = body.masterObject.AddComponent<TwinsActiveSummonTracker>();
                }
            }
        }

        public void ToddHoward()
        {
            //AkSoundEngine.StopAll();
        }
        
        public void Rebirth()
        {
            var _master = body.master;
            Vector3 positionAtDeath = _master.deathFootPosition;
            if (_master.killedByUnsafeArea)
            {
                positionAtDeath = TeleportHelper.FindSafeTeleportDestination(positionAtDeath, body, RoR2Application.rng) ?? _master.deathFootPosition;
            }

            _master.Respawn(positionAtDeath, Quaternion.identity);
            body.AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
            GameObject rezEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
            if (_master.bodyInstanceObject)
            {
                EntityStateMachine[] array = _master.bodyInstanceObject.GetComponents<EntityStateMachine>();
                foreach (EntityStateMachine esm in array)
                {
                    esm.initialStateType = esm.mainStateType;
                }
                if (rezEffect)
                {
                    EffectManager.SpawnEffect(rezEffect, new EffectData
                    {
                        origin = positionAtDeath,
                        rotation = _master.bodyInstanceObject.transform.rotation
                    }, transmit: true);
                }
            }
        }
    }
}
