namespace KamunagiOfChains.Data.Bodies.Kamunagi.Passive
{
	public class KamunagiSkillFamilyPassive : Asset, ISkillFamily
	{
		public Type[] GetSkillAssets() => new[] { typeof(KamunagiDash) };
	}
}