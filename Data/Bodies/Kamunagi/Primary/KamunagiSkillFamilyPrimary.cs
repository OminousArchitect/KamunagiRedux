using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	public class KamunagiSkillFamilyPrimary : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<SoeiMusou>(), GetAsset<ReaverMusou>(), GetAsset<AltSoeiMusou>() };

		public string GetNameToken(GenericSkill skill) =>
			skill.skillName == "SaraanaPrimary" ? "NINES_SARAANA_PRIMARY" : "NINES_URURUU_PRIMARY";
	}

	public class KamunagiSkillFamilyPrimary2 : KamunagiSkillFamilyPrimary
	{
	}
}