using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class KamunagiSkillFamilySecondary : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() =>
			new Concentric[]
			{
				GetAsset<EnnakamuyEarth>(), GetAsset<DenebokshiriBrimstone>(), GetAsset<KujyuriFrost>(), GetAsset<AccelerateWinds>(), GetAsset<LightPillar5>()
			};

		public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaSecondary"
			? "NINES_SARAANA_SECONDARY"
			: "NINES_URURUU_SECONDARY";
	}

	public class KamunagiSkillFamilySecondary2 : KamunagiSkillFamilySecondary
	{
	}
}