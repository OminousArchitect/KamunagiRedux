using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
    public class KamunagiSkillFamilyPrimary : Asset, ISkillFamily
    {
        public SkillFamily BuildObject()
        {
            var family = ScriptableObject.CreateInstance<SkillFamily>();
            if (TryGetAsset<SoeiMusou>(out var soei) && TryGetAsset<ReaverMusou>(out var reaver) && TryGetAsset<AltSoeiMusou>(out var alt))
                family.variants = new[] { (SkillFamily.Variant)soei, (SkillFamily.Variant)reaver, (SkillFamily.Variant)alt };
            return family;
        }

        public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaPrimary"  ? "NINES_SARAANA_PRIMARY" : "NINES_URURUU_PRIMARY";
    }
}