using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class KamunagiSkillFamilyUtility : Asset, ISkillFamily
	{
		public IEnumerable<Type> GetSkillAssets() =>
			new[] { typeof(Mikazuchi), typeof(JachdwaltStrikes), typeof(WoshisZone), typeof(AtuysTides), typeof(HonokasVeil) };

		public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaUtility" ? "NINES_SARAANA_UTILITY" : "NINES_URURUU_UTILITY";
	}
	public class KamunagiSkillFamilyUtility2 : KamunagiSkillFamilyUtility {}
}