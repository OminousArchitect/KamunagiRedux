using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Events;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class SummonFriendlyEnemyState : IndicatorSpellState
	{
		public override int meterGain => 0;
		public override int meterGainOnExit => possibleElites.Length != 0 || deadElites.Length != 0 ? 10 : 0;
		public override float indicatorScale => 1f;
		public string[] possibleElites;
		public CharacterMaster[] deadElites;

		public override void OnEnter()
		{
			base.OnEnter();

			possibleElites = twinBehaviour.masterBehaviour.wispies.Where(x => x.Value == null).Select(x => x.Key).ToArray();
			deadElites = twinBehaviour.masterBehaviour.wispies.Where(x => x.Value != null && x.Value && x.Value.lostBodyToDeath).Select(x => x.Value!).ToArray();
			if (possibleElites.Length == 0 && deadElites.Length == 0) outer.SetNextStateToMain();
		}

		public override void Fire(Vector3 targetPosition)
		{
			var xoro = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
			var spawnPosition = targetPosition +
			               Vector3.up;
			if (possibleElites.Length != 0)
			{
				var summon = new MasterSummon
				{
					masterPrefab = Asset.GetGameObject<SummonFriendlyEnemy, IMaster>(),
					position =
						spawnPosition, //Utils.FindNearestNodePosition((base.transform.position + Vector3.up * 2) + UnityEngine.Random.rotation.normalized.eulerAngles * RoR2Application.rng.RangeFloat(1, 3), RoR2.Navigation.MapNodeGroup.GraphType.Air),//rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward),
					summonerBodyObject = gameObject,
					ignoreTeamMemberLimit = true
				};
				summon.preSpawnSetupCallback += master =>
				{
					var whichWisp = possibleElites[possibleElites.Length > 1 ? xoro.RangeInt(0, possibleElites.Length - 1) : 0]; 
					master.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex(whichWisp));
					twinBehaviour.masterBehaviour.wispies[whichWisp] = master;
				};
				var characterMaster = summon.Perform();
				var deployable = characterMaster.gameObject.AddComponent<Deployable>();
				deployable.onUndeploy ??= new UnityEvent();
				deployable.onUndeploy.AddListener(characterMaster.TrueKill);
				characterBody.master.AddDeployable(deployable, SummonFriendlyEnemy.deployableSlot);
			} else if (deadElites.Length != 0)
			{
				deadElites[deadElites.Length > 1 ? xoro.RangeInt(0, deadElites.Length - 1) : 0].Respawn(spawnPosition, Quaternion.identity);
			}
		}
	}

	public class SummonFriendlyEnemy : Asset, ISkill, IMaster, IBody
	{
		public static DeployableSlot deployableSlot;

		public SummonFriendlyEnemy()
		{
			deployableSlot = DeployableAPI.RegisterDeployableSlot((_,_) => 4);
		}

		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.activationStateMachineName = "Weapon";
			skill.skillName = "Extra Skill 5";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA5_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA5_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("n");
			// TODO i dont know what else to put here
			return skill;
		}

		GameObject IBody.BuildObject()
		{
			Material wispMat1 = new Material(LoadAsset<Material>("RoR2/Base/Wisp/matWispFire.mat"));
			wispMat1.SetFloat("_BrightnessBoost", 2.63f);
			wispMat1.SetFloat("_AlphaBoost", 1.2f);
			wispMat1.SetTexture("_RemapTex", LoadAsset<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
			wispMat1.SetColor("_TintColor", Colors.wispNeonGreen);

			var customWisp1Body = LoadAsset<GameObject>("RoR2/Base/Wisp/WispBody.prefab")!.InstantiateClone("CustomWisp1", true);
			customWisp1Body.GetComponent<CharacterBody>().baseNameToken = "NUGWISOMKAMI1_BODY_NAME";
			var wispPs = customWisp1Body.GetComponentsInChildren<ParticleSystemRenderer>();
			var firePs = wispPs[0]; //this is verified to be "Fire"
			firePs.material = wispMat1;
			var wtfModel = customWisp1Body.GetComponentInChildren<CharacterModel>();
			wtfModel.baseRendererInfos[1].defaultMaterial = wispMat1;
			wtfModel.baseLightInfos[0].defaultColor = Colors.wispNeonGreen;
			var stuffIdk = customWisp1Body.transform.GetChild(1).gameObject;
			var moreParticles = stuffIdk.GetComponentsInChildren<ParticleSystemRenderer>();
			moreParticles[0].material = wispMat1;
			stuffIdk.GetComponentInChildren<Light>().color = Colors.wispNeonGreen;
			return customWisp1Body;
		}

		GameObject IMaster.BuildObject()
		{
			var master = LoadAsset<GameObject>("RoR2/Base/Wisp/WispMaster.prefab")!.InstantiateClone("TwinsWispMaster", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = GetGameObject<SummonFriendlyEnemy, IBody>();
			return master;
		}

		public Type[] GetEntityStates() => new []{typeof(SummonFriendlyEnemyState)};
	}
}