using EntityStates;
using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
	public class BaseTwinState : BaseSkillState, IZealState
	{
		public virtual int meterGain => 10;
		public virtual int meterGainOnExit => 0;
		private TwinBehaviour? _twinBehaviour;
		public TwinBehaviour twinBehaviour => _twinBehaviour ??= characterBody.GetComponent<TwinBehaviour>();
		public string twinMuzzle => twinBehaviour.twinMuzzle;
		
		public bool IsKeyDownAuthority()
		{
			var slot = skillLocator.FindSkillSlot(activatorSkillSlot);
			return slot == SkillSlot.None ? inputBank.interact.down : this.IsKeyDownAuthority(skillLocator, inputBank);
		}
	}

	public interface IZealState
	{
		public int meterGain { get; }
		public virtual int meterGainOnExit => 0;
	}
}