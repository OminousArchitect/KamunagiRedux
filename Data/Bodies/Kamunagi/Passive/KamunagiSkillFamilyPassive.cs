namespace KamunagiOfChains.Data.Bodies.Kamunagi.Passive
{
	public class KamunagiSkillFamilyPassive : Asset, ISkillFamily
	{
		public IEnumerable<Type> GetSkillAssets() => new[] { typeof(KamunagiDash) };
	}
}