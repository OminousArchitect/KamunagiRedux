using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class KamunagiSkillFamilyExtra : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[]
		{
			GetAsset<MothMoth>(), GetAsset<XinZhao>(), GetAsset<SummonNugwisomkami>(), GetAsset<MashiroBlessing>(), GetAsset<SummonTatari>()
		};

		public string GetNameToken(GenericSkill skill) =>
			skill.skillName == "SaraanaExtra" ? "NINES_SARAANA_EXTRA" : "NINES_URURUU_EXTRA";
	}
}