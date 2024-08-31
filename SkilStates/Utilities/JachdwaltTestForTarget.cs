using System.Linq;
using EntityStates;
using EntityStates.Merc;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using Console = System.Console;

namespace Kamunagi
{
	class JachdwaltTestForTarget : BaseTwinState
	{
		public bool hitSomething;
		public Vector3 hitPos;
		public override void OnEnter()
		{
			base.OnEnter();
			Shoot();
		}

		void Shoot()
		{
			Ray aimRay = GetAimRay();
			BulletAttack bullet = new BulletAttack();
			bullet.maxDistance = 1000;
			bullet.stopperMask = LayerIndex.entityPrecise.mask | LayerIndex.world.mask;
			bullet.owner = base.gameObject;
			bullet.weapon = base.gameObject;
			bullet.origin = aimRay.origin;
			bullet.aimVector = aimRay.direction;
			bullet.minSpread = 0;
			bullet.maxSpread = 0.4f;
			bullet.bulletCount = 1U;
			bullet.damage = base.damageStat * 1f;
			bullet.force = 155;
			bullet.tracerEffectPrefab = null;
			bullet.muzzleName = muzzleString;
			bullet.hitEffectPrefab = null;
			bullet.isCrit = base.RollCrit();
			bullet.radius = 1.5f;
			bullet.procCoefficient = 0.4f;
			bullet.smartCollision = true;
			bullet.falloffModel = BulletAttack.FalloffModel.None;

			bullet.hitCallback = delegate(BulletAttack attack, ref BulletAttack.BulletHit info)
			{
				if (info.hitHurtBox)
				{
					hitSomething = true;
					hitPos = info.point;
				}
				return true;
			};
			bullet.Fire();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (fixedAge > 0.02f)
			{
				if (hitSomething)
				{
					outer.SetNextState(new JachdwaltInitEvis()
					{
						vector3 = hitPos
					});
				}
				else
				{
					outer.SetNextStateToMain();
				}
			}
		}
	}

	class JachdwaltInitEvis : BaseTwinState
	{
		public override int meterGain => 0;
		private Transform modelTransform;
		public static GameObject blinkPrefab;
		private float stopwatch;
		private Vector3 aimDirectionVector = Vector3.zero;
		public float smallHopVelocity = 8;
		public float dashPrepDuration = 0.1f;
		public float dashDuration = 0.3f;
		public float speedCoefficient = 25f;
		public float overlapSphereRadius = 8f;
		private bool isDashing;
		public Vector3 vector3;

		public override void OnEnter()
		{
			base.OnEnter();
			modelTransform = GetModelTransform();

			aimDirectionVector = base.inputBank.aimDirection;
			Debug.LogWarning("Entered Pre-evis state");
		}

		private void CreateBlinkEffect(Vector3 origin)
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(aimDirectionVector);
			effectData.origin = origin;
			EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.deltaTime;
			if (stopwatch > dashPrepDuration && !isDashing)
			{
				isDashing = true;
				aimDirectionVector = base.inputBank.aimDirection;
				CreateBlinkEffect(vector3);
			}

			bool finished = stopwatch >= dashDuration + dashPrepDuration;
			if (isDashing)
			{
				if (base.isAuthority)
				{
					Collider[] colliders = Physics.OverlapSphere(
						vector3,
						overlapSphereRadius * 1.8f, //idk these are merc defaults
						LayerIndex.entityPrecise.mask
						);
					
					for (int i = 0; i < colliders.Length; i++)
					{
						HurtBox hurtBox = colliders[i].GetComponent<HurtBox>();
						if (hurtBox && hurtBox.healthComponent != base.healthComponent)
						{
							outer.SetNextState(new JachdwaltDoEvis
							{
								theVector3	= vector3
							});
							return;
						}
					}
				}
			}

			if (finished && base.isAuthority)
			{
				Debug.Log("no colliders were found, exiting");
				outer.SetNextStateToMain();
			}
		}
	}
		class JachdwaltDoEvis : BaseTwinState
		{
			private Transform modelTransform;
			public static GameObject blinkPrefab = Prefabs.Load<GameObject>("RoR2/Base/Huntress/HuntressFireArrowRain.prefab");
			public static float duration = 2f;
			public static float damageCoefficient = 1.1f;
			public static float damageFrequency = 7f;
			public static float procCoefficient = 1f;
			public static string beginSoundString = "Play_merc_R_start";
			public static string endSoundString = "Play_merc_R_end";
			public static float maxRadius = 16f;
			public static string slashSoundString = "Play_merc_sword_swing";
			public static string impactSoundString = "";
			public static string dashSoundString = "";
			public static float slashPitch = 1f;
			public static float smallHopVelocity = 0f;
			public static float lingeringInvincibilityDuration = 0.6f;
			private CharacterModel characterModel;
			private float stopwatch;
			private float attackStopwatch;
			private float totalAge;
			private bool crit;
			private static float minimumDuration = 0.5f;
			public Vector3 theVector3;
			private TemporaryOverlay overlay;

			public override void OnEnter()
			{
				base.OnEnter();
				CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
				Util.PlayAttackSpeedSound(beginSoundString, base.gameObject, 1.2f);
				crit = Util.CheckRoll(critStat, base.characterBody.master);
				modelTransform = GetModelTransform();
				if (modelTransform)
				{
					characterModel = modelTransform.GetComponent<CharacterModel>();
				}

				if (NetworkServer.active)
				{
					base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
				}
				Debug.LogWarning("Evis State");
				if (!overlay)
				{
					overlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
					overlay.duration = 4f;
					overlay.animateShaderAlpha = true;
					overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					overlay.destroyComponentOnEnd = false;
					overlay.originalMaterial = Prefabs.Load<Material>("RoR2/DLC1/voidstage/matVoidCrystal.mat");
					overlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
				}
			}

			public override void OnExit()
			{
				Util.PlaySound(endSoundString, base.gameObject);
				//CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
				modelTransform = GetModelTransform();
				if (modelTransform)
				{
					Material purpleStuff = new Material(LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded"));
					purpleStuff.SetColor("_TintColor", new Color(0.24f, 0f, 0.58f));
					
					TemporaryOverlay doNotTouchThis1 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
					doNotTouchThis1.duration = 0.6f;
					doNotTouchThis1.animateShaderAlpha = true;
					doNotTouchThis1.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					doNotTouchThis1.destroyComponentOnEnd = true;
					doNotTouchThis1.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEvisTarget");
					doNotTouchThis1.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
					TemporaryOverlay doNotTouchThis2 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
					doNotTouchThis2.duration = 0.7f;
					doNotTouchThis2.animateShaderAlpha = true;
					doNotTouchThis2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					doNotTouchThis2.destroyComponentOnEnd = true;
					doNotTouchThis2.originalMaterial = purpleStuff;
					doNotTouchThis2.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
				}

				if (overlay)
				{
					Destroy(overlay);
				}
				
				if (NetworkServer.active)
				{
					base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
					base.characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.HiddenInvincibility.buffIndex, lingeringInvincibilityDuration);
				}

				Util.PlaySound(endSoundString, base.gameObject);
				//SmallHop(base.characterMotor, smallHopVelocity);
				Debug.Log("exited Evis");
				Debug.Log($"{totalAge} Evis fixedAge");
				base.OnExit();
			}
			public override void FixedUpdate()
			{
				base.FixedUpdate();
				stopwatch += Time.deltaTime;
				attackStopwatch += Time.deltaTime;
				totalAge = fixedAge;
				float num = 1f / damageFrequency / attackSpeedStat;
				if (attackStopwatch >= num)
				{
					attackStopwatch -= num;
					HurtBox target = SearchForTarget();
					if (target)
					{
						Util.PlayAttackSpeedSound(slashSoundString, base.gameObject, slashPitch);
						//Util.PlaySound(dashSoundString, base.gameObject);
						//Util.PlaySound(impactSoundString, base.gameObject);
						HurtBoxGroup hurtBoxGroup = target.hurtBoxGroup;
						HurtBox hurtBox2 = hurtBoxGroup.hurtBoxes[Random.Range(0, hurtBoxGroup.hurtBoxes.Length - 1)];
						if (hurtBox2)
						{
							Vector3 position = hurtBox2.transform.position;
							Vector2 normalized = Random.insideUnitCircle.normalized;
							EffectManager.SimpleImpactEffect(Prefabs.JachdwaltStrikeEffect, position, new Vector3(normalized.x, 0f, normalized.y), false);
							Transform hurtBoxTranform = target.hurtBoxGroup.transform;
							TemporaryOverlay temporaryOverlay = hurtBoxTranform.gameObject.AddComponent<TemporaryOverlay>();
							temporaryOverlay.duration = num;
							temporaryOverlay.animateShaderAlpha = true;
							temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
							temporaryOverlay.destroyComponentOnEnd = true;
							temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEvisTarget");
							//Prefabs.Load<Material>("RoR2/DLC1/voidstage/matVoidCrystal.mat")
							temporaryOverlay.AddToCharacerModel(hurtBoxTranform.GetComponent<CharacterModel>());
							if (NetworkServer.active)
							{
								DamageInfo damageInfo = new DamageInfo();
								damageInfo.damage = damageCoefficient * damageStat;
								damageInfo.attacker = base.gameObject;
								damageInfo.procCoefficient = procCoefficient;
								damageInfo.position = hurtBox2.transform.position;
								damageInfo.crit = crit;
								hurtBox2.healthComponent.TakeDamage(damageInfo);
								GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtBox2.healthComponent.gameObject);
								GlobalEventManager.instance.OnHitAll(damageInfo, hurtBox2.healthComponent.gameObject);
							}
							CreateBlinkEffect(target.transform.position);
						}
						
					}
					else if (base.isAuthority && stopwatch > minimumDuration)
					{
						outer.SetNextStateToMain();
					}
				}

				if (characterMotor)
				{
					//base.characterMotor.velocity = Vector3.zero;
				}

				if (stopwatch >= duration && base.isAuthority)
				{
					var closestEnemy = SearchForTarget();
					if (closestEnemy)
					{
						
					}
					else
					{
						CreateBlinkEffect(characterDirection.forward + Vector3.one * 2f);
					}
					outer.SetNextStateToMain();
				}
			}

			private HurtBox SearchForTarget()
			{
				BullseyeSearch bullseyeSearch = new BullseyeSearch();
				bullseyeSearch.searchOrigin = theVector3;
				bullseyeSearch.searchDirection = Random.onUnitSphere;
				bullseyeSearch.maxDistanceFilter = maxRadius;
				bullseyeSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(GetTeam());
				bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
				bullseyeSearch.RefreshCandidates();
				bullseyeSearch.FilterOutGameObject(base.gameObject);
				return bullseyeSearch.GetResults().FirstOrDefault();
			}

			private void CreateBlinkEffect(Vector3 origin)
			{
				EffectData effectData = new EffectData();
				effectData.rotation = Util.QuaternionSafeLookRotation(Vector3.up);
				effectData.origin = origin;
				EffectManager.SpawnEffect(blinkPrefab, effectData, false);
			}
		}
	}
