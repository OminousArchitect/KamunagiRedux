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
using HG;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using Rewired.ComponentControls.Effects;

namespace Kamunagi
{
    class BaseTwinState : BaseSkillState
    {
        public virtual int meterGain
        {
            get
            {
                return 10;
            }
        }
        public TwinBehaviour twinBehaviour;
        public ChildLocator childLocator;
        public string muzzleString
        {
            get
            {
                return twinBehaviour ? twinBehaviour.muzzleString : "MuzzleRight";
            }
        }
        public override void OnEnter()
        {
            base.OnEnter();
            twinBehaviour = base.GetComponent<TwinBehaviour>();
            twinBehaviour.AddMeter(meterGain);
            childLocator = base.GetModelChildLocator();
        }
    }
}
