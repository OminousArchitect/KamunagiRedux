using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
    public class KamunagiSkillFamilyUtility : Asset, ISkillFamily
    {
        public SkillFamily BuildObject()
        {
            var family = ScriptableObject.CreateInstance<SkillFamily>();
            if (TryGetAsset<Mikazuchi>(out var miza))
                family.variants = new []{(SkillFamily.Variant) miza};
            return family;
        }
        public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaUtility"  ? "NINES_SARAANA_UTILITY" : "NINES_URURUU_UTILITY";
    }
}