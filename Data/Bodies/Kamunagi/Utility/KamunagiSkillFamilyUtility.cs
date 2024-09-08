using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class KamunagiSkillFamilyUtility : Asset, ISkillFamily
	{
		public Type[] GetSkillAssets() =>
			new[] { typeof(Mikazuchi), typeof(AtuysTides), typeof(HonokasVeil), typeof(WoshisZone), typeof(JachdwaltStrikes) };

		public string GetNameToken(GenericSkill skill) =>
			skill.skillName == "SaraanaUtility" ? "NINES_SARAANA_UTILITY" : "NINES_URURUU_UTILITY";
	}
}