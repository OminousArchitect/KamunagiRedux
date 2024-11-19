using EntityStates;
using EntityStates.LunarWisp;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	#region BodyAndMaster
	public class DecaySpirit : Concentric, IBody, IMaster //4
	{
		async Task<GameObject> IBody.BuildObject()
		{
			Material fireMat = new Material(await LoadAsset<Material>("RoR2/Base/Wisp/matWispFire.mat"));
			fireMat.SetFloat("_BrightnessBoost", 2.63f);
			fireMat.SetFloat("_AlphaBoost", 1.2f);
			fireMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
			fireMat.SetColor("_TintColor", new Color(0, 0.32f, 1f));
			
			var nugwisoBody= (await LoadAsset<GameObject>("RoR2/Base/Wisp/WispBody.prefab"))!.InstantiateClone("Nugwiso4", true);
			var charModel = nugwisoBody.GetComponentInChildren<CharacterModel>();
			charModel.baseLightInfos[0].defaultColor = Colors.jachdwaltColor;
			//charModel.baseRendererInfos[0].ignoreOverlays = true;
			var mdl = nugwisoBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			var thePSR = mdl.GetComponentInChildren<ParticleSystemRenderer>();
			mdl.GetComponentInChildren<HurtBox>().transform.SetParent(mdl.transform); //set parent of the hurtbox outside of the armature, so we don't destroy it, too
			thePSR.transform.SetParent(mdl.transform); //do the same to the fire particles
			UnityEngine.Object.Destroy(mdl.transform.GetChild(1).gameObject); //destroy armature, we don't need it
			var meshObject = mdl.transform.GetChild(0).gameObject;
			UnityEngine.Object.Destroy(meshObject.GetComponent<SkinnedMeshRenderer>());
			UnityEngine.Object.Destroy(mdl.GetComponentInChildren<SkinnedMeshRenderer>());
			meshObject.AddComponent<MeshFilter>().mesh= (await LoadAsset<Mesh>("bundle2:IceMask"));
			nugwisoBody.GetComponent<Rigidbody>().mass = 300f;
			var theRenderer = meshObject.AddComponent<MeshRenderer>();
			theRenderer.material= (await LoadAsset<Material>("RoR2/Junk/AncientWisp/matAncientWisp.mat"));
			charModel.baseRendererInfos[0].renderer = theRenderer;
			charModel.baseRendererInfos[0].defaultMaterial= (await LoadAsset<Material>("RoR2/Junk/AncientWisp/matAncientWisp.mat")); //mesh
			charModel.baseRendererInfos[1].renderer = thePSR;
			charModel.baseRendererInfos[1].defaultMaterial = fireMat;
			meshObject.transform.localPosition = new Vector3(0, -2.4f, 0.4f);
			meshObject.transform.localScale = Vector3.one * 3;
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI4_BODY_NAME";
			cb.baseMaxHealth = 330f;
			cb.baseDamage = 12f;
			cb.baseMoveSpeed = 4f;

			var cPairs = mdl.GetComponent<ChildLocator>().transformPairs;
			cPairs[0].transform = mdl.transform;
			cPairs[0].name = "Muzzle";
			
			#region itemdisplays
			var idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
			idrs.keyAssetRuleGroups = charModel.itemDisplayRuleSet.keyAssetRuleGroups;
			
			var hauntedDisplay = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/EliteHaunted/EliteHauntedEquipment.asset"));
			var hauntedRules = new ItemDisplayRule[hauntedDisplay.rules.Length];
			Array.Copy(hauntedDisplay.rules, hauntedRules, hauntedDisplay.rules.Length);
			hauntedDisplay.rules = hauntedRules;
			hauntedDisplay.rules[0].childName = "Muzzle";
			hauntedDisplay.rules[0].localPos = new Vector3(-0.02014F, 0.18649F, -0.16408F);
			hauntedDisplay.rules[0].localAngles = new Vector3(270F, 0F, 0F);
			hauntedDisplay.rules[0].localScale = new Vector3(0.3F, 0.3F, 0.3F);

			var iceDisplay = idrs.FindDisplayRuleGroup(await LoadAsset<EquipmentDef>("RoR2/Base/EliteIce/EliteIceEquipment.asset"));
			var iceRules = new ItemDisplayRule[iceDisplay.rules.Length];
			Array.Copy(iceDisplay.rules, iceRules, iceDisplay.rules.Length);
			iceDisplay.rules = iceRules;
			iceDisplay.rules[0].childName = "Muzzle";
			iceDisplay.rules[0].localPos = new Vector3(0.0189F, 1.05928F, 0.03792F);
			iceDisplay.rules[0].localAngles = new Vector3(270F, 0F, 0F);
			iceDisplay.rules[0].localScale = new Vector3(0.08F, 0.08F, 0.08F);

			charModel.itemDisplayRuleSet = idrs;
			#endregion
			
			var secondary = nugwisoBody.AddComponent<GenericSkill>();
			secondary.skillName = "NugwisoSkill2";
			secondary._skillFamily = await GetSkillFamily<DecayPrimaryFamily>();
			secondary.baseSkill = await GetSkillDef<DecaySecondary>();
			nugwisoBody.GetComponent<SkillLocator>().secondary = secondary;
			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = await GetSkillFamily<DecayPrimaryFamily>();
			return nugwisoBody;
		}

		async Task<GameObject> IMaster.BuildObject()
		{
			var master= (await LoadAsset<GameObject>("RoR2/Base/Wisp/WispMaster.prefab"))!.InstantiateClone("Nugwiso4Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();
			return master;
		}
	}
	#endregion

	public class ShootDaggerState : BaseState
	{
		public static GameObject? daggerFab;
		public override void OnExit()
		{
			int projectileCount = 4;
			var xoro = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
			var spacingDegrees = 360f / projectileCount;
			var forward = Vector3.ProjectOnPlane(inputBank.aimDirection, Vector3.up);
			var centerPoint = characterBody.footPosition;
			for (var i = 0; i < projectileCount; i++)
			{
				ProjectileManager.instance.FireProjectile(
					daggerFab,
					centerPoint,
					Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(spacingDegrees * i, Vector3.up) * forward),
					gameObject,
					damageStat * 2f,
					95f,
					RollCrit()
				);
			}
			base.OnExit();
		}
	}
	
	public class DecayPrimary : Concentric, ISkill
	{
		public override async Task Initialize()
		{
			await base.Initialize();
			ShootDaggerState.daggerFab = await LoadAsset<GameObject>("RoR2/Base/Dagger/DaggerProjectile.prefab");
		}
		
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 5f;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(ShootDaggerState) };
	}
	public class DecayPrimaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<DecayPrimary>() };
	}
	//secondary state goes here
	public class DecaySecondary : Concentric, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 3f;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}
		
		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(ShootDaggerState) };
	}
	public class DecaySecondaryFamily : Concentric, ISkillFamily
	{
		public IEnumerable<Concentric> GetSkillAssets() => new Concentric[] { GetAsset<DecaySecondary>() };
	}
}