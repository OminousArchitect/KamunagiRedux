using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Events;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class SummonTatariState : IndicatorSpellState
	{
		public override void Fire(Vector3 position)
		{
			
			var tatariSummon = new MasterSummon()
			{
				masterPrefab = Asset.GetGameObject<TatariBody, IMaster>(),
				position = position + Vector3.up * 4,
				summonerBodyObject = gameObject,
				ignoreTeamMemberLimit = true
			};
			var tatariMaster = tatariSummon.Perform();
			var deployable = tatariMaster.gameObject.AddComponent<Deployable>();
			deployable.onUndeploy ??= new UnityEvent();
			deployable.onUndeploy.AddListener(tatariMaster.TrueKill);
			characterBody.master.AddDeployable(deployable, SummonTatari.deployableSlot);
		}
	}

	public class SummonTatari : Asset, ISkill
	{
		public static DeployableSlot deployableSlot;

		public SummonTatari()
		{
			deployableSlot = DeployableAPI.RegisterDeployableSlot((_,_) => 1);
		}

		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 6";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA7_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA7_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle:Uitsalnemetia");
			// TODO i dont know what else to put here
			return skill;
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(SummonTatariState) };
	}

	public class TatariBody : Asset, IBody, IMaster
	{
		GameObject IBody.BuildObject()
		{
			Material tatariMat = new Material(LoadAsset<Material>("RoR2/DLC1/Gup/matGupBodySimple.mat"));
			tatariMat.SetColor("_Color", new Color(0.33f, 0.22f, 0.78f));
			tatariMat.SetColor("_EmColor", new Color(1f, 0f, 0.87f));
			tatariMat.SetFloat("_EmPower", 0.67f);
			tatariMat.SetFloat("_SpecularStrength", 0.05f);
			tatariMat.SetFloat("_SpecularExponent", 3.2f);
			tatariMat.SetFloat("_FlowSpeed", 15f);
			//mat.SetFloat("");
			
			var gupBody = LoadAsset<GameObject>("RoR2/DLC1/Gup/GupBody.prefab")!.InstantiateClone("LargeTatariBody", true);
			var legs = gupBody.transform.Find("ModelBase/mdlGup/mdlGup.003").gameObject;
			legs.GetComponent<SkinnedMeshRenderer>().enabled = false; //attempt #1
			GameObject model = gupBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			gupBody.GetComponent<CharacterDeathBehavior>().deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));
			var mdl = model.GetComponent<CharacterModel>();
			mdl.baseRendererInfos[0].defaultMaterial = tatariMat;
			mdl.baseRendererInfos[1].renderer.enabled = false; //attempt #2
			var lr = model.AddComponent<LegRemover>();
			foreach (SkinnedMeshRenderer m in model.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				switch (m.name)
				{
					case "mdlGup.003":
						lr.legs = m.gameObject;
						break;
				}
			}
			return gupBody;
		}

		GameObject IMaster.BuildObject()
		{
			var tatariMaster = LoadAsset<GameObject>("RoR2/DLC1/Gup/GupMaster.prefab")!.InstantiateClone("LargeTatariMaster", true);
			tatariMaster.AddComponent<SetDontDestroyOnLoad>();
			tatariMaster.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<TatariBody, IBody>();
			return tatariMaster;
		}

		public class LegRemover : MonoBehaviour
		{
			public GameObject legs;

			public void Awake()
			{
				legs.SetActive(false);
			}
		}
	}
}