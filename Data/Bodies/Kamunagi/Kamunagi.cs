using EntityStates;
using ExtraSkillSlots;
using HG;
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
			idrs.keyAssetRuleGroups = ArrayUtils.Clone((await LoadAsset<GameObject>("RoR2/Base/Commando/CommandoBody.prefab")).GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet.keyAssetRuleGroups);

			var keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Behemoth/Behemoth.asset");
			var behemoth = idrs.FindDisplayRuleGroup(keyAsset);
			var behemothRules = new ItemDisplayRule[behemoth.rules.Length];
			Array.Copy(behemoth.rules, behemothRules, behemoth.rules.Length);
			behemoth.rules = behemothRules;
			behemoth.rules[0].childName = "U_Chest";
			behemoth.rules[0].localPos = new Vector3(-0.026F, 0.176F, -0.202F);
			behemoth.rules[0].localAngles = new Vector3(344.46F, 153.99F, 330.8F);
			behemoth.rules[0].localScale = new Vector3(0.07F, 0.07F, 0.07F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = behemothRules });

			//Red Items
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Dagger/Dagger.asset");
			var dagger = idrs.FindDisplayRuleGroup(keyAsset);
			var daggerRules = new ItemDisplayRule[dagger.rules.Length];
			Array.Copy(dagger.rules, daggerRules, dagger.rules.Length);
			dagger.rules = daggerRules;
			dagger.rules[0].childName = "S_Head";
			dagger.rules[0].localPos = new Vector3(0.072F, 0.182F, -0.1F);
			dagger.rules[0].localAngles = new Vector3(278F, 201.02F, 38.57F);
			dagger.rules[0].localScale = new Vector3(0.8F, 0.8F, 0.8F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = daggerRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Icicle/Icicle.asset");
			var frostR = idrs.FindDisplayRuleGroup(keyAsset);
			var frostRRules = new ItemDisplayRule[frostR.rules.Length];
			Array.Copy(frostR.rules, frostRRules, frostR.rules.Length);
			frostR.rules = frostRRules;
			frostR.rules[0].childName = "MuzzleCenter";
			frostR.rules[0].localPos = new Vector3(-0.083F, -0.215F, -0.201F);
			frostR.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			frostR.rules[0].localScale = new Vector3(1.3F, 1.3F, 1.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = frostRRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/GhostOnKill/GhostOnKill.asset");
			var HMask = idrs.FindDisplayRuleGroup(keyAsset);
			var HMaskRules = new ItemDisplayRule[HMask.rules.Length];
			Array.Copy(HMask.rules, HMaskRules, HMask.rules.Length);
			HMask.rules = HMaskRules;
			HMask.rules[0].childName = "U_Head";
			HMask.rules[0].localPos = new Vector3(-0.082F, 0.149F, 0.141F);
			HMask.rules[0].localAngles = new Vector3(10.97F, 303.42F, 353.94F);
			HMask.rules[0].localScale = new Vector3(0.8F, 0.9F, 0.8F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = HMaskRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/FallBoots/FallBoots.asset");
			var fallBoots = idrs.FindDisplayRuleGroup(keyAsset);
			var fallBootsRules = new ItemDisplayRule[fallBoots.rules.Length];
			Array.Copy(fallBoots.rules, fallBootsRules, fallBoots.rules.Length);
			fallBoots.rules = fallBootsRules;
			fallBoots.rules[0].childName = "S_Ankle";
			fallBoots.rules[0].localPos = new Vector3(-0.003F, -0.019F, 0.024F);
			fallBoots.rules[0].localAngles = new Vector3(303.33F, 359.95F, 359.52F);
			fallBoots.rules[0].localScale = new Vector3(0.4F, 0.4F, 0.4F);
			fallBoots.rules[1].childName = "U_Ankle";
			fallBoots.rules[1].localPos = new Vector3(0.015F, -0.019F, 0.023F);
			fallBoots.rules[1].localAngles = new Vector3(302.29F, 0.03F, 0.74F);
			fallBoots.rules[1].localScale = new Vector3(0.4F, 0.4F, 0.4F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = fallBootsRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/NovaOnHeal/NovaOnHeal.asset");
			var healHorns = idrs.FindDisplayRuleGroup(keyAsset);
			var healHornsRules = new ItemDisplayRule[healHorns.rules.Length];
			Array.Copy(healHorns.rules, healHornsRules, healHorns.rules.Length);
			healHorns.rules = healHornsRules;
			healHorns.rules[0].childName = "S_Wrist";
			healHorns.rules[0].localPos = new Vector3(0.008F, 0.001F, -0.013F);
			healHorns.rules[0].localAngles = new Vector3(276.71F, 29.95F, 117.67F);
			healHorns.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			healHorns.rules[1].childName = "U_Wrist";
			healHorns.rules[1].localPos = new Vector3(0.013F, 0.014F, -0.011F);
			healHorns.rules[1].localAngles = new Vector3(323.72F, 116.43F, 23.79F);
			healHorns.rules[1].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = healHornsRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/ShockNearby/ShockNearby.asset");
			var tesla = idrs.FindDisplayRuleGroup(keyAsset);
			var teslaRules = new ItemDisplayRule[tesla.rules.Length];
			Array.Copy(tesla.rules, teslaRules, tesla.rules.Length);
			tesla.rules = teslaRules;
			tesla.rules[0].childName = "U_Earring";
			tesla.rules[0].localPos = new Vector3(-0.002F, 0.071F, 0.004F);
			tesla.rules[0].localAngles = new Vector3(84.23F, 201.46F, 89.88F);
			tesla.rules[0].localScale = new Vector3(0.13F, 0.06F, 0.13F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = teslaRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Clover/Clover.asset");
			var clover = idrs.FindDisplayRuleGroup(keyAsset);
			var cloverRules = new ItemDisplayRule[clover.rules.Length];
			Array.Copy(clover.rules, cloverRules, clover.rules.Length);
			clover.rules = cloverRules;
			clover.rules[0].childName = "S_Head";
			clover.rules[0].localPos = new Vector3(-0.13F, 0.371F, 0.069F);
			clover.rules[0].localAngles = new Vector3(47.48F, 312.88F, 10.66F);
			clover.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = cloverRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/BounceNearby/BounceNearby.asset");
			var meatHook = idrs.FindDisplayRuleGroup(keyAsset);
			var meatHookRules = new ItemDisplayRule[meatHook.rules.Length];
			Array.Copy(meatHook.rules, meatHookRules, meatHook.rules.Length);
			meatHook.rules = meatHookRules;
			meatHook.rules[0].childName = "U Bone";
			meatHook.rules[0].localPos = new Vector3(-0.006F, 0.473F, -0.057F);
			meatHook.rules[0].localAngles = new Vector3(356.37F, 1.36F, 42.8F);
			meatHook.rules[0].localScale = new Vector3(0.7F, 0.6F, 0.6F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = meatHookRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/AlienHead/AlienHead.asset");
			var alien = idrs.FindDisplayRuleGroup(keyAsset);
			var alienRules = new ItemDisplayRule[alien.rules.Length];
			Array.Copy(alien.rules, alienRules, alien.rules.Length);
			alien.rules = alienRules;
			alien.rules[0].childName = "S_Sash";
			alien.rules[0].localPos = new Vector3(-0.016F, 0.003F, 0.008F);
			alien.rules[0].localAngles = new Vector3(77.09F, 259.49F, 158.23F);
			alien.rules[0].localScale = new Vector3(0.8F, 0.8F, 0.8F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = alienRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Talisman/Talisman.asset");
			var soulbound = idrs.FindDisplayRuleGroup(keyAsset);
			var soulboundRules = new ItemDisplayRule[soulbound.rules.Length];
			Array.Copy(soulbound.rules, soulboundRules, soulbound.rules.Length);
			soulbound.rules = soulboundRules;
			soulbound.rules[0].childName = "MuzzleCenter";
			soulbound.rules[0].localPos = new Vector3(1.948F, 0.776F, -0.181F);
			soulbound.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			soulbound.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = soulboundRules });

			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/UtilitySkillMagazine/UtilitySkillMagazine.asset");
			var hardlight = idrs.FindDisplayRuleGroup(keyAsset);
			var hardlightRules = new ItemDisplayRule[hardlight.rules.Length];
			Array.Copy(hardlight.rules, hardlightRules, hardlight.rules.Length);
			hardlight.rules = hardlightRules;
			hardlight.rules[0].childName = "U_Ankle";
			hardlight.rules[0].localPos = new Vector3(0.007F, 0.078F, -0.055F);
			hardlight.rules[0].localAngles = new Vector3(58.56F, 180F, 180F);
			hardlight.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			hardlight.rules[1].childName = "S_Ankle";
			hardlight.rules[1].localPos = new Vector3(-0.007F, 0.078F, -0.054F);
			hardlight.rules[1].localAngles = new Vector3(57.34F, 180F, 180F);
			hardlight.rules[1].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = hardlightRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/HeadHunter/HeadHunter.asset");
			var vultures = idrs.FindDisplayRuleGroup(keyAsset);
			var vulturesRules = new ItemDisplayRule[vultures.rules.Length];
			Array.Copy(vultures.rules, vulturesRules, vultures.rules.Length);
			vultures.rules = vulturesRules;
			vultures.rules[0].childName = "U_Head";
			vultures.rules[0].localPos = new Vector3(0.008F, 0.287F, 0.002F);
			vultures.rules[0].localAngles = new Vector3(350.78F, 354.05F, 0.53F);
			vultures.rules[0].localScale = new Vector3(0.65F, 0.28F, 0.25F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = vulturesRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/KillEliteFrenzy/KillEliteFrenzy.asset");
			var brainstalks = idrs.FindDisplayRuleGroup(keyAsset);
			var brainstalksRules = new ItemDisplayRule[brainstalks.rules.Length];
			Array.Copy(brainstalks.rules, brainstalksRules, brainstalks.rules.Length);
			brainstalks.rules = brainstalksRules;
			brainstalks.rules[0].childName = "S_Head";
			brainstalks.rules[0].localPos = new Vector3(0.016F, 0.334F, 0.015F);
			brainstalks.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			brainstalks.rules[0].localScale = new Vector3(0.3F, 0.2F, 0.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = brainstalksRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/IncreaseHealing/IncreaseHealing.asset");
			var rejuvRack = idrs.FindDisplayRuleGroup(keyAsset);
			var rejuvRackRules = new ItemDisplayRule[rejuvRack.rules.Length];
			Array.Copy(rejuvRack.rules, rejuvRackRules, rejuvRack.rules.Length);
			rejuvRack.rules = rejuvRackRules;
			rejuvRack.rules[0].childName = "U_Head";
			rejuvRack.rules[0].localPos = new Vector3(-0.1F, 0.207F, -0.01F);
			rejuvRack.rules[0].localAngles = new Vector3(0F, 270F, 0F);
			rejuvRack.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			rejuvRack.rules[1].childName = "S_Head";
			rejuvRack.rules[1].localPos = new Vector3(0.101F, 0.229F, 0F);
			rejuvRack.rules[1].localAngles = new Vector3(0F, 90F, 0F);
			rejuvRack.rules[1].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = rejuvRackRules });

			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/ArmorReductionOnHit/ArmorReductionOnHit.asset");
			var justice = idrs.FindDisplayRuleGroup(keyAsset);
			var justiceRules = new ItemDisplayRule[justice.rules.Length];
			Array.Copy(justice.rules, justiceRules, justice.rules.Length);
			justice.rules = justiceRules;
			justice.rules[0].childName = "S_Chest";
			justice.rules[0].localPos = new Vector3(-0.098F, 0.15F, -0.235F);
			justice.rules[0].localAngles = new Vector3(303.12F, 308.61F, 43.52F);
			justice.rules[0].localScale = new Vector3(0.2F, 0.2F, 0.2F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = justiceRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/LaserTurbine/LaserTurbine.asset");
			var laserDisc = idrs.FindDisplayRuleGroup(keyAsset);
			var laserDiscRules = new ItemDisplayRule[laserDisc.rules.Length];
			Array.Copy(laserDisc.rules, laserDiscRules, laserDisc.rules.Length);
			laserDisc.rules = laserDiscRules;
			laserDisc.rules[0].childName = "MuzzleCenter";
			laserDisc.rules[0].localPos = new Vector3(-1.988F, 0.218F, -0.914F);
			laserDisc.rules[0].localAngles = new Vector3(270F, 180F, 0F);
			laserDisc.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = laserDiscRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Plant/Plant.asset");
			var deskPlant = idrs.FindDisplayRuleGroup(keyAsset);
			var deskPlantRules = new ItemDisplayRule[deskPlant.rules.Length];
			Array.Copy(deskPlant.rules, deskPlantRules, deskPlant.rules.Length);
			deskPlant.rules = deskPlantRules;
			deskPlant.rules[0].childName = "S_Earring";
			deskPlant.rules[0].localPos = new Vector3(0.008F, 0.086F, -0.003F);
			deskPlant.rules[0].localAngles = new Vector3(86.52F, 220.08F, 96.7F);
			deskPlant.rules[0].localScale = new Vector3(0.03F, 0.03F, 0.03F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = deskPlantRules });
			
			/*keyAsset = await LoadAsset<ItemDef>("RoR2/Base/CaptainDefenseMatrix/CaptainDefenseMatrix.asset"); //huh
			var microbots = idrs.FindDisplayRuleGroup(keyAsset);
			var microbotsRules = new ItemDisplayRule[microbots.rules.Length];
			Array.Copy(microbots.rules, microbotsRules, microbots.rules.Length);
			microbots.rules = microbotsRules;
			microbots.rules[0].childName = "MuzzleCenter";
			microbots.rules[0].localPos = new Vector3(0.06302F, -0.31085F, 0.46304F);
			microbots.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			microbots.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = microbotsRules });*/

			/*keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/DroneWeapons/DroneWeapons.asset"); //huh
			var droneParts = idrs.FindDisplayRuleGroup(keyAsset);
			var dronePartsRules = new ItemDisplayRule[droneParts.rules.Length];
			Array.Copy(droneParts.rules, dronePartsRules, droneParts.rules.Length);
			droneParts.rules = dronePartsRules;
			droneParts.rules[0].childName = "MuzzleCenter";
			droneParts.rules[0].localPos = new Vector3(0.06302F, -0.31085F, 0.46304F);
			droneParts.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			droneParts.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = dronePartsRules });*/
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/PermanentDebuffOnHit/PermanentDebuffOnHit.asset");
			var scorpion = idrs.FindDisplayRuleGroup(keyAsset);
			var scorpionRules = new ItemDisplayRule[scorpion.rules.Length];
			Array.Copy(scorpion.rules, scorpionRules, scorpion.rules.Length);
			scorpion.rules = scorpionRules;
			scorpion.rules[0].childName = "S_Head";
			scorpion.rules[0].localPos = new Vector3(0.004F, 0.395F, 0.08F);
			scorpion.rules[0].localAngles = new Vector3(62.02F, 176.75F, 353F);
			scorpion.rules[0].localScale = new Vector3(0.8F, 0.8F, 0.8F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = scorpionRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/ImmuneToDebuff/ImmuneToDebuff.asset");
			var raincoat = idrs.FindDisplayRuleGroup(keyAsset);
			var raincoatRules = new ItemDisplayRule[raincoat.rules.Length];
			Array.Copy(raincoat.rules, raincoatRules, raincoat.rules.Length);
			raincoat.rules = raincoatRules;
			raincoat.rules[0].childName = "U_Sash";
			raincoat.rules[0].localPos = new Vector3(0.018F, 0.256F, -0.032F);
			raincoat.rules[0].localAngles = new Vector3(14.63F, 111.91F, 183.53F);
			raincoat.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = raincoatRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/RandomEquipmentTrigger/RandomEquipmentTrigger.asset");
			var bottleChaos = idrs.FindDisplayRuleGroup(keyAsset);
			var bottleChaosRules = new ItemDisplayRule[bottleChaos.rules.Length];
			Array.Copy(bottleChaos.rules, bottleChaosRules, bottleChaos.rules.Length);
			bottleChaos.rules = bottleChaosRules;
			bottleChaos.rules[0].childName = "S_Waist";
			bottleChaos.rules[0].localPos = new Vector3(0.373F, -0.019F, -0.006F);
			bottleChaos.rules[0].localAngles = new Vector3(350.01F, 86.66F, 355.27F);
			bottleChaos.rules[0].localScale = new Vector3(0.3F, 0.4F, 0.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = bottleChaosRules });
			
			//Yellow items
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Knurl/Knurl.asset");
			var knurl = idrs.FindDisplayRuleGroup(keyAsset);
			var knurlRules = new ItemDisplayRule[knurl.rules.Length];
			Array.Copy(knurl.rules, knurlRules, knurl.rules.Length);
			knurl.rules = knurlRules;
			knurl.rules[0].childName = "S_Chest";
			knurl.rules[0].localPos = new Vector3(0.011F, 0.283F, 0.148F);
			knurl.rules[0].localAngles = new Vector3(304.76F, 189.53F, 353.8F);
			knurl.rules[0].localScale = new Vector3(0.02F, 0.02F, 0.03F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = knurlRules });

			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/TitanGoldDuringTP/TitanGoldDuringTP.asset");
			var goldSeed = idrs.FindDisplayRuleGroup(keyAsset);
			var goldSeedRules = new ItemDisplayRule[goldSeed.rules.Length];
			Array.Copy(goldSeed.rules, goldSeedRules, goldSeed.rules.Length);
			goldSeed.rules = goldSeedRules;
			goldSeed.rules[0].childName = "U_Chest";
			goldSeed.rules[0].localPos = new Vector3(0.002F, 0.196F, 0.195F);
			goldSeed.rules[0].localAngles = new Vector3(22.31F, 0.03F, 0.03F);
			goldSeed.rules[0].localScale = new Vector3(0.1F, 0.1F, 0.1F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = goldSeedRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/SprintWisp/SprintWisp.asset");
			var sprintWisp = idrs.FindDisplayRuleGroup(keyAsset);
			var sprintWispRules = new ItemDisplayRule[sprintWisp.rules.Length];
			Array.Copy(sprintWisp.rules, sprintWispRules, sprintWisp.rules.Length);
			sprintWisp.rules = sprintWispRules;
			sprintWisp.rules[0].childName = "S_Head";
			sprintWisp.rules[0].localPos = new Vector3(0.014F, 0.104F, 0.23F);
			sprintWisp.rules[0].localAngles = new Vector3(7.23F, 3.25F, 357.1F);
			sprintWisp.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = sprintWispRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Pearl/Pearl.asset");
			var pearl = idrs.FindDisplayRuleGroup(keyAsset);
			var pearlRules = new ItemDisplayRule[pearl.rules.Length];
			Array.Copy(pearl.rules, pearlRules, pearl.rules.Length);
			pearl.rules = pearlRules;
			pearl.rules[0].childName = "MuzzleCenter";
			pearl.rules[0].localPos = new Vector3(-0.127F, 0.122F, -0.979F);
			pearl.rules[0].localAngles = new Vector3(90F, 0F, 0F);
			pearl.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = pearlRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/ShinyPearl/ShinyPearl.asset");
			var perfectPearl = idrs.FindDisplayRuleGroup(keyAsset);
			var perfectPearlRules = new ItemDisplayRule[perfectPearl.rules.Length];
			Array.Copy(perfectPearl.rules, perfectPearlRules, perfectPearl.rules.Length);
			perfectPearl.rules = perfectPearlRules;
			perfectPearl.rules[0].childName = "MuzzleCenter";
			perfectPearl.rules[0].localPos = new Vector3(-0.101F, -0.376F, -0.884F);
			perfectPearl.rules[0].localAngles = new Vector3(0F, 86.65F, 0F);
			perfectPearl.rules[0].localScale = new Vector3(0.4F, 0.4F, 0.4F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = perfectPearlRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/NovaOnLowHealth/NovaOnLowHealth.asset");
			var genesisLoop = idrs.FindDisplayRuleGroup(keyAsset);
			var loopRules = new ItemDisplayRule[genesisLoop.rules.Length];
			Array.Copy(genesisLoop.rules, loopRules, genesisLoop.rules.Length);
			genesisLoop.rules = loopRules;
			genesisLoop.rules[0].childName = "S_Wrist";
			genesisLoop.rules[0].localPos = new Vector3(0.073F, -0.028F, 0.013F);
			genesisLoop.rules[0].localAngles = new Vector3(341.74F, 252.79F, 346.09F);
			genesisLoop.rules[0].localScale = new Vector3(0.09F, 0.05F, 0.09F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = loopRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/FireballsOnHit/FireballsOnHit.asset");
			var firePerf = idrs.FindDisplayRuleGroup(keyAsset);
			var firePerfRules = new ItemDisplayRule[firePerf.rules.Length];
			Array.Copy(firePerf.rules, firePerfRules, firePerf.rules.Length);
			firePerf.rules = firePerfRules;
			firePerf.rules[0].childName = "U_Wrist";
			firePerf.rules[0].localPos = new Vector3(0.019F, 0.112F, -0.051F);
			firePerf.rules[0].localAngles = new Vector3(282.81F, 103.62F, 165.26F);
			firePerf.rules[0].localScale = new Vector3(0.02F, 0.02F, 0.02F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = firePerfRules });
			
			/*keyAsset = await LoadAsset<ItemDef>("RoR2/Base/LightningStrikeOnHit/LightningStrikeOnHit.asset"); //huh
			var chargedPerf = idrs.FindDisplayRuleGroup(keyAsset);
			var chargedPerfRules = new ItemDisplayRule[chargedPerf.rules.Length];
			Array.Copy(chargedPerf.rules, chargedPerfRules, chargedPerf.rules.Length);
			chargedPerf.rules = chargedPerfRules;
			chargedPerf.rules[0].childName = "MuzzleCenter";
			chargedPerf.rules[0].localPos = new Vector3(0.06302F, -0.31085F, 0.46304F);
			chargedPerf.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			chargedPerf.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = chargedPerfRules });*/

			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/SiphonOnLowHealth/SiphonOnLowHealth.asset");
			var miredUrn = idrs.FindDisplayRuleGroup(keyAsset);
			var miredUrnRules = new ItemDisplayRule[miredUrn.rules.Length];
			Array.Copy(miredUrn.rules, miredUrnRules, miredUrn.rules.Length);
			miredUrn.rules = miredUrnRules;
			miredUrn.rules[0].childName = "U_Waist";
			miredUrn.rules[0].localPos = new Vector3(0.454F, 0.004F, 0.037F);
			miredUrn.rules[0].localAngles = new Vector3(350.72F, 86.19F, 359.55F);
			miredUrn.rules[0].localScale = new Vector3(0.1F, 0.12F, 0.1F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = miredUrnRules });
			
			/*keyAsset = await LoadAsset<ItemDef>("RoR2/Base/RoboBallBuddy/RoboBallBuddy.asset"); //huh
			var empathyCores = idrs.FindDisplayRuleGroup(keyAsset);
			var empathyCoresRules = new ItemDisplayRule[empathyCores.rules.Length];
			Array.Copy(empathyCores.rules, empathyCoresRules, empathyCores.rules.Length);
			empathyCores.rules = empathyCoresRules;
			empathyCores.rules[0].childName = "MuzzleCenter";
			empathyCores.rules[0].localPos = new Vector3(0.06302F, -0.31085F, 0.46304F);
			empathyCores.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			empathyCores.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = empathyCoresRules });*/
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/ParentEgg/ParentEgg.asset");
			var planula = idrs.FindDisplayRuleGroup(keyAsset);
			var planulaRules = new ItemDisplayRule[planula.rules.Length];
			Array.Copy(planula.rules, planulaRules, planula.rules.Length);
			planula.rules = planulaRules;
			planula.rules[0].childName = "U_Sash";
			planula.rules[0].localPos = new Vector3(-0.059F, 0.09F, 0.016F);
			planula.rules[0].localAngles = new Vector3(359.48F, 292.14F, 183.36F);
			planula.rules[0].localScale = new Vector3(0.07F, 0.07F, 0.07F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = planulaRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/MinorConstructOnKill/MinorConstructOnKill.asset");
			var XiBossItem = idrs.FindDisplayRuleGroup(keyAsset);
			var XiBossItemRules = new ItemDisplayRule[XiBossItem.rules.Length];
			Array.Copy(XiBossItem.rules, XiBossItemRules, XiBossItem.rules.Length);
			XiBossItem.rules = XiBossItemRules;
			XiBossItem.rules[0].childName = "MuzzleCenter";
			XiBossItem.rules[0].localPos = new Vector3(1.82F, 0.488F, -1.47F);
			XiBossItem.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			XiBossItem.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = XiBossItemRules });
			
			//Void Items
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/CloverVoid/CloverVoid.asset");
			var benthicBloom = idrs.FindDisplayRuleGroup(keyAsset);
			var benthicBloomRules = new ItemDisplayRule[benthicBloom.rules.Length];
			Array.Copy(benthicBloom.rules, benthicBloomRules, benthicBloom.rules.Length);
			benthicBloom.rules = benthicBloomRules;
			benthicBloom.rules[0].childName = "S_Head";
			benthicBloom.rules[0].localPos = new Vector3(-0.127F, 0.368F, 0.067F);
			benthicBloom.rules[0].localAngles = new Vector3(47.48F, 312.88F, 10F);
			benthicBloom.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = benthicBloomRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/TreasureCacheVoid/TreasureCacheVoid.asset");
			var keyVoid = idrs.FindDisplayRuleGroup(keyAsset);
			var keyVoidRules = new ItemDisplayRule[keyVoid.rules.Length];
			Array.Copy(keyVoid.rules, keyVoidRules, keyVoid.rules.Length);
			keyVoid.rules = keyVoidRules;
			keyVoid.rules[0].childName = "U_Sash";
			keyVoid.rules[0].localPos = new Vector3(-0.031F, 0.203F, -0.047F);
			keyVoid.rules[0].localAngles = new Vector3(70.1F, 290.95F, 179.09F);
			keyVoid.rules[0].localScale = new Vector3(0.8F, 0.8F, 0.8F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = keyVoidRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/CritGlassesVoid/CritGlassesVoid.asset");
			var seerLens = idrs.FindDisplayRuleGroup(keyAsset);
			var seerLensRules = new ItemDisplayRule[seerLens.rules.Length];
			Array.Copy(seerLens.rules, seerLensRules, seerLens.rules.Length);
			seerLens.rules = seerLensRules;
			seerLens.rules[0].childName = "U_Head";
			seerLens.rules[0].localPos = new Vector3(0.006F, 0.111F, 0.185F);
			seerLens.rules[0].localAngles = new Vector3(13.9F, 0F, 0F);
			seerLens.rules[0].localScale = new Vector3(0.4F, 0.35F, 0.4F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = seerLensRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/EquipmentMagazineVoid/EquipmentMagazineVoid.asset");
			var lysateCell = idrs.FindDisplayRuleGroup(keyAsset);
			var lysateCellRules = new ItemDisplayRule[lysateCell.rules.Length];
			Array.Copy(lysateCell.rules, lysateCellRules, lysateCell.rules.Length);
			lysateCell.rules = lysateCellRules;
			lysateCell.rules[0].childName = "S_Sash";
			lysateCell.rules[0].localPos = new Vector3(0.036F, -0.263F, -0.05F);
			lysateCell.rules[0].localAngles = new Vector3(353.65F, 202.47F, 163.09F);
			lysateCell.rules[0].localScale = new Vector3(0.2F, 0.12F, 0.2F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = lysateCellRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/BleedOnHitVoid/BleedOnHitVoid.asset");
			var needletick = idrs.FindDisplayRuleGroup(keyAsset);
			var needletickRules = new ItemDisplayRule[needletick.rules.Length];
			Array.Copy(needletick.rules, needletickRules, needletick.rules.Length);
			needletick.rules = needletickRules;
			needletick.rules[0].childName = "U_Head";
			needletick.rules[0].localPos = new Vector3(-0.002F, 0.451F, -0.02F);
			needletick.rules[0].localAngles = new Vector3(297.4F, 180F, 180F);
			needletick.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = needletickRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/VoidMegaCrabItem.asset");
			var zoea = idrs.FindDisplayRuleGroup(keyAsset);
			var zoeaRules = new ItemDisplayRule[zoea.rules.Length];
			Array.Copy(zoea.rules, zoeaRules, zoea.rules.Length);
			zoea.rules = zoeaRules;
			zoea.rules[0].childName = "U_Head";
			zoea.rules[0].localPos = new Vector3(0.13F, 0.428F, 0.021F);
			zoea.rules[0].localAngles = new Vector3(62.68F, 274.17F, 273.71F);
			zoea.rules[0].localScale = new Vector3(0.1F, 0.1F, 0.1F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = zoeaRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/MissileVoid/MissileVoid.asset");
			var plimp = idrs.FindDisplayRuleGroup(keyAsset);
			var plimpRules = new ItemDisplayRule[plimp.rules.Length];
			Array.Copy(plimp.rules, plimpRules, plimp.rules.Length);
			plimp.rules = plimpRules;
			plimp.rules[0].childName = "S_Head";
			plimp.rules[0].localPos = new Vector3(0.154F, -0.135F, -0.039F);
			plimp.rules[0].localAngles = new Vector3(18.95F, 5.67F, 267.98F);
			plimp.rules[0].localScale = new Vector3(0.08F, 0.08F, 0.08F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = plimpRules });

			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/ChainLightningVoid/ChainLightningVoid.asset");
			var lightningVoid = idrs.FindDisplayRuleGroup(keyAsset);
			var lightningVoidRules = new ItemDisplayRule[lightningVoid.rules.Length];
			Array.Copy(lightningVoid.rules, lightningVoidRules, lightningVoid.rules.Length);
			lightningVoid.rules = lightningVoidRules;
			lightningVoid.rules[0].childName = "S_Waist";
			lightningVoid.rules[0].localPos = new Vector3(-0.212F, 0.27F, 0.139F);
			lightningVoid.rules[0].localAngles = new Vector3(342.02F, 316.62F, 325.25F);
			lightningVoid.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = lightningVoidRules });

			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/ElementalRingVoid/ElementalRingVoid.asset");
			var ringVoid = idrs.FindDisplayRuleGroup(keyAsset);
			var ringVoidRules = new ItemDisplayRule[ringVoid.rules.Length];
			Array.Copy(ringVoid.rules, ringVoidRules, ringVoid.rules.Length);
			ringVoid.rules = ringVoidRules;
			ringVoid.rules[0].childName = "S_RingF";
			ringVoid.rules[0].localPos = new Vector3(-0.001F, 0.007F, 0.008F);
			ringVoid.rules[0].localAngles = new Vector3(84.96F, 128.64F, 124.49F);
			ringVoid.rules[0].localScale = new Vector3(0.2F, 0.2F, 0.2F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = ringVoidRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/SlowOnHitVoid/SlowOnHitVoid.asset");
			var tenta = idrs.FindDisplayRuleGroup(keyAsset);
			var tentaRules = new ItemDisplayRule[tenta.rules.Length];
			Array.Copy(tenta.rules, tentaRules, tenta.rules.Length);
			tenta.rules = tentaRules;
			tenta.rules[0].childName = "S_Wrist2";
			tenta.rules[0].localPos = new Vector3(0.131F, 0.051F, 0.095F);
			tenta.rules[0].localAngles = new Vector3(282.84F, 42.74F, 318.45F);
			tenta.rules[0].localScale = new Vector3(0.2F, 0.2F, 0.2F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = tentaRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/ExplodeOnDeathVoid/ExplodeOnDeathVoid.asset");
			var voidsent = idrs.FindDisplayRuleGroup(keyAsset);
			var voidsentRules = new ItemDisplayRule[voidsent.rules.Length];
			Array.Copy(voidsent.rules, voidsentRules, voidsent.rules.Length);
			voidsent.rules = voidsentRules;
			voidsent.rules[0].childName = "S_Waist";
			voidsent.rules[0].localPos = new Vector3(-0.338F, 0.12F, 0.096F);
			voidsent.rules[0].localAngles = new Vector3(20.87F, 153.34F, 4.25F);
			voidsent.rules[0].localScale = new Vector3(0.08F, 0.08F, 0.08F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = voidsentRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/MushroomVoid/MushroomVoid.asset");
			var sprintHeal = idrs.FindDisplayRuleGroup(keyAsset);
			var sprintHealRules = new ItemDisplayRule[sprintHeal.rules.Length];
			Array.Copy(sprintHeal.rules, sprintHealRules, sprintHeal.rules.Length);
			sprintHeal.rules = sprintHealRules;
			sprintHeal.rules[0].childName = "S_Head";
			sprintHeal.rules[0].localPos = new Vector3(0.003F, 0.397F, -0.06F);
			sprintHeal.rules[0].localAngles = new Vector3(14.59F, 140.66F, 355.08F);
			sprintHeal.rules[0].localScale = new Vector3(0.07F, 0.07F, 0.07F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = sprintHealRules });
			
			//Seekers Items
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/BoostAllStats/BoostAllStats.asset");
			var stats = idrs.FindDisplayRuleGroup(keyAsset);
			var statsRules = new ItemDisplayRule[stats.rules.Length];
			Array.Copy(stats.rules, statsRules, stats.rules.Length);
			stats.rules = statsRules;
			stats.rules[0].childName = "S_Head";
			stats.rules[0].localPos = new Vector3(0.006F, 0.256F, 0.164F);
			stats.rules[0].localAngles = new Vector3(37.43F, 171.25F, 352.64F);
			stats.rules[0].localScale = new Vector3(0.6F, 0.6F, 0.6F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = statsRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/DelayedDamage/DelayedDamage.asset");
			var deathsDance = idrs.FindDisplayRuleGroup(keyAsset);
			var deathsDanceRules = new ItemDisplayRule[deathsDance.rules.Length];
			Array.Copy(deathsDance.rules, deathsDanceRules, deathsDance.rules.Length);
			deathsDance.rules = deathsDanceRules;
			deathsDance.rules[0].childName = "U_Chest";
			deathsDance.rules[0].localPos = new Vector3(0.004F, 0.218F, 0.192F);
			deathsDance.rules[0].localAngles = new Vector3(331.93F, 0F, 0F);
			deathsDance.rules[0].localScale = new Vector3(0.2F, 0.2F, 0.2F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = deathsDanceRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/ExtraShrineItem/ExtraShrineItem.asset");
			var chanceDoll = idrs.FindDisplayRuleGroup(keyAsset);
			var chanceDollRules = new ItemDisplayRule[chanceDoll.rules.Length];
			Array.Copy(chanceDoll.rules, chanceDollRules, chanceDoll.rules.Length);
			chanceDoll.rules = chanceDollRules;
			chanceDoll.rules[0].childName = "U_Waist";
			chanceDoll.rules[0].localPos = new Vector3(-0.435F, 0.128F, 0.058F);
			chanceDoll.rules[0].localAngles = new Vector3(351.14F, 263.84F, 20.24F);
			chanceDoll.rules[0].localScale = new Vector3(0.2F, 0.2F, 0.2F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = chanceDollRules });

			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/IncreaseDamageOnMultiKill/IncreaseDamageOnMultiKill.asset");
			var item6 = idrs.FindDisplayRuleGroup(keyAsset);
			var item6Rules = new ItemDisplayRule[item6.rules.Length];
			Array.Copy(item6.rules, item6Rules, item6.rules.Length);
			item6.rules = item6Rules;
			item6.rules[0].childName = "S_Wrist";
			item6.rules[0].localPos = new Vector3(-0.016F, 0.105F, -0.029F);
			item6.rules[0].localAngles = new Vector3(275.22F, 0F, 180F);
			item6.rules[0].localScale = new Vector3(0.1F, 0.1F, 0.1F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item6Rules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/IncreasePrimaryDamage/IncreasePrimaryDamage.asset");
			var item7 = idrs.FindDisplayRuleGroup(keyAsset);
			var item7Rules = new ItemDisplayRule[item7.rules.Length];
			Array.Copy(item7.rules, item7Rules, item7.rules.Length);
			item7.rules = item7Rules;
			item7.rules[0].childName = "U_Wrist2";
			item7.rules[0].localPos = new Vector3(-0.001F, -0.001F, -0.06F);
			item7.rules[0].localAngles = new Vector3(6.87F, 181.72F, 75.01F);
			item7.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item7Rules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/KnockBackHitEnemies/KnockBackHitEnemies.asset");
			var item8 = idrs.FindDisplayRuleGroup(keyAsset);
			var item8Rules = new ItemDisplayRule[item8.rules.Length];
			Array.Copy(item8.rules, item8Rules, item8.rules.Length);
			item8.rules = item8Rules;
			item8.rules[0].childName = "U_Chest";
			item8.rules[0].localPos = new Vector3(-0.008F, 0.241F, -0.078F);
			item8.rules[0].localAngles = new Vector3(357.09F, 0F, 0F);
			item8.rules[0].localScale = new Vector3(0.7F, 1F, 1F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item8Rules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/LowerHealthHigherDamage/LowerHealthHigherDamage.asset");
			var item9 = idrs.FindDisplayRuleGroup(keyAsset);
			var item9Rules = new ItemDisplayRule[item9.rules.Length];
			Array.Copy(item9.rules, item9Rules, item9.rules.Length);
			item9.rules = item9Rules;
			item9.rules[0].childName = "S_Waist";
			item9.rules[0].localPos = new Vector3(0.385F, 0.158F, 0.085F);
			item9.rules[0].localAngles = new Vector3(11.21F, 232.1F, 353.99F);
			item9.rules[0].localScale = new Vector3(0.9F, 0.9F, 0.9F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item9Rules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/LowerPricedChests/LowerPricedChests.asset");
			var item10 = idrs.FindDisplayRuleGroup(keyAsset);
			var item10Rules = new ItemDisplayRule[item10.rules.Length];
			Array.Copy(item10.rules, item10Rules, item10.rules.Length);
			item10.rules = item10Rules;
			item10.rules[0].childName = "MuzzleCenter";
			item10.rules[0].localPos = new Vector3(0.909F, 1.414F, 0.328F);
			item10.rules[0].localAngles = new Vector3(270F, 0F, 0F);
			item10.rules[0].localScale = new Vector3(0.6F, 0.6F, 0.6F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item10Rules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/MeteorAttackOnHighDamage/MeteorAttackOnHighDamage.asset");
			var item11 = idrs.FindDisplayRuleGroup(keyAsset);
			var item11Rules = new ItemDisplayRule[item11.rules.Length];
			Array.Copy(item11.rules, item11Rules, item11.rules.Length);
			item11.rules = item11Rules;
			item11.rules[0].childName = "U_Head";
			item11.rules[0].localPos = new Vector3(-0.135F, 0.399F, 0.009F);
			item11.rules[0].localAngles = new Vector3(358.08F, 3.69F, 26.84F);
			item11.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item11Rules });
			
			/*keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/NegateAttack/NegateAttack.asset"); //huh
			var item12 = idrs.FindDisplayRuleGroup(keyAsset);
			var item12Rules = new ItemDisplayRule[item12.rules.Length];
			Array.Copy(item12.rules, item12Rules, item12.rules.Length);
			item12.rules = item12Rules;
			item12.rules[0].childName = "MuzzleCenter";
			item12.rules[0].localPos = new Vector3(0.06302F, -0.31085F, 0.46304F);
			item12.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			item12.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item12Rules });*/
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/OnLevelUpFreeUnlock/OnLevelUpFreeUnlock.asset");
			var item13 = idrs.FindDisplayRuleGroup(keyAsset);
			var item13Rules = new ItemDisplayRule[item13.rules.Length];
			Array.Copy(item13.rules, item13Rules, item13.rules.Length);
			item13.rules = item13Rules;
			item13.rules[0].childName = "S_Waist";
			item13.rules[0].localPos = new Vector3(-0.184F, 0.19F, 0.212F);
			item13.rules[0].localAngles = new Vector3(336.37F, 325.4F, 13.21F);
			item13.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			item13.rules[1].childName = "MuzzleCenter";
			item13.rules[1].localPos = new Vector3(1.193F, 0.785F, 0.238F);
			item13.rules[1].localAngles = new Vector3(0F, 15.58F, 0F);
			item13.rules[1].localScale = new Vector3(1F, 1F, 1F);
			
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item13Rules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/ResetChests/ResetChests.asset");
			var item14 = idrs.FindDisplayRuleGroup(keyAsset);
			var item14Rules = new ItemDisplayRule[item14.rules.Length];
			Array.Copy(item14.rules, item14Rules, item14.rules.Length);
			item14.rules = item14Rules;
			item14.rules[0].childName = "U_Waist";
			item14.rules[0].localPos = new Vector3(-0.339F, 0.208F, 0.179F);
			item14.rules[0].localAngles = new Vector3(349.19F, 285.08F, 8.76F);
			item14.rules[0].localScale = new Vector3(1.2F, 1.2F, 1.2F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item14Rules });

			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/TeleportOnLowHealth/TeleportOnLowHealth.asset");
			var item16 = idrs.FindDisplayRuleGroup(keyAsset);
			var item16Rules = new ItemDisplayRule[item16.rules.Length];
			Array.Copy(item16.rules, item16Rules, item16.rules.Length);
			item16.rules = item16Rules;
			item16.rules[0].childName = "S_Chest";
			item16.rules[0].localPos = new Vector3(0.008F, 0.17F, 0.22F);
			item16.rules[0].localAngles = new Vector3(329.03F, 0F, 0F);
			item16.rules[0].localScale = new Vector3(0.7F, 0.7F, 0.7F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item16Rules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC2/Items/TriggerEnemyDebuffs/TriggerEnemyDebuffs.asset");
			var item17 = idrs.FindDisplayRuleGroup(keyAsset);
			var item17Rules = new ItemDisplayRule[item17.rules.Length];
			Array.Copy(item17.rules, item17Rules, item17.rules.Length);
			item17.rules = item17Rules;
			item17.rules[0].childName = "S_Sash";
			item17.rules[0].localPos = new Vector3(0.011F, -0.152F, -0.01F);
			item17.rules[0].localAngles = new Vector3(345.24F, 195.12F, 264.67F);
			item17.rules[0].localScale = new Vector3(0.9F, 0.9F, 0.9F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = item17Rules });
			
			//"Followers" & Equipment
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/FocusConvergence/FocusConvergence.asset");
			var convergence = idrs.FindDisplayRuleGroup(keyAsset);
			var convergenceRules = new ItemDisplayRule[convergence.rules.Length];
			Array.Copy(convergence.rules, convergenceRules, convergence.rules.Length);
			convergence.rules = convergenceRules;
			convergence.rules[0].childName = "MuzzleCenter";
			convergence.rules[0].localPos = new Vector3(1.492F, 0.821F, 0.016F);
			convergence.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			convergence.rules[0].localScale = new Vector3(0.1F, 0.1F, 0.1F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = convergenceRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/DLC1/RandomlyLunar/RandomlyLunar.asset");
			var euology = idrs.FindDisplayRuleGroup(keyAsset);
			var euologyRules = new ItemDisplayRule[euology.rules.Length];
			Array.Copy(euology.rules, euologyRules, euology.rules.Length);
			euology.rules = euologyRules;
			euology.rules[0].childName = "MuzzleCenter";
			euology.rules[0].localPos = new Vector3(-1.817F, 0.635F, -0.271F);
			euology.rules[0].localAngles = new Vector3(281.03F, 0F, 0F);
			euology.rules[0].localScale = new Vector3(1F, 1F, 1F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = euologyRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/LunarSkillReplacements/LunarSpecialReplacement.asset");
			var essence = idrs.FindDisplayRuleGroup(keyAsset);
			var essenceRules = new ItemDisplayRule[essence.rules.Length];
			Array.Copy(essence.rules, essenceRules, essence.rules.Length);
			essence.rules = essenceRules;
			essence.rules[0].childName = "MuzzleCenter";
			essence.rules[0].localPos = new Vector3(-2.107F, 0.514F, -1.816F);
			essence.rules[0].localAngles = new Vector3(270F, 0F, 0F);
			essence.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = essenceRules });

			var equipAsset = await LoadAsset<EquipmentDef>("RoR2/Base/Blackhole/Blackhole.asset");
			var cube = idrs.FindDisplayRuleGroup(equipAsset);
			var cubeRules = new ItemDisplayRule[cube.rules.Length];
			Array.Copy(cube.rules, cubeRules, cube.rules.Length);
			cube.rules = cubeRules;
			cube.rules[0].childName = "MuzzleCenter";
			cube.rules[0].localPos = new Vector3(-0.078F, 0.694F, -1.665F);
			cube.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			cube.rules[0].localScale = new Vector3(0.7F, 0.7F, 0.7F);
			idrs.SetDisplayRuleGroup(equipAsset, new DisplayRuleGroup { rules = cubeRules });
			
			equipAsset = await LoadAsset<EquipmentDef>("RoR2/DLC1/BossHunter/BossHunter.asset");
			var pirateGun = idrs.FindDisplayRuleGroup(equipAsset);
			var pirateGunRules = new ItemDisplayRule[pirateGun.rules.Length];
			Array.Copy(pirateGun.rules, pirateGunRules, pirateGun.rules.Length);
			pirateGun.rules = pirateGunRules;
			pirateGun.rules[0].childName = "MuzzleRear";
			pirateGun.rules[0].localPos = new Vector3(-0.07F, 1.423F, 1.981F);
			pirateGun.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			pirateGun.rules[0].localScale = new Vector3(1F, 1F, 1F);
			pirateGun.rules[0].childName = "MuzzleCenter";
			pirateGun.rules[0].localPos = new Vector3(-0.007F, 0.43F, 0.247F);
			pirateGun.rules[0].localAngles = new Vector3(90F, 0F, 0F);
			pirateGun.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(equipAsset, new DisplayRuleGroup { rules = pirateGunRules });
			
			equipAsset = await LoadAsset<EquipmentDef>("RoR2/Base/CritOnUse/CritOnUse.asset");
			var HUD = idrs.FindDisplayRuleGroup(equipAsset);
			var HUDRules = new ItemDisplayRule[HUD.rules.Length];
			Array.Copy(HUD.rules, HUDRules, HUD.rules.Length);
			HUD.rules = HUDRules;
			HUD.rules[0].childName = "U_Head";
			HUD.rules[0].localPos = new Vector3(0.014F, 0.069F, 0.334F);
			HUD.rules[0].localAngles = new Vector3(0F, 3.36F, 0F);
			HUD.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			idrs.SetDisplayRuleGroup(equipAsset, new DisplayRuleGroup { rules = HUDRules });
			
			equipAsset = await LoadAsset<EquipmentDef>("RoR2/Base/GainArmor/GainArmor.asset");
			var elephant = idrs.FindDisplayRuleGroup(equipAsset);
			var elephantRules = new ItemDisplayRule[elephant.rules.Length];
			Array.Copy(elephant.rules, elephantRules, elephant.rules.Length);
			elephant.rules = elephantRules;
			elephant.rules[0].childName = "U_Waist";
			elephant.rules[0].localPos = new Vector3(0.332F, 0.153F, 0.164F);
			elephant.rules[0].localAngles = new Vector3(327.83F, 313.11F, 33.21F);
			elephant.rules[0].localScale = new Vector3(0.6F, 0.6F, 0.5F);
			idrs.SetDisplayRuleGroup(equipAsset, new DisplayRuleGroup { rules = elephantRules });
			
			/*equipAsset = await LoadAsset<EquipmentDef>("RoR2/DLC1/GummyClone/GummyClone.asset"); //huh
			var gooboo = idrs.FindDisplayRuleGroup(equipAsset);
			var goobooRules = new ItemDisplayRule[gooboo.rules.Length];
			Array.Copy(gooboo.rules, goobooRules, gooboo.rules.Length);
			gooboo.rules = goobooRules;
			gooboo.rules[0].childName = "MuzzleCenter";
			gooboo.rules[0].localPos = new Vector3(0.06302F, -0.31085F, 0.46304F);
			gooboo.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			gooboo.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(equipAsset, new DisplayRuleGroup { rules = goobooRules });*/

			equipAsset = await LoadAsset<EquipmentDef>("RoR2/DLC1/LunarPortalOnUse/LunarPortalOnUse.asset");
			var littleGuy = idrs.FindDisplayRuleGroup(equipAsset);
			var littleGuyRules = new ItemDisplayRule[littleGuy.rules.Length];
			Array.Copy(littleGuy.rules, littleGuyRules, littleGuy.rules.Length);
			littleGuy.rules = littleGuyRules;
			littleGuy.rules[0].childName = "MuzzleCenter";
			littleGuy.rules[0].localPos = new Vector3(-1.954F, 0.779F, -0.239F);
			littleGuy.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			littleGuy.rules[0].localScale = new Vector3(1F, 1F, 1F);
			idrs.SetDisplayRuleGroup(equipAsset, new DisplayRuleGroup { rules = littleGuyRules });
			
			equipAsset = await LoadAsset<EquipmentDef>("RoR2/Base/Meteor/Meteor.asset");
			var meteor = idrs.FindDisplayRuleGroup(equipAsset);
			var meteorRules = new ItemDisplayRule[meteor.rules.Length];
			Array.Copy(meteor.rules, meteorRules, meteor.rules.Length);
			meteor.rules = meteorRules;
			meteor.rules[0].childName = "MuzzleCenter";
			meteor.rules[0].localPos = new Vector3(-0.116F, 0.759F, -2.009F);
			meteor.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			meteor.rules[0].localScale = new Vector3(0.7F, 0.7F, 0.7F);
			idrs.SetDisplayRuleGroup(equipAsset, new DisplayRuleGroup { rules = meteorRules });
			
			equipAsset = await LoadAsset<EquipmentDef>("RoR2/Base/Saw/Saw.asset");
			var saw = idrs.FindDisplayRuleGroup(equipAsset);
			var sawRules = new ItemDisplayRule[saw.rules.Length];
			Array.Copy(saw.rules, sawRules, saw.rules.Length);
			saw.rules = sawRules;
			saw.rules[0].childName = "MuzzleCenter";
			saw.rules[0].localPos = new Vector3(-1.458F, 0.448F, 0.109F);
			saw.rules[0].localAngles = new Vector3(90F, 0F, 0F);
			saw.rules[0].localScale = new Vector3(0.1F, 0.1F, 0.1F);
			idrs.SetDisplayRuleGroup(equipAsset, new DisplayRuleGroup { rules = sawRules });

			//Other Items That Would Look Good 
			/*keyAsset = await LoadAsset<ItemDef>("RoR2/Base/BarrierOnKill/BarrierOnKill.asset"); //left off here
			var topaz = idrs.FindDisplayRuleGroup(keyAsset);
			var topazRules = new ItemDisplayRule[topaz.rules.Length];
			Array.Copy(topaz.rules, topazRules, topaz.rules.Length);
			topaz.rules = topazRules;
			topaz.rules[0].childName = "MuzzleCenter";
			topaz.rules[0].localPos = new Vector3(0.06302F, -0.31085F, 0.46304F);
			topaz.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			topaz.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = topazRules });*/
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonus.asset");
			var crystal = idrs.FindDisplayRuleGroup(keyAsset);
			var crystalRules = new ItemDisplayRule[crystal.rules.Length];
			Array.Copy(crystal.rules, crystalRules, crystal.rules.Length);
			crystal.rules = crystalRules;
			crystal.rules[0].childName = "U_Chest";
			crystal.rules[0].localPos = new Vector3(0.007F, 0.285F, 0.155F);
			crystal.rules[0].localAngles = new Vector3(339.89F, 359.62F, 0.96F);
			crystal.rules[0].localScale = new Vector3(0.04F, 0.04F, 0.04F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = crystalRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Mushroom/Mushroom.asset");
			var bungus = idrs.FindDisplayRuleGroup(keyAsset);
			var bungusRules = new ItemDisplayRule[bungus.rules.Length];
			Array.Copy(bungus.rules, bungusRules, bungus.rules.Length);
			bungus.rules = bungusRules;
			bungus.rules[0].childName = "S_Head";
			bungus.rules[0].localPos = new Vector3(0.004F, 0.42F, -0.06F);
			bungus.rules[0].localAngles = new Vector3(14.59F, 140.66F, 355.08F);
			bungus.rules[0].localScale = new Vector3(0.07F, 0.07F, 0.07F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = bungusRules });

			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/ExplodeOnDeath/ExplodeOnDeath.asset");
			var willowisp = idrs.FindDisplayRuleGroup(keyAsset);
			var willowispRules = new ItemDisplayRule[willowisp.rules.Length];
			Array.Copy(willowisp.rules, willowispRules, willowisp.rules.Length);
			willowisp.rules = willowispRules;
			willowisp.rules[0].childName = "S_Waist";
			willowisp.rules[0].localPos = new Vector3(-0.338F, 0.12F, 0.096F);
			willowisp.rules[0].localAngles = new Vector3(20.87F, 153.34F, 4.25F);
			willowisp.rules[0].localScale = new Vector3(0.08F, 0.08F, 0.08F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = willowispRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/Feather/Feather.asset");
			var hopoo = idrs.FindDisplayRuleGroup(keyAsset);
			var hopooRules = new ItemDisplayRule[hopoo.rules.Length];
			Array.Copy(hopoo.rules, hopooRules, hopoo.rules.Length);
			hopoo.rules = hopooRules;
			hopoo.rules[0].childName = "U_Head";
			hopoo.rules[0].localPos = new Vector3(-0.026F, 0.308F, -0.124F);
			hopoo.rules[0].localAngles = new Vector3(0F, 0F, 0F);
			hopoo.rules[0].localScale = new Vector3(0.04F, 0.04F, 0.04F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = hopooRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/ElementalRings/FireRing.asset");
			var fireRing = idrs.FindDisplayRuleGroup(keyAsset);
			var fireRingRules = new ItemDisplayRule[fireRing.rules.Length];
			Array.Copy(fireRing.rules, fireRingRules, fireRing.rules.Length);
			fireRing.rules = fireRingRules;
			fireRing.rules[0].childName = "S_RingF";
			fireRing.rules[0].localPos = new Vector3(-0.001F, 0.007F, 0.008F);
			fireRing.rules[0].localAngles = new Vector3(84.96F, 128.64F, 124.49F);
			fireRing.rules[0].localScale = new Vector3(0.2F, 0.2F, 0.2F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = fireRingRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/ElementalRings/IceRing.asset");
			var iceRing = idrs.FindDisplayRuleGroup(keyAsset);
			var iceRingRules = new ItemDisplayRule[iceRing.rules.Length];
			Array.Copy(iceRing.rules, iceRingRules, iceRing.rules.Length);
			iceRing.rules = iceRingRules;
			iceRing.rules[0].childName = "U_RingF";
			iceRing.rules[0].localPos = new Vector3(-0.001F, 0.007F, 0.008F);
			iceRing.rules[0].localAngles = new Vector3(84.96F, 128.64F, 124.49F);
			iceRing.rules[0].localScale = new Vector3(0.2F, 0.2F, 0.2F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = iceRingRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/TPHealingNova/TPHealingNova.asset");
			var daisy = idrs.FindDisplayRuleGroup(keyAsset);
			var daisyRules = new ItemDisplayRule[daisy.rules.Length];
			Array.Copy(daisy.rules, daisyRules, daisy.rules.Length);
			daisy.rules = daisyRules;
			daisy.rules[0].childName = "U_Head";
			daisy.rules[0].localPos = new Vector3(-0.05F, 0.381F, -0.125F);
			daisy.rules[0].localAngles = new Vector3(309.68F, 210.35F, 168.94F);
			daisy.rules[0].localScale = new Vector3(0.5F, 0.5F, 0.5F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = daisyRules });
			
			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/BonusGoldPackOnKill/BonusGoldPackOnKill.asset");
			var tome = idrs.FindDisplayRuleGroup(keyAsset);
			var tomeRules = new ItemDisplayRule[tome.rules.Length];
			Array.Copy(tome.rules, tomeRules, tome.rules.Length);
			tome.rules = tomeRules;
			tome.rules[0].childName = "U_Waist";
			tome.rules[0].localPos = new Vector3(-0.391F, 0.098F, 0.059F);
			tome.rules[0].localAngles = new Vector3(351.82F, 262.43F, 19.8F);
			tome.rules[0].localScale = new Vector3(0.1F, 0.1F, 0.1F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = tomeRules });

			keyAsset = await LoadAsset<ItemDef>("RoR2/Base/DeathMark/DeathMark.asset");
			var mark = idrs.FindDisplayRuleGroup(keyAsset);
			var markRules = new ItemDisplayRule[mark.rules.Length];
			Array.Copy(mark.rules, markRules, mark.rules.Length);
			mark.rules = markRules;
			mark.rules[0].childName = "U_Wrist2";
			mark.rules[0].localPos = new Vector3(-0.018F, 0.102F, -0.032F);
			mark.rules[0].localAngles = new Vector3(81.25F, 100.57F, 99.54F);
			mark.rules[0].localScale = new Vector3(0.02F, 0.02F, 0.02F);
			idrs.SetDisplayRuleGroup(keyAsset, new DisplayRuleGroup { rules = markRules });
			
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
			//passiveSkill.hideInCharacterSelect = family.variants.Length == 1;

			SetStateOnHurt timesweeper = bodyPrefab.GetComponent<SetStateOnHurt>();
			timesweeper.targetStateMachine = bodyStateMachine;
			timesweeper.idleStateMachine = new[] { weaponStateMachine, hoverStateMachine, spellStateMachine };

			skillLocator.passiveSkill = new SkillLocator.PassiveSkill
			{
				enabled = true,
				icon = await LoadAsset<Sprite>("kamunagiassets2:EverSpinning"),
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