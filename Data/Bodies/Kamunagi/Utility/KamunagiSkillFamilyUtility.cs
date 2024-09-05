using System;
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
    }
}