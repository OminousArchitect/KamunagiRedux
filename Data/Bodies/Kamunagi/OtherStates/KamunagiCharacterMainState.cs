using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Extra;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates
{
	public class KamunagiCharacterMainState : GenericCharacterMain
	{
		public GenericSkill passiveSkill;
		public EntityStateMachine hoverStateMachine;
		public static GameObject chainsEffect = Asset.GetEffect<KamunagiAsset>().WaitForCompletion();
		public Transform UBone;
		public Transform SBone;
		public EffectManagerHelper? chainsLeftInstance;
		public EffectManagerHelper? chainsRightInstance;
		public bool chainsSpawned;
		public SceneDef? currentStage;

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

		public override void OnExit() {
			base.OnExit();
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
			var sulfurPoolsDef = LoadAsset<SceneDef>("RoR2/DLC1/sulfurpools/sulfurpools.asset");
			var meridianDef = LoadAsset<SceneDef>("RoR2/DLC2/meridian/meridian.asset");
			if (currentStage == meridianDef || currentStage == sulfurPoolsDef) return;
			if (!chainsSpawned && passiveSkill.IsReady())
			{
				if (chainsLeftInstance == null || !chainsLeftInstance)
					chainsLeftInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(chainsEffect, UBone,
					data: new EffectData() { rootObject = UBone.gameObject, });
				if (chainsRightInstance == null || !chainsRightInstance)
					chainsRightInstance = EffectManagerKamunagi.GetAndActivatePooledEffect(chainsEffect, SBone,
						data: new EffectData() { rootObject = SBone.gameObject });
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
		}

		public override void ProcessJump()
		{
			base.ProcessJump();
			if (characterMotor.isGrounded)
			{
				base.ProcessJump();
			}
			else
			{
				if (hasInputBank)
				{
					if (inputBank.jump.justPressed && (characterMotor as IPhysMotor).velocity.y <= -14f && passiveSkill.ExecuteIfReady()) //ascension threshold
					{
						return;
					}

					if (inputBank.jump.down && (characterMotor as IPhysMotor).velocity.y <= 0)
						hoverStateMachine.SetInterruptState(new KamunagiHoverState(), InterruptPriority.Any);
				}
				base.ProcessJump();
			}
		}
	}

	public class KamunagiHoverState : BaseState, IZealState
	{
		public float hoverVelocity = -0.02f; //below negative increases downard velocity, so increase towards positive numbers to hover longer
		public float hoverAcceleration = 80;
		public static GameObject muzzleEffect = Asset.GetEffect<KamunagiHover>().WaitForCompletion();
		private EffectManagerHelper muzzleInstanceLeft;
		private EffectManagerHelper muzzleInstanceRight;

		public override void OnEnter()
		{
			base.OnEnter();
			muzzleInstanceLeft =
				EffectManagerKamunagi.GetAndActivatePooledEffect(muzzleEffect,
					new EffectData() { rootObject = FindModelChild("MuzzleLeft").gameObject });
			muzzleInstanceRight =
				EffectManagerKamunagi.GetAndActivatePooledEffect(muzzleEffect,
					new EffectData() { rootObject = FindModelChild("MuzzleRight").gameObject });
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

			if (isGrounded || !IsKeyDownAuthority())
			{
				outer.SetNextStateToMain();
				return;
			}

			if (!isAuthority) return;
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

		public bool IsKeyDownAuthority() => inputBank.jump.down;

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class KamunagiHover : Asset, IEntityStates, IEffect
	{
		public IEnumerable<Type> GetEntityStates() => new[] { typeof(KamunagiHoverState) };

		GameObject IEffect.BuildObject()
		{
			var mashiroEffect = GetGameObject<MashiroBlessing, IEffect>()!.InstantiateClone("mashiroEffect", false);
			var electricOrbPink = LoadAsset<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbGhost.prefab")!.InstantiateClone("TwinsPinkHandEnergy", false);
			mashiroEffect.transform.SetParent(electricOrbPink.transform);
			electricOrbPink.AddComponent<ModelAttachedEffect>();
			UnityEngine.Object.Destroy(electricOrbPink.GetComponent<ProjectileGhostController>());
			UnityEngine.Object.Destroy(electricOrbPink.GetComponent<VFXAttributes>());
			var pinkChild = electricOrbPink.transform.GetChild(0);
			pinkChild.transform.localScale = Vector3.one * 0.1f;
			var pinkTransform = pinkChild.transform.GetChild(0);
			pinkTransform.transform.localScale = Vector3.one * 0.25f;
			var pink = new Color(1f, 0f, 0.34f);
			var pinkAdditive = new Color(0.91f, 0.3f, 0.84f);
			foreach (var r in electricOrbPink.GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				var name = r.name;
				if (name != "SpitCore") continue;
				r.material.SetTexture("_RemapTex",
					LoadAsset<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampVoidRing.png"));
				r.material.SetFloat("_AlphaBoost", 3.2f);
				r.material.SetColor("_TintColor", pinkAdditive);
			}

			var pinkTrails = electricOrbPink.GetComponentsInChildren<TrailRenderer>();
			pinkTrails[0].material.SetColor("_TintColor", pink);
			pinkTrails[1].material.SetColor("_TintColor", pink);
			electricOrbPink.GetComponentInChildren<Light>().color = Colors.twinsDarkColor;
			electricOrbPink.SetActive(false);
			return electricOrbPink;
		}
	}
}