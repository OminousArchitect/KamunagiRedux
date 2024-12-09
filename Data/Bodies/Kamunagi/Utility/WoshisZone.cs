using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using Console = System.Console;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Utility
{
	public class SpawnWoshisWard : BaseTwinState
	{
		public Vector3 position;

		public override void OnEnter()
		{
			base.OnEnter();
			if (!NetworkServer.active) return;

			var ward = Object.Instantiate(Concentric.GetNetworkedObject<WoshisZone>().WaitForCompletion(), position, Quaternion.identity);
			ward.GetComponent<TeamFilter>().teamIndex = TeamIndex.Monster;
			NetworkServer.Spawn(ward);
		}

		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write(position);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			position = reader.ReadVector3();
		}
	}
	public class WoshisZoneState : IndicatorSpellState
	{
		public override float duration => 90f;
		public override float failedCastCooldown => 0f;
		public override float indicatorScale => 11f;
		public override int meterGain => 0;

		public override void Fire(Vector3 targetPosition)
		{
			outer.SetNextState(new SpawnWoshisWard() { position = targetPosition });
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class WoshisZone : Concentric, ISkill, INetworkedObject, IBuff, IItem, IMaterialSwap
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Utility 5";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "UTILITY3_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "UTILITY3_DESCRIPTION";
			skill.icon = await LoadAsset<Sprite>("kamunagiassets:Woshis");
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 9f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.interruptPriority = InterruptPriority.Any;
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(WoshisZoneState), (typeof(SpawnWoshisWard))};

		async Task<GameObject> INetworkedObject.BuildObject()
		{
			var woshisWard =
				(await LoadAsset<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab"))!.InstantiateClone("WoshisWard",
					true);
			var woshisEnergy =
				new Material(
					await LoadAsset<Material>("RoR2/Base/BleedOnHitAndExplode/matBleedOnHitAndExplodeAreaIndicator.mat"));
			woshisEnergy.SetFloat("_DstBlendFloat", 3f);
			woshisEnergy.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampImp2.png"));
			woshisEnergy.SetFloat("_Boost", 0.065f);
			woshisEnergy.SetFloat("_RimPower", 0.48f);
			woshisEnergy.SetFloat("_RimStrength", 0.12f);
			woshisEnergy.SetFloat("_AlphaBoost", 6.55f);
			woshisEnergy.SetFloat("_IntersectionStrength", 3.2f);

			Object.Destroy(woshisWard.GetComponent<NetworkedBodyAttachment>());
			woshisWard.GetComponentInChildren<MeshRenderer>().material = woshisEnergy;
			var ward = woshisWard.GetComponent<BuffWard>();
			ward.radius = 11f;
			ward.buffDef = await this.GetBuffDef();
			woshisWard.AddComponent<DestroyOnTimer>().duration = 8f;

			return woshisWard;
		}

		async Task<BuffDef> IBuff.BuildObject()
		{
			var buffDef = ScriptableObject.CreateInstance<BuffDef>();
			buffDef.name = "WoshisCurseDebuff";
			buffDef.buffColor = Color.red;
			buffDef.canStack = false;
			buffDef.isDebuff = true;
			buffDef.iconSprite = await LoadAsset<Sprite>("kamunagiassets:CurseScroll");
			buffDef.isHidden = true;

			return buffDef;
		}

		async Task<ItemDef> IItem.BuildObject()
		{
			var customGhostItem = ScriptableObject.CreateInstance<ItemDef>();
			customGhostItem.name = "NINES_WOSHISGHOST_NAME";
			customGhostItem.nameToken = "NINES_WOSHISGHOST_NAME";
			customGhostItem.pickupToken = "NINES_WOSHISGHOST_PICKUP";
			customGhostItem.descriptionToken = "NINES_WOSHISGHOST_DESC";
			customGhostItem.loreToken = "NINES_WOSHISGHOST_LORE";
			customGhostItem.tier = ItemTier.NoTier;
			customGhostItem.deprecatedTier = ItemTier.NoTier;
			customGhostItem.pickupIconSprite = await LoadAsset<Sprite>("RoR2/Base/Beetle/texBuffBeetleJuiceIcon.tif");
			customGhostItem.pickupModelPrefab = await LoadAsset<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab");
			customGhostItem.canRemove = false;
			customGhostItem.hidden = true;
			return customGhostItem;
		}
		
		public WoshisZone() => GlobalEventManager.onCharacterDeathGlobal += CharacterDeath;

		~WoshisZone() => GlobalEventManager.onCharacterDeathGlobal -= CharacterDeath;

		public void CharacterDeath(DamageReport report)
		{
			if (!NetworkServer.active || !report.victimBody || !report.victimBody.HasBuff(this.GetBuffDef().WaitForCompletion()) ||
			    report.victimBody.inventory.GetItemCount(this.GetItemDef().WaitForCompletion()) > 0) return;

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
				master.inventory.GiveItem(this.GetItemDef().WaitForCompletion());
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

		async Task<Material> IMaterialSwap.BuildObject()
		{
			var woshisGhostOverlay = new Material(await LoadAsset<Material>("RoR2/Base/Common/VFX/matGhostEffect.mat"));
			woshisGhostOverlay.SetTexture("_RemapTex", await LoadAsset<Texture2D>("kamunagiassets:texRampWoshis"));
			return woshisGhostOverlay;
		}
		
		public bool CheckEnabled(CharacterModel model, CharacterModel.RendererInfo targetRendererInfo) => !targetRendererInfo.ignoreOverlays && model.body && model.body.inventory && model.body.inventory.GetItemCount(this.GetItemDef().WaitForCompletion()) > 0;

		public int Priority => 1;
	}

	public class WoshisZoneWispGhost : Concentric, IMaterialSwap
	{
		async Task<Material> IMaterialSwap.BuildObject()
		{
			var redWispMat = new Material(await LoadAsset<Material>("RoR2/Base/Wisp/matWispFire.mat"));
			redWispMat.SetFloat("_BrightnessBoost", 2.63f);
			redWispMat.SetFloat("_AlphaBoost", 1.2f);
			redWispMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
			redWispMat.SetColor("_TintColor", Color.red);
			return redWispMat;
		}

		public bool CheckEnabled(CharacterModel model, CharacterModel.RendererInfo targetRendererInfo) => targetRendererInfo.ignoreOverlays && model.body && model.body.inventory &&
		                                                                            model.body.inventory.GetItemCount(GetItemIndex<WoshisZone>().WaitForCompletion()) > 0;

		public int Priority => 1;
	}
}