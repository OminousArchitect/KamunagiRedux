using EntityStates;
using KamunagiOfChains.Data.Bodies.Kamunagi.OtherStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace KamunagiOfChains.Data.Bodies.Kamunagi.Extra
{
	public class SummonMothMoth : BaseTwinState
	{
		private float duration = 0.55f;

		public override void OnEnter()
		{
			base.OnEnter();
			Util.PlaySound(EntityStates.BeetleQueenMonster.SpawnWards.attackSoundString, gameObject);
			if (NetworkServer.active)
			{
				var ward = Object.Instantiate(Concentric.GetNetworkedObject<MothMoth>().WaitForCompletion(), characterBody.corePosition, Quaternion.identity);
				ward.GetComponent<TeamComponent>().teamIndex = teamComponent.teamIndex;
				ward.GetComponent<TeamFilter>().teamIndex = teamComponent.teamIndex;
				NetworkServer.Spawn(ward);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= duration && isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
	}

	public class MothMoth : Concentric, INetworkedObject, ISkill
	{
		private static readonly int Cull = Shader.PropertyToID("_Cull");
		private static readonly int Color = Shader.PropertyToID("_Color");
		private static readonly int EmColor = Shader.PropertyToID("_EmColor");
		private static readonly int FresnelRamp = Shader.PropertyToID("_FresnelRamp");
		private static readonly int PrintRamp = Shader.PropertyToID("_PrintRamp");
		private static readonly int RemapTex = Shader.PropertyToID("_RemapTex");
		private static readonly int TintColor = Shader.PropertyToID("_TintColor");

		async Task<GameObject> INetworkedObject.BuildObject()
		{
			var mothMoth =
				(await LoadAsset<GameObject>("addressable:RoR2/Base/Beetle/BeetleWard.prefab"))!.InstantiateClone("MothMoth");
			mothMoth.GetComponent<BuffWard>().buffDef =
				await LoadAsset<BuffDef>("RoR2/Base/LifestealOnHit/bdLifeSteal.asset");

			var impMat = new Material(await LoadAsset<Material>("addressable:RoR2/Base/Imp/matImpBoss.mat"));
			impMat.SetFloat(Cull, 0);
			impMat.SetColor(Color, new Color(0.2588235f, 0.2705882f, 0.6352941f));
			impMat.SetColor(EmColor,
				new Color(0.07058824f, 0.07058824f,
					0.8823529f)); //you will probably need the shader stub for hgstandard here.
			impMat.SetTexture(FresnelRamp,
				await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampLunarElectric.png"));
			impMat.SetTexture(PrintRamp,
				await LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png"));
			
			mothMoth.GetComponentInChildren<SkinnedMeshRenderer>().material = impMat;

			//Main color, Emission color, Fresnel ramp, and Print Ramp are your 4 big ticket items here
			var mothLight = mothMoth.GetComponentInChildren<Light>();
			mothLight.color = new Color(0f, 0.391f, 0.9f);
			mothLight.range = 4f;

			var (mothWParticles, (garbage, _)) = mothMoth.GetComponentsInChildren<ParticleSystemRenderer>();
			Object.Destroy(garbage);
			var particlesMat = new Material(await LoadAsset<Material>("addressable:RoR2/DLC1/PortalVoid/matPortalVoid.mat"));
			particlesMat.SetTexture(RemapTex,
				await LoadAsset<Texture2D>("addressable:RoR2/Base/Captain/texRampCrosshair2.png"));
			particlesMat.SetColor(TintColor, new Color(0f, 0.6784314f, 1f));
			mothWParticles.material = particlesMat;
			var particlesTransform = mothWParticles.transform;
			particlesTransform.localPosition = new Vector3(0f, 0.3f, 0f);
			particlesTransform.localScale = Vector3.one * 0.3f;

			var outlineMaterial = new Material(await LoadAsset<Material>("addressable:RoR2/Base/Nullifier/matNullifierZoneAreaIndicatorLookingIn.mat"));
			outlineMaterial.SetColor(TintColor, new Color(0f, 0.274509804f, 1f));
			outlineMaterial.SetFloat("_RimPower", 3.8f);
			mothMoth.GetComponentInChildren<MeshRenderer>().material = outlineMaterial;
			mothMoth.GetComponent<BuffWard>().radius = 2.8f;
			mothMoth.AddComponent<DestroyOnTimer>().duration = 10;
			return mothMoth;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(SummonMothMoth) };

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skill = ScriptableObject.CreateInstance<SkillDef>();
			skill.skillName = "Extra Skill 4";
			skill.skillNameToken = KamunagiAsset.tokenPrefix + "EXTRA4_NAME";
			skill.skillDescriptionToken = KamunagiAsset.tokenPrefix + "EXTRA4_DESCRIPTION";
			skill.icon= (await LoadAsset<Sprite>("kamunagiassets:no-type2"));
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
	}
}