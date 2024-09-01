using System;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

public class Helpers 
{
    //I stole all of these from Dragonyck haha
    
    public static GenericSkill NewGenericSkill(GameObject obj, SkillDef skill)
    {
        GenericSkill generic = obj.AddComponent<GenericSkill>();
        SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
        newFamily.variants = new SkillFamily.Variant[1];
        generic._skillFamily = newFamily;
        
        SkillFamily skillFamily = generic.skillFamily;
        skillFamily.variants[0] = new SkillFamily.Variant
        {
            skillDef = skill,
            viewableNode = new ViewablesCatalog.Node(skill.skillNameToken, false, null)
        };
        ContentAddition.AddSkillFamily(skillFamily);
        return generic;
    }
    public static void AddAlt(SkillFamily skillFamily, SkillDef SkillDef)
    {
        Array.Resize<SkillFamily.Variant>(ref skillFamily.variants, skillFamily.variants.Length + 1);
        skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
        {
            skillDef = SkillDef,
            viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
        };
    }  
    
    internal static EffectComponent RegisterEffect(GameObject effect, float duration, string soundName = "", bool parentToReferencedTransform = true, bool positionAtReferencedTransform = true)
    {
        var effectcomponent = effect.GetComponent<EffectComponent>();
        if (!effectcomponent)
        {
            effectcomponent = effect.AddComponent<EffectComponent>();
        }                                   
        if (duration != -1)
        {
            var destroyOnTimer = effect.GetComponent<DestroyOnTimer>();
            if (!destroyOnTimer)
            {
                effect.AddComponent<DestroyOnTimer>().duration = duration;
            }
            else
            {
                destroyOnTimer.duration = duration;
            }
        }
        if (!effect.GetComponent<NetworkIdentity>())
        {
            effect.AddComponent<NetworkIdentity>();
        }
        if (!effect.GetComponent<VFXAttributes>())
        {
            effect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
        }
        effectcomponent.applyScale = false;
        effectcomponent.effectIndex = EffectIndex.Invalid;
        effectcomponent.parentToReferencedTransform = parentToReferencedTransform;
        effectcomponent.positionAtReferencedTransform = positionAtReferencedTransform;
        effectcomponent.soundName = soundName;
        ContentAddition.AddEffect(effect);
        return effectcomponent;
    }
}