using EntityStates.GolemMonster;
using EntityStates.LunarWisp;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	#region BodyAndMaster
	public class WarMachine : Asset, IBody, IMaster //2
	{
		async Task<GameObject> IBody.BuildObject()
		{
			Material wispMat = new Material(await LoadAsset<Material>("RoR2/Base/LunarWisp/matLunarWispFlames.mat"));
			//wispMat.SetFloat("_BrightnessBoost", 2.63f);
			//wispMat.SetFloat("_AlphaBoost", 1.2f);
			wispMat.SetTexture("_RemapTex", await LoadAsset<Texture2D>("bundle:purpleramp"));
			wispMat.SetColor("_TintColor", Color.white);

			var nugwisoBody= (await LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispBody.prefab"))!.InstantiateClone("Nugwiso2", true);
			var mdl = nugwisoBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			Vector3 particles = Vector3.one * 0.6f;
			mdl.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
			//mdl.transform.GetChild(1).localScale = smaller;
			mdl.transform.GetChild(2).localScale = new Vector3(0.4f, 0.4f, 0.4f);;
			mdl.transform.GetChild(4).localScale = new Vector3(0.4f, 0.4f, 0.4f);
			mdl.transform.GetChild(5).localScale = new Vector3(0.6f, 0.6f, 0.6f);
			mdl.transform.GetChild(6).localScale = new Vector3(0.6f, 0.6f, 0.6f);
			mdl.GetComponentInChildren<Light>().color = Colors.twinsLightColor;
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI2_BODY_NAME";
			cb.baseMaxHealth = 230f;

			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = await GetSkillFamily<HKPrimaryFamily>();
			return nugwisoBody;
		}

		async Task<GameObject> IMaster.BuildObject()
		{
			var master= (await LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab"))!.InstantiateClone("Nugwiso2Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();
			master.AddComponent<SetDontDestroyOnLoad>();
			return master;
		}
	}
	#endregion
	
	public class FireWispGunsState : ChargeLunarGuns
	{
		public override void OnEnter()
		{
			this.duration = 0.8f;
			this.muzzleTransformRoot = base.FindModelChild(ChargeLunarGuns.muzzleNameRoot);
			this.muzzleTransformOne = base.FindModelChild(ChargeLunarGuns.muzzleNameOne);
			this.muzzleTransformTwo = base.FindModelChild(ChargeLunarGuns.muzzleNameTwo);
			this.loopedSoundID = Util.PlaySound(ChargeLunarGuns.windUpSound, base.gameObject);
			base.PlayCrossfade("Gesture", "MinigunSpinUp", 0.2f);
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(this.duration);
			}
		}
	}
	
	internal class ReplaceHKPrimary : Asset, ISkill
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = "";
			skill.skillDescriptionToken = "";
			skill.baseRechargeInterval = 8f;
			skill.icon= (await LoadAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png"));
			return skill;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(FireWispGunsState) };
	}
	
	public class HKPrimaryFamily : Asset, ISkillFamily
	{
		public IEnumerable<Asset> GetSkillAssets() => new Asset[] { GetAsset<ReplaceHKPrimary>() };
	}
}