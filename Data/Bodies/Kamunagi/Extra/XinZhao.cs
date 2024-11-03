using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class XinZhaoState : BaseTwinState
	{
		private float stopwatch;
		private float bufferTime = 0.85f;

		public (Vector3, HealthComponent)[]? enemyHurtBoxes;
		// Collect hurtboxes under authority
		// Init new state
		// new state serializes that data, and then unserializes

		public override void OnEnter()
		{
			base.OnEnter();
			PlayAnimation("Saraana Override", "Saraana NSM");
			PlayAnimation("Ururuu Override", "Ururuu NSM");
			enemyHurtBoxes = new SphereSearch
				{
					origin = characterBody.corePosition, radius = 30, mask = LayerIndex.entityPrecise.mask
				}
				.RefreshCandidates()
				.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamComponent.teamIndex))
				.OrderCandidatesByDistance()
				.FilterCandidatesByDistinctHurtBoxEntities()
				.GetHurtBoxes()
				.Select(x => (x.transform.position, x.healthComponent))
				.ToArray();
		}

		public override void FixedUpdate()
		{
			stopwatch += Time.deltaTime;

			if (stopwatch >= bufferTime)
			{
				outer.SetNextState(new XinZhaoForceFieldState
				{
					hurtBoxes = enemyHurtBoxes, forceFieldPosition = transform.position
				});
			}
		}
	}

	public class XinZhaoForceFieldState : BaseTwinState
	{
		public (Vector3, HealthComponent)[] hurtBoxes;
		public Vector3 forceFieldPosition;

		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write(forceFieldPosition);
			writer.Write(hurtBoxes.Length);
			foreach (var (position, healthComponent) in hurtBoxes)
			{
				writer.Write(position);
				writer.Write(healthComponent.netId);
			}
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			forceFieldPosition = reader.ReadVector3();
			var length = reader.ReadInt32();
			hurtBoxes = new (Vector3, HealthComponent)[length];
			for (var i = 0; i < length; i++)
			{
				hurtBoxes[i].Item1 = reader.ReadVector3();
				hurtBoxes[i].Item2 = Util.FindNetworkObject(reader.ReadNetworkId()).GetComponent<HealthComponent>();
			}
		}

		public override void OnEnter()
		{
			base.OnEnter();
			if (!NetworkServer.active) return;
			NetworkServer.Spawn(UnityEngine.Object.Instantiate(Concentric.GetNetworkedObject<XinZhao>().WaitForCompletion(),
				forceFieldPosition,
				Quaternion.identity));
			foreach (var (position, healthComponent) in hurtBoxes)
			{
				if (healthComponent == this.healthComponent || !healthComponent.body) continue;
				float mass = 0;
				var rigidMotor = healthComponent.body.GetComponent<RigidbodyMotor>();
				if (healthComponent.body.characterMotor)
				{
					mass = ((IPhysMotor)healthComponent.body.characterMotor).mass;
				}
				else if (rigidMotor)
				{
					mass = ((IPhysMotor)rigidMotor).mass;
				}

				var damageInfo = new DamageInfo();
				damageInfo.damage = 4;
				// might need to network this too, if it feels inconsistent on multiplayer
				damageInfo.force = (healthComponent.body.footPosition - characterBody.footPosition).normalized * mass *
				                   55;
				damageInfo.canRejectForce = false;
				damageInfo.position = position;
				damageInfo.inflictor = gameObject;
				damageInfo.canRejectForce = gameObject;
				damageInfo.crit = RollCrit();
				healthComponent.TakeDamage(damageInfo);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= 0.35f) outer.SetNextStateToMain();
		}
	}

	public class XinZhao : Concentric, ISkill, INetworkedObject
	{
		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Extra Skill 3";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA3_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA3_DESCRIPTION";
			skill.icon = (await LoadAsset<Sprite>("bundle:darkpng2"));
			skill.activationStateMachineName = "Weapon";
			skill.baseRechargeInterval = 2f;
			skill.beginSkillCooldownOnSkillEnd = true;
			skill.canceledFromSprinting = false;
			skill.fullRestockOnAssign = false;
			skill.interruptPriority = InterruptPriority.Any;
			skill.mustKeyPress = true;
			skill.cancelSprintingOnActivation = false;
			return skill;
		}

		async Task<GameObject> INetworkedObject.BuildObject()
		{
			Material coolStuff =
				new Material(await LoadAsset<Material>("RoR2/Base/EliteHaunted/matHauntedEliteAreaIndicator.mat"));
			coolStuff.SetTexture("_RemapTex",await  LoadAsset<Texture2D>("RoR2/DLC1/voidraid/texRaidPlanetPurple.png"));
			coolStuff.SetFloat("_DstBlendFloat", 1f);
			coolStuff.SetTexture("_Cloud2Tex", await LoadAsset<Texture2D>("RoR2/Base/Common/texCloudCaustic3.jpg"));

			var forceField =
				(await LoadAsset<GameObject>("RoR2/DLC1/MajorAndMinorConstruct/MajorConstructBubbleShield.prefab")!
					).InstantiateClone("ForceField", true);
			forceField.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
			forceField.transform.Find("Collision").gameObject.AddComponent<RootMotionGoByeBye>();
			forceField.transform.GetChild(1).gameObject.transform.localScale = Vector3.one * 0.5f;
			forceField.GetComponentInChildren<MeshCollider>().gameObject.layer = 3;
			forceField.transform.localScale = Vector3.one * 0.45f;
			UnityEngine.Object.Destroy(forceField.GetComponent<NetworkedBodyAttachment>());
			UnityEngine.Object.Destroy(forceField.GetComponent<VFXAttributes>());
			var ffScale = forceField.GetComponentInChildren<ObjectScaleCurve>();
			ffScale.useOverallCurveOnly = true;
			ffScale.overallCurve = AnimationCurve.Linear(0, 0.35f, 1, 1);
			forceField.AddComponent<DestroyOnTimer>().duration = 15;
			var forceMesh = forceField.GetComponentInChildren<MeshRenderer>();

			forceMesh.sharedMaterials = new[] { coolStuff };

			var forceP = forceField.GetComponentInChildren<ParticleSystemRenderer>();
			forceP.material = new Material(forceP.material);
			forceP.material.SetColor("_TintColor", new Color(0.07843f, 0.02745f, 1));
			forceP.material.DisableKeyword("VERTEXCOLOR");
			return forceField;
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(XinZhaoState), typeof(XinZhaoForceFieldState) };
	}

	public class RootMotionGoByeBye : MonoBehaviour
	{
		private RaycastHit[] results = new RaycastHit[25];
		public float radius = 18f;
		private Ray direction;
		private TeamFilter teamFilter;
		private float mult = 2f;

		public void Awake()
		{
			direction = new Ray { direction = Vector3.up, origin = transform.position };
			teamFilter = GetComponentInParent<TeamFilter>();
		}

		private void FixedUpdate()
		{
			var hits = Physics.SphereCastNonAlloc(direction, radius, results);
			if (hits <= 0) return;
			for (var i = 0; i < hits; i++)
			{
				var other = results[i];
				if (!other.rigidbody) continue;
				var characterBody = other.rigidbody.GetComponent<CharacterBody>();
				if (characterBody && characterBody.teamComponent && characterBody.characterMotor && characterBody.teamComponent.teamIndex != teamFilter.teamIndex)
				{
					characterBody.characterMotor.rootMotion =
						(other.transform.position - transform.position) * (Time.fixedDeltaTime * mult);
				}
			}
		}
	}
}