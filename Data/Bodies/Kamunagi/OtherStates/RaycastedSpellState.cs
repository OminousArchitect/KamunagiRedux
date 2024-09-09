using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
	public class RaycastedSpellState : BaseTwinState
	{
		public RaycastHit lastHit;
		public bool didHit;
		public virtual float duration => 5f;
		public virtual float failedCastCooldown => 2f;
		public virtual bool requireFullCharge => false;

		public override void Update()
		{
			if (!isAuthority) return;
			didHit = inputBank.GetAimRaycast(float.PositiveInfinity, out lastHit);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (isAuthority && (duration != 0 && fixedAge >= duration || !IsKeyDownAuthority()))
				outer.SetNextStateToMain();
		}

		public virtual void Fire(Vector3 targetPosition)
		{
		}

		public override void OnExit()
		{
			base.OnExit();
			if (!isAuthority) return;
			if (didHit && (!requireFullCharge || fixedAge >= duration))
			{
				Fire(lastHit.point);
				return;
			}

			activatorSkillSlot.rechargeStopwatch = activatorSkillSlot.finalRechargeInterval - failedCastCooldown;
		}
	}
}