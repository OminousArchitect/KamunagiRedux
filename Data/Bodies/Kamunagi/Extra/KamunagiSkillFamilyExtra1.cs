using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class KamunagiSkillFamilyExtra1 : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[]
		{
			GetAsset<MothMoth>(), GetAsset<XinZhao>(), 
			GetAsset<SummonTatari>(), 
			GetAsset<SummonNugwisomkami>(),
			GetAsset<MashiroBlessing>(),
			
			GetAsset<KuonFlashbang>(), GetAsset<HonokasVeil>()
		};

		public string GetNameToken(GenericSkill skill) =>
			skill.skillName == "SaraanaExtra" ? "NINES_SARAANA_EXTRA" : "NINES_URURUU_EXTRA";
	}
}