using KamunagiOfChains.Data.Bodies.Kamunagi.Extra;
using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class KamunagiSkillFamilyUtility : Concentric, ISkillFamily
	
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] 
			{ 
				GetAsset<KuonFlashbang>(),
				GetAsset<WoshisZone>(), 
				GetAsset<AtuysTides>(), 
				GetAsset<JachdwaltStrikes>(),
				GetAsset<Mikazuchi>(), 
				GetAsset<HonokasVeil>() 
			};

		public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaUtility" ? "NINES_SARAANA_UTILITY" : "NINES_URURUU_UTILITY";
	}
	public class KamunagiSkillFamilyUtility2 : KamunagiSkillFamilyUtility {}
}