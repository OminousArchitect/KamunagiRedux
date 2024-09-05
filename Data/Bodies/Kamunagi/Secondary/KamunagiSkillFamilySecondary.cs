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
            if (TryGetAsset<WindBoomerang>(out var wind) && TryGetAsset<DenebokshiriBrimstone>(out var fiery))
                family.variants = new[] { (SkillFamily.Variant)wind, (SkillFamily.Variant)fiery };
            
            return family;
        }
    }
}