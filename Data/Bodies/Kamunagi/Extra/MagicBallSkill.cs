using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class ShootMagicBallState : BaseTwinState
	{
		public override int meterGain => 0;
		public static GameObject viendFlash;
		private Vector3 position;

		public override void OnEnter()
		{
			base.OnEnter();
			AkSoundEngine.PostEvent("Play_voidman_m2_shoot", gameObject);
			EffectManager.SimpleMuzzleFlash(viendFlash, gameObject, twinMuzzle, false);
			if (isAuthority)
			{
				var aimRay = GetAimRay();
				ProjectileManager.instance.FireProjectile(new FireProjectileInfo
				{
					crit = false,
					damage = 0f,
					force = 500,
					owner = gameObject,
					position = aimRay.origin,
					rotation = Quaternion.LookRotation(aimRay.direction),
					projectilePrefab = Concentric.GetProjectile<MagicBallSkill>().WaitForCompletion(),
					useSpeedOverride = true,
					speedOverride = 105f
				});
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!IsKeyDownAuthority())
			{
				if (twinBehaviour.magicBall && twinBehaviour.magicBall != null)
				{
					outer.SetNextState(new MagicBallTeleportState()
					{
						ballPosition = position
					});
				}
				else
				{
					outer.SetNextStateToMain();
				}
			}
		}

		public override void OnExit()
		{
			if (NetworkServer.active && twinBehaviour.magicBall && twinBehaviour.magicBall != null) //it's called we do a little avoiding networking for now
			{
				position = twinBehaviour.magicBall.transform.position;
			}
			log.LogDebug("Exit");
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class MagicBallTeleportState : BaseTwinState
	{
		public static GameObject blinkPrefab;
		public static GameObject viendPrefab;
		private Transform modelTransform;
		private CharacterModel charModel;
		private HurtBoxGroup hurtBoxGroup;
		public Vector3 ballPosition;

		private float stopwatch;
		public float speedCoefficient = 25f;
		private bool doNothing;
		
		public override void OnEnter()
		{
			base.OnEnter();
			modelTransform = GetModelTransform();
			if (modelTransform)
			{
				charModel = modelTransform.GetComponent<CharacterModel>();
				hurtBoxGroup = modelTransform.GetComponent<HurtBoxGroup>();
				if (charModel && hurtBoxGroup)
				{
					charModel.invisibilityCount++;
					hurtBoxGroup.hurtBoxesDeactivatorCounter++;
				}
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.fixedDeltaTime;
			if (twinBehaviour.magicBall == null)
			{
				outer.SetNextStateToMain();
			}
			if (base.characterMotor && base.characterDirection)
			{
				CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
				(characterMotor as IPhysMotor).velocityAuthority = Vector3.zero;

				if (twinBehaviour.magicBall && twinBehaviour.magicBall != null)
				{
					base.characterMotor.rootMotion += ballPosition * (moveSpeedStat * speedCoefficient * GetDeltaTime());
					DoViendExplosion(twinBehaviour.magicBall.transform.position);
					EntityState.Destroy(twinBehaviour.magicBall);
				}
			}
			
			if (stopwatch >= 0.3f && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
			
			if (!outer.destroying)
			{
				modelTransform = base.GetModelTransform();
				if (modelTransform)
				{
					Material outerPurple = new Material(LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded"));
					outerPurple.SetColor("_TintColor", Colors.twinsLightColor);
					Material innerPurple = new Material(LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashBright"));
					innerPurple.SetColor("_TintColor", Colors.twinsDarkColor);
					
					TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
					temporaryOverlayInstance.duration = 0.6f;
					temporaryOverlayInstance.animateShaderAlpha = true;
					temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlayInstance.destroyComponentOnEnd = true;
					temporaryOverlayInstance.originalMaterial = innerPurple;
					temporaryOverlayInstance.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
					TemporaryOverlayInstance temporaryOverlayInstance2 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
					temporaryOverlayInstance2.duration = 0.7f;
					temporaryOverlayInstance2.animateShaderAlpha = true;
					temporaryOverlayInstance2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlayInstance2.destroyComponentOnEnd = true;
					temporaryOverlayInstance2.originalMaterial = outerPurple;
					temporaryOverlayInstance2.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
				}
			}
			if (charModel != null && charModel && hurtBoxGroup != null && hurtBoxGroup) //temporary invulnerability, even if we didn't blink
			{
				charModel.invisibilityCount--;
				hurtBoxGroup.hurtBoxesDeactivatorCounter--;
			}
			if (base.characterMotor)
			{
				base.characterMotor.disableAirControlUntilCollision = false; //Huntress blink uses this idk if we need it
			}
			base.OnExit();
		}

		private void CreateBlinkEffect(Vector3 origin)
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(Util.GetCorePosition(characterBody));
			effectData.origin = origin;
			EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
		}
		
		private void DoViendExplosion(Vector3 origin)
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(inputBank.aimDirection);
			effectData.origin = origin;
			EffectManager.SpawnEffect(viendPrefab, effectData, transmit: true);
		}
	}
	
	public class MagicBallSkill : Concentric, ISkill, IProjectile, IProjectileGhost
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			ShootMagicBallState.viendFlash  = await LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab");
			
			MagicBallTeleportState.viendPrefab = await LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterExplosion.prefab");
			MagicBallTeleportState.blinkPrefab = await LoadAsset<GameObject>("RoR2/Base/Huntress/HuntressBlinkEffect.prefab");
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 0";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA9_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA9_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("kamunagiassets:notype"));
			skill.activationStateMachineName = "Weapon";
			skill.mustKeyPress = true;
			skill.isCombatSkill = false;
			skill.hideStockCount = false;
			skill.canceledFromSprinting = false;
			skill.cancelSprintingOnActivation = true;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.dontAllowPastMaxStocks = true;
			skill.baseRechargeInterval = 4f;
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(ShootMagicBallState), typeof(MagicBallTeleportState) };

		async Task<GameObject> IProjectile.BuildObject()
		{
			var proj = (await LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab")!).InstantiateClone("TwinsBallBlinkProjectile", true);
			proj.AddComponent<TeleportToBall>();
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var ghost = (await GetProjectileGhost<SoeiMusou>())!.InstantiateClone("TwinsBallBlinkGhost", false);
			return ghost;
		}
	}
	[RequireComponent(typeof(ProjectileController))]
	public class TeleportToBall : MonoBehaviour
	{
		private ProjectileController controller;
		private CharacterBody ownerBody;
		private TwinBehaviour twinBehaviour;
		private void Start()
		{
			controller = GetComponent<ProjectileController>();
			ownerBody = controller.owner.GetComponent<CharacterBody>();
			twinBehaviour = ownerBody.GetComponent<TwinBehaviour>();
			twinBehaviour.magicBall = this.gameObject;
		}
	}
}