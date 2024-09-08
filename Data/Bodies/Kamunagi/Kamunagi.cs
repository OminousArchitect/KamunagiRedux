using EntityStates;
using ExtraSkillSlots;
using KamunagiOfChains.Data.Bodies.Kamunagi.Primary;
using KamunagiOfChains.Data.Bodies.Kamunagi.Secondary;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using KamunagiOfChains.Data.Bodies.Kamunagi.Special;
using KamunagiOfChains.Data.Bodies.Kamunagi.Extra;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Passive;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies.Kamunagi
{
	public class KamunagiAsset : Asset, IBody, IBodyDisplay, ISurvivor, IModel, IEntityStates, ISkin, IMaster, IEffect
	{
		public const string tokenPrefix = "NINES_KAMUNAGI_BODY_";

		Type[] IEntityStates.GetEntityStates() =>
			new[]
			{
				typeof(VoidPortalSpawnState), typeof(BufferPortal), typeof(VoidDeathState),
				typeof(KamunagiCharacterMainState)
			};

		SkinDef ISkin.BuildObject() =>
			(SkinDef)ScriptableObject.CreateInstance(typeof(SkinDef), obj =>
			{
				var skinDef = (SkinDef)obj;
				ISkin.AddDefaults(ref skinDef);
				skinDef.name = "KamunagiDefaultSkinDef";
				skinDef.nameToken = tokenPrefix + "DEFAULT_SKIN_NAME";
				skinDef.icon = LoadAsset<Sprite>("bundle:TwinsSkin");

				if (!TryGetGameObject<KamunagiAsset, IModel>(out var model)) return;
				skinDef.rootObject = model;
				var modelRendererInfos = model.GetComponent<CharacterModel>().baseRendererInfos;
				var rendererInfos = new CharacterModel.RendererInfo[modelRendererInfos.Length];
				modelRendererInfos.CopyTo(rendererInfos, 0);
				skinDef.rendererInfos = rendererInfos;
			});

		GameObject IMaster.BuildObject()
		{
			var master = LoadAsset<GameObject>("RoR2/Base/Merc/MercMonsterMaster.prefab")!.InstantiateClone(
				"NinesKamunagiBodyMonsterMaster", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<KamunagiAsset, IBody>();
			return master;
		}

		Asset[] IModel.GetSkins() => new Asset[] { this };

		GameObject IModel.BuildObject()
		{
			var model = LoadAsset<GameObject>("bundle:mdlKamunagi")!;
			var characterModel = model.GetOrAddComponent<CharacterModel>();
			var childLocator = model.GetComponent<ChildLocator>();

			CharacterModel.RendererInfo RenderInfoFromChild(Component child, bool dontHopoo = false)
			{
				var renderer = child.GetComponent<Renderer>();
				return new CharacterModel.RendererInfo()
				{
					renderer = renderer,
					defaultMaterial = !dontHopoo ? renderer.material.SetHopooMaterial() : renderer.material,
					ignoreOverlays = false,
					defaultShadowCastingMode = ShadowCastingMode.On
				};
			}

			var voidCrystalMat = LoadAsset<Material>("addressable:RoR2/DLC1/voidstage/matVoidCrystal.mat");
			characterModel.baseRendererInfos = new[]
			{
				new CharacterModel.RendererInfo
				{
					renderer = childLocator.FindChild("S Cloth01").GetComponent<Renderer>(),
					defaultMaterial = voidCrystalMat,
					ignoreOverlays = false,
					defaultShadowCastingMode = ShadowCastingMode.On
				},
				new CharacterModel.RendererInfo()
				{
					renderer = childLocator.FindChild("U Cloth01").GetComponent<Renderer>(),
					defaultMaterial = voidCrystalMat,
					ignoreOverlays = false,
					defaultShadowCastingMode = ShadowCastingMode.On
				},
				RenderInfoFromChild(childLocator.FindChild("S Body")),
				RenderInfoFromChild(childLocator.FindChild("U Body")),
				RenderInfoFromChild(childLocator.FindChild("S Cloth02"), true),
				RenderInfoFromChild(childLocator.FindChild("U Cloth02"), true),
				RenderInfoFromChild(childLocator.FindChild("S Hair")),
				RenderInfoFromChild(childLocator.FindChild("U Hair")),
				RenderInfoFromChild(childLocator.FindChild("S Jewelry")),
				RenderInfoFromChild(childLocator.FindChild("U Jewelry")),
				RenderInfoFromChild(childLocator.FindChild("S HandItems")),
				RenderInfoFromChild(childLocator.FindChild("U HandItems")),
				RenderInfoFromChild(childLocator.FindChild("S Shoe")),
				RenderInfoFromChild(childLocator.FindChild("U Shoe"))
			};

			var modelHurtBoxGroup = model.GetOrAddComponent<HurtBoxGroup>();
			var mainHurtBox = childLocator.FindChild("MainHurtbox").gameObject;
			mainHurtBox.layer = LayerIndex.entityPrecise.intVal;
			var mainHurtBoxComponent = mainHurtBox.GetOrAddComponent<HurtBox>();
			mainHurtBoxComponent.isBullseye = true;
			modelHurtBoxGroup.hurtBoxes = new[] { mainHurtBoxComponent };

			return model;
		}

		GameObject IBodyDisplay.BuildObject()
		{
			if (!TryGetGameObject<KamunagiAsset, IModel>(out var model)) throw new Exception("Model not loaded.");
			var displayModel = model.InstantiateClone("KamunagiDisplay", false);
			displayModel.GetComponent<Animator>().runtimeAnimatorController =
				LoadAsset<RuntimeAnimatorController>("bundle:animHenryMenu");
			return displayModel;
		}

		GameObject IBody.BuildObject()
		{
			if (!TryGetGameObject<KamunagiAsset, IModel>(out var model)) throw new Exception("Model not loaded.");
			var bodyPrefab = LoadAsset<GameObject>("legacy:Prefabs/CharacterBodies/CommandoBody")!
				.InstantiateClone("NinesKamunagiBody");

			var bodyComponent = bodyPrefab.GetComponent<CharacterBody>();
			var bodyHealthComponent = bodyPrefab.GetComponent<HealthComponent>();
			var twinBehaviour = bodyPrefab.AddComponent<TwinBehaviour>();

			bodyComponent.preferredPodPrefab = null;
			bodyComponent.baseNameToken = tokenPrefix + "NAME";
			bodyComponent.subtitleNameToken = tokenPrefix + "SUBTITLE";
			bodyComponent.bodyColor = Colors.twinsLightColor;
			bodyComponent.portraitIcon = LoadAsset<Texture>("bundle:Twins");
			bodyComponent._defaultCrosshairPrefab = LoadAsset<GameObject>("RoR2/Base/Croco/CrocoCrosshair.prefab");

			bodyComponent.baseMaxHealth = 150f;
			bodyComponent.baseRegen = 1.5f;
			bodyComponent.baseArmor = 0f;
			bodyComponent.baseDamage = 12f;
			bodyComponent.baseCrit = 1f;
			bodyComponent.baseAttackSpeed = 1f;
			bodyComponent.baseMoveSpeed = 7f;
			bodyComponent.baseAcceleration = 80f;
			bodyComponent.baseJumpPower = 15f;

			//bodyCharacterBody.levelDamage = 2.6f; overwrote by below values in henry
			bodyComponent.levelMaxHealth = Mathf.Round(bodyComponent.baseMaxHealth * 0.3f);
			bodyComponent.levelMaxShield = Mathf.Round(bodyComponent.baseMaxShield * 0.3f);
			bodyComponent.levelRegen = bodyComponent.baseRegen * 0.2f;

			bodyComponent.levelMoveSpeed = 0f;
			bodyComponent.levelJumpPower = 0f;

			bodyComponent.levelDamage = bodyComponent.baseDamage * 0.2f;
			bodyComponent.levelAttackSpeed = 0f;
			bodyComponent.levelCrit = 0f;

			bodyComponent.levelArmor = 0f;
			bodyComponent.sprintingSpeedMultiplier = 1.45f;

			// I assume these were meant to be on?
			bodyComponent.bodyFlags |= CharacterBody.BodyFlags.ImmuneToExecutes;
			bodyComponent.bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;

			#region Setup Model

			var bodyHurtBoxGroup = model.GetComponentInChildren<HurtBoxGroup>();
			foreach (var hurtBox in bodyHurtBoxGroup.hurtBoxes)
			{
				hurtBox.healthComponent = bodyHealthComponent;
			}

			var bodyModelLocator = bodyPrefab.GetComponent<ModelLocator>();
			Object.Destroy(bodyModelLocator.modelTransform.gameObject);
			model.transform.parent = bodyModelLocator.modelBaseTransform;
			bodyModelLocator.modelTransform = model.transform;

			#endregion

			#region Setup StateMachines

			foreach (var toDestroy in bodyPrefab.GetComponents<EntityStateMachine>())
			{
				Object.Destroy(toDestroy);
			}

			var networkStateMachine = bodyPrefab.GetOrAddComponent<NetworkStateMachine>();

			var bodyStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
			bodyStateMachine.customName = "Body";
			bodyStateMachine.initialStateType = new SerializableEntityStateType(typeof(VoidPortalSpawnState));
			bodyStateMachine.mainStateType = new SerializableEntityStateType(typeof(KamunagiCharacterMainState));

			var hoverStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
			hoverStateMachine.customName = "Hover";
			hoverStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
			hoverStateMachine.mainStateType = hoverStateMachine.initialStateType;

			var weaponStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
			weaponStateMachine.customName = "Weapon";
			weaponStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
			weaponStateMachine.mainStateType = weaponStateMachine.initialStateType;

			networkStateMachine.stateMachines = new[] { bodyStateMachine, weaponStateMachine, hoverStateMachine };

			var deathBehaviour = bodyPrefab.GetOrAddComponent<CharacterDeathBehavior>();
			deathBehaviour.deathStateMachine = bodyStateMachine;
			deathBehaviour.idleStateMachine = new[] { weaponStateMachine, hoverStateMachine };
			deathBehaviour.deathState = new SerializableEntityStateType(typeof(VoidDeathState));

			#endregion

			#region Setup Skills

			foreach (var toDestroy in bodyPrefab.GetComponents<GenericSkill>())
			{
				Object.Destroy(toDestroy);
			}

			var skillLocator = bodyPrefab.GetComponent<SkillLocator>();
			var extraSkillLocator = bodyPrefab.AddComponent<ExtraSkillLocator>();
			if (TryGetAsset<KamunagiSkillFamilyPrimary>(out var skillFamilyPrimary))
			{
				var skill = bodyPrefab.AddComponent<GenericSkill>();
				skill.skillName = "SaraanaPrimary";
				skill._skillFamily = skillFamilyPrimary;
				skillLocator.primary = skill;
				var skill2 = bodyPrefab.AddComponent<GenericSkill>();
				skill2.skillName = "UruruuPrimary";
				skill2._skillFamily = skillFamilyPrimary;
			}

			if (TryGetAsset<KamunagiSkillFamilySecondary>(out var skillFamilySecondary))
			{
				var skill = bodyPrefab.AddComponent<GenericSkill>();
				skill.skillName = "SaraanaSecondary";
				skill._skillFamily = skillFamilySecondary;
				skillLocator.secondary = skill;
				var skill2 = bodyPrefab.AddComponent<GenericSkill>();
				skill2.skillName = "UruruuSecondary";
				skill2._skillFamily = skillFamilySecondary;
			}

			if (TryGetAsset<KamunagiSkillFamilyUtility>(out var skillFamilyUtility))
			{
				var skill = bodyPrefab.AddComponent<GenericSkill>();
				skill.skillName = "SaraanaUtility";
				skill._skillFamily = skillFamilyUtility;
				skillLocator.utility = skill;
				var skill2 = bodyPrefab.AddComponent<GenericSkill>();
				skill2.skillName = "UruruuUtility";
				skill2._skillFamily = skillFamilyUtility;
			}

			if (TryGetAsset<KamunagiSkillFamilySpecial>(out var skillFamilySpecial))
			{
				var skill = bodyPrefab.AddComponent<GenericSkill>();
				skill.skillName = "SaraanaSpecial";
				skill._skillFamily = skillFamilySpecial;
				skillLocator.special = skill;
				var skill2 = bodyPrefab.AddComponent<GenericSkill>();
				skill2.skillName = "UruruuSpecial";
				skill2._skillFamily = skillFamilySpecial;
			}

			if (TryGetAsset<KamunagiSkillFamilyExtra>(out var skillFamilyExtra))
			{
				var skill = bodyPrefab.AddComponent<GenericSkill>();
				skill.skillName = "SaraanaExtra";
				skill._skillFamily = skillFamilyExtra;
				extraSkillLocator.extraFourth = skill;
			}

			if (TryGetAsset<KamunagiSkillFamilyPassive>(out var skillFamilyPassive))
			{
				var skill = bodyPrefab.AddComponent<GenericSkill>();
				var family = (SkillFamily)skillFamilyPassive;
				skill.hideInCharacterSelect = family.variants.Length == 1;
				skill.skillName = "SaraanaPassive";
				skill._skillFamily = family;
			}

			skillLocator.passiveSkill = new SkillLocator.PassiveSkill
			{
				enabled = true,
				icon = LoadAsset<Sprite>("bundle:TwinsPassive"),
				skillDescriptionToken = tokenPrefix + "PASSIVE_DESCRIPTION",
				skillNameToken = tokenPrefix + "PASSIVE_NAME"
			};

			#endregion

			return bodyPrefab;
		}

		SurvivorDef ISurvivor.BuildObject()
		{
			var survivor = ScriptableObject.CreateInstance<SurvivorDef>();
			survivor.primaryColor = Colors.twinsLightColor;
			survivor.displayNameToken = tokenPrefix + "NAME";
			survivor.descriptionToken = tokenPrefix + "DESCRIPTION";
			survivor.outroFlavorToken = tokenPrefix + "OUTRO_FLAVOR";
			survivor.mainEndingEscapeFailureFlavorToken = tokenPrefix + "OUTRO_FAILURE";
			survivor.desiredSortPosition = 100f;

			if (TryGetGameObject<KamunagiAsset, IBody>(out var body))
				survivor.bodyPrefab = body;

			if (TryGetGameObject<KamunagiAsset, IBodyDisplay>(out var display))
				survivor.displayPrefab = display;

			return survivor;
		}

		GameObject IEffect.BuildObject()
		{
			var kamunagiChains = LoadAsset<GameObject>("bundle:KamunagiChains")!;
			kamunagiChains.AddComponent<ModelAttachedEffect>();
			kamunagiChains.transform.position = Vector3.zero;
			kamunagiChains.transform.rotation = Quaternion.identity;
			kamunagiChains.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
			kamunagiChains.GetOrAddComponent<ParticleUVScroll>();

			var comp = kamunagiChains.GetOrAddComponent<EffectComponent>();
			comp.parentToReferencedTransform = true;
			comp.positionAtReferencedTransform = true;
			var vfx = kamunagiChains.GetOrAddComponent<VFXAttributes>();
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;
			vfx.DoNotPool = false;
			return kamunagiChains;
		}
	}
}