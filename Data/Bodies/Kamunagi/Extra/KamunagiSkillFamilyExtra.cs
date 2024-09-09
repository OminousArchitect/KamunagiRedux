using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class KamunagiSkillFamilyExtra : Asset, ISkillFamily
	{
		public Type[] GetSkillAssets() => new[] { typeof(MothMoth), typeof(MashiroBlessing), typeof(SummonFriendlyEnemy) };

		public string GetNameToken(GenericSkill skill) =>
			skill.skillName == "SaraanaExtra" ? "NINES_SARAANA_EXTRA" : "NINES_URURUU_EXTRA";
	}
}