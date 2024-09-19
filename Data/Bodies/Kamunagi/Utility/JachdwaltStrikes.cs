using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
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
			bullet.muzzleName = twinMuzzle;
			bullet.hitEffectPrefab = null;
			bullet.isCrit = base.RollCrit();
			bullet.radius = 1.5f;
			bullet.procCoefficient = 0.1f;
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
						bulletHitPos = hitPos
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
		private float stopwatch;
		private Vector3 aimDirectionVector = Vector3.zero;
		public float smallHopVelocity = 8;
		public float dashDuration = 0.3f;
		public float speedCoefficient = 25f;
		public float overlapSphereRadius = 8f;
		private bool isDashing;
		public Vector3 bulletHitPos;

		public override void OnEnter()
		{
			base.OnEnter();
			aimDirectionVector = base.inputBank.aimDirection;
		}

		private void CreateBlinkEffect(Vector3 origin)
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(aimDirectionVector);
			effectData.origin = origin;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.deltaTime;
			
			if (stopwatch > 0.1f && !isDashing)
			{
				isDashing = true;
				aimDirectionVector = base.inputBank.aimDirection;
				
				CreateBlinkEffect(bulletHitPos);
			}

			bool finished = stopwatch >= dashDuration;
			if (isDashing)
			{
				if (base.isAuthority)
				{
					Collider[] colliders = Physics.OverlapSphere(
						bulletHitPos,
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
								theVector3	= bulletHitPos
							});
							return;
						}
					}
				}
			}

			if (finished && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}
	}
		class JachdwaltDoEvis : BaseTwinState
		{
			private Transform modelTransform;
			public static GameObject blinkPrefab;
			public static float duration = 2f;
			public static float damageFrequency = 7f;
			public static float procCoefficient = 0.5f;
			public static string beginSoundString = "Play_merc_R_start";
			public static string endSoundString = "Play_merc_R_end";
			public static float maxRadius = 16f;
			public static string slashSoundString = "Play_merc_sword_swing";
			public static string impactSoundString = "";
			public static string dashSoundString = "";
			public static float slashPitch = 1f;
			public static float smallHopVelocity = 0f;
			public static float lingeringInvincibilityDuration = 0.6f;
			private float stopwatch;
			private float attackStopwatch;
			private float totalAge;
			private bool crit;
			private static float minimumDuration = 0.5f;
			public Vector3 theVector3;
			private TemporaryOverlayInstance cosmicOverlay;

			public override void OnEnter()
			{
				base.OnEnter();
				CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
				Util.PlayAttackSpeedSound(beginSoundString, base.gameObject, 1.2f);
				crit = Util.CheckRoll(critStat, base.characterBody.master);
				modelTransform = GetModelTransform();
				//mdl = modelTransform.GetComponent<CharacterModel>();
				
				if (NetworkServer.active)
				{
					base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
				}
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				stopwatch += Time.deltaTime;
				attackStopwatch += Time.deltaTime;
				float num = 1f / damageFrequency / attackSpeedStat;
				if (attackStopwatch >= num)
				{
					attackStopwatch -= num;
					
					BullseyeSearch bullseyeSearch = new BullseyeSearch();
					bullseyeSearch.searchOrigin = theVector3;
					bullseyeSearch.searchDirection = UnityEngine.Random.onUnitSphere;
					bullseyeSearch.maxDistanceFilter = maxRadius;
					bullseyeSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(GetTeam());
					bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
					bullseyeSearch.RefreshCandidates();
					bullseyeSearch.FilterOutGameObject(base.gameObject);
					var hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
					
					while (hurtBox && hurtBox != null && !hurtBox.healthComponent.alive)
					{
						bullseyeSearch.FilterOutGameObject(hurtBox.healthComponent.gameObject);
						hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
					}
					
					if (hurtBox) //&& hurtBox.healthComponent.alive)
					{
						Util.PlayAttackSpeedSound(slashSoundString, base.gameObject, slashPitch);
						Util.PlaySound(dashSoundString, base.gameObject);
						Util.PlaySound(impactSoundString, base.gameObject);
						HurtBoxGroup hurtBoxGroup = hurtBox.hurtBoxGroup;
						HurtBox hurtBox2 = hurtBoxGroup.hurtBoxes[UnityEngine.Random.Range(0, hurtBoxGroup.hurtBoxes.Length - 1)];
						if (hurtBox2)
						{
							Vector3 position = hurtBox2.transform.position;
							Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
							EffectManager.SimpleImpactEffect(normal: new Vector3(normalized.x, 0f, normalized.y), effectPrefab: Asset.GetGameObject<JachdwaltStrikes, IEffect>(), hitPos: position, transmit: false);
							Transform transform = hurtBox.hurtBoxGroup.transform;
							TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(transform.gameObject);
							temporaryOverlayInstance.duration = num;
							temporaryOverlayInstance.animateShaderAlpha = true;
							temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
							temporaryOverlayInstance.destroyComponentOnEnd = true;
							temporaryOverlayInstance.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEvisTarget");
							temporaryOverlayInstance.AddToCharacterModel(transform.GetComponent<CharacterModel>());
							if (NetworkServer.active)
							{
								DamageInfo damageInfo = new DamageInfo();
								damageInfo.damage = 1.3f * damageStat;
								damageInfo.attacker = base.gameObject;
								damageInfo.procCoefficient = procCoefficient;
								damageInfo.position = hurtBox2.transform.position;
								damageInfo.crit = crit;
								hurtBox2.healthComponent.TakeDamage(damageInfo);
								GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtBox2.healthComponent.gameObject);
								GlobalEventManager.instance.OnHitAll(damageInfo, hurtBox2.healthComponent.gameObject);
							}
						}
					}
					else
					{
						outer.SetNextStateToMain();
					}
				}

				if (stopwatch >= duration && base.isAuthority)
				{
					outer.SetNextStateToMain();
				}
			}

			private void CreateBlinkEffect(Vector3 origin)
			{
				EffectData effectData = new EffectData();
				effectData.rotation = Util.QuaternionSafeLookRotation(Vector3.up);
				effectData.origin = origin;
				EffectManager.SpawnEffect(blinkPrefab, effectData, false);
			}
			
			public override void OnExit()
			{
				Util.PlaySound(endSoundString, base.gameObject);
				CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
				modelTransform = GetModelTransform();
				if (modelTransform)
				{
					Material purpleStuff = new Material(LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded"));
					purpleStuff.SetColor("_TintColor", new Color(0.24f, 0f, 0.58f));
					
					TemporaryOverlayInstance mercEvisTarget = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
					mercEvisTarget.duration = 0.6f;
					mercEvisTarget.animateShaderAlpha = true;
					mercEvisTarget.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					mercEvisTarget.destroyComponentOnEnd = true;
					mercEvisTarget.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEvisTarget");
					mercEvisTarget.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
					
					TemporaryOverlayInstance huntressFlashExpanded = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
					huntressFlashExpanded.duration = 0.7f;
					huntressFlashExpanded.animateShaderAlpha = true;
					huntressFlashExpanded.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					huntressFlashExpanded.destroyComponentOnEnd = true;
					huntressFlashExpanded.originalMaterial = purpleStuff;
					huntressFlashExpanded.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
				}

				if (NetworkServer.active)
				{
					base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
					base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility,
						lingeringInvincibilityDuration);
				}

				Util.PlaySound(endSoundString, base.gameObject);
				base.OnExit();
			}
		}

		public class JachdwaltStrikes : Asset, ISkill, IEffect
		{
			SkillDef ISkill.BuildObject()
			{
				var skill = ScriptableObject.CreateInstance<SkillDef>();
				skill.skillName = "Utility 5";
				skill.skillNameToken = KamunagiAsset.tokenPrefix + "UTILITY2_NAME";
				skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "UTILITY2_DESCRIPTION";
				skill.icon = LoadAsset<Sprite>("bundle:Jachdwalt");
				skill.activationStateMachineName = "Weapon";
				skill.baseRechargeInterval = 4f;
				skill.beginSkillCooldownOnSkillEnd = true;
				skill.interruptPriority = InterruptPriority.Any;
				skill.isCombatSkill = true;
				skill.mustKeyPress = false;
				skill.cancelSprintingOnActivation = true;
				return skill;
			}

			IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(JachdwaltTestForTarget), typeof(JachdwaltInitEvis), typeof(JachdwaltDoEvis) };

			GameObject IEffect.BuildObject()
			{
				var effect = LoadAsset<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMercEvis.prefab")!.InstantiateClone("JachdwaltStrikeEffect", false);
				Material soDifficult0 = new Material(LoadAsset<Material>("RoR2/Base/Common/VFX/matOmniHitspark3.mat"));
				soDifficult0.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampEngi.png"));
				Material soDifficult1 = new Material(LoadAsset<Material>("RoR2/Base/Common/VFX/matOmniHitspark4.mat"));
				soDifficult1.SetColor("_TintColor", new Color(0.61f, 1f, 0.55f));
				Material soDifficult2 = new Material(LoadAsset<Material>("RoR2/Base/Common/VFX/matOmniRing2.mat"));
				//soDifficult2.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAntler.png"));
				Material soDifficult3 = new Material(LoadAsset<Material>("RoR2/Base/Common/VFX/matOmniHitspark2.mat"));
				soDifficult3.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampCaptainAirstrike.png"));

				var whatIsThis = effect.GetComponent<OmniEffect>();
				var array = whatIsThis.omniEffectGroups;
				array[6].omniEffectElements[0].particleSystemOverrideMaterial = soDifficult3;
				array[3].omniEffectElements[0].particleSystemOverrideMaterial = soDifficult2;
				array[3].omniEffectElements[1].particleSystemOverrideMaterial = soDifficult2;
				array[1].omniEffectElements[0].particleSystemOverrideMaterial = soDifficult1;
				array[4].omniEffectElements[0].particleSystemOverrideMaterial = soDifficult0;
				array[4].omniEffectElements[1].particleSystemOverrideMaterial = soDifficult0;
				foreach (ParticleSystemRenderer r in effect.GetComponentsInChildren<ParticleSystemRenderer>())
				{
					var name = r.name;
					var greenWithLessAlpha = new Color(0.01712415f, 0.3615091f, 0);
					var windyGreen = new Color(0.175f, 0.63f, 0.086f);

					if (name == "Hologram")
					{
						r.material.SetColor("_TintColor", windyGreen);
					}

					if (name == "Scaled Hitspark 3, Radial (Random Color)")
					{
						//this one is being stubborn 0
						var mat = r.material;

						mat.SetTexture("_RemapTex",
							LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampEngi.png"));
					}

					if (name == "Scaled Hitspark 4, Directional (Random Color) (1)")
					{
						//this one is being stubborn 1
						var mat = r.material;
						mat.SetColor("_TintColor", new Color(0.61f, 1f, 0.55f));
					}

					if (name == "Impact Slash")
					{
						//this one is being stubborn 2
						r.material = soDifficult2;
					}

					if (name == "Scaled Hitspark 2 (Random Color)")
					{
						//this one is being stubborn 3
						var mat = r.material;
						mat.SetTexture("_RemapTex",
							LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampCaptainAirstrike.png"));
					}
				}
				foreach (ParticleSystem p in effect.GetComponentsInChildren<ParticleSystem>(false))
				{
					var name = p.name;
					var main = p.main;

					if (name == "Scaled Hitspark 3, Radial (Random Color)")
					{
						main.startColor = new ParticleSystem.MinMaxGradient(Color.white);
					}

					if (name == "Scaled Hitspark 4, Directional (Random Color)")
					{
						main.startColor = new ParticleSystem.MinMaxGradient(Color.white);
					}

					if (name == "Dash, Bright")
					{
						main.startColor = Color.white;
					}

					if (name == "Flash, Hard")
					{
						main.startColor = Color.white;
					}
				}

				return effect;
			}
		}
}