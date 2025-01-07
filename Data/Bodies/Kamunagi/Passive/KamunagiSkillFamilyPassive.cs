namespace KamunagiOfChains.Data.Bodies.Kamunagi.Passive
{
	public class KamunagiSkillFamilyPassive : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[]
		{
			GetAsset<KamunagiDash>()
		};

		public bool HiddenFromCharacterSelect => GetSkillAssets().Count() == 1;
	}
}