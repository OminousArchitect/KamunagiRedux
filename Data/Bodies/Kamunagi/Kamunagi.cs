using EntityStates;
using ExtraSkillSlots;
using Kamunagi;
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
	public class KamunagiAsset : Concentric, IBody, IBodyDisplay, ISurvivor, IModel, IEntityStates, ISkin, IMaster,
		IEffect
	{
		public const string tokenPrefix = "NINES_KAMUNAGI_BODY_";

		IEnumerable<Type> IEntityStates.GetEntityStates() =>
			new[] { typeof(VoidDeathState), typeof(KamunagiCharacterMainState) };

		async Task<SkinDef> ISkin.BuildObject()
		{
			var icon = await LoadAsset<Sprite>("kamunagiassets:TwinsSkin");
			var model = await LoadAsset<GameObject>("kamunagiassets:mdlKamunagi")!;
			return (SkinDef)ScriptableObject.CreateInstance(typeof(SkinDef), obj =>
			{
				var skinDef = (SkinDef)obj;
				ISkin.AddDefaults(ref skinDef);
				skinDef.name = "KamunagiDefaultSkinDef";
				skinDef.nameToken = tokenPrefix + "DEFAULT_SKIN_NAME";
				skinDef.icon = icon;

				skinDef.rootObject = model;
				var modelRendererInfos = model.GetComponent<CharacterModel>().baseRendererInfos;
				var rendererInfos = new CharacterModel.RendererInfo[modelRendererInfos.Length];
				modelRendererInfos.CopyTo(rendererInfos, 0);
				skinDef.rendererInfos = rendererInfos;
			});
		}

		async Task<GameObject> IMaster.BuildObject()
		{
			var master = (await LoadAsset<GameObject>("RoR2/Base/Merc/MercMonsterMaster.prefab"))!.InstantiateClone(
				"NinesKamunagiBodyMonsterMaster", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();
			return master;
		}

		IEnumerable<Concentric> IModel.GetSkins() => new Concentric[] { this };

		async Task<GameObject> IModel.BuildObject()
		{
			var model = await LoadAsset<GameObject>("kamunagiassets:mdlKamunagi")!;
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

			var voidCrystalMat = await LoadAsset<Material>("addressable:RoR2/DLC1/voidstage/matVoidCrystal.mat");
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

			// this might be why the client player was dying for hosts
			modelHurtBoxGroup.mainHurtBox = mainHurtBoxComponent;

			#region itemdisplays

			var idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
			idrs.keyAssetRuleGroups = (await LoadAsset<GameObject>("RoR2/Base/Commando/CommandoBody.prefab")).GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet.keyAssetRuleGroups;

			var keyAsset = await LoadAsset<EquipmentDef>("RoR2/Base/EliteFire/EliteFireEquipment.asset");
			var fireDisplay = idrs.FindDisplayRuleGroup(keyAsset);
			var fireRules = new ItemDisplayRule[fireDisplay.rules.Length];
			Array.Copy(fireDisplay.rules, fireRules, fireDisplay.rules.Length);
			fireDisplay.rules = fireRules;
			fireDisplay.rules[0].childName = "S Hair";
			fireDisplay.rules[0].localPos = new Vector3(0.37824F, 0.18649F, 0.31578F);
			fireDisplay.rules[0].localAngles = new Vector3(290.8627F, 338.1044F, 46.19113F);
			fireDisplay.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			fireDisplay.rules[1].childName = "U Hair";
			fireDisplay.rules[1].localPos = new Vector3(-0.34832F, 0.26794F, 0.14957F);
			fireDisplay.rules[1].localAngles = new Vector3(52.33278F, 60.16898F, 218.7332F);
			fireDisplay.rules[1].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = fireRules });

			keyAsset = await LoadAsset<EquipmentDef>("RoR2/Base/EliteLightning/EliteLightningEquipment.asset");
			var lightningDisplay = idrs.FindDisplayRuleGroup(keyAsset);
			var lightningRules = new ItemDisplayRule[lightningDisplay.rules.Length];
			Array.Copy(lightningDisplay.rules, lightningRules, lightningDisplay.rules.Length);
			lightningDisplay.rules = lightningRules;
			lightningDisplay.rules[0].childName = "Muzzle";
			lightningDisplay.rules[0].localPos = new Vector3(0.06302F, -0.31085F, 0.46304F);
			lightningDisplay.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			lightningDisplay.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			lightningDisplay.rules[1].childName = "Muzzle";
			lightningDisplay.rules[1].localPos = new Vector3(0.04168F, 0.95129F, 0.15072F);
			lightningDisplay.rules[1].localAngles = new Vector3(335.6771F, 357.8F, 180F);
			lightningDisplay.rules[1].localScale = new Vector3(-0.40586F, 0.40586F, 0.40586F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = lightningRules });

			characterModel.itemDisplayRuleSet = idrs;

			#endregion

			return model;
		}

		async Task<GameObject> IBodyDisplay.BuildObject()
		{
			var model = await this.GetModel();
			var displayModel = model.InstantiateClone("KamunagiDisplay", false);
			displayModel.GetComponent<Animator>().runtimeAnimatorController =
				await LoadAsset<RuntimeAnimatorController>("kamunagiassets:animHenryMenu");
			displayModel.AddComponent<LobbySound>();
			return displayModel;
		}

		async Task<GameObject> IBody.BuildObject()
		{
			var model = await this.GetModel();
			var bodyPrefab = (await LoadAsset<GameObject>("legacy:Prefabs/CharacterBodies/MageBody"))!
				.InstantiateClone("NinesKamunagiBody");

			var bodyComponent = bodyPrefab.GetComponent<CharacterBody>();
			var bodyHealthComponent = bodyPrefab.GetComponent<HealthComponent>();
			var twinBehaviour = bodyPrefab.AddComponent<TwinBehaviour>();

			//bodyHealthComponent.body = bodyComponent; this isn't actually set in the commando prefab

			bodyComponent.preferredPodPrefab = null;
			bodyComponent.baseNameToken = tokenPrefix + "NAME";
			bodyComponent.subtitleNameToken = tokenPrefix + "SUBTITLE";
			bodyComponent.bodyColor = Colors.twinsLightColor;
			bodyComponent.portraitIcon = await LoadAsset<Texture>("kamunagiassets:Twins");
			bodyComponent._defaultCrosshairPrefab =
				await LoadAsset<GameObject>("RoR2/Base/Croco/CrocoCrosshair.prefab");

			bodyComponent.baseMaxHealth = 150f;
			bodyComponent.baseRegen = 1.5f;
			bodyComponent.baseArmor = 0f;
			bodyComponent.baseDamage = 12f;
			bodyComponent.baseCrit = 1f;
			bodyComponent.baseAttackSpeed = 1f;
			bodyComponent.baseMoveSpeed = 7f;
			bodyComponent.baseAcceleration = 80f;
			bodyComponent.baseJumpPower = 15f;

			bodyComponent.levelDamage = 2.4f;
			bodyComponent.levelMaxHealth = Mathf.Round(bodyComponent.baseMaxHealth * 0.3f);
			bodyComponent.levelMaxShield = Mathf.Round(bodyComponent.baseMaxShield * 0.3f);
			bodyComponent.levelRegen = bodyComponent.baseRegen * 0.2f;

			bodyComponent.levelMoveSpeed = 0f;
			bodyComponent.levelJumpPower = 0f;

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
			model.GetComponent<CharacterModel>().body = bodyComponent;
			bodyModelLocator.modelTransform = model.transform;
			//bodyHealthComponent.modelLocator = bodyModelLocator; this isnt even serialized by unity, so its not set in the prefab either

			#region OhNoBro

			/*var idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
			var kamRuleset = model.GetComponent<CharacterModel>().itemDisplayRuleSet;
			kamRuleset.keyAssetRuleGroups = idrs.keyAssetRuleGroups;*/

			#endregion

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

			var spellStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
			spellStateMachine.customName = "Spell";
			spellStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
			spellStateMachine.mainStateType = spellStateMachine.initialStateType;

			networkStateMachine.stateMachines =
				new[] { bodyStateMachine, weaponStateMachine, hoverStateMachine, spellStateMachine };

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

			var primarySkill = bodyPrefab.AddComponent<GenericSkill>();
			primarySkill.skillName = "SaraanaPrimary";
			primarySkill._skillFamily = await GetSkillFamily<KamunagiSkillFamilyPrimary>();
			skillLocator.primary = primarySkill;
			var primarySkill2 = bodyPrefab.AddComponent<GenericSkill>();
			primarySkill2.skillName = "UruruuPrimary";
			primarySkill2._skillFamily = await GetSkillFamily<KamunagiSkillFamilyPrimary2>();


			var secondarySkill = bodyPrefab.AddComponent<GenericSkill>();
			secondarySkill.skillName = "SaraanaSecondary";
			secondarySkill._skillFamily = await GetSkillFamily<KamunagiSkillFamilySecondary>();
			skillLocator.secondary = secondarySkill;
			var secondarySkill2 = bodyPrefab.AddComponent<GenericSkill>();
			secondarySkill2.skillName = "UruruuSecondary";
			secondarySkill2._skillFamily = await GetSkillFamily<KamunagiSkillFamilySecondary2>();


			var utilitySkill = bodyPrefab.AddComponent<GenericSkill>();
			utilitySkill.skillName = "SaraanaUtility";
			utilitySkill._skillFamily = await GetSkillFamily<KamunagiSkillFamilyUtility>();
			skillLocator.utility = utilitySkill;
			var utilitySkill2 = bodyPrefab.AddComponent<GenericSkill>();
			utilitySkill2.skillName = "UruruuUtility";
			utilitySkill2._skillFamily = await GetSkillFamily<KamunagiSkillFamilyUtility2>();


			var specialSkill = bodyPrefab.AddComponent<GenericSkill>();
			specialSkill.skillName = "SaraanaSpecial";
			specialSkill._skillFamily = await GetSkillFamily<KamunagiSkillFamilySpecial>();
			skillLocator.special = specialSkill;
			var specialSkill2 = bodyPrefab.AddComponent<GenericSkill>();
			specialSkill2.skillName = "UruruuSpecial";
			specialSkill2._skillFamily = await GetSkillFamily<KamunagiSkillFamilySpecial2>();


			var divineSkill = bodyPrefab.AddComponent<GenericSkill>();
			divineSkill.skillName = "SaraanaExtra";
			divineSkill._skillFamily = await GetSkillFamily<KamunagiSkillFamilyExtra>();
			extraSkillLocator.extraFourth = divineSkill;
			var divineSkill2 = bodyPrefab.AddComponent<GenericSkill>();
			divineSkill2.skillName = "UruruuExtra";
			divineSkill2._skillFamily = await GetSkillFamily<KamunagiSkillFamilyExtra2>();

			var passiveSkill = bodyPrefab.AddComponent<GenericSkill>();
			var family = await GetSkillFamily<KamunagiSkillFamilyPassive>();
			passiveSkill.skillName = "AscensionPassive";
			passiveSkill._skillFamily = family;
			passiveSkill.hideInCharacterSelect = family.variants.Length == 1;

			SetStateOnHurt timesweeper = bodyPrefab.GetComponent<SetStateOnHurt>();
			timesweeper.targetStateMachine = bodyStateMachine;
			timesweeper.idleStateMachine = new[] { weaponStateMachine, hoverStateMachine, spellStateMachine };

			skillLocator.passiveSkill = new SkillLocator.PassiveSkill
			{
				enabled = true,
				icon = await LoadAsset<Sprite>("kamunagiassets:TwinsPassive"),
				skillDescriptionToken = tokenPrefix + "PASSIVE_DESCRIPTION",
				skillNameToken = tokenPrefix + "PASSIVE_NAME"
			};

			#endregion

			return bodyPrefab;
		}

		async Task<SurvivorDef> ISurvivor.BuildObject()
		{
			var survivor = ScriptableObject.CreateInstance<SurvivorDef>();
			survivor.primaryColor = Colors.twinsLightColor;
			survivor.displayNameToken = tokenPrefix + "NAME";
			survivor.descriptionToken = tokenPrefix + "DESCRIPTION";
			survivor.outroFlavorToken = tokenPrefix + "OUTRO_FLAVOR";
			survivor.mainEndingEscapeFailureFlavorToken = tokenPrefix + "OUTRO_FAILURE";
			survivor.desiredSortPosition = 100f;

			survivor.bodyPrefab = await this.GetBody();
			survivor.displayPrefab = await this.GetBodyDisplay();

			return survivor;
		}

		async Task<GameObject> IEffect.BuildObject()
		{
			var kamunagiChains = await LoadAsset<GameObject>("kamunagiassets:KamunagiChains")!;
			kamunagiChains.AddComponent<ModelAttachedEffect>();
			kamunagiChains.transform.position = Vector3.zero;
			kamunagiChains.transform.rotation = Quaternion.identity;
			kamunagiChains.transform.localScale = new Vector3(0.17f, 0.25f, 0.17f);
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