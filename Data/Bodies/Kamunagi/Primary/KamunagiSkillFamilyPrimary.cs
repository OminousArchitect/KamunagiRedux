using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Primary
{
	public class KamunagiSkillFamilyPrimary : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<SoeiMusou>(), GetAsset<AltSoeiMusou>(), GetAsset<ReaverMusou>(), GetAsset<MultiMusou>(), GetAsset<DebugZeal>() };

		public string GetNameToken(GenericSkill skill) =>
			skill.skillName == "SaraanaPrimary" ? "NINES_SARAANA_PRIMARY" : "NINES_URURUU_PRIMARY";
	}

	public class KamunagiSkillFamilyPrimary2 : KamunagiSkillFamilyPrimary
	{
	}
}