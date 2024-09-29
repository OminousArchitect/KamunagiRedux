using EntityStates;
using EntityStates.GrandParent;
using HarmonyLib;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;

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
			PlayAnimation("Saraana Override", "Cast2");
			PlayAnimation("Ururuu Override", "Cast2");
			animator = GetModelAnimator();
			animator.SetBool("inAxiom", true);
			spawnPos = childLocator.FindChild("MuzzleCenter").position + characterDirection.forward * 5;
			leftMuzzleInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(
				Asset.GetGameObject<LightOfNaturesAxiom, IEffect>(), childLocator.FindChild("MuzzleLeft"), true,
				new EffectData() { scale = 0.3f });
			rightMuzzleInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(
				Asset.GetGameObject<LightOfNaturesAxiom, IEffect>(), childLocator.FindChild("MuzzleRight"), true,
				new EffectData() { scale = 0.3f });
			channelSound = Util.PlaySound(ChannelSunStart.beginSoundName, gameObject);
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
			sun = UnityEngine.Object.Instantiate(Asset.GetGameObject<NaturesAxiom, INetworkedObject>(), spawnPos,
				Quaternion.identity);
			sun.GetComponent<GenericOwnership>().ownerObject = gameObject;
			sun.GetComponent<NaturesAxiom.UmbralSunController>().bullseyeSearch.teamMaskFilter =
				TeamMask.GetEnemyTeams(teamComponent.teamIndex);
			NetworkServer.Spawn(sun);
			EffectManager.SimpleEffect(Asset.GetGameObject<NaturesAxiom, IEffect>(), spawnPos, Quaternion.identity,
				true);
		}

		public override void OnExit()
		{
			base.OnExit();
			if (sun)
			{
				Destroy(sun);
				EffectManager.SimpleEffect(Asset.GetGameObject<NaturesAxiom, IEffect>(), spawnPos, Quaternion.identity,
					true);
			}
			animator.SetBool("inAxiom", false);
			if (leftMuzzleInstance != null) leftMuzzleInstance.ReturnToPool();
			if (rightMuzzleInstance != null) rightMuzzleInstance.ReturnToPool();
			AkSoundEngine.StopPlayingID(channelSound);
			if (!isAuthority) return;
			characterMotor.useGravity = true;
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;
	}

	[HarmonyPatch]
	public class LightOfNaturesAxiom : Asset, ISkill, IEffect
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Special 2";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "SPECIAL2_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "SPECIAL2_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle:RoU");
			skill.activationStateMachineName = "Body";
			skill.baseRechargeInterval = 3f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.canceledFromSprinting = true;
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = true;
			skill.keywordTokens = new[] { KamunagiAsset.tokenPrefix + "TWINSCURSE_KEYWORD" };
			return skill;
		}

		GameObject IEffect.BuildObject()
		{
			var chargeSunEffect =
				LoadAsset<GameObject>("RoR2/Base/Grandparent/ChargeGrandParentSunHands.prefab")!.InstantiateClone(
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
				LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampBottledChaos.png"));
			var sunP = chargeSunEffect.GetComponentInChildren<ParticleSystemRenderer>(true);
			//sunP[0].material = new Material(sunP[0].material);
			sunP.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
			sunP.material.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
			var lol = chargeSunEffect.GetComponentsInChildren<ParticleSystem>();
			var mainMain = lol[1].main;
			mainMain.startColor = new Color(0.45f, 0, 1);
			return chargeSunEffect;
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(LightOfNaturesAxiomState) };
	}

	public class NaturesAxiom : Asset, INetworkedObject, IEffect, IBuff
	{
		GameObject INetworkedObject.BuildObject()
		{
			var naturesAxiom =
				LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandParentSun.prefab")!.InstantiateClone("TwinsUltSun",
					true);
			UnityEngine.Object.Destroy(naturesAxiom.GetComponent<EntityStateMachine>());
			UnityEngine.Object.Destroy(naturesAxiom.GetComponent<NetworkStateMachine>());
			UnityEngine.Object.Destroy(naturesAxiom.GetComponent<GrandParentSunController>());
			naturesAxiom.AddComponent<UmbralSunController>();
			var vfxRoot = naturesAxiom.transform.GetChild(0).gameObject;
			vfxRoot.transform.localScale = Vector3.one * 0.5f;
			var sunL = naturesAxiom.GetComponentInChildren<Light>();
			sunL.intensity = 100;
			sunL.range = 70;
			sunL.color = new Color(0.45f, 0, 1);
			var sunMeshes = naturesAxiom.GetComponentsInChildren<MeshRenderer>(true);
			var sunIndicator = sunMeshes[0];
			sunIndicator.material = new Material(sunIndicator.material);
			sunIndicator.material.SetTexture("_RemapTex",
				LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampPortalVoid.png"));
			sunIndicator.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
			sunIndicator.transform.localScale = Vector3.one * 85f; //visual indicator
			var Sunmesh2 = sunMeshes[1];
			Sunmesh2.material = GetGameObject<LightOfNaturesAxiom, IEffect>().GetComponentInChildren<MeshRenderer>(true)
				.material;
			sunMeshes[2].enabled = false;
			var sunPP = naturesAxiom.GetComponentInChildren<PostProcessVolume>();
			sunPP.profile = LoadAsset<PostProcessProfile>("RoR2/Base/Common/ppLocalVoidFogMild.asset");
			sunPP.sharedProfile = sunPP.profile;
			sunPP.gameObject.AddComponent<SphereCollider>().radius = 40;
			var sunP = GetGameObject<NaturesAxiom, IEffect>().GetComponentInChildren<ParticleSystemRenderer>(true);
			var destroyed = naturesAxiom.transform.Find("VfxRoot/Particles/GlowParticles, Fast").gameObject;
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
							new Material(LoadAsset<Material>("RoR2/Junk/Common/VFX/matTeleportOutBodyGlow.mat"));
						r.material.SetColor("_TintColor", new Color(0f, 0.4F, 1)); //todo example of new Material()
						r.transform.localScale = Vector3.one * 0.5f;
						break;
					case "Donut":
					case "Trails":
						var material = new Material(r.material);
						material.SetColor("_TintColor", new Color(0.45f, 0, 1));
						material.SetTexture("_RemapTex",
							LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
						r.trailMaterial = material;
						r.material = material;
						break;
					case "Goo, Drip":
					case "Sparks":
						r.enabled = false;
						break;
				}
			}

			return naturesAxiom;
		}

		GameObject IEffect.BuildObject()
		{
			var sunExplosion =
				LoadAsset<GameObject>("RoR2/Base/Grandparent/GrandParentSunSpawn.prefab")!.InstantiateClone(
					"TwinsSunExplosion", false);
			var sunPP = LoadAsset<PostProcessProfile>("RoR2/Base/Common/ppLocalVoidFogMild.asset");
			var sunePP = sunExplosion.GetComponentInChildren<PostProcessVolume>();
			sunePP.profile = sunPP;
			sunePP.sharedProfile = sunPP;
			var suneL = sunExplosion.GetComponentInChildren<Light>();
			suneL.intensity = 100;
			suneL.range = 40;
			suneL.color = new Color(0.45f, 0, 1);
			var remapTex = LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png");
			foreach (ParticleSystemRenderer r in sunExplosion.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				if (r.material)
				{
					r.material = new Material(r.material);
					r.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
					r.material.SetTexture("_RemapTex",
						LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
					r.trailMaterial = r.material;
				}
			}

			return sunExplosion;
		}

		BuffDef IBuff.BuildObject()
		{
			var buffDef = ScriptableObject.CreateInstance<BuffDef>();
			buffDef.name = "AxiomOverheat";
			buffDef.buffColor = Colors.twinsDarkColor;
			buffDef.canStack = true;
			buffDef.isDebuff = true;
			buffDef.iconSprite = LoadAsset<Sprite>("RoR2/Base/Grandparent/texBuffOverheat.tif");
			buffDef.isHidden = false;
			return buffDef;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.AddTimedBuff), typeof(BuffDef), typeof(float))]
		private static void AddTimedBuffHook(CharacterBody __instance, BuffDef buffDef, float duration)
		{
			if (!TryGetAsset<NaturesAxiom, IBuff>(out var customOverheat)) return;
			var overheatDef = (BuffDef)customOverheat;
			if (buffDef != overheatDef) return;
			foreach (var timedBuff in __instance.timedBuffs)
			{
				if (timedBuff.buffIndex != overheatDef.buffIndex) continue;
				if (!(timedBuff.timer < duration)) continue;
				timedBuff.timer = duration;
				//this is making sure all stacks of the
				//buff are refreshed, which would be the opposite behaviour of Collapse
			}
		}

		public static DotController.DotIndex CurseIndex;

		public override void Initialize()
		{
			CurseIndex = DotAPI.RegisterDotDef(
				new DotController.DotDef
				{
					interval = 0.2f,
					damageCoefficient = 0.1f,
					damageColorIndex = DamageColorIndex.Void,
					associatedBuff = (BuffDef)GetAsset<AxiomBurn, IBuff>()
				}, (self, stack) =>
				{
					if (stack.dotIndex != CurseIndex) return;
					var pos = self.victimBody.corePosition;
					Debug.Log("A stack was added");
				}, self =>
				{
					if (!self || !self.victimObject) return;
					var modelLocator = self.victimObject.GetComponent<ModelLocator>();
					if (!modelLocator || !modelLocator.modelTransform) return;
					if (self.GetComponent<KamunagiBurnEffectController>()) return;
					var kamunagiEffectController = self.gameObject.AddComponent<KamunagiBurnEffectController>();
					kamunagiEffectController.effectParams = KamunagiBurnEffectController.defaultEffect;
					kamunagiEffectController.target = modelLocator.modelTransform.gameObject;
					Debug.LogWarning("added Kamunagi Controller");
				});
		}

		public class CurseParticles : Asset, IEffect
		{
			GameObject IEffect.BuildObject()
			{
				var effect = LoadAsset<GameObject>("RoR2/Base/Common/VoidFogMildEffect.prefab")!.InstantiateClone("CurseParticles", false);
				effect.transform.position = new Vector3(0f, 1f, 0f);
				var timer = effect.AddComponent<DestroyOnTimer>();
				timer.age = 2f;
				var helperControler = effect.AddComponent<BurnEffectControllerHelper>();
				helperControler.burnParticleSystem = effect.GetComponent<ParticleSystem>();
				helperControler.destroyOnTimer = timer;
				UnityEngine.Object.Destroy(effect.GetComponent<TemporaryVisualEffect>());
				UnityEngine.Object.Destroy(effect.GetComponent<EffectComponent>());
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

			private List<BurnEffectControllerHelper> burnEffectInstances;
			public GameObject target;
			private TemporaryOverlayInstance temporaryOverlay;
			private int soundID;
			public KamunagiEffectParams effectParams = defaultEffect;
			public static KamunagiEffectParams defaultEffect;
			public float fireParticleSize = 5f;
			
			private void Awake()
			{
				defaultEffect = new KamunagiEffectParams
				{
					startSound = "",
					stopSound = "",
					overlayMaterial = GetAsset<AxiomBurn, IMaterial>(),
					particleEffectPrefab = GetGameObject<CurseParticles, IEffect>()
				};
			}

			private void Start()
			{
				if (effectParams == null)
				{
					Debug.LogError("KamunagiBurnEffectController on " + base.gameObject.name + " has no effect type!");
					return;
				}

				Util.PlaySound(effectParams.startSound, base.gameObject);
				burnEffectInstances = new List<BurnEffectControllerHelper>();
				if (effectParams.overlayMaterial != null)
				{
					temporaryOverlay = TemporaryOverlayManager.AddOverlay(base.gameObject);
					temporaryOverlay.originalMaterial = effectParams.overlayMaterial;
				}

				if (!target)
				{
					return;
				}

				CharacterModel component = target.GetComponent<CharacterModel>();
				if (!component)
				{
					return;
				}

				if (temporaryOverlay != null)
				{
					temporaryOverlay.AddToCharacterModel(component);
				}

				CharacterBody body = component.body;
				CharacterModel.RendererInfo[] baseRendererInfos = component.baseRendererInfos;
				if (!body)
				{
					return;
				}

				for (int i = 0; i < baseRendererInfos.Length; i++)
				{
					if (!baseRendererInfos[i].ignoreOverlays)
					{
						BurnEffectControllerHelper burnEffectControllerHelper =
							AddFireParticles(baseRendererInfos[i].renderer, body.coreTransform);
						if ((bool)burnEffectControllerHelper)
						{
							burnEffectInstances.Add(burnEffectControllerHelper);
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
					if ((bool)burnEffectInstances[i])
					{
						burnEffectInstances[i].EndEffect();
					}
				}
			}

			private BurnEffectControllerHelper AddFireParticles(Renderer modelRenderer, Transform targetParentTransform)
			{
				if (modelRenderer is MeshRenderer || modelRenderer is SkinnedMeshRenderer)
				{
					GameObject fireEffectPrefab = effectParams.particleEffectPrefab;
					EffectManagerHelper andActivatePooledEffect =
						EffectManager.GetAndActivatePooledEffect(fireEffectPrefab, targetParentTransform);
					if (!andActivatePooledEffect)
					{
						Debug.LogWarning("Could not spawn the ParticleEffect prefab: " +
						                 ((object)fireEffectPrefab)?.ToString() + ".");
						return null;
					}

					BurnEffectControllerHelper component =
						andActivatePooledEffect.GetComponent<BurnEffectControllerHelper>();
					if (!component)
					{
						Debug.LogWarning("Burn effect " + ((object)fireEffectPrefab)?.ToString() +
						                 " doesn't have a BurnEffectControllerHelper applied.  It can't be applied.");
						andActivatePooledEffect.ReturnToPool();
						return null;
					}

					component.InitializeBurnEffect(modelRenderer);
					return component;
				}

				return null;
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
				public float maxDistance = 75f;
				public int minimumStacksBeforeBurning = 2;
				public float burnDuration = 1f;

				[SerializeField] private LoopSoundDef activeLoopDef =
					LoadAsset<LoopSoundDef>("RoR2/Base/Grandparent/lsdGrandparentSunActive.asset")!;

				[SerializeField] private LoopSoundDef damageLoopDef =
					LoadAsset<LoopSoundDef>("RoR2/Base/Grandparent/lsdGrandparentSunDamage.asset")!;

				[SerializeField] private string stopSoundName = "Play_grandParent_attack3_sun_destroy";

				private Run.FixedTimeStamp previousCycle = Run.FixedTimeStamp.negativeInfinity;
				private int cycleIndex;
				private List<HurtBox> cycleTargets = new List<HurtBox>();
				internal BullseyeSearch bullseyeSearch = new BullseyeSearch();
				private bool isLocalPlayerDamaged;
				private uint activeSoundLoop;
				private uint damageSoundLoop;
				private BuffIndex overheatBuffDef;
				private GameObject overheatApplyEffect;

				private void Awake()
				{
					teamFilter = base.GetComponent<TeamFilter>();
					ownership = base.GetComponent<GenericOwnership>();
				}

				private void Start()
				{
					activeSoundLoop = AkSoundEngine.PostEvent(activeLoopDef.startSoundName, base.gameObject);
					overheatBuffDef = GetAsset<NaturesAxiom, IBuff>();
					overheatApplyEffect = GetGameObject<AxiomBurn, IEffect>();
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
											inflictDotInfo.totalDamage =
												0.5f * characterBody2.damage * burnDuration * theNumber;
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

			public class AxiomBurn : Asset, IEffect, IBuff, IMaterial
		{
			public GameObject BuildObject()
			{
				var curseBurnFx =
					LoadAsset<GameObject>("RoR2/Base/GreaterWisp/GreaterWispDeath.prefab")!.InstantiateClone(
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
								LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
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

			BuffDef IBuff.BuildObject()
			{
				var buff = ScriptableObject.CreateInstance<BuffDef>();
				buff.name = "KamunagiCurseDebuff";
				buff.iconSprite = LoadAsset<Sprite>("bundle:CurseScroll");
				buff.buffColor = Color.white;
				buff.canStack = true;
				buff.isDebuff = true;
				buff.isHidden = false;
				return buff;
			}

			Material IMaterial.BuildObject()
			{
				//this probably should use IOverlay instead?
				//nah it doesn't need to actually
				var purpleFireOverlay = new Material(LoadAsset<Material>("RoR2/Base/BurnNearby/matOnHelfire.mat"));
				purpleFireOverlay.SetTexture("_RemapTex",
					LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
				purpleFireOverlay.SetFloat("_FresnelPower", -15.8f);
				return purpleFireOverlay;
			}
		}
	}
}