using EntityStates;
using RoR2.Skills;
using UnityEngine;

public class Defaults
{
    public void Yep()
    {
        var skillDef = ScriptableObject.CreateInstance<SkillDef>();
        skillDef.mustKeyPress = false;
        skillDef.isCombatSkill = true;
        skillDef.hideStockCount = false;
        skillDef.canceledFromSprinting = false;
        skillDef.cancelSprintingOnActivation = true;
        skillDef.beginSkillCooldownOnSkillEnd = false;
        skillDef.dontAllowPastMaxStocks = false;
        skillDef.fullRestockOnAssign = true;
        skillDef.attackSpeedBuffsRestockSpeed = false;
        skillDef.stockToConsume = 1;
        skillDef.requiredStock = 1;
        skillDef.rechargeStock = 1;
        skillDef.baseMaxStock = 1;
        skillDef.baseRechargeInterval = 1f;
        skillDef.interruptPriority = InterruptPriority.Skill;
    }
}