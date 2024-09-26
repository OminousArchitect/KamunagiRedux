using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class KamunagiSkillFamilyUtility : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() =>
			new Asset[] { GetAsset<Mikazuchi>(), GetAsset<JachdwaltStrikes>(), GetAsset<WoshisZone>(), GetAsset<AtuysTides>(), GetAsset<TwinsChildTeleport>() };

		public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaUtility" ? "NINES_SARAANA_UTILITY" : "NINES_URURUU_UTILITY";
	}
	public class KamunagiSkillFamilyUtility2 : KamunagiSkillFamilyUtility {}
}