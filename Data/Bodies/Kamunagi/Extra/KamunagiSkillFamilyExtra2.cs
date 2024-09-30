using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class KamunagiSkillFamilyExtra2 : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[]
		{
			GetAsset<TwinsChildTeleport>(), GetAsset<MashiroBlessing>(), GetAsset<SummonTatari>()
		};

		public string GetNameToken(GenericSkill skill) =>
			skill.skillName == "SaraanaExtra2" ? "NINES_SARAANA_EXTRA" : "NINES_URURUU_EXTRA";
	}
}