using RoR2;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
	public class DelayedFireWithEffectState<T> : BaseTwinState where T : Asset, IEffect
	{
		public virtual float baseDuration => 1.25f;
		public virtual float baseDelay => 0.4f;
		public virtual string muzzle => "MuzzleCenter";
		public float duration;
		public float fireDelay;
		public bool hasFired;
		public EffectManagerHelper chargeEffectInstance;

		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;
			fireDelay = baseDelay / attackSpeedStat;
			var muzzleTransform = FindModelChild(muzzle);
			if (muzzleTransform && Asset.TryGetGameObject<T, IEffect>(out var muzzleEffect))
				chargeEffectInstance = EffectManager.GetAndActivatePooledEffect(muzzleEffect, muzzleTransform, true);
		}

		public virtual void Fire()
		{
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;
			if (fixedAge >= fireDelay && !hasFired)
			{
				hasFired = true;
				Fire();
			}

			if (fixedAge >= duration) outer.SetNextStateToMain();
		}

		public override void OnExit()
		{
			base.OnExit();
			if (chargeEffectInstance != null) chargeEffectInstance.ReturnToPool();
		}
	}
}