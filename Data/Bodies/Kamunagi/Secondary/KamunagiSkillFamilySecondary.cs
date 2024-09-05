using System;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
    public class KamunagiSkillFamilySecondary : Asset, ISkillFamily
    {
        public SkillFamily BuildObject()
        {
            var family = ScriptableObject.CreateInstance<SkillFamily>();
            if (TryGetAsset<WindBoomerang>(out var wind))
                family.variants = new[] { (SkillFamily.Variant)wind };
            return family;
        }
    }
}