using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class KamunagiSkillFamilyExtra : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[]
		{
			GetAsset<MothMoth>(), GetAsset<XinZhao>(), GetAsset<SummonTatari>(), 
			GetAsset<SummonNugwisomkami>(), GetAsset<MashiroBlessing>(), GetAsset<Overzeal>(), 
			GetAsset<KuonFlashbang>(), GetAsset<HonokasVeil>(), GetAsset<MagicBallSkill>()
		};

		public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaExtra" ? "NINES_SARAANA_EXTRA" : "NINES_URURUU_EXTRA";
	}

	public class KamunagiSkillFamilyExtra2 : KamunagiSkillFamilyExtra {}
}