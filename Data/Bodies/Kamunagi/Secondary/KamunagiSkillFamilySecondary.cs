using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class KamunagiSkillFamilySecondary : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() =>
			new Asset[]
			{
				GetAsset<EnnakamuyEarth>(), GetAsset<WindBoomerang>(), GetAsset<DenebokshiriBrimstone>(), GetAsset<KujyuriFrost>()
			};

		public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaSecondary"
			? "NINES_SARAANA_SECONDARY"
			: "NINES_URURUU_SECONDARY";
	}

	public class KamunagiSkillFamilySecondary2 : KamunagiSkillFamilySecondary
	{
	}
}