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
					position = twinBehaviour.magicBall.transform.position;
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
		private Vector3 currentPosition;
		private static Vector3 storedPosition;
		private float duration;
		private bool blinked;

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
				CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
			}
			if (isAuthority)
			{
				currentPosition = characterBody.corePosition;
				storedPosition = ballPosition;
				duration = (currentPosition - storedPosition).magnitude;
				duration = Util.Remap(duration, 0f, 100f, 0.2f, 0.6f);
				characterDirection.forward = storedPosition;
			}
			if (twinBehaviour.magicBall && twinBehaviour.magicBall != null)
			{
				DoViendExplosion(twinBehaviour.magicBall.transform.position);
				EntityState.Destroy(twinBehaviour.magicBall);
			}
		}

		private void SetPosition(Vector3 newPosition)
		{
			if (characterMotor)
			{
				characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
			}
		}
		
		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (base.characterMotor && base.characterDirection)
			{
				(characterMotor as IPhysMotor).velocityAuthority = Vector3.zero;

				if (!blinked)
				{
					SetPosition(Vector3.Lerp(currentPosition, storedPosition, base.fixedAge / this.duration));
				}
			}
			
			if (fixedAge >= duration && base.isAuthority)
			{
				blinked = true;
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

		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write(ballPosition);
			writer.Write(currentPosition);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			ballPosition = reader.ReadVector3();
			currentPosition = reader.ReadVector3();
		}
	}
	
	public class MagicBallSkill : Concentric, ISkill, IProjectile, IProjectileGhost, IEffect
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			ShootMagicBallState.viendFlash  = await LoadAsset<GameObject>("addressable:RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab");

			MagicBallTeleportState.viendPrefab = await this.GetEffect();
			MagicBallTeleportState.blinkPrefab = await LoadAsset<GameObject>("RoR2/Base/Huntress/HuntressBlinkEffect.prefab");
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Primary 0";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA9_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA9_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("kamunagiassets2:WaterSeal"));
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
			var proj = (await GetProjectile<AltMusouChargeBall>())!.InstantiateClone("TwinsMagicBall", true);
			proj.AddComponent<TeleportToBall>();
			proj.GetComponent<ProjectileController>().ghostPrefab = await this.GetProjectileGhost();
			var impact = proj.GetComponent<ProjectileImpactExplosion>();
			impact.impactEffect = await this.GetEffect();
			return proj;
		}

		async Task<GameObject> IProjectileGhost.BuildObject()
		{
			var musouInstance = new Material(await LoadAsset<Material>("addressable:RoR2/Base/Brother/matBrotherPreBossSphere.mat"));
			musouInstance.SetTexture("_RemapTex", await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampMoonPreBoss.png"));
			musouInstance.SetColor("_TintColor", new Color32(0, 79, 255, 255));
			
			var ghost = (await GetProjectileGhost<AltMusouChargeBall>())!.InstantiateClone("TwinsMagicBallGhost", false);
			ghost.transform.localScale = Vector3.one * 0.5f;
			var coolSphere = ghost.GetComponent<MeshRenderer>();
			coolSphere.materials = new[] { musouInstance };
			UnityEngine.Object.Destroy(ghost.GetComponent<ObjectScaleCurve>());
			return ghost;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			Material newMat = new Material(await LoadAsset<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterSphereAreaIndicator.mat"));
			newMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning.png"));
			Material newMat2 = new Material(await LoadAsset<Material>("RoR2/DLC1/Common/Void/matOmniHitspark2Void.mat"));
			newMat2.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
			
			var effect = (await LoadAsset<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterExplosion.prefab"))!.InstantiateClone("MagicBallImpact", false);
			var recolor = effect.transform.GetChild(2).gameObject;
			recolor.GetComponent<ParticleSystemRenderer>().material = newMat;
			var core = effect.transform.GetChild(6).gameObject;
			foreach (ParticleSystemRenderer r in core.GetComponentsInChildren<ParticleSystemRenderer>())
			{
				switch (r.name)
				{
					case "ScaledHitsparks 1":
					case "ScaledHitsparks 2":
						r.material = newMat2;
						break;
				}
			}
			effect.GetComponentInChildren<Light>().color = Colors.oceanColor;
			UnityEngine.Object.Destroy(effect.transform.GetChild(0).gameObject);
			UnityEngine.Object.Destroy(effect.transform.GetChild(1).gameObject);
			return effect;
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