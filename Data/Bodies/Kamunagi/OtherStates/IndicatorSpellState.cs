using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
	public class IndicatorSpellState : RaycastedSpellState
	{
		public virtual float indicatorScale => 3f;
		private GameObject? indicator;
		private bool wasActive;

		public override void Update()
		{
			base.Update();
			if (didHit)
			{
				if (indicator == null || !indicator)
				{
					indicator = UnityEngine.Object.Instantiate(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
					indicator.transform.localScale = Vector3.one * indicatorScale;
				}

				indicator.transform.position = lastHit.point;
				if (wasActive) return;
				indicator.SetActive(true);
				wasActive = true;
				return;
			}

			if (indicator == null || !indicator || !wasActive) return;
			indicator.SetActive(false);
			wasActive = false;
		}

		public override void OnExit() {
			base.OnExit();
			if (indicator == null || !indicator) return;
			Destroy(indicator);
		}
	}
}