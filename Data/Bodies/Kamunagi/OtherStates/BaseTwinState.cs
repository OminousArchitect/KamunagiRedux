using EntityStates;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
    public class BaseTwinState : BaseSkillState
    {
        public virtual int meterGain => 10;
        private TwinBehaviour? _twinBehaviour;
        public TwinBehaviour twinBehaviour => _twinBehaviour ??= characterBody.GetComponent<TwinBehaviour>();
        public string twinMuzzle => twinBehaviour.twinMuzzle;
    }
}