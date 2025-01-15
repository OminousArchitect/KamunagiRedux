using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Extra;
using KamunagiOfChains.Data.Bodies.Kamunagi.Passive;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
	public class KamunagiCharacterMainState : GenericCharacterMain
	{
		public GenericSkill passiveSkill;
		public EntityStateMachine hoverStateMachine;
		public static GameObject chainsEffect;
		public Transform UBone;
		public Transform SBone;
		public EffectManagerHelper? chainsLeftInstance;
		public EffectManagerHelper? chainsRightInstance;
		public bool chainsSpawned;
		public SceneDef? currentStage;
		public static SceneDef meridianDef;
		public static SceneDef sulfurPoolsDef;
		private bool _chainsPrimed;

		public bool chainsPrimed
		{
			get
			{
				return _chainsPrimed;
			}
			set
			{
				if (value != _chainsPrimed)
				{
					if (value)
					{
						if (chainsLeftInstance != null || chainsLeftInstance)
						{
							chainsLeftInstance!.GetComponent<Renderer>().material.color = Colors.twinsLightColor;
						}

						if (chainsRightInstance != null || chainsRightInstance)
						{
							chainsRightInstance!.GetComponent<Renderer>().material.color = Colors.twinsLightColor;
						}
					}
					else
					{
						if (chainsLeftInstance != null || chainsLeftInstance)
						{
							chainsLeftInstance!.GetComponent<Renderer>().material.color = Colors.twinsDarkColor;
						}

						if (chainsRightInstance != null || chainsRightInstance)
						{
							chainsRightInstance!.GetComponent<Renderer>().material.color = Colors.twinsDarkColor;
						}
					}

					_chainsPrimed = value;
				}
			}
		}

		public override void OnEnter()
		{
			base.OnEnter();
			hoverStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Hover");
			passiveSkill = skillLocator.FindSkill("AscensionPassive");
			var childLocator = GetModelChildLocator();
			UBone = childLocator.FindChild("U Bone");
			SBone = childLocator.FindChild("S Bone");
			currentStage = SceneCatalog.GetSceneDefForCurrentScene();
		}

		public override void OnExit()
		{
			base.OnExit();
			chainsPrimed = false;
			if (chainsLeftInstance != null || chainsLeftInstance)
			{
				chainsLeftInstance!.ReturnToPool();
				chainsLeftInstance = null;
			}

			if (chainsRightInstance != null || chainsRightInstance)
			{
				chainsRightInstance!.ReturnToPool();
				chainsRightInstance = null;
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!chainsSpawned && passiveSkill.IsReady())
			{
				if (chainsLeftInstance == null || !chainsLeftInstance)
					chainsLeftInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(chainsEffect, UBone,
						data: new EffectData() { rootObject = UBone.gameObject, });
				if (chainsRightInstance == null || !chainsRightInstance)
					chainsRightInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(chainsEffect, SBone,
						data: new EffectData() { rootObject = SBone.gameObject });
				chainsPrimed = false;
				chainsSpawned = true;
			}
			else if (chainsSpawned && !passiveSkill.IsReady())
			{
				if (chainsLeftInstance != null || chainsLeftInstance)
				{
					chainsLeftInstance!.ReturnToPool();
					chainsLeftInstance = null;
				}

				if (chainsRightInstance != null || chainsRightInstance)
				{
					chainsRightInstance!.ReturnToPool();
					chainsRightInstance = null;
				}

				chainsSpawned = false;
			}

			if (!characterMotor.isGrounded)
			{
				chainsPrimed = true;
				if (inputBank.interact.justPressed && passiveSkill.ExecuteIfReady())
				{
					chainsPrimed = false;
				}
			}
		}

		public override void ProcessJump()
		{
			if (characterMotor.isGrounded)
			{
				base.ProcessJump();
			}
			else
			{
				if (hasInputBank)
				{
					if (inputBank.jump.down && (characterMotor as IPhysMotor).velocity.y <= 0)
						hoverStateMachine.SetInterruptState(new KamunagiHoverState(), InterruptPriority.Any);
				}

				base.ProcessJump();
			}
		}
	}

	public class KamunagiHoverState : BaseState, IZealState
	{
		public float
			hoverVelocity =
				0.02f; //below negative increases downard velocity, so increase towards positive numbers to hover longer

		public float hoverAcceleration = 80;
		public static GameObject muzzleEffect;
		private EffectManagerHelper muzzleInstanceLeft;
		private EffectManagerHelper muzzleInstanceRight;
		private UserProfile user;
		private bool exiting;

		public override void OnEnter()
		{
			base.OnEnter();
			muzzleInstanceLeft =
				EffectManagerKamunagi.GetAndActivatePooledEffect(muzzleEffect,
					new EffectData() { rootObject = FindModelChild("MuzzleLeft").gameObject });
			muzzleInstanceRight =
				EffectManagerKamunagi.GetAndActivatePooledEffect(muzzleEffect,
					new EffectData() { rootObject = FindModelChild("MuzzleRight").gameObject });
			user = NetworkUser.readOnlyLocalPlayersList[0].localUser.userProfile;
		}

		public override void OnExit()
		{
			base.OnExit();
			if (muzzleInstanceLeft != null) muzzleInstanceLeft.ReturnToPool();
			if (muzzleInstanceRight != null) muzzleInstanceRight.ReturnToPool();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (!isAuthority) return;

			if (user.toggleArtificerHover && IsKeyDownAuthority() && fixedAge > 0.2f)
			{
				exiting = true;
				return;
			}

			if (isGrounded || exiting && !IsKeyDownAuthority() || !user.toggleArtificerHover && !IsKeyDownAuthority())
			{
				outer.SetNextStateToMain();
				return;
			}

			var motor = characterMotor as IPhysMotor;
			motor.velocityAuthority = new Vector3(
				motor.velocityAuthority.x,
				Mathf.MoveTowards(
					motor.velocityAuthority.y,
					hoverVelocity,
					hoverAcceleration * Time.deltaTime
				),
				motor.velocityAuthority.z
			);
		}

		public int meterGain => 0;

		private bool IsKeyDownAuthority() => inputBank.jump.down;

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class KamunagiHover : Concentric, IEntityStates, IEffect
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			KamunagiHoverState.muzzleEffect = await this.GetEffect();
			KamunagiCharacterMainState.chainsEffect = await GetEffect<KamunagiAsset>();
			KamunagiCharacterMainState.meridianDef = await LoadAsset<SceneDef>("RoR2/DLC2/meridian/meridian.asset");
			KamunagiCharacterMainState.sulfurPoolsDef =
				await LoadAsset<SceneDef>("RoR2/DLC1/sulfurpools/sulfurpools.asset");
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(KamunagiHoverState) };

		async Task<GameObject> IEffect.BuildObject()
		{
			var hoverFlames = (await GetEffect<ShadowflameMuzzle>())!.InstantiateClone("TwinsHoverFlames", false);
			UnityEngine.Object.Destroy(hoverFlames.GetComponent<EffectComponent>());
			var doubleHoverFx =
				(await LoadAsset<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbGhost.prefab"))!.InstantiateClone(
					"TwinsPinkHandEnergy", false);
			hoverFlames.transform.SetParent(doubleHoverFx.transform);
			doubleHoverFx.AddComponent<ModelAttachedEffect>();
			UnityEngine.Object.Destroy(doubleHoverFx.GetComponent<ProjectileGhostController>());
			UnityEngine.Object.Destroy(doubleHoverFx.GetComponent<VFXAttributes>());
			var pinkChild = doubleHoverFx.transform.GetChild(0);
			pinkChild.transform.localScale = Vector3.one * 0.1f;
			var pinkTransform = pinkChild.transform.GetChild(0);
			pinkTransform.transform.localScale = Vector3.one * 0.25f;
			var pink = new Color(1f, 0f, 0.34f);
			var pinkAdditive = new Color(0.91f, 0.3f, 0.84f);
			foreach (var r in doubleHoverFx.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				var name = r.name;
				if (name != "SpitCore") continue;
				r.material.SetTexture("_RemapTex",
					await LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampVoidRing.png"));
				r.material.SetFloat("_AlphaBoost", 3.2f);
				r.material.SetColor("_TintColor", pinkAdditive);
			}

			var pinkTrails = doubleHoverFx.GetComponentsInChildren<TrailRenderer>();
			pinkTrails[0].material.SetColor("_TintColor", pink);
			pinkTrails[1].material.SetColor("_TintColor", pink);
			doubleHoverFx.GetComponentInChildren<Light>().color = Colors.twinsDarkColor;
			doubleHoverFx.SetActive(false);
			doubleHoverFx.EffectWithSound("");
			return doubleHoverFx;
		}
	}

	public class ShadowflameMuzzle : Concentric, IEffect
	{
		async Task<GameObject> IEffect.BuildObject()
		{
			var effect = await LoadAsset<GameObject>("kamunagiassets:ShadowFlame.prefab")!;
			var vfx = effect.AddComponent<VFXAttributes>();
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;
			vfx.DoNotPool = false;
			effect.transform.localPosition = Vector3.zero;
			effect.transform.localScale = Vector3.one * 0.6f;
			return effect;
		}
	}
}