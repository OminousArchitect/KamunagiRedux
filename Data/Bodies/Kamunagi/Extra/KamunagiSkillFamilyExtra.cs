using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
    public class KamunagiSkillFamilyExtra : Asset, ISkillFamily
    {
        public SkillFamily BuildObject()
        {
            var family = ScriptableObject.CreateInstance<SkillFamily>();
            if (TryGetAsset<MothMoth>(out var mothmoth))
                family.variants = new[] { (SkillFamily.Variant)mothmoth };
            return family;
        }
    }
}