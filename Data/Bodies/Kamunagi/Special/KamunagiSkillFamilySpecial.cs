using System;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Special
{
    public class KamunagiSkillFamilySpecial : Asset, ISkillFamily
    {
        public SkillFamily BuildObject()
        {
            var family = ScriptableObject.CreateInstance<SkillFamily>();
            if (TryGetAsset<TheGreatSealing>(out var sealing))
                family.variants = new []{(SkillFamily.Variant)sealing};
            return family;
        }
    }
}