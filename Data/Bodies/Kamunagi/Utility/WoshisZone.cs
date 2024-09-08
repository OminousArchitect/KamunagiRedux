using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class WoshisZoneState : IndicatorSpellState
	{
		public override float duration => 0.45f;
		public override float failedCastCooldown => 0f;
		public override float indicatorScale => 10f;

		public override void Fire(Vector3 targetPosition)
		{
			base.Fire(targetPosition);
			if (twinBehaviour.activeBuffWard)
			{
				NetworkServer.Destroy(twinBehaviour.activeBuffWard);
			}

			var ward = Object.Instantiate(Asset.GetGameObject<WoshisZone, INetworkedObject>(), targetPosition,
				Quaternion.identity);
			ward.GetComponent<TeamFilter>().teamIndex = TeamIndex.Monster;
			twinBehaviour.activeBuffWard = ward;
			NetworkServer.Spawn(ward);
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class WoshisZone : Asset, ISkill, INetworkedObject, IBuff, IItem, IMaterialSwap
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Utility 5";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "UTILITY3_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "UTILITY3_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle:Woshis");
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 4f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			return skill;
		}

		Type[] ISkill.GetEntityStates() => new[] { typeof(WoshisZoneState) };

		GameObject INetworkedObject.BuildObject()
		{
			var woshisWard =
				LoadAsset<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab")!.InstantiateClone("WoshisWard",
					true);
			var woshisEnergy =
				new Material(
					LoadAsset<Material>("RoR2/Base/BleedOnHitAndExplode/matBleedOnHitAndExplodeAreaIndicator.mat"));
			woshisEnergy.SetFloat("_DstBlendFloat", 3f);
			woshisEnergy.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampImp2.png"));
			woshisEnergy.SetFloat("_Boost", 0.1f);
			woshisEnergy.SetFloat("_RimPower", 0.48f);
			woshisEnergy.SetFloat("_RimStrength", 0.12f);
			woshisEnergy.SetFloat("_AlphaBoost", 6.55f);
			woshisEnergy.SetFloat("_IntersectionStrength", 5.12f);

			Object.Destroy(woshisWard.GetComponent<NetworkedBodyAttachment>());
			woshisWard.GetComponentInChildren<MeshRenderer>().material = woshisEnergy;
			var ward = woshisWard.GetComponent<BuffWard>();
			ward.radius = 10f;
			ward.buffDef = (BuffDef)this;
			woshisWard.AddComponent<DestroyOnTimer>().duration = 8f;

			return woshisWard;
		}

		BuffDef IBuff.BuildObject()
		{
			var buffDef = ScriptableObject.CreateInstance<BuffDef>();
			buffDef.name = "WoshisCurseDebuff";
			buffDef.buffColor = Color.red;
			buffDef.canStack = false;
			buffDef.isDebuff = true;
			buffDef.iconSprite = LoadAsset<Sprite>("bundle:CurseScroll");
			buffDef.isHidden = true;

			return buffDef;
		}

		ItemDef IItem.BuildObject()
		{
			var customGhostItem = ScriptableObject.CreateInstance<ItemDef>();
			customGhostItem.name = "NINES_WOSHISGHOST_NAME";
			customGhostItem.nameToken = "NINES_WOSHISGHOST_NAME";
			customGhostItem.pickupToken = "NINES_WOSHISGHOST_PICKUP";
			customGhostItem.descriptionToken = "NINES_WOSHISGHOST_DESC";
			customGhostItem.loreToken = "NINES_WOSHISGHOST_LORE";
			customGhostItem.tier = ItemTier.NoTier;
			customGhostItem.pickupIconSprite = LoadAsset<Sprite>("RoR2/Base/Beetle/texBuffBeetleJuiceIcon.tif");
			customGhostItem.pickupModelPrefab = LoadAsset<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab");
			customGhostItem.canRemove = false;
			customGhostItem.hidden = true;
			return customGhostItem;
		}


		//[HarmonyPrefix, HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.UpdateRendererMaterials))]
		private static void CharacterModelUpdateRenderers(CharacterModel __instance)
		{
			if (!__instance.body || !__instance.body.inventory) return;
			// ReSharper disable once InconsistentNaming
			var _this = GetAsset<WoshisZone>();
			if (__instance.body.inventory.GetItemCount(_this) <= 0) return;
			for (var i = 0; i < __instance.baseRendererInfos.Length; i++)
			{
				var renderInfo = __instance.baseRendererInfos[i];
				if (!renderInfo.renderer.GetComponent<ParticleSystemRenderer>())
				{
					renderInfo.defaultMaterial = _this; //jesus fucking christ, this was the solution
				}
				else
				{
					renderInfo.defaultMaterial = GetAsset<WoshisZoneWispGhost>();
				}

				__instance.baseRendererInfos[i] = renderInfo;
			}
		}


		public WoshisZone() => GlobalEventManager.onCharacterDeathGlobal += CharacterDeath;

		~WoshisZone() => GlobalEventManager.onCharacterDeathGlobal -= CharacterDeath;

		public void CharacterDeath(DamageReport report)
		{
			if (!NetworkServer.active || !report.victimBody || !report.victimBody.HasBuff(this) ||
			    report.victimBody.inventory.GetItemCount(this) > 0) return;

			var prefab = BodyCatalog.FindBodyPrefab(report.victimBody);
			if (prefab == null || !prefab) return;

			var masterIndex = MasterCatalog.FindAiMasterIndexForBody(prefab.GetComponent<CharacterBody>().bodyIndex);
			if (masterIndex == MasterCatalog.MasterIndex.none) return;
			var masterPrefab = MasterCatalog.GetMasterPrefab(masterIndex);

			var victimTransform = report.victimBody.transform;
			var direction = report.victimBody.GetComponent<CharacterDirection>();

			var summon = new MasterSummon
			{
				masterPrefab = masterPrefab,
				ignoreTeamMemberLimit = false,
				useAmbientLevel = true,
				inventoryToCopy = report.victimBody.inventory,
				position = victimTransform.position,
				rotation = direction ? Quaternion.Euler(0f, direction.yaw, 0f) : victimTransform.rotation,
				summonerBodyObject = report.attacker ? report.attacker : null,
				teamIndexOverride = report.attackerBody
					? report.attackerBody.teamComponent.teamIndex
					: TeamIndex.Player
			};

			summon.preSpawnSetupCallback += master =>
			{
				master.inventory.GiveItem(this);
				master.inventory.GiveItem(RoR2Content.Items.HealthDecay, 15);
				master.inventory.GiveItem(RoR2Content.Items.BoostDamage, 10);
			};
			var master = summon.Perform();
			if (!master) return;
			var summonBody = master.GetBody();
			if (!summonBody) return;
			foreach (var esm in summonBody.GetComponents<EntityStateMachine>())
			{
				esm.initialStateType = esm.mainStateType;
			}
		}

		Material IMaterialSwap.BuildObject()
		{
			var woshisGhostOverlay = new Material(LoadAsset<Material>("RoR2/Base/Common/VFX/matGhostEffect.mat"));
			woshisGhostOverlay.SetTexture("_RemapTex", LoadAsset<Texture2D>("bundle:texRampWoshis"));
			return woshisGhostOverlay;
		}
		
		public bool CheckEnabled(CharacterModel model, CharacterModel.RendererInfo targetRendererInfo) => !targetRendererInfo.ignoreOverlays && model.body && model.body.inventory && model.body.inventory.GetItemCount(this) > 0;

		public int Priority => 1;
	}

	public class WoshisZoneWispGhost : Asset, IMaterialSwap
	{
		Material IMaterialSwap.BuildObject()
		{
			var redWispMat = new Material(LoadAsset<Material>("RoR2/Base/Wisp/matWispFire.mat"));
			redWispMat.SetFloat("_BrightnessBoost", 2.63f);
			redWispMat.SetFloat("_AlphaBoost", 1.2f);
			redWispMat.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
			redWispMat.SetColor("_TintColor", Color.red);
			return redWispMat;
		}

		public bool CheckEnabled(CharacterModel model, CharacterModel.RendererInfo targetRendererInfo) => targetRendererInfo.ignoreOverlays && model.body && model.body.inventory &&
		                                                                            model.body.inventory.GetItemCount(GetAsset<WoshisZone>()) > 0;

		public int Priority => 1;
	}
}