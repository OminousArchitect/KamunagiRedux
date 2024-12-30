using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class BallTeleportState : BaseTwinState
	{
		public override int meterGain => 0;
		public static GameObject MuzzlePrefab;

		public override void OnEnter()
		{
			base.OnEnter();
			AkSoundEngine.PostEvent("Play_voidman_m2_shoot", gameObject);
			EffectManager.SimpleMuzzleFlash(MuzzlePrefab, gameObject, twinMuzzle, false);
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
					projectilePrefab = Concentric.GetProjectile<BallTeleport>().WaitForCompletion(),
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
				outer.SetNextStateToMain();	
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}
	
	#region whoKnows
	/*public class TwinsBlink : BaseState
	{
		private Transform modelTransform;
		public static GameObject blinkPrefab;
		private float stopwatch;
		private Vector3 blinkVector = Vector3.zero;
		public float duration = 0.3f;
		public float speedCoefficient = 25f;
		public string beginSoundString;
		public string endSoundString;
		private CharacterModel characterModel;
		private HurtBoxGroup hurtboxGroup;

		public override void OnEnter()
		{
			base.OnEnter();
			Util.PlaySound(beginSoundString, base.gameObject);
			modelTransform = GetModelTransform();
			if ((bool)modelTransform)
			{
				characterModel = modelTransform.GetComponent<CharacterModel>();
				hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
			}

			if ((bool)characterModel)
			{
				characterModel.invisibilityCount++;
			}

			if ((bool)hurtboxGroup)
			{
				HurtBoxGroup hurtBoxGroup = hurtboxGroup;
				int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
				hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}

			blinkVector = GetBlinkVector();
			CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		}

		protected virtual Vector3 GetBlinkVector()
		{
			return base.inputBank.aimDirection;
		}

		private void CreateBlinkEffect(Vector3 origin)
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(blinkVector);
			effectData.origin = origin;
			EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += GetDeltaTime();
			if ((bool)base.characterMotor && (bool)base.characterDirection)
			{
				base.characterMotor.velocity = Vector3.zero;
				base.characterMotor.rootMotion += blinkVector * (moveSpeedStat * speedCoefficient * GetDeltaTime());
			}

			if (stopwatch >= duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			if (!outer.destroying)
			{
				Util.PlaySound(endSoundString, base.gameObject);
				CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
				modelTransform = GetModelTransform();
				if ((bool)modelTransform)
				{
					TemporaryOverlayInstance temporaryOverlayInstance =
						TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
					temporaryOverlayInstance.duration = 0.6f;
					temporaryOverlayInstance.animateShaderAlpha = true;
					temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlayInstance.destroyComponentOnEnd = true;
					temporaryOverlayInstance.originalMaterial =
						LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashBright");
					temporaryOverlayInstance.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
					TemporaryOverlayInstance temporaryOverlayInstance2 =
						TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
					temporaryOverlayInstance2.duration = 0.7f;
					temporaryOverlayInstance2.animateShaderAlpha = true;
					temporaryOverlayInstance2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlayInstance2.destroyComponentOnEnd = true;
					temporaryOverlayInstance2.originalMaterial =
						LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded");
					temporaryOverlayInstance2.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
				}
			}

			if ((bool)characterModel)
			{
				characterModel.invisibilityCount--;
			}

			if ((bool)hurtboxGroup)
			{
				HurtBoxGroup hurtBoxGroup = hurtboxGroup;
				int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
				hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}

			if ((bool)base.characterMotor)
			{
				base.characterMotor.disableAirControlUntilCollision = false;
			}

			base.OnExit();
		}
	}*/
	#endregion whoKnows
	
	public class BallTeleport : Concentric, ISkill, IProjectile, IProjectileGhost
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			BallTeleportState.MuzzlePrefab  = await (LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab"))!;
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
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(BallTeleportState) };

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