using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi;
using R2API;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KamunagiOfChains
{
	[HarmonyPatch]
	public class KamunagiHUD : Asset, IGameObject
	{
		GameObject IGameObject.BuildObject()
		{
			var healthBarObject = LoadAsset<GameObject>("RoR2/Base/UI/HUDSimple.prefab")!.GetComponent<HUD>().healthBar
				.gameObject.InstantiateClone("ZealBar", false);
			var hud = new GameObject("ZealBarParent");
			var transform = hud.AddComponent<RectTransform>();
			transform.SetParent(healthBarObject.transform.parent);
			hud.layer = LayerIndex.ui.intVal;
			healthBarObject.transform.SetParent(hud.transform);
			hud.AddComponent<ZealBarHider>().zealBar = healthBarObject;
			var healthBar = healthBarObject.GetComponent<HealthBar>();
			healthBarObject.AddComponent<ZealBar>().InitFromHealthBar(healthBar);
			Object.Destroy(healthBar);
			return hud;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(HUD), nameof(HUD.Awake))]
		public static void AddZealBar(HUD __instance)
		{
			var healthBarTransform = __instance.healthBar.gameObject.transform;
			var zealBar = Object.Instantiate(GetGameObject<KamunagiHUD, IGameObject>(), healthBarTransform.parent.parent);
			zealBar.transform.rotation = healthBarTransform.rotation;
			zealBar.transform.localScale = healthBarTransform.localScale;
			zealBar.transform.localPosition = healthBarTransform.localPosition;
		}
	}

	public class ZealBarHider : MonoBehaviour
	{
		public GameObject zealBar;
		public HUD hud;
		public BodyIndex kamunagiIndex;
		public GameObject cachedBody;

		public void Start()
		{
			hud = GetComponentInParent<HUD>();
			kamunagiIndex = Asset.GetAsset<KamunagiAsset>();
			zealBar.transform.SetParent(hud.healthBar.transform.parent);
		}

		public void Update()
		{
			if (!hud || hud.targetBodyObject == cachedBody) return;
			cachedBody = hud.targetBodyObject;
			zealBar.SetActive(cachedBody && cachedBody.GetComponent<CharacterBody>().bodyIndex == kamunagiIndex);
		}
	}

	public class ZealBar : MonoBehaviour
	{
		public UIElementPairAllocator<Image, BarInfo> barAllocator;
		public RectTransform barContainer;
		public SpriteAsNumberManager spriteAsNumberManager;
		public HUD hud;
		public TwinBehaviour twinsBehaviour;
		public int? cachedZeal;
		public Object cachedBody;
		public GameObject barPrefab;
		public HealthBarStyle.BarStyle zealStyle;

		public List<BarInfo> barInfos = new List<BarInfo>()
		{
			new BarInfo(),
			new BarInfo()
		};

		public float cachedZealForLerp;
		public float zealVelocity;
		public HealthBarStyle.BarStyle trailingOverZealStyle;
		public Material zealMat;
		public Material defaultMaterial;

		public void InitFromHealthBar(HealthBar healthBar)
		{
			barContainer = healthBar.barContainer;
			barPrefab = healthBar.style.barPrefab;
			spriteAsNumberManager = healthBar.spriteAsNumberManager;
			trailingOverZealStyle = healthBar.style.trailingOverHealthBarStyle;
			trailingOverZealStyle.baseColor = Colors.zealColor;
			trailingOverZealStyle.sprite = null;
			
			zealStyle = healthBar.style.instantHealthBarStyle;
			zealStyle.baseColor = Colors.zealColor * new Color(1.1f, 1.1f, 1.1f);
			
			zealMat = LoadAsset<Material>("bundle2:ZealMat")!;
			defaultMaterial = barPrefab.GetComponentInChildren<Image>().material;
		}

		public void Awake()
		{
			hud = GetComponentInParent<HUD>();
			barAllocator = new UIElementPairAllocator<Image, BarInfo>(barContainer, barPrefab);
		}

		public void Update()
		{
			if (!hud || hud.targetBodyObject == null) return;
			var targetBody = hud.targetBodyObject;
			if (targetBody != cachedBody)
			{
				twinsBehaviour = targetBody.GetComponent<TwinBehaviour>();
				cachedBody = targetBody;
			}

			if (!twinsBehaviour) return;
			if (cachedZeal != twinsBehaviour.zealMeter)
			{
				cachedZeal = twinsBehaviour.zealMeter;
				spriteAsNumberManager.SetHitPointValues((int)cachedZeal, twinsBehaviour.maxZeal);
			}
			
			barInfos.Clear();

			var zealInfo = new BarInfo();
			ApplyStyle(ref zealInfo, zealStyle, defaultMaterial);
			zealInfo.normalizedXMax = twinsBehaviour.zealMeterNormalized;
			zealInfo.enabled = !zealInfo.normalizedXMax.Equals(0);
			if (zealInfo.enabled)
				barInfos.Add(zealInfo);
			
			var trailingOverZeal = new BarInfo();
			ApplyStyle(ref trailingOverZeal, trailingOverZealStyle, zealMat);
			cachedZealForLerp = Mathf.SmoothDamp(cachedZealForLerp, twinsBehaviour.zealMeterNormalized, ref zealVelocity, 0.2f,
				float.PositiveInfinity, Time.deltaTime);
			trailingOverZeal.normalizedXMax = cachedZealForLerp > 0.01 ? cachedZealForLerp : 0;
			trailingOverZeal.enabled = !trailingOverZeal.normalizedXMax.Equals(0);
			if (trailingOverZeal.enabled)
				barInfos.Add(trailingOverZeal);
			
			var activeBars = barInfos.Count;
			barAllocator.AllocateElements(activeBars);
			for (var i = 0; i < activeBars; i++)
			{
				var info = barInfos[i];
				HandleBar(i, ref info);
				barAllocator.SetData(info, i);	
			}
		}

		public static void ApplyStyle(ref BarInfo barInfo, HealthBarStyle.BarStyle barStyle, Material material)
		{
			barInfo.enabled &= barStyle.enabled;
			barInfo.color = barStyle.baseColor;
			barInfo.sprite = barStyle.sprite;
			barInfo.imageType = barStyle.imageType;
			barInfo.sizeDelta = barStyle.sizeDelta;
			barInfo.material = material;
		}

		public void HandleBar(int i, ref BarInfo barInfo)
		{
			if (!barInfo.enabled) return;
			var image = barAllocator.elements[i];
			var cachedBarInfo = barAllocator.elementsData[i];
			if (!cachedBarInfo.DimsEqual(barInfo))
			{
				cachedBarInfo.SetDims(barInfo);
				SetRectPosition((RectTransform)image.transform, barInfo.normalizedXMin, barInfo.normalizedXMax,
					barInfo.sizeDelta);
			}

			if (barInfo.imageType != cachedBarInfo.imageType)
			{
				cachedBarInfo.imageType = barInfo.imageType;
				image.type = barInfo.imageType;
			}

			if (barInfo.sprite != cachedBarInfo.sprite)
			{
				cachedBarInfo.sprite = barInfo.sprite;
				image.sprite = barInfo.sprite;
			}

			if (barInfo.color != image.color)
			{
				cachedBarInfo.color = barInfo.color;
				image.color = barInfo.color;
			}

			if (barInfo.material != image.material)
			{
				cachedBarInfo.material = barInfo.material;
				image.material = barInfo.material;
			}

			if (Mathf.Abs(barInfo.normalizedXMin - cachedBarInfo.normalizedXMin) > 0.01)
				cachedBarInfo.normalizedXMin = barInfo.normalizedXMin;
			if (Mathf.Abs(barInfo.normalizedXMax - cachedBarInfo.normalizedXMax) > 0.01)
				cachedBarInfo.normalizedXMax = barInfo.normalizedXMax;
			barAllocator.SetData(cachedBarInfo, i);
		}

		public static void SetRectPosition(RectTransform rectTransform, float xMin, float xMax, float sizeDelta)
		{
			rectTransform.anchorMin = new Vector2(xMin, 0f);
			rectTransform.anchorMax = new Vector2(xMax, 1f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = new Vector2(sizeDelta * 0.5f + 1f, sizeDelta + 1f);
		}
	}
	public struct BarInfo
	{
		public bool DimsEqual(BarInfo rhs)
		{
			return this.normalizedXMin == rhs.normalizedXMin && this.normalizedXMax == rhs.normalizedXMax && this.sizeDelta == rhs.sizeDelta;
		}
		
		public void SetDims(BarInfo rhs)
		{
			normalizedXMin = rhs.normalizedXMin;
			normalizedXMax = rhs.normalizedXMax;
			sizeDelta = rhs.sizeDelta;
		}
		
		public bool enabled;
		public Color color;
		public Sprite sprite;
		public Image.Type imageType;
		public int index;
		public float normalizedXMin;
		public float normalizedXMax;
		public float sizeDelta;
		public Material material;
	}
}