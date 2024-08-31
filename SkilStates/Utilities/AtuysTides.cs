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
using Kamunagi;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;

namespace Kamunagi
{
    class AtuysTides : BaseTwinState
    {
	    public GameObject muzzleflashEffectPrefab;

	    private int totalProjectileCount = 2;
	    private float damageCoefficient = 1.2f;
	    private float projectileSpeed = 80f;
	    private float totalYawSpread = 1f;
	    public float force = 20f;
	    public float selfForce;
	    public string attackString;
	    public string muzzleString;
	    private float duration;
	    private float fireDuration = 1f;
	    private int projectilesFired;
	    private int bubbetMath;
	    private bool soTrue = true;
	    private bool lootbox;
	    private GameObject projectilePrefab = Prefabs.tidalProjectile;

	    public override void OnEnter()
		{
			base.OnEnter();
			bubbetMath = Mathf.RoundToInt(20 / 3 * base.characterBody.attackSpeed - 4.6f); //no idea wtf is going on here
			totalProjectileCount = bubbetMath + 1;

			duration = 2f;
			Util.PlaySound(attackString, base.gameObject);
		}
		
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority)
			{
				int num = Mathf.FloorToInt(base.fixedAge / this.fireDuration * (float)this.totalProjectileCount);
				if (this.projectilesFired <= num && this.projectilesFired < this.totalProjectileCount)
				{
					float blessingChance = critStat + 17f;
					if (Util.CheckRoll(blessingChance, base.characterBody.master))
					{
						projectilePrefab = Prefabs.luckyTidalProjectile;
					}
					Ray aimRay = base.GetAimRay();
					float speedOverride = this.projectileSpeed;
					ProjectileManager.instance.FireProjectile(
						projectilePrefab, 
						aimRay.origin, 
						Util.QuaternionSafeLookRotation(aimRay.direction), 
						base.gameObject, 
						this.damageStat * this.damageCoefficient, 
						this.force,
						Util.CheckRoll(this.critStat, base.characterBody.master), 
						DamageColorIndex.Default, 
						null, 
						speedOverride
						);
					this.projectilesFired++;
				}
			}
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			Debug.Log($"The projectile count is {totalProjectileCount}");
			Debug.Log($"Total skill fire time is {fixedAge}");
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
    }
}
