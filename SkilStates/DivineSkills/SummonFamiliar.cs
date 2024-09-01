using EntityStates;
using RoR2;
using UnityEngine;
using ExtraSkillSlots;

using UnityEngine.Events;

namespace Kamunagi
{
    class SummonFamiliar : BaseTwinState
    {
        private float CooldownPerSummon;
        private float maxDuration = 5f;
        private float maxDistance = 100f;
        private Vector3 position;
        private GameObject areaIndicator;
        private GameObject blinkVfxInstance;
        private CharacterModel model;
        private ExtraInputBankTest input;

        public override void OnEnter()
        {
            base.OnEnter();
            input = outer.GetComponent<ExtraInputBankTest>();
            if (base.isAuthority)
            {
                areaIndicator = UnityEngine.Object.Instantiate(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab, base.transform.position, Quaternion.identity);
                areaIndicator.transform.localScale = new Vector3(2, 3, 2);
                areaIndicator.GetComponentInChildren<MeshRenderer>().material = Prefabs.Load<Material>("RoR2/Base/Nullifier/matNullifierZoneAreaIndicatorLookingIn.mat");
            }

            //if you wanna have a specific cd per summon 
            /*if (twinBehaviour.activeSummonTracker.theWispies.Length > 0)
            {
                var baseCD = base.skillLocator.special.skillDef.baseRechargeInterval;
                CooldownPerSummon = baseCD / SkillPrefabs.WispCount(null, 0);
                base.skillLocator.special.RunRecharge(baseCD - CooldownPerSummon);
            }*/
        }
        
        public override void Update()
        {
            base.Update();
            if (areaIndicator)
            {
                float num = 0f;
                RaycastHit raycastHit;
                if (Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(base.GetAimRay(), base.gameObject, out num), out raycastHit, maxDistance + num, LayerIndex.world.mask | LayerIndex.entityPrecise.mask))
                {
                    position = raycastHit.point;
                    areaIndicator.transform.position = position;
                }
            }
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= maxDuration || !input.extraSkill4.down)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            var whatTheFuck = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
            MasterSummon summon = new MasterSummon
            { 
                masterPrefab = Prefabs.wispMaster, 
                position = areaIndicator.transform.position + (Vector3.up * 1), //Utils.FindNearestNodePosition((base.transform.position + Vector3.up * 2) + UnityEngine.Random.rotation.normalized.eulerAngles * RoR2Application.rng.RangeFloat(1, 3), RoR2.Navigation.MapNodeGroup.GraphType.Air),//rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward),
                summonerBodyObject = base.gameObject, ignoreTeamMemberLimit = true
            };
            CharacterMaster characterMaster = summon.Perform();
            Deployable deployable = characterMaster.gameObject.AddComponent<Deployable>();
            deployable.onUndeploy = new UnityEvent();
            deployable.onUndeploy.AddListener(new UnityAction(characterMaster.TrueKill));
            base.characterBody.master.AddDeployable(deployable, Prefabs.wisp);

            if (characterMaster.inventory)
            {
                
                switch (whatTheFuck.RangeInt(0, 3))
                {
                    case 0: 
                        //characterMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteFireEquipment"));
                        Debug.Log("This is Wisp 0");
                        break;
                    case 1:
                        //characterMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteLightningEquipment"));
                        Debug.Log("This is Wisp 1");
                        break;
                    case 2:
                        //characterMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteEarthEquipment"));
                        Debug.Log("This is Wisp 2");
                        break;
                    case 3:
                        //characterMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteLunarEquipment"));
                        Debug.Log("This is Wisp 3");
                        break;
                }
            }

            if (areaIndicator)
            {
                Destroy(areaIndicator);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
