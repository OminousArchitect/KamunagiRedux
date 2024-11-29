using EntityStates;
using EntityStates.GrandParent;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using Console = System.Console;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Special
{
	public class LightOfNaturesAxiomState : BaseTwinState
	{
		public Vector3 spawnPos;
		public EffectManagerHelper leftMuzzleInstance;
		public EffectManagerHelper rightMuzzleInstance;
		public GameObject sun;
		public uint channelSound;
		public Animator animator;

		public override void OnEnter()
		{
			base.OnEnter();

			var childLocator = GetModelChildLocator();
			PlayAnimation("Saraana Override", "EnterAxiom");
			PlayAnimation("Ururuu Override", "EnterAxiom");
			spawnPos = childLocator.FindChild("MuzzleCenter").position + characterDirection.forward * 5;
			leftMuzzleInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(
				Concentric.GetEffect<LightOfNaturesAxiom>().WaitForCompletion(), childLocator.FindChild("MuzzleLeft"), true,
				new EffectData() { scale = 0.3f });
			rightMuzzleInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(
				Concentric.GetEffect<LightOfNaturesAxiom>().WaitForCompletion(), childLocator.FindChild("MuzzleRight"), true,
				new EffectData() { scale = 0.3f });
			channelSound = Util.PlaySound(ChannelSunStart.beginSoundName, gameObject);
			animator = GetModelAnimator();
			animator.SetBool("inAxiom", true);
			if (!isAuthority) return;
			(characterMotor as IPhysMotor).velocityAuthority = Vector3.zero;
			characterMotor.useGravity = false;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (isAuthority && !IsKeyDownAuthority()) outer.SetNextStateToMain();
			(characterMotor as IPhysMotor).velocityAuthority = Vector3.zero;
			if (!NetworkServer.active || fixedAge < 1 || sun) return;
			sun = UnityEngine.Object.Instantiate(Concentric.GetNetworkedObject<NaturesAxiom>().WaitForCompletion(), spawnPos,
				Quaternion.identity);
			sun.GetComponent<GenericOwnership>().ownerObject = gameObject;
			sun.GetComponent<UmbralSunController>().bullseyeSearch.teamMaskFilter =
				TeamMask.GetEnemyTeams(teamComponent.teamIndex);
			NetworkServer.Spawn(sun);
			EffectManager.SimpleEffect(Concentric.GetEffect<NaturesAxiom>().WaitForCompletion(), spawnPos,
				Quaternion.identity, true);
		}

		public override void OnExit()
		{
			base.OnExit();
			if (sun)
			{
				Destroy(sun);
				EffectManager.SimpleEffect(Concentric.GetEffect<NaturesAxiom>().WaitForCompletion(), spawnPos,
					Quaternion.identity,
					true);
			}

			animator.SetBool("inAxiom", false);
			if (leftMuzzleInstance != null) leftMuzzleInstance.ReturnToPool();
			if (rightMuzzleInstance != null) rightMuzzleInstance.ReturnToPool();
			AkSoundEngine.StopPlayingID(channelSound);
			if (isAuthority)
			{
				characterMotor.useGravity = true;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;
	}

	public class LightOfNaturesAxiom : Concentric, ISkill, IEffect
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Special 2";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SPECIAL2_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SPECIAL2_DESCRIPTION";
			skill.icon = (await LoadAsset<Sprite>("kamunagiassets:RoU"));
			skill.activationStateMachineName = "Body";
			skill.baseRechargeInterval = 3f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.canceledFromSprinting = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = true;
			skill.keywordTokens = new[]
			{
				KamunagiAsset.tokenPrefix + "TWINSCURSE2_KEYWORD",
				KamunagiAsset.tokenPrefix + "TWINSCURSE3_KEYWORD"
			};
			return skill;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var chargeSunEffect =
				(await LoadAsset<GameObject>("RoR2/Base/Grandparent/ChargeGrandParentSunHands.prefab"))
				.InstantiateClone(
					"TwinsUltHands", false);

			var comp = chargeSunEffect.GetOrAddComponent<EffectComponent>();
			comp.applyScale = true;
			comp.parentToReferencedTransform = true;
			comp.positionAtReferencedTransform = true;
			var vfx = chargeSunEffect.GetOrAddComponent<VFXAttributes>();
			vfx.DoNotPool = false;
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;

			UnityEngine.Object.Destroy(chargeSunEffect.GetComponentInChildren<FlickerLight>());
			chargeSunEffect.transform.localScale = Vector3.one * 0.35f;
			chargeSunEffect.GetComponentInChildren<ObjectScaleCurve>().transform.localScale = Vector3.one * 1.5f;
			UnityEngine.Object.Destroy(chargeSunEffect.GetComponentInChildren<Light>());
			var Sunmesh = chargeSunEffect.GetComponentInChildren<MeshRenderer>(true);
			Sunmesh.gameObject.SetActive(true);
			Sunmesh.material =
				new Material(Sunmesh
					.material);
			Sunmesh.material.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampBottledChaos.png"));
			var sunP = chargeSunEffect.GetComponentInChildren<ParticleSystemRenderer>(true);
			//sunP[0].material = new Material(sunP[0].material);
			sunP.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
			sunP.material.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
			var lol = chargeSunEffect.GetComponentsInChildren<ParticleSystem>();
			var mainMain = lol[1].main;
			mainMain.startColor = new Color(0.45f, 0, 1);
			return chargeSunEffect;
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(LightOfNaturesAxiomState) };
	}

	[HarmonyPatch]
	public class NaturesAxiom : Concentric, INetworkedObject, IEffect, IBuff
	{ 
		async Task<GameObject> INetworkedObject.BuildObject()
		{
			Material shell = new Material(await LoadAsset<Material>("RoR2/DLC1/TreasureCacheVoid/matLockboxVoidEgg.mat"));
			shell.SetFloat("_SpecularStrength", 0.02f);
			shell.SetFloat("_SpecularExponent", 0.38f);
			
			shell.SetFloat("_EmissionPower", 1.67f);
			shell.SetFloat("_HeightStrength", 3.95f);
			shell.SetFloat("_HeightBias", 0.28f);

			var naturesAxiom = (await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandParentSun.prefab"))!.InstantiateClone("TwinsUltSun", true);
			UnityEngine.Object.Destroy(naturesAxiom.GetComponent<EntityStateMachine>());
			UnityEngine.Object.Destroy(naturesAxiom.GetComponent<NetworkStateMachine>());
			UnityEngine.Object.Destroy(naturesAxiom.GetComponent<GrandParentSunController>());
			naturesAxiom.AddComponent<UmbralSunController>();
			var theMeshObject = naturesAxiom.transform.Find("VfxRoot/Mesh/SunMesh").gameObject;
			theMeshObject.GetComponent<MeshRenderer>().material = shell;
			theMeshObject.name = "DarkStarShell";
			var vfxRoot = naturesAxiom.transform.GetChild(0).gameObject;
			vfxRoot.transform.localScale = Vector3.one * 0.5f;
			var sunL = naturesAxiom.GetComponentInChildren<Light>();
			sunL.intensity = 100;
			sunL.range = 70;
			sunL.color = new Color(0.45f, 0, 1);
			var sunMeshes = naturesAxiom.GetComponentsInChildren<MeshRenderer>(true);
			var sunIndicator = sunMeshes[0];
			sunIndicator.material = new Material(sunIndicator.material);
			sunIndicator.material.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampPortalVoid.png"));
			sunIndicator.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
			sunIndicator.transform.localScale = Vector3.one * 85f; //visual indicator
			sunMeshes[2].enabled = false;
			var sunPP = naturesAxiom.GetComponentInChildren<PostProcessVolume>();
			sunPP.profile = (await LoadAsset<PostProcessProfile>("RoR2/Base/Common/ppLocalVoidFogMild.asset"));
			sunPP.sharedProfile = sunPP.profile;
			sunPP.gameObject.AddComponent<SphereCollider>().radius = 40;
			var sunP = (await this.GetEffect()).GetComponentInChildren<ParticleSystemRenderer>(true);
			var destroyed = naturesAxiom.transform.Find("VfxRoot/Particles/GlowParticles, Fast").gameObject;
			var indicator = naturesAxiom.transform.Find("VfxRoot/Mesh/AreaIndicator");
			indicator.transform.localScale = Vector3.one * 100f;
			UnityEngine.Object.Destroy(destroyed);
			foreach (var r in naturesAxiom.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				var name = r.name;
				switch (name)
				{
					case "GlowParticles, Fast":
						r.material = sunP.material;
						r.transform.localScale = Vector3.one * 0.6f;
						break;
					case "GlowParticles":
						r.enabled = false;
						break;
					case "SoftGlow, Backdrop":
						r.material =
							new Material(await LoadAsset<Material>("RoR2/Junk/Common/VFX/matTeleportOutBodyGlow.mat"));
						r.material.SetColor("_TintColor", new Color(0f, 0.4F, 1));
						r.transform.localScale = Vector3.one * 0.5f;
						//r.enabled = false
						break;
					case "Donut":
					case "Trails":
						var material = new Material(r.material);
						material.SetColor("_TintColor", new Color(0.45f, 0, 1));
						material.SetTexture("_RemapTex",
							await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
						r.trailMaterial = material;
						r.material = material;
						break;
					case "Goo, Drip":
					case "Sparks":
						r.enabled = false;
						break;
				}
			}
			
			var darkStarCore = (await GetProjectileGhost<PrimedStickyBomb>())!.InstantiateClone("DarkStarCore", false);
			var trash = darkStarCore.transform.Find("Scaler/GameObject").gameObject;
			darkStarCore.transform.localPosition = Vector3.zero;
			UnityEngine.Object.Destroy(trash);
			UnityEngine.Object.Destroy(darkStarCore.GetComponent<ProjectileGhostController>());
			UnityEngine.Object.Destroy(darkStarCore.GetComponent<VFXAttributes>());
			UnityEngine.Object.Destroy(darkStarCore.GetComponent<EffectManagerHelper>());
			darkStarCore.transform.SetParent(theMeshObject.transform);

			GameObject particle = await LoadAsset<GameObject>("kamunagiassets2:InnerCoreDistCurve2");
			particle.GetComponent<ParticleSystemRenderer>().material = await LoadAsset<Material>("RoR2/Base/Common/VFX/matInverseDistortion.mat");
			particle.transform.SetParent(theMeshObject.transform);
			return naturesAxiom;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var sunExplosion =
				(await LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandParentSunSpawn.prefab"))!.InstantiateClone(
					"TwinsSunExplosion", false);
			var sunPP = (await LoadAsset<PostProcessProfile>("RoR2/Base/Common/ppLocalVoidFogMild.asset"));
			var sunePP = sunExplosion.GetComponentInChildren<PostProcessVolume>();
			sunePP.profile = sunPP;
			sunePP.sharedProfile = sunPP;
			var suneL = sunExplosion.GetComponentInChildren<Light>();
			suneL.intensity = 100;
			suneL.range = 40;
			suneL.color = new Color(0.45f, 0, 1);
			var remapTex = (await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
			foreach (ParticleSystemRenderer r in sunExplosion.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				if (r.material)
				{
					r.material = new Material(r.material);
					r.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
					r.material.SetTexture("_RemapTex",
						await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
					r.trailMaterial = r.material;
				}
			}

			return sunExplosion;
		}

		async Task<BuffDef> IBuff.BuildObject()
		{
			var buffDef = ScriptableObject.CreateInstance<BuffDef>();
			buffDef.name = "AxiomOverheat";
			buffDef.buffColor = Colors.twinsDarkColor;
			buffDef.canStack = true;
			buffDef.isDebuff = true;
			buffDef.iconSprite = (await LoadAsset<Sprite>("RoR2/Base/Grandparent/texBuffOverheat.tif"));
			buffDef.isHidden = false;
			return buffDef;
		}

		[HarmonyPrefix,
		 HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.AddTimedBuff), typeof(BuffDef), typeof(float))]
		private static void AddTimedBuffHook(CharacterBody __instance, BuffDef buffDef, float duration)
		{
			var customOverheat = GetBuffDef<NaturesAxiom>().WaitForCompletion();
			if (buffDef != customOverheat) return;
			foreach (var timedBuff in __instance.timedBuffs)
			{
				if (timedBuff.buffIndex != customOverheat.buffIndex) continue;
				if (!(timedBuff.timer < duration)) continue;
				timedBuff.timer = duration;
				//this is making sure all stacks of the
				//buff are refreshed, which would be the opposite behaviour of Collapse
			}
		}

		public static DotController.DotIndex CurseIndex;

		public override async Task Initialize()
		{
			CurseIndex = DotAPI.RegisterDotDef(
				new DotController.DotDef
				{
					interval = 0.2f,
					damageCoefficient = 0.1f,
					damageColorIndex = DamageColorIndex.Void,
					associatedBuff = await GetBuffDef<AxiomBurn>()
				}, (self, stack) =>
				{
					if (stack.dotIndex != CurseIndex) return;
					var pos = self.victimBody.corePosition;
					//Debug.Log("A stack was added");
				}, self =>
				{
					if (!self || !self.victimObject) return;
					var modelLocator = self.victimObject.GetComponent<ModelLocator>();
					if (!modelLocator || !modelLocator.modelTransform) return;
					if (self.GetComponent<KamunagiBurnEffectController>()) return;
					var kamunagiEffectController = self.gameObject.AddComponent<KamunagiBurnEffectController>();
					kamunagiEffectController.effectParams = KamunagiBurnEffectController.defaultEffect;
					kamunagiEffectController.target = modelLocator.modelTransform.gameObject;
					log.LogDebug("added Kamunagi Controller");
				});

			UmbralSunController.activeLoopDef =
				(await LoadAsset<LoopSoundDef>("RoR2/Base/Grandparent/lsdGrandparentSunActive.asset"))!;
			UmbralSunController.damageLoopDef =
				(await LoadAsset<LoopSoundDef>("RoR2/Base/Grandparent/lsdGrandparentSunDamage.asset"))!;
			KamunagiBurnEffectController.defaultEffect = new KamunagiBurnEffectController.KamunagiEffectParams
			{
				startSound = "",
				stopSound = "",
				overlayMaterial = await GetMaterial<AxiomBurn>(),
				particleEffectPrefab = await GetEffect<CurseParticles>()
			};
		}
	}

	public class CurseParticles : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var voidFog = (await LoadAsset<GameObject>("RoR2/Base/Common/VoidFogMildEffect.prefab"));
			var effect =
				voidFog.transform.GetChild(0).gameObject!.InstantiateClone("CurseParticles", false); //revisit this
			UnityEngine.Object.Destroy(effect.transform.GetChild(2).gameObject);
			var ps = effect.GetComponentInChildren<ParticleSystem>();
			var main = ps.main;
			main.startSize = new ParticleSystem.MinMaxCurve(0.4f);

			KamunagiBurnEffectControllerHelper helper = effect.AddComponent<KamunagiBurnEffectControllerHelper>();
			helper.burnParticleSystem = ps;
			effect.GetOrAddComponent<VFXAttributes>().DoNotPool = false;
			return effect;
		}
	}

	public class KamunagiBurnEffectController : MonoBehaviour
	{
		public class KamunagiEffectParams
		{
			public string startSound;
			public string stopSound;
			public Material overlayMaterial;
			public GameObject particleEffectPrefab;
		}

		private List<KamunagiBurnEffectControllerHelper> burnEffectInstances;
		public GameObject target;
		private TemporaryOverlayInstance temporaryOverlay;
		private int soundID;
		public KamunagiEffectParams effectParams = defaultEffect;
		public static KamunagiEffectParams defaultEffect;

		private void Start()
		{
			if (effectParams == null)
			{
				Debug.LogError("KamunagiBurnEffectController on " + base.gameObject.name + " has no effect type!");
				return;
			}

			Util.PlaySound(effectParams.startSound, base.gameObject);
			burnEffectInstances = new List<KamunagiBurnEffectControllerHelper>();
			if (effectParams.overlayMaterial != null)
			{
				temporaryOverlay = TemporaryOverlayManager.AddOverlay(base.gameObject);
				temporaryOverlay.originalMaterial = effectParams.overlayMaterial;
			}

			if (!target)
			{
				return;
			}

			CharacterModel charModel = target.GetComponent<CharacterModel>();
			if (!charModel)
			{
				return;
			}

			temporaryOverlay.AddToCharacterModel(charModel);
			var body = charModel.body;
			var baseRendererInfos = charModel.baseRendererInfos;

			for (int i = 0; i < baseRendererInfos.Length; i++)
			{
				if (!baseRendererInfos[i].ignoreOverlays)
				{
					var helper = AddFireParticles(baseRendererInfos[i].renderer, body.coreTransform);
					if (helper)
					{
						burnEffectInstances.Add(helper);
					}
				}
			}
		}

		private void OnDestroy()
		{
			Util.PlaySound(effectParams.stopSound, base.gameObject);
			if (temporaryOverlay != null)
			{
				temporaryOverlay.Destroy();
			}

			for (int i = 0; i < burnEffectInstances.Count; i++)
			{
				if (burnEffectInstances[i])
				{
					burnEffectInstances[i].EndEffect();
				}
			}
		}

		private KamunagiBurnEffectControllerHelper AddFireParticles(Renderer modelRenderer,
			Transform targetParentTransform)
		{
			if (modelRenderer is not MeshRenderer && modelRenderer is not SkinnedMeshRenderer) return null;
			var particles = effectParams.particleEffectPrefab;
			EffectManagerHelper getandactivate = EffectManagerKamunagi.GetAndActivatePooledEffect(particles,
				targetParentTransform, false, new EffectData { scale = 3f });
			if (!getandactivate)
			{
				Debug.LogWarning("Could not spawn the ParticleEffect prefab: " + particles + ".");
				return null;
			}

			var kamunagiHelper = getandactivate.GetComponent<KamunagiBurnEffectControllerHelper>();
			if (!kamunagiHelper)
			{
				Debug.LogWarning("Burn effect " + particles +
				                 " doesn't have a BurnEffectControllerHelper applied.  It can't be applied.");
				getandactivate.ReturnToPool();
				return null;
			}

			kamunagiHelper.InitializeBurnEffect(modelRenderer);
			return kamunagiHelper;
		}
	}

	public class KamunagiBurnEffectControllerHelper : MonoBehaviour
	{
		public ParticleSystem burnParticleSystem;
		public DestroyOnTimer destroyOnTimer;
		public LightIntensityCurve lightIntensityCurve;
		public NormalizeParticleScale normalizeParticleScale;
		public BoneParticleController boneParticleController;

		private void Awake()
		{
			if (!burnParticleSystem)
			{
				burnParticleSystem = GetComponent<ParticleSystem>();
			}

			if (!destroyOnTimer)
			{
				destroyOnTimer = GetComponent<DestroyOnTimer>();
			}

			if (!lightIntensityCurve)
			{
				lightIntensityCurve = GetComponentInChildren<LightIntensityCurve>();
			}

			if (!normalizeParticleScale)
			{
				normalizeParticleScale = GetComponentInChildren<NormalizeParticleScale>();
			}

			if (!boneParticleController)
			{
				boneParticleController = GetComponentInChildren<BoneParticleController>();
			}
		}

		private void OnEnable()
		{
			if ((bool)lightIntensityCurve)
			{
				lightIntensityCurve.enabled = false;
			}
		}

		public void InitializeBurnEffect(Renderer modelRenderer)
		{
			if (!burnParticleSystem || !modelRenderer)
			{
				return;
			}

			ParticleSystem.ShapeModule shape = burnParticleSystem.shape;
			if (modelRenderer is MeshRenderer meshRenderer)
			{
				shape.shapeType = ParticleSystemShapeType.MeshRenderer;
				shape.meshRenderer = meshRenderer;
			}
			else if (modelRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
			{
				shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
				shape.skinnedMeshRenderer = skinnedMeshRenderer;
				if (boneParticleController)
				{
					boneParticleController.skinnedMeshRenderer = skinnedMeshRenderer;
				}
			}

			if (normalizeParticleScale)
			{
				normalizeParticleScale.UpdateParticleSystem();
			}

			burnParticleSystem.gameObject.SetActive(true);
		}

		public void EndEffect()
		{
			if (burnParticleSystem)
			{
				ParticleSystem.EmissionModule emission = burnParticleSystem.emission;
				emission.enabled = false;
			}

			if (lightIntensityCurve)
			{
				lightIntensityCurve.enabled = true;
			}
		}
	}

	[RequireComponent(typeof(TeamFilter))]
	[RequireComponent(typeof(GenericOwnership))]
	public class UmbralSunController : MonoBehaviour
	{
		private TeamFilter teamFilter;
		private GenericOwnership ownership;

		public float overheatBuffDuration = 2f;
		public float cycleInterval = 0.5f;
		public float maxDistance = 50f;
		public int minimumStacksBeforeBurning = 2;
		public float burnDuration = 1f;
		private Run.FixedTimeStamp previousCycle = Run.FixedTimeStamp.negativeInfinity;
		private int cycleIndex;
		private List<HurtBox> cycleTargets = new List<HurtBox>();
		internal BullseyeSearch bullseyeSearch = new BullseyeSearch();
		private bool isLocalPlayerDamaged;
		private uint activeSoundLoop;
		private uint damageSoundLoop;
		private BuffIndex overheatBuffDef;
		private GameObject overheatApplyEffect;
		public static LoopSoundDef activeLoopDef;
		public static LoopSoundDef damageLoopDef;
		[SerializeField] private string stopSoundName = "Play_grandParent_attack3_sun_destroy";

		private void Awake()
		{
			teamFilter = base.GetComponent<TeamFilter>();
			ownership = base.GetComponent<GenericOwnership>();
		}

		private void Start()
		{
			activeSoundLoop = AkSoundEngine.PostEvent(activeLoopDef.startSoundName, base.gameObject);
			overheatBuffDef = Concentric.GetBuffIndex<NaturesAxiom>().WaitForCompletion();
			overheatApplyEffect = Concentric.GetEffect<AxiomBurn>().WaitForCompletion();
		}

		private void OnDestroy()
		{
			AkSoundEngine.StopPlayingID(activeSoundLoop);
			//Util.PlaySound(activeLoopDef.stopSoundName, base.gameObject);
			//Util.PlaySound(damageLoopDef.stopSoundName, base.gameObject);
			Util.PlaySound(stopSoundName, base.gameObject);
		}

		private void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				ServerFixedUpdate();
			}

			bool wtf = isLocalPlayerDamaged;
			isLocalPlayerDamaged = false;
			foreach (HurtBox hurtBox in cycleTargets)
			{
				CharacterBody characterBody = null;
				if (hurtBox && hurtBox.healthComponent)
				{
					characterBody = hurtBox.healthComponent.body;
				}

				if (characterBody &&
				    (characterBody.bodyFlags & CharacterBody.BodyFlags.OverheatImmune) !=
				    CharacterBody.BodyFlags.None && characterBody.hasEffectiveAuthority)
				{
					Vector3 position = base.transform.position;
					Vector3 corePosition = characterBody.corePosition;
					RaycastHit raycastHit;
					if (!Physics.Linecast(position, corePosition, out raycastHit, LayerIndex.world.mask,
						    QueryTriggerInteraction.Ignore))
					{
						isLocalPlayerDamaged = true;
					}
				}
			}

			if (isLocalPlayerDamaged && !wtf)
			{
				//Util.PlaySound(damageLoopDef.startSoundName, base.gameObject);

				damageSoundLoop = AkSoundEngine.PostEvent(damageLoopDef.startSoundName, base.gameObject);
				return;
			}

			if (!isLocalPlayerDamaged && wtf)
			{
				//Util.PlaySound(damageLoopDef.stopSoundName, base.gameObject);

				AkSoundEngine.StopPlayingID(damageSoundLoop);
			}
		}

		private void ServerFixedUpdate()
		{
			float num = Mathf.Clamp01(previousCycle.timeSince / cycleInterval);
			int num2 = (num == 1f) ? cycleTargets.Count : Mathf.FloorToInt((float)cycleTargets.Count * num);
			Vector3 position = base.transform.position;
			while (cycleIndex < num2)
			{
				HurtBox hurtBox = cycleTargets[cycleIndex];
				if (hurtBox)
				{
					CharacterBody body = hurtBox.healthComponent.body;
					if ((body.bodyFlags & CharacterBody.BodyFlags.OverheatImmune) ==
					    CharacterBody.BodyFlags.None)
					{
						Vector3 corePosition = body.corePosition;
						Ray ray = new Ray(position, corePosition - position);
						RaycastHit raycastHit;
						if (!Physics.Linecast(position, corePosition, out raycastHit, LayerIndex.world.mask,
							    QueryTriggerInteraction.Ignore))
						{
							float num3 = Mathf.Max(1f, raycastHit.distance);
							body.AddTimedBuff(overheatBuffDef,
								overheatBuffDuration / num3); //the interesting stuff starts here
							if (overheatApplyEffect)
							{
								EffectData effectData = new EffectData
								{
									origin = corePosition,
									rotation = Util.QuaternionSafeLookRotation(-ray.direction),
									scale = body.bestFitRadius
								};
								effectData.SetHurtBoxReference(hurtBox);
								EffectManager.SpawnEffect(overheatApplyEffect, effectData, true);
							}

							int theNumber = body.GetBuffCount(overheatBuffDef) - minimumStacksBeforeBurning;
							if (theNumber > 0)
							{
								var inflictDotInfo = new InflictDotInfo();
								inflictDotInfo.dotIndex = NaturesAxiom.CurseIndex;
								inflictDotInfo.attackerObject = ownership.ownerObject;
								inflictDotInfo.victimObject = body.gameObject;
								inflictDotInfo.damageMultiplier = 1f;

								GenericOwnership genericOwnership = ownership;
								CharacterBody characterBody;
								if (genericOwnership == null)
								{
									characterBody = null;
								}
								else
								{
									GameObject ownerObject = genericOwnership.ownerObject;
									characterBody = ((ownerObject != null)
										? ownerObject.GetComponent<CharacterBody>()
										: null);
								}

								CharacterBody characterBody2 = characterBody;
								if (characterBody2 && characterBody2.inventory)
								{
									inflictDotInfo.totalDamage = 2f * characterBody2.damage * burnDuration * theNumber;
								}

								DotController.InflictDot(ref inflictDotInfo);
							}
						}
					}
				}

				cycleIndex++;
			}

			if (previousCycle.timeSince >= cycleInterval)
			{
				previousCycle = Run.FixedTimeStamp.now;
				cycleIndex = 0;
				cycleTargets.Clear();
				SearchForTargets(cycleTargets);
			}
		}

		private void SearchForTargets(List<HurtBox> dest)
		{
			bullseyeSearch.searchOrigin = transform.position;
			bullseyeSearch.minAngleFilter = 0f;
			bullseyeSearch.maxAngleFilter = 180f;
			bullseyeSearch.maxDistanceFilter = maxDistance;
			bullseyeSearch.filterByDistinctEntity = true;
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
			bullseyeSearch.viewer = null;
			bullseyeSearch.RefreshCandidates();
			dest.AddRange(bullseyeSearch.GetResults());
		}
	}

	public class AxiomBurn : Concentric, IEffect, IBuff, IMaterial
	{
		public async Task<GameObject> BuildObject()
		{
			var curseBurnFx =
				(await LoadAsset<GameObject>("RoR2/Base/GreaterWisp/GreaterWispDeath.prefab"))!.InstantiateClone(
					"TwinsCurseFx",
					false);
			curseBurnFx.transform.localScale = Vector3.one * 0.4f; //0.65
			foreach (var r in curseBurnFx.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				switch (r.name)
				{
					case "Ring":
						r.material = new Material(r.material);
						r.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
						r.material.SetTexture("_RemapTex",
							await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
						break;
					case "Chunks":
					case "Mask":
					case "Chunks, Sharp":
					case "Flames":
					case "Flash":
					case "Distortion":
						r.enabled = false;
						break;
				}
			}

			foreach (var r in curseBurnFx.GetComponentsInChildren<ParticleSystem>(false))
			{
				if (r.name != "Ring") continue;
				var main = r.main;
				main.simulationSpeed = 3.5f;
			}

			UnityEngine.Object.Destroy(curseBurnFx.GetComponent<ShakeEmitter>());
			curseBurnFx.GetComponentInChildren<Light>().color = Colors.twinsLightColor;
			return curseBurnFx;
		}

		async Task<BuffDef> IBuff.BuildObject()
		{
			var buff = ScriptableObject.CreateInstance<BuffDef>();
			buff.name = "KamunagiCurseDebuff";
			buff.iconSprite = (await LoadAsset<Sprite>("kamunagiassets:CurseScroll"));
			buff.buffColor = Color.white;
			buff.canStack = true;
			buff.isDebuff = true;
			buff.isHidden = false;
			return buff;
		}

		async Task<Material> IMaterial.BuildObject()
		{
			//this probably should use IOverlay instead?
			//nah it doesn't need to actually
			var purpleFireOverlay = new Material(await LoadAsset<Material>("RoR2/Base/BurnNearby/matOnHelfire.mat"));
			purpleFireOverlay.SetTexture("_RemapTex",
				await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
			purpleFireOverlay.SetFloat("_FresnelPower", -15.8f);
			return purpleFireOverlay;
		}
	}
}