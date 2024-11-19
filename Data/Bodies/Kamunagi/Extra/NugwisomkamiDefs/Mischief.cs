using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using R2API;
using RoR2;
using RoR2.Navigation;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	#region BodyAndMaster

	public class AssassinSpirit : Concentric, IBody, IMaster //1
	{
		async Task<GameObject> IBody.BuildObject()
		{
			Material fireMat = new Material(await LoadAsset<Material>("RoR2/Base/Wisp/matWispFire.mat"));
			fireMat.SetFloat("_BrightnessBoost", 2.63f);
			fireMat.SetFloat("_AlphaBoost", 1.2f);
			fireMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
			fireMat.SetColor("_TintColor", Colors.zealColor);

			var nugwisoBody =
				(await LoadAsset<GameObject>("RoR2/Base/Wisp/WispBody.prefab"))!.InstantiateClone("Nugwiso1", true);
			var charModel = nugwisoBody.GetComponentInChildren<CharacterModel>();
			charModel.baseLightInfos[0].defaultColor = Colors.zealColor;
			//charModel.baseRendererInfos[0].ignoreOverlays = true;
			var mdl = nugwisoBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			var thePSR = mdl.GetComponentInChildren<ParticleSystemRenderer>();
			mdl.GetComponentInChildren<HurtBox>().transform
				.SetParent(mdl
					.transform); //set parent of the hurtbox outside of the armature, so we don't destroy it, too
			thePSR.transform.SetParent(mdl.transform); //do the same to the fire particles
			UnityEngine.Object.Destroy(mdl.transform.GetChild(1).gameObject); //destroy armature, we don't need it
			var meshObject = mdl.transform.GetChild(0).gameObject;
			UnityEngine.Object.Destroy(meshObject.GetComponent<SkinnedMeshRenderer>());
			UnityEngine.Object.Destroy(mdl.GetComponentInChildren<SkinnedMeshRenderer>());
			meshObject.AddComponent<MeshFilter>().mesh = (await LoadAsset<Mesh>("bundle2:TheMask"));
			nugwisoBody.GetComponent<Rigidbody>().mass = 300f;
			var theRenderer = meshObject.AddComponent<MeshRenderer>();
			theRenderer.material = (await LoadAsset<Material>("RoR2/DLC1/Assassin2/matAssassin2.mat"));
			charModel.baseRendererInfos[0].renderer = theRenderer;
			charModel.baseRendererInfos[0].defaultMaterial =
				(await LoadAsset<Material>("RoR2/DLC1/Assassin2/matAssassin2.mat")); //mesh
			charModel.baseRendererInfos[1].renderer = thePSR;
			charModel.baseRendererInfos[1].defaultMaterial = fireMat;
			meshObject.transform.localPosition = new Vector3(0, -4.8f, 0);
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI1_BODY_NAME";
			cb.baseMaxHealth = 200f;
			cb.baseDamage = 20f;
			cb.levelDamage = 2.3f;
			cb.baseMoveSpeed = 13f;

			var array = mdl.GetComponent<ChildLocator>().transformPairs;
			array[0].transform = mdl.transform;
			array[0].name = "Muzzle";

			#region itemdisplays
			var idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
			idrs.keyAssetRuleGroups = charModel.itemDisplayRuleSet.keyAssetRuleGroups;
			
			var fireDisplay = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/EliteFire/EliteFireEquipment.asset"));
			var fireRules = new ItemDisplayRule[fireDisplay.rules.Length];
			Array.Copy(fireDisplay.rules, fireRules, fireDisplay.rules.Length);
			fireDisplay.rules = fireRules;
			fireDisplay.rules[0].childName = "Muzzle";
			fireDisplay.rules[0].localPos = new Vector3(0.37824F, 0.18649F, 0.31578F);
			fireDisplay.rules[0].localAngles = new Vector3(290.8627F, 338.1044F, 46.19113F);
			fireDisplay.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			fireDisplay.rules[1].childName = "Muzzle";
			fireDisplay.rules[1].localPos = new Vector3(-0.34832F, 0.26794F, 0.14957F);
			fireDisplay.rules[1].localAngles = new Vector3(52.33278F, 60.16898F, 218.7332F);
			fireDisplay.rules[1].localScale = new Vector3(0.3F, 0.3F, 0.3F);
			
			var lightningDisplay = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/EliteLightning/EliteLightningEquipment.asset"));
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
			
			charModel.itemDisplayRuleSet = idrs;
			#endregion

			var secondary = nugwisoBody.AddComponent<GenericSkill>();
			secondary.skillName = "NugwisoSkill2";
			secondary._skillFamily = await GetSkillFamily<AssassinSpiritSecondaryFamily>();
			secondary.baseSkill = await GetSkillDef<AssassinSpiritSecondary>();
			nugwisoBody.GetComponent<SkillLocator>().secondary = secondary;

			var skills = nugwisoBody.GetComponents<GenericSkill>();
			skills[0]._skillFamily = await GetSkillFamily<AssassinSpiritPrimaryFamily>();
			return nugwisoBody;
		}

		async Task<GameObject> IMaster.BuildObject()
		{
			var master =
				(await LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab"))!.InstantiateClone(
					"Nugwiso1Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();
			return master;
		}
	}

	#endregion

	public class TrackingWispsState : BaseState
	{
		private float stopwatch;
		private float missileStopwatch;
		public static float firingDuration = 3.5f;
		public static float missileForce = 700;
		public static float damageCoefficient = 1f;
		public static float maxSpread = 165f;

		public static string muzzleString = "Head";
		public static string jarOpenSoundString = "Play_gravekeeper_attack1_open";
		public static string jarCloseSoundString = "Play_gravekeeper_attack1_close";
		public static GameObject? jarOpenEffectPrefab;
		public static GameObject? jarCloseEffectPrefab;
		public static GameObject? muzzleflashPrefab;
		private static int BeginGravekeeperBarrageStateHash = Animator.StringToHash("BeginGravekeeperBarrage");
		private static int EndGravekeeperBarrageStateHash = Animator.StringToHash("EndGravekeeperBarrage");
		public static GameObject projectilePrefab;

		public override void OnEnter()
		{
			base.OnEnter();

			EffectManager.SimpleMuzzleFlash(jarOpenEffectPrefab, base.gameObject, muzzleString, false);
			//Util.PlaySound(jarOpenSoundString, base.gameObject);
			base.characterBody.SetAimTimer(firingDuration + 1f);
		}

		private void FireWispyBall(Ray projectileRay, float bonusPitch, float bonusYaw)
		{
			projectileRay.direction = Util.ApplySpread(projectileRay.direction, 0f, maxSpread, 1f, 1f, bonusYaw, bonusPitch);
			EffectManager.SimpleMuzzleFlash(muzzleflashPrefab, base.gameObject, muzzleString, false);
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(
					projectilePrefab,
					projectileRay.origin + Vector3.down * 2f,
					Util.QuaternionSafeLookRotation(projectileRay.direction),
					base.gameObject,
					damageStat * damageCoefficient, 
					missileForce,
					Util.CheckRoll(this.critStat, base.characterBody.master),
					DamageColorIndex.Default,
					null,
					-1f
				);
			}
		}

		public override void OnExit()
		{
			EffectManager.SimpleMuzzleFlash(jarCloseEffectPrefab, base.gameObject, muzzleString, false);
			//Util.PlaySound(jarCloseSoundString, base.gameObject);
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.deltaTime;
			missileStopwatch += Time.deltaTime;

			if (missileStopwatch >= 0.5f)
			{
				FireWispyBall(GetAimRay(), 7f, 7f);
				missileStopwatch = 0f;
			}

			if (this.stopwatch >= firingDuration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}
	}

	public class AssassinSpiritPrimary : Concentric, ISkill
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			TrackingWispsState.muzzleflashPrefab = (await LoadAsset<GameObject>("RoR2/Base/Gravekeeper/MuzzleflashTrackingFireball.prefab"))!;
			TrackingWispsState.jarOpenEffectPrefab =
				(await LoadAsset<GameObject>("RoR2/Base/Gravekeeper/GravekeeperJarOpen.prefab"))!;
			TrackingWispsState.jarCloseEffectPrefab =
				(await LoadAsset<GameObject>("RoR2/Base/Gravekeeper/GravekeeperJarOpen.prefab"))!;
			TrackingWispsState.projectilePrefab =
				await LoadAsset<GameObject>("RoR2/Base/Gravekeeper/GravekeeperTrackingFireball.prefab");
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 3f;
			skill.icon = (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(TrackingWispsState) };
	}

	public class AssassinSpiritPrimaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<AssassinSpiritPrimary>() };
	}

	public class SpiritTeleportState : BaseState
	{
		public CharacterModel? charModel;
		public HurtBoxGroup? hurtBoxGroup;
		public EffectManagerHelper? veilEffect;
		public static GameObject childTpFx;
		private Vector3 teleportPosition;
		private float duration = 0.45f;
		private bool teleported;
		private NodeGraph? availableNodes;

		public override void OnEnter()
		{
			base.OnEnter();
			var mdl = GetModelTransform();
			if (mdl)
			{
				charModel = mdl.GetComponent<CharacterModel>();
				hurtBoxGroup = mdl.GetComponent<HurtBoxGroup>();
				if (charModel && hurtBoxGroup)
				{
					charModel.invisibilityCount++;
					hurtBoxGroup.hurtBoxesDeactivatorCounter++;
				}
			}

			Util.PlaySound("Play_imp_attack_blink", gameObject);
			DoChildFx(characterBody.corePosition);
			NodeGraph airNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Air);
			NodeGraph groundNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
			availableNodes = airNodes;
			var nodesInRange = availableNodes.FindNodesInRange(characterBody.footPosition, 25f, 37f, HullMask.Human);
			NodeGraph.NodeIndex nodeIndex = nodesInRange.ElementAt(UnityEngine.Random.Range(1, nodesInRange.Count));
			availableNodes.GetNodePosition(nodeIndex, out var footPosition);
			footPosition += Vector3.up * 1.5f;
			teleportPosition = footPosition;
			characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.CrocoRegen.buffIndex, 1.5f);
		}

		public void DoChildFx(Vector3 effectPos)
		{
			EffectManager.SpawnEffect(childTpFx, new EffectData { origin = effectPos, scale = 1f }, transmit: true);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;

			if (fixedAge > 0.2f && !teleported)
			{
				teleported = true;
				TeleportHelper.TeleportBody(base.characterBody, teleportPosition);
			}

			if (base.fixedAge >= duration)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			if (NetworkServer.active) characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
			if (veilEffect != null) veilEffect.ReturnToPool();
			if (charModel != null && charModel && hurtBoxGroup != null && hurtBoxGroup)
			{
				charModel.invisibilityCount--;
				hurtBoxGroup.hurtBoxesDeactivatorCounter--;
			}

			Util.PlaySound("Play_imp_attack_blink", gameObject);
			DoChildFx(characterBody.corePosition);
		}
	}

	internal class AssassinSpiritSecondary : Concentric, ISkill
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			SpiritTeleportState.childTpFx = (await LoadAsset<GameObject>("RoR2/Base/Imp/ImpBlinkEffect.prefab"))!;
		}

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 8f;
			skill.icon = (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(SpiritTeleportState) };
	}

	public class AssassinSpiritSecondaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<AssassinSpiritSecondary>() };
	}
}