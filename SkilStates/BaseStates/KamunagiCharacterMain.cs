using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;
using Console = System.Console;

namespace Kamunagi
{
	class KamunagiCharacterMain : GenericCharacterMain
	{
		private EntityStateMachine hoverStateMachine;
		private EntityStateMachine slideStateMachine;
		private TwinBehaviour twinBehaviour;
		private float ascensionCooldown = 2.5f;
		private float bufferTimer = 0.68f;

		public override void OnEnter()
		{
			base.OnEnter();
			twinBehaviour = this.GetComponent<TwinBehaviour>();
			hoverStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Jet");
			slideStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Slide");
		}

		public override void ProcessJump()
		{
			base.ProcessJump();

			if (this.hasInputBank && base.isAuthority)
			{
				bool neagtiveYVelocity = base.characterMotor.velocity.y < 0f;
				bool jumpDownAndNotGrounded = base.inputBank.jump.down && !base.characterMotor.isGrounded;
				bool isHovering = this.hoverStateMachine.state.GetType() == typeof(Hover);
				bool isCharging = slideStateMachine.state.GetType() == typeof(ChannelAscension);
				bool isAscending = slideStateMachine.state.GetType() == typeof(DarkAscension);

				if (neagtiveYVelocity && jumpDownAndNotGrounded && !isHovering) //&& !isCharging) //&& !isAscending)
				{
					if (characterMotor.jumpCount == 1) characterMotor.jumpCount++;
					hoverStateMachine.SetNextState(new Hover());
				}
				
				var longHoverThreshold = 5f;
				//checking time for spacebar being held
				var comingFromLongHover = 0.1f;
				var comingFromAnythingElse = 0.34f; //turn this up, sleeptime now
				
				bufferTimer = twinBehaviour.timeSpentHovering > longHoverThreshold ? 0.1f : 0.34f; //if you were hovering, it takes this long, else it takes THIS long
				//first ternary operator usage, pog
				var timermodifierFromFallingAlot = twinBehaviour.timeSinceLastHover >= 0.7f ? 0.34f : 0f;
				var trueValue = bufferTimer + timermodifierFromFallingAlot;
				
				if (twinBehaviour.timeSinceLastHover > trueValue)
				{
					if (characterMotor.jumpCount > 1 && twinBehaviour.offCooldown && jumpDownAndNotGrounded)
					{
						//set Ascension
						Debug.Log($"ternary is {trueValue}");
						Debug.LogWarning($"time since last hover {twinBehaviour.timeSinceLastHover}");
						twinBehaviour.secondsSinceLastAscension = 0;
						twinBehaviour.chainsVfx1.SetActive(false);
						twinBehaviour.chainsVfx2.SetActive(false);
						slideStateMachine.SetInterruptState(new ChannelAscension(), InterruptPriority.Skill);
					}
				}
			}
		}
	}
	
	internal class Hover : BaseTwinState
	{
		public override int meterGain => 10; //todo this is useful for testing but remember to change this
		public float hoverVelocity = -0.04f;//below negative increases downard velocity, so increase towards positive numbers to hover longer
		private float hoverAcceleration = 80;
		private GameObject hoverFX1;
		private GameObject hoverFX2;
		private GameObject hoverFX3;
		private GameObject hoverFX4;

		public override void OnEnter()
		{
			base.OnEnter();
			if (!twinBehaviour.isInVeil)
			{
				hoverFX1 = UnityEngine.Object.Instantiate(Prefabs.hoverMuzzleFlames, FindModelChild("MuzzleLeft"));
				hoverFX2 = UnityEngine.Object.Instantiate(Prefabs.hoverMuzzleFlames, FindModelChild("MuzzleRight"));
				hoverFX3 = UnityEngine.Object.Instantiate(Prefabs.electricOrbPink, FindModelChild("MuzzleLeft"));
				hoverFX4 = UnityEngine.Object.Instantiate(Prefabs.electricOrbPink, FindModelChild("MuzzleRight"));
			}
			//Debug.LogWarning("Hover has been entered");
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority)
			{
				float num = base.characterMotor.velocity.y;
				num = Mathf.MoveTowards(num, hoverVelocity, hoverAcceleration * Time.deltaTime);
				base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, num, base.characterMotor.velocity.z);
				twinBehaviour.timeSinceLastHover = 0;
				if (!base.inputBank.jump.down)
				{
					outer.SetNextStateToMain();
				}
				twinBehaviour.timeSpentHovering = fixedAge;
			}
		}

		public override void OnExit()
		{
			if (hoverFX1)
			{
				Destroy(hoverFX1);
			}
			if (hoverFX2)
			{
				Destroy(hoverFX2);
			}
			if (hoverFX3)
			{
				Destroy(hoverFX3);
			}
			if (hoverFX4)
			{
				Destroy(hoverFX4);
			}
			//Debug.Log("Hover has been left");
			twinBehaviour.secondsSinceLastAscension += Time.deltaTime;
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Any;
		}
	}
}
