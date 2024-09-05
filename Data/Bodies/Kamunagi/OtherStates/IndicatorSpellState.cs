using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
    public class IndicatorSpellState : BaseTwinState
    {
        public virtual float duration => 5f;
        public virtual float failedCastCooldown => 2f;
        public virtual float indicatorScale => 3f;
        private GameObject? indicator;
        private bool wasActive;

        public override void Update()
        {
            if (!isAuthority) return;
            if (inputBank.GetAimRaycast(float.PositiveInfinity, out var hit))
            {
                if (indicator == null || !indicator)
                {
                    indicator = Object.Instantiate(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
                    indicator.transform.localScale = Vector3.one * indicatorScale;
                }

                indicator.transform.position = hit.point;
                if (wasActive) return;
                indicator.SetActive(true);
                wasActive = true;
                return;
            }

            if (indicator == null || !indicator || !wasActive) return;
            indicator.SetActive(false);
            wasActive = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority || (fixedAge < duration && IsKeyDownAuthority())) return;
            outer.SetNextStateToMain();
        }

        public virtual void Fire(Vector3 targetPosition)
        {
        }

        public override void OnExit()
        {
            base.OnExit();
            if (!isAuthority) return;
            if (indicator != null && indicator)
            {
                Destroy(indicator);
                if (indicator.activeSelf)
                {
                    Fire(indicator.transform.position);
                    return;
                }
            }
            activatorSkillSlot.rechargeStopwatch = activatorSkillSlot.finalRechargeInterval - failedCastCooldown;
        }
    }
}