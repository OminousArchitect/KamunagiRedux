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
	public class AssassinSpirit : Asset, IBody, IMaster
	{
		GameObject IBody.BuildObject()
		{
			Material fireMat = new Material(LoadAsset<Material>("RoR2/Base/Wisp/matWispFire.mat"));
			fireMat.SetFloat("_BrightnessBoost", 2.63f);
			fireMat.SetFloat("_AlphaBoost", 1.2f);
			fireMat.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
			fireMat.SetColor("_TintColor", Colors.wispNeonGreen);

			var nugwisoBody = LoadAsset<GameObject>("RoR2/Base/Wisp/WispBody.prefab")!.InstantiateClone("Nugwiso1", true);
			nugwisoBody.GetComponent<CharacterBody>().baseNameToken = "NUGWISOMKAMI1_BODY_NAME";
			var charModel = nugwisoBody.GetComponentInChildren<CharacterModel>();
			charModel.baseLightInfos[0].defaultColor = Colors.wispNeonGreen;
			//charModel.baseRendererInfos[0].ignoreOverlays = true;
			var mdl = nugwisoBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			var thePSR = mdl.GetComponentInChildren<ParticleSystemRenderer>();
			mdl.GetComponentInChildren<HurtBox>().transform.SetParent(mdl.transform); //set parent of the hurtbox outside of the armature, so we don't destroy it, too
			thePSR.transform.SetParent(mdl.transform); //do the same to the fire particles
			UnityEngine.Object.Destroy(mdl.transform.GetChild(1).gameObject); //destroy armature, we don't need it
			var meshObject = mdl.transform.GetChild(0).gameObject;
			UnityEngine.Object.Destroy(meshObject.GetComponent<SkinnedMeshRenderer>());
			UnityEngine.Object.Destroy(mdl.GetComponentInChildren<SkinnedMeshRenderer>());
			meshObject.AddComponent<MeshFilter>().mesh = LoadAsset<Mesh>("bundle2:TheMask");
			var theRenderer = meshObject.AddComponent<MeshRenderer>();
			theRenderer.material = LoadAsset<Material>("RoR2/DLC1/Assassin2/matAssassin2.mat");
			charModel.baseRendererInfos[0].renderer = theRenderer;
			charModel.baseRendererInfos[0].defaultMaterial = LoadAsset<Material>("RoR2/DLC1/Assassin2/matAssassin2.mat"); //mesh
			charModel.baseRendererInfos[1].renderer = thePSR;
			charModel.baseRendererInfos[1].defaultMaterial = fireMat;
			meshObject.transform.localPosition = new Vector3(0, -4.8f, 0);
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseMaxHealth = 150f;
			
			var secondary = nugwisoBody.AddComponent<GenericSkill>();
			secondary.skillName = "NugwisoSkill2";
			secondary._skillFamily = GetAsset<AssassinSpiritSecondaryFamily>();
			secondary.baseSkill = GetAsset<AssassinSpiritSecondary, ISkill>();
			nugwisoBody.GetComponent<SkillLocator>().secondary = secondary;
			
			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = GetAsset<AssassinSpiritPrimaryFamily>();
			return nugwisoBody;
		}
		
		GameObject IMaster.BuildObject()
		{
			var master = LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab")!.InstantiateClone("Nugwiso1Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<AssassinSpirit, IBody>();
			return master;
		}
	}
	#endregion
	
	public class TrackingWispsState : BaseState
	{
		private float stopwatch;
		private float missileStopwatch;
		public static float firingDuration = 3f;
		public static float missileForce = 1000f;
		public static float damageCoefficient = 1f;
		public static float maxSpread = 180f;
		
		public static string muzzleString = "Head";
		public static string jarOpenSoundString = "Play_gravekeeper_attack1_open";
		public static string jarCloseSoundString = "Play_gravekeeper_attack1_close";
		public static GameObject? jarOpenEffectPrefab;
		public static GameObject? jarCloseEffectPrefab;
		public static GameObject? muzzleflashPrefab;
		private static int BeginGravekeeperBarrageStateHash = Animator.StringToHash("BeginGravekeeperBarrage");
		private static int EndGravekeeperBarrageStateHash = Animator.StringToHash("EndGravekeeperBarrage");
		
		public override void OnEnter()
		{
			base.OnEnter();
			muzzleflashPrefab = LoadAsset<GameObject>("RoR2/Base/Gravekeeper/MuzzleflashTrackingFireball.prefab")!;
			jarOpenEffectPrefab = LoadAsset<GameObject>("RoR2/Base/Gravekeeper/GravekeeperJarOpen.prefab")!;
			jarCloseEffectPrefab = LoadAsset<GameObject>("RoR2/Base/Gravekeeper/GravekeeperJarOpen.prefab")!;
			
			EffectManager.SimpleMuzzleFlash(jarOpenEffectPrefab, base.gameObject, muzzleString, false);
			Util.PlaySound(jarOpenSoundString, base.gameObject);
			base.characterBody.SetAimTimer(firingDuration + 1f);
		}
		
		private void FireWispyBall(Ray projectileRay, float bonusPitch, float bonusYaw)
		{
			projectileRay.direction = Util.ApplySpread(projectileRay.direction, 0f, maxSpread, 1f, 1f, bonusYaw, bonusPitch);
			EffectManager.SimpleMuzzleFlash(muzzleflashPrefab, base.gameObject, muzzleString, false);
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(
					LoadAsset<GameObject>("RoR2/Base/Gravekeeper/GravekeeperTrackingFireball.prefab"),
					projectileRay.origin + Vector3.forward * 3f,
					Util.QuaternionSafeLookRotation(projectileRay.direction),
					base.gameObject,
					damageStat * damageCoefficient, missileForce,
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
			Util.PlaySound(jarCloseSoundString, base.gameObject);
			base.OnExit();
		}
		
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.deltaTime;
			missileStopwatch += Time.deltaTime;
			
			if (missileStopwatch >= 0.8f)
			{
				FireWispyBall(GetAimRay(), 5f, 5f);
				missileStopwatch = 0f;
			}
			if (this.stopwatch >= firingDuration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}
	}
	
	public class AssassinSpiritPrimary : Asset, ISkill
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 8f;
			skill.icon = LoadAsset<Sprite>("n");
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(TrackingWispsState) };
	}
	
	public class AssassinSpiritPrimaryFamily : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<AssassinSpiritPrimary>() };
	}
	
	public class SpiritTeleportState : BaseState
	{
		public CharacterModel? charModel;
		public HurtBoxGroup? hurtBoxGroup;
		public EffectManagerHelper? veilEffect;
		private GameObject childTpFx;
		private Vector3 teleportPosition;
		private float duration = 0.45f;
		private bool teleported;
		private NodeGraph? availableNodes;

		public override void OnEnter()
		{
			base.OnEnter();
			childTpFx = LoadAsset<GameObject>("RoR2/DLC2/Child/FrolicTeleportVFX.prefab")!;
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
			Vector3 effectPos = characterBody.corePosition;
			EffectManager.SpawnEffect(childTpFx, new EffectData
			{
				origin = effectPos,
				scale = 1f
			}, transmit: true);
			NodeGraph airNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Air);
			NodeGraph groundNodes = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
			availableNodes = airNodes;
			var nodesInRange = availableNodes.FindNodesInRange(characterBody.footPosition, 25f, 37f, HullMask.Human);
			NodeGraph.NodeIndex nodeIndex = nodesInRange.ElementAt(UnityEngine.Random.Range(1, nodesInRange.Count));
			availableNodes.GetNodePosition(nodeIndex, out var footPosition);
			footPosition += Vector3.up * 1.5f;
			teleportPosition = footPosition;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!isAuthority) return;

			if (fixedAge > 0.2f && !teleported)
			{
				teleported = true;
				Util.PlaySound("Play_child_attack2_reappear", base.gameObject);
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
			Vector3 effectPos = characterBody.corePosition;
			EffectManager.SpawnEffect(childTpFx, new EffectData
			{
				origin = effectPos,
				scale = 1f
			}, transmit: true);
		}
	}
	
	internal class AssassinSpiritSecondary : Asset, ISkill
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 8f;
			skill.icon = LoadAsset<Sprite>("n");
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(SpiritTeleportState) };
	}
	
	public class AssassinSpiritSecondaryFamily : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<AssassinSpiritSecondary>() };
	}
}