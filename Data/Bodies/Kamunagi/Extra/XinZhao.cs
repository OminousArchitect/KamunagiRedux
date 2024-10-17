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
		private float bufferTime;
		public (Vector3, HealthComponent)[]? enemyHurtBoxes;
		// Collect hurtboxes under authority
		// Init new state
		// new state serializes that data, and then unserializes

		public override void OnEnter()
		{
			base.OnEnter();
			bufferTime = twinBehaviour.runtimeNumber1;
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
					hurtBoxes = enemyHurtBoxes,
					forceFieldPosition = transform.position
				});
			}
		}
	}

	public class XinZhaoForceFieldState : BaseTwinState
	{
		public (Vector3, HealthComponent)[] hurtBoxes;
		public override int meterGain => 0;
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
			NetworkServer.Spawn(UnityEngine.Object.Instantiate(Asset.GetGameObject<XinZhao, INetworkedObject>(), forceFieldPosition,
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
				damageInfo.force = (healthComponent.body.footPosition - characterBody.footPosition).normalized * mass * 55;
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

	public class XinZhao : Asset, ISkill, INetworkedObject
	{
		SkillDef ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Extra Skill 3";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA3_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA3_DESCRIPTION";
			skill.icon = LoadAsset<Sprite>("bundle2:172714677590348517"); //lol, lmao
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

		GameObject INetworkedObject.BuildObject()
		{
			Material coolStuff = new Material(LoadAsset<Material>("RoR2/Base/EliteHaunted/matHauntedEliteAreaIndicator.mat"));
			coolStuff.SetTexture("_RemapTex",LoadAsset<Texture2D>("RoR2/DLC1/voidraid/texRaidPlanetPurple.png"));
			coolStuff.SetFloat("_DstBlendFloat", 1f);
			coolStuff.SetTexture("_Cloud2Tex", LoadAsset<Texture2D>("RoR2/Base/Common/texCloudCaustic3.jpg"));
			
			var forceField = LoadAsset<GameObject>("RoR2/DLC1/MajorAndMinorConstruct/MajorConstructBubbleShield.prefab")!.InstantiateClone("ForceField", true);
			forceField.GetComponentInChildren<MeshCollider>().gameObject.layer = 3;
			forceField.transform.localScale = Vector3.one * 0.45f;
			UnityEngine.Object.Destroy(forceField.GetComponent<NetworkedBodyAttachment>());
			UnityEngine.Object.Destroy(forceField.GetComponent<VFXAttributes>());
			var ffScale = forceField.GetComponentInChildren<ObjectScaleCurve>();
			ffScale.useOverallCurveOnly = true;
			ffScale.overallCurve = AnimationCurve.Linear(0, 0.35f, 1, 1);
			forceField.AddComponent<DestroyOnTimer>().duration = 15;
			var forceMesh = forceField.GetComponentInChildren<MeshRenderer>();

			forceMesh.sharedMaterials = new[]
			{
				coolStuff
			};

			var forceP = forceField.GetComponentInChildren<ParticleSystemRenderer>();
			forceP.material = new Material(forceP.material);
			forceP.material.SetColor("_TintColor", new Color(0.07843f, 0.02745f, 1));
			forceP.material.DisableKeyword("VERTEXCOLOR");
			return forceField;
		}

		public IEnumerable<Type> GetEntityStates() => new []{typeof(XinZhaoState), typeof(XinZhaoForceFieldState)};
	}
}