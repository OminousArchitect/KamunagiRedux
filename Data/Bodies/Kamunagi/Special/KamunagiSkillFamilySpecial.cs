using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Special
{
	public class KamunagiSkillFamilySpecial : Asset, ISkillFamily
	{
		public Type[] GetSkillAssets() =>
			new[] { typeof(SobuGekishoha), typeof(TheGreatSealing), typeof(LightOfNaturesAxiom) };

		public string GetNameToken(GenericSkill skill) =>
			skill.skillName == "SaraanaSpecial" ? "NINES_SARAANA_SPECIAL" : "NINES_URURUU_SPECIAL";
	}

	public class KamunagiSkillFamilySpecial2 : KamunagiSkillFamilySpecial
	{
	}
}