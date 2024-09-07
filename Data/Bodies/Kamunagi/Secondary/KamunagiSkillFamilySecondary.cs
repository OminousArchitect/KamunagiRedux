using System;
using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Secondary
{
	public class KamunagiSkillFamilySecondary : Asset, ISkillFamily
	{
		public Type[] GetSkillAssets() =>
			new[] { typeof(WindBoomerang), typeof(DenebokshiriBrimstone), typeof(EnnakamuyEarth) };

		public string GetNameToken(GenericSkill skill) =>
			skill.skillName == "SaraanaSecondary"
				? "NINES_SARAANA_SECONDARY"
				: "NINES_URURUU_SECONDARY";
	}
}