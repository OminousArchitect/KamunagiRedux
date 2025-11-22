using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using RoR2;
using RoR2.Skills;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class KamunagiSkillFamilyExtra : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[]
		{
			GetAsset<MothMoth>(), GetAsset<XinZhao>(), 
			GetAsset<SummonNugwisomkami>(), GetAsset<MashiroBlessing>(), GetAsset<Overzeal>(),
			GetAsset<KuonFlashbang>(), GetAsset<HonokasVeil>() 
		};
	}

	public class KamunagiSkillFamilyExtra2 : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[]
		{
			GetAsset<HonokasVeil>(), GetAsset<KuonFlashbang>(), 
			GetAsset<Overzeal>(), GetAsset<MashiroBlessing>(), GetAsset<SummonNugwisomkami>(),
			GetAsset<XinZhao>(), GetAsset<MothMoth>() 
		};
	}
}