using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	public class KamunagiSkillFamilyPrimary : Asset, ISkillFamily
	{
		public Type[] GetSkillAssets() => new[] { typeof(SoeiMusou), typeof(ReaverMusou), typeof(AltSoeiMusou) };

		public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaPrimary" ? "NINES_SARAANA_PRIMARY" : "NINES_URURUU_PRIMARY";
	}
}