namespace KamunagiOfChains.Data.Bodies.Kamunagi.Passive
{
	public class KamunagiSkillFamilyPassive : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<KamunagiDash>() };
	}
}