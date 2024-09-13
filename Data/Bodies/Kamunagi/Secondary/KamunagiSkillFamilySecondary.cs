using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class KamunagiSkillFamilySecondary : Asset, ISkillFamily
	{
		public Type[] GetSkillAssets() =>
			new[]
			{
				typeof(EnnakamuyEarth), typeof(WindBoomerang), typeof(DenebokshiriBrimstone), typeof(KujyuriFrost)
			};

		public string GetNameToken(GenericSkill skill) => skill.skillName == "SaraanaSecondary"
			? "NINES_SARAANA_SECONDARY"
			: "NINES_URURUU_SECONDARY";
	}

	public class KamunagiSkillFamilySecondary2 : KamunagiSkillFamilySecondary
	{
	}
}