using System;
using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
    public class WoshisZoneState : IndicatorSpellState
    {
        public override float duration => 0.45f;
        public override float failedCastCooldown => 0f;
        public override float indicatorScale => 10f;

        public override void Fire(Vector3 targetPosition)
        {
            base.Fire(targetPosition);
            if (twinBehaviour.activeBuffWard)
            {
                NetworkServer.Destroy(twinBehaviour.activeBuffWard);
            }

            var ward = Object.Instantiate(Asset.GetGameObject<WoshisZone, INetworkedObject>(), targetPosition,
                Quaternion.identity);
            ward.GetComponent<TeamFilter>().teamIndex = TeamIndex.Monster;
            twinBehaviour.activeBuffWard = ward;
            NetworkServer.Spawn(ward);
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }

    public class WoshisZone : Asset, ISkill, INetworkedObject
    {
        SkillDef ISkill.BuildObject()
        {
            var skill = ScriptableObject.CreateInstance<SkillDef>();
            skill.skillName = "Utility 5";
            skill.skillNameToken = KamunagiAsset.tokenPrefix + "UTILITY3_NAME";
            skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "UTILITY3_DESCRIPTION";
            skill.icon = LoadAsset<Sprite>("bundle:Woshis");
            skill.activationStateMachineName = "Weapon";
            skill.baseRechargeInterval = 4f;
            skill.beginSkillCooldownOnSkillEnd = true;
            skill.interruptPriority = InterruptPriority.Any;
            return skill;
        }

        Type[] ISkill.GetEntityStates() => new[] { typeof(WoshisZoneState) };

        GameObject INetworkedObject.BuildObject()
        {
            var woshisWard =
                LoadAsset<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab")!.InstantiateClone("WoshisWard",
                    true);
            var woshisEnergy =
                new Material(
                    LoadAsset<Material>("RoR2/Base/BleedOnHitAndExplode/matBleedOnHitAndExplodeAreaIndicator.mat"));
            woshisEnergy.SetFloat("_DstBlendFloat", 3f);
            woshisEnergy.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampImp2.png"));
            woshisEnergy.SetFloat("_Boost", 0.1f);
            woshisEnergy.SetFloat("_RimPower", 0.48f);
            woshisEnergy.SetFloat("_RimStrength", 0.12f);
            woshisEnergy.SetFloat("_AlphaBoost", 6.55f);
            woshisEnergy.SetFloat("_IntersectionStrength", 5.12f);

            Object.Destroy(woshisWard.GetComponent<NetworkedBodyAttachment>());
            woshisWard.GetComponentInChildren<MeshRenderer>().material = woshisEnergy;
            woshisWard.GetComponent<BuffWard>().radius = 10f;
            woshisWard.AddComponent<DestroyOnTimer>().duration = 8f;
            return woshisWard;
        }
    }
}