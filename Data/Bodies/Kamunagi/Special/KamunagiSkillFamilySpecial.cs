using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Special
{
    public class KamunagiSkillFamilySpecial : Asset, ISkillFamily
    {
        public SkillFamily BuildObject()
        {
            var family = ScriptableObject.CreateInstance<SkillFamily>();
            if (TryGetAsset<SoeiMusou>(out var soei) && TryGetAsset<ReaverMusou>(out var reaver) && TryGetAsset<AltSoeiMusou>(out var alt))
                family.variants = new[] { (SkillFamily.Variant)soei, (SkillFamily.Variant)reaver, (SkillFamily.Variant)alt };
            return family;
        }
    }
}