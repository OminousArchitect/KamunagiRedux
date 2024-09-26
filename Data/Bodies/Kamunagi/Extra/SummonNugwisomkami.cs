using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.Secondary;
using KamunagiOfChains.Data.Bodies.Kamunagi.Utility;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Events;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class SummonNugwisomkamiState : IndicatorSpellState
	{
		public override int meterGain => 0;
		public override int meterGainOnExit => possibleSpirits.Length != 0 || deadSpirits.Length != 0 ? 10 : 0;
		public GameObject[] possibleSpirits;
		public CharacterMaster[] deadSpirits;

		public static Dictionary<GameObject, List<string>> NugwisoDualEliteAspects = new Dictionary<GameObject, List<string>>()
		{
			{
				Asset.GetGameObject<AssassinSpirit, IMaster>(),
				new List<string>() { "EliteLightningEquipment", "EliteFireEquipment" }
			},
			
			{
				Asset.GetGameObject<HunterKillerWisp, IMaster>(),
				new List<string>() { "EliteVoidEquipment", "EliteLunarEquipment" }
			},

			{
				Asset.GetGameObject<TankyArchWisp, IMaster>(),
				new List<string>() { "EliteEarthEquipment", "ElitePoisonEquipment" }
			}
		};
		public override void OnEnter()
		{
			base.OnEnter();
			//collect dictionary
			//then put the keys with null value into an array
			possibleSpirits = twinBehaviour.masterBehaviour.NugwisoSpiritDefs.Where(x => x.Value == null).Select(x => x.Key).ToArray();
			deadSpirits = twinBehaviour.masterBehaviour.NugwisoSpiritDefs.Where(x => x.Value != null && x.Value && x.Value.lostBodyToDeath).Select(x => x.Value!).ToArray();
			
			if (possibleSpirits.Length == 0 && deadSpirits.Length == 0) outer.SetNextStateToMain();
			var pos = characterBody.corePosition + Vector3.up * 4;
		}

		public override void Fire(Vector3 targetPosition)
		{
			SpawnSpirit(targetPosition);
		}

		public void SpawnSpirit(Vector3 spawnPosition)
		{
			
			if (possibleSpirits.Length != 0)
			{
				var index = Mathf.RoundToInt(UnityEngine.Random.Range(0, possibleSpirits.Length));
				log.LogInfo("Index: " + index);
				var whichSpirit = possibleSpirits[index];
				var summon = new MasterSummon
				{
					masterPrefab = whichSpirit,
					position = spawnPosition,
					summonerBodyObject = gameObject,
					ignoreTeamMemberLimit = true
				};
				summon.preSpawnSetupCallback += master =>
				{
					var equipList = NugwisoDualEliteAspects[whichSpirit];
					var whichEquip = equipList[Mathf.RoundToInt(UnityEngine.Random.Range(0, equipList.Count))];
					master.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex(whichEquip));
					twinBehaviour.masterBehaviour.NugwisoSpiritDefs[whichSpirit] = master;
				};
				var characterMaster = summon.Perform();
				var deployable = characterMaster.gameObject.AddComponent<Deployable>();
				deployable.onUndeploy ??= new UnityEvent();
				deployable.onUndeploy.AddListener(characterMaster.TrueKill);
				
				characterBody.master.AddDeployable(deployable, SummonNugwisomkami.deployableSlot);
				
			} else if (deadSpirits.Length != 0)
			{
				var nextSpiritMaster = deadSpirits[Mathf.RoundToInt(UnityEngine.Random.Range(0, deadSpirits.Length))];
				nextSpiritMaster.Respawn(spawnPosition, Quaternion.identity);
				var equipList = NugwisoDualEliteAspects.First(x => x.Key.GetComponent<CharacterMaster>().masterIndex == nextSpiritMaster.masterIndex).Value;
				var whichEquip = equipList[Mathf.RoundToInt(UnityEngine.Random.Range(0, equipList.Count))];
				nextSpiritMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex(whichEquip));
			}
		}
	}

	public class SummonNugwisomkami : Asset, ISkill
	{
		public static DeployableSlot deployableSlot;

		public SummonNugwisomkami()
		{
			deployableSlot = DeployableAPI.RegisterDeployableSlot((_,_) => 1);
		}

		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA5_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA5_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle:Uitsalnemetia");
			// TODO i dont know what else to put here
			return skill;
		}

		public IEnumerable<Type> GetEntityStates() => new []{typeof(SummonNugwisomkamiState)};
	}
	
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
			
			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = GetAsset<AssassinSpiritPrimaryFamily>();
			array[1]._skillFamily = GetAsset<AssassinSpiritSecondaryFamily>();
			return nugwisoBody;
		}
		
		GameObject IMaster.BuildObject()
		{
			var master = LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab")!.InstantiateClone("Nugwiso1Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<AssassinSpirit, IBody>();
			return master;
		}
	}

	public class HunterKillerWisp : Asset, IBody, IMaster
	{
		GameObject IBody.BuildObject()
		{
			Material wispMat = new Material(LoadAsset<Material>("RoR2/Base/LunarWisp/matLunarWispFlames.mat"));
			//wispMat.SetFloat("_BrightnessBoost", 2.63f);
			//wispMat.SetFloat("_AlphaBoost", 1.2f);
			wispMat.SetTexture("_RemapTex", LoadAsset<Texture2D>("bundle:purpleramp"));
			wispMat.SetColor("_TintColor", Color.white);

			var nugwisoBody = LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispBody.prefab")!.InstantiateClone("Nugwiso2", true);
			var mdl = nugwisoBody.GetComponent<ModelLocator>().modelTransform.gameObject;
			Vector3 smaller = Vector3.one * 0.23f;
			mdl.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
			mdl.transform.GetChild(1).localScale = smaller;
			mdl.transform.GetChild(2).localScale = smaller;
			mdl.transform.GetChild(4).localScale = smaller; //particle center
			mdl.transform.GetChild(5).localScale = smaller; //particle left
			mdl.transform.GetChild(6).localScale = smaller; //particle right
			mdl.GetComponentInChildren<Light>().color = Colors.twinsLightColor;
			var cb = nugwisoBody.GetComponent<CharacterBody>();
			cb.baseNameToken = "NUGWISOMKAMI2_BODY_NAME";
			//cb.moveSpeed = 15f;
			//cb.acceleration = 14f;

			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = GetAsset<HKPrimaryFamily>();
			//array[1]._skillFamily = GetAsset<HKSecondaryFamily>();
			return nugwisoBody;
		}
		
		GameObject IMaster.BuildObject()
		{
			var master = LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab")!.InstantiateClone("TwinsHunterKillerWisp", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<HunterKillerWisp, IBody>();
			master.AddComponent<SetDontDestroyOnLoad>();
			return master;
		}
	}
	
	public class TankyArchWisp : Asset, IBody, IMaster
	{
		GameObject IBody.BuildObject()
		{
			var nugwisoBody = LoadAsset<GameObject>("RoR2/Junk/ArchWisp/ArchWispBody.prefab")!.InstantiateClone("Nugwiso3", true);
			nugwisoBody.GetComponent<CharacterBody>().baseNameToken = "NUGWISOMKAMI3_BODY_NAME";
			var array = nugwisoBody.GetComponents<GenericSkill>();
			array[0]._skillFamily = GetAsset<LargeArchWispPrimaryFamily>();
			return nugwisoBody;
		}

		GameObject IMaster.BuildObject()
		{
			var master = LoadAsset<GameObject>("RoR2/Base/LunarWisp/LunarWispMaster.prefab")!.InstantiateClone("Nugwiso3Master", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<TankyArchWisp, IBody>();
			return master;
		}
	}
}